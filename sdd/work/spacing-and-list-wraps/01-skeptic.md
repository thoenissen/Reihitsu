Role: skeptic
Commit: f8239ce
Verdict: #1642: do not make the change (the reported output is what the spec requires, and the inconsistency the issue claims does not exist); #1635: output question (real defect; where the commas go is not decided by the spec)

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The issue texts carry their own suspected causes ("the phase seems to treat the gap as a line break"); I ignored them and checked the code and the CLI instead.

## thoenissen/Reihitsu#1642

### Premise

The issue reports output that does reproduce, but it is not a defect. The issue's central claim is that the space behind a single-line comment is kept while the same space behind a multi-line comment is removed. That claim is false: a single-line comment in the same gap gives exactly the same output.

Evidence. I ran `scripts/format.sh` on `main` (f8239ce) over the issue's input and over a single-line copy of it:

```csharp
// input (single-line copy of the issue's example)
void M(System. /* a b */ IDisposable x)
int y = 1 +  /* a b */ 2;
class D<T> : IComparable</* a b */ T>
    where T : System. /* a b */ IDisposable
// output
void M(System. /* a b */IDisposable x)
int y = 1 + /* a b */ 2;
class D<T> : IComparable</* a b */T>
    where T : System. /* a b */IDisposable
```

The multi-line forms give the same output line for line: `b */IDisposable`, `b */ 2`, `b */T>`, `b */IDisposable`. That matches the issue's "Actual Output".

The spec requires this output:
- FMT-SPACE-001 (last sentence): "A line break inside the text of a multi-line comment is not a line break of the gap." The phase does treat the gap as a same-line gap, so the issue's guessed cause is wrong too.
- FMT-SPACE-013: the rule's decision applies to the whitespace between the comment and the right token, and the whitespace in front of the comment is only collapsed. Its own example is `M( /* four */ a)` → `M( /* four */a)`.
- The deciding rule for each case:
  - The dot of a qualified name, FMT-SPACE-006: no space after it, so `*/IDisposable`.
  - `<` of a type argument list, FMT-SPACE-005: no space, so `*/T`.
  - The binary operator, FMT-SPACE-010: one space, so `*/ 2`.

I widened the check to every kind of rule. Multi-line and single-line comments behaved identically in each fixture, and a second pass was stable:
- The operator rule inserting a missing space: `a +/* p⏎ q */b` → `*/ b`.
- The comma rule: `M(a,/* p⏎ q */b)` → `*/ b`.
- The no-space rule after `(`: `M( /* p⏎ q */ a, b)` → `*/a`.
- A right-carried `/** */` comment (FMT-SPACE-014): `a /** p⏎ q */+b` → `*/ + b`.
- Collapsing in front of the comment: `a    /* p⏎ q */   + b` → `a /* p⏎ q */ + b`.

Callers: the CLI and whole-document formatting reach the case, as the issue says. The output is still correct for both.

Is the spec wrong instead? No. The requested output, `System. /* a⏎ b */ IDisposable`, would contradict FMT-SPACE-013. Its Why is sound: the rule answers for the distance to the next token, so the gap behind the comment is what it decides. FMT-SPACE-006 and FMT-SPACE-005 have sound Whys of their own.

### Mechanism and candidates

_N/A_: there is no defect. FMT-SPACE-001's last sentence (a multi-line comment is not a line break of the gap) has no test today. `rg` finds no multi-line-comment test under `src/Reihitsu.Formatter.Test/Regression/Spacing`.

### Breaking inputs

None fail. A characterization test would pin the issue's three shapes with a multi-line comment, each beside its single-line twin, under LF and CRLF with a second pass (FC-017, FC-033):
- the dot of a qualified name, in a parameter type and in a constraint clause;
- `<` of a type argument list;
- the binary operator.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `HorizontalSpacingPhase` and the spacing rules | must stay |
| `sdd/specs/formatter/horizontal-spacing.md` | must stay |
| Formatter tests | a characterization test only, if the lead keeps one |
| Analyzers, code fixes, `Reihitsu.Core`, rule docs | not involved |

### Refuse option

There is nothing to refuse, because no change is warranted. Declaring a multi-line comment in a same-line gap unsupported would delete no special case, so it is not an option.

### Output questions

_None._ FMT-SPACE-006 and FMT-SPACE-013 decide the output, and both requirements are unchanged.

### Internal choices

Whether to add the characterization test. I recommend adding it, because the FMT-SPACE-001 sentence it would cover has no test today. It changes no visible output.

### Issue goals

- Drop the expected output "`b */ IDisposable x`". It contradicts FMT-SPACE-006 and FMT-SPACE-013. The issue's own criterion ("wherever it is kept behind a single-line comment") is already met, because the space is not kept behind a single-line comment either. Dropping it changes no visible output.
- Change "needs a regression test with the idempotency check" to a characterization test of today's output. This changes no visible output.
- Under `AGENTS.md`, *Issue intake*, the PR links the issue with `Refs #1642`. Whether a reported output that is correct counts as escalation 4 ("does not reproduce") is the lead's call.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1635

### Premise

The defect is real. Callers that reach it:
- the CLI and whole-document formatting;
- the RH5102 code fix. RH5102 reports the issue's input, and its fix delegates to the formatter (RH5102-003), so it is offered and changes nothing.

Evidence, from `scripts/format.sh` on f8239ce. "Unchanged" means the list stays partly wrapped.

| Input | Output today |
|---|---|
| `Foo(1⏎, 2, 3);` (the issue's input) | unchanged |
| `Foo(1, 2⏎, 3);` | unchanged |
| `new B(1⏎, 2, 3)` | unchanged |
| `: this(a⏎, 2, 3)` | unchanged |
| `class P(...) : B(a⏎, b, c);` | unchanged |
| `[AttributeUsage(AttributeTargets.All⏎, AllowMultiple = true, Inherited = false)]` | unchanged, only realigned |
| `Foo(1 // keep⏎, 2, 3);` | unchanged |

`scripts/apply-fix.sh RH5102` on the issue's input gives `no-progress (1 iteration)` under LF and CRLF (FC-009, FC-007, FC-013). RH5101, RH5107, and RH6002 do not report it.

The spec decides the outcome. FMT-BREAK-012 says that a list wrapped anywhere — after its opening parenthesis or between two elements — MUST become one element per line. The non-goal in `line-breaks.md` leaves only the comma's position in an argument list undecided. That position is a decided behavior change for vNext (thoenissen/Reihitsu#956: leading commas move to the end of the previous line). The maintainer kept #1635 separate from #956 for 1.2.2.

### Mechanism and candidates

Mechanism: `LineBreakListRewriter` counts a list as wrapped only when a line break sits behind a separator or in front of an element's first token. It reads this after the first element has been joined to the opening delimiter. Two line breaks are not counted:
- a line break in front of a separator;
- a line break behind the opening delimiter, which only `VisitBracketedParameterList` records (`isWrappedAfterOpener`).

As a result, a list wrapped only in those places is either left partly wrapped or joined onto one line. Code region: `EnsureSeparatorsHaveEndOfLine` and `HasElementSplitSignal` in `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakListRewriter.cs`, and the four visitors that call them. In parameter lists, `CollapseSeparatorsToPreviousParameterLine` moves leading commas first (FMT-BREAK-011), so a wrap in front of a comma reaches the split step only when a comment pins the comma (FMT-BREAK-001).

The dispatch is the `LineBreakListRewriter` visitors for the lists FMT-BREAK-012 covers:
- `VisitArgumentList` (`ArgumentListSyntax`). Owners: invocation, object creation, implicit object creation, constructor initializer, primary-constructor base type.
- `VisitAttributeArgumentList` (`AttributeArgumentListSyntax`).
- `VisitParameterList` (`ParameterListSyntax`). Owners: method, constructor, operator, conversion operator, delegate, local function, record and primary constructor, anonymous method, parenthesized lambda.
- `VisitBracketedParameterList` (`BracketedParameterListSyntax`): indexers.

These lists are not candidates, because FMT-BREAK-014 joins them: `BracketedArgumentListSyntax`, `TypeArgumentListSyntax`, `TypeParameterListSyntax`, `FunctionPointerParameterListSyntax`. Tuples and collection expressions are outside FMT-BREAK-012.

### Breaking inputs

Failure classes that can reach the change:
- FC-016: sibling list kinds; the indexer list was fixed in #1644 and the other lists were not.
- FC-004, FC-019, FC-020, FC-021: a comment or directive in front of a comma, or behind `(`.
- FC-010, FC-012, FC-017: a second pass, CRLF.
- FC-027: a nested wrapped list as an element; a list inside a lambda or an initializer.
- FC-013, FC-032, FC-007, FC-009: the RH5101 and RH5102 fixes delegate to the formatter, so their results change with it.
- FC-034: test the issue's exact shape.

Class matrix. The axes are where the wrap sits, crossed with the list kind. Every cell was checked with `scripts/format.sh`, and the parameter cells also with `scripts/apply-fix.sh RH5109`.

| Wrap position | Argument list (invocation, `new`, `: this(...)`, primary-ctor base) | Attribute argument list | Parameter list (method, local function, delegate, operator, record, lambda, anonymous method) | Indexer parameter list |
|---|---|---|---|---|
| W1: only behind `(`/`[` — `M(⏎a, b)` | **fails**: joined to `M(a, b)` | **fails**: `[A(⏎1, 2)]` → `[A(1, 2)]` | **fails**: joined to one line | holds (`isWrappedAfterOpener`) |
| W2: in front of a comma — `M(a⏎, b, c)` (the issue) | **fails**: unchanged | **fails**: unchanged | holds: the comma is moved, then split | holds |
| W2c: in front of a comma pinned by a comment — `M(a, b // keep⏎, c)` | **fails**: unchanged (RH5102 fix withheld by RH5102-004) | (same mechanism; not run) | **fails**: `void N(int first, int second // keep⏎, int third)` and the lambda form unchanged | **fails**: unchanged |
| W3: behind a comma — `M(a,⏎b, c)` | holds (FMT-BREAK-012 example) | holds | holds | holds |
| W4: only in front of `)` — `M(a, b⏎)` | not a wrap of the list (by FMT-BREAK-011's example `void N(int a, int b⏎)` → one line); kept today; see *Follow-up candidates* | — | holds (`)` joined) | holds |
| W2m: mixed — `Foo(1⏎, 2,⏎3)`, each element already on its own line | **wrong-looking**: the comma is split off onto its own line (`Foo(1⏎,⏎2,⏎3)`), stable on a second pass; see *Output questions* 2 | (same mechanism) | n/a (commas moved) | n/a |

Notes on these cells:
- W1 and W2c are failing members of the same class, as the second comment on the issue proposes. FMT-BREAK-012 names W1 explicitly ("after its opening parenthesis").
- W1 is common code. Fixing it changes visible output, which the unchanged FMT-BREAK-012 and RH5109-003 require: RH5109's fix already gives `void N(int a,⏎int b)` for `void N(⏎int a, int b)` while the formatter joins it. The RH5101 and RH5102 fixes for W1 argument lists would also change from joining to one-per-line (RH5101-003 and RH5102-003 delegate to the formatter).
- W2c is a rare class by definition: RH5035 reports comments inside a parameter list, and RH5038 reports them inside an argument list. FMT-BREAK-012, FMT-BREAK-011 and FMT-BREAK-001 decide its output: `int first,⏎int second // keep⏎, int third`, with the comment and the leading comma left in place.

Candidate inputs the test implementer must cover:
- each failing cell, under LF and CRLF, with a second pass;
- the issue's input verbatim;
- `Foo(1, 2⏎, 3)`: a wrap at the last separator;
- a two-element list `Foo(a⏎, b)`, which must stay as it is;
- a directive in front of a comma (`#if X⏎, 2⏎#endif`);
- a nested list element `Foo(Bar(1⏎, 2), 3)`;
- the RH5102 code fix on the issue's input, which must make progress afterwards;
- the RH5101 and RH5102 fixes on a W1 argument list.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `LineBreakPhase` / `LineBreakListRewriter` | must change |
| `IndentationPhase` | must stay. It already aligns a leading comma under the first element; `Foo(1⏎, 2⏎, 3)` is stable |
| `HorizontalSpacingPhase` | not involved |
| RH5101 and RH5102 code fixes | no code change; their results change through the formatter (FC-032). Expected values in their tests may flip, and FMT-BREAK-012 requires the new values |
| RH5109 code fix | not involved. It builds its own layout and already splits W1 |
| RH5101, RH5102, RH5107, RH5109, RH5203, RH6002 analyzers | must stay. None reports any of the options below |
| `sdd/specs/formatter/line-breaks.md` | must stay, unless the maintainer's answer on comma position has to be recorded there (the non-goal or FMT-BREAK-012). That would be a clarification for the spec author |
| `Reihitsu.Core`, rule docs | not involved |

### Refuse option

- **Leave the construct as written** for an argument or attribute argument list with a line break in front of a comma (today's output). Costs:
  - an exception in FMT-BREAK-012, which is a spec change and therefore a question for the maintainer;
  - withholding RH5102's fix for that class (a cheap check), because today it is offered and changes nothing;
  - RH5102 would still report formatter output (SHARED-DIAG-010) unless it stops reporting the class too.

  Leading commas are seldom written in C#, so the class is rare. But the maintainer's triage keeps this a bug against FMT-BREAK-012 for 1.2.2, so I do not recommend refusing. The comment-pinned cell (W2c) is rare by definition (RH5035/RH5038) and may be refused locally. It probably costs nothing to keep, because it is the same mechanism.
- **Declare unsupported**: not an option.
  - "A line break in front of a comma in an argument list" would delete no special case. No leading-comma code exists for argument lists, and parameter-list comma joining serves FMT-BREAK-011 for common code.
  - "A comment between a list element and its following comma" would delete nothing either. The guard involved, `WouldJoinAcrossUnjoinableTrivia`, serves FMT-BREAK-001 everywhere.
- **Narrower option**: fix only W2 in argument and attribute argument lists, and file W1 and W2c as a follow-up. This leaves a known failing part of the same class in the release (FC-016).

### Output questions

**1 · Formatter · an argument list wrapped in front of a comma**

Input:
```csharp
Foo(1
    , 2, 3);
```

Before (`main`): unchanged.

After, A (recommended):
```csharp
Foo(1
    , 2
    , 3);
```

After, B:
```csharp
Foo(1
    , 2,
    3);
```

After, C:
```csharp
Foo(1,
    2,
    3);
```

Why A: every argument gets its own line in the author's leading-comma style, no comma moves to another line, and the result matches the other leading-comma lists the formatter keeps until the planned change that moves leading commas behind the previous argument (#956). The same answer applies to attribute argument lists (`[AttributeUsage(AttributeTargets.All⏎, AllowMultiple = true, Inherited = false)]`).

What would change the pick to B: if the maintainer prefers the smallest change to the layout logic over a uniform comma style. A needs a check of which side of the commas the list already wraps on, decided from the list's own separators; B does not.

What would change the pick to C: if the maintainer wants this release to match the vNext layout now, accepting that `Foo(a⏎, b)` keeps its leading comma until then while `Foo(a⏎, b, c)` loses it.

**2 · Formatter · a comma split off onto its own line**

Input (each argument already starts its own line):
```csharp
Foo(1
    , 2,
    3);
```

Before (`main`):
```csharp
Foo(1
    ,
    2,
    3);
```

After, A (recommended): unchanged.
```csharp
Foo(1
    , 2,
    3);
```

After, B: as on `main`.

Why A: every argument is already on its own line, so FMT-BREAK-012 asks for nothing here, and a comma alone on a line reads as a mistake.

### Internal choices

Whether W1 and W2c are fixed in this run or filed separately. I recommend fixing them in this run:
- they fail by the same mechanism, at the same decision point, and the unchanged FMT-BREAK-012 requires them;
- splitting them out leaves a known failing part of the class in the release (FC-016);
- whatever the change costs is shared across all of them.

Whatever the lead decides, it should note that W1 changes the formatter's output and the RH5101/RH5102 fix results for common code. This is required by the unchanged requirement and so does not put an output in question, but the PR should say so under `Review notes`.

### Issue goals

- Keep "every argument on its own line". FMT-BREAK-012 requires it.
- "Settle the comma's position as an output question if the spec does not decide it": this is *Output questions* 1 and 2 above.
- The second comment's observation 1 (W1): keep it. FMT-BREAK-012 decides it, so it is not open. It changes visible output for common code, and the spec requires that change.
- The second comment's observation 2 (W2c): the comment calls it open. It is not: FMT-BREAK-012 together with FMT-BREAK-001 decides it (split behind `first,`, and the comment keeps the leading comma in front of `third`). It is a rare class (RH5035/RH5038) that the lead may refuse locally.

### Architecture findings

There is one rule stated in more than one place. FMT-BREAK-012's "a list wrapped behind its opening delimiter is wrapped" is recorded only in `VisitBracketedParameterList` (`isWrappedAfterOpener`, added in #1644). `VisitArgumentList`, `VisitAttributeArgumentList`, and `VisitParameterList` compute the same decision without it, so they disagree with it on W1. These are sibling copies of one rule that have drifted apart (FC-016).

A separate observation, pre-existing and not of this run: the RH5109 code fix builds its own parameter-list layout (RH5109-003, a deliberate design), while `LineBreakListRewriter` owns the same layout. They disagree today on W1 (`void N(⏎int a, int b)`: the fix splits it, the formatter joins it). Fixing W1 makes them agree. I recommend no follow-up.

### Follow-up candidates

- **W4: an argument list wrapped only in front of `)`.** For `Foo(1, 2, 3⏎    );`:
  - RH5102 reports it (RH5102-001: the parentheses are on different lines and arguments share a line);
  - its fix makes no progress (`no-progress` under LF and CRLF, FC-009);
  - the formatter keeps it.

  thoenissen/Reihitsu#956 already decides the target: the closing parenthesis follows the last argument, which gives one line, and RH5102 then stops reporting. This is a candidate for a note on #956, not a new issue, unless the lead wants the no-progress fix withheld until then.

## Whole run

- Commands run on f8239ce: `scripts/format.sh` over fixtures in the scratchpad (two passes where stability mattered), and `scripts/apply-fix.sh` for RH5101, RH5102, RH5105, RH5107, RH5109, RH5203, and RH6002 over the #1635 fixtures under LF and CRLF.
- All fixtures were deleted afterwards. No repository file was changed except this handover.
