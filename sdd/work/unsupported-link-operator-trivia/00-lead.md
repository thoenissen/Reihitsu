# Lead — unsupported-link-operator-trivia

- Issue: thoenissen/Reihitsu#894 — `agent-filed` (goals are proposals; the maintainer's triage comment on the issue is binding).
- Slug: `unsupported-link-operator-trivia`
- Branch: `claude/clever-mccarthy-d581sj` (session-designated). Draft PR: #1881.
- Lane: **behavior change**. Reason: declaring a new unsupported shape changes visible output (the formatter refuses and reports files it formats today, code fixes are withheld) and requires a change doc and a new FMT-UNSUP requirement.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped. Issue manager before publishing when follow-up candidates exist.

## Maintainer clarifications (from the issue, summarized)

The issue text proposes a new analyzer rule. The maintainer's triage comment on #894 (2026-11-17) overrides it:

- Do **not** add a new analyzer rule; the proposed rule and its "no code fix" section are dropped.
- **Declare the shape unsupported** in this issue's own change: a comment, preprocessor directive, or disabled text between the tokens of a member-access chain link operator (`?.`, `!.`, `!?.`, `?[`), e.g. `a? // note` ⏎ `.B()`. A new FMT-UNSUP requirement in `sdd/specs/formatter/unsupported-syntax.md`; the precheck refuses every input holding the shape; every formatting entry point returns it unchanged and reports it with reason and location; the CLI leaves the file unchanged and reports it; code fixes whose formatted node holds the shape are withheld (SHARED-FIX-010).
- The change deletes the special cases the shape makes unnecessary — named by the maintainer: the gap-keeping guards in `ChainLineBreakRewriter` (`TryCloseGap`/`CloseInnerGaps`) and the operator-column alignment in `MethodChainAlignmentContributor.AlignRestOfRun` — and the change doc lists them under *Special cases* (Deleted / Checked and kept).
- #893's second case (columns of trivia-kept pieces) becomes unnecessary; that run re-checks it (not this run's work).
- Ordering precondition: the run for #895 must be merged first — it is (PR #1870 merged).

## Maintainer answers

- Escalation 10 (declare the shape unsupported): answered before the run — **declare it, in this issue's change** (triage comment on #894, 2026-11-17).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## Open worklist / next step

- Next: spawn spec author (01).
