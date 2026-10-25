#!/usr/bin/env pwsh
# scripts/run.ps1
# Launches the Auditor Management System WPF application from the repository root.
#
# Usage:
#   .\scripts\run.ps1
#
# Requires: .NET 10 SDK installed

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$projectPath = Join-Path $rootDir "src/AuditorManagementSystem.App/AuditorManagementSystem.App.csproj"

if (-not (Test-Path $projectPath)) {
    Write-Error "Project file not found: $projectPath"
    exit 1
}

Write-Host "Starting Auditor Management System..." -ForegroundColor Cyan
Write-Host "Project: $projectPath" -ForegroundColor Gray

dotnet run --project $projectPath
