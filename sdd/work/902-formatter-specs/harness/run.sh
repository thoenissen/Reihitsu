#!/usr/bin/env bash
# usage: run.sh <dir-or-file>...
export PATH=/root/.dotnet:$PATH
exec dotnet /root/.local/share/dotnet/runfile/fmt-a7eaf9f79544b37429de4dccee48923158862d86731d6fbc00ca2bcfce2385ab/bin/release/fmt.dll "$@"
