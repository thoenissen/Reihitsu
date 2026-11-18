Role: spec author
Commit: d2c3711 (statements derived from this head; spec text only)
Verdict: spec written

Repair round 1, decisions 14 and 15 (`00-lead.md`), from `12-test-implementer.md` notes 1 and 2. My prompt carried no intended fix or hoped-for verdict beyond the worklist, which restates the lead's decisions.

## Changed requirements

- SHARED-GETONLY-008 — The fix and a member that shares its line — added (`sdd/specs/shared/get-only-expression-bodies.md`, registered in `sdd/specs/requirement-ids.txt`).
  - Paragraph 1 (decision 15): shared line as SHARED-FIX-007 states, only when the member's line begins with a member as RH3202-002 / RH5417-003 define it; otherwise nothing is separated and only the accessor list is replaced.
  - Paragraph 2 (decision 14): no fix when the member shares its line with a sibling of the same list across a gap (first token / token in front, or last token / token behind, the other token a sibling's) that holds a comment and no line break; whether or not the line begins with a member; takes precedence over the "comment behind the closing brace / semicolon follows the new semicolon" cases of SHARED-GETONLY-003/-005. Recorded in its `Why:` as a rare class refused by a cheap trivia check (AGENTS.md *Code-fix coverage*).
- Shared spec `Uses` gains RH3202-002, RH5417-003. RH3209-001 / RH3210-001: `Uses` gains SHARED-GETONLY-008, intro now "001 to 008", and the fix sentence names -008. Change doc `Affected requirements` range now 001 to 008 (no visible-output section changed: withholding for a rare class and the existing RH3202-002 exception put nothing in question, per decisions 14 and 15).
- No rule page change: a withheld fix and a corner-case exception are not page material.

`scripts/test.sh --project architecture`: 198 passed.

## Examples for the tests

SHARED-GETONLY-008, four examples:
1. Sibling behind and sibling in front on one line: separated, no blank line inserted (today the fix keeps them on one line: the repair's target, matches 12's tests).
2. Type written on one line (`class C { private int _value; public int A { get => _value; } }`): converted in place, nothing separated. Today's fix output already matches (`scripts/apply-fix.sh RH3209`).
3. `/* c */` in the gap to a sibling, both directions: reported, no fix offered (replaces the two comment-gap tests of 12, note 1).
4. Other side: comment behind the member, sibling on the next line: fix offered, comment follows the new semicolon. Today's output matches.

## Output questions

_None._

## Internal choices

- The comment-gap withholding applies whether or not the line begins with a member (alternative: only where the fix would separate). The unconditional form is one cheap trivia check, independent of the line-start test; both classes are rare, and on a one-line type with such a comment RH5404 reports the type anyway.
- The line-start exception is referenced by RH3202-002 / RH5417-003 with the list of non-member line starts given as explanation, not restated in full (alternative: copy the definition; the rulebook prefers a reference).

## Follow-up candidates

_None new._ The RH3202 family's comment-gap behavior (kept on one line against SHARED-FIX-007) is already in `00-lead.md`.
