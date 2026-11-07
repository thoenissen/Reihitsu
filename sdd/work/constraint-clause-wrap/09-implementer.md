Role: implementer
Commit: see the commit that adds this file (production changes and handover in one commit), on top of 57b8811
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The change follows FMT-SPACE-009 (changed, Q10) and FMT-BREAK-037 (changed, Q11) as `07-spec-author.md` states them.

All 15 tests that `08-test-implementer.md` expected to fail now pass, and so do the D1 tests (7) and D3 (1) from `06-implementer.md`. Every other test in the formatter, analyzer, core, CLI, architecture, and tooling suites passes.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/HorizontalSpacing/Rules/SingleSpaceSpacingRule.cs` | `IsSpacedColonToken` also matches the colon of a `TypeParameterConstraintClauseSyntax`, so the constraint colon gets one space on each side, like a base-list or constructor-initializer colon. This covers a colon a join wrote without a space and a one-line `where T:class`. A colon starting a line is spaced only behind it (FMT-SPACE-001 leaves a line-start gap alone). Class summary updated. | FMT-SPACE-009 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/GenericConstraintLineBreakRewriter.cs` | `IsWrapped` also returns `true` when a line break still stands between the type parameter and the colon after `JoinUnstructuredWraps`. That join refuses only a gap holding a comment, a directive, or disabled text, so this is exactly a colon kept on its own line. The clause is then laid out one constraint per line by the existing `PlaceOneConstraintPerLine`. The summary of `IsWrapped` and the class remarks are updated. | FMT-BREAK-037 |

RH5110's fix is unchanged; it formats through `FormatNodeInDocumentAsync`, so both changes reach it (RH5110-003).

## Policy owners
- Constraint-colon spacing: `rg -ln "ColonToken" src/Reihitsu.Formatter/Pipeline/HorizontalSpacing` finds `SingleSpaceSpacingRule`, `NameColonSpacingRule`, `AttributeTargetColonSpacingRule`, `ConditionalOperatorSpacingRule`. Only `SingleSpaceSpacingRule` owns declaration colons; it is extended. No other rule matches the constraint colon.
- Constraint-clause layout: `rg -ln "TypeParameterConstraintClauseSyntax" src/Reihitsu.Formatter src/Reihitsu.Core` finds `GenericConstraintLineBreakRewriter`, `GenericConstraintContributor`, and now `SingleSpaceSpacingRule`. The contributor needed no change: it measures from the colon's position after spacing.
- `rg -n "BaseListSyntax.*ConstructorInitializerSyntax" src` outside tests finds no private copy of the declaration-colon list.

## Tests run
```text
scripts/test.sh --configuration Debug --project formatter     Passed!  - Failed: 0, Passed: 4583, Total: 4583
scripts/test.sh --configuration Debug --project analyzer      Passed!  - Failed: 0, Passed: 7340, Total: 7340
scripts/test.sh --configuration Debug --project core          Passed!  660/660
scripts/test.sh --configuration Debug --project cli           Passed!  251/251
scripts/test.sh --configuration Debug --project architecture  Passed!  115/115
scripts/test.sh --configuration Debug --project tooling       Passed!  126/126
scripts/format.sh <both changed files>                         Formatted 0 of 2 file(s).
```

## Disputed tests or scope
_None._ Two production files are changed, and both were already named by the handovers. No new public member or dependency.

## Architecture findings
_None._

## Follow-up candidates
_None_ beyond those already in `00-lead.md` (7: a line inside a constraint that a comment keeps wrapped outside a type argument list; 8: no analyzer reports the tight constraint colon). The self-hosting scan is CI's; the new spacing may surface `where T:` in repository source there.
