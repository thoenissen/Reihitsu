Role: spec author
Commit: f484a30
Verdict: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements

- FMT-USING-011 — A header changes no directive's indentation — added (clarification). Records maintainer answers 1A and 2A: in a reordered block, the directive that takes the first position (or its first leading comment) starts at the whitespace of the original first directive's line, kept as written (tab included); the original first directive and the comments that move with it keep their own lines' whitespace; this holds on both sides of the FMT-USING-009 split and in every scope, also an indented compilation unit or file-scoped namespace. Examples: block namespace without blank line (the issue's input), with a blank line below the header, with a comment leading the new first directive (b2), the tab variant in prose, and an indented file-scoped namespace.
- FMT-USING non-goal on indentation — reworded to name FMT-USING-011 next to SHARED-USING-010.
- RH7201–RH7207 — `Uses` gains FMT-USING-011, and the code-fix requirement (the "rewrite the whole using block … as the phase does" sentence) now ends "and at the indentation FMT-USING-011 specifies". Wording only; no behavior of those rules changes beyond what the phase does.
- `sdd/specs/requirement-ids.txt` — FMT-USING-011 inserted after FMT-USING-010.

Placement: a new requirement rather than a sentence in FMT-USING-009. FMT-USING-009 decides which comments stay at the top; indentation is a different decision with its own qualifiers (blank line or not, scope kind, tab), and putting it there would have meant rewriting 009's column-0 examples. The new requirement references 009 instead of restating it.

Change doc: **not required.** `sdd/README.md`, *When a change doc is required*: a clarification that writes down behavior that already exists needs none. FMT-USING-011 states the output the phase already gives the same block without the comments, and the maintainer's answers 1A and 2A chose exactly that reading for the comment shapes; for the block-namespace members SHARED-FIX-007 already demanded it. No requirement changes meaning and none is retired. The fix output for the s1/u1/t1 members does change from today's column 0, but that is the bug fix bringing the code to the requirement the maintainer's answers decided, recorded in 00-lead.md. If the lead reads 1A as new intended behavior rather than existing behavior, a change doc would be needed for s1/u1 only; I recommend not.

Architecture test `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` now fails with "FMT-USING-011: no test covers it" until the test implementer tags a test `[Requirement("FMT-USING-011")]`. The other 50 SDD architecture tests pass.

## Examples for the tests

FMT-USING-011 (all examples, through `OrganizeUsingDirectivesInDocumentAsync` or the RH72xx fixes, not whole-document formatting — FC-033). The test matrix from 01-skeptic stays the source for the other members.

## Output questions

_None._ (Answered: 1A, 2A.)

## Internal choices

- New requirement FMT-USING-011 instead of amending FMT-USING-009 (alternative: a paragraph and examples in 009).
- RH7201–RH7207 code-fix sentences name FMT-USING-011 explicitly (alternative: rely on "as the phase does" only).
- Tab case written as prose in the spec, not as a block with literal tabs, so the Markdown stays unambiguous (alternative: a fenced block with tab characters).
- Rule pages unchanged: the normal case of what is reported and how to fix it does not change.

## Follow-up candidates

_None._
