Role: skeptic
Commit: df6c45c
Verdict: proceed

My prompt named the issue, slug, lane, and branch. It carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

The issue's premise is real. I checked each statement about today's code against `df6c45c`, which matches `origin/main` `735d0db` for every path under `src/`:

- **The helpers check only text.** True. `AssertRuleResult` (`src/Reihitsu.Formatter.Test/Helpers/FormatterTestsBase.cs`, public and private overloads) compares the first pass with the expectation and a second pass with the expectation. When no expectation is given, it compares one pass with the input. `AssertSecondPassIsStable` compares the second pass with the first. Both go through `ApplyRule`, which calls `FormattingPipeline.Execute`. No phase is looked at.
- **`FormattingPipeline.ExecutePhases` takes a phase list.** True. Its signature is `internal static SyntaxNode ExecutePhases(SyntaxNode, FormattingContext, IReadOnlyList<IFormattingPhase>, Action<string, SyntaxNode, SyntaxNode>, CancellationToken)`, and `Reihitsu.Formatter.Test` has `InternalsVisibleTo`. The list of production phases is **not** reachable, though: `CreatePhases()` is `private`. The 12 phase classes are internal, have parameterless constructors, and hold no instance fields, so a test can construct them. Making `CreatePhases` reachable would change shipped source and move the run out of the tests lane.
- **Reversing fixtures pass today.** True. `scripts/test.sh --project formatter --filter` over `CreationInitializerBraceBehindCommentThatLeadsTheLineMovesNoOtherLine`, `SeparatesDelimitedDocumentationFromPrecedingUsingDirectiveInOnePass`, and `WrappedAngleBracketListTests` gives 29 passed, 0 failed.
- **Each of the three causes reverses today, under LF and CRLF.** I ran `scripts/trace.sh <fixture> --passes 3` on scratch fixtures, which are deleted. Every reversal reports `Stable after pass 2 (2 phase changes observed)`, and the CRLF copies show the same phase pairs:
  1. **Using block.** Input: `using System;⏎⏎/** doc */⏎using System.Text;` (the expected output of `SeparatesDelimitedDocumentationFromPrecedingUsingDirectiveInOnePass`), and the same with `// comment`. In pass 2, `UsingDirectiveOrderingPhase` removes the blank line above the comment and `BlankLinePhase` inserts it again. The issue's qualifier "of the same group" holds: `using System;⏎⏎// comment⏎using Xunit;` is stable with 0 phase changes.
  2. **Brace and blank line.**
     - A block statement directly after an `if` block's `}`: in pass 2, `BlankLinePhase` inserts a blank line between `}` and `{`, and `LineBreakPhase` removes it.
     - `var p = new P⏎        /* o */ {` (the source of `CreationInitializerBraceBehindCommentThatLeadsTheLineMovesNoOtherLine`, `CommentLedFirstTokenAnchorTests.cs`): `BlankLinePhase` inserts a blank line above the comment, and `LineBreakPhase` removes it.
     - Some neighbouring shapes do **not** reverse: the expected outputs of `ComplexElementAlignsToItsBraceBehindCommentThatLeadsTheLine`, `CommentsAroundTheArrowLeadTheCreatedStatement`, and `int[] b =⏎⏎/* c */ {` (the array-initializer row) are all stable with 0 phase changes. So the issue's class names are a hint, not a membership rule.
  3. **Line start.** For `typeof(Dictionary<,⏎                >)` and for `Run(1,⏎            (x) => x)`, `LineBreakPhase` strips the line's indentation in pass 2, and `IndentationPhase` writes it back.
- **Which phase the check flags.** Run alone on the final output, the *earlier* phase of each pair changes it. Those are `UsingDirectiveOrderingPhase` for cause 1, `BlankLinePhase` for cause 2, and `LineBreakPhase` for cause 3, which matches how the issue describes them. The later phase leaves the final output unchanged.
- **Whether "exactly the fixtures that reverse today" are the ones the issue lists.** I did not settle this. The issue names test classes "among others". Only running the extended check over the whole suite gives the list. The sibling issues name a fourth reversal: thoenissen/Reihitsu#1500 lists "the blank-line phase's own steps no longer undo each other" (thoenissen/Reihitsu#1499). That one happens inside a single phase, so this check cannot see it, and it is moot for this run. A fixture that the check flags for a cause outside the three has no cause the issue names. It must not be listed under the nearest one.

Nothing an earlier package changed makes a statement of the issue true or moot. No package of thoenissen/Reihitsu#1482 has landed; `main` is at `735d0db`.

## Mechanism and candidates

_N/A._ This is not a bug fix. The check is driven by the production phase list in `FormattingPipeline.CreatePhases()`. Each of these 12 `IFormattingPhase` types is a candidate under LF and under CRLF:

`TabReplacementPhase`, `StructuralTransformPhase`, `RegionFormattingPhase`, `DocumentationCommentFormattingPhase`, `UsingDirectiveOrderingPhase`, `BlankLinePhase`, `LineBreakPhase`, `SwitchCaseBracePhase`, `HorizontalSpacingPhase`, `IndentationPhase`, `CleanupPhase`, `LineEndingNormalizationPhase`.

## Breaking inputs

Failure classes that reach this change:

- **FC-010 and FC-012.** These are the property under test. Every cause I traced reverses under both LF and CRLF, but a reversal that happens under one ending only is in FC-010's description. An entry therefore has to say which line endings it covers, or a CRLF-only or LF-only reversal either goes unlisted or turns stale under the other ending.
- **FC-015.** The phase list would be stated twice. `CreatePhases()` is private. A list restated in the test helper drifts the moment a package adds, removes, or reorders a phase; thoenissen/Reihitsu#1485 adds a `PreparationPhase`. A drifted list skips the new phase silently. The phases the check runs must be tied to the production pipeline, so the check fails when the two differ.
- **FC-017.** The assertion can end up weaker than the guarantee:
  - An entry keyed by fixture and phase hides *any* change that phase makes on that fixture, including a second, unrelated reversal that appears later. Candidate input: a listed fixture whose listed phase gains a different change.
  - If entries are keyed per test method, a method that calls the helper more than once makes one entry cover several fixtures. Examples: `AssertConditionalAndPlainFormsMatch` in `ConditionalAccessWrapBetweenOperatorTokensTests.cs`, and test methods that call `AssertRuleResult` several times. The key must tell those calls apart, and "stale" must be decidable per call.
- **FC-033.** The harness can normalize the scenario away:
  - The phase that runs alone must see what a second pass sees: the final *text*, reparsed with the fixture's `parseOptions` (fixtures with `#if` symbols, e.g. `SeparatesDelimitedDocumentationInsideActiveDirectiveBranchInOnePass`, pass them), with the same `FormattingContext`: end of line and `LanguageVersionResolver.Resolve(tree.Options)`.
  - The pipeline's in-memory result tree is not the same input. thoenissen/Reihitsu#1482 measures 125 phase outputs that do not reparse to the same tree.
  - Leaving out the parse options turns active code into disabled text, and the check then passes vacuously.

Candidate inputs the test implementer must cover:

1. A fixture of each cause is caught with the right phase, under LF and CRLF:
   - cause 1: `UsingDirectiveOrderingPhase`, with `/** */` and with `//`;
   - cause 2: `BlankLinePhase`, for a block after `}` and for `/* o */ {`;
   - cause 3: `LineBreakPhase`, for `Dictionary<,⏎>` and for a parenthesized lambda argument.
2. A stale entry, one whose fixture no longer reverses, fails the test that carries it, per line ending as the entry states it.
3. An unlisted fixture that reverses fails with a message naming the fixture, the phase, and the line ending.
4. A fixture that passes the second-pass check and no phase changes stays green. Examples are the stable neighbours above.
5. The `AssertRuleResult(input)` path (no expectation) and the `AssertSecondPassIsStable` path both run the check. A fixture that uses `parseOptions` runs it with those options.
6. The phase set the check runs matches the production pipeline's phases, in name and count, so that a phase added to `CreatePhases()` is either checked or fails the helper.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `src/Reihitsu.Formatter.Test/Helpers/FormatterTestsBase.cs` | Must change |
| Exception list next to it in `src/Reihitsu.Formatter.Test/Helpers/` | Must change (new). Cause texts must not cite tracker numbers; `Reihitsu.ArchitectureTests` enforces this |
| `FormattingPipeline` / `CreatePhases()` and every phase (`src/Reihitsu.Formatter/`) | Must stay. A visibility change is shipped source and widens the lane |
| `ObservedPipeline`, `FormatterPhaseTestsBase` | Not involved. They may be reused, but nothing requires them to change |
| Tests that check idempotency without these two helpers (node-level entry points, `ReihitsuFormatter*Tests` that format through the public API) | Not involved. thoenissen/Reihitsu#1500 leaves the node-level question to its own change doc |
| `sdd/specs/formatter/pipeline.md` (FMT-PIPE-004) | Must stay. The per-phase property is not specified today, and the code violates it, so writing it now would not describe existing behavior. thoenissen/Reihitsu#1500 adds it and the `FMT-PIPE-004` tag. Skipping the spec author is consistent with that |
| Analyzer, code fixes, Fix All, `Reihitsu.Core`, rule docs, CLI | Not involved |

## Simpler option

_None_ that meets the target. Asserting that no phase changes text during an observed second pass, using the production list through the observer, would avoid restating the phase list. But in that pass, each phase after the first one that changes text receives altered input. The later phase of a pair then shows up as well: in cause 1, `BlankLinePhase` puts the blank line back. That is not "every phase run alone", which this issue and thoenissen/Reihitsu#1500 require.

## Questions for the maintainer

_None._ The open design points are listed under FC-010, FC-017, and FC-033 above: which line endings an entry covers, what key identifies a fixture, and what makes an entry stale. These decide test structure, not formatter output, and the test implementer can settle them against **Done when**.

## Architecture findings

The run's check exposes three pre-existing defects. Each has the structural cause "two components own the same decision":

1. **Blank line above a comment that leads a using directive of the same group.** Owned by `UsingDirectiveOrderingPhase` (removes it) and `BlankLinePhase` (inserts it). Shown by `using System;⏎⏎/** doc */⏎using System.Text;`.
2. **Blank line next to `{` or `}`.** Owned by `BlankLinePhase` (inserts it) and `LineBreakPhase` (removes it). Shown by a block statement after an `if` block's `}`, and by `var p = new P⏎        /* o */ {`.
3. **Leading whitespace of a line.** Owned by `LineBreakPhase` (strips it) and `IndentationPhase` (writes it back). Shown by `typeof(Dictionary<,⏎        >)` and by `Run(1,⏎    (x) => x)`.

The maintainer's tracking issue thoenissen/Reihitsu#1482 and thoenissen/Reihitsu#1500 already route each one to a structural package:

- cause 1 to thoenissen/Reihitsu#1497;
- cause 3's lambda shape to thoenissen/Reihitsu#1494;
- cause 3's `Dictionary<,⏎>` shape and cause 2 to thoenissen/Reihitsu#1487 and thoenissen/Reihitsu#1496, reached through thoenissen/Reihitsu#1499.

This run is meant to record them, not fix them. Whether that recorded plan answers escalation 12 is the lead's call.

## Follow-up candidates

_None_ beyond the packages already filed under thoenissen/Reihitsu#1482.
