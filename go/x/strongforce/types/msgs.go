package types

import (
	"encoding/json"

	"github.com/cosmos/cosmos-sdk/types"
)

// MsgExecuteAction represents a request to execute an action
type MsgExecuteAction struct {
	Doer   types.AccAddress
	Action []byte
}

// NewMsgExecuteAction is the constructor for MsgExecuteAction
func NewMsgExecuteAction(doer types.AccAddress, action []byte) MsgExecuteAction {
	return MsgExecuteAction{
		Doer:   doer,
		Action: action,
	}
}

// Route - Implements types.Message
func (msg MsgExecuteAction) Route() string { return "strongforce" }

// Type - Implements types.Message
func (msg MsgExecuteAction) Type() string { return "execute_action" }

// ValidateBasic  - Implements types.Message
func (msg MsgExecuteAction) ValidateBasic() types.Error {
	if msg.Doer.Empty() {
		return types.ErrInvalidAddress(msg.Doer.String())
	}
	return nil
}

// GetSignBytes  - Implements types.Message
func (msg MsgExecuteAction) GetSignBytes() []byte {
	b, err := json.Marshal(msg)
	if err != nil {
		panic(err)
	}
	return types.MustSortJSON(b)
}

// GetSigners  - Implements types.Message
func (msg MsgExecuteAction) GetSigners() []types.AccAddress {
	return []types.AccAddress{msg.Doer}
}
