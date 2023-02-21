#!/bin/bash

function run() {
    echo " TEST     $1"
    if !(echo "bye" | cat tests/harness.fs $1 - | ./crust); then
        echo
        echo "test(s) failed"
        exit 1
    fi
}

if [ -z "$1" ]; then
    for fn in tests/test_*; do
        run $fn
    done

    echo "all tests passed"
else
    run $1
fi