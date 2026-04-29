#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [string]$Command = "dev",

    [Parameter(Position = 1, ValueFromRemainingArguments = $true)]
    [string[]]$Args
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackendDir = Resolve-Path (Join-Path $ScriptDir "..")
$ApiProject = "src/ZealEducation.API"

Push-Location $BackendDir
try {
    function Show-Usage {
        @"
Usage: ./scripts/run.ps1 [command] [extra dotnet args]

Commands:
  dev              Run the API in Development (default)
  prod             Run the API in Production
  watch            Run with hot reload (dotnet watch)
  build            Build the solution (Debug)
  publish [dir]    Publish Release build (default dir: ./publish)
  restore          Restore NuGet packages
  clean            Clean build artifacts

Examples:
  ./scripts/run.ps1
  ./scripts/run.ps1 dev
  ./scripts/run.ps1 watch
  ./scripts/run.ps1 dev --launch-profile https
  ./scripts/run.ps1 publish ./out
"@ | Write-Host
    }

    function Invoke-Cmd {
        param([string[]]$CmdArgs)
        & dotnet @CmdArgs
        if ($LASTEXITCODE -ne 0) { throw "dotnet failed with exit code $LASTEXITCODE" }
    }

    $extra = @(); if ($Args) { $extra = $Args }

    switch ($Command.ToLower()) {
        "dev" {
            $env:ASPNETCORE_ENVIRONMENT = "Development"
            Invoke-Cmd (@("run", "--project", $ApiProject) + $extra)
        }
        "prod" {
            $env:ASPNETCORE_ENVIRONMENT = "Production"
            Invoke-Cmd (@("run", "--project", $ApiProject, "-c", "Release") + $extra)
        }
        "watch" {
            $env:ASPNETCORE_ENVIRONMENT = "Development"
            Invoke-Cmd (@("watch", "--project", $ApiProject, "run") + $extra)
        }
        "build"   { Invoke-Cmd (@("build") + $extra) }
        "publish" {
            $out = if ($extra.Count -gt 0) { $extra[0] } else { "./publish" }
            $rest = if ($extra.Count -gt 1) { $extra[1..($extra.Count - 1)] } else { @() }
            Invoke-Cmd (@("publish", $ApiProject, "-c", "Release", "-o", $out) + $rest)
        }
        "restore" { Invoke-Cmd (@("restore") + $extra) }
        "clean"   { Invoke-Cmd (@("clean") + $extra) }
        { $_ -in "help", "-h", "--help" } { Show-Usage }
        default {
            Write-Error "Unknown command: $Command"
            Show-Usage
            exit 1
        }
    }
}
finally {
    Pop-Location
}
