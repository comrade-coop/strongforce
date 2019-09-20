package strongforce

import (
	"encoding/base64"

	"github.com/cosmos/cosmos-sdk/codec"
	sdk "github.com/cosmos/cosmos-sdk/types"
	abci "github.com/tendermint/tendermint/abci/types"
)

// NewQuerier returns a querier for strongforce
func NewQuerier(keeper Keeper) sdk.Querier {
	return func(ctx sdk.Context, path []string, req abci.RequestQuery) ([]byte, sdk.Error) {
		switch path[1] {
		case "state":
			{
				id, err := base64.RawURLEncoding.DecodeString(path[2])
				if err != nil {
					return nil, sdk.ErrInvalidAddress("cannot parse strongforce contract address")
				}
				state := keeper.GetState(ctx, id)
				return state, nil
			}
		case "addresses":
			{
				var addresses []string

				for iterator := keeper.GetContractsIterator(ctx); iterator.Valid(); iterator.Next() {
					address := iterator.Key()
					addresses = append(addresses, base64.RawURLEncoding.EncodeToString(address))
				}

				result, err := codec.MarshalJSONIndent(keeper.cdc, addresses)
				if err != nil {
					return nil, sdk.ErrInternal("could not convert address list to json")
				}

				return result, nil
			}
			// return nil, sdk.ErrUnknownRequest("invalid parameters for strongforce/contract endpoint")
		default:
			return nil, sdk.ErrUnknownRequest("unknown strongforce query endpoint")
		}
	}
}
