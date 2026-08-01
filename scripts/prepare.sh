#!/usr/bin/env bash
# Prepares the shell environment for building, testing, and formatting Reihitsu.
#
#   scripts/prepare.sh [--no-install] [--quiet]
#
# Installs the required .NET SDK only when it is missing, so the script is a
# no-op verification on machines and images that already ship it.
#
# The script runs in its own process and therefore cannot export PATH into the
# calling shell — it prints the export line instead. Do not source it: every
# other repository script resolves the SDK itself, so nothing needs the export,
# and sourcing would leak this script's shell options into the caller.
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# shellcheck source=lib/dotnet-env.sh
. "$script_dir/lib/dotnet-env.sh"

reihitsu_ensure_dotnet "$@"

if [ -x "$REIHITSU_DOTNET_ROOT/dotnet" ]; then
    case ":$PATH:" in
        *":$REIHITSU_DOTNET_ROOT:"*)
            echo "hint: for direct dotnet calls in this shell, run: export PATH=\"$REIHITSU_DOTNET_ROOT:\$PATH\""
            ;;
    esac
fi
