#!/usr/bin/env bash
# Create and push v{Version} if missing. Version comes from Directory.Build.props (no v prefix).
set -euo pipefail

VERSION="${1:?Usage: ensure-git-tag.sh <semver from Directory.Build.props>}"
TAG="v${VERSION}"

if [[ -z "${CI_COMMIT_SHA:-}" ]]; then
  echo "error: CI_COMMIT_SHA not set — run inside Woodpecker" >&2
  exit 1
fi

# Annotated tags: rev-parse must peel to the commit (^{commit}), not the tag object.
tag_commit_sha() {
  local tag="$1"
  git rev-parse "${tag}^{commit}" 2>/dev/null || git rev-parse "refs/tags/${tag}^{commit}"
}

remote_tag_commit_sha() {
  local tag="$1"
  local peeled
  peeled="$(git ls-remote origin "refs/tags/${tag}^{}" 2>/dev/null | awk '{print $1}' | head -1)"
  if [[ -n "${peeled}" ]]; then
    printf '%s' "${peeled}"
    return 0
  fi
  git ls-remote origin "refs/tags/${tag}" 2>/dev/null | awk '{print $1}' | head -1
}

git config user.email "${WOODPECKER_GIT_EMAIL:-woodpecker@dvalin.robotico.dev}"
git config user.name "${WOODPECKER_GIT_NAME:-Woodpecker CI}"

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

if git rev-parse "refs/tags/${TAG}" >/dev/null 2>&1; then
  existing="$(tag_commit_sha "${TAG}")"
  if [[ "${existing}" == "${CI_COMMIT_SHA}" ]]; then
    echo "[INFO] tag ${TAG} already points at ${CI_COMMIT_SHA:0:7}"
    exit 0
  fi
  echo "error: tag ${TAG} exists at ${existing:0:7} but current commit is ${CI_COMMIT_SHA:0:7}" >&2
  echo "error: bump <Version> in Directory.Build.props — tags are immutable" >&2
  exit 1
fi

remote_sha="$(remote_tag_commit_sha "${TAG}" || true)"
if [[ -n "${remote_sha}" ]]; then
  if [[ "${remote_sha}" == "${CI_COMMIT_SHA}" ]]; then
    echo "[INFO] remote tag ${TAG} already at ${CI_COMMIT_SHA:0:7}"
    exit 0
  fi
  echo "error: remote tag ${TAG} exists at ${remote_sha:0:7}, not ${CI_COMMIT_SHA:0:7}" >&2
  echo "error: bump <Version> in Directory.Build.props" >&2
  exit 1
fi

git tag -a "${TAG}" -m "Release ${VERSION} (from Directory.Build.props via Woodpecker)"

git push origin "${TAG}"
echo "[OK] created and pushed tag ${TAG}"
