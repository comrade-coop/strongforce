package strongforce

import (
	"github.com/spf13/cobra"

	"github.com/cosmos/cosmos-sdk/client"
	"github.com/cosmos/cosmos-sdk/codec"
)

// GetQueryCmd generates the entrypoint for the strongforce module
func GetQueryCmd(keeper Keeper, cdc *codec.Codec) *cobra.Command {
	strongforceQueryCmd := &cobra.Command{
		Use:                        ModuleName,
		Short:                      "Strongforce query subcommands",
		DisableFlagParsing:         true,
		SuggestionsMinimumDistance: 2,
		RunE:                       client.ValidateCmd,
	}

	strongforceQueryCmd.AddCommand(client.GetCommands()...)

	return strongforceQueryCmd
}
