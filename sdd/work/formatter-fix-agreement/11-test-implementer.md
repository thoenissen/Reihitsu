Role: test implementer
Commit: (this commit)
Verdict: tests written (repair round 1, decision 7: one test deleted)

My prompt carried only the decision to carry out (`00-lead.md`, Decisions row 7) and the checks to make first; no suspected cause or hoped-for verdict. I re-checked the deletion against the changed FMT-PIPE-024 rather than taking it as given.

## thoenissen/Reihitsu#1529

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ReihitsuFormatterDetachedRootTests.ConditionalAccessAssignmentTargetKeepsTheWrappedLinkOfItsAssignedMember` — **deleted** | was tagged FMT-PIPE-023, FMT-PIPE-024 | `AssertFormatsTarget` (`FormatNodeInDocumentAsync`) | — |

Why it goes: its target, an assignment that is the `WhenNotNull` of a conditional access passed as the root of `FormatNodeInDocumentAsync`, is the one root the changed FMT-PIPE-024 states no caller passes and leaves undecided (`AGENTS.md`, *Supported inputs*). After the RH5111-006 change the RH5111 fix formats that assignment through `FormatNodeInDocumentWithContextAsync`; `08-auditor.md` *Policy ownership* lists every other `FormatNodeInDocumentAsync` caller, none of which can pass such an assignment.

What else it covered, and where that coverage stays:
- FMT-PIPE-023 (detached root, the marked space/tab inside the target, write-back of the target only): 9 other tests in the same class with the same `AssertFormatsTarget`/`⇥` fixture, among them `ChainPrefixTargetKeepsTheLayoutOfTheLongerChain` (a wrapped chain inside a node-level target), plus `ReihitsuFormatterNodeBlankLineTests` (6).
- FMT-PIPE-024 chain-continuation fact: `ChainPrefixTargetKeepsTheLayoutOfTheLongerChain` and 13 further FMT-PIPE-024 tests in the class; 3 in `ReihitsuFormatterNodeColumnTests`.
- The layout of `a?.B⏎ .C = 5` itself (wrapped link of the assigned member kept and aligned): 27 RH5111-006 tests in `RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests` through the fix the callers actually use (e.g. `VerifyFixKeepsAWrappedMemberThatStandsInTheAnchorColumn`, `VerifyFixAlignsAWrappedMemberBehindAMemberBindingUnderTheAnchor`), plus the RH5201/RH5112 silence tests.

### Other tests reaching the removed fact through an entry point
None. Searched every formatter and Core test file that calls `FormatNode`/`FormatNodeInDocumentAsync` (`rg -l "FormatNodeInDocumentAsync|FormatNode\("` over `src/Reihitsu.Formatter.Test`, `src/Reihitsu.Core.Test`) for `AssignmentExpressionSyntax`/`ConditionalAccess` targets: the only other assignment target is `P = Call(1, 2)` (line 426, not behind `?`). `FluentChainTests.LeftSideOfNullConditionalAssignmentIsNoOutermostChainNode` reads an assignment attached to its parsed conditional access, not a root, so the fact was never read there. The only analyzer test calling a node-level entry point is `RH5604…FormatterTests`, with no conditional access. `rg ConditionalAccessAssignmentTargetKeeps` outside `sdd/work` finds nothing else.

### Not covered, and why
No new test: the case no caller reaches needs none (`AGENTS.md`, *Supported inputs*), and FMT-PIPE-024 now decides nothing for it.

### Commands
```shell
scripts/test.sh --configuration Debug --project formatter      # Passed 4849/4849
scripts/test.sh --configuration Debug --project architecture   # Passed 117/117
```
`scripts/format.sh` on the changed file: no change.

## thoenissen/Reihitsu#1543

Nothing to do in this repair round.
