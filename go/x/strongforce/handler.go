package strongforce

import (
	"fmt"

	"github.com/cosmos/cosmos-sdk/types"
)

// NewHandler returns a handler for "strongforce" messages.
func NewHandler(keeper Keeper, serverAddress string) types.Handler {
	connection := NewConnection(serverAddress, keeper)
	return func(ctx types.Context, msg types.Msg) types.Result {
		switch msg := msg.(type) {
		case MsgExecuteAction:
			return handleMsgExecuteAction(ctx, keeper, connection, msg)
		default:
			errMsg := fmt.Sprintf("Unrecognized strongforce Msg type: %v", msg.Type())
			return types.ErrUnknownRequest(errMsg).Result()
		}
	}
}

func handleMsgExecuteAction(ctx types.Context, keeper Keeper, connection Connection, msg MsgExecuteAction) types.Result {
	if !connection.SendAction(ctx, msg.Doer, msg.Action) {
		return types.ErrInternal("Couldn't execute action!").Result()
	}
	return types.Result{}
}
