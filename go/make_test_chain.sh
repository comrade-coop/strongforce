#!/usr/bin/env bash

set -e

[ -d ~/.strongforced ] && echo 'Warning: `~/.strongforced` exists'
[ -d ~/.strongforcecli ] && echo 'Warning: `~/.strongforcecli` exists'

if [ -d ~/.strongforced ] || [ -d ~/.strongforcecli ]; then
  # read -p "Do you wish to remove those folders [y/n/C]? " yn
  yn=Y
  case $yn in
      [Yy]* ) set -x; rm -r ~/.strongforced ~/.strongforcecli; { set +x; } 2>/dev/null;;
      [Nn]* ) ;;
      * ) exit 1;;
  esac
fi

CHAIN=strongforce-test-chain-$RANDOM
TESTPASS=testpass

pushd `dirname $0` > /dev/null
set -x

./bin/strongforcecli config chain-id $CHAIN
./bin/strongforcecli config trust-node true
./bin/strongforcecli config output json
./bin/strongforcecli config indent true
./bin/strongforced unsafe-reset-all
./bin/strongforced init local-test-node -o --chain-id $CHAIN
echo $TESTPASS | ./bin/strongforcecli keys add A
echo $TESTPASS | ./bin/strongforcecli keys add B
./bin/strongforced add-genesis-account $(./bin/strongforcecli keys show A -a) 100000000stake
./bin/strongforced add-genesis-account $(./bin/strongforcecli keys show B -a) 100000000stake
echo $TESTPASS | ./bin/strongforced gentx --name A
./bin/strongforced collect-gentxs
./bin/strongforced validate-genesis

{ set +x; } 2>/dev/null
popd > /dev/null