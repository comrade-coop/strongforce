module github.com/comrade-coop/strongforce/go

go 1.12

require (
	github.com/cosmos/cosmos-sdk v0.34.5
	github.com/golang/protobuf v1.3.1
	github.com/spf13/cobra v0.0.3
	github.com/spf13/viper v1.0.3
	github.com/tendermint/tendermint v0.31.5
	google.golang.org/grpc v1.19.0
)

// see also https://github.com/cosmos/cosmos-sdk/issues/3129
replace golang.org/x/crypto => github.com/tendermint/crypto v0.0.0-20180820045704-3764759f34a5
