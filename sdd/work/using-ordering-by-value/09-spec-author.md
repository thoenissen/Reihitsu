Role: spec author
Commit: e336446
Verdict: spec written (repair of 08-spec-checker Finding 1)

Repair round 1. Only Finding 1 of `08-spec-checker.md` is repaired. No requirement ID, change doc, or rule page is touched.

## thoenissen/Reihitsu#1105

### Changed requirements
- RH7201-001 — clarified (example markup only). In the Unicode-escape example of `sdd/specs/analyzer/RH7201.md`, `using System;` below `Microsoft.Win32` is now marked `using {|RH7201:System|};`, as the requirement text already demands and as the test `UnicodeEscapedSystemDirectiveIsReported` and `main` already report. The escape `System.IO` stays the literal escape sequence (checked with grep: `System` occurs twice in the file, in the prose line and in the example, never decoded).

### Examples for the tests
RH7201-001 (Unicode-escape example; the existing test already matches it).

### Output questions
_None._

### Internal choices
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1266
Nothing to do in this repair round.

## thoenissen/Reihitsu#1267
Nothing to do in this repair round.
