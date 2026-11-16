Role: auditor
Commit: 0b2aa9f6f248726254cd900d94a6a3b5cf2a929a
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta, round 1: head `0b2aa9f` against the earlier audited SHA `5ce6f54`. Base `origin/main` = `372daeee7a757fe7a3026896a594ece4df03fa8a` (matches the prompt; merged, since the base is an ancestor of the head). The checkout is the head SHA.

Outside `sdd/work/`, the repair diff is `scripts/usage.sh` only (18 insertions, 11 deletions). That is what `08-implementer.md` describes: one jq function `responses` (in `responses_def`) prepended to `turns_of`, `context_of`, and `summarize`, and a sign for negative numbers in the Markdown formatter `n`. The scope did not grow: no spec, change doc, lead text, or other surface changed. `scripts/lane-check.sh behavior-change`: confirmed, needs only tooling, as decision 9 records. All other evidence from `07-auditor.md` is reused unchanged, because the files it rests on (`squad/lead.md`, `scripts/README.md`, the change doc) are byte-identical between `5ce6f54` and `0b2aa9f`.

Previous findings:
- **#1 (FC-015, response filter copied three times): closed.** `rg 'select\(\(\.message \| type\)|group_by\(\.message\.id\)|max_by\(\.output_tokens\)' scripts/ squad/ .claude/` finds one owner, `scripts/usage.sh:107-109` (`responses_def`). `turns_of`, `context_of`, and `summarize` all call `responses`. `summarize` applies its tier with `map(.model |= tier)` after `responses`, so it still groups on the same key it used before.
- **07 hint, negative growth losing its sign: taken in by decision 10. It now holds.**

How I checked it:
- `bash -n scripts/usage.sh`: OK.
- I compared the old script (`git show 5ce6f54:scripts/usage.sh`) with the new one on this session's transcripts (`--session ac22de41-…`). Both `--json` and Markdown are byte-identical.
- I built my own transcript in the scratchpad, separate from the implementer's. It has two prompts, the large and medium tiers, a duplicated content-block line for one message id (output 10 then 30, effort only on the first line), a `<synthetic>` line, and first context 1,234,007 against last 3. It has three subagents: one response (growth 0), two responses with growth −5, and no response.
  - `--json`: byte-identical between old and new. The lead row shows −1234004. The duplicate counts once with output 30 and effort `high`. The no-response spawn has four nulls with empty `models` and `turns`. The turns split p1/p2 at 1/1.
  - Markdown: the only difference is the *Context growth* cells: `1.234.004` → `-1.234.004` (repeated on both tier rows), `5` → `-5`. The zero cell stays `0`, and the no-response spawn has no row, as before. Every other column and the tier and total tables are unchanged.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None. #1 is closed (see Scope)._

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which transcript lines are responses, and their per-message fields | `responses` (`scripts/usage.sh:106-109`): an object `message`, an object `usage`, a model other than `<synthetic>`; grouped by `message.id`; min timestamp, `.[0]` model, first effort, usage with max output tokens | Previously three copies (see 07). Now the single definition, and every consumer calls it | One owner. Old and new JSON are byte-identical on real and constructed transcripts | ok |
| Sign in the Markdown number format | `n`: `type == "number" and . < 0` → `"-"` + `n` of the absolute value; otherwise the digit grouping as before | Growth cell guard `if . == null then "-" else n end` (unchanged); all other columns are sums of non-negative counts | Null never reaches `n` for growth; non-negative values take the unchanged branch (identical output on real transcripts) | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Response filter and dedupe per message id | `scripts/usage.sh:107-109` only (`responses_def`) | one owner, ok |
| Markdown number format | `scripts/usage.sh:218` (`def n`) only | one owner, ok |
| Size rule, definition of context and growth | Unchanged since 07 (`squad/lead.md:50`, `scripts/README.md:133-134`, change doc, `usage.sh` header and `context_of` comment) | ok, as in 07 |

## Assertion adequacy
_N/A: the repair adds and changes no test (lead decision 7: `scripts/usage.sh` has no test project). I checked the script directly, as described under Scope._

## Failure classes dismissed
- FC-001 to FC-014, FC-016, FC-018 to FC-030, FC-033 to FC-044: no formatter, analyzer, code fix, C# rewrite, test, or unsupported shape is touched. The repair changes only a Bash/jq script (same reasons as 07).
- FC-015: covered under Policy ownership. Closed.
- FC-017: no test added or changed.
- FC-031: the new `responses_def` comment describes the function. The `summarize` comment drops the dedupe clause that `responses_def` now states. The header (lines 15-16, "responses are counted once per message id"), `turns_of`, and `context_of` comments remain true. The one imprecision is under Hints.
- FC-032, FC-037: no C# or build input changed.
- No `TODO` and no tracker number in `scripts/usage.sh`. No visible surface changes (check 10). Special cases (check 11): not applicable.

## Architecture findings
_None._

## Hints
- The `responses_def` comment says it is "the one definition every filter below starts from". The report-building filter (`scripts/usage.sh:~190`) and the Markdown filter come later in the file and do not start from it; they work on rows. Only the three per-transcript filters do. The behavior is unaffected. If the comment is touched again, "every per-transcript filter below" would be exact.

## Follow-up candidates
_None._
