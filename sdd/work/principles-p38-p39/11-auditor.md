Role: auditor
Commit: d8a838e7aaff2a3dfee1fa1bba5043856d642a24
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Repair delta for thoenissen/Reihitsu#1415 against `d74cc14` (the commit of `07-auditor.md`).
- Head `d8a838e7aaff2a3dfee1fa1bba5043856d642a24`. The checkout is that commit, the working tree is clean, and the branch on `origin` points to it.
- Base `c81091b883383d0735af167e15603fb702f1d7b4`. It equals `git ls-remote origin refs/heads/main`, is merged, and is unchanged since the first audit.
- `git diff d74cc14 HEAD` holds:
  - the repair-round section of `00-lead.md`;
  - `08-test-implementer.md`, `09-cleaner.md` and `10-spec-checker.md`;
  - one summary in `src/Reihitsu.Analyzer.Test/Ordering/RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.cs`.
- No spec, ledger, rule page, decision, production code, or test assertion changed. Scope did not grow.
- The text-only proof is my own run: `scripts/verify-text-only.sh --base d74cc14 --head worktree` gives `TEXT-ONLY PROOF: PASS`. All 3651 tokens are identical, directives and disabled text are identical, and the only changed trivia is documentation. Every other piece of evidence in `07-auditor.md` is byte-identical and reused.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | The lead classified it as pre-existing and recorded it as a follow-up, as `07-auditor.md` suggested. Comment 6370315578 on thoenissen/Reihitsu#1384 (2026-10-23T17:02) names the RH7103 input, the observed split, the expected unchanged field, every reproduced fix (RH7103; RH7109 and RH7102 through `TypeMemberOrderingCodeFixProviderBase`; RH7104; RH5110; RH8305 on the type's own summary), and asks for a code-fix regression test next to the formatter tests. That is enough for the follow-up to reproduce and pin the code-fix surface. `10-spec-checker.md` lists it with the known violations of the amended SHARED-FIX-003, and `00-lead.md` puts it in the run report. |
| 2 | closed | The summary no longer states the old coverage limit. It now says the comment still documents `Create` after the fix (the offered side of SHARED-FIX-003's fifth condition, which the test's tag covers), that the test runs in mode `Parse`, and that this input has no directive between the comment and `Create`, stated as a property of the input. The unasserted items (where the comment starts, layout) match SHARED-DOC-004. The assertions are unchanged, as the text-only proof shows. |

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| — | — | — | — | — | None. | — |

## Guard scope

_N/A — the repair adds or changes no predicate, guard, or exemption; its only compiled-file change is a documentation comment (text-only proof above)._

## Policy ownership

_N/A — the repair changes no policy; the owners recorded in `07-auditor.md` are unchanged._

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7103 `FixCarriesADocumentationCommentBehindCodeWithTheMovedDeclaration` (summary only) | Unchanged from `07-auditor.md`: after the move, the comment still documents `Create` | Unchanged | Unchanged: `ApplyCodeFixAsync` plus a reparse in mode `Parse` | adequate. The summary now describes exactly what the assertions check. |

## Failure classes dismissed

- FC-031: the one changed summary is checked above. The repair changes no other method.
- FC-037: the comment-only change is proven with `scripts/verify-text-only.sh`, not with `grep`.
- FC-032: no compiled behavior changed since the first audit, so the earlier build and test results stay valid. The cleaner's Release build (0 warnings) and the RH7103 class run (46 passed) are consistent with that.
- Every other class: dismissed for the reasons in `07-auditor.md`. The repair reaches no code, spec, or test assertion that those reasons depend on.

## Hints

- The new summary calls `Parse` "the mode in which the comment documents `Create`". SHARED-DOC-004 says the comment documents it in `Parse` and in `Diagnose`, so "a mode in which" would be exact. The sentence is still true of the mode the test runs in.
- The repair section of `00-lead.md` still reads as a plan ("the lead drafts a comment … for the maintainer to approve"). Only `10-spec-checker.md` records that the comment was posted.
- The RH8305 `no-progress` hint from `07-auditor.md` (a type whose documentation comment starts the file) is still unfiled. It stays a follow-up candidate.
