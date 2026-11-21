Role: spec author
Commit: 5ceb32f
Verdict: open question

The rule is cognitive complexity (Maintainer answers 1, overriding clarification 2), with the nesting increment growing with depth (Maintainer answers 2), default 15, configured in `reihitsu.json`, a no-nesting exception, and this repository's own violations left for a follow-up. The spec, change doc, and rule page are written on the recommendation of each output question below; the tests depend on the answers, hence `open question`. The prompt carried no intended fix to ignore.

Files:
- `sdd/changes/2026-11/method-complexity-rule.md` (Status: Draft)
- `sdd/specs/analyzer/RH2009.md` (new)
- `sdd/specs/analyzer/RH0001.md` (intro, non-goal, RH0001-007, `Uses` + RH2009-001)
- `sdd/specs/README.md` (row), `sdd/specs/requirement-ids.txt` (RH2009-001 to RH2009-006)
- `documentation/rules/RH2009.md` (new, UTF-8 with a BOM like its neighbours; its violation example has complexity 16, which the default reports, so it needs no entry in `RuleDocumentationExampleOptOuts`)

Rule ID: RH2009, the next free ID among the member-level Design rules (RH20xx; RH21xx holds type-level rules). Title "Methods should not exceed the complexity limit"; message "This member has a cognitive complexity of {0}, which exceeds the limit of {1}."; code owner `RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzer`. Setting `design.maximumMethodComplexity`.

Checks:
- Every C# block of the spec, change doc, and rule page (markup removed, each block in its own namespace) compiles with the .NET 11 SDK: `Build succeeded`. The complexities in the examples were worked out by hand from RH2009-003, because no analyzer exists yet; the test implementer's tests are the first mechanical check of them.
- `scripts/test.sh --project architecture`: 195 passed, 3 failed, all as expected before the analyzer exists: `AnalyzerSpecHeaderMatchesItsRule` (no rule RH2009), `EveryCodeOwnerNamesAnExistingType` (the analyzer type is missing), `EveryRequirementOfAnEnforcedSpecIsCovered` (RH2009-001 to RH2009-006 have no tests). The `Uses`, ledger, and index checks pass.

## Changed requirements

- RH2009-001 — The limit is `design.maximumMethodComplexity` — added
- RH2009-002 — The members judged — added
- RH2009-003 — Cognitive complexity of a body (increments, nesting level, the nesting increment growing with depth) — added
- RH2009-004 — A body without a nesting increment is not reported (the no-nesting exception, and how it fits the score) — added
- RH2009-005 — Nested functions count toward the member that holds them — added
- RH2009-006 — One diagnostic per member, located on its name — added
- RH0001-007 — The `design` section holds the line and complexity limits — changed: accepts `maximumMethodComplexity`

## Examples for the tests

RH2009-001 to RH2009-006 (every example), and the two new cases in the RH0001-007 example: `maximumMethodComplexity` accepted, `-1` reported as not a whole number from 1 to 2147483647, `maximumTestMethodComplexity` reported as unknown (changes if question 4 is answered B).

## Output questions

All within thoenissen/Reihitsu#352. The maintainer wants to review the concept before anything is built, so the lead may also present the change doc's *What* and *Visible output changes* as the concept. Each question below is a point the published definition of cognitive complexity and the maintainer's answers leave open for C#.

**1 · RH2009 · Methods should not exceed the complexity limit — local function**

Input, with `{ "design": { "maximumMethodComplexity": 3 } }`:
```csharp
class C
{
    int M(int value)
    {
        if (value > 0)
        {
            return Local(value);
        }

        return 0;

        int Local(int input)
        {
            foreach (var digit in input.ToString())
            {
                if (digit == '0')
                {
                    return 0;
                }
            }

            return input;
        }
    }
}
```

Before (`main`): not reported.

After, A (recommended) — the local function counts toward `M`, one level deeper (`M` = 1 + 2 + 3 = 6), and is also judged on its own (`Local` = 3):
```csharp
int {|RH2009:M|}(int value)   // "This member has a cognitive complexity of 6, which exceeds the limit of 3."
// Local: 3, not reported
```

After, B — the local function is judged only on its own; `M` = 1 and has no nesting increment:
```csharp
int M(int value)   // not reported
// Local: 3, not reported
```

Why A: moving a nested block into a local function keeps the method as hard to follow as before, and RH2008 counts a local function's lines toward its method in the same way. Requirements: RH2009-005.

**2 · RH2009 · Methods should not exceed the complexity limit — nesting only through a lambda**

Input, with `{ "design": { "maximumMethodComplexity": 1 } }`:
```csharp
using System.Linq;

class C
{
    int M(int[] values) => values.Count(value =>
    {
        if (value > 0)
        {
            return true;
        }

        return false;
    });
}
```

Before (`main`): not reported.

After, A (recommended) — a lambda nests like a block: its `if` scores 2 (nesting 1), so `M` has a nesting increment and the no-nesting exception does not apply:
```csharp
int {|RH2009:M|}(int[] values) => values.Count(value =>   // "This member has a cognitive complexity of 2, which exceeds the limit of 1."
```

After, B — a lambda nests for the score but not for the exception: only a branch or loop inside another branch or loop counts as nesting:
```csharp
int M(int[] values) => values.Count(value =>   // not reported
```

Why A: one notion of nesting serves the score and the exception, and the code inside a lambda is read in the context of the call that holds it. Requirements: RH2009-003, RH2009-004, RH2009-005.

**3 · RH2009 · Methods should not exceed the complexity limit — `and` and `or` patterns**

Input, with `{ "design": { "maximumMethodComplexity": 3 } }`:
```csharp
class C
{
    void M(int x, bool a)
    {
        if (a)
        {
            while (x is (1 or 2) and not 3)
            {
                x++;
            }
        }
    }
}
```

Before (`main`): not reported.

After, A (recommended) — `and` and `or` count like `&&` and `||`: `if` 1, `while` 2, two sequences 2 = 5:
```csharp
void {|RH2009:M|}(int x, bool a)   // "This member has a cognitive complexity of 5, which exceeds the limit of 3."
```

After, B — patterns score nothing: 3:
```csharp
void M(int x, bool a)   // not reported
```

Why A: `x is 1 or 2` is the same condition as `x == 1 || x == 2`, and rewriting one as the other should not change the score. Requirements: RH2009-003.

**4 · RH2009 · Methods should not exceed the complexity limit — limit in test code**

Input — a method in a test project, with `{ "design": { "maximumMethodComplexity": 10 } }`, whose body has a cognitive complexity of 12 and nests a structure.

Before (`main`): not reported; RH0001 reports the unknown setting.

After, A (recommended) — one setting for production and test code: reported, "This member has a cognitive complexity of 12, which exceeds the limit of 10."; a setting `maximumTestMethodComplexity` is reported by RH0001 as unknown.

After, B — a separate setting `maximumTestMethodComplexity` with its own default (15), as RH2008 has: not reported; `{ "design": { "maximumTestMethodComplexity": 10 } }` reports it.

Why A: test methods are longer than production methods by nature, but not more complex — a test that branches is itself a smell — so a second setting adds configuration without a use. Requirements: RH2009-001, RH0001-007.

**5 · RH2009 · Methods should not exceed the complexity limit — recursion**

Input, with `{ "design": { "maximumMethodComplexity": 3 } }`:
```csharp
class C
{
    int M(int value)
    {
        if (value > 0)
        {
            if (value > 1)
            {
                return M(value - 1);
            }
        }

        return 0;
    }
}
```

Before (`main`): not reported.

After, A (recommended) — a call of the member itself scores nothing: 1 + 2 = 3, not reported.

After, B — each call of the member itself scores 1, as the published definition of cognitive complexity charges recursion: 4:
```csharp
int {|RH2009:M|}(int value)   // "This member has a cognitive complexity of 4, which exceeds the limit of 3."
```

Why A: the issue asks for nesting, control flow, jumps, and logical operators, not recursion, and a recursive call reads like any other call. Requirements: RH2009-003 (non-goal "Recursion").

## Internal choices

- Rule ID RH2009 (member-level Design range), title, message, and setting name `maximumMethodComplexity`, modelled on RH2008 and `maximumMethodLines`; alternative: `maximumCognitiveComplexity`. Title and message are shown in the change doc for the maintainer's concept review.
- Members judged, members without a body, and the diagnostic location are copied from RH2008-002, RH2008-004, and RH2008-005, so both rules about one member stand on one token; alternative: a spec of shared member selection under `shared/`. Not done, because RH2009 does not count lines and SHARED-LINES does not apply to it.
- RH2009-001 states the default-on-invalid behavior itself instead of using SHARED-LINES-002, which is scoped to line counts; alternative: widen SHARED-LINES into a shared "configured limit" spec.
- RH0001-007 is changed in place (its heading gains "and complexity") rather than adding RH0001-008, because RH0001-007's "accept only the settings …" list would otherwise reject the new one.
- C#-specific points that the published definition of cognitive complexity settles, written into RH2009-003 without a question: a `switch` expression scores like a `switch` statement; every form of `goto` scores 1; `??`, `??=`, `?.`, `?[]`, `try`, `finally`, `using`, and `lock` score nothing and do not nest; a constructor initializer is not part of the body (as in RH2008).
- The ruleset handling of this repository's own violations (maintainer clarification 3) is not specified; how the build and the self-hosting scan ignore RH2009 is the lead's to decide. `Release.ruleset` lists RH2008 and RH2111 with `Action="Error"`, so RH2009 needs an entry or an exclusion either way.

## Follow-up candidates

- The self-hosting violations of RH2009 and a review of the default (already on the lead's list).
