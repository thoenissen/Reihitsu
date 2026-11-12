Role: test implementer
Commit: (this commit; parent fb02ca4)
Verdict: tests written

Scope: decision #33 (`00-lead.md`). The disputed test from 27-implementer becomes a test that RH5029 reports the diagnostic and offers no code fix, as SHARED-STMT-004 at 9c87631 requires. My prompt carried the change asked for, which is the lead's decision itself; it carried no suspected cause and no hoped-for verdict. No production code was touched.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests.VerifyNoCodeFixWhenWholeSwitchStatementStandsOnOneLineInsideInitializer` (renamed from `VerifyDiagnosticDoesNotCompensateWhenSwitchLabelItselfIsNotFirstOnItsLine`) | SHARED-STMT-004 | `VerifyNoCodeFixIsOffered` | pass (passes on 663da84) |

What changed in the test: the input is unchanged (a whole `switch (value) { case 0: Method(1); var y = 1; break; }` on one line inside a lambda in an object initializer). The markup became `{|RH5029:var|}`, the expected fixed source was removed, and the assertion is now `VerifyNoCodeFixIsOffered(testCode)`. The summary no longer says that no requirement decides the shape; it states SHARED-STMT-004's reason: `switch` starts the anchor line, and it is neither the first token of a statement of the section's list nor a label of the switch. Tagged `[Requirement("SHARED-STMT-004")]`.

Red-before evidence: the previous version of this test, which asserted that the fix is offered, passed on 30237c4 (27-implementer reports it as the only test that turned red with 663da84). So the new assertion fails on 30237c4 because a fix is offered there. I did not run the old production code myself.

## Scenarios (bug fix)
_N/A_ — no new reproduction; this round rewrites one existing test.

## Not covered, and why
- No CRLF variant: the refusal is decided by which token starts the anchor line, which carries no line ending; the sibling no-fix tests of SHARED-STMT-004 in this class run LF only as well.
- No Fix All test: no fix is offered, so there is nothing to apply.
- Neighbouring cells of the same axis (label continuation lines, comment-led lines) are already covered by 25-test-implementer's tests.

## Commands
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests"`: Failed: 0, Passed: 47.
- `scripts/format.sh src/Reihitsu.Analyzer.Test/Formatting/RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests.cs`: Formatted 0 of 1 file(s).
