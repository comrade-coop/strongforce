package strongforce

import (
	"github.com/cosmos/cosmos-sdk/types"
	abci "github.com/tendermint/tendermint/abci/types"
)

// NewQuerier returns a querier for strongforce
func NewQuerier(keeper Keeper) types.Querier {
	return func(ctx types.Context, path []string, req abci.RequestQuery) (res []byte, err types.Error) {
		switch path[0] {
		case "hello-world":
			return []byte("Hello world!"), nil
		default:
			return nil, types.ErrUnknownRequest("unknown strongforce query endpoint")
		}
	}
}
