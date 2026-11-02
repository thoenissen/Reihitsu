Role: auditor
Commit: a24578b
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

This audit covers only the delta a04ae63..a24578b, which carries out maintainer answer 2 (`00-lead.md`, decisions 36 to 39). Head `claude/confident-rubin-j3pkrc` at a24578b is checked out. `origin/main` is aec8dc64b5e98626c0720171689686a9f2427bab and is an ancestor of the head. `scripts/lane-check.sh behavior-change`: confirmed.

The delta changes these files outside `sdd/work/`:

- the change doc;
- `sdd/specs/formatter/line-breaks.md` (FMT-BREAK-002, -006, -033, -034, -035, and `Uses`);
- `sdd/specs/shared/comment-placement.md` (SHARED-COMMENT-012 and `Uses`);
- `StatementBehindMultiLineCommentTests.cs` (7 new tests);
- the new `SwitchSectionBehindMultiLineCommentTests.cs` (7 tests);
- `SwitchSectionLineStartTests.cs` (3 tags added).

No production file changed, which matches handovers 24 to 27. Because the delta adds specification text and tests, this is a full audit of those. The code evidence of `19-auditor.md` and `23-auditor.md` still applies, because no compiled production token changed.

I probed with the CLI built from the head (`scripts/format.sh`), using fixtures in the scratchpad: 6 files and about 40 statement, section, and label shapes. Every output was stable on a second pass. I did not re-run the roles' focused test runs, because no finding turns on them.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1–3, 5, 6 | closed | Closed in earlier audits. The delta touches no code or test they depend on |
| 4 | superseded | Maintainer answer 2 decides the statement members of #4's class: they stay behind `*/`. Decision 38 decides the section and label members on the lead's reading only (see #7). #4's `/** */` axis is still misstated in FMT-BREAK-006 (see #8) |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 7 | high | `SwitchSectionBehindMultiLineCommentTests.cs`: `SectionBehindTailOfCommentOnItsOwnLineAfterReturnStaysBehindTheComment`, `SectionBehindTailOfCommentOnItsOwnLineAfterBreakStaysAndGetsTheBlankLineAboveTheComment`, `LabelBehindTailOfCommentOnItsOwnLineStaysBehindTheComment`, `FirstSectionBehindTailOfCommentOnItsOwnLineStaysBehindTheComment`, `LabelBehindTailOfDocumentationCommentBehindLabelStaysBehindTheComment`, and the rows `case 2:` and `case 5:` of `SectionsAndLabelsBehindCommentTailsFollowTheExampleOfOneSectionPerLine`; the specs FMT-BREAK-033 (second bullet), FMT-BREAK-034 (last sentence), and the section and label clause of SHARED-COMMENT-012 | check 10 | – (#6 was the same check for statements, before answer 2) | undecided output | **The tests now assert that a switch section or label behind the `*/` of a multi-line comment that starts on a line of its own stays behind the `*/`, and the specs require it. No maintainer answer covers this output.** Answer 2 was asked and answered for statements (`00-lead.md`, answer 2, which shows only statement output). Decision 38 extends it to sections and labels on the lead's reading. The change doc says so itself: "This extension is the lead's reading of the answer, not a separate answer". The maintainer was told, not asked. AGENTS.md says silence is not consent. Before this delta, the specs required the opposite output. SHARED-COMMENT-012 said the formatter MUST break in front of a token behind the `*/` "whatever stands in front of the comment". FMT-BREAK-033 relies on SHARED-BLANK-008, under which a section behind `*/` does not start its line, so the phase MUST insert a line break. The merged change doc `switch-section-line-start.md` (*What*) says a section or label "behind the last line of a multi-line comment does not" start its line. The rendered output is the same on `main` and on the head (25-spec-author checked both), so nothing visible changes today. But the run now writes an undecided case into a spec and locks it with tests, which `sdd/README.md` (*Open questions*) and check 10 both forbid. **Members:** all of them stay, identically on `main`. A section after `return`, `break`, `throw`, `goto`, or `continue`. The first section. A `case`, `default`, or pattern label. A multi-line `/** */` comment behind a label that moves onto its own line and leaves the label behind its `*/`. The side where the comment starts behind the previous token (sections and labels move) is not in question: FMT-BREAK-033 as it stood demanded it | Ask the output question now: input `case 1:⏎ return 1;⏎/* b⏎ */ case 2:`, before = after A = unchanged (recommended), after B = `*/⏎case 2:`. Record the answer in the change doc's `Decisions` section. If the maintainer has already answered in chat, record that answer under *Maintainer answers* and drop the sentence "not a separate answer". If the answer is B, the specs and these tests revert, and the change becomes a code change |
| 8 | medium | `sdd/specs/formatter/line-breaks.md` FMT-BREAK-006, the new paragraph and its two exceptions. The pointers in FMT-BREAK-002 and FMT-BREAK-035 inherit the problem, and so do the change doc's section *A statement behind a multi-line comment's closing line* and SHARED-COMMENT-012's statement clause | FC-016, FC-003 | – | PR-introduced | **FMT-BREAK-006 says it covers "`/* */` or `/** */`", but its exceptions give the wrong output for a multi-line `/** */` comment written behind code.** Before the line-break phase runs, FMT-DOC-001 moves such a comment onto a line of its own behind a statement or a brace, and FMT-BREAK-035's comment move does the same behind a label. The statement then stays behind the `*/`. The exceptions, read on the input, say it MUST start the line below the comment. CLI output on the head, stable on a second pass: (a) `a++; /** y⏎ */ return a;` → `a++;⏎⏎/** y⏎ */ return a;` (exception 1 predicts `*/⏎⏎return a;`); (b) `A(); /** p⏎ */ break;` in a section → `A();⏎⏎/** p⏎ */ break;` (exception 1); (c) `{ /** f⏎ */ A(); }` as a nested block, and `void N()⏎{ /** f⏎ */ A();` → `{⏎/** f⏎ */ A();` (exception 2); (d) `case 1: /** p⏎ */ A();` → `case 1:⏎/** p⏎ */ A();` (exception 2). One member holds: `A(); /** x⏎ */ A();` with no blank-line requirement, which the spec says stays and the code keeps. Sections and labels also hold, because FMT-BREAK-033 states this exact case ("A `/** */` comment behind the previous token is among the comments that move with the token … the section stays behind its `*/`"), and so does the head (`{ /** f⏎ */ case 1:`, `A(); /** g⏎ */ case 3:`, `case 4: /** h⏎ */ default:` all stay). FMT-BREAK-006 also says the comment "is glued to the statement (FMT-BLANK-018)", which is not true for a documentation comment: FMT-BLANK-018 glues none when the first comment is a documentation comment. The code's output is the decided one (answer 2: a statement behind a comment that starts on its own line stays), so the defect is in the requirement. No test covers the `/** */` statement members on either side. `19-auditor.md` #4 already listed this axis as (c) | Spec author adds the sentence FMT-BREAK-033 already has, for statements: a `/** */` comment written behind the previous statement, the brace, or the label starts a line of its own first (FMT-DOC-001, FMT-BREAK-035), so the statement stays behind its `*/`. Also drop "glued" for that case. Test implementer adds one tagged test for an exception-1 member and one for an exception-2 member, for example (a) and (d) |
| 9 | medium | `sdd/specs/shared/comment-placement.md` SHARED-COMMENT-012, formatter clause: "It leaves a statement there unless a blank line or a brace or label line requires the break, and a section or a label unless the comment starts behind the token in front of it" | FC-015 (applied to spec text) | – | PR-introduced | **The placement rule is stated twice: once in FMT-BREAK-006/033/034, and again in SHARED-COMMENT-012 as a summary.** The sentence before it in SHARED-COMMENT-012 already points to those requirements. 24-spec-author's *Internal choices* says the exception "is stated once in FMT-BREAK-006; SHARED-COMMENT-012 points to it rather than restating the conditions", and the decision 38 edit then added the restatement. It already differs from FMT-BREAK-006. It leaves out FMT-BREAK-006's condition on where the comment starts, so it reads as breaking `a++;⏎/* z⏎ */ return a;`, where a blank line is required but the statement stays. It also misses the `/** */` behavior of #8. `sdd/README.md` asks that a rule be stated once and referenced by ID | Spec author removes the summary sentence and keeps the pointer sentence |

## Guard scope
_N/A: the delta changes no production code, so it adds and changes no predicate, guard, or exemption._

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Placement of a statement behind a multi-line comment's closing line, in code | `LineBreakBlockRewriter.EnsureStatementsStartOnSeparateLines` (its summary matches FMT-BREAK-006's two conditions and says "A comment that starts its own line is not handled here"), `TokenGapNormalizer.EnsureTokenStartsLine`, `ChainLineBreakRewriter.IsBehindMultiLineCommentTail`, `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex` | Unchanged since `19-auditor.md`. Answer 2 accepts the output they produce together |
| The same placement, in spec text | FMT-BREAK-006 (statements), FMT-BREAK-033 (sections, with labels referenced from FMT-BREAK-034), and a summary in SHARED-COMMENT-012's formatter clause. The `Why`s of FMT-BREAK-006/033 and SHARED-COMMENT-012 give reasons only. The change doc records it | Two statements: #9 |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `StatementBehindMultiLineCommentTests.StatementsBehindCommentTailFollowTheExampleOfOneStatementPerLine` | FMT-BREAK-006's example, character for character | Any line differs, under LF or CRLF, or on a second pass | `AssertRuleResult` (expected value, second pass, `PhaseStabilityCheck`) | adequate. Input and output are identical to the spec example |
| The four "stays" statement tests (`…WithoutBlankLineRequirementStaysBehindTheComment`, `ReturnBehindTailOfCommentOnItsOwnLine…`, `FirstStatementBehindTailOfCommentOnItsOwnLine…`, `FirstStatementOfSectionBehindTailOfCommentOnItsOwnLine…`) | The statement keeps its place behind `*/` | The output differs from the input | `AssertRuleResult(input)`: output must equal the input under LF and CRLF, plus `PhaseStabilityCheck`; output equal to input implies a fixed point | adequate; the output is covered by answer 2. FC-003: both sides of both conditions are covered for `/* */`. The `/** */` side is not covered (#8) |
| The two "moves" first-statement tests (behind the brace, behind the label) | The statement starts the line below `*/` | The statement stays, or the comment moves | `AssertRuleResult` with expected | adequate |
| `SwitchSectionBehindMultiLineCommentTests` (7) | FMT-BREAK-033/034 placement on both sides | as above | `AssertRuleResult` | The assertions are strong enough, but five tests and two example rows assert output no answer covers (#7) |
| `SwitchSectionLineStartTests` (3 retagged) | Unchanged | Unchanged | Unchanged | adequate; the tags match SHARED-COMMENT-012's section and label clause |

Visible output (check 10): the expected statement values are covered by answer 2. The "moves" values for sections and labels are demanded by FMT-BREAK-033 as it stood. The "stays" values for sections and labels are not covered (#7). No rule page changed.

## Failure classes dismissed

- FC-001, FC-002, FC-005, FC-015 (code), FC-016 (code): no guard, predicate, or helper changed. The delta has no production diff.
- FC-003: for the tests, see *Assertion adequacy*. The only uncovered side is #8.
- FC-004, FC-019, FC-020, FC-021: no edit changed. In the tests, a directive in the gap ends its line, so a token behind it starts the line (SHARED-BLANK-008, test `SectionBehindDirectiveKeepsItsLine`). No condition the delta states can reach a directive.
- FC-006, FC-022 to FC-029, FC-036: no rewrite changed.
- FC-007, FC-008, FC-009, FC-018, FC-030: no code fix changed.
- FC-010, FC-011, FC-012: every probe fixture was stable on a second CLI pass. The tests run LF and CRLF, a second pass, and `PhaseStabilityCheck`.
- FC-013: RH5103 (RH5103-001) reports only a statement that starts on the line where its predecessor starts, and a statement behind `*/` is on a later line. The SHARED-STMT-001 rules see the blank line that the formatter puts above an own-line comment, or below the comment where the statement moves. RH5104 is a content rule (decision 17). No analyzer rule judges where a switch section or a label starts its line.
- FC-014: no new rule.
- FC-017: see *Assertion adequacy*.
- FC-031: the class summaries of both test files match their tests. The summary of `LineBreakBlockRewriter.EnsureStatementsStartOnSeparateLines` matches the new FMT-BREAK-006.
- FC-032: no compiled change.
- FC-033: the tests run the full pipeline under both line endings. No harness step normalizes the comment shape away.
- FC-034: the inputs are the spec examples and the spec-author items themselves.
- FC-035: the expected values are `main`'s output, which answer 2 adopts. The code is not patched, so the tests still guard against a later change to it.
- FC-037: no text-only claim is made.

## Architecture findings

**One rule stated in more than one place.** The specs state the placement of a statement, section, or label behind a multi-line comment's closing line twice. FMT-BREAK-006 states it for statements, FMT-BREAK-033 for sections, and FMT-BREAK-034 refers labels to FMT-BREAK-033. SHARED-COMMENT-012's formatter clause states it again as a summary, and the two statements already disagree (#9). The narrow fix stays within files the run already changes.

## Hints

- **The formatter clause of SHARED-COMMENT-012 is still general.** It now names statements, sections, and labels as its only exceptions, so it says even more firmly that every other construct gets the line break. For members this is already known to be false on the head (`int _b; /* b⏎ */ int _c;` stays, `25-spec-author.md`). Decision 39 files that as a follow-up, and the PR should say so next to the follow-up link.
- **The test summary may drift once #8 is repaired.** `PipelineRequirementTests.StatementBehindTheEndOfAMultiLineCommentIsPutOnItsOwnLine` describes the break "where the block asks the statement to start its own line". Its input is a first statement behind the brace, so the test still holds. Since FMT-BREAK-006 changed, the summary reads wider than the rule. The SHARED-COMMENT-012 example caption uses the same wording and is unchanged by the delta.
