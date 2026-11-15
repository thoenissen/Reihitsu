Role: skeptic
Commit: 87f9643
Verdict: #1764: output question (restated; two of the spec author's options are excluded by SHARED-DIAG-011); #1765: proceed

How facts were established (all fixtures in the scratchpad, outside the repository, deleted afterwards):

- `scripts/apply-fix.sh RH7105|RH5011|RH5010|RH5012|RH7501|RH5009|RH5024 <dir>` (parses at `LanguageVersion.Latest`, which is C# 15 with Roslyn 5.12; it flags syntax errors in a fix result, not semantic errors).
- Compile errors: a scratch `net11.0` class library built with SDK 11.0.100, `LangVersion` 15 and then `preview`, `AllowUnsafeBlocks`.
- Which keywords Roslyn 5.12 parses into `MemberDeclarationSyntax.Modifiers`: a scratch file-based app against `Microsoft.CodeAnalysis.CSharp` 5.12.0 that parsed `public <kw> void M() {}` (and class/field/property variants) for every keyword kind at `LanguageVersion.Preview`.
- Formatter: `scripts/format.sh <dir>` twice on a labeled-`break`/`continue` fixture, and once on the same fixture with the labels removed.

The prompt carried no intended fix or verdict. The issues carry suspected causes and prescribed solutions (#1764: explicit ranks; #1765: `GetNextToken`); they were not taken as given — each claim below was checked independently.

## thoenissen/Reihitsu#1764

### Premise

Real, and reached by a caller: the RH7105 analyzer runs on any C# 15 project (`closed` needs no preview; `safe` needs `LangVersion preview`), and the code fix is offered in the IDE and by Fix All.

- `GetRh7105Rank` (`src/Reihitsu.Core/ModifierOrderingUtilities.cs`, the switch at the end) has no arm for `ClosedKeyword` or `SafeKeyword`; both fall to rank 100, behind `ref` (13) and `partial` (14).
- `apply-fix.sh RH7105` observed results:
  - `public closed partial class Gate` -> reported, fix writes `public partial closed class Gate`;
  - `public new closed partial class N` (nested) -> fix writes `public new partial closed class N`;
  - `internal closed unsafe partial class U` -> fix writes `internal unsafe partial closed class U`;
  - `public closed partial record R;` -> fix writes `public partial closed record R;`;
  - `public safe ref struct S1` -> fix writes `public ref safe struct S1`;
  - `closed public class Gate` -> fix writes `public closed class Gate` (correct);
  - `public closed class Gate`, `public closed record class R;` -> no diagnostic (correct).
- Compiler (C# 15 / preview): every `partial closed ...` and `partial safe ...` order above is CS0267; `public ref safe struct` is CS1585 ("Member modifier 'ref' must precede the member type and name"). The reverse orders (`closed partial`, `safe partial`, `safe ref`, `closed unsafe partial`, `unsafe closed partial`, `closed new partial`, `new closed partial`) all compile. The tooling marked these fixes as `fixed`, not `invalid-result`, because CS0267 and CS1585 are not syntax-tree errors; a verifier test that compiles the fixed code catches them.
- So today both the report and the fix break shared requirements, independently of where `closed`/`safe` should stand:
  - SHARED-DIAG-011: RH7105 reports `public closed partial class` asking for `public partial closed class`, a change that cannot be written at that position. The same holds for every declaration that holds `closed` or `safe` together with `partial` or `ref`.
  - SHARED-FIX-003: the fix produces a compile error the code did not have.
- The issue says the diagnostic is reported "at `public`". By RH7105-003 and `TryGetRh7105Violation`, it is on `closed`, the first modifier that differs from the canonical order. Not observed directly (the tooling prints no location); the test will settle it.
- The issue understates the class: `safe` breaks the fix with `ref` too (`public safe ref struct`), not only with `partial`.

### Mechanism and candidates

Mechanism: a modifier that `GetRh7105Rank` does not name sorts behind `ref` and `partial`, which the grammar requires directly in front of the type keyword or return type, so every declaration that holds an unranked modifier together with `ref` or `partial` is reported and fixed toward an order that does not compile.

Dispatch: `GetRh7105Rank`'s `switch` over `SyntaxKind`. Candidates are the keywords Roslyn 5.12 parses as member modifiers that have no arm. The probe found `const`, `fixed`, `scoped`, `closed`, and `safe`, and no other (`union` and `extension` are not modifiers).

| Unranked modifier | With `partial` | With `ref` (type modifier) | Status |
|---|---|---|---|
| `const` | cannot combine | cannot combine | holds |
| `fixed` | cannot combine | cannot combine | holds |
| `scoped` | — | — | CS0106 on every member: the input does not compile, not a failing member |
| `closed` (class, record class) | fails: reported (SHARED-DIAG-011) + fix CS0267 | unreachable: `closed` is CS0106 on struct and record struct | `closed` without `partial` holds |
| `safe` (methods, fields, properties, events, constructors, indexers, operators, delegates, classes, structs) | fails: reported + fix CS0267 (compiled for a partial method; other partial member kinds follow the same grammar rule, not compiled one by one) | fails: `public safe ref struct` -> `public ref safe struct`, CS1585 | `safe` without `partial`/`ref` produces compilable code today in every combination compiled (`static extern safe`, `required safe`, `override safe`, `async safe`, `readonly safe`, `new safe`) |

Compiler facts that shape the candidates (verified by compiling):
- `closed` + `abstract` is CS9384; `closed` + `sealed` or `static` is CS9381.
- `safe` + `unsafe` is CS9388; `safe const` is CS0106.
- `closed` combines with an accessibility, `file`, `new`, `unsafe`, and `partial`; `new` and `unsafe` compile on either side of it.
- `safe` combines with everything else tried.

### Breaking inputs

Failure classes:
- FC-006: modifier order, language-version gate.
- FC-024: `partial` types and members.
- FC-016: the sibling hazard. `ref` fails like `partial`.
- FC-033: a test of `safe` needs `LanguageVersion.Preview`. `RH1001…Tests.ApplyPreviewLanguageVersionToTestProject` shows the pattern, and a spec example with `safe` cannot be checked at `Latest`. The fixed code must be compiled by the verifier, or CS0267 and CS1585 go unnoticed, as they did in `apply-fix.sh`.

The test implementer must cover:
- `public closed partial class`, `public closed partial record`, `public new closed partial class` (nested), `internal closed unsafe partial class`;
- `public safe partial void M();` with its implementing part, `public safe ref struct`, `public safe readonly ref partial struct`;
- the members that hold today and must keep holding: `closed public class` -> `public closed class`, `public closed class` and `public closed record class` not reported, and `const`/`fixed` behind every other modifier;
- RH2001's fix, a property -> `readonly` field with `safe`, e.g. `private safe int P { get; }`. Its output is `private readonly safe int _p;` today and stays so under every option below, because every option puts `safe` behind `readonly`. That is a reason for a regression test, not a change.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Reihitsu.Core` `ModifierOrderingUtilities.GetRh7105Rank` | must change (where `closed`/`safe`, or every unranked modifier, sort) |
| RH7105 analyzer | must stay (its verdict follows the rank) |
| RH7105 code fix / Fix All | must stay (writes `OrderModifiersForRh7105`) |
| RH2001 code fix (consumer of `OrderModifiersForRh7105`) | must stay; visible output unchanged under every option |
| RH7106 | not involved (orders only `protected`/`internal`/`private`) |
| `sdd/specs/analyzer/RH7105.md` RH7105-001 | must change under every option the maintainer can pick (see *Output questions*) |
| Rule page `documentation/rules/RH7105.md` | under A: not involved, because line 12 already says "`ref` comes after every other modifier, and `partial` is last", which is A; under B: the order list on line 22 would have to name `closed`/`safe` at their new places |
| Formatter | not involved (no phase orders modifiers) |
| Tests: Core `ModifierOrderingUtilitiesTests`, RH7105 analyzer/fix tests, RH2001 | must change (new tests) |

### Refuse option

- **Refuse option:** for a declaration that holds an unranked modifier together with `partial` or `ref`, neither report nor fix. Withholding only the fix is not enough: SHARED-DIAG-011 forbids the report too.
  - Cost: a guard in `TryGetRh7105Violation` plus a fix-side check, a few lines.
  - What it gives away: a gap. `unsafe public closed partial class` would never be reported, while `unsafe public closed class` is.
  - Rarity: the class is not rare. `closed` is the C# 15 closed-hierarchy modifier, and `partial` is common on such types (source generators).
- **Option A of both questions below:** move the default rank in front of `ref`/`partial`. It is one changed number, cheaper than the refuse option, and leaves no gap.
- **Option B:** two new ranks. Also a few lines.
- **Declaring a shape unsupported:** not an option. Modifier order is decided by no formatter phase, the precheck has nothing to recognize here, and the shape would delete no special case.

### Output questions

The spec author's 1A and 2A ("reported as today, no fix") are not options. They keep reporting `public closed partial class` and `public safe partial void P();`, whose requested order cannot be written, which SHARED-DIAG-011 forbids. Whatever the answer, `public closed partial class A` and `public safe partial void P();` are not reported. An unchanged requirement demands that.

The spec author's 1C is the rule "every modifier RH7105 does not name stands in front of `ref` and `partial`". RH7105-001's own `Why:` reads that way ("`ref` and `partial` come last because C# requires them right before the type keyword"), and so do the remark on `GetRh7105Rank` and the shipped rule page (`documentation/rules/RH7105.md:12`: "`ref` comes after every other modifier, and `partial` is last"). Applied to `safe`, that rule gives an option the spec author did not list (A in question 2). It changes no output that is correct today: only declarations that hold `closed`/`safe` together with `partial`/`ref` change, and all of them are broken today. Restated:

**1 · RH7105 · Declaration keywords must follow order — where `closed` stands**

Input:
```csharp
public closed partial class A
{
    public new closed partial class N
    {
    }
}

internal closed unsafe partial class U
{
}
```

Before (`main`): `closed` is reported on `A`, `N`, and `U`, and the fix writes `public partial closed class A`, `public new partial closed class N`, `internal unsafe partial closed class U`; none of them compiles.

After, A (recommended) — `closed` stands directly in front of `partial`, like every modifier the order does not name: `A` and `N` are not reported; `U` is reported on `closed` and the fix writes:
```csharp
internal unsafe closed partial class U
{
}
```

After, B — `closed` takes the place of `abstract`/`sealed`: `A` and `U` are not reported; `N` is reported on `new` and the fix writes:
```csharp
public closed new partial class N
{
}
```

Why A: the order is the one written today with only the impossible part removed. `closed` stays behind every named modifier, as the order puts every unnamed one, and in front of `partial`, as C# requires. A later modifier needs no new decision.

**2 · RH7105 · Declaration keywords must follow order — where the preview `safe` stands**

Input:
```csharp
public partial class C
{
    public static safe extern void M();

    public safe partial void P();

    public safe partial void P()
    {
    }

    public safe required int R { get; set; }
}

public safe ref struct S
{
}
```

Before (`main`): `safe` is reported on `M`, both parts of `P`, `R`, and `S`. The fix writes:
- `public static extern safe void M();`
- `public partial safe void P();` (does not compile)
- `public required safe int R { get; set; }`
- `public ref safe struct S` (does not compile)

After, A (recommended) — `safe` stands directly in front of `ref` and `partial`, like every modifier the order does not name: `P` and `S` are not reported; `M` and `R` are reported and fixed as today:
```csharp
public static extern safe void M();

public required safe int R { get; set; }
```

After, B — `safe` takes the place of `unsafe`, behind `extern` and in front of `required`: `P`, `R`, and `S` are not reported; `M` is reported and fixed to `public static extern safe void M();`.

After, C — `safe` stands directly in front of `extern`: none of `M`, `P`, `R`, `S` is reported.

Why A: it changes only the output that does not compile today, and fixes no position for a preview keyword whose rules can still change before release. The answer to question 1 normally carries over: A in both is one rule.

### Internal choices

- Under answer A, the spec can state the rule for unnamed modifiers generically, as "every other modifier in front of `ref` and `partial`", and name `closed` and `safe` only as examples. Alternatively it names them in the list. Recommendation: generic, so a later modifier is already covered. The issue's "Done when" item "the spec names the new modifiers" is then met by the example.
- Whether a change doc is required: RH7105-001's order list changes under every answer, which `sdd/README.md` (*When a change doc is required*) decides; the lead's call. Under A, every visible change is in a case that is broken today.

### Issue goals

- "`closed` (and `safe`) have explicit ranks": this is a prescribed solution. Under A, no explicit rank is needed; the default position moves. Drop it or replace it with "they never sort behind `ref`/`partial`". No visible change by itself.
- "The fix never moves a modifier behind `partial`": keep, and widen it to `ref` (`public safe ref struct`).
- "A reproduction test shows today's false positive": keep, and add the broken fix result, which the verifier must compile.
- "`RH7105.md` names the new modifiers": keep, as an example under the generic rule (internal).
- The issue's "Open output question": stands, restated above, without the options SHARED-DIAG-011 excludes.

### Architecture findings

One rule is stated in four places that drift apart:
- RH7105-001's order list says "every other modifier last".
- Its `Why:` says `ref` and `partial` come last because the grammar requires it.
- The shipped rule page (`documentation/rules/RH7105.md:12`) says `ref` comes after every other modifier and `partial` is last.
- The remark on `GetRh7105Rank` says the default rank is correct only while no unranked modifier can combine with `ref`/`partial`.

`closed` and `safe` are the inputs where the order list disagrees with the other three. Answer A makes the order list say what the `Why:`, the remark, and the rule page say. Answer B keeps the default rank behind `ref`/`partial`, so the next unranked modifier repeats this issue. This is within the run's issue.

### Follow-up candidates

_None._ thoenissen/Reihitsu#1338, about other RH7105 modifier lists, is already filed. Local functions, which it covers, can also carry `safe`. Only that issue's run decides it.

## thoenissen/Reihitsu#1765

### Premise

Real, worse than reported, and reached by a caller: the RH5011 analyzer (build/IDE) and its code fix (IDE, Fix All) run on C# 15 code with a labeled `break`.

`apply-fix.sh RH5011` on labeled `break` fixtures gave **not-converged (10 iterations) on every fixture, under LF and CRLF**:
- `break outer;` before a block's `}`: the fix adds one blank line in front of `}` per application, 10 after 10 iterations;
- before `else` (`if (a > 0) break outer; else a++;`): splits the line, then keeps adding blank lines in front of `else`;
- before the `while` of a `do`;
- blank line + statement;
- statement directly behind;
- `sw: switch` with `break sw;` before the next section;
- the formatter's own output of a labeled-`break` fixture.

So:
- Every labeled `break` is reported, whatever follows it, against RH5011-002 and RH5011-008 (closing brace, directive, `else`, `do` footer) and RH5011-001 (blank line already present).
- Where the report is right (statement or section directly behind), the fix never silences it. That breaks SHARED-FIX-001 (FC-007).
- The formatter's output is reported by a layout rule whose fix inserts blank lines. That breaks SHARED-DIAG-010 (FC-013).

The issue only claims the false positive in front of `}`.

### Mechanism and candidates

Mechanism: the analyzer finds "the token behind the `break` statement" by stepping one token past the `break` keyword and skipping a single `;`. For a statement whose keyword is followed by anything other than its semicolon, it yields a token inside the statement, the label. The blank-line measure (`TokenGapAnalysis.Between(';', label)`) then counts no blank line, and the exemptions for `}`, `else`, `do`-`while`, and directives never see the real next token.

Dispatch: `StatementShouldBeFollowedByABlankLineAnalyzerBase<TStatement>` has exactly one subclass, `RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer`, the only `GetNextToken` override. `rg "Keyword\.GetNextToken\("` over production code finds no other keyword-relative "next token" derivation.

Candidates and status:
- RH5011 analyzer: fails, on every labeled `break`.
- RH5011 code fix: holds. It derives the gap from `breakStatement.SemicolonToken`. Its non-convergence is the analyzer's re-report.
- RH5010 and RH5012 (`StatementShouldBePrecededByABlankLineAnalyzerBase`, `Keyword.GetPreviousToken()`; a label stands behind the keyword): hold. `apply-fix.sh` showed no diagnostic on the formatter output, and fixed raw input in 1 action / 2 iterations.
- RH5010's exemption, `BlankLineSpacingPolicy.IsTerminalDirectSwitchSectionBreak` (type-based): holds.
- RH7501 analyzer and fix: hold. `break outer;` and `break inner;` (the switch's own label) are reported and moved behind the block with the label kept; `continue outer;` is not reported.
- RH5009 (`goto` only): not involved, no diagnostic on any fixture.
- RH5024: no diagnostic on any fixture.
- Formatter: holds. Two passes are idempotent, and its output equals the output for the same input with the labels removed, character for character after removing the labels: `SwitchCaseBraceRewriter`, `BlankLineBreakSpacingRewriter`, and `BlankLineSpacingPolicy` are type-based.

### Breaking inputs

Failure classes:
- FC-007 (convergence)
- FC-013 (formatter output reported)
- FC-016 (sibling rules)
- FC-002: "the token behind the statement" is the qualifier that never became the predicate's condition.

RH5011 class matrix. Axis: what follows the labeled `break` statement. Every member fails today, either as a false positive with a non-converging fix, or as a correct report with a non-converging fix:

| Follows `break outer;` | Requirement | Today |
|---|---|---|
| `}` of a block | RH5011-002 | false positive; fix adds blank lines in front of `}` |
| `}` of a switch | RH5011-002 | false positive |
| directive line | RH5011-002 | false positive |
| `else` (same line and next line) | RH5011-008 | false positive; fix splits the line, then adds blank lines |
| `while` of `do` | RH5011-008 | false positive |
| blank line + statement | RH5011-001 | false positive |
| statement on the next line | RH5011-001 | reported (right), fix does not converge |
| statement on the same line | RH5011-001/-004 | reported (right), fix does not converge (by the same analyzer re-report; the same-line branch of the fix was not observed separately) |
| next switch section (`break sw;`, the switch's own label) | RH5011-001 | reported (right), fix does not converge |

The test implementer must cover:
- every row above;
- the code fix to convergence for the three "reported" rows, under LF and CRLF;
- the issue's own input (nested `foreach`, `break outer;` before `}`), not only a simplified cousin (FC-034).

Analyzer tests run at `LanguageVersion.Latest` (C# 15), so labeled `break` parses without a transform.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5011 analyzer (`GetNextToken`) | must change |
| `StatementShouldBeFollowedByABlankLineAnalyzerBase` | not involved unless the analyzer change is placed there (internal) |
| RH5011 code fix / Fix All | must stay (converges once the analyzer measures from the semicolon); gets tests |
| RH5010, RH5012, RH7501 analyzers and fixes | must stay; get tests (spec examples exist) |
| RH5009 | not involved |
| `Reihitsu.Core` `BlankLineSpacingPolicy` | not involved |
| Formatter | not involved (verified, see above) |
| Rule pages | not involved |
| Specs | the spec author's clarifications in RH5010, RH5011, and RH7501 match every observed result (the RH5010 and RH7501 examples were reproduced by `apply-fix.sh`); no further change needed |

### Refuse option

- **Refuse option:** do not report RH5011 for a labeled `break`. That leaves `break outer; a++;` unreported, against RH5011-001, which demands the report. That is a visible change, for a construct that C# 15 code will commonly write. It costs about as much as the correction itself, a change of a few lines that measures from the statement's semicolon, so refusing buys nothing.
- **Declaring a shape unsupported:** not an option. The formatter already handles the construct, and no special case would be deleted.
- **Simpler options:** _None._

### Output questions

_None._ RH5011-001, -002, and -008, unchanged in substance, demand every corrected output. The spec author's labeled sentences only clarify them. I considered whether a labeled `break` ending a switch section should count as the terminal `break` (RH5010-002, RH7501-001, FMT-SWITCH-002). The formatter, RH5010, and RH7501 all treat it so today, consistently. The resulting output (`case 1: { a++; } break outer;`) is the formatter's existing shape, and I would not call it wrong. Not raised.

### Internal choices

Where the corrected "next token" is derived:
- in the RH5011 override, from the statement's semicolon, as the code fix already does;
- or in the base class, from `statement.GetLastToken()`, which makes the override unnecessary.

Both give the same output for every row above. Recommendation: the base class, so analyzer and fix both derive the gap from the statement's end. The implementer and lead decide.

### Issue goals

- "Fix RH5011 for labeled `break`": keep.
- "Check RH5009 … add tests": drop. RH5009 judges only `goto` and never sees a `break` or `continue`, so a test there asserts nothing about labels. No visible change.
- "RH5012 and the other blank-line rules that look at `continue`": keep RH5012. No other rule looks at `continue` (`rg` over production code).
- "RH5010, RH7501 tests": keep.
- "The formatter needs no change": confirmed by running it.
- "Done when: a reproduction test shows the false positive": widen it to cover the non-converging fix (FC-007) and a formatted labeled-`break` input (FC-013). The issue's single example misses both.

### Architecture findings

The RH5011 analyzer and its code fix own the same decision, "the token behind the `break` statement", and compute it differently:
- the analyzer: `BreakKeyword.GetNextToken()`, skipping one `;`;
- the fix: `SemicolonToken.GetNextToken(includeZeroWidth: true, includeSkipped: true)`.

`break outer;` in front of `}` shows them disagreeing: the analyzer stops at `outer`, the fix at `}`. This is the run's own defect, and small. Making both derive the gap from the statement's semicolon removes the second computation (see *Internal choices*). The lead decides.

### Follow-up candidates

_None._ RH7501 reports the formatter's output when a section's existing block ends in `break` (labeled or not, identical). FMT-SWITCH-009 and the switch-case-braces non-goals decide this explicitly: the formatter keeps an existing block, and the `break` inside it is RH7501's concern. So it is no defect.
