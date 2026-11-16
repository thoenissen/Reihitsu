Role: spec author
Commit: 207cc60
Verdict: spec written

Small amendment carrying out lead decisions 11, 12, and 13 (`00-lead.md`; `05-skeptic.md`, *Internal choices* 1, 2, 4, 5). No output changes. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements

- RH2110-001 — A nested union is reported — clarified: the example's interface union is renamed `Value` → `Shape` (decision 11; `Value` is CS0542 because every union has an implicit `Value` property). `Why:` now says "every declaration keyword has its own nested-type rule" instead of "every kind of declaration" (decision 13; RH2106 covers both `record` and `record struct`).
- RH5418-001 — An empty union is reported from C# 15 on — clarified: the "Below C# 15 it MUST NOT report" clause stays; the example's closing sentence "Under C# 14, no union is reported." becomes "Below C# 15, `union` does not declare a type, so nothing is reported." (decision 12, option b; no C# 14 test).
- RH5418 non-goal reworded: "every union declares its case types in one of its declarations, and the list is never checked for being empty" replaces "a union without case types does not exist" (decision 13; a `partial` part may omit the list).

No new IDs; `requirement-ids.txt` and `sdd/specs/README.md` unchanged.

Not changed, checked:
- Change doc `sdd/changes/2026-11/union-analyzer-rules.md`: holds no union named `Value`, no C# 14 sentence, and no case-type non-goal. Its *Decisions* sentence "one rule per kind of declaration" is left as it is, which the skeptic allowed: it describes both the empty-type rules (where `record` and `record struct` do have separate rules, RH5415/RH5416) and the nested-type rules, so "per keyword" would be no more accurate there.
- Rule pages `documentation/rules/RH2110.md` (example union `Result`) and `RH5418.md` (`Pet`): none of the reworded text appears; unchanged.

## Examples for the tests

- RH2110-001: reported `Inner`, `Choice`, `Result`, `Shape`; not reported `Top`, `Free`, `Entry`. Never name a test union `Value`.
- RH5418-001: unchanged from `04-spec-author.md`, except that no C# 14 test is written.

## Output questions

_None._

## Internal choices

- Change-doc *Decisions* wording kept (see above). Alternative: reword it like the RH2110 `Why:`.

## Follow-up candidates

_None_ beyond those already in `00-lead.md`.
