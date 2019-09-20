package main

import (
	"encoding/json"
	"io"
	"os"

	"github.com/spf13/cobra"

	abci "github.com/tendermint/tendermint/abci/types"
	"github.com/tendermint/tendermint/libs/cli"
	"github.com/tendermint/tendermint/libs/log"
	tendermintTypes "github.com/tendermint/tendermint/types"
	db "github.com/tendermint/tm-db"

	app "github.com/comrade-coop/strongforce/go"
	"github.com/cosmos/cosmos-sdk/server"
	"github.com/cosmos/cosmos-sdk/x/genaccounts"
	genaccountscli "github.com/cosmos/cosmos-sdk/x/genaccounts/client/cli"
	genutilcli "github.com/cosmos/cosmos-sdk/x/genutil/client/cli"
	"github.com/cosmos/cosmos-sdk/x/staking"
)

// DefaultNodeHome sets the folder where the application data and configuration will be stored
var DefaultNodeHome = os.ExpandEnv("$HOME/.strongforced")

const (
	flagOverwrite = "overwrite"
)

func main() {
	cobra.EnableCommandSorting = false

	cdc := app.MakeCodec()
	ctx := server.NewDefaultContext()

	rootCmd := &cobra.Command{
		Use:               "strongforced",
		Short:             "strongforce App Daemon (server)",
		PersistentPreRunE: server.PersistentPreRunEFn(ctx),
	}

	rootCmd.AddCommand(
		genutilcli.InitCmd(ctx, cdc, app.ModuleBasicManager, app.DefaultNodeHome),
		genutilcli.CollectGenTxsCmd(ctx, cdc, genaccounts.AppModuleBasic{}, app.DefaultNodeHome),
		genutilcli.GenTxCmd(ctx, cdc, app.ModuleBasicManager, staking.AppModuleBasic{}, genaccounts.AppModuleBasic{}, app.DefaultNodeHome, app.DefaultCLIHome),
		genutilcli.ValidateGenesisCmd(ctx, cdc, app.ModuleBasicManager),
		// AddGenesisAccountCmd allows users to add accounts to the genesis file
		genaccountscli.AddGenesisAccountCmd(ctx, cdc, app.DefaultNodeHome, app.DefaultCLIHome),
	)
	server.AddCommands(ctx, cdc, rootCmd, newApp, exportAppStateAndValidators)

	// prepare and add flags
	executor := cli.PrepareBaseCmd(rootCmd, "SF", DefaultNodeHome)
	err := executor.Execute()
	if err != nil {
		print()

		// handle with #870
		panic(err)
	}
}

func newApp(logger log.Logger, db db.DB, traceStore io.Writer) abci.Application {
	return app.NewStrongForceApp(logger, db)
}

func exportAppStateAndValidators(
	logger log.Logger, db db.DB, traceStore io.Writer, height int64, forZeroHeight bool, jailWhiteList []string,
) (json.RawMessage, []tendermintTypes.GenesisValidator, error) {

	strongforceApp := app.NewStrongForceApp(logger, db)

	if height != -1 {
		err := strongforceApp.LoadHeight(height)
		if err != nil {
			return nil, nil, err
		}
	}

	return strongforceApp.ExportAppStateAndValidators(forZeroHeight, jailWhiteList)
}
