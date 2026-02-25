#!/bin/bash
dotnet --info | grep "RID:" | awk -F':' '{gsub(/^ +| +$/, "", $2); printf "%s", $2}'