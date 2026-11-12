#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet --version
dotnet restore MarketingManager.sln
dotnet build MarketingManager.sln --configuration Release --no-restore
