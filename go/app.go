package app

import (
	"encoding/json"
	"os"

	"github.com/cosmos/cosmos-sdk/x/auth"
	"github.com/cosmos/cosmos-sdk/x/distribution"
	"github.com/cosmos/cosmos-sdk/x/genaccounts"
	"github.com/cosmos/cosmos-sdk/x/genutil"
	"github.com/cosmos/cosmos-sdk/x/params"
	"github.com/cosmos/cosmos-sdk/x/slashing"
	"github.com/cosmos/cosmos-sdk/x/staking"

	"github.com/cosmos/cosmos-sdk/x/bank"
	abci "github.com/tendermint/tendermint/abci/types"
	"github.com/tendermint/tendermint/libs/common"
	"github.com/tendermint/tendermint/libs/log"
	tendermintTypes "github.com/tendermint/tendermint/types"
	db "github.com/tendermint/tm-db"

	"github.com/cosmos/cosmos-sdk/baseapp"
	"github.com/cosmos/cosmos-sdk/codec"
	"github.com/cosmos/cosmos-sdk/types"
	"github.com/cosmos/cosmos-sdk/types/module"

	"github.com/comrade-coop/strongforce/go/x/strongforce"
	"github.com/cosmos/cosmos-sdk/x/supply"
)

const (
	appName = "strongforce"
)

var (
	// DefaultCLIHome is the default home directory for the application CLI
	DefaultCLIHome = os.ExpandEnv("$HOME/.strongforcecli")

	// DefaultNodeHome is the default the folder where the applcation data and configuration are stored
	DefaultNodeHome = os.ExpandEnv("$HOME/.strongforced")

	// ModuleBasicManager is in charge of setting up basic module elemnets
	ModuleBasicManager = module.NewBasicManager(
		auth.AppModuleBasic{},
		distribution.AppModuleBasic{},
		genaccounts.AppModuleBasic{},
		genutil.AppModuleBasic{},
		params.AppModuleBasic{},
		slashing.AppModuleBasic{},
		staking.AppModuleBasic{},
		strongforce.AppModuleBasic{},
		supply.AppModuleBasic{},
		bank.AppModuleBasic{},
	)
	// account permissions
	maccPerms = map[string][]string{
		auth.FeeCollectorName:     nil,
		distribution.ModuleName:   nil,
		staking.BondedPoolName:    []string{supply.Burner, supply.Staking},
		staking.NotBondedPoolName: []string{supply.Burner, supply.Staking},
	}

	blacklist = map[string]bool{}
)

// StrongForceApp - Type containing needed information for the strongforce cosmos/tendermint application
type StrongForceApp struct {
	*baseapp.BaseApp

	cdc *codec.Codec

	keyAccount      *types.KVStoreKey
	keyDistribution *types.KVStoreKey
	keySupply       *types.KVStoreKey
	keyMain         *types.KVStoreKey
	keyParams       *types.KVStoreKey
	keySlashing     *types.KVStoreKey
	keyStaking      *types.KVStoreKey
	keyStrongforce  *types.KVStoreKey

	tkeyParams  *types.TransientStoreKey
	tkeyStaking *types.TransientStoreKey

	accountKeeper      auth.AccountKeeper
	distributionKeeper distribution.Keeper
	supplyKeeper       supply.Keeper
	paramsKeeper       params.Keeper
	slashingKeeper     slashing.Keeper
	stakingKeeper      staking.Keeper
	bankKeeper         bank.Keeper
	strongforceKeeper  strongforce.Keeper

	mm *module.Manager
}

// NewStrongForceApp - Constructs a new StrongForceApp
func NewStrongForceApp(logger log.Logger, db db.DB) *StrongForceApp {

	// First define the top level codec that will be shared by the different modules. Note: Codec will be explained later
	cdc := MakeCodec()

	// BaseApp handles interactions with Tendermint through the ABCI protocol
	baseApp := baseapp.NewBaseApp(appName, logger, db, auth.DefaultTxDecoder(cdc))

	var app = &StrongForceApp{
		BaseApp: baseApp,
		cdc:     cdc,

		keyAccount:      types.NewKVStoreKey(auth.StoreKey),
		keyDistribution: types.NewKVStoreKey(distribution.StoreKey),
		keySupply:       types.NewKVStoreKey(supply.StoreKey),
		keyMain:         types.NewKVStoreKey(baseapp.MainStoreKey),
		keyParams:       types.NewKVStoreKey(params.StoreKey),
		keySlashing:     types.NewKVStoreKey(slashing.StoreKey),
		keyStaking:      types.NewKVStoreKey(staking.StoreKey),
		keyStrongforce:  types.NewKVStoreKey(strongforce.StoreKey),
		tkeyParams:      types.NewTransientStoreKey(params.TStoreKey),
		tkeyStaking:     types.NewTransientStoreKey(staking.TStoreKey),
	}

	app.paramsKeeper = params.NewKeeper(app.cdc, app.keyParams, app.tkeyParams, params.DefaultCodespace)
	// Set specific supspaces
	authSubspace := app.paramsKeeper.Subspace(auth.DefaultParamspace)
	bankSupspace := app.paramsKeeper.Subspace(bank.DefaultParamspace)
	stakingSubspace := app.paramsKeeper.Subspace(staking.DefaultParamspace)
	distrSubspace := app.paramsKeeper.Subspace(distribution.DefaultParamspace)
	slashingSubspace := app.paramsKeeper.Subspace(slashing.DefaultParamspace)

	app.accountKeeper = auth.NewAccountKeeper(
		app.cdc,
		app.keyAccount,
		authSubspace,
		auth.ProtoBaseAccount,
	)

	app.bankKeeper = bank.NewBaseKeeper(
		app.accountKeeper,
		bankSupspace,
		bank.DefaultCodespace,
		blacklist,
	)

	app.supplyKeeper = supply.NewKeeper(
		app.cdc,
		app.keySupply,
		app.accountKeeper,
		app.bankKeeper,
		maccPerms,
	)

	app.strongforceKeeper = strongforce.NewKeeper(cdc, app.keyStrongforce)

	app.stakingKeeper = staking.NewKeeper(
		app.cdc,
		app.keyStaking,
		app.tkeyStaking,
		app.supplyKeeper, // In lieu of a bank keeper
		stakingSubspace,
		staking.DefaultCodespace,
	)

	app.distributionKeeper = distribution.NewKeeper(
		app.cdc,
		app.keyDistribution,
		distrSubspace,
		&app.stakingKeeper,
		app.supplyKeeper,
		distribution.DefaultCodespace,
		auth.FeeCollectorName,
		blacklist,
	)

	app.slashingKeeper = slashing.NewKeeper(
		app.cdc,
		app.keySlashing,
		&app.stakingKeeper,
		slashingSubspace,
		slashing.DefaultCodespace,
	)

	app.stakingKeeper = *app.stakingKeeper.SetHooks(
		staking.NewMultiStakingHooks(
			app.distributionKeeper.Hooks(),
			app.slashingKeeper.Hooks()),
	)

	app.mm = module.NewManager(
		auth.NewAppModule(app.accountKeeper),
		distribution.NewAppModule(app.distributionKeeper, app.supplyKeeper),
		genaccounts.NewAppModule(app.accountKeeper),
		bank.NewAppModule(app.bankKeeper, app.accountKeeper),
		supply.NewAppModule(app.supplyKeeper, app.accountKeeper),
		genutil.NewAppModule(app.accountKeeper, app.stakingKeeper, app.BaseApp.DeliverTx),
		slashing.NewAppModule(app.slashingKeeper, app.stakingKeeper),
		staking.NewAppModule(app.stakingKeeper, app.distributionKeeper, app.accountKeeper, app.supplyKeeper),
		strongforce.NewAppModule(app.strongforceKeeper, "127.0.0.1:8989"),
	)

	app.mm.SetOrderBeginBlockers(distribution.ModuleName, slashing.ModuleName)
	app.mm.SetOrderEndBlockers(staking.ModuleName)

	// Sets the order of Genesis - Order matters, genutil is to always come last
	app.mm.SetOrderInitGenesis(
		genaccounts.ModuleName,
		auth.ModuleName,
		bank.ModuleName,
		supply.ModuleName,
		staking.ModuleName,
		distribution.ModuleName,
		slashing.ModuleName,
		strongforce.ModuleName,
		genutil.ModuleName,
	)

	app.mm.RegisterRoutes(app.Router(), app.QueryRouter())

	app.SetInitChainer(app.InitChainer)
	app.SetBeginBlocker(app.BeginBlocker)
	app.SetEndBlocker(app.EndBlocker)

	app.SetAnteHandler(
		auth.NewAnteHandler(
			app.accountKeeper,
			app.supplyKeeper,
			auth.DefaultSigVerificationGasConsumer,
		),
	)

	app.MountStores(
		app.keyAccount,
		app.keyDistribution,
		app.keySupply,
		app.keyMain,
		app.keyParams,
		app.keySlashing,
		app.keyStaking,
		app.keyStrongforce,

		app.tkeyParams,
		app.tkeyStaking,
	)

	err := app.LoadLatestVersion(app.keyMain)
	if err != nil {
		common.Exit(err.Error())
	}

	return app
}

// GenesisState represents the genesis state of a strongforce app
type GenesisState map[string]json.RawMessage

// NewDefaultGenesisState returns the default GenesisState
func NewDefaultGenesisState() GenesisState {
	return ModuleBasicManager.DefaultGenesis()
}

// InitChainer prepares an abci ResponseInitChain command
func (app *StrongForceApp) InitChainer(ctx types.Context, req abci.RequestInitChain) abci.ResponseInitChain {
	var genesisState GenesisState

	err := app.cdc.UnmarshalJSON(req.AppStateBytes, &genesisState)
	if err != nil {
		panic(err)
	}

	return app.mm.InitGenesis(ctx, genesisState)
}

// BeginBlocker prepares an abci ResponseBeginBlock command
func (app *StrongForceApp) BeginBlocker(ctx types.Context, req abci.RequestBeginBlock) abci.ResponseBeginBlock {
	return app.mm.BeginBlock(ctx, req)
}

// EndBlocker prepares an abci ResponseEndBlock command
func (app *StrongForceApp) EndBlocker(ctx types.Context, req abci.RequestEndBlock) abci.ResponseEndBlock {
	return app.mm.EndBlock(ctx, req)
}

// LoadHeight sets the app to a given height
func (app *StrongForceApp) LoadHeight(height int64) error {
	return app.LoadVersion(height, app.keyMain)
}

// ExportAppStateAndValidators exports the app state and validators
func (app *StrongForceApp) ExportAppStateAndValidators(forZeroHeight bool, jailWhiteList []string,
) (appState json.RawMessage, validators []tendermintTypes.GenesisValidator, err error) {

	// as if they could withdraw from the start of the next block
	ctx := app.NewContext(true, abci.Header{Height: app.LastBlockHeight()})

	genState := app.mm.ExportGenesis(ctx)
	appState, err = codec.MarshalJSONIndent(app.cdc, genState)
	if err != nil {
		return nil, nil, err
	}

	validators = staking.WriteValidators(ctx, app.stakingKeeper)

	return appState, validators, nil
}

// MakeCodec - Creates the codec needed for strongforce
func MakeCodec() *codec.Codec {

	var cdc = codec.New()

	ModuleBasicManager.RegisterCodec(cdc)
	types.RegisterCodec(cdc)
	codec.RegisterCrypto(cdc)

	return cdc
}
