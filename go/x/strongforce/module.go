package strongforce

import (
	"encoding/json"

	"github.com/comrade-coop/strongforce/go/cmd/rest"
	"github.com/cosmos/cosmos-sdk/client/context"
	"github.com/cosmos/cosmos-sdk/codec"
	"github.com/cosmos/cosmos-sdk/types"
	sdk "github.com/cosmos/cosmos-sdk/types"
	"github.com/cosmos/cosmos-sdk/types/module"
	"github.com/gorilla/mux"
	"github.com/spf13/cobra"
	abci "github.com/tendermint/tendermint/abci/types"
)

var (
	_ module.AppModule      = AppModule{}
	_ module.AppModuleBasic = AppModuleBasic{}

	moduleCdc = codec.New()
)

// AppModuleBasic object
type AppModuleBasic struct {
	keeper Keeper
}

// Name implements module.AppModuleBasic
func (AppModuleBasic) Name() string {
	return ModuleName
}

// RegisterCodec implements module.AppModuleBasic
func (AppModuleBasic) RegisterCodec(cdc *codec.Codec) {
	RegisterCodec(cdc)
}

// DefaultGenesis implements module.AppModuleBasic
func (AppModuleBasic) DefaultGenesis() json.RawMessage {
	return moduleCdc.MustMarshalJSON(DefaultGenesisState())
}

// ValidateGenesis implements module.AppModuleBasic
func (AppModuleBasic) ValidateGenesis(bz json.RawMessage) error {
	var data GenesisState
	err := moduleCdc.UnmarshalJSON(bz, &data)
	if err != nil {
		return err
	}
	// once json successfully marshalled, passes along to genesis.go
	return ValidateGenesis(data)
}

// RegisterRESTRoutes implements module.AppModuleBasic
func (AppModuleBasic) RegisterRESTRoutes(ctx context.CLIContext, rtr *mux.Router) {
	rest.RegisterRoutes(ctx, rtr, StoreKey)
}

// GetQueryCmd implements module.AppModuleBasic
func (ab AppModuleBasic) GetQueryCmd(cdc *codec.Codec) *cobra.Command {
	return GetQueryCmd(RouterKey, cdc)
}

// GetTxCmd implements module.AppModuleBasic
func (ab AppModuleBasic) GetTxCmd(cdc *codec.Codec) *cobra.Command {
	return GetTxCmd(cdc)
}

// AppModule implements module.AppModule
type AppModule struct {
	AppModuleBasic
	backendURL string
}

// NewAppModule creates a new AppModule Object
func NewAppModule(k Keeper, backendURL string) AppModule {
	return AppModule{
		AppModuleBasic: AppModuleBasic{
			keeper: k,
		},
		backendURL: backendURL,
	}
}

// Name implements module.AppModule
func (AppModule) Name() string {
	return ModuleName
}

// RegisterInvariants implements module.AppModule
func (am AppModule) RegisterInvariants(ir types.InvariantRegistry) {}

// Route implements module.AppModule
func (am AppModule) Route() string {
	return RouterKey
}

// NewHandler implements module.AppModule
func (am AppModule) NewHandler() types.Handler {
	return NewHandler(am.keeper, "localhost:8989")
}

// QuerierRoute implements module.AppModule
func (am AppModule) QuerierRoute() string {
	return ModuleName
}

// NewQuerierHandler implements module.AppModule
func (am AppModule) NewQuerierHandler() types.Querier {
	return NewQuerier(am.keeper)
}

// BeginBlock implements module.AppModule
func (am AppModule) BeginBlock(_ sdk.Context, _ abci.RequestBeginBlock) {}

// EndBlock implements module.AppModule
func (am AppModule) EndBlock(sdk.Context, abci.RequestEndBlock) []abci.ValidatorUpdate {
	return []abci.ValidatorUpdate{}
}

// InitGenesis implements module.AppModule
func (am AppModule) InitGenesis(ctx types.Context, data json.RawMessage) []abci.ValidatorUpdate {
	var genesisState GenesisState
	moduleCdc.MustUnmarshalJSON(data, &genesisState)
	return InitGenesis(ctx, am.keeper, genesisState)
}

// ExportGenesis implements module.AppModule
func (am AppModule) ExportGenesis(ctx types.Context) json.RawMessage {
	gs := ExportGenesis(ctx, am.keeper)
	return moduleCdc.MustMarshalJSON(gs)
}
