Role: spec author
Commit: 2ea4e56
Verdict: spec written (all seven issues; clarifications, no change doc)

Inputs: the seven issues and their comments, `00-lead.md`. Current behavior was observed with `scripts/apply-fix.sh` on scratch fixtures (documentation mode `Parse`, no symbols defined); the observations quoted below come from those runs. The prompt carried no intended fix or verdict to ignore.

## thoenissen/Reihitsu#1136

### Changed requirements

- SHARED-FIX-003 — A fix that could break the code is withheld — clarified:
  - condition 3 now names disabled text and dropping (removing a comment, directive, or disabled text is a change to its text; the one exception is the `#endregion` of a region whose `#region` the diagnostic reports, as RH7310 removes it);
  - new condition 4: move a comment away from the code it stands with, reorder two comments that stand with the same code, or change which code a directive governs. Bounded in the new bullet **Comments and directives in place**: where a comment stands (behind code at the end of its line; starting its line with no blank line before the next code or directive line), what counts as moving it away, what a directive governs, and that splitting a directive pair across a brace (moving one directive across a brace, or adding a brace between the two) counts, per the maintainer's clarification. Coverage sentence: a comment separated by a blank line from the code below it and a comment between two tokens of one line are not decided;
  - new bullet **Build configurations**: conditions 1 and 2 hold in every build configuration; the existing condition about `#if`-excluded code (now the fifth) is named as one instance;
  - the indentation of disabled-text lines is layout (today's RH7004 fix keeps it unchanged when moving disabled text);
  - examples added (see below); `Why:` extended. `Uses` gains SHARED-ORDER-006, RH7004-004, RH7309-006.

### Examples for the tests

SHARED-FIX-003, examples "a directive pair split across a brace, for RH7501", "a brace added between the two directives of a pair, for RH7004" (both configurations, plus the balanced pair that stays offered), "code of another build configuration, for RH7501", "a comment that stays with the code it stands with" (points at RH7004-004 and RH7309-006).

Observed today: RH7501 with `#pragma` pair moves `disable` with the `break` and leaves `restore` inside. RH7004 `#if DEBUG s.Flush(); #endif` without `DEBUG`: the new block closes in front of `#if DEBUG` (fix offered). RH7004 with a `#pragma warning disable/restore` pair around the following statement: `disable` ends up inside the new block, `restore` behind it (fix offered) — a member of the same class, now withheld by the brace-split sentence. RH7004 balanced pair followed by another statement: fix offered, pair moves into the block (disabled line keeps its indentation).

### Open questions for the maintainer

See the Q1–Q4 blocks under the issues they belong to (#1262, #1336).

## thoenissen/Reihitsu#1134

### Changed requirements

No requirement changed: SHARED-FIX-003 (bullet **Run time**) already forbids reordering the instance fields of a sequential-layout type. Added only an example to SHARED-FIX-003, "a reorder that changes the layout, for RH7109" (default struct, withheld; static fields of a struct, offered). I did not add a guard requirement to `shared/ordering-move-safety.md` although the issue says the guard belongs there: it would restate SHARED-FIX-003. RH7109-006 is unchanged; the test tags SHARED-FIX-003.

### Examples for the tests

SHARED-FIX-003, examples "a reorder that changes the layout, for RH7109" and the existing "a reorder that changes no layout, for RH7109". Observed today: `struct S { public int _a; public readonly int _b; }` — fix offered, swaps the fields (the bug); the static-field variant — fix offered (correct).

## thoenissen/Reihitsu#1135

### Changed requirements

- RH7309-004 — The fix orders the type's regions by category — clarified: its claim now holds only when neither RH7309-005 nor RH7309-006 withholds the fix.
- RH7309-006 — The fix is withheld when the reorder would change the program or separate a comment — added (also serves #1262). Bullets 1 and 2 cover this issue: two same-context members with runtime initializers (as SHARED-ORDER-007 counts them) that change order, also against a member between blocks; two instance fields of a layout-sequential type that change order.

### Examples for the tests

RH7309-006, examples "an initializer that reads a field of a block it would cross" (the issue's input; observed today: fix offered and moves `Fields` in front) and "the instance fields of a struct".

## thoenissen/Reihitsu#1262

### Changed requirements

- RH7309-004 — clarified as above.
- RH7309-006 — added; bullets 3 and 4: a comment directly above the `#region` line of a block the fix moves (no blank line between); a documentation comment whose declaration would change (SHARED-DOC-004), in modes `Parse`/`Diagnose`. Coverage sentence: a comment above a block that keeps its position is not covered (Q2).
- RH7304-005 — A region with a directive inside executable code is not reported — added: neither directive of a pair is reported when one of them, or a region directive between them, lies inside executable code. RH7304-001 already restricted its claim to such pairs and pairs like the compiler (FMT-REGION-003); the maintainer decided the `MUST NOT`.
- RH7306-004 — The fix is withheld when nothing remains of the description — added: no fix for a reported directive whose description is the word `implementation` alone. RH7306-003 only claimed the case with text before the word. Observed today: `#region Implementation`/`#endregion // Implementation` gives `#region` and `#endregion //`; `#endregion //Implementation` behind `#region IDisposable` gives a fix that changes nothing (`no-progress`).

Not in the lead's list, added because the decided outcomes had no requirement to tag: RH7304-005 and RH7306-004. Both state only what the maintainer decided on this issue.

Instances that need no spec change, with the requirement they are bugs against:

- RH7303 straddling region (`#region` at type level, `#endregion` in a method; only the `#endregion` is reported): the fix removes the unreported `#region` — SHARED-FIX-003 condition 3 (dropping a directive no diagnostic of its rule reports). RH7303-004 claims only pairs whose two directives are both reported.
- RH7304 `#region Fields⏎    /* c */ int _a;`: the fix result does not parse — SHARED-FIX-003 condition 1.
- RH7309 regions inside a field initializer or a nested enum: SHARED-TYPEREGION-006 (not top-level regions of the type) and SHARED-FIX-003.

### Examples for the tests

RH7309-006 examples "a comment directly above a block the fix would move" and the three documentation shapes plus the mode-`None` counter-example; RH7304-005 example; RH7306-004 example. Observed today, mode `Parse`: comment above a moved block — fix offered, comment stays and heads the other block; shape 3 (`///` above `#endregion`) — fix offered, the comment moves away from `M`.

### Open questions for the maintainer

**Q1 — RH7309, comment above a moved block with a blank line between.**
Input:

```csharp
using System;

public class C : IDisposable
{
    // The disposal logic

    #region IDisposable

    public void Dispose()
    {
    }

    #endregion // IDisposable

    #region Fields

    private int _a;

    #endregion // Fields
}
```

Real output today: the fix is offered; `Fields` moves in front, and `// The disposal logic` stays at the top, now above `Fields`. Why it looks wrong: the comment most likely heads `IDisposable` and ends up heading `Fields`. Requirements: RH7309-004 (claim excludes it: comment outside blocks), RH7309-006, SHARED-FIX-003 (its coverage sentence leaves blank-line-separated comments undecided). Readings: A — withhold, a comment with only blank lines between it and a `#region` heads that block; B — offer, the blank line makes the comment free-standing and it stays where it is. Recommendation: A — the maintainer's own instance withholds for the comment without the blank line, a blank line under a heading comment is common, and "prefer no fix" applies.

**Q2 — RH7309, comment directly above a block that keeps its position.**
Input: blocks `Fields` (custom), `IDisposable` (interface), `Methods` (custom), with `// The fields` directly above `#region Fields`. Real output today: the fix is offered; `Methods` moves in front of `IDisposable`; `Fields` and its comment stay first. Why it needs asking: the decision on #1262 reads "withheld when a comment stands directly above a region block", which literally covers this input, although nothing separates the comment from its block. Requirements: RH7309-006 (coverage sentence leaves it out), SHARED-FIX-003. Readings: A — offer (today); B — withhold whenever any block has a comment directly above it. Recommendation: A — nothing is relocated, and B gives away safe ground.

**Q3 — RH7306, the `#endregion` partner left with an empty comment.**
Input:

```csharp
class C
{
    #region IDisposable implementation

    int _a;

    #endregion // implementation
}
```

Real output today (and claimed by RH7306-003): `#region IDisposable` and `#endregion //`. Why it looks wrong: the `#endregion` keeps a `//` with nothing after it; the `#endregion` line is itself reported, and RH7306-004 now withholds the fix of that diagnostic, while the fix of the `#region` diagnostic still empties the same line. Requirements: RH7306-003, RH7306-004. Readings: A — keep today's output; B — withhold the fix when the partner line would be left with nothing of its description; C — remove the `//` together with the word, giving `#endregion`. Recommendation: B — consistent with RH7306-004 and the "naming is the user's" decision; C if the maintainer prefers a fix here.

## thoenissen/Reihitsu#1332

### Changed requirements

- RH7501-004 — The fix moves the `break` behind the block — clarified:
  - condition 1 is reworded to cover a `break` that is the block's only statement (no blank line between the `break` and the token in front of it, the opening brace included), since the maintainer kept that case (Q9) and the old wording excluded it;
  - condition 3 measures from that same token;
  - "A block that the fix leaves empty MUST stay." (today's behavior, verified: `{ break; }` → `{ }` + `break;`);
  - "When the second or the third condition does not hold, the code fix MUST NOT be offered; the diagnostic is still reported." The blank-line case is explicitly not decided ("This requirement does not decide the fix for a `break` with a blank line in front of it"), because it moved to #1437 and stays out of this run; today it is offered and moves the blank line with the `break` (observed).
  - `Uses` gains SHARED-FIX-003.

### Examples for the tests

RH7501-004 examples: the empty-block case; the three shared-line shapes (Q8 inputs 1–3); the `#if` and `#region` shapes (Q7 inputs 1, 2). SHARED-FIX-003 examples for the `#pragma` pair (Q7 input 4) and the other-configuration case (Q7 input 3).

## thoenissen/Reihitsu#1336

### Changed requirements

- RH7101-004 — The fix splits the declaration — clarified: "Unless RH7101-005 or RH7101-006 withholds it".
- RH7101-006 — The fix is withheld for a documented declaration — added: modes `Parse`/`Diagnose`, `///` or `/** */`; mode `None` keeps the split; an ordinary comment does not withhold. Observed today (mode `Parse`): both comment forms — fix offered, comment kept on the first field only.
- RH7004-004 — The fix wraps the following statements into a using statement — clarified: a comment behind the declaration at the end of its line stays behind the `using` statement's header, on its line (the issue's expected output for A2). Observed today: the comment ends up behind the new block's `}`. `Uses` gains SHARED-FIX-003.
- SHARED-MODORDER-003 — clarified: a directive of any kind, conditional ones included, between the last modifier and the token after it withholds the fix; example added. Observed today: `static public⏎#pragma warning disable CS0169⏎int _a;` → `public static #pragma warning disable CS0169` (`invalid-result`).

Not in the lead's list: RH7004-004 and SHARED-MODORDER-003, added because the issue's decided outcomes had no requirement stating them.

Instances that need no spec change: A1 (RH7004, directive pair around the following statements) — SHARED-FIX-003 (brace split; other configuration); B3 (`namespace N { int a, b; }`, fix changes nothing) — SHARED-FIX-005; RH7101-001 does not claim a report outside a type, so a test that the diagnostic stays tags no RH7101 requirement.

### Examples for the tests

RH7101-006 (three blocks), RH7004-004 (comment example), SHARED-MODORDER-003 (directive behind the last modifier; the issue also names `internal protected`, `class C`, and `#if`), SHARED-FIX-003 RH7004 examples.

### Open questions for the maintainer

**Q4 — RH7105/RH7106, a comment between the last modifier and the type.**
Input:

```csharp
class C
{
    static public
    // the counter
    int _a;
}
```

Real output today: the fix is offered and gives `public static`, a blank line, `// the counter`, `int _a;` — the comment stays with `int _a;`, but a blank line appears inside the declaration. Why it looks wrong: the fix adds a blank line the diagnostic does not name (SHARED-FIX-006), and #1336 asks the SHARED-MODORDER-003 guard to inspect the trivia in front of the token after the last modifier, which for that guard covers comments as well as directives. Requirements: SHARED-MODORDER-002 (claim excludes it), SHARED-MODORDER-003, SHARED-FIX-006. Readings: A — withhold the fix when a comment stands between the last modifier and the next token, as for directives; B — offer it and keep the comment on its own line without the blank line. Recommendation: A — same guard, one line of spec, and a comment written there cannot be laid out by the fix without touching layout the user chose.

## thoenissen/Reihitsu#1340

### Changed requirements

- RH7003-005 — A namespace that contains a namespace is not reported — added (clarification of RH7003-001, whose claim already excluded such namespaces without stating the `MUST NOT`). Non-goal added: merging a namespace and its one inner namespace into one file-scoped namespace (the issue names it a possible later feature).
- Rule page `documentation/rules/RH7003.md` — left unchanged: its sentence "the rule only reports a namespace that can actually be converted" already covers a nested namespace, and `create-rule-doc` forbids listing exceptions on a rule page.

The analyzer today checks only that the namespace is the compilation unit's only member (`RH7003FileScopedNamespacesShouldBeUsedAnalyzer.IsEligibleNamespace`), so both inputs of the issue are reported (read from the code; RH7003 has no fix, so `apply-fix.sh` cannot run it).

### Examples for the tests

RH7003-005, both blocks.

## Whole run

- Files: `sdd/specs/shared/code-fixes.md`, `sdd/specs/shared/modifier-ordering.md`, `sdd/specs/analyzer/RH7003.md`, `RH7004.md`, `RH7101.md`, `RH7304.md`, `RH7306.md`, `RH7309.md`, `RH7501.md`, `sdd/specs/requirement-ids.txt` (RH7003-005, RH7101-006, RH7304-005, RH7306-004, RH7309-006 inserted in sorted order). No change doc, no rule page changed, `sdd/specs/README.md` unchanged (no code owner changed).
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage"`: 50 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`, listing exactly the five new requirements, which need their tests from the test implementer.

### Open questions for the maintainer

Q1, Q2 (#1262), Q3 (#1262), Q4 (#1336) above. None blocks the run: each case is left out of the spec.

### Follow-up candidates

- `documentation/rules/RH7003.md` has an "Exceptions" section, which `create-rule-doc` says a rule page does not carry. Out of scope.

### Architecture findings

_None._ Note for the lead: the initializer-order guard is now stated for member moves (SHARED-ORDER-007) and for region reorders (RH7309-006 bullet 1). RH7309-006 takes its definition of a runtime initializer from SHARED-ORDER-007 rather than restating it, and the issue names the shared helper `OrderingDeclarationUtilities.ChangesInitializerExecutionOrder`. If the implementation copies that logic into the RH7309 fix instead of reusing the helper, that is an architecture finding.
