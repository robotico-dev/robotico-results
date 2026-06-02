#!/usr/bin/env bash
# Exit 0 when this commit should not run tag/pack/push (docs/CI only since last v{Version} tag).
set -euo pipefail

if [[ -z "${CI_COMMIT_SHA:-}" ]]; then
  exit 1
fi

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "${ROOT}"

if ! grep -q '<Version>' Directory.Build.props 2>/dev/null; then
  exit 1
fi

VERSION="$(bash scripts/read-package-version.sh)"
TAG="v${VERSION}"

if [[ -n "${GITEA_TOKEN:-}" ]]; then
  if [[ -n "${CI_REPO_CLONE_URL:-}" ]]; then
    clone_url="${CI_REPO_CLONE_URL}"
    if [[ "${clone_url}" == https://* ]]; then
      clone_url="https://oauth2:${GITEA_TOKEN}@${clone_url#https://}"
    fi
    git remote set-url origin "${clone_url}"
  elif [[ -n "${CI_REPO_OWNER:-}" && -n "${CI_REPO_NAME:-}" ]]; then
    git remote set-url origin "https://oauth2:${GITEA_TOKEN}@brokkr.robotico.dev/${CI_REPO_OWNER}/${CI_REPO_NAME}.git"
  elif [[ -n "${CI_REPO:-}" ]]; then
    git remote set-url origin "https://oauth2:${GITEA_TOKEN}@brokkr.robotico.dev/${CI_REPO}.git"
  fi
fi

git fetch origin --tags --force

if ! git rev-parse "refs/tags/${TAG}^{commit}" >/dev/null 2>&1; then
  exit 1
fi

tag_sha="$(git rev-parse "refs/tags/${TAG}^{commit}")"

if [[ "${tag_sha}" == "${CI_COMMIT_SHA}" ]]; then
  exit 1
fi

git fetch origin "${tag_sha}" --depth=1 2>/dev/null || git fetch origin "${tag_sha}" 2>/dev/null || true

while IFS= read -r path; do
  [[ -z "${path}" ]] && continue
  case "${path}" in
    README.md|.woodpecker.yml|scripts/*|assets/*|docs/*|benchmarks/*|.github/*|CHANGELOG.md|IMPLEMENTATION*.md|global.json|coverlet.runsettings|*.slnx|Robotico.Library.*.props|LICENSE|nuget.config) ;;
    *)
      exit 1
      ;;
  esac
done < <(git diff --name-only "${tag_sha}" "${CI_COMMIT_SHA}" 2>/dev/null || true)

if ! git diff --name-only "${tag_sha}" "${CI_COMMIT_SHA}" | grep -q .; then
  exit 1
fi

echo "[INFO] release skipped: no packable changes since ${TAG} (${tag_sha:0:7})"
exit 0
