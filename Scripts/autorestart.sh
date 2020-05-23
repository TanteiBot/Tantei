﻿﻿#!/bin/sh
# This script is taken from https://dsharpplus.github.io/articles/hosting/raspberry_pi.html
# and slightly modified for this particular bot since i'm fine with hardcoded bot name.
# I haven't testet it on linux yet.
echo Using `which dotnet`
while true
do
    dotnet "PaperMalKing.dll"
    echo "Application crashed, restarting in 5 seconds..."
    sleep 5
done
