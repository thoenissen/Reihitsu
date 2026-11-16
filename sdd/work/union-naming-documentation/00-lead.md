# Lead — union naming and documentation

- Issue: thoenissen/Reihitsu#1766 — `agent-filed` (goals are proposals). Parent #1716; dependency #1761 is closed (merged in #1779, Roslyn 5.12, C# 15).
- Slug: `union-naming-documentation`
- Branch: `claude/squad-1766-qkoyls` (the session's designated branch; see decision 1)
- Draft PR: #1784
- Lane: **behavior change / new rule** — the issue requests new analyzer behavior for union declarations (naming and documentation diagnostics), probably new rule IDs; that is visible output and needs a change doc.
- Roles: spec author → skeptic → (output questions) → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.
- Maintainer clarifications: none beyond the issue.

## Maintainer answers

- Batch 1 (Q1–Q5 of 01-spec-author.md): Q1 A (new rules RH4012, RH8035, RH8036), Q2 A (parameter rules do not check unions), Q3 A (RH8030 reports an empty union summary), Q4 A (RH8108–RH8111 check union type parameters).
- Q5 (RH8203 on unions): not answered with an option. The maintainer asked whether `<inheritdoc/>` is meant only for members and stated that a type should carry its own documentation. Asked back with options A/B/C. Answer: **5A** — `<inheritdoc/>` on a type may be used but is not forced; reported only on a union without a base list (as written). Question 6 (stricter RH8203 for other types) is moot; no follow-up.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 3 | Cleaner | S1125 warning in unchanged `FormatCommandHandler.cs:501`, seen only in the format-script build | Not a follow-up | Not reported by the Release build; unrelated to the run | Internal build warning | – |
| 2 | Skeptic | Spec examples with union `Value` | Fresh spec author amends examples (original agent large: 61 tool uses) | Examples must compile; no rule output changes | Rename of example identifiers only | 5019e1a |
| 1 | Claim | Branch name | Use `claude/squad-1766-qkoyls` instead of `claude/issue-1766-<slug>` | The session environment permits pushing only to this branch | Branch name is internal | 81a7af9 |

## Follow-up candidates

- Stale comment in RH5404 analyzer (unions parse only under preview) — belongs to #1762 (union layout), not a new issue (01-spec-author.md).

## Worklist / state

- Spec author done (01-spec-author.md, commit 2a43412): change doc `sdd/changes/2026-11/union-naming-documentation.md`, new RH4012/RH8035/RH8036, 5 output questions. Lane check confirmed behavior-change.
- Skeptic done (02-skeptic.md, 2ce1739): premise holds, agrees with A on Q1–5; three spec examples name a union `Value` (CS0542) — amendment done by a fresh spec author (03-spec-author.md, 5019e1a; also fixed RH8035 rule page example, CS0246). Recommends a source polyfill (IUnion, UnionAttribute) for analyzer tests.
- Batch 1 answered and recorded in the change doc (cd23842); lead set the change doc Accepted (efe4373).
- Test implementer done (04-test-implementer.md, 8e6503d): new tests for RH4012/RH8035/RH8036, extended RH8030, RH8101–RH8111, RH8203, RH8020/21, RH4105/RH4122; test-only union polyfill assembly `Base/UnionSupport.cs`. 
- Implementer done (05-implementer.md, 066f383): 4 new rule types on existing bases, union added to DocumentableDeclarationKinds and TypeParameterOwnerKinds, resources, ruleset, sln, package README; full analyzer + architecture tests green in Debug. 
- Cleaner done (06-cleaner.md, 2005af3): clean, Release build 0 warnings. Next: spec checker (07).
