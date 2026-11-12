Role: test implementer
Commit: (this commit; parent 5edb254)
Verdict: reproduced

Repair round 1, decision 24 in `00-lead.md`, auditor finding #2 (`10-auditor.md`), RH8402-006 as widened in `11-spec-author.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1342

All tests in `src/Reihitsu.Analyzer.Test/Documentation/RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzerTests.cs`. New constant `SpecificationCopyrightTextEndingInLineBreak` (= the spec's `copyrightText` + `"\n"`) and an overload `VerifySpecificationFileFix(source, fixedSource, copyrightText)`; the existing two-argument helper delegates to it with the unchanged text, so the existing tests are unaffected (39 pass).

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FixWithHeaderEndingInLineBreakReplacesOldHeaderDirectlyInFrontOfCode` | RH8402-006 (spec example 1) | `Verify` via `VerifySpecificationFileFix` | fails |
| `FixWithHeaderEndingInLineBreakReplacesOldHeaderDirectlyInFrontOfCodeWithCarriageReturnLineFeed` | RH8402-006 | same | fails |
| `FixWithHeaderEndingInLineBreakReplacesOldHeaderAndKeepsBlankLineBehindIt` | RH8402-006 (spec example 2) | same | fails |
| `FixWithHeaderEndingInLineBreakReplacesOldHeaderAndKeepsBlankLineBehindItWithCarriageReturnLineFeed` | RH8402-006 | same | fails |
| `FixWithHeaderEndingInLineBreakReplacesOldHeaderAndKeepsTwoBlankLinesBehindIt` | RH8402-006 ("N blank lines") | same | fails |
| `FixWithHeaderEndingInLineBreakReplacesOldDelimitedHeaderFollowedByCodeOnItsLine` | RH8402-006 ("the header's own line break separates it and the fix adds none") | same | passes (other side of the condition) |

`Verify` runs the one-pass fix, Fix All, and re-analysis, so convergence (FC-007/FC-008) is asserted by the same calls.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `// Copyright (c) Old Corp.⏎namespace N;⏎`, header ending in `\n` | LF | code fix | reproduced | observed: one blank line in front of `namespace N;`; expected: none |
| same | CRLF | code fix | reproduced | same |
| `// Copyright (c) Old Corp.⏎⏎namespace N;⏎` | LF | code fix | reproduced | observed: two blank lines; expected: one |
| same | CRLF | code fix | reproduced | same |
| same with two blank lines | LF | code fix | reproduced | observed: three; expected: two |
| `/* Copyright (c) Old Corp. */ namespace N;⏎` | LF | code fix | passes | header, then `namespace N;` on the next line |

### Not covered, and why

- A blank line then a directive or a documentation comment behind the run, with a header ending in `\n`: the auditor's class lists them, but the blank-line count follows the same span end as the "N blank lines" rows (one call site, `GetLeadingHeaderSpan` then `EnsureHeaderIsSeparatedFromCode` in `RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderCodeFixProvider`); the existing non-trailing-break tests cover the directive and doc-comment stops of the run.
- Insert and no-comment paths with a header ending in `\n`: RH8402-005, outside this finding; the auditor reports them as holding.
- Formatter counterpart: RH8402 has no formatter counterpart (the formatter writes no header).
- CRLF for the two-blank-lines and the code-on-its-line rows: same line-ending handling as the CRLF rows above.

## thoenissen/Reihitsu#1269, #1343, #1459

Nothing to do in this round; no finding concerns them.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzerTests.FixWithHeaderEndingInLineBreak"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzerTests"
```
