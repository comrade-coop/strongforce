module github.com/comrade-coop/strongforce/go

go 1.12

// see also https://github.com/cosmos/cosmos-sdk/issues/3129
// replace golang.org/x/crypto => github.com/tendermint/crypto v0.0.0-20180820045704-3764759f34a5

// replace github.com/cosmos/cosmos-sdk => github.com/cosmos/cosmos-sdk v0.0.0-20190620122648-6d5ca0b4f17e
// replace github.com/cosmos/cosmos-sdk => github.com/cosmos/cosmos-sdk v0.28.2-0.20190616100639-18415eedaf25

require (
	github.com/btcsuite/btcd v0.0.0-20190807005414-4063feeff79a // indirect
	github.com/cosmos/cosmos-sdk v0.36.0
	github.com/cosmos/go-bip39 v0.0.0-20180819234021-555e2067c45d // indirect
	github.com/go-kit/kit v0.9.0 // indirect
	github.com/golang/mock v1.3.1 // indirect
	github.com/gorilla/mux v1.7.3
	github.com/libp2p/go-buffer-pool v0.0.2 // indirect
	github.com/magiconair/properties v1.8.1 // indirect
	github.com/mattn/go-isatty v0.0.9 // indirect
	github.com/onsi/ginkgo v1.8.0 // indirect
	github.com/onsi/gomega v1.5.0 // indirect
	github.com/pelletier/go-toml v1.4.0 // indirect
	github.com/prometheus/client_golang v1.1.0 // indirect
	github.com/prometheus/client_model v0.0.0-20190812154241-14fe0d1b01d4 // indirect
	github.com/rakyll/statik v0.1.6 // indirect
	github.com/rcrowley/go-metrics v0.0.0-20190706150252-9beb055b7962 // indirect
	github.com/rs/cors v1.7.0 // indirect
	github.com/spf13/cobra v0.0.5
	github.com/spf13/viper v1.4.0
	github.com/stretchr/testify v1.4.0 // indirect
	github.com/tendermint/go-amino v0.15.0
	github.com/tendermint/tendermint v0.32.2
	github.com/tendermint/tm-db v0.1.1
	golang.org/x/net v0.7.0 // indirect
	google.golang.org/genproto v0.0.0-20190817000702-55e96fffbd48 // indirect
	google.golang.org/grpc v1.23.0
)
