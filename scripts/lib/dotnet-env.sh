#!/usr/bin/env bash
# Shared .NET environment helper for the Reihitsu repository scripts.
#
# Source this file, do not execute it:
#
#   . "$(dirname "$0")/lib/dotnet-env.sh"
#   reihitsu_ensure_dotnet
#
# The helper keeps every repository script working the same way in a cloud
# sandbox without an SDK and on a machine where the SDK is preinstalled: it
# probes first and installs only when the required SDK is genuinely missing.

REIHITSU_DOTNET_CHANNEL="${REIHITSU_DOTNET_CHANNEL:-10.0}"
REIHITSU_DOTNET_MAJOR="${REIHITSU_DOTNET_MAJOR:-10}"
REIHITSU_DOTNET_ROOT="${REIHITSU_DOTNET_ROOT:-$HOME/.dotnet}"

# Absolute path of the repository root, derived from this file's location.
reihitsu_repo_root()
{
    local script_dir
    script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

    cd "$script_dir/../.." && pwd
}

# Succeeds when a dotnet on PATH reports an SDK of the required major version.
reihitsu_has_required_sdk()
{
    command -v dotnet > /dev/null 2>&1 || return 1
    dotnet --list-sdks 2> /dev/null | grep -q "^${REIHITSU_DOTNET_MAJOR}\." || return 1
}

# Adds a previously installed private SDK to PATH when one exists.
reihitsu_use_private_sdk()
{
    if [[ -x "$REIHITSU_DOTNET_ROOT/dotnet" ]]; then
        export PATH="$REIHITSU_DOTNET_ROOT:$PATH"
        export DOTNET_ROOT="$REIHITSU_DOTNET_ROOT"

        return 0
    fi

    return 1
}

# Ensures a usable SDK is on PATH.
#
#   reihitsu_ensure_dotnet [--no-install] [--quiet]
#
# Exit status: 0 ready, 2 missing and not installable (or installation refused).
reihitsu_ensure_dotnet()
{
    local allow_install=1
    local quiet=0
    local argument

    for argument in "$@"; do
        case "$argument" in
            --no-install) allow_install=0 ;;
            --quiet) quiet=1 ;;
        esac
    done

    if reihitsu_has_required_sdk; then
        [[ "$quiet" -eq 1 ]] || echo "dotnet: using $(command -v dotnet) ($(dotnet --version))"

        return 0
    fi

    if reihitsu_use_private_sdk && reihitsu_has_required_sdk; then
        [[ "$quiet" -eq 1 ]] || echo "dotnet: using $REIHITSU_DOTNET_ROOT/dotnet ($(dotnet --version))"

        return 0
    fi

    if [[ "$allow_install" -eq 0 ]]; then
        echo "dotnet: no .NET ${REIHITSU_DOTNET_MAJOR} SDK found and --no-install was requested." >&2

        return 2
    fi

    echo "dotnet: no .NET ${REIHITSU_DOTNET_MAJOR} SDK found, installing channel ${REIHITSU_DOTNET_CHANNEL} into ${REIHITSU_DOTNET_ROOT}"

    local installer
    installer="$(mktemp -t dotnet-install-XXXXXX.sh)"

    if ! curl -sSL --proto '=https' --proto-redir '=https' https://dot.net/v1/dotnet-install.sh -o "$installer"; then
        rm -f "$installer"
        echo "dotnet: could not download dotnet-install.sh (no network egress?)." >&2

        return 2
    fi

    if ! bash "$installer" --channel "$REIHITSU_DOTNET_CHANNEL" --install-dir "$REIHITSU_DOTNET_ROOT"; then
        rm -f "$installer"
        echo "dotnet: dotnet-install.sh failed." >&2

        return 2
    fi

    rm -f "$installer"

    export PATH="$REIHITSU_DOTNET_ROOT:$PATH"
    export DOTNET_ROOT="$REIHITSU_DOTNET_ROOT"

    if ! reihitsu_has_required_sdk; then
        echo "dotnet: installation finished but no ${REIHITSU_DOTNET_MAJOR}.* SDK is visible." >&2

        return 2
    fi

    [[ "$quiet" -eq 1 ]] || echo "dotnet: installed $(dotnet --version) in $REIHITSU_DOTNET_ROOT"

    return 0
}
