package strongforce

import (
	"fmt"

	"github.com/comrade-coop/strongforce/go/x/strongforce/types"
	sdk "github.com/cosmos/cosmos-sdk/types"
)

// NewHandler returns a handler for "strongforce" messages.
func NewHandler(keeper Keeper, serverAddress string) sdk.Handler {
	connection := NewConnection(serverAddress, keeper)
	return func(ctx sdk.Context, msg sdk.Msg) sdk.Result {
		switch msg := msg.(type) {
		case types.MsgExecuteAction:
			return handleMsgExecuteAction(ctx, keeper, connection, msg)
		default:
			errMsg := fmt.Sprintf("Unrecognized strongforce Msg type: %v", msg.Type())
			return sdk.ErrUnknownRequest(errMsg).Result()
		}
	}
}

func handleMsgExecuteAction(ctx sdk.Context, keeper Keeper, connection Connection, msg types.MsgExecuteAction) sdk.Result {
	actionResult := connection.SendAction(ctx, msg.Doer, msg.Action)
	if !actionResult.IsOK() {
		return sdk.ErrInternal("Couldn't execute action!").Result()
	}
	return sdk.Result{Events: actionResult.Events}
}
