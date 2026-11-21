Role: test implementer
Commit: (this commit; parent 3a2b11ce)
Verdict: tests written

Repair round 1, auditor finding #1 (`12-auditor.md`). My prompt carried no intended fix, suspected cause, or hoped-for verdict. I re-derived both expected outputs from the specs rather than from the auditor's suggestion; they agree with it.

Derivation:
- (a) First link behind an own-line `*/` in the root column (`var a = c` ⏎ `/* x` ⏎ `*/ .Next.Self();`): RH5201's scope leaves "a first link that starts a line below the root" to RH5112 (`RH5201.md` line 18), and the SHARED-CHAIN-005 `*/` exception "does not decide the chain's first link, which SHARED-CHAIN-003 and SHARED-CHAIN-004 govern"; SHARED-CHAIN-004 keeps it in place. So `.Next` is not reported. The comment keeps the first link on its own line, so the first call `.Self` is reported (RH5201-006, SHARED-CHAIN-005) and aligns to the start of the root (SHARED-CHAIN-007). Fix result = formatter layout (RH5201-005), which I confirmed with `scripts/format.sh` on the unchanged formatter: `*/ .Next` ⏎ `.Self();` in the root column.
- (b) Block comment between root and first link without a line break (`var a = c /* x */.Next.Self()` ⏎ `.Self();` aligned under the first `.Self`): no line break stands in front of any link before the first call and nothing keeps the first link on a line of its own, so RH5201-006 does not apply to the first `.Self`; the second `.Self` starts a line in the anchor column. Not reported. The input is the formatter's own output for that shape (the formatter removes the space before `.Next` and aligns `.Self`), so it is stable valid code.

## Tests

Both in `src/Reihitsu.Analyzer.Test/Formatting/RH5201MethodChainsShouldBeAlignedFirstCallTests.cs`, LF and CRLF (`DataRow`).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFirstLinkBehindOwnLineCommentTailInRootColumnIsNotReported` — x01 shape: only the first call `.Self` reported; fix in one application and by Fix All gives the formatter's layout | RH5201-006, RH5201-005, SHARED-CHAIN-004, SHARED-CHAIN-005; FC-003, FC-007, FC-017 | `Verify` + fix (`VerifyFix`, 1 diagnostic, 1 iteration) | pass |
| `VerifyFirstCallBehindFirstLinkBlockedByBlockCommentOnRootLineIsNotReported` — comment blocks the first link on the root's line, no line break: not reported | RH5201-006; FC-003, FC-017 | `Verify` | pass |

Seen red for the right reason against the two conditions the auditor named (production edits made locally, run, and reverted with `git checkout`; nothing committed):
- dropping `linkIndex > 0 &&` in `RH5201MethodChainsShouldBeAlignedAnalyzer.IsMisplaced`: the first test fails under LF and CRLF, `expected "1" actual "2"` (`.Next` reported); every other RH5201 test passes.
- reducing `FirstLink.StartsLine && IsFirstLinkBlocked` to `IsFirstLinkBlocked` in `FluentChain.IsRootLineShortOfFirstInvokedLink`: the second test fails under LF and CRLF, `expected "0" actual "1"`; every other RH5201 test passes.

Existing tests whose asserted output changes: none (this round adds tests only and changes no production code).

## Not covered, and why

- Formatter-side `VerifyFormatter` variants of these two shapes: the formatter is unchanged by this round, and both are the formatter's own stable output or are formatted to the fixed text asserted above (checked with `scripts/format.sh`); the auditor's x01 fixture already showed the formatter output is RH5201-clean.
- A directive variant of (b): a directive cannot stand between the root and the first link without a line break, so (b) has no directive side.
- Other failure classes: as in `04-test-implementer.md`; this round adds no rewrite.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5201"
scripts/test.sh --project architecture
```

On this head: analyzer RH5201 filter 191 passed (187 + 4 new runs); architecture 198 passed.

## Hints

- RH5201-002's `*/` paragraph says the link behind `*/` is reported "also when it is a prefix link" and does not itself say "behind the first"; the exclusion of the chain's first link follows only from RH5201's scope line and the SHARED-CHAIN-005 exception. The spec author may want to name the first-link exclusion in RH5201-002 so the requirement reads like the code (`linkIndex > 0`) and like auditor finding #2's summary fix.

## Follow-up candidates

_None._
