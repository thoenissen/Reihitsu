Role: auditor
Commit: 498009c1169899ea8721d18776d8dae16ec03dae
Verdict: findings (#1568: pass; #1698: pass; #1567: findings)

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head 498009c1169899ea8721d18776d8dae16ec03dae (checked out, clean tree); base `origin/main` fe7b8f85d45a85e3354da783ca73a20b3e39eb6d, equal to `git ls-remote origin refs/heads/main` and an ancestor of the head. The diff matches the handovers: four production files (`LambdaAlignmentContributor`, `LayoutModel`, `CommentIndentationContributor`, `LineBreakListRewriter`), three change docs, `indentation.md`, `line-breaks.md`, `requirement-ids.txt`, and the tests 05 and 08 list. `scripts/lane-check.sh behavior-change`: confirmed. Maintainer answers 1B, 2A, 3A (`00-lead.md`).

Evidence beyond reading: fixtures in the session scratchpad (`f1`–`f4`, outside the repository), each run through `scripts/format.sh` twice (all stable on the second pass), and `scripts/apply-fix.sh` for RH5308, RH5206, RH5203, RH5201, RH5204, RH5205, RH5302, RH5102, RH5101, RH5111, RH5112, RH5417 over the formatted anonymous-method fixtures (LF and CRLF: 4 no-diagnostic each, nothing else).

## thoenissen/Reihitsu#1568

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _none_ | | | | | | | |

Checked against FMT-INDENT-031 for every position the skeptic and the spec name, plus more: field value, constructor initializer `: base(delegate …)`, `E += delegate (object s, EventArgs e)`, `E += delegate`, inside a lambda's expression body, query `select (Func<int>)delegate`, cast and parenthesized cast, `async delegate` as argument, block holding `#region`/`#endregion` and a `switch` statement, collection initializer elements, expression-bodied local function, regular-string interpolation (C# 14), named argument, both operands of `+`, wrapped first argument, object initializer, conditional branches, switch-expression arm (also as the arm's whole value), collection expression, anonymous-object member, a chain of two anonymous methods, an operand of `&&`. Every block lands under `delegate` (`async`), matching the block lambda in the same position; stable on the second pass. Tab cells: a tab inside a string literal and inside a `/* */` comment in front of the anchor on the same line — the anonymous method lands in the same column as its block-lambda twin (tab counted as one column, FMT-TAB-003), stable.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whether an anonymous method's block is shifted (`LambdaAlignmentContributor.Contribute`) | the anonymous method's `Block.OpenBraceToken` starting its line (shared with the lambda cases) | the block-lambda cases of the same switch; `RH5204.ComputeBlockIndentation` skips `BlockSyntax { Parent: AnonymousFunctionExpressionSyntax }` (anonymous methods included) | the hole-only condition was removed, not narrowed; the remaining condition is the lambdas' | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Column of an anonymous method's block | `LambdaAlignmentContributor` (only formatter owner; `rg AnonymousMethodExpression src/Reihitsu.Formatter` — the other hits are line breaks and switch-case braces). Analyzers: RH5204 skips anonymous-function blocks; RH5205 excludes anonymous methods (non-goal, follow-up candidate in `00-lead.md`) | one owner |
| Hole lookup `TryGetRawStringHole` / `IsAlignedInRawStringHole` | deleted; `rg` finds no reference in `src`, `tests`, `sdd/specs` | ok |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `AnonymousMethodAlignmentTests` (15 tests) | exact output per FMT-INDENT-031 position; literal and disabled-text lines keep their columns | any block line off its anchor column, any literal line moved, a second pass changing the output, CRLF drift | `AssertRuleResult` (exact output, LF and CRLF, second pass, `PhaseStabilityCheck`) | adequate |
| `ReihitsuFormatterNodeColumnTests` (tab in front of a node-level target; chain targets) | FMT-PIPE-017 with the block under `delegate` | block off by the tab's width | `AssertFormatsTarget` (LF/CRLF, second pass) | adequate |
| RH5101/RH5102/RH5111/RH5205 fix tests | the fix result puts the block under `delegate` | a column 16/8/25 result | `VerifyCodeFixWithEndOfLine`; the RH5111 Fix All case through `Verify` per line ending (08) | adequate |
| RH5203/RH5204/RH5205 parity tests | the aligned layout is not reported and stays | a diagnostic, or the formatter moving it | `VerifyFormatter` | adequate |
| Rewritten existing tests (05, table "Existing tests whose asserted output the change moves") | only the anonymous method's `{`…`}` lines move | any other line changed | unchanged helpers | adequate; diff checked, every changed expected line is a block line moved under `delegate` |

### Failure classes dismissed
- FC-001/002 — no narrowed guard; a condition was removed.
- FC-004, FC-005, FC-019, FC-020, FC-021 — whitespace in front of lines only; `#region`, `#if NEVER`, and comment lines inside the moved block are covered by `BlockContentMovesWithTheBlockWhileLiteralLinesAndDisabledTextKeepTheirColumns`, `BlockBehindConditionalDirectivesAlignsUnderTheActiveDelegate`, and my `#region` fixture (moved with the block, as for a lambda).
- FC-006, FC-023–026, FC-029, FC-036 — no token, name, or member changes.
- FC-007–009, FC-018, FC-030 — no code-fix code changed; fix results move only through the formatter, pinned by the fix tests.
- FC-010/011/012 — no loop; every fixture stable on the second pass, `PhaseStabilityCheck` in the helper.
- FC-013 — swept twelve layout rules over the new layout: no diagnostic.
- FC-014 — no new rule.
- FC-015/016 — one owner; the shape's siblings (positions) enumerated above.
- FC-022 — verbatim and raw literal lines inside the block keep their columns (test above).
- FC-027 — the enclosing-scope axis is the change itself; enumerated above.
- FC-028 — node-level targets covered by `ReihitsuFormatterNodeColumnTests` and four fix paths.
- FC-031 — the class summary of `LambdaAlignmentContributor` and its inline comment describe the new code; `LayoutModel`'s summary still holds.
- FC-032 — the implementer's full-project run was taken after 11f3b34; later commits change only tests and handovers.
- FC-038–043 — no shape declared, no precheck change.
- FC-044 — no modeled result.
- Tab matrix — the cells "node-level target with the tab in front of it" (test) and "tab inside a literal / comment in front of the anchor, whole document" (fixture) hold.

### Architecture findings
_None._

### Hints
_None._

### Follow-up candidates
_None_ beyond `00-lead.md`.

## thoenissen/Reihitsu#1698

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | low | `sdd/specs/formatter/raw-string-alignment.md:357` (FMT-RAW-007, second bullet) | FC-015 (one rule stated twice) | – | PR-introduced | The bullet paraphrases FMT-INDENT-006 as "the column of the token it precedes, or one level deeper than a closing brace it precedes". For a hole line of a multi-line raw literal that holds the closing brace of a nested regular string's interpolation (`$$"""` … `{{$"{⏎a⏎// f⏎}"}}`), FMT-INDENT-006 now gives the brace's column, and the code does (fixture `x` in `f3/H.cs`: `// f` and `}` both in column 22). The paraphrase now states the opposite of the rule it defers to. Text only: no compiled behavior, no diagnostic | Drop the paraphrase and keep "the column FMT-INDENT-006 gives it", or add the interpolation exception to it |

Code checked against FMT-INDENT-006: `ClosesContentOneLevelDeeper` keys on `closeBrace.Parent is InterpolationSyntax` and `RawStringLineUtilities.IsMultiLineRawStringLiteral(interpolation.Parent)` — the string the interpolation belongs to, as the spec says. Fixtures (formatted twice, stable): regular, `$@"`, single-line raw `$"""{…}"""`, two comments in front of one `}` with text behind the brace, a multi-line `/* */` comment, a regular string nested in a raw hole (brace column) next to the raw hole's own `}}` (one level deeper, unchanged), content column ≠ brace column (`$"{g(⏎a)⏎// f⏎}"`), and a hole inside a block lambda. Every comment lands in the brace's column with the FMT-BLANK-011 blank line above it and no blank line between it and the brace.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Comment in front of `}` one level deeper (`CommentIndentationContributor.AlignCommentsBeforeToken` + `ClosesContentOneLevelDeeper`) | the `}` token's parent and that interpolation's string start token | (a) `BraceBlankLinePolicy` (line 58): every `}` incl. interpolations — decides blank lines, not the column; FMT-BLANK-011/020 keep the blank line above and remove the one below, pinned next to the column by the tests. (b) `RH5204.AlignCommentIndentation` adds one level in front of every `}` — but only on a line with an expectation, and an interpolation's `}` line has none (`ShouldAnalyzeToken` needs `IsIndentingScope`; `InterpolationSyntax` is not one), so the differing statement is unreachable; the RH5204 parity test confirms. (c) `RawStringLineUtilities.IsMultiLineRawStringLiteral` reused, not copied | spans agree where both decide | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Column of a comment in front of a closing delimiter | `CommentIndentationContributor` (formatter); `RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.AlignCommentIndentation` (pre-existing copy, unreachable for interpolation braces as above) | parity holds on every reachable line; no finding |
| Multi-line raw literal test | `RawStringLineUtilities.IsMultiLineRawStringLiteral` (reused) | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `InterpolationClosingBraceCommentTests` (10 tests) | comment in the brace's column for regular, verbatim, single-line raw, nested regular in raw; raw hole's own brace one level deeper; blank line above kept | comment four columns right of `}`; blank line missing; raw side moved | `AssertRuleResult` (exact, LF/CRLF, second pass, phase stability) | adequate; both sides of the boundary in one input (FC-003) |
| RH5205 fix test | node-level path gives the same column | column + 4 | `VerifyCodeFixWithEndOfLine` | adequate |
| RH5204 parity test | the new column is not reported | a diagnostic | `VerifyFormatter` | adequate (raw `r` left out because of the `$$` markup; that side is unchanged and pinned by `RawStringInterpolationHoleTests`) |

### Failure classes dismissed
- FC-002 — the qualifier "not part of a multi-line raw string literal" is the explicit condition.
- FC-003 — both sides tested.
- FC-004/005/019/020 — only the column of a comment line changes; directives are not decided by this sentence (05, *Not covered*).
- FC-021 — line and delimited comment before the closing delimiter covered.
- FC-022 — regular, verbatim, single-line raw, multi-line raw.
- FC-027 — nesting regular-in-regular, regular-in-raw, raw-in-regular, inside a lambda body: covered.
- FC-006–009, FC-018, FC-023–026, FC-029, FC-030, FC-036 — no token or code-fix change.
- FC-010–012 — stable; the blank-line phase runs before indentation and does not read columns.
- FC-013 — RH5204, RH5020, RH5104, RH5024 report nothing (skeptic; RH5204 pinned).
- FC-028 — RH5205 fix path pinned.
- FC-031 — summaries of `AlignCommentsBeforeToken` and `ClosesContentOneLevelDeeper` and the inline comment describe the new rule.
- FC-038–044 — no shape declared (decision 3), nothing modeled.

### Architecture findings
_None._

### Hints
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1567

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `tests/Reihitsu.Formatter.Test/Regression/LineBreaks/` (no test) | FC-034 | – | same requirement | The issue's own reported shape — a parenthesized lambda whose modifier stands on the line above its `(` as a whole value: `Func<int, int> f = static⏎    (int x) => x;`, `async⏎(int x) => …`, and `b ? … : static⏎(int x) => x` — is pinned by no test. The run closes the issue on the finding that FMT-BREAK-041 already joins it (01, 02; my fixture `f3/L.cs` confirms the join), but `rg` over the test projects finds this gap only inside an argument list (`ListElementInteriorJoinTests:1263`, `PrefixKeywordSpacingTests:31`, joined by FMT-BREAK-015) and as a simple lambda (`ContinuationJoinTests` row "Modifier of a lambda", `static⏎y => y`). The new tests pin the shape only together with a return type (`ModifierReturnTypeAndParameterListOnThreeLinesJoinIntoOne`), whose `(` FMT-BREAK-045 joins, not FMT-BREAK-041. A regression of the modifier → `(` join for a value would pass every test while the PR says the issue is closed | Add characterization rows tagged FMT-BREAK-041 for the issue's three inputs (declaration value with `static`, with `async`, conditional branch), e.g. rows in `ContinuationJoinTests.EachGapIsJoined` |
| 2 | low | `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ListElementInteriorJoinRewriter.cs:165-167` (`IsOwnedGap`) | FC-031, FC-015 | – | PR-introduced | The comment says `LineBreakListRewriter` collapses the parameter-list `(` of every owner "except a parenthesized lambda; the lambda's opening gap has no other owner and is joined here". The new `LineBreakListRewriter.VisitParenthesizedLambdaExpression` now owns that gap for a lambda with an explicit return type, so inside a list element two rewriters own it. Output is the same — `ListElementInteriorJoinRewriter` runs first (`LineBreakPhase.cs:47-48`), both refuse through `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia`, and both write through `LineBreakTriviaUtilities` (minimal separator); FMT-BREAK-045 names the overlap ("where FMT-BREAK-015 joins the same gap") — so no corruption, but the comment states a single ownership that no longer holds. Comment only: cannot reach compiled behavior | Reword the comment to name FMT-BREAK-045's collapse of a lambda's `(` behind a return type (or treat that gap as owned there, which leaves one owner and the same output) |

Code checked against FMT-BREAK-045: `VisitParenthesizedLambdaExpression` calls `CollapseOwnedOpenParenToDeclarationLine` only when `ReturnType != null`; the previous token of `(` is then the return type's last token, inside the lambda. Fixtures (`f3/L.cs`, twice, stable): declaration value, conditional branch, `static async Task<int>`, block body (aligned under the joined `(`), a lambda in another lambda's expression body, a blank line in the gap, a line break inside the parameter list after `(` (joined by the list rules), and an attribute list in front (`[A]` stays on its own line, `int (int x)` joined — not this requirement's gap). Comment, directive, and disabled text in the gap keep the line break (four tests).

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Join return type → `(` (`LineBreakListRewriter.VisitParenthesizedLambdaExpression` → `CollapseOwnedOpenParenToDeclarationLine`) | the gap from the return type's last token to `(`: `HasLineBreakBetween`, `WouldJoinAcrossUnjoinableTrivia` | `ListElementInteriorJoinRewriter.CanJoin` for the same gap inside a list element (`AreSeparatedByEndOfLine`, `WouldJoinAcrossUnjoinableTrivia`); the precheck's FMT-UNSUP-017 gaps (`JoinedGapUtilities.GetGap`) deliberately do not include this gap (07), so a comment there keeps the line break instead of refusing the file, as FMT-BREAK-045 states | same trivia guard and writer; boundary identical | ok, see finding #2 for the comment |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Joining a parameter-list opener onto its owner's line | `LineBreakListRewriter.CollapseOwnedOpenParenToDeclarationLine` (reused by `DeclarationBraceLineBreakRewriter`); no private copy | one owner |
| The return type → `(` gap of a lambda inside a list element | `ListElementInteriorJoinRewriter` (FMT-BREAK-015, runs first) and `LineBreakListRewriter` (FMT-BREAK-045) | two owners with proven parity, named by the spec; finding #2 (stale comment) |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `LambdaReturnTypeJoinTests` (17 formatter tests) | joined in every position and return-type kind; comment/directive/disabled text keep the break | a kept line break; a moved comment; unstable second pass | `AssertRuleResult` (exact, LF/CRLF, second pass, phase stability) | adequate |
| `LambdaReturnTypeJoinTests.JoinAppliesInANodeLevelRoot` | the join applies through `FormatNode` | output keeps the line break | direct, LF/CRLF, `Trim()` of a one-line result | adequate for its invariant (the output is one line, so `Trim` hides nothing the join decides) |
| `RH5111…VerifyFixJoinsTheReturnTypeOfAnAssignedLambdaWithItsParameterList` | code-fix path joins | `f = int⏎(int x) => x` | `VerifyCodeFixWithEndOfLine` | adequate |
| (missing) the issue's modifier → `(` case as a value | — | — | — | finding #1 |

### Failure classes dismissed
- FC-001/002 — the guard is the shared helper's; the qualifier "explicit return type" is the `ReturnType != null` condition.
- FC-003 — with and without return type, with and without trivia: both sides tested.
- FC-004/005/019/020/021 — comment (line and delimited), directive, disabled text in the gap tested; end-of-line comment on the return type's line is the spec's negative example.
- FC-006 — joining whitespace only; `int (int x)` parses the same.
- FC-010–012 — stable; no later phase re-breaks the gap (attribute placement leaves it, fixture).
- FC-013 — RH5105 does not report a lambda's `(` (P11; follow-up candidate in `00-lead.md`); RH6006/6008/6012/6015/5105/5108 report nothing on the joined layout (02).
- FC-027 — positions enumerated by the tests and my fixture.
- FC-028 — `FormatNode` and the RH5111 fix path pinned.
- FC-038 — no comment moves: the join is withheld whenever trivia other than whitespace and line breaks is in the gap.
- FC-039–044 — no precheck change, nothing modeled.
- FC-007–009, FC-018, FC-022–026, FC-029, FC-030, FC-036 — no code-fix code, literal, name, or member change.

### Architecture findings
_None._ (Finding #2 is two owners with parity that the spec names; not a disagreement.)

### Hints
_None._

### Follow-up candidates
_None_ beyond `00-lead.md`.

## Whole run

- Visible output is decided (check 10): every added or changed expected value of a formatter, code-fix, or parity test moves an anonymous method's block under its anchor (1B), puts a comment in front of a non-multi-line-raw interpolation brace in the brace's column with the blank line kept (2A), or joins a lambda's return type and `(` (3A). Changed existing tests checked line by line in the diff: only block lines moved. No rule page changed.
- Cost of special cases (check 11): `ClosesContentOneLevelDeeper` and `VisitParenthesizedLambdaExpression` each serve an output the maintainer chose over leaving the construct as written (2A over B, 3A over B); `IsAlignedInRawStringHole` and `TryGetRawStringHole` were deleted. No shape is declared, so no *Special cases* list applies.
- Scope (check 9): no edit outside the three issues; production diff 4 files, +39/−67.
- Comments (check 8): no `TODO`, no tracker reference in added source or test lines.
- `pass, text cleanup` does not apply: finding #1 asks for a test.
