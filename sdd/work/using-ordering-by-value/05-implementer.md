Role: implementer
Commit: 5208268
Verdict: #1105: tests pass; #1266: test disputed; #1267: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the binding decisions in `00-lead.md` (Decisions 2–4), which I followed.

## thoenissen/Reihitsu#1105

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/UsingDirectiveOrderingUtilities.cs` | New private `GetNameValue`: the concatenated `ValueText` of every token of the directive's name (drops `@`, decodes Unicode escapes, leaves out whitespace and comments between tokens), with a leading `global::` stripped. `IsSystemNamespaceUsing`, `GetRootNamespace`, and the non-alias branch of `GetSortKey` read it instead of `Name.ToString()`. | SHARED-USING-004, SHARED-DIAG-002, SHARED-USING-007, FMT-PIPE-004 |

The formatter, RH7201, RH7207, and every code fix change only through these three reads (Decision 2); no formatter or code-fix file changed.

Note on the sort key: it now leaves out a leading `global::` (before: `global::System.IO` was the key). SHARED-USING-004 states the sort key of `global :: System.IO` is `System.IO`, and the formatter test `GlobalQualifierWithWhitespaceBelongsToTheSystemGroup` needs it (`System` before `global :: System.IO`). No existing test locked the old key.

## thoenissen/Reihitsu#1266

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/UsingDirectiveOrderingUtilities.cs` | New public `GetDirectivesOutOfOrderWithinTheirGroup(SyntaxList<UsingDirectiveSyntax>)`: every directive whose sort key is smaller than the key of any earlier directive of its group (`AreInSameGroup`), in source order | SHARED-USING-007 |
| `RH7203…Analyzer.cs`, `RH7204…Analyzer.cs`, `RH7206…Analyzer.cs` (`src/Reihitsu.Analyzer/Rules/Organization/`) | The private adjacent-predecessor loops are replaced by the Core method, filtered to the rule's kind (regular / alias / static); RH7203's `AnalyzeGroup` and its `SystemNamespace`/`OtherNamespace` pre-partition are removed (the root comparison in `AreInSameGroup` already separates them); unused usings removed | RH7203-001/002, RH7204-001/002, RH7206-001/002, Decision 3 (FC-015) |

### Disputed tests or scope
- **`RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzerTests.MicrosoftUsingsAreFixedWithoutMovingSortedSystemGroup`** (pre-existing on `main`, not touched by the test implementer) fails: expected 1 diagnostic, actual 3. Its input block is
  ```csharp
  using Microsoft.CodeAnalysis.CSharp;
  using {|#0:Microsoft.CodeAnalysis|};
  using Microsoft.CodeAnalysis.CodeActions;
  using Microsoft.CodeAnalysis.CodeFixes;
  ```
  It marks only `Microsoft.CodeAnalysis`. Under the amended RH7203-001 ("every regular namespace directive whose sort key is smaller than the key of any earlier regular namespace directive of the same group") and SHARED-USING-007, `…CodeActions` and `…CodeFixes` both sort before the earlier `…CSharp` (`O` < `S`, case-insensitive), so they must be reported too — exactly the spec's own example `Microsoft.C; {|RH7203:Microsoft.A|}; {|RH7203:Microsoft.B|}`. The test asserts `main`'s adjacent-only behavior. Its `fixedCode` is unaffected (the fix organizes the whole block). Route: test implementer, to mark `{|#1:Microsoft.CodeAnalysis.CodeActions|}` and `{|#2:Microsoft.CodeAnalysis.CodeFixes|}` and expect three diagnostics. No visible output is in question: the spec already demands it.

## thoenissen/Reihitsu#1267

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/UsingDirectiveOrderingUtilities.cs` | `ComputeCanonicalOrder` gets one more key right after the kind: a directive without a name (an alias whose target is not a name) sorts after every directive with one, so its group comes after every other group of its section. Order between two such aliases is unchanged (alias name, then source order; Decision 4). `GetNamespaceGroupOrderKey` is unchanged (a Core test pins its `""` for `System`). | SHARED-USING-004 |

RH7204 needed no change (Decision 4); its characterization tests pass.

## Whole run

### Policy owners
- `rg -n "CompareSortKeys|AreInSameGroup\(|GetDirectivesOutOfOrderWithinTheirGroup|GetSortKey\(|GetRootNamespace\(|IsSystemNamespaceUsing\(" src --glob '!*Test*/**'`: every definition is in `src/Reihitsu.Core/UsingDirectiveOrderingUtilities.cs`. Consumers: RH7201 (`IsSystemNamespaceUsing`), RH7203/RH7204/RH7206 (`GetDirectivesOutOfOrderWithinTheirGroup`), RH7207 (`AreInSameGroup`), formatter `UsingGrouping` (delegates to Core). No private copy of the within-group comparison remains.
- `rg -n "\.Name\b.*ToString\(\)" src --glob '!*Test*/**'`: no using-name read by spelling remains in production (the one hit is `DocumentationCommentUtilities` XML tag names, unrelated).

### Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 4 files changed, 81 insertions(+), 78 deletions(-). No new type; one new public member (`GetDirectivesOutOfOrderWithinTheirGroup`, Decision 3) and one new private member (`GetNameValue`).

### Deleted special cases
_N/A._ No shape declared unsupported.

### Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH720"`: Failed 1, Passed 231, Total 232 (the disputed test above; all 51 handed-over failing tests pass).
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~UsingDirective"`: Passed 169 of 169 (the 19 handed-over tests pass).
- Full projects: analyzer 9236/9237 (same one), formatter 4955/4955, core 651/651, cli 279/279, tooling 126/126, architecture 118/118.

### Disputed tests or scope
- The RH7203 test above (#1266).
- Scope: one new public Core member, as Decision 3 asked; otherwise within the files the handovers name.

### Architecture findings
_None._ Decision 3's FC-015 finding is resolved: RH7203, RH7204, and RH7206 take the within-group comparison from Core.

### Follow-up candidates
_None_ beyond those already in `00-lead.md`.
