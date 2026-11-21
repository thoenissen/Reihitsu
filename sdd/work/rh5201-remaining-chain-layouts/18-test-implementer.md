Role: test implementer
Commit: (this commit; parent aea177fe)
Verdict: tests written

Repair round 2, auditor finding #3 (`17-auditor.md`). My prompt carried no intended fix, suspected cause, or hoped-for verdict.

I checked the finding against the spec rather than taking it as given. RH5201-002 now carries a MUST NOT clause: the `*/` paragraph does not report the chain's first link. Its inline example is `var a = c` ⏎ `/* x` ⏎ `*/ .Next.Self();`, where `.Self` is reported and `.Next` is not. `VerifyFirstLinkBehindOwnLineCommentTailInRootColumnIsNotReported` is exactly that input and asserts exactly that outcome. So the test is derived from RH5201-002's normative sentence and its example, not only incidentally related to it, and it gets the tag. The change is test-only: one attribute line, with no assertion or input changed.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFirstLinkBehindOwnLineCommentTailInRootColumnIsNotReported` (in `src/Reihitsu.Analyzer.Test/Formatting/RH5201MethodChainsShouldBeAlignedFirstCallTests.cs`). Added `[Requirement("RH5201-002")]`; the tags are now RH5201-006, RH5201-005, RH5201-002, SHARED-CHAIN-004, SHARED-CHAIN-005 | RH5201-006, RH5201-005, RH5201-002, SHARED-CHAIN-004, SHARED-CHAIN-005; FC-003, FC-007, FC-017 | `Verify` + fix (`VerifyFix`) | pass |

This round has no new red run, because the assertions are unchanged. The red evidence for this test is in `13-test-implementer.md`: dropping `linkIndex > 0 &&` gives `expected "1" actual "2"` under LF and CRLF.

Existing tests whose asserted output changes: none.

## Not covered, and why

Nothing new. The tag adds no behavior to cover. Other failure classes are as in `13-test-implementer.md`.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5201"
scripts/test.sh --project architecture
```

Results on this head:
- analyzer, RH5201 filter: 191 passed;
- architecture: 198 passed. This includes the requirement-tag conventions.

`scripts/format.sh` on the test file changed nothing (0 of 1).

## Follow-up candidates

_None._ The auditor's pre-existing column-of-`*/` candidate is already recorded in `00-lead.md`.
