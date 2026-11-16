Role: auditor
Commit: 4ebe340445ddf151f883669c9831463a6e7d9fbc
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

This audit covers the repair delta of round 1: 6b45f17..4ebe340. Head is 4ebe340, and the checkout is at that SHA. `origin/main` is eb89003591b8371d91e4a14b8590d764bdf6d605: `git ls-remote` confirms it, and `git merge-base --is-ancestor` confirms that the branch contains it.

The repair diff matches the handovers `09-spec-author.md`, `10-test-implementer.md`, `11-cleaner.md`, and `12-spec-checker.md`. It changes three things:

- FMT-BLANK-011's example in `sdd/specs/formatter/blank-lines.md`;
- `src/Reihitsu.Formatter.Test/Regression/BlankLines/FirstCommentBehindOpeningTokenTests.cs`;
- `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5020CommentsShouldBePrecededByABlankLineFormatterTests.cs`.

It also adds the run's work files. It changes no production file, rule page, change doc, requirement ID, or spec header. Scope did not grow, so the guard tables and the dismissed classes of `08-auditor.md` still hold for the unchanged production code, and I reuse them.

## Previous findings

- **#1 — closed.** The FMT-BLANK-011 formatter example no longer has the `void O<` member, and its lead-in no longer names `<`. The normative sentence still names the generic `<`. Every enumerated failing member is gone from exact-output assertions:
  - The type-parameter `O<` cell is gone from the spec example test, from the author's-blank-line test, and from the RH5020 parity test.
  - The `M<` and `new int[` cells are gone from `FirstCommentBehindOtherOpeningTokensGetsNoBlankLine`.
  - The `<` and array-size cells now sit in blank-line-only assertions: `FirstCommentBehindEveryOtherOpeningTokenGetsNoBlankLine` (cells `// array size` and `// type parameter`) and the new `AuthorsBlankLineBehindLessThanOfTypeParameterListIsKept`.

  I checked for other places that still pin these columns. Every line the run adds that ends in `<` or `new <type>[` is in one of these:
  - RH5020-002 and RH8303-002 analyzer examples, which state reported or not reported only.
  - RH5020 and RH8303 analyzer tests. Neither code fix calls the formatter: both only use `ReihitsuFormatterHelpers.DetectEndOfLine`, so their fixed output keeps the input's columns.
  - The formatter blank-line-only tests.
  - The change doc's entry 3. Lead decision 6 keeps it as a record of real output: its *before* already has these columns, and its *after* differs only in the decided blank line. A change doc is not a spec, so the "never in a spec" rule of `sdd/README.md`, *Open questions*, does not reach it.

  The RH8303 parity test and the RH3004 and RH5305 fix tests hold no such cell.
- **#2 — closed.** All three untested sides are now pinned by FMT-BLANK-tagged tests, and the tests also pin the comment-free side of the guard:
  - side (a), the empty list pattern: `AuthorsBlankLineBehindOpeningBracketOfEmptyListPatternIsKept`;
  - side (b), the comment glued to `]`: `BlankLineBehindOpeningBracketIsRemovedAboveCommentGluedToClosingBracket`, for both a collection expression and a list pattern;
  - side (c), the property pattern behind a positional `(`: `AuthorsBlankLineBehindOpeningParenthesisIsKeptAboveCommentBeforePropertyPattern`;
  - the comment-free side: `BlankLineBehindOpeningParenthesisIsRemovedDirectlyBeforePropertyPatternWithoutComment`;
  - the empty collection: `AuthorsBlankLineBehindOpeningBracketOfEmptyCollectionExpressionIsKept` and `...WhileBlankLineBehindCommentBeforeClosingBracketIsRemoved`.

  The test implementer's two reverted mutations show that the tests decide both conjuncts of `isAuthorsBlankLineBehindOpeningToken`. Forcing the guard to `false` fails four tests, and forcing the comment conjunct to `true` fails the property-pattern test without a comment. The glued-comment test survives the first mutation, as it should: FMT-BLANK-018 removes that blank line on its own, and the test is tagged with that requirement.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

## Guard scope

_N/A — the repair changes no predicate, guard, or exemption. The production code is byte-identical to 6b45f17, and the table in `08-auditor.md` stands._

## Policy ownership

_N/A — the repair changes no policy owner. The ownership table in `08-auditor.md` stands._

## Assertion adequacy

I extracted every input of `FirstCommentBehindOpeningTokenTests` into a scratch directory outside the repository and ran `scripts/format.sh --dry-run` over it. The CLI reports no input as unsupported. So no blank-line-only fixture, the extended opener matrix included, passes vacuously through a precheck refusal (FMT-UNSUP-004 excepts the array-size and lambda comments as inside an initializer value). Each diff shows the blank-line state the test asserts.

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `SpecExampleFirstCommentBehindOpeningParenthesisOrBracketGetsNoBlankLine`, `FirstCommentBehindOtherOpeningTokensGetsNoBlankLine`, `AuthorsBlankLineBehindOpeningParenthesisOrBracketIsKept` (changed) | Exact output, LF/CRLF, second pass, per-phase stability; every remaining column is decided by FMT-INDENT-012/025 or by FMT-BLANK-007 for the `{` side | Any blank-line or column difference | `AssertRuleResult` | Adequate. No undecided column remains |
| `FirstCommentBehindEveryOtherOpeningTokenGetsNoBlankLine` (two cells added) | No blank line above the first comment behind the array-size `[` and the type-parameter `<` | A blank line above `// array size` or `// type parameter`. `main` inserts one (change doc entry 3, *before*) | `AssertBlankLinesAbove`; labels unique | Adequate. The fixture is not refused |
| `AuthorsBlankLineBehindLessThanOfTypeParameterListIsKept` | The author's blank line behind the type-parameter `<` is kept (FMT-BLANK-011) | The blank line removed | `AssertBlankLinesAbove` | Adequate |
| `AuthorsBlankLineBehindOpeningBracketOfEmptyCollectionExpressionIsKept`, `...OfEmptyListPatternIsKept`, `...WhileBlankLineBehindCommentBeforeClosingBracketIsRemoved` | Blank line kept above the comment the closing `]` owns; none above `]` | The blank line collapsed by the cleanup rewriter, or one left above `]` | `AssertBlankLinesAbove`; the first `];` line in each output is the closing bracket under test | Adequate. Fails under the guard-`false` mutation |
| `BlankLineBehindOpeningBracketIsRemovedAboveCommentGluedToClosingBracket` | No blank line above a comment glued to `]` (FMT-BLANK-018) | A blank line above `/* collection */` or `/* pattern */` | `AssertBlankLinesAbove` | Adequate for FMT-BLANK-018. It pins the outcome that `08-auditor.md` side (b) named, not the guard |
| `AuthorsBlankLineBehindOpeningParenthesisIsKeptAboveCommentBeforePropertyPattern`, `BlankLineBehindOpeningParenthesisIsRemovedDirectlyBeforePropertyPatternWithoutComment` | Both sides of the comment conjunct in front of a property pattern's `{` | The blank line removed with a comment, or kept without one | `AssertBlankLinesAbove`. The label `{ Length: 0 }` is unique, and a join onto the `(` line would fail the lookup | Adequate. Each fails under one of the two mutations |
| `RH5020...FormatterTests.VerifyFormatterLeavesFirstCommentBehindOpeningTokenWithoutBlankLine` (changed) | FC-013: the formatter output is not reported, and a second pass is stable | A diagnostic on the output | `VerifyFormatter` | Adequate. The `O<` cell is removed. RH5020's `<` cells stay covered by its analyzer tests, which state no column |

## Failure classes dismissed

The repair adds no compiled behavior, so the dismissals of `08-auditor.md` carry over unchanged for the production code. For the delta:

- FC-017: checked above, and no assertion is wider than its requirement.
- FC-003: both sides of each guard condition are now tested.
- FC-032 to FC-035: the expected values match the CLI output of this head, under LF and CRLF.
- FC-038 to FC-043: no fixture is refused, and no shape is declared.
- FC-031: the changed test summaries describe the changed fixtures.
- Every other class: no production path changed.

## Architecture findings

_None._

## Hints

- The hints of `08-auditor.md` stand unchanged: the BOM in the RH5020 analyzer file, FMT-BLANK-012's restatement of the exemption, the two equivalent switch-label colon checks, and the long summary line in `PrecedingTokenFacts.cs`. The repair touched none of them.
- The RH5020 analyzer test `SpecExampleFirstCommentBehindOpeningParenthesisBracketOrLessThanIsNotReported` keeps "LessThan" in its name, while its formatter counterpart was renamed. This is consistent, because RH5020-002's example still holds the `<` member and states no column.

## Follow-up candidates

_None_ beyond those already in `00-lead.md`. This includes the FMT-BLANK-019 wording question for an empty `[⏎⏎]` without a comment, and the column of the first element behind a type-parameter `<`, a type-argument `<`, or an array-size `[`.
