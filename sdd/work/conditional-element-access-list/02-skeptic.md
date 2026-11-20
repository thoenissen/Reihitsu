Role: skeptic
Commit: 196829de
Verdict: #1877: output question (proceed on A once answered); #1878: output question (proceed on A once answered)

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Both issues are `agent-filed`; their goals are treated as proposals. The two output questions the spec author raised in `01-spec-author.md` stand as written; I add none and agree with both recommendations (reasons below).

Fixtures were run from `/tmp/claude-0/.../scratchpad/fx{1,2,3}` (deleted afterwards) on `196829de`, whose production code equals `main`.

## thoenissen/Reihitsu#1877

### Premise

Real. Whole-document formatting and the CLI reach it, and so does every code fix that formats a node holding the list (the spec author observed RH5101, RH5111, RH5302, RH5308). Reproduced independently with `scripts/format.sh <fx1> --dry-run`. Input (excerpt of `fx1/A.cs`, `c` is `int[,]?`, `o?.Q` is a `D?` with a two-argument indexer, `f` is `Func<int, int>[,]?`, `e` is `int[,][,]?`):

```csharp
        var a1 = c?[1,
                    2];
        N(c?[1,
             2] ?? 0, 3);
        var a2 = o?.Q?[1,
                       2];
        var a3 = o?.Q[1,
                      2];
        var a5 = this.Get()?[1,
                             2];
        var a6 = f?[1,
                    2](3);
        var a7 = new D { [c?[1,
                             2] ?? 0, 1] = 3 };
        Func<int?> a8 = () => c?[1,
                                 2];
        var a9 = $"{c?[1,
                       2]}";
        var b1 = c?[1,
                    2 +
                    3];
        var b2 = c!?[1,
                     2];
        var b3 = e?[0,
                    1][1,
                       2];
```

Output on `main` (excerpt): every `?[` list stays wrapped — `a1`, `a2`, `a5`, `a6`, `a8`, `a9`, `b2`, the binding `[0,⏎1]` of `b3`; `N(...)` becomes `N(c?[1,⏎     2] ?? 0,⏎  3);` (the outer argument list is split because its first argument stays multi-line); `a7` becomes the expanded initializer with `[c?[1,⏎    2] ?? 0, 1] = 3` still wrapped. Element accesses in the same file are joined: `a3` → `o?.Q[1, 2]`, and the element access of `b3` → `e?[0,⏎1][1, 2]`.

The same contexts with element accesses (`fx2/B.cs`: `N(c[1,⏎2] + 0, 3)`, `f[1,⏎2](3)`, `new D { [c[1,⏎2] + 0, 1] = 3 }`, `() => c[1,⏎2]`, `$"{c[1,⏎2]}"`) all join in one pass on `main`: `N(c[1, 2] + 0, 3);`, `f[1, 2](3)`, the expanded initializer holding `[c[1, 2] + 0, 1] = 3`, `() => c[1, 2]`, `$"{c[1, 2]}"`. These are the outputs the element-binding forms must reach under FMT-BREAK-014 as changed.

Not a regression: the exclusion is as old as the predicate; no history evidence needed.

### Mechanism and candidates

Mechanism: the formatter decides whether a wrapped bracketed argument list is joined by the kind of the list's parent, and its predicate `LineBreakListRewriter.IsElementAccessArgumentList` (`src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakListRewriter.cs:68`) admits `ElementAccessExpressionSyntax` and `ImplicitElementAccessSyntax` but not `ElementBindingExpressionSyntax`, so every bracketed argument list whose parent is an element binding is never joined, in every context. The same predicate feeds `RootConstruct.ElementAccessArguments` through `ReihitsuFormatterHelpers.CaptureRootConstruct` (`src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs:128`), the fact a node-level entry point passes when the formatting root is the list itself.

Dispatch: `LineBreakListRewriter.VisitBracketedArgumentList` visits every `BracketedArgumentListSyntax`. Its possible parents, from the `SyntaxFactory` methods that take one (Roslyn 5.12.0 XML docs): `ElementAccessExpressionSyntax`, `ElementBindingExpressionSyntax`, `ImplicitElementAccessSyntax`, `VariableDeclaratorSyntax` (fixed-size buffer; an array-style declarator `int a[3];` is a syntax error the formatter skips). So the candidate set is exactly these four; the change concerns the second, the fourth stays outside (see *Internal choices*).

Rewriter order (`LineBreakPhase.CreateRewriters`): the list rewriter runs after the assignment, attached-part, and list-element-interior rewriters and before the binary, chain, and ternary rewriters, the same position for element accesses and element bindings. The predicate is the only point where the two differ in the line-break phase; indentation (`ArgumentAlignmentContributor:122`) handles every bracketed list alike.

### Breaking inputs

Failure classes that reach the change: FC-015/FC-016 (the eligibility rule has two private copies, see *Architecture findings*), FC-019/FC-020/FC-021 (comments and directives in the gaps the join touches), FC-027 (enclosing contexts), FC-028 (node-level root = the list), FC-010/FC-017 (CRLF and second-pass idempotency), FC-012 (FMT-BREAK-040's join of `?⏎[` next to this join), FC-032 (a formatter change flips code-fix results and existing tests), FC-038 (no phase may produce an FMT-UNSUP-014 shape — the join only writes whitespace between `[` and `]`, so no member fails on reasoning, but a test with a comment in front of `?` is cheap).

Matrix — axes: parent kind of the list {element access, element binding, implicit element access} × what stands in front of the brackets {plain receiver, `!`, `?`, `!?`, `?.Q?`} × enclosing context {declaration value, invocation argument, implicit-element-access argument in an initializer, lambda body, interpolation hole, receiver of an element access, receiver of an invocation, followed by `.Member`} × gap content {whitespace only, `//` comment between brackets, `/* */` before `]`, directive between brackets, multi-line argument}. On `main`, every cell with parent = element binding fails to join (all whitespace-only cells above); every cell with another parent joins. Cells that must keep the list as written for every parent: comment or directive between the brackets (FMT-BREAK-001), multi-line argument (`b1`: on `main` the binary operator moves (`2⏎+ 3`) and the list stays wrapped, as for element accesses).

Candidates the test implementer must cover (whole document, LF and CRLF, two passes):

- `c?[1,⏎2]`, `c?[⏎1, 2]`, `c?[1, 2⏎]` (closing bracket alone), `c!?[1,⏎2]`, `o?.Q?[1,⏎2]`, `this.Get()?[1,⏎2]`, `f?[1,⏎2](3)`, `c?[1,⏎2].ToString()`.
- `e?[0,⏎1][1,⏎2]` (binding as receiver of an element access: both lists join) and `e?[0,⏎1]?[1,⏎2]` (two bindings).
- Contexts: `N(c?[1,⏎2] ?? 0, 3)` must end as `N(c?[1, 2] ?? 0, 3);` in one pass, like `N(c[1,⏎2] + 0, 3)`; `new D { [c?[1,⏎2] ?? 0, 1] = 3 }` must end like the element-access analog (expanded initializer, `[c?[1, 2] ?? 0, 1] = 3`) in one pass; lambda body; interpolation hole.
- `c?⏎[1,⏎2]` and `c⏎?[1,⏎2]`: FMT-BREAK-040 joins the `?[` gap and FMT-BREAK-014 the list, giving `c?[1, 2]`.
- Kept: `c?[1, // note⏎2]`, `c?[1,⏎/* c */ 2]`, `c?[1,⏎#if X⏎2⏎#else⏎3⏎#endif⏎]` (directive between brackets), `c?[1,⏎2 +⏎3]`.
- A comment in front of the `?` (`c /* c */ ?[1,⏎2]`): an FMT-UNSUP-014 shape, so the whole document is returned unchanged and reported; covered by the existing precheck, one test is enough.
- Node level: the RH5307 fix formats the list itself as the root (`FormatNodeInDocumentAsync(document, bracketedArgumentList)`), so the join depends on `RootConstruct.ElementAccessArguments` being captured for an element binding's list. A fix test on `c?[1,⏎2]` covers it; it would show as `no-progress`/not-converged otherwise.
- Code fixes that format a node holding the list (FC-032): at least the RH5111 example of the change doc; the spec author observed RH5101, RH5302, RH5308 as well.
- Fixed-size buffer `fixed int b[⏎3];` stays as on `main` (characterization; see *Internal choices*).

Existing tests that lock today's output and flip with A (FC-032):

- `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ListElementAttributeLayoutSinglePassTests.cs:417` `ConditionalElementAccessKeepsWrappedArgumentsWhenNestedIndexerCollapses` asserts `a?[b[1,⏎2],⏎3]` → `a?[b[1, 2],⏎           3]`. Under FMT-BREAK-014 as changed, the outer list is joined too: `a?[b[1, 2], 3]` (its written element is multi-line, but `CanSafelyCollapseInteriorList` blocks only an element that is still multi-line after its own lists were rewritten, `LineBreakListRewriter.cs:331`). Its single-pass purpose stays; its expectation and name change. This is visible output covered by output question 1.
- `src/Reihitsu.Analyzer.Test/Formatting/RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzerTests.cs:1253` `VerifyNullConditionalElementAccessIsNotReported` (see #1878).
- No other test, and no non-`.cs` fixture, holds a `?[` whose `]` is on a later line (`rg --pcre2 '\?\[[^\]\n]*$' src`); `n?[{|RH5307:m[1,⏎2]|}]` (lines 738, 1194) keeps its expectation, because the outer binding's only argument spans lines.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter, `LineBreakPhase` (`LineBreakListRewriter` eligibility) | Must change |
| Formatter node-level entry points (`CaptureRootConstruct`) | Must change, through the same predicate; no new mechanism |
| Formatter, other phases (indentation, spacing, precheck) | Must stay |
| `Reihitsu.Core` | Not involved, unless the lead takes the architecture finding below |
| Analyzers other than RH5307 | Not involved (no layout rule reports a one-line list) |
| Code fixes formatting a node that holds the list (RH5101, RH5111, RH5302, RH5308, …) | Must stay in code; their results change through the formatter |
| Formatter spec (FMT-BREAK-014) | Changed by the spec author; FMT-BREAK-040 must stay |
| Tests | Must change (new cases; one existing expectation flips) |

### Refuse option

- Leave the construct as written (output question 1, option B): FMT-BREAK-014 gains an exclusion, zero production lines, against the requested change's one added parent kind in one predicate. The input class (a wrapped list behind `?`) is uncommon but not a safety class: the join is the same interior-only rewrite element accesses already get, so nothing makes it unsafe, and refusing contradicts the maintainer's direction on #1869. With B, #1878 can only stay "not reported" (SHARED-DIAG-010).
- Declaring a shape unsupported: not an option worth escalating. The only shape near the change is "a comment, directive, or disabled text between the brackets of a conditional element access" (from its `[` through its `]`, nothing excepted); declaring it would delete no special case, because the interior comment guard (`CanSafelyCollapseInteriorList`) serves element accesses, implicit element accesses, and every angle-bracket list too, and it would refuse `c?[1, // note⏎2]`, which people write. The gaps around the `?` are already FMT-UNSUP-014's.
- Simpler option: none; the requested change is already the smallest one.

### Output questions

Output question 1 of `01-spec-author.md` stands as written; I recommend A, for the reason given there and because the change is one parent kind in an existing predicate with no new guard. The maintainer should know that A also flips the existing expectation `a?[b[1, 2],⏎3]` → `a?[b[1, 2], 3]` (see *Breaking inputs*); it is a member of question 1, not a separate question. No other.

### Internal choices

- Fixed-size buffer (`fixed int b[⏎3];`, parent `VariableDeclaratorSyntax`): FMT-BREAK-014 says it does not decide it and the change doc keeps it out. Recommend keeping it that way and adding a characterization test that it stays as on `main`, so the change provably touches only the element binding (no visible output in question; rare, `unsafe`-only).
- FMT-BREAK-014 names the lists by parent kind and RH5307-001 restates the same three kinds. Recommend letting RH5307-001 refer to FMT-BREAK-014's definition ("a bracketed argument list that holds an index, FMT-BREAK-014") instead of restating it, so the rule is stated once in the specs (the RH5307 spec already lists FMT-BREAK-014 under `Uses`). Wording only.
- The spec's `c /* c */ ?{|RH5307:[…]|}` example states no fix result. SHARED-FIX-010 decides it from `FindUnsupportedSyntax` on the node the fix formats (the list), which does not hold the comment, so the fix is offered and gives `c /* c */ ?[1, 2]`, as for `d /* c */ [1,⏎2]` today. Determined, not in question; the test implementer asserts the observed result.

### Issue goals

- "With A, FMT-BREAK-014 stays as written": change — the spec author clarified FMT-BREAK-014 (parent kinds, gap division with FMT-BREAK-040). Spec wording only, no visible output.
- "Why A: … RH5307 stays silent on it, which P11 allows": drop — stale; #1878 in the same run makes RH5307 report it. No visible output.
- "No join across a comment, directive, or disabled text (FMT-BREAK-001)": keep; it is today's guard.

### Architecture findings

One rule stated in two places (FC-015): which bracketed argument lists hold an index is decided by `LineBreakListRewriter.IsElementAccessArgumentList` (formatter, `LineBreakListRewriter.cs:68`, also behind `CaptureRootConstruct`) and by `RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.IsIndexerArgumentList` (analyzer, `RH5307…Analyzer.cs:47`, also used by the code fix). Both carry the same omission, which is why the defect needed two issues. The input that shows it: `c?[1,⏎2]`, excluded identically by both. The analyzer cannot call the formatter, but both reference `Reihitsu.Core`. The narrow fix adds the same parent kind to both copies, keeping two statements; a shared predicate in an existing `Reihitsu.Core` utility class would be a few lines, no new type, within components the run changes. The lead decides. (The interior comment guards — `RH5307…Analyzer.CanSafelyCollapseToSingleLine` and `LineBreakListRewriter.CanSafelyCollapseInteriorList` — differ by design, with attribute-formatting handling only in the formatter, and are not part of this finding.)

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1878

### Premise

Real. Reproduced with `scripts/apply-fix.sh RH5307 <fx3>` on this input (`fx3/R.cs`):

```csharp
#nullable enable

class C
{
    void M(int[,]? c, int[,] d)
    {
        var a = c?[1,
                   2];
        var b = d![1,
                   2];
        var g = d[1,
                  2];
    }
}
```

Observed, LF and CRLF: `fixed (1 action(s), 2 iteration(s))`; `b` → `var b = d![1, 2];`, `g` → `var g = d[1, 2];`, `a` unchanged (not reported). The issue's `c[1,⏎2] (null-forgiving)` line lost its `!`; the observation holds for `d![1,⏎2]`. Caller: the analyzer and its code fix in the IDE.

### Mechanism and candidates

Mechanism: RH5307 reports a bracketed argument list only when `IsIndexerArgumentList` admits its parent (`ElementAccessExpressionSyntax` or `ImplicitElementAccessSyntax`), so every list whose parent is an `ElementBindingExpressionSyntax` is never reported, and the code fix, which asks the same predicate (`TryGetFormatTarget`), is never offered for it.

Dispatch: `RegisterSyntaxNodeAction(OnBracketedArgumentList, SyntaxKind.BracketedArgumentList)`; parents as listed under #1877. Location today: `Parent is ElementAccessExpressionSyntax` → the element access, else the list. An element binding's list therefore falls into the list branch without further change, which is location A of output question 2. The fix's `TryGetFormatTarget` resolves a span with `FindNode(span, getInnermostNodeForTie: true)`: for an element binding the binding and its list share one span, the innermost is the list, and the existing `BracketedArgumentListSyntax list when list.Span == span` arm takes it. Location B (from the receiver through `]`) is no syntax node when code follows the `]` (`c?[1,⏎2].ToString()`), so it would need a new mapping in the fix; that is a further reason to prefer A.

### Breaking inputs

Failure classes: FC-015 (shared with #1877), FC-007 (the fix silences its own diagnostic in one pass — depends on the formatter joining a root list of an element binding, see #1877 *Node level*), FC-008 (overlapping diagnostics with the same start: `e?[0,⏎1][1,⏎2]` reports the binding `[0,⏎1]` and the element access `[0,⏎1][1,⏎2]`, both starting at `[0`; Fix All through `DocumentOrderFixAllProvider`), FC-013 (parity with the formatter: report exactly what FMT-BREAK-014 joins), FC-019/FC-021 (comments around and inside), FC-030 (the fix changes only the list), FC-041/FC-043 (the fix asks `FindUnsupportedSyntax` about the list, which holds the `[`'s leading trivia but not the `?`).

Matrix — axes: what stands in front of the `[` {plain, `!`, `?`, `!?`, `?.Q?`} × what follows the `]` {nothing, `.Member`, `?? 0`, another `[…]`, `?[…]`, a trailing comment} × comment position {none, between the brackets, in front of `?`, between `?` and `[` on one line (`c? /* c */ [`), between `?` and `[` as an own line (`c?⏎// c⏎[`)} × argument shape {all one line, one multi-line}. On `main` every cell with `?` or `!?` is not reported; every other cell behaves as RH5307-001/-002 state.

Candidates for the test implementer:

- Reported and fixed: `c?[1,⏎2]`, `c?[⏎1, 2]`, `c!?[1,⏎2]`, `o?.Q?[1,⏎2]`, `c?[1,⏎2].ToString()`, `c?[1,⏎2] ?? 0`, `c?[1,⏎2]; // note`, and `d![1,⏎2]` (the `!` form the issue asks to state).
- Two diagnostics: `e?[0,⏎1][1,⏎2]` and `e?[0,⏎1]?[1,⏎2]`; each fix alone, then Fix All, giving the spec's joined output.
- Not reported: `c?[1, // note⏎2]`, a directive between the brackets, `c?[1,⏎2 +⏎3]`, `c?[1, 2]`, and the one-line binding `[0]` of `e?[0][1,⏎2]` (only the element access is reported, as today).
- Comments around the `?`: `c /* c */ ?[1,⏎2]`, `c? /* c */ [1,⏎2]`, `c?⏎// c⏎[1,⏎2]` — reported (RH5307-001 as changed); whether the fix is offered follows SHARED-FIX-010 from `FindUnsupportedSyntax(list)`. For the first two the list does not hold the comment; for the third the comment is in the leading trivia of the list's `[`. Settle each by running it; the element-access analog `d⏎// c⏎[1,⏎2]` is reported and fixed on `main` (spec author's observation), and FMT-UNSUP-003 makes a node-level input that lacks the `?` not hold the shape — expect the fix offered, with the leading trivia written back as in the document.
- RH5115 next to it: `c?⏎[1,⏎2]` gets RH5115 on the `?[` gap and RH5307 on the list. On `main` `scripts/apply-fix.sh RH5115` turns it into `c?[1,⏎             2]` (fixture `fx3/S.cs`, LF and CRLF, `fixed (1 action(s), 1 iteration(s))`), and RH5307 does not report it. After the change, both fixes in either order must converge to `c?[1, 2]`.
- The existing test `VerifyNullConditionalElementAccessIsNotReported` (`RH5307…AnalyzerTests.cs:1253`) asserts the removed exclusion; it is inverted or replaced.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5307 analyzer (eligibility) | Must change |
| RH5307 analyzer (location) | Must stay with A (the list branch already covers it); must change with B |
| RH5307 code fix (`TryGetFormatTarget`) | Must stay with A apart from the shared predicate; must change with B |
| RH5307 Fix All (`DocumentOrderFixAllProvider`) | Must stay; covered by a two-diagnostic test |
| Formatter | Involved through #1877; the fix's result depends on it |
| RH5115 analyzer and fix | Must stay |
| `documentation/rules/RH5307.md` | Changed by the spec author (exclusion sentence removed) |
| `Reihitsu.Core` | Not involved, unless the architecture finding is taken |

### Refuse option

- Keep the exclusion (output question 2, option C): zero production lines against one parent kind added to the predicate. With #1877 A, C leaves the formatter joining a shape the rule's own construct covers without reporting it — the rule's own gap under P11 — and keeps the rule page's exclusion sentence; it contradicts the maintainer's direction. Not a safety class, and not rare by kind (`?[` is common; the wrapped multi-argument form is uncommon).
- Withhold only the fix: no reason — the fix is the formatter's own join, scoped to the list.
- Declaring a shape unsupported: not an option (see #1877).
- Simpler option: none.

### Output questions

Output question 2 of `01-spec-author.md` stands as written; I recommend A, for the reason given there and because A is the location the analyzer and the fix already produce for a list whose parent is not an element access, so A needs no new location code, while B needs a new span mapping in the fix. No other.

### Internal choices

- The spec example `n` (`e?{|RH5307:{|RH5307:[0,⏎1]|}[1,⏎2]|}`) has two diagnostics with the same start; the existing `j[0,⏎1][1,⏎2]` case already exercises that tie, so no new Fix All mechanism is in question. Recommend one Fix All test on `n`.
- See #1877 for the wording choice in RH5307-001.

### Issue goals

- "The diagnostic for `c?[…]` … the span covers the whole conditional access expression the reader sees": change to location A (the bracketed list). Visible output — it is output question 2.
- "`c[1,⏎2]` keeps being reported and fixed; the spec and a test state it": keep, read as `d![1,⏎2]` (the `!` was lost in the issue text). The spec example has it; a test must too.
- "With this issue the analyzer reports the case, so the P11 argument in #1877 … no longer applies; … Neither closes the other": both issues are met by one change; each can be closed by the PR. Not visible.
- "Not a duplicate of #894": not checked further; no goal depends on it.

### Architecture findings

Shared with #1877 (the same finding, one statement): `IsIndexerArgumentList` and `IsElementAccessArgumentList` state one rule twice.

### Follow-up candidates

_None._
