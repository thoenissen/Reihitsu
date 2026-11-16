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
| 8 | Audit 3 | #3 repeats the class: guard predicts which declarations the compiler joins | Architecture finding, refuse wider: whenever the union symbol has more than one declaration, read every declaration, whatever its modifiers; spec ("one of its declarations") already demands it, so no spec change. #4 is accepted as dropping once #3 is refused before the probe; (b) stays a safety net without a falsifying input | Removes a prediction instead of adding one; cheap | Input does not compile; not reported, as on `main` | a9ee349 |
| 7 | Decision 6 impl | Probe expands a union's documentation twice; first catch-all in the analyzer | Accept both | Cost only for unions; the catch-all is decision 6 (b), narrowing it to one exception type would predict the compiler again; `OperationCanceledException` still propagates | Internal | 3a41188 |
| 6 | Audit 2 | #2 repeats #1's class (guard narrower than the compiler's binding) | Architecture finding, refuse the class wider: (a) the refused class is any `param`/`paramref` element whatever the case of its tag name, with tag lookup kept to one owner; (b) no union may reach AD0001 through the compiler's documentation expansion — if the expansion fails for a union, that union is not judged. Spec wording also states the failure is in Diagnose mode. Mechanism left to the owners | README: a repeated class is refused, not repaired member by member; (b) ends the loop of predicting the compiler | Rare class (compiler error CS1569 with documentation generation); not reported, as on `main` | – |
| 5 | Repair 1 | Partial union where another part's doc holds `param`/`paramref` (may still crash) | Widen the refusal of decision 4 to cover it: no crash, the union is not judged when any part's documentation holds such an element | Same rare class (decision 4); a crash must not ship | Refusing a rare class; nothing visible in question | 97c083f, 7d03636 |
| 4 | Audit 1 | AD0001 on a union whose doc comment holds `param`/`paramref` | Refuse the class: the affected rules must not crash and do not judge that union's documentation; recorded in the rule specs; mechanism left to the implementer | Rare input class: the same input already makes the compiler fail with CS1569 when the documentation file is generated (AGENTS.md, Code-fix coverage; README, Architecture findings) | Refusing a rare class puts nothing in question (README, What reaches the maintainer); named under Review notes | – |
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
- Cleaner done (06-cleaner.md, 2005af3): clean, Release build 0 warnings. 
- Spec checker done (07-spec-checker.md, 7eafd21): consistent. - Auditor 1 (08-auditor.md, audited SHA 3ccbe182cd0d6d85f8d92891c0393a169d185d7f, main 1d2d29a): findings — #1 high, PR-introduced: RH8035/RH8036/RH8108/RH8203 throw AD0001 (Roslyn NRE in GetDocumentationCommentXml) when a union's doc comment holds `<param name>`/`<paramref name>` in Diagnose mode.
- Repair round 1 (of 3): decision 4. Spec author done (09-spec-author.md, 85c14f1): new SHARED-DOC-019, used by RH8035/RH8036/RH8108/RH8203. Test implementer done (10-test-implementer.md, 50a4fc7): 24 tests, 17 failing as intended. Implementer done (11-implementer.md, f89f746): cheap syntax check `IsUnionWithParameterElement`, early return in RH8035/RH8036 base, RH8108, RH8203; analyzer tests green; architecture: 1 failure (SHARED-DOC-019 example cites RH8035-001/RH8036-001/RH8203-001 not in its Uses row). Implementer notes a partial union with `<param>` in another part may still crash RH8203 (decision 5).
- Round 1 continued: spec author 97c083f (Uses fixed, partial unions in SHARED-DOC-019), test implementer a2ac7d9 (7 partial-union tests), implementer 7d03636 (checks all parts of a partial union; analyzer 10217 + architecture 118 green). Cleaner 12 (dadee73) clean; spec checker 13 (46cf659) consistent. Auditor 14 (5389ddd, audited SHA 28450b6): #1 closed; new #2 high, repeats #1's class: the guard matches only lower-case `param`/`paramref`, but the compiler binds any tag-name case (`<Param>`, `<PARAMREF>`) → AD0001 again. Handled as an architecture finding (decision 6), not a repair round (rounds used: 1 of 3).
- Decision 6 in progress: spec author 15 (1a32759), test implementer 16 (9283c29, 20 tests, 14 failing as intended), implementer 17 (3a41188: case-insensitive lookup via `GetTagsIncludingNested`, `CanExpandDocumentation` probe; analyzer 10237 + architecture 118 green). Next: cleaner (18) → spec checker (19) → auditor (20, repair only, base 28450b6) → validator. Cleaner 18 (dd1026e) clean, moved a method for S4136.
- Auditor 20 (a0cfb73, audited SHA cbac9ac): #1, #2 closed. #3 high, repeats #2's class: the guard reads other declarations only when the analyzed one carries `partial`; the compiler joins same-named declarations without it (CS0260/CS0101) → RH8035 reports under Parse. #4 medium: SHARED-DOC-019 (b) has no falsifying test; drops if #3 is refused before the probe. Decision 8.
- Test implementer 21 (9519c00): 10 tests, 5 Parse-mode failing as intended. Implementer 22 (a9ee349): partial condition dropped; analyzer 10247 + architecture 118 green. Cleaner 23 (c0d32d5) clean; spec checker 24 (e7d6a5a) consistent. Next: auditor (25, base cbac9ac) → validator. Repair rounds used: 1 of 3.
