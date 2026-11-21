Role: auditor
Commit: fe965f7b412bac04c2f817ab5d7a46cf96a5a7de
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Scope

This is a repair delta for repair round 2 (decision 26), compared with the last audited tree 4982c76 (15-auditor) plus the proven text-only cleanup a244cc1 (16-spec-author). It covers the diff 87c9a26..fe965f7. I checked out head fe965f7 with a clean tree. `git ls-remote origin refs/heads/main` gives d63eab4bee09632526c02bfb4a46f54aabdd8001, which is an ancestor of the head.

The diff matches handovers 17–20 and decision 26. Outside `sdd/work/` it touches one file, `src/Reihitsu.Tooling/LaneCheckCommand.cs`. The validation that followed the argument loop of `TryParseArguments` moved into the new private `TryValidateArguments`, and the diff adds 26 lines and removes none. The repair stayed inside a surface the first audit covered, changed no spec, test, or Markdown outside `sdd/work/`, and added no type, so this is a delta audit.

Previous findings:
- **#1 — closed** (15-auditor). Nothing in this round touches it.
- **#2 — closed** (15-auditor). Its open member became #4.
- **#3 — closed** (15-auditor). Nothing in this round touches it.
- **#4 — closed.** Text cleanup a244cc1 deleted the derived sentence. `rg "fails the text-only proof"` over `scripts/README.md` and `sdd/changes/` finds nothing.

This round has no new finding, so none repeats an earlier class. The RH2008 failure from 17-validator is not an audit finding of an earlier round. It is a line-limit violation in the code this run added, now repaired.

## Findings
_N/A — no confirmed finding in the repair delta._

Evidence for the refactor:
- **The moved block is byte-identical.** I extracted the text from `if (showHelp)` to the end of the file at 87c9a26 and at fe965f7 and compared them with `diff`. The only difference is the inserted `return TryValidateArguments(...)` and the new method header with its re-initialization of the out values. Every check, its order (help, `--wording-paths` exclusivity, `--lane` via `LaneClassifier.TryParseLane`, `--text-only`), and every error text are unchanged.
- **The out values are the same on every path.**
  - Early exit through the `default:` case of the loop: `TryParseArguments` sets `claimed = Lane.Docs`, `isTextOnlyProven = false`, and `error`, as before.
  - Every path through validation: on entry, `TryValidateArguments` sets `claimed = Lane.Docs`, `isTextOnlyProven = false`, `error = null`. Those are exactly the values the old code had at that point, because the loop never assigns them.
  - `showHelp` and `listWordingPaths` are still assigned only in `TryParseArguments`.
  - `Execute` still consumes all five out values in the same order.
- **The line limits hold.** 18-implementer reports that the RH2008/RH2111 line-count rows pass (18 of 18), and these rows scan the whole solution. Every other analyzer runs in the Release build from `binaries/`, and 18-implementer and 19-cleaner report 0 warnings. `scripts/format.sh` changed nothing in the file.
- **No formatter self-hosting row exists for this file.** `src/Reihitsu.Tooling` is not in `FormatterSourceCatalog.SourceDirectories`. `scripts/test.sh --no-build --project selfhosting --filter "Name~LaneCheckCommand"` matched no test. Log: `/tmp/claude-0/-home-user-Reihitsu/ba32ea5f-3902-5d32-8481-e4d5998124ec/scratchpad/audit21-selfhost.log`.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Argument validity for `lane-check` (help short-circuit, `--wording-paths` exclusivity, lane name, text-only value) | `laneName`, `textOnly`, `showHelp`, `listWordingPaths` as the parse loop leaves them; the same values the old inline block read | `LaneClassifier.TryParseLane` for the lane name, unchanged | Same inputs, same order, same boundaries; the block is moved byte for byte | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Lane-name parsing | `rg -n "TryParseLane" src --glob '!**/*Test*/**'`: one owner, `LaneClassifier.TryParseLane` (`LaneClassifier.cs:188`), whose only production caller is `TryValidateArguments` | one owner, no copy introduced |
| `--wording-paths` exclusivity | `rg -n "takes no other argument" src --glob '!**/bin/**'`: only `TryValidateArguments`, plus the test assertion at `LaneCheckCommandTests.cs:460` | one owner |

## Assertion adequacy
_N/A — the repair adds and changes no test. The existing `LaneCheckCommandTests` cover the moved checks: the `--wording-paths` exclusivity error at line 460, the unexpected-argument error at line 476, and the valid `--lane`/`--text-only` combinations throughout. 18-implementer and 19-cleaner report all 204 tooling tests passing on this tree. Before the move, nothing tested the invalid `--lane` value or the invalid `--text-only` value either. The move does not change those two branches. See Hints._

## Failure classes dismissed
- FC-001, FC-002: the repair changes no predicate's span; see *Guard scope*.
- FC-015: no policy gained a second owner; see *Policy ownership*.
- FC-017: no new or changed test.
- FC-031: the new method's summary and parameter documentation describe it: it validates the parsed values and derives the lane and the verdict. The summary of `TryParseArguments` ("Parses the command arguments") and its parameter documentation still hold, because it still parses and still returns all five values. No `TODO`, no commented-out code, no tracker reference.
- FC-032: the self-hosting line-count rows that caught the defect were run on the repaired tree (18-implementer). The build is warning-free, so every other analyzer is satisfied.
- All other classes (FC-003 to FC-014, FC-016, FC-018 to FC-030, FC-033 to FC-044): the repair is a behavior-preserving extraction in a tooling command. It touches no formatter phase, analyzer, code fix, trivia, proof code, script, or Markdown process text. The dismissals in 10-auditor and 15-auditor stand.

## Architecture findings
_None._

## Hints
- `LaneCheckCommandTests` does not test the two error texts `--lane expects …` (unknown or missing lane) and `--text-only expects …`. They predate this run and the repair only moved them, so this does not block. A test for each would pin the extraction against a future reorder.

## Follow-up candidates
_None_ beyond the lead's candidate 2 in `00-lead.md`. This round's RH2008 failure in a production file already serves as its evidence.
