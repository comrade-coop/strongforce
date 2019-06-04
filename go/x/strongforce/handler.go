package strongforce

import (
	"fmt"

	"github.com/cosmos/cosmos-sdk/types"
)

// NewHandler returns a handler for "strongforce" messages.
func NewHandler(keeper Keeper) types.Handler {
	return func(ctx types.Context, msg types.Msg) types.Result {
		switch msg := msg.(type) {
		case MsgExecuteAction:
			return handleMsgExecuteAction(ctx, keeper, msg)
		default:
			errMsg := fmt.Sprintf("Unrecognized strongforce Msg type: %v", msg.Type())
			return types.ErrUnknownRequest(errMsg).Result()
		}
	}
}

func handleMsgExecuteAction(ctx types.Context, keeper Keeper, msg MsgExecuteAction) types.Result {
	println("Oof, unimplemented!")
	return types.Result{}
}
