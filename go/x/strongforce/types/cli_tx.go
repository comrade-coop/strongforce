package types

import (
	"github.com/spf13/cobra"

	"github.com/cosmos/cosmos-sdk/client"
	"github.com/cosmos/cosmos-sdk/client/context"
	"github.com/cosmos/cosmos-sdk/codec"

	"github.com/cosmos/cosmos-sdk/types"
	"github.com/cosmos/cosmos-sdk/x/auth"
	"github.com/cosmos/cosmos-sdk/x/auth/client/utils"
)

// GetTxCmd generates the entrypoint for the strongforce module
func GetTxCmd(cdc *codec.Codec) *cobra.Command {
	strongforceTxCmd := &cobra.Command{
		Use:                        ModuleName,
		Short:                      "Strongforce transaction subcommands",
		DisableFlagParsing:         true,
		SuggestionsMinimumDistance: 2,
		RunE:                       client.ValidateCmd,
	}

	strongforceTxCmd.AddCommand(client.PostCommands(
		GetCmdExecuteAction(cdc),
	)...)
	return strongforceTxCmd
}

// GetCmdExecuteAction is the CLI command for sending a ExecuteAction transaction
func GetCmdExecuteAction(cdc *codec.Codec) *cobra.Command {
	return &cobra.Command{
		Use:   "execute-action [name] [value]",
		Short: "execute an action",
		Args:  cobra.ExactArgs(1),
		RunE: func(cmd *cobra.Command, args []string) error {
			cliCtx := context.NewCLIContext().WithCodec(cdc)
			//.WithAccountDecoder(cdc)

			txBldr := auth.NewTxBuilderFromCLI().WithTxEncoder(utils.GetTxEncoder(cdc))

			msg := NewMsgExecuteAction(cliCtx.GetFromAddress(), []byte(args[0]))
			err := msg.ValidateBasic()
			if err != nil {
				return err
			}

			return utils.GenerateOrBroadcastMsgs(cliCtx, txBldr, []types.Msg{msg})
		},
	}
}
