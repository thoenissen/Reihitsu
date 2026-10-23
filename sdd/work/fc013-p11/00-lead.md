Role: lead
Commit: c81091b
Verdict: run claimed

## Run

- Issue: thoenissen/Reihitsu#1419 — FC-013 and skeptic check 3: leave out the cases P11 calls intended.
- Slug: `fc013-p11`
- Branch: `claude/stoic-wozniak-hg1i0b` (the session's mandated branch, instead of `claude/issue-1419-<slug>`; same as the precedent run thoenissen/Reihitsu#1413).
- Draft PR: thoenissen/Reihitsu#1421, based on `main`.

## Lane

Tooling — the issue changes squad instructions (`squad/failure-classes.md`, `squad/roles/skeptic.md`), no behavior change.

Roles: implementer, cleaner, spec checker, auditor, validator. Skipped: spec author and skeptic (not run by the tooling lane); test implementer (no test covers these Markdown files).

## Maintainer's clarifications (summarized)

- Narrow FC-013 (`squad/failure-classes.md`) and check 3 (`squad/roles/skeptic.md`) so they agree with `sdd/README.md`, section "The formatter may do more than the analyzers (P11)". Analyzer-clean code that the formatter changes is neither a failure nor a conflict where P11 calls the rewrite intended.
- The skeptic still names such a case in its handover and classifies it as P11 does: the rule's own gap is a bug or behavior change of that rule; a different construct is a new-rule candidate, mentioned in the run report, with no issue created.
- "Formatter output is flagged by the analyzer" stays a failure; cite SHARED-DIAG-010 (P31: a layout rule does not report a shape the formatter keeps) as its counterpart.
- Both texts link the P11 section by its anchor.
- Must not: change other failure classes or other skeptic checks; change code, specs, or tests; touch `sdd/` outside `sdd/work/` (thoenissen/Reihitsu#1415 runs in parallel on `sdd/specs/`); create issues without the maintainer's approval of the exact text.
