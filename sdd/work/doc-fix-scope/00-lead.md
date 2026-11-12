# Lead — doc-fix-scope

- Issues (prompt order): thoenissen/Reihitsu#1269, #1342, #1343, #1459 — all four carry `agent-filed` (goals are proposals).
- Slug: `doc-fix-scope`
- Branch: `claude/squad-tickets-1269-1342-gxh523` (the session's designated branch; overrides the playbook's `claude/batch-1269-...` name)
- Draft PR: #1679
- Base at claim: `8ac5705` (origin/main)

## Lane

**Bug fix**, for every issue. Each reports an observed-versus-expected difference in documentation code fixes against existing requirements:

- #1269 — RH8201/RH8202/RH8204 fixes change more than the reported markup (layout, line endings, re-serialized content) or need two applications (SHARED-FIX-001/006/008).
- #1342 — RH8305/RH8307/RH8309/RH8402 fixes reformat beyond the reported comment, move a behind-code `///`, drop a blank line (SHARED-FIX-006/007, SHARED-DOC-004).
- #1343 — RH8308/RH8309/RH8401 fixes need two applications (SHARED-FIX-001/005); RH8401 rule page text to be aligned with behavior (docs path, admitted by the bug-fix lane).
- #1459 — RH8305 fix makes no progress on the first type of a file without a namespace (RH8305-004). Same instance also commented on #1343.

Roles: skeptic → spec author (only for clarifications and the RH8401 rule page) → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if follow-up candidates exist.

## Maintainer clarifications

None in the prompt.

## Maintainer answers

- Q1 (#1342; RH8305/RH8307/RH8309, documentation comment written behind code on the same line as code): **A** — withhold the fix for that shape; the diagnostic stays. The #1342 comment's expected in-place layout is dropped.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/squad-tickets-1269-1342-gxh523` | Harness requires it | Internal | a200b98 |
| 2 | Lane | Spec author before skeptic? | Skeptic first; spec author only for clarifications and the RH8401 rule page | Bug-fix lane runs the spec author only for a clarification | Internal | — |
| 3 | Skeptic | #1269 RH8202: report each `<value>` or fix all in one application | Keep one diagnostic; the fix removes every `<value>` the rule would report | SHARED-FIX-001 already demands it | Diagnostics unchanged; fix result demanded by an unchanged requirement | — |
| 4 | Skeptic | #1269 RH8201 member written behind `*/` | Member goes on the line after `/// <inheritdoc/>` at the column the `/**` had (RH8201-005 reading) | Same as today for standard indentation | RH8201-005 decides it | — |
| 5 | Skeptic | #1269 goal "`/** */` and member joined" | Dropped | A `///` line cannot share its line with code | No change against today | — |
| 6 | Skeptic | #1343 RH8309 diagnostic in a later of several doc comments | Withhold the fix when the declaration's leading trivia holds more than one documentation comment | Rare class, cheap check; issue's Done-when admits withholding | Withholding for a rare class puts nothing in question | — |
| 7 | Skeptic | #1343 RH8401 architecture finding (analyzer vs `DocumentationCommentUtilities` on U+FEFF/U+001A before `///`) | Refuse the class first: withhold the RH8401 fix for a comment where a line has U+FEFF or U+001A before `///`; no change to the Core helper | README *Architecture findings*: rare class → refuse first; avoids touching a shared helper with other callers | Rare-class withholding; issue's Done-when admits it | — |
| 8 | Skeptic | #1343 `RH8401.md` | Remove the *Code fix behavior* corner-case text (spec author) | AGENTS.md: rule pages list no exceptions or corner cases; withheld fixes are covered by general-notes.md | Demanded by the repository's rule-page convention and the issue's goal | — |
| 9 | Skeptic | #1459 scope | Widen to every declaration directly in the compilation unit, RH8305 and RH8307; formatter's FMT-PIPE-010 stays | Same class, same cause as #1342 | Output demanded by RH8305-004/RH8307-004 and SHARED-FIX-006 | — |
| 10 | Skeptic | #1343 RH8308 span | Span runs to the end of the line through references (RH8308-003) | Bug against unchanged requirement | RH8308-003 already demands it | — |
| 11 | Skeptic | Follow-up: RH8202 `<value>` in a second doc comment | Dropped | Two documentation comments on one declaration is a rare class | — | — |
| 12 | Spec author | RH8204 whitespace-only placeholder content: `the<placeholder> </placeholder>customer` → `the customer` (today `thecustomer`) | Accept | Maintainer decided "content verbatim" (#1201 Q1, round 7a) | Covered by a maintainer answer | ae70663 |
| 13 | Spec author | Change doc for the five withheld-fix requirements | None | Rare-class withholding is a decision recorded in the rule spec (AGENTS.md *Code-fix coverage*); bug-fix lane stays | Rare-class withholding | ae70663 |
| 14 | Spec author | RH8309-007 scope | Narrow: withhold only when the reported element lies in another documentation comment than the one the fix edits | A guard that refuses safely rewritable input is worth narrowing when cheap | Narrower refusal keeps today's fix | — |
| 15 | Spec author | Follow-up: RH8309 order across two documentation comments not reported | Dropped | Rare class (several doc comments on one declaration) | — | — |
| 16 | Spec author | Follow-up: RH8402 deletes a non-copyright note in the old header's comment run | Dropped | #1342 states replacing the whole leading comment run is intended (maintainer round 8 item 20) | Decided by the maintainer | — |
| 17 | Test implementer | Rewrote `NoFixIsOfferedWhenTheDocumentedMemberHoldsACommentTheFormatterRefuses` (RH8305/RH8307/RH8309) to expect the fix offered with the body unchanged | Accept | The withholding existed only because the fix formatted the whole declaration; SHARED-FIX-006 plus the amended -004 requirements demand changing only the element | Output demanded by SHARED-FIX-006 and RH83xx-004 | 6f88024 |
| 18 | Test implementer | RH8201 `…UsesEnvironmentEndOfLine` → `…UsesDocumentEndOfLine` | Accept | SHARED-FIX-008 | Demanded by SHARED-FIX-008 | 6f88024 |
| 19 | Implementer | Disputed RH8201 tests `VerifyDiagnosticForFieldLikeOverrideEvent`, `VerifyMultipleFieldLikeOverrideEventsAreFixedInOneFixAllIteration` (input member at column 8, expected column 4) | The test input is wrong; test implementer corrects the input to the class's usual column | RH8201-005 (as amended) keeps the member's lines and columns | Decided by RH8201-005 | — |
| 20 | Implementer | RH8305/RH8307 analyzers report a positional record's comment twice (type + primary constructor run) | Take in as adjacent fix (guard in the two analyzers, already tested by the compilation-unit rows) | Small defect in components the run changes; a duplicate diagnostic of the same span is a defect | Removes a duplicate of an identical diagnostic | 42eeedd |
| 21 | Implementer | RH8307 fix withheld for an element inside `/** */` | Accept (rare class); spec author records it in RH8307 spec, test implementer adds a test | AGENTS.md *Code-fix coverage* | Rare-class withholding | — |
| 22 | Implementer | Architecture finding: `LayOutElementAsync` re-indents element lines, copying the formatter's `///` prefix decision; no `ReihitsuFormatter` entry point lays out one documentation element | (c) Add a narrow `ReihitsuFormatter` entry point that lays out one documentation element; the fixes call it and the copy goes. (a) keeps a copy (FC-015); (b) renames a type to evade the FMT-PIPE-009 ban | README *Architecture findings*: the narrow fix would itself add a copy; change stays in the formatter, the counterpart the fixes already use; well under 100 lines, no new type | Internal structure; outputs unchanged | — |
| 23 | Spec author (05) | FMT-PIPE-032 entry point name `FormatDocumentationElementInDocumentAsync`; added lines take the start-tag line's indentation; fixes keep their own registration checks | Accept all three | Matches today's fix output; the formatter now owns the prefix decision | Internal; outputs unchanged | ffbb0bb |
| 24 | Auditor (10) | Finding #2 RH8402: header text ending in a line break yields an extra blank line | Repair round 1: spec author widens RH8402-006 coverage to that configuration, test implementer adds a test, implementer writes one line break | PR-introduced regression against main | Restores main's output; demanded by RH8402-006 once covered | — |
| 25 | Auditor (10) | Finding #1 RH8204 dead fallback branch | Repair round 1: implementer deletes the unreachable branch | PR-introduced dead code | Internal | — |
| 26 | Auditor (10) | RH8309 accessor follow-up candidate | Dropped: the fix is now withheld there (no SHARED-FIX-005 violation left), and accessor documentation is a rare class | — | Rare-class withholding | 2ed4500 |
| 27 | Repair 1 | RH8402-005 clarification for a header ending in a line break (11-spec-author) | No change; the insert path already writes one line break | Spec checker may raise it if it misreads | Insert path output unchanged | — |
| 28 | Implementer (13) | Follow-up: insert path with a file starting with a blank line then a non-copyright comment, header ending in a line break (not probed) | Dropped | Rare class, unprobed, outside the paths the run changes | — | — |

## Follow-up candidates

- Other documentation analyzers registered on comment nodes probably report twice on positional records / primary-constructor types; the guard could live in `RegisterSyntaxNodeActionWithDocumentationModeCheck` — 04-implementer.md (not run).
- RH8307 reports an element inside a `/** */` comment, which now has no fix while RH8304/RH8305/RH8308 do not report there; question: keep reporting without fix (A) or stop reporting (B, recommended) — 05-spec-author.md.
- RH8307 `ViolatesRule` and the documentation phase's `NeedsLineAlignmentNormalization` are two copies of one predicate (pre-existing); the fix now depends on them agreeing — 10-auditor.md.

## Worklist / next step

- Done: skeptic (01-skeptic.md, commit 67d5ea7).
- Q1 answered: A.
- Done: spec author (02, ae70663); continued once to narrow RH8309-007.
- Done: test implementer (03, 6f88024); all four reproduced, 90 failing rows.
- Done: implementer (04, 42eeedd): all handed tests pass; 2 RH8201 tests disputed; architecture finding.
- Done: spec author (05, ffbb0bb): RH8307-006, FMT-PIPE-032, positional-record examples.
- Done: test implementer (06, fa68bde).
- Done: implementer (07, 2ed4500): entry point added, copy removed, all tests pass.
- Done: cleaner (08, c6b0a5c, clean); spec checker (09, 1d852e3, consistent).
- Done: auditor (10, 9987064): findings #1 (low), #2 (medium). Audited SHA 1d852e3.
- Repair round 1 (of 3): spec author (11, 5edb2548), test implementer (12, 5672b193), implementer (13, 261ad231) done.
- Done: cleaner (14), spec checker (15, consistent), auditor (16, 8630b3ed): PASS. Audited SHA 54b50c9c.
- Next: validator (17) → lane check → issue manager (follow-ups) → note, delete sdd/work → trigger commit → PR body.
