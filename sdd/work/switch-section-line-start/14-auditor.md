Role: auditor
Commit: de305b2
Verdict: pass, text cleanup

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta `066bcf6..de305b2` against the first audit `09-auditor.md` (on `8023e46`). Head `de305b24e13fd6e72d3df6aa9a38d201f41a1565` on branch `claude/festive-goldberg-qb8sp9`. The checkout is the head SHA. `git ls-remote origin refs/heads/main` returns `324fcd58fb2f3d3ac468616d1cfd99206f4e8d28`, and the branch already contains it. `scripts/lane-check.sh behavior-change` confirms the lane.

The diff matches handovers 10 to 13. It changes the change doc, FMT-BREAK-033/034/035 in `sdd/specs/formatter/line-breaks.md`, nine new tests in `SwitchSectionLineStartTests.cs`, and the XML summary of `TokenGapNormalizer.EnsureTokenStartsLine`. `git diff 8023e46..de305b2 -- src` shows that production code changed only in that summary. Compiled behavior is therefore the same as in the first audit, and I reuse that audit's corpus evidence. Scope did not grow: the spec change restates the rule the code already implemented, on answers 8 and 9.

I added a 20-input corpus for the restated rule, kept in the scratchpad outside the repository and formatted twice with `scripts/format.sh`. All 20 were stable after pass 1. Four block-body inputs were also run on a temporary `origin/main` worktree, since removed.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | `00-lead.md` records answer 8, decided under the standing instruction. The change doc no longer says the maintainer decided every entry. *Maintainer answers* separates the half he answered from the half decided under his instruction, and the decision text names who decided which half. `OrdinaryCommentStaysOnLabelLineWhileCommentsFromDocumentationCommentMove` pins `case 4:` / `/** f */ case 5:`. |
| 2 | closed | FMT-BREAK-033 now states the rule the code implements. The comments that move are the token's leading trivia on its line, from the first `/** */` comment up to the token. An ordinary comment in front of that comment stays. FMT-BREAK-034 and -035 refer to it. Every member of the first audit's enumeration now matches the spec: `/** d */ /* c */ A();` moves whole; `/** a */ /** b */ case 4:` moves whole; the multi-line `/** a⏎b */ A();` moves whole; `/* c */ /** d */ A();` and `/** d */ A();` hold. Three tests pin these. Members I traced beyond that enumeration also match the restated text: `/* x⏎y */ /** d */ A();` (multi-line ordinary comment in front, which stays); `/** a⏎b */ /* c */ A();`; `/** a */ /** b⏎c */ case 5:`; `/* c */ /** d */ /* e */ case 2: /* f */ /** g */ A();` (label and statement in one section); a `/** */` run in front of `default:` and behind a `when` label; `/** d */ // x⏎A();` and `/* c */ /** d */⏎A();` (the token already starts its line, so it is kept); `/**/ A();` and `/*** x */ A();` (not documentation comments, so they stay); and a `/** */` comment in front of an empty first statement, which is left as written. |
| 3 | closed | Six `AssertRuleResult` tests cover a comment behind a braced section's `break`/`return`, in front of the next section and in front of the switch's `}`, with single-line and multi-line comments. The expected values follow FMT-BREAK-001 and FMT-BLANK-004, plus decision 6 for `}`. |
| 4 | open | The summary's first sentence was rewritten but still does not describe the test the method makes. See the row for #4 below. |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 4 | low | `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/TokenGapNormalizer.cs:92` `EnsureTokenStartsLine` summary, first sentence | FC-031 | – (previous #4, still open) | PR-introduced | The new first sentence says "Puts a token on a line of its own when other content on its line stands in front of it". For a token behind a glued comment this is wrong. Example: `/** d */ case 2:` on a line of its own. The `/** */` comment is other content on the token's line in front of it, yet the method returns the node unchanged, because `StartsLineAtLeadingTriviaIndex` is true at the comment's index. The token also does not end up alone where the method acts: `case 1: /** d */ A();` gives `/** d */ A();`. The summary's second sentence contradicts the first. The rewrap also left line 93 at 136 characters, while the lines around it stay under 122. | Reword the first sentence to state the test, for example: "Moves a token, with the comments glued in front of it, onto a new line when the first content on the token's line does not start that line (SHARED-BLANK-008)." Then rewrap the paragraph. This is text only: `TokenGapNormalizer` is `internal`, so the summary is not public API documentation. |

## Guard scope

_N/A — the repair changes no predicate, guard, or exemption. Production code changed only in an XML summary. The first audit's guard table stands unchanged. The spec's restated span ("the token's own leading trivia on its line, from the first `/** */` comment on") now equals the span `FindFirstContentIndexOnLine(leadingTrivia, leadingTrivia.Count)` inspects. I checked this against the members listed under #2._

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which comments move with a switch section, label, or first statement | `TokenGapNormalizer.EnsureTokenStartsLine` → `TriviaListUtilities.FindFirstContentIndexOnLine` + `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex`; called only from `LineBreakBlockRewriter` (sections, labels, first statement) | Single owner, unchanged by the repair. The spec now describes this owner. |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `CommentRunFromFirstDelimitedDocumentationCommentMovesWithLabel` | FMT-BREAK-034 example verbatim: the run from the first `/** */` comment moves with the label, and `/* c */` stays | Any byte difference, a second-pass change, or a phase that is unstable alone, under LF or CRLF | `AssertRuleResult` (exact, LF+CRLF, second pass, `PhaseStabilityCheck`) | Adequate |
| `CommentRunFromFirstDelimitedDocumentationCommentMovesWithFirstStatement` | FMT-BREAK-035 example verbatim, the multi-line `/** */` comment included | Same | `AssertRuleResult` | Adequate |
| `OrdinaryCommentStaysOnLabelLineWhileCommentsFromDocumentationCommentMove` | Change-doc entry 6 verbatim, answers 6, 8, and 9 | Same | `AssertRuleResult` | Adequate |
| `BracedSectionEndingIn{Break,Return}KeepsTrailing{,MultiLine}Comment…` (4 tests) | The comment stays behind `break`/`return`; one blank line behind `break` and none behind `return`, in run 1 | Comment moved, a blank line count other than the expected one, or a change in run 2 | `AssertRuleResult` | Adequate. It closes the gap the first audit reported as #3. |
| `BracedSectionEndingInBreakKeepsTrailing{,MultiLine}CommentInFrontOfClosingBrace` (2 tests) | No blank line in front of the switch's `}` in either run (decision 6) | A blank line in front of `}` after run 1, or a change in run 2 | `AssertRuleResult` | Adequate |

## Failure classes dismissed

The repair does not move compiled behavior, so every dismissal in `09-auditor.md` still holds on the same evidence. For the restated rule I checked these classes again:

- FC-002: the spec's span and the code's span now agree. See *Guard scope* and the evidence for #2.
- FC-004, FC-020, FC-021: no comment was lost or reordered in the new corpus. That includes runs of two or three comments, multi-line ordinary and documentation comments, `//` and `///` behind a label, and `/**/` and `/***` comments.
- FC-010, FC-012: all 20 new corpus inputs were stable after pass 1.
- FC-017: see *Assertion adequacy*.
- FC-031: open as #4.
- FC-033, FC-034, FC-035: the expected values of the F2 tests are the spec and change-doc examples copied verbatim. The values of the F3 tests are derived from FMT-BREAK-001 and FMT-BLANK-004, not from patched code, which did not change.
- FC-037: the cleaner's change is a comment. `git diff 8023e46..de305b2 -- src/Reihitsu.Formatter` shows only `///` lines.
- Every other class: no compiled change in this round, so the first audit's reasons apply unchanged.

## Architecture findings

_None._

## Hints

1. A block that forms a section's body keeps comments from a `/** */` comment on the label's line. `case 1: /** d */ /* c */ { A(); } break;` gives `case 1: /** d */ /* c */`, then `{` on the next line. `case 1:⏎/** d */ { A(); break; }` keeps `{` behind the comment on the same line. `origin/main` gives the same output for every block-body input I ran. FMT-BREAK-035 leaves a block body to FMT-BREAK-008, so no requirement the run changes decides these outputs. FMT-BREAK-008 does not say where comments in front of `{` go. The spec author may add that sentence when FMT-BREAK-008 is next touched. Not a finding of this run.
2. The restated FMT-BREAK-033 says the C# parser ends the previous token's trailing trivia in front of the first `/** */` comment. That is true when the parse options' `DocumentationMode` is `Parse` or higher, which is what the CLI uses (`CSharpParseOptions.Default`). If a code fix runs on a project whose parse options have `DocumentationMode.None`, `/** d */` lexes as an ordinary comment. It would then trail the label and stay, as the ordinary-comment clause says. I have not established which parse options the IDE or `dotnet format` use. The `/**/` and `/***` inputs show the same split: they are ordinary comments, so they stay. Spec wording only, uncertain.
