Role: spec author
Commit: 54b6489
Verdict: spec written

This round revises the spec diff of `02-spec-author.md` to state the maintainer's answers in `squad/decisions/2026-10-18-shared-fix-003-skeptic-scope.md`. The two earlier decisions and the `00-lead.md` clarifications still hold. My prompt carried no intended fix, suspected cause, or hoped-for verdict. It did carry constraints, which I followed: no change doc, no region-name tab exception, no requirement the issue does not name, the issue's `AGENTS.md` sentence, examples that match today's code, and no reference to SHARED-DIAG-003.

Files changed: `sdd/specs/shared/code-fixes.md` only. `sdd/specs/requirement-ids.txt` and `AGENTS.md` keep the state of 1bf3eeb. Both are still correct: the three IDs are in sorted position, and the `AGENTS.md` sentence is the issue's text. `Uses` stays `—`, and `sdd/specs/README.md` needs no change.

Check: `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecStructureTests|FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~RequirementCoverageTests"` gives 38 passed and 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, for SHARED-FIX-003, -004 and -005. That is expected until the covering tests exist.

## Changed requirements

- SHARED-FIX-001 — One application corrects a diagnostic — clarified.
  - "At that location" is now "at the place it checks … not only the span the diagnostic covers". The RH5002 example carries a sentence for the span side: the diagnostic covers `if`, and the gap in front of it is corrected. This answers the skeptic's non-blocking finding.
  - The two-rule example is now RH5028 and RH5023 on a leading run, which passes today. It replaces RH5022 and RH5023 below `{`, which fails today.
- SHARED-FIX-003 — A fix that could break the code is withheld — added. The four conditions are the issue's. The second and third conditions are bounded in two bullets:
  - **Run time.** From the two decisions:
    - the new name or accessibility does not count (rename decision);
    - reordering instance fields whose declaration order determines the type's layout counts (skeptic-scope answer 3). The example is a struct with sequential layout. A reorder that changes neither a layout nor the order in which code runs does not count.
  - **Comment, directive, and literal text.** From:
    - the own-reported-text decision;
    - skeptic-scope answer 1: a doc-comment name that binds to the renamed symbol is renamed with it, and a word that only reads like the name stays;
    - skeptic-scope answer 2: the layout of comments, directives, and raw-literal indentation does not count, and a changed word or a changed literal value counts.

  New examples: the RH4106 `cref`, and RH3202 layout. The RH8107 sentence is corrected: the removed space in front of the element is layout. This answers the skeptic's non-blocking finding.
- SHARED-FIX-004 — The fixes of two rules do not undo each other — added. The only example is the RH2005 → RH4106 chain. It now shows the final text `private string _first;`.
- SHARED-FIX-005 — An offered fix changes the document — added. The only example is RH4106 `_1st`.
- Non-goals: as in 1bf3eeb. The no-op class is removed, and the cycle class is covered by SHARED-FIX-004. "A fix leaves diagnostics of other rules behind" stays.

### Where the shipped text differs from the issue

The lead set the constraint that examples must match today's code. So I removed every example that fails today, including the three the issue names. The normative sentences still cover each one, so each stays a bug against the spec, owned by its follow-up:

| Removed example | Requirement | Fails today because | Owner |
|---|---|---|---|
| RH5022 on `{⏎⏎⏎M();` (issue item 1, #1066 Q1) | SHARED-FIX-001 | fixed in 2 iterations | thoenissen/Reihitsu#1076 |
| RH4106 with `return Value + 1;` under `#if NEVER` | SHARED-FIX-003 | the excluded use stays `Value` | thoenissen/Reihitsu#1073 |
| RH5024 then RH5032 on `#region R⏎⏎}` (issue item 3, #1065 Q1) | SHARED-FIX-004 | the two fixes alternate | thoenissen/Reihitsu#1076 |
| RH5023 at the end of `"class C\n{\n}\n\n"` (issue item 4, #1063 Q1) | SHARED-FIX-005 | `no-progress` | thoenissen/Reihitsu#1076 |

Qualifiers whose triggering side has no passing example today: FC-002 wants an example on each side, and only the non-triggering side is shown. The triggering side is a known bug, and its regression test belongs to the follow-up:

- condition 4, an excluded branch that names the symbol (#1073);
- field order that determines a layout (the RH7109 follow-up from answer 3);
- a changed word outside the reported text (the RH7103 region follow-up from answer 2).

### SHARED-DIAG-003 alignment

The spec text does not name SHARED-DIAG-003. Answer 4 says SHARED-FIX-003 owns the rule for fixes and relies on SHARED-DIAG-003 only for what counts as comment or literal content.

When the two PRs are aligned, the second to merge adds that reference to the third bullet and lists the ID under `Uses`. RH5604-005 is then decided by SHARED-FIX-003: the line breaks are reported by its own rule, and they are layout. The lead notes this in the PR.

### Departure from the decision's wording

Answer 3 reads "fields whose order determines the layout (sequential or explicit struct layout)". I kept the criterion and named only sequential layout. With `LayoutKind.Explicit`, `[FieldOffset]` sets every offset, so declaration order does not determine that layout. This is part of open question B below.

## Examples for the tests

Every example in SHARED-FIX-001, -003, -004 and -005 matches today's code. Each was run with `scripts/apply-fix.sh <RH####> <fixture-dir>`, LF and CRLF, on fixtures in the scratchpad, or is already pinned by a tagged test:

- **SHARED-FIX-001**
  - RH5028, one application (RH5028-003 tests).
  - RH5023, one application for each diagnostic (RH5023-004 tests).
  - The leading run `⏎⏎⏎class C⏎{⏎}⏎`:
    - RH5023 removes lines 2 and 3 in 2 applications and leaves line 1;
    - RH5028 removes all three lines in 1 application.
- **SHARED-FIX-003**
  - The RH4106 collision (SHARED-CASING-009 test).
  - RH4106 `nameof(Value)` → `nameof(_value)`.
  - RH7103 `_b` withheld and `_a` fixed (SHARED-ORDER-007 tests).
  - RH4106 `<see cref="Value"/>` → `<see cref="_value"/>`, with the summary word `Value` unchanged.
  - RH3202 on the two-method class, one file. The output is exactly the block in the spec, 2 applications.
  - RH8107 single-line `<returns>`.
  - RH5604 (RH5604-003, RH5604-005 tests).
  - RH4106 with `return 0;` under `#if NEVER`: fixed, with the excluded branch unchanged.
- **SHARED-FIX-004:** RH2005 on `public string first;`, then RH4106, gives `private string _first;`. Assert the text after both fixes (FC-017).
- **SHARED-FIX-005:** RH4106 `_1st`: `no-fix-offered`. Assert that no action is registered (FC-009).

## Open questions to file

All three belong to `SDD open questions: sdd/specs/shared/code-fixes.md`. That issue was thoenissen/Reihitsu#1066, which is closed, so the lead reopens it or opens a new one. None of them blocks this run: no example or covering test depends on the answer. Question A has to be answered before the RH5011 and RH5027 instances of thoenissen/Reihitsu#1075 can be fixed, and before thoenissen/Reihitsu#1083 writes its exception. The lead decides whether it goes to the maintainer before this PR merges, because the spec carries the decision's wording.

### A — Does the layout exemption cover whitespace and line breaks inside the text of a `//` or `/* */` comment, or of a directive?

Touches SHARED-FIX-003, and SHARED-DIAG-003 once #1069 merges.

**Input** (RH5011, the instance #1061 Q2 in thoenissen/Reihitsu#1075):

```csharp
class C
{
    void M(int a)
    {
        while (a > 0)
        {
            break; /* a
            b */
            a++;
        }
    }
}
```

**Real output:** the fix inserts a blank line between `/* a` and `b */`. It does so again on every application, and is not converged after 10 applications.

**Why it looks wrong:** skeptic-scope answer 2 says the "whitespace, line breaks, and indentation of comments and directives" do not count. Read that way, the inserted line is layout, and the instance is no longer a SHARED-FIX-003 violation. It stays a SHARED-FIX-001 violation. Two other decisions say the opposite:

- thoenissen/Reihitsu#1075 says this fix changes comment text against SHARED-FIX-003, and so does RH5027 on `/* x⏎⏎ */ while`.
- thoenissen/Reihitsu#1083 decides two things:
  - a tab in a `#warning` message and a tab in a comment after a directive are not changed;
  - replacing a tab in a region name needs an exception to SHARED-FIX-003.

**Readings:**

- **(a) Any whitespace or line break in a comment or directive is layout.** Effects:
  - the RH5011 and RH5027 insertions and deletions are not SHARED-FIX-003 violations;
  - #1083's region-name exception is unnecessary;
  - nothing in SHARED-FIX-003 keeps a fix from changing the tab in `#warning a→b` or in `// a→b`.
- **(b) Layout is the whitespace around and between the lines of a comment or directive.** That means its indentation, and the line structure, `///` exterior and inter-element spacing of a documentation comment. Whitespace inside the text of a `//` or `/* */` comment, or inside a directive's name or message, counts like a word. Effects:
  - consistent with #1075 and #1083;
  - the RH3202, RH8107 and RH5604 examples are unchanged.

**Recommendation:** (b). It is the only reading consistent with all three decisions, and it changes none of this spec's examples.

### B — Which types does "fields whose declaration order determines the type's layout" cover?

Touches SHARED-FIX-003.

**Inputs** (RH7109):

```csharp
public struct S
{
    public int _a;
    public readonly long _b;
}
```

```csharp
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public struct S
{
    [FieldOffset(0)]
    public int _a;
    [FieldOffset(8)]
    public readonly long _b;
}
```

**Real output:** in both, the fix moves `_b`, with its attributes, in front of `_a`.

**Why it looks wrong:** answer 3's own input carried `[StructLayout(LayoutKind.Sequential)]`. The C# compiler gives every struct sequential layout unless it says otherwise, so the first struct has the same layout as that input. Under explicit layout, `[FieldOffset]` sets the offsets, so the reorder in the second input changes no layout. Answer 3 still names "explicit".

**Readings:**

- **(a) Only a type that says `[StructLayout(LayoutKind.Sequential)]` or `Explicit`.** Output: the first struct is fixed as today, and the second is withheld.
- **(b) Every type whose field order sets its layout.** That is every struct without `LayoutKind.Auto`, and a class with `[StructLayout(LayoutKind.Sequential)]`. Not explicit layout. Output: the first struct is withheld, and the second is fixed as today. This reading withholds RH7109, and any other ordering fix that moves instance fields, for most structs.
- **(c) Sequential or explicit, written or by default.** Output: both are withheld.

**Recommendation:** (b), the decision's own criterion. The spec text states that criterion, with sequential layout as its example. Whichever is chosen, the RH7109 follow-up from answer 3 implements it.

### C — RH7302's fix changes the `#endregion` comment, which its diagnostic does not report

Still open after the new decisions:

- Answer 1 covers only names in documentation comments that bind to the renamed symbol. `// members` binds to nothing.
- Answer 2 makes a changed word count.
- The own-reported-text decision permits only text the rule reports.

So the decisions read as a violation. The case is still not decided, because the comment repeats the name of the very region the fix renames. Touches SHARED-FIX-003. RH7302 has no spec.

**Input** (`RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzerTests.VerifyCodeFixUpdatesMatchingEndRegionComment`, reduced):

```csharp
class C
{
    {|RH7302:#region members|}
    #endregion // members
}
```

**Real output:** `#region Members` and `#endregion // Members`.

**Why it looks wrong:** the diagnostic spans only the `#region` directive. The `// members` comment is text that no RH7302 diagnostic reports, so SHARED-FIX-003 says it stays unchanged.

**Readings:**

- **(a) Bug.** The fix leaves `#endregion // members`, which RH7301 may then report as a mismatch, or the fix is withheld when an `#endregion` comment repeats the name. The pinned test becomes wrong.
- **(b) Clarification.** The `#endregion` comment that repeats the name of the region the fix renames is part of the rename, as a binding doc-comment name is under answer 1. SHARED-FIX-003 would need that exemption in its third bullet. Output as today.

**Recommendation:** (b). Reading (a) makes RH7302 and RH7301 trade diagnostics, and (b) matches the pattern of answer 1.

## Follow-up candidates, out of scope

- `AGENTS.md` now says SHARED-FIX-003 "states what makes a fix unsafe". The sentence before it, and `documentation/rules/general-notes.md`, also call it unsafe to drop, reorder, or relocate a comment, a directive, or disabled text (FC-004). SHARED-FIX-003 covers changed text, not relocation or reordering. A requirement for relocation is not named by the issue, so it is not added here.
- The known contradiction between the per-rule SHARED-FIX-001 and RH5022-003/-004, and RH5024 to RH5027, also involves SHARED-WS-001, which RH5022 uses: "no whitespace outside the span is changed". thoenissen/Reihitsu#1076 resolves all of them.
- The `sdd/specs/analyzer/RH2005.md` non-goal on references from outside the type is now decided by the first condition of SHARED-FIX-003. The same holds for the RH7103 non-goal on how far the reformat reaches, now decided by answer 2.
- The follow-ups named in answers 2 and 3: RH7103 renaming an unrelated region, and RH7109 on layout-ordered structs. Their scope depends on questions A and B.

## Decisions needed

_None._
