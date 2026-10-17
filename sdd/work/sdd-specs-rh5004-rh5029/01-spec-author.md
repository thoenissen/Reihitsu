Role: spec author
Commit: 375746b (production code unchanged since 33a29f7); spec commit cf18737
Verdict: spec written

No intended behavior changes, so there is no change doc: every requirement writes down what the code does today (lane "specs and tests for existing behavior"). No shared spec requirement was changed.

## What was written

- 16 new specs, one per rule, from `sdd/templates/analyzer-spec.md`, following RH5001/RH5003/RH5010: `sdd/specs/analyzer/RH5004.md`, `RH5005.md`, `RH5006.md`, `RH5007.md`, `RH5008.md`, `RH5009.md`, `RH5012.md`, `RH5013.md`, `RH5014.md`, `RH5015.md`, `RH5016.md`, `RH5017.md`, `RH5018.md`, `RH5019.md`, `RH5021.md`, `RH5029.md`.
- Every spec has `Uses` = SHARED-STMT-001, SHARED-STMT-002, SHARED-STMT-003, SHARED-STMT-004. The header rows (Layout, Warning, enabled, Fix All) match the descriptors and code-fix providers. Every provider derives from `StatementShouldBePrecededByABlankLineCodeFixProviderBase`, which returns `WellKnownFixAllProviders.BatchFixer`.
- 34 IDs added to `sdd/specs/requirement-ids.txt` in ordinal order. No existing line moved. 16 rows added to `sdd/specs/README.md`.
- Rule pages derived from the specs:
  - `documentation/rules/RH5019.md`: added the missing exception "directly follows another `yield return` or `yield break` statement" (RH5019-002).
  - `documentation/rules/RH5005.md`: one sentence saying a `using` declaration is a local declaration and RH5029 checks it (RH5005-001).
- `scripts/test.sh --project architecture`: 71 passed and 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, which lists exactly the 34 new IDs as uncovered. That is expected until the tests are tagged. `SpecConsistencyTests`, `SpecStructureTests` and `RuleDocumentationConventionTests` pass.

## How the statements were established

- Code read: every analyzer in scope (`GetPreviousToken`, `GetLocation`, `IsRelevant`), `StatementShouldBePrecededByABlankLineAnalyzerBase<TStatement>`, and `StatementShouldBePrecededByABlankLineCodeFixProviderBase`.
- Examples run: each example block was saved as a fixture with the markup stripped. `scripts/apply-fix.sh --no-install <ID> <dir>` ran under LF and CRLF, and the LF diff was patched back onto the fixture. The result was compared with the expected output, which is the input with one blank line inserted above each `{|RH####:…|}` line and nowhere else.
- Result: all 19 example blocks matched (RH5029 has one merged block). Every run says `fixed`, with no `not-converged` and no `line-ending-drift`. This also confirms the negative cases inside each block: unmarked statements received no blank line.
- What the runs do not prove: the exact diagnostic token. That comes from `GetLocation` in the code: the keyword for RH5004–RH5019, and `GetFirstToken()` for RH5021 and RH5029. The test markup has to pin it.
- No example uses `await using` or `await foreach` (thoenissen/Reihitsu#946), and no example uses a block comment on the statement's line (thoenissen/Reihitsu#972).

## Changed requirements

- RH5004-001 — A `do` statement that follows a statement needs a blank line — added
- RH5004-002 — The diagnostic is on the `do` keyword — added
- RH5005-001 — A `using` statement that follows a statement needs a blank line (a `using` declaration is not reported) — added
- RH5005-002 — The diagnostic is on the `using` keyword — added
- RH5006-001 — A `foreach` statement that follows a statement needs a blank line (deconstructing `foreach` included) — added
- RH5006-002 — The diagnostic is on the `foreach` keyword — added
- RH5007-001 — A `for` statement that follows a statement needs a blank line — added
- RH5007-002 — The diagnostic is on the `for` keyword — added
- RH5008-001 — A `return` statement that follows a statement needs a blank line (a `return` that ends a switch section is reported) — added
- RH5008-002 — The diagnostic is on the `return` keyword — added
- RH5009-001 — A `goto` statement that follows a statement needs a blank line (`goto` label / `goto case` / `goto default`; a `goto` that ends a switch section is reported) — added
- RH5009-002 — The diagnostic is on the `goto` keyword — added
- RH5012-001 — A `continue` statement that follows a statement needs a blank line — added
- RH5012-002 — The diagnostic is on the `continue` keyword — added
- RH5013-001 — A `throw` statement that follows a statement needs a blank line (a `throw` expression is not reported) — added
- RH5013-002 — The diagnostic is on the `throw` keyword — added
- RH5014-001 — A `switch` statement that follows a statement needs a blank line (a `switch` expression is not reported) — added
- RH5014-002 — The diagnostic is on the `switch` keyword — added
- RH5015-001 — A `checked` statement that follows a statement needs a blank line (an `unchecked` statement and a `checked` expression are not reported) — added
- RH5015-002 — The diagnostic is on the `checked` keyword — added
- RH5016-001 — An `unchecked` statement that follows a statement needs a blank line (a `checked` statement and an `unchecked` expression are not reported) — added
- RH5016-002 — The diagnostic is on the `unchecked` keyword — added
- RH5017-001 — A `fixed` statement that follows a statement needs a blank line — added
- RH5017-002 — The diagnostic is on the `fixed` keyword — added
- RH5018-001 — A `lock` statement that follows a statement needs a blank line — added
- RH5018-002 — The diagnostic is on the `lock` keyword — added
- RH5019-001 — A `yield` statement that follows a statement needs a blank line — added
- RH5019-002 — A `yield` statement that follows a `yield` statement is exempt — added
- RH5019-003 — The diagnostic is on the `yield` keyword — added
- RH5021-001 — An expression statement that follows a local declaration needs a blank line (a `using` declaration counts as one; an expression statement after a non-declaration is not reported) — added
- RH5021-002 — An assignment after a local declaration is exempt (simple and compound) — added
- RH5021-003 — The diagnostic is on the first token of the expression statement — added
- RH5029-001 — A local declaration that follows another kind of statement needs a blank line (`const` and `using` declarations included; a declaration after a declaration is not reported) — added
- RH5029-002 — The diagnostic is on the first token of the declaration — added

## Examples for the tests

Every `-001` requirement and RH5019-002 and RH5021-002 carry an example block, and each one was verified as described above. The location requirements (`-002`, and `-003` for RH5019 and RH5021) point at the `-001` example.

Existing tests look like plausible covers for most `-001`/`-002` IDs: each test class has a "WithoutPrecedingBlankLine" diagnostic test, a "WithPrecedingBlankLine" negative, and an "AtStartOfBlock" or "AfterCaseLabel" negative. Grepping the 16 test classes found no existing test for these qualifiers stated in the specs:

- RH5005-001: a `using` declaration (`using var …;`) that is not reported.
- RH5008-001: a `return` that ends a switch section and is reported. The class has no `default:` and no mid-section return case.
- RH5013-001: a `throw` expression (`?? throw`) that is not reported.
- RH5014-001: a `switch` expression that is not reported.
- RH5015-001: an `unchecked` statement or a `checked` expression that RH5015 does not report.
- RH5016-001: a `checked` statement or an `unchecked` expression that RH5016 does not report.
- RH5021-001: an expression statement after a `using` declaration that is reported.
- RH5021-002: a compound assignment (`+=`) that is exempt. The existing tests have only the plain assignment.
- RH5029-001/-002: a `using` declaration that is reported on `using`. `const` appears in the RH5029 tests.

RH5009-001: the `goto case` and `goto default` tests exist, and their gotos are terminal in their sections.

The RH5029 class has many shared-fix indentation tests (anchor scopes, switch labels). They belong to SHARED-STMT-004 or to no requirement, not to RH5029-001/-002. The one named `VerifyDiagnosticOverIndentsWhenLabelSpansMultipleLinesAndPrecedingStatementSharesItsContinuationLine` pins output that looks wrong against SHARED-STMT-004 (see Q1 below), so it must not be tagged SHARED-STMT-004.

## Open questions to file

### Q1 — The fix indents one level too deep when a multi-line `case` label's continuation line holds the preceding statement

Belongs to: `SDD open questions: sdd/specs/shared/statement-blank-lines.md`. No such issue exists yet (00-lead.md), so a new one is needed as a sub-issue of thoenissen/Reihitsu#912. The behavior is in the shared code-fix base, so it applies to every rule that uses SHARED-STMT-004, not only RH5029.

Input (RH5029; from the existing test `RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests.VerifyDiagnosticOverIndentsWhenLabelSpansMultipleLinesAndPrecedingStatementSharesItsContinuationLine`, written minimal here):

```csharp
internal class C
{
    private readonly Config _config = new Config
    {
        Handler = value =>
        {
            switch (value)
            {
                case 1 when
                    Check(value): Method(1); var y = 1;
                    break;
            }
        }
    };

    private static bool Check(int value) => true;

    private static void Method(int value)
    {
    }
}

internal sealed class Config
{
    public System.Action<int> Handler { get; set; }
}
```

Real output of the code fix (pinned by that test), around the switch section:

```csharp
                case 1 when
                    Check(value): Method(1);

                        var y = 1;
                    break;
```

Why it looks wrong: SHARED-STMT-004 says a statement that shares its line with the statement before it is moved to a new line "indented like the statement it follows". Here `var y = 1;` lands four columns deeper than both the line holding `Method(1);` and the section's next statement `break;`. The test's own comment calls it "one indentation level too many".

It is also called a known, documented limitation: `documentation/rules/RH5103.md` says "check the indentation of the moved statement" when a `case` label spans several lines, and the rule pages of RH5001–RH5029 refer to that limitation. The question is whether it is intended (SHARED-STMT-004 gets a stated exception), a bug (the fix aligns `var y` with `break;`), or a change.

Requirement IDs touched: SHARED-STMT-004 (and through it RH5001–RH5010, RH5012–RH5019, RH5021, RH5029).

## Follow-up candidates (out of scope, not filed)

- `sdd/specs/analyzer/RH5002.md` still has a non-goal that calls the block-comment case "an open question". It was decided as a bug in thoenissen/Reihitsu#929 (Q1, now thoenissen/Reihitsu#972). The rulebook also forbids naming an open question in a non-goal. RH5002 is not in this run's scope.
- The rule pages of RH5001–RH5029 have a paragraph on how the fix reads indentation inside object initializers ("from whichever switch-section label leads the shared physical line, the same way RH5103's code fix does"). That reads as implementation detail on an end-user page. Not touched.

## Decisions needed

_None._ Nothing blocks the run.

## Note on the prompt

The prompt carried no intended fix, suspected cause, or hoped-for verdict.
