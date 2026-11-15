Role: skeptic
Commit: 2e81a160e8afec7f430f218987ad0e7e05c716d3
Verdict: #1737: proceed narrower; #1738: output question

The prompt carried no intended fix or suspected cause. The issues' own "Required output" / "Proposed Behavior" sections and the spec author's recommendations are treated as proposals.

How facts were established: formatter outputs by `scripts/format.sh` on scratch fixtures, run a second time to confirm stability (both stable); fix outputs by `scripts/apply-fix.sh <ID>` (LF and CRLF identical); rule reports on candidate outputs by running `scripts/apply-fix.sh` for RH5301, RH5303, RH5304, RH5305, RH5306, RH5411, and RH6002 over fixtures that hold exactly the candidate output. Fixtures lived in the session scratchpad, outside the repository.

## thoenissen/Reihitsu#1737

### Premise

Real, and reachable: RH5107 reports the comma and offers its fix (the fix's guard only covers the gap between the previous parameter and the comma, `RH5107CommaMustBeOnSameLineAsPreviousParameterCodeFixProvider.RegisterCodeFixesCoreAsync`); the formatter reaches the same input through the CLI and whole-document formatting.

Observed (`apply-fix.sh RH5107`, every fixture `fixed`, 1 iteration; P/Q/R as in the change doc):

| Input (the comma's line) | RH5107 fix | Formatter |
|---|---|---|
| P `, /* c */ int b` | `int a,` ⏎ `/* c */ int b` | `int a, /* c */` ⏎ `int b` |
| Q `, // c` ⏎ `int b` | `int a,` ⏎ `// c` ⏎ `int b` | `int a, // c` ⏎ `int b` |
| R `, /* c */` ⏎ `int b` | `int a,` ⏎ `/* c */` ⏎ `int b` | `int a, /* c */` ⏎ `int b` |
| X `, /* c */ /* d */ int b` | comments stay in front of `int b` | both comments behind the comma |
| V two leading commas with comments (Fix All, 2 iterations) | both comments stay in front of their parameter | both behind their comma |
| W lambda `(int x⏎, /* c */ int y) => x` | as P | as P |
| T `,` alone, ⏎ `/* c */ int b` | `int a,` ⏎ `/* c */ int b` | identical — no disagreement |
| U `int b /* c */` ⏎ `, int c` | that comma is not reported (SHARED-JOIN-001) | comma held, stays |

Two statements of the spec-author handover and the Draft change doc do not hold:

1. **"Formatting the fix's result changes nothing (checked)" / "Each result is stable under the other component" is false for Q and R.** Formatting the fix's Q result gives `int a,` ⏎ *blank line* ⏎ `// c` ⏎ `int b` (FMT-BLANK-011 inserts the blank line above a comment on its own line); R likewise. So fix-then-format yields a third output, different from format-only. It holds for P.
2. **Today's fix violates SHARED-FIX-003 for Q and R.** On the comma's line the comment stands behind code at the end of its line — the comma — so it "stands with" the comma (SHARED-FIX-003, *Comments and directives in place*, first bullet). The fix moves the comma and leaves the comment behind, the second listed way of moving a comment away from its code. No exception applies (the comment is not what RH5107 reports). Per `AGENTS.md` (*Code-fix coverage*) that is a bug against the spec, not an open question. For P (and X, V, W) the comment stands between two tokens of one line, which SHARED-FIX-003 explicitly leaves undecided.

Consequence: option B of output question 1 ("the formatter follows the fix", fix unchanged) is not available for Q and R — it would ship a SHARED-FIX-003 violation. B also contradicts FMT-BREAK-012, whose `Why:` derives the argument-list placement from the parameter list ("A comment behind a comma stays behind it, as in a parameter list and in a constraint clause (FMT-BREAK-037)"); changing the parameter list alone would leave that `Why:` without its premise.

**The input class is rare by definition.** Every input of the class holds a comment inside a parameter list, which RH5035 reports (RH5035-001, every ordinary comment from `(` to `)`). `AGENTS.md` (*Code-fix coverage*): "Every input that already violates another Reihitsu rule is rare by definition."

### Mechanism and candidates

Mechanism: RH5107's fix is a text edit that inserts a bare comma behind the previous parameter and replaces only the indentation, the comma, and the whitespace behind it, so every comment in the comma's trailing trivia stays where it was; the line-break phase (`LineBreakListRewriter.CollapseSeparatorsToPreviousElementLine`) moves the comma together with its trailing trivia. Membership: the reported comma's trailing trivia holds a comment.

Dispatch: the analyzer reports commas of `ParameterListSyntax` (declarations, primary constructors, anonymous methods, parenthesized lambdas; indexers are a non-goal); the fix resolves `FirstAncestorOrSelf<ParameterListSyntax>`, so every owner goes through the same text edit. Candidates: block comment followed by the parameter (P), `//` comment (Q), block comment ending the line (R), several comments (X), a multi-line `/* a⏎ b */` behind the comma, each in a method, a primary constructor, a local function, an anonymous method, and a lambda (W).

### Breaking inputs

- FC-004 / FC-021: P, Q, R, X, and a multi-line block comment behind the comma — the shapes the fix mishandles today.
- FC-003: T (comment on the line behind the comma's line — must keep today's fix and output) next to R (comment on the comma's line).
- FC-005 / FC-020: a directive in the gap behind the comma (`,` ⏎ `#if X` ⏎ `int b` ⏎ `#endif`) — the comma's line then holds only the comma; check the fix's line removal keeps the directive.
- FC-008: V under Fix All (two diagnostics, one comment each), and a list mixing one commented and one uncommented leading comma.
- FC-010 / FC-017: LF and CRLF.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5107 analyzer | Must stay |
| RH5107 code fix | Must change (either option below) |
| Fix All (BatchFixer) | Must stay; covered by the V test |
| Formatter (LineBreakPhase, BlankLinePhase) | Must stay — FMT-BREAK-011/012 already give the formatter's placement |
| `Reihitsu.Core` | Not involved |
| RH5107 spec | Must change (RH5107-003, or a new requirement for the withheld class) |
| Rule page `RH5107.md` | Not involved — `general-notes.md` already covers "a diagnostic without a code fix" |
| Tests | RH5107 code-fix tests; RH5107 formatter tests only under option A |

### Refuse option

**Withhold the fix when the reported comma's trailing trivia holds a comment** (P, Q, R, X, V, W; T keeps its fix). Cost: the existing guard span in `RegisterCodeFixesCoreAsync` is extended past the comma's trailing trivia — about one line, no new type. The class is rare by definition (RH5035). It satisfies SHARED-FIX-003, ends the disagreement (no fix result exists that the formatter would change), and puts nothing in question (`squad/README.md`, *What reaches the maintainer*: withholding a fix for a rare class); the PR names the class under `Review notes`.

Requested change (option A, the fix moves the comment with the comma): the fix's text edit has to append the comma *and* its comments behind the previous parameter, distinguish a `//` comment (ends the line) from a block comment followed by code (line break behind the comment, next parameter at the first parameter's column) and from a block comment ending the line (whole line removed). Roughly 15–30 production lines, and it writes a second copy of FMT-BREAK-012's comment-with-comma placement into a fix that does not use the formatter (see *Architecture findings*). For a rare class, `AGENTS.md` (*Special cases do not grow the source*) prefers the refusal.

Declaring a shape unsupported: see *Whole run*.

### Output questions

_None_ on the recommended option: withholding for a rare class is not in question. If the lead prefers option A over withholding, the question to the maintainer is **A vs. withhold**, not A vs. B — B is unavailable for Q and R (above). In that case output question 1 is re-asked with *After, B* replaced by "no fix offered; the diagnostic stays", and its *Before* corrected: formatting the fix's Q/R result adds a blank line above the comment.

### Internal choices

- Withhold (recommended) vs. option A — see *Refuse option*. Both remove the disagreement; withholding is cheaper, needs no copy of the formatter's rule, and needs no maintainer answer.
- The Draft change doc `rh5107-comment-behind-leading-comma.md`: under withholding it states the withheld class and why (SHARED-FIX-003 for Q/R, rarity via RH5035), and its *Why* must drop "each result is stable under the other component".

### Issue goals

- "RH5107's fix result and the formatter's output agree on the input above" — a prescribed solution. Change to: no fix result disagrees with the formatter — the fix is withheld for the class, a test asserts no fix is offered for P, Q, and R, and the spec states the withheld class. The visible change (fix no longer offered on these inputs) is withholding for a rare class, not in question. `Closes #1737` fits: the issue's purpose, ending the disagreement, is met.
- The issue's "Required output" B is not a valid outcome for Q-type inputs (SHARED-FIX-003).

### Architecture findings

RH5107's code fix and the line-break phase own the same decision — where a comma that starts a parameter-list line goes, and with which trivia. The fix lays out by a hand-written text edit with its own anchor-column computation instead of delegating to the formatter (`AGENTS.md`, *Key conventions*: formatting-aware fixes delegate final layout), so it copies FMT-BREAK-011 and diverged on the comment (P/Q/R above). Under `squad/README.md` (*Architecture findings*), the class is rare, so refusing the class first applies; the structural change is a follow-up candidate, not this run's.

### Follow-up candidates

- Make RH5107's fix delegate its layout to the formatter (e.g. formatting the parameter in its list, as SHARED-FIX-007 describes for RH5111) instead of its own text edit and anchor computation — structural cause above. Low priority once the comment class is withheld.

## thoenissen/Reihitsu#1738

### Premise

Real, reachable, and wider than the issue states — confirmed on every construct of FMT-BREAK-021 and FMT-BREAK-022, as the spec author reported (object initializer with leading and trailing comma, anonymous object, `with`, collection initializer, dictionary indexer, complex element initializer, nested `Items = { … }`, collection expression, list pattern, property pattern; array initializers keep the comma on the element's line but at the statement's column). One correction to the reachability: **the code-fix path reaches it too, and violates SHARED-FIX-007 today.**

- RH5301 reports `new D { X = 3⏎    , Y = 4 };` (braces on `new`'s line) and also a correctly braced comma-first initializer (`, Y = 4` puts `Y` off the members' column, RH5301-002). Its fix (formatter-delegating) writes the comma alone on a line at **column 16** (the column of `new`); whole-document formatting of that result moves it to **column 8**. RH5303 (collection initializer) writes it at **column 14**, RH5304 (nested `Items = { … }`) at **column 14**; formatting moves both to 8. Three entry-point-dependent columns for one line: the indentation phase has no rule for it, so its fallback depends on the node it receives (FC-028).
- The issue's "Actual Output" is partly stale: it shows `new D(1, 2) { X = 3` kept on one line, but today's formatter puts both braces on their own lines; only the comma line is as described.

Rarity: comma-first object and collection initializers with the element on the comma's line are reported by RH5301-002 and RH5303-002 (checked), so for those two constructs the input is rare by definition. For collection expressions, list patterns, property patterns, anonymous objects, and `with` initializers no rule reports it (RH5305/RH5306 report nothing on `, 2`). Either way, the output in question is one the formatter produces itself — it splits `, Y = 4` and leaves the lone comma — so "leave the construct as written" is not a refusal the formatter can make by skipping a construct (below).

### Mechanism and candidates

Mechanism (two parts, both needed):

1. Line breaks — `LineBreakInitializerRewriter.EnsureElementsOnSeparateLines` moves each element's first token (and the braces/brackets via `BracePlacer`) onto a new line and never touches a separator, so a comma that a line break separates from the token in front of it keeps its line while the element behind it is moved off it.
2. Column — of the indentation contributors only `ArgumentAlignmentContributor` and `GenericConstraintContributor` call `ListElementAligner.AlignSeparators`; `ObjectInitializerContributor`, `AnonymousObjectContributor`, `CollectionExpressionContributor`, `ListPatternContributor`, and `RecursivePatternContributor` place elements and delimiters but no separator, so a line that starts with a comma keeps the fallback column of the enclosing statement (whole document) or another column (node-level entry points, above).

Membership is decidable for any construct: a comma separator of a list whose line-break rewriter places only element starts, and whose indentation contributor does not align separators.

Dispatch candidates — every `SeparatedSyntaxList` the formatter lays out, checked on `main`:

| Construct (defining type) | Comma that starts a line today |
|---|---|
| `InitializerExpressionSyntax` (object, collection, complex element, nested assignment, array), `AnonymousObjectCreationExpressionSyntax`, `WithExpressionSyntax`, `CollectionExpressionSyntax`, `ListPatternSyntax`, `PropertyPatternClauseSyntax` | at the statement's column (fails) — the run's family |
| `SwitchExpressionSyntax` arms | `, _ => 3` at the statement's column (fails) — outside the family |
| `PositionalPatternClauseSyntax` | `, 2` at the statement's column (fails) — outside the family |
| `BaseListSyntax` | `, IB` at the declaration's column 0 (fails) — outside the family |
| `EnumDeclarationSyntax` members | `, B` at the members' column (holds) |
| `TupleExpressionSyntax`, `ArgumentListSyntax`, `ParameterListSyntax`, attribute arguments, constraint clauses | specified (FMT-INDENT-012, FMT-BREAK-011/012/037) — hold |
| `VariableDeclarationSyntax` (fields), type argument lists, attribute lists | rewritten away (field split, join, list split) — not reached |

The spec author's scope — the whole FMT-BREAK-021/022 family — is right: every member fails by the same two-part mechanism, and narrowing to object initializers would leave the same output on the rest. The three failing constructs outside the family share only part 2 (column); they are follow-up candidates.

### Breaking inputs

Failure classes: FC-004/FC-021 (comment behind the comma; comment in front of it), FC-005/FC-020 (directive or `#if` branch holding a comma), FC-013 (see *Output questions*), FC-016/FC-027 (every construct of the family, nesting), FC-028 and FC-032 (node-level fixes RH5301, RH5303, RH5304, RH5305, RH5306 delegate to the formatter, so their results flip), FC-010 (CRLF, second pass).

Candidate inputs the test implementer must cover:

- A **trailing** comma that starts a line (`X = 3⏎, }`), where the separator count equals the element count — the issue's own example — and a held trailing comma (`X = 3 // k⏎, }`).
- A comma followed on its line by a block comment and an element (`, /* c */ Y = 4`), by a `//` comment, by a block comment ending the line, and by a multi-line block comment (`, /* a⏎ b */ Y = 4`, SHARED-BLANK-008 counting).
- A comma alone on its line with the element on the next line.
- A comma held by a comment (`X = 3 // k⏎, Y = 4`) and by a directive (`X = 3⏎#if A⏎, Y = 4⏎#endif`).
- Every construct in the first row of the table, plus a property pattern directly behind a `case` label; array initializers as `new int[] { … }`, `new[] { … }`, `int[] x = { 1⏎, 2 }` (first element on the brace line), and `stackalloc`.
- Nesting: an initializer inside an argument, a lambda body, and another initializer.
- Code-fix paths: RH5301, RH5303, RH5304 (and RH5305/RH5306 where they report) on a leading-comma input — the fix result must equal the whole-document output.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| LineBreakPhase (`LineBreakInitializerRewriter`) | Must change under option A; must stay under option B |
| IndentationPhase (the five contributors above) | Must change under either option |
| RH5301/RH5303/RH5304/RH5305/RH5306 code fixes | Must stay (they delegate); their output changes with the formatter (FC-032) — tests should cover it |
| Analyzers (RH5301-RH5306, RH5410/RH5411, RH6002) | Must stay — checked: none reports today's output or option A/B output, except the held-comma shape noted below |
| `Reihitsu.Core` | Not involved |
| Rule pages | Not involved |
| Specs | `line-breaks.md` (new requirement, option A only), `indentation.md` (new requirement) |

### Refuse option

- **Leave the construct as written:** not available as a cheap refusal. The lone comma line is produced by the line-break phase itself (it splits `, Y = 4`), and the array case shows the column defect without any line-break rewrite (FMT-BREAK-021 keeps array layout, yet `, 2` lands at the statement's column). Leaving the construct as written would need a guard in the line-break phase and a skip in the indentation phase for every construct of the family — two phases, and the RH5301/RH5303/RH5304 fixes would then make no progress on a reported diagnostic and need withholding too. More expensive than the requested change.
- **Withhold RH5301/RH5303/RH5304's fixes for a comma that starts a line:** a cheap guard, and the class is rare for those constructs, but it only removes the SHARED-FIX-007 violation; the formatter's output stays at the statement's column.
- **Declare unsupported:** the narrow shape — a comma separator of a FMT-BREAK-021/022 construct with a line break (as SHARED-BLANK-008 counts) between the token in front of it and the comma — would delete no special case (the formatter has none for it today), so `AGENTS.md` says it is not declared. The broad shape is under *Whole run*.
- **Simpler option:** the requested change is cheap with existing helpers — the column via `ListElementAligner.AlignSeparators` (already shared by two contributors), the line breaks (option A) via the separator-collapse logic `LineBreakListRewriter` already applies to argument lists; reuse has to handle a trailing separator (separator count equal to element count), which argument lists never have.

### Output questions

Corrections to the spec author's questions 2 and 3:

- **Question 2, option A as drafted conflicts with RH5301, RH5303, and RH5304 (FC-013, SHARED-DIAG-010).** Its held-comma line `, Y = 4` (case `c`) puts the member's first token two columns right of the members' column; RH5301-002 reports it, RH5303-002 reports `, 2` in a collection initializer, and RH5304 reports it in a nested `Items = { … }` (all three checked on a fixture holding exactly that output). Their fixes delegate to the formatter, which under that option would write the same line back — a permanent diagnostic. The held comma has to stay alone on its line, the element on the next line, as `main` already breaks it; only the column changes. That is also cheaper: the line-break phase leaves held commas as today.
- **Question 3 is not open as drafted.** Its option B (move the comma in an array initializer) contradicts FMT-BREAK-021, whose `Why:` keeps array layout as written for a stated reason. Only the column is open, and it is the same column question 2 decides. Fold it into question 2.

Recommended form (one question):

**2 · Formatter — comma that starts a line inside an initializer, collection expression, or pattern**

Input:
```csharp
var a = new D(1, 2) { X = 3
    , };
var b = new D { X = 3
    , /* c */ Y = 4 };
var c = new D
{
    X = 3 // k
    , Y = 4
};
int[] d = [1
    , 2];
var e = new int[]
{
    1
    , 2
};
```

Before (`main`): as in the spec author's question 2 and 3 — the comma alone on its line at the statement's column, `e`'s `, 2` at the statement's column; through RH5301's fix the comma lands at the column of `new` instead.

After, A (recommended) — the comma ends the previous element's line, a comment behind it goes with it; a comma a comment keeps off that line stays alone on its line at the elements' column; an array initializer keeps its line breaks and its comma line takes the elements' column:
```csharp
        var a = new D(1, 2)
                {
                    X = 3,
                };
        var b = new D
                {
                    X = 3, /* c */
                    Y = 4
                };
        var c = new D
                {
                    X = 3 // k
                    ,
                    Y = 4
                };
        int[] d = [
                      1,
                      2
                  ];
        var e = new int[]
                {
                    1
                    , 2
                };
```

After, B — the comma keeps its own line, at the elements' column (`e` as in A):
```csharp
        var a = new D(1, 2)
                {
                    X = 3
                    ,
                };
        var b = new D
                {
                    X = 3
                    , /* c */
                    Y = 4
                };
        var c = new D
                {
                    X = 3 // k
                    ,
                    Y = 4
                };
        int[] d = [
                      1
                      ,
                      2
                  ];
```

Why A: the comma sits where every argument and parameter list puts it, and the element behind it starts its line like every other element.

### Internal choices

- Scope: the whole FMT-BREAK-021/022 family (agree with decision 3 in `00-lead.md`); switch expression arms, positional patterns, and base lists outside it, as follow-ups.
- New requirements vs. paragraphs in existing ones — internal; no visible difference. The indentation requirement's wording should state the column for a comma line where the first element shares the opening brace's line (`int[] x = { 1⏎, 2 }`): the column an element that starts a line takes (FMT-INDENT-022/023/025), not the first element's column.

### Issue goals

- "Specify the decided behavior and cover it with a test" — keep; widen the coverage to the family and to the code-fix paths (RH5301/RH5303/RH5304).
- "Alternatives Considered: leave as written, the shape is rare" — drop: not a cheap refusal here (see *Refuse option*), and not rare outside object and collection initializers. No visible output depends on dropping it beyond the question above.

### Architecture findings

_None._ The entry-point-dependent column (16/14/8) is a consequence of the missing indentation rule, not of two components owning one decision; once a contributor places the line, every entry point gets the same column. The tests should still assert it per fix (FC-028).

### Follow-up candidates

Same indentation defect outside the run's family — a comma that starts a line falls to the enclosing column. Each is an `agent-filed` follow-up carrying an output question:

**Formatter — comma that starts a line between switch-expression arms**

Input:
```csharp
var s = x switch
{
    1 => 2
    , _ => 3
};
```

Before (`main`):
```csharp
        var s = x switch
                {
                    1 => 2
        , _ => 3
                };
```

After, A (recommended): `, _ => 3` at the arms' column (or the comma moved behind `1 => 2`, matching the run's answer).

**Formatter — comma that starts a line in a positional pattern** — `o is (1⏎, 2)` gives `, 2` at the statement's column; the subpatterns' column (after `(`) is the candidate, as FMT-INDENT-012 places separators of parenthesized lists.

**Formatter — comma that starts a line in a base list** — `class K : IA⏎    , IB` gives `, IB` at column 0; the base types' column is the candidate.

## Whole run

### Declaring a shape unsupported (escalation 10 — named, not recommended)

Named because it is an option for both issues' shapes, as `AGENTS.md` (*Special cases do not grow the source*) requires:

- **Shape:** a comma separator of any comma-separated list that a line break (as SHARED-BLANK-008 counts) separates from the token in front of it — from that token's last character through the comma — excepting nothing.
- **Special cases it would delete:** the leading-comma move with its comment carry-over and the held-comma handling in `LineBreakListRewriter.CollapseSeparatorsToPreviousElementLine` (FMT-BREAK-011/012 comma paragraphs) and in `GenericConstraintLineBreakRewriter` (FMT-BREAK-037 comma parts); this run's planned initializer comma handling would never be written. RH5107's fix does not format through the formatter, so SHARED-FIX-010 would not withhold it.
- **Common code it would refuse:** every comma-first file — parameter lists RH5107 reports and the formatter converts correctly today, argument lists (no rule reports a leading comma there), initializers, collection expressions. The formatter would stop converting comma-first code it handles today.

Recommendation: do not declare. The existing comma handling is small and correct for argument and parameter lists, the run's addition reuses it, and refusing whole files would regress output users get today.

### Corrections to `01-spec-author.md`

- #1737: "Formatting the fix's result changes nothing" is false for Q and R (blank line, FMT-BLANK-011); today's fix violates SHARED-FIX-003 for Q and R; option B is unavailable for them.
- #1738: reachability includes the RH5301/RH5303/RH5304 fixes, which violate SHARED-FIX-007 today; option A's held-comma line conflicts with RH5301/RH5303/RH5304; question 3's option B contradicts FMT-BREAK-021.
