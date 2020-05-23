﻿﻿#!/bin/bash
# This script is taken from https://dsharpplus.github.io/articles/hosting/raspberry_pi.html
# and slightly modified for this particular bot since i'm fine with hardcoded bot name.
# I haven't tested it on linux yet.
echo Using `which dotnet`
while true
do
    dotnet "PaperMalKing.dll"
    exitcode=$?
    if [ $"exitcode" == "0" ]
    then
    echo "Bot exited clearly, quitting"
    break
    fi
    echo "Application crashed, restarting in 5 seconds..."
    sleep 5
done
