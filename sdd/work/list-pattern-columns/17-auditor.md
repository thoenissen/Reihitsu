Role: auditor
Commit: 5314ea2843baa33a046b9a37e693ff27eff6576a
Verdict: pass

Repair round 1, thoenissen/Reihitsu#1929 only (audit finding #1, maintainer answer 9A, decision 8). My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Head `5314ea2843baa33a046b9a37e693ff27eff6576a` is checked out.
- Base `origin/main` is `75cc39fa61efdb2aa564ba57f36e86127b33485b`. I confirmed it with `git fetch`, and it is merged (`merge-base --is-ancestor`).
- This is a repair delta against `9caef701739028a807c8e7cf8387d23d007c9255` (`git diff 9caef70..5314ea2`).
- The diff matches handovers 12 to 16. It touches FMT-INDENT-026 in `sdd/specs/formatter/indentation.md`, the change doc `sdd/changes/2026-11/pattern-anchor-behind-wrapper.md`, `PatternAnchorBehindWrapperTests.cs` (14 new tests, and two spec-example tests brought in line), one new RH5206 test, and the work folder.
- There is no change under `src/` and no ledger change. Scope did not grow: the repair stays on finding #1's surface.
- Evidence I reused from the first audit: `PatternAnchor.cs` is byte-identical to the audited `9caef70`, so the consumer analysis there (`RecursivePatternContributor`, `ParenthesizedPatternContributor`) and its node-level parity still hold.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Answer 9A (`00-lead.md`, *Maintainer answers*) decides every member finding #1 named. The change doc records it under `Decisions`, with option B rejected, and lists every member under *Visible output changes* with its before and after columns. FMT-INDENT-026's normative text now names typed property patterns and parenthesized patterns behind `and`/`or`/`not` in an arm, a list element, a slice, and a `case` label, and a new example pins them. Every member has its own test. See the table below. |

Finding #1's members and their tests:

| Member (finding #1) | Test |
|---|---|
| (a) typed behind `or`, arm | `TypedBracesBehindOrInASwitchArmStandUnderTheArmsFirstToken` |
| (a) typed behind `not`, arm | `TypedBracesBehindNotInASwitchArmStandUnderTheNot`, RH5206 `VerifyFixPutsBracesOfTypedPropertyPatternBehindNotUnderTheArmsFirstToken` |
| (a) `null or P {`, arm | `TypedBracesBehindOrAfterNullInASwitchArmStandUnderTheNull` |
| (a) `case not Q {` | `TypedBracesBehindNotInACaseLabelStandUnderTheNot` |
| (a) `case null or P {` | `TypedBracesBehindOrAfterNullInACaseLabelStandUnderTheNull` |
| (a) `[1, not Q {` | `TypedBracesBehindNotInAListPatternElementStandUnderTheNot` |
| (a) `[.. not Q {` (traced only in 11) | `TypedBracesBehindNotInASliceStandUnderTheNot` |
| (b) `case not (` | `ParenthesizedPatternBehindNotInACaseLabelStandsUnderTheNot` |
| (b) `case > 5 and (` | `ParenthesizedPatternBehindAndInACaseLabelStandsUnderTheLabelPatternsFirstToken` |
| (b) `case 1 or (2 or 3)` (traced only in 11) | `ParenthesizedPatternBehindOrInACaseLabelStandsUnderTheLabelPatternsFirstToken` |
| 8A follower: `P⏎or⏎(`, arm | `ParenthesizedPatternBehindOrInASwitchArmStandsUnderTheArmsFirstToken` |
| 8A follower: `[1, not (` | `ParenthesizedPatternBehindNotInAListPatternElementStandsUnderTheNot` |
| 8A follower: `[.. not (` | `ParenthesizedPatternBehindNotInASliceStandsUnderTheNot` |
| whole 9A example | `SpecExampleTypedAndParenthesizedPatternsBehindAndOrNotStandUnderTheOutermostPatternsFirstToken` |

I checked each test's expected columns against the member table in `12-spec-author.md`. For example, `case not Q {` has its content at 21 and its `}` at 17, and the `.. not (` slice has its `(` under `not`.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

Class enumeration beyond finding #1's members. The fallback in `PatternAnchor.GetColumn` reads only the parent of the climbed pattern. It does not read the pattern's head, and it treats `and` and `or` alike as `BinaryPatternSyntax`. The members it reaches therefore span three axes: head (none, type, positional, or type plus positional), wrapper (`and`, `or`, `not`, or a chain of them), and context (arm, list element, slice, `case` label). The other parents of a climbed pattern (`is`, a subpattern, a parenthesized pattern) take the unchanged branches.

I ran the head CLI twice over a fixture with the members the tests do not name, in `/tmp/claude-0/-home-user-Reihitsu/86292ddd-4907-5234-9281-ccf5f9f4a5ca/scratchpad/r1/a.cs`. Each output was stable on the second pass and matches 9A:
- `not P(1, 2) { … }` in an arm and in a `case` label: braces and content laid out from `not`.
- `I and P { … }` in an arm: braces under `I`. In a `case` label, the content and `}` are laid out from `I`.
- `1 or 2 or not P { … }` in an arm: braces under `1`.
- `o is not P { … }`: under `is`, unchanged.
- `{ A: not P { … } }`: under `A`, unchanged.

## Guard scope
_N/A — the repair adds or changes no predicate, guard, or exemption. There is no `src/` change._

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Pattern anchor (FMT-INDENT-026) | `PatternAnchor.GetColumn` is the one owner. `rg PatternAnchor src` finds the consumers `RecursivePatternContributor.cs:88` and `ParenthesizedPatternContributor.cs:42`. | ok, unchanged since 9caef70 |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 14 new `PatternAnchorBehindWrapperTests` methods (table above) | FMT-INDENT-026 anchor for each 9A member | a delimiter or content line at `main`'s column (under the type, or `case` + 4). 13 shows all 14 fail with only `PatternAnchor.cs:49` reverted to `pattern.GetFirstToken()`. | `AssertRuleResult` (exact text, LF and CRLF, second pass, phase stability) | ok |
| `SpecExampleTypedAndParenthesizedPatternsBehindAndOrNotStandUnderTheOutermostPatternsFirstToken` | the whole FMT-INDENT-026 9A example | any column differing from the spec block | `AssertRuleResult` | ok. I compared by script: `TypedAndParenthesizedInput`/`Expected`, with the raw-string indentation removed, equal the spec's last two blocks and the change doc's blocks character for character. |
| `SpecExampleBracesBehindAndOrNotStandUnderTheOutermostPatternsFirstToken` (decision 8) | 8A example | as above | `AssertRuleResult` | ok. Only `object o` changed to `P o`, in input and expected alike, so no column changed. |
| RH5206 `VerifyFixPutsBracesOfTypedPropertyPatternBehindNotUnderTheArmsFirstToken` (LF and CRLF rows) | the fix result for a typed arm, through the node-level formatter | `not Q` braces at 23 | `VerifyCodeFixWithEndOfLine` (convergence, re-analysis, compiled input) | ok. The left operand `P { … }` pins the unchanged side. |

## Failure classes dismissed
- FC-001, FC-002, FC-003, FC-044: no guard is added or changed. Both sides of the anchor's boundary are pinned: wrapper versus no wrapper (`PropertyPatternsWithoutAWrapperKeepTheirAnchors`, and the left operand in the spec-example and RH5206 tests).
- FC-004, FC-005, FC-019–FC-021: the repair moves no token and joins no line. Only the anchor column is at stake, and the code is unchanged since 9caef70, where the first audit covered these classes.
- FC-006, FC-018, FC-022–FC-026, FC-029, FC-036: no rewrite, rename, literal, or key/value layout.
- FC-007–FC-009, FC-030: no new fix action. RH5206 converges in its new test, which also covers CRLF.
- FC-010–FC-012: every new test runs a second pass and checks phase stability. My extra fixture was stable.
- FC-013, FC-014: no analyzer reports a pattern anchor column. RH5206 lays out arms through the formatter, and its fix result is the formatter's own output.
- FC-015: one owner (see *Policy ownership*).
- FC-016, FC-027: the class is enumerated above, and every member holds. Typed patterns behind `and`, positional-headed patterns, and combinator chains have no dedicated test. They take the same branch as tested members, and no code distinguishes them.
- FC-017: the helpers are adequate (see *Assertion adequacy*).
- FC-028: unchanged since the first audit. `ReihitsuFormatterNodeColumnTests` pins node-level parity, and no code fix formats a recursive or parenthesized pattern on its own.
- FC-031: the summaries of the new tests describe what they assert. No production comment changed. `PatternAnchor`'s summary ("the first token of the outermost pattern the wrappers lead to") already covers the typed and parenthesized members. No `TODO`, and no tracker numbers in source.
- FC-032: no compiled change after the full-project results in 14 (formatter 7188/7188, analyzer 11939/11939 on 637b531). After that, only handovers changed (15: build green).
- FC-033, FC-034: the members are finding #1's own shapes, in both line endings.
- FC-035: the expected values are the head's outputs. Answer 9A chose exactly those outputs ("as the head does"), and 13 showed that the tests fail on the `main` fallback.
- FC-037: no text-only claim.
- FC-038–FC-043: the run declares no shape.

## Architecture findings
_None._

## Hints
_None._

## Follow-up candidates
_None._
