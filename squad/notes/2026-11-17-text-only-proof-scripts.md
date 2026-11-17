# text-only-proof-scripts

| | |
|---|---|
| Date | 2026-11-17 |
| Run | issues #1808, #1817 |
| Pull request | #1833 |

## Questions to the maintainer

None. Both issues are `agent-filed` tooling issues whose either/or goals touch no visible output, so the lead decided them.

## Cost

No stops and no repair rounds. The test implementer ran twice: the first pass left a case open (a `#:package` directive without a version), which the lead decided and routed back for a test. The cleaner ran twice: the second pass fixed a stale test summary the auditor found, proven text-only against the audited SHA, so no second audit was needed. The auditor and the implementer were the most expensive spawns.

## Usage

Session `22802854-e950-5f23-bf94-53602cf88cc7`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 9 | 39 | 91 | 15.155 | 3.067 | 5.390.366 | 100.108 | 69.573 |
| test-implementer | 1 | a4a4f6b124faf6497 | Test implementer for #1808/#1817 | large | medium | 1 | 13 | 27 | 14.254 | 2.811 | 1.280.546 | 68.109 | 43.710 |
| implementer | 1 | a0285f26ed77267c6 | Implementer for #1808/#1817 | large | medium | 1 | 33 | 68 | 25.453 | 7.952 | 3.493.910 | 135.870 | 61.033 |
| test-implementer | 2 | a3b2dd15f3c6a2f50 | Test amendment for #1817 | large | medium | 1 | 9 | 18 | 5.383 | 223 | 778.860 | 47.858 | 23.552 |
| cleaner | 1 | af3f4e8fa3a11f521 | Cleaner for #1808/#1817 | medium | low | 1 | 5 | 10 | 1.327 | 47 | 308.947 | 79.025 | 4.377 |
| spec-checker | 1 | a580448299de2b283 | Spec checker for #1808/#1817 | medium | medium | 1 | 5 | 10 | 2.898 | 240 | 346.504 | 95.175 | 20.989 |
| auditor | 1 | abf29f995995042cf | Auditor for #1808/#1817 | large | high | 1 | 24 | 50 | 22.866 | 10.724 | 3.037.584 | 106.959 | 82.901 |
| cleaner | 2 | ae351feaa9146debe | Cleaner: audit comment finding | medium | low | 1 | 5 | 10 | 1.403 | 0 | 325.206 | 84.423 | 9.640 |
| validator | 1 | ab2d6047c6f8160ac | Validator for #1808/#1817 | small | low | 1 | 6 | 12 | 3.039 | 799 | 105.091 | 57.896 | 7.966 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 118 | 254 | 83.111 | 24.777 | 13.981.266 | 458.904 |
| medium | 15 | 30 | 5.628 | 287 | 980.657 | 258.623 |
| small | 6 | 12 | 3.039 | 799 | 105.091 | 57.896 |
| **Total** | 139 | 296 | 91.778 | 25.863 | 15.067.014 | 775.423 |

## Decisions taken

Nine. These are the ones the maintainer may want to see:
- #1808: Markdown under `scripts/` counts as text for the text-only proof, and Markdown under `.github/workflows/` does too.
- #1817: the pin follows `RoslynVersion`, and the guard checks `#:package` directives instead of exempting the script. A Roslyn `#:package` with no version counts as drift.

## Suggestions

- In the tooling lane the implementer writes `scripts/README.md` text, because the lane has no spec author. Here that worked without friction. A tool with no tests, such as the text-only proof, is checked only by hand, both by the implementer and by the auditor.
