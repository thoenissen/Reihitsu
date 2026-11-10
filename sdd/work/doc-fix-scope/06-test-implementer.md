Role: test implementer
Commit: 21f6bba
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Work follows `00-lead.md` decisions 19–23 and the specs at `ffbb0bb` (RH8305-003, RH8307-003, RH8307-006, FMT-PIPE-032, FMT-UNSUP-002). No production file was touched (a temporary local edit to watch tests fail was reverted before commit; see #1459).

**The formatter test project does not compile until the implementer adds `ReihitsuFormatter.FormatDocumentationElementInDocumentAsync`.** The tests call it as `Task<Document> FormatDocumentationElementInDocumentAsync(Document document, XmlElementSyntax element, CancellationToken cancellationToken)` — the name from FMT-PIPE-032; the parameter type `XmlElementSyntax` and the return type `Task<Document>` are my reading of "a document and one XML element … returns the document". Observed now: `CS0117: 'ReihitsuFormatter' does not contain a definition for 'FormatDocumentationElementInDocumentAsync'` (two call sites, nothing else). Because the architecture project builds the whole solution, `scripts/test.sh --project architecture` fails to build until then as well.

## thoenissen/Reihitsu#1269

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH8201…`VerifyDiagnosticForFieldLikeOverrideEvent` (input corrected) | RH8201-001, RH8201-004 (existing tags) | `Verify` | passes |
| RH8201…`VerifyMultipleFieldLikeOverrideEventsAreFixedInOneFixAllIteration` (input corrected) | RH8201-001 (existing tag) | `Verify` | passes |

Decision 19: in both inputs the line after the markup read `|}        public override event …` (member at column 8 once the markup is gone, every other line of the class at 4). Changed to `|}    public override event …` (three lines), as the class's other tests write it. Expected outputs and tags are unchanged. Both failed on `42eeedd` (04-implementer.md) and pass now.

### Scenarios (bug fix)

_None new._

## thoenissen/Reihitsu#1342

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH8307…`VerifySpecificationExampleNoCodeFixForElementOfDelimitedDocumentationComment` | RH8307-006 | `VerifyNoCodeFixIsOffered` | passes |
| RH8307…`VerifyNoCodeFixForNestedElementOfDelimitedDocumentationComment` | RH8307-006 | `VerifyNoCodeFixIsOffered` | passes |
| RH8307…`VerifyFixForNestedElementOfSingleLineDocumentationComment` (other side of the `/** */` condition) | RH8307-004, RH8307-006 | `Verify` | passes |
| `ReihitsuFormatterDocumentationElementTests.SpecificationExampleLaysOutTheRemarksAndKeepsTheRestOfTheDocument` | FMT-PIPE-032 | `AssertLaysOut` (LF, CRLF, second pass) | does not compile |
| `…SpecificationExampleAddedLinesTakeTheIndentationOfTheStartTagLine` | FMT-PIPE-032 | same | does not compile |
| `…ElementOnALaterLineTakesTheIndentationOfItsOwnStartTagLine` | FMT-PIPE-032 | same | does not compile |
| `…NestedElementsAreRebuiltInTheSameRun` (FMT-DOC-011 example, through the entry point) | FMT-PIPE-032 | same | does not compile |
| `…ByteOrderMarkInFrontOfTheSeparatorIsNotRepeated` | FMT-PIPE-032 | same | does not compile |
| `…CanonicalElementIsLeftAsItIs` | FMT-PIPE-032 | same | does not compile |
| `…EverythingOutsideTheElementStaysAsWritten` | FMT-PIPE-032 | same | does not compile |
| `…ElementOfADocumentWithAnUnsupportedShapeIsLaidOut` | FMT-UNSUP-002 | same | does not compile |

The RH8307-006 tests pass on `21f6bba`: the implementer's `CanLayOutElement` already withholds the fix there; the analyzer reports both spans as marked. They pin the requirement, they do not reproduce a defect.

`AssertLaysOut` (in the new file `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterDocumentationElementTests.cs`) builds an `AdhocWorkspace` document (documentation mode `Parse`) under LF and under CRLF, selects the element by name with `DescendantNodes(descendIntoTrivia: true)`, asserts the exact document text (so FMT-PIPE-007's line ending is checked by the CRLF run), then calls the entry point again on the same element of the result and asserts the text does not change (FMT-DOC-010 / idempotency).

Expected values: both FMT-PIPE-032 examples verbatim; the later-line case and the byte-order-mark case from the normative sentence "each line … MUST start with the spaces and tabs that stand in front of the `///` on the line of the element's start tag … A byte order mark … is not repeated" (the later-line input puts the start-tag line at column 10, the comment's first line and the element's own continuation line at 4: every line of the new element is at 10); the nested case is the FMT-DOC-011 example with `<remarks>` as the element; "everything outside" keeps a `///   <summary>` separator, a `///<param>` element the phase would rebuild, another documentation comment and the blank line between them.

### Scenarios (bug fix)

_None new._

## thoenissen/Reihitsu#1343

_No work in this round._

## thoenissen/Reihitsu#1459

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH8305…`VerifySpecificationExampleSummaryOfPositionalRecordIsReportedOnce` | RH8305-003 | `Verify` (diagnostics) | passes |
| RH8305…`VerifySummaryOfTypeWithPrimaryConstructorIsReportedOnce` (6 rows: class, struct, record struct, record class with body, record in a namespace, record nested in a class) | RH8305-003 | `Verify` (diagnostics) | passes |
| RH8305…`VerifySummaryOfExplicitConstructorIsReportedOnce` (other side of the guard) | RH8305-003 | `Verify` (diagnostics) | passes |
| RH8307…`VerifySpecificationExampleElementOfPositionalRecordIsReportedOnce` | RH8307-003 | `Verify` (diagnostics) | passes |
| RH8307…`VerifyElementOfTypeWithPrimaryConstructorIsReportedOnce` (same 6 rows) | RH8307-003 | `Verify` (diagnostics) | passes |
| RH8307…`VerifyElementOfExplicitConstructorIsReportedOnce` | RH8307-003 | `Verify` (diagnostics) | passes |

Watched failing: with `DocumentationAnalysisUtilities.IsPrimaryConstructorRun` forced to `false` locally, all 14 primary-constructor tests failed and the two explicit-constructor tests passed; the file was restored byte for byte before the commit (`git status` shows no production change). The existing `public record R(int A);` rows of the compilation-unit tests keep their RH8305-004/RH8307-004 tags; the new tests carry -003.

### Scenarios (bug fix)

_None new._

## Not covered, and why

- FMT-PIPE-032 inputs the requirement does not decide (behind-code comment, FMT-DOC-013 comment, `/** */` comment, element without its own end tag) and syntax-error / generated-code documents: no caller passes them (AGENTS.md *Supported inputs*); no test.
- Text behind the end tag on its line: no RH8305/RH8307 input I could build puts code there without the comment being behind-code or FMT-DOC-013; left out.
- FC-008 (Fix All) for RH8307-006: no fix exists for the class. For the primary-constructor rows, the existing compilation-unit Fix All tests and each class's `GetFixAllScenario` already run.
- FC-019–FC-021 (comment/directive in the gap): the entry point edits inside one documentation element, where neither can stand.
- FC-038–FC-043: no shape is declared unsupported in this round; FMT-UNSUP-002's "refuses no input" is covered by `ElementOfADocumentWithAnUnsupportedShapeIsLaidOut`.
- The entry point's TestContext-less overloads, cancellation: no requirement.

## Commands

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Documentation.RH8201|FullyQualifiedName~Documentation.RH8305|FullyQualifiedName~Documentation.RH8307"
# on 21f6bba + these tests: Passed 184, Failed 0
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~ReihitsuFormatterDocumentationElementTests"
# build fails with CS0117 until the entry point exists
```
