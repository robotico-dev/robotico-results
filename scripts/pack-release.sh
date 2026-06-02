#!/usr/bin/env bash
# Pack projects listed in scripts/pack-projects.list; version from Directory.Build.props only.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="${ROOT}/nupkgs"
SCRIPT_DIR="$(dirname "$0")"
LIST="${SCRIPT_DIR}/pack-projects.list"

if [[ ! -f "${LIST}" ]]; then
  echo "error: missing ${LIST}" >&2
  exit 1
fi

VERSION="$(bash "${SCRIPT_DIR}/read-package-version.sh")"
rm -rf "${OUT}"
mkdir -p "${OUT}"

echo "pack-release: version=${VERSION} (Directory.Build.props) output=${OUT}"
while IFS= read -r proj || [[ -n "${proj}" ]]; do
  [[ -z "${proj}" ]] && continue
  [[ "${proj}" =~ ^# ]] && continue
  proj_path="${ROOT}/${proj}"
  if [[ ! -f "${proj_path}" ]]; then
    echo "error: project not found: ${proj_path}" >&2
    exit 1
  fi
  echo "  pack ${proj}"
  dotnet pack "${proj_path}" -c Release --nologo -o "${OUT}" -p:NuGetAudit=false
done < "${LIST}"

shopt -s nullglob
files=("${OUT}"/*.nupkg)
shopt -u nullglob
if [[ "${#files[@]}" -eq 0 ]]; then
  echo "error: no packages produced" >&2
  exit 1
fi
ls -la "${OUT}"/*.nupkg
