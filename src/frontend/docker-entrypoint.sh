#!/bin/sh
set -eu

INDEX_HTML="/usr/share/nginx/html/index.html"

if [ -f "$INDEX_HTML" ]; then
  token='__VITE_API_URL__'
  value="${VITE_API_URL:-}"

  # Escape for sed replacement delimiter.
  escaped=$(printf '%s' "$value" | sed -e 's/[\\&/]/\\\\&/g')
  sed -i "s|$token|$escaped|g" "$INDEX_HTML"
fi

exec nginx -g 'daemon off;'
