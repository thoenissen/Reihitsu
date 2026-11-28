# Lead — region-column-nested-rows

- Issue: thoenissen/Reihitsu#1399 (`agent-filed`: goals are proposals)
- Slug: region-column-nested-rows
- Branch: claude/epic-rubin-2swis1 (session-designated); draft PR thoenissen/Reihitsu#1995
- Lane: behavior change — the issue requests changed formatter output (region directive column inside initializers, collection expressions, switch expressions, argument lists, patterns; nested array-initializer rows laid out like complex element initializers) that FMT-INDENT-010 / FMT-INDENT-023 do not state today, so a change doc is required.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.
- Issue context (summarized): the issue says the two decisions were taken in thoenissen/Reihitsu#912 (round 9 items 20, 21, both option b). Two later comments add instances: list patterns and parenthesized patterns wrapped over several lines (item 20 shape), and a region inside a switch *section* where `#endregion` stands four columns left of `#region` (not checked whether FMT-INDENT-010 decides it).

## Maintainer answers

- Batch 1 (after skeptic), answered:
  - Q1–Q5 (region directive columns inside initializers, patterns, lists, switch sections): a region inside a member is to be declared **unsupported** (escalation 10, answered: declare). Scope of "inside a member" and whether it is declared in this run are asked back (batch 1b).
  - Q6 (row opening behind `}, {`): neither option fits. Rule given instead: each initializer is either single-line or multi-line, judged per initializer when nested; a multi-line initializer puts its `{` and `}` on their own lines. Its reach (rows only vs every array initializer) is asked back (batch 1b).
  - Q7, Q8: not answered; Q8 becomes moot under the declaration (fixes withheld, SHARED-FIX-010). Q7 (comment-led row brace) is a rare class: lead leaves it to local refusal / asks again only if needed.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

- F1 (01-spec-author): blank line inserted after an opening `[`/`(` before a `#region` and before a closing `]`/`)` after an `#endregion`, but not for `{`/`}` — blank-line phase.
- F2 (01-spec-author): a region inside a wrapped method chain, binary expression, or query takes the statement column; spec leaves it unspecified.
- F3 (02-skeptic): RH7304 vs FMT-INDENT-010 for regions crossing a construct's end (attribute list `)]`) or in a base list; RH7304's `IsWithinElementBody` treats initializers/parameter lists/attribute args/base lists as between declarations, which SHARED-TYPEREGION-005 does not state.

## State

- Last step finished: spec author (01, c9404e4; change doc Draft; open output question 1 + several change-doc entries needing answers).
- Skeptic (02, c1f75f2): verdict output question; premise holds, reachable via CLI; no architecture findings; escalation 10 not an option (deletes no special case).
- Batch 1 asked and answered (see above). Waiting: batch 1b (scope of the declaration, timing, reach of the initializer line rule).
- Old: output-question batch 1 (Q1 scope A/B, Q2 switch-section #endregion, Q3 anonymous object, Q4 patterns, Q5 other aligned lists, Q6 entry 9, Q7 entry 10, Q8 code-fix results). Next after answers: route to spec author, then test implementer.
