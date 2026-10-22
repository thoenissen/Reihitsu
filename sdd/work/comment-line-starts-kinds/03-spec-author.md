Role: spec author (revision after the skeptic)
Commit: 9b8f97d
Verdict: spec written

Inputs:
- `02-skeptic.md` (F1–F7, verdict "proceed narrower").
- `squad/decisions/2026-10-22-1139-p12-scope-rh53xx.md`, which puts RH5301, RH5302, RH5303, RH5305, RH5306 and RH5308 in scope.
- The lead's notes. thoenissen/Reihitsu#1362 Q4 and Q5 are filed, and the RH5206 brace is commented on #1147, so I repeated none of that.

I derived each change below from the findings and the specs; the skeptic's suggestions are only suggestions. No new requirement ID is minted.

## Changed requirements

- **SHARED-COMMENT-011 — A line that starts with a comment is not judged by the token behind it — changed.** The normative text is now:
  - A rule that checks where a line starts takes a comment of any kind that starts its line, as SHARED-BLANK-002 defines it, as the line's first element.
  - When that comment stands where the rule asks the line to start, a token behind it on the comment's first line is not reported for its position: neither for not starting its line nor for its column.
  - A comment that does not start its line, such as one behind code on the line above, does not make the line start with a comment.
- **F1.** The sentence "MUST NOT change how the line is judged" is gone. A comment that does not start its line now only falls outside the predicate. Each rule's own requirement decides that line: SHARED-CHAIN-004, RH5305-001 and RH5306-001 keep their exemptions, and RH5203-001 keeps reporting.
- **F2 and F3.** The normative sentence is now scoped positively to a leading comment that "stands where the rule asks the line to start". The "does not cover" note is removed. A line whose leading comment is itself out of place falls outside the sentence, so no spec text answers #1362 Q2.
- **F4.** "Starts its line" now references SHARED-BLANK-002 instead of defining it again, and comment-placement's `Uses` is now SHARED-BLANK-002. The phrase "on the comment's first line" leaves out a token behind the tail of a multi-line comment (#1362 Q4) without deciding it.
- **F7.** The `Why:` now argues from P12 itself, not from fix availability.
- **Examples (only shapes that conform today).**
  - Not reported, with documentation comments parsed and with `DocumentationMode.None`:
    - RH5204: `    /* a */   private int _a;`
    - RH5204: `    /** <summary>B</summary> */   private int _b;`. This is the "of any kind" side.
    - RH5201: a `/* all */` comment in the anchor column, followed by extra spaces before `.ToList()`.
  - Reported by RH5203 (counterparts):
    - the argument `2` behind `P(1, /* one */`, with the comment behind code on the line above;
    - the argument `2` below `// two`, with the comment on a line of its own above.
  - I re-probed the not-reported block as written, in both modes, with the scratch host against the Release analyzer. Neither rule reports it.
- **RH5110-001, RH5112-001, RH5201-002, RH5203-001, RH5204-001, RH5205-001, RH5206-001 — clarified, reworded (F2, F7).** I took my earlier per-rule sentences back out, because they read "MUST NOT be reported for its position/column" without the scope. Each requirement now carries only the reference "A line that starts with a comment is judged as SHARED-COMMENT-011 specifies." Everything else is back to its text on `main`. RH5112's sentence no longer restates SHARED-CHAIN-004.
- **RH5301-001, RH5302-002, RH5303-001, RH5305-001, RH5306-001, RH5308-002 — clarified (escalation 5 answer).** Each gains the same reference sentence, and its spec lists SHARED-COMMENT-011 under `Uses`. In RH5301, RH5303, RH5305, RH5306 and RH5308 the sentence goes in the requirement that judges a brace, bracket or operator column. In RH5302 it goes in -002, which judges where the operator starts its line.
- **Intro of `shared/comment-placement.md` (F5).** It now says the two cross-rule requirements apply "for the rules that list them under `Uses`", instead of "every rule".
- **Non-goal "Moving a comment" (F7).** It now names "the rules that report a comment in a position" instead of "these rules".
- **Unchanged:** SHARED-COMMENT-001 (P13 text, and the conforming example of the earlier revision), the ledger, the index, `documentation/`.

Not taken from the skeptic's report:
- I did not drop RH5112 from scope. SHARED-COMMENT-011 never decides an RH5112 shape today, because SHARED-CHAIN-004 is broader. RH5112 still judges whether a line starts with its first link, so it stays listed through the reference sentence alone.
- No test should tag SHARED-COMMENT-011 on an RH5112 shape (FC-033, as the skeptic notes).

Spec checks: `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage"` passed 38 and failed 1. The one failure is the expected `SHARED-COMMENT-011: no test covers it (shared/comment-placement.md:418)`. The `Uses` and ledger checks pass.

## Examples for the tests

- **SHARED-COMMENT-011.**
  - The not-reported block (RH5204 with both lines, RH5201) passes in both documentation modes. The existing RH5201 test `VerifyBlockCommentStartingTheLinkLineIsAlignedWithTheLink` already pins the RH5201 shape.
  - The RH5203 counterparts are reported.
  - Do not tag an RH5112 shape: SHARED-CHAIN-004 decides it.
  - Do not tag a line led by the tail of a multi-line comment (#1362 Q4).
  - Do not tag the RH5110, RH5203, RH5205 or RH5206 comment-led shapes, nor the RH5206 brace shape (all #1147).
  - Do not tag the RH5302 or RH5308 comment-led shapes (#1362 Q5).
  - I did not probe RH5301, RH5303, RH5305 or RH5306 for this revision. The skeptic found RH5301 not reported, and RH5305 and RH5306 exempt the shape through their own requirements. I added no example for them.
- **SHARED-COMMENT-001.** Unchanged from `01-spec-author.md`, Revision: the conforming shapes only.

## Open questions to file

_None._ The lead has filed F4 and the RH5302/RH5308 shapes already.

## Decisions needed

_None._

## Revision — placement of the reference sentence (skeptic finding N4)

Commit: 7628661. Decision: I added the reference rather than moved it. "A line that starts with a comment is judged as SHARED-COMMENT-011 specifies." now also stands in the requirement that decides each shape N4 names:

- RH5201-001, which reports a later link for not starting a line.
- RH5301-002, the member columns.
- RH5303-002, the element columns.

The existing references stay where they were: in RH5201-002 (link columns, next to its own block-comment sentence), RH5301-001 and RH5303-001 (brace columns). Those requirements judge comment-led lines too, so moving the sentence would have left them without it.

This changes no behavior, and `Uses` stays the same. Following the lead's note, I wrote no spec text about N1–N3 (thoenissen/Reihitsu#1362 Q6, Q7, and the Q2/Q4 precedents).
