Role: auditor
Commit: 9c65d5f7c8ef99f7b6f97553214039ba78a493f4
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict. It said the Release build is green on the head. I took that as the cleaner's result (10) and did not re-run it.

`␍` is a lone `\r` and `⏎` is `\n`.

## Scope

- The checkout is head `9c65d5f7c8ef99f7b6f97553214039ba78a493f4`. The working tree is clean.
- The base `origin/main` is `f52df4598d6e5634ca0a00d5a0444137d253ca72`, and it is an ancestor of the head.
- This is a repair delta against `34ca7c3`: `git diff 34ca7c3 HEAD -- . ':(exclude)sdd/work'`.
- The repair diff is what 08 and 09 describe, and nothing more:
  - the specs `formatter/pipeline.md` (the FMT-PIPE-007 example) and `shared/code-fixes.md` (SHARED-FIX-008 only);
  - eight test files: RH5201, RH5301, RH5303, RH5304, RH5405, RH5406, RH5417 and `ReihitsuFormatterTests`.
- No shipped source, tooling, project or ID file changed in the repair or in the whole PR (`git diff --name-only`).
- `scripts/lane-check.sh tooling`: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.
- Scope did not grow. The repair removes an over-claim, adds one spec example and adds tests of conforming shapes. So this is a delta audit, not a full one.
- Evidence from 07 that the repair did not touch is reused unchanged: the policy owners and the line-ending runs of RH5011, RH8304 and RH5417.
- The open questions are Q-1380-1 and Q-1380-2, in a comment on thoenissen/Reihitsu#1408. I read them there.

### Previous findings (07)

| # | Status | Evidence |
|---|---|---|
| 1 (SHARED-FIX-008: the "kept breaks stay a lone `\r`" clause) | closed | All three parts are gone from SHARED-FIX-008: the MUST, its Why sentence and the example's closing clause. What remains in an all-`␍` document is the decided P35 text (#1380, P35): created breaks are `\n`, and a lone `\r` counts as `\n`. No spec or source file still claims that kept lone `\r` breaks survive (`rg`). The RH5405 test no longer pins `}␍`, and its summary and comment now claim only the created breaks. The kept-break behavior is filed as Q-1380-2. |
| 2 (SHARED-FIX-008 against RH5011-004 and RH8304-004) | closed | 08 lists the contradiction among those the run leaves in place, and RH5011-004 and RH8304-004 are unchanged. It is filed as Q-1380-1, marked "Decided by principle P35 (#912)", naming both rule requirements and both private owners. No tagged test pins either side. The PR-text part of my suggestion is still to come (see Hints). |

### Hints from 07

| Hint | Status |
|---|---|
| The further #1382 instance (the alignment column) was not commented | closed: the comment is on thoenissen/Reihitsu#1382 |
| The RH5203-001 amendment was missing from the lead's record | closed: 00-lead.md records its acceptance |
| RH5304 untested | closed: `VerifyElementInItsColumnBehindMisplacedBlockCommentIsDetected`, LF and CRLF |
| The RH5417 example untested | closed: `FixWritesLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns` |
| No CRLF arms on the RH5201, RH5301 and RH5303 tests | closed |
| The 1:1 tie test is weak on its own | unchanged. The pair is adequate, and the new `FormatSyntaxTree` pair adds a second pair. |
| #1081 item 2 asks for an example | closed: FMT-PIPE-007 now has a code example |
| RH5604 and the formatter differ on all-`\r` files | unchanged; pre-existing and unrelated |

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| _None._ | | | | | | |

## Guard scope

The repair changed no code predicate. The rows compare each requirement the repair changed with the code that decides the same thing.

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| SHARED-FIX-008, narrowed: in an all-`␍` document, the line breaks a fix creates are `\n` | Only the line breaks a fix creates. Kept breaks in such a document are no longer specified (Q-1380-2). | Fixes that lay out through the formatter: `FormatNodeInDocumentAsync`, i.e. `LineEndingNormalizationPhase` over the target. Text-edit fixes: `DetectEndOfLine` or a private copy (07). | Agree, with one exception: RH5011 and RH8304 create `␍`. That contradiction is recorded and filed (Q-1380-1). Neither the spec nor any test now claims anything about kept breaks, so the code's "inside vs. outside the laid-out target" boundary no longer conflicts with the spec. | ok |
| FMT-PIPE-007, example 2: two lone `\r` and one `\r\n`; and the tie sentence | Every end-of-line trivia of the input | `LineEndingUtilities.DetectEndOfLine`: `Span.Length >= 2` counts as CRLF and everything else as LF; a tie goes to CRLF | Agree. 2:1 gives LF and 2:2 gives CRLF; both are traced and pinned by the new tests. | ok |
| SHARED-COMMENT-011 for RH5304: a line led by a comment in the wrong column | The comment that starts the element's line | `InitializerLayoutAnalysisUtilities` (via RH5304) | Agree. I computed the columns of the new fixture: `{` at 33, elements at 37, `}` at 33. Without the comment, the shape conforms to RH5304-002. With `/* c */` in column 0 it is reported, and with the comment in column 37 it is not (`VerifyNoDiagnosticForCommentPrefixedOrdinaryElements`). | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which line break a code fix writes (SHARED-FIX-008) | Byte-identical to 07, since no source changed. The shared owner is `LineEndingUtilities.DetectEndOfLine`. Private copies: `RH5011…CodeFixProvider.ApplyCodeFixAsync` and `DocumentationCommentCodeFixUtilities.GetLineBreak`. `RH5202…RealignContent` is a separate decision about literal content. I re-ran `rg` over `src/Reihitsu.Analyzer.CodeFixes` for `EndOfLine(`, `CarriageReturnLineFeed`, `ElasticLineFeed`, `LineFeed` and `Environment.NewLine`, and found no new owner. | ok. The two private copies are recorded and filed (Q-1380-1). |
| Predominant line ending (FMT-PIPE-007) | `LineEndingUtilities.DetectEndOfLine` only, unchanged | ok |
| Judgment of a comment-led line (SHARED-COMMENT-011) | Per-rule implementations, unchanged; the gap is tracked by #1147 | Pre-existing; not caused by this PR |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH5405…Tests.FixWritesLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns` (rewritten) | Narrowed SHARED-FIX-008: the three breaks the fix creates (behind `if (a)`, behind `{`, behind the body) are `\n`, and the layout is the example's | A created break is `␍` or `\r\n` (the `Contains` substring requires `\n` directly behind each); or the layout differs | `ApplyCodeFixAsync` (no normalization: `SourceText.From`), then `NormalizeEveryLineBreakToLineFeed` for the layout and `Assert.Contains` for the created breaks | adequate. It asserts exactly what the narrowed requirement states and nothing about kept breaks. |
| `RH5406…Tests.FixWritesLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns` (new) | The same, for a wrapped body that spans two lines (FC-027, FC-034: the counterexample shape from 07) | A created break behind `if (a)`, `{` or `b);` is not `\n`; or the layout differs | same; the break inside the body is not asserted | adequate |
| `RH5417…Tests.FixWritesLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns` (new) | The same, for the issue's P35 example (`void M() { }`) | A created break behind `void M()` or `{` is not `\n` | same | adequate |
| RH5201, RH5301, RH5303 `…BehindMisplacedBlockCommentIsDetectedWithCrLf` (new) | Amended SHARED-COMMENT-011 under CRLF | No report while the token stands in its column | Analyzer `Verify` with markup, CRLF via `NormalizeToCarriageReturnLineFeed` | adequate. They mirror the LF tests from 07. |
| RH5304 `VerifyElementInItsColumnBehindMisplacedBlockCommentIsDetected`, plus `…WithCrLf` (new) | SHARED-COMMENT-011, RH5304-002, and RH5304-003 (one diagnostic on the whole assignment) | No report, or a diagnostic anywhere other than the whole assignment | Analyzer `Verify` with the markup over the assignment | adequate. The shape conforms without the comment (Guard scope), so only the comment's column triggers the report. The other side is covered by `VerifyNoDiagnosticForCommentPrefixedOrdinaryElements`. |
| `ReihitsuFormatterTests.FormatSyntaxTreeWritesLineFeedWhenLoneCarriageReturnsOutnumberCarriageReturnLineFeed` (new) | FMT-PIPE-007 example 2: a lone `\r` counts as `\n`, and 2:1 gives `\n` | If a lone `\r` were not counted, the count would be 0:1 and give `\r\n`. If it were counted as CRLF, it would be 0:3. Either way the output fails the exact assertion. | Public `FormatSyntaxTree`; the input string is built in the test (FC-033); exact output | adequate |
| `…FormatSyntaxTreeWritesCarriageReturnLineFeedWhenLoneCarriageReturnsTieWithCarriageReturnLineFeed` (new) | The example's tie sentence: 2:2 gives `\r\n` | The tie rule flips to `\n` | same | adequate as a pair with the test above. On its own it cannot discriminate counting, as with the `DetectEndOfLine` pair. Idempotency is not claimed; 08 ran a second pass and found it stable. |

## Failure classes dismissed

- **FC-001, FC-002**: no code guard changed. The narrowed spec predicate is compared under Guard scope.
- **FC-003**: covered on both sides: FMT-PIPE-007 by 2:1 and 2:2, and SHARED-COMMENT-011 for RH5304 by the comment in the wrong column and in the right one.
- **FC-004, FC-005, FC-019, FC-020, FC-021**: the repair edits no gap and changes no code that does.
- **FC-006, FC-022, FC-023, FC-024, FC-025, FC-026, FC-029, FC-036**: no rewrite, literal handling or rename changed.
- **FC-007, FC-008, FC-009, FC-018, FC-030**: no fix changed. The new fix tests apply the first offered action on unchanged code.
- **FC-010, FC-011, FC-012**: no phase changed.
- **FC-013, FC-014**: the repair changes no analyzer or formatter behavior.
- **FC-028**: the new formatter tests use `FormatSyntaxTree` only. The `FormatNode` line-ending gap is tracked by #1388 (07), and the repair neither reaches nor introduces it.
- **FC-031**: no production method changed. The summary and inline comment of the rewritten RH5405 test now describe its assertion. The summaries of the new tests describe their tests.
- **FC-032**: no compiled change.
- **FC-035**: expected values come from the spec examples and run on unchanged code. No production code was patched.
- **FC-037**: no text-only claim is made.

Applied, not dismissed:
- FC-015 and FC-016 (Policy ownership)
- FC-017 (Assertion adequacy)
- FC-027 and FC-034 (the RH5406 shape)
- FC-033 (the inputs are built in the tests; see also Hints)

## Hints

- **SHARED-FIX-008 no longer says which line breaks of its example the fix creates.**
  - The removed clause named the break behind the new closing brace as the kept one. The example now says only "the three the fix creates".
  - The natural text-diff reading picks the same three breaks the tests assert: behind `if (a)`, behind `{`, and behind the body. So do the decided source (#1363 Q6, `if (b)⏎ {⏎ M(b);⏎ }␍`) and Q-1380-2.
  - A token-anchored reading would instead treat the break behind the body as kept and the one behind `}` (which is `␍`) as created. Under that reading, the example would violate its own sentence.
  - I do not count this as a defect. Naming the three created breaks in the example's closing sentence would remove the ambiguity, if the spec author or the maintainer answering Q-1380-2 wants it.
- **FC-033 residual.**
  - The three all-`␍` fix tests no longer observe a surviving `␍`, so they would stay green if `ApplyCodeFixAsync` ever normalized line endings.
  - Today it builds the document with `SourceText.From(source)` and returns the raw text, so nothing is normalized away.
  - Asserting a kept `␍` would pin the open Q-1380-2, so I would not add one.
- **Two styles of the same test normalization.** `NormalizeEveryLineBreakToLineFeed` is a private helper in the RH5405 test class. The RH5406 and RH5417 tests inline the same `.Replace("\r\n", "\n").Replace("\r", "\n")`. The base `AnalyzerTestsBase<,>` already owns `NormalizeToCarriageReturnLineFeed`. This affects test code only and does not change any assertion.
- **The question numbers in 09 are swapped relative to #1408** (11 noted this). On #1408, Q-1380-1 is the RH5011/RH8304 P35 instance and Q-1380-2 is the kept-break question. Only `sdd/work` prose is affected. The PR text should use the numbers as filed.
- **Still to do from finding 2 (the lead, at publish):** the PR description should say that P35 is not today's behavior for the RH5011 and RH8304 fixes in an all-`\r` document (Q-1380-1). It should also name thoenissen/Reihitsu#1081 as partly addressed, as 00-lead.md plans.
