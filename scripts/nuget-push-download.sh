#!/usr/bin/env bash
# Push *.nupkg to download.robotico.dev (BaGetter). Never overwrites: --skip-duplicate.
# Existing version → warning only (exit 0).
set -euo pipefail

NUGET_SOURCE="${NUGET_PUSH_SOURCE:-https://download.robotico.dev/nuget/v3/index.json}"
API_KEY="${PUBLISH_TOKEN:?Set PUBLISH_TOKEN (Woodpecker secret publish_token)}"
DIR="${1:-./nupkgs}"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "error: dotnet required on PATH" >&2
  exit 1
fi

if [[ ! -d "$DIR" ]]; then
  echo "error: nupkg directory not found: $DIR" >&2
  exit 1
fi

shopt -s nullglob
files=("$DIR"/*.nupkg)
shopt -u nullglob

if [[ "${#files[@]}" -eq 0 ]]; then
  echo "error: no .nupkg files in $DIR" >&2
  exit 1
fi

published=0
warnings=0
skipped_symbols=0

for f in "${files[@]}"; do
  base="$(basename "$f")"
  if [[ "$base" == *.symbols.nupkg ]]; then
    skipped_symbols=$((skipped_symbols + 1))
    continue
  fi

  set +e
  out="$(dotnet nuget push "$f" --source "$NUGET_SOURCE" --api-key "$API_KEY" --skip-duplicate 2>&1)"
  code=$?
  set -e
  printf '%s\n' "$out"

  if [[ "$code" -eq 0 ]]; then
    if echo "$out" | grep -qiE 'already exists|skipped|duplicate|409|conflict'; then
      echo "[WARN] $base — version already on registry (not overwritten)"
      warnings=$((warnings + 1))
    else
      echo "[OK]   $base — published"
      published=$((published + 1))
    fi
  else
    if echo "$out" | grep -qiE 'already exists|skipped|duplicate|409|conflict|already been pushed'; then
      echo "[WARN] $base — version already on registry (not overwritten)"
      warnings=$((warnings + 1))
    else
      echo "[ERROR] $base — push failed:" >&2
      echo "$out" >&2
      exit 1
    fi
  fi
done

echo "nuget-push-download: published=$published warnings=$warnings symbols_skipped=$skipped_symbols"
if [[ "$published" -eq 0 && "$warnings" -gt 0 ]]; then
  echo "nuget-push-download: all packages already present — pipeline OK (no overwrite)"
fi
