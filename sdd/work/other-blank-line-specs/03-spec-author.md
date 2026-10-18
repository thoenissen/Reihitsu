Role: spec author
Commit: 0fef616cd13121cac53d98d3e069c5aa26908978
Verdict: spec written

Repair round on `02-skeptic.md` (verdict: proceed narrower). This handover supersedes the open-question list of `01-spec-author.md`. My prompt carried no intended fix, suspected cause, or hoped-for verdict. No production code, test, or rule page was touched, and no requirement of SHARED-BLANK, FMT-BLANK, SHARED-STMT, SHARED-WS, or SHARED-FIX changed. No ID was added, retired, or renumbered, so the ledger and the index are unchanged in this round.

## How the corrections were established

- I reproduced every skeptic item with my own fixtures in the scratchpad, outside the repository. The analyzer and the first code action ran through `CodeFixTargetResolver`, and `scripts/trace.sh --no-install` showed the formatter. All eight items reproduce as described, so I agree with all eight and with Questions A and B. Inputs and outputs are under *Open questions to file*.
- The example harness now also flags a fix that needs more applications than the number of diagnostics it started with. The skeptic was right that the first round's "apply until clean" check could hide a second pass. After the revision, all 44 checked example blocks of the 13 specs match their markup, their "is not reported" claims, and their fix results, and none needs a second pass.
- `scripts/test.sh --no-install --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"`: 26 passed, 0 failed.

## Spec changes in this round

| Item | Spec | Change |
|---|---|---|
| 1 | RH5011-004 | The separate-line fix is stated only for a `break` whose line ends outside a comment. The multi-line comment shape is left out (Q5). |
| — | RH5011-001 | The reported case requires the trivia to hold no documentation comment. `ContainsBlankLine` does not see a blank line below a `///` comment, so that shape is left out (Q11). |
| 2 | RH5020-003 | Reworded to the real boundary: a comment or directive after the first line break outside a comment behind the preceding token. Both sides of the boundary pair are in the example: `// d` after `/* a`…`b */ // c` on their own lines is not reported; behind `a++;` it is reported. |
| 3 | RH5024-001 | Stated only for a blank line whose line above is not a `#region` directive. The RH5032 cycle is left out (Q9). |
| 4 | RH5025-001 | Widened to every shape that continues the code above it. These are reported by both RH5025 and the formatter: namespace, type, member, accessor list, lambda, statement body, switch-expression body, initializer or anonymous-object body after `new`, and a property pattern continuing an expression. The example shows each of them. Standalone and sibling braces stay out (Q2). The intro sentence and the RH5025-004 `Why:` were adjusted. |
| 5 | RH5030-003 | Stated only when a line break outside any comment stands between the brace and the next statement. The block-comment shape is left out (Q6). |
| 6 | RH5022-003, RH5023 non-goal, RH5024-004, RH5025-003, RH5026-003, RH5027-003; non-goals of RH5024–RH5027 | Every statement about runs of blank lines is removed: the false "RH5023 reports the rest", "the two rules never report the same line", and the RH5022-003 run example. A run next to a brace or keyword is left out of every spec (Q10). The location `Why:`s now say only that the reported line is the line the fix deletes. |
| 7 | RH5022 and RH5024 non-goals | Reworded so they no longer contradict the requirements. Each rule's non-goal points to the other rule only for the line that is not shared, and it says that in an empty body both rules report the same line (verified: each fix alone clears it in one application). |
| 8 | RH5027-001 | Stated only when nothing but whitespace, or whitespace and the body block's `}`, stands in front of `while` on its line. The other shapes are left out (Q3). The RH5027-004 `Why:` is limited to the lines RH5027-001 names. |

### Where I deviate from the skeptic

- **Top-level type after a `break`.** I did not add the skeptic's optional sentence on a top-level type declaration after a `break` to RH5011. Example: `while (a < 3) break;` followed by `class C`. The reported set "followed by another statement or by the next section" leaves it unspecified, the shape is harmless, and it is not an open question either.
- **Braced switch section.** `case 1:`, then a blank line, then the section's `{` is reported by RH5025 and removed by the formatter. I did not write it into RH5025-001, because the brace is a block statement that follows a label, which is the same syntactic question as Q2. Q2 names it so the maintainer's answer covers it.

## Why FMT-BLANK policy is restated, not referenced

The maintainer asked for FMT-BLANK IDs to be referenced where the formatter owns the policy a rule checks. In these 12 rules, every formatter counterpart differs from the analyzer in detail. Several of the differences are exactly the open questions below, which may not appear in a spec as a requirement, non-goal, or note. So each analyzer spec states its own rule, and the only FMT-BLANK ID referenced is FMT-BLANK-005, in RH5030-003's `Why:`, where the two agree.

| Rule | Formatter counterpart | Where it differs |
|---|---|---|
| RH5011 | FMT-BLANK-004 | FMT-BLANK-004 asks for the line only "in the same list", so it excludes `else` and the do-statement `while` (Q4). |
| RH5020 | FMT-BLANK-011 | FMT-BLANK-011 covers documentation comments as well, says "exactly one" blank line, and its comment exemption differs from RH5020-003. |
| RH5022, RH5024, RH5028 | FMT-BLANK-007 | FMT-BLANK-007 removes the whole run in one go (Q10). It also removes the line below `{ // note` (Q8). |
| RH5025–RH5027 | FMT-BLANK-008 | The formatter keeps the line above a one-line `do` and the RH5027 literal and comment shapes (Q3). |
| RH5023 | FMT-BLANK-014, FMT-BLANK-015 | RH5023 excludes every line of an interpolated string, holes included. |
| RH5030 | FMT-BLANK-005 | The same-line pair is decided differently, but SHARED-BLANK-003 already states that, so RH5030 references SHARED-BLANK-003. |

I put this note here rather than in the specs. The rulebook only requires that a shared policy be referenced and not restated. Here no FMT-BLANK requirement is the policy these rules check, and a sentence in a spec about how the formatter differs would have to describe open-question cases.

## Changed requirements

Wording changed in this round (all still clarifications of current behavior):

- RH5011-001 — clarified
- RH5011-004 — clarified
- RH5020-003 — clarified
- RH5022-003 — clarified
- RH5024-001 — clarified
- RH5024-004 — clarified
- RH5025-001 — clarified
- RH5025-003 — clarified
- RH5025-004 — clarified
- RH5026-003 — clarified
- RH5027-001 — clarified
- RH5027-003 — clarified
- RH5027-004 — clarified
- RH5030-003 — clarified

The full list of the 50 added requirements is in `01-spec-author.md`; none was retired.

## Requirement IDs that need covering tests

None is tagged yet. `RequirementCoverageTests` fails until all of these have a covering test:

- RH5011-001 to RH5011-006
- RH5020-001 to RH5020-005
- RH5022-001 to RH5022-004
- RH5023-001 to RH5023-004
- RH5024-001 to RH5024-005
- RH5025-001 to RH5025-004
- RH5026-001 to RH5026-004
- RH5027-001 to RH5027-004
- RH5028-001 to RH5028-003
- RH5030-001 to RH5030-003
- RH5031-001 to RH5031-003
- RH5032-001 to RH5032-003
- SHARED-REGION-001, SHARED-REGION-002
- Newly enforced through `Uses`: SHARED-BLANK-002 (RH5020), SHARED-BLANK-003 (RH5030), SHARED-BLANK-005, SHARED-BLANK-006, SHARED-BLANK-007 (RH5031/RH5032). SHARED-BLANK-001, SHARED-BLANK-004 and SHARED-WS-001 already have covering tests.

Seeds for the tests:

- **Rule examples.** Every requirement except the location requirements has its own example. The location requirements point at their rule's first example: RH5011-003, RH5020-004, RH5022-003, RH5023-003, RH5024-004, RH5025-003, RH5026-003, RH5027-003, RH5030-002, RH5031-002, RH5032-002.
- **RH5020-003** has the boundary pair: one side not reported, one side reported.
- **RH5028** has two cases without a block: a file of only whitespace (reported over the whole file, and the fix leaves it empty) and an empty file (not reported). Both were verified by running them.
- **Shared examples.** The examples of SHARED-BLANK-003 and SHARED-BLANK-004 apply to RH5030, and those of SHARED-BLANK-005, SHARED-BLANK-006 and SHARED-BLANK-007 apply to RH5031 and RH5032.

Do not tag a test on any input of the open questions below. A test that pins one of those shapes stays untagged as a characterization test. Use the verifier's default iteration count, so a fix that needs a second application fails rather than hides.

## Checked against the already-filed issues

- **thoenissen/Reihitsu#949.** RH5020 reports the comment that leads a terminal `break` in both shapes, unbraced and after brace insertion, and its fix agrees with the expected output of #949. The case is left out of RH5020 and nothing is filed.
- **#954, #1000, #1001.** Not touched; nothing is filed for them.
- **Open-question issues.** None exists yet for any target spec below (searched; the skeptic also read #912's sub-issues), so each target spec needs a new `SDD open questions: <spec path>` issue under thoenissen/Reihitsu#912.

## Open questions to file

Eleven questions in seven issues. None of them blocks this run, and every case is left out of the specs. Q5, Q6 and Q7 contradict SHARED-FIX-001, an enforced requirement, so "intended" is not an available answer without a change doc for SHARED-FIX. The lead may file them as bugs instead.

---

### Issue `SDD open questions: sdd/specs/analyzer/RH5025.md`

#### Q1 — RH5025 deletes a line of an interpolated string's content

**Touches:** RH5025-001, RH5025-004, SHARED-WS-001, FMT-BLANK-015.

**Input:**

```csharp
class C
{
    string M(int x)
    {
        return $"""
            a

            {x}
            """;
    }
}
```

**Real output:** RH5025 reports the empty line between `a` and `{x}`, because the interpolation's `{` is an opening-brace token that begins its line. The code fix deletes the line:

```csharp
        return $"""
            a
            {x}
            """;
```

The same happens in a verbatim interpolated string: `$@"a`, an empty line, then `{x}";`.

**Why it looks wrong:**
- The deleted line is part of the string's value, so the fix changes what the program does.
- The formatter keeps the line (FMT-BLANK-015).
- AGENTS.md prefers no fix over one that alters a literal.

**Decision needed:** a bug (RH5025 ignores interpolation braces), or intended (a clarification adds them).

#### Q2 — RH5025 reports a brace that starts a sibling rather than continuing the code above it

**Touches:** RH5025-001, RH5025-004, FMT-BLANK-008, RH5032-001, SHARED-BLANK-006.

**Input:**

```csharp
class C
{
    void M(int a, string s)
    {
        a++;

        {
            a++;
        }

        var d = new System.Collections.Generic.Dictionary<string, int>
        {
            { "a", 1 },

            { "b", 2 },
#region A

            { "c", 3 },
#endregion
        };
        var e = s switch
        {
            { Length: 0 } => 0,

            { Length: 1 } => 1,
            _ => 2,
        };
    }
}
```

**Real output:**
- RH5025 reports the blank line above each of these braces:
  - the standalone block;
  - the dictionary elements `{ "b", 2 }` and `{ "c", 3 }`;
  - the switch arm `{ Length: 1 }`.
- Its fix deletes each line.
- The formatter removes the same lines (FMT-BLANK-008), except the one directly below `#region A`.
- The line below `#region A` is the line RH5032 requires (SHARED-BLANK-006). After RH5025's fix deletes it, RH5032 reports `#region A` and inserts it again, so no text satisfies both rules.
- Array-initializer elements behave like the dictionary elements: `{ 1 },`, a blank line, then `{ 2 },`.
- A related shape the answer should cover: the braced switch section `case 1:`, a blank line, then `{`. RH5025 reports it and the formatter removes the line.

**Why it looks wrong:**
- These braces do not belong to the code above them. A standalone block, an initializer element, and a switch arm are siblings of the previous statement, element, or arm, and the blank line separates two units.
- The rule page describes RH5025 as keeping "a declaration and its opening brace" together.
- The region shape is a cycle between two rules.

**Readings:**
- A — reported. A clarification adds sibling braces to RH5025-001; the region cycle then needs a separate decision, either an RH5025 exception for a line SHARED-BLANK-006 requires, or a change to SHARED-BLANK-006.
- B — not reported. A change doc first, because FMT-BLANK-008 changes as well.

---

### Issue `SDD open questions: sdd/specs/analyzer/RH5027.md`

#### Q3 — RH5027 checks the line above the line holding `while`, not the gap in front of `while`

**Touches:** RH5027-001, RH5027-004, RH5004-001, SHARED-WS-001, FMT-BLANK-001, FMT-BLANK-008.

**Input:**

```csharp
class C
{
    void M(int a)
    {
        a++;

        do a++; while (a < 3);
        do

            a++; while (a < 3);
        do
            a += @"x

".Length; while (a < 3);
        do
        {
        } /* x

        */ while (a < 3);
    }
}
```

**Real output:** RH5027 reports four lines, and its fix deletes each of them:
1. The blank line above `do a++; while (a < 3);`. After the fix deletes it, RH5004 reports `do` because the blank line RH5004-001 requires is gone. Restoring the line brings RH5027 back.
2. The blank line between `do` and its body `a++;`.
3. The empty line inside the verbatim string. Deleting it changes the string's value.
4. The empty line inside the block comment. Deleting it edits the comment's text.

The formatter keeps all four lines.

**Why it looks wrong:**
- In shapes 1 and 2 the line is not between the body and the footer. In shapes 3 and 4 it is not layout at all.
- Shape 1 is a cycle with RH5004.
- Shapes 3 and 4 alter a literal and a comment, which AGENTS.md forbids for a fix.

**Decision needed:** a bug (RH5027 checks only the gap directly in front of `while`), or intended. "Intended" would need a change to the enforced RH5004-001, so a change doc.

---

### Issue `SDD open questions: sdd/specs/analyzer/RH5011.md`

#### Q4 — RH5011 reports a `break` whose next token continues the enclosing statement

**Touches:** RH5011-001, RH5011-004, RH5011-005, RH5026-001, RH5027-001.

**Input:**

```csharp
class C
{
    void M(int a)
    {
        while (true)
        {
            if (a > 0)
                break;
            else
                a++;
        }

        do break; while (a < 3);
    }
}
```

**Real output:** RH5011 reports both `break`s: the next token is `else` for the first and `while` for the second, and neither is `}` or a directive.
- For the first, the fix inserts an empty line between `break;` and `else`. RH5026 then reports that line, and deleting it brings RH5011 back.
- The one-line form `if (a > 0) break; else a++;` is reported too. Its fix produces `if (a > 0) break;`, an empty line, then `else a++;`, which RH5026 also reports.
- For `do break; while (a < 3);`, the fix produces `do break;`, an empty line, then `while (a < 3);`, which RH5027 reports.
- The formatter leaves `do break; while (a < 3);` unchanged, so formatter output is flagged by RH5011. For `else` the formatter adds braces first, and its output is clean.

**Why it looks wrong:**
- `else` and the do-statement `while` continue the statement that encloses the `break`. They are not a new path or a new case, which is what RH5011-001's `Why:` is about.
- Neither pair of rules leaves a clean state.

**Decision needed:** a bug (RH5011 does not report a `break` whose next token continues an enclosing statement), or intended.

#### Q5 — RH5011's fix inserts empty lines inside a block comment and never converges

**Touches:** RH5011-004, SHARED-FIX-001, SHARED-BLANK-001.

**Input:**

```csharp
class C
{
    void M(int a)
    {
        while (true)
        {
            break; /* a
            b */
            a++;
        }
    }
}
```

The same happens with `b */ a++;` on one line.

**Real output:**
- RH5011 reports `break`.
- The fix inserts an empty line directly below the line of `break;`, which is inside the comment, between `/* a` and `b */`.
- That line is comment text, not a blank line (SHARED-BLANK-001), so the diagnostic stays. Every further application inserts one more empty line into the comment: four after `apply-fix.sh`'s cap of 4, and fifty after my harness's cap of 50.
- The formatter inserts the blank line after `b */`.

**Why it looks wrong:**
- The fix edits a comment's text.
- The fix can never clear its diagnostic (SHARED-FIX-001).

**Decision needed:** a bug against SHARED-FIX-001. The fix inserts the line after the comment's end, or it is withheld.

---

### Issue `SDD open questions: sdd/specs/analyzer/RH5030.md`

#### Q6 — RH5030's fix needs two applications and leaves broken layout when a block comment spans the gap

**Touches:** RH5030-003, SHARED-FIX-001.

**Input:**

```csharp
class C
{
    void M(int a)
    {
        if (a > 0)
        {
        } /* a
        b */ a++;
    }
}
```

**Real output:**
- RH5030 reports `}`.
- The first application inserts a line break at the start of `a++`'s leading trivia. That trivia is empty because the comment belongs to the brace's trailing trivia, so the fix only breaks the line and puts `a++;` at column 0 below `b */ `.
- RH5030 then reports again, and the second application inserts the blank line.
- The final text is `} /* a`, then `b */ ` with a trailing space, then an empty line, then `a++;` at column 0.
- The formatter reaches the expected layout.

**Why it looks wrong:**
- One application does not correct the diagnostic (SHARED-FIX-001).
- The result leaves trailing whitespace and an unindented statement.

It is not a duplicate of thoenissen/Reihitsu#972, which concerns the SHARED-STMT base and two inserted lines.

**Decision needed:** a bug against SHARED-FIX-001. The fix produces the blank line and the indentation in one application, or it is withheld.

---

### Issue `SDD open questions: sdd/specs/analyzer/RH5023.md`

#### Q7 — RH5023 reports an empty span at the end of a file that ends with one blank line

**Touches:** RH5023-001, RH5023-003, RH5023-004, SHARED-FIX-001.

**Input:** the file text `"class C\n{\n}\n\n"`, which is one blank line after `}` followed by the final line break. CRLF behaves the same.

**Real output:**
- RH5023 reports one diagnostic with a zero-length span at the end of the file (position 13). It treats the position after the last line break as a blank line that follows the blank line above.
- The fix deletes the empty span and nothing changes. `scripts/apply-fix.sh RH5023` reports `no-progress` under LF and CRLF.
- With `"class C\n{\n}\n\n\n"` it reports the real second blank line and the same empty span.

**Why it looks wrong:**
- The file has a single blank line, which RH5023-001's `Why:` allows.
- The diagnostic's fix can never clear it (SHARED-FIX-001).

**Decision needed:** a bug against SHARED-FIX-001. The position after the last line break is not a line.

---

### Issue `SDD open questions: sdd/specs/analyzer/RH5022.md`

#### Q8 — RH5022 does not report a blank line below an opening brace that carries a trailing comment

**Touches:** RH5022-001, FMT-BLANK-007, SHARED-BLANK-005.

**Input:**

```csharp
class C
{
    void M(int a)
    { // note

        a++;
    }
}
```

**Real output:**
- RH5022 reports nothing, because the brace's line does not end with the brace.
- The formatter removes the blank line (FMT-BLANK-007, whose `Why:` says the formatter mirrors RH5022).
- The mirrored shapes are reported by their rules: RH5024 reports the blank line above `} // note`, and RH5025 reports the blank line above `{ // note`.

**Why it looks wrong:**
- The line sits directly below the opening brace and separates nothing.
- Analyzer and formatter disagree.

Counterpoint: SHARED-BLANK-005 treats a brace line with a trailing comment as a content line for regions.

**Readings:**
- A — not reported. A clarification states the exclusion in RH5022-001, and FMT-BLANK-007's mirror sentence becomes imprecise.
- B — reported. A bug fix in RH5022.

---

### Issue `SDD open questions: sdd/specs/shared/blank-line-spacing.md`

#### Q9 — RH5024 and RH5032 cycle on a `#region` directly above a closing brace

This is the skeptic's Question A. It belongs to the shared issue because one reading changes SHARED-BLANK-006.

**Touches:** RH5024-001, RH5024-005, RH5032-001, SHARED-BLANK-006, FMT-BLANK-013.

**Input:**

```csharp
class C
{
    int _a;

    #region R

}
#endregion
```

**Real output:**
- RH5024 reports the blank line between `#region R` and `}`, and its fix deletes it.
- RH5032 then reports `#region R`, because SHARED-BLANK-006 exempts only `#endregion` before `}`, and its fix inserts the line again. This is the behavior the existing test `VerifyRegionBeforeClosingBraceIsDetectedAndFixed` pins.
- The formatter keeps the line, so formatter output is flagged by RH5024.

**Why it looks wrong:** no text satisfies both rules.

**Readings:**
- A — RH5024 does not report a blank line directly below a `#region`, and RH5032's output stands. This is a bug fix in RH5024 and matches the formatter.
- B — a `#region` before `}` is exempt in SHARED-BLANK-006, as `#endregion` already is. A change doc first, because SHARED-BLANK-006 and FMT-BLANK-013 change.

#### Q11 — A blank line below a documentation comment does not count as a blank line for the statement blank-line rules

**Touches:** SHARED-BLANK-001, SHARED-STMT-001, SHARED-STMT-004, RH5011-001, RH5011-004.

**Input:**

```csharp
class C
{
    void M(int a)
    {
        while (true)
        {
            break;
            /// doc

            a++;
        }
    }
}
```

**Real output:**
- RH5011 reports `break` although a line holding only whitespace stands between `/// doc` and `a++;`.
- Its fix inserts a second blank line above `/// doc`.
- The same holds for the SHARED-STMT rules: with `a--;` in place of `break;` and `if (a > 0) { }` in place of `a++;`, RH5002 reports `if` and its fix inserts a blank line above `/// doc`.
- `SyntaxTriviaUtilities.ContainsBlankLine` resets its count on the documentation-comment trivia, whose own trailing line break is inside the comment's structure.

**Why it looks wrong:**
- SHARED-BLANK-001 counts that line as blank, and SHARED-STMT-001 says it relies on SHARED-BLANK-001.
- The fix doubles a separator that is already there.

Low severity: a `///` comment inside a method body documents nothing.

**Decision needed:** a bug (the helper counts the line, matching SHARED-BLANK-001), or intended (a clarification adds an exception to SHARED-BLANK-001 or SHARED-STMT-001).

---

### Issue `SDD open questions: sdd/specs/shared/code-fixes.md`

#### Q10 — A run of blank lines next to a brace or chained keyword needs two applications of each rule

This is the skeptic's Question B. It belongs to the shared issue because the question is how SHARED-FIX-001 is read across rules.

**Touches:** SHARED-FIX-001, RH5022-001, RH5022-003, RH5024-001, RH5024-004, RH5025-001, RH5025-003, RH5026-001, RH5026-003, RH5027-001, RH5027-003, RH5023-001, FMT-BLANK-007, FMT-BLANK-008.

**Input:**

```csharp
class C
{
    void M()
    {


        M();


    }
}
```

The analogous run of two blank lines above `{`, `else`, `catch`, `finally`, or a do-statement `while` behaves the same way.

**Real output:**
- RH5022 reports the first line of the run below `{`, and RH5023 reports the second.
- RH5024 reports the line directly above `}`, which is the run's last line. RH5023 reports that same line, so nobody reports the first line of the run until the second line is gone. RH5025, RH5026 and RH5027 behave like RH5024.
- Each of RH5022, RH5024, RH5025, RH5026 and RH5027 alone needs two applications (`fixed (1 action(s), 2 iteration(s))`).
- For RH5022 together with RH5023, one pass of both clears the run. For RH5024 to RH5027 together with RH5023 it does not, because they report the same line.
- The formatter removes the whole run in one pass (FMT-BLANK-007, FMT-BLANK-008).

**Why it looks wrong:** one application does not correct the reported problem. Whether SHARED-FIX-001 allows that when another rule (RH5023) owns the run is not specified.

**Readings:**
- A — each rule alone clears the run in one application, by reporting every line or deleting the whole run. A bug fix against SHARED-FIX-001.
- B — runs belong to RH5023, and SHARED-FIX-001 is read across rules. That is a clarification for RH5022 only. For RH5024 to RH5027 it needs a behavior change so they report the run's first line, so a change doc.

## Follow-up candidates (out of scope, not filed)

- `BlankLineCodeFixUtilities`, used by RH5030's fix and RH3208's, has no spec owner. When RH3208 is specified, decide whether it belongs to RH5030 or to a shared spec.
