package strongforce

import (
	"io"
	"google.golang.org/grpc"
	"github.com/cosmos/cosmos-sdk/types"
)
//go:generate protoc --go_out=plugins=grpc:. -I=../../ ../../StrongForce.proto

// Connection represents a connection to the .NET StrongForce app
type Connection struct {
	serverAddress string
	keeper Keeper

	client StrongForceClient
}

// NewConnection creates a new connection to the .NET StrongForce app
func NewConnection(serverAddress string, keeper Keeper) Connection {
	return Connection{
		serverAddress: serverAddress,
		keeper: keeper,
	}
}

func (c Connection) ensureConnected() bool {
	if c.client == nil {
		channel, err := grpc.Dial(c.serverAddress)
		if err == nil {
			return false
		}
		c.client = NewStrongForceClient(channel)
	}
	return true
}

// SendAction sends an action over the connection
func (c Connection) SendAction(ctx types.Context, from types.AccAddress, action []byte) bool {
	if !c.ensureConnected() {
		panic("Could not connect to the .NET StrongForce app!")
	}

	stream, err := c.client.ExecuteAction(ctx)
	if err != nil {
		return false
	}

	waitc := make(chan bool)

	go func() {
		for {
			request, err := stream.Recv()
			if err == io.EOF {
				waitc <- true
				close(waitc)
				stream.CloseSend()
				return
			}
			if err != nil {
				waitc <- false
				close(waitc)
				return
			}
			if request.Data == nil {
				err = stream.Send(&ActionOrContract {
					Data: &ActionOrContract_Contract {
						&ContractResponse {
							Address: request.Address,
							Data: c.keeper.GetState(ctx, request.Address),
						},
					},
				})
				if err != nil {
					waitc <- false
					close(waitc)
					return
				}
			} else {
				c.keeper.SetState(ctx, request.Address, request.Data)
			}
		}
	}()

	if err := stream.Send(&ActionOrContract {
		Data: &ActionOrContract_Action {
			&Action {
				Address: from,
				Data: action,
			},
		},
	}); err != nil {
		return false
	}
	// stream.CloseSend()
	return <-waitc
}
