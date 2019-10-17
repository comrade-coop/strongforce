package strongforce

import (
	"fmt"

	sdk "github.com/cosmos/cosmos-sdk/types"
	abci "github.com/tendermint/tendermint/abci/types"
)

// GenesisContractData contains the genesis information of a strongforce contract
type GenesisContractData struct {
	Address  []byte `json:"address"`
	Data     []byte `json:"data"`
	TypeName []byte `json:"typeName"`
}

// GenesisState contains the genesis information of a strongforce network
type GenesisState struct {
	ContractData []GenesisContractData `json:"contracts"`
}

// NewGenesisState constructs a new GenesisState
func NewGenesisState(c []GenesisContractData) GenesisState {
	return GenesisState{ContractData: c}
}

// ValidateGenesis validates a GenesisState
func ValidateGenesis(data GenesisState) error {
	for _, record := range data.ContractData {
		if record.Address == nil {
			return fmt.Errorf("Invalid GenesisContractData: Data: %s. Error: Missing Address", record.Data)
		}
		if record.Data == nil {
			return fmt.Errorf("Invalid GenesisContractData: Address: %s. Error: Missing Data", record.Address)
		}
	}
	return nil
}

// DefaultGenesisState creates a default GenesisState
func DefaultGenesisState() GenesisState {
	return GenesisState{
		ContractData: []GenesisContractData{},
	}
}

// InitGenesis initializes a Keeper with a GenesisState
func InitGenesis(ctx sdk.Context, keeper Keeper, data GenesisState) []abci.ValidatorUpdate {
	for _, record := range data.ContractData {
		keeper.SetState(ctx, record.Address, record.Data, record.TypeName)
	}
	return []abci.ValidatorUpdate{}
}

// ExportGenesis gets the GenesisState from a Keeper
func ExportGenesis(ctx sdk.Context, k Keeper) GenesisState {
	var records []GenesisContractData
	for iterator := k.GetContractsStateIterator(ctx); iterator.Valid(); iterator.Next() {
		address := iterator.Key()
		data := k.GetState(ctx, address)
		typeName := k.GetType(ctx, address)
		records = append(records, GenesisContractData{
			Address:  address,
			Data:     data,
			TypeName: typeName,
		})
	}
	return GenesisState{
		ContractData: records,
	}
}
