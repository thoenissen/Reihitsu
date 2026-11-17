Role: spec author
Commit: 340608b
Verdict: change doc written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: thoenissen/Reihitsu#1759 with its comment, the maintainer decision recorded on thoenissen/Reihitsu#1756 (2026-11-15: analyzers judge the code as the current build's preprocessor symbols are set), and `00-lead.md` (no maintainer answers yet).

Change doc: `sdd/changes/2026-11/report-across-conditional-directives.md` (Status: Draft). Its *Visible output changes* lists every changed output with input, before, and after.

## Changed requirements

- SHARED-DIAG-008 — A conditional directive in the judged span — changed: reporting half reversed (the rule judges the build's version; excluded code counts as absent); fix half stated by the part of the judged span the fix rewrites or moves, plus SHARED-FIX-003/009 for a fix that leaves the directive in place.
- SHARED-DIAG-010 — changed: the RH5111 exception covers a directive of any kind.
- SHARED-TRAIL-002 — A comma in front of a conditional directive is reported without a fix — changed (in place; title changed).
- SHARED-ATTR-019 — A comment or directive inside the list withholds the split fix — changed (in place; title changed).
- SHARED-ATTR-020 — A comment or directive around the group withholds the merge fix — changed (in place; title changed).
- SHARED-EMPTY-001 — clarified: directives and excluded code in the body do not keep the type from being reported (the code reports them today).
- SHARED-MODORDER-003 — changed: a conditional directive in front of a later modifier withholds the fix (it was excluded because such declarations were not reported).
- RH5102-004, RH5109-004 — changed: a conditional directive in a gap no longer keeps the list unreported; it withholds the fix.
- RH5111-005 — changed (title too): a conditional directive around `=` keeps the construct reported, without a fix.
- RH7101-001 — changed: declarators counted as the build compiles them; reported on the second compiled declarator, no fix (RH7101-005 / FMT-UNSUP-010).
- RH7102-001, RH7103-001 — changed: a conditional directive in the modifier list no longer keeps the member unreported; fix withheld. RH7102-006 — changed: its premise sentence ("a reported constant has no conditional directive ...") replaced.
- RH7105-001, RH7106-001 — changed.
- RH7107-001, RH7108-001 — changed; RH7107-005, RH7108-005 — changed: "still reported, also when a conditional directive takes part in the refusal"; the fix is decided by the SHARED-ORDER requirements as for any move.
- RH7311-001 — changed: regions the build compiles are compared, also inside a branch; regions in an excluded branch are not.
- RH8201-003 — changed (title too): the implicit implementation is reported; the fix is withheld when an `#if` appears in one of the named files.
- RH3002-010 — clarified: no longer called a departure.
- RH7003-001 — clarified: the "nor an `#if` ..." clause removed; the analyzer never had it (test `RH7003FileScopedNamespacesShouldBeUsedAnalyzerTests.VerifyDirectiveContainingNamespaceIsReported` asserts the report).
- RH2005-001, RH2001-007 — clarified: SHARED-DIAG-008 reference added, behavior unchanged.

No requirement ID was added or retired; `requirement-ids.txt` is unchanged. `Uses` rows updated: diagnostics.md (−RH3002-010, −SHARED-TRAIL-002, +SHARED-FIX-009), trailing-commas.md, empty-type-declarations.md, modifier-ordering.md, RH2001, RH2005, RH7003, RH7101, RH7311, RH8201.

Documentation: `documentation/rules/general-notes.md` (one paragraph under *Code excluded by `#if`*); *Exceptions* sections removed from `RH5409.md`, `RH5410.md`, `RH5411.md`, `RH8201.md`, which stated the old silence. No other rule page states it.

## Examples for the tests

SHARED-DIAG-008 (RH5412, RH7105, RH7106 `#pragma`, RH5409 `#region` with fix result), SHARED-TRAIL-002, SHARED-ATTR-019, SHARED-ATTR-020, SHARED-EMPTY-001, RH5102-004, RH5109-004, RH5111-005, RH7101-001, RH7102-001, RH7103-001, RH7105-001, RH7106-001, RH7107-001, RH7108-001, RH7311-001, RH8201-003, RH7003-001, RH2005-001.

Existing tests that assert the old silence (found by tag and name, not exhaustive): RH7102 `ConstantWhoseModifiersAnIfSplitsInto*IsNotReported`; RH7103 `StaticMemberWhoseStaticModifierAnIfSplitsIsNotReported`; RH7105/RH7106 `Misordered*SplitByAConditionalDirectiveAreNotReported`, `ConditionalDirectiveThatStandsBetweenModifiersWhereTheSymbolIsDefinedKeepsTheDeclarationFromBeingReported`; RH7107/RH7108 `*KeepsThe*AccessorFromBeingReported`, `*KeepsItFromBeingReported`; RH5102 `VerifyConditionalDirective*KeepsTheListFromBeingReported*`; RH5109 the same; RH5111 `VerifyDeclaratorWithConditionalDirective*IsNotReported`, `VerifyConditionalDirectiveInTheJudgedSpanKeepsTheDeclaratorFromBeingReportedButNotOneInsideTheValue`; RH7101 `FieldDeclarationWithA*ConditionalDirective*IsNotReported`, `FieldDeclarationWithAConditionalDeclaratorBetweenItsDeclaratorsIsNotReported`; RH5409 `VerifyEnumWithConditionalBlockAfterFinalMemberIsNotFlagged`, `VerifySpecificationExampleReportsCommaInFrontOfRegionButNotInFrontOfConditionalBlock`, `VerifyCommaInFrontOfEmptyConditionalBlockIsNotReported`, `VerifyCommaInFrontOfElseAndElifBranchesIsNotReported`, `VerifyCommaInFrontOfInactiveIfFalseBlockIsNotReported`; RH5410/RH5411 `*WithConditionalBlockAfterFinalItemIsNotFlagged`; RH7311 `SameDescriptionInIfAndElseBranchIsNotReported*`, `SameDescriptionInIfAndElifBranchIsNotReported`, `RegionInABranchInsideADeclarationThatAConditionalDirectiveEnclosesIsNotCompared`; RH8201 `VerifyNoDiagnostic*Conditional*`, `VerifyNoDiagnosticWhenInterfaceMemberIsConditional`, `VerifySpecificationExampleConditionalInterfaceListSuppressesOnlyImplicitImplementations`; and the SHARED-ATTR-019/020 tests that assert a conditional directive keeps a list unreported.

How the statements were established:

- Baseline on `340608b` with `scripts/apply-fix.sh <ID> <dir>` on one fixture per rule (the inputs of the change doc): `no-diagnostic` for RH5102, RH5109, RH5111, RH5409, RH5506, RH5524, RH7101, RH7102, RH7103, RH7105, RH7106, RH7107, RH7108, RH8201; `no-fix-offered` for RH5412, RH5416, RH2005 (issue's example), RH2001 (`#if DEBUG set; #endif` in the accessor list). RH8201 without the `#if` in the base list: `fixed`. RH7311 and RH7003 have no fix provider; their current behavior is taken from their tests.
- `scripts/format.sh` on the RH5102/RH5109 inputs: the formatter breaks `M(1, 2, #if … 3)` and `P(int a, int b, #if … int c)` exactly as it breaks the `#pragma` variants, which RH5102-004/RH5109-004 already report, so SHARED-DIAG-010 does not keep the new reports silent. It keeps the gap around `=` for RH5111 (hence the extended exception), keeps attribute lists with `#if` or `#pragma` unsplit (both variants alike; the `#pragma` variant is reported today), and keeps the comma in front of `#if` (FMT-STRUCT-019).
- `scripts/apply-fix.sh RH5409` on the SHARED-DIAG-008 `#region` example: `fixed`, output as written in the spec.
- `scripts/test.sh --project architecture`: 185 passed. `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RuleDocumentation"`: 7 passed.
- Examples of behavior not built yet (the new reports) compile; their unchanged parts are today's inputs. Not verified: the RH7107/RH7108 example's claim that SHARED-ORDER-003 refuses the move (the `#endif` is leading trivia of the moved accessor and its `#if` lies in the crossed accessor's leading trivia) — it cannot run until the analyzer reports.

## Output questions

The change doc's *Visible output changes* covers every change; these three are where the issue's text leaves room for a different reading.

**1 · RH5409 · Final enum members must not have trailing commas (also RH5410, RH5411) — comma in front of an `#if` block**

Input (build without `DEBUG`):
```csharp
enum E
{
    A,
#if DEBUG
    B,
#endif
}
```
Before (`main`): not reported.

After, A (recommended):
```csharp
enum E
{
    A{|RH5409:,|}
#if DEBUG
    B,
#endif
}
```
reported, no fix offered.

After, B: not reported (SHARED-TRAIL-002 keeps its own silence).

Why A: it is the decided principle; the comma is a trailing comma in the build analyzed, and the author can move it into the branch. B would be the one rule left silent for the reason the decision overruled. Requirement IDs: SHARED-TRAIL-002, SHARED-DIAG-008.

**2 · RH7311 · Region descriptions must be unique within a type — region inside a compiled branch**

Input (build without `DEBUG`): the class of the change doc's RH7311 section (`#region Fields` in the `#if DEBUG` and in the `#else` branch, and `#region Fields` outside).

Before: not reported. After, A (recommended): the region outside is reported, since it repeats the description of the compiled `#else` branch's region; branch regions are never compared with each other. After, B: not reported (RH7311 keeps comparing only regions outside every branch).

Why A: RH7311's silence is a directive-silence guard of its own, which the issue includes; the editor outline of that build shows two `Fields` entries. Requirement IDs: RH7311-001, SHARED-TYPEREGION-006.

**3 · RH8201 · The inheritdoc tag should be used if possible — implicit implementation in a file with `#if`**

Input: the change doc's RH8201 section. Before: not reported. After, A (recommended): reported, no fix offered. After, B: not reported (RH8201-003 unchanged).

Why A: same principle; the fix stays withheld because `<inheritdoc/>` could leave the member undocumented in a build whose interfaces differ. Requirement IDs: RH8201-003.

None of the three blocks writing tests for the other rules; the run can continue on the recommendations.

## Internal choices

- Fix half of SHARED-DIAG-008 bounded by the part of the judged span the fix rewrites or moves, not by "any conditional directive in the span" — the blanket reading would withdraw RH2005's fix for an excluded modifier in front of the first modifier (offered today) and contradict SHARED-ORDER-006 for RH7107/RH7108. Alternative: blanket withholding (recorded as rejected in the change doc).
- RH7107/RH7108 fixes decided by the SHARED-ORDER requirements alone; no new guard. Alternative: withhold for any conditional directive outside the bodies.
- SHARED-TRAIL-002, SHARED-ATTR-019, SHARED-ATTR-020, and RH8201-003 changed in place rather than retired and replaced, so the rule specs that use them and the tests tagging them keep resolving. Alternative: retire and add new IDs.
- The empty-type clarification is stated once in SHARED-EMPTY-001, not in the six rule specs the issue comment asks for. Alternative: one sentence per rule spec.
- RH7102-001's first sentence ("contains no `#if` ...") was broader than the analyzer, which only checks the modifier list; removed with the reversal.
- Silences kept because their reason holds in every build (listed in the change doc's *Decisions*): SHARED-JOIN-001 and its rules, RH5307-002, RH5408-003, RH7310, SHARED-USING-009. RH7309 already judges the build's regions.

## Follow-up candidates

**RH7201–RH7207 · using directive ordering — a using block that holds a preprocessor directive** (SHARED-USING-009)

Input:
```csharp
using System.Linq;
#if DEBUG
using System.Diagnostics;
#endif
using System;

class C
{
}
```
Before (`main`): not reported (any directive in the block silences the rules).

After, A: the out-of-order directive is reported, no fix offered. After, B: unchanged.

Why it may be wrong: the ordering rules are content rules, which SHARED-DIAG-010 says report what they judge also where the formatter keeps the code, and *Code-fix coverage* makes a diagnostic without a fix a supported end state; SHARED-USING-009's stated reason is that the fix and the formatter cannot produce the order. Outside #1759: its silence is not a build-configuration reason. Recommendation: file it for the maintainer, no recommendation on the outcome.

## Architecture findings

_None._ Source comments citing SHARED-DIAG-008 for the old silence (`OrderingDeclarationUtilities`, `AccessorOrderingUtilities`, the RH5102, RH5109, RH5111, RH7101, RH7102, RH7103, RH7105, RH7106, RH7107, RH7108 analyzers, `TargetAttributeListShapeAnalyzerBase`, `TrailingCommaAnalyzerBase<TNode>`) become stale with the code change; the implementer owns them.
