Role: spec author
Commit: 960af97
Verdict: open question

Two output questions block tests for part of the run: the switch-section gap of #1385 and all of #1239. The #1398 clarification and the statement-path clarification of #1385 are written and do not depend on the answers. Today's outputs quoted below were taken by running `scripts/format.sh` on scratch copies at 960af97.

## #1385 — A required blank line goes after directives (formatter part)

Scope per the triage comments: formatter only (the RH5002/RH5008 code-fix part moved to #1447); bug against FMT-BLANK-016; no change doc. The issue's "Lane: behavior change", its change-doc "Done when" item and its SHARED-STMT-004 item no longer apply (triage).

### Changed requirements
- FMT-BLANK-016 — A required blank line goes after directives, never directly in front of one — clarified: the coverage sentence now names every statement path (FMT-BLANK-001 to -005, i.e. compound/jump statement, local declaration and the statement behind one, `yield`, statement behind `break`, statement behind a closing brace), including a gap that holds a whole conditional group with disabled text. Two examples added. The switch-section gap is deliberately not named yet (output question 1).

### Examples for the tests
- FMT-BLANK-016: the new conditional-group example (`#if NEVER … #endif`, `// why`, `if`) and the local-declaration/after-`break` example. Today both put the blank line above `#if` (confirmed). The existing tests `ReihitsuFormatterNodeBlankLineTests.InsertsBlankLineAboveDirectiveOfCommentedBlockStatementAfterBreak` pins the wrong output for the after-`break` path; `InsertsBlankLineAboveDirectiveOfSwitchSectionAfterBreakSection` pins the wrong output for the section gap and is rewritten once question 1 is answered.
- The `while` statement directly behind `#endif` in the second example gets no blank line (FMT-BLANK-006) — a negative case worth keeping in the test.

### Output questions

**1 · Formatter blank lines — directive between a `break` section and the next switch section**

Input:
```csharp
switch (value)
{
    case 1:
        value++;
        break;
#pragma warning disable CS0168
    case 2:
        value--;
        break;
}
```

Before (`main`, confirmed): a blank line directly above `#pragma` (same with `#if !DEBUG` in place of `#pragma`).
```csharp
        break;

#pragma warning disable CS0168
    case 2:
```

After, A (recommended) — a directive directly in front of a section suppresses the section's blank line, as FMT-BLANK-006 does for a statement; output equals the input:
```csharp
        break;
#pragma warning disable CS0168
    case 2:
```

After, B — the blank line goes after the directive (literal reading of FMT-BLANK-004 + FMT-BLANK-016, since FMT-BLANK-006 names only statements):
```csharp
        break;
#pragma warning disable CS0168

    case 2:
```

Under both readings, a section led by a comment behind the directive (`break;⏎#if X⏎// c⏎case 2:`) gets the blank line after the directive, in front of the comment.

Why A: it matches the statement path (in a loop body, `break;⏎#pragma …⏎a++;` gets no blank line), matches RH5011-002 (the analyzer does not report a `break` followed by a directive, in a section gap too), and is what the issue asks for; B puts a blank line inside the conditional region the `#if` opens.

Requirement IDs: FMT-BLANK-004, FMT-BLANK-006, FMT-BLANK-016. Note for the lead: the spec text as written reads as B, so A widens FMT-BLANK-006 to switch sections — a change of written intended behavior; per `sdd/README.md` that needs a change doc recording the answer, unless you judge it a clarification. On A, I write the FMT-BLANK-006 widening (and the change doc if required) and name the section gap in FMT-BLANK-016; on B, I name the section gap in FMT-BLANK-016's coverage sentence with B's example.

## #1398 — A break-only switch section never makes the switch braced

Scope per triage: bug against FMT-SWITCH-006, no change doc; the issue's lane/change-doc lines no longer apply.

### Changed requirements
- FMT-SWITCH-004 — A multi-line delimited pattern in a label makes a section multi-line — clarified: FMT-SWITCH-006 takes precedence; a `break`-only section makes no switch multi-line whatever its labels span and stays unbraced when another section braces the switch. Why extended; two examples added.

### Examples for the tests
- FMT-SWITCH-004: "A section with a multi-line property pattern whose only statement is `break` …" (left unchanged; today the switch is braced — confirmed: `case string s:` gets `{ B(); }`), and "When another section is multi-line …" (braced, `break`-only section bare).

### Output questions
_None._

## #1239 — Whitespace between `#` and `region`

### Changed requirements
None yet; depends on output question 2.

### Output questions

**2 · Formatter region lines — whitespace between `#` and `region` / `endregion`**

Input:
```csharp
class C
{
    # region fields

    int _a;

    #  endregion
}
```

Before (`main`, confirmed):
```csharp
class C
{
    # region Fields

    int _a;

    #  endregion // Fields
}
```

After, A (recommended) — FMT-REGION-007 ("`#region`, one space, description") and the `#endregion` forms of FMT-REGION-002/-004 cover the whole line, so the gap after `#` is removed on every paired region directive, also in disabled branches:
```csharp
class C
{
    #region Fields

    int _a;

    #endregion // Fields
}
```

After, B — the gap after `#` is kept (today's output); the region requirements cover only the text behind the keyword.

Why A: the requirement's own wording writes the line as `#region`, and RH7302 reports the formatter's output under B. Requirement IDs: FMT-REGION-007, FMT-REGION-002, FMT-REGION-004. On A, I clarify those three to name the `#`–keyword gap and add the example; #1239 is then a bug fix. On B, the issue is not a bug (`Refs #1239`) and I add a sentence keeping the gap. A third reading — one policy for the gap after `#` in every directive (`# pragma`, `# if`) — would widen the run to a behavior change and is not recommended here (see follow-ups).

## Internal choices
- #1398: clarified inside FMT-SWITCH-004 (precedence sentence + examples) rather than adding a new requirement; alternative: a new FMT-SWITCH requirement stating the precedence.
- #1385: clarified FMT-BLANK-016's coverage sentence rather than adding examples to FMT-BLANK-002/-004; alternative: one example per asking requirement.
- No new requirement IDs; `requirement-ids.txt` unchanged.

## Non-goals
- #1385: the RH5002/RH5008 code fixes and SHARED-STMT-004 (#1447); blank lines the author wrote in front of a directive (FMT-BLANK-009); region directives (FMT-BLANK-013).
- #1398: switch-section layout other than bracing; sections whose labels wrap only in `or`/`and`/`when` (already not multi-line).
- #1239: RH7302's reading of the description (#1174); directives other than `#region`/`#endregion`.

## Follow-up candidates
- Only if question 2 is answered A: whitespace between `#` and the keyword of other directives (`#  pragma warning disable`, `# if X`) is kept today; whether the formatter normalizes it is a separate, low-priority output question for an `agent-filed` issue.

## Prompt note
The prompt carried no intended fix or hoped-for verdict beyond the issue references.
