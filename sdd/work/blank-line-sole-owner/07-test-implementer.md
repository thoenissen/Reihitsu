Role: test implementer
Commit: c9b738d
Verdict: tests written

Routed-findings round after the implementer: lead decisions 9, 10, 13, 15, 16, the maintainer's D3 answer (A), `05-implementer.md` D1–D3 and Scope, `06-spec-author.md` *Examples for the tests*. The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore. Expected values come from the spec examples and requirement text (FMT-BLANK-009, -017, -018, FMT-SWITCH-001/002/003, SHARED-BLANK-004, RH5025-005), not from the patched code. Not pinned, as decision 16 says: the region-directive gap before a collection expression's `]`.

Every test below passes on c9b738d (formatter 3626/3626, core 544/544, architecture 92/92, analyzer RH5020/5024/5025/5026/5027/5030/5031/5032/5303 classes 221/221). To see each new test fail for the right reason, I ran the branch's whole `Reihitsu.Formatter.Test` project (minus `TokenGapNormalizerTests.cs`, whose call site no longer compiles there) against `main`'s production code at 741157c in a scratch worktree. The worktree has been removed. "On main" below gives that result.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `BlockStatementBlankLineTests.PlacesBlankLineBelowDirectiveAndKeepsAuthorBlankLineBelowDirective` (changed: `#if DEBUG` → `#if !NEVER`, active branch; expected blank lines unchanged) | FMT-BLANK-017 | `AssertRuleResult` | passes; on main: output mismatch (blank line missing) |
| `BlockStatementBlankLineTests.SeparatesBlockStatementsThatFollowAStatementInsideSwitchSection` (changed: expected wraps the section in the block FMT-SWITCH-001/003 add, `break;` directly below its `}` per FMT-SWITCH-002 and SHARED-BLANK-004; FMT-BLANK-017 blank lines inside unchanged) | FMT-BLANK-017 | `AssertRuleResult` | passes; on main: output mismatch |
| `ReihitsuFormatterLineEndingTests.LineBreaksEndingPragmaAndRegionDirectivesAreRewritten` (changed: no blank line between `int x;` and `#pragma warning restore`, as the maintainer answered) | FMT-EOL-002 | `AssertFormatted` | passes; on main: fails on that blank line |
| `DirectiveBeforeEndRegionBlankLineTests.InsertsNoBlankLineInFrontOfDirectiveWhenRegionDirectiveEndsTheGap` (new; the FMT-BLANK-009 spec example, `class C` + `class D`) | FMT-BLANK-009 | `AssertRuleResult` | passes; on main: blank line inserted above `#pragma warning restore` |
| `DirectiveBeforeEndRegionBlankLineTests.InsertsRegionBlankLineButNoneInFrontOfDirectiveInTypeBody` (new; type body, no author blank line: the region's blank line is inserted below the directive, none above it) | FMT-BLANK-009 | `AssertRuleResult` | passes; on main: blank line inserted above `#pragma` |
| `DirectiveBeforeEndRegionBlankLineTests.KeepsDirectiveDirectlyBelowStatementWhenRegionDirectiveEndsTheGapInMethodBody` (new; `a++;⏎#pragma …⏎⏎#endregion⏎}` unchanged) | FMT-BLANK-009 | `AssertRuleResult` (no-change form) | passes; on main: changed already-formatted source |
| `DirectiveBeforeEndRegionBlankLineTests.InsertsNoBlankLineInFrontOfConditionalBlockWhenRegionDirectiveEndsTheGap` (new; `#if NEVER … #endif⏎⏎#endregion⏎}` in a method body unchanged) | FMT-BLANK-009 | `AssertRuleResult` (no-change form) | passes; on main: blank line inserted above `#if` |
| `DirectiveBeforeEndRegionBlankLineTests.KeepsAuthorBlankLineInFrontOfDirectiveWhenRegionDirectiveEndsTheGap` (new; the author's blank line above the first directive is kept) | FMT-BLANK-009 | `AssertRuleResult` (no-change form) | passes; passes on main (holds) |
| `DirectiveBeforeEndRegionBlankLineTests.InsertsBlankLineBelowEndRegionInFrontOfDirectiveBeforeClosingBrace` (new; `#endregion⏎#pragma …⏎}` gains the blank line below `#endregion`) | FMT-BLANK-009 | `AssertRuleResult` | passes; passes on main (holds) |
| `DirectiveBeforeEndRegionBlankLineTests.KeepsBlankLineBelowEndRegionInFrontOfDirectiveBeforeClosingBrace` (new; that blank line is kept) | FMT-BLANK-009 | `AssertRuleResult` (no-change form) | passes; passes on main (holds) |
| `GluedCommentBlankLineTests.SeparatesCommentGluedToOneLineComplexElementInitializerOnly` (new; the FMT-BLANK-018 complex-element spec example, both sides) | FMT-BLANK-018 | `AssertRuleResult` | passes; on main: first and second pass match, the phase-stability check fails (blank-line phase alone inserts a blank line above `/* c1 */`) |
| `GluedCommentBlankLineTests.RemovesAuthorBlankLineAboveCommentGluedToMultiLineComplexElementInitializer` (new; the author's blank line above `/* c1 */ {` on a multi-line complex element is removed) | FMT-BLANK-018 | `AssertRuleResult` | passes; on main: phase-stability check fails, as above |
| `GluedCommentBlankLineTests.SeparatesCommentGluedToClosingBraceOfAssignedCollectionInitializerOnly` (new; the assigned vs. unassigned spec example) | FMT-BLANK-018 | `AssertRuleResult` | passes; on main: phase-stability check fails (blank-line phase alone inserts a blank line above `/* m */`) |
| `GluedCommentBlankLineTests.RemovesAuthorBlankLineAboveCommentGluedToClosingBraceOfUnassignedCollectionInitializerOnly` (new; author's blank line removed above `/* m */ }`, kept above `/* l */ }`) | FMT-BLANK-018 | `AssertRuleResult` | passes; on main: phase-stability check fails, as above |
| `GluedCommentBlankLineTests.SeparatesCommentInFrontOfDocumentationCommentOnBraceLineOnly` (new; the documentation-comment spec example, both sides) | FMT-BLANK-018 | `AssertRuleResult` | passes; on main: phase-stability check fails (blank-line phase alone inserts a blank line above `/* y */`) |
| `BlankLineSpacingPolicyTests.IsBlockStatementAfterStatementReturnsTrueForBlockAfterStatementInBlock` (new) | RH5025-005, FMT-BLANK-017 | direct Core call | passes |
| `BlankLineSpacingPolicyTests.IsBlockStatementAfterStatementReturnsTrueForBlockAfterClosingBrace` (new) | RH5025-005, FMT-BLANK-017 | direct Core call | passes |
| `BlankLineSpacingPolicyTests.IsBlockStatementAfterStatementReturnsTrueForBlockAfterStatementInSwitchSection` (new) | RH5025-005, FMT-BLANK-017 | direct Core call | passes |
| `BlankLineSpacingPolicyTests.IsBlockStatementAfterStatementReturnsFalseForFirstStatementOfBlock` (new) | RH5025-005, FMT-BLANK-017 | direct Core call | passes |
| `BlankLineSpacingPolicyTests.IsBlockStatementAfterStatementReturnsFalseForFirstStatementOfSwitchSection` (new) | RH5025-005, FMT-BLANK-017 | direct Core call | passes |
| `BlankLineSpacingPolicyTests.IsBlockStatementAfterStatementReturnsFalseForIfStatementBody` (new) | FMT-BLANK-017 | direct Core call | passes |
| `BlankLineSpacingPolicyTests.IsBlockStatementAfterStatementReturnsFalseForMethodBody` (new) | FMT-BLANK-017 | direct Core call | passes |
| `BlankLineSpacingPolicyTests.IsBlockStatementAfterStatementReturnsFalseForBlockOfLabeledStatement` (new) | FMT-BLANK-017 | direct Core call | passes |
| `BlankLineSpacingPolicyTests.IsBlockStatementAfterStatementReturnsFalseForTopLevelBlock` (new) | FMT-BLANK-017 | direct Core call | passes |

The five FMT-BLANK-018 tests pin today's output, as the spec states. On main, the first and second passes already give the expected output; only the phase-level check fails, because the blank-line phase run alone disagrees with the line-break phase there. That is the two-owner state this run removes.

FMT-BLANK-011's D5 exemption (`A();⏎/* a */ {`) stays covered by the existing `GluedCommentBlankLineTests.KeepsCommentsGluedToBracesWithoutBlankLine`. The FMT-BLANK-018 examples for a region directive and for a block-statement brace stay covered by the existing `KeepsBlankLinesOfGapWithDirectiveInFrontOfGluedComment` and `SeparatesCommentGluedToBlockStatementBraceAfterStatement`. I added no new test for these.

## Not covered, and why

- The region-directive gap before a collection expression's `]`: open output question (lead decision 16). Not pinned.
- CRLF: every formatter test above runs LF and CRLF through `AssertRuleResult`. The line-ending test is CRLF by construction.
- Idempotency and phase stability: built into `AssertRuleResult`.
- Combined pipeline: `AssertRuleResult` runs the whole pipeline. FMT-BLANK-013 (region framing) and FMT-SWITCH (D2 braces) interact here, and both are part of the asserted output.
- Code-fix convergence and Fix All: no code fix changed in this round.
- Analyzer: RH5025-005 analyzer tests are unchanged. The Core tests pin the shared predicate directly.

## Commands

```shell
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~DirectiveBeforeEndRegionBlankLineTests|FullyQualifiedName~GluedCommentBlankLineTests|FullyQualifiedName~BlockStatementBlankLineTests|FullyQualifiedName~LineBreaksEndingPragmaAndRegionDirectivesAreRewritten"
scripts/test.sh --configuration Debug --project core --filter "FullyQualifiedName~BlankLineSpacingPolicyTests"
scripts/test.sh --configuration Debug --project formatter
scripts/test.sh --configuration Debug --project core
scripts/test.sh --configuration Debug --project architecture
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5025|FullyQualifiedName~RH5020|FullyQualifiedName~RH5030|FullyQualifiedName~RH5024|FullyQualifiedName~RH5026|FullyQualifiedName~RH5027|FullyQualifiedName~RH5031|FullyQualifiedName~RH5032|FullyQualifiedName~RH5303"
```
