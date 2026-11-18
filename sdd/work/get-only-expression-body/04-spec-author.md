Role: spec author
Commit: 19aae47
Verdict: #849: spec written; #850: spec written

My prompt asked me to carry out the maintainer's answer to Q7 (B), as `00-lead.md` records it. It carried no intended fix and no hoped-for verdict beyond that recorded answer. I worked from `00-lead.md`, `03-spec-author.md`, and the specs. The change doc stays `Status: Accepted`.

## Changed requirements (both issues; the rules share SHARED-GETONLY)

- SHARED-GETONLY-007 — changed (Q7 B). The new title is "A member whose directive the formatter keeps is reported without a fix". A member of SHARED-GETONLY-001, in the code the build compiles, is now reported on its accessor list without a fix when it holds a directive or disabled text from the trailing trivia of the last token in front of the accessor list through `}`. The boundary is unchanged.
  - Two places are excluded. The two gaps around the getter's `=>` belong to SHARED-GETONLY-006. A conditional group wholly inside the getter's expression is reported with a fix, because FMT-STRUCT-013 converts it.
  - A member that holds both such a directive and a comment is reported without a fix. Before this change it was not reported.
  - A member whose `set` stands in an active branch is not get-only in that build, so SHARED-GETONLY-002 leaves it out.
  - The example now carries `{|RH3209:…|}` markup on `A`, `B`, and `D`, compiled without `WRITABLE`.
- SHARED-GETONLY-001 — clarified: its *Why* now names directives together with comments and refused shapes as reported without a fix.
- RH3209-001, RH3210-001 — clarified: SHARED-GETONLY-007 moved from "not reported" to the list of members reported without a fix.

Change doc `sdd/changes/2026-11/get-only-expression-body-rules.md`:
- *What* now groups directives with comments and refused shapes.
- Visible change 6 now reads: reported on the accessor list of `A` and `B`, no fix. A note covers the `WRITABLE` build, and a sentence covers a conditional group inside the expression, which is reported with a fix.
- The first decision now covers directives.
- The decision "A directive the formatter keeps the member for is not reported" was rewritten as "…is reported without a fix". It gives the reasons and the contrast with SHARED-BODY-001.
- The rejected alternative is now the reverse: "Do not report a member the formatter keeps for a directive".

Rule pages are unchanged. For the user, the normal case is still "reported, fixed". A withheld fix for a directive is what `general-notes.md` already describes.

## Examples for the tests

SHARED-GETONLY-007: `A`, `B`, and `D` are reported without a fix. The expression-group example is reported with a fix. The `WRITABLE`-defined build has `A` not reported, which can be a test with a preprocessor symbol. The rest is unchanged from `03-spec-author.md`, SHARED-GETONLY-001 to -006, RH3209-001, and RH3210-001.

## Output questions

_None._

## Internal choices

- SHARED-GETONLY-003 (comments) and SHARED-GETONLY-007 (directives) now have the same outcome. I kept them as two requirements anyway, because their boundaries and their reasons differ. The alternative was to fold 007 into 003, but the IDs are already registered and tests will reference them.
- For the conditional group inside the getter's expression, the example states only "reported, and the code fix is offered". It shows no fix output, so that no test derives an expected layout from the formatter's indentation of a conditional group inside an argument list. That layout is the follow-up candidate already in `00-lead.md`. The test implementer should assert that the fix is offered and that its result equals the formatter's conversion, not a hand-written layout.

## Follow-up candidates

_None_ beyond the one already in `00-lead.md`.

## Checks

- The examples are the code `03-spec-author.md` checked, now carrying diagnostic markup. That handover compiled them and checked the formatter's output: `A`, `B`, and `D` are kept, and the group inside the expression converts. No example code changed in this round.
- `scripts/test.sh --project architecture`: 195 passed, 3 failed. The three failures are the same expected ones as before: `AnalyzerSpecHeaderMatchesItsRule`, `EveryCodeOwnerNamesAnExistingType`, and `EveryRequirementOfAnEnforcedSpecIsCovered`. They clear once the types and tests exist.
