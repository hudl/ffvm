#!/usr/bin/env bash
##########################################################################
# This is the Cake bootstrapper script for Linux and OS X.
# This file was based off of https://github.com/cake-build/resources
# Feel free to change this file to fit your needs.
##########################################################################

set -eo pipefail

command -v dotnet >/dev/null 2>&1 || {
    echo >&2 "This project requires dotnet core but it could not be found"
    echo >&2 "Please install dotnet core and ensure it is available on your PATH"
    exit 1
}

SCRIPT_ROOT="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
CAKE_VERSION=${CAKE_VERSION:-0.35.0-HUDL}
TOOLS_DIR=${TOOLS_DIR:-"${SCRIPT_ROOT}/.setup/dev/tmp/cake-install-${CAKE_VERSION}/"}
CAKE_NETCOREAPP_VERSION=${CAKE_NETCOREAPP_VERSION:-2.0}

# Temporarily skip verification of addins.
export CAKE_SETTINGS_SKIPVERIFICATION='true'

mkdir -p "${TOOLS_DIR}"

CAKE_DLL=$(find "${TOOLS_DIR}" -type f -name 'Cake.dll' | head -n 1)

if [ -n "$CAKE_TEST_ECHO_ARGS" ]; then
    echo "Arguments received by Bash:"
    for arg in "$@"
    do
        echo -n "* "
        echo "$arg"
    done
fi

# Define default arguments.
SCRIPT="./.setup/dev/build.cake"
CAKE_ARGUMENTS=()
# Parse arguments.
while [ "$#" -gt 0 ]; do
    case $1 in
        # Script arg needs to be pulled out into variable
        --script=*) SCRIPT="${1:9}" ;;
        # `--` arg indicates all following args are ignored by Cake. Break handle the remaining args in the next loop.
        --) CAKE_ARGUMENTS+=("$1"); shift; break ;;
        # We want to always skip over Cake's built-in help, so we'll prefix this and look for it when determining the target in the Cake script.
        --help) CAKE_ARGUMENTS+=("--:$1") ;;
        # Normal argument; Pass it through as-is.
        -*) CAKE_ARGUMENTS+=("$1") ;;
        # Plain argument; add prefix so we don't blow up Cake, then remove it within Cake script.
        *) CAKE_ARGUMENTS+=("--:$1") ;;
    esac
    shift
done
# If we hit an `--` in the previous loop, we'll have remaining args that we need to go through and prefix
while [ "$#" -gt 0 ]; do
    CAKE_ARGUMENTS+=("--:$1")
    shift
done

###########################################################################
# INSTALL CAKE
###########################################################################

if [ ! -f "${CAKE_DLL}" ]; then
    echo "Installing Cake ${CAKE_VERSION} to ${TOOLS_DIR}..."

    TOOLS_PROJ="${TOOLS_DIR%/}/cake.csproj"
    echo "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>netcoreapp${CAKE_NETCOREAPP_VERSION}</TargetFramework></PropertyGroup></Project>" > "${TOOLS_PROJ}"
    dotnet add "${TOOLS_PROJ}" package Cake.CoreCLR -v "${CAKE_VERSION}" --package-directory "${TOOLS_DIR%/}/Cake.CoreCLR.${CAKE_VERSION}"

    CAKE_DLL=$(find "${TOOLS_DIR}" -type f -name 'Cake.dll' | head -n 1)

    if [ ! -f "${CAKE_DLL}" ]; then
        echo >&2 "Failed to install Cake ${CAKE_VERSION}"
        exit 1
    fi
fi

# Because in WSL we can't mount folders which are not on host we have to use
# home directory from Windows
WSL_CHECK=`uname -r | sed -n 's/.*\( *Microsoft *\).*/\1/p'`
if [[ $WSL_CHECK == "Microsoft" ]]; then
    export HOME=$(wslpath $(cmd.exe /C echo %USERPROFILE% | tr -d '\r'))
fi

###########################################################################
# RUN BUILD SCRIPT
###########################################################################

# Start Cake
echo "Running Hudl Command Line Tools script..."
if [ -n "$CAKE_TEST_ECHO_ARGS" ]; then
    set -x
fi

exec dotnet "$CAKE_DLL" $SCRIPT --cache_enabled=true "${CAKE_ARGUMENTS[@]}"
