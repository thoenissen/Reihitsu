# Lead — ordering-move-directives

- Issues: thoenissen/Reihitsu#970 (`agent-filed`), thoenissen/Reihitsu#1668 (`agent-filed`) — goals of both are proposals.
- Slug: `ordering-move-directives`
- Branch: `claude/squad-970-1668-3bsm54` (session-designated); draft PR #1940
- Lane: **behavior-change**
  - #970: the issue text says bug fix, but the triage comment reclassified it as an extension: SHARED-ORDER-006 currently forbids a move across any other directive kind, a balanced `#pragma` pair included. Offering a fix that is withheld today changes a visible output and a spec requirement → behavior change with a change doc.
  - #1668: a decision between two rules for conditional blocks in the moved/crossed span of the member-moving fixes (RH7102, RH7103, RH7109, RH7309). Choosing the simpler rule withholds fixes `main` offers (visible), with a spec and change-doc change → behavior change. Shared mechanism with #970: the directive guards of the ordering move (`OrderingDeclarationUtilities`, `SHARED-ORDER-*`).
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; none skipped.

## Maintainer clarifications (from the issues, summarized)

- #970: triage moved it to an extension because SHARED-ORDER-006 forbids moves across any other directive kind, including a balanced `#pragma` pair. An earlier decision (#927 comment, `ordering-move-safety.md` Q1) is cited by the issue as having chosen to allow balanced pairs inside a crossed member's body.
- #1668: the maintainer kept it open for closer analysis: if checking precisely which conditional blocks matter turns out to be a non-trivial check, main's current behavior should not be kept, and the fixes should refuse any conditional block in the moved span instead (AGENTS.md, *Special cases do not grow the source*).

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

- If 1668-1 is answered B: RH2001's fix (RH2001-010, `ConditionalGroupsWrapWholeMembers`) remains the only place parsing excluded branches; same criterion would apply (01-spec-author.md).

## State

- Last step: spec author (88ae496): #970 spec + change doc written; #1668 draft change doc, spec waits for answer. Open output questions 970-1, 970-2, 1668-1, 1668-2.
- Next: skeptic, then batch output questions.
