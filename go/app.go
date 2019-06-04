package app

import (
	"encoding/json"

	"github.com/cosmos/cosmos-sdk/x/auth"
	"github.com/cosmos/cosmos-sdk/x/params"
	"github.com/cosmos/cosmos-sdk/x/staking"

	"github.com/tendermint/tendermint/libs/common"
	"github.com/tendermint/tendermint/libs/db"
	"github.com/tendermint/tendermint/libs/log"
	abci "github.com/tendermint/tendermint/abci/types"
	tendermintTypes "github.com/tendermint/tendermint/types"

	"github.com/cosmos/cosmos-sdk/baseapp"
	"github.com/cosmos/cosmos-sdk/codec"
	"github.com/cosmos/cosmos-sdk/types"

	"github.com/comrade-coop/strongforce/go/x/strongforce"
)

const (
	appName = "strongforce"
)

// StrongForceApp - Type containing needed information for the strongforce cosmos/tendermint application
type StrongForceApp struct {
	*baseapp.BaseApp

	cdc *codec.Codec

	accountKeeper     auth.AccountKeeper
	paramsKeeper      params.Keeper
	strongforceKeeper strongforce.Keeper

	keyMain        *types.KVStoreKey
	keyAccount     *types.KVStoreKey
	keyParams      *types.KVStoreKey
	tkeyParams     *types.TransientStoreKey
	keyStrongforce *types.KVStoreKey
}

// NewStrongForceApp - Constructs a new StrongForceApp
func NewStrongForceApp(logger log.Logger, db db.DB) *StrongForceApp {

	// First define the top level codec that will be shared by the different modules. Note: Codec will be explained later
	cdc := MakeCodec()

	// BaseApp handles interactions with Tendermint through the ABCI protocol
	baseApp := baseapp.NewBaseApp(appName, logger, db, auth.DefaultTxDecoder(cdc))

	var app = &StrongForceApp{
		BaseApp:        baseApp,
		keyMain:        types.NewKVStoreKey("main"),
		keyAccount:     types.NewKVStoreKey("account"),
		keyParams:      types.NewKVStoreKey("params"),
		tkeyParams:     types.NewTransientStoreKey("transient_params"),
		keyStrongforce: types.NewKVStoreKey("strongforce"),
		cdc:            cdc,
	}

	app.paramsKeeper = params.NewKeeper(app.cdc, app.keyParams, app.tkeyParams)
	app.accountKeeper = auth.NewAccountKeeper(
		app.cdc,
		app.keyAccount,
		app.paramsKeeper.Subspace(auth.DefaultParamspace),
		auth.ProtoBaseAccount,
	)
	app.strongforceKeeper = strongforce.NewKeeper(cdc, app.keyStrongforce)
	// app.SetAnteHandler(auth.NewAnteHandler(app.accountKeeper))

	app.Router()

	app.QueryRouter().
		AddRoute("acc", auth.NewQuerier(app.accountKeeper)).
		AddRoute("strongforce", strongforce.NewQuerier(app.strongforceKeeper))

	app.MountStores(
		app.keyMain,
		app.keyAccount,
		app.keyParams,
		app.keyStrongforce,
		app.tkeyParams,
	)

	err := app.LoadLatestVersion(app.keyMain)
	if err != nil {
		common.Exit(err.Error())
	}

	return app
}

// GenesisState represents chain state at the start of the chain. Any initial state (account balances) are stored here.
type GenesisState struct {
	AuthData auth.GenesisState   `json:"auth"`
	Accounts []*auth.BaseAccount `json:"accounts"`
}

// ExportAppStateAndValidators exports the app state and validators to json
func (app *StrongForceApp) ExportAppStateAndValidators() (appState json.RawMessage, validators []tendermintTypes.GenesisValidator, err error) {
	ctx := app.NewContext(true, abci.Header{})
	accounts := []*auth.BaseAccount{}

	appendAccountsFn := func(acc auth.Account) bool {
		account := &auth.BaseAccount{
			Address: acc.GetAddress(),
			Coins:   acc.GetCoins(),
		}

		accounts = append(accounts, account)
		return false
	}

	app.accountKeeper.IterateAccounts(ctx, appendAccountsFn)

	genState := GenesisState{
		Accounts: accounts,
		AuthData: auth.DefaultGenesisState(),
	}

	appState, err = codec.MarshalJSONIndent(app.cdc, genState)
	if err != nil {
		return nil, nil, err
	}

	return appState, validators, err
}

// MakeCodec - Creates the codec needed for strongforce
func MakeCodec() *codec.Codec {

	var cdc = codec.New()

	auth.RegisterCodec(cdc)
	staking.RegisterCodec(cdc)
	strongforce.RegisterCodec(cdc)
	types.RegisterCodec(cdc)
	codec.RegisterCrypto(cdc)

	return cdc
}
