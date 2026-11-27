Role: skeptic
Commit: 212b612a4867db0e25cc407ecd5b5db7c5528404
Verdict: proceed narrower

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the issue itself.

## Premise

Real, reachable, and worse than the issue says: the fixes produce code that does not compile. One issue rule (RH5104) is wrong, and the issue's claim that no fix reaches node-level formatting once these rules stop reporting is also wrong.

**Reachability.** The analyzers run in every project that references the package, whatever its `LangVersion`. A net6.0 project defaults to C# 10, and the analyzers read the effective version from `SyntaxTree.Options` (`CSharpParseOptions.LanguageVersion`), as RH5417 does (`RH5417...Analyzer.cs:67-71`). The code fixes are reached from the IDE on those diagnostics. The CLI and whole-document formatting are already correct (FMT-BREAK-044, `SingleLineInterpolationUtilities`).

**Language facts (compiled).** A scratch `net11.0` library with `<LangVersion>10</LangVersion>` (`dotnet build`, outside the repo) gives these results:
- A line break between the braces of a `$"` interpolation, also inside a `$@"` string nested in such a hole, is `error CS8967: Newlines inside a non-verbatim interpolated string are not supported in C# 10.0`.
- A line break in a `$@"` hole compiles. So does a line break in the part of a `$@"` hole that lies outside a nested `$"` string, e.g. `$@"a {$"c {new P { A = 1 }.A}" + new P⏎{⏎A = 2⏎}.A} b"`.
- A `/* c */` in a single-line `$"` hole compiles under C# 10.
- All 65 probe fixtures below compile under C# 10 as written (each on one line).

**Observed today (C# 10).** The command is `scripts/apply-fix.sh <ID> <dir> --lang-version 10 --no-install`. The issue's line "Not run: ... apply-fix.sh parses with the newest language version only" is stale: the script takes `--lang-version`. Each issue input sits in a method of a class:

```csharp
var a = $"a {new P { A = 1, B = 2 }.A} b";              // RH5301 reported, fix writes:
var a = $"a {new P
             {
                 A = 1,
                 B = 2
             }.A} b";                                      // CS8967 (compiled)

var d = $"a {F(() => { var a = 1; return a; })} b";      // RH5008 reported, fix writes:
var d = $"a {F(() => { var a = 1;

    return a; })} b";                                      // CS8967 (compiled)

var e = $"a {(f = [Obsolete] () => 1)} b";               // RH5513 reported, fix writes:
var e = $"a {(f = [Obsolete]
() => 1)} b";                                              // CS8967 (compiled)
```

RH5303 and RH5206 do the same: their fixes write the initializer and the switch braces onto new lines. RH5103 is reported with its fix withheld (RH5103-005). Under `--lang-version 11` all seven issue rules report as well, and every fix result is valid there.

`apply-fix.sh` labels these broken results `fixed`, not `invalid-result`: its check (`FixtureRunner.HasSyntaxErrors`) reads only syntax-tree diagnostics, and CS8967 is not one. This matches FMT-BREAK-044's *Why*: "the parser does not report it".

**RH5104 is not a member.** Its change can be written at that position: the comment moves onto its own line above the statement. RH5104-001's own example reports `$"{x {|RH5104:/* hole */|}}"`. Under C# 10 the fix writes the following, which compiles (verified):

```csharp
/* c */
var g = $"a {x } b";
```

SHARED-DIAG-011's second sentence therefore requires this report to stay.

**The node-level formatting path stays reachable.** A rule whose change *can* be written on one line stays reported in the hole, and its fix still formats a target inside the hole. `ReihitsuFormatter.FormatNodeInDocumentAsync` (`ReihitsuFormatter.cs:235`) and `FormatNode` (`:158`) detach the target. `FormatNodeInDocumentWithContextAsync` formats a context node that itself lies inside the hole. In each case `SingleLineInterpolationUtilities.Annotate` walks a root with no enclosing `InterpolationSyntax`, so nothing is protected. Observed under C# 10:

```csharp
var s = $"a {G(delegate (int p) { var a = p; return a; })} b";   // RH3004 fix writes:
var s = $"a {G((int p) =>
               {
                   var a = p; return a;
               })} b";                                             // CS8967

var s = $"a {F(() => { int L() => 1; return L(); })} b";        // RH3208 fix writes:
var s = $"a {F(() => { int L()
                       {
                           return 1;
                       } return L(); })} b";                       // CS8967
```

Both rules' changes can be written on one line (`(int p) => { ... }`, `int L() { return 1; }`), so both diagnostics stay reported (SHARED-DIAG-011), and their fixes break the code. This violates SHARED-FIX-003 ("a compile error that the code did not have") and FMT-BREAK-044 ("the formatter MUST NOT write a line break ... at any depth").

The existing test `NonVerbatimInterpolationLineBreakTests.FormatNodeInDocumentAsyncKeepsInterpolationOnOneLineInCSharp10Project` only uses a target that *contains* the string (method `M`). No test uses a target inside the hole.

`RH3004` with an expression-bodied result, `$"a {G(delegate (int p) { return p; })} b"` → `(int p) => p`, is fine. In a `$@"` hole the RH3208 result is valid.

## Mechanism and candidates

**Mechanism.**
- **Analyzer side.** A rule whose requested change needs a line break or a blank line at the reported position reports it even when that position lies, at any depth, between the braces of an interpolation of a `$"` string while the effective language version is below C# 11. There no line break can be written (CS8967). Only RH5417 asks `StringInterpolationUtilities.IsInsideSingleLineInterpolation`.
- **Fix side.** A code fix whose result holds a line break at such a position writes it. For formatter-based fixes, the cause is that the node-level entry points detach the target, so the whole-document guard (FMT-BREAK-044) never sees the enclosing interpolation.

**Dispatch.** I derived the candidates from the rule set, not from the issue:
- every Layout analyzer with a code fix (116 IDs, `src/Reihitsu.Analyzer/Rules/Layout/` ∩ `src/Reihitsu.Analyzer.CodeFixes/Rules/`), each run under C# 10 over 76 one-line hole fixtures;
- the 17 Layout rules without a fix (RH5033–RH5049, comment placement), read, not run;
- every non-layout fix that calls `ReihitsuFormatter.FormatNode*` (RH3004, RH3208, RH7101, RH2001, and RH3202/3203/3205/3206/3207 via `ExpressionBodyToBlockCodeFixProviderBase`);
- every non-layout fix that writes `EndOfLine`/`ElasticCarriageReturnLineFeed` itself (RH3004, RH7004, RH7309, RH8201, RH8303, RH8402).

Only RH3004 and RH3208 can have a target inside an expression. The others act on members, types, regions, or documentation.

**Analyzer members: reported in a C# 10 `$"` hole, change needs a line break or blank line, so MUST NOT report.** A probe fixture number means the rule was observed reporting there.

| Rule | Defining type / shared owner | Construct in the hole | Probe |
|---|---|---|---|
| RH5001–RH5019 (blank line before try/if/while/do/using/foreach/for/return/goto/break/continue/throw/switch/checked/unchecked/fixed/lock/yield) | SHARED-STMT-001 (`statement-blank-lines.md`) | a statement after another statement in a lambda, anonymous-method, or local-function block | observed: 5001, 5002, 5003, 5004, 5005, 5006, 5007, 5008, 5009, 5010, 5012, 5013, 5014, 5015, 5016, 5018, 5019. Not run: RH5017 (needs `unsafe`). RH5011 (break *followed by* a statement) is a member by RH5011-001 ("a statement on the same line as the `break` statement is reported too") but was not observed: probe 80 ended the section with `}` |
| RH5021, RH5029 | local declaration followed / preceded by blank line | `var t = 0; t++;`, `x++; var t = 0;` | observed |
| RH5103 | multiple statements on one line | `{ var a = 1; return a; }` | observed (fix withheld) |
| RH5403 | block with statements on one line | `if (b) { return 1; }` inside the lambda | observed; fix writes CS8967 |
| RH5110 | constraints on their own line | `int L<T>() where T : class => 1;` in the lambda | observed; fix writes CS8967 |
| RH5206 | switch-expression braces | `x switch { 1 => "one", _ => "other" }` | observed |
| RH5301 | object initializer (SHARED-INIT) | `new P { … }`, `new P() { … }`, nested `{ Q = { A = 1 } }`, `{ L = { 1, 2 } }`, indexer initializer `new Dictionary<int,int> { [1] = 2 }`, inside an anonymous object, target-typed `new() { A = 1 }` | observed |
| RH5303 | collection initializer (SHARED-INIT) | `new List<int> { 1, 2 }`, `new Dictionary<int,int> { { 1, 2 } }` | observed |
| RH5513 | method attribute placement (SHARED-ATTR) | `[A] () => 1`, `G([A] (int p) => p)`, `[A] int L() => 1;` in the lambda | observed |
| RH5527 | return-value attribute placement (SHARED-ATTR) | `[return: A] () => 1` | observed |

**Not members** (report and change stay as they are):
- **RH5104**: the change can be written, as shown under *Premise*.
- **RH5405** (braces): reported with no fix offered; `if (b) { return 1; }` can be written on one line.
- **RH5514, RH5528** (one attribute per list): reported with the fix withheld; `[A][B]` can be written on one line.
- **RH5030**: not reported, because SHARED-BLANK-003 exempts a statement on the same line.
- **RH5417**: already exempt (RH5417-005).
- **RH3004, RH3208**: the change can be written on one line, so they stay reported. They belong to the fix side, below.
- **Comment-placement rules RH5033–RH5049**: they ask for a comment to move out of a construct, which a single line admits.
- **Not observed reporting**: arrays (`new[] { 1, 2 }`), anonymous objects without a nested initializer, `with`, property patterns, queries, ternaries, logical expressions, and fluent chains. RH5302, RH5308, RH5201, RH5101, RH5102, RH5111, and RH5112 report only forms that already span lines, which a compiling C# 10 hole cannot hold. RH5305 and RH5306 need C# 11 and later (collection expressions, list patterns).
- **RH5603**: reported on every fixture. That was noise from the fixtures' final newline.

**Fix-side members** (a fix that stays reachable after the analyzer change and writes a line break into the hole): RH3004 (anonymous method with a block of more than one return), RH3208 (expression-bodied local function in a lambda block). The node-level entry points are the shared path. Every other formatter-based fix on the member list stops being reached once its rule stops reporting.

## Breaking inputs

Failure classes:
- **FC-006**: language-version gate.
- **FC-016 / FC-015**: the shared Core predicate exists. Each rule must use it, not a private copy.
- **FC-027 / FC-039**: nesting. At any depth, `$@"` in a `$"` hole, `$"` in a `$@"` hole.
- **FC-001 / FC-002**: "inside" is the qualifier. A construct that *contains* a `$"` string but whose break lies outside the hole stays reported.
- **FC-028 / FC-041**: detached-node vs. document formatting.
- **FC-019 / FC-021**: trivia axis. `/* */` and `/** */` in the hole do not change the verdict.
- **FC-003**: both sides of the version boundary.

Matrix for the analyzer side. Axes:
- language version: C# 10 (also 9, 7.3), C# 11, latest;
- string kind: `$"`, `$@"`, `@$"`;
- nesting: direct; `$@"` inside a `$"` hole; `$"` inside a `$@"` hole with the construct inside the inner `$"`; same with the construct in the outer `$@"` hole outside the inner string; triple `$"{$@"{$"{…}"}"}"`;
- where in the interpolation: expression, alignment clause, format clause (`{F(…),10}`, `{F(…):D3}`);
- trivia: none, `/* c */`, `/** c */` between the construct's tokens.

| Cell | Expected | Observed today (C# 10) |
|---|---|---|
| `$"` direct, any member rule, C# 10 | not reported | reported (all members above) |
| `$"` direct, C# 11 / latest | reported (holding) | reported |
| `$@"` / `@$"` direct, C# 10 (probe 35, 59) | reported (holding) | reported |
| `$@"` inside `$"` hole (probe 33) | not reported | RH5301 reported |
| `$"` inside `$@"` hole, construct in inner (probe 34) | not reported | RH5301 reported |
| `$"` inside `$@"` hole, construct outside inner (probe 61) | reported (holding) | reported |
| triple nest (probe 60) | not reported | RH5301 reported |
| construct in the expression of an interpolation with an alignment or format clause (probes 64, 65) | not reported | RH5008, RH5103 reported |
| `/* c */` or `/** c */` among the construct's tokens (probes 57, 58, 62, 63) | not reported (the comment does not change the verdict) | RH5301, RH5008, RH5206 reported |
| construct that contains a `$"` string, break outside the hole: `new P { A = 1, B = $"{x}".Length }`; RH5417-005's lambda that holds `$"{1}"` | reported (holding) | not run for RH5301; RH5417 pinned by `LambdaBodyHoldingAnInterpolatedStringIsReportedBeforeCSharp11` |

Fix-side matrix:
- entry point: the fix's own lines (RH5008/RH5021 via `BlankLineCodeFixUtilities`, RH5513/RH5527) and `FormatNodeInDocumentAsync` / `FormatNodeInDocumentWithContextAsync` / `FormatNode` with the target or context *inside* the hole;
- language version: C# 10 or C# 11;
- string kind: `$"` or `$@"`.

Failing cells: every `$"` + C# 10 cell for RH3004 (block result) and RH3208. The `$@"` + C# 10 cell (RH3208, observed) and the C# 11 cells hold.

Candidate inputs the test implementer must cover. No existing test pins any of them. Searched `tests/` for `LanguageVersion.CSharp10` and `CSharp9`: only RH4105, RH5412–RH5417, RH7003 use them, and only RH5417 uses them for holes.

Analyzer:
1. One C# 10 not-reported test per member rule with its probe construct. For the 19 SHARED-STMT rules, one per rule or a shared-policy test, as the coverage convention allows.
2. A C# 11 holding test per rule.
3. The nesting cells 33, 34, 60, 61 and the alignment/format-clause cell 64/65 for at least one rule (RH5301, RH5008).
4. The trivia cell 57/58 (RH5301).
5. A `$@"` holding cell (35).
6. The "contains a string" holding cell for RH5301.

Fixes:
7. RH3004 `$"a {G(delegate (int p) { var a = p; return a; })} b"` under C# 10.
8. RH3208 `$"a {F(() => { int L() => 1; return L(); })} b"` under C# 10, plus its `$@"` holding cell.
9. A formatter regression test of a node-level target inside the hole under C# 10, if the chosen fix shape lives in the entry point (see *Internal choices*).

Every fixture is a method of `class C { private Func<int> _f; static int F(Func<int> g) {...} static int G(Func<int,int> g) {...} public string M(int x, bool b, int[] xs, R r) { var s = <expr>; return s; } }`, with `P { int A, B; List<int> L; P Q; }` and `record R(int A)`.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzers RH5001–RH5019, RH5021, RH5029, RH5103, RH5110, RH5206, RH5301, RH5303, RH5403, RH5513, RH5527 | must change: not reported inside a C# ≤10 `$"` hole |
| RH5104 analyzer and fix | must stay (RH5104-001 example) |
| RH5405, RH5514, RH5528, RH5030, RH5417 | must stay |
| RH3004, RH3208 analyzers | must stay (change can be written) |
| RH3004, RH3208 code fixes | must change: no line break in the hole (withhold, see *Refuse option*) |
| Fix All of the above | follows the per-diagnostic fix; not separately involved |
| `ReihitsuFormatter` node-level entry points | involved only if the internal choice puts the check there; FMT-BREAK-044 is violated through them today |
| Whole-document formatter / CLI | must stay (already correct) |
| `Reihitsu.Core` `StringInterpolationUtilities.IsInsideSingleLineInterpolation` | must stay; reused as is. Its ancestor walk already gives the "at any depth" semantics the compile results show |
| Rule pages, `general-notes.md` | not involved (corner cases stay off rule pages) |
| Specs | SHARED-STMT, SHARED-ATTR (placement), SHARED-INIT, and the rule specs of RH5103, RH5110, RH5206, RH5403 state the position. Recommend once per shared owner rather than per rule (FC-015) |
| `scripts/apply-fix.sh` | not involved in the run (see follow-ups) |

## Refuse option

- **Analyzers.** Not reporting is what unchanged SHARED-DIAG-011 demands, not a refusal. The alternative "keep reporting and withhold every member fix in the hole" contradicts SHARED-DIAG-011 and leaves diagnostics no user can satisfy.
- **Fixes of RH3004 and RH3208.** The refuse option is to withhold the fix when the fixed node lies inside a C# ≤10 `$"` interpolation, with the diagnostic staying. That is one cheap check with the existing Core predicate and no new type. The class is rare: a block-bodied anonymous method with parameters, or an expression-bodied local function inside a lambda block, written inside an interpolation hole, in a project below C# 11. README *What reaches the maintainer* says withholding for a rare class puts nothing in question. The alternative is to make the rewrite land on one line. That would mean predicting the formatter's layout or running the node-level formatter with knowledge of the enclosing string, and it gives a new visible output. It is more than the rare class justifies.
- **Declaring a shape unsupported.**
  - *Span*: from the `{` through the `}` of an interpolation of a `$"` string under C# ≤10, holding a brace-delimited construct (block, initializer, switch-expression braces) or an attribute list.
  - *It would delete*: `SingleLineInterpolationUtilities` (the annotate-and-restore wrapper in the line-break and blank-line phases), the documentation-comment-in-hole guard in `ControlFlowBraceTransform`, the `DocumentationCommentFormattingPhase` guard, the `SwitchCaseBraceRewriter` guard (FMT-SWITCH-017), and the `LambdaAlignmentContributor` guard added in 107d5f7.
  - *It would refuse*: every C# ≤10 file that holds, for example, `$"{x switch { 1 => "a", _ => "b" }}"` or `$"{new P { A = 1 }.A}"`, which is ordinary code.
  - *My reading*: this is not an option under the precheck contract. The language version is not a token of the input, and the input that fails here, a detached node-level target inside the hole, does not hold the shape (FMT-UNSUP-003, FC-041). So declaring would not remove this run's defect. The whole-document path it would replace already works.
  - *Recommendation*: do not declare. Named so the lead can decide whether escalation 10 applies.
- **Simpler options.** None that serve SHARED-DIAG-011. The predicate exists in Core. Each member rule needs one call at its reporting point, or one call in the shared base a family uses, if it has one.

## Output questions

_None._

Not reporting inside the hole is demanded by unchanged SHARED-DIAG-011. Keeping RH5104 is demanded by unchanged RH5104-001. Withholding the RH3004/RH3208 fixes for a rare class is not in question (README).

## Internal choices

1. **Where the fix-side check lives.**
   - (a) In each affected fix (RH3004, RH3208): withhold when the fixed node is inside such a hole.
   - (b) Once in the node-level entry points: a target or context inside such a hole is returned unchanged.

   (b) covers every future caller and states FMT-BREAK-044 for the node-level path. But each fix must then not register an action that changes nothing (FC-009 / SHARED-FIX-005), and a fix that builds its node before formatting would otherwise write the unformatted rewrite. Note also that (b) puts a version-dependent check on an entry point that the precheck contract cannot carry.

   Recommend (a), together with a node-level formatter regression test that documents the gap is not reached. Both give the same visible output, "no fix offered".
2. **Where the analyzer check lives.** Per rule at its reporting point, or in a shared base where a family has one (the SHARED-STMT blank-line rules, the attribute placement base). Recommend the shared base where one exists, so the policy has one owner per family.

## Issue goals

- **"Each rule above reports nothing inside such a hole"**: drop RH5104. Its change can be written and RH5104-001 reports exactly this example. Dropping it changes no visible output against `main`.
- **Widen to the full member list above.** That list adds RH5001–RH5007, RH5009–RH5019, RH5021, RH5029, RH5110, RH5403, RH5527, the same mechanism under the same unchanged requirement. Widening changes visible output only as SHARED-DIAG-011 already demands, so nothing is in question. Size: about 25 analyzers. Several share a base, so the lead may weigh escalation 8 / a split against README *A batch shares a mechanism*. The mechanism is one, so I recommend keeping it in this run.
- **"No fix formats a target inside such a hole"**: keep, but correct the premise "once these diagnostics are not reported there, no caller reaches it". RH3004 and RH3208 still reach it, so the goal needs its own change.
- **"the rules' specs state the position"**: state it once per shared owner (SHARED-STMT, SHARED-ATTR placement, SHARED-INIT), and per rule only for rules without one (RH5103, RH5110, RH5206, RH5403).
- **The issue's "Not run" note** is stale (`--lang-version` exists), with no effect on the goals.

## Architecture findings

- **FMT-BREAK-044's decision has two paths and only one applies it.** The decision is "no line break between the braces of a `$"` interpolation below C# 11". The whole-document path applies it: `SingleLineInterpolationUtilities.Annotate` reads it from the root. The node-level entry points detach the target (`ReihitsuFormatter.cs:158`, `:235`) or format a context that lies inside the hole, so the same decision is not applied. RH3004 and RH3208 above show the two paths disagreeing (FC-028). The class is rare, so README *Architecture findings* says refuse first: internal choice 1(a).
- **The analyzer-side position rule ("no layout report inside such a hole") is stated only in RH5417-005 today** and is about to be needed by about 25 rules. Stating it once per shared owner keeps it from drifting (FC-015). This is a spec-placement point for the spec author, not a structural code change.

## Follow-up candidates

- **`scripts/apply-fix.sh` reports a fix result that does not compile under the chosen language version as `fixed`.** `FixtureRunner.HasSyntaxErrors` reads only syntax-tree diagnostics, and CS8967 is not one. A tooling follow-up could make `--lang-version` runs flag such results; it is not visible output.
- **The RH5110 fix also rewrites an expression-bodied local function into a block body**, through node-level formatting (probe 19). This is likely the formatter's intended structural transform (P11), noted only because it surfaced here; no action proposed.
