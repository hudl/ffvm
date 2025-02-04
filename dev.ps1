#!/usr/bin/env pwsh

[CmdletBinding()]
Param(
    [string]$Script = ".setup/dev/build.cake",

    [Parameter(Position = 0)]
    [ValidateSet( `
        "check", `
        "install", `
        "uninstall", `
        "help" `
    )]
    [string]$Target,
    [string]$Configuration,
    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
    [string]$Verbosity = 'Normal',
    [switch]$ShowDescription,
    [Alias("WhatIf", "Noop")]
    [switch]$DryRun,
    [switch]$PowershellTestCommands,
    [switch]$PowershellTestRun,
    [switch]$Experimental,
    [switch]$SkipToolPackageRestore,
    [Alias("h")]
    [switch]$Help,
    [Parameter(Mandatory = $false, ValueFromRemainingArguments = $true)]
    [string[]]$ScriptArgs = @()
)

if (-not (Get-Command -Name 'dotnet' -CommandType Application -ErrorAction SilentlyContinue)) {
    throw "This project requires dotnet core but it could not be found. Please install dotnet core and ensure it is available on your PATH"
}

function Get-EnvironmentVariableOrDefault {
    Param(
        [string] $Name,
        [string] $Default
    )

    $result = [System.Environment]::GetEnvironmentVariable($Name)
    if (-not $result) {
        return $Default
    }
    else {
        return $result
    }
}
$CakeVersion = Get-EnvironmentVariableOrDefault -Name 'CAKE_VERSION' -Default '0.35.0-HUDL'
$ToolsDir = Get-EnvironmentVariableOrDefault -Name 'TOOLS_DIR' -Default (Join-Path -Path $PSScriptRoot -ChildPath ".setup/dev/tmp/cake-install-$CakeVersion/")
$CakeNetcoreappVersion = Get-EnvironmentVariableOrDefault -Name 'CAKE_NETCOREAPP_VERSION' -Default '2.0'

if (-not (Test-Path -Path $ToolsDir)) {
    New-Item -ItemType Directory -Path $ToolsDir | Out-Null
}

$CakeDLL = Get-ChildItem -Path $ToolsDir -Recurse -Filter 'Cake.dll' | Select-Object -ExpandProperty FullName -First 1

###########################################################################
# INSTALL CAKE
###########################################################################

if (-not $CakeDLL) {
    Write-Output "Installing Cake $CakeVersion to $ToolsDir..."

    $ToolsProj = Join-Path -Path $ToolsDir -ChildPath 'cake.csproj'
    "<Project Sdk=`"Microsoft.NET.Sdk`"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>netcoreapp$CakeNetcoreappVersion</TargetFramework></PropertyGroup></Project>" | Set-Content -Path $ToolsProj
    Start-Process -FilePath 'dotnet' -ArgumentList @('add', $ToolsProj, 'package', 'Cake.CoreCLR', '-v', $CakeVersion, '--package-directory', $ToolsDir) -NoNewWindow -Wait -ErrorAction Stop

    $CakeDLL = Get-ChildItem -Path $ToolsDir -Recurse -Filter 'Cake.dll' | Select-Object -ExpandProperty FullName

    if (-not $CakeDLL) {
        throw "Failed to install Cake $CakeVersion"
    }
}

###########################################################################
# Add `--:` to arguments so it's not required
###########################################################################
$EchoArgs = [System.Environment]::GetEnvironmentVariable('CAKE_TEST_ECHO_ARGS')
if($EchoArgs) {
    Write-Output "Arguments received by PowerShell:"
    foreach ($arg in $ScriptArgs) {
        Write-Output "* $arg"
    }
}

$ScriptArgs = $ScriptArgs `
    | Where-Object { -not [System.String]::IsNullOrWhiteSpace($_) } `
    | ForEach-Object {
        if ($Target -eq "yarn") {
            $_ = "--:" + $_
        }

        $_
}

# Fix arguments that need quotes or escaping
$ScriptArgs = $ScriptArgs `
    | Where-Object { -not [System.String]::IsNullOrWhiteSpace($_) } `
    | ForEach-Object {
        if ($_.Contains(" ") -or $_.Contains("`"")) {
            # Escape quotes - TODO - this isn't working quite right
            # $_ = $_.Replace("`"", "```"")
            # Surround with quotes (only if not already surrounded)
            if (-not ($_.StartsWith("`"") -and $_.EndsWith("`""))){
                $_ = "`"" + $_ + "`""
            }
        }

        $_
}

###########################################################################
# RUN BUILD SCRIPT
###########################################################################

$cakeArguments = @("$PSScriptRoot\$Script");

# Add target as last known argument to help with parsing of args to send on to commands like yarn

# Enable cashe for cake to speed up execution
$cakeArguments += "--cache_enabled=true"

if ($Configuration) { $cakeArguments += "-configuration=$Configuration" }
if ($Verbosity) { $cakeArguments += "-verbosity=$Verbosity" }
if ($ShowDescription) { $cakeArguments += "-showdescription" }
if ($DryRun) { $cakeArguments += "-dryrun" }
if ($Experimental) { $cakeArguments += "-experimental" }
$cakeArguments += "-settings_skipverification=true"


if ($Help -or -not $Target) {
    $cakeArguments += "-target=help"
} else {
    $cakeArguments += "-target=$Target"
}

$cakeArguments += $ScriptArgs

Write-Host "Running Hudl Command Line Tools script..."

$command = "& dotnet `"$CakeDLL`" $cakeArguments"
if ($EchoArgs){
    Write-Output $command
}

Invoke-Expression $command

exit $LASTEXITCODE
