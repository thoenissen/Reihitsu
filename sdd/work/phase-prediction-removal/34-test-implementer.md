Role: test implementer
Commit: ae9039c
Verdict: tests written

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. It asked me to:
- check that each test `33-spec-author.md` lists for FMT-PIPE-018 asserts what the requirement states;
- tag the ones that do;
- cover the requirement's examples if no test pins them;
- run the architecture project.

## Check of the listed tests

FMT-PIPE-018 states, for a target of `FormatNodeInDocumentWithContextAsync` that starts its line and whose leading trivia FMT-PIPE-010 does not keep:
1. every blank line of the document above the target and between its comments and directives stays;
2. no blank line is added;
3. the comments, the directives, and the first token take the indentation phase's columns.

| Test | Target and context starting its line? | Asserts (1)–(3) with exact output? | Tagged |
|---|---|---|---|
| `ReihitsuFormatterNodeColumnTests.EnumMemberTargetBehindTwoBlankLinesKeepsBothBlankLinesAndTakesThePhaseColumn` | yes; enum member in its enum | yes: two blank lines that document-level formatting collapses stay; `B` 2 → 4 | FMT-PIPE-018 |
| `…EnumMemberTargetBehindCommentLineKeepsTheMissingBlankLineAndTakesThePhaseColumns` | yes | yes: no blank line added above `// b`; both end at 4 | FMT-PIPE-018 |
| `…EnumMemberTargetBehindConditionalDirectiveAndBlankLineKeepsBoth` | yes | yes: the blank line behind `#if` stays; `#if` at 0 (FMT-INDENT-009); `B` at 4 | FMT-PIPE-018 |
| `…EnumMemberTargetBehindRegionDirectiveKeepsTheMissingBlankLinesAndTakesThePhaseColumns` | yes | yes: none of the blank lines FMT-BLANK-013 adds around the region; `#region` and `B` at 4 | FMT-PIPE-018 |
| `…EnumMemberTargetBehindDocumentationCommentKeepsTheMissingBlankLineAndTakesThePhaseColumns` | yes | yes: no blank line added above `///`; every line at 4 | FMT-PIPE-018 |
| `…EnumMemberTargetBehindTwoBlankLinesAndDocumentationCommentKeepsBothBlankLines` | yes | yes | FMT-PIPE-018 |
| `…EnumMemberTargetBehindSeveralCommentsKeepsTheDocumentsBlankLinesAndTakesThePhaseColumns` | yes | yes: the blank line between the comments stays, and none is added above the first | FMT-PIPE-018 |
| `…ParameterTargetBehindTwoBlankLinesAndCommentKeepsBothBlankLinesAndTakesThePhaseColumns` | yes; parameter in its list | yes | FMT-PIPE-018 |
| RH5111: `VerifyEnumMemberBehindTwoBlankLinesKeepsBothBlankLines`, `…BehindConditionalDirective…`, `…BehindRegionDirective…`, `…BehindDocumentationComment…`, `…BehindSeveralComments…`, `…BehindSeveralComments…WithCrLf`, `VerifyParameterBehindTwoBlankLinesAndCommentKeepsBothBlankLines`, `VerifyDeclaratorBehindBlankLineKeepsTheBlankLine` | yes; the RH5111 fix formats the member, parameter, or declarator with the enum, parameter list, or statement as context | yes: exact fixed source through the verifier | FMT-PIPE-018 |
| RH5111: `VerifyCommentInFrontOfEnumMemberDoesNotWithholdTheFix`, `VerifyCommentInFrontOfParameterDoesNotWithholdTheFix` | yes | yes: no blank line is added above `// second` (document-level formatting adds one), and the columns stay the phase's | FMT-PIPE-018 |

All 18 listed tests assert what FMT-PIPE-018 states, so all 18 are tagged. The tags they already had stay.

## Tests

New tests in `Reihitsu.Formatter.Test.Unit.ReihitsuFormatterNodeColumnTests`. They call `FormatNodeInDocumentWithContextAsync` under LF and CRLF, with a second pass that must change nothing.

| Test | Requirement / failure class | Expected now |
|---|---|---|
| `SpecExampleTwoBlankLinesAboveEnumMemberStay` (first example, verbatim) | FMT-PIPE-018, FMT-PIPE-017 | passes |
| `SpecExampleNoBlankLineIsAddedAboveCommentInFrontOfEnumMember` (second example, verbatim) | FMT-PIPE-018, FMT-PIPE-017 | passes |
| `DirectChildOfRootTargetWithContextKeepsItsLeadingTriviaAsItStands` (the other side of the qualifier, FC-002 and FC-003) | FMT-PIPE-018, FMT-PIPE-010 | passes |

The third test covers the other side of the qualifier. Its target is a type that is a direct child of the root, with the compilation unit as context. Its leading trivia (two blank lines, then `// c` and `class D` in column 2) is written back as it stands. The phases would collapse the blank lines and move both to column 0.

When I first wrote it, I expected the type's braces and member to follow `class D` in column 2. They do not. FMT-PIPE-017 shifts the formatted node, which here is the context at its depth with offset 0. The kept trivia is outside that requirement, so only the first line stays at 2. I corrected the expectation to what the specs state, and the input now has the braces at the depth column. The test asserts only the kept trivia.

The two spec-example tests are verbatim versions of the round-32 near-twins. Those near-twins fail on 8316e8a (`32-test-implementer.md`, *Falsification*), and the examples use the same mechanism. I did not falsify the other-side test separately: it pins existing behavior that the FMT-PIPE-010 condition decides, and that condition is unchanged since 8316e8a.

## Not covered, and why

- **The blank lines of `FormatNodeInDocumentAsync`**: FMT-PIPE-018 leaves them open (the spec author's Q1).
- **A delimited comment on the target's line, where the phases add or remove a line break**: the requirement says such a break follows the phases. No fix reaches a context target with a comment in front of it on its own line: RH5111-005 withholds the fix for a comment inside the construct, and the public API case needs a target behind a delimited comment that still starts its line, which is a contradiction. The `RestoreBlankLines` branch for gaps that do not start their line has no test (`31-implementer.md` notes it as code-only).

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterNodeColumnTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5111"
scripts/test.sh --project architecture
```

On ae9039c plus these files:
- formatter `ReihitsuFormatterNodeColumnTests`: `Passed: 23`;
- analyzer `RH5111`: `Passed: 61`;
- architecture: `Passed: 89`. This includes `EveryRequirementOfAnEnforcedSpecIsCovered`, which `33-spec-author.md` saw failing on FMT-PIPE-018.
