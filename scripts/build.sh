#!/usr/bin/env bash
# Builds the Reihitsu solution in Release configuration.
#
#   scripts/build.sh [--no-install] [<extra dotnet build arguments>...]
#
# The solution is addressed by absolute path, so the caller's working directory
# is never changed and relative arguments keep their meaning.
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# shellcheck source=lib/dotnet-env.sh
. "$script_dir/lib/dotnet-env.sh"

install_arguments=()
build_arguments=()

for argument in "$@"; do
    case "$argument" in
        --no-install) install_arguments+=("$argument") ;;
        *) build_arguments+=("$argument") ;;
    esac
done

reihitsu_ensure_dotnet "${install_arguments[@]+"${install_arguments[@]}"}" --quiet

dotnet build "$(reihitsu_repo_root)/Reihitsu.sln" -c Release --verbosity minimal "${build_arguments[@]+"${build_arguments[@]}"}"
