Role: auditor (repair round)
Commit: 24b3cbc6f59b2ca80026481844e256a8d2bb45a1
Verdict: pass

The coordinator's message named the head, the base, the repair handovers 09–12 and the new decisions entry. It carried no intended fix or verdict.

## Scope

- **State verified.**
  - HEAD is `24b3cbc`, and the tree is clean.
  - `origin/main` is `d70f487601a974c5e6b8c9f74b37663897a8f6b0`, confirmed with `git ls-remote`, and it is an ancestor of the head.
  - The merge commit `24b3cbc` brings in exactly `git diff 5c5b701 d70f487`. The only difference is the line offsets in `requirement-ids.txt`.
  - From main, no production file changed. The test base gains `AssertFormattingChangesNothing`, which no test of this run uses. `shared/code-fixes.md` gains SHARED-FIX-006 to SHARED-FIX-008.
  - The merged ledger is sorted (`LC_ALL=C sort -c`) and has no duplicates. SHARED-COMMENT-011 and SHARED-FIX-006 to SHARED-FIX-008 each head exactly one requirement.
- **Repair delta against `e758862`:** `git diff e758862 c62a23b`, which leaves out the merge.
  - Specs: `shared/comment-placement.md` (SHARED-COMMENT-011 text and a second example), RH5303-003 and RH5304-002 (reference sentence), and RH5304 `Uses`.
  - Tests:
    - two new later-token tests each in RH5201 and RH5303, LF and CRLF;
    - two RH5304 tags.
  - Process files: the decisions entry `squad/decisions/2026-10-22-1139-p12-scope-rh5304.md` and handovers 09–12.
- The scope did not grow. The repair stays inside the three earlier findings and one earlier hint.
- `scripts/lane-check.sh tooling --base d70f487 --head HEAD` gives `confirmed — claimed tooling, diff needs tooling`.
- Evidence reused from `08-auditor.md`: every spec, test and code path outside the repair delta is byte-identical. That covers the guard-scope, policy-ownership and failure-class analysis there, except the rows below.
- Results trusted from other roles: the test implementer's 162/162 (RH5201, RH5303, RH5304, `TestCategory=SHARED-COMMENT-011`) and architecture 72/72 on `5a9d9ce`, and the cleaner's warning-free build. The merge brought no production change, so they stay valid (FC-032).

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| — | — | — | — | — | None. | — |

### Earlier findings

| # (08) | Status | Evidence |
|---|---|---|
| 1 — SHARED-COMMENT-011 exempted every token on the line | closed | The text now reads "the first token behind it on the comment's first line" and adds "Only that first token is exempt; a later token on the same line is judged by the rule's own requirements". It matches P12's "the token" and SHARED-INIT-001's "the token". In both, the token whose leading trivia holds the comment is the first token behind a line-leading comment. My three counterexamples are now consistent with the spec: RH5201 `.ToList()`, RH5303 `2`, RH5301 `B = 2`. For B, SHARED-INIT-001 and SHARED-COMMENT-011 now agree, because B's leading trivia holds no comment. The new example block conforms. I ran it as written through `scripts/apply-fix.sh` for RH5201 and RH5303, LF and CRLF: both report and are fixed in one iteration. |
| 2 — no test on the later-token side | closed | RH5201 `VerifyLaterLinkOnLineStartingWithBlockCommentIsDetectedAndFixed` (+`WithCrLf`) and RH5303 `VerifyLaterElementOnLineStartingWithCommentIsDetectedAndFixed` (+`WithCrLf`) each expect exactly one diagnostic, on the later token. An implementation that exempts every token behind a line-leading comment reports none and fails. One that also reports the first token (`.Select`, `1`) reports two and fails. |
| 3 — RH5304 missing | closed | The maintainer decided it in the new decisions entry, whose format matches `squad/decisions/README.md`. RH5304 `Uses` lists SHARED-COMMENT-011, and RH5304-002 ends with the reference sentence. `VerifyNoDiagnosticForCommentPrefixedOrdinaryElements` and `VerifyNoDiagnosticForCommentPrefixedNestedCollectionClosingBrace` are tagged, as decided. My 08 probe showed both shapes conform. |
| Hint — RH5303-003 lacked the reference sentence | closed | Added. |
| Hint — #1362 Q5 Touches line, and the spec checker's count | closed | `12-spec-checker.md` records the correcting issue comment and the count correction. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| SHARED-COMMENT-011: which token is exempt | The first token behind the line-leading comment, on the comment's first line | SHARED-INIT-001: "the token" carrying the comment in its leading trivia, and every other token at its own column. RH5201-001, RH5303-002 and RH5308-001 checks of later tokens. | Agree. Two leading comments: the first token behind both is still the one exempt. SHARED-INIT-001 is silent there (#1362 Q7, unchanged). | ok |
| SHARED-COMMENT-011: "a later token … is judged by the rule's own requirements" | Second and later tokens on the comment-led line | RH5305-001 (no report for a closing bracket with any comment in front of it on its line), RH5306-001 (any comment), SHARED-CHAIN-004 | Neutral wording, so each rule's broader exemption stands without contradiction. The RH5305 spec and code divergence is pre-existing (#1362 Q6). | ok |
| RH5304-002 and RH5303-003 reference sentence | As for RH5303-002 | SHARED-INIT-001 and SHARED-INIT-002, for one comment | Agree, as established in 08 for RH5301 and RH5303 | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| A comment-led line is judged by its first element (spec side) | Unchanged from 08. RH5304 now lists SHARED-COMMENT-011 next to SHARED-INIT-001. | The SHARED-INIT-001 and SHARED-COMMENT-011 overlap now also covers RH5304. Parity holds for one comment, and for later tokens after finding 1's repair. The two-comment divergence is filed (#1362 Q7). No new owner. |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5201 `VerifyLaterLinkOnLineStartingWithBlockCommentIsDetectedAndFixed`, `…WithCrLf` | Only the first link behind the comment is exempt. `.ToList()` is reported on its `.`, and the fix gives the chain its formatted layout (RH5201-005). | No diagnostic, a second one on `.Select`, or a different fixed text | `Verify(source, fixed, diag)` on `BatchCodeFixTestsBase`: exact output, iterative and Fix All checks | adequate. The pinned output is formatter-stable (`scripts/format.sh` changed nothing, so SHARED-FIX-007 and SHARED-CHAIN-008 hold). See Hints for the blank line it contains. |
| RH5303 `VerifyLaterElementOnLineStartingWithCommentIsDetectedAndFixed`, `…WithCrLf` | Only the first element behind the comment is exempt. `2` is reported for sharing the line, and the fix puts it on its own line. | No diagnostic, a second one on `1`, or a different fixed text | same | adequate. The pinned output is formatter-stable. |
| RH5304 `VerifyNoDiagnosticForCommentPrefixedOrdinaryElements`, `VerifyNoDiagnosticForCommentPrefixedNestedCollectionClosingBrace` (tags added) | A comment-led element or closing brace in its column is not reported | Any diagnostic. Without the comment, the element or brace would be off its column. | analyzer only | adequate. SHARED-INIT-001 decides these identically; it is not broader (FC-033). |

## Failure classes dismissed

- These classes reach only the repair delta. For everything else, the dismissals in `08-auditor.md` stand unchanged.
- FC-004 to FC-009, FC-018, FC-030: no fix behavior changed.
  - The new tests pin existing fix outputs, which converge in one iteration (my probes, and the verifier's iterative check).
  - The RH5201 output keeps the user's comment in place.
  - Main's new SHARED-FIX-006 forbids blank lines inserted before the changed node or between it and its neighbours. The RH5201 blank line is inside the chain, which SHARED-CHAIN-008 makes the node the fix lays out.
- FC-010 to FC-014: no formatter or analyzer change.
- FC-015, FC-016: covered under Policy ownership. The RH5304 sibling is closed. Unpinned later-token shapes in RH5301, RH5304 and others are not required: the requirement's other side has two witnesses.
- FC-031: the summaries of the four new tests describe their data.
- FC-032: the merge brought no compiled production change.
- FC-033: CRLF variants are present. The new shapes hold no documentation comment, so no doc-mode variant is due.
- FC-035: expected values come from runs of unpatched code, checked against SHARED-CHAIN-008 and FMT-BLANK-011.
- No `TODO` or tracker number is added to source or specs. The `#1139` mentions are in the decisions entry, as in every entry of that directory.

## Hints

- **Blank line pinned inside a chain.** The RH5201 later-link tests pin a fix output with a blank line inside the chain, between `items.Where(value => value > 0)` and the `/* all */` line.
  - It follows from RH5201-005 → SHARED-CHAIN-008 → FMT-BLANK-011, as `10-test-implementer.md` argues. The formatter leaves the output unchanged, and SHARED-FIX-006, newly merged from main, does not reach inside the changed node.
  - It is spec-consistent, but it ties a SHARED-COMMENT-011 witness to FMT-BLANK-011 inserting blank lines inside an expression. That is pre-existing behavior; the 08 hint saw the formatter do the same.
  - Follow-up candidate if the maintainer finds a blank line inside an expression unwanted: the test changes together with FMT-BLANK-011.
- `11-cleaner.md` records "see result line in the report" for the analyzer tests, without a result. No finding turns on it: the cleaner changed nothing ("Formatted 0 of 9"), so the test implementer's 162/162 stands.
- The architecture project was not re-run on the merged head; `12-spec-checker.md` says so. I checked the merged ledger statically: it is sorted, has no duplicates, and every new ID heads exactly one requirement. The validator's full run covers the rest.
