Role: spec author
Commit: 5dd288a
Verdict: change doc written

The prompt carried no intended fix or suspected cause beyond the issue text; the issue's proposed behavior and "Done when" were treated as proposals (`agent-filed`).

## Files

- `sdd/changes/2026-11/switch-section-line-start.md` — change doc, `Status: Draft`.
- `sdd/specs/formatter/line-breaks.md` — FMT-BREAK-033 to FMT-BREAK-035 added; FMT-BREAK-006 changed; intro and `Uses` (+FMT-BLANK-004, FMT-INDENT-003, FMT-SWITCH-006, SHARED-BLANK-008).
- `sdd/specs/formatter/blank-lines.md` — FMT-BLANK-004 changed (one sentence, `Why:`, one example added); non-goal wording; `Uses` (+FMT-BREAK-030, FMT-BREAK-033, SHARED-BLANK-008).
- `sdd/specs/formatter/switch-case-braces.md` — one non-goal added (pointer only, no `Uses` change).
- `sdd/specs/requirement-ids.txt` — FMT-BREAK-033, FMT-BREAK-034, FMT-BREAK-035.
- No rule page changes: no diagnostic, no fix boundary, and no rule text changes. `documentation/` holds no formatter page.

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"`: 36 passed, 1 failed — `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered`, for exactly FMT-BREAK-033, FMT-BREAK-034, FMT-BREAK-035 (no covering test yet; expected until the test implementer runs). Every structure, `Uses`, ledger, and code-owner check passes.

## Issue statements against current `main` (5dd288a)

Checked by reading the code and by `scripts/trace.sh` / `scripts/format.sh` on the issue's inputs.

| Statement | Holds? | Evidence |
|---|---|---|
| `BlankLineBreakSpacingRewriter.EnsureBlankLineBeforeSection` inserts exactly one line break whether or not the section starts its line | Holds | It inserts one `EndOfLine` at index 0 of the section's first-token leading trivia after `HasBlankLineBeforeToken` is false |
| The statement rules insert two when the gap has none (`TokenGapAnalysis.RequiredLineBreakCountForBlankLine`) | Holds | `BlankLineEditor.InsertBlankLineBeforeToken` uses it; `TokenGapAnalysis` is in `src/Reihitsu.Core` |
| In front of a section that does not start its line, the blank line appears only in the second pass | Holds | Trace of input 1: pass 1 `BlankLinePhase` writes only the line end; pass 2 `BlankLinePhase` adds the blank line in front of `case 2:` |
| After a section ending in `return` or `throw`, the sections stay on one line | Holds | Input 2 gives `case 1: return 1; case 2: return 2;`, stable after pass 1. Also holds behind `goto case` |
| The line-break phase has no rule for a section label; `LineBreakBlockRewriter.VisitSwitchSection` handles only the section's block | Holds in substance | `VisitSwitchSection` also applies `EnsureStatementsStartOnSeparateLines` (statements 2..n only, FMT-BREAK-006) and `NormalizeContainedBlock`; nothing places the section's first token, its labels, or its first statement |
| The issue's "today's output" for both inputs | Holds exactly | Trace output matches both blocks |
| Blocker B2 (thoenissen/Reihitsu#1487) provides the line-start fact | Holds | SHARED-BLANK-008 and `SyntaxTokenPositionUtilities.StartsLine` exist |

Nothing an earlier package made moot was found.

Further current outputs found (all from one `format.sh` run, used in the change doc): `case 1: case 2: A();` stays on one line; `default: break;` stays on one line; `case 1: /* c */ A();` keeps `A()` behind the comment; `case 1: ; break;` becomes `case 1:; break;` (see follow-ups); RH5417's code fix on a one-line method body holding a switch writes `case 1: return 1; case 2: return 2;` on one line (`scripts/apply-fix.sh RH5417`).

## Changed requirements

- FMT-BREAK-033 — Every switch section starts its own line — added
- FMT-BREAK-034 — Each label of a switch section starts its own line — added (depends on output question 1)
- FMT-BREAK-035 — The first statement of a switch section starts its own line — added (its empty-statement sentence and last example depend on output question 2)
- FMT-BREAK-006 — One statement per line — changed: scoped to a statement that follows another statement of the same list; points to FMT-BREAK-002 / FMT-BREAK-035 for first statements. Example unchanged
- FMT-BLANK-004 — `break` is separated on both sides — changed: holds whether or not the section starts its line; the blank-line phase writes the line-ending line break together with the blank line; `Why:` and one example added

## Examples for the tests

FMT-BREAK-033 (two examples: mixed `return`/`goto case`/`throw` with a trailing `/* c */`; multi-line comment vs. `//` comment), FMT-BREAK-034, FMT-BREAK-035 (main example; empty-statement example is a phase-alone example, run through `LineBreakPhase` only), FMT-BLANK-004 (new third example: `break; case 2:` gets the blank line in one run — the regression for FMT-PIPE-004). The issue's two inputs are visible output changes 1 and 2 of the change doc; "Done when" asks that their tests also check the second pass.

## Output questions

The change doc's visible output changes 1, 2, 3, and 5 are the issue's own target behavior and are asked by the lead as listed. Two cases within the issue are open, and the spec is written on the recommendation for each:

**1 · Formatter · labels of one switch section on one line** (the issue asks the change doc to decide this). Touches FMT-BREAK-034.

Input:
```csharp
switch (x)
{
    case 1: case 2:
        A();
        break;
}
```

Before (`main`): unchanged.

After, A (recommended):
```csharp
switch (x)
{
    case 1:
    case 2:
        A();
        break;
}
```

After, B: unchanged (labels keep the lines the author wrote).

Why A: it is the shape the formatter already writes when it braces such a section (FMT-SWITCH-006), and every value of the section stands at the label column. If B: FMT-BREAK-034 is removed from the spec and ledger before merge, and visible change 4 of the change doc is dropped.

**2 · Formatter · empty statement as a section's first statement.** Touches FMT-BREAK-035 (last sentence of its normative paragraph and its last example).

Input:
```csharp
switch (x)
{
    case 1: ; break;
}
```

Before (`main`):
```csharp
switch (x)
{
    case 1:; break;
}
```

After, A (recommended): same as before.

After, B:
```csharp
switch (x)
{
    case 1:
        ; break;
}
```

Why A: FMT-BREAK-006 already leaves an empty statement and its neighbor as written because moving it only moves a stray `;`; the first statement follows the same exception. What would change the pick: if the maintainer considers `case 1:;` itself wrong (see follow-up candidate 1), B removes that shape for this input. Neither option blocks writing the other tests.

## Internal choices

- The issue's "Done when" item "`EnsureBlankLineBeforeSection` writes no line break that ends a line" is not met literally: the blank-line phase runs before the line-break phase, and the line-break phase inserts no blank line (FMT-BREAK-030), so a one-run blank line in front of a section on the `break`'s line needs the blank-line phase to write both line breaks (the statement rules' count). Spec written that way (FMT-BLANK-004); where no blank line is required, the blank-line phase writes no line break in front of a section and FMT-BREAK-033 owns it. Alternative: keep the issue's wording, which forces either a prediction (FMT-PIPE-001) or a two-pass blank line (FMT-PIPE-004). Output is identical either way.
- Three requirements (section / labels / first statement) instead of one, so each decision has its own `Why:` and question 1 can drop FMT-BREAK-034 cleanly. Alternative: one combined requirement.
- FMT-BREAK-033 excludes the first section (FMT-BREAK-002 already places the token after the switch's `{`). Alternative: cover every section and overlap with FMT-BREAK-002.
- FMT-BREAK-006 rewording treated as part of this change (scope clarification), not a separate requirement. Alternative: leave FMT-BREAK-006 literal, which would read as already demanding the first statement on its own line and make the current code a bug against it.
- The node-level sentence (gap in front of the target kept, FMT-PIPE-010) lives once in FMT-BREAK-033 and names FMT-BREAK-034/035.
- No code owners added: the behavior falls to existing owners (`LineBreakBlockRewriter`, `BlankLineBreakSpacingRewriter`); the implementer may add a type, in which case `sdd/specs/README.md` and the spec header need it.
- `AGENTS.md`'s phase-ownership table was not changed (its `LineBreakPhase` row does not enumerate statement lines either); changing it would add a tooling path to the diff.

## Follow-up candidates

1. **Formatter · `case 1: ;` loses the space between label and empty statement** (outside the issue: horizontal spacing, FMT-SPACE). Reached by whole-document formatting.

   Input:
   ```csharp
   switch (x) { case 1: ; break; }
   ```
   Real output (`main`):
   ```csharp
   switch (x)
   {
       case 1:; break;
   }
   ```
   Looks wrong because every other label is followed by one space before a statement on its line. Readings: A — `case 1: ; break;` (one space after a label's `:`); B — keep `case 1:;`. Recommendation A. Becomes moot for this input if output question 2 is answered B.

2. **RH5103 · a statement of another section on the same line is not reported** — the rule's own gap (P11: RH5103's construct is statements on one line; it skips statements of different sections). RH5103-001/RH5103-003 currently decide "not reported", e.g. `case 4: break; case 5: M();` reports nothing for `M();`. Reporting it would be a behavior change of RH5103 with a change doc. After this change the formatter splits that shape.

3. **New-rule candidate · first statement on its label's line, and labels sharing a line** — a different construct from RH5103 (the label is not a statement; RH5103-001 excludes it), so per P11 a candidate for a new rule, not a widening of RH5103. The formatter now rewrites `case 1: A();` and `case 1: case 2:`.

## Scope notes

- No test in `Reihitsu.Formatter.Test` or `Reihitsu.Cli.Test` has a statement or a second label behind a `case`/`default` label on one line (regex search), and no repository source outside tests has that shape, so the self-hosting rows should be unaffected. Analyzer tests with such inputs (RH5103, RH5008, RH5029) do not format through the formatter in the paths seen; the test implementer should confirm for any fix that formats a switch (RH5417, RH5403 do).
