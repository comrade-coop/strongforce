package strongforce

import (
	"encoding/base64"
	"strings"

	"github.com/cosmos/cosmos-sdk/codec"
	sdk "github.com/cosmos/cosmos-sdk/types"
	authtypes "github.com/cosmos/cosmos-sdk/x/auth/types"
	abci "github.com/tendermint/tendermint/abci/types"
)

// NewQuerier returns a querier for strongforce
func NewQuerier(keeper Keeper) sdk.Querier {
	return func(ctx sdk.Context, path []string, req abci.RequestQuery) ([]byte, sdk.Error) {
		switch path[1] {
		case "state":
			{
				addresses := strings.Split(path[2], "+")
				stateMap := map[string][]byte{}
				for _, element := range addresses {
					id, err := base64.RawURLEncoding.DecodeString(element)
					if err != nil {
						return nil, sdk.ErrInvalidAddress("cannot parse strongforce contract address")
					}
					state := keeper.GetState(ctx, id)
					stateMap[element] = state
				}
				result, err := codec.MarshalJSONIndent(keeper.cdc, stateMap)

				if err != nil {
					return nil, sdk.ErrInternal("could not convert address list to json")
				}

				return result, nil
			}
		case "addresses":
			{
				var addresses []string

				for iterator := keeper.GetContractsStateIterator(ctx); iterator.Valid(); iterator.Next() {
					address := iterator.Key()
					addresses = append(addresses, base64.RawURLEncoding.EncodeToString(address))
				}

				result, err := codec.MarshalJSONIndent(keeper.cdc, addresses)
				if err != nil {
					return nil, sdk.ErrInternal("could not convert address list to json")
				}

				return result, nil
			}
		case "type":
			{
				var addresses []string

				for iterator := keeper.GetContractsTypeIterator(ctx); iterator.Valid(); iterator.Next() {
					address := iterator.Key()
					value := string(iterator.Value())
					if value == path[2] {
						addresses = append(addresses, base64.RawURLEncoding.EncodeToString(address))
					}
				}

				result, err := codec.MarshalJSONIndent(keeper.cdc, addresses)
				if err != nil {
					return nil, sdk.ErrInternal("could not convert address list to json")
				}

				return result, nil
			}
		case "amino-action-wrap":
			{
				var stdTx authtypes.StdTx
				str := string(req.Data)
				x, err := base64.RawURLEncoding.DecodeString(str)
				keeper.cdc.MustUnmarshalJSON(x, &stdTx)
				txBytes, err := keeper.cdc.MarshalBinaryLengthPrefixed(stdTx)
				if err != nil {
					return nil, sdk.ErrInternal("could not encode Action to amino")
				}
				//encoded := []byte(base64.StdEncoding.EncodeToString(txBytes))
				return txBytes, nil
			}
			// return nil, sdk.ErrUnknownRequest("invalid parameters for strongforce/contract endpoint")
		default:
			return nil, sdk.ErrUnknownRequest("unknown strongforce query endpoint")
		}
	}
}
