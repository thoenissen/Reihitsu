Role: implementer
Commit: c285efc (head the changes were made on; production change and this handover are in the commit that adds this file)
Verdict: tests pass

Repair round 2, lead decision 11 (auditor 11, thoenissen/Reihitsu#1289 findings #1, #2; tests from 12). My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond decisions 7, 9, and 11. No test, assertion, input, or expected output was changed.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Design/RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer.cs` `FindCodeDirectives` | Finding #1: a `@*` or `<!--` opens a comment only when it starts its line after optional white space (the existing `StartsLine`, the same check the directive uses). An opener behind other text on its line (in a `@{ }` string, an expression, a C# comment of a control-flow block, an attribute value, script text, after a letter) is text and opens nothing, so a later directive stays reported, as on `main`. Summary updated | RH2109-001, SHARED-DIAG-003, decision 11 |
| same file, `SkipCSharpBlock` | Finding #2: when a C# token of the block starts with `@<` (template markup, which no valid C# outside a literal or comment holds), the skip stops behind `@<` and the outer scan continues over the rest of the block as markup. So `{` or `/*` in template text no longer keeps the block open; a later directive stays reported, as on `main`. Summary and `<returns>` updated | RH2109-001, decision 11 |

Both are cheap syntactic checks on characters the scanner already inspects; no new type, member, or dependency.

### Fallback behavior (decision 11), for the decision-7 follow-up
- A comment opened behind other text on its line that spans lines and holds a line-starting `@code {`: reported (as on `main`; before this round it was not). A mid-line `@code` inside it stays unreported (Q3).
- After a template `@<` in a `@code`/`@functions` block, the block's remaining C# is scanned as markup: a line-starting `@code {` inside a later C# comment or literal of that block is reported (as on `main`), and a line-starting `@*`/`<!--` there opens a comment.
- Residual miss (not fully closed by a cheap check): a `@*` or `<!--` that starts its own line inside a multi-line C# literal or comment of an unlexed host (`@{ }`, control flow, expression), e.g. a verbatim string in `@{ }` whose continuation line begins with `<!--`, still swallows the text to its terminator, and a real directive in between is missed. Telling it from a real comment needs the host's C# extent, which is the decision-7 follow-up. Not honoring line-starting openers at all would close it but break the spec-required multi-line comment tests (`VerifyCodeBlockInsideMultiLineRazorComment…`, `…HtmlComment…`). Recorded here for the lead; no test pins it.

## Policy owners
- `rg -n "@code|<!--|ParseTokens" src --glob '!*Test*'` — Razor scanning only in RH2109; `ParseTokens` elsewhere (`Reihitsu.Core/TokenSeparationUtilities.cs`, `CodeFixes/Core/ExcludedCodeUtilities.cs`) serves unrelated purposes. `StartsLine` reused, no copy.

## Production diff size
- `git diff --shortstat c2866ea...HEAD -- src ':(exclude,glob)src/*Test*/**'` after this commit: 5 files changed, 191 insertions(+), 15 deletions(-).
- This round: 1 file, +14/−6.
- RH2109 against `c2866ea`: +155/−8 gross; 95 added lines are neither blank nor `///` (was 91 in 05), within decision 9's ~100. No new type.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests"` — Passed 46/46 (13 new from 12, all previously failing).
- `scripts/test.sh --configuration Debug --project analyzer` — Passed 8605/8605.
- `scripts/build.sh --configuration Debug` — 0 warnings, 0 errors. `scripts/format.sh` on the file — formatted 0 of 1.

## Disputed tests or scope
_None._ The residual miss above is a scope note for the lead, not a dispute.

## Architecture findings
_None._

## Follow-up candidates
- Add to the decision-7 follow-up (#1289 remainder): the residual miss above (line-starting comment opener inside a multi-line C# literal/comment of an unlexed host), and the fallback false positives after a template `@<` in a block and inside a multi-line comment opened mid-line.
