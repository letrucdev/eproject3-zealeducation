#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [string]$Command = "help",

    [Parameter(Position = 1, ValueFromRemainingArguments = $true)]
    [string[]]$Args
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackendDir = Resolve-Path (Join-Path $ScriptDir "..")
$InfrastructureProject = "src/ZealEducation.Infrastructure"
$StartupProject = "src/ZealEducation.API"

Push-Location $BackendDir
try {
    function Invoke-Ef {
        param([string[]]$EfArgs)
        & dotnet ef @EfArgs `
            --project $InfrastructureProject `
            --startup-project $StartupProject
        if ($LASTEXITCODE -ne 0) { throw "dotnet ef failed with exit code $LASTEXITCODE" }
    }

    function Show-Usage {
        @"
Usage: ./scripts/migrate.ps1 <command> [args]

Commands:
  update [target]     Apply migrations up to <target> (default: latest)
  add <Name>          Create a new migration
  remove              Remove the last migration (only if not applied)
  list                List all migrations and their status
  script [from] [to]  Generate idempotent SQL script (default: full schema)
  drop                Drop the database (asks for confirmation)
  reset               Drop database then re-apply all migrations

Examples:
  ./scripts/migrate.ps1 update
  ./scripts/migrate.ps1 add AddStudentTable
  ./scripts/migrate.ps1 script 0 | Out-File migration.sql
"@ | Write-Host
    }

    switch ($Command.ToLower()) {
        "update" {
            $target = if ($Args.Count -gt 0) { $Args[0] } else { $null }
            if ($target) { Invoke-Ef @("database", "update", $target) }
            else         { Invoke-Ef @("database", "update") }
        }
        "add" {
            if ($Args.Count -lt 1) { Write-Error "Migration name required"; Show-Usage; exit 1 }
            Invoke-Ef @("migrations", "add", $Args[0])
        }
        "remove" { Invoke-Ef @("migrations", "remove") }
        "list"   { Invoke-Ef @("migrations", "list") }
        "script" {
            $from = if ($Args.Count -gt 0) { $Args[0] } else { "0" }
            $to   = if ($Args.Count -gt 1) { $Args[1] } else { $null }
            if ($to) { Invoke-Ef @("migrations", "script", $from, $to, "--idempotent") }
            else     { Invoke-Ef @("migrations", "script", $from, "--idempotent") }
        }
        "drop"   { Invoke-Ef @("database", "drop", "--force") }
        "reset"  {
            Invoke-Ef @("database", "drop", "--force")
            Invoke-Ef @("database", "update")
        }
        { $_ -in "help", "-h", "--help", "" } { Show-Usage }
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
