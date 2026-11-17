Role: test implementer
Commit: 43267e0
Verdict: tests written

Repair round 1. My prompt carried no intended fix, suspected cause, or hoped-for verdict; I treated the auditor's suggested change as a suggestion and derived the expected output from the specs.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.VerifyFixIsOfferedForAMethodBesideAMethodWithACommentBehindItsArrow` (now LF and CRLF rows) | SHARED-FIX-010; FC-017, FC-043, FC-009 | `GetCodeFixActionsAsync` (B: no action), `TryApplyCodeFixAtAsync` (A: applied, exact text), `Verify` (result: B still reported) | pass |
| `ExpressionBodiedTransformTests.KeepsCommentTrailingArrowInIndexer` | — (untagged) | — | deleted (lead decision 8) |

### Strengthened RH3202 test (audit 09, finding 1)

Input `int A() => 1; int B() => /* c */ 2;` on one line. The test now asserts, under LF and CRLF:

1. no action is registered for `B` (SHARED-FIX-010, first sentence);
2. the action registered for `A`, once invoked, produces exactly
   ```csharp
   class C
   {
       int A()
       {
           return 1;
       }
       int B() => /* c */ 2;
   }
   ```
   — `A` converted and formatted, `B` on a line of its own with its text as written (SHARED-FIX-010, last sentence: no action that "changes nothing or leaves its edit unformatted");
3. the analyzer still reports `B`'s RH3202 diagnostic on the result (SHARED-FIX-010: "The diagnostic is still reported").

Expected output derivation: RH3202-002 with SHARED-FIX-007 *Shared line* — the line begins with `A`'s first token, so `A` gets lines of its own and the member behind it starts on the next line, with no blank line inserted (same shape as RH3202-002's `/* c */ int Get() … int _y;` example). Not taken from running the code (FC-035); the run confirmed it.

Watched it fail for the right reason: a temporary mutation of `RH3202ExpressionStyleMethodsShouldNotBeUsedCodeFixProvider.ApplyCodeFixAsync` that returns the unchanged document for method `A` (the regression the auditor describes: action offered, nothing changed) fails both rows on `Assert.AreEqual(RemoveMarkup(expected), actual)`. The mutation was reverted with `git checkout` and never committed; `git status` was clean apart from the test files before the commit.

### Deleted indexer test (lead decision 8)

`KeepsCommentTrailingArrowInIndexer` called the structural phase directly with `int this[int i] => // why⏎_data[i];`, which the precheck refuses (FMT-UNSUP-012) and no caller passes to a phase. It carried no requirement tag. Its input stays covered through public entry points:

- `ExpressionBodiedIndexerTests.InlineCommentAfterArrowIsUnsupported` (FMT-UNSUP-012) — a line comment behind an indexer's arrow is refused;
- `UnsupportedExpressionBodyArrowCommentTests.CommentBehindTheArrowOfEveryExpressionBodyIsUnsupported`, row *Indexer*, and the spec-example rows with `int this[int i] => /* c3 */ i;` (FMT-UNSUP-012).

Its neighbour `KeepsCommentLeadingSemicolonInIndexer` stays: a comment in front of the `;` is outside the shape's gap and supported.

## Scenarios (bug fix)
_Not a bug fix._

## Not covered, and why
- Fix All on the two-method input: Fix All skips `B` because no action is registered for it; `VerifyNoCodeFixIsOffered` tests of the refused side already check every diagnostic of a document, and the batch scenario of the class (`GetFixAllScenario`) is unchanged.
- No other test changed. The rest of 04's matrix stands; the auditor rated it adequate.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~ExpressionBodiedTransformTests|FullyQualifiedName~ExpressionBodiedIndexerTests|FullyQualifiedName~UnsupportedExpressionBodyArrowCommentTests"
scripts/test.sh --project architecture
```

Results at 43267e0 (Release): analyzer RH3202 class 73 passed; formatter filter 179 passed; architecture 198 passed.

## Follow-up candidates
_None._
