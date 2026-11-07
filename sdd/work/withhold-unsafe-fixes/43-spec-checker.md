Role: spec checker
Commit: 370faabe
Verdict: consistent

Class-ending narrowing round after audit 6 (lead rows 46-49). Checked the whole diff against `origin/main`, with emphasis on `3108e585..HEAD`: specs RH3004 and RH3106, the RH3004 code fix, the RH3004 and RH3106 test classes, handovers 37-42. My prompt carried no intended fix or verdict to set aside.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. None added; bug-fix lane. RH3004-008/-009 (changed), RH3004-007/-003/-006 (clarified), RH3106-003 (gap restated by outcome) record deliberate gaps in a fix's or report's coverage under unchanged SHARED-FIX-003/-007 and SHARED-DIAG-011, decided on recommendation under the standing instruction (rows 46-49). The departure from maintainer answer 7 (column-0 directive in the block now withholds) is recorded in row 47 and is for the PR.
2. Specs match the diff: ok. RH3004-009 (any directive or disabled text from the first token through `}`, both paths) matches `ContainsDirectiveOrDisabledText` via `SyntaxTriviaUtilities.ContainsDirectives`; RH3004-008 (string of any kind or delimited comment spanning lines, block path only) matches `ContainsStringOrCommentSpanningLines` and its placement behind the expression-body alternative; RH3004-006's stay/move rule matches the simplified `IsMovingLine`; RH3004-007 points to -008 for the block. No stale text of the removed conditions remains in the spec. RH3106-003 describes the unchanged code behavior (no production change), with MUST NOT / MAY wording that cannot contradict it. Format (one heading, normative sentence, `Why:`, markup) holds. Row 47 says "from `delegate`"; the spec says "from its first token, `async` or `delegate`", equal in outcome (RH3004-004) and noted by the spec author.
3. IDs: ok. No ID added or retired in `3108e585..HEAD`; `requirement-ids.txt` unchanged there.
4. Code owners: ok. Headers unchanged and exist; `sdd/specs/README.md` row for RH3004 matches. Architecture project: `Passed! - Failed: 0, Passed: 115, Skipped: 0, Total: 115`.
5. User documentation: ok. `documentation/rules/RH3004.md` and `RH3106.md` not changed; neither carries a withheld-fix note, formatter mention, or corner case; footer present; `### Correction` equals the fix's output for the normal case.
6. Open questions: ok. None raised in handovers 39-42; the standing instruction applies and rows 46-49 record the decisions; follow-ups (general RH3004 structural fix, RH3106 consuming-expression probe) are under `Follow-up candidates` in `00-lead.md`.
7. Coverage: ok. Every active RH3004 requirement and RH3106-003 has covering `[Requirement]` tests in the class (RH3004-003: 14, -004: 5, -005: 10, -006: 27, -007: 6, -008: 37, -009: 33).
8. Tags: ok. The 30 flipped tests, 4 retagged, and the new RH3004/RH3106 tests in handover 40 carry the tags its tables list; every requirement the spec author named under `Examples for the tests` is tagged on at least one test.
9. Nothing left behind: ok. `sdd/work/` holds only `withhold-unsafe-fixes`. No tracker numbers or `TODO` in the changed provider. Working tree clean before this handover.
