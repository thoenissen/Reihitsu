Role: spec checker
Commit: d99f68a
Verdict: findings

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 2 | `sdd/specs/shared/casing.md`, SHARED-CASING-009, first bullet ("inside the renamed type itself, whose member, nested type, type parameter, or positional parameter now has the type's name") | Read on its own, the bullet withholds every rename after which a member has the type's name. The diff's own test `RH4005InterfaceNameCasingAnalyzerTests.FixRenamesANamespaceInterfaceToTheNameOfOneOfItsMethods` (decision 12) and the enum example state that the fix is offered for an interface whose method gets the interface's name (no compile error, CS0542 does not apply to interfaces) and for an enum member. Only the enum case is written down. The interface case is left to the lead sentence "when the result has a compile error that the code did not have", so the bullet over-states. Add the interface exception next to the enum one (or say the bullet applies where the member declaration is a compile error). A clarification, no new ID. | spec author |

## Checked
1. Change doc: ok. Lane is bug fix; every spec edit keeps its ID and states a withheld fix or a coverage limit of existing behavior; no new, retired, or removed requirement, rule, or phase. No change doc is required and none exists; `sdd/changes/` untouched.
2. Specs match the diff: finding 1 otherwise ok. Format kept (normative sentence, `Why:`, examples, `{|RH####:…|}` markup). Examples avoid asserting RH4103 reports for extern or metadata-rooted methods. SHARED-FIX-003 native-entry-point text matches the implementer's `GetNativeEntryPoint` (missing `EntryPoint`, `nameof(self)` withheld; literal kept; `[DllImport]` and `[LibraryImport]`; methods and local functions). RH4120-003 and SHARED-CASING-007 limits match the tests.
3. IDs: ok. No new IDs; `sdd/specs/requirement-ids.txt` unchanged.
4. Code owners: ok. No owner type renamed, added, or removed; `sdd/specs/README.md` unchanged. `Uses` lines gained SHARED-FIX-003 in casing.md, RH4104, RH4120 (consistent with the architecture tests).
5. User documentation: ok. `documentation/rules/` unchanged, correct because every change is a withheld fix, which `general-notes.md` covers. No page edited, so no page note to check.
6. Open questions: ok. The spec author lists no output questions; lead decisions in `00-lead.md` cover the maintainer-decided points. The follow-up (RH2001 inheriting the excluded-code limit, #1286 check) is under the spec author's follow-up candidates; the lead's list already carries it.
7. Coverage: `scripts/test.sh --project architecture` -> `Passed! - Failed: 0, Passed: 115, Skipped: 0, Total: 115`. Every active requirement the diff changed (SHARED-FIX-003, SHARED-CASING-007, SHARED-CASING-009, RH4104-002, RH4120-003) has covering `[Requirement]` tests in the diff.
8. Tags: ok. All 55 methods the diff adds or newly tags in `src/Reihitsu.Analyzer.Test` carry `[Requirement]` for the IDs in the test implementer's tables (`03`, corrected by `05`); the RH4106 existing test gained `SHARED-CASING-007`. Every requirement under `Examples for the tests` (SHARED-FIX-003, SHARED-CASING-009, SHARED-CASING-007, RH4104-002, RH4120-003) is tagged on at least one added test.
9. Nothing left behind: ok for this check; only `sdd/work/casing-fix-unsafe-renames/` exists, which the lead deletes before publishing.
