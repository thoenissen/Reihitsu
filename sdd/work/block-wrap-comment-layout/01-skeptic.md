Role: skeptic
Commit: 79f96c1
Verdict: proceed narrower

## Premise

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The issue's own "Affected" and "Done when" sections prescribe a solution (one shared block-wrap component, shrunken directive guards); they are treated as proposals below, not as the target.

**Reachability.** Every case below is reached by whole-document formatting through the CLI (`scripts/format.sh` on a file), which runs `StructuralTransformPhase` (phase 2) and `SwitchCaseBracePhase` (phase 8) in the full pipeline. No direct method call is needed.

**Method.** The issue's two inputs, plus fixtures for every candidate below, were formatted twice with `scripts/format.sh <dir>` on branch head `79f96c1` (= `main` `fbf5b6f` plus the run's `sdd/work/` files) and the two passes compared. Fixtures lived in the scratchpad and are deleted.

**The `if` / `else` half of the issue is already met on `main`.** PR #1513 (thoenissen/Reihitsu#1488) replaced the trivia rebuild in `ControlFlowBraceTransform` with `GetCarriedLeadingTrivia` / `GetCarriedTrailingTrivia` (`src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/ControlFlowBraceTransform.cs:149-195`), which keep the trivia from the line of the first leading comment on, and keep the trailing trivia as written. The issue's `if` input formats, in one stable pass, to exactly the issue's "Required output":

```csharp
        if (a)
        {
            // a

            // b
            A();
        }

        if (b)
        {
            /* lead */ C();
        }

        if (c)
        {
            /* one */ /* two */
            E();
        }
```

The same three layouts in an `else` body hold too (`// a` blank `// b`; `/* lead */ A();`; `/* one */ /* two */` then `A();`), as do `A(); /* x */ // y` and `A();    // four` (gap kept) as `if` / `else` bodies. The issue's "Today's output" for `if` is stale.

**The switch half reproduces.** The issue's switch input gives today, stable over two passes:

```csharp
            case 1:
                {
                    // a
                    // b
                    A();
                    A();
                }
                break;

            case 2:
                {
                    /* lead */
                    C();
                    C();
                }
                break;
```

This violates FMT-SWITCH-007 (`sdd/specs/formatter/switch-case-braces.md:508`), which states that the layout of the label's trailing comment, the first statement's leading comments, and the last statement's trailing comments is kept as FMT-PIPE-013 states, and FMT-PIPE-013 itself (`sdd/specs/formatter/pipeline.md:340`), whose scope names the braces of a switch section.

**A further failing member with a safety-rail consequence (FMT-PIPE-004, idempotency).** A `///` comment leading the first statement of a section that gets braces:

```csharp
            case 2:
                /// doc
                /// more
                A();
                A();
                break;
```

Pass 1 writes a blank line after `/// more` inside the new block; pass 2 removes it. The same with a single `/// doc`. The formatter is not idempotent on this input today. The same `///` as an `if` body is stable.

**Statements of the issue about today's code, checked against `79f96c1`:**

| Statement (written against `735d0dba`) | Today |
|---|---|
| `CollectComments` is identical in both components | Stale. `ControlFlowBraceTransform` has no `CollectComments` any more; only `SwitchCaseBraceRewriter.CollectComments` exists (`SwitchCaseBraceRewriter.cs:734`) |
| The rebuilds are copies: `ControlFlowBraceTransform.NormalizeLeadingTrivia` / `NormalizeTrailingTrivia` and `SwitchCaseBraceRewriter.BuildLeadingTrivia` / `BuildTrailingTrivia` | Stale for `ControlFlowBraceTransform` (methods gone, replaced by carry-as-written). True for the switch side (`SwitchCaseBraceRewriter.cs:570`, `:652`, plus `BuildLabelTrailingTrivia` and `BuildTerminalBreakLeadingTrivia`, which the issue does not name) |
| Both need a directive guard because they recreate trivia | Half stale. `ControlFlowBraceTransform.CanWrap` (`:96`) still refuses directive-bearing bodies, but its own rationale is now the brace landing beside a directive, not a rebuild. `SwitchCaseBraceRewriter.SectionCarriesDirectives` (`:747`) still guards a rebuild |
| FMT-PIPE-013 tests for `if`, `else`, switch use a single own-line comment | Stale for `if` (`StructuralTransformSeparatorFinalOutputTests` covers two leading block comments on the statement line, a blank line between leading comments, two trailing comments, trailing gaps) and for expression bodies (`PipelineRequirementTests.ConvertedExpressionBodyKeepsTheSpaceBetweenTwoTrailingComments`, `ExpressionBodyArrowCommentTests.BlankLineBetweenTwoCommentsAfterArrowIsKept`). True for `else` (`PipelineRequirementTests.BracedElseBodyKeepsOwnLineAndTrailingCommentsOnTheirLines`) and switch (`PipelineRequirementTests.BracedSwitchSectionKeepsOwnLineAndTrailingCommentsOnTheirLines`) |
| Blocked by B3 (#1488) | Closed; PR #1513 merged. It also changed the separator contract only for the structural transforms (FMT-STRUCT-026); the switch phase is not governed by it |

**Is the spec wrong instead?** No. FMT-PIPE-013 and FMT-SWITCH-007 have sound `Why:` texts and the `if` path already meets them; the switch path is the only one that does not.

**Conflict with the counterpart surface.** Keeping `/* lead */ C();` on one line leaves a shape RH5104 (Comments must be on their own line) reports. That is no SHARED-DIAG-010 conflict: RH5104's fix moves the comment above the line (RH5104-004), which is not the formatter's layout, so SHARED-DIAG-010 does not cover it; and the formatter already keeps that shape in an `if` body and in plain code. No other rule reports the switch-brace comment layout. No analyzer change.

## Mechanism and candidates

**Mechanism.** `SwitchCaseBraceRewriter.AddBraces` rebuilds every trivia list it carries across a brace it inserts from the comment trivia of that list alone, writing a fixed separator around each comment (a line break after every leading comment, one space in front of the trailing comments, nothing between them), so any whitespace or line break the author wrote between or around those comments that differs from that separator is lost, and a comment that ends with its own line break (`///`) gets a second one.

**Dispatch.** The candidate sites are the trivia rebuilds `AddBraces` performs (`SwitchCaseBraceRewriter.cs:787-878`), each on a list of `SyntaxTriviaList`:

1. last label's trailing trivia — `BuildLabelTrailingTrivia` (`SwitchLabelSyntax`, any of `CaseSwitchLabelSyntax`, `CasePatternSwitchLabelSyntax`, `DefaultSwitchLabelSyntax`);
2. first wrapped statement's leading trivia — `BuildLeadingTrivia` (`StatementSyntax`, any kind);
3. last wrapped statement's trailing trivia (its last token) — `BuildTrailingTrivia`;
4. terminal `break`'s leading trivia — `BuildTerminalBreakLeadingTrivia` (`BreakStatementSyntax` per `BlankLineSpacingPolicy.IsTerminalDirectSwitchSectionBreak`);
5. terminal `break`'s trailing trivia — `BuildTrailingTrivia`.

`RemoveBraces` is unreachable (switch-case-braces.md non-goal: no shipped behavior removes braces) and not a candidate.

The other FMT-PIPE-013 wrap sites, from `StructuralTransformPhase.CreateRewriters` and the `SyntaxFactory.Block(` creators: `ControlFlowBraceTransform` (`if` body, `else` body) and `ExpressionBodyToBlockConverter` (the seven expression-bodied member kinds via their transforms). Both were traced and hold for every FMT-PIPE-013 layout (see matrix).

## Breaking inputs

Failure classes that reach the changed code: **FC-004** (the rebuild collapses gaps around comments), **FC-016** (the `if` path was fixed by #1513 while the sibling switch path keeps the same hazard), **FC-021** (`/* */` ending mid-line in front of a statement, two comments on one line), **FC-019** (each layout tested with comments in the gap), **FC-010** / **FC-017** (the `///` member is a phase not stable on its own output; tests must assert two passes and CRLF as well as LF, FMT-PIPE-020), **FC-005** / **FC-020** (the directive guard `SectionCarriesDirectives` must keep refusing directive-bearing sections — FMT-SWITCH-010 is unchanged), **FC-033** / **FC-034** (the reproduction uses the issue's exact shapes, under LF and CRLF), **FC-026** (a switch nested in an `if` body that also gets braces: both wraps in one pass).

**Class matrix** — site × layout. "Fails" means today's output differs from the input layout; "demanded" means an unchanged requirement (FMT-SWITCH-007 with FMT-PIPE-013) requires the kept layout.

| Site | L1 space between two comments on one line | L2 comment on its statement's line | L3 blank line between leading comments | L4 gap between code and a trailing comment | L5 `///` that ends with its own line break |
|---|---|---|---|---|---|
| 1 label trailing | **fails, demanded**: `case 4: /* p */ /* q */` → `case 4: /* p *//* q */` | holds (stays on label line) | n/a | collapses: `case 3:    // label` → `case 3: // label` — output question 1 | not traced (a `///` behind a label is not a plausible shape) |
| 2 first statement leading | **fails, demanded**: `/* one */ /* two */` + `A();` → each comment on its own line | **fails, demanded**: `/* lead */ C();` → `/* lead */` / `C();`; also `/** doc */ A();` and a multi-line `/* multi` / `line */ A();` | **fails, demanded**: `// a` blank `// b` → blank dropped | n/a | **fails, idempotency**: `/// doc` → blank line after it in pass 1, removed in pass 2 |
| 3 last statement trailing | **fails, demanded**: `A(); /* x */ // y` → `A(); /* x */// y` | holds | n/a | collapses: `A();    // four` → `A(); // four` — output question 1 | n/a |
| 4 terminal `break` leading | splits: `/* b1 */ /* b2 */ break;` → `/* b1 */` / `/* b2 */` / `break;` — output question 2 | splits: `/* b2 */ break;` → `/* b2 */` / `break;` — output question 2 | holds (FMT-SWITCH-007 keeps it; `// b1` blank `// b2` kept) | n/a | holds (`EndsWithLineBreak` handled at `:592`) |
| 5 terminal `break` trailing | `break; /* x */ // y` → `break; /* x */// y` — output question 2 | holds | n/a | collapses: `break;    // gap` → `break; // gap` — output question 1 | n/a |
| `if` / `else` body (`ControlFlowBraceTransform`) | holds | holds | holds | kept as written (change doc `phase-tree-round-trip.md`) | holds |
| expression body (`ExpressionBodyToBlockConverter`) | holds (`42 /* x */ /* y */;`) | holds where FMT-STRUCT-006 specifies it | holds | not traced | not traced |

Candidate inputs the test implementer must cover (full pipeline, two passes, LF and CRLF), all as switch sections that get braces because the switch has a multi-line section:

- the issue's switch input verbatim (`case 1`: `// a` blank `// b` then `A(); A(); break;`; `case 2`: `/* lead */ C(); C(); break;`);
- `/* one */ /* two */` on its own line above the first statement;
- `/** doc */ A();` and a multi-line block comment ending in front of the first statement on its line;
- `A(); /* x */ // y` as the last wrapped statement;
- `case 4: /* p */ /* q */` as the last label, also with a `default:` label and a pattern label `case int n when n > 0:`;
- `///` leading the first statement (one line, two lines) — the idempotency member;
- a switch inside an unbraced `if` body, with a comment layout from the list above (FC-026);
- the `else` body with the three issue layouts (characterization: passes today; the issue's "Done when" asks for it);
- unchanged by the run, as guards: a section carrying `#if` / `#region` in label or statement trivia stays unbraced (FMT-SWITCH-010), and the existing `TerminalBreakCommentTests` keep passing.

Output-question members (L4 at sites 1, 3, 5; L1/L2 at sites 4, 5) get tests only after the maintainer answers.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter `SwitchCaseBracePhase` (`SwitchCaseBraceRewriter`) | Must change — sites 2 and 3 (and 1 for L1) to meet FMT-SWITCH-007 / FMT-PIPE-013; site 2 for FMT-PIPE-004 |
| Formatter `StructuralTransformPhase` (`ControlFlowBraceTransform`) | Must stay in output; already meets FMT-PIPE-013. Touched only if the lead takes the shared-component option (internal) |
| `ExpressionBodyToBlockConverter` | Not involved |
| Other formatter phases (blank lines, line breaks, spacing, indentation) | Must stay. Note: the switch phase runs after the blank-line and line-break phases (pipeline.md phase table, rows 6-8), so it writes the brace lines itself; that is why its separator contract differs from FMT-STRUCT-026 |
| Directive guards (`SectionCarriesDirectives`, `CanWrap`) | Must stay — FMT-SWITCH-010 and FMT-STRUCT-003 are unchanged MUST NOTs |
| Analyzers, code fixes, Fix All | Not involved. RH7501's fix works on an existing block; RH5104 is not covered by SHARED-DIAG-010 (see Premise) |
| `Reihitsu.Core` | Not involved |
| Rule docs | Not involved |
| Specs | FMT-PIPE-013 and FMT-SWITCH-007 need no change for the demanded members. A maintainer answer to output question 1 or 2 is recorded in FMT-SWITCH-007 (spec author, clarification). FMT-STRUCT-003's `Why:` ("Bracing rebuilds the body's surrounding trivia from its comments only") is stale since #1513; its second clause still carries the requirement — optional clarification, lead's call |
| Tests | Must add: switch regression tests tagged `FMT-PIPE-013` / `FMT-SWITCH-007`, the `///` idempotency test (`FMT-PIPE-004`), `else`-body characterization tests |

## Simpler option

Fix the switch path only: carry the trivia of the five sites as written (the switch phase still writes the line breaks the new braces need, since the line-break phase has already run), and leave `ControlFlowBraceTransform` alone. That closes every demanded member and the `///` idempotency member without touching the structural phase, whose output is already correct. Whether to extract a shared helper is an internal choice (below).

## Output questions

**1 · Formatter · braced switch section — gap in front of a trailing comment**

Input:
```csharp
switch (x)
{
    case 1:    // label
        A();
        A();    // four
        break;    // gap
}
```

Before (`main`):
```csharp
switch (x)
{
    case 1: // label
        {
            A();
            A(); // four
        }
        break; // gap
}
```

After, A (recommended):
```csharp
switch (x)
{
    case 1:    // label
        {
            A();
            A();    // four
        }
        break;    // gap
}
```

After, B: as Before (the gap in front of a trailing comment collapses to one space where the switch phase braces a section).

Why A: the same statements keep their gaps in a plain block and in a braced `if` / `else` body (maintainer decision in `sdd/changes/2026-10/phase-tree-round-trip.md`), so B leaves the braced switch section the one place where a wrap changes the gap.

**2 · Formatter · braced switch section — comments around the terminal `break`**

Input:
```csharp
switch (x)
{
    case 1:
        A();
        A();
        /* b1 */ /* b2 */ break; /* x */ // y
}
```

Before (`main`):
```csharp
switch (x)
{
    case 1:
        {
            A();
            A();
        }

        /* b1 */
        /* b2 */
        break; /* x */// y
}
```

After, A (recommended):
```csharp
switch (x)
{
    case 1:
        {
            A();
            A();
        }

        /* b1 */ /* b2 */ break; /* x */ // y
}
```

After, B: as Before.

Why A: the `break` stays outside the block but its trivia is rebuilt by the same wrap; FMT-PIPE-013's `Why:` says a wrap that loses this layout only because it rebuilt the trivia is no normalization, and FMT-SWITCH-007 already applies FMT-PIPE-013 to these comments' blank lines.

## Internal choices

1. **Scope of FMT-PIPE-013 vs. the terminal `break`.** FMT-PIPE-013 speaks of "the comments that the wrapped code carries"; the terminal `break` is not wrapped. The two readings differ in output, so they are output question 2, not an internal choice.
2. **Shared block-wrap component vs. switch-local fix.** A shared helper that carries trivia across an inserted brace would serve both `ControlFlowBraceTransform` and `SwitchCaseBraceRewriter`, but the two run at different stages with different separator contracts: the structural transform writes separators only (FMT-STRUCT-026) and leaves brace lines to the line-break phase; the switch phase runs after the line-break and blank-line phases and must write the brace lines and FMT-SWITCH-007's blank line in front of a `break` comment itself. A shared helper therefore has to take the separators as input. Consequence of sharing: one owner of "how a wrap carries comment trivia" (see Architecture findings), at the cost of touching a component whose output is already right. Consequence of not sharing: two implementations of the same decision remain. Recommendation: let the lead decide under *Architecture findings*; the narrow switch-local fix is sufficient for every demanded member, and nothing visible depends on the choice.

## Issue goals

thoenissen/Reihitsu#1490 is `agent-filed`.

- **"One shared block-wrap component moves the wrapped statements' trivia unchanged and adds only the line break a new brace needs … both use it."** A prescribed solution. Drop as binding; keep as the internal option above. Its premise ("adds only the line break a new brace needs") does not fit the switch phase, which runs after the line-break phase and writes the whole brace layout. Dropping it changes no visible output. "Moves the trivia unchanged" would also change the L4 gaps — that part is output question 1, not decided by the goal.
- **"Their directive guards shrink to the cases where a directive cannot move into a block (FMT-SWITCH-010's `#if` region)."** Drop. FMT-STRUCT-003 (`structural-transforms.md:108`) and FMT-SWITCH-010 (`switch-case-braces.md:772`) are unchanged MUST NOT requirements that demand today's output for every directive-bearing body and section; shrinking the guards would brace bodies and sections carrying `#region`, `#pragma`, or a balanced `#if`, a visible behavior change that needs a change doc (behavior-change lane) and is not part of FMT-PIPE-013. Dropping keeps the output the unchanged specs demand, so nothing visible is in question. Follow-up candidate below.
- **"Regression tests tagged FMT-PIPE-013 cover the three layouts for an `if` body, an `else` body, and a switch section."** Keep. `if` is already covered by `StructuralTransformSeparatorFinalOutputTests` (`TwoLeadingBlockCommentsOfABracedBodyStayOnTheStatementsLine`, `BlankLineBetweenTwoLeadingCommentsOfABracedBodyIsKept`, and the trailing-comment tests); `else` needs characterization tests (pass today); switch needs the reproduction.
- **Required output for `if`.** Already met on `main`; no change. The PR should say so: the issue's `if` premise was made moot by #1513.
- **"Before the run" says the target behavior and "Done when" stay binding.** The label `agent-filed` makes them proposals (`squad/README.md`, *Issues are hypotheses*); the two drops above put nothing visible in question.

## Architecture findings

**Two components own the decision "how a wrap carries the comment trivia around the code it wraps".** `ControlFlowBraceTransform` (`GetCarriedLeadingTrivia` / `GetCarriedTrailingTrivia`, `ControlFlowBraceTransform.cs:149-195`) carries the trivia as written; `SwitchCaseBraceRewriter` (`BuildLeadingTrivia`, `BuildTrailingTrivia`, `BuildLabelTrailingTrivia`, `BuildTerminalBreakLeadingTrivia`, `CollectComments`, `SwitchCaseBraceRewriter.cs:570-737`) rebuilds it from comments only. FMT-PIPE-013 states one rule for both. Input that shows them disagreeing, the same last statement in both wraps:

```csharp
if (a)
    A();    // four
```

keeps the four spaces inside the new block, while the same statement as the last of a section that gets braces,

```csharp
case 1:
    A();
    A();    // four
    break;
```

comes out as `A(); // four`; likewise `/* lead */ C();` stays on one line in the `if` body and is split in the switch section. `ExpressionBodyToBlockConverter` is a third carrier of the same rule for expression bodies and agrees with `ControlFlowBraceTransform` on every traced layout. The defect of this run (the switch rebuild) is this structural cause; the lead decides between a shared carrier and a narrow switch-local fix.

## Follow-up candidates

1. **Directive guards for wraps that now carry trivia.** With trivia carried instead of rebuilt, FMT-STRUCT-003's and FMT-SWITCH-010's refusal of every directive-bearing body/section may be wider than needed (`#region`, `#pragma`, a balanced `#if` wholly inside the trivia). A behavior change with its own change doc and output questions; the issue's "Done when" asked for it. Not a defect of this run: an unbraced body or section is the outcome the unchanged specs demand.
2. **FMT-STRUCT-003 `Why:` is stale** since #1513 ("Bracing rebuilds the body's surrounding trivia from its comments only" no longer describes `ControlFlowBraceTransform`). Text-only; the lead may fold it into this run as a spec-author clarification instead.
3. **Comment directly at the arrow of an expression body** lands on the new opening brace's line with no space, `int Get() => /* one */ /* two */ 42;` → `{/* one */ /* two */`. FMT-STRUCT-006 explicitly leaves this placement unspecified and existing tests assert it (`ExpressionBodyDirectiveGuardTests.MethodWithInlineCommentIsStillConverted`, `ExpressionBodyNextLineExpressionTests`); outside this issue. Only worth filing if no issue already tracks FMT-STRUCT-006's open placement.
