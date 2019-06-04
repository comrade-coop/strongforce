package strongforce

import (
	"github.com/cosmos/cosmos-sdk/codec"
	"github.com/cosmos/cosmos-sdk/types"
)

// Keeper is the keeper for strongforce
type Keeper struct {
	cdc      *codec.Codec
	storeKey types.StoreKey
}

// NewKeeper creates a new keeper for strongforce
func NewKeeper(cdc *codec.Codec, storeKey types.StoreKey) Keeper {
	return Keeper{
		cdc:      cdc,
		storeKey: storeKey,
	}
}

// SetState sets the state of a contract
func (k Keeper) SetState(ctx types.Context, id []byte, data []byte) {
	store := ctx.KVStore(k.storeKey)
	store.Set(id, data)
}

// GetState sets the state of a contract
func (k Keeper) GetState(ctx types.Context, id []byte) []byte {
	store := ctx.KVStore(k.storeKey)
	return store.Get(id)
}
