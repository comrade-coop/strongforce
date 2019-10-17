package strongforce

import (
	"encoding/base64"
	"io"

	"github.com/cosmos/cosmos-sdk/types"
	"github.com/tendermint/tendermint/libs/common"
	"google.golang.org/grpc"
)

//go:generate protoc --go_out=plugins=grpc:. -I=../../ ../../StrongForce.proto

// Connection represents a connection to the .NET StrongForce app
type Connection struct {
	serverAddress string
	keeper        Keeper

	client  StrongForceClient
	channel *grpc.ClientConn
}

// NewConnection creates a new connection to the .NET StrongForce app
func NewConnection(serverAddress string, keeper Keeper) Connection {
	return Connection{
		serverAddress: serverAddress,
		keeper:        keeper,
	}
}

func (c *Connection) ensureConnected() bool {
	if c.client == nil {
		var err error
		c.channel, err = grpc.Dial(c.serverAddress, grpc.WithInsecure())
		if err != nil {
			println("Error while connecting: " + err.Error())
			return false
		}
		c.client = NewStrongForceClient(c.channel)
	}
	return true
}

// SendAction sends an action over the connection
func (c *Connection) SendAction(ctx types.Context, from types.AccAddress, action []byte) types.Result {
	if !c.ensureConnected() {
		panic("Could not connect to the .NET StrongForce app!")
	}
	goContext := ctx.Context()
	stream, err := c.client.ExecuteAction(goContext)
	if err != nil {
		println("Error while communicating to .NET: " + err.Error())
		return types.Result{Code: types.CodeInternal, Codespace: "Error while communicating to .NET"}
	}

	waitc := make(chan types.Result)
	events := types.EmptyEvents()
	go func() {
		defer func() {
			if r := recover(); r != nil {
				waitc <- types.Result{Code: types.CodeInternal, Codespace: "Error while communicating to .NET"}
				close(waitc)
				stream.CloseSend()
				return
			}
		}()

		for {
			request, err := stream.Recv()
			if err == io.EOF {
				waitc <- types.Result{Code: types.CodeOK, Events: events}
				close(waitc)
				stream.CloseSend()
				return
			}
			if err != nil {
				println("Error while communicating to .NET: " + err.Error())
				waitc <- types.Result{Code: types.CodeInternal, Codespace: "Error while communicating to .NET"}
				close(waitc)
				return
			}
			if request.Data == nil {
				err = stream.Send(&ActionOrContract{
					Data: &ActionOrContract_Contract{
						&ContractResponse{
							Address: request.Address,
							Data:    c.keeper.GetState(ctx, request.Address),
						},
					},
				})
				if err != nil {
					println("Error while communicating to .NET: " + err.Error())
					waitc <- types.Result{Code: types.CodeInternal, Codespace: "Error while communicating to .NET"}
					close(waitc)
					return
				}
			} else {
				events = events.AppendEvent(
					types.Event{
						Type: "ContractStateUpdate",
						Attributes: []common.KVPair{
							{Key: []byte("Address"), Value: []byte(base64.RawURLEncoding.EncodeToString(request.Address))},
							{Key: []byte("State"), Value: request.Data},
						},
					},
				)
				c.keeper.SetState(ctx, request.Address, request.Data, request.TypeName)
			}
		}
	}()

	if err := stream.Send(&ActionOrContract{
		Data: &ActionOrContract_Action{
			&Action{
				Address: from,
				Data:    action,
			},
		},
	}); err != nil {
		println("Error while communicating to .NET: " + err.Error())
		return types.Result{Code: types.CodeInternal, Codespace: "Error while communicating to .NET"}
	}

	return <-waitc
}
