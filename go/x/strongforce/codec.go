package strongforce

import (
	"github.com/cosmos/cosmos-sdk/codec"
)

// RegisterCodec registers strongforce types in the codec
func RegisterCodec(cdc *codec.Codec) {
	cdc.RegisterConcrete(MsgExecuteAction{}, "strongforce/SetName", nil)
}
