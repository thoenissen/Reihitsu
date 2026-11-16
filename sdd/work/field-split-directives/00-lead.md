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
- #1586 output question: open — to be asked before the test implementer.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

- Event field / local declaration: a declarator written after an `#endif` loses its continuation indent on `main` (01-spec-author.md).

## State

- Last step: spec author done (3b1b782), change doc `sdd/changes/2026-11/field-declaration-directives.md` Draft; #1586 output question drafted in 01-spec-author.md.
- Next: skeptic.
