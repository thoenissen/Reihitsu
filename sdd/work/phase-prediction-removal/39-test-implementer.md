Role: test implementer
Commit: 5a5c708
Verdict: tests written

This is the final round under decision audit 3, option B (`00-lead.md`, last section).

**About the prompt.**
- It carried no suspected cause and no verdict anyone hoped for.
- It did name the revert of the context entry point. That revert is the recorded decision, not a hint, and I took no expected value from it.

**Expected values.** They come from the narrowed specs (`git diff 714c199 -- sdd AGENTS.md`):
- FMT-PIPE-018 and FMT-PIPE-010: the target's leading trivia is written back as it stands, in every case. This covers blank lines, comments and directives in their columns, and the whitespace in front of the first token, including tabs.
- FMT-PIPE-017: the target's other lines take the phases' columns. A target that starts the first line of its context is laid out from the characters in front of it, each counted as one column. The gap example is given verbatim.

**Cross-check.** After I wrote every expected value by hand, I checked it in a temporary worktree, since removed: HEAD with only `ReihitsuFormatter.cs` taken from 504310f, which is the state the decision returns to. All 46 tests of the three classes pass there.

"Fails" means the test fails on 5a5c708 plus these files. I read every failure: each fails on the difference the test is about (column, blank line, inserted whitespace, or rewritten tabs), never on a compile or markup error. Columns are 0-based.

## Tests

### Public API: `Reihitsu.Formatter.Test.Unit.ReihitsuFormatterNodeColumnTests`

Every test calls `FormatNodeInDocumentWithContextAsync` under LF and CRLF, with a second pass that must change nothing (`AssertFormatsTargetWithContext`).

Most inputs wrap `B =` / `2`, so the entry point visibly formats the target while its trivia stays.

**Replaced: they asserted the #8 repair.**

| Test | Input → expected | Requirement / failure class | Expected now |
|---|---|---|---|
| `ArgumentTargetOnALaterLineOfItsContextKeepsItsFirstLineColumnAndLeavesTheAlignedLineInThePhaseLayout` (was `…IsWrittenInThePhaseColumnWithTheLinesAlignedToIt`) | `Call(1,` / `Other(2,` at 17 / `3));` → `Other` at 17, `3` at 19. This is the recorded gap (28-auditor #8b). | none: characterizes the gap of FMT-PIPE-017, deliberately untagged | fails: `Other` at 13 |
| `OneLineArgumentTargetOnALaterLineOfItsContextKeepsTheWhitespaceInFrontOfIt` (was `…IsWrittenInThePhaseColumn`) | `2  +  3` at 17 → `2 + 3` at 17 | FMT-PIPE-018 | fails: 13 |
| `EnumMemberTargetBehindTwoBlankLinesKeepsBothBlankLinesAndItsColumn` | two blank lines, `B` at 2 → both kept, `B = 2,` at 2 | FMT-PIPE-018 | fails: `B` at 4 |
| `EnumMemberTargetBehindCommentLineKeepsTheCommentAndTheColumnsAsInTheDocument` | `// b` at 6, `B` at 2 → unchanged columns, no blank line | FMT-PIPE-018 | fails: 4/4 |
| `EnumMemberTargetBehindConditionalDirectiveAndBlankLineKeepsBothAsInTheDocument` | `#if !DEBUG` at 2, blank, `B` at 6 → as written | FMT-PIPE-018; FC-005, FC-020 | fails: `#if` at 0, `B` at 4 |
| `EnumMemberTargetBehindRegionDirectiveKeepsTheDirectiveAndTheColumnsAsInTheDocument` | `#region R` and `B` at 2 → as written | FMT-PIPE-018; FC-020 | fails |
| `EnumMemberTargetBehindDocumentationCommentKeepsTheCommentAndTheColumnsAsInTheDocument` | three-line `///` at 6, `B` at 2 → as written | FMT-PIPE-018; FC-021 | fails |
| `EnumMemberTargetBehindTwoBlankLinesAndDocumentationCommentKeepsThemAsInTheDocument` | two blank lines + the same `///` → as written | FMT-PIPE-018 | fails |
| `EnumMemberTargetBehindSeveralCommentsKeepsThemAsInTheDocument` | `// b` at 6, blank, `/* c */` at 2, `B` at 6 → as written | FMT-PIPE-018; FC-019 | fails |
| `ParameterTargetBehindTwoBlankLinesAndCommentKeepsThemAsInTheDocument` | two blank lines, `// b` at 6, `int b =` / `1)` at 15 → `int b = 1)` at 15, the rest as written | FMT-PIPE-018 | fails |
| `SpecExampleTwoBlankLinesAboveEnumMemberStay` | FMT-PIPE-018 first example, verbatim | FMT-PIPE-018 | fails |
| `SpecExampleNoBlankLineIsAddedAboveCommentInFrontOfEnumMember` | FMT-PIPE-018 second example, verbatim | FMT-PIPE-018 | fails |

The FMT-PIPE-017 tag is removed from the ten FMT-PIPE-018 tests:
- after the join, each target is one line;
- that line is outside FMT-PIPE-017 (kept trivia and the gap).

**New: the auditor's #11 members and the other side of the gap.**

| Test | Input → expected | Requirement / failure class | Expected now |
|---|---|---|---|
| `EnumMemberTargetWithDocumentationCommentBehindThePreviousMemberKeepsThatLineAsWritten` | `A, /// Doc` / `B =` / `2,` → `A, /// Doc` / `B = 2,` | FMT-PIPE-010, FMT-PIPE-018; FC-021, FC-028 | fails: 4 spaces inserted before `///` |
| `ParameterTargetWithDocumentationCommentBehindThePreviousParameterKeepsThatLineAsWritten` | `void M(int a, /// Doc` / `int b =` / `1)` → first line as written, `int b = 1)` at 11 | FMT-PIPE-010, FMT-PIPE-018 | fails: whitespace inserted |
| `ChainTargetWithDocumentationCommentBehindTheOpeningBraceKeepsThatLineAsWritten` | the chain with the block as context; `{ /// Doc` / `a.Trim()` / `.ToUpper();` → `{ /// Doc` kept, `.ToUpper()` at 9 | FMT-PIPE-010, FMT-PIPE-018 | fails: 8 spaces inserted |
| `ChainTargetWithDocumentationCommentBehindThePreviousStatementKeepsThatLineAsWritten` | the same behind `var x = 1; /// Doc` | FMT-PIPE-010, FMT-PIPE-018 | fails: 8 spaces inserted |
| `DeclaratorTargetOnALaterLineOfATabIndentedContextKeepsTheWhitespaceInFrontOfIt` | `→→int a = 1,` / `→→    b =` / `→→        2;` → `→→    b = 2;` (37-auditor hint) | FMT-PIPE-018; FC-033 | fails: `  b = 2;` |
| `TargetStartingTheFirstLineOfItsContextIsLaidOutFromTheWhitespaceInFrontOfIt` | invocation target, statement context, `Call(1,` at 5 / `2);` → `2` at 10 | FMT-PIPE-017 (the other side of the gap); FC-002, FC-003 | passes |
| `TargetStartingTheFirstLineOfItsContextCountsEachTabInFrontOfItAsOneColumn` | the same with `→→Call(1,` → tabs kept, `2` at 7 | FMT-PIPE-017 (every character in front of the context's first token counts as one column), FMT-PIPE-018; FC-033 | fails: tabs replaced, `Call` at 8, `2` at 13 |

**Unchanged and still passing.** `DirectChildOfRootTargetWithContextKeepsItsLeadingTriviaAsItStands` (FMT-PIPE-018, FMT-PIPE-010), the `FormatNodeInDocumentAsync` tests, and the mid-line chain test.

The class summary now names the kept-trivia exception.

### Through the RH5111 fix: `Reihitsu.Analyzer.Test.Formatting.RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests`

All tests use `Verify(source, fixedSource, diagnostics)` with exact output. The enum, parameter, and declarator fixes run through the context entry point.

| Test | Input → expected | Requirement / failure class | Expected now |
|---|---|---|---|
| `VerifyDeclaratorOnALaterLineKeepsItsFirstLineColumnAndLeavesTheWrappedArgumentInTheFormattersLayout` (was `VerifyDeclaratorOnALaterLineIsFixedAsTheFormatterLaysItOut`) | The FMT-PIPE-017 gap example, verbatim: `b = Other(1,` at 14, `2);` at 18 | none: characterizes the gap, deliberately untagged (see *Spec observations*) | fails: `b` at 8 |
| `VerifyParameterOnALaterLineIsJoinedInItsColumn` (was `…IsFixedAsTheFormatterLaysItOut`) | `int b = 5)` at 15 | RH5111-004, FMT-PIPE-018 | fails: 11 |
| `VerifyEnumMemberOnALaterLineIsJoinedInItsColumn` (was `…IsFixedAsTheFormatterLaysItOut`) | `B = 2` at 6 | RH5111-004, FMT-PIPE-018 | fails: 4 |
| `VerifyDeclaratorOnALaterLineOfATabIndentedDeclarationKeepsTheWhitespaceInFrontOfIt` (new) | `→→    b = 2;` | RH5111-004, FMT-PIPE-018; FC-033 | fails: `  b = 2;` |
| `VerifyEnumMemberWithDocumentationCommentBehindThePreviousMemberKeepsThatLineAsWritten` (new, #11) | `First, /// Doc` kept, `Second = 2,` at 4 | RH5111-004, FMT-PIPE-010, FMT-PIPE-018 | fails: 4 spaces inserted |
| `VerifyParameterWithDocumentationCommentBehindThePreviousParameterKeepsThatLineAsWritten` (new, #11) | `void Method(int first, /// Doc` kept, `int second = 1)` at 16 | RH5111-004, FMT-PIPE-010, FMT-PIPE-018 | fails: whitespace inserted |
| `VerifyEnumMemberBehindTwoBlankLinesKeepsBothBlankLines` | both kept, `Second = 2,` at 2 | RH5111-004, SHARED-FIX-006, FMT-PIPE-018 | fails: 4 |
| `VerifyEnumMemberBehindConditionalDirectiveKeepsTheDirectiveAndTheBlankLineAsInTheDocument` | `#if !DEBUG` at 2, blank, member at 6 | same | fails |
| `VerifyEnumMemberBehindRegionDirectiveAddsNoBlankLineAndKeepsTheColumns` | `#region Values` and member at 2 | same | fails |
| `VerifyEnumMemberBehindDocumentationCommentAddsNoBlankLineAndKeepsTheColumns` | `///` at 6, member at 2 | same | fails |
| `VerifyEnumMemberBehindSeveralCommentsKeepsThemAsInTheDocument` / `…WithCrLf` | `// b` at 6, blank, `/* c */` at 2, member at 6; LF and CRLF | same | fails |
| `VerifyParameterBehindTwoBlankLinesAndCommentKeepsThemAsInTheDocument` | both kept, `// second` at 6, `int second = 1)` at 20 | same | fails |
| `VerifyDeclaratorBehindBlankLineKeepsTheBlankLineAndItsColumn` | blank kept, `second = 2;` at 14 | same | fails: 8 |

**Tags removed.**
- FMT-PIPE-017, from the eight blank-line tests.
- RH5111-004 and FMT-PIPE-017, from the declarator gap test. In the gap output `2` no longer stands under `1`, which RH5111-004's "a value that spans several lines keeps its own layout" would not support.

`VerifyCommentInFrontOfEnumMemberDoesNotWithholdTheFix` and `…Parameter…` are unchanged. Their trivia already stands in the phase's columns, so they pass on both states and keep their FMT-PIPE-018 tag (no blank line is added above the comment).

### Through the RH5201 fix: `Reihitsu.Analyzer.Test.Formatting.RH5201MethodChainsShouldBeAlignedAnalyzerTests` (new, #11)

| Test | Input → expected | Requirement / failure class | Expected now |
|---|---|---|---|
| `VerifyChainWithDocumentationCommentBehindTheOpeningBraceKeepsThatLineAsWritten` | `{ /// Doc` / `a.Trim()` / `.ToUpper();` → the brace line as written, `.ToUpper()` at 9 | RH5201-005, FMT-PIPE-010, FMT-PIPE-018 | fails: `{         /// Doc` |
| `VerifyChainWithDocumentationCommentBehindThePreviousStatementKeepsThatLineAsWritten` | `var x = 1; /// Doc` / `a.Trim()` / `.ToUpper();` → the statement line as written, `.ToUpper()` at 9 | same | fails: `var x = 1;         /// Doc` |

The four failing #11 members also reproduce with `scripts/apply-fix.sh` on 5a5c708, under LF and CRLF. The outputs are those `37-auditor.md` reports: RH5111 enum member +4, parameter +16 in my fixture, RH5201 +8 twice.

## Spec observations

These are for the spec checker and the lead. I changed no spec.

- **SHARED-FIX-007 is not narrowed for the gap.** It says "formatting the result changes nothing in that code". Several outputs that FMT-PIPE-018 and the FMT-PIPE-017 gap now require are moved by document-level formatting:
  - the declarator gap example (`b` 14 → 8, `2` 18 → 14);
  - the one-line later-line members (the parameter at 15 → 11, the enum member at 6 → 4, the declarator at 14 → 8);
  - the `///` behind the previous member, which document-level formatting moves onto its own line.

  No test tags SHARED-FIX-007 for them. The change doc says the output stays as it is on `main`.
- **RH5111-004's "A value that spans several lines keeps its own layout from its first line on"** does not hold for the gap example. That test is therefore untagged.

## Not covered, and why

- **The members the auditor reports as holding.** Each is covered by an existing test, or offers no fix:
  - `/* c */` behind the previous member: it is that token's trailing trivia, not the target's;
  - the declarator behind `int a = 1, /// Doc`: no fix is offered;
  - every `FormatNodeInDocumentAsync` shape: the existing `ReihitsuFormatterNodeColumnTests` and the RH5111/RH5201 suites.
- **CRLF of the RH5111/RH5201 members**: the analyzer tests run one line ending each, as in this class. The several-comments member has a CRLF twin, and every public-API test of the same mechanism runs LF and CRLF.
- **Failure classes**:
  - FC-004: no join across these gaps;
  - FC-006, FC-018, FC-022–FC-025, FC-029, FC-036: no rewrite or rename;
  - FC-008, FC-009: no Fix All change and no newly withheld fix;
  - FC-010–FC-012: a second pass in every public-API test;
  - FC-014: no new rule;
  - FC-031, FC-032, FC-037: for the cleaner and the auditor;
  - FC-035: expected values come from the specs, and the reverted tree only confirmed them.
- **Tests outside these classes.** Whole suites on the cross-check tree with these files:
  - formatter `Passed: 3002, Failed: 0`;
  - analyzer `Passed: 6242, Failed: 0`.

  So no other test asserts the #8 repair's behavior.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterNodeColumnTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5111|FullyQualifiedName~RH5201"
scripts/test.sh --project architecture
```

On 5a5c708 plus these files (Debug):
- formatter `ReihitsuFormatterNodeColumnTests`: `Failed: 18, Passed: 12, Total: 30`. Failing:
  - the 12 replaced tests;
  - the four #11 public-API tests;
  - the two tab tests (`DeclaratorTargetOnALaterLineOfATabIndentedContext…` and `TargetStartingTheFirstLineOfItsContextCountsEachTab…`).
- analyzer `RH5111|RH5201`: `Failed: 16, Passed: 133, Total: 149`. Failing:
  - the 13 replaced or new RH5111 later-line and blank-line tests;
  - `VerifyDeclaratorOnALaterLineOfATabIndented…`;
  - the two RH5201 #11 tests.
- architecture: `Passed: 89`.
