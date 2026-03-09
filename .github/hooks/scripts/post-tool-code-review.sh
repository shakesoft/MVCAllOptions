#!/usr/bin/env bash
# post-tool-code-review.sh
# Runs on the PostToolUse hook event (after every tool call by the agent).
# Filters to only file-editing tools and emits a systemMessage prompting
# the code-review skill whenever the AI has just made code changes.

set -euo pipefail

# Read the PostToolUse hook payload from stdin
input=$(cat)

# Extract the tool name from the JSON payload
tool_name=$(echo "$input" | grep -o '"tool_name":"[^"]*"' | head -1 | cut -d'"' -f4)

# Only act on file-editing tools — ignore all other tool calls
case "$tool_name" in
  create_file|replace_string_in_file|multi_replace_string_in_file|edit_notebook_file)
    ;;
  *)
    exit 0
    ;;
esac

# Emit a userMessage that actually triggers the code-review skill automatically.
# Returning a `userMessage` causes the agent to treat it like user input,
# so the slash command is fired without requiring manual intervention.
cat <<'EOF'
{
  "userMessage": "/code-review all local changes"
}
EOF
