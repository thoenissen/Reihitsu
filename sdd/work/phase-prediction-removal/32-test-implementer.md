Role: test implementer
Commit: 7bf7c46
Verdict: tests written

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. It asked me to enumerate the class that `31-implementer.md` left untested and to cover it with exact output, through the public API and through the fix that reaches it. The class is a target of `FormatNodeInDocumentWithContextAsync` that starts its line, whose leading trivia is now the phases' trivia with the document's blank lines put back.

Expected values:
- Columns: what FMT-PIPE-017 requires, the columns the indentation phase gives the code. I took them from `scripts/format.sh` on the same inputs.
- Blank lines: the document's own. FMT-PIPE-010 keeps the rest of the document as it is; SHARED-FIX-006 forbids a fix to change the blank lines between the node it changes and its neighbours.

Every new test passes on 7bf7c46. Each one also fails on an earlier state of the entry point, shown under *Falsification*.

## Tests

### The class, axis by axis

| Axis | Members |
|---|---|
| Blank lines in the leading trivia | none where the phases add one (above a comment, a `///` comment, or a `#region`); two where the phases keep one (excessive-line collapse); one that the phases keep (behind a conditional directive, between two comments) |
| What stands in the trivia | nothing; one `//` comment; `/* */` after a blank line after `//` (several comments); a `///` comment of three lines; `#if` (active branch) followed by a blank line; `#region` |
| Target kind and context | enum member in its enum; parameter in its parameter list; declarator in its declaration statement |
| Line ending | LF; CRLF |

### Public API: `Reihitsu.Formatter.Test.Unit.ReihitsuFormatterNodeColumnTests` (new tests)

Each test calls `FormatNodeInDocumentWithContextAsync` under LF and CRLF, with a second pass that must change nothing. The target is the member `B` with the enum as context, or the last parameter with its parameter list as context.

| Test | Input → expected | Requirement / failure class |
|---|---|---|
| `EnumMemberTargetBehindTwoBlankLinesKeepsBothBlankLinesAndTakesThePhaseColumn` | two blank lines, `B` at 2 → both blank lines kept, `B` at 4 (document-level formatting keeps one) | FMT-PIPE-017 |
| `EnumMemberTargetBehindCommentLineKeepsTheMissingBlankLineAndTakesThePhaseColumns` | `// b` at 6, `B` at 2 → no blank line, both at 4 (document-level formatting adds one) | FMT-PIPE-017 |
| `EnumMemberTargetBehindConditionalDirectiveAndBlankLineKeepsBoth` | `  #if !DEBUG`, a blank line, `B` at 6 → `#if` at 0, blank line kept, `B` at 4 | FMT-PIPE-017; FC-005, FC-020 |
| `EnumMemberTargetBehindRegionDirectiveKeepsTheMissingBlankLinesAndTakesThePhaseColumns` | `  #region R`, `  B` → `#region R` and `B` at 4, no blank line. The `#endregion` belongs to the next member and stays where it is | FMT-PIPE-017; FC-020 |
| `EnumMemberTargetBehindDocumentationCommentKeepsTheMissingBlankLineAndTakesThePhaseColumns` | a three-line `///` at 6, `B` at 2 → every line at 4, no blank line | FMT-PIPE-017; FC-021 |
| `EnumMemberTargetBehindTwoBlankLinesAndDocumentationCommentKeepsBothBlankLines` | two blank lines, then the same `///` → both kept | FMT-PIPE-017 |
| `EnumMemberTargetBehindSeveralCommentsKeepsTheDocumentsBlankLinesAndTakesThePhaseColumns` | `// b` at 6, blank, `/* c */` at 2, `B` at 6 → no blank above `// b`, the blank between the comments kept, all at 4 | FMT-PIPE-017; FC-019 |
| `ParameterTargetBehindTwoBlankLinesAndCommentKeepsBothBlankLinesAndTakesThePhaseColumns` | two blank lines, `// b` at 6, `int b` at 15 → both kept, comment and parameter at 11 (document-level formatting keeps one) | FMT-PIPE-017 |

These tests carry only FMT-PIPE-017 (see *Spec observation*).

### Through the RH5111 fix: `Reihitsu.Analyzer.Test.Formatting.RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests` (new tests)

These use `Verify(source, fixedSource, diagnostics)` with exact output. The fix formats the enum member, parameter, or declarator with the enum, parameter list, or statement as context. Requirements: RH5111-004, SHARED-FIX-006, FMT-PIPE-017.

| Test | Input → expected |
|---|---|
| `VerifyEnumMemberBehindTwoBlankLinesKeepsBothBlankLines` | `Second =` / `2` at 2 behind two blank lines → `Second = 2,` at 4, both blank lines kept |
| `VerifyEnumMemberBehindConditionalDirectiveKeepsTheBlankLineAndTakesTheColumns` | `  #if !DEBUG`, blank, member at 6 → `#if` at 0, blank kept, member at 4 |
| `VerifyEnumMemberBehindRegionDirectiveAddsNoBlankLineAndTakesTheColumns` | `  #region Values`, member at 2 → both at 4, no blank line |
| `VerifyEnumMemberBehindDocumentationCommentAddsNoBlankLineAndTakesTheColumns` | three-line `///` at 6, member at 2 → all at 4, no blank line |
| `VerifyEnumMemberBehindSeveralCommentsKeepsTheBlankLineBetweenThem` | `// b`, blank, `/* c */`, member → all at 4, only the blank between the comments |
| `VerifyEnumMemberBehindSeveralCommentsKeepsTheBlankLineBetweenThemWithCrLf` | the same under CRLF (`NormalizeToCarriageReturnLineFeed`) |
| `VerifyParameterBehindTwoBlankLinesAndCommentKeepsBothBlankLines` | two blank lines, `// second`, `int second =` / `1` at 20 → both kept, comment and `int second = 1)` at 16 |
| `VerifyDeclaratorBehindBlankLineKeepsTheBlankLine` | `int first = 1,`, blank, `second =` / `2` at 14 → blank kept, `second = 2;` at 8 (the declarator column of document-level formatting) |

The two RH5111 tests from round 31, `VerifyCommentInFrontOfEnumMemberDoesNotWithholdTheFix` and `…Parameter…`, cover the single-comment member through the fix and stay unchanged.

### Falsification

I ran these tests in temporary worktrees, removed afterwards, against two earlier states of the context entry point:

| State | Formatter tests failing | RH5111 tests failing |
|---|---|---|
| 8316e8a: phases' trivia written back without the document's blank lines | 7 of the 8 new tests. The directive member passes, because the phases keep the blank line behind `#if`. | 8: six new ones, plus the two round-31 comment tests (which still assert the document's blank lines). Not failing: the directive and declarator members, whose blank lines the phases keep. |
| 1f3c79d: original trivia restored, so the target's first line keeps its input column | all 8 new tests, plus the two #8(b) tests | all 8 new tests, plus the three #8 tests |

So every new test fails for one of the two halves of the requirement: columns from the phases, or blank lines from the document.

## Spec observation

The blank-line behavior of `FormatNodeInDocumentWithContextAsync` for a target that starts its line is not stated in any formatter requirement:
- FMT-PIPE-010 says when the leading trivia is kept, and this target is not one of those cases.
- FMT-PIPE-017 decides columns only.
- SHARED-FIX-006 binds the fixes.

So the public-API tests assert the implementation's behavior, which the RH5111 tests derive from SHARED-FIX-006, but they tag only FMT-PIPE-017. This is for the spec checker.

## Not covered, and why

- **The hint in `28-auditor.md` about a `///` behind the preceding token** (a target that starts the context and carries such a comment): not one of this class's members, because the target here starts a later line of the context. Still unprobed.
- **A directive in the inactive branch in front of the target**: the target would be disabled text, which no fix reaches. My first fixture, `#if DEBUG`, showed that, so I used `#if !DEBUG`.
- **Kind and order of the trivia items changing between the original and the formatted trivia** (the fallback in `RestoreBlankLines`): the phases keep every comment and directive, so no input reaches it (`31-implementer.md`).
- **Failure classes**:
  - FC-004: no join across these gaps.
  - FC-006, FC-018, FC-022–FC-025, FC-029, FC-036: no rewrite or rename.
  - FC-008: Fix All interaction is unchanged by this class.
  - FC-014: no new rule.
  - FC-031, FC-032, FC-037: for the auditor.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterNodeColumnTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5111"
scripts/test.sh --project architecture
```

On 7bf7c46 plus these files:
- formatter `ReihitsuFormatterNodeColumnTests`: `Passed: 20`;
- analyzer `RH5111`: `Passed: 61`;
- architecture: `Passed: 89`.
