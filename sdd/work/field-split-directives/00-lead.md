# Lead — field-split-directives

- Issues: thoenissen/Reihitsu#1728 (`agent-filed`), thoenissen/Reihitsu#1586 (`agent-filed`). Goals of both are proposals.
- Branch: `claude/laughing-thompson-r56h99` (session-designated). Draft PR: thoenissen/Reihitsu#1809.
- Slug: `field-split-directives`
- Lane: behavior change.
  - #1728: requests new behavior — declare a directive or disabled text inside a multi-declarator field declaration an unsupported shape (new FMT-UNSUP requirement), deleting the field split's directive special cases. Needs a change doc.
  - #1586: requests changed behavior — split a multi-declarator field whose directive sits in front of the declaration (e.g. a `#region`), formatter and RH7101 fix. Needs a change doc.
  - Shared mechanism: the field split's directive guard (FMT-STRUCT-023, `FieldDeclarationSplitTransform.CarriesDirective`, FMT-PIPE-031, RH7101-005).
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publish if follow-up candidates exist. None skipped.

## Maintainer answers

- Escalation 10 for #1728 (carried by the issue text, which the maintainer filed as the outcome of an earlier run): declare a directive or disabled text inside a multi-declarator field declaration unsupported, in a change of its own (this run). Summarized: "no directives in declarations".
- Q1 (#1586, inputs: region/`#if !RELEASE` in front; inactive `#if DEBUG` attribute in front): answer **A** — split when a directive stands in front; keep whole, fix withheld, when disabled text stands in front. Not declared unsupported (C, D rejected).
- Q2 (#1728 confirmation): answer **A** — declare a directive or disabled text inside a multi-declarator field declaration unsupported, in this change; CLI reports it, fixes formatting it are withheld.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Spec author | One change doc for both issues; RH7101-005 changed in place; no initializer exception; FMT-UNSUP-009 reason wins at the tie | As recommended | Shared cause; consistent with FMT-UNSUP-004/009 | Internal spec structure; the span was the maintainer's answer | 3b1b782 |
| 3 | Audit 2 | Finding #1 repeats audit 1's class (tie order untested; a missed member) — architecture finding? | Treat under *Architecture findings*: fix narrowly — the test implementer enumerates every tie member of both candidate lists by cause, not the one cell; the auditor's single-list restructuring hint is not taken (cosmetic, adds no safety) | No rare input class to refuse; the code is correct; a test closes the gap without production growth | Reason text is already decided by FMT-UNSUP-010 | f6df1fa |
| 4 | Repair 2 | D2: a directive before a declarator comma of a local inside a field value — FMT-UNSUP-010's "every other directive" could be read to give 010's reason | Keep FMT-UNSUP-009's reason (the code's output, and `main`'s output for this input); spec author adds one clarifying sentence | Matches FMT-UNSUP-009's first bullet and the code | The reported reason equals `main`'s for this input (CLI run: FMT-UNSUP-009 reason at 8,1); no output changes | 098cfeb |
| 2 | Skeptic | Disabled text in the first token's leading trivia (attribute/modifier of another build) would be split onto the first field only | Keep the declaration whole and withhold the RH7101 fix for that class (today's output) | Rare class; FMT-PIPE-003/FMT-STRUCT-024 forbid the split; one cheap check | Today's output stays for that class; refusing a rare class is not in question | b48ac9c |

## Follow-up candidates

- Event field / local declaration: a declarator written after an `#endif` loses its continuation indent on `main` (01-spec-author.md).
- S1125 warnings in `src/Reihitsu.Cli/FormatCommandHandler.cs` (490, 501, 555) seen in the format script's Debug build, pre-existing on `main` (07-cleaner.md).

## State

- Last step: spec author done (3b1b782), change doc `sdd/changes/2026-11/field-declaration-directives.md` Draft; #1586 output question drafted in 01-spec-author.md.
- Skeptic done (d361337): #1728 proceed, #1586 proceed narrower.
- Amendment done (b48ac9c).
- Now: asked #1586 output question with option D (escalation 10), then test implementer. Answers received: 1A, 2A.
- Change doc Accepted (5fb97e9).
- Test implementer done (707b6b8 tests, 505948e handover).
- Implementer done (f5afc3a, handover bb66d1c); tie reason at non-declarator comma / FMT-UNSUP-008 follows FMT-UNSUP-010 text.
- Cleaner clean (37c1315).
- Spec checker consistent (cce9f0a). Build green. Audit base main=eb89003, head cce9f0a.
- Audit 1 (e3a5531): #1728 findings (#1 medium: tie-reason order untested; #2 low: AGENTS.md structural row lacks HasDisabledTextInLeadingTrivia); #1586 pass.
- Repair round 1: #1 → test implementer (10), #2 → spec author (11); then cleaner, spec checker, auditor (repair only), validator.
- Repair round 1 done: tests ec05997 (10), text fbf5ac8 (11). Cleaner (12), spec checker (13) clean. Audit 2 (dd41b4e): #2 closed; #1 still open for the declarator-comma candidate list (FMT-UNSUP-010 vs FMT-UNSUP-008 order untested).
- Repair round 2: test implementer (15) enumerates every tie across both candidate lists; then cleaner, spec checker, auditor on repair.
- Repair round 2 tests done (f6df1fa, 15). Spec clarification D2 → spec author (16); then cleaner (17), spec checker (18), auditor (19).
