Role: spec author
Commit: a4913b6
Verdict: spec written

No change doc: every requirement writes down current behavior (lane *specs and tests for existing behavior*). No production code, test, or rule page was touched. The prompt carried no intended fix or verdict to ignore.

## Files

- `sdd/specs/shared/element-documentation.md` — new shared spec, prefix `SHARED-DOC`.
- `sdd/specs/analyzer/RH8001.md` to `RH8019.md` — 19 new rule specs, two requirements each (what is reported, where the diagnostic is).
- `sdd/specs/analyzer/RH8107.md` — header and two reference edits, see below.
- `sdd/specs/README.md` — 19 analyzer rows and the shared row added; RH8107 row's code owners updated.
- `sdd/specs/requirement-ids.txt` — 46 IDs inserted (SHARED-DOC-001..008, RH8001-001..RH8019-002); no line removed or moved, ledger still sorted.

## Changed requirements

- SHARED-DOC-001 — Every declaration of the rule's element kind is checked — added
- SHARED-DOC-002 — Private and non-private groups — added
- SHARED-DOC-003 — An enum member belongs to the group of its enum — added
- SHARED-DOC-004 — The documentation comments of a declaration precede it — added
- SHARED-DOC-005 — A top-level `<summary>` or `<inheritdoc>` documents the element — added
- SHARED-DOC-006 — A later documentation comment counts too — added
- SHARED-DOC-007 — An included documentation file counts — added
- SHARED-DOC-008 — Nothing is reported when documentation is not parsed — added
- RH8001-001 … RH8019-001 — Undocumented <group> <kind> are reported — added (19)
- RH8001-002 … RH8019-002 — The diagnostic is on the <kind> name — added (19)
- RH8107-001 — clarified: a reference "as SHARED-DOC-004 defines it" appended to the existing sentence "The documentation comment is the first one in the member's leading trivia"; no other word changed, ID and status unchanged.
- RH8107-004 — clarified: ", as SHARED-DOC-008 specifies" appended to the normative sentence; no other word changed, ID and status unchanged.

No RH8107 requirement was moved or retired. The two RH8107 edits only add references so that `Uses` can list SHARED-DOC-004 and SHARED-DOC-008, as the maintainer's clarification in `00-lead.md` asks; the behavior text is untouched. The lead summary also says not to change existing requirements of another spec without asking — if the lead reads these two reference edits as such a change, reverting them means also setting RH8107 `Uses` back to `—`.

### Code ownership

- Moved from RH8107 to the shared spec: `DocumentationAnalysisUtilities` (accessibility groups, documented check, diagnostic location), `DirectDocumentationSyntaxChecker` (documentation comment selection, top-level tag lookup), `DocumentationModeAnalysisContextExtensions` (documentation mode). Added to the shared spec: `SplitElementDocumentationAnalyzerBase`, `XmlDocumentationExpander` (include expansion; was owned by no spec).
- Stays with RH8107: `DocumentationCommentCodeFixUtilities` — element removal, which only RH8107-006 specifies so far.
- Fact for the skeptic: two shared-owned types also implement behavior that only RH8107 specifies — `DirectDocumentationSyntaxChecker.GetFirstDocumentingTag` (sample skipping, RH8107-003; also called by RH8202) and `DocumentationAnalysisUtilities.IsVoidReturnType` (RH8107-001). Moving those statements into the shared spec would retire RH8107 IDs, so they stay in RH8107.
- `SplitElementDocumentationAnalyzerBase` also skips explicit interface implementations (`NeedsDocumentation`); none of the kinds in RH8001–RH8019 can be one, so that behavior is left to the later batches (thoenissen/Reihitsu#983 to #985), which can add it as a new SHARED-DOC requirement.

### How the statements were established

- Read the code at a4913b6: `SplitElementDocumentationAnalyzerBase`, the 19 analyzers (each passes one `SyntaxKind` and a `DocumentationAccessibilityGroup`; RH8013/RH8014 are standalone; RH8019 uses `Any`), `DocumentationAnalysisUtilities.MatchesAccessibilityGroup`/`IsPurePrivateDeclaration`/`HasImplicitPrivateAccessibility`/`HasRequiredDocumentation`/`GetDiagnosticLocation`, `DirectDocumentationSyntaxChecker`, `XmlDocumentationExpander`, `DocumentationModeAnalysisContextExtensions`, `DiagnosticAnalyzerBase` (Warning, enabled by default). No code-fix provider exists for RH8001–RH8019.
- Ran every C# example block of the 20 new specs (29 blocks) through a scratch harness outside the repository: Roslyn `Microsoft.CodeAnalysis.CSharp` 5.9.0, the Debug build of `Reihitsu.Analyzer` at a4913b6, `DocumentationMode.Diagnose`, an `XmlFileResolver` over the fixture directory (for SHARED-DOC-007's `docs.xml`). For each block the reported (rule, line, span text) set matched the markup exactly, with no compiler errors. The SHARED-DOC-008 prose example was run in `None` (nothing reported) and `Parse` (reported on `C`).
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"`: 26 passed.
- `RequirementCoverageTests` fails as expected: the 38 RH80xx requirements and the SHARED-DOC requirements have no tagged test yet.

## Examples for the tests

RH8001-001 … RH8019-001 (one example block each; RH80nn-002 points at it), SHARED-DOC-001 to SHARED-DOC-007 (SHARED-DOC-002 and SHARED-DOC-003 have one block per group), SHARED-DOC-008 (prose: the block under `None` versus `Parse`/`Diagnose`).

Facts about existing coverage, from reading and searching the 19 test classes (none found where it says none): no existing test uses `<include>` or sets an `XmlReferenceResolver` (SHARED-DOC-007); none has two documentation comments on one declaration (SHARED-DOC-006) or a documentation comment after an attribute list (SHARED-DOC-004); none puts a static constructor under RH8018 or a primary constructor under RH8017; RH8017, RH8018, and RH8019 have one diagnostic test each. Every test class has a `VerifyNoDiagnosticsWhenDocumentationModeIsNone` test. The two malformed-XML tests in `RH8001NonPrivateClassesMustBeDocumentedAnalyzerTests` and the one in `RH8013NonPrivateEnumMembersMustBeDocumentedAnalyzerTests` check that the analyzer does not crash.

## Open questions to file

Both belong to a new issue `SDD open questions: sdd/specs/shared/element-documentation.md` (sub-issue of thoenissen/Reihitsu#912); none exists yet. Both cases are left out of the spec through the coverage sentences of SHARED-DOC-005, SHARED-DOC-006, and SHARED-DOC-007.

### Partial types: an undocumented part is reported, a part with only `<remarks>` is not

Input (RH8001, default documentation mode):

```csharp
/// <summary>
/// Summary
/// </summary>
public partial class P
{
}

public partial class P
{
}

/// <remarks>
/// Remarks
/// </remarks>
public partial class P
{
}
```

Real output: RH8001 on the second part only. A part without any documentation comment counts as undocumented (the syntax check finds no comment), while the third part counts as documented because the compiler joins the documentation of all parts and that joined documentation has the first part's `<summary>`.

Why it looks wrong: the result depends on whether a part has some unrelated documentation comment, not on whether the type is documented. Two readings produce different output: (A) a partial type is documented when any part has a top-level `<summary>` or `<inheritdoc>` — nothing is reported here; (B) every part must carry its own `<summary>` or `<inheritdoc>` — the second and the third part are reported. Requirements touched: SHARED-DOC-005, SHARED-DOC-006, SHARED-DOC-007; via them RH8001–RH8010 (every type kind that can be partial).

### A summary in a later documentation comment is ignored when the documentation is not well-formed XML

Input (RH8001):

```csharp
/// <remarks>R</remarks>
// x
/// <summary>a & b</summary>
public class A
{
}

/// <summary>a & b</summary>
public class B
{
}
```

Real output: RH8001 on `A`, nothing on `B` (in both `Parse` and `Diagnose` mode). For `B` the syntax check of the first documentation comment finds the `<summary>`; for `A` the summary is only in the second comment, which is read from the compiler's joined documentation, and that is discarded as malformed because of the unescaped `&`. The same happens with any malformed later comment, e.g. `/// <summary>S</summary> <x>`. A well-formed summary in a later comment counts (SHARED-DOC-006).

Why it looks wrong: whether the same `<summary>` counts depends on which documentation comment it sits in. Two readings: (A) a top-level `<summary>`/`<inheritdoc>` in any documentation comment counts, well-formed or not — `A` is not reported; (B) malformed documentation never counts — `B` is reported too, which contradicts the existing test `VerifyNoDiagnosticForClassWithMalformedAmpersandInDocumentation`. Requirements touched: SHARED-DOC-005, SHARED-DOC-006.

## Follow-up candidates (out of scope)

- The rule pages `documentation/rules/RH8001.md` to `RH8018.md` do not say what "private" means (`private protected` counts as non-private; a declaration without a modifier in a class, struct, or record is private; static constructors fall under RH8018). They do not contradict the specs, and the documented behavior did not change, so they were left unchanged.

## Decisions needed

_None._
