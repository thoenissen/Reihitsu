# Lead — indent-spec-clarifications

- Issues: thoenissen/Reihitsu#1400 (`agent-filed`, goals are proposals), thoenissen/Reihitsu#1936 (`agent-filed`, goals are proposals)
- Slug: `indent-spec-clarifications`
- Branch: `claude/practical-lovelace-ky737o` (designated by the session)
- PR: thoenissen/Reihitsu#2002 (draft claim)
- Lane: **tests** (claimed docs, widened after the spec author; decision 2)
  - #1400: spec text only in `sdd/specs/formatter/indentation.md` (FMT-INDENT-010, FMT-INDENT-028) and `sdd/specs/formatter/horizontal-spacing.md` (FMT-SPACE-001/-013); the issue confirms today's behavior in every case and changes only spec text. Its "Done when" allows a characterization test where no existing test covers the confirmed behavior; if one is needed, the lane widens to tests.
  - #1936: the `Why:` text of FMT-INDENT-021 in `sdd/specs/formatter/indentation.md`; text only. The issue says "Lane: wording", but it changes no diagnostic or code-fix text, so by the lane-check table it is docs.
- Roles: spec author → (output-question batch, if any) → spec checker → validator only if a compiled file changed. Skipped: skeptic, test implementer, implementer, cleaner, auditor (not in the docs lane).

## Maintainer clarifications

- #1400: an issue comment (phase 6 inventory, 2026-11-16) reports item 24 already done: FMT-SPACE-001 states that a line break inside a multi-line comment's text is not a line break of the gap. Items 22 and 23 remain.
- #1936: an issue comment moved it into this run with #1400 because both edit `indentation.md`.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Lane | #1936 says "Lane: wording" | Docs lane | It changes spec text only, no resource text | Spec text is internal | 48e5073 |
| 2 | Spec author | Spec author needs three characterization tests (FMT-INDENT-010 two examples, FMT-INDENT-028 second example) | Widen lane docs → tests; add skeptic, test implementer, cleaner, validator | No existing test pins the off-column inactive region, the tab in front of it, or `static int (x)`/`static x` block lambdas; #1400 "Done when" allows characterization tests | Tests pin today's output; no output changes | d9ba2be |
| 3 | Spec author | Item 24 left unedited | Accept | FMT-SPACE-001 already states it (issue comment of 2026-11-16); pinned by `MultiLineCommentGapSpacingTests` | Spec text only | d9ba2be |
| 4 | Spec author | Inactive region called "a directive, not disabled text"; FMT-INDENT-021 `Why:` points at FMT-INDENT-034 | Accept | Matches the parser's model and the existing continuation rule | Spec text only | d9ba2be |

## Follow-up candidates

(none yet)

## Worklist / next step

- Claim done (48e5073, PR #2002). Spec author done (d9ba2be): verdict needs a test; no output or open questions, so no output-question batch. Lane widened to tests (decision 2). Next: skeptic, then test implementer, cleaner, spec checker, validator.
