Role: auditor
Commit: e829e1aa0b5c8feebdb76317085d74db08b1f3c8
Verdict: #1642: pass; #1635: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md` (decision 18), `11-auditor.md`, handovers 12–15, the specs, the change doc, and the diff 8943764..e829e1a.

## thoenissen/Reihitsu#1642

Nothing in the repair touches this issue. The diff 8943764..e829e1a changes no file of it, so the pass of `11-auditor.md` stands unchanged.

## thoenissen/Reihitsu#1635

### Scope

Head e829e1a, base `origin/main` 8a26419 (merged). This is a repair delta against 8943764. The scope did not grow:
- FMT-BREAK-012 gains a third bullet, a sentence in its `Why:`, one example, and SHARED-DIAG-010 in `Uses`;
- the change doc gains matching sentences and one rejected alternative;
- `ListWrapByElementsTests.cs` gains 11 tests;
- the `EnsureSeparatorsHaveEndOfLine` summary changes. `scripts/verify-text-only.sh --base 8943764 --head e829e1a` reports it text-only (documentation trivia, 2949 tokens identical).

No production behavior changed. The diff matches handovers 12–15. `scripts/lane-check.sh behavior-change` reports "confirmed".

Previous findings:
- **#1 — closed.** The spec now states the code's output. FMT-BREAK-012's third bullet says a first element held off the opening line by a comment, a directive, or disabled text starts its own line, and the list is one element per line. The change doc's *What* and *Decisions* say the same. Tests now pin the refused side of the opener join (FC-003). I traced every failing member that #1 listed against the new text, and none of them still disagrees:
  - argument list of an invocation and of `new`, attribute argument list, method, lambda, and indexer parameter lists, each with `//` behind the opener;
  - a block comment on the line below `(`;
  - `#pragma` in front of the first element;
  - disabled text in front of it.

  Each of these has a test. The holding member (`Foo(/* a⏎ b */1, 2)`) has one too. The `EnsureSeparatorsHaveEndOfLine` summary names the case.

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

How I checked it, first on the head:
- I ran `scripts/format.sh` twice over 14 boundary fixtures. The second pass made no change.
- Every fixture's output agrees with the third bullet's qualifier on both sides.
- Held, so the list is split:
  - `Foo(/* c */⏎1, 2)`: a comment on the opener line, then a line break;
  - `Foo(⏎/* c */⏎1, 2)`;
  - `Foo(/* a⏎ b */⏎1, 2)`: a multi-line comment, then a line break;
  - `Foo( /// c⏎1, 2)`;
  - `Foo( // c⏎1, 2,⏎3)`;
  - `Foo( // c⏎1⏎, 2, 3)`: the leading comma also moves (answer C);
  - a nested `Foo(Bar( // c⏎1, 2), 3)`;
  - `#if Y … #else 5, #endif 1, 2`;
  - a constructor initializer `: this( // c⏎1, 2)`;
  - a primary-constructor parameter list;
  - a delegate parameter list.
- Not held, so the list is joined: `Foo(⏎⏎1, 2)`.
- A single element is unchanged: `Foo( // c⏎1)`.
- An element access `x[ // c⏎1, 2]` is unchanged. It is outside FMT-BREAK-012, because bracketed argument lists are excluded.

Then against `main`:
- I formatted the same fixtures on an `origin/main` worktree. Every output was identical except the leading-comma fixture, which changes as answer C requires.
- I ran the 11 new tests against `main`'s production code by copying the head test file into that worktree. All 11 passed. So every expected value the repair adds is `main`'s output, and none is a visible change.

The change doc and `Why:` make two claims about the analyzers, and I checked both with `scripts/apply-fix.sh`, LF and CRLF:
- RH5102 reports the held lists kept on one line (`//`, `#pragma`, disabled text) and offers no fix (`6 no-fix-offered`).
- RH5101 and RH5102 report none of the formatted outputs (`no-diagnostic`). RH5105–RH5109 report none of the formatted parameter-list outputs either (method, indexer, primary constructor, delegate).

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| The first element is held off the opening line, so the list is multi-line | `HasElementSplitSignal`: `HasLeadingEndOfLine(element.GetFirstToken())`, which checks whether an end of line separates the previous token's trailing trivia from the element's leading trivia. Unchanged by the repair | FMT-BREAK-012's third bullet: a comment, directive, or disabled text between the opener and the first token, with a line break separating them, counted as SHARED-BLANK-008 counts. The other side is `CollapseFirstElementToSameLine`'s refusal (FMT-BREAK-001) | They agree on every fixture above. A line break inside the comment's text alone holds nothing (`Foo(/* a⏎ b */1, 2)` is unchanged, tested). A line break behind the comment holds the element (`Foo(/* a⏎ b */⏎1, 2)` is split). A bare line break is joined | agree |
| Other guards | Unchanged since 8943764 | — | Evidence from `11-auditor.md` reused (byte-identical code) | agree |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether a list is multi-line | `LineBreakListRewriter.EnsureSeparatorsHaveEndOfLine` and `HasElementSplitSignal`. These have been unchanged since 8943764 (only the summary text differs) | one owner. Its definition now matches FMT-BREAK-012 |
| Other policies | Unchanged | `11-auditor.md` stands |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `SpecificationExampleHeldFirstElementMakesListOneElementPerLine` | The spec example verbatim, byte for byte (`//` in a parameter list and an argument list, plus the `#pragma` twin) | The example's output differs, or the list is kept on the held line | `AssertRuleResult(input, expected)`: exact output, LF and CRLF, a second pass, and phase stability | adequate |
| `LineCommentBehindOpeningParenthesisMakesArgumentListOneArgumentPerLine`, `…ObjectCreation…`, `…Attribute…`, `…ParameterList…`, `…LambdaParameterList…`, `…OpeningBracket…IndexerParameterList…` | A first element held by `//` behind the opener makes the list one element per line, for each list kind FMT-BREAK-012 covers | `1, 2` kept on one line, a join across the comment, or a column change | `AssertRuleResult` | adequate |
| `BlockCommentInFrontOfFirstArgument…`, `DirectiveInFrontOfFirstArgument…`, `DisabledTextInFrontOfFirstArgument…` | The same, when the holder is a block comment on the element's line, a `#pragma`, or disabled text (`X` undefined, so `0,` is disabled text) | The same as above, or the directive or disabled text moves | `AssertRuleResult` | adequate |
| `MultiLineCommentBehindOpeningParenthesisLeavesArgumentListSingleLine` | A line break inside a comment's text holds nothing (SHARED-BLANK-008) | The list is split | `AssertRuleResult(input)`: the input must stay unchanged | adequate |

Two shapes the spec names are not tested: `Foo(⏎/* c */⏎1, 2)` and `Foo(⏎// c⏎1, 2)`. Handover 13 gives the reason: the blank line in front of the comment that the formatter inserts is decided elsewhere, and the lead dropped it under decision 19. The decision these shapes share is pinned by the block-comment and `//` tests, so this is not a gap.

Visible output decided (check 10): all 11 new expected values are `main`'s output (they pass on `main`). So the repair adds no visible output change. Decision 18 records this as a rare-class refusal of the join. No rule page changed.

### Failure classes dismissed

The repair changes no compiled production code. So only the classes that act on specs, tests, and comments can reach it:
- FC-001, FC-002, FC-003: see the guard-scope table. FC-003 is closed by the held/holding test pair.
- FC-017: see the assertion table.
- FC-019, FC-020: the directive and disabled-text members stay in place and are tested.
- FC-027: every list kind FMT-BREAK-012 covers has a held-first test. I also checked a constructor initializer, a primary constructor, and a delegate in fixtures.
- FC-031: the changed summary describes the code's behavior. `HasElementSplitSignal`'s documentation is unchanged and still accurate.
- FC-033: `AssertRuleResult` runs both line endings. The disabled-text test depends on `X` being undefined, which it is.
- FC-034, FC-035: the expected values are the spec example verbatim and `main`'s output.
- FC-037: no text-only claim covers the whole diff. The claim for the production file is proven by `verify-text-only.sh`.

Every other class needs a compiled production change. None exists in this repair, so `11-auditor.md`'s dismissals stand.

Cost of special cases (check 11): no production code was added.

### Architecture findings

_None._

### Hints

- FMT-BREAK-012's first bullet still says, with no qualifier, that a multi-line list MUST have "the first element on the line of the opening parenthesis or bracket". The third bullet overrides this for a held first element ("the first element stays where it is"), and FMT-BREAK-001 forbids the join. A cross-reference in the first bullet would make the specific-over-general reading explicit. It is a wording nicety, not a contradiction a reader can act on wrongly.

## Whole run

- State:
  - the checkout is at e829e1a;
  - `git ls-remote origin refs/heads/main` gives 8a26419, which is merged;
  - `scripts/lane-check.sh behavior-change` reports "confirmed".
- Commands I ran:
  - `scripts/format.sh` twice over 14 scratch fixtures on the head, and once on an `origin/main` worktree;
  - on that worktree, `scripts/test.sh --project formatter --filter` over the 11 new tests, using the head's test file: 11 passed;
  - `scripts/apply-fix.sh` for RH5101, RH5102, and RH5105–RH5109, LF and CRLF;
  - `scripts/verify-text-only.sh --base 8943764 --head e829e1a`.
- I removed the worktree and the fixtures afterwards.
