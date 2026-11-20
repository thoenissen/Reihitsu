Role: spec checker
Commit: 95b436c
Verdict: consistent

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

## Checked
1. Change doc: ok. `sdd/changes/2026-11/prefix-chain-anchor.md` is required (SHARED-CHAIN-007 changes intended output), is Accepted, has no task list, has `Visible output changes`, and its `Decisions` records maintainer answers 1A, 2A, 3A (same as `00-lead.md`).
2. Specs match the diff: ok. The `GetAnchorLink` change in `FluentChain.cs` (first invoked link in front of the first wrapped link, else the last link in front of it, else the first link, root fallback unchanged) is what SHARED-CHAIN-007 states. FMT-INDENT-029, RH5201-001/-002, RH5112-003 and RH5111-006 examples agree with it. No spec still states the old "chain's first link" anchor (grep of `sdd/specs` and `documentation`). Format (normative first, Why, examples, markup) holds.
3. IDs: ok. No ID added, retired or renamed; `requirement-ids.txt` is unchanged.
4. Code owners: ok. No type renamed, added or removed; `RH5112` Uses row gained SHARED-CHAIN-007. `scripts/test.sh --project architecture`: Passed 198, Failed 0.
5. User documentation: ok. `documentation/rules/RH5201.md` text matches the spec, has no formatter mention, withheld-fix note or corner case, keeps its footer; the Correction example is unchanged. Other chain rule pages do not state the anchor.
6. Open questions: ok. The skeptic's and the spec author's output questions are answered under `Maintainer answers` in `00-lead.md`; no follow-up candidates exist.
7. Coverage: ok. Architecture run above (198 passed); every requirement the spec author named under `Examples for the tests` has a test (SHARED-CHAIN-007, FMT-INDENT-029, RH5201-001/-002, RH5112-003, RH5111-006).
8. Tags: ok. Every added or newly tagged test carries the `[Requirement]` tags of the test implementer's table; updated RH5111/RH5201/RH5112 tests keep their existing tags (RH5111-006, RH5201-001, SHARED-CHAIN-005); the `ConditionalAccessWrapBetweenOperatorTokensTests` update carries only its FMT-BREAK-019 tag, as the table states.
9. Nothing left behind: ok. `sdd/work/` holds only `prefix-chain-anchor/`; the lead deletes it before publishing (the selfhosting `WorkingDocumentsDirectoryContainsNoFiles` failure the implementer noted clears then).
