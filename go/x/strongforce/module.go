package strongforce

import (
	"encoding/json"

	"github.com/gorilla/mux"
	"github.com/spf13/cobra"

	"github.com/cosmos/cosmos-sdk/client/context"
	"github.com/cosmos/cosmos-sdk/codec"
	"github.com/cosmos/cosmos-sdk/types"
	"github.com/cosmos/cosmos-sdk/types/module"

	abci "github.com/tendermint/tendermint/abci/types"
)

var (
	_ module.AppModule      = AppModule{}
	_ module.AppModuleBasic = AppModuleBasic{}

	moduleCdc = codec.New()
)

// ModuleName - the name of the module
const ModuleName = "strongforce"

// RouterKey - the key used for routes
const RouterKey = "strongforce"

// StoreKey - the key used for state storage
const StoreKey = "strongforce"

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
	// pass
}

// GetQueryCmd implements module.AppModuleBasic
func (ab AppModuleBasic) GetQueryCmd(cdc *codec.Codec) *cobra.Command {
	return GetQueryCmd(ab.keeper, cdc)
}

// GetTxCmd implements module.AppModuleBasic
func (ab AppModuleBasic) GetTxCmd(cdc *codec.Codec) *cobra.Command {
	return GetTxCmd(ab.keeper, cdc)
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
func (am AppModule) BeginBlock(_ types.Context, _ abci.RequestBeginBlock) types.Tags {
	return types.EmptyTags()
}

// EndBlock implements module.AppModule
func (am AppModule) EndBlock(types.Context, abci.RequestEndBlock) ([]abci.ValidatorUpdate, types.Tags) {
	return []abci.ValidatorUpdate{}, types.EmptyTags()
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
