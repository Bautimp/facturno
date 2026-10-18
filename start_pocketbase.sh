#!/usr/bin/env bash
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$DIR/tools/pocketbase"
./pocketbase serve --http="127.0.0.1:8090"
