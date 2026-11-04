Role: spec author
Commit: dc3ac6f
Verdict: change doc written

Amendment round. Inputs: `00-lead.md` (*Maintainer answers*, *Decisions taken* rows 6–8, *Follow-up candidates*) and `02-skeptic.md`. `01-spec-author.md` is left unchanged; where it differs from this file, this file holds. The change doc `sdd/changes/2026-11/rh7101-fix-through-context-entry-point.md` is rewritten. It now has twelve RH7101 cases, states that the using fixes do not change, and records the answers under *Decisions*. It stays `Status: Draft`.

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"`: 36 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, for FMT-PIPE-029, FMT-PIPE-030, and FMT-PIPE-031, which need tests.

## Changed requirements

- FMT-PIPE-005 — Code with syntax errors is left untouched — changed: `OrganizeUsingDirectivesInDocumentAsync` checks no syntax errors, like `FormatNode`, so the using fixes keep today's behavior.
- FMT-PIPE-006 — Generated code is left untouched — changed (answer 4b A). Only `FormatDocumentAsync` and `FormatSyntaxTree` check the header. `FormatNode`, `FormatNodeInDocumentAsync`, `FormatNodeInDocumentWithContextAsync`, and `OrganizeUsingDirectivesInDocumentAsync` must not check it. The example is split in two: a document-level example, and a node-level example that formats `M` in a header-marked file. `Uses` += SHARED-DIAG-001.
- FMT-PIPE-009 — Host-independent entry points — changed. The entry point list adds `OrganizeUsingDirectivesInDocumentAsync` and `SplitsFieldDeclaration`. The architecture rule is added: a code fix reaches a phase or transform only through these members, never runs or calls a member of one, and formatter helpers that are neither stay callable. The same rule is added to `AGENTS.md`, *Key conventions*, naming `Reihitsu.ArchitectureTests` as the enforcement. The architecture test may tag FMT-PIPE-009.
- FMT-PIPE-029 — The context entry point writes back every field the split makes of its target — changed (decision 6, answer 3A):
  - A tab counts as one column, and later fields are indented with spaces.
  - Three bullets cover a member in front, a member behind (the whitespace before it becomes a line break plus indentation, an exception to keeping the trailing trivia), and a comment in the gap (it stays behind the code in front, and only the whitespace behind it is replaced). No whitespace is left at a line end.
  - `{` and `}` keep their line.
  - New examples: `_p; /* c */ … _q;`, and the tab case.
- FMT-PIPE-030 — The using entry point organizes one scope's using block — added (answer 1A). It runs only the using phase on the given scope and writes back that scope's block and nothing else. Nested namespaces are not touched. It uses the document's line ending and language version, and checks neither syntax errors nor the header. The phase's output, scope indentation included, is final. Supported input: the RH7201–RH7207 scope, with a block of at least two directives that may be reordered. The example output is measured with today's RH7203 fix.
- FMT-PIPE-031 — The formatter answers whether it splits a field declaration — added (decision 7). `SplitsFieldDeclaration` returns true exactly when the declaration has more than one declarator, is a member of a class, struct, record, or interface, and carries no directive or disabled text.
- FMT-USING scope sentence and indentation non-goal — changed. The fixes go through FMT-PIPE-030. The scope indentation is final in that entry point's output. `Uses` += FMT-PIPE-030.
- RH7101-001 — changed (answer 4a A): no report in a file with a syntax error, by referencing SHARED-JOIN-002.
- RH7101-002 — changed (decision 8, revised; see *Output questions*): a field in an extension block is not reported.
- RH7101-004 — changed (decision 6): fields share no line with another member, and a comment in the gap stays. The shared-line example is now the two-sided `_p; /* c */ … _q;`.
- RH7101-005 — changed: the fix asks the formatter (FMT-PIPE-031).
- RH7101-006 — removed from the spec and the ledger. It was only ever on this branch, never on `main`, and answer 4a replaced it with RH7101-001's SHARED-JOIN-002 exception. See *Internal choices*.
- Ledger: FMT-PIPE-030 and FMT-PIPE-031 added; RH7101-006 removed.
- pipeline `Uses` += RH7201-004, SHARED-DIAG-001, SHARED-FIX-005, SHARED-USING-009, SHARED-USING-010, FMT-USING-003.
- RH7101 `Uses` is now FMT-PIPE-005, FMT-PIPE-017, FMT-PIPE-029, FMT-PIPE-031, FMT-STRUCT-021/022/023, SHARED-FIX-006/007, and SHARED-JOIN-002.
- Unchanged from 01: FMT-STRUCT-026, and SHARED-USING-010. SHARED-USING-010's Why already names "the output of the using-directive phase on its own", which FMT-PIPE-030 is.

## Examples for the tests

- **FMT-PIPE-006:** its node-level example, with a header-marked file and `FormatNodeInDocumentAsync` on `M`.
- **FMT-PIPE-029:** all five examples. A Fix All test on `private int _a, _b; private int _c, _d;` must equal the two single fixes applied in sequence (SHARED-FIX-002; 02-skeptic class 2).
- **FMT-PIPE-030:** its example. The RH7201–RH7207 code-fix tests must keep their outputs, because none changes.
- **FMT-PIPE-031:** its example, `_a, _b` true and the `#if` declaration false. Also an extension-block field, which returns false.
- **RH7101-001/002:** the syntax-error file (not reported; tag SHARED-JOIN-002 as well), and the extension-block field (not reported).
- **RH7101-004:** its examples. The change doc's cases 1–9 give the full set of inputs, case 6 with initializers and case 8 with a tab included. Case 12 has the header-marked file under `generated_code = false`; FC-033 applies, because the harness must turn the generated-code heuristic off.

## Output questions

**1 · RH7101 · Do not combine fields — a field in an extension block** (change doc case 11). No maintainer answer covers this. Decision 8 chose "fix withheld", and the maintainer's principle "when an analyzer reports, a code fix is offered" points the other way.

Input:
```csharp
static class E
{
    extension(int i)
    {
        private static int _x, _y;
    }
}
```

Before (`main`): RH7101 is reported on `_y`. A fix is offered that changes nothing (02-skeptic measured `no-progress`).

After, A (recommended, in the spec): not reported.

After, B: reported, and no code fix is offered (decision 8).

Why A: the compiler rejects any field in an extension block (CS9282, measured with the .NET 10 SDK, `LangVersion` 14), so the diagnostic asks the user to split code that cannot compile. Under the maintainer's principle a diagnostic without a fix should not be reported where the fix can never apply. B keeps a permanent unfixable diagnostic.

No other new visible output is in question. Change doc cases 1–10 and 12 are covered by the maintainer's answers. Case 7 is demanded by the unchanged SHARED-FIX-007 (decision 6). The using fixes do not change.

## Internal choices

- **RH7101-006** is deleted, not retired. It never reached `main`, so no link or test refers to it. Alternative: keep it as `Status: Retired`, which would leave a never-shipped requirement in the spec.
- **Syntax-error exception** is written as a reference to SHARED-JOIN-002 inside RH7101-001, not as a requirement of its own. This follows the rule that a shared rule is referenced, never restated. RH5105 and RH5106 do the same.
- **Member names** are chosen by the spec, because FMT-PIPE-009 enumerates the entry points: `ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync(Document, SyntaxNode scope, CancellationToken)` and `ReihitsuFormatter.SplitsFieldDeclaration(FieldDeclarationSyntax)`. If the implementer renames them, the spec follows.
- **Architecture rule placement:** stated in FMT-PIPE-009 and in `AGENTS.md`, not as a requirement of its own. It constrains the code-fix-to-formatter surface, which FMT-PIPE-009 already owns. Alternative: a separate FMT-PIPE requirement, which would state a code-structure rule as behavior.
- **`FormatNode`** is added to FMT-PIPE-006's "checks no marker" list. It already checked none (FMT-PIPE-012), so this is wording only.
- **The using entry point applies no file-level guard.** Today's fixes have none, and outputs must stay (answer 1A). Alternative: check syntax errors, which would put the seven rules' outputs in question in files with a syntax error.
- **`AGENTS.md`** is edited by this role. It is a tooling path, which the behavior-change lane admits along with the change doc. The lead should confirm with `scripts/lane-check.sh`.
- **The rule page `documentation/rules/RH7101.md`** is unchanged. Not reporting in a syntax-error file or an extension block is a corner case, not the normal report.

## Follow-up candidates

None new. The candidates in `00-lead.md` stand. The RH5111/RH5417/RH3208 no-op candidate is now limited to syntax errors, because answer 4b resolves the header half for every node-level fix.
