# Lead — RH2001 anonymous-object member name

- Issue: thoenissen/Reihitsu#1990 — `agent-filed` (goals are proposals; the output decision below is the maintainer's and binding).
- Slug: `rh2001-anonymous-member-name`
- Branch: `claude/great-galileo-pf6zoo` (session-designated); draft PR thoenissen/Reihitsu#1997.
- Base: `origin/main` 13b0ed2.
- Lane: **behavior-change** — the run changes what the RH2001 code fix produces (a fix offered where it is withheld today), which needs a change doc.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager only if follow-up candidates exist.

## Maintainer clarifications (summarized)

The maintainer answered the issue's output question in an issue comment: option B. RH2001's fix is offered where a reference to the property gives an anonymous-object member its inferred name, and the member keeps its name explicitly (`new { Value }` → `new { Value = _value }`, property becomes `private int _value;`). The run also resolves the spec conflict that SHARED-CASING-015, read literally, covers RH2001. Tuple elements stay as RH2001-011 states them (issue text).

## Maintainer answers

1. Issue output question (A/B): **B** — given in the issue comment before the run.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | spec author | Fold the anonymous-member behavior into RH2001-011 or a new requirement? | New requirement RH2001-012; RH2001-011 keeps only tuples | RH2001-011's title is about withholding the fix | Spec structure only; output is the maintainer's answer B | 0070611 |
| 2 | spec author | Edit SHARED-CASING-015's scope sentence to name RH2001? | No; RH2001 lists SHARED-CASING-015 under Uses | Avoids an RH2001 row in the casing spec; the conflict is resolved by RH2001 following the scope sentence | Spec wording only | 0070611 |
| 3 | spec author | List `CasingAnonymousMemberUtilities` as a SHARED-CASING code owner? | Yes | It implements SHARED-CASING-015 and was unowned | Spec metadata only | 0070611 |
| 4 | skeptic | Architecture finding FC-015: RH2001's `GivesInferredMemberNameAsync` anonymous branch and `CasingAnonymousMemberUtilities.GetMemberNamedByReference` both decide which anonymous member takes its name from a reference | Change the structure in this run: RH2001 keeps no private copy of the anonymous-member decision and uses the casing helper; its tuple branch (RH2001-011) stays | The narrow fix would otherwise keep a second statement of the rule; within components the run changes | Same outputs as answer B | pending |
| 5 | skeptic | Reword RH2001-012 to defer to SHARED-CASING-015 for the written name? | Yes | One statement of the rule | Differs only for escaped/verbatim identifiers (rare) | 204f576 |
| 6 | skeptic | Issue's "Done when": Fix All test over the issue's input | Adjusted: single-fix test for the input plus Fix All over two properties (skeptic rows 17, 18) | The input has one diagnostic; Fix All needs two | Tests only | c808bf1 |
| 7 | skeptic | Fix All on `new { Value.Name }` (row 18) may lose an edit in BatchFixer | Test implementer pins the RH2001-012 result with Fix All; decide on failure | Unsettled by reading | Expected output follows RH2001-012 | pending |

## Follow-up candidates

None yet.

## Audited SHA

—

## Open worklist / next step

- Spec author done (01-spec-author.md, 0070611): no output questions.
- Skeptic done (02-skeptic.md, 6387aa5): proceed, no output questions.
- Spec author amendment done (204f576); decision 5 carried out there; change doc Accepted.
- Test implementer done (03-test-implementer.md, c808bf1): 23 red tests, intended.
- Next: implementer (decision 4 structure; decision 7 row 18 to check).
