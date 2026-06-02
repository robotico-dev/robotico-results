#!/usr/bin/env bash
# Woodpecker release step: read Version → tag → pack → push (single shell; avoids YAML line-join bugs).
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "${ROOT}"

test -f Directory.Build.props || {
  echo "MUST - Directory.Build.props with Version element" >&2
  exit 1
}

if bash scripts/should-skip-release.sh; then
  exit 0
fi

VERSION="$(bash scripts/read-package-version.sh)"
echo "release Directory.Build.props Version=${VERSION}"
bash scripts/ensure-git-tag.sh "${VERSION}"
bash scripts/pack-release.sh
bash scripts/nuget-push-download.sh nupkgs
