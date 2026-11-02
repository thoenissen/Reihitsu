Role: skeptic
Commit: 8c45342
Verdict: proceed narrower — the spec diff needs one amendment (FMT-BLANK-021 breaks FMT-PIPE-004 in switch sections) and the change doc one more listed visible change (code-fix results)

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The maintainer answer in `00-lead.md` (remove FMT-BREAK-005's move) overrides the issue's example output; I checked the spec diff of 1a072d5 against it.

Method: besides reading the specs and the deciding code, I ran an unpublished scratch copy of 8c45342, outside the repository, in which only `FormattingPipeline.CreatePhases` swaps the order to LineBreak → SwitchCaseBrace → BlankLine. Nothing else was changed there: FMT-BREAK-005's move, FMT-SWITCH-015/016, and FMT-BLANK-021 are not implemented in it. It answers one question — what the reorder alone changes — and it is not a source of expected values (FC-035). The copy is deleted.

## Premise

- **The issue is real for the CLI and whole-document formatting.** Every shape below reaches `FormattingPipeline` through `scripts/format.sh` (the CLI). The node-level entry points run the same phase list, so code fixes that call `FormatNodeInDocumentAsync` are reached too (see *Surfaces*).
- **The reorder by itself is output-neutral on everything tested.** In the scratch copy, the formatter tests ran 4274, 1 failed: `FormattingPipelineTests.ExecuteObserverReportsAllPhasesInOrderWithoutChangingOutput`, which asserts the old order. The analyzer tests (code fixes included) ran 6397, 0 failed. `scripts/format.sh` gave byte-identical output to `main` for a set of switch/comment/blank-line fixtures (sections on one line, `/* */` across lines in front of a section and a statement, blank-line runs around leading and trailing comments in a section, `#if` in a section, `if … else … ;` pairs, `try … finally` pairs), all stable after pass 2 — except the class under *Breaking inputs*.
- **The issue's three defects, checked:**
  - Two-run blank line above the moved `if` comment: reproduced by the spec author; the maintainer's answer removes the move, which alone removes this defect. The reorder is not needed for it.
  - The blank-line phase splits same-line statements: true on `main` (`BlankLineEditor.InsertBlankLineBeforeToken`, via `EnsureBlankLineBeforeStatement`, `EnsureBlankLineBeforeSection`, `EnsureBlankLineAfterClosingBrace`). The output is correct on `main`; this is an ownership defect, not a visible one.
  - The switch-brace phase's layout copy: true (`SwitchCaseBraceRewriter.AddBraces`, `BlockWrapTriviaUtilities.GetCarriedLeadingTrivia(..., keepBlankLinesAboveFirstComment: false/true)`). Internal; no visible defect found.
  So the reorder has no visible gain of its own; its value is structural. The maintainer's answer keeps it in this run, so I do not propose dropping it (see *Simpler option*).
- **A further replay that the reorder makes obsolete**, not named by the issue: the blank-line phase calls the line-break phase's predicate `ChainLineBreakRewriter.BreaksLineBehindMultiLineCommentTail` from `BraceBlankLinePolicy.FindTokenLineStartIndex` (`src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BraceBlankLinePolicy.cs:131`) to predict where the later chain layout breaks a line. See *Architecture findings*.

## Mechanism and candidates

Not a bug fix; the mechanism of the one defect the spec diff introduces:

**After the reorder, the switch-brace phase measures whether a section spans several lines (FMT-SWITCH-003) before the blank-line phase writes the line break of FMT-BLANK-021, so a section whose multi-line measure depends on that line break is left unbraced in the first run and braced in the second.**

Dispatch: `SwitchCaseBraceRewriter.IsMultiLineSection` (`src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs:68`). Its line-dependent branches:

| Branch | Can FMT-BLANK-021's line break change it? |
|---|---|
| `LabelContainsMultiLinePattern` | no — no statement pair in a label |
| non-terminal statements > 1 | no — a count, not a line measure |
| one non-terminal statement, `SpansMultipleLines` | no — an empty-statement pair is two statements; a pair nested in a lambda or block body is multi-line already through brace placement (tried: `return Run(() => { ; return 1; });`, `{ ; return 2; }` — identical to `main`) |
| one non-terminal statement, `HasMultiLineBodyEndingInReturnOrThrow` (first statement's line vs. the `return`/`throw` line) | **yes** — when the one non-terminal statement is `;` and the `return`/`throw` stands on its line |
| `ContainsMultiLineSwitchExpression` | no |

No other phase between the line-break phase and the blank-line phase measures lines.

## Breaking inputs

Failure classes reaching the change: FC-010 (a phase on its own output is not a no-op — here the pipeline), FC-012 (neighbouring phases), FC-015 (a rule with two owners — see *Architecture findings*), FC-017 (one pass asserted for idempotency), FC-019/FC-021 (comments in the gaps the switch wrap and the line-break phase touch), FC-026 (`else if` chains for the retired move), FC-028 (node-level vs. document), FC-031 (stale comments describing the old order), FC-032 (code-fix results flip because the fixes delegate to the formatter), FC-035 (expected values must come from the spec, not from a patched copy).

**Defect — FMT-BLANK-021 line break after the switch-brace measure.** Class matrix: axes are (a) the one non-terminal statement: `;` / other; (b) the statement behind it on the same line: `return` / `throw` / `break` / `continue`/`goto` / another statement; (c) another multi-line section in the same switch: yes / no.

| (a) | (b) | (c) | Result with the reorder |
|---|---|---|---|
| `;` | `return 1;` | no | **fails** — run 1 unbraced, run 2 braces (traced, `scripts/trace.sh`: pass 2 `SwitchCaseBracePhase`) |
| `;` | `throw …;` | no | **fails** — same (traced); also with the statement on the label's line, `case 1: ; throw null;` |
| `;` | `return`/`throw` | yes | holds — the switch is braced through the other section |
| `;` | terminal `break` | either | holds — the terminal `break` is exempt (SHARED-BLANK-004), nothing is split |
| `;` | `continue;` / `goto case` | either | holds — two non-terminal statements |
| other | any | either | holds — FMT-BREAK-006 splits in the line-break phase, before the measure |

Failing input (CLI, whole document):

```csharp
class C
{
    int M(int x)
    {
        switch (x)
        {
            case 1:
                ; return 1;
            default:
                return 0;
        }
    }
}
```

`main` (one run, stable):

```csharp
        switch (x)
        {
            case 1:
                {
                    ;

                    return 1;
                }
            default:
                {
                    return 0;
                }
        }
```

Reorder with FMT-BLANK-021 as specified, run 1 (run 2 then inserts the braces above):

```csharp
        switch (x)
        {
            case 1:
                ;

                return 1;
            default:
                return 0;
        }
```

This contradicts FMT-PIPE-004 and FMT-PIPE-025's own first sentence (a phase decides from a line break a later phase adds), and the change doc's claim that the reorder changes no other output. It is a safety-rail class (idempotency) if shipped as specified.

**Candidate inputs the test implementer must cover** (beyond the spec author's list):

- The two failing rows above, asserted over two runs (FC-017), plus the `(c) = yes` and terminal-`break` rows as holds.
- FMT-SWITCH-016's carried blank lines in front of a first statement led by `/// d`, by `/** e */` on its own line, and by `/* c */ A();` on one line — `main` removes each blank line behind a plain block's `{` (checked), so the wrap may carry them; and a run of two blank lines in front of the terminal `break`'s leading comment (the spec's example).
- Every FMT-BREAK-036 head with both comment shapes (`// c` + brace on the next line, `/* c */ { … }` on one line), `else if` included; an unbraced `if` body that FMT-STRUCT braces (change doc output change 4).
- Code-fix results of RH5405/RH5406/RH5407 for `if (a) // c` + unbraced body (see *Surfaces*), and the existing `else if (y) // note` tests in `src/Reihitsu.Analyzer.Test/Formatting/RH5405BracesMustNotBeOmittedAnalyzerTests.cs` unchanged.
- Chain links behind a multi-line comment's closing line (FMT-BLANK-018 with `BreaksLineBehindMultiLineCommentTail`), if the replay is removed.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `FormattingPipeline.CreatePhases` | must change (order) |
| `LineBreakContainedBlockRewriter` (move, `IsElseIfBranch`), `RootConstruct.ElseIf`, `ReihitsuFormatterHelpers` | must change — `IsElseIfBranch` has no reader but the move (checked: only `LineBreakContainedBlockRewriter.cs:278` and `ReihitsuFormatterHelpers.cs:95`) |
| `SwitchCaseBraceRewriter` | must change (FMT-SWITCH-015/016); outputs must stay |
| `BlankLineEditor` / blank-line rewriters | must change only per the outcome of *Internal choices* 1; outputs must stay |
| `BraceBlankLinePolicy` | should change: the chain replay and the remark at `RemovesBlankLinesBehindOwnLineComment` ("the line-break phase runs later") become wrong (FC-031); see *Architecture findings* |
| Code fixes RH5405/RH5406/RH5407 (`StatementBracesCodeFixProviderBase` → `FormatNodeInDocumentAsync`) | **visible result changes**, not listed in the change doc: on `main`, `scripts/apply-fix.sh RH5405` turns `if (a) // c` + `Foo();` into `// c` above `if (a)` with braces (also in a loop body); after FMT-BREAK-005's retirement the comment stays behind `if (a)`. Other fixes that format a node containing such an `if` follow too. |
| Analyzers | not involved (no reporting condition changes; 6397 analyzer tests pass on the reorder alone) |
| `Reihitsu.Core` | not involved |
| Rule pages | not involved — no page shows a condition comment (rule pages never mention the formatter) |
| CLI | not involved beyond formatter output |
| `AGENTS.md` order sentence and ownership table | must change (done in 1a072d5) |

## Simpler option

Retiring FMT-BREAK-005 alone (maintainer answer 1) removes the only visible defect of the issue; the reorder adds no visible gain and is what introduces the switch defect above. The maintainer's answer keeps the reorder in this run, so this option is recorded, not recommended.

## Output questions

_None_, provided the lead takes *Internal choices* 1 A, which keeps `main`'s output for the failing class. If the lead rejects A and the remaining options change what the failing input gives, that becomes an output question (input above; `main` braces the section in one run; option B of *Internal choices* 1 leaves it unbraced).

The RH5405/RH5406/RH5407 code-fix change is the same behavior as maintainer answer 1 (the comment behind an `if` condition stays where the author wrote it) on another surface. I read it as covered by that answer; the change doc must still list it as a visible output change (`sdd/README.md`, *Change docs*), and the lead confirms the coverage under *Decisions taken*. Example for the list, `scripts/apply-fix.sh RH5405`:

```csharp
// input
        Foo();

        if (a) // c
            Foo();
```

```csharp
// before (main)
        Foo();

        // c
        if (a)
        {
            Foo();
        }
```

```csharp
// after
        Foo();

        if (a) // c
        {
            Foo();
        }
```

## Internal choices

1. **Who ends the line of a statement behind an empty statement, when a blank-line rule separates them** (FMT-BLANK-021 vs. FMT-BREAK-006).
   - **A (recommended)** — the line-break phase starts that statement on its own line: FMT-BREAK-006's empty-statement exemption stops where a blank-line requirement (FMT-BLANK-001 to FMT-BLANK-004, SHARED-BLANK-003) asks for a blank line between the pair, decided through one statement-kind predicate both phases read, not a copy of it. Today that predicate is split: `BlankLineStatementSpacingRewriter.NeedsBlankLineForStatementKind` is private to the blank-line phase, and only the closing-brace part is shared (`BlankLineSpacingPolicy.RequiresBlankLineAfterClosingBrace`, `Reihitsu.Core`), so A needs the private part moved next to it — a production path beyond the handovers' list, inside components the run changes. The blank-line phase then writes no line break that ends a line, FMT-BLANK-021 is not needed, the issue's Done-when holds unnarrowed (decision 2 can be reverted), and the switch measure sees the split, so `main`'s output holds in one run for every row above. FMT-PIPE-025's wording must then say that it forbids reading a later phase's line breaks or blank lines in the tree, not a statement-kind predicate both phases share; the change doc's rejected alternative "Let the line-break phase split an empty statement from its neighbour where a blank line is required" is then the choice. Checked: the node-level case (`; if (a) Foo();` through RH5405's fix) keeps `; if` on one line on `main`, because neither phase touches the gap in front of the root, so A changes nothing there.
   - **B** — keep FMT-BLANK-021 and leave empty statements out of `HasMultiLineBodyEndingInReturnOrThrow`: stable, but the failing input stays unbraced — a visible change.
   - **C** — keep FMT-BLANK-021 and let the switch-brace phase predict its line break: forbidden by FMT-PIPE-025.
2. **FMT-PIPE-025's "line break between two tokens"** is wider than meant: (i) the blank-line phase removes and adds line breaks (blank lines are line breaks), and (ii) the cleanup phase, after the blank-line phase, removes the line breaks between a file's last token and the end-of-file token (FMT-CLEAN-005, FMT-CLEAN-007) and at a node-level root's end (FMT-CLEAN-008). Recommend wording that states the decided fact — whether two tokens with text stand on one line — and names the blank lines separately; no output changes.
3. **FMT-SWITCH-016's "removes none"** goes beyond the issue (which asked for "writes no blank lines"). Output is unchanged because FMT-BLANK-007/010/014 remove what the wrap no longer drops (checked on `main` for plain blocks with `/// d`, `/** e */`, and `/* c */ A();` behind a blank line). Recommend keeping it: one owner for every blank line.
4. **FMT-SWITCH-015 names a mechanism** ("by the line-break phase's placement … not a placement of its own"). The observable part is the line positions, which match `main`; the mechanism clause is an internal constraint in a behavior spec. Recommend keeping it, as it is what the issue's ownership goal is about, and noting that a test can only check the lines.

## Issue goals

thoenissen/Reihitsu#1498 is `agent-filed`.

- "Desired Formatted Output" (comment above the `if`, with a blank line): superseded by maintainer answer 1; the Done-when "stable in one run" holds for the kept comment. No further change.
- "The blank-line phase inserts no line break that ends a statement's line": narrowed by decision 2 to keep FMT-BLANK-021; that narrowing is what breaks idempotency above. With *Internal choices* 1 A the goal holds as written, with no visible change.
- "`SwitchCaseBraceRewriter` … places braces through `BracePlacer`": a prescribed mechanism; keep as internal (FMT-SWITCH-015). "the switch-case brace tests keep their outputs": holds on the reorder alone (all formatter tests pass apart from the order test).
- "FMT-PIPE-001 also forbids … every phase after the line-break phase to move a comment onto its own line": taken as FMT-PIPE-026, which also forbids the reverse; no phase after the line-break phase does either today (checked: the switch wrap keeps every comment on its line per FMT-SWITCH-007, and the spacing and indentation phases write no end-of-line trivia).
- The issue comment (FMT-BLANK-004 sentence reverted; `switch (x) { case 1: break; case 2: break; }` stable in one run): holds on the reorder alone (traced: `LineBreakPhase` then `BlankLinePhase`, stable after pass 2).

## Architecture findings

- **The blank-line phase replays a line-break decision.** `BraceBlankLinePolicy.FindTokenLineStartIndex` (`src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BraceBlankLinePolicy.cs:131`) calls `ChainLineBreakRewriter.BreaksLineBehindMultiLineCommentTail` to classify comments in front of a chain link as on their own line before the line-break phase has moved the link; `ChainLineBreakRewriter.GetWrappedLayoutLineBreaks`' remark states the dependency ("the blank-line phase, which runs earlier … reads the same answer"). Components: blank-line phase and line-break phase; shared decision: whether a chain link starts its line behind a multi-line comment's closing line. After the reorder the line-break phase has already moved the link, so `IsBehindMultiLineCommentTail` is false for it and the call decides nothing in a document run; it remains a second statement of the line-break rule. It is in components this run changes, and FMT-PIPE-025's Why names exactly this kind of replay. Recommend removing it in this run, with the FMT-BLANK-018 chain tests as the guard.
- **FMT-BLANK-021 as specified is the shape FMT-PIPE-025 forbids** (*Breaking inputs*); *Internal choices* 1 A resolves it without a copy.

## Follow-up candidates

- `case 1: ;` keeps the empty statement glued to the label (`case 1:;`, also `case 1: ; break;` → `case 1:; break;`) on `main`, unchanged by this run. Whether an empty statement behind a label should start its line is outside thoenissen/Reihitsu#1498; a follow-up only if the lead considers the output wrong. Recommend none: FMT-BREAK-006 deliberately leaves empty statements as written.
- Code comments that state the old order (FC-031), listed by the spec author, plus `BraceBlankLinePolicy.RemovesBlankLinesBehindOwnLineComment`'s remark and `ChainLineBreakRewriter.GetWrappedLayoutLineBreaks`' remark — in scope for the implementer, not a follow-up.
