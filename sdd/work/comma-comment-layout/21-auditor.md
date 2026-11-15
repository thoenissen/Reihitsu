Role: auditor
Commit: 31dd2dd137cfbbbb236f54da09c018c3c487c0bd
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the issues, the slug, the SHAs, the previous audit, and the handovers to read.

## Scope

Repair delta against `d1741804bd97da6e977138d52b67f995bc997266` (13-auditor). Head `31dd2dd137cfbbbb236f54da09c018c3c487c0bd`: checkout verified, working tree clean. Base: `origin/main` = `git ls-remote origin refs/heads/main` = `450f2746e63d6e6a4a73df0a2c50dc4f1e83d315`, an ancestor of head (merged). `scripts/lane-check.sh behavior-change`: confirmed.

The repair is what handovers 14–20 describe, and nothing else:
- two production files: `TrailingCommaRemovalUtilities.cs` and `RH5107CommaMustBeOnSameLineAsPreviousParameterCodeFixProvider.cs`;
- three test files: `TrailingCommaDocumentationCommentTests.cs` (new), `LeadingCommaDocumentationCommentTests.cs` (new), and five added RH5107 tests;
- RH5107-004, FMT-STRUCT-019, FMT-BREAK-012, FMT-BREAK-038, the FMT-UNSUP-009 `Why:` and its `Uses` row, and three change docs.

Scope did not grow, and no requirement ID was added. The two merges of `origin/main` (#1750/#1752 earlier; #1751 and #1753 here) touch no production file of this run (`git diff --name-only` intersection is empty). I checked the one interaction that matters, layout rules judging comment-led lines (#1753) against the run's decision-18 output (FC-013, below).

Evidence came from static tracing plus scratch fixtures outside the repository:
- the branch's CLI, run twice for stability;
- `scripts/apply-fix.sh` for RH5107, RH5301, RH5303, RH5304, RH5305, RH5306, RH5307, RH5203, and RH5109;
- a file-based harness calling `ReihitsuFormatter.FormatSyntaxTree` under `DocumentationMode.None` and `Parse`;
- for before/after comparison, a scratch worktree of `origin/main` 450f274 (which does not contain the run).

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Every member the finding listed now keeps the documentation comment, also as a second pass: enum (one-line, multi-line, with `// after`, two documentation comments), array initializer (one-line, multi-line, nested, `stackalloc`), collection initializer (one-line, broken to lines, nested, complex element `{ 1, 2 } /** e */,`). The comment survives alongside `/* */` behind the comma and with `}` on the next line. Each output is byte-identical to `main`'s first pass, except the member in #3. The tree now matches its reparse (FMT-PIPE-021): the trivia leads `}`. `ContainsNonFormattingTrivia(separator.LeadingTrivia)` is under *Checked and kept*, and the `Why:` and the method summary are corrected. |
| 2 | closed (a), superseded (b) | (a) RH5107: `apply-fix` withholds the fix for `, /** c */ int b`, `, /// c⏎int b`, `,/** c */int b`, and `, /** c */ /* d */ int b` under LF and CRLF. It keeps the fix for `,⏎/** c */ int b` and `, int b /** d */`. (b) FMT-BREAK-012/038: decision 18 changed the requirements. A documentation comment in `Parse`/`Diagnose` is now placed by the documentation-comment phase, as for comma-last code; characterization tests pin it. The decision-18 output is stable, and RH5301, RH5303, RH5304, RH5305, RH5306, RH5307, RH5203, and RH5109 report nothing on it after the #1753 merge. |

Audit-1 hints:
- Zero-width loop in `PrecheckPhase.EnumerateGapTrivia`: refuted for trees with syntax errors, which reach it through `FindUnsupportedSyntax` in code fixes (16-implementer). Kept, accepted.
- FMT-BREAK-038 line-count wording: aligned to text lines.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | medium | `TrailingCommaRemovalUtilities.PreserveSeparatorTrivia`, new early-return branch; `TrailingCommaDocumentationCommentTests`; *Checked and kept* entry in `sdd/changes/2026-11/comma-behind-comment-unsupported.md` ("the text is the same") | FC-017 (missing member), FC-010 | – | PR-introduced | **No test covers the one member whose formatted output differs from `main`.** Moving the comma's trivia so it leads `}`, rather than trails the last element, keeps the structural phase's text. Later phases now see the documentation comment where a reparse puts it, and that changes the final output when a comment line follows. Input (CLI, `Parse`): `enum E3 { A, B /** doc B */, ⏎ // note ⏎ }` (multi-line). Head gives `B /** doc B */⏎    // note`, stable. `main` gives `B /** doc B */⏎⏎    // note` on the first pass and removes the blank line on the second: an idempotency break on `main` (FMT-PIPE-004) that the repair fixes. The same holds for an array initializer (`2 /** two */,⏎// note⏎};`). Head's output equals the formatter's output for the same text written without the comma, so FMT-PIPE-004 (unchanged) decides it, and no maintainer question is needed. But no test pins it: every new row has the comment as the last thing before `}` or before a `//` on the same line. The change doc tells the reader nothing visible changes, which is true only of the removal step. **Members that hold:** every other member listed under #1 (output byte-identical to `main`). | Add a row per removing transform (enum, array initializer; collection initializer optional) with a `//` comment line between the comma's line and `}`, in `Parse` and `Diagnose`, expecting no blank line. Note in the change doc that this input's output now converges in one pass where `main` needed two. |
| 4 | low | `RH5107CommaMustBeOnSameLineAsPreviousParameterCodeFixProvider.ApplyCodeFixAsync`, inline comment "Content remains on the comma's line after it (the next element, or a comment)" | FC-031 | – | PR-introduced | The run's guard (round 0 for `//` and `/* */`, this repair for `/** */` and `///`) now withholds the fix whenever a comment follows the comma on its line, in every documentation mode. The `else` branch is reached only with the next element behind the comma, so "or a comment" describes a case that can no longer occur. Comment only; it cannot reach compiled behavior. | Drop "or a comment" (the next element is the only content left). |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Keep a removed trailing comma's leading trivia (`ContainsNonFormattingTrivia(separator.LeadingTrivia)` → trivia leads `}`) | The comma's leading trivia | FMT-UNSUP-009 precheck (`FindTriviaInFrontOfComma`): non-formatting trivia in front of a comma on a later text line is refused | When the branch fires, the trivia is on the element's line. That is only a `/** */` in `Parse`/`Diagnose`: `//`, `/* */`, and None-mode comments trail the element, a directive needs its own line, and skipped tokens mean a syntax error, which is not formatted. A multi-line `/** a⏎b */,` puts the comma on a later text line and is refused. | agree |
| Early return of that branch vs. the FMT-STRUCT-027 path (`lastToken.TrailingTrivia.Any(EndsLine)`) | The last element's trailing trivia | Same precheck | The branch fires only when the comma is on the element's line, so the last token's trailing trivia has no line break. The two paths never overlap. | agree |
| RH5107-004 guard `HasCommentBehindCommaOnItsLine` | The comma's trailing trivia up to its line break. When that has none, the next token's leading trivia up to the first `EndOfLineTrivia`. | RH5107-004's two conditions; the guard in front of the comma (`SpanContainsCommentOrDirective(previousToken.Span.End..comma)`); the formatter's collapse, which carries only the comma's trailing trivia (decision 18) | The trailing trivia stops without a line break only when the next token is on the same line or when a documentation comment (`Parse`/`Diagnose`) stopped the scan. The second loop sees exactly that comment. `///` carries its own newline in its structured trivia and is caught before it. A comment that starts a later line is behind the comma's line break, so the fix is kept (spec `V`, fixtures `g`). The fix and the formatter cannot disagree, because the fix is withheld for every comment the formatter places. | agree |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Trivia of a removed trailing comma | `rg "separator\.LeadingTrivia\|comma\w*\.LeadingTrivia" src --glob '!*Test*'`: only `TrailingCommaRemovalUtilities.PreserveSeparatorTrivia`, called by `EnumTrailingCommaRemovalTransform` and `InitializerTrailingCommaRemovalTransform` | one owner, ok |
| "A comment behind the comma on its line" (RH5107) | `rg "HasCommentBehindComma\|CommentBehindComma"`: the private `HasCommentBehindCommaOnItsLine` only. `rg "public static bool \w*(Comment\|SameLine\|Behind)\w*\(" src/Reihitsu.Core` finds no shared helper with this span (`HasCommentDirectlyAbove`, `GapContainsComment`, and `SpanContainsComment` inspect other spans). | one owner, ok. The second owner of comma placement (the fix's text edit vs. the line-break phase) is still decision 6's follow-up. |
| Placement of a documentation comment behind a comma | The documentation-comment phase (decision 18). The line-break collapse carries only the comma's trailing trivia. | ok, as specified in FMT-BREAK-012/038 |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `TrailingCommaDocumentationCommentTests` (4 tests × Parse, Diagnose, None) | FMT-STRUCT-019/020: the comment stays behind the last member/element in every mode | Comment dropped or moved; second-pass change; tree not equal to its reparse | `AssertRuleResult` (LF, CRLF, second pass, phase stability, FMT-PIPE-021 tree check) | adequate for its rows; misses the comment-line member (#3) |
| `RH5107…VerifySpecificationExampleDocumentationCommentsBehindTheCommaWithholdTheFix`, `…VerifyNoCodeFixForEachDocumentationCommentBehindTheComma`, `…WithCarriageReturnLineFeeds` | RH5107-004 for `/** */` and `///` in each mode | A fix offered | `VerifyNoCodeFixIsOffered(source, DocumentationMode)` (markup diagnostics plus action enumeration) | adequate; failed on d1741804 for the Parse/Diagnose rows (15-test-implementer) |
| `…VerifyDocumentationCommentOnTheLineBehindTheCommaKeepsTheFix`, `…VerifySingleLineDocumentationCommentOnTheLineBehindTheCommaKeepsTheFix` | The other side of the line condition (FC-003) | Fix withheld, or other text | `Verify` (verifier default mode, Diagnose) | adequate. The input's comment is behind the comma's line break, so the guard's outcome does not depend on the mode. |
| `LeadingCommaDocumentationCommentTests` (decision-18 rows, Parse/Diagnose/None) | FMT-BREAK-012/038 as clarified; comma-first equals comma-last | Other text; comma-first ≠ comma-last; instability | `AssertRuleResult` with parse options | adequate. These are characterization tests by decision 18. Their expected values are the spec examples, and they also hold on head's CLI. |

## Failure classes dismissed
- FC-001, FC-002: guard scope table. Each new predicate's span matches its counterpart's.
- FC-003: both sides of RH5107-004's line condition are tested, for `/** */` and `///`. The trailing-comma branch is tested in the modes that do and do not fire it (None vs. Parse/Diagnose).
- FC-004, FC-021: no trivia is lost in the probes listed under #1. A doc comment with `/* */` behind the comma, with `//` behind it, and nested give `main`'s text.
- FC-005, FC-020: a directive cannot share the element's line. A conditional boundary between the comma and `}` still skips the removal (`ContainsConditionalCompilationBoundary`), which this repair leaves unchanged.
- FC-006, FC-022–FC-026, FC-029, FC-036: the change touches no tokens other than the removed comma.
- FC-007, FC-008, FC-009: the RH5107 fix is only withheld for more inputs. Fix All goes through the same registration guard.
- FC-010: CRLF is covered by `AssertRuleResult` and by the RH5107 CRLF rows. `apply-fix` ran LF and CRLF.
- FC-011, FC-012: no loop or phase pairing changed. Every fixture is stable on a second pass.
- FC-013: the decision-18 output on head (after the #1753 merge) is reported by none of RH5301, RH5303, RH5304, RH5305, RH5306, RH5307, RH5203, or RH5109.
- FC-014, FC-018, FC-030: no new rule, no naming fix, no fix scope changed.
- FC-015, FC-016: policy table. Both removing transforms share the one helper.
- FC-017: see the assertion table and #3.
- FC-019: comment rows exist; directives are dismissed above.
- FC-027: nested initializers and complex elements were probed and match `main`.
- FC-028, FC-041, FC-043: the structural phase runs alike in every entry point. The RH5107 fix does not call the formatter.
- FC-032: every result was taken on head 31dd2dd. The CLI was built from it, and the analyzer and formatter results in 20-test-implementer are on the merge 569da61, whose `src` equals head's.
- FC-033, FC-034: the reproductions are 13-auditor's shapes verbatim.
- FC-035: the expected values come from the spec examples and from `main`'s text (FMT-STRUCT-019 example = `main`'s output). They are not taken from patched code.
- FC-037: no text-only claim.
- FC-038: head's output with `/** */` leading `}` is not refused on a second pass. The CLI reported nothing unsupported, exit 0. FMT-UNSUP-004 does not apply: the comment is inside the initializer value. FMT-UNSUP-009 does not apply: `}` is not a comma.
- FC-039: the precheck is unchanged.
- FC-040: the run's rows give the same output in all three modes. The one mode-dependent output found is pre-existing (hint).
- FC-042: the restored guard serves an input outside the declared shape and is recorded under *Checked and kept*. No guard that serves only the shape was added.

## Architecture findings
_None._

## Hints
- Pre-existing, outside the run (FC-040 in the blank-line phase): for the comma-less text `B /** doc B */⏎    // note⏎}`, the formatter inserts a blank line in front of `// note` under `DocumentationMode.None` and none under `Parse`. The harness calls `FormatSyntaxTree`, and both outputs are stable. The phase decides by which token's trivia carries the documentation comment. With the repair, a removed trailing comma gives exactly this text's output, so #3's expected value is the `Parse` row. A `None` row would expect the blank line, unless that phase is changed.

Fixtures (scratch, outside the repository):
- `/tmp/claude-0/-home-user-Reihitsu/860180bf-a5c6-59a9-840c-7ebc2946f7a6/scratchpad/a21/` (`e1.orig` input, `e1.cs` head output), `/tmp/claude-0/-home-user-Reihitsu/860180bf-a5c6-59a9-840c-7ebc2946f7a6/scratchpad/a21m/` (main output), `/tmp/claude-0/-home-user-Reihitsu/860180bf-a5c6-59a9-840c-7ebc2946f7a6/scratchpad/b21/` and `/tmp/claude-0/-home-user-Reihitsu/860180bf-a5c6-59a9-840c-7ebc2946f7a6/scratchpad/b21m/` (#3, head and main), and `/tmp/claude-0/-home-user-Reihitsu/860180bf-a5c6-59a9-840c-7ebc2946f7a6/scratchpad/h21/` (trivia combinations);
- `/tmp/claude-0/-home-user-Reihitsu/860180bf-a5c6-59a9-840c-7ebc2946f7a6/scratchpad/f21/` (RH5107 shapes) and `/tmp/claude-0/-home-user-Reihitsu/860180bf-a5c6-59a9-840c-7ebc2946f7a6/scratchpad/g21/` (decision-18 output for the layout rules);
- `/tmp/claude-0/-home-user-Reihitsu/860180bf-a5c6-59a9-840c-7ebc2946f7a6/scratchpad/app/` (documentation-mode harness);
- a scratch git worktree of 450f274 for the `main` comparison, removed again after the audit.
