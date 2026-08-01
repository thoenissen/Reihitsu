#!/usr/bin/env bash
# Prepares the shell environment for building, testing, and formatting Reihitsu.
#
#   scripts/prepare.sh [--no-install] [--quiet]
#
# Installs the required .NET SDK only when it is missing, so the script is a
# no-op verification on machines and images that already ship it.
#
# The script runs in its own process and therefore cannot export PATH into the
# calling shell. Either source it, or let the other repository scripts resolve
# the SDK themselves — they all do.
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# shellcheck source=lib/dotnet-env.sh
. "$script_dir/lib/dotnet-env.sh"

reihitsu_ensure_dotnet "$@"

if [ "${BASH_SOURCE[0]}" = "$0" ] && [ -x "$REIHITSU_DOTNET_ROOT/dotnet" ]; then
    case ":$PATH:" in
        *":$REIHITSU_DOTNET_ROOT:"*)
            echo "hint: for direct dotnet calls in this shell, run: export PATH=\"$REIHITSU_DOTNET_ROOT:\$PATH\""
            ;;
    esac
fi
