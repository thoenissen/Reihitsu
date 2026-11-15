Role: spec checker
Commit: 4d9c42f
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
None for either issue.

## thoenissen/Reihitsu#966

### Checked
1. Change doc: ok. SHARED-USING-009 is clarified (token-wise sentence plus the comment-text sentence, both inside the existing requirement); no requirement is added, changed in intent, or retired, so no change doc is needed. Note: the code now reports and reorders blocks that a `#` comment line used to block; that follows SHARED-DIAG-003 and the unchanged "preprocessor directive" wording, so it reads as a bug fix, not an intended-behavior change.
2. Specs match the diff: ok. `UsingDirectiveOrderingSafety.CanSafelyReorder` now reads `ContainsDirectives` (leading, trailing, and between-token trivia), which is what the amended SHARED-USING-009 states; both new examples carry markup and a Why exists.
3. IDs: ok. No ID added for this issue.
4. Code owners: ok. `UsingDirectiveOrderingSafety` still exists; `sdd/specs/README.md` row unchanged and correct. Uses gains SHARED-DIAG-003. No spec, page, or code comment still names the removed `_preprocessorDirectiveRegex`, `HasUnsafeTrivia`, or `ContainsPreprocessorDirective` (grep over src, sdd/specs, documentation).
5. User documentation: ok. RH7201-RH7207 pages are untouched and carry the general-notes footer; nothing on them describes the guard.
6. Open questions: ok. No output question for #966; decisions 2, 3, 5 are in `00-lead.md`.
7. Coverage: `scripts/test.sh --project architecture` -> "Passed! - Failed: 0, Passed: 118, Skipped: 0, Total: 118".
8. Tags: ok. Core test class (15 tests) and formatter class (11 of 12; the A9 test carries FMT-PIPE-033 as the table lists) carry `[Requirement("SHARED-USING-009")]`; one tagged test per RH7201-RH7207 analyzer plus the RH7203 matrix. The spec author's named examples (B1 and the `# Header` example) are tagged.
9. Leftovers: ok. Only `sdd/work/comment-text-not-syntax/` exists.

## thoenissen/Reihitsu#1289

### Checked
1. Change doc: ok. `sdd/changes/2026-11/rh2109-csharp-lines-read-as-markup.md` exists for the changed RH2109-001 / new RH2109-004 / SHARED-DIAG-003 sentence, has `Visible output changes` (None, matching the unchanged production code), no task list, and is `Accepted`. Its Decisions section records the maintainer's answer A.
2. Specs match the diff: ok. No production change; RH2109-004 states today's behavior (reported direction and the converse `@*` / `<!--` miss with closer pairing), has Why and three examples; RH2109-001 points to it; SHARED-DIAG-003 states the exception.
3. IDs: ok. RH2109-004 is the next free number and sits in sorted position in `requirement-ids.txt`; nothing else changed.
4. Code owners: ok. RH2109 spec header unchanged and listed in `sdd/specs/README.md`; `shared/diagnostics.md` Uses lists RH2109-004 (the earlier architecture failure is repaired).
5. User documentation: ok. `documentation/rules/RH2109.md` is untouched and the behavior is unchanged for users.
6. Open questions: ok. Q1 is answered under `Maintainer answers`. The follow-up candidates (HTML comment opened behind other text; RH7203 column-0 indentation in a block namespace) are listed under `Follow-up candidates` and written into no spec.
7. Coverage: same architecture run as above, 118 passed.
8. Tags: ok. The retagged and 14 new RH2109 tests carry `RH2109-004` (the retagged first one also `RH2109-001`), matching the test implementer's table; the spec author's three examples (lines 1/3/7, `@*` hides the rest, `-->` does not close `@*`) are each covered.
9. Leftovers: ok, as above.
