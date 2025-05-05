#!/usr/bin/env bash
set -e

ENV_FILE=".env"

# 1. Check for .env
if [[ ! -f "$ENV_FILE" ]]; then
  echo "Error: $ENV_FILE not found in $(pwd)"
  exit 1
fi

# 2. Load and export all variables from .env
set -o allexport
# shellcheck disable=SC1091
source "$ENV_FILE"
set +o allexport

# 3. Verify OPENAI_API_KEY is set
if [[ -z "$OPENAI_API_KEY" ]]; then
  echo "Error: OPENAI_API_KEY is not set in $ENV_FILE"
  exit 1
fi

# 4. Run the Docker container
docker run \
  -e OPENAI_API_KEY="$OPENAI_API_KEY" \
  -d --rm \
  -p 9001:9001 \
  kernelmemory/service
