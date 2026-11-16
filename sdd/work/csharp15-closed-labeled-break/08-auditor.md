Role: auditor
Commit: da6a450
Verdict: pass

## Scope

- Head `da6a450` is checked out on `claude/ecstatic-feynman-ef6145`.
- Base `origin/main` is `1d2d29a970cd9144d0527e1c505303f2ef164a66`. `git fetch` confirms it is still the tip, and it is an ancestor of the head, so it is merged.
- Full audit, the first of the run, of `1d2d29a..da6a450`.
- The diff matches the handovers:
  - 3 production files: `ModifierOrderingUtilities.cs`, `StatementShouldBeFollowedByABlankLineAnalyzerBase{TStatement}.cs`, and `RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.cs`.
  - 8 test files.
  - The RH5010, RH5011, RH7105, and RH7501 specs.
  - The change doc `sdd/changes/2026-11/rh7105-closed-safe-modifiers.md`.
  - The rule page `documentation/rules/RH7105.md`.
  - The `sdd/work/` handovers.
- `scripts/lane-check.sh behavior-change`: confirmed.
- My prompt carried no intended fix, suspected cause, or hoped-for verdict.
- Evidence beyond static tracing: `scripts/apply-fix.sh RH5011` on a scratch fixture outside the repository. Under LF and CRLF it gave `fixed (1 action(s), 3 iteration(s))`. The fixture held these cases:
  - `break` with its label on the next line, followed by a statement;
  - `break outer; // x` in front of the next switch section;
  - `break outer;` in front of an `#if` line;
  - `break outer;` between two labeled statements.
- I trusted the cleaner's Release build and the focused test results of the roles. No re-run was needed, because no finding depends on them.

## thoenissen/Reihitsu#1764

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| | | | | | | _None._ | |

How the requirements were checked against the code:

- **RH7105-001, the new positions.** `GetRh7105Rank` gives `ClosedKeyword` rank 4, the rank of `sealed`. Its arm `_ when modifier.Text == "safe"` gives `safe` rank 11, the rank of `unsafe`.
- **The examples.** I traced every example of RH7105-001, -002, and -004 and every input of the change doc through `OrderModifiersForRh7105`:
  - `new closed partial` and `static safe extern` are reported on `new` and on `safe`.
  - `closed partial`, `closed new partial`, `closed unsafe partial`, `static extern safe`, `safe partial` (both parts), `safe required`, and `safe ref` are in order.
- **Pairs the spec leaves undecided.** `closed`/`sealed` and `safe`/`unsafe` tie on rank. The stable `ThenBy(index)` keeps the order as written, so neither pair is ever reported.
- **The `safe` match by text.** Only modifier tokens reach the switch, and every arm that names a kind comes first. A modifier token whose text is `safe` is the `safe` keyword, so matching on text is equivalent to matching on its kind. It is a cheap token check, not a rule over text.
- **Every modifier `closed` and `safe` can stand with.**
  - `closed` combines only with an accessibility, `file`, `new`, `unsafe`, and `partial` (compiler facts in `02-skeptic.md`). Every one of those pairs is covered by a Core rank row or an analyzer test.
  - For `safe`, the Core rows cover the modifiers in front of it (public, static, new, readonly, async, extern) and behind it (required, ref, partial). The analyzer tests compile the fixed code at `Preview`.
  - `abstract`, `virtual`, `sealed`, `override`, and `volatile` were not tested with `safe`. Their ranks (2–8) are all below 11, the same relation `unsafe` already has to them.
- **Issue goals.** These are as adjusted by decision 4:
  - The fix no longer moves `closed` or `safe` behind `ref` or `partial`.
  - The reproduction test is the issue's own input, doc comment included.
  - The spec and the rule page name both modifiers.
  - The fixed code is compiled by the verifier.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Rank of a modifier in the RH7105 order (`GetRh7105Rank`, new arms for `closed` and `safe`) | One modifier token: its kind, and for `safe` its text | `TryGetRh7105Violation` (compares `RawKind` against the sorted list) and the RH7105 and RH2001 fixes (both write `OrderModifiersForRh7105`) | All three use one ranking. The violation check compares kinds, and tied ranks keep the written order, so a tie can never show up as a kind difference | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| The canonical RH7105 modifier order | `rg "SealedKeyword\|UnsafeKeyword\|ClosedKeyword\|SafeKeyword\|\"safe\"" src -g '!*Test*'` finds only `ModifierOrderingUtilities.GetRh7105Rank`. Its consumers, found with `rg "OrderModifiersForRh7105\|TryGetRh7105Violation"`, are the `RH7105DeclarationKeywordsMustFollowOrderAnalyzer`, the `RH7105DeclarationKeywordsMustFollowOrderCodeFixProvider`, and the `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider`. The text statements of the order are RH7105-001 and `documentation/rules/RH7105.md`, and they now agree with the code | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7105 `ClosedInFrontOf{Partial…,New…,Unsafe…}`, `ClosedBehindTheAccessibilityWithoutPartialIsNotReported`, `Safe{InFrontOfPartial…,InFrontOfRef…,InFrontOfRequired…}IsNotReported`, `SpecExampleClosedAndSafeInOrderAreNotReported` | The declaration is not reported, and the source compiles | Any RH7105 diagnostic, or a compiler error (the verifier checks errors; `ClosedTypes` adds `IsClosedTypeAttribute` so that `closed` compiles) | `Verify(source, onConfigure)` | adequate |
| RH7105 `NewInFrontOfClosed…`, `UnsafeInFrontOfClosed…`, `RequiredInFrontOfSafe…`, `SafeInFrontOf{Extern,Async,ReadonlyOnARefPartialStruct}…`, `ClosedInFrontOfTheAccessibility…`, `SpecExampleClosedAndSafeAreFixed` | The diagnostic is on the exact token, the fix writes the exact text, the fixed code compiles, and Fix All converges | A different location, a different text, CS0267 or CS1585 in the fixed state, or a diagnostic left over | `Verify(source, fixedSource, onConfigure)`, with `FixedState` carrying the attribute and preview applied as a solution transform | adequate |
| Core `Rh7105RanksClosedInThePositionOfSealed`, `Rh7105RanksSafeInThePositionOfUnsafe`, `Rh7105RanksUnnamedModifiersLast` | The pair is accepted in canonical order; the reverse is reported on its first token and reordered | `TryGetRh7105Violation` accepts the reverse, reports another token, or the reorder differs | `AssertRh7105Order` | adequate. The `safe` token comes from preview source and its text is asserted |
| RH2001 `VerifyPrivateGetOnlySafeAutoPropertyCodeFixWritesReadonlyInFrontOfSafe` | The second consumer of the order writes `readonly safe`, and the result compiles | `safe readonly`, or a compile error | `Verify(source, fixedSource, onConfigure, diagnostic)` | adequate |

## thoenissen/Reihitsu#1765

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| | | | | | | _None._ | |

How the requirements were checked against the code:

- **How the analyzer reads the gap.** `OnStatement` now takes `statement.GetLastToken().GetNextToken()`. For a valid `break` statement, the last token is its semicolon, so the label lies in front of the gap.
- **RH5011-001, -002, and -008.** I traced each row of the skeptic's class matrix against the new predicate. Every row now gives the specified result:
  - in front of a block's `}`: not reported;
  - in front of a switch's `}`: not reported;
  - in front of a directive: not reported;
  - in front of `else` on the same line and on the next line: not reported;
  - in front of a `do` footer: not reported;
  - blank line, then a statement: not reported;
  - a statement on the next line: reported;
  - a statement on the same line: reported;
  - `break sw;` in front of the next section: reported.
- **Members beyond the matrix.** My `apply-fix.sh` probe added these members, and all converge:
  - the label on the line after `break`;
  - an end-of-line comment in front of the next section;
  - an `#if` line behind the statement;
  - a labeled statement behind it.
- **Other consumers of the base.** `StatementShouldBeFollowedByABlankLineAnalyzerBase` has one subclass, and its constructor is `private protected`, so removing the abstract `GetNextToken` changes no other output.
- **RH5010, RH5012, and RH7501.** Their labeled clarifications match the code:
  - RH5010 and RH5012 use `Keyword.GetPreviousToken()`, and a label stands behind the keyword.
  - `BlankLineSpacingPolicy.IsTerminalDirectSwitchSectionBreak` and RH7501 decide by type.
- **Issue goals.** These are as adjusted by decision 7:
  - the issue's input;
  - fix convergence under LF and CRLF;
  - a formatted labeled-`break` input (two formatter tests, LF and CRLF, each with a second pass);
  - tests for RH5010, RH5012, and RH7501.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| The token behind the `break` statement, and the gap whose blank lines and directive decide RH5011 (analyzer, `OnStatement`) | From `statement.GetLastToken()` to its `GetNextToken()`: `TokenGapAnalysis.Between`, and the last token's trailing trivia plus the next token's leading trivia for `IsFollowedByDirective`. The checks for `}`, `None`, `else`, and `do`-`while` look at that next token | RH5011 code fix: `SemicolonToken.GetNextToken(includeZeroWidth: true, includeSkipped: true)`. `FindLineEndingTrivia` uses the semicolon's trailing trivia plus the next token's leading trivia | Both measure the same gap on syntax-valid code: the last token is the semicolon, and with no missing or skipped tokens the flags select the same next token. They can differ only on syntax-invalid code, a missing `;` or skipped tokens, which no caller of the repository relies on | holds |
| `ContinuesEnclosingStatement` (unchanged), now fed the real next token | One token: `else`, or `while` whose parent is a `do` statement | RH5011-008 | agree | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| "The token behind a `break` statement" for RH5011 | `rg "GetNextToken" src/Reihitsu.Analyzer/Base src/Reihitsu.Analyzer*/Rules/Layout/RH5011*` finds two computations: `StatementShouldBeFollowedByABlankLineAnalyzerBase.OnStatement` and `RH5011…CodeFixProvider` (`ApplyCodeFixAsync`, `CanRegisterCodeFix`). Both now start at the statement's end. `rg "Keyword\.GetNextToken"` over production code finds no other derivation from the keyword | Two call sites with one definition. Parity holds on every syntax-valid input, as the guard-scope row shows. The lead decided this under decision 6; see *Hints* |
| Terminal-`break` exemption and switch-section `break` handling for labeled `break` | `BlankLineSpacingPolicy.IsTerminalDirectSwitchSectionBreak`, `BlankLineBreakSpacingRewriter`, `SwitchCaseBraceRewriter`, `RH7501…Analyzer`. All decide by type. None is changed | unchanged, consistent |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5011 `VerifyNoDiagnosticForLabeledBreakBefore{ClosingBraceOfBlock,ClosingBraceOfSwitch,Directive,ElseOnTheSameLine,ElseOnTheNextLine,DoStatementFooter}`, `…FollowedByBlankLine`, and the two spec-example tests | Not reported | Any RH5011 diagnostic | `Verify(source)` | adequate. Both sides of RH5011-008 are covered: the `while` statement test is reported (FC-003) |
| RH5011 `VerifyLabeledBreakFollowedByStatement{,OnTheSameLine}IsReportedAndFixedWith{LineFeed,CarriageReturnLineFeed}` | Reported on `break`; the fix writes the exact text, with the given line ending, and converges | Re-reported after the fix, a different text, or line-ending drift | `VerifyCodeFixWithEndOfLine` | adequate (LF and CRLF) |
| RH5011 `VerifyBreakNamingItsSwitch…`, `…AsEmbeddedBody…`, `…FollowedByCommentLine…`, `…WithCommentSpanningItsLine…`, `VerifyFixAllCorrectsSeveralLabeledBreakStatements` | Exact fix text, convergence, Fix All over two diagnostics next to one that is not reported | Same as above | `Verify(source, fixedSource)` | adequate |
| RH5011 `VerifyLabeledBreakWithCommentBeforeSameLineStatementIsReportedWithoutFix` | Reported, and no fix is offered (RH5011-006) | A fix is offered | `VerifyNoCodeFixIsOffered` | adequate |
| RH5011 formatter tests `VerifyFormatterKeepsLabeledBreakStatementsAndAnalyzerReportsNothing` and `VerifyFormatterFixesLabeledBreakViolation` | Formatter output equals the exact expected text, stays stable on a second pass, and the analyzer reports nothing on it (SHARED-DIAG-010) | RH5011 on the formatted text, a second pass that changes it, or a different text | `VerifyFormatter` (LF and CRLF, two passes, analyzer after each pass) | adequate |
| RH5010 (two tests), RH5012 (two), RH7501 (two) labeled tests | Pin the spec clarifications: exempt or reported by statement type, and the RH7501 move keeps the label | A diagnostic that differs, or a different fix text | `Verify(source)` / `Verify(source, fixedSource)` | adequate (characterization; they pass on `main`) |

## Whole run

### Visible output is decided

The RH7105 expected values each follow from one of three sources:

- the order of maintainer answers 1B and 2B, recorded under `Maintainer answers` in `00-lead.md` and in the change doc's `Decisions`;
- behavior on `main` that stays unchanged (`closed public class` → `public closed class`, `safe async` → `async safe`, `static safe extern` → `static extern safe`);
- SHARED-DIAG-011 and SHARED-FIX-003, which are unchanged.

The rule-page order line is the order those answers decide, and the change doc lists it under `Visible output changes`. The RH2001 expected value is unchanged behavior.

The RH5011 expected values are demanded by RH5011-001 through -008, which are unchanged in substance; the run only adds clarifying sentences. The RH5010, RH5012, and RH7501 tests pin behavior that is on `main` today.

No expected value is uncovered.

### Cost of special cases

- The `safe` text arm is the cost of an experimental Roslyn kind (`RSEXPERIMENTAL006`) and replaces a suppression. It is not code for a rare input.
- The run adds no guard and declares no shape. The RH5011 change removes one computation (the keyword-relative `GetNextToken` override).
- The formatter is unchanged, so no FC-042 guard exists to delete.

### Comments and conventions

- The summary of `OnStatement` and the remarks of `GetRh7105Rank` describe the new code.
- The removed abstract member leaves no stale `<inheritdoc>` behind.
- There is no `TODO`, no commented-out code, and no reference to a tracker number in the changed source (`git diff … -- src | grep -E "#[0-9]{3,}|TODO"` finds nothing).
- No edit falls outside the run's scope.

### Failure classes dismissed
- FC-004, FC-019, FC-020, FC-021 — No trivia handling changed. The RH7105 write path (`TryWriteModifiersInOrder`, SHARED-MODORDER-002/-003) is untouched, and only the computed order changes. The RH5011 fix is untouched. A comment and a directive in the RH5011 gap are covered by the labeled tests, and the analyzer reads the same gap as before.
- FC-005 — No comment guard changed.
- FC-008 — Fix All is unchanged. Two-diagnostic Fix All runs for RH7105 (`SpecExampleClosedAndSafeAreFixed`) and RH5011 (`VerifyFixAllCorrectsSeveralLabeledBreakStatements`) pass.
- FC-009 — No fix registration changed. RH5011's `CanRegisterCodeFix` is untouched, and RH5011-006 still withholds the fix (test).
- FC-010, FC-011, FC-012, FC-014 — No formatter phase changed. The formatter tests show two stable passes on labeled `break`.
- FC-018, FC-036 — No rename.
- FC-022, FC-023 — No literals or identifiers are rewritten. The `safe` text match is on a modifier token, and a verbatim `@safe` cannot stand in a modifier list.
- FC-025, FC-026, FC-027, FC-029 — No expression-body, multi-token re-resolution, nesting, or key/value layout logic is touched. RH5011 nesting (an embedded body, a `do` body, nested loops) is covered.
- FC-028, FC-030 — No formatter entry point is used differently. The RH5011 fix writes text directly, and the RH7105 fix writes modifier tokens.
- FC-032 — No project result is reused. The roles' results were taken after the production commit 50321f7.
- FC-035 — The expected values come from the specs and the maintainer answers. The test implementer saw them fail before the production change (`04-test-implementer.md`, `05-implementer.md`).
- FC-037 — No text-only claim is made.
- FC-038 to FC-043 — No shape is declared unsupported, and the precheck is untouched.
- Covered rather than dismissed:
  - FC-001, FC-002: guard-scope tables above.
  - FC-003: both sides of each new boundary are tested.
  - FC-006, FC-024: compiled fixed code, and `partial` on both parts.
  - FC-007: convergence under LF and CRLF.
  - FC-013: formatter tests.
  - FC-015: policy tables.
  - FC-016: the `ref` sibling of `partial`, RH2001 as the second consumer, and RH5010, RH5012, and RH7501.
  - FC-017: assertion tables.
  - FC-031: comments above.
  - FC-033: preview language version and the closed-type attribute, so the harness compiles what it claims.
  - FC-034: the issue's own inputs.

### Architecture findings
_None._ Both findings raised by the skeptic are resolved in this diff:

- RH7105: the order list, its `Why:`, the rule page, and the rank remark now agree.
- RH5011: the analyzer and the fix both measure from the statement's end.

### Hints
- RH5011: the analyzer (`GetLastToken().GetNextToken()`) and the code fix (`SemicolonToken.GetNextToken(includeZeroWidth: true, includeSkipped: true)`) still derive the next token in two call sites. Their flags differ. They agree on every syntax-valid input, and the implementer and decision 6 accepted this. Hint only, not a finding.
- `ModifierOrderingUtilities.GetRh7105Rank` matches `safe` by text because `SyntaxKind.SafeKeyword` is experimental in Roslyn 5.12. When a Roslyn update removes the experimental marker, the arm can become a kind arm. The rank remark already states the reason, and nothing needs tracking now.
