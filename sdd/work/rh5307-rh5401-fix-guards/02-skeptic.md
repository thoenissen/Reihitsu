Role: skeptic
Commit: 9042bdb
Verdict: proceed (RH5307: the class is wider than the issue and the change doc name; two small output questions on RH5401)

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

Both reports are real, and a code fix reaches each case: the RH5307 and RH5401 code fixes (lightbulb and Fix All) on code that compiles. All fixtures below were run with `scripts/apply-fix.sh <ID> <dir>` (LF and CRLF give the same status in every row) and with `scripts/format.sh <copy-of-dir>` for the whole-document comparison. The fixtures were written to the session scratchpad and are listed in full under *Breaking inputs*.

- **RH5307.** The diagnostic is on the whole element access (RH5307-003, `RH5307…Analyzer.cs`, `OnBracketedArgumentList`). The fix's `TryGetFormatTarget` (`RH5307…CodeFixProvider.cs`) starts from `root.FindToken(diagnostic.Location.SourceSpan.Start).Parent`, which is the receiver's first token. It takes the nearest enclosing `BracketedArgumentListSyntax` of that token, or if there is none, the `ArgumentList` of its nearest enclosing `ElementAccessExpressionSyntax`. It then checks that list instead of the reported one. Observed: `x[m[1,⏎2]]` and `j[0 /* c */][1,⏎2]` give `no-fix-offered`, as the issue says.
- **The issue's source is wrong about one instance.** thoenissen/Reihitsu#1217 Q3 says that with a plain receiver (`j[0][1,⏎2]`) "the fix is offered and joins". Observed: `no-progress (1 iteration)`. The fix formats `[0]`, which changes nothing, so the diagnostic can never clear. The spec author found the same. An action that does nothing is a defect in its own right, not a withheld fix.
- **RH5401.** `CanCollapse` formats the detached declaration (`ReihitsuFormatter.FormatNode`) and requires `formatted.ToString()` to contain no `\n`. `ToString()` leaves out only the node's outer trivia, so the attribute lists count. FMT-BREAK-027 gives every attribute list a line of its own, so every attributed property is refused. Observed: `[Obsolete]⏎int E⏎    => 1;` gives `no-fix-offered`, while `scripts/format.sh` gives `[Obsolete]⏎int E => 1;`, which is not reported.
- **The `?[ ]` clarification matches today's code.** The analyzer accepts only a parent of type `ElementAccessExpressionSyntax` or `ImplicitElementAccessSyntax`. `c?[1,⏎2]` gives `no-diagnostic`.

## Mechanism and candidates

**RH5307 mechanism.** The fix resolves its target from the diagnostic's *first token*: the innermost bracketed argument list around that token, otherwise the argument list of the innermost element access around it. It fails whenever that token lies inside another bracketed argument list or inside another element access (or element binding) than the reported one. Dispatch: `TryGetFormatTarget` runs `AncestorsAndSelf()` over `BracketedArgumentListSyntax`, falling back to `ElementAccessExpressionSyntax.ArgumentList`. `BracketedArgumentListSyntax` has three parents in this context: `ElementAccessExpressionSyntax`, `ImplicitElementAccessSyntax`, and `ElementBindingExpressionSyntax`. A fourth parent, `VariableDeclaratorSyntax`, belongs to fixed buffers and can hold no element access in compiling code, so it is dropped as unreachable.

**RH5401 mechanism.** The withhold check measures the line count of the formatted declaration *including its attribute lists*. Any attribute list that the formatter places on a line of its own refuses the fix. Dispatch: `CanCollapse` over `PropertyDeclarationSyntax`. Its only variable is where the formatter puts the attribute lists (FMT-BREAK-027, with an unspecified case for a comment between the brackets).

## Breaking inputs

Failure classes that reach the changed code (the catalog has no matrix for them, so the axes come from the dispatch above):

- FC-009: an action that cannot change the document is offered (RH5307 `b`, `i`, `j`, `n`).
- FC-008: Fix All on nested spans (RH5307 `n`).
- FC-027: nesting and enclosing scope (RH5307 axis 1).
- FC-001: the span a guard inspects is not the span the decision is about. Both defects are this class.
- FC-021 and FC-020: a comment or directive in the gap between the attribute lists and the declaration (RH5401 `d`, `e`, `g`, `l`).
- FC-028: the RH5401 guard reads `FormatNode`, while the fix applies `FormatNodeInDocumentAsync`.
- FC-030: the fix must not format the enclosing list or the collection expression (RH5307 `p`, `l`).
- FC-013: no conflict. `scripts/format.sh` joins every reported list below, so the fix's output and the formatter's agree.

### RH5307 — class matrix

Axis 1: where the receiver's first token sits. Axis 2: what the wrong list holds (on one line / comment / multi-line argument / element binding). Status today → status the changed RH5307-004 requires. Every row is reported today, except `f`.

| # | Input (`⏎` = line break, continuation aligned under the first argument) | Wrong list resolved today | Today | Required | Whole-document format |
|---|---|---|---|---|---|
| a | `var a = x[m[1,⏎2]];` (inside an element access list) | `x`'s list (multi-line arg) | no-fix-offered | fix: `x[m[1, 2]]` | `x[m[1, 2]]` |
| h | `var h = c?[m[1,⏎2]];` (inside an element binding list) | `c?[…]` (not an indexer list) | no-fix-offered | fix: `c?[m[1, 2]]` | `c?[m[1, 2]]` |
| m | `new Dictionary<int, int> { [m[1,⏎2]] = 1, }` (inside an implicit element access) | `[m…]` (multi-line arg) | no-fix-offered | fix: `[m[1, 2]] = 1` | joined |
| p | `var p = x[/* c */ m[1,⏎2]];` | `x`'s list (comment) | no-fix-offered | fix: `x[/* c */ m[1, 2]]` (the space behind the comment stays: SHARED-FIX-006) | `x[/* c */m[1, 2]]` (document formatting also removes the space, which the fix must not do) |
| b | `var b = j[0][1,⏎2];` (receiver is an element access, its list on one line) | `[0]` | **no-progress** | fix: `j[0][1, 2]` | same |
| i | `var i = a[0].P[0][1,⏎2];` (element access first in a member-access chain) | `a`'s `[0]` | **no-progress** | fix: `a[0].P[0][1, 2]` | same |
| j | `var j = this[0][1,⏎2];` | `this`'s `[0]` | **no-progress** | fix: `this[0][1, 2]` | same |
| c | `var c = j[0 /* c */][1,⏎2];` | `[0 /* c */]` (comment) | no-fix-offered | fix: `j[0 /* c */][1, 2]` | same |
| d | `var d = j[N(1,⏎2)][1,⏎2];` | `[N(…)]` (multi-line arg) | no-fix-offered | fix: `j[N(1,⏎2)][1, 2]` | same |
| n | `var n = j[0,⏎1][1,⏎2];` (both lists reported) | both fixes target `[0,⏎1]` | **no-progress (2 iterations)**: the inner list is joined, the outer never is | each fix joins its own list; Fix All gives `j[0, 1][1, 2]` | `j[0, 1][1, 2]` |
| e | `var e = c?[0][1,⏎2];` (receiver is an element binding; the reported access *is* checked) | `?[0]` (binding, not an indexer list) | no-fix-offered | fix: `c?[0][1, 2]` | `c?[0][1, 2]` |
| g | `var g = c?.P[1,⏎2];` (member-binding receiver) | — | fixed | stays fixed | same |
| k | `N(m[1,⏎2]);` (whole argument of an invocation) | — | fixed | stays fixed | same |
| l | `int[] l = [m[1,⏎2]];` (collection expression element) | — | fixed | stays fixed (`[m[1, 2]]`; document formatting also breaks the collection expression, which the fix must not do) | broken over 3 lines |
| o | `var o = (j[0])[1,⏎2];` (parenthesized receiver) | — | fixed | stays fixed | same |
| f | `var f = c?[1,⏎2];` | — | no-diagnostic | stays not reported (RH5307-001) | left wrapped (see *Follow-up candidates*) |

All types used: `x` is `int[]`, `m` is `int[,]`, `j` is `int[][,]` (in `n`: `int[,][,]`), `c` is `int[][,]` / `int[,]` / a class with `public int[,] P`, `a` is `D[]` with `public int[][,] P`, `this` is an indexer returning `int[,]`.

**Members the test implementer must cover**, beyond the change doc's `a`–`d`: `e`, `h`, `m`, `i` or `j` (one is enough, since they share a cell: element-access receiver whose own list is on one line), and `n`, both as one fix and as the Fix All scenario. The rows that hold today (`g`, `k`, `l`, `o`) must stay fixed. They also cover the inputs where the reported element access has the same span as its parent node (an `ArgumentSyntax` in `( )` for `k`, in `[ ]` for `a`, `h`, `m`, and a collection-expression element for `l`): resolving "the list the diagnostic is located on" must survive a parent that has the same span. Two existing tests assert today's withheld fix and must change: `VerifyFixIsNotOfferedForElementAccessInsideAnotherArgumentList` (`a`) and `VerifyFixIsNotOfferedWhenReceiverStartsWithElementAccess` (`c`).

### RH5401 — class matrix

Axis 1: where the attribute lists sit. Axis 2: what the attribute region holds. Axis 3: whether the formatter keeps the property's own code on one line. All rows are reported today.

| # | Input (the property is inside `class C`, `using System;` above) | Today | Required | Whole-document format |
|---|---|---|---|---|
| a | `[Obsolete]⏎int E⏎    => 1;` | no-fix-offered | fix | `[Obsolete]⏎int E => 1;` |
| b | `[Obsolete] int E⏎    => 1;` (attribute on the property's line) | no-fix-offered | fix (spec author's Q3, A) | `[Obsolete]⏎int E => 1;` |
| j | `[Obsolete, CLSCompliant(false)] public int E⏎    => 1;` | no-fix-offered | fix | `[Obsolete]⏎[CLSCompliant(false)]⏎public int E => 1;` |
| c | `[Obsolete("a",⏎          false)]⏎int E⏎    => 1;` (attribute arguments over several lines) | no-fix-offered | fix | attribute list kept over 2 lines, `int E => 1;` |
| d | `[Obsolete] // why⏎int E⏎    => 1;` | no-fix-offered | fix | `[Obsolete] // why⏎int E => 1;` |
| e | `[Obsolete] /* c */ int E⏎    => 1;` | no-fix-offered | fix | `[Obsolete] /* c */⏎int E => 1;` |
| f | `[Obsolete /* c */] int E⏎    => 1;` (comment between the brackets) | **fixed today**: `[Obsolete /* c */] int E => 1;` | stays fixed, attribute stays on the line (FMT-BREAK-027 leaves this placement unspecified) | same |
| g | `[Obsolete]⏎#if DEBUG⏎[CLSCompliant(false)]⏎#endif⏎int E⏎    => 1;` | no-fix-offered | fix | directives kept, `int E => 1;` |
| i | `/// <summary>Doc</summary>⏎[Obsolete]⏎int E⏎    => 1;` | no-fix-offered | fix; the doc comment stays one line (SHARED-FIX-006) | doc comment spread over 3 lines |
| k | `int F; [Obsolete] int E⏎    => 1;` (shares the line with a member) | no-fix-offered | fix, member separated (SHARED-FIX-007) | `int F;⏎[Obsolete]⏎int E => 1;` |
| l | `[Obsolete]⏎// note⏎int E⏎    => 1;` (comment between the attribute lists and the property's own code) | no-fix-offered | fix (see *Output questions* 2) | `[Obsolete]⏎⏎// note⏎int E => 1;` (blank line inserted by FMT-BLANK-011) |
| h | `[Obsolete]⏎int E =>⏎    1⏎    + 2;` | no-fix-offered | withheld (own code stays on 2 lines) | `int E => 1⏎         + 2;` |
| n | `[Obsolete]⏎int E⏎    => X switch { … }` (switch over several lines) | no-fix-offered | withheld | switch kept over several lines |
| m | `[Obsolete]⏎string E⏎    => @"a⏎b";` | no-fix-offered | withheld | literal kept |
| — | `int E⏎    => 1; // c` (no attributes, trailing comment) | fixed | stays fixed: a trailing comment behind `;` must not count as a line of the property's own code | — |

Further candidates, not run: an attributed property whose attribute list holds a shape the formatter refuses, such as a comment between an attribute argument and a comma on a later line (FMT-UNSUP-009). It must stay withheld by SHARED-FIX-010, although the property's own code fits on one line. RH5401-004 names only FMT-UNSUP-012/-013, and SHARED-FIX-010 in `Uses` carries the rest. Also two attributed properties in one document, for the Fix All scenario (BatchFixer). The existing `VerifyDiagnosticCoversAttributeListsAndModifiers` runs only the analyzer and stays unchanged.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5307 analyzer | must stay: what it reports, and where, is already what RH5307-001/-003 say (`?[ ]` not reported; `e` and `h` reported) |
| RH5307 code fix | must change: target resolution |
| RH5307 Fix All (`DocumentOrderFixAllProvider`) | must stay, but is newly exercised by `n` (nested diagnostic spans) |
| RH5401 analyzer | must stay |
| RH5401 code fix | must change: the withhold measure |
| RH5401 Fix All (`BatchFixer`) | must stay |
| Formatter (FMT-BREAK-014, FMT-BREAK-027, FMT-BLANK-011) | not involved; the fixes delegate to it unchanged |
| `Reihitsu.Core` | not involved |
| Rule pages RH5307, RH5401 | change (spec author's diff); see *Output questions* 1 for RH5401 |
| Tests | the two RH5307 characterization tests named above change; new tests per the matrices |

## Refuse option

- **Refuse (keep withholding) for the RH5307 class.** Today's state, with one difference: `b`, `i`, `j`, and `n` are `no-progress` today and would have to be withheld by a new check, so even refusing changes code. Cost of the requested change: replace the first-token walk with the node at the diagnostic's location. That is a few lines, no new type, and smaller than today's two-step lookup. Cost of refusing: a guard that recognizes "the receiver's first token is in another list or access", which is as much code as the fix itself, and it withholds the fix for code people write (`x[m[i, j]]`, `a[0].P[i, j]`, `this[0][i, j]`). Not a rare class. Refusing costs more and gives less.
- **Refuse for the RH5401 class.** Today's state. Attributed expression-bodied properties are common code (`[JsonIgnore]`, `[Obsolete]`), so refusing them is not covered by *Special cases do not grow the source*. The requested change narrows an existing measure, with no new type and a few lines.
- **Declare a shape unsupported.** No option here: neither defect is a shape the formatter cannot handle. Every input above is formatted correctly by `scripts/format.sh`, and the defects are a code fix reading the wrong node or measuring too much. Nothing would be deleted.
- **A narrower option** for RH5401 `l` (a comment between the attribute lists and the own code) only: see *Output questions* 2.

## Output questions

The spec author's Q3 (`[Obsolete] int E⏎    => 1;`, attribute on the property's line): I agree with A. The property's own code fits on one line, and moving the attribute is the formatter's layout of the reported span (RH5401-002). Nothing to add.

**1 · RH5401 · Expression style get-only properties should be single lined — rule page sentence**

Input (a property whose attribute keeps the property's line):
```csharp
[Obsolete /* c */] int E
    => 1;
```

Before (`main`): the fix gives `[Obsolete /* c */] int E => 1;`. The page says "The code fix is offered when the whole declaration fits on one line."

After, A (spec author's text): "The code fix is offered when the property fits on one line; attributes keep lines of their own above it." This is not true for the input above, whose attribute stays on the property's line.

After, B (recommended): "The code fix is offered when the property, apart from its attributes, fits on one line."

Why B: it states the condition the fix checks without describing a layout that has an exception, and rule pages carry no corner cases.

**2 · RH5401 · Expression style get-only properties should be single lined — comment between the attributes and the property**

Input:
```csharp
[Obsolete]
// note
int E
    => 1;
```

Before (`main`): no fix offered.

After, A (recommended):
```csharp
[Obsolete]

// note
int E => 1;
```
(whole-document formatting gives exactly this; the blank line comes from FMT-BLANK-011, since `]` is not an opening token. The test implementer confirms the fix's own output)

After, B: no fix offered; the diagnostic stays.

Why A: it is the formatter's specified layout of the reported declaration, and B costs an extra guard for a rare shape. The change doc does not list this output yet; the lead may decide that #1219 Q1 covers it and add it to the change doc's *Visible output changes* instead of asking.

## Internal choices

- **RH5401-003 wording.** "each attribute list of the property on a line of its own above it (FMT-BREAK-027)" and "an attribute list that shares the line … moves onto a line of its own" are false for `f`, whose comment-in-brackets placement FMT-BREAK-027 leaves unspecified (output unchanged from today: the attribute stays). Recommend "the attribute lists as FMT-BREAK-027 places them" and no absolute statement. Spec text only; `f`'s output does not change.
- **RH5307-001/-004 examples.** The new MUST NOT for `c?[…]` can be misread as excluding `c?[0][1,⏎2]` (`e`), which is reported and must get the fix. Recommend adding `e` (or `h`) to the RH5307-004 example, so the test implementer has it from the spec.
- **Change doc, RH5307 section.** It lists `a`–`d` only. Recommend naming the class ("wherever the receiver's first token lies in another bracketed argument list, element access, or element binding") and the `no-progress` members `b`, `i`, `j`, `n` under *before*. Their after-output is already decided by #1217 Q3's principle, so no question is needed.

## Issue goals

The issue is `agent-filed`.

- "`var a = x[m[1,⏎2]];` and `j[0 /* c */][1,⏎2]` → the fix is offered": keep it, and widen it to the whole class above. Two instances leave `b`, `e`, `h`, `i`, `j`, `m`, `n` (and the `no-progress` defect) for the next audit. The visible output is the same principle the maintainer decided (#1217 Q3).
- "`c?[1,⏎2]` stays not covered; state it in the spec and the rule page": keep it. Today's code already behaves this way.
- RH5401 instance: keep it.
- "Lane: bug fix and spec clarification": already changed to behavior change (decision 1). Nothing visible.
- "Requirements: … FMT-BREAK-014": the run need not touch FMT-BREAK-014. Its `?[ ]` gap is a formatter question outside the maintainer's RH5307 decision (follow-up below). Nothing visible in this run.
- #1217 Q3's statement that `j[0][1,⏎2]` "is offered and joins" is stale or wrong: today it is `no-progress`. The PR should say so.

## Architecture findings

**RH5307: the analyzer and its code fix own the same decision, which argument list the diagnostic is about.** The analyzer decides it and encodes it as the location (the element access, or for an implicit access the list). The fix derives it again by a different rule (first-token ancestor walk) and disagrees for every row of the RH5307 matrix marked no-fix-offered or no-progress. It also re-states the analyzer's reporting condition in a private copy (`IsIndexerArgumentList`, `CanSafelyCollapseToSingleLine`, duplicated in `RH5307…Analyzer.cs` and `RH5307…CodeFixProvider.cs`). Once the fix reads the node the diagnostic is located on, it no longer re-derives the decision, and the copied condition only re-checks what the analyzer already decided for that node. The structural fix and the narrow fix are the same few lines, within the run's components. The lead decides whether the copied guard stays.

RH5401: none. The fix reads the formatter's result (the safe shape, question 10), and only the span it measures changes. The detached `FormatNode` that the guard reads and the in-document formatting that the fix applies use the same line-break phase for attribute lists. Neither predicts a column.

## Follow-up candidates

- **FMT-BREAK-014 and `?[ ]`** (from the spec author, confirmed): `scripts/format.sh` leaves `var f = c?[1,⏎                   2];` wrapped, although FMT-BREAK-014 literally covers every wrapped `BracketedArgumentListSyntax`. It is reachable through whole-document formatting and the CLI. It carries the output question the spec author wrote (A: join it, B: exclude element-binding lists from FMT-BREAK-014). I agree with A.
- None other.
