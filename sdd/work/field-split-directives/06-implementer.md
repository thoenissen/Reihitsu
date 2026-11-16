Role: implementer
Commit: f5afc3a
Verdict: #1728: tests pass; #1586: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Work follows the accepted change doc `sdd/changes/2026-11/field-declaration-directives.md` and the handed-over tests (05-test-implementer.md); no test was changed.

## thoenissen/Reihitsu#1728

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` | New reason constant `FieldDeclarationDirectiveReason`; `FindUnsupportedSyntax` visits every `FieldDeclarationSyntax` and asks the new `FindDirectiveInFieldDeclaration` (declarations with at least two declarators of the parsed build; every token's leading and trailing trivia, skipping the first token's leading trivia; first directive or disabled text). Candidate order at a tie: at a declarator comma FMT-UNSUP-009 stays ahead of FMT-UNSUP-010; otherwise FMT-UNSUP-010 comes first. Comment above the order rewritten | FMT-UNSUP-010, FMT-UNSUP-009, FMT-UNSUP-001, FMT-UNSUP-007 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7101DoNotCombineFieldsCodeFixProvider.cs` | Doc of `TryGetFixableFieldDeclaration` only: the shape is withheld through `FindUnsupportedSyntax` on the type, disabled text in front through `SplitsFieldDeclaration`. Code unchanged | RH7101-005, SHARED-FIX-010 |

Tie decision: FMT-UNSUP-010 says "Every other directive in the declaration gives this requirement's reason", so a directive that both FMT-UNSUP-010 and FMT-UNSUP-009 refuse at a comma that is *not* between declarators (for example in an attribute argument list or a value's argument list of a combined field) reports FMT-UNSUP-010. FMT-UNSUP-009's own tie paragraph only addresses declarator commas, so I read the two as consistent. No test covers that tie; the auditor may want one. The tie with FMT-UNSUP-008 (directive behind `?` or `:` inside a value of a combined field) also gives FMT-UNSUP-010, on the same sentence.

### Deleted special cases
- `FieldDeclarationSplitTransform.CarriesDirective` and its operand in `Splits` — change doc *Deleted*, first entry. Replaced by the kept check for disabled text in the leading trivia (*Checked and kept*), see #1586.
- The comment in `SplitFields` explaining that `Splits` leaves directive-bearing declarations intact — same entry.
- The directive condition of `ReihitsuFormatter.SplitsFieldDeclaration` — it is `Splits`, so it narrowed with it; its doc sentence rewritten — change doc *Deleted*, second entry.
- No further special case found. `rg -n "FMT-STRUCT-023|CarriesDirective" src --glob '!*Test*/**'` finds nothing left (the only hit is `SwitchCaseBraceRewriter.SectionCarriesDirectives`, unrelated).

## thoenissen/Reihitsu#1586

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/FieldDeclarationSplitTransform.cs` | `Splits` now asks `HasDisabledTextInLeadingTrivia` (any `DisabledTextTrivia` in `GetLeadingTrivia()`) instead of `CarriesDirective`; summaries of `Splits`, the new method, and `CreateSplitField` updated | FMT-STRUCT-022, FMT-STRUCT-021 |
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs` | Doc of `SplitsFieldDeclaration` only; body unchanged (`Splits`) | FMT-PIPE-031 |

No change in `FormatSplitFieldInContext` was needed: the region and group outputs of the handed-over tests came out of the existing split and layout.

### Deleted special cases
See #1728; the narrowed check is the one *Checked and kept* names.

## Whole run

### Policy owners
- Unsupported shapes: `rg -n "FindUnsupportedSyntax" src --glob '!*Test*/**'` — one predicate, `PrecheckPhase.FindUnsupportedSyntax`, which `ReihitsuFormatter.FindUnsupportedSyntax` calls; the code fixes ask the latter. No copy added.
- Field split applicability: `rg -n "Splits\(|SplitsFieldDeclaration" src --glob '!*Test*/**'` — owner `FieldDeclarationSplitTransform.Splits`; `ReihitsuFormatter.SplitsFieldDeclaration` delegates; RH7101 fix and `FormatNodeInDocumentWithContextAsync` call the latter.
- Disabled text in leading trivia: `rg -n "IsKind\(SyntaxKind.DisabledTextTrivia\)" src --glob '!*Test*/**'` — Core helpers test conditional directives *or* disabled text (`SyntaxTriviaUtilities` 463/475), indentation, ordering, and modifier ordering; none states "disabled text in a declaration's leading trivia" alone, so the one-line check stays local to the split.
- Directive-or-disabled-text trivia: `SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia` reused.

### Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 4 files changed, 82 insertions(+), 21 deletions(-). No new type, no new public member (the new reason constant is `internal`).

### Tests run
All Debug, on f5afc3a:
- `scripts/test.sh --configuration Debug --project formatter` — Passed! Failed: 0, Passed: 5355
- `scripts/test.sh --configuration Debug --project analyzer` — Passed! Failed: 0, Passed: 10437
- `scripts/test.sh --configuration Debug --project cli` — Passed! Failed: 0, Passed: 307
- `scripts/test.sh --configuration Debug --project tooling` — Passed! Failed: 0, Passed: 133
- `scripts/test.sh --configuration Debug --project architecture` — Passed! Failed: 0, Passed: 153
- `scripts/test.sh --configuration Debug --project core` — Passed! Failed: 0, Passed: 738

`scripts/format.sh` on the four changed files: formatted 0 of 4.

### Disputed tests or scope
_None._ (Untested tie order at a non-declarator comma noted under #1728.)

### Architecture findings
_None._

### Follow-up candidates
_None_ beyond the one in 00-lead.md.
