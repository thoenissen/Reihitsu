Role: spec author
Commit: 61e9ee1
Verdict: change doc written

Amendment round after the maintainer's answers (`00-lead.md`, *Maintainer answers*, 1–7) and the skeptic's amendments 1–5 (`02-skeptic.md`). Change doc: `sdd/changes/2026-11/using-comment-blank-line.md`, still `Status: Draft` (the lead sets `Accepted`).

## Changed requirements

- SHARED-USING-011 — rewritten (answer 7 B, amendments 1, 2, 4):
  - it allows the blank line above any leading comment, whatever its kind and whether it is on its own line or the directive's line;
  - the comment classification is gone, and with it the shared spec's `Uses` on FMT-BLANK-011 and FMT-BLANK-018, which is back to `—`;
  - it states that only line breaks in front of the first comment count, and that blank lines below the comment or between comments are not decided by the using rules;
  - new examples: glued `/* c */`, a blank line below the comment, a blank line between two comments, and a blank line above a directive with no comment, which is still not organized;
  - its `Why:` names RH5020 for ordinary comments only.
- FMT-USING-010 — rewritten (answers 6 A and 7 B, amendments 3, 4):
  - every leading comment counts;
  - new example: a glued comment, with its blank line kept through a reorder;
  - its `Why:` records that where none stood the fix writes none and formatting may add it, citing SHARED-FIX-006 and the *Blank lines* bullet of SHARED-FIX-007;
  - `Uses` adds SHARED-FIX-006 and SHARED-FIX-007.
- RH7207-001 — the glued example is removed (amendment 4).
- RH7207-002 — two new not-reported examples: a glued comment and a blank line below a comment. Its `Why:` attributes the blank line to the blank-line rules, with RH5020 for ordinary comments (amendments 1, 2, 4).
- RH7201-004 to RH7207-004 — the `Why:` no longer says "one organized form". It says the fixes and the formatter keep the author's choice where the organized form leaves one, with SHARED-USING-011 named (amendment 5). RH7201–RH7206 `Uses` adds SHARED-USING-011.
- Unchanged in this round: SHARED-USING-008, FMT-USING-002, the FMT-USING non-goal, FMT-BLANK-011.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~Requirement"`: 50 passed, 1 failed (`EveryRequirementOfAnEnforcedSpecIsCovered`). It fails only because FMT-USING-010 and SHARED-USING-011 have no covering tests yet.

## Change doc

- *What* and *Why* are rewritten for answer 7. *What* now covers the glued comment and says it is the one formatter output that changes. *Why* includes the cycle where the RH5020 and RH7207 fixes undo each other.
- New visible output change 6 replaces the *Unchanged* glued bullet:
  - input `using System;⏎⏎/* c */ using System.Linq;` (class `C` with an empty body);
  - before: RH7207 reported, and the fix and the formatter remove the blank line;
  - after: not reported, and the formatter keeps the blank line (plus `class C;`).
- *Unchanged* keeps the answer-6 reorder case and now says formatting the fix's result adds the blank line.
- *Decisions* now records:
  - the maintainer's answers 1–7;
  - "Every leading comment counts", which replaces "A comment glued to the directive is not covered";
  - the SHARED-FIX-006/007 justification under answer 6, and that the issue's "formatting the fix's output changes nothing" is narrowed by that case;
  - "Only the line breaks in front of the first comment count";
  - "The organized form leaves the author a choice";
  - the RH5020 attribution, now limited to ordinary comments.
- *Alternatives considered*:
  - the glued-exclusion reading is listed as rejected by answer 7;
  - "insert where none stood" is listed as rejected by answer 6.

## Examples for the tests

- SHARED-USING-011: every example, including the glued comment, the blank line below the comment, the blank line between two comments, and the negative case of a directive without a comment.
- FMT-USING-010: all examples, including the glued comment through a reorder. These are using-phase outputs.
- RH7207-001, RH7207-002 (including the glued case and the blank line below a comment), RH7207-004.
- Change doc entry 6: formatter output keeps `⏎⏎/* c */ using System.Linq;`. RH5020 still reports the input without the blank line, and RH7207 does not report the output of RH5020's fix.

## Output questions

_None._ Answers 1–7 cover every listed visible output change.

## Internal choices

- Amendment 5 is carried out as a `Why:` rewording in all seven `-004` requirements; the normative sentences are unchanged. Alternative: leave the wording.
- The SHARED-FIX-006/007 justification (amendment 3) goes into FMT-USING-010's `Why:` and the change doc, not into RH7207-004. The fixes delegate to the phase, and RH7201-004 to RH7207-004 already point to FMT-USING-010. Alternative: repeat it in all seven fix requirements.

## Architecture findings

None. Under answer 7 B the using rules need no comment classification, so the skeptic's conditional finding does not arise.

## Follow-up candidates

_None._ The skeptic's conditional follow-up applied only under answer 7 A.
