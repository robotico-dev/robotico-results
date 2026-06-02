#!/usr/bin/env bash
# Restore/build projects listed in scripts/pack-projects.list (gitnautica libs; no WPF).
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
LIST="${ROOT}/scripts/pack-projects.list"

test -f "${ROOT}/Directory.Build.props" || { echo "MUST - Directory.Build.props required"; exit 1; }
test -f "${LIST}" || { echo "MUST - scripts/pack-projects.list required"; exit 1; }

DOTNET_FLAGS=(-p:NuGetAudit=false -p:RunAnalyzersDuringBuild=false)

while IFS= read -r proj || [[ -n "${proj}" ]]; do
  [[ -z "${proj}" || "${proj}" =~ ^# ]] && continue
  echo "CI pack project: ${proj}"
  dotnet restore "${ROOT}/${proj}" "${DOTNET_FLAGS[@]}"
  dotnet build "${ROOT}/${proj}" -c Release --no-restore "${DOTNET_FLAGS[@]}"
done < "${LIST}"
