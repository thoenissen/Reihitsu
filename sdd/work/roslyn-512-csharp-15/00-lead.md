# Lead — roslyn-512-csharp-15

- Issue: thoenissen/Reihitsu#1761 — `agent-filed` (goals are proposals). Sub-issue of #1716 (C# 15 support); milestone 1.3.0.
- Slug: `roslyn-512-csharp-15`
- Branch: `claude/squad-1761-nz1rw8` (the session's designated branch, used instead of `claude/issue-1761-<slug>`; see decision 1)
- Draft PR: #1779
- Lane: **behavior change**. Reason: the issue raises the formatter's supported language version (`LanguageVersionResolver.MaxSupportedLanguageVersion`) and the CLI help text, which is shipped source with a visible effect (C# 15 input is now formatted at its version; the help text changes), plus package references in project files (tooling paths). Shipped source next to tooling needs a change doc, so only the behavior-change lane admits it.
- Roles: spec author → skeptic → (output questions) → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.

## Maintainer clarifications

None beyond the issue text.

## Maintainer answers

Batch 1 (before test implementer):
1. Default language version becomes C# 15 (help `latest selects 15.0`, `preview` warning names 15.0, `--lang-version 15`/`15.0` accepted): **A**, raise the maximum.
2. Interim output of a one-line union (braces stay as the phases give them, e.g. `{ public override string ToString()` … `} }`): **A**, accepted until #1762.
3. C# 14 code with a type named `union`/`closed` in positions C# 15 reads as keywords is skipped as syntax errors under the default; `--lang-version 14` restores the old result: **A**. Addition: if any existing test uses such a type name and breaks, fix that test by renaming the type.
4. Analyzer package's minimum host compiler rises to Roslyn 5.12 (release-note item for 1.3.0): **A**.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/squad-1761-nz1rw8` | The session environment allows pushes only to that branch | Branch names are internal | claim commit |
| 2 | Skeptic | Issue goal "update the CLI help text" | Drop as a code task; only tests/specs follow | Help text is derived from the constant (02-skeptic) | The help line still changes via the constant and is asked as an output question | — |
| 3 | Skeptic | Issue goal "update the two LanguageVersionResolver tests" | Widen to every test that uses `(LanguageVersion)1500` as above-maximum, incl. `ResolveClampsVersionsAboveTheMaximum` (FC-017) | 1500 is now CSharp15 | Tests are internal | — |
| 4 | Skeptic | Issue goal "test infrastructure can compile a union" | Drop from this run; carry the facts to the first union sub-issue as a follow-up | Nothing in this issue compiles a union; Testing 1.1.4 knows only up to net10.0 | Test infrastructure is internal | — |
| 5 | Skeptic | Change-doc wording ("valid C# 14 input formatted as before"; `closed` type names) and FMT-PIPE-008 `Why:` | Amend as the skeptic recommends (after the maintainer's answers) | The old wording is false for `union`/`closed` type names | Spec/change-doc wording is internal; the visible cases are asked | — |

## Follow-up candidates

- Blank line inserted after `(` in front of a first comment in a record/union parameter list (FMT-BLANK-011 exempts only blocks/type bodies/switch sections) — open output question for the maintainer, carried in the issue. Source: 02-skeptic.
- Union test infrastructure facts (CS0518/CS0656 against net10.0 refs; clean against SDK net11.0 refs; Testing 1.1.4 only up to net10.0) — extend the first union sub-issue of #1716. Source: 02-skeptic.

## Worklist / next step

- Done: 01-spec-author (change doc, commit 4cd929a), 02-skeptic (db5ecf9).
- Batch 1 answered (1A 2A 3A+rename 4A).
- Done: 03-spec-author (b9ad11e); change doc Accepted by the lead.
- Next: test implementer (04).
