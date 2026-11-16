Role: skeptic
Commit: 24aeea5
Verdict: proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the issue's own text; the issue's prescribed solution (thoenissen/Reihitsu#1672, `agent-filed`) is treated as a proposal below.

## Premise

**Reachability.** Both paths the issue names are real repository callers:

- Whole-document formatting (CLI, `FormatDocumentAsync`, `FormatSyntaxTree`) and every node-level entry point run `PrecheckPhase` first (`src/Reihitsu.Formatter/Pipeline/FormattingPipeline.cs:148`), which calls `PrecheckPhase.FindUnsupportedSyntax` on the whole input (`PrecheckPhase.cs:349`).
- About 30 code-fix providers call `ReihitsuFormatter.FindUnsupportedSyntax` (`ReihitsuFormatter.cs:469`, which delegates to the same predicate) while they register, on the node they would format and before any formatting. Examples: `FluentChainCodeFixHelper.IsFormattingRefused` (RH5112 and RH5201) asks about the statement that starts the chain's line, which is the whole `var q = ...;` local declaration statement for a chain in an initializer. RH5308 asks about the conditional's target, RH5302 about the logical expression's formatting node, and RH5303 and RH5305 about the initializer or collection.

**The claimed cost exists, but the issue attributes it to the wrong mechanism.** `FindCommentInDeclaration` (`PrecheckPhase.cs:239-269`) has two separate super-linear terms:

1. **Spine walk per token (removed by the issue's goal).** For every token inside an initializer value, lines 251-255 evaluate `values.Find(...)`, `value.GetFirstToken()`, and `value.GetLastToken()`. Roslyn's `GetFirstToken` and `GetLastToken` descend the value's leftmost or rightmost child path, so each call costs that path's depth, not O(1). The scan therefore costs O(tokens in the value × spine depth). The spine is deep for left-nested expressions (a `+` concatenation, `&&` or `||` chains, fluent call chains) and for right-nested ones (`?:` chains, `??` chains, nested lambdas). This term dominates in every adversarial measurement below.
2. **Nested-declaration re-walk (not removed by the issue's goal).** A declaration nested d levels deep inside lambda values is enumerated once for its own span and once for each enclosing declaration's span (`GetSpanTokens`, `PrecheckPhase.cs:287`/`313`), so the scan visits O(tokens × d) tokens. This is the mechanism the issue names. Computing the first and last token once per declaration does not change the number of visited tokens. It only removes term 1 from each visit, which cuts the nested-lambda case by about 2.5×. That matches "removes most of it" only loosely.

A third term is `values.Find`, which costs O(number of declarators) per token, so a declaration with n declarators costs O(n²). Computing the tokens once leaves this term in place: `many-declarators n=2000` takes 305 ms before and 80 ms after.

**Measurement.** The harness is outside the repository at `/tmp/claude-0/-home-user-Reihitsu/9bd33050-340c-58c2-9c2d-6557b4e466dc/scratchpad/bench/` (`Bench.csproj`, `Program.cs`, `Scan.cs`). It references the Release build of `Reihitsu.Formatter.dll` at HEAD 24aeea5, built with `scripts/build.sh --no-install`, and Microsoft.CodeAnalysis 5.12.0, the version the formatter uses. It compares the following:

- `precheck`: the real `ReihitsuFormatter.FindUnsupportedSyntax`.
- `declOrig`: a verbatim copy of `FindCommentInDeclaration` and `GetSpanTokens` run over every `VariableDeclarationSyntax`.
- `declCached`: the same copy with the issue's goal applied, the first and last token of each value computed once per declaration.
- `walk`: the bare `DescendantNodesAndSelf` walk.
- `format`: `FormatSyntaxTree`.

Times are best-of-N in ms on 4 cores. For every input and every corpus file, the harness asserts that `declOrig` and `declCached` return the same trivia. They agreed everywhere: 1912 corpus files plus all generated inputs.

```
export PATH=$HOME/.dotnet:$PATH
cd /tmp/claude-0/-home-user-Reihitsu/9bd33050-340c-58c2-9c2d-6557b4e466dc/scratchpad/bench
dotnet build -c Release
dotnet bin/Release/net11.0/Bench.dll adv                    # adversarial, precheck only
dotnet bin/Release/net11.0/Bench.dll fmt                    # moderate adversarial, with format time
dotnet bin/Release/net11.0/Bench.dll fmtbig                 # large adversarial, one format pass
TOP=6 FMTALL=1 dotnet bin/Release/net11.0/Bench.dll corpus  # every .cs under src/ of this repo
```

Realistic input. The corpus is all 1912 `.cs` files under `src/`, shipped and test code:

| | ms |
|---|---|
| precheck, all files | 515.9 |
| declaration scan before | 344.0 |
| declaration scan with the issue's goal | 309.5 |
| **gain** | **34.5** |
| FormatSyntaxTree, all files, one pass | 109 759 |

On real code, the gain is 0.03% of formatting time and 10% of the declaration scan. The largest single file (`RH3002…AnalyzerTests.cs`, 11 092 tokens) gains 0.27 ms and takes 4052 ms to format. On a synthetic but ordinary file of 1000 LINQ statements with 2-deep lambdas (56 021 tokens), the declaration scan drops from 41.3 to 17.3 ms. At 100 statements it drops from 4.3 to 1.7 ms, and formatting that file takes 1433 ms.

Adversarial input that a caller reaches. Every shape is a local declaration statement, so each is also the node that RH5201, RH5308, or RH5302 code-fix registration scans:

| input (generator in `Program.cs`) | tokens | declOrig | declCached | format, 1 pass |
|---|---|---|---|---|
| nested-lambda-locals d=50 | 783 | 5.3 | 2.1 | 656 |
| nested-lambda-locals d=200 | 3 033 | 93.1 | 36.7 | 48 241 |
| left-deep-concat n=500 | 1 016 | 15.0 | 0.14 | 529 |
| left-deep-concat n=2000 | 4 016 | 269.9 | 0.57 | 30 034 |
| left-deep-concat n=5000 | 10 016 | 1 772.9 | 1.5 | – |
| fluent-chain n=20 | 200 | 0.28 | 0.03 | – |
| fluent-chain n=200 | 1 820 | 22.4 | 0.28 | 1 251 |
| fluent-chain n=1000 | 9 020 | 559.9 | 1.5 | 182 568 |
| right-deep-ternary n=200 | 1 222 | 13.7 | 0.16 | 621 |
| right-deep-ternary n=1000 | 6 022 | 352.3 | 0.8 | 108 573 |

Shapes, as `dump` prints them at size 3. Each generator emits one more line per unit of size.

```csharp
var s = "x"
    + "p0"
    + "p1"
    + "p2";                       // left-deep-concat

var q = items
    .Where(x => x > 0)
    .Where(x => x > 1)
    .Where(x => x > 2)
    .ToList();                    // fluent-chain

var r = k == 0 ? 0
    : k == 1 ? 1
    : k == 2 ? 2
    : -1;                         // right-deep-ternary

var a0 = F(() => {
var a1 = F(() => {
var a2 = F(() => {
var z = 1;
return 0; });
return 0; });
return 0; });                     // nested-lambda-locals
```

**Severity.**

- **CLI and whole-document formatting.** The gain is negligible on every input. On real code it is 0.03%. On adversarial input the phases after the precheck cost 100 to 300 times more than the whole precheck: concatenation n=2000 takes 30 s to format against 0.27 s of declaration scan.
- **Code-fix registration.** Only the precheck runs on this path, so the scan is the whole cost. It is invisible at realistic sizes, for example 0.28 ms for a 20-link chain. It becomes noticeable only on adversarial sizes: 22 ms per registration at 200 links, 0.56 s at 1000 links, and 1.8 s for a 5000-term concatenation. With the issue's goal applied, all of these fall to 1.5 ms or less.
- **Conclusion.** The asymptotic gain is real: term 1 goes from quadratic to linear. It matters only for generated-looking, very long expressions in the code-fix path. The change is time-only and costs a few lines with no new type.
- **Close or proceed.** The issue says to close it "if the difference is negligible". On realistic code the difference is negligible. On adversarial code the issue's goal gives 100-1000× on the declaration scan. Because the change is cheap and touches no visible output, I recommend proceeding and justifying it with the adversarial code-fix-path numbers, not the nested-lambda framing.

Baseline: `scripts/test.sh --no-install --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Precheck"` returned `Passed! - Failed: 0, Passed: 235` at HEAD.

## Mechanism and candidates

This is not a correctness bug, so there is no failure mechanism in the usual sense. The cost mechanism: for every token inside an initializer value, the scan re-derives that value's first and last token by descending the value's spine, so a value of t tokens whose leftmost or rightmost spine has depth s costs O(t·s).

Dispatch: `FindUnsupportedSyntax` switches on `VariableDeclarationSyntax`. `GetSpanTokens` switches on the declaration's parent:

- `BaseFieldDeclarationSyntax`, covering `FieldDeclarationSyntax` and `EventFieldDeclarationSyntax`;
- `LocalDeclarationStatementSyntax`, including `using` and `const` declarations;
- `ForStatementSyntax`;
- `UsingStatementSyntax`;
- `FixedStatementSyntax`;
- any other parent, or the input root, which takes the declaration's own tokens.

Every one of these candidates reaches the value-token loop and carries the cost whenever it has an initializer value with a deep spine. Values of a single token or with a shallow spine carry none.

## Breaking inputs

The change must not alter any verdict. The relevant catalog classes for the precheck:

- **FC-039 (reaches the change).** A declaration nested inside a value is still checked with its own span, and an outer declaration's value data must not apply to an inner declaration. Covered today by `SpecExampleDeclarationInsideAValueIsCheckedOnItsOwn`.
- **FC-041 (reaches the change).** Locality: the `isInputRoot` handling and the input-root span stay as they are. Covered by `UnsupportedSyntaxEntryPointTests`.
- **FC-040.** Not affected. Trivia classification (`SyntaxTriviaUtilities.IsCommentTrivia`) is untouched.
- **FC-038, FC-042, FC-043.** Not involved: no phase, guard, or code-fix target changes.

**Coverage gap the test implementer should close.** No existing test puts a comment at an edge of an initializer value that is not the declaration's first value and has several tokens. A lookup that pairs a token with the wrong value's first or last token would pass all 68 `UnsupportedVariableDeclarationTests` today. The verdicts below come from the real precheck at HEAD, run with `printf '<input>' | dotnet bin/Release/net11.0/Bench.dll check`, each as a method body inside `class C { void M() { ... } }`:

| input | verdict at HEAD |
|---|---|
| `int a = x + y, b = p + q /* c */;` | unsupported, FMT-UNSUP-004 reason, at `/* c */` (line 5, col 34) |
| `int a = x + y, b =⏎    // c⏎    p + q;` | unsupported, FMT-UNSUP-004 reason, at `// c` (line 6, col 13) |
| `int a = x + y, b = p /* c */ + q;` | supported |
| `int a = x + y, b = p + /* c */ q;` | supported |

Line and column are 1-based here; the tool printed them 0-based as `4,33` and `5,12`. These are characterization tests: they pass today and must still pass after the change. No test can assert the timing itself; the corpus and adversarial equivalence check above is the evidence that no verdict changes.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `PrecheckPhase.FindCommentInDeclaration` | Must change if the run proceeds; the issue's only target |
| `PrecheckPhase.GetSpanTokens`, `FindUnsupportedSyntax` (candidate order, reasons, locations), `FindTriviaInFrontOfComma`, `FindTriviaBehindConditionalOperator` | Must stay |
| `ReihitsuFormatter.FindUnsupportedSyntax` | Must stay; FMT-UNSUP-007 requires it to call the same predicate |
| Specs (`sdd/specs/formatter/unsupported-syntax.md`) | Not involved; no requirement states time, and FMT-UNSUP-004's verdicts are unchanged |
| Code fixes, analyzers, Fix All, `Reihitsu.Core` | Not involved |
| Rule docs, CLI | Not involved |
| Tests | Existing precheck tests must pass unchanged; add the four characterization inputs above |

## Refuse option

The refuse option is not applicable: the change refuses no input class and changes no output. Declaring a shape unsupported is not an option either, since no shape is involved.

The simpler alternative is **no change**, closing the issue as not worth it. It costs nothing and leaves the quadratic term in place. That term is invisible on realistic code (34.5 ms over 1912 files) and reaches 0.5-1.8 s per code-fix registration only on 1000-plus-link chains or concatenations. The requested change is a few production lines with no new type and no new mechanism. Either outcome is defensible. I recommend proceeding because the cost is small and the numbers above justify it.

## Output questions

_None._ No visible output differs. The harness found the same verdicts before and after on all 1912 corpus files and every generated input.

## Internal choices

1. **Whether to also remove the nested-declaration re-walk (term 2).** Removing it would need a single pass that assigns each token to its innermost declaration, a restructuring well beyond "compute once". After the issue's goal, nesting costs 2.1 ms at d=50 and 36.7 ms at d=200, and real code nests 2 to 3 deep. Recommend: do not; keep the change to the issue's goal.
2. **Whether to also remove the per-token `values.Find` (term 3).** It matters only with thousands of declarators in one declaration (n=2000: 80 ms after the issue's goal). Recommend: not in this run unless it comes for free.
3. **Justification in the PR.** Recommend citing the code-fix-registration path and the deep-spine numbers (for example a 1000-link chain going from 560 ms to 1.5 ms per precheck), and stating that the gain on real code is negligible (34.5 ms over the repository's 1912 files, against 110 s of formatting). The issue's nested-lambda O(tokens × depth) framing describes a term this change does not remove.

## Issue goals

- **"Computing the first and last token of each value once per declaration removes most of it."** This is overstated for the mechanism the issue names. For nested lambdas the change cuts about 60%, and the O(tokens × depth) visit count stays. It removes almost all of the cost for deep expression spines, a mechanism the issue does not name. Change the justification only; no visible output is involved.
- **"Measure before and after on a large file with deeply nested lambdas."** That input is the weakest case for the change (2.5×). Measure on deep expression spines and on the repository corpus instead, as above. No visible output is involved.
- **"Existing precheck tests pass unchanged."** Keep. They pass at HEAD (235 tests).
- **"Or the issue is closed as not worth it."** This is the alternative under *Refuse option*; the lead decides.

## Architecture findings

_None._ Only one component, the precheck's declaration scan, is involved.

## Follow-up candidates

1. **`PrecheckPhase.FindTriviaInFrontOfComma` is quadratic in the length of a separated list.** This method is outside the issue, in the same phase. `comma.GetPreviousToken()` (line 122) and `previousToken.GetNextToken(...)` (line 170) each cost O(list length) inside Roslyn for a comma in a long separated list. A single-line array initializer of n elements (`static readonly int[] A = new[] { 0, 1, ... };`, `WideArray` in the harness) gives:

   | n | GetPreviousToken over all commas | plus GetNextToken | whole precheck | format, 1 pass |
   |---|---|---|---|---|
   | 1 000 | 24.8 ms | 43.3 ms | 40-62 ms | 1 439 ms |
   | 5 000 | 299.6 ms | 519.6 ms | 429-521 ms | 8 549 ms |
   | 20 000 | 4 676 ms | 8 087 ms | 7.3-14.4 s | 144 129 ms |

   The precheck share is about 5% on the formatting path. On the code-fix registration path it is the whole cost, for example for RH5303 or RH5305 on a large data-table initializer. It is time-only with no visible output. It could be an adjacent fix if the lead finds it small, since it is the same component and phase. Otherwise it is an `agent-filed` follow-up.
2. **The formatter phases themselves are super-linear on long expressions and lists.** These are the numbers from the `format` columns above:
   - fluent chain of 1000 links: 183 s for one pass;
   - `?:` chain of 1000 branches: 109 s;
   - concatenation of 2000 terms: 30 s;
   - array of 20 000 elements: 144 s.

   The phase that dominates was not traced; that is outside this run. This is a candidate performance follow-up for the CLI path. It dwarfs anything in the precheck.
