Role: test implementer
Commit: f49ecb3
Verdict: tests written

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. It asked me to check the dispute in `11-implementer.md` against the test framework and to correct the two disputed tests. The spec is not in question.

## The claim, checked

The claim holds.

- Microsoft.CodeAnalysis.Analyzer.Testing 1.1.4 documents `SolutionState.MarkupHandling` (`~/.nuget/packages/microsoft.codeanalysis.analyzer.testing/1.1.4/lib/net461/Microsoft.CodeAnalysis.Analyzer.Testing.xml`) as follows: "For fixed (output) sources, the default value is IgnoreFixable".
- `MarkupMode.IgnoreFixable` is documented as: "fixable diagnostics suggested by markup syntax are ignored".
- RH5303 and RH5304 are in their providers' `FixableDiagnosticIds`. So the `{|RH5303:…|}` and `{|RH5304:…|}` markup that I put in the fixed sources was dropped, and the framework expected no diagnostic in the fixed state.
- My tests were therefore wrong. They meant "the withheld diagnostic stays", which RH5303-006 and RH5304-005 require, but they asserted "no diagnostic remains".

The repository already handles this case. `RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzerTests` (around line 1288) sets `FixedState.MarkupHandling` and `BatchFixedState.MarkupHandling` to `MarkupMode.Allow` for a fixed source that keeps a diagnostic whose fix is withheld.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Reihitsu.Analyzer.Test.Formatting.RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyFixAllFixesOnlyTheCreationWithoutComplexElementBehindTheLastLineOfAMultiLineComment` (corrected) | RH5303-006, RH5303-005; FC-008 | `Verify(source, fixedSource, config, diagnostics)` | passes |
| `Reihitsu.Analyzer.Test.Formatting.RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzerTests.VerifyFixAllFixesOnlyTheCreationWithoutComplexElementBehindTheLastLineOfAMultiLineComment` (corrected) | RH5304-005, RH5304-004; FC-008 | same | passes |

The correction is the same in both tests:
- Their configuration now also sets `config.FixedState.MarkupHandling = MarkupMode.Allow;` and `config.BatchFixedState.MarkupHandling = MarkupMode.Allow;`, with a comment saying why. Each file gains `using Microsoft.CodeAnalysis.Testing;`.
- Inputs, fixed sources, expected diagnostics, iteration counts, and tags are unchanged.

What the tests now assert:
- In the input, both creations are reported.
- After the incremental fix and after Fix All, the creation that holds the complex element behind the multi-line comment's last line keeps exactly its marked diagnostic, unfixed.
- The other creation is laid out as the formatter lays it out: 32/28 for RH5303, 40/36 for RH5304.

Each test was watched failing for the right reason. In a temporary worktree of e61306c, which has these tests but not the guard of 37a5ea4, both corrected tests fail with `Context: Iterative code fix application`, because the fix is applied to the withheld creation. I removed the worktree afterwards.

## Not covered, and why

Nothing new. The coverage decisions in `10-test-implementer.md` stand.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5303|FullyQualifiedName~RH5304"
```

On f49ecb3 plus this correction: `Passed! - Failed: 0, Passed: 104, Total: 104`.
