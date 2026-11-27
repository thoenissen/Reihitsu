Role: implementer
Commit: dc2132b
Verdict: tests pass

## Changes
| File | What changed | Requirement |
|---|---|---|
| `squad/roles/implementer.md` | *Inputs*: added "In the tooling lane, which runs no spec author and a test implementer only when the tool has tests, also every issue of the run by number, read through the GitHub MCP tool `issue_read`." | Issue #1981, "Done when" (no spec covers role files) |

Verified before writing it, rather than taking `01-spec-author.md`'s proposal on trust:
- `.claude/agents/implementer.md` allows `mcp__github__issue_read`, so the clause agrees with the agent definition; `.claude/agents/README.md` states that every role reading the run's issues by number gets `issue_read`.
- "every issue of the run by number" is the phrasing of `squad/roles/test-implementer.md`, `skeptic.md`, and `auditor.md`.
- The lane reason matches the lane table in `squad/README.md` (Tooling: spec author –, skeptic –, test implementer "when the tool has tests").

The prompt carried no intended fix beyond pointing at the spec author's proposal; I checked that proposal as above and used it unchanged.

## Policy owners
N/A: no code policy touched. `grep -rn "issue of the run\|issue_read" squad/ .claude/agents/` found the phrasing owners listed above; no other role file states the implementer's inputs.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src`: empty. No new type.

## Deleted special cases
_N/A._

## Tests run
No tests apply (text in a role file). `scripts/lane-check.sh tooling --head worktree`:
```
TEXT-ONLY PROOF: PASS — base=f21c69c head=worktree — 3 changed file(s) (3 non-compiled, 0 C# comment/layout-only); xml-doc impact: none
LANE CHECK: confirmed — claimed tooling, diff needs tooling
```

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
