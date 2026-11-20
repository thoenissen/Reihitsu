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
| 1 | Spec author | New requirement or widen FMT-UNSUP-014? | New FMT-UNSUP-015 | Separate shape (link operator vs. attached part); keeps each requirement one check | Spec structure only | 568f167 |
| 2 | Spec author | Does FMT-UNSUP-015 cover `?[`? | No, FMT-UNSUP-014 already covers it | `?[` is an attached part, not a link (SHARED-CHAIN-001) | Output already refused by FMT-UNSUP-014 | 568f167 |
| 3 | Spec author | Change SHARED-CHAIN-006 / RH5201-003? | Leave unchanged | Not contradicted by the new shape | No output change | 568f167 |
| 4 | Skeptic | Is a one-line block comment inside the operator (`s? /* c */ .Length`) part of the shape? | Yes — not asked | Maintainer's triage wording "between the tokens" has no line-break condition; matches FMT-UNSUP-008/011/014 | Covered by the maintainer's answer | 568f167 |
| 5 | Skeptic | One definition of the link-operator tokens shared by precheck and chain model (A) or separate definition pinned by a full matrix test (B)? | A where it stays a cheap check; otherwise B with the full operator × trivia-side matrix test | Removing the in-phase guard makes comment safety rest on the precheck matching `FluentChainLink.OperatorTokens` (FC-004) | Internal structure | — |
| 6 | Lead | Accept the change doc? | Accepted after the batch-1 answers; spec author sets `Status: Accepted` in its next follow-up | Skeptic found no premise defect | — | — |

## Follow-up candidates

- Escalation 10 candidate (01-spec-author.md): comment between a link's `.` and its member name (`a.B(). // c` ⏎ `C();`) — declare unsupported in a change of its own? Ask with the next batch.

## Open worklist / next step

- Done: spec author (01, 568f167), skeptic (02, 49f7b66; verdict proceed, premise and reachability confirmed, `?[` already covered by FMT-UNSUP-014).
- PR review note candidate: the visible cost is a comment justifying a `!` behind it in a wrapped chain (`x! // why` ⏎ `.Y()`); a comment in front of the `!` stays supported.
- Spec-checker wording point: FMT-UNSUP-015's "meets FMT-UNSUP-004 only outside a value" may be vacuous (02-skeptic.md).
- WAITING on batch 1 (asked in chat): (1) CLI reason text for FMT-UNSUP-015; (2) escalation 10 on the `.`-to-member-name gap.
- Next after answers: spec author follow-up (Status Accepted, reason text per answer) → test implementer (03).
