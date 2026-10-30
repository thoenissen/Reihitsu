# Lead — line-start-fact

- Issue: thoenissen/Reihitsu#1487 (formatter restructuring package B2 of thoenissen/Reihitsu#1482)
- Slug: `line-start-fact`
- Branch: `claude/great-archimedes-xwz86i` — the session that runs the squad is bound to this branch and may not push to another, so the playbook's `claude/issue-<N>-<slug>` name is not used.
- Draft PR: thoenissen/Reihitsu#1510
- Blocker thoenissen/Reihitsu#1485 (B1) is closed, merged with thoenissen/Reihitsu#1508; `main` at 802afbf.

## Lane

Behavior change. The issue requests changed formatter behavior (FMT-INDENT-002 places the line behind a `///` written behind code; FMT-SPACE-015 is retired; FMT-DOC-003's second example changes) and states that a change doc under `sdd/changes/` is required. Two of its cases (FMT-INDENT-015 zero-width tokens, FMT-INDENT-006 comment interiors) are bugs against existing requirements and ride in the same run.

Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.

## Clarifications from the maintainer

None so far; the issue has no comments. The issue asks the run to first check every statement about today's code against current `main` and drop what an earlier package already made true or moot.

## Escalations

- Escalation 2 — change doc approved as written, analyzer consequences included (`squad/decisions/2026-10-30-line-start-fact-change-doc.md`).
- Escalation 12 — skeptic's architecture finding: option B. The line-break rewriters for conditionals, conditional-access chains, and do-while, and the line-terminator count over disabled text, are routed through the shared line-start fact and terminator predicate in this run, with tests (`squad/decisions/2026-10-30-line-start-fact-architecture-finding.md`).

## Scope baseline (escalation 5)

The issue's Proposed Behavior and Done when — including its restructuring items: the shared facts in `Pipeline/Core`, the neutral-helper moves, the `ConfigurableStructuralTransforms` move to `Data/`, the `ReihitsuFormatterHelpers` split, and the one-contributor-per-line test — plus the components added under escalation 12.
- Escalation 9 — `#endregion` behind the last `}` without a final line break: fixed in this run (`squad/decisions/2026-10-30-line-start-fact-end-of-file-region.md`).
- One-contributor-per-line test: written by a second test-implementer pass after the implementer, against the restructured layout model (`squad/decisions/2026-10-30-line-start-fact-one-contributor-test.md`).
- Escalation 12 (implementer findings): 1B `SpacingPolicy` and the unary-operator guard move to `Pipeline/Core`; 2B a wrapped list of omitted type arguments is joined per FMT-BREAK-014 — change doc, FMT-INDENT-015 example, and zero-width tests are amended (change-doc amendment goes back to the maintainer, escalation 2); 3A `AttributeTargetUtilities.StartsLine` and analyzer-side terminator sets are follow-ups (`squad/decisions/2026-10-30-line-start-fact-implementer-findings.md`).
- Escalation 2 (amendment): approved (`squad/decisions/2026-10-30-line-start-fact-amended-change-doc.md`).
- Escalation 12 (contributor overlap, spacing move): 1A one contributor per line with unchanged output; 2B the join stops computing spacing and leaves the gap to the horizontal-spacing phase, fallback to moving policy and rules (`squad/decisions/2026-10-30-line-start-fact-contributor-and-spacing.md`).
- Standing instruction from the maintainer for the rest of the run: every escalation takes the lead's recommended option, and any further change doc is provisionally accepted; the run continues to the published PR, and each such decision is recorded and listed in the run report.
- Escalation 12 (join tight-pair table): A — kept in this run, follow-up candidate (`squad/decisions/2026-10-30-line-start-fact-join-spacing-pairs.md`).
- Audit 1 (12-auditor.md): findings. Escalation 12 for finding 2 — B, both copies go through the shared fact (`squad/decisions/2026-10-30-line-start-fact-audit-1-copies.md`). Repair round 1 of 3: findings 1 (blank line in a wrapped omitted-type-argument list needs two passes), 2 (RH6021/RH6005 copies), 3 (`ReihitsuFormatterHelpers.ComputeTokenColumn` left in the entry-point class) — test implementer, then implementer, cleaner, spec checker, auditor on the repair, validator.
