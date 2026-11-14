Role: spec checker
Commit: 041886c
Verdict: consistent

Repair round 1, checked with `git diff 2102800 041886c`.

## thoenissen/Reihitsu#1105

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

### Checked
1. Change doc: ok. The repair touches one example; no behavior changes, no change doc needed.
2. Specs match the diff: ok. Finding 1 of `08-spec-checker.md` is resolved. In the Unicode-escape example of RH7201-001, `using System;` is now `using {|RH7201:System|};`, so both `System.IO` and `System` below `Microsoft.Win32` are marked, as the requirement text and the test `UnicodeEscapedSystemDirectiveIsReported` demand. The escape stays literal.
3. IDs: ok. `requirement-ids.txt` is untouched.
4. Code owners: ok. No owner header changed.
5. User documentation: ok. No rule page changed.
6. Open questions: ok. The repair handover has none.
7. Coverage: ok. No requirement is added; the covering test already exists and matches the example. The architecture run was not repeated: the diff is example markup only.
8. Tags: ok. No test changed.
9. Nothing left behind: ok. `sdd/work/` holds only this run's directory.

## thoenissen/Reihitsu#1266
Nothing changed in this repair round; the checks of `08-spec-checker.md` stand.

## thoenissen/Reihitsu#1267
Nothing changed in this repair round; the checks of `08-spec-checker.md` stand.
