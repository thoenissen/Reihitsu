Role: skeptic
Commit: d29dd6a
Verdict: output question

The prompt carried no intended fix or hoped-for verdict. The issue's "Proposed Behavior" and "Done when" are proposals of an `agent-filed` issue and were checked as such (*Issue goals*).

All measurements: `scripts/apply-fix.sh <RH####> <dir>` on the tree at `d29dd6a`, whose code equals `main` at `21c2ff4` (the spec commit changes no code). Every fixture was run under LF and CRLF, and both arms gave the same result. Columns are 0-based character columns. Fixtures were in the scratchpad and are deleted.

## Premise

The premise is real and reachable. `FormatNodeInDocumentWithContextAsync` has exactly three callers, all code fixes (`rg` over `src/` without the test projects):

- `RH5111AssignmentsMustHaveProperLineBreaksCodeFixProvider.GetFormattingContextNode`. It passes a `VariableDeclaratorSyntax` with `Parent.Parent` as context (`LocalDeclarationStatementSyntax`, `FieldDeclarationSyntax`, `EventFieldDeclarationSyntax`, and also a `ForStatementSyntax`/`UsingStatementSyntax`/`FixedStatementSyntax` for a declarator in their declaration). It passes a `ParameterSyntax` with its `ParameterListSyntax`, and an `EnumMemberDeclarationSyntax` with its `EnumDeclarationSyntax`.
- `RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedCodeFixProvider.FormatAsync`. It passes a local function with the label that starts its line, but only when the function does not start its own line.
- `FluentChainCodeFixHelper.FormatChainAsync` (RH5112, RH5201). It passes the outermost chain node with the statement or member that starts the chain's line.

I reproduced every "before" output of the change doc exactly: changes 1–6 (c1, c2, c3, c4, c5 through RH5111, c6 through RH3208).

The severity claim ("a result that the alignment rules report") holds only in part. I ran today's fix outputs through other rules:
- Change 1's output (`2` off `1`) is reported by **RH5203**.
- Change 2's output (braces at column 12) is reported by **RH5301**.
- Change 4's output is reported by RH5109, but the input already broke RH5109 (`int a` alone on its line, `int b, string c` sharing one), so the fix did not introduce that report.
- Change 6's output (RH3208) and the misaligned binary operator `+` of changes 3–5 are reported by no rule (RH5203, RH5301, RH5109, RH5204 checked). There the defect is visible misalignment that the next formatting pass moves, and nothing more. The change is still justified, but the change doc's *Why* overstates it for these cases.

## Mechanism and candidates

Mechanism: the phases lay out the target's other lines from the place their layout gives the target's first token, while the entry point writes the target back behind the document's unchanged text in front of it. Every target whose first token the context's layout moves, to another line or another column, gets its anchored lines off their anchors.

The dispatch is the switch in `GetFormattingContextNode` and `GetFormattingNode` (RH5111), `GetLineStartingLabel` (RH3208), and `FormatChainAsync`'s ancestor search. A target's first token can be moved by:
- (a) the line-start column of a later line,
- (b) a line join in front of it,
- (c) a line break in front of it,
- (d) horizontal spacing in front of it on its line,
- (e) a move of the line it stands on.

Measured members (today's output, all reached by the RH5111 or RH3208 fix):

| # | Target / shape | Way | Today | Listed in change doc? |
|---|---|---|---|---|
| c1, c2, c3 | declarator / enum member / parameter on a later line | (a) | misaligned | yes (1–3) |
| p2 | **first** declarator on the line after its type (`int` / `a =` / `Other(1,` / `2)`) | (a) | `2` col 18, `1` col 22 | no |
| j3 | declarator in a `for` initializer, context the whole `for` statement | (a) | `2` col 18, `1` col 27 | no |
| f1 | **field** declarator on a later line (`private static int _a = 1,` / `_b =` …) | (a) | `2` col 15, `1` col 34 | no |
| t1 | declarator on a later line in a tab-indented file | (a) | `2` col 12, `1` char col 16 | no |
| j1, j2 | **first parameter on the line after `(`**: `void M(` / `int a` / `= 1 +` / `2)` | **(b) join** | `+` always col 19 (the joined place), whatever the column of `int a` | no, and FMT-PIPE-017 names no join |
| c4 | parameter behind `int b,` | (c) | misaligned | yes (4) |
| p3 | enum member behind `{`: `enum E { A =` / `1 +` / `2 }` | (c) | `+` col 8, `1` col 13 | no |
| c6 | local function behind `L:` and several spaces | (d) | misaligned | yes (6) |
| s1 | first declarator behind its type with extra spaces: `int    a =` | (d) | `2` col 22, `1` col 25 | no |
| s3 | first parameter behind `(` with extra spaces: `M(   int a` | (d) | `+` col 19, `1` col 22 | no, and the change doc's *Unchanged* line names "the first parameter behind `(`" as unchanged |
| s2, s5, s6 | parameter / declarator / enum member behind a sibling with extra spaces (`int a,    string c`, `int a = 1,    b =`, `A = 1,    B =`) | (d) | misaligned | no |
| c5 | declarator behind a raw-string closing line | (e) | misaligned | yes (5) |
| s7 | parameter behind a raw-string closing line | (c), which masks (e) | `+` col 22, `"y"` col 28 | no |

Members that already hold today and must not change:
- p1: first parameter on the line after `(` at column 11, where the joined place happens to equal the document column.
- p4, f2: declarator behind a sibling with one space.
- p5: enum member behind a sibling with one space.
- j4, j5: a parameter on a later line that the layout leaves at the same column.
- p6: a one-line value.
- p7: `L:int`, where the zero-width gap stays and the result is aligned.
- s4: declarator behind the last line of a multi-line comment, already aligned.
- t2: parameter with a `//` comment line and a blank line above it.
- The fluent-chain fixes: reasoned only. `HasSameChainLayoutAsync` falls back to the context result whenever the chain's first-token column differs. The change keeps the chain-only result's first token at its document column, so the fallback decision is unchanged. The existing RH5112/RH5201 tests confirm it after implementation (FC-032).

## Breaking inputs

Failure classes that reach the change:

- **FC-016 / FC-027** — sibling shapes. The table above is the class matrix:
  - axes: the target kind (declarator in a local, field, `for`, or `using`; parameter; enum member; local function), the way it moves (a)–(e), and the value's anchor (wrapped argument, binary operator, object creation braces, lambda block, collection initializer braces);
  - every member marked "misaligned" or with a column pair above must get its anchored lines under their anchors;
  - every member that holds must stay byte-identical.
- **FC-004 / FC-019 / FC-020 / FC-021** — trivia. For a parameter and an enum member, the fix checks only the construct's interior (`InteriorContainsCommentOrDirective`). A comment, a documentation comment, an `#if`/`#endif`, or a blank line in the target's leading trivia therefore still gets the fix, and the documentation, line-break, and blank-line phases must keep that trivia. Cover:
  - a target on a later line with a `//` line above it and a wrapped value;
  - a `///` written behind the sibling in front of it (FMT-PIPE-018's existing example, now with a wrapped value);
  - a directive line above an enum member with a wrapped value.
- **FC-022** — raw strings, for (e): c5, and s7 for a parameter.
- **FC-010 / FC-017** — LF and CRLF for every case. FMT-PIPE-018's write-back restores the document's line endings.
- Tabs: t1, in a tab-indented file, with a wrapped value. The existing tab test has no wrapped value. After the change, `2` should stand at character column 16 under `1`, with tabs counted as one column (FMT-PIPE-017).
- **FC-007** — convergence. For changes 1 and 2, RH5203 and RH5301 must no longer report the fix's output, and RH5111 must not report it again.
- **FC-031** — the XML remarks and inline comments of `ReihitsuFormatter.FormatNodeInDocumentWithContextAsync` describe the gap.
- **FC-032** — every test of a fix that delegates to this entry point (RH5111, RH3208, RH5112, RH5201) must be re-run after the formatter change.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `ReihitsuFormatter.FormatNodeInDocumentWithContextAsync`, `FormattingContext` (target-position fact) | must change |
| `LineBreakPhase` (no break and no join in front of the target) | must change |
| `IndentationPhase` (the target's anchored lines laid out from the fact's column) | must change |
| `DocumentationCommentFormattingPhase`, `BlankLinePhase` (keep the target's leading trivia) | must change, or prove already kept |
| `HorizontalSpacingPhase` | must stay as the only owner of same-line gaps. See *Architecture findings* |
| `StructuralTransformPhase`, `RegionFormattingPhase`, other phases | not involved (no target move observed; field declarators are not split in f1/f2) |
| RH5111 code fix, RH3208 code fix | must stay (no code change); their outputs change |
| `FluentChainCodeFixHelper` (RH5112, RH5201) | must stay; output must not change |
| `FormatNodeInDocumentAsync`, `FormatNode`, whole-document formatting, CLI | not involved; must stay. No FC-013 risk, since document output does not change |
| Analyzers | not involved |
| Rule pages (`documentation/rules/RH5111.md`, `RH3208.md`) | not involved (the normal case does not change) |
| Tests | RH5111 and formatter `ReihitsuFormatterNodeColumnTests` expectations change (see *Internal choices*) |

## Simpler option

_None._ The obvious alternative was to have the fixes call `FormatNodeInDocumentAsync` on the target, which already keeps its first token's column and lays out from it. I rejected it because it decides the target's leading trivia by FMT-PIPE-010 instead of keeping it (FMT-PIPE-018, SHARED-FIX-006). That changes blank lines and comment placement above a target that starts its line, which is a different set of visible changes, not a smaller one. The spec author's internal choices (keep the write-back; depth-placed lines do not follow) are sound. The write-back is needed for the tab case in `DeclaratorTargetOnALaterLineOfATabIndentedContextKeepsTheWhitespaceInFrontOfIt`.

## Output questions

The change doc's *Visible output changes* list is incomplete. The members below change visibly under the amended FMT-PIPE-017, are reached by the RH5111 fix, and are neither listed nor asked. Its *Unchanged* line is also wrong for s3: "the first parameter behind `(`" changes when spaces stand between `(` and the parameter. Each member has only one output consistent with FMT-PIPE-010, because what stands in front of the target is outside it. They belong in the change doc and in the batch the lead asks before the test implementer, as the listed six do. Recommendation for all of them: A.

**1 · RH5111 · Assignments must have proper line breaks — first parameter on the line after `(`, wrapped value**

```csharp
    void M(
        int a
            = 1 +
              2)
```
Before (`main`):
```csharp
    void M(
        int a = 1
                   + 2)
```
After, A (recommended):
```csharp
    void M(
        int a = 1
                + 2)
```
Why A: the parameter stays on its own line where the user put it, and `+` stays under the value.

**2 · RH5111 — enum member behind `{` on its line, wrapped value**

```csharp
enum E { A =
           1 +
           2 }
```
Before (`main`):
```csharp
enum E { A = 1
        + 2 }
```
After, A (recommended):
```csharp
enum E { A = 1
             + 2 }
```
Why A: same as 1.

**3 · RH5111 — construct behind extra spaces on its line (first declarator behind its type, first parameter behind `(`, parameter, declarator, or enum member behind a sibling)**

```csharp
        int    a =
            Other(1,
                  2);
```
Before (`main`):
```csharp
        int    a = Other(1,
                      2);
```
After, A (recommended):
```csharp
        int    a = Other(1,
                         2);
```
Why A: the wrapped argument stays under the first one. The same applies to `M(   int a`, `int a,    string c`, `int a = 1,    b =`, and `A = 1,    B =`.

**4 · RH5111 — declarator on a later line of a field, a `for` initializer, or the first declarator on the line after its type**

```csharp
    private static int _a = 1,
                       _b =
                           Other(1,
                                 2);
```
Before (`main`):
```csharp
    private static int _a = 1,
                       _b = Other(1,
               2);
```
After, A (recommended):
```csharp
    private static int _a = 1,
                       _b = Other(1,
                                  2);
```
Why A: these are the same case as change 1. They are listed because the field case is the most visible one (19 columns off today).

**5 · RH5111 — declarator on a later line of a tab-indented declaration (shown with `→` for a tab)**

```csharp
→→int a = 1,
→→    b =
→→        Other(1,
→→              2);
```
Before (`main`): `→→    b = Other(1,` / 12 spaces `2);`
After, A (recommended): `→→    b = Other(1,` / 16 spaces `2);`. `2` stands under `1` in character columns, with each tab counted as one, as FMT-PIPE-017 counts it.
Why A: it follows the existing tab rule. Formatting the document later replaces the tabs and moves both lines together.

## Internal choices

1. **The spec states a mechanism that cannot hold for mid-line targets.** FMT-PIPE-018's new paragraph says the indentation phase "gives that token its column in the document (FMT-PIPE-017), which changes at most the whitespace directly in front of it". FMT-PIPE-024's new bullet says it "places the first token of the target … in that fact's column". Both fail for two of the ways the spec claims to cover:
   - Way (e), c5: the indentation phase moves the raw string's closing line 7 columns right within the root. `b` then stands 7 columns right of its document column, with one space in front of it, so no whitespace change can place it there.
   - Way (d), c6/s1–s6: `HorizontalSpacingPhase` shrinks the gap in front of the target. Putting the token back by widening that gap would make the indentation phase write a same-line gap, which `AGENTS.md`'s ownership table gives to `HorizontalSpacingPhase` alone.

   The visible result the spec demands (the change doc's "after" outputs) is reachable either way. Only the stated mechanism is wrong.

   Recommendation: the spec author restates both sentences as a result. The target's lines that align to an anchor on its first line are laid out as if its first token stood in the fact's column, and the root itself may keep that token where the layout puts it, since what stands in front of it is not written back. Leave the mechanism to the implementer. The alternative is to have `HorizontalSpacingPhase` keep the gap in front of the target and add it to FMT-PIPE-024's bullets. That still fails way (e), so I recommend against it.
2. **FMT-PIPE-017's list of ways is incomplete.**
   - Add the join, way (b). The first bullet says "gives that line another column", but j1/j2 move the line. Something like: "or joins it to the line in front of it: the RH5111 fix of a first parameter on the line after `(`".
   - Name the declarator and the enum member under the second bullet (s1, s5, s6, p3).
   - These are spec wording, routed with the change doc amendment.
3. **Two existing formatter tests use targets that no caller passes**: `ArgumentTargetOnALaterLineOfItsContextKeepsItsFirstLineColumnAndLeavesTheAlignedLineInThePhaseLayout` (an argument) and `InvocationTargetInsideAMovingRawStringLiteralTakesThePhasesColumnOnItsSecondLine` (an invocation). Their expected values change with a generic implementation, and the outputs are not visible.
   - Recommendation: remove them, and cover FMT-PIPE-017's three-plus-one ways with the code-fix-shaped spec examples instead.
   - Alternative: update their expectations. It costs nothing visible, but it pins outputs no caller needs (`AGENTS.md`, *Supported inputs*).

## Issue goals

- **"FMT-PIPE-017's two recorded-gap examples produce aligned output under tests" — change.** The second example (argument `Wrap(…)`) is unreachable, so drop it. Replace both with the RH5111/RH3208 inputs, as the spec author did. No visible output is affected.
- **"Desired output" for the `Wrap(…)` raw-string example — drop.** No caller passes that target.
- **"No longer replaces the target's leading trivia after the last phase" — drop.** The write-back is what keeps the document's tabs (FMT-PIPE-018, the tab test). The spec author's version keeps the write-back but makes it change no column. That matches today's visible output for the tab test. Nothing new is in question.
- **"The line-break phase does not break in front of the target" — widen to "nor joins".** j1/j2 show a reachable join. The spec diff already says "neither … nor joining" in FMT-PIPE-024. That join change is output question 1.
- **"FMT-PIPE-018's examples keep their output" — keep.** It is binding as stated, and the spec diff keeps it.

## Architecture findings

None in the code as of `d29dd6a`. The spec diff as written would create one: the indentation phase writing the same-line gap in front of a mid-line target, which `HorizontalSpacingPhase` owns (*Internal choices* 1). Stated as a result instead of a mechanism, it disappears.

## Follow-up candidates

_None._ I found no find outside the run's issue that a caller reaches.
