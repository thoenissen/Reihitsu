Role: auditor
Commit: ba3cf9d
Verdict: findings

## Scope

Full audit of thoenissen/Reihitsu#1497. Head `ba3cf9d` (checked out, clean). Base `origin/main` `b27f3295165c309d3348a44af6a5b5b5a5d4e24c`, which is the merge base, so the branch is not behind. `scripts/lane-check.sh behavior-change`: confirmed. The diff matches what `05-implementer.md`, `06-implementer.md` and `04-test-implementer.md` describe. The production diff touches 4 files: `UsingDirectiveOrderingUtilities.cs` (Core), the RH7207 analyzer, `UsingDirectiveOrderingRewriter.cs`, and `UsingLeadingTriviaBuilder.cs`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

The build and the focused test results come from the cleaner and the implementers, and I trusted them. I checked the following with `scripts/format.sh` (twice) and `scripts/apply-fix.sh RH7207` on fixtures in the scratchpad, under LF and CRLF. Where noted, I also ran them in a temporary `origin/main` worktree, which I have since removed:

- `f1`, `f2`, `f5`, `f6`, `k`: reorders with a line-spanning delimited comment in a predecessor's trailing trivia, a header, or two comment-led directives.
- `f3`: a whitespace-only blank line above the comment.
- `f4`, `g9`: a block-scoped and a file-scoped namespace.
- `g1`, `g2`: a documentation comment on the directive's line.
- `g3`: a trailing comment, plus a glued comment through a reorder.
- `g6`, `g7`: `global using` and `using static` groups.
- `g8`, `h1`, `h2`: blank lines below and between comments.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Formatter/Pipeline/UsingDirectives/Rewriter/UsingDirectiveOrderingRewriter.cs:116-121` (header membership) with `:162-179` (the two line-break insertions) | FC-003, FC-016 | – | same requirement | FMT-USING-010 (change doc entry 5) requires exactly one blank line above the comment of the original first directive when it moves away from the header. Suppose the directive's canonical predecessor keeps a trailing delimited comment that spans lines and has no line break, so `beginsLine` is false. Then the code fix writes one line break and no blank line. Input, RH7207 fix (also RH7201–RH7206, same phase), LF and CRLF:<br>`// Header⏎⏎// About Linq⏎using System.Linq;⏎using System.Collections; /* a⏎   b */ using System;`<br>Fix output: `// Header⏎⏎using System;⏎using System.Collections; /* a⏎   b */ ⏎// About Linq⏎using System.Linq;`. The blank line above `// About Linq` is lost, and RH5020 now reports the comment. The membership insertion adds one line break. The restore branch at `:175` adds the second line break only when the directive's own leading layout holds one. The remainder of a header split starts right after the header's blank line, so its layout has none. Class members: (a) source-scan membership × line in front ends: holds (tests, `f5`, `g3`). (b) source-scan × line in front does not end: holds, because the leading layout then always carries a line break and both branches fire (`f1` writes `y */ ⏎⏎// About Text`). (c) header membership × line in front ends: holds (`f6`, `CodeFixKeepsHeaderBlankLineAboveCommentThatMovesAwayFromTheTop`). (d) header membership × line in front does not end × remainder starts with its comment: **fails** (above). (e) Like (d), but with a whitespace-only line in the remainder: holds (traced). Whole-document formatting holds for (d): `scripts/format.sh` writes the blank line, because the blank-line phase inserts it. Only the code-fix output fails. On `main` the same input made the fix move `// About Linq` behind `b */` on the same line; head no longer does. | Make the number of line breaks written for a member of `directivesWithBlankLineAboveComment` depend on whether the line in front ends: two when `beginsLine` is false, one otherwise. Add a regression test for member (d) in `UsingDirectivePhaseCommentBlankLineTests` and an RH7207 fix test. Owners: implementer, test implementer. |
| 2 | medium | `src/Reihitsu.Core/UsingDirectiveOrderingUtilities.cs:158`, `:180`; `src/Reihitsu.Core.Test/UsingDirectiveOrderingUtilitiesTests.cs` | FC-017, FC-003 | – | new mechanism | The two new public Core predicates `IsLedByComment` and `HasBlankLineAbove` have no Core test. Every other public member of `UsingDirectiveOrderingUtilities` has one. The predecessor side of `HasBlankLineAbove` cannot be observed through any caller's final output. That is the case where the predecessor's trailing trivia has no line break and the leading layout has one, which gives `false`. Its doc comment says the case exists for the formatter's pipeline, after the documentation-comment phase. A count that ignored the predecessor would put the directive in the set, but the blank-line phase writes that blank line anyway (FMT-BLANK-011), so the formatter output is the same. RH7207 never sees the shape on parsed source. So no test can tell the shared measurement from the two it replaced. The boundary of 1 line break against 2 is pinned only through the callers. | Add direct Core tests. `HasBlankLineAbove`: line-ending predecessor with 0 and 1 leading line breaks; predecessor without a line break and 1 or 2 leading line breaks (built with `SyntaxFactory`); a directive sharing its predecessor's line; a directive with no significant leading trivia. `IsLedByComment`: each of the four comment kinds; leading whitespace only; no leading trivia. Owner: test implementer. |
| 3 | low | `src/Reihitsu.Formatter/Pipeline/UsingDirectives/Utilities/UsingLeadingTriviaBuilder.cs:2` | – | – | PR-introduced | The file's only change in the diff is an added `using System.Linq;`, which is redundant. `src/Reihitsu.Formatter/Properties/GlobalUsings.cs` already declares `global using System.Linq;`, and the file used `Skip`/`Take`/`Any` on `main` without it. Commit `f7c142a` added it for `HasBlankLineAboveLeadingComment`. Commit `59340b1` removed that method but left the import. 131 of the formatter's 133 files carry no local `System.Linq` import. | Remove the line, which leaves the file unchanged against `main`. Owner: cleaner. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH7207 accepts a blank line above a same-group directive (`IsOrganized`, `sameGroup && hasBlankLine && IsLedByComment`) | Predecessor's trailing trivia (any line break counts once) plus the leading layout up to the first significant trivia | The using phase's membership scan (`UsingDirectiveOrderingRewriter.cs:89-94`), which uses the same two Core predicates against the source predecessor | On an organized block, canonical order is source order, so both decide the same directives. The phase skips index 0, and so does the analyzer's loop. The phase's extra header member applies only to a block whose order changes, and RH7207 reports such a block anyway | holds |
| `IsLedByComment`, which is the first significant trivia being a comment, against SHARED-USING-011's "any comment in its leading trivia" | First non-whitespace, non-line-break trivia | A directive, disabled text, or a `#` makes `UsingDirectiveOrderingSafety.CanSafelyReorder` false. Both callers return before the predicate in that case | Equivalent on every block either caller reaches | holds |
| `HasBlankLineAbove` against the removed RH7207 `HasBlankLineBefore` (any line break in the leading layout) | See row 1 | Parsed source puts a line's end in the trailing trivia of its last token. That includes a trailing `/* a⏎ b */`, which is followed by its line break, and a directive sharing its predecessor's line, which gives 0 on both sides | Same result on every parsed input. Analyzer tests are unchanged and green | holds; unpinned at its boundary (#2) |
| Header membership (`:118`): "the header ends with a blank line" | Remainder after `GetHeaderSplitIndex`, whose first trivia comes right after an end-of-line pair | `GetHeaderSplitIndex` splits only after two adjacent end-of-line trivia. A whitespace-only blank line does not split, so the comment stays in the header and the predicate is never reached | Agrees by construction | holds |
| FMT-USING-010 insertion (`:165-169`) against the line in front ending (`beginsLine`, `UsingTrailingTriviaBuilder.RequiresSeparatingLineBreak`) | Only `startsNewGroup` and set membership | `RequiresSeparatingLineBreak` treats a trailing delimited comment that spans lines as ending the line (`SpansLine`) and adds no line break. The restore branch at `:175` adds one only when the directive's own leading layout had one | Disagree for header members whose line in front does not end | **fails** (#1) |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| A blank line stands above a using directive or its leading comment | `UsingDirectiveOrderingUtilities.HasBlankLineAbove` only. It is called by `RH7207…Analyzer.IsOrganized` and `UsingDirectiveOrderingRewriter.OrganizeUsingDirectives`. Search: `rg -n "EndOfLineTrivia" src` intersected with `UsingDirective`, which finds `UsingTrailingTriviaBuilder`, `UsingLeadingTriviaBuilder` and Core. The two builders decide other things: line separation, and the header split | one owner |
| A comment leads a using directive | `UsingDirectiveOrderingUtilities.IsLedByComment` only (`rg -n "IsLedByComment\|IsCommentTrivia\(leadingTrivia" src`) | one owner |
| The header's blank line | `UsingLeadingTriviaBuilder.GetHeaderSplitIndex`. The decision is distinct, and the membership derives from it rather than re-measuring | one owner |
| The successor of a rebuilt directive starts its own line, and how many line breaks it gets | `UsingTrailingTriviaBuilder.RequiresSeparatingLineBreak`, the rewriter's restore branch (`:175`), and the new FMT-USING-010 insertion (`:165`) | three places, see *Architecture findings* |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `UsingDirectivePhaseCommentBlankLineTests` (20 tests) | Exact phase output for FMT-USING-010 and FMT-USING-002, and the phase is a no-op on its own output | Any byte difference under LF or CRLF, or a change on the second run | `AssertResult`: exact string, LF and CRLF, second run | adequate. Member (d) of #1 is missing |
| `UsingDirectiveCommentBlankLineTests` (13) and the two de-listed regression tests | Exact formatter output, a stable second pass, and no phase changing the final output alone | Output diff, second-pass diff, or a phase change not on the list | `AssertRuleResult` (includes the phase stability check), LF and CRLF | adequate |
| `PhaseStabilityCheckTests.CheckFindsNoChange…OfTheSameGroup` (2, rewritten) | No phase changes the formatted comment-led block | Any phase alone changing it | `AssertNoPhaseChanges` | adequate. The check's ability to catch a reversal is still shown by `CheckFindsThePhaseThatChangesTheSource` and `CheckCatchesLineBreakPhaseStrippingIndentationOfParenthesizedLambdaArgument` |
| RH7207 analyzer tests (24 new or changed) | No diagnostic on accepted shapes; exact fix and Fix All output; convergence | An unexpected diagnostic, a fixed-code diff, or an extra iteration | `Verify(source[, fixed])` (verifier iteration check, Fix All), `ApplyCodeFixAsync` under CRLF | adequate |
| RH7207 formatter-parity tests (5) | RH7207 does not report formatter output (FC-013) | RH7207 reported on formatter output | `VerifyFormatter`, LF and CRLF | adequate |
| RH7201 and RH7203 sibling fix tests; RH5020 sequence tests | Sibling fixes share the phase; the RH5020 fix output is the RH7207 not-reported input | Fixed-code diff | `Verify(source, fixed)` | adequate. RH7202, RH7204, RH7205 and RH7206 are covered by a static proof that the shared base is sealed, which I re-checked in `UsingDirectiveOrderingCodeFixProviderBase` |
| Core `HasBlankLineAbove`, `IsLedByComment` | — | — | none | missing (#2) |

Visible output is decided (check 10). I compared every added or changed expected value against the change doc's entries 1–6 and *Unchanged*, the maintainer answers 1–7, and unchanged requirements (SHARED-USING-008, RH5020-001/005, FMT-BLANK-011, FMT-BLANK-018). All are covered. The formatter outputs in the full-pipeline tests equal `main`'s output, except entry 6 (answer 7). I confirmed this for `h1` (`using System;⏎⏎// About Linq⏎⏎using System.Linq;`) and `h2` (two comment paragraphs), which give byte-identical output on `main` and head. No rule page changed.

## Failure classes dismissed

- FC-001, FC-002: the qualifiers "same group", "first comment", "directly above" and "in a block it reorders" each map to an explicit condition (Guard scope, rows 1–4).
- FC-004, FC-021: comments move with their directive in every probe. The one relocation seen (`k`) is pre-existing and identical on `main` (see *Hints*).
- FC-005, FC-019, FC-020: any directive or disabled text makes the block unsafe for both callers (`CanSafelyReorder`), and existing tests cover it.
- FC-006, FC-022–FC-029, FC-036: no semantics, literals, identifiers, members, or detached-node formatting are involved.
- FC-007, FC-008: fix convergence and Fix All are covered by the verifier, and `apply-fix` showed 1 iteration on every probe.
- FC-009: no new code action. A block RH7207 now accepts gets no diagnostic, so no fix is offered.
- FC-010: the phase-alone second run in every unit test, and `format.sh` second pass "0 of N" on every probe.
- FC-011: no loop.
- FC-012: the reversal the run removes is gone. Both exception-list entries are deleted and their fixtures pass the stability check, and no new reversal appeared on any probe.
- FC-013: RH7207 reports no formatter output on any probe (`g`, all no-diagnostic, LF and CRLF), and the parity tests cover it.
- FC-014: not a new rule.
- FC-015: one owner per policy (see *Policy ownership*), after lead decision 3.
- FC-016: RH7201–RH7207 share the phase; I enumerated the class of #1.
- FC-018: no rename.
- FC-030: the fix scope is unchanged.
- FC-031: the changed summaries describe the new code (`IsOrganized`, `OrganizeUsingDirectives`, the inline comments, the Core doc comments). The inline comment at `:162-164` claims "exactly one is written", which #1 contradicts for member (d). It is fixed with #1.
- FC-032: the cleaner's build and test results are on the final production commit `59340b1`; later commits are handovers only.
- FC-033, FC-034: the issue's own inputs are tested verbatim (change doc entry 1), under LF and CRLF, through formatter, analyzer, and fix.
- FC-035: per `04-test-implementer.md`, expected values were written on `edcf520` before any production change. The failure counts there match.
- FC-037: no comment-only claim.

## Architecture findings

Within one component, `UsingDirectiveOrderingRewriter.OrganizeUsingDirectives` together with `UsingTrailingTriviaBuilder`, three places decide how many line breaks stand between two rebuilt neighbors:

- `UsingTrailingTriviaBuilder.RequiresSeparatingLineBreak`. It treats a trailing delimited comment that spans lines as having ended the line (`SpansLine`), and adds nothing.
- The restore branch (`UsingDirectiveOrderingRewriter.cs:175-179`). It adds a line break only when the successor's own leading layout carried one.
- The new FMT-USING-010 insertion (`:165-169`). It adds one line break and assumes the line in front ends.

The result depends on which of them fire. Two inputs show this:

- Finding #1: header member, a line-spanning trailing comment in front, so 1 line break where 2 are needed.
- The pre-existing `k` input in *Hints*. A comment-led directive with no blank line gets no line break, so its comment moves behind `b */`.

The decision the three share is "how far below the line in front does this directive's first trivia start". The run added the third place. The defect that reaches the run is #1; the lead decides whether to fix it narrowly or to give that decision one owner.

## Hints

- Pre-existing, identical on `main` and head; a follow-up candidate, with the structural cause above. The RH7201–RH7207 code fixes move an ordinary comment off its own line and behind a line-spanning delimited comment, which relocates user-authored trivia (SHARED-FIX-003, FC-004). Input `k`, reported by RH7207:<br>`using System.Collections; /* a⏎   b */ using System;⏎// About Linq⏎using System.Linq;`<br>Fix output: `using System;⏎using System.Collections; /* a⏎   b */ // About Linq⏎using System.Linq;`. `scripts/format.sh` gives a correct layout, because later phases restore it. Cause: `RequiresSeparatingLineBreak` returns false for a trailing comment that spans lines, and the restore branch requires a line break in the successor's own leading layout.
- Pre-existing: in the same shapes (`f1`, `f2`), the fix output keeps the whitespace in front of the reordered neighbor on the comment's last line (`b */ ⏎`). This is trailing whitespace that only the formatter's cleanup phase removes. It is the same on `main`.
- The `OrganizeUsingDirectives` summary wraps mid-sentence ("A block that cannot be⏎/// reordered safely"), which is cosmetic.
