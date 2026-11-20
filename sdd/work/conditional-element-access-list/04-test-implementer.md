Role: test implementer
Commit: 97a7b5a2
Verdict: #1877: tests written; #1878: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples (FMT-BREAK-014, RH5307-001/-003/-004 as changed), from the change doc's visible output changes, and, where neither states a column, from the element-access analog on unchanged code (`d[1,⏎2]` in the same place), which FMT-BREAK-014 ("which of the three it is MUST NOT change whether the list is joined") makes the expected output of the `c?[1,⏎2]` form. Production code was not touched. Every new test was run on this head; every test marked "fails" below fails on the expected-versus-actual difference of the change (the list stays wrapped, or RH5307 reports nothing on the `?[` list), not on layout elsewhere or a compile error.

## thoenissen/Reihitsu#1877

### Tests

New file `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ConditionalElementAccessArgumentListTests.cs` (class `ConditionalElementAccessArgumentListTests`); all use `AssertRuleResult` (LF and CRLF, second pass, phase stability) unless noted.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SpecExampleJoinsConditionalAndNullForgivingElementAccessListsAlike` | FMT-BREAK-001, FMT-BREAK-014 | `AssertRuleResult` | fails (`c?[` lists stay wrapped) |
| `WrappedConditionalElementAccessListJoins` | FMT-BREAK-014 | `AssertRuleResult` | fails |
| `ConditionalElementAccessListWithOpeningBracketAloneJoins` | FMT-BREAK-014 | `AssertRuleResult` | fails |
| `ConditionalElementAccessListWithClosingBracketAloneJoins` | FMT-BREAK-014 | `AssertRuleResult` | fails |
| `ConditionalElementAccessListBehindNullForgivingOperatorJoins` (`c!?[`) | FMT-BREAK-014 | `AssertRuleResult` | fails |
| `ConditionalElementAccessListBehindMemberBindingJoins` (`o?.Q?[`) | FMT-BREAK-014 | `AssertRuleResult` | fails |
| `ConditionalElementAccessListBehindInvocationJoins` (`this.Get()?[`) | FMT-BREAK-014 | `AssertRuleResult` | fails |
| `ConditionalElementAccessListFollowedByInvocationJoins` (`f?[1, 2](3)`) | FMT-BREAK-014; FC-027 | `AssertRuleResult` | fails |
| `ConditionalElementAccessListFollowedByMemberAccessJoins` (`.ToString()`) | FMT-BREAK-014; FC-027 | `AssertRuleResult` | fails |
| `ElementBindingAsReceiverOfElementAccessJoinsBothLists` (`e?[0,⏎1][1,⏎2]`) | FMT-BREAK-014 | `AssertRuleResult` | fails (element access joined, binding not) |
| `ConditionalElementAccessListInInvocationArgumentJoinsInOnePass` (`N(c?[1,⏎2] ?? 0, 3)` → one line) | FMT-BREAK-014; FC-027 | `AssertRuleResult` | fails |
| `ConditionalElementAccessListInImplicitElementAccessJoinsInOnePass` (initializer, expanded as the `d[…]` analog) | FMT-BREAK-014; FC-027 | `AssertRuleResult` | fails |
| `ConditionalElementAccessListInLambdaBodyJoins` | FMT-BREAK-014; FC-027 | `AssertRuleResult` | fails |
| `ConditionalElementAccessListInInterpolationHoleJoins` | FMT-BREAK-014; FC-027 | `AssertRuleResult` | fails |
| `LineBreakBetweenQuestionMarkAndBracketAndWrappedListAreBothJoined` (`c?⏎[1,⏎2]`) | FMT-BREAK-014, FMT-BREAK-040; FC-012 | `AssertRuleResult` | fails (gap joined, list not) |
| `LineBreakInFrontOfQuestionMarkAndWrappedListAreBothJoined` (`c⏎?[1,⏎2]`) | FMT-BREAK-014, FMT-BREAK-040; FC-012 | `AssertRuleResult` | fails (gap joined, list not) |
| `BlockCommentBetweenBracketsKeepsConditionalElementAccessListAsWritten` | FMT-BREAK-001, FMT-BREAK-014; FC-019, FC-021 | `AssertRuleResult` (input stable) | passes |
| `DirectiveBetweenBracketsKeepsConditionalElementAccessListAsWritten` | FMT-BREAK-001, FMT-BREAK-014; FC-019, FC-020 | `AssertRuleResult` (input stable) | passes |
| `MultiLineArgumentKeepsConditionalElementAccessListWrapped` (`2 +⏎3` → `2⏎+ 3`, list stays wrapped) | FMT-BREAK-014; FC-003 | `AssertRuleResult` | passes |
| `CommentInFrontOfQuestionMarkOfWrappedConditionalElementAccessIsUnsupported` | FMT-UNSUP-014; FC-038 | `AssertUnsupportedFor(AttachedPartCommentReason, …)` | passes |
| `FixedSizeBufferSizeListStaysWrapped` (lead decision 3; characterization of `main`: `fixed int b[⏎3];` stays wrapped, `3` aligned behind `[`) | — (FMT-BREAK-014 states it does not decide it) | `AssertRuleResult` | passes |

Changed and added in existing files:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ListElementAttributeLayoutSinglePassTests.ConditionalElementAccessJoinsWrappedArgumentsWhenNestedIndexerCollapses` — renamed from `…KeepsWrappedArguments…`; expectation `a?[b[1, 2],⏎3]` → `a?[b[1, 2], 3]` per the maintainer's answer 1A (change doc, *Join the list in the formatter*); tag added | FMT-BREAK-014; FC-032 | `AssertRuleResult` | fails |
| `ReihitsuFormatterDetachedRootTests.ConditionalElementAccessArgumentListTargetGetsItsArgumentsCollapsed` — the list of `a?[…]` as target of `FormatNodeInDocumentAsync` (the node the RH5307 fix formats), space and tab in the target | FMT-BREAK-014, FMT-PIPE-024; FC-028 | `AssertFormatsTarget` | fails |
| `RH5307IndexerBracketedArgumentsShouldBeSingleLinedFormatterTests.VerifyFormatterFixesMultilineConditionalElementAccessArguments` (analyzer/formatter parity) | FMT-BREAK-014, RH5307-001; FC-013 | `FormatterTestsBase<RH5307>.VerifyFormatter(source, fixed, diag)` | fails (RH5307 reports nothing) |
| `RH5307IndexerBracketedArgumentsShouldBeSingleLinedFormatterTests.VerifyFormatterKeepsConditionalElementAccessArgumentsWithInteriorComment` | FMT-BREAK-001, RH5307-002; FC-013 | `VerifyFormatter(source)` | passes |
| `RH5101FirstArgumentShouldBeOnSameLineAnalyzerTests.VerifyFixJoinsConditionalElementAccessListInTheFirstArgument` (`N(⏎c?[1,⏎2] ?? 0, 3)` → `N(c?[1, 2] ?? 0, 3);`) | FMT-BREAK-014; FC-032 | `VerifyCodeFixWithEndOfLine`, LF and CRLF | fails |
| `RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests.VerifyFixJoinsConditionalElementAccessListInTheValue` (change doc example 2) | FMT-BREAK-014; FC-032 | `VerifyCodeFixWithEndOfLine`, LF and CRLF | fails |
| `RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyFixJoinsConditionalElementAccessListInAnOperand` (`x &&⏎(c?[1,⏎2] ?? false) && y` → `x⏎&& (c?[1, 2] ?? false) && y`) | FMT-BREAK-014; FC-032 | `VerifyCodeFixWithEndOfLine`, LF and CRLF | fails |
| `RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyFixJoinsConditionalElementAccessListInABranch` (`x ?⏎(c?[1,⏎2] ?? 0) : 1` → `x⏎? (c?[1, 2] ?? 0)⏎: 1`) | FMT-BREAK-014; FC-032 | `VerifyCodeFixWithEndOfLine`, LF and CRLF | fails |

The four code-fix outputs were derived from `scripts/apply-fix.sh` on the `d[1,⏎2]` analog in the same fixture on this head (RH5101 `N(d[1, 2] + 0, 3);`, RH5111 `b = d[1, 2] + 0;`, RH5302 `x⏎&& d[1, 2] && y;`, RH5308 `x⏎? d[1, 2]⏎: 1;`); each test's failure diff shows only the list staying wrapped (RH5101 additionally splits `3` onto its own line today, as the skeptic observed).

### Existing tests that assert a changed output

`rg --pcre2 '\?\[[^\]\n"]*$'` over `src` (all test projects, code-fix tests included) finds four wrapped `?[`: `ListElementAttributeLayoutSinglePassTests:425` (changed, above); `RH5307…AnalyzerTests` `n?[{|RH5307:m[1,⏎2]|}]` (two places, kept: the outer binding's only argument spans lines, RH5307-002/FMT-BREAK-014 keep it); `RH5307…AnalyzerTests.VerifyNullConditionalElementAccessIsNotReported` (removed, see #1878); `RH5038…AnalyzerTests.VerifyCommentInConditionalElementAccessIsReported` (analyzer only, comment in the list, kept). No non-`.cs` fixture holds the shape (skeptic's check). `ChainLinkLayoutTests` and `UnsupportedAttachedPartCommentTests` hold only one-line `?[0]` lists: unchanged.

### Not covered, and why

- `FormatNode` on the bracketed list of an element access or element binding: I wrote both, and both fail on this head for the element-access form too (`FormatNode` returns `        [1,⏎         2]`); no caller formats a list in a tree through `FormatNode` — the RH5307 fix uses `FormatNodeInDocumentAsync` (covered above), code fixes use `FormatNode` only for detached nodes. Dropped as unreachable (`AGENTS.md`, *Supported inputs*).
- Comment with `//` closing bracket on its own line (`c?[1, 2 // note⏎]`): the comment-between-brackets side is covered by `//` (spec example), `/* */`, and a directive; one more comment position adds no condition.
- FC-004/FC-005: the join removes only whitespace gaps between `[` and `]`, guarded by the existing interior guard; the comment and directive tests above are both sides.
- FC-006, FC-018, FC-022–FC-026, FC-029, FC-036: no rename, literal, member-kind, or key/value change involved.
- FC-011: no loop added. FC-039–FC-043: no shape declared or changed; the one FMT-UNSUP-014 test checks the existing refusal with a wrapped list.
- CLI end-to-end: the CLI formats through the same whole-document entry point `AssertRuleResult` uses; no CLI option changes.

### Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ConditionalElementAccessArgumentListTests|FullyQualifiedName~ConditionalElementAccessJoinsWrappedArgumentsWhenNestedIndexerCollapses|FullyQualifiedName~ConditionalElementAccessArgumentListTargetGetsItsArgumentsCollapsed"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~VerifyFixJoinsConditionalElementAccessList|FullyQualifiedName~RH5307IndexerBracketedArgumentsShouldBeSingleLinedFormatterTests"
```

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1878

### Tests

All in `src/Reihitsu.Analyzer.Test/Formatting/RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzerTests.cs`. `Verify(source)` is analyzer-only; `VerifyCodeFixWithEndOfLine` runs the Roslyn code-fix verifier (re-analysis, iterations, Fix All) and the fix on the plain source, under LF and CRLF via `DataRow`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyConditionalAndNullForgivingElementAccessListsAreReportedAlike` (spec example of RH5307-001: `a`, `b`, `f`, `j`, `k`) | RH5307-001, RH5307-003; FC-019, FC-021 | `Verify(source)` | fails (1 of 5 reported: only `d![…]`) |
| `VerifyConditionalElementAccessWithCommentBetweenBracketsIsNotReported` (spec negative example) | RH5307-001, RH5307-002 | `Verify(source)` | passes |
| `VerifyConditionalElementAccessWithDirectiveBetweenBracketsIsNotReported` | RH5307-002; FC-019, FC-020 | `Verify(source)` | passes |
| `VerifyConditionalElementAccessTheFormatterKeepsIsNotReported` (multi-line argument; one-line `c?[1, 2]`) | RH5307-001, RH5307-002; FC-003, FC-013 | `Verify(source)` | passes |
| `VerifyFixJoinsConditionalElementAccessListsAndKeepsTheCodeAround` (spec example of RH5307-004: `?? 0`, `e?[0,⏎1][1,⏎2]` with two overlapping diagnostics, trailing `// note`) | RH5307-001, RH5307-003, RH5307-004, SHARED-FIX-006; FC-007, FC-008, FC-030 | `VerifyCodeFixWithEndOfLine` | fails |
| `VerifyFixJoinsConditionalElementAccessList` | RH5307-001, RH5307-003, RH5307-004; FC-007 | `VerifyCodeFixWithEndOfLine` | fails |
| `VerifyFixJoinsConditionalElementAccessListWithOpeningBracketAlone` | RH5307-001, RH5307-004 | `VerifyCodeFixWithEndOfLine` | fails |
| `VerifyFixJoinsConditionalElementAccessListBehindNullForgivingOperator` (`c!?[`) | RH5307-001, RH5307-004 | `VerifyCodeFixWithEndOfLine` | fails |
| `VerifyFixJoinsElementAccessListBehindNullForgivingOperator` (`d![`, the form the issue's text lost its `!` on) | RH5307-001, RH5307-004 | `VerifyCodeFixWithEndOfLine` | passes |
| `VerifyFixJoinsConditionalElementAccessListBehindMemberBinding` (`o?.Q?[`) | RH5307-001, RH5307-004 | `VerifyCodeFixWithEndOfLine` | fails |
| `VerifyFixJoinsConditionalElementAccessListFollowedByMemberAccess` (`.ToString()` stays) | RH5307-001, RH5307-003, RH5307-004; FC-030 | `VerifyCodeFixWithEndOfLine` | fails |
| `VerifyFixesJoinTheListsOfTwoConditionalElementAccesses` (`e?[0,⏎1]?[1,⏎2]`, two diagnostics, Fix All) | RH5307-001, RH5307-003, RH5307-004; FC-008 | `VerifyCodeFixWithEndOfLine` | fails |
| `VerifyFixJoinsConditionalElementAccessListWithCommentInFrontOfQuestionMark` (`c /* c */ ?[` → `c /* c */ ?[1, 2]`) | RH5307-001, RH5307-004; FC-021, FC-043 | `VerifyCodeFixWithEndOfLine` | fails |
| `VerifyFixJoinsConditionalElementAccessListWithCommentBehindQuestionMark` (`c? /* c */ [` → `c? /* c */ [1, 2]`) | RH5307-001, RH5307-004; FC-021, FC-043 | `VerifyCodeFixWithEndOfLine` | fails |
| `VerifyFixJoinsConditionalElementAccessListBehindCommentLine` (`c?⏎// c⏎[1,⏎2]` → the two lines in front of `[` as written, `[1, 2]`) | RH5307-001, RH5307-004; FC-021, FC-041, FC-043 | `VerifyCodeFixWithEndOfLine` | fails |
| `VerifyEachFixJoinsItsOwnListWhenTheReceiverIsAnElementBinding` (`e?[0,⏎1][1,⏎2]`: each fix alone joins only its list, the other fix behind it joins both, both orders) | RH5307-004; FC-008, FC-030 | `TryApplyCodeFixAtAsync`, `ApplyCodeFixAsync` | fails (element-access fix as specified; then no diagnostic left on the binding) |
| `VerifyFixAndIndexAccessFixConvergeInEitherOrder` (`c?⏎[1,⏎2]`: RH5307 then RH5115, and RH5115 then RH5307, both end at `c?[1, 2]`) | RH5307-001, RH5307-004; FC-012 | `Verify` + private `VerifyIndexAccessFix` (RH5115 verifier, as `CodeFixInteractionTests` does) | fails at the first RH5307 step |

The fix results for the three comment-around-`?` tests are what RH5307-004 and SHARED-FIX-006 give (only the list changes, the `[` keeps its line), with the fix offered because the list the fix formats does not hold the `?` (SHARED-FIX-010, FMT-UNSUP-003), as the skeptic determined; the element-access analogs `d⏎// c⏎[1,⏎2]` and `d /* c */ [1,⏎2]` give exactly this result on this head (`scripts/apply-fix.sh RH5307`). The RH5115 steps of the interplay test were checked separately with `scripts/apply-fix.sh RH5115` on this head: `c?⏎[1,⏎2]` → `c?[1,⏎             2]` (the `2` keeps its column), `c?⏎[1, 2]` → `c?[1, 2]`.

Changed in the file:

- Removed `VerifyNullConditionalElementAccessIsNotReported` (asserted the exclusion RH5307-001 no longer has; superseded by the tests above).
- `VerifyFixJoinsListBehindAnElementBinding`: summary only — "although the list of the element binding is not" now reads "while the list of the element binding, on one line, is not" (FC-031); assertions unchanged.
- Added `using System.Threading`, `Microsoft.CodeAnalysis.Testing`, `Reihitsu.Analyzer.Test.Verifiers` and a `#region Methods` with the private `VerifyIndexAccessFix` helper.

### Existing tests that assert a changed output

Searched for every RH5307 test with `?[`: `VerifyFixJoinsListBehindAnElementBinding` (`c?{|RH5307:[0][1,⏎2]|}`) and `VerifyFixesJoinEveryReportedListWhateverItsReceiverOrContainer` (`ve`, `vh`) keep their expectations (one-line binding list; outer binding with a multi-line argument). The `GetFixAllScenario` holds no `?[`. No other analyzer's tests report on the list.

### Not covered, and why

- `c!?[` and `o?.Q?[` dispatch: the analyzer registers one action for `SyntaxKind.BracketedArgumentList` and decides by the list's parent kind; what stands in front of the `?` does not reach that decision, but each form still has its own test since the location and fix target are resolved per span.
- Rule page: `AnalyzerPackageMetadataTests` keeps the docs in sync; no test added.
- FC-009: the comment-around-`?` tests verify the offered fix changes the document; a withheld case does not arise in the spec.

### Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzerTests"
```

### Follow-up candidates

_None._

## Whole run

- Failing on this head, each checked to fail on the join or the report this change adds: `--project formatter` 18 tests (16 in the new class, the flipped single-pass test, the detached-root target test); `--project analyzer` 34 rows (25 RH5307 rows, 8 rows of the RH5101/RH5111/RH5302/RH5308 tests, the parity test). Every other new or touched test passes.
- `scripts/test.sh --project architecture`: 198 passed.
- `scripts/format.sh` run over every changed test file.
