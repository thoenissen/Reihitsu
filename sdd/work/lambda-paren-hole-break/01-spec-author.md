Role: spec author
Commit: e094895
Verdict: change doc written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the issue itself.

## Issue statements checked against e094895

All observations below come from `scripts/trace.sh` and `scripts/format.sh` on scratch inputs, and from reading the named code.

- `LineBreakListRewriter.CollapseOpenParenToDeclarationLine`, called from `VisitParameterList`, still runs for a parenthesized lambda's parameter list — **true**. On the issue's first input, pass 2 of the trace shows the line-break phase moving `(sum, item) => sum + item` to column 0 and the indentation phase writing column 36 back — **confirmed**.
- **New:** the strip also removes the blank lines in front of the lambda (`Aggregate(0,⏎⏎(sum, item) => …)` loses its blank line in the line-break phase; `Aggregate(0,⏎⏎5)` keeps it), against FMT-BREAK-030. In a raw-string hole, a lambda that starts a line no alignment places (a switch-expression arm value) ends in column 0, against FMT-RAW-007. Both are visible output changes 5 and 6 of the change doc.
- `ListElementInteriorJoinRewriter.IsOwnedGap` states the opposite policy (a lambda's opening gap has no other owner) — **true**.
- `LayoutModel.ReleaseUnplacedRawStringHoleLines` exists and runs from `LayoutComputer` — **true**. The issue's second input gives column 13 for the broken anonymous method's lines — **confirmed**; in an already aligned literal the same lines end in column 0.
- **Changed scope:** the issue says only the line-break phase breaks inside the hole. On `main`, the blank-line phase writes line breaks there too (`delegate { int x = 1; return x; }` is split by `BlankLinePhase` before `LineBreakPhase` runs). The spec therefore binds every phase (FMT-PIPE-025), not only the line-break phase.
- `RawStringInterpolationHoleTests.AnonymousMethodBrokenInsideHoleIsStableOnTheSecondPass` and `BlockLambdaBrokenInsideHoleOfAlignedLiteralKeepsItsAlignment` exist — **true**.
- "Phase-level idempotency exceptions": `PhaseReversalExceptionList` holds five entries, all with cause `PhaseReversalCauses.LineStartIndentation`. Which of them this change removes is not established here. `PhaseStabilityCheckTests.CheckCatchesLineBreakPhaseStrippingIndentationOfParenthesizedLambdaArgument` uses this reversal as its example.
- The blocker thoenissen/Reihitsu#1488 is closed. No issue statement is moot.

## Changed requirements

- FMT-BREAK-011 — Parameter lists hug their declaration — **changed**: the opening-parenthesis clause no longer covers parenthesized lambdas, the comma and closing-parenthesis clauses still do, and the owner of the gap in front of a lambda is named. A new example covers both sides.
- FMT-BREAK-002 — Braces start their own line — **changed**: names FMT-PIPE-025 as its one exception.
- FMT-PIPE-025 — An anonymous method in a raw-string hole keeps its lines — **added** (`sdd/specs/formatter/pipeline.md`, ledger updated).
- Non-goals pointing to FMT-PIPE-025 were added to `line-breaks.md` and `blank-lines.md`. `Uses` was updated in `pipeline.md` and `line-breaks.md`.
- Change doc: `sdd/changes/2026-11/lambda-paren-hole-break.md` (Draft).
- No rule page changes, because this is formatter-only behavior.
- Spec architecture tests: everything passes except `EveryRequirementOfAnEnforcedSpecIsCovered`, which fails only because FMT-PIPE-025 has no test yet.

## Examples for the tests

- FMT-PIPE-025: every example. That means one line in a moved literal and in an aligned one (the second with two statements, which tests the blank-line phase), an anonymous method in a lambda's expression body, a block lambda inside an anonymous method, an anonymous method inside a block lambda's body (broken and placed), an author-written multi-line anonymous method (unchanged), and a single-line raw string literal (broken as today). The change doc's entries 1–4 are the before/after cases.
- FMT-BREAK-011: the new second example — the anonymous method's `(` joins `delegate`, the lambda's comma and `)` are collapsed, and the lambda argument with a blank line above it is unchanged. Change-doc entry 6 (lambda starting a hole line in a switch arm) belongs to FMT-BREAK-011 and FMT-RAW-007.
- Change-doc entry 5 (blank line kept) is FMT-BREAK-030 and FMT-BREAK-011.

## Output questions

Each question uses the input, Before, and After of the change doc's `Visible output changes` entry it names. The "before" outputs were taken from `main` at e094895.

1. **Formatter · anonymous method in an interpolation hole of a multi-line raw string** (entries 1–4).
   - Before: the anonymous method is broken onto lines that stay in column 13 (moved literal) or column 0 (aligned literal).
   - A (recommended): the anonymous method stays on the line the author wrote, everything inside it included.
   - B: it is broken and aligned under `delegate`, like a block lambda under its anchor. This needs a new alignment rule, and outside holes an anonymous method takes the scope-depth column instead.
   - Why A: nothing places those lines, and keeping the author's line gives the same output wherever the literal stood.
   - Touches FMT-PIPE-025, FMT-BREAK-002, FMT-RAW-007.
2. **Formatter · blank line in front of a parenthesized lambda argument** (entry 5).
   - Before: the blank line is removed.
   - A (recommended): the blank line is kept, as for any other argument.
   - Why A: FMT-BREAK-030 already demands it. Listed because the output changes.
3. **Formatter · lambda starting a hole line that no alignment places** (entry 6).
   - Before: the lambda is moved to column 0.
   - A (recommended): the lambda keeps its column.
   - Why A: FMT-RAW-007 already demands it.

## Internal choices

- The hole rule is one pipeline requirement (FMT-PIPE-025), because both the blank-line and the line-break phases insert line breaks there. Alternatives were a FMT-BREAK requirement plus a FMT-BLANK copy (one rule in two places), or extending FMT-RAW-007 (indentation-phase owners).
- "No alignment places the lines" is decided by construct: an anonymous method that no block lambda of the same, innermost hole contains. The alternative was deciding from the indentation phase's layout, which would be a phase prediction. The constructs probed inside holes were initializers, anonymous objects, switch expressions, nested conditionals, chains, operators, lists, block lambdas (including `static` and `async`), and query expressions. Only anonymous methods produced lines that nothing places.
- FMT-BREAK-011 keeps its comma and closing-parenthesis clauses for lambdas (RH5106 and RH5107 report them).
- FMT-PIPE-025 leaves two cases unspecified, through its coverage sentence: a node-level target that lies inside such an anonymous method (a detached root cannot see the hole, FMT-PIPE-023), and holes of non-raw interpolated strings.
- The change doc records that `AnonymousMethodBrokenInsideHoleIsStableOnTheSecondPass` changes its expectation to the one-line hole.

## Follow-up candidates

1. **Formatter · anonymous method in a hole of a regular interpolated string under C# 10.**
   - Input, parsed as C# 10 and formatted with `ReihitsuFormatter.FormatSyntaxTree`, observed through a scratch file-based app:
     ```csharp
     var b1 = $"J {M(delegate { return 1; })}";
     ```
   - Output:
     ```csharp
     var b1 = $"J {M(delegate
     {
         return 1;
     })}";
     ```
   - The output fails to compile under C# 10 with `CS8967: Newlines inside a non-verbatim interpolated string are not supported in C# 10.0`. That makes it a safety-rail defect (code that does not compile), and it is outside this issue, which covers raw strings only.
   - Reachability claim to verify: a code fix, or `FormatDocumentAsync`, on a C# 10 project formats a member that contains such a string (FMT-PIPE-008 reads the project's language version). The CLI path was not checked.
   - Readings:
     - A: below C# 11, insert no line break into a hole of a non-verbatim interpolated string; the output is the input.
     - B: leave as is.
   - Recommendation: A.
2. **Formatter · a parenthesized lambda behind a modifier on the next line, as an assignment value.**
   - Input:
     ```csharp
     Func<int, int> f = static
         (int x) => x;
     ```
   - Output on `main` (unchanged by this run):
     ```csharp
     Func<int, int> f = static
     (int x) => x;
     ```
   - The `(` lands in the statement's column, like a new statement.
   - Readings:
     - A: join to `static (int x) => x`.
     - B: indent the continuation one level.
     - C: keep.
   - Recommendation: A, because the wrap structures nothing, as FMT-BREAK-015 already judges for the same gap inside a list element.
