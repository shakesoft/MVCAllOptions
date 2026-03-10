#!/usr/bin/env bash
# stop-code-review.sh
# Runs on the Stop hook event (agent session ends).
# Emits a systemMessage instructing the agent to invoke the code-review skill
# so any changes made during the session are reviewed before the session closes.

set -euo pipefail

# Read stdin (Stop hook payload) — discard, we don't need it
input=$(cat)

# Check whether there are any local changes worth reviewing
cd "$(git rev-parse --show-toplevel 2>/dev/null || echo ".")"

has_staged=$(git diff --cached --quiet && echo "no" || echo "yes")
has_unstaged=$(git diff --quiet && echo "no" || echo "yes")

if [[ "$has_staged" == "no" && "$has_unstaged" == "no" ]]; then
  # No local changes — nothing to review, exit cleanly
  exit 0
fi

# Emit a systemMessage asking the agent to run the code-review skill
cat <<'EOF'
{
  "systemMessage": "The agent session is ending. Before stopping, please invoke the `code-review` skill to review all local changes (staged and unstaged) in this session against the ABP Framework rules defined in `.cursor/rules/` and `.github/copilot-instructions.md`. Run: `/code-review` with scope 'all local changes' (staged + unstaged). Produce the full structured review report."
}
EOF
