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

# Emit a systemMessage asking the agent to run the code-review skill
cat <<'EOF'
{
  "systemMessage": "You just made code changes. Please invoke the `code-review` skill to review all local changes (staged and unstaged) against the ABP Framework rules defined in `.cursor/rules/` and `.github/copilot-instructions.md`. Run: `/code-review` with scope 'all local changes' (staged + unstaged). Produce the full structured review report."
}
EOF
