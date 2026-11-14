Role: spec author
Commit: 4633e2a
Verdict: #1264: spec written; #1333: spec written; #1334: spec written; #1339: spec written; #1337: spec written; #1412: no spec change; #1592: no spec change

All changes are clarifications: each aligns a rule's or shared spec's own text with a shared requirement that already demands the behavior (SHARED-TYPEREGION-006, SHARED-DIAG-006, SHARED-DIAG-008, SHARED-DIAG-009), as the triage comments ask. No requirement ID was added, retired, or renumbered, so `requirement-ids.txt` and `sdd/specs/README.md` are unchanged. No change doc is needed. No rule page was changed (see *Internal choices* of #1264). `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"` passes (37/37).

The prompt carried no intended fix or verdict beyond the issues and triage comments.

## thoenissen/Reihitsu#1264

### Changed requirements

- RH7308-001 — A member of a kind the label does not name is reported — clarified: a member is judged by the innermost region that encloses it (top-level or nested); the region around it never judges it instead, also when the innermost label is not recognized. Intro sentence aligned. Was: "outside any region nested in it", which left members of nested regions unjudged.
- RH7311-001 — A repeated description is reported — clarified: "outside any `#if` branch" spelled out as outside every branch (`#if`, `#elif`, `#else`) of a conditional directive; a region in a branch is compared with no other region. Meaning unchanged; example added for FC-002.
- RH7311-002 — Only the top-level regions of one declaration are compared — clarified: a region of a nested enum is listed with the nested types.
- SHARED-REGGROUP-001 — Class, struct, and record declarations are checked — clarified: the rules judge a member by the innermost region (top-level or nested in one), per SHARED-TYPEREGION-006. Example added (the input from the issue comment).
- RH7401-001 … RH7412-001 — clarified: "lies in no region nested in a top-level region … does not lie in a top-level region whose name matches" replaced by "the innermost region that encloses it does not have a name that matches"; RH7405-001 … RH7408-001 "lies in a top-level region, and in no region nested in it, that is an override region" replaced by "the innermost region that encloses it is an override region". Intro sentences aligned.
- RH7405-002 … RH7408-002 — Other members and other regions are not reported — clarified in the same way.
- RH7309 — no text change: RH7309-001/-002 already defer to SHARED-TYPEREGION-006, which states that a nested enum owns its regions. The enum defect is in `RegionDirectiveUtilities.GetTopLevelRegions` (issue comment of 2026-11-06).

### Examples for the tests

RH7308-001 (two new examples: `Value`/`_other` in nested `Properties`; nested `Helpers` not checked), RH7311-001 (new `#if`/`#else`/outside example), RH7311-002, SHARED-REGGROUP-001 (new `M`/`N` example), RH7401-001 (pointer to SHARED-REGGROUP-001), RH7405-001 (new nested example, `N` reported, `O` not). The enum inputs are the SHARED-TYPEREGION-002 and SHARED-TYPEREGION-006 examples (RH7305, RH7311); for RH7309 the test implementer derives them from the issue comment of 2026-11-06 (a region in `C` and one in nested `enum E`; two regions wholly inside `E`), both expected not reported.

### Output questions

**Q1 · RH7309 · Regions should follow a fixed category order — regions in two branches of one conditional directive**

Input:
```csharp
using System;

public class C : IDisposable
{
    public void Dispose()
    {
    }

#if DEBUG
    #region IDisposable

    private int _a;

    #endregion
#else
    #region Fields

    private int _b;

    #endregion
#endif
}
```

Before (`main`): expected to report `#region Fields` (the analyzer does not look at branches; the skeptic should confirm).

After, A (recommended): not reported.

After, B: `#region Fields` reported, no fix (RH7309-005 withholds it).

Why A: no build compiles both branches, so the two regions never stand in one outline; SHARED-TYPEREGION-006 says so for comparing descriptions, and RH7309's category is read from the description. The issue's "mutually exclusive branches are not compared" covers it, but RH7309's own text does not decide it, so I did not write it.

### Internal choices

- RH7308, innermost label not recognized: a member in a nested `Helpers` region inside `Fields` is not checked; `Fields` does not judge it instead. I read this as decided by SHARED-TYPEREGION-006 ("judge it by the innermost region") together with RH7308-002 ("a region whose label is not recognized MUST NOT be checked"), and wrote it with an example. If the lead reads a fallback to the nearest recognized region as a competing reading, it becomes an output question: input as in the new RH7308-001 example; A not reported (recommended); B `Value` reported against `Fields`.
- Rule pages left unchanged. The pages of RH7308 and RH7401–RH7412 say "top-level region", and RH7405–RH7408 say "used by override methods"; in the normal case (flat regions, one member kind) nothing changes for the user, so per the role file they stay. Alternative: drop "top-level" and say "overrides" in a short docs touch in this PR through `/create-rule-doc`.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1333

### Changed requirements

- SHARED-REGGROUP-005 — The diagnostic is on the member's name — clarified: each declarator of a field-like event is judged on its own and reported on its own identifier (SHARED-DIAG-006), instead of "a field-like event with one declarator". New RH7411 example.
- RH7403-001, RH7407-001, RH7411-001 — clarified: "a field-like event with one declarator" replaced by "each declarator of a field-like event declaration, judged on its own (SHARED-REGGROUP-005)"; RH7407-001 also: any declarator of an override field-like event makes a region an override region.
- SHARED-REGGROUP-006 — An override region — counts every declarator of an override field-like event (see #1334).

### Examples for the tests

SHARED-REGGROUP-005 (new RH7411 example: `A, X` in region `IA`, `X` reported; `Y, Z` outside, both reported). The issue's five inputs, each also with the declarators in the opposite order, as its "Done when" asks.

### Output questions

_None._ The verdicts follow from SHARED-DIAG-006, as the triage comment says.

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1334

### Changed requirements

- SHARED-REGGROUP-006 — An override region — clarified to SHARED-DIAG-009: a region (top-level or nested) is an override region when its name matches the type that an override of **any** member kind in **any** declaration of the type comes from; the "analyzed type declaration" vs "any declaration" inconsistency is resolved for any declaration. Two new RH7405 examples (#1304 Q1: a property override makes `B` an override region for methods; #1304 Q4: the override in the other part).
- SHARED-REGGROUP-004 — An implementation comes from the interface that declares the member — clarified to SHARED-DIAG-009: an implicit implementation of members of several interfaces comes from each, and a region named after any one matches, whatever the base-list order (#1304 Q2; the example is SHARED-DIAG-009's).
- RH7405-001 … RH7408-001 — "override region of methods/properties/events/indexers" replaced by "override region" (SHARED-REGGROUP-006); intro sentences say "one of the type's overrides".
- RH7409-001 … RH7412-001 — accept a region named after any of several interfaces (SHARED-REGGROUP-004).
- RH7401-001 … RH7412-001, partial paragraph — "This requirement covers a `partial` … only when each of its parts … would be reported; … is not covered" replaced by "It is reported when any of its parts, judged on its own by the region it lies in, would be reported: every part has to lie in a matching region" (RH7401–RH7404, RH7409–RH7412) or "no part may lie in an override region" (RH7405–RH7408). The text "that is not `partial`" the triage comment names was already gone on `main`; the "not covered" sentence that replaced it is what this run removes. The part that carries the one diagnostic is SHARED-DIAG-012 (rule 2: a part that would be reported alone, the defining part first). New examples in RH7401-001, RH7405-001, RH7409-001.
- SHARED-REGGROUP `Uses` gains SHARED-DIAG-006 and SHARED-DIAG-009.

### Examples for the tests

SHARED-REGGROUP-006 (two new examples), SHARED-REGGROUP-004 / SHARED-DIAG-009 (both base-list orders), RH7401-001 (defining part in `B`, implementing part in `Other`: reported on the implementing part), RH7405-001 (defining part in `Methods`, implementing part in override region `B`: reported on the implementing part), RH7409-001 (`M` reported on the defining part, `N` on the implementing part).

### Output questions

**Q2 · RH7401–RH7412 · partial member with only some parts in place**

The triage comment of 2026-10-24 decides this one ("a partial member counts as one member whose parts must all lie in a matching region, and it is reported once"), and the issue body records it as the maintainer's round-8 decision. I wrote the spec on it. It is listed so the lead can confirm the triage comment counts as the answer; for RH7405–RH7408, "matching" read as "outside every override region" is my analogue, not the comment's words.

Input (RH7409):
```csharp
interface I
{
    void M();
}

partial class C : I
{
    #region I

    public partial void M();

    #endregion

    #region Methods

    public partial void M()
    {
    }

    #endregion
}
```

Before (`main`): not reported (judged at the defining part; pinned by the RH7409/RH7410 partial tests).

After, A (recommended, written): `public partial void {|RH7409:M|}()` — the implementing part is reported.

After, B: not reported.

Why A: the member is in its place only when every part is; today the verdict flips when the two parts swap regions (SHARED-DIAG-009).

### Internal choices

- "Any member kind" in SHARED-REGGROUP-006 is read from SHARED-DIAG-009's third bullet (a verdict must not depend on whether the type declares a member of another kind) together with the issue's Q1 expectation and the triage comment; the alternative reading (only overrides of the rule's own kind) is what the old text said and what the triage comment says to align away.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1339

### Changed requirements

- RH7102-003 — Fields in one top-level region are compared — clarified: an explicit MUST NOT for a constant and a field in two top-level regions, or one in a region and one outside every region; example with both inputs (the issue's and the one carried over from #1664).
- RH7109-003 — Fields in one top-level region are compared — clarified in the same way, with the issue's input.

RH7102-001 and RH7109-001 already state the scope ("both lie outside every region, or both lie in the same top-level region"), and SHARED-ORDER-004 decides it; the text did not disagree, it only lacked the negative example FC-002 asks for.

### Examples for the tests

RH7102-003 (new example, classes `C` and `D`), RH7109-003 (new example, classes `C` and `D`). The existing test `NoCodeFixWhenDirectivesAreInLeadingTrivia` (field outside every region, constant in a region) then expects no diagnostic, as the issue comment asks.

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1337

### Changed requirements

- RH5102-004 — A comment or directive in the argument list withholds the fix — clarified to SHARED-DIAG-008: an argument list with a conditional directive, or code it excludes, between its parentheses is not reported; a comment or another directive (`#region`, `#pragma`) there still withholds only the fix; anything on its own line in front of `(` still withholds only the fix. New example. `Uses` gains SHARED-DIAG-008.
- RH5109-004 — the same for the parameter list. New example. `Uses` gains SHARED-DIAG-008.
- RH5111-005 — clarified: the judged span is from the target's last token through the value's first token (the gaps around `=`, which RH5111-001/-002 judge); a conditional directive there means not reported; a comment or other directive there, and any comment or directive elsewhere in the construct, still withholds only the fix. New example. `Uses` gains SHARED-DIAG-008.
- RH7101-001, RH7105-001, RH7106-001, RH7107-001, RH7108-001 — no text change: each already excludes a conditional directive in its judged span. The analyzers do not check it at all (no directive handling in any of the five), so the defect is code only.
- SHARED-MODORDER-003 — not changed; see Q3.

Scope note for the lead: the three aligned layout texts make three tagged tests contradict their spec, and RH5102, RH5109, and RH5111 then need analyzer changes, although the issue body lists only RH7101 and RH7105–RH7108: `VerifyDirectiveInsideArgumentListWithholdsTheCodeFix` (RH5102-004), `VerifyDirectiveInsideParameterListWithholdsTheCodeFix` (RH5109-004), `VerifyDeclaratorWithDirectiveIsReportedWithoutCodeFix` (RH5111-005) — each holds an empty `#if`/`#endif` pair in the judged span and expects a diagnostic. The triage comment names these three requirements, so I treated them as part of #1337.

### Examples for the tests

RH5102-004, RH5109-004, RH5111-005 (new examples), and the issue's four inputs for RH7101-001, RH7105-001/RH7106-001, RH7107-001, RH7108-001 (all not reported). The tests the issue names (`MisorderedModifiersWithDirectiveAreReportedWithoutFix`, `MisorderedCompoundAccessModifiersWithDirectiveAreReportedWithoutFix`, `NoCodeFixWhenDirectivesAreInAccessorLeadingTrivia`, `NoCodeFixWhenDirectiveFollowsAccessorAttributeList`) should be checked against these texts: a `#pragma` or `#region` in the span stays reported without a fix.

### Output questions

**Q3 · RH7105 · Declaration keywords must follow order (and RH7106) — conditional directive between the last modifier and the type**

Input:
```csharp
class C
{
    static public
#if DEBUG
    readonly
#endif
    int _a;
}
```

Before (`main`): `static` reported, no fix offered (as SHARED-MODORDER-003 states: "a directive of any kind, a conditional directive included, stands between the last modifier and the token after it … The diagnostic is still reported").

After, A (recommended): not reported.

After, B: `{|RH7105:static|}` reported, no fix offered (as today).

Why A: the excluded `readonly` is a modifier in another build, so the gap behind the last modifier is part of the modifier list the rule judges (SHARED-DIAG-008), as RH7102-001 already defines its own judged span ("from its first modifier up to the token that follows its last modifier"). If A, SHARED-MODORDER-003's clause "a conditional directive included" is narrowed and RH7105-001/RH7106-001 say "between or behind its modifiers".

**Q4 · RH5111 · Assignments must have proper line breaks — conditional directive inside the value**

Input:
```csharp
class C
{
    void M()
    {
        var other
            = N(1,
#if DEBUG
#endif
                2);
    }

    int N(int a, int b) => a + b;
}
```

Before (`main`): reported, no fix offered.

After, A (recommended, written): reported, no fix offered (unchanged).

After, B: not reported.

Why A: RH5111 judges where `=` stands relative to the target and where the value starts relative to `=`; a directive after the value's first token cannot change either in any build, so it lies outside the judged span. The case where the directive sits between the target and `=` (the existing test) is not reported under both readings.

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1412

No spec change: SHARED-MODORDER-001 lists constructor declarations, and RH7105-001 / RH7106-001 judge each part of a `partial` member by its own modifiers (SHARED-DIAG-004, second paragraph). The issue's input is the test seed: both parts of `internal protected partial C();` / `internal protected partial C() { }` reported by RH7106, each on its own `protected`; the same with `static public`-style order for RH7105.

## thoenissen/Reihitsu#1592

No spec change: SHARED-FIX-005 forbids a fix whose result equals its input, and RH7309-004 already says the fix reorders the top-level region blocks of the type declaration, however a block begins. Shape 1 (a block that starts with a nested type) is fixed as RH7309-004 states; shape 2 (identity permutation) has no fix. Note for the skeptic: shape 2 needs a reported type whose blocks are already in order, i.e. analyzer and fix disagree about the top-level regions; after #1264's enum fix in `GetTopLevelRegions` it may no longer be reachable, which decides whether it gets a reproduction or a characterization test.

## Whole run

- Files changed: `sdd/specs/shared/member-region-grouping.md`, `sdd/specs/analyzer/RH7308.md`, `RH7311.md`, `RH7401.md` … `RH7412.md`, `RH7102.md`, `RH7109.md`, `RH5102.md`, `RH5109.md`, `RH5111.md`.
- Output questions Q1, Q3, Q4 do not block the run: the specs are written on the recommendations or left unchanged where noted (Q1: RH7309 text untouched; Q3: SHARED-MODORDER-003 untouched); the main inputs of every issue are decided. Q2 is written on the triage comment's decision.
