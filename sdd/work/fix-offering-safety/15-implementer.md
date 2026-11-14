Role: implementer
Commit: 66a181fe
Verdict: tests pass

Repair round 2 for audit findings #1 and #3 (13-auditor.md; 00-lead.md decisions 21–22). My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decision 21, which I implemented as stated.

## thoenissen/Reihitsu#1600

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/ReihitsuCodeFixProviderBase.cs` | New private `MayOfferFixesAsync(Document, CancellationToken)`: the one statement of the per-document decision (hiding carrier; syntax error when `FormatsThroughFormatter`). `RegisterCodeFixesAsync` asks it. New protected `CreateFixAllProvider(fixAllAsync)` wraps a per-document Fix All delegate and returns the document unchanged when `MayOfferFixesAsync` says no. Class summary names the base as the one owner | SHARED-FIX-012, SHARED-FIX-011 (audit #1, decision 21) |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider.cs` | `GetFixAllProvider` returns `CreateFixAllProvider(ApplyFixAllAsync)` instead of `FixAllProvider.Create(ApplyFixAllAsync)` | SHARED-FIX-012 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7101DoNotCombineFieldsCodeFixProvider.cs` | same | SHARED-FIX-012 |
| `src/Reihitsu.Cli/Abstractions/ISourceFormatter.cs`, `DefaultSourceFormatter.cs` | New `HoldsHidingCarrier(SyntaxTree, CancellationToken)`, delegating to `ReihitsuFormatter.HoldsHidingCarrier` | CLI-021 (audit #3) |
| `src/Reihitsu.Cli/FormatCommandHandler.cs` | The carrier check goes through `_formatter.HoldsHidingCarrier`, like the generated-file check | CLI-021 (audit #3) |

### Fix All paths after the change
`rg -n "FixAllProvider.Create\(" src/Reihitsu.Analyzer.CodeFixes`: the only direct `FixAllProvider.Create` calls left are the base's wrapper and `DocumentOrderFixAllProvider.Create` (which re-registers each fix through the base's sealed `RegisterCodeFixesAsync`). BatchFixer re-registers through the base. `CasingFixAllProvider` asks the base's `HoldsHidingCarrierAsync` for every changed document (its own document included); renames do not format, so SHARED-FIX-011 does not apply there. No path restates the check.

### Policy owners
- `rg -n "HoldsHidingCarrier|HasSyntaxErrors\(syntaxTree\)|FormatsThroughFormatter &&|MayOfferFixes" src/Reihitsu.Analyzer.CodeFixes`: per-document decision only in `ReihitsuCodeFixProviderBase.MayOfferFixesAsync`; other-document extension through `HoldsHidingCarrierAsync` (Casing base, RH2001), unchanged.
- `rg -n "HoldsHidingCarrier" src/Reihitsu.Cli`: one call, through `ISourceFormatter`; definition stays `ReihitsuFormatterHelpers.HoldsHidingCarrier`.

### Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 122 files changed, 1526 insertions(+), 381 deletions(-). Round delta (`HEAD~1..HEAD`): 6 files, 62 insertions, 14 deletions. No new type; one new protected base member and one new interface member (internal).

### Deleted special cases
_N/A._ (No shape declared in this round.)

### Disputed tests or scope
_None._

### Architecture findings
_None._ The finding of 13-auditor.md is repaired by decision 21's structure. Note for the lead: nothing forces a future provider to use `CreateFixAllProvider` instead of `FixAllProvider.Create`; an architecture test (no `FixAllProvider.Create` outside the base and `DocumentOrderFixAllProvider`) would pin it. That is a test, so not mine to add.

### Follow-up candidates
_None new._

## thoenissen/Reihitsu#1150

Nothing in this round. SHARED-FIX-011 now also holds on the custom Fix All paths through the same `MayOfferFixesAsync` (RH7101 sets `FormatsThroughFormatter`; RH3002 does not).

## thoenissen/Reihitsu#1274

Nothing in this round.

## thoenissen/Reihitsu#1143

Nothing in this round.

## thoenissen/Reihitsu#1673

Nothing in this round.

## thoenissen/Reihitsu#1678

Nothing in this round.

## Whole run

Build: `scripts/build.sh --configuration Debug` — 0 warnings, 0 errors. `scripts/format.sh` over the changed files: formatted 0 of 4.

Tests run (Debug, `dotnet build-server shutdown` between projects):
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~HidingCarrier|FullyQualifiedName~RH3002|FullyQualifiedName~RH7101"` — Passed 384/384 (includes the four RH3002/RH7101 project/solution tests of 14, which failed before).
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~ReihitsuFormatterHiddenCodeTests"` — Passed 39/39.
- `--project analyzer` — Passed 9181/9181.
- `--project formatter` — Passed 4936/4936.
- `--project cli` — Passed 279/279.
- `--project architecture` — Passed 117/117.
