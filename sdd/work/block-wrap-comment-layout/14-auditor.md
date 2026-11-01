Role: auditor
Commit: e1dce804a83ce048daaafce76dbef715ffa8259f
Verdict: pass, text cleanup

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta against `2abb2a7` (repair round 2, decision 10 of `00-lead.md`). Head `e1dce80` (branch `claude/dazzling-planck-2e0bj3`, checked out, clean). Base `origin/main` `fbf5b6f`, equal to the `git ls-remote` SHA in the prompt and an ancestor of the head (merged). The scope did not grow, so this is a delta audit, not a full audit.

`git diff 2abb2a7 e1dce80` matches the handovers 10 to 13. It holds the FMT-SWITCH-007 amendment, 12 tests appended to `BracedSwitchSectionCommentLayoutTests` (insertions only), the two comments in `SwitchCaseBraceRewriter`, and `sdd/work/`. `scripts/lane-check.sh bug-fix`: confirmed. `scripts/verify-text-only.sh --base 2abb2a7 --head e1dce80`: `SwitchCaseBraceRewriter.cs` is text-only (2861 tokens identical, changed trivia: comments, documentation, no public API documentation). The only compiled change is the test file. So the production behavior is byte-identical to the first audit, and its evidence is reused: the fixtures, the guard-scope rows, and the policy ownership.

Previous findings:

- **#1 — closed.** FMT-SWITCH-007 now states answer 3A at both sites: the first paragraph covers the first statement, and the second covers the terminal `break`. It also says what "as the wrap finds it" means: FMT-BLANK-014 collapses a run to one, and FMT-BLANK-010 removes the blank line behind `///`. Both are unchanged requirements of the blank-line phase, which runs before the switch phase. Every member of #1's enumeration is pinned by a test. Changed members: `//` and `/* */` at both sites, one blank line and several, and the single-statement section. Holding members: `///` at both sites. The "last comment" qualifier (FC-002) has tests on both sides: the new `BlankLinesBetweenAndBehind…` tests, and the existing tests with no blank line behind the last comment (FC-003).
- **#2 — closed.** Both comments now name both hazards that #2 asked for: a directive the carry would discard, and one it would move onto a line it does not own. While doing so, the rewrite added a new misstatement, reported as #3.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | low | `SwitchCaseBraceRewriter.cs:591-593` (summary of `SectionCarriesDirectives`: "The carry keeps only the comments, so it would discard a directive outside them, and it would move a directive within them…") and `:644-646` (inline comment in `AddBraces`: "carries the comments of the label, first-statement, last-statement, and trailing-break trivia across the new braces and drops the rest of that trivia") | FC-031 | #2 | PR-introduced | Both comments now say the carry keeps only the comments and drops everything else. The carry they describe, `BlockWrapTriviaUtilities`, keeps whole spans of trivia, not just the comments. Its own summary says the comments "keep the layout the author gave them: the whitespace in front of and between them, the blank lines between them". The comments are wrong for these members: (a) at the first statement, `GetCarriedLeadingTrivia(…, false)` keeps everything from the first comment's line to the statement, so the blank line behind `// a` that this repair pins (`BlankLineBetweenTheLastLineCommentAndTheFirstStatementIsKept`) is carried, not dropped; (b) at the terminal `break`, `GetCarriedLeadingTrivia(…, true)` returns the whole leading list when it holds a comment, so nothing is dropped; (c) for that reason the comments draw the hazard line in the wrong place. A directive written after the last comment that leads the first statement (`// a` / `#if X` / `A();`) is outside the comments, yet it would be carried, not discarded. A directive written above the first comment that leads the `break` would also be carried. Right for these members: leading trivia without a comment is dropped whole; at the first statement, trivia in front of the first comment's line is dropped; with `endsLine: true`, trailing trivia behind the last comment is dropped. The guard is unchanged and its span covers every carry site (first audit, Guard scope), so no output changes. Only the text is wrong, and it contradicts the requirement this repair pinned. | State the span instead of "only the comments". For example: "The carry keeps the trivia from the first comment on, or up to the last comment, and drops the trivia outside that span. A directive outside the span would be discarded, and a directive inside it would land on a line it does not own." Or name `BlockWrapTriviaUtilities` as the owner of that span and do not restate it. |

Why `pass, text cleanup`: #3 is confined to a private method's XML summary and an inline comment. The text-only proof shows the token stream, the directives, and the disabled text are identical, and that no public API documentation changed. It cannot reach compiled behavior, diagnostics, or rule metadata.

Why `Repeats #2`: the decision is the same, the same two comments in `SwitchCaseBraceRewriter` describing what the carry does with the non-comment trivia of the section, and it fails again in the opposite direction. #2's text said the carry moves everything; #3's text says it keeps only the comments. Neither matches the carrier. See *Architecture findings* for the structural reading.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Directive-bearing section stays unbraced (`SectionCarriesDirectives`) | Unchanged since `2abb2a7` (proven text-only); leading and trailing trivia of every label and statement | Carry sites of `AddBraces`, unchanged | Byte-identical to the first audit's row | holds (evidence reused) |
| Blank line behind the last leading comment (first statement, terminal `break`) | `GetCarriedLeadingTrivia`, unchanged: from the first comment's line to the end, or the whole list | FMT-SWITCH-007 as amended, plus FMT-BLANK-010 and FMT-BLANK-014 for the blank-line phase that runs earlier | The spec now states the observed boundary. Several blank lines give one and `///` gives none, both decided before the wrap. Every member of #1 has a test | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Carrying comment trivia across inserted braces | Unchanged since the first audit. The repair adds no code: `SwitchCaseBraceRewriter` is proven text-only, and the test file is the only compiled change | One owner, `BlockWrapTriviaUtilities`. The first audit's evidence is reused |
| Description of the carry's span | `BlockWrapTriviaUtilities` summaries (owner); the `SwitchCaseBraceRewriter` comments at `:590-593` and `:644-647` restate it; `ControlFlowBraceTransform.CanWrap`'s summary, unchanged since before this run, restates it too | The restatements have drifted from the owner: finding #3 |
| Blank lines behind a comment that leads code | FMT-BLANK-010 and FMT-BLANK-014 (blank-line phase, unchanged); FMT-SWITCH-007 now defers to them ("as earlier phases leave it") instead of deciding them again | One owner per decision; the spec cites them rather than restating them |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `SpecificationExampleKeepsTheBlankLineBehindTheLastLeadingCommentAtTheFirstStatementAndTheTerminalBreak` | FMT-SWITCH-007's new example, verbatim, input and output | A blank line dropped at either site; any other byte; a second pass or a single phase that changes it; CRLF differs | `AssertRuleResult(input, expected)`: exact equality under LF and CRLF, second pass, per-phase stability | adequate. Matches the spec text (compared) |
| `BlankLineBetweenTheLast{Line,Block}CommentAndThe{FirstStatement,TerminalBreak}IsKept` (4) | The blank line behind the last leading `//` or `/* */` is carried, at each site | The blank line is missing, as on `main` (03/11 ran these on an extracted `main` tree, where they fail) | `AssertRuleResult` | adequate |
| `SeveralBlankLinesBetweenTheLastCommentAndThe{FirstStatement,TerminalBreak}AreCarriedAsOne` (2) | Three blank lines reach the output as exactly one | Zero or two or more blank lines | `AssertRuleResult` (exact) | adequate |
| `BlankLinesBetweenAndBehindTheCommentsLeadingThe{FirstStatement,TerminalBreak}AreKept` (2) | Both the blank line between the comments and the one behind the last comment survive (FC-002 qualifier) | Either blank line lost | `AssertRuleResult` | adequate |
| `BlankLineBehindADocumentationCommentLeadingThe{FirstStatement,TerminalBreak}IsNotCarried` (2) | `///` stands directly above its statement or `break` in the output of the pass that braces the section | A blank line behind `///` in pass 1 (the FMT-PIPE-004 defect the run fixed shows up here on `main`) | `AssertRuleResult` (pass-1 output, then idempotency) | adequate. The inputs have the blank line (lines 1783, 1845), so the tests exercise the removal |
| `BlankLineBehindTheCommentLeadingTheOnlyStatementOfASectionIsKept` | A single-statement section keeps the blank line behind its comment and the four-space gap in front of `/* x */` (answer 1A) | Either one normalized | `AssertRuleResult` | adequate |

Visible output is decided: every expected value above is demanded either by maintainer answer 3A, which is recorded under `Maintainer answers` in `00-lead.md` and in FMT-SWITCH-007, or by answer 1A (the gap in the single-statement test), or by unchanged requirements (FMT-BLANK-010, FMT-BLANK-014, FMT-PIPE-004). No existing expected value changed (insertions only). No rule page changed.

## Failure classes dismissed
- FC-001, FC-002, FC-003, FC-005, FC-020 — the guards and the carrier are byte-identical to the first audit, which dismissed or closed these. The qualifier "last comment" now has tests on both sides.
- FC-004, FC-019 — no compiled production change; the tests show no comment lost or relocated at the sites of #1.
- FC-006 to FC-009, FC-013, FC-014, FC-018, FC-021 to FC-030, FC-033 to FC-037 — unchanged reachability since the first audit. The repair adds tests and text only, and no code fix, rule, token rewrite, or entry point is touched.
- FC-010, FC-011, FC-012 — `AssertRuleResult` checks the second pass, per-phase stability, and CRLF for every new test.
- FC-015, FC-016 — see Policy ownership. The single-statement sibling shape now has a test.
- FC-017 — see Assertion adequacy.
- FC-031 — reported as #3.
- FC-032 — the last compiled production change predates `2abb2a7`. The test implementer ran the class on `efcf8a0`, and the cleaner's change after it is proven text-only, so the results apply to the head.

Class matrix "Tab width across formatter phases": not reachable, as in the first audit.

## Architecture findings
The `SwitchCaseBraceRewriter` guard comments (`SectionCarriesDirectives` summary, `AddBraces` inline comment) restate the trivia span that `BlockWrapTriviaUtilities` owns and documents. That is one rule stated in more than one place, and the statements have drifted twice in this run: #2, then #3. The same restatement exists in `ControlFlowBraceTransform.CanWrap`'s summary, which is not changed by this run. The decision is text only. It cannot put compiled behavior or visible output in question.

## Hints
- `ControlFlowBraceTransform.CanWrap`'s summary still has the one-sided wording the first audit noted. It is not changed by this run and is the third restatement of the same span.
- `13-spec-checker.md` item 2 says FMT-SWITCH-007's `Uses` "gained FMT-SPACE-001". That ID was already there at `2abb2a7`; only FMT-BLANK-010 and FMT-BLANK-014 were added. Handover wording only; the spec is correct.
- `11-test-implementer.md` explains why its result differs from the first audit's "holds" for `///` at the first statement. The first audit compared the converged two-pass output. On `main`, pass 1 emits the blank line, which is the FMT-PIPE-004 defect decision 6 put in scope. Both statements are consistent.
