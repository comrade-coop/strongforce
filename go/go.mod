module github.com/comrade-coop/strongforce/go

go 1.12

// see also https://github.com/cosmos/cosmos-sdk/issues/3129
replace golang.org/x/crypto => github.com/tendermint/crypto v0.0.0-20180820045704-3764759f34a5

// replace github.com/cosmos/cosmos-sdk => github.com/cosmos/cosmos-sdk v0.0.0-20190620122648-6d5ca0b4f17e
replace github.com/cosmos/cosmos-sdk => github.com/cosmos/cosmos-sdk v0.28.2-0.20190616100639-18415eedaf25

require (
	bou.ke/monkey v1.0.1 // indirect
	github.com/cosmos/cosmos-sdk v0.0.0-20190605094302-3180e68c7b57
	github.com/golang/protobuf v1.3.0
	github.com/gorilla/mux v1.7.0
	github.com/otiai10/copy v0.0.0-20180813032824-7e9a647135a1 // indirect
	github.com/otiai10/curr v0.0.0-20150429015615-9b4961190c95 // indirect
	github.com/otiai10/mint v1.2.3 // indirect
	github.com/spf13/cobra v0.0.3
	github.com/spf13/viper v1.0.3
	github.com/tendermint/go-amino v0.15.0
	github.com/tendermint/tendermint v0.31.5
	google.golang.org/grpc v1.19.0
)
