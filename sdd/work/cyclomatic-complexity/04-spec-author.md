Role: spec author
Commit: c1e9ebc
Verdict: open question

Applies Maintainer answers 1–22 (19 superseding 10, 15, 16 for lambdas, anonymous methods, and `&`/`|`/`^`; 20 for parenthesized chained `?:`; 21 for the guide; 22 keeps initializer lambdas unjudged) and Decisions 1–4 to the spec, change doc, shared specs, rule page, and the new scoring guide. The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore. The change doc stays `Status: Draft`. The four output questions below are written into the spec on their recommendation; the concept goes back to the maintainer before tests anyway, so they can be asked in the same batch.

Files:
- `sdd/specs/analyzer/RH2009.md` — rewritten for the answers.
- `sdd/specs/shared/configured-limits.md` — new shared spec SHARED-LIMIT (decision 2): SHARED-LIMIT-001 (a setting without a valid value keeps its default), SHARED-LIMIT-002 (reported when greater than the limit). Code owners `—`, see *Internal choices*.
- `sdd/specs/shared/line-count.md` — SHARED-LINES-002 reduced to "each kind of code has a limit of its own", referencing SHARED-LIMIT-001/002; `Uses` updated. RH2008 and RH2111 reference SHARED-LINES-002 unchanged.
- `sdd/specs/analyzer/RH0001.md` — RH0001-007 *Why* references SHARED-LIMIT-001; `Uses` now SHARED-LIMIT-001, SHARED-LINES-003.
- `sdd/specs/README.md` (row), `sdd/specs/requirement-ids.txt` (SHARED-LIMIT-001, SHARED-LIMIT-002).
- `sdd/changes/2026-11/method-complexity-rule.md` — rewritten; every departure from the paper under *Alternatives considered*.
- `documentation/rules/RH2009.md` — description updated (chained `?:`, `&`/`|`/`^` on bool, `!`, recursion, lambdas count, local functions on their own, exception wording per answer 4) and links the guide; examples unchanged (16 / 1, 3, 2 still hold).
- `documentation/cognitive-complexity.md` — new scoring guide.

Checks:
- Every C# block of the spec, the two shared specs, the change doc, the guide, and the rule page (markup removed; rule-page methods wrapped in a class) compiles with the .NET 11 SDK, `FEATURE` defined: `Build succeeded`. 49 blocks in all.
- Every score was recomputed by hand under the final rules; they are listed in the files as per-line `// +n` comments (guide, change doc) or in the prose (spec). The test implementer takes them from the spec, not from the analyzer.
- `scripts/test.sh --project architecture`: 195 passed, 3 failed, all expected before the analyzer exists: `AnalyzerSpecHeaderMatchesItsRule`, `EveryCodeOwnerNamesAnExistingType` (the RH2009 analyzer), `EveryRequirementOfAnEnforcedSpecIsCovered` (RH2009-001 to -006 and SHARED-LIMIT-001/002 have no tests). `Uses`, ledger, and index checks pass.
- Documentation tests: `RuleDocumentationConventionTests`, `RuleDocumentationExampleDiscovery`, and `AnalyzerMetadataDiscovery` read only `documentation/rules/RH*.md`; nothing forces `documentation/cognitive-complexity.md` into rule-page form. `SquadNoteReferenceTests` scans every `.md` but only for references to `squad/notes/`; the guide has none. The guide names no tracker number and does not mention the formatter.

## Changed requirements

- RH2009-001 — The limit is `design.maximumMethodComplexity` — changed: compares through SHARED-LIMIT-001/002 instead of restating them.
- RH2009-002 — The members judged — changed: references RH2008-002 and RH2008-004 (decision 1).
- RH2009-003 — Cognitive complexity of a body — changed: scored part excludes local functions; chained `?:` (also in parentheses) scores 1 without nesting; `&`, `|`, `^` on `bool`/`bool?` count in sequences; `not` breaks a pattern tree; combinators in case labels and arms score nothing, `is` in a `when` clause counts; recursion 1 once, direct only, by invocation; switch nested part includes labels, patterns, `when`; catch filter and query clauses do not nest; `#if` scores nothing.
- RH2009-004 — A body without a nesting increment is not reported — changed: nesting only through lambdas/anonymous methods does not make a member reportable; chained `?:` bullet.
- RH2009-005 — Lambdas count toward the member that holds them, local functions do not — changed (title and content: answers 3, 19, 22).
- RH2009-006 — One diagnostic per member, located on its name — changed: references RH2008-005 (decision 1).
- SHARED-LIMIT-001 — A setting without a valid value keeps its default — added (moved from SHARED-LINES-002 and RH2009-001; decision 2).
- SHARED-LIMIT-002 — A declaration is reported when its measure is greater than its limit — added (same).
- SHARED-LINES-002 — Each kind of code has a limit of its own — changed: wording only, references SHARED-LIMIT; behavior identical.
- RH0001-007 — clarified: *Why* references SHARED-LIMIT-001.

## Examples for the tests

RH2009-001 to RH2009-006 (every example), SHARED-LIMIT-001 and SHARED-LIMIT-002 (RH2008 examples; existing RH2008 tests of the default and the boundary can carry the new tags), and RH0001-007's `maximumMethodComplexity` cases. Beyond the examples, `02-skeptic.md` *Breaking inputs* and `03-skeptic.md` additions stand, with: a self-call through `this.M`, `C.M`, a generic `M<T>`, another overload, a method group, and inside a lambda (counts) or a local function (does not count for the holder); `&` on `bool?`; `&=` on `bool`; a `?:` inside a `when` clause; a local function inside a lambda.

## Output questions

All within thoenissen/Reihitsu#352. Scores are by hand from RH2009 as written; "Before (`main`)" is "not reported" throughout, because RH2009 does not exist on `main`. Every input compiles with the .NET 11 SDK.

**1 · RH2009 · Methods should not exceed the complexity limit — `&` and `|` on `bool?`**

Answer 19 counts `&`, `|`, `^` on `bool`; `bool?` (three-valued logic) is not named.

Input, with `{ "design": { "maximumMethodComplexity": 3 } }`:
```csharp
class C
{
    bool M(bool? a, bool b)
    {
        if (b)
        {
            if ((a & b) == true)
            {
                return true;
            }
        }

        return false;
    }
}
```

Before (`main`): not reported.

After, A (recommended) — `&` on `bool?` counts like `&` on `bool`: `if` 1, `if` 2, `&` 1 = 4:
```csharp
bool {|RH2009:M|}(bool? a, bool b)   // "This member has a cognitive complexity of 4, which exceeds the limit of 3."
```

After, B — only `bool` operands count: 3:
```csharp
bool M(bool? a, bool b)   // not reported
```

Why A: it is the same logical operator on the same conditions; only the null case differs. Requirement: RH2009-003.

**2 · RH2009 · Methods should not exceed the complexity limit — `&=`, `|=`, `^=` on `bool`**

Input, with `{ "design": { "maximumMethodComplexity": 3 } }`:
```csharp
class C
{
    bool M(bool[] checks, bool strict)
    {
        var ok = true;

        if (strict)
        {
            foreach (var check in checks)
            {
                ok &= check;
            }
        }

        return ok;
    }
}
```

Before (`main`): not reported.

After, A (recommended) — a compound assignment is an assignment, not a condition: `if` 1, `foreach` 2 = 3:
```csharp
bool M(bool[] checks, bool strict)   // not reported
```

After, B — `&=` counts as a sequence of its own: 4:
```csharp
bool {|RH2009:M|}(bool[] checks, bool strict)   // "This member has a cognitive complexity of 4, which exceeds the limit of 3."
```

Why A: the paper counts sequences of binary operators in a condition; `ok &= check` accumulates a result and is read as one statement. Requirement: RH2009-003.

**3 · RH2009 · Methods should not exceed the complexity limit — recursion through an operator, accessor, or constructor**

Answer 14 scores 1 once for a member that calls itself directly. Methods and local functions call themselves by an invocation; an operator, a property, or a constructor reaches itself through an operator, a property access, or `new`.

Input, with `{ "design": { "maximumMethodComplexity": 3 } }`:
```csharp
class Node
{
    public Node Child;

    public Node(int depth)
    {
        if (depth > 0)
        {
            if (depth % 2 == 0)
            {
                Child = new Node(depth - 1);
            }
        }
    }
}
```

Before (`main`): not reported.

After, A (recommended) — only an invocation `M(…)` of the judged method or local function itself counts: `if` 1, `if` 2 = 3:
```csharp
public Node(int depth)   // not reported
```

After, B — every use that binds to the member itself counts: `new Node(…)` in its constructor, its own property in an accessor, its own operator in an operator: 4:
```csharp
public {|RH2009:Node|}(int depth)   // "This member has a cognitive complexity of 4, which exceeds the limit of 3."
```

Why A: one recognizable form, the call a reader sees as recursion; a property or operator that uses itself is nearly always endless recursion, and a recursive constructor is rare. Requirement: RH2009-003.

**4 · RH2009 · Methods should not exceed the complexity limit — a `?:` inside a `when` clause**

Input, with `{ "design": { "maximumMethodComplexity": 3 } }`:
```csharp
class C
{
    int M(int value, bool strict)
    {
        switch (value)
        {
            case > 0 when (strict ? value > 10 : value > 1):
                return 1;

            default:
                return 0;
        }
    }
}
```

Before (`main`): not reported.

After, A (recommended) — a `when` clause belongs to its section, so the `?:` is nested in the switch: `switch` 1, `?:` 2 (nesting 1) = 3; not reported at 3, reported at 2 ("… of 3, which exceeds the limit of 2."):
```csharp
int M(int value, bool strict)   // not reported
```

After, B — a `when` clause is a condition, like an `if`'s, and does not nest: `switch` 1, `?:` 1 = 2, and the body has no nesting increment, so it is never reported:
```csharp
int M(int value, bool strict)   // not reported, also at limit 1
```

Why A: the case label and its `when` clause are read inside the switch, as part of the decision the switch opened. Requirement: RH2009-003, RH2009-004. (Decision 3 settled that the `&&` in a `when` clause counts; this is the nesting of a structure in one, which the skeptic noted as rare.)

## Internal choices

- **SHARED-LIMIT as a new shared spec** (decision 2) rather than widening `shared/line-count.md`: RH2009 counts no lines, has one setting for both kinds of code, and would otherwise use a line-count spec. SHARED-LINES-002 keeps its ID and states only the separate test/production limits. Alternative: widen SHARED-LINES-002's wording to "count or complexity".
- **SHARED-LIMIT code owners `—`**: today `LineCountAnalyzerBase.ResolveLimit` implements it, and that type owns SHARED-LINES. Decision 2 has the implementer share the configuration-reading code; the shared type it extracts becomes the code owner of SHARED-LIMIT, and the implementer (or spec checker) puts it in the header and the index row.
- **Decision 1's shared member mapping** (member → body, expression body, name token) implements RH2008-002/004/005, so the extracted type is a code owner of `analyzer/RH2008.md`; add it to that header and the index row when it exists. RH2009's spec references the three requirements and no longer restates them.
- **Recursion detection by symbol**: an invocation whose target is the member's own symbol (original definition for generic methods); `base.M()` binds to another symbol and does not count; a self-call inside a lambda counts (the lambda body is part of the scored part); a call of the holder from inside a local function counts for neither.
- **`?:` chain parentheses**: "one or more pairs of parentheses" for answer 20's "parentheses directly around the false branch" — `((…))` is still nothing but parentheses.
- **`when` clause of a switch arm/label is in the switch's nested part; a catch filter is not** (the catch's nested part is its block, like a condition). Q4 asks the switch case because it changes a score; the catch-filter case is the same reading as conditions everywhere else.
- **Lambdas in a constructor initializer** (`: base(x => …)`) are not judged and count toward nothing, as for field initializers (RH2008-002 excludes the initializer from the body).
- **Rule page**: kept to a summary list and a link; the lambda-does-not-lift-the-exception nuance is on the guide only (a corner case by the rule-page convention). The page now says "a method that calls itself adds 1".
- **Guide** is a plain documentation page (not a rule page): no rule-page table or footer; it links the paper by URL and names it as the source, and lists the departures.

## Follow-up candidates

- Already on the lead's list: self-hosting violations and the default; judging lambdas in field, property, and event initializers (answer 22).
- Indirect recursion (the paper's full rule) if wanted later; needs a compilation-end analysis.
