Role: test implementer
Commit: 28835f5
Verdict: tests written (repair round 1)

Inputs: `00-lead.md` (decisions 6–8), `06-test-implementer.md`, `07-implementer.md`, `10-auditor.md`, `12-spec-author.md` (commit 60a109a: RH7501-004 and SHARED-ORDER-007 examples), the accepted change docs, and the specs they name. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the worklist; the auditor's suggested changes were taken as suggestions. Expected outputs come from the spec examples, from `main`'s output where decision 7 says so (#5) or where the spec demands the fix `main` offered (#1), and from the formatter's layout where decision 7 leaves it to the formatter (#6). `main` outputs were observed with `scripts/apply-fix.sh` on a worktree of `a59a003`; the formatter's layout with `scripts/format.sh` on scratch copies.

All tests are in `src/Reihitsu.Analyzer.Test`. "Fails" means it fails on 60a109a's production code (unchanged since 1d19e02) for the audit's own difference: no fix offered where one is required (source unchanged in the verifier diff, or "No code fix action was registered"), a fix offered where none may be (`Assert.IsEmpty(actions)`), or the #5 output. Every failure message was checked; none fails on a compile error or on unrelated layout.

## thoenissen/Reihitsu#1136

`RH7004UsingDeclarationsShouldNotBeUsedAnalyzerTests` — finding #1, members (a)–(e). Each expected output is `main`'s output, which RH7004-004 demands, since SHARED-FIX-003 withholds none of them.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FixIsOfferedInsideAConditionalPairThatEndsInFrontOfTheClosingBrace` (a) | RH7004-004, SHARED-FIX-003 | `Verify` + DEBUG transform | fails (no fix) |
| `FixIsOfferedInsideTheActiveBranchOfAConditionalPairWhoseElseBranchEndsInFrontOfTheClosingBrace` (b) | RH7004-004, SHARED-FIX-003 | `Verify` + DEBUG transform | fails (no fix) |
| `FixIsOfferedForAPragmaWarningDirectiveWithoutRestoreAmongTheFollowingStatements` (c) | RH7004-004, SHARED-FIX-003 | `Verify` | fails (no fix) |
| `…WithoutRestoreAmongTheFollowingStatementsUnderBothLineEndings` (c) | RH7004-004, SHARED-FIX-003 | `ApplyCodeFixAsync`, LF + CRLF | fails (no action registered) |
| `FixIsOfferedForANullableDirectiveWithoutPartnerAmongTheFollowingStatements` (d) | RH7004-004, SHARED-FIX-003 | `Verify` | fails (no fix) |
| `FixIsOfferedForAPragmaPairOfTheFollowingStatementsBehindADisableOfAnotherWarning` (e) | RH7004-004, SHARED-FIX-003 | `Verify` | fails (no fix) |

The counter-sides (genuine splits withheld, a pair wholly among the statements moved) are the existing tests of `06-test-implementer.md`, which still pass.

For (c) and (d) the directive stands behind the first following statement, not directly behind the declaration. With the directive directly behind the declaration, `main`'s fix inserts a blank line between a following local declaration and the next statement (any declaration does this, with or without a directive: SHARED-FIX-007 *Moved code*) and drifts the line ending under CRLF (SHARED-FIX-008) — both pre-existing and already the lead's follow-up candidate. A test on that shape would trip over those defects instead of the guard, so the guard is tested on the shape that `main` fixes correctly under both line endings (checked).

## thoenissen/Reihitsu#1134

`RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests` — finding #2 (a), (c), (d) and finding #4 / decision 6 / SHARED-ORDER-007 examples (60a109a).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NoCodeFixForAPartialClassWithSequentialLayoutOnAnotherPart` (#2 a) | SHARED-FIX-003, RH7109-006 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| `NoCodeFixForAPartialRecordWithSequentialLayoutOnAnotherPart` (#2 c) | SHARED-FIX-003, RH7109-006 | same | fails (fix offered) |
| `FixReordersTheFieldsOfAPartialStructWithAutoLayoutOnAnotherPart` (#2 d) | SHARED-FIX-003 | `Verify` | fails (no fix) |
| `FixReordersTheFieldsOfAPartialStructWithExplicitLayoutOnAnotherPart` (#2 d) | SHARED-FIX-003 | `Verify` | fails (no fix) |
| `FixReordersTheFieldsOfAStructWithAutoLayoutWrittenAsANamedArgument` (#2 d) | SHARED-FIX-003 | `Verify` | fails (no fix) |
| `NoCodeFixWhenTheMoveCrossesAnInitializedFieldOfExcludedCode` (spec example, #4) | SHARED-ORDER-007, RH7109-006 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| `FixCrossesAFieldOfExcludedCodeWithoutInitializer` (other side of the condition) | SHARED-ORDER-007 | `Verify` | passes |
| `FixMovesExcludedCodeInFrontOfTheMovedFieldWithIt` (spec example, leading-trivia side) | SHARED-ORDER-007 | `Verify` | passes |

`FixMovesExcludedCodeInFrontOfTheMovedFieldWithIt` differs from the spec example in one line: `Log` has a block body instead of `=> 0`. The RH7109 fix formats the whole type, which rewrites `=> 0` into a block — the pre-existing whole-member formatting (FC-030, thoenissen/Reihitsu#969), not this run's guard. With the spec's own member the test would fail on that unrelated layout. See *Tests or specs found wrong*.

## thoenissen/Reihitsu#1135

`RH7309RegionsShouldFollowCategoryOrderAnalyzerTests` — finding #2 (b), (d) and decision 8. `RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests` — decision 6 (the RH7103 counterpart).

| Test | Class | Requirement / failure class | Helper | Expected now |
|---|---|---|---|---|
| `NoCodeFixForTheInstanceFieldsOfAPartialClassWithSequentialLayoutOnAnotherPart` (#2 b) | RH7309 | RH7309-006 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| `CodeFixReordersTheInstanceFieldsOfAPartialStructWithAutoLayoutOnAnotherPart` (#2 d) | RH7309 | RH7309-006 | `Verify` | fails (no fix) |
| `CodeFixReordersAStructWhosePropertyReadsAMemberNamedFieldBeforeCSharp14` (decision 8, LangVersion 13) | RH7309 | RH7309-006, SHARED-FIX-003 | `Verify` + language-version transform | fails (no fix) |
| `NoCodeFixWhenTheMoveCrossesAnInitializedStaticFieldOfExcludedCode` | RH7103 | SHARED-ORDER-007, RH7103-007 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| `FixMovesExcludedCodeInFrontOfTheMovedStaticFieldWithIt` | RH7103 | SHARED-ORDER-007 | `Verify` | passes |

Decision 8: the counter-side (the `field` keyword under C# 14 gives a backing field and withholds the fix) is the existing `NoCodeFixForAPropertyThatUsesTheFieldKeywordOfAStruct`. RH7109 cannot reach decision 8: `rg -n "HasBackingField|IsLayoutField" src` shows the only consumers are `ChangesLayoutOrder` (RH7109) and `RegionBlockLayout` (RH7309); `ChangesLayoutOrder` asks only when the moved member is itself an instance field of a layout type, and RH7109 then always crosses the non-readonly instance field it is moved in front of, so a property is never the deciding member.

## thoenissen/Reihitsu#1262

`RH7309RegionsShouldFollowCategoryOrderAnalyzerTests` — finding #3, both confirmed shapes, mode `Parse`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NoCodeFixWhenADocumentationCommentAboveTheLastEndRegionLineWouldDocumentAMember` | RH7309-006 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| `NoCodeFixWhenADocumentationCommentBehindTheLastCodeOfTheLastBlockWouldDocumentAMember` | RH7309-006 | same | fails (fix offered) |
| `CodeFixKeepsADocumentationCommentThatDocumentsNothingInALastBlockThatKeepsItsPlace` (other side: the comment documents nothing before and after) | RH7309-006 | `Verify` | passes |

## thoenissen/Reihitsu#1332

`RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzerTests` — the RH7501-004 examples of 60a109a.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyCodeFixMovesBreakWithItsCommentsBehindBlock` (changed: case 2 now `/* exit */ break;`, as the spec example) | RH7501-004 | `Verify` | passes |
| `NoCodeFixForTheSharedLineSpecificationExample` (changed: case 4 `{ break;` ⏎ `}` added, as the spec example) | RH7501-004 | `VerifyNoCodeFixIsOffered` | passes |
| `NoCodeFixWhenTheBreakSharesItsLineWithTheOpeningBrace` (case 4 alone) | RH7501-004 | same | passes |

Both changed tests are spec-example tests this role wrote; the change only adds the spec's new input, and both pass on the unchanged code. The implementer has not worked against them in a way the change touches.

## thoenissen/Reihitsu#1336

`RH7004UsingDeclarationsShouldNotBeUsedAnalyzerTests` — findings #6 and #5.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FixKeepsACommentBehindTheUsingKeyword` (replaces `OfferedFixKeepsACommentBehindTheUsingKeyword`) | SHARED-FIX-003 | `Verify` | passes |
| `FixKeepsACommentInFrontOfTheSemicolon` (replaces `OfferedFixKeepsACommentInFrontOfTheSemicolon`) | SHARED-FIX-003 | `Verify` | passes |
| `FixKeepsACommentThatDoesNotEndItsLineBehindTheNewBlock` (#5) | RH7004-004 | `Verify` | fails (`… MemoryStream()) /* c */` ⏎ `{` ⏎ `}}`) |
| `FixKeepsACommentThatDoesNotEndItsLineBehindTheNewBlockUnderBothLineEndings` (#5) | RH7004-004 | `ApplyCodeFixAsync`, LF + CRLF | fails (same) |

#6: both tests now assert the full output, so dropping, moving, or withholding fails them. `using /* c */ (var s = new MemoryStream())` keeps the comment behind `using` (SHARED-FIX-003 *Comments and directives in place*: the declaration is rewritten in place). For the semicolon input the comment stays on the line in front of `)`, and the blank line above it is the formatter's layout: formatting `using (var s = new MemoryStream()` ⏎ `// c` ⏎ `)` (comment indented one level) gives exactly the asserted text, which is a fixed point (checked with `scripts/format.sh`). Decision 7 leaves this layout to the formatter. The helper `AssertEveryOfferedFixKeeps` is removed with its last callers.

#5: `main`'s output `}` ⏎ … `} /* c */ }`, as decision 7 restores, under LF and CRLF.

## thoenissen/Reihitsu#1340

No new tests; the audit passed.

## Whole run

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| #1 (a)–(e), branch | LF, CRLF | code fix | reproduced | no fix vs `main`'s fix |
| #1 (c), (d) directive directly behind the declaration, `main` | LF, CRLF | code fix | pre-existing defect | blank line inserted; CRLF drift (follow-up already listed) |
| #2 (a), (b), (c) | LF | code fix | reproduced | fix offered vs withheld |
| #2 (d) partial `Auto`/`Explicit` on another part, named `layoutKind:` | LF | code fix | reproduced | no fix vs reorder |
| #3 both shapes | LF | code fix | reproduced | fix offered, comment then documents `Dispose` |
| #4 RH7109, RH7103 | LF | code fix | reproduced | fix offered vs withheld |
| #5 | LF, CRLF | code fix | reproduced | `}}` vs `} /* c */ }` |
| decision 8 (C# 13) | LF | code fix | reproduced | no fix vs reorder |
| RH7501 case 4, `/* exit */ break;` | LF | code fix | holds | no fix; comment moves with the `break` |

### Not covered, and why

- #2 (d) partial struct `Explicit` on another part for RH7309: same `FieldOrderSetsLayout` call as `Auto` (`RegionBlockLayout.cs:73`); the `Auto` test covers the arm.
- #4 with `DEBUG` defined: the field is active then, which the existing initializer-order tests cover.
- Line endings for #2, #3, #4, decision 8: the guards decide on type attributes, members, and documentation trivia, not on line structure; one LF case each. #1 (c) and #5 have CRLF variants because their output is layout.
- FC walk: unchanged from `06-test-implementer.md`; the repair adds both sides of each widened or narrowed condition (FC-003), partial types (FC-024), and the other-build member (FC-016, SHARED-FIX-003 *Build configurations*).

### Tests or specs found wrong

- SHARED-ORDER-007's second new example (60a109a, `sdd/specs/shared/ordering-move-safety.md`) shows the fix output with `private static int Log(string name) => 0;` unchanged. On this branch and on `main`, the RH7109 and RH7103 fixes format the whole type and rewrite it into a block body (observed with `scripts/apply-fix.sh` on both examples). That is the pre-existing whole-member formatting (FC-030, thoenissen/Reihitsu#969, against SHARED-FIX-006), not a defect of this run. The tests use a block-bodied `Log`. The spec example is right by SHARED-FIX-006; the code is what disagrees, so no spec change is proposed. The lead may want the spec example to avoid an expression-bodied member until #969 is fixed, so that a test can mirror it exactly.

### Output questions

_None._ Every new expected output is a spec example, `main`'s output that RH7004-004 demands (#1), `main`'s output that decision 7 restores (#5), or the formatter's layout that decision 7 leaves to it (#6).

### Architecture findings

_None_ beyond decision 6.

### Build and tests

- `scripts/format.sh` over the five changed test files: nothing to format.
- `scripts/test.sh --project analyzer`: 6620 tests, 6600 passed, **20 failed — exactly the tests marked "fails" above**.
- `scripts/test.sh --project architecture`: 115 passed.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7004Using|FullyQualifiedName~RH7103Static|FullyQualifiedName~RH7109|FullyQualifiedName~RH7309|FullyQualifiedName~RH7501"
scripts/test.sh --project architecture
```
