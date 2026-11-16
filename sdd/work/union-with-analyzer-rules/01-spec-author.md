Role: spec author
Commit: 18437a7
Verdict: change doc written

The prompt carried no intended fix or hoped-for verdict. thoenissen/Reihitsu#1761 is closed (merged in #1779): Roslyn 5.12 is on `main`, and the analyzer tests compile unions through `UnionSupport`.

## How the current behavior was established

A scratch harness (outside the repository) ran every analyzer of the Debug build of `Reihitsu.Analyzer` over probe files compiled as C# preview against the .NET 11 runtime assemblies (unions compile there without errors), and `scripts/format.sh` / `scripts/apply-fix.sh` ran the formatter and fixes on copies. Each union input was compared with the same shape on a struct or class. Findings on `main`:

- Not reported for a union, reported for the struct/class twin: RH2004 (the union declaration itself), RH5404, RH7102, RH7103, RH7104, RH7105, RH7106, RH7109, RH7110, RH7305, RH7308, RH7309, RH7311, RH7401, RH7409 (RH74xx share one base), RH5105, RH5106, RH5108, RH5110, RH5507, RH5508.
- Already reported for a union: RH2004 on a member declared in a union body; RH2101 on a class nested in a union; RH5107, RH5109 (fixes converge, apply-fix), RH5035 on the case-type list, including a comment between the name and `(`.
- Already reported for `with(…)`: RH5101, RH5102, RH5203, RH5038, RH5305; their fixes give the formatter's layout (apply-fix). RH5106 does not check argument lists; RH6001 does not report `with(`.
- The compiler applies union attributes to the struct target: `[AttributeUsage(AttributeTargets.Class)]` on a union fails with CS0592, `AttributeTargets.Struct` compiles.
- Formatter: `[Serializable, Obsolete] public union U(int, string);` keeps one list (a struct gets one per list); union case-type lists `U\n(…)`, `U(\n…)`, `U(…\n)` are joined; `where` gets its own line.
- Union members the compiler accepts: methods, non-auto properties, indexers, operators, constructors (not a public single-parameter one), non-field events, static and const fields, nested types; `partial`, `readonly`, `file`, accessibility modifiers. Instance fields, auto-properties, `ref`, `sealed`, `static` are compile errors.

## Changed requirements

Change doc: `sdd/changes/2026-11/union-analyzer-rules.md` (Draft). No requirement ID was added, so `requirement-ids.txt` and `sdd/specs/README.md` are unchanged.

- RH2004-001 — union reported — changed; RH2004-006 — `private` in a union body — changed (the member part writes down existing behavior)
- RH5404-001, RH5404-003 — union body on one line, fix — changed
- RH7102-001, RH7103-001, RH7109-001, RH7110-001 — union body ordered — changed
- RH7104-001, RH7104-004 — partial union; `private` for a type nested in a union — changed
- RH7308-001, RH7309-002, RH7311-001 (and its negative-case sentence on nested unions) — changed
- RH5105-001, RH5106-001, RH5108-001 — case-type list of a union — changed
- RH5110-001 — union constraint clauses — changed
- RH5507-001, RH5508-001 — union attribute lists (struct target) — changed
- SHARED-ATTR-001 — `union` resolves to the struct target — changed
- SHARED-MODORDER-001 — union checked by RH7105, RH7106 — changed
- SHARED-TYPEREGION-001, -002, -006 — union checked by RH7305/RH7305A; regions inside a union belong to it — changed
- SHARED-REGGROUP-001 — union checked by RH7401–RH7412 (heading renamed) — changed
- SHARED-NESTED-001 — union body counts as containing type — clarified
- RH5035-001, RH5107-001, RH5109-001 — union case-type list — clarified
- RH5101-001, RH5038-001, RH5102-001, RH5203-001 — `with(…)` argument list — clarified
- RH5305-002 — `with(…)` is an element — clarified
- FMT-STRUCT-028 — union example added — clarified (the requirement already names every type; today's output is a bug against it)
- RH6001 — new non-goal: the `with` of a `with(…)` element

Rule pages changed: `documentation/rules/RH5404.md` (drops "Union declarations are not reported either"), `RH5105.md`, `RH5108.md`, `RH5507.md`, `RH5508.md`. The other rules' pages name no type kinds and stay unchanged.

## Examples for the tests

Each changed or clarified requirement above has a new union or `with(…)` example appended (labelled "Example — a union" / "— a `with(…)` element" / "— the case-type list of a union"): RH2004-001, RH2004-006, RH5404-001, RH5404-003, RH7102-001, RH7103-001, RH7104-001, RH7104-004, RH7109-001, RH7110-001, RH7308-001, RH7309-002, RH7311-001, RH5105-001, RH5106-001, RH5108-001, RH5110-001, RH5507-001, RH5508-001, RH5035-001, RH5107-001, RH5109-001, RH5101-001, RH5038-001, RH5102-001, RH5203-001, RH5305-002, SHARED-ATTR-001, SHARED-MODORDER-001 (RH7105, RH7106 examples), SHARED-TYPEREGION-001, SHARED-TYPEREGION-002 (bodiless union not reported), SHARED-REGGROUP-001 (RH7401 in a union), SHARED-NESTED-001 (RH2101 in a union), FMT-STRUCT-028. Union tests need `UnionSupport` as an additional reference. Spans of the already-reported `with(…)` and case-list examples were checked against the harness; the spans of the new union diagnostics follow each rule's location requirement and cannot be checked before the implementation.

## Output questions

The change doc's *Visible output changes* 1–11 hold input, before, and after for every visible change. Grouped for the batch:

**Q1 · RH2004, RH5105, RH5106, RH5108, RH5110, RH5404, RH7102–RH7106, RH7109, RH7110, RH7305/RH7305A, RH7308, RH7309, RH7311, RH7401–RH7412 — do unions join the existing type-kind rules?** (issue question 1)

Input:
```csharp
union Choice(int, string) { int Get() => 1; }
```
Before: RH2004 on `Get` only.
After, A (recommended): RH2004 on `Choice` and `Get`, RH5404 on `Choice`; fixes give
```csharp
internal union Choice(int, string)
{
    private int Get() => 1;
}
```
After, B: unchanged; every listed rule keeps skipping unions, recorded as a decision in each spec.
Why A: each rule names a part a union has in the same form as a struct, and the formatter already lays it out that way. (The lead may split this by rule family; changes 1–9 of the change doc give each family's input and output.)

**Q2 · RH5507 · Struct attributes must be on their own line / RH5508 · Struct attribute lists must use one attribute per list, and the formatter — union attribute lists**

Input:
```csharp
[Serializable, Obsolete] public union U(int, string);
```
Before: not reported; formatter output `[Serializable, Obsolete]⏎public union U(int, string);`.
After, A (recommended): RH5507 and RH5508 on the list; formatter and RH5508 fix give
```csharp
[Serializable]
[Obsolete]
public union U(int, string);
```
After, B: new union attribute rules (new rule IDs, escalation 5), same output as A under other IDs.
After, C: not reported, and the formatter keeps one list (FMT-STRUCT-028 then needs an exception for unions).
Why A: the compiler applies a union's attributes to the struct target, which is what these rules are keyed by.

**Q3 · Empty union body — new rule in the RH5412–RH5416 family?** (issue question 2; escalation 5)

Input:
```csharp
public union U(int, string)
{
}
```
Before: not reported; the formatter writes `public union U(int, string);` (FMT-STRUCT-017, accepted in union-declaration-layout.md).
After, A (recommended): new rule RH5418 "Empty unions should use semicolon declarations" (Layout, Warning, enabled, from C# 15, using SHARED-EMPTY-001 to -004), reported on `U`, fix gives `public union U(int, string);`.
After, B: no rule; only the formatter converts it (P11), and RH5404 (after Q1 A) reports only the one-line form `{ }`.
Why A: every other type kind with a semicolon form has its own rule, and unions got their own IDs for naming and documentation. No spec is written for RH5418 until the answer.

**Q4 · Nested union — new rule in the RH2101–RH2108 family?** (issue lists RH2101–RH2107; escalation 5)

Input:
```csharp
public class C
{
    union U(int, string);
}
```
Before: not reported (a nested struct reports RH2104).
After, A (recommended): new rule RH2110 "Nested unions should not be used" (Design, Warning, enabled, no code fix, using SHARED-NESTED-001/-002), reported on `U`.
After, B: no rule; nested unions stay unreported.
Why A: each nestable type kind has its own nested-type rule, and a nested union is as hard to find as a nested struct. No spec is written for RH2110 until the answer.

Not in question (unchanged output, decided by accepted specs): issue question 3 — RH5305 keeps requiring a `with(…)` element on its own line in a wrapped collection expression; that is today's behavior and FMT-BREAK-022's. RH6001 stays silent on `with(` (FMT-SPACE-028 removes that space). RH2102/RH2103 (which the issue groups with the nested-type rules) are about primary constructors; a union's case-type list is not one, so nothing changes for them.

## Internal choices

- Widen the existing requirements' kind lists instead of adding union requirements — alternative: new per-rule union requirements; widening keeps one statement per rule and matches union-declaration-layout.md.
- One change doc for all union rules and the `with(…)` clarifications — alternative: two docs; the `with(…)` part changes no output and shares the issue.
- FMT-STRUCT-028 is treated as already covering unions ("a type"), so the formatter split is a bug fix against it and only gains an example — alternative: a formatter requirement change.
- RH5108 includes the union's case-type list although class/struct primary constructors are not checked — alternative: leave unions out with them; the issue names RH5108 and the record form is checked.
- The `with(…)` and case-list clarifications are written into the requirements so tests can tag them — alternative: tests only.

## Follow-up candidates

- **RH5108 · Parameter list must follow declaration — primary constructors of classes and structs.** Input `public class K1(⏎    int a, string b);`; real output: not reported, while RH5105/RH5106 check the same list and the formatter joins it to `public class K1(int a, string b);`. Readings: A — report `int` (rule's own gap, P11), B — keep unchecked. Recommendation A. Touches RH5108-001.
- **RH5110 · record struct constraint clauses** — `record struct S<T> where T : class;` is not reported (RH5110-001 lists no record struct); `sdd/README.md` P11 uses it as its example. The lead should check whether it is already filed.
