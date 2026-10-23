Role: auditor
Commit: 34ca7c3d61bed2053e73dd4b8c7d6b4fadc70733
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict. It stated that the Release build is green on the head, which I took as the cleaner's result (05) and did not re-run.

## Scope

- Head `34ca7c3d61bed2053e73dd4b8c7d6b4fadc70733` is checked out. The working tree is clean.
- Base `origin/main` is `f52df4598d6e5634ca0a00d5a0444137d253ca72`, re-fetched, and it is an ancestor of the head.
- This is a full audit of `f52df45..34ca7c3`.
- `scripts/lane-check.sh tooling`: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.
- The diff is what 01, 03 and 04 describe:
  - the spec changes in `analyzer/RH5203.md`, `formatter/{blank-lines,pipeline,structural-transforms,switch-case-braces}.md`, `shared/{code-fixes,comment-placement}.md` and `requirement-ids.txt`;
  - the 13 test files listed in 04;
  - the three `squad/decisions/*-1380-*` entries;
  - the handovers 00 to 06.
- No production, tooling or project file changed.

How I verified:
- **Static tracing** of the specs against `LineEndingUtilities`, `ReihitsuFormatter.FormatNodeInDocumentAsync`, `LineEndingNormalizationPhase`, `StatementBracesCodeFixProviderBase`, the RH5011 fix, `DocumentationCommentCodeFixUtilities`, and the comment-led-line owners of the rules.
- **Executed checks** for the line-ending findings:
  - `scripts/apply-fix.sh` normalizes every fixture to LF and CRLF, so it cannot build an all-`\r` document. I used a scratch file-based harness outside the repository instead. It references `src/Reihitsu.Tooling`, runs the analyzer, applies the first offered fix, and prints every line break.
  - Fixtures under the session scratchpad: RH5405, RH5406, RH5011, RH8304 and RH5417 in all-`\r` documents, with LF and CRLF controls, plus RH5304 for SHARED-COMMENT-011.
- Notation: `␍` is a lone `\r` and `⏎` is `\n`.

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | high | `sdd/specs/shared/code-fixes.md`, SHARED-FIX-008: the sentence "a line break the fix keeps MUST stay a lone `\r`", the Why sentence "The lone `\r` line breaks a fix keeps there are the document's own…", and the example's last clause | FC-016, FC-027, FC-034 | PR-introduced | The clause is false for fixes that lay out a node through the formatter: the RH5406 fix turns a kept `\r` inside the body into `\n` (details below) | Remove the clause, or narrow it to what the code does (details below) |
| 2 | medium | `sdd/specs/shared/code-fixes.md` SHARED-FIX-008, against `sdd/specs/analyzer/RH5011.md` RH5011-004 and `sdd/specs/analyzer/RH8304.md` RH8304-004 | FC-015, FC-016 | PR-introduced (the contradiction); same requirement (P35) | In an all-`\r` document, the RH5011 and RH8304 fixes create `\r` as their rule requirements say, against the decided P35 text; nothing records or tracks it (details below) | Keep the decided text; record the contradiction and file a P35 follow-up candidate (details below) |

### Finding 1: the "kept stays a lone `\r`" clause is false for formatter-delegating fixes

**What goes wrong.**
- The clause is not part of the decided P35 text, which says nothing about kept line breaks. It generalizes from the one-line RH5405 shape of #1363 Q6.
- It is false for every fix that lays out a node through `FormatNodeInDocumentAsync` (SHARED-FIX-007).
- **Mechanism.** `LineEndingNormalizationPhase` rewrites every end-of-line trivia inside the formatted target to the detected `\n`. Only the target's boundary trivia is restored from the input: the last token's trailing trivia, and the first token's leading trivia when the target does not start its line.
- **Counterexample (run).** The RH5406 fix on an all-`\r` document:
  - Input: `class C␍{␍    void M(bool a, bool b)␍    {␍        if (a) M(a,␍                 b);␍    }␍}␍`
  - Output: `if (a)⏎{⏎M(a,⏎b);⏎}␍`
  - The line break between `M(a,` and `b);` was in the input. It is not one of the breaks the fix creates, yet it becomes `\n`.
  - Under the spec's own first sentence, "creates or keeps" is exhaustive, so this break is a kept break, and the requirement says it MUST stay `␍`.
- **Second instance.** The RH5405 fix on `if (a)␍            M(a, b);␍` writes `\n` after `if (a)` as well.
- The LF control gives an all-`\n` output.
- Nothing tracks this: it is neither a decided principle with a follow-up nor today's behavior.

**Suggested change.** The spec author's call:
- **Remove** the kept-break MUST, its Why sentence and the example clause. Then SHARED-FIX-008 states for all-`\r` documents only what P35 decided: created line breaks are `\n`. Per `sdd/README.md`, "Open questions", leave the kept breaks out and file an open question if the mixed result looks wrong.
- **Or narrow** the clause to what the code does: line breaks outside the code the fix lays out keep their `\r`, and those inside it get the detected `\n`. A narrowed clause needs a covering test of the inside case, such as RH5406.

The RH5405 lone-`\r` test may keep its exact output either way. Its summary sentence about the kept break should match whatever the spec ends up saying.

### Finding 2: SHARED-FIX-008 now contradicts RH5011-004 and RH8304-004

**What goes wrong.**
- The amended SHARED-FIX-008 is the decided P35 text: a fix writes the line ending FMT-PIPE-007 detects, so every line break it creates in an all-`\r` document MUST be `\n`.
- RH5011-004 requires the inserted line to end "with the same line break as that line". RH8304-004 requires inserting "the line break of the line the element starts on". In an all-`\r` document, both mean `\r`.
- **Today's behavior (run):**
  - RH5011: `a++;␍break;␍a++;` becomes `break;␍␍a++;`.
  - RH8304: `/// <summary>Runs</summary><remarks>…` becomes `…</summary>␍    /// <remarks>…`.
  - Both create `\r`. The LF and CRLF controls stay uniform.
- Before this PR, SHARED-FIX-008 ("the line ending the document already uses") agreed with both rules. The PR therefore introduces a contradiction between active requirements.
- It also falsifies the issue's claim, "P35: today's behavior confirmed, no code change", for these two fixes.
- No handover, decisions entry or follow-up records it. 01's list of "Contradictions left between active requirements" omits it.
- The code-level cause is two private owners of "which line break a fix writes":
  - `RH5011BreakStatementsShouldBeFollowedByABlankLineCodeFixProvider.ApplyCodeFixAsync` copies `breakLine.End..EndIncludingLineBreak`;
  - `DocumentationCommentCodeFixUtilities.GetLineBreak` copies the affected line's break.
- RH5417, which the issue names, conforms: `void M()⏎{⏎}␍`, verified.

**Suggested change.**
- The decided text stays (00-lead.md, clarification 3).
- Record the contradiction with the others this run leaves in place.
- File the RH5011 and RH8304 instances as a P35 follow-up candidate (clarification 9: "Decided by principle P35 (#912)"). Name both rule requirements, since that follow-up changes them.
- State in the PR that P35 is not today's behavior for these fixes.
- Whether the "no code change" framing of P35 needs the maintainer is the lead's call.

## Guard scope

No code predicate changed. The rows compare each changed requirement's predicate with the code that decides the same thing.

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| SHARED-FIX-008: the line ending of created vs kept line breaks in an all-`\r` document | Every line break in the fix's result | Formatter-delegating fixes: `FormatNodeInDocumentAsync`, i.e. `LineEndingNormalizationPhase` over the whole target, restoring only boundary trivia. Text-edit fixes: `DetectEndOfLine`, or a copy of the line's own break (RH5011, RH8304). | Disagree. The code's boundary is "inside the laid-out target vs. outside it". The spec's boundary is "created vs. kept". Text-edit fixes that copy a break write `\r`. | Findings 1 and 2 |
| FMT-PIPE-007: a one-character line break counts as `\n` | End-of-line trivia (including structured trivia) and XML newline tokens | `LineEndingUtilities.DetectEndOfLine`: `Span.Length >= 2` counts as CRLF, everything else as LF; a tie goes to CRLF | Agree, including the U+0085, U+2028 and U+2029 terminators (length 1) | ok |
| SHARED-COMMENT-011: a misplaced line-leading comment makes the line reported | The comment that starts the line, and the first token behind it | Per rule: `RH5204…Analyzer.AlignCommentIndentation`, `RH5201…Analyzer.GetLineStartColumn` and `InitializerLayoutAnalysisUtilities` (RH5301–RH5305) judge the comment. RH5110, RH5203, RH5205, RH5206, RH5302, RH5305, RH5306 and RH5308 measure the token behind it (02's table). | Partial; the gap is tracked by #1147. I re-verified RH5304, which 02 did not run: it conforms. | ok in this lane |
| SHARED-COMMENT-012, rule bullet | A token behind the closing `*/` whose comment starts its line | RH5203 reports the example only through the column check, so `2` in the alignment column behind the tail is not reported (03) | Partial; the gap belongs to #1382 | ok in this lane (see Hints) |
| FMT-BLANK-016: where the inserted blank line goes | The gap from the last directive to the code, and the comments in it | BlankLinePhase: the FMT-BLANK-005 path inserts after the last directive and in front of the comments (tests); the FMT-BLANK-001, -002 and -004 paths insert above the first directive (02) | Partial; the gap is tracked by #1385. No test tagged FMT-BLANK-001 to -005 inserts a blank line in front of a non-region directive (scanned). | ok in this lane |
| FMT-STRUCT-024: refuse an unsafe rewrite | The result of each structural transform | `ExpressionBodyRewriteUtilities.BlocksRewrite` (FMT-STRUCT-007) and the FMT-STRUCT-012 comment guards agree; `FieldDeclarationSplitTransform` splits documented declarations | Partial; tracked by #1384, #963, #1390 and #1395. None of the documented-split tests is tagged; FMT-STRUCT-021 and -022 tags were checked. | ok in this lane |
| FMT-PIPE-013: the comment layout of a wrap | Comments of the wrapped statement, label and expression | `ControlFlowBraceTransform`, the `ExpressionBodied*Transform` classes, `SwitchCaseBracePhase` | Partial; tracked by #1381 and #963. A scan of every test tagged FMT-STRUCT-002, -006 and FMT-SWITCH-007 (18 tests) found no glued comments, split same-line comments, or dropped blank line between comments. | ok in this lane |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| The line ending a code fix writes (SHARED-FIX-008, P35) | See below the table | Finding 2 |
| Predominant line ending (FMT-PIPE-007) | `LineEndingUtilities.DetectEndOfLine` is the only owner. `RH5604…Analyzer.OnSyntaxTree` counts distinct texts only to decide whether endings are mixed, then defers to `DetectEndOfLine`. | ok |
| Judgment of a comment-led line (SHARED-COMMENT-011) | Per-rule implementations (Guard scope, row 3). This is pre-existing and the PR changed no code. | Pre-existing, tracked by #1147; not caused by this PR |

Owners of the line ending a code fix writes:
- **Shared owner:** `Reihitsu.Core/LineEndingUtilities.DetectEndOfLine`, reached through `ReihitsuFormatterHelpers.DetectEndOfLine`.
- **Users of the shared owner:**
  - `FormatNodeInDocumentAsync`
  - `BlankLineCodeFixUtilities`
  - `StatementShouldBePrecededByABlankLineCodeFixProviderBase`
  - `UsingDirectiveCodeFixUtilities`
  - the `TargetAttribute*` and `RegionDirectiveBlankLine` bases
  - RH5020, RH5103, RH5104, RH5109, RH5114, RH5402, RH5604, RH7101, RH8201 and RH8303
- **Private copies:**
  - `RH5011…CodeFixProvider.ApplyCodeFixAsync`, which copies the `break` line's line break
  - `DocumentationCommentCodeFixUtilities.GetLineBreak`, used by RH8304
- **Separate decision:** `RH5202…CodeFixProvider.RealignContent` picks the line ending of raw-string content. That is literal content under SHARED-FIX-003, so it is justified.

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `PipelineRequirementTests.WrappingInABlockKeepsOwnLineAndTrailingCommentsOnTheirLines`, `BracedElseBody…`, `BracedSwitchSection…` | FMT-PIPE-013: own-line and trailing comments keep their lines in the new block | A comment moved, joined or glued | `AssertRuleResult` (exact output, LF and CRLF, second pass) | adequate |
| `…ConvertedExpressionBodyKeepsTheSpaceBetweenTwoTrailingComments` | FMT-PIPE-013: the space between two same-line comments | `/* a *//* b */` | same | adequate |
| `…EveryConvertedExpressionBodyKindKeepsItsTrailingComment` | FMT-PIPE-013 for the constructor, finalizer, operator, conversion and local function kinds (FC-025) | Any kind loses the comment's place in front of `;` | same | adequate; the method kind is covered by the first test |
| `…ParameterListsAndAssignmentsAreLaidOutAlike` | FMT-PIPE-014: the family treatment for parameter lists and `=` | One member left unformatted | same | adequate |
| `…FunctionPointerParameterListKeepsItsOwnJoinedLayout`, `…OneLineArrayInitializerAndCollectionExpressionKeepTheirOwnLayout`, `…PropertyArrowKeepsItsOwnJoinedLayout` | FMT-PIPE-014: the family clause does not override per-construct choices | A family treatment applied over FMT-BREAK-014, -021, -022 or -026 | same; the identity test has no expected value | adequate |
| `…StatementBehindTheEndOfAMultiLineCommentIsPutOnItsOwnLine` | SHARED-COMMENT-012, formatter bullet | `Foo();` stays behind `q */` | same | adequate |
| `StructuralTransformRequirementTests.ExpressionBodyWhoseConversionWouldNotCompileIsLeftUnchanged`, plus the six FMT-STRUCT-024 tags in `ExpressionBodyDirectiveGuardTests` | FMT-STRUCT-024, "would not compile" | The conversion is applied | same | adequate |
| `…AccessorConversionIsAppliedOnlyWhereEachCommentStaysNextToItsCode` | FMT-STRUCT-024, both sides | `get` converted, or `set` kept | same | adequate |
| `BlankLineRequirementTests.BlankLineAfterClosingBraceGoesBetweenDirectiveAndComment`, `…BehindLastDirectiveOfConditionalGroup`, and the two newly tagged existing tests | FMT-BLANK-016 and the clarified FMT-BLANK-005 | A blank line in front of a directive, or between the comment and the statement | same; disabled text included (FC-020) | adequate |
| `ReihitsuFormatterHelpersTests.DetectEndOfLine…` (three tests) | FMT-PIPE-007: one-character breaks count as `\n`, and a tie goes to CRLF | The 2:1 test returns CRLF if a lone `\r` is not counted or is counted as CRLF; the 1:1 test returns LF if the tie rule flips | `DetectEndOfLine` | adequate as a pair (see Hints) |
| `ReihitsuFormatterTests.FormatSyntaxTreeWritesLineFeedWhenEveryLineBreakIsALoneCarriageReturn` | FMT-PIPE-007 and FMT-EOL on an all-`\r` file | Any `\r` survives | Public `FormatSyntaxTree`; the string is built in the test (FC-033) | adequate |
| `RH5405…Tests.FixWritesLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns` | SHARED-FIX-008 on an all-`\r` document | A created break is not `\n`, or another break changes | `ApplyCodeFixAsync` (no normalization), exact string | adequate for its one-line shape. The requirement it covers claims more than this shape (finding 1) and holds for fewer fixes (finding 2). |
| RH5204 `VerifyMemberBehindMisplacedBlockComment…` and `VerifyMemberInScopeColumn…` (LF and CRLF); RH5201, RH5301, RH5303 `…BehindMisplacedBlockCommentIsDetected` | Amended SHARED-COMMENT-011 | No report while the token stands in its column | Analyzer `Verify` with markup | adequate. The token-in-column shapes isolate the comment's column. |
| RH5112 `VerifyNoDiagnosticForFirstLinkBehindBlockCommentStartingTheLine` (LF and CRLF) | The SHARED-COMMENT-011 exemption sentence and SHARED-CHAIN-004 | Any RH5112 report | Analyzer `Verify` | adequate |
| RH5203 `VerifyArgumentBehindEndOfMultiLineCommentIsDetected` (LF and CRLF) | SHARED-COMMENT-012 rule bullet and RH5203-001 | No report on `2` | Analyzer `Verify`; analyzer only, because the fix makes no progress (FC-007) | adequate for the spec example. It cannot tell a report "by P33" from a report "by column", but the only discriminating shape is non-conforming (#1382). |

## Failure classes dismissed

- **FC-001, FC-002, FC-003**: no code guard changed. The spec predicates are compared under Guard scope. FC-003 for FMT-PIPE-007 is covered by the 2:1 and 1:1 tests.
- **FC-004, FC-005, FC-019, FC-020, FC-021**: no code edits a gap. The new tests fill the gaps they claim: `#pragma`, `#if NEVER` with disabled text, and own-line, trailing and multi-line comments.
- **FC-006, FC-022, FC-023, FC-024, FC-026, FC-029, FC-036**: no rewrite or rename changed.
- **FC-007, FC-008, FC-009, FC-018, FC-030**: no fix changed. The non-progress of the RH5203 fix on the SHARED-COMMENT-012 example is pre-existing and commented on #1382.
- **FC-010, FC-011, FC-012**: no phase changed. The formatter tests assert a second pass.
- **FC-013, FC-014**: the parity of P37's shared side is deferred by decision `2026-10-22-1380-p37-shared-side-deferred`. Nothing in this PR changes analyzer or formatter behavior.
- **FC-025**: covered by `EveryConvertedExpressionBodyKindKeepsItsTrailingComment`.
- **FC-028**: FormatNode scope for FMT-BLANK-016 is non-conforming and tracked by #1385; FormatNode line-ending detection is tracked by #1388. Neither is introduced here.
- **FC-031**: no production method changed. The summaries of the new tests describe their tests.
- **FC-032**: no compiled change.
- **FC-033**: the all-`\r` tests build their inputs without a normalizing harness.
- **FC-035**: expected values come from spec examples; no production code was patched.
- **FC-037**: no text-only claim is made.

Applied, not dismissed: FC-015 and FC-016 (finding 2), FC-016, FC-027 and FC-034 (finding 1), and FC-017 (Assertion adequacy).

## Hints

- **Further #1382 instance not yet commented.** 03 found it: `P(1,⏎          /* a⏎     b */ 2);` is not reported, although amended RH5203-001 and SHARED-COMMENT-012 require a report. It is not on thoenissen/Reihitsu#1382 yet, whose two comments cover the non-progressing fix and `{⏎/* p⏎q */ Foo();⏎}`.
- **RH5203-001 amendment not in the lead's record.** It lies outside the placements in 00-lead.md, clarification 1, and 00-lead.md records acceptance only for the FMT-PIPE-007 sentence. 03 gives the reason (it wires SHARED-COMMENT-012 into an enforced spec). I found no defect in the edit; the lead may want to record the acceptance.
- **RH5304 conforms but is untested.** It conforms to the amended SHARED-COMMENT-011 (verified): `/* c */` in column 0 with the element in column 28 is reported, and the same line without the comment is not. No tagged test covers it.
- **RH5417 example untested.** The issue's P35 example for RH5417 conforms (verified), and no test covers it.
- **No CRLF arms on three new tests.** The RH5201, RH5301 and RH5303 SHARED-COMMENT-011 tests run under LF only. The judgment is kind- and column-based, so the risk is low.
- **The 1:1 tie test is weak on its own.** `DetectEndOfLineReturnsCarriageReturnLineFeedWhenALoneCarriageReturnTiesWithIt` alone cannot tell "a lone `\r` counts as `\n`" from "a lone `\r` is not counted"; the 2:1 test is the one that discriminates.
- **#1081 item 2.** It asks for "an example". FMT-PIPE-007 gained a prose line after its example, not a code example. 00-lead.md already plans to name #1081 as partly addressed.
- **RH5604 and the formatter differ on all-`\r` files (pre-existing, unrelated).** RH5604 does not report an all-`\r` file, because it has one distinct line ending, while the formatter rewrites every break to `\n`. That is the formatter doing more than the analyzers (P11); it is not introduced here.
