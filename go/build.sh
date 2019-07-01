#!/usr/bin/env bash

set -e

pushd `dirname $0` > /dev/null

go build -o ./bin/strongforced ./cmd/strongforced
go build -o ./bin/strongforcecli ./cmd/strongforcecli

popd > /dev/null