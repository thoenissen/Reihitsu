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
- Audit 2 (17-auditor.md): findings #1–#3 closed; #4 (RH6001) repeats #2's class → escalation 12, B: full sweep of analyzer/code-fix line-break copies decided by trivia kind, every failing member routed through the shared fact; not a repair round (`squad/decisions/2026-10-31-line-start-fact-audit-2-repeat.md`). Order: test implementer (enumeration and reproductions) → spec author (change-doc listing) → implementer → cleaner → spec checker → auditor on the structural change → validator.
- Spec checker 22 finding (lead-owned): RH6001 checks `fixed`, which RH6001-001 does not list; predates the run, so it is a follow-up candidate drafted for the maintainer, not changed here.

## Follow-up candidates (drafted for the maintainer, not created)

- RH5204 fix does not converge (behind a stranded `///` with an over-indented block; at the start of a file behind a doc comment or directive).
- RH5204 does not report a misindented field whose first token is a predefined-type keyword (`int B;`).
- The documentation-comment phase relocates `///` comments that FMT-DOC-001 leaves undecided (spec gap).
- RH5048 treats every documentation-looking comment as documentation, while SHARED-COMMENT-001 counts a `///`/`/** */` that documents nothing as ordinary.
- `AttributeTargetUtilities.StartsLine` and the terminator sets of RH5202, RH5401, RH5603, RH8402, and the copyright-header helpers (decision 3A).
- `ListElementInteriorJoinRewriter`'s tight-pair table for pairs no spacing rule covers (join-spacing-pairs decision).
- `Data/RootListPosition` reads a line-break-phase helper.
- RH6001 checks `fixed`, which RH6001-001 does not list.
- Audit 3 (23-auditor.md, of ba5a99b): #4 closed; enumeration complete; finding #5 (medium, missing test for `if/* c *//// x` / `(c)`, not a repeat). Repair round 2 of 3: test implementer, cleaner, spec checker, auditor on the repair, validator.
- Follow-up candidate from audit 3: RH6001 reports `if/** d */(c)`, which the formatter keeps; RH6001's spec leaves the shape undecided.
- Audit 4 (27-auditor.md, of f57c5d7): pass, text cleanup (#5 closed; #6 low, wording of four RH6001 test summaries). Cleaner reworded them (28-cleaner.md); `verify-text-only.sh --strict-docs` fails only because the summaries sit on public test methods (non-strict proof passes, token stream identical). Per the playbook a failing proof makes this repair round 3 of 3: spec checker, auditor on the text change, validator.

## After publication (PR thoenissen/Reihitsu#1510, CI green on a681e8e)

The maintainer answered the follow-up list:
- Follow-ups are created and linked: new thoenissen/Reihitsu#1511 (RH5204 fix at the start of a file) and thoenissen/Reihitsu#1512 (remaining analyzer-side line-start and terminator copies); existing issues reused: #1157 (RH5204 `int B;`), #1386 (RH5048), #962 and #1491 (documentation-comment phase), #1493 (join tight-pair table), #1496 (`RootListPosition`).
- RH6001 is intended to check `fixed`: RH6001-001's keyword list gains `fixed` in this PR (a clarification of existing analyzer behavior, no change doc), and the existing `fixed` test gains its tag.
- RH6001 reporting `if/** d */(c)` stays as it is; no follow-up.

Roles for this addition: spec author (spec), test implementer (tag), cleaner, spec checker, auditor (compiled test change after the audit), validator. Not a repair round.
- Audit of the addition (05-auditor.md): finding #7 — the formatter keeps `fixed(`, so formatted code raises RH6001. Maintainer: A, `fixed` joins FMT-SPACE-012 and the formatter's keyword spacing; change doc accepted in advance (`squad/decisions/2026-10-31-line-start-fact-fixed-spacing.md`). Not a repair round: a maintainer-chosen extension. Roles: spec author (change doc, FMT-SPACE-012), test implementer, implementer, cleaner, spec checker, auditor, validator.
- Audit 12 (of 3815e6e): #7 closed; #8 (medium) — buffer with a comment against the type changes output (`public fixed/* c */int` becomes `public fixed/* c */ int`), the change doc claims buffers unchanged, no test. Routed: spec author corrects the change doc (within the maintainer's advance acceptance of option A, since the output is what FMT-SPACE-012/013 already require), test implementer adds the test.
