Role: skeptic
Commit: 431caae3b22d347f0963069b2461c413cbaa843a
Verdict: proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored.

## Premise

Real, and wider than the report.

- **Caller and path.** Whole-document formatting through the CLI with `--lang-version 10` (CLI-spec option, parsed per FMT-PIPE-008). Reproduced on 431caae with `scripts/format.sh --lang-version 10 <dir>` on the issue's input: the output is exactly the issue's "Actual Formatted Output" (the `delegate` block broken over four lines inside the hole). The code-fix path (a formatter-backed fix in a C# 10 project, `FormatNodeInDocumentAsync` reads the project's parse options) was not run: `scripts/apply-fix.sh` parses with `LanguageVersion.Latest` only (`src/Reihitsu.Tooling/FixtureRunner.cs:145`, `:437`). CLI reachability is enough for the premise.
- **The output does not compile.** Checked with a throwaway Roslyn 5.12.0 probe (same version as `RoslynVersion`) that parses each file with `CSharpParseOptions(CSharp10)` and compiles it against the runtime's reference assemblies (no existing entry point compiles semantically). Issue output: `error CS8967: Newlines inside a non-verbatim interpolated string are not supported in C# 10.0`.
- **CS8967 is a binder error, not a syntax error.** The C# 10 parse of the broken output has no syntax diagnostic; a second CLI pass under `--lang-version 10` does not report "Skipped ... syntax errors" and changes nothing (`Formatted 0 of 1 file(s).`). So FMT-PIPE-005 (syntax errors leave input untouched) and FMT-PIPE-021 (each phase's tree parses back) do not catch it, and the output is idempotent. The second pass leaves the broken file alone because the tree is unchanged, not because the file is skipped.
- **Which spec it violates.** FMT-PIPE-003 ("MUST NOT change what the formatted code compiles to"). FMT-PIPE-008's `Why:` says the same thing: the formatter must not produce syntax the project's compiler rejects. No formatter requirement states the exception today. The FMT-BREAK requirements that break blocks and initializers say nothing about the version, so as written they demand the break. See *Internal choices* 1.
- **C# 11 side holds.** Every formatted output listed below compiles under `CSharp11` (probe: no errors). The verbatim case (c15) compiles under `CSharp7_3` before and after formatting.
- **Input class that compiles below C# 11.** Under C# 10, *any* line break anywhere between an interpolation's braces in a `$"` string is CS8967, including a line break inside a nested verbatim string's text. Probe: `$"a {@"x⏎y".Length} b"` and `$"a {$@"x⏎{1}".Length} b"` both give CS8967. So every input that compiles below C# 11 has each non-verbatim hole on one line. The formatter therefore never has to join anything there. It only must not write a line break into such a hole.
- **Severity.** The maintainer triaged it as maintenance (the formatter is used from C# 12 today). The class is rare in that sense, but the CLI option and C# 7.3–10 projects reach it. It is a safety-rail class (output that does not compile, `squad/README.md` escalation 2), so it is a bug against FMT-PIPE-003 and not an output preference.

## Mechanism and candidates

**Mechanism.** The line-break phase writes line breaks between tokens that lie inside an `InterpolationSyntax` of an `InterpolatedStringExpressionSyntax` whose start token is `InterpolatedStringStartToken` (`$"`), at any depth of nesting, without reading `FormattingContext.LanguageVersion`. Below C# 11, every line break at such a position is CS8967.

Membership is decidable from the syntax: the hole of a `$"` string at any depth, so a `$@"`/`@$"` string nested inside such a hole is included, and a `$"` string nested inside a verbatim hole is included too. A raw string needs C# 11, so it cannot occur.

**Owner.** Only `LineBreakPhase` writes the line breaks. `scripts/trace.sh` on c11, c12, c18 (one pass, newest version) shows that `StructuralTransformPhase` changes the hole without any line break (`if (b) {return 1; }`, `int G() {return 1;}`), and `LineBreakPhase` writes every line break. The blank-line phase only adds blank lines between lines that are already broken, so it cannot touch a one-line hole. Indentation, horizontal spacing, and cleanup write no line breaks. The documentation-comment, using, and region phases have nothing in a one-line hole: c29 and c30 put a `/** */` in a hole, and it stays in place. `SwitchCaseBracePhase` runs after the line-break phase. On a multi-statement section it inserts braces (c28 at the newest version). What it does on a section the line-break phase leaves on one line cannot be observed on `main`, so it is unsettled and must be covered (c28).

**Dispatch.** `LineBreakPhase.CreateRewriters` (`src/Reihitsu.Formatter/Pipeline/LineBreaks/LineBreakPhase.cs:37-54`). These rewriters can write a line break into a one-line hole: `LineBreakBlockRewriter`, `LineBreakInitializerRewriter`, `LineBreakContainedBlockRewriter`, `DeclarationBraceLineBreakRewriter`, `GenericConstraintLineBreakRewriter`, `AttributeTargetFormattingRewriter`, `SwitchCaseWhenLineBreakRewriter`, and `MemberLineStartRewriter`/`BracePlacer`, which they share. The join rewriters (`AttachedPartLineStartRewriter`, `ContinuationJoinRewriter`, `ListElementInteriorJoinRewriter`) and the rewriters that only act on an already wrapped construct (`LineBreakListRewriter`, `BinaryOperatorLineBreakRewriter`, `ChainLineBreakRewriter`, `TernaryLineBreakRewriter`, `LineBreakAssignmentRewriter`) found nothing to do in a one-line hole in the fixtures (c19, c31). The candidates below come from these rewriters, not from the issue text.

## Breaking inputs

Failure classes that reach the changed code: FC-006 (output does not compile), FC-016 (sibling shapes keep the hazard), FC-027 and FC-039 (nested levels: a lambda in a lambda, a string in a string), FC-003 (both sides of the boundary: C# 10 and C# 11, `$"` and `$@"`), FC-033 (a harness that parses with `Latest` normalizes the scenario away: every reproduction must parse with C# 10 or lower), FC-017 (assert the exact output; the compile check alone is weaker), FC-010 (idempotency), FC-015 (the policy already lives privately in RH5417, see *Architecture findings*), FC-028 (`FormatNode` on a `SyntaxFactory`-built node resolves to the newest version per FMT-PIPE-008, unlike the document entry points; that is the specified behavior and not part of this run), FC-013 (layout rules flag the C# 10 one-line hole, see *Follow-up candidates*).

**Class matrix.** The axes are: the construct the line-break phase breaks in the hole; the string kind that owns the hole and the kind nested in it; and the language version. Every fixture below is one line (`class C { ... }`, the containing method `string M(...) { var s = <expr>; return s; }`, and `using System;`/helpers as needed). Each compiles under C# 10 with no error (probe). It was formatted with `scripts/format.sh --lang-version 10 --force <dir>`, and the output was compiled with the probe under `CSharp10` and `CSharp11`. In the table, "fails" means CS8967 under C# 10. Every output compiles under C# 11.

| # | Hole expression (`var s = …`) | C# 10 output |
|---|---|---|
| c01 | `$"a {F(delegate { return 1; })} b"` (`int F(Func<int> f) => f();`) | fails |
| c23 | same, in an expression-bodied method `string M() => $"a {F(delegate { return 1; })} b";` | fails |
| c27 | `$"a {F(delegate { })} b"` (`int F(Action f) { f(); return 1; }`) | fails |
| c02 | `$"a {F(() => { return 1; })} b"` | fails |
| c26 | `$"a {F(() => { })} b"` (`Action`) | fails |
| c10 | `$"a {F(() => { var a = 1; return a; })} b"` | fails |
| c11 | `$"a {F(() => { if (b) return 1; return 2; })} b"` | fails |
| c12 | `$"a {F(() => { switch (x) { case 1: return 1; default: return 2; } })} b"` | fails |
| c28 | `$"a {F(() => { switch (x) { case 1: G(); return 1; default: return 2; } })} b"` | fails (newest version: switch-case brace phase inserts section braces) |
| c18 | `$"a {F(() => { int G() => 1; return G(); })} b"` | fails |
| c35 | `$"a {F(() => { int G<T>() where T : class => 1; return G<string>(); })} b"` | fails |
| c37 | `$"a {F(() => { [A] int G() => 1; return G(); })} b"` | fails |
| c25 | `$"a {F(() => { try { return 1; } catch { return 2; } })} b"` | fails |
| c34 | `$"a {F(() => { using (var m = new MemoryStream()) return 1; })} b"` | fails |
| c32 | `$"a {F(() => F(() => { return 1; }))} b"` | fails |
| c03 | `$"a {new P { A = 1, B = 2 }.A} b"` | fails |
| c21 | `$"a {new P { Q = { A = 1 } }.Q.A} b"` | fails |
| c04 | `$"a {new List<int> { 1, 2 }.Count} b"` | fails |
| c05 | `$"a {new { A = 1, B = 2 }.A} b"` | fails |
| c33 | `$"a {new { A = 1, B = new { C = 2 } }.B.C} b"` | fails |
| c13 | `$"a {(r with { X = 1 }).X} b"` (`record R(int X);`) | fails |
| c06 | `$"a {x switch { 1 => "one", _ => "other" }} b"` | fails |
| c36 | `$"a {o switch { int i when i > 1 => 1, _ => 2 }} b"` | fails |
| c14 | `$"a {(f = [A] () => 1)} b"` (`Func<int> f = null;`) | fails: `[A]` / `() => 1` split |
| c16 | `$"a {$@"x {F(delegate { return 1; })}"} b"` (verbatim nested in a regular hole) | fails |
| c17 | `$@"a {$"x {F(delegate { return 1; })}"} b"` (regular nested in a verbatim hole) | fails |
| c15 | `$@"a {F(delegate { return 1; })} b"` (verbatim only) | holds: breaks, compiles (also under C# 7.3) |
| c07 | `$"a {x is { Length: 1 }} b"` | holds: stays on one line |
| c08 | `$"a {new int[] { 1, 2 }.Length} b"` | holds: stays on one line |
| c22 | `$"a {((Span<int>)stackalloc int[] { 1, 2 }).Length} b"` | holds: stays on one line |
| c09 | `$"a {(from x in xs where x > 1 select x).Count()} b"` | holds: stays on one line |
| c19 | `$"a {(b ? 1 : 2)} b"` | holds |
| c31 | `$"a {xs.Where(x => x > 1).Select(x => x * 2).OrderBy(x => x).Count()} b"` | holds |
| c24 | `$"a {x /* c */} b"` | holds |
| c29 | `$"a {/** d */ x} b"` | holds |
| c30 | `$"a {x /** d */} b"` | holds |

Example output (c02, C# 10):

```csharp
        var s = $"a {F(() =>
                       {
                           return 1;
                       })} b";
```

Example output (c14, C# 10):

```csharp
        var s = $"a {(f = [A]
            () => 1)} b";
```

**Inputs the test implementer must cover.** Coverage search: `tests/` has no test that uses C# 10 or lower parse options together with an interpolated string, and none that mentions CS8967. Formatter tests use `CSharp10` only in `Regression/Structural/*` (empty type semicolon, expression bodies). So none of these inputs is pinned today.

- The issue's own input (c01 shape in its `var b1` form), under C# 10, with exact output: the hole keeps its line, and the blank line in front of `return b1;` still appears (it is outside the hole).
- One input per failing rewriter family: block (c02/c01), statements inside a block (c11, c18, c28), initializer (c03, c04, c05, c13), switch expression (c06), lambda attribute (c14), nested string both ways (c16, c17), lambda in a lambda (c32).
- The boundary: the same inputs under C# 11, where today's breaking stays (FC-003), and c15 (`$@"`) under C# 10, where the breaking stays.
- c28 in particular, because the switch-case brace phase's behavior on a one-line section cannot be observed on `main`.
- Idempotency under C# 10 (a second pass is a no-op).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter `LineBreakPhase` | must change: it must write no line break inside a hole of a `$"` string, at any depth, below C# 11 |
| Formatter `SwitchCaseBracePhase` | unsettled, possibly must change: covered by c28 |
| Formatter `StructuralTransformPhase`, `HorizontalSpacingPhase` | must stay: they change the hole without line breaks (braces, spacing), and the result compiles |
| Formatter `BlankLinePhase`, `IndentationPhase`, cleanup, documentation, using, and region phases, precheck | not involved (nothing to act on in a one-line hole) |
| `ReihitsuFormatter` node-level entry points | not involved: they pass the project's language version (FMT-PIPE-008). No entry point predicts a phase decision here (question 10: none) |
| CLI | not involved (`--lang-version` already reaches the phases) |
| Analyzers / code fixes | not involved in this issue. RH5417 already exempts the class (RH5417-005). Other layout rules do not: see *Follow-up candidates* |
| `Reihitsu.Core` | not involved, unless the lead acts on the architecture finding |
| Rule docs | not involved |
| Specs | a clarification is needed: see *Internal choices* 1 |
| Tests | must change: new formatter regression tests (above) |

## Refuse option

- **Leave the construct as written.** Below C# 11, the formatter writes no line break into a hole of a `$"` string. This is a cheap syntactic check: an ancestor `InterpolationSyntax` whose string starts with `InterpolatedStringStartToken`, plus `FormattingContext.LanguageVersion`. It is the requested change itself, so it costs nothing extra: no new type, no new mechanism. The class is rare (C# 10 or lower, with a block, initializer, switch expression, or lambda attribute inside a regular hole). The refuse option and the fix coincide.
- **Declaring a shape unsupported** is not an option under the FMT-UNSUP contract. The shape depends on the language version, which is not part of the input's tokens and trivia, so the precheck cannot recognize it "from the tokens and trivia inside that input alone". It would also delete no special case: the formatter holds none for interpolation holes today. AGENTS.md says a shape that deletes nothing is not declared. If the lead still counts it as one: the span would be every token between the braces of an interpolation of a `$"` string, compiled below C# 11, that the line-break phase would break, and it would refuse whole C# 10 files that hold `$"{new P { A = 1 }.A}"`, which is ordinary C# 10 code. Not recommended.
- **Version-independent alternative.** Never break inside a `$"` hole in any version. Simpler, but it changes C# 11+ output (a visible change) and contradicts RH5417-001, which reports such a body from C# 11 on (SHARED-DIAG-010). Not recommended.
- **Rewrite the string as verbatim** (`$"` → `$@"`) so the breaks compile. This changes how escapes in the literal's text are read (FMT-PIPE-003). Rejected.

## Output questions

_None._ The C# 10 output for the reported class (the hole keeps its line, and every other requirement still applies inside it) is what FMT-PIPE-003 demands from an unchanged requirement. Outside holes nothing changes, and C# 11+ is unchanged. See *Internal choices* 2 for the one reading that could look like a question.

## Internal choices

1. **Where the exception is stated.** The FMT-BREAK requirements that break blocks, initializers, switch expressions, and attribute lists demand the break without a version condition, and only FMT-PIPE-003 and the `Why:` of FMT-PIPE-008 forbid it here. Options:
   - (a) The spec author adds a clarification, one requirement in `sdd/specs/formatter/line-breaks.md`, or an example under FMT-PIPE-008, saying that below C# 11 no line break is written into a hole of a non-verbatim interpolated string at any depth, with RH5417-005's `Why:`. New tests then have a requirement to tag.
   - (b) No spec text; tests tag FMT-PIPE-003.

   Recommend (a): the spec checker will otherwise find tests whose asserted output contradicts the unconditional FMT-BREAK wording. This is a bug-fix clarification, not a change doc: it brings the output back to FMT-PIPE-003, and C# 11+ output does not change.
2. **"Output equals the input" versus "no line break in the hole".** The issue's expected output says "the output equals the input", but its own example adds a blank line before `return b1;`. For c11 the two readings differ inside the hole: structural transforms add `{ return 1; }` and spacing normalizes. Reading "only no line break" follows from unchanged requirements (FMT-STRUCT braces, FMT-SPACE), and the result compiles. Reading "the hole untouched" would break those requirements without a compile reason. Recommend "only no line break". Unchanged requirements decide it, so nothing visible is in question.

## Issue goals

- "Below C# 11 … the output equals the input": change it to "below C# 11, no line break is written into a hole of a non-verbatim interpolated string, at any depth; every other requirement still applies inside the hole". The issue's own example already departs from "equals the input" (the blank line). No visible output beyond the corrected compile behavior is in question.
- "find the whole class … a test per construct that checks the output compiles under LangVersion 10": keep the class enumeration (done above). Change "checks the output compiles" to exact expected output (FC-017): the test projects do not compile semantically, and a compile check alone is weaker than the output.
- "the same concern likely applies to … the line-break and blank-line phases": narrow it to the line-break phase, plus the switch-case brace phase as unsettled. The blank-line phase cannot write into a one-line hole (trace evidence above).
- "A verbatim interpolated string allows newlines in a hole from C# 8 on": inaccurate but harmless. `$@"` allows them in every version (c15 compiles under C# 7.3); only the `@$"` order needs C# 8. No effect on the run.
- "From C# 11 on, the current breaking stays valid": confirmed (all outputs compile under C# 11). Keep.

## Architecture findings

**One rule stated in more than one place (FC-015).** RH5417's analyzer states "a non-verbatim interpolation, at any depth, below C# 11 holds no line break" in a private method, `IsInsideSingleLineInterpolation` (`src/Reihitsu.Analyzer/Rules/Layout/RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.cs:66-77`). The fix for this issue states the same predicate a second time in the formatter's line-break phase. The layout rules listed under *Follow-up candidates* need it a third and further times and lack it today. The input that shows the components disagreeing: `$"a {F(() => { return 1; })} b"` under C# 10. RH5417 correctly does not report it, while the formatter (today) and RH5301/RH5303/RH5206/RH5103/RH5008/RH5513/RH5104 (each on its own construct, c03, c04, c06, c10, c14, c24) act on it. `Reihitsu.Core` is referenced by both the analyzer and the formatter and already holds `StringInterpolationUtilities`. Whether to share it in this run is the lead's call. The run's own scope is the formatter plus the counterpart rule RH5417.

## Follow-up candidates

1. **Layout rules report what cannot be written in a non-verbatim hole below C# 11 (SHARED-DIAG-011).** Checked with a throwaway probe that runs every `DiagnosticAnalyzer` in `Reihitsu.Analyzer.dll` (Debug, built from 431caae) over the one-line c-fixtures parsed as C# 10, keeping only diagnostics located inside an interpolation. Reported inside the hole under C# 10:
   - RH5301 (c03, c21);
   - RH5303 (c04);
   - RH5206 (c06);
   - RH5103 (c10, c11, c18);
   - RH5008 (c10, c11, c18);
   - RH5513 (c14);
   - RH5104 (c24).

   Each asks for a line break or a blank line inside the hole, which CS8967 forbids below C# 11. RH5417 is correctly silent there (c01, c02, c16, c17, …), and so it reports c15 (`$@"`) only. RH5301, RH5303, and RH5206 have formatter-backed fixes, so in a C# 10 project their fix either writes non-compiling code (today) or, once the formatter is fixed, cannot change the code while the diagnostic stays (FC-007/FC-009). These are pre-existing analyzer defects against the unchanged SHARED-DIAG-011, outside this issue, and analyzer diagnostics are visible output, so they make one follow-up issue (the class, not seven issues). Content and order rules reported in the same holes (RH3007, RH3208, RH4125, RH5405) ask for changes that can be written on one line and are not part of this.
2. **`scripts/apply-fix.sh` and `scripts/trace.sh` cannot take a language version.** `FixtureRunner` hardcodes `LanguageVersion.Latest`, and `trace.sh` has no `--lang-version`. A version-dependent code-fix or phase question cannot be settled with the repository's own entry points. Tooling follow-up, low priority.
