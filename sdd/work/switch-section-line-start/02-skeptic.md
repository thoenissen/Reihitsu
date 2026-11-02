Role: skeptic
Commit: 06a5ae8
Verdict: proceed, with one spec correction and one added output question

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The issue's goals and "Done when" items were treated as proposals because the issue is labeled `agent-filed`.

## Premise

The issue is real. Its case is reached by whole-document formatting (the CLI) and by every code fix that formats a node containing a switch statement. The spec author ran RH5417 through `scripts/apply-fix.sh`. RH5403 and RH3208 also format nodes that can hold a switch. Commands below were run on 06a5ae8 (code identical to `origin/main` 324fcd5) with fixtures under the scratchpad, which have been deleted.

- `scripts/format.sh` on the issue's two inputs gives exactly the issue's "today's output". Input 1 gets the blank line in front of `case 2:` only in the second run (`diff` of pass 1 and pass 2: one added line, before `case 2:`). Input 2 is stable with `case 1: return 1; case 2: return 2;` on one line.
- The code agrees with the issue. `BlankLineBreakSpacingRewriter.EnsureBlankLineBeforeSection` inserts one `EndOfLine` at index 0 of the section's first-token leading trivia. `LineBreakBlockRewriter.VisitSwitchSection` places only statements 2..n (`EnsureStatementsStartOnSeparateLines`) and a block body (`NormalizeContainedBlock`). Nothing places a section's first token, its labels, or its first statement.
- Further shapes on `main` (one `format.sh` run, then `scripts/trace.sh --passes 3`):
  - `case 1: continue; case 2: A();` and `case 1: return; case 2: return;` inside a lambda stay on one line.
  - `} case 2:` behind a section whose body is a block ending in `break` stays on one line.
  - `default: case 1: A();` is unchanged.
- No new requirement conflicts with SHARED-DIAG-010:
  - RH5008, RH5029, and the other "preceded by a blank line" rules decide by position, so the first statement of a section stays exempt on any line.
  - RH5204 accepts a label at the label column and a statement one level deeper.
  - RH5011 already reports `break; case 2:` on one line.
- Analyzer-clean code that the formatter will now change is intended under P11. `case 1: A();` and `case 1: case 2:` are a different construct from RH5103's (a label is not a statement, RH5103-001), so they make a new-rule candidate. `case 4: break; case 5: M();` is RH5103's own construct, which RH5103-003 explicitly decides not to report, so changing it would be a behavior change of RH5103. Both are listed under follow-ups. Neither is a conflict.
- The switch-brace decision is not changed by the new line breaks. `IsMultiLineSection` measures `node.Span` of statements (`SyntaxNodeUtilities.IsSingleLine`), label patterns, and first-statement-to-last-statement lines (`HasMultiLineBodyEndingInReturnOrThrow`). FMT-BREAK-033 to -035 change none of these, so no switch flips between braced and unbraced.

The spec diff has one factual error. **FMT-BREAK-034's `Why:` and output question 1's "Why A" say stacked labels are "the shape the formatter already writes when it braces a section (FMT-SWITCH-006)". That is false.** FMT-SWITCH-006 keeps the labels where it finds them; its example input is already stacked. On `main`, `switch (x) { case 1: case 2: A(); B(); break; ... }` is braced as:

```csharp
            case 1: case 2:
                {
                    A();
                    B();
                }
                break;
```

The `Why:` sentence, the change doc's "Labels each start a line" decision, and output question 1's reason have to be reworded before the question goes to the maintainer. The recommendation can stand on its other reason: every value at the label column, as FMT-BREAK-033 argues for sections.

## Mechanism and candidates

_N/A_. This is a behavior change. The candidates below come from the dispatch, not from the issue.

Dispatch: `SwitchStatementSyntax.Sections` (`SwitchSectionSyntax`) → `Labels` (`CaseSwitchLabelSyntax`, `CasePatternSwitchLabelSyntax` with or without `WhenClauseSyntax`, `DefaultSwitchLabelSyntax`) → `Statements` (any `StatementSyntax`; special cases are `EmptyStatementSyntax`, `BlockSyntax`, `LabeledStatementSyntax`, and terminal `BreakStatementSyntax`/`ReturnStatementSyntax`/`ThrowStatementSyntax`/`GotoStatementSyntax`/`ContinueStatementSyntax`).

Phases that write the line break in front of a section boundary, in pipeline order:

1. `BlankLinePhase`: FMT-BLANK-004, leading trivia of the section's first token.
2. `LineBreakPhase`: FMT-BREAK-033 to -035 (new), plus `EnsureBraceOnOwnLine` for the switch's `}`.
3. `SwitchCaseBracePhase.AddBraces`:
   - It always ends a braced section's last line itself: `GetCarriedTrailingTrivia(..., endsLine: true)` on the trailing `break` or last statement, and `closeBraceTrailing = EOL` when there is no trailing `break`.
   - It never reads the next token's leading trivia.

## Breaking inputs

Failure classes that reach the change:

- FC-003: start of line and not start of line, for each of section, label, and statement.
- FC-004, FC-019, FC-021: comments in the gap. `/* */` on one line, the tail of a multi-line `/* */`, `//`, `///`, and `/** */`.
- FC-020: a directive in the gap. It already starts a line and must be kept.
- FC-010, FC-012, FC-017: two passes, LF and CRLF, and the per-phase check that the formatter test helpers already run.
- FC-016: section, label, and first statement are sibling shapes.
- FC-027: a switch nested in a switch section, inside a lambda or local function, and inside a braced switch.
- FC-028, FC-030, FC-032: code fixes that format a switch (RH5417, RH5403, RH3208); analyzer tests must be rerun after the change.

### Class: the line break behind a braced section is written twice (FC-012, FC-010)

**Decision.** How many line breaks stand between the last token of a section that `SwitchCaseBracePhase` braces and the next token. `AddBraces` writes one line break into that last token's trailing trivia. It does not look at line breaks an earlier phase of the same pass put into the next token's leading trivia, so the two add up.

**Check** against FMT-BLANK-004 (exactly one blank line behind a `break` section, none otherwise), FMT-BLANK on closing braces (no blank line before `}`), and FMT-PIPE-004. Run under LF and CRLF, two passes.

| Axis | Members |
|---|---|
| End of the braced section | trailing `break` (the `break` stays outside the block); no `break` (`return`, `throw`, `goto case`, `continue`; the block's `}` gets the line break) |
| Next token | the next section's first token; the switch's closing `}` |
| Line break in the next token's leading trivia from this pass | none (input already on separate lines); written by `BlankLinePhase` (FMT-BLANK-004); written by `LineBreakPhase` (FMT-BREAK-033, or `EnsureBraceOnOwnLine` for `}`) |

Cells settled on `main` (trace `--passes 3`):

- **Fails on `main` today:** braced section (trailing `break` or not) on the line of the switch's `}`. Pass 1 has a blank line in front of `}`, and pass 2 removes it. Shapes: `switch (x) { case 1: case 2: A(); B(); break; case 3: C(); break; }` and `switch (x) { case 1: A(); return 1; case 2: return 2; }`. This is FMT-PIPE-004. The change does not touch it, but it is the same class.
- **Correct on `main` only by accident:** braced section ending in `break`, with the next section on the `break`'s line. `BlankLinePhase` writes 1 line break and `AddBraces` 1 more, which gives the one blank line in pass 1. The amended FMT-BLANK-004 has `BlankLinePhase` write 2 line breaks. If they go into the next section's leading trivia, as `EnsureBlankLineBeforeSection` writes today, the total is 3: two blank lines in pass 1, collapsed in pass 2. **This member is newly exposed by the change.**
- **Depends on where FMT-BREAK-033's line break lands:** braced section without `break`, with the next section on its line. Today `}` + `case 2:` with no blank line (`case 1: A(); return 1; case 2: return 2;`). If FMT-BREAK-033 writes into the next section's leading trivia, a blank line appears between sections that FMT-BLANK-004 gives none.
- **Hold:**
  - Every cell whose input already has the next token on its own line, because the line break is then in the previous token's trailing trivia, which `AddBraces` replaces.
  - Every cell whose section already had a block (`AddBraces` returns it unchanged).
  - The first-statement side (`GetCarriedLeadingTrivia` drops a comment-free leading trivia, so a line break FMT-BREAK-035 puts in front of the first statement is not doubled).

The test implementer must cover the two cells the change touches with two-pass, LF+CRLF regression tests. Minimal shapes:

```csharp
switch (x) { case 1: A(); B(); break; case 2: C(); break; }
```

```csharp
switch (x) { case 1: A(); B(); return 1; case 2: return 2; }
```

Expected: one blank line before `case 2:` in the first, none in the second, both after one run.

### Other candidates the tests must cover

Each is checked for two passes under LF and CRLF.

- Previous section ends in `break`, `return`, `throw`, `goto case`, `goto default`, `continue` (switch in a loop), or a block (`{ ...; break; } case 2:`, which gets no blank line because `EndsInBreak` sees a block).
- Section behind `break; /* c */`, behind a multi-line comment tail, behind `// c` (kept), and behind a directive (kept).
- Labels: `case 1: case 2:`, `default: case 1:`, `case 3: /* c */ default:`, and a label behind a multi-line comment tail (`case 3: /* a⏎b */ case 4:`, unchanged today).
- Pattern label with `when` on the label's line and on its own line.
- First statement:
  - expression statement, `break` (`default: break;`), `return`, local declaration, labeled statement (`case 2: L: A();`);
  - block (FMT-BREAK-008; `case 1: { A(); } break;`);
  - empty statement (`case 1: ; break;` and `case 2: case 3: ; break;`; output question 2);
  - behind `/* c */`, behind a multi-line comment tail, behind `// c` (kept), behind `/// e` (FMT-DOC-001 already moves it, `case 3:⏎/// e⏎A();`), and behind `/** d */` (output question 3 below).
- Braced and unbraced switches for each of the above: `SwitchCaseBracePhase` runs after the line-break phase.
- Nesting: a switch in a section of a switch, in a lambda (`() => { switch (x) { case 1: return; case 2: return; } }`), and in a local function.
- Node level: RH5417 on `int M(int x) { switch (x) { case 1: return 1; case 2: return 2; } return 0; }` (change-doc entry 5). A node-level target that is itself the first statement on a label's line keeps its leading gap (FMT-PIPE-010). That is not a failing member.
- Existing tests that may change: `LineBreakBlockRewriterTests.VisitSwitchSectionSplitsTwoStatementsOnOneLine` (assertion `N(); \nN();` still holds), and the analyzer test project as a whole (FC-032). The RH5029 and RH5103 fixed codes with `case 0: Method(1);` are written without the formatter (fixed code keeps the label-line statement), so they should not change. Confirm that by running them.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `LineBreakPhase` (`LineBreakBlockRewriter.VisitSwitchSection` or a new rewriter) | Must change: FMT-BREAK-033 to -035 |
| `BlankLinePhase` (`BlankLineBreakSpacingRewriter.EnsureBlankLineBeforeSection`) | Must change: FMT-BLANK-004, two line breaks when the section does not start its line |
| `SwitchCaseBracePhase` (`AddBraces`) | Must stay in behavior. Its trailing line break is the doubling partner in the class above; whether it changes is the lead's call (Architecture findings) |
| `IndentationPhase`, `HorizontalSpacingPhase`, `CleanupPhase` | Not involved (existing label and statement columns apply; `case 1:;` spacing is a follow-up) |
| `DocumentationCommentFormattingPhase` | Not involved (`///` behind a label is already moved; `/** */` is thoenissen/Reihitsu#1533) |
| `Reihitsu.Core` (`SyntaxTokenPositionUtilities.StartsLine`, `TokenGapAnalysis`) | Not involved, reused |
| Analyzers (RH5103, RH5011, RH5204, RH50xx blank-line rules) | Must stay |
| Code fixes formatting a switch (RH5417, RH5403, RH3208) | Not changed in source; their output changes through the formatter (change-doc entry 5) |
| Fix All | Not involved |
| Rule docs | Not involved (no rule changes; no formatter page exists) |
| Specs | Changed by the spec author. FMT-BREAK-034 `Why:` needs the correction above |

## Simpler option

None that meets the issue's desired output. Input 1's desired output needs FMT-BREAK-035, and input 2 needs FMT-BREAK-033. FMT-BREAK-034 is the only optional part, and output question 1 already offers dropping it. I agree with the spec author's rejection of the issue's own alternative ("first statement may stay").

## Output questions

The spec author's questions 1 and 2 stand. For question 1, correct the reason as stated under *Premise*. I agree with both recommendations (1A, 2A). I add one:

**3 · Formatter · a `/** */` comment between a label and the first statement on the same line.** Roslyn attaches `/** d */` here to the statement as leading trivia, unlike `/* c */`, which trails the label's colon. FMT-BREAK-001 ("in front of the moved token") and FMT-BREAK-035 ("a comment that trailed the section's last label stays on the label's line") read differently on it. The same shape exists for FMT-BREAK-033 (`return 1; /** d */ case 2:`) and FMT-BREAK-034 (`case 4: /** f */ case 5:`).

Input:
```csharp
switch (x)
{
    case 2: /** d */ A();
        break;
}
```

Before (`main`): unchanged.

After, A:
```csharp
switch (x)
{
    case 2: /** d */
        A();
        break;
}
```

After, B (recommended):
```csharp
switch (x)
{
    case 2:
        /** d */ A();
        break;
}
```

Why B: this is where the formatter already puts this comment. It does so when it splits two statements (`A(); /** d */ A();` becomes `A();⏎⏎/** d */ A();`) and when it braces this section (`{⏎/** d */ A();⏎}` on `main`). Under A, a braced section would also change from `/** d */ A();` to a comment line above `A();` inside the block. This question is low-stakes and becomes moot for this input if thoenissen/Reihitsu#1533 moves such comments in the documentation phase.

The change doc's visible change 3 (`case 1: /* c */ A();` keeps `/* c */` on the label's line) decides for unbraced sections what open issue thoenissen/Reihitsu#1540 asks for braced sections. Its recommended option A gives the same placement. When the lead asks visible change 3, it should name that link, so the maintainer's two answers do not diverge.

## Internal choices

- **Done-when 2 dropped ("`EnsureBlankLineBeforeSection` writes no line break that ends a line"): I agree with the spec author.** With the phases in today's order, it can only hold through a two-run blank line (FMT-PIPE-004) or a prediction (FMT-PIPE-001). thoenissen/Reihitsu#1498 (D1) moves the line-break phase before the blank-line phase and is blocked by this issue, so it is the place where that goal becomes possible. Its own Done-when names statements only, so see follow-up 4. No visible output changes.
- **Where the inserted line breaks land:** in the previous token's trailing trivia, or in the next token's leading trivia. The two produce identical output on their own, but not together with `AddBraces` (class above). This is not a spec question. The tests pin the output and the implementer chooses.
- **FMT-BREAK-033 excludes the first section:** agree (FMT-BREAK-002 already places it).
- **Three requirements instead of one:** agree. It lets output question 1 drop FMT-BREAK-034 cleanly.
- **FMT-BREAK-006 rewording:** agree. Without it, FMT-BREAK-006 would read as already demanding the first statement on its own line, which would make the current code a bug against an unchanged requirement.

## Issue goals

- "A new FMT-BREAK rule …": kept (FMT-BREAK-033, FMT-BREAK-035).
- "The blank-line phase's section rule then only counts blank lines": changed. The phase still writes both line breaks behind a `break` on the same line. No visible change.
- "Uses the same line-break count as the statement rules": kept.
- Change-doc decisions: all three are made. The labels decision depends on output question 1.
- Done-when 1 (both inputs in one run, tests check the second run): kept. Extend it to the braced-switch cells above.
- Done-when 2: dropped in its first half. Its second half ("the line-break phase owns the section start") holds except in front of a section behind a `break` that is on the same line. No visible change. Carried to thoenissen/Reihitsu#1498 (follow-up 4).

## Architecture findings

**Two components own the line break that ends a braced switch section's last line.**

- Components: `SwitchCaseBraceRewriter.AddBraces` (with `BlockWrapTriviaUtilities.GetCarriedTrailingTrivia(..., endsLine: true)` and `closeBraceTrailing`) on one side; `BlankLineBreakSpacingRewriter.EnsureBlankLineBeforeSection`, `LineBreakPhase` brace placement, and the new FMT-BREAK-033 on the other.
- Shared decision: how many line breaks stand between a braced section's last token and the next token.
- Evidence: the cells under *Breaking inputs*. One fails on `main` (blank line before the switch's `}` in pass 1, removed in pass 2). One is newly exposed by FMT-BLANK-004's second line break. One depends on where FMT-BREAK-033's line break lands.
- The structural cause is already filed as thoenissen/Reihitsu#1518 ("inserted switch-section braces write no layout of their own") and thoenissen/Reihitsu#1498 (phase order).
- The run must not regress the newly exposed cell. Whether that takes a narrow fix or the structural one is the lead's decision. A narrow fix inside `AddBraces` that inspects the next token's leading trivia would add another copy of the layout decision.

## Follow-up candidates

1. **FMT-PIPE-004 · a braced switch section on the line of the switch's `}` gets a blank line in front of `}` in the first run only.** Pre-existing on `main`, reached by whole-document formatting. Same class as the architecture finding. It probably belongs as an input added to thoenissen/Reihitsu#1518 rather than a new issue.

   Input:
   ```csharp
   switch (x) { case 1: A(); B(); return 1; case 2: return 2; }
   ```
   Real output after run 1 (`main`), with run 2 removing the blank line:
   ```csharp
           switch (x)
           {
               case 1:
                   {
                       A();

                       return 1;
                   }
               case 2:
                   {
                       return 2;
                   }

           }
   ```
2. The spec author's follow-ups 1 to 3 (`case 1:;` spacing; RH5103 behind another section; new-rule candidate for label lines). I agree with all three.
3. **thoenissen/Reihitsu#1533** (delimited documentation comment behind a switch-label colon): link it to output question 3, whose answer it may make moot.
4. **thoenissen/Reihitsu#1498:** extend its Done-when "The blank-line phase inserts no line break that ends a statement's line" to switch sections. FMT-BLANK-004's new sentence (the blank-line phase writes the line break in front of a section on the `break`'s line) is meant to be reverted there.
