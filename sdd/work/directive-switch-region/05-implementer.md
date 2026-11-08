Role: implementer
Commit: aaa7241
Verdict: test disputed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. All 35 handed-over red tests pass; one existing test outside the handover now fails because it pins the output FMT-BLANK-016 forbids (see *Disputed tests or scope*).

## thoenissen/Reihitsu#1385

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BlankLineEditor.cs` | `EnsureBlankLineBeforeStatement` and `EnsureBlankLineBeforeSection` both delegate to one new private `EnsureBlankLineUnlessPrecededByDirective<TNode>`: no blank line when the gap already holds one or when the token is directly preceded by a directive (`SyntaxTriviaUtilities.IsPrecededByDirective`, a whole conditional group counts through its `#endif`); otherwise the blank line is inserted at `SyntaxTriviaUtilities.FindIndexAfterLeadingDirectives` instead of leading-trivia index 0. Sections previously had no directive exemption and both inserted at index 0, in front of the directive | FMT-BLANK-004, FMT-BLANK-006, FMT-BLANK-016 |

Callers are unchanged: `BlankLineStatementSpacingRewriter` (FMT-BLANK-001..004 statement kinds, the blank line before a `break`) and `BlankLineBreakSpacingRewriter` (behind a `break`, the section behind a `break`-ending section, and the node-level root section/statement). `EnsureBlankLineAfterClosingBrace` (FMT-BLANK-005, no directive exemption) already inserted after the directives and is untouched.

### Policy owners
- `rg -n "IsPrecededByDirective|FindIndexAfterLeadingDirectives" src --glob '!*.Test/**'`: both owned by `src/Reihitsu.Core/SyntaxTriviaUtilities.cs`; consumers `StatementShouldBePrecededByABlankLineAnalyzerBase{TStatement}.cs` (analyzer exemption), `BlankLineCodeFixUtilities.cs` (code-fix insertion point), `BlankLineTriviaBoundaryRewriter.cs`, and `BlankLineEditor.cs`. No private copy written; the editor reuses both Core owners.

## thoenissen/Reihitsu#1398

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs` | `HasAnyMultiLineSection` skips a section whose only statements are `break` (`IsEmptySection`, the predicate `BuildSection` already used to keep such a section bare), so a multi-line label pattern of such a section no longer braces the switch | FMT-SWITCH-004, FMT-SWITCH-006 |

### Policy owners
- `rg -n "IsEmptySection|BreakStatement\) == false\) == false" src --glob '!*.Test/**'`: one owner, `SwitchCaseBraceRewriter.IsEmptySection`; now used in both the switch-level decision and the per-section decision.

## thoenissen/Reihitsu#1239

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/RegionFormatting/Utilities/RegionNamingRewriter.cs` | `Rewrite` strips the number sign's trailing trivia of each paired `#region`/`#endregion` (two private `WithoutWhitespaceAfterNumberSign` overloads) before building the canonical trivia, so every rewritten line, described or not, active or disabled, starts `#region`/`#endregion`. The Core helper `RegionDirectiveUtilities.CreateCanonicalEndRegionTrivia` is unchanged (Decision 2): the formatter passes it the already stripped directive, and RH7301's fix output keeps the gap (`RegionFixKeepsWhitespaceBetweenNumberSignAndEndRegionKeyword` passes) | FMT-REGION-002, FMT-REGION-004, FMT-REGION-007 |

### Policy owners
- `rg -n "HashToken" src --glob '!*.Test/**'`: only the two new overloads; no other owner of the gap after `#`.

## Tests run

```shell
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~RequiredBlankLineBehindDirectiveTests|FullyQualifiedName~ReihitsuFormatterNodeBlankLineTests|FullyQualifiedName~SwitchCaseBraceRequirementTests|FullyQualifiedName~SwitchCaseBraceRewriterTests|FullyQualifiedName~RegionLineFormTests"
```
Passed! - Failed: 0, Passed: 217

```shell
scripts/test.sh --configuration Debug --project formatter
```
Failed! - Failed: 1, Passed: 4732 (the disputed test below)

```shell
scripts/test.sh --configuration Debug --project analyzer
```
Passed! - Failed: 0, Passed: 7352 (includes `RH5011…FormatterTests` and `RH7301RegionsShouldMatchAnalyzerTests`)

`--project cli` Passed 251; `--project architecture` Passed 115; `--project core` Passed 660. Self-hosting scans not run (CI).

## Disputed tests or scope

**`src/Reihitsu.Formatter.Test/Unit/Pipeline/PhaseLineEndingOwnershipTests.cs` — `LineBreaksAroundDirectivesAndCommentsAreTheDetectedLineEndingInEveryPhase`** (FMT-PIPE-019; not in the test implementer's handover, which reported 34 red tests and a green rest at f574f97 — this one passed there because it pins the old output).

Input gap in front of the `if`:
```csharp
#else
        Enabled();
#endif
#pragma warning disable CS0168
        /* block */
        if (a)
```
It expects the FMT-BLANK-001 blank line in front of `#endif`:
```csharp
        Enabled();

#endif
#pragma warning disable CS0168
        /* block */
```
The formatter now writes it after the last directive, in front of the comment:
```csharp
        Enabled();
#endif
#pragma warning disable CS0168

        /* block */
```
FMT-BLANK-016 (amended in this run) requires the latter: "insert the blank line after the last directive of the gap, in front of the comments … and it MUST NOT insert a blank line directly in front of a directive." The test's subject is line endings (FMT-PIPE-019), not blank-line placement; its `expected` needs the blank line moved from above `#endif` to between `#pragma warning disable CS0168` and `/* block */`. Route to the test implementer. No visible output is in question beyond what the change doc and FMT-BLANK-016 already decide.

Scope: three production files, all named by the handovers' requirements; no new public member, dependency, or shared helper.

## Architecture findings

_None._

## Follow-up candidates

_None_ beyond those already in `00-lead.md`.
