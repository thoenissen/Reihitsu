Role: auditor
Commit: 147bd4f641fc902c9c420c861e4e17a1784dfdc5
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head 147bd4f (checked out, clean tree). `git ls-remote origin refs/heads/main` = eb89003591b8371d91e4a14b8590d764bdf6d605, which is the SHA in my prompt and an ancestor of HEAD. The diff against it is what the handovers describe: `PipelineTraceCommand.cs` (+11/−1), `PipelineTraceCommandTests.cs` (+198), `scripts/README.md` (one sentence), the change doc, and `sdd/work/trace-hiding-skip/`. `scripts/lane-check.sh behavior-change`: confirmed.

Issue thoenissen/Reihitsu#1724 (`agent-filed`), "Done when": the trace of such a file says whole-document formatting skips it (met: `Skipped (code hidden in another build configuration): <path>`), by the public `ReihitsuFormatter.HoldsHidingCarrier` (met), and a test covers it (met, in `Reihitsu.Cli.Test`, not a separate tooling project; lead decision 3). The spec alternative and the "trace anyway" option are dropped in the change doc with reasons; nothing visible to an end user depends on them.

Reachability: `scripts/trace/trace.cs` is the only non-test caller of `PipelineTraceCommand.ExecuteAsync` (`InternalsVisibleTo("Reihitsu.Trace")`); `reihitsu-format`'s `Program` does not route to it. The trace's output is therefore repository tooling, not a visible surface (check 10 has nothing to cover).

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Skip a file holding a hiding carrier (`PipelineTraceCommand.GetInvalidTraceExitCode`, new third check) | the whole parsed tree of the pass, via `ReihitsuFormatter.HoldsHidingCarrier(syntaxTree)` → `ReihitsuFormatterHelpers.HoldsHidingCarrier(root)` | `ReihitsuFormatter.FormatDocumentAsync`/`FormatSyntaxTree` (`ReihitsuFormatter.cs:68`, `:104`), `FormatCommandHandler.ProcessFileAsync` (`:331`, via `DefaultSourceFormatter`), `PlaygroundFormatter.Format` (`:33`) | Same predicate, same tree span, no copy. Order syntax errors → generated content → carrier matches the formatter's entry points and CLI-021; the trace's earlier file-name check (`GeneratedFileUtilities.IsGeneratedFile`) matches the CLI's walk-time skip. Parse options: trace parses with default options, CLI with `CSharpParseOptions.Default.WithLanguageVersion(...)`; no preprocessor symbols in either, so the active/inactive group structure the predicate reads is the same; the language-version difference is pre-existing and shared with the syntax-error check | holds |
| Later pass (`pass > 1`) → stderr + exit `2` | same predicate on the reparsed pass output | the two existing later-pass branches in the same method | identical shape through `WriteInvalidPassMessage` | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which tree holds a hiding carrier (FMT-PIPE-033) | `rg -n HoldsHidingCarrier src --glob '!*Test*/**'`: owner `ReihitsuFormatterHelpers.HoldsHidingCarrier` (`src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs:88`); public facade `ReihitsuFormatter.HoldsHidingCarrier` (`ReihitsuFormatter.cs:457`); callers `ReihitsuFormatter.cs:68,104`, `DefaultSourceFormatter.cs:32`, `ReihitsuCodeFixProviderBase.cs:102` (cached), `PlaygroundFormatter.cs:33`, `PipelineTraceCommand.cs:218` | one owner; the trace adds a caller, no copy |
| Skip reason text `code hidden in another build configuration` | `FormatCommandHandler.cs:335`, `PipelineTraceCommand.cs:220` | two literals, as `syntax errors` and `generated` already are; the change doc chooses the CLI's wording deliberately. Not a decision, only a label; acceptable |
| Ordered list of whole-document skips | `ReihitsuFormatter.cs:62–69`, `:95–105`, `FormatCommandHandler.cs:309–339`, `PlaygroundFormatter.cs:27–36`, `PipelineTraceCommand.cs:204–223` | five written-out copies; pre-existing structure the skeptic reported, lead decision 2 fixes narrowly and keeps the shared skip query as a follow-up candidate. The narrow fix adds no new owner of the carrier rule |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ExecuteAsyncHiddenCodeIsSkippedWithoutRunningAnyPhase` (verbatim `#else` and `/* */` `#endif`, LF and CRLF) | a carrier file is skipped on pass 1 with the reason, no phase runs, exit 0, file untouched | any extra stdout line (a `Pass 1`, a diff), any stderr, another exit code, changed bytes | exact `AreEqual` on whole stdout, empty stderr, `AreSequenceEqual` on bytes | adequate (FC-017); two carrier kinds and two directive words make a string-only private copy fail |
| `ExecuteAsyncHiddenCodeIsSkippedBeforeUnsupportedSyntaxIsRefused` | carrier skip precedes the precheck | the precheck's refusal on stderr, a `Pass 1` line | exact stdout, empty stderr | adequate; bytes not asserted here, but the previous test pins that and the precheck path never writes |
| `ExecuteAsyncEarlierSkipTakesPrecedenceOverHiddenCode` (`// <auto-generated/>`, `.g.cs`, trailing `internal class {`) | syntax-error and generated skips win over the carrier skip | the carrier reason or a `Pass 1` | `Contains` full-path reason, `DoesNotContain` carrier reason and `Pass 1`, empty stderr, bytes | adequate. These rows were green before the change, so I checked they can fail: in a throw-away worktree I moved the carrier check to the top of `GetInvalidTraceExitCode`; the `// <auto-generated/>` and syntax-error rows failed (2 failed, 1 passed — the `.g.cs` row is decided by the file-name check before parsing, as expected). Worktree removed |
| `ExecuteAsyncLiteralOutsideHiddenCodeDefinitionIsTraced` (`#else` line without enclosing `#if`; `# Title` inside `#if true`) | both sides of FMT-PIPE-033's conditions are traced, not skipped | a `Skipped` line, no `Pass 1`, no phase diff | `DoesNotContain("Skipped")`, `Contains("Pass 1"+NewLine)`, `Contains` a phase header, empty stderr, bytes | adequate (FC-003) |

The later-pass branch has no test; no input is known that makes a phase produce a carrier (skeptic probed the documentation-comment relocation), the two existing later-pass branches have none either, and the change doc records the decision. Not a required test.

## Failure classes dismissed
- FC-001, FC-002 — the new check is a call to the shared predicate on the whole tree, not a narrowed guard; no qualifier of FMT-PIPE-033 is re-expressed in the trace.
- FC-003 — covered: both sides of the group condition and the directive-word condition are tested.
- FC-004, FC-005, FC-019, FC-020, FC-021 — the change edits no trivia; the skipped file is only read, and bytes are asserted unchanged.
- FC-006, FC-022–FC-027, FC-029, FC-036 — no rewrite; literal kinds are axes of the shared predicate, covered by the formatter's tests, and two kinds are exercised here.
- FC-007, FC-008, FC-009, FC-018, FC-030, FC-043 — no code fix touched.
- FC-010, FC-011, FC-012 — no phase changed; the trace's loop is bounded by `passes`, and the new branch returns.
- FC-013, FC-014 — no analyzer or rule touched.
- FC-015, FC-016 — one owner (Policy ownership); the sibling skips already exist; no other private copy of the carrier rule found.
- FC-017 — see Assertion adequacy.
- FC-028, FC-041 — no node-level entry point involved; the trace always works on a whole tree.
- FC-031 — the one changed method's summary names the three checks and their order, matching the code; `ExecuteAsync`'s `returns` ("legitimately skipped") still holds.
- FC-032 — no project result reused across a compiled change; the focused tests were run on the final code by the implementer and cleaner.
- FC-033, FC-034 — fixtures use an active `#if true` group (a `#if DEBUG` fixture would hit the syntax-error skip); LF and CRLF both tested for the main case.
- FC-035 — expected values are the trace's literal skip line, not taken from patched output.
- FC-037 — no comment-only change claimed.
- FC-038, FC-039, FC-040, FC-042 — no unsupported shape declared or precheck changed; the carrier is not an FMT-UNSUP shape (FMT-PIPE-033 decided against declaring it), so no guard serves a declared shape.
- FC-044 — the trace reads the formatter's computed verdict, models nothing.

## Architecture findings
_None_ new. The five written-out copies of the ordered skip list (skeptic, `02-skeptic.md`) are pre-existing; this change follows lead decision 2 and adds no copy of the carrier rule.

## Hints
- The summaries of `ReihitsuFormatter.HoldsHidingCarrier` (`src/Reihitsu.Formatter/ReihitsuFormatter.cs:450–452`) and `ReihitsuFormatterHelpers.HoldsHidingCarrier` (`src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs:82–83`) list their consumers ("the command-line tool and the playground ask it to name such a file as skipped") and do not name the pipeline trace. Both remain true as written ("all of them ask this one definition"), and neither method changed, so this is not FC-031; adding "the pipeline trace" would only complete the list.
- `GetInvalidTraceExitCode` calls `ReihitsuFormatter.HoldsHidingCarrier(syntaxTree)` without the cancellation token; the root is already materialized on line 147, so `GetRoot` does no work. Cosmetic.

## Follow-up candidates
_None_ beyond the one in `00-lead.md` (one shared "why does whole-document formatting skip this file" query for the five callers; structural cause: one rule stated in five places).
