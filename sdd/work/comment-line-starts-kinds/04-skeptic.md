Role: skeptic (second round, against the revision)
Commit: 8e37967
Verdict: proceed

Inputs:
- `03-spec-author.md` and `squad/decisions/2026-10-22-1139-p12-scope-rh53xx.md`.
- `git diff 9064299 8e37967 -- sdd/specs` and `git diff origin/main 8e37967 -- sdd/specs`.
- The analyzers and tests named below.

My prompt asked me to check the revision the way I checked the first version, and to probe RH5301, RH5303, RH5305 and RH5306. It carried no intended fix or verdict.

## Premise

The revision resolves every finding of `02-skeptic.md` that needed a spec change. The spec text now agrees with the code wherever it states current behavior. Four facts it does not yet account for came out of the new probes (N1–N4 below). None of them needs a spec edit to the requirement text. Three need the lead to file or annotate an issue, and N4 is a placement choice.

### My earlier findings, checked against the revision

| Finding | Status | Evidence |
|---|---|---|
| F1 — sentence 3 contradicts SHARED-CHAIN-004/007 | resolved | SHARED-COMMENT-011 now says only that a comment that does not start its line "does not make the line start with a comment". RH5112 (`IsFirstLinkBlocked`) and RH5201 (anchor fallback) are again decided by their own requirements, and their existing SHARED-CHAIN-004 tests stand. |
| F2 — per-rule sentences answered #1362 Q2 | resolved | Every rule spec now carries only "A line that starts with a comment is judged as SHARED-COMMENT-011 specifies." The rule sentences that still settle the case in their own words are on `main`, not added by this run: RH5201-002 judges the first character that is not whitespace, RH5204-001 judges the line's indentation, and RH5306-001 exempts any comment. |
| F3 — "does not cover" note | resolved | The sentence is now scoped positively: "When such a comment stands where the rule asks the line to start …". |
| F4 — restated predicate, tail-led line | resolved | "starts its line (SHARED-BLANK-002)" is referenced, and comment-placement lists SHARED-BLANK-002 under `Uses`. "On the comment's first line" leaves out a token behind the tail of a multi-line comment without deciding it (#1362 Q4). |
| F5 — scope | resolved by escalation 5 | The six RH53xx specs list SHARED-COMMENT-011. The intro now reads "for the rules that list them under `Uses`". |
| F6 — RH5206 brace | handled by the lead | It is commented on #1147. |
| F7 — `Why:`, RH5112, non-goal | resolved | The `Why:` argues from P12. RH5112 carries the reference only. The non-goal names "the rules that report a comment in a position". |

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage"` passes 38 and fails 1. The failure is the expected `SHARED-COMMENT-011: no test covers it (shared/comment-placement.md:418)`. The `Uses` and ledger checks pass.

### New probes: RH5301, RH5303, RH5305, RH5306

I ran `scripts/apply-fix.sh <ID> <fixture-dir>` on each rule, so every fixture ran under LF and CRLF. Results were identical in both. Each fixture places the token behind the comment off its column, so a rule that judged the token itself would report it. The fixtures were in the scratchpad and are deleted.

| Rule | Shape (comment in the column the rule asks for unless noted) | Result |
|---|---|---|
| RH5301 | `/* init */ {` opening brace at `new` | not reported |
| RH5301 | `/* end */ };` closing brace at `new` | not reported |
| RH5301 | `/* a */ A = 1,` member | not reported |
| RH5301 | `/** a */ A = 1,` member (comment kind) | not reported |
| RH5301 | `/* a */ /* b */ A = 1,` member, two comments | **reported**, fix `no-progress` (N2) |
| RH5301 | `/* a */ A = 1,` with the comment 8 columns left of the member column | reported and fixed (SHARED-INIT-001, #1362 Q2 territory) |
| RH5303 | the same shapes, with a `new List<int> { … }` collection initializer | same results: opening brace, closing brace and single-comment element not reported; two comments reported with `no-progress` (N2); misplaced comment reported and fixed |
| RH5305 | `/* end */ ];` closing bracket, comment in the opening bracket's column | **reported**, fix `no-progress` (N1) |
| RH5305 | `/* end */ ];`, comment off that column | reported, fix `no-progress` |
| RH5305 | misaligned `]` without a comment (control) | reported and fixed |
| RH5306 | `/* end */ ];` comment in the opening bracket's column | not reported |
| RH5306 | `/* end */ ];` comment off that column | not reported (RH5306-001 exempts any comment) |
| RH5306 | misaligned `]` without a comment (control) | reported and fixed |

**N1 — RH5305 does not conform. This corrects `02-skeptic.md`.** In my first handover I wrote "exempt by RH5305-001". I took that from the spec text without probing it, and `03-spec-author.md` repeated it.
- The analyzer, `RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer.OnCollectionExpression`, compares only the columns of the opening and closing brackets. It has no comment condition.
- So the qualifier in RH5305-001 on `main`, "whose closing bracket has no comment in front of it on the bracket's line", was never a condition of the predicate (FC-002).
- Before this run, the spec was merely silent on a bracket with a comment in front of it. Now SHARED-COMMENT-011 says the bracket behind a comment in the opening bracket's column must not be reported, and the code reports it.
- No test pins the shape.
- This is a new instance decided by P12, to be filed in #1362 (clarification 7), with this input: `int[] b = [⏎                      1⏎                  /* end */ ];`, with `[` and the comment in column 18.
- The formatter leaves the line in place, because the fix makes no progress. So RH5305 also flags the formatter's output (FC-013).

**N2 — RH5301 and RH5303 do not conform for a line led by two comments.**
- SHARED-INIT-001 applies only to a token "in front of which at most one comment starts on the token's own line". With two comments, RH5301-002 and RH5303-002 judge the member's or the element's own column and report it.
- SHARED-COMMENT-011 has no such limit. The first comment stands in the member column, so the member behind it must not be reported.
- No test pins the shape: neither test file has a line with two block comments.
- This is another new instance decided by P12, to be filed in #1362. Input: `new C⏎               {⏎                   /* a */ /* b */ A = 1,⏎                   B = 2,⏎               };`, and the same shape with `new List<int>` and the element `1`.

**N3 — Existing specs already answer #1362 Q2 and Q4 in different ways. The maintainer should see this when deciding those questions.**
- SHARED-INIT-001, used by RH5301, RH5303 and RH5304:
  - Q2: a comment that starts the token's line in the wrong column is reported on the member behind it. That is reading (a), and my misplaced-comment probe confirms it.
  - Q4: a comment that starts above the token's line and ends on it must start in the expected column, and the token is not checked. That is a third reading: the comment's start column is judged.
  - Existing tests pin this: RH5301 `VerifyShiftedCommentBeforeAssignmentIsDetectedAndFixed` and `VerifyCommentsOutsideTheirColumnAreReported`, and RH5303 `VerifyMultilineCommentBeforeComplexExpressionIsDetectedAndFixed`.
- RH5306-001 never reports a list pattern with a comment, wherever the comment is: reading (b). My off-column probe confirms it.
- RH5201-002 and RH5204-001 judge the comment's column: reading (a).
- RH5305's code reports both shapes, while its spec is silent on both.
- For a line with one comment, SHARED-INIT-001 and SHARED-COMMENT-011 now state the same behavior for RH5301 and RH5303, and they agree. If #1362 Q2 or Q4 is answered differently from SHARED-INIT-001, both requirements must change together (FC-015).

**N4 — Minor: where the reference sentence sits.** `Uses` is per spec, so SHARED-COMMENT-011 applies to the whole rule. But in three places the reference sits in a different requirement from the one that decides the shape. A reader or test author who works from the deciding requirement alone sees the opposite.
- RH5201: the reference is in -002. RH5201-001 reports every later link "that does not start a line", and a link behind a leading comment does not start its line under P12. SHARED-COMMENT-011's own RH5201 example (`/* all */   .ToList()`, not reported) relies on the exemption in -001 as well. The code conforms, because `StartsLine` is true for such a link.
- RH5301 and RH5303: the reference is in -001, the braces. The member and element columns of N2 are decided in -002, which references only SHARED-INIT-001.
- This changes no behavior and is the spec author's call.

## Mechanism and candidates

_N/A_ as a bug. The candidate set is the one in `02-skeptic.md`, extended by escalation 5. Current conformance of a comment-led line whose comment stands where the rule asks the line to start:

- Conforming:
  - RH5201 and RH5204.
  - RH5112, through SHARED-CHAIN-004.
  - RH5301 and RH5303 with one comment: braces, members, elements.
  - RH5306, through RH5306-001.
- Not conforming, tracked:
  - RH5110, RH5203, RH5205, RH5206 arm and RH5206 brace: #1147.
  - RH5302 and RH5308: #1362 Q5.
- Not conforming, untracked:
  - RH5305 closing bracket (N1).
  - RH5301 and RH5303 with two leading comments (N2).

## Breaking inputs

- **Ready witnesses for SHARED-COMMENT-011** that decide the shape directly:
  - RH5201 `VerifyBlockCommentStartingTheLinkLineIsAlignedWithTheLink`.
  - RH5301 `VerifyNoDiagnosticForCommentPrefixedOpeningBrace`, `VerifyNoDiagnosticForCommentPrefixedClosingBrace` and `VerifyNoDiagnosticForCommentPrefixedAssignment`.
  - RH5303 `VerifyNoDiagnosticForCommentPrefixedCollectionOpeningBrace` and `VerifyNoDiagnosticForCommentPrefixedCollectionClosingBrace`.
  - All are tagged with their own rule or SHARED-INIT-001 today. The new RH5204 tests come from the spec example: `/* a */` and `/** … */` in both documentation modes.
- **FC-002 ("of any kind").** `/** a */ A = 1,` (RH5301) and `/** <summary>B</summary> */   private int _b;` (RH5204) are not reported.
- **FC-003.** The RH5203 counterparts are reported: a comment behind code on the line above, and a comment on a line of its own above.
- **FC-033.** Do not tag SHARED-COMMENT-011 on an RH5306 shape, because RH5306-001's broader comment exemption decides it. The same goes for RH5112 and SHARED-CHAIN-004.
- **Do not pin as SHARED-COMMENT-011:**
  - the N1 and N2 shapes;
  - the #1147 and #1362 Q5 shapes;
  - a line led by the tail of a multi-line comment (#1362 Q4);
  - a leading comment outside the rule's column (#1362 Q2).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `sdd/specs/shared/comment-placement.md` | must stay as revised |
| The 13 rule specs listing SHARED-COMMENT-011 | must stay; N4 placement is optional |
| RH5305-001 qualifier "no comment in front of it on the bracket's line" | must stay in this run (from `main`); the RH5305 follow-up decides it |
| Analyzers, code fixes, formatter, `Reihitsu.Core` | must stay (no production change in this lane) |
| Rule documentation | not involved |
| Tests | must change: the decided removals and SHARED-COMMENT-011 witnesses from the list above |
| Decisions entries | must stay |

## Simpler option

_None._ The revision is already the narrow form.

## Questions for the maintainer

_None blocking._ These go to the lead:

1. File N1 (RH5305) and N2 (RH5301 and RH5303 with two comments) in #1362 as instances decided by P12, using the inputs above.
2. Add N3's precedents to #1362 Q2 and Q4. This covers SHARED-INIT-001 with its tests, and RH5306-001.

Follow-up candidates, out of scope, reproducible on `main` code:
- The RH5305 fix on a comment-led closing bracket offers an action that changes nothing (`no-progress`, FC-009).
- The RH5301 and RH5303 fixes do the same on a member or element led by two comments.
