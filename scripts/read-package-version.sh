#!/usr/bin/env bash
# Emit Version from Directory.Build.props (via MSBuild evaluation on a library project).
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PROJ="${PACK_VERSION_PROJECT:-src/BlueLake.Data.Abstractions/BlueLake.Data.Abstractions.csproj}"
PROJ_PATH="${ROOT}/${PROJ}"

if [[ ! -f "${PROJ_PATH}" ]]; then
  echo "error: PACK_VERSION_PROJECT not found: ${PROJ_PATH}" >&2
  exit 1
fi

if [[ ! -f "${ROOT}/Directory.Build.props" ]]; then
  echo "error: missing ${ROOT}/Directory.Build.props (MUST for release)" >&2
  exit 1
fi

VERSION="$(
  dotnet msbuild "${PROJ_PATH}" \
    -getProperty:Version \
    -p:NuGetAudit=false \
    -nologo \
    | tr -d '\r'
)"

if [[ -z "${VERSION}" ]]; then
  echo "error: Version is empty — set <Version> in Directory.Build.props" >&2
  exit 1
fi

if [[ "${VERSION}" =~ ^v ]]; then
  echo "error: Version must not include a 'v' prefix (use 1.0.0 not v1.0.0)" >&2
  exit 1
fi

if ! [[ "${VERSION}" =~ ^[0-9]+\.[0-9]+\.[0-9]+([.-][0-9A-Za-z.-]+)?$ ]]; then
  echo "error: Version '${VERSION}' is not a valid semver-like release id" >&2
  exit 1
fi

printf '%s' "${VERSION}"
