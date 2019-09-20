package strongforce

import (
	"github.com/comrade-coop/strongforce/go/x/strongforce/types"
)

const (
	ModuleName = types.ModuleName
	RouterKey  = types.RouterKey
	StoreKey   = types.StoreKey
)

var (
	NewMsgExecuteAction = types.NewMsgExecuteAction
	RegisterCodec       = types.RegisterCodec
	GetQueryCmd         = types.GetQueryCmd
	GetTxCmd            = types.GetTxCmd
)

type (
	MsgExecuteAction = types.MsgExecuteAction
)
