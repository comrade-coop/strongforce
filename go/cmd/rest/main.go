package rest

import (
	"encoding/base64"
	"fmt"
	"io/ioutil"
	"net/http"
	"os"

	types "github.com/comrade-coop/strongforce/go/x/strongforce/types"
	"github.com/cosmos/cosmos-sdk/client/context"
	sdk "github.com/cosmos/cosmos-sdk/types"
	"github.com/cosmos/cosmos-sdk/types/rest"
	auth "github.com/cosmos/cosmos-sdk/x/auth/client/rest"
	"github.com/cosmos/cosmos-sdk/x/auth/client/utils"
	authtypes "github.com/cosmos/cosmos-sdk/x/auth/types"
	bank "github.com/cosmos/cosmos-sdk/x/bank/client/rest"
	"github.com/gorilla/mux"
)

const (
	restName = "address"
)

// RegisterRoutes - Central function to define routes that get registered by the main application
func RegisterRoutes(cliCtx context.CLIContext, r *mux.Router, storeName string) {
	r.HandleFunc(fmt.Sprintf("/%s/contract/addresses", storeName), contractAddressesHandler(cliCtx, storeName)).Methods("GET")
	r.HandleFunc(fmt.Sprintf("/%s/contract/amino-action-wrap", storeName), resolveAminoActionWrapHandler(cliCtx, storeName)).Methods("POST")
	r.HandleFunc(fmt.Sprintf("/%s/contract/state/{%s}", storeName, restName), resolveStateHandler(cliCtx, storeName)).Methods("GET")
	r.HandleFunc(fmt.Sprintf("/%s/contract/type/{%s}", storeName, restName), contractTypeHandler(cliCtx, storeName)).Methods("GET")
	r.HandleFunc(fmt.Sprintf("/%s/contract/action", storeName), receiveActionHandler(cliCtx)).Methods("POST")
	auth.RegisterTxRoutes(cliCtx, r)
	bank.RegisterRoutes(cliCtx, r)
}

// --------------------------------------------------------------------------------------
// Tx Handler
type actionReq struct {
	BaseReq rest.BaseReq `json:"base_req"`
	Name    string       `json:"name"`
	Action  []byte       `json:"action"`
}

// ./bin/strongforcecli tx sign unsignedTx.json --from A --offline --chain-id strongforce-test-chain-20336 --sequence 2 --account-number 0 > signedTx.json

// curl -XPOST -s http://localhost:1317/strongforce/contract/action --data-binary '{"base_req": {"from": "cosmos1cd2t4x9w03ul3dghxp04gsw7u8shxzt49vasx7","name": "A","chain_id": "strongforce-test-chain-20336"  }, "action": "eyJUeXBlIjogIkluc3RhbnRpYXRlS2l0IiwgIlRhcmdldHMiOiBbIkFBIl0sICJQYXlsb2FkIjoge319"}' > unsignedTx.json
//
func receiveActionHandler(cliCtx context.CLIContext) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		var req actionReq

		if !rest.ReadRESTReq(w, r, cliCtx.Codec, &req) {
			rest.WriteErrorResponse(w, http.StatusBadRequest, "failed to parse request")
			return
		}

		baseReq := req.BaseReq.Sanitize()
		if !baseReq.ValidateBasic(w) {
			return
		}

		addr, err := sdk.AccAddressFromBech32(baseReq.From)
		if err != nil {
			rest.WriteErrorResponse(w, http.StatusBadRequest, err.Error())
			return
		}

		cliCtx.FromAddress = addr

		// create the message
		msg := types.NewMsgExecuteAction(addr, req.Action)
		err = msg.ValidateBasic()
		if err != nil {
			rest.WriteErrorResponse(w, http.StatusBadRequest, err.Error())
			return
		}

		//Comment to run transactions
		utils.WriteGenerateStdTxResponse(w, cliCtx, baseReq, []sdk.Msg{msg})
		//Uncomment to run transactions

		// gasAdj, ok := rest.ParseFloat64OrReturnBadRequest(w, baseReq.GasAdjustment, flags.DefaultGasAdjustment)
		// if !ok {
		// 	return
		// }

		// simAndExec, gas, err := flags.ParseGas(baseReq.Gas)
		// if err != nil {
		// 	rest.WriteErrorResponse(w, http.StatusBadRequest, err.Error())
		// 	return
		// }

		// cliCtx.FromName = req.Name
		// cliCtx.SkipConfirm = true

		// txBldr := authtypes.NewTxBuilder(
		// 	utils.GetTxEncoder(cliCtx.Codec), baseReq.AccountNumber, baseReq.Sequence, gas, gasAdj,
		// 	baseReq.Simulate || simAndExec, baseReq.ChainID, baseReq.Memo, baseReq.Fees, baseReq.GasPrices,
		// )
		// err = CompleteAndBroadcastTxCLI(txBldr, cliCtx, []sdk.Msg{msg})

		// rest.WriteErrorResponse(w, http.StatusBadRequest, err.Error())
	}
}

func CompleteAndBroadcastTxCLI(txBldr authtypes.TxBuilder, cliCtx context.CLIContext, msgs []sdk.Msg) error {
	txBldr, err := utils.PrepareTxBuilder(txBldr, cliCtx)
	if err != nil {
		return err
	}

	fromName := cliCtx.GetFromName()

	if txBldr.SimulateAndExecute() || cliCtx.Simulate {
		txBldr, err = utils.EnrichWithGas(txBldr, cliCtx, msgs)
		if err != nil {
			return err
		}

		gasEst := utils.GasEstimateResponse{GasEstimate: txBldr.Gas()}
		_, _ = fmt.Fprintf(os.Stderr, "%s\n", gasEst.String())
	}

	if cliCtx.Simulate {
		return nil
	}

	passphrase := "testpass"

	// build and sign the transaction
	txBytes, err := txBldr.BuildAndSign(fromName, passphrase, msgs)
	if err != nil {
		return err
	}
	cliCtx.BroadcastMode = "async"
	// broadcast to a Tendermint node
	res, err := cliCtx.BroadcastTx(txBytes)
	if err != nil {
		return err
	}

	return cliCtx.PrintOutput(res)
}

//--------------------------------------------------------------------------------------
// Query Handlers
func resolveAminoActionWrapHandler(cliCtx context.CLIContext, storeName string) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		// vars := mux.Vars(r)
		// txBytes := []byte(vars[restName])
		// // txBytes, err := base64.RawURLEncoding.DecodeString(paramType)
		// print(fmt.Sprintf("JSON %v", txBytes))
		// // if err != nil {
		// // 	rest.WriteErrorResponse(w, http.StatusExpectationFailed, err.Error())
		// // 	return
		// // }
		body, err := ioutil.ReadAll(r.Body)

		res, _, err := cliCtx.QueryWithData(fmt.Sprintf("custom/%s/contract/amino-action-wrap", storeName), body)
		if err != nil {
			rest.WriteErrorResponse(w, http.StatusNotFound, err.Error())
			return
		}

		rest.PostProcessResponse(w, cliCtx, base64.StdEncoding.EncodeToString(res))
	}
}

func resolveStateHandler(cliCtx context.CLIContext, storeName string) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		vars := mux.Vars(r)
		paramType := vars[restName]

		res, _, err := cliCtx.QueryWithData(fmt.Sprintf("custom/%s/contract/state/%s", storeName, paramType), nil)
		if err != nil {
			rest.WriteErrorResponse(w, http.StatusNotFound, err.Error())
			return
		}

		rest.PostProcessResponse(w, cliCtx, res)
	}
}

func contractAddressesHandler(cliCtx context.CLIContext, storeName string) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		res, _, err := cliCtx.QueryWithData(fmt.Sprintf("custom/%s/contract/addresses", storeName), nil)
		if err != nil {
			rest.WriteErrorResponse(w, http.StatusNotFound, err.Error())
			return
		}
		rest.PostProcessResponse(w, cliCtx, res)
	}
}

func contractTypeHandler(cliCtx context.CLIContext, storeName string) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		vars := mux.Vars(r)
		paramType := vars[restName]
		res, _, err := cliCtx.QueryWithData(fmt.Sprintf("custom/%s/contract/type/%s", storeName, paramType), nil)
		if err != nil {
			rest.WriteErrorResponse(w, http.StatusNotFound, err.Error())
			return
		}
		rest.PostProcessResponse(w, cliCtx, res)
	}
}
