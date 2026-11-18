Role: skeptic
Commit: c3e32ed (reviewed diff 28dda88..476ec37)
Verdict: output question

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the review points (is the shape stated by its cause, is the check cheap, are the deleted and kept lists right, are the withheld fixes complete, does it refuse common code), and I took those as the scope.

Summary:

- The declaration is sound. Both code items under *Deleted* really are deleted, and every *Checked and kept* item is really needed.
- I found no commonly written code that it refuses.
- **One conflict the spec diff misses (finding 1).** FMT-SPACE-028's own example holds the shape on one line. That makes the pending question (a), about block comments on the same line, a question with a concrete cost.
- **One gap in the withheld-fix list (finding 2).** RH5112's fix, which is offered on `main`, is withheld after the change.

## Premise

This handover reviews a spec amendment the maintainer asked for (question 6: 6.1A, 6.2A, 6.3A), not the issue's premise. The issue's premise was settled in 02-skeptic.

Reachability: the CLI and whole-document formatting reach the shape, and so does every code fix whose formatted node holds it. Change 6 and change 7 show those paths, and the RH5112 fixture below shows one more.

### Do the deleted special cases exist, and does the declaration really delete them?

**1. `ArgumentListLineStartRewriter.CanJoin`'s `WouldJoinAcrossUnjoinableTrivia` term** (`src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ArgumentListLineStartRewriter.cs:99-103`).

- The term tests the trailing trivia of the token in front and the `(`'s leading trivia, for comments, directives, and disabled text (`SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia`, `src/Reihitsu.Core/SyntaxTriviaUtilities.cs:229`).
- `IsArgumentListOpenParenthesis` admits exactly the six parents that FMT-UNSUP-014's first bullet lists.
- `CanJoin` joins only when `root.FullSpan.Contains(previousToken.Span)`. That is the same boundary FMT-UNSUP-014 draws for a node-level input that holds only one side of the gap (FMT-UNSUP-003).
- `LineBreakTriviaUtilities.JoinGaps` (`LineBreakTriviaUtilities.cs:251`) has no trivia guard of its own. Once the term is deleted, the precheck is the only protection. The two tests are equal, so that holds, but the implementer must not narrow the precheck's parent list without narrowing `IsArgumentListOpenParenthesis` with it.
- None of the phases that run before the line-break phase writes trivia into this gap:
  - the structural transforms in `StructuralTransformPhase.CreateRewriters`: braces, expression bodies, accessor bodies, trailing commas, the field split, and attribute-list shape;
  - region formatting;
  - documentation-comment relocation;
  - using ordering.

  No transform creates an invocation, element access, or `!` either. A grep of the code fixes found none that builds such a part.

The term is really deleted.

**2. `MethodChainAlignmentContributor`'s `"MethodChainAttachedPart"` column** (`src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/MethodChainAlignmentContributor.cs:170-176`).

`FluentChain.Collect` (`src/Reihitsu.Core/FluentChain.cs:359-431`) produces four kinds of attached part, and each first token falls in a gap that FMT-BREAK-039 or FMT-BREAK-040 joins:

| Part kind | First token | Owner of its gap |
|---|---|---|
| `ArgumentList` | an invocation's `(` | FMT-BREAK-039 |
| `ElementAccess` | `[` | FMT-BREAK-040 |
| `NullForgiving` | `!` not followed by `.` or `?` | FMT-BREAK-040 |
| `ConditionalElementAccess` | `!` or `?` | FMT-BREAK-040 |

A `!` followed by `?.` becomes part of the link operator `!?.`. A `!` followed by `?[` is the first token of the conditional element access.

The token in front of a part always lies inside the chain, so inside the root, and the root-boundary exception never applies. The joins also reach interpolation holes. Fixture `H.cs`, `scripts/format.sh` on `main`:

```csharp
var s = $"{items.Where(x => x > 0)
    .ToArray()
    [0]}";
var t = $"{Foo
    (1)}";
```

gives

```csharp
var s = $"{items.Where(x => x > 0)
                .ToArray()[0]}";
var t = $"{Foo(1)}";
```

With the declaration in place, no supported input reaches the column. This holds only if the new FMT-BREAK-040 owner joins in every position, as the spec requires. If it skips a position, the deleted column comes back there as a part at an unowned column, so the test implementer should lock parts in a chain inside an interpolation hole, a lambda body, and a switch arm.

**3. "The refusal the join of FMT-BREAK-040 would need".** This is code that will not be written, not code that is deleted. The wording is accurate, and I recommend no change.

### Checked and kept

All the items are needed: `CloseGap` (member names, link operators), `ListElementInteriorJoinRewriter.CanJoin`, the parameter-list joins, the Core chain checks (analyzers), and the code fixes.

One fact for question (b) concerns `AlignRestOfRun(attachedPart.Tokens, model)`, called at lines 154 and 178:

- The only attached part with more than one token is a conditional element access (`!`?, `?`, `[`).
- Its `!`–`?` gap is already in the shape, so the call does work only for a `[` that the `?`–`[` gap keeps on its own line.

## Mechanism and candidates

_N/A_ (behavior change). The dispatch facts are under *Premise*.

## Breaking inputs

**Common code the precheck would refuse.** I found none.

- **Ternary with collection-expression branches.** Collection-expression branches of a conditional expression are not conditional accesses. Fixture `T.cs`, `scripts/format.sh` on `main`:

  ```csharp
  int[] x = condition
      ? [1]
      : [2];
  var y = condition ? [3] : a;
  int z = n
      ? [0] ?? 0;
  ```

  gives

  ```csharp
  int[] x = condition
                ? [1]
                : [2];
  var y = condition ? [3] : a;
  int z = n?[0] ?? 0;
  ```

  `JoinConditionalElementAccesses` joins only the real conditional access. So `condition // c⏎? [1]⏎: [2]` is a conditional expression and is not refused.
- **Repository source.** `rg -U` over the non-test source under `src/` found no comment, directive, or disabled text in front of an argument list's `(`, a `[`, a `?[`, or an attached `!`.
- **Sibling tokens.** The tokens next to the shape stay outside it, by parent kind:
  - `if`, `while`, `for`, `fixed`, `using`, `lock`, `switch`, `await`, `throw`, and a cast are followed by a parenthesized expression or a statement's `(`, not an argument list;
  - `typeof` and `default` are other expression kinds with their own `(`;
  - `delegate`, lambdas, `union B1 /* c */ (int, string)`, and attributes have parameter or attribute argument lists;
  - collection expressions, list patterns, and implicit element accesses have their own `[`.

  Exactly one shape in this group is refused: `nameof` parses as an invocation, so `nameof /* c */ (X)` is refused. That is rare.

**Inputs the test implementer must cover, beyond 04-spec-author's list:**

- **The FMT-UNSUP-014 examples, checked on `main`.** The refused example is formatted on `main` today (`[0]` and `?[0]` move to the statement's column, and a blank line is inserted above `// c3`), so it is a real output change. The "other side" example is formatted, and its second pass is stable ("Formatted 0 of 1 file(s)").
- **RH5112 fix withheld.** Fixture `A.cs`, `scripts/apply-fix.sh RH5112` on `main`, LF and CRLF:

  ```csharp
  internal sealed class Example
  {
      private static string Run(int[] items)
      {
          return items // element
              ?[0]
              .ToString();
      }
  }
  ```

  On `main` the fix is offered and gives `return items // element⏎               ?[0].ToString();`. In that output, `?[0]` gets the root column from the deleted `MethodChainAttachedPart` code. After the change, the chain the fix formats holds the shape, so the fix is withheld.

  The existing test `RH5112...AnalyzerTests.VerifyWrappedFirstLinkBehindAttachedElementAccessIsReported` calls the analyzer-only `Verify(source, diagnostics)`. It stays green and does not lock the withholding, so a new test should.
- **Existing tests whose expected value becomes "refused":**
  - `ChainLinkLayoutTests.CommentBeforeElementAccessKeepsElementAccessAtAnchorColumn` and `ChainLinkLayoutTests.CommentBeforeArgumentListKeepsArgumentListAtAnchorColumn` (`src/Reihitsu.Formatter.Test/Regression/LineBreaks/ChainLinkLayoutTests.cs`, around lines 1600 and 1670; untagged);
  - the comment, directive, and disabled-text gap tests in `ArgumentListLineStartTests` (tagged FMT-BREAK-039):
    - `RequirementExampleKeepsTheLineBreakWhenTheGapHoldsCommentDirectiveOrDisabledText`
    - `DelimitedCommentInFrontOfOpeningParenthesisKeepsTheLineBreak`
    - `CommentLineInTheGapKeepsTheLineBreak`
    - `PragmaDirectiveInTheGapKeepsTheLineBreak`
    - `RegionDirectiveInTheGapKeepsTheLineBreak`
    - the two `...WithoutDocumentationParsing` tests
    - `CommentInTheGapKeepsTheLineBreakForEveryOwner`
  - Under reading A of question (a), also the two `[with /* size */ (4)]` cases in `WithElementSpacingTests` (lines 35 and 141, FMT-SPACE-028).
  - `ChainLinkLayoutTests.CommentInsideConditionalElementAccessPutsBracketUnderQuestionMark` stays as it is unless (b) is chosen.
- **Fix-side inputs that stay offered, as negative tests:**
  - RH5101's spec example `_ = M /* note */ (⏎1,⏎2);` stays valid. The fix formats only the argument list (`RH5101...CodeFixProvider.cs:36`), which holds the part but not the token in front (FMT-UNSUP-003), so the fix is still offered.
  - RH3101's example `return base /* a */ [0];` → `this /* a */ [0]` stays valid. The fix does not format through the formatter and keeps a shape that was already in its input.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `PrecheckPhase` | must change (FMT-UNSUP-014) |
| `ArgumentListLineStartRewriter` | must change (trivia term and remarks sentence deleted) |
| `MethodChainAlignmentContributor` | must change (attached-part column deleted; `AlignRestOfRun` stays, see (b)) |
| Other formatter phases | not involved by the declaration |
| Code fixes | must stay in code; RH3202, RH2001, RH7101, RH5112, RH5302, and every other fix that formats through the formatter are withheld through SHARED-FIX-010 |
| Analyzers | not involved (they do not consult the precheck) |
| `sdd/specs/formatter/horizontal-spacing.md` FMT-SPACE-028 | **must change under reading A** (finding 1); missing from the diff |
| Rule pages | not involved |

## Refuse option

The declaration is itself the refuse option the maintainer chose. Its exact span, the special cases it deletes, and what it refuses are verified under *Premise* and *Breaking inputs*. I found no simpler option.

## Output questions

**Finding 1: FMT-SPACE-028 conflicts with the "On any line" reading.** This adds a fact to the lead's pending question (a).

`sdd/specs/formatter/horizontal-spacing.md:1428` says:

> A comment in the gap keeps its place, and the decision applies to the whitespace behind it

Its example (lines 1445 and 1461) formats

```csharp
List<string> d = [with /* size */ (4)];
```

to

```csharp
List<string> d = [with /* size */(4)];
```

`WithElementSyntax` is one of FMT-UNSUP-014's argument-list parents. Under reading A, as written now, the FMT-SPACE-028 example file is refused as a whole and the sentence can never apply. The spec diff does not touch FMT-SPACE-028, and the change doc does not list it among the affected requirements. Under reading B (a gap without a line break is not refused), FMT-SPACE-028 stays valid.

What each reading costs:

- **A** deletes no code. It needs this FMT-SPACE-028 amendment: remove the comment sentence or replace it with a pointer to FMT-UNSUP-014, and change the example's `d` line. It also refuses RH2001 and RH7101 fixes on `= Offset /* three */ (3)`, which are offered on `main`. In its favor, it matches the maintainer's choice for FMT-UNSUP-008 and FMT-UNSUP-011, where a gap is a place in the syntax tree.
- **B** is the reading stated strictly by cause. FMT-BREAK-039 and FMT-BREAK-040 join only gaps that hold a line break, and both deleted special cases act only there. It adds a line-break condition to the check: block comments with no line break in the gap are not refused.

My recommendation stays A, for consistency with FMT-UNSUP-008 and FMT-UNSUP-011. The input is equally rare in both forms, and both checks are cheap. If A is chosen, the spec author must amend FMT-SPACE-028 and add it to the change doc's *Affected requirements* in this run.

For the output question, here is the FMT-SPACE-028 case:

````markdown
**Formatter · a block comment between a collection expression's `with` and its argument list**

Input:
```csharp
List<string> d = [with /* size */ (4)];
```

Before (`main`):
```csharp
List<string> d = [with /* size */(4)];
```

After, A (recommended): the file is unchanged, and the CLI reports `Error: <file>(…): unsupported syntax: a comment or directive in front of an argument list, an element access, or a null-forgiving operator. The file was left unchanged.`

After, B: as on `main`.
````

**Question (b): also declare the gap between `?` and `[`.** These facts are for the lead's escalation 10.

Declaring it would delete:

- the refusal the new FMT-BREAK-040 owner needs for that inner gap;
- both `AlignRestOfRun(attachedPart.Tokens, model)` calls in `MethodChainAlignmentContributor`, at lines 154 and 178. `AlignRestOfRun` itself stays for link operators.

It would refuse `n? // c⏎[0]`, a comment placed between the two characters of `?[`, which is rarer still than the shape already declared. It would also let the shape be stated purely by its cause, as every gap FMT-BREAK-039 and FMT-BREAK-040 join. FMT-BREAK-040's *Why* would then drop its carve-out sentence ("the removal of a line break there still yields to FMT-BREAK-001").

I recommend declaring it, but it is the maintainer's to decide.

## Internal choices

- **Finding 2: change 7's list of withheld fixes.** The list is complete in substance through its closing sentence ("The same holds for every other fix …"). I recommend adding RH5112 (`Wrapped fluent calls should keep the first call on the original line`) with the fixture above as a fourth example. It is the chain rule whose fix output on `main` is exactly the column this change deletes. No visible output beyond what the declaration already implies.
- **Wording in FMT-UNSUP-014's first sentence.** It reads "an argument list … whose gap FMT-BREAK-039 or FMT-BREAK-040 joins", and the *On any line* bullet then includes gaps with no line break, which no join touches. I recommend "of the kinds whose gap FMT-BREAK-039 or FMT-BREAK-040 joins", so the sentence names the part kinds rather than the joined gaps. This applies only if A stays. Internal; no visible output.
- **Overlaps with other shapes.** Only FMT-UNSUP-004 (attribute arguments) and FMT-UNSUP-010 (fields with several declarators) can start at the same trivia, as the spec states.
  - FMT-UNSUP-008, -009, -011, -012, and -013 each end in front of an expression's first token or a comma. A part's first token never starts an expression, so none of them can share trivia with this shape.
  - Requirement order in `unsupported-syntax.md` (FMT-UNSUP-014 placed before FMT-UNSUP-005) follows the file's grouping of shapes.

  No change.

## Issue goals

_N/A_ for this review. 02-skeptic covers the issue's goals.

## Architecture findings

_None._ The declaration removes the second component that decided the column of a part kept below, and that resolves the remaining part of 02-skeptic's finding.

## Follow-up candidates

_None_ new. The fixtures used here were in the scratchpad, outside the repository.
