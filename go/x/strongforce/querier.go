package strongforce

import (
	"encoding/base64"

	"github.com/cosmos/cosmos-sdk/codec"
	"github.com/cosmos/cosmos-sdk/types"
	abci "github.com/tendermint/tendermint/abci/types"
)

// NewQuerier returns a querier for strongforce
func NewQuerier(keeper Keeper) types.Querier {
	return func(ctx types.Context, path []string, req abci.RequestQuery) ([]byte, types.Error) {
		switch path[0] {
		case "contract":
			if len(path) == 2 {
				id, err := base64.RawURLEncoding.DecodeString(path[1])
				if err != nil {
					return nil, types.ErrInvalidAddress("cannot parse strongforce contract address")
				}
				state := keeper.GetState(ctx, id)
				return state, nil
			}
			if len(path) == 1 {
				var addresses []string

				for iterator := keeper.GetContractsIterator(ctx); iterator.Valid(); iterator.Next() {
					address := iterator.Key()
					addresses = append(addresses, base64.RawURLEncoding.EncodeToString(address))
				}

				result, err := codec.MarshalJSONIndent(keeper.cdc, addresses)
				if err != nil {
					return nil, types.ErrInternal("could not convert address list to json")
				}

				return result, nil
			}
			return nil, types.ErrUnknownRequest("invalid parameters for strongforce/contract endpoint")
		default:
			return nil, types.ErrUnknownRequest("unknown strongforce query endpoint")
		}
	}
}
