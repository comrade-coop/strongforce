package strongforce

import (
	"bytes"
	"fmt"
	"encoding/json"

	"github.com/spf13/cobra"

	"github.com/cosmos/cosmos-sdk/client"
	"github.com/cosmos/cosmos-sdk/client/context"
	"github.com/cosmos/cosmos-sdk/codec"
)

// GetQueryCmd generates the entrypoint for the strongforce module
func GetQueryCmd(queryRoute string, cdc *codec.Codec) *cobra.Command {
	strongforceQueryCmd := &cobra.Command{
		Use:                        ModuleName,
		Short:                      "Strongforce query subcommands",
		DisableFlagParsing:         true,
		SuggestionsMinimumDistance: 2,
		RunE:                       client.ValidateCmd,
	}

	strongforceQueryCmd.AddCommand(client.GetCommands(
		GetCmdGetState(queryRoute, cdc),
		GetCmdListState(queryRoute, cdc),
	)...)

	return strongforceQueryCmd
}

func GetCmdGetState(queryRoute string, cdc *codec.Codec) *cobra.Command {
	return &cobra.Command{
		Use:   "state [address]",
		Short: "get the state of a contract at the address",
		Args:  cobra.ExactArgs(1),
		RunE: func(cmd *cobra.Command, args []string) error {
			cliCtx := context.NewCLIContext().WithCodec(cdc)
			address := args[0]

			res, _, err := cliCtx.QueryWithData(fmt.Sprintf("custom/%s/contract/%s", queryRoute, address), nil)
			if err != nil {
				fmt.Printf("could not state address - %s \n", address)
				fmt.Println(err.Error())
				return nil
			}

			var out bytes.Buffer
			err = json.Indent(&out, res, "", "  ")
			if err != nil {
				fmt.Println(err.Error())
				return nil
			}

			fmt.Println(string(out.Bytes()))
			return nil
		},
	}
}

func GetCmdListState(queryRoute string, cdc *codec.Codec) *cobra.Command {
	return &cobra.Command{
		Use:   "addresses",
		Short: "list the addresses of all contracts present",
		Args:  cobra.ExactArgs(0),
		RunE: func(cmd *cobra.Command, args []string) error {
			cliCtx := context.NewCLIContext().WithCodec(cdc)

			res, _, err := cliCtx.QueryWithData(fmt.Sprintf("custom/%s/contract", queryRoute), nil)
			if err != nil {
				fmt.Println("could not list addresses")
				fmt.Println(err.Error())
				return nil
			}

			fmt.Println(string(res))
			return nil
		},
	}
}
