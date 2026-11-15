Role: spec checker
Commit: bd57a14
Verdict: consistent

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. Bug fix and clarification of SHARED-CASING-009; no change doc, none in the diff; decision 15 records a withheld fix for a rare class under unchanged SHARED-FIX-003.
2. Specs match the diff: ok. The allow-list (properties, events, ordinary methods) in the spec paragraph and Why matches `symbol is not (IPropertySymbol or IEventSymbol or IMethodSymbol { MethodKind: MethodKind.Ordinary })`; the type-rename and positional-parameter consequences match the code; the RH4002 and RH4120 examples have the required markup, before/after, and text.
3. IDs: ok. No new ID; `requirement-ids.txt` untouched.
4. Code owners: ok. Spec headers and `sdd/specs/README.md` untouched; architecture project run below.
5. User documentation: ok. No rule page touched; the gap is covered by `general-notes.md`.
6. Open questions: ok. 26-spec-author, 27, 28 report none; decisions 14 and 15 are recorded in 00-lead.md.
7. Coverage: ok. `scripts/test.sh --project architecture`: Passed! - Failed: 0, Passed: 118, Skipped: 0, Total: 118. SHARED-CASING-009 is covered by tagged tests (RH4002 t1, t2, RH4003 t5, RH4120 pair, and earlier rounds).
8. Tags: ok. The five new tests carry SHARED-CASING-009 (the RH4120 no-fix test also SHARED-FIX-003) as 27-test-implementer lists; every example named under Examples for the tests is tagged on a new test.
9. Nothing left behind: ok. Only sdd/work/casing-rename-call-sites exists; the lead deletes it before publishing.
