Role: auditor
Commit: bd63778f2b4ebfe9d72cb942289dea1f37ac9f9b
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore.

## Scope

- Full audit of `d83f2ae0b88a68fc626936144a7d7b1e01b861a4...bd63778f2b4ebfe9d72cb942289dea1f37ac9f9b`.
- State checks:
  - The checkout is at `bd63778`, and the tree is clean.
  - `git ls-remote origin refs/heads/main` returns `d83f2ae0b88a68fc626936144a7d7b1e01b861a4`, and that commit is an ancestor of HEAD.
  - `scripts/lane-check.sh tooling --head worktree` reports `confirmed — claimed tooling, diff needs tooling`.
  - The diff is what `01`–`06` describe:
    - 20 specs, the ledger, the index, and RH8107;
    - two decisions entries;
    - two test-base files and 20 test classes;
    - no file under `src/Reihitsu.Analyzer`, `Reihitsu.Core`, `Reihitsu.Analyzer.CodeFixes`, or `Reihitsu.Formatter`.
- Inputs: issue thoenissen/Reihitsu#982, open-question issue thoenissen/Reihitsu#1037, `00`–`06`, both `squad/decisions/2026-10-18-*.md`, the specs, and the counterpart code:
  - `SplitElementDocumentationAnalyzerBase`;
  - `DocumentationAnalysisUtilities`, `DirectDocumentationSyntaxChecker`, `XmlDocumentationExpander`, `DocumentationModeAnalysisContextExtensions`;
  - the RH8013 and RH8014 analyzers, and the constructors of all 19 analyzers;
  - `XmlDocumentationElementOrderingUtilities.GetTagName`;
  - `sdd/specs/formatter/documentation-comments.md` FMT-DOC-001 to FMT-DOC-003.
- Execution:
  - A scratch harness outside the repository: Roslyn 5.9.0, the Debug `Reihitsu.Analyzer.dll` and `Reihitsu.Core.dll` of this tree (built 13:30 UTC today; the last change to `src/Reihitsu.Analyzer` and `src/Reihitsu.Core` is `5918189`, from 2026-10-17), and an `XmlFileResolver` over the fixture folder.
  - It printed the RH80xx diagnostics, the compiler warnings, and the generated XML documentation.
  - I trusted the cleaner's build and the roles' focused test runs. I re-ran no repository test.

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | high | `sdd/specs/shared/element-documentation.md`, SHARED-DOC-002: its coverage sentence and its `Why:` | FC-016 (a sibling of the partial-type shape that SHARED-DOC-001 and SHARED-DOC-005 now exclude), FC-002 | same requirement | See *Finding 1* below. SHARED-DOC-002 puts a part of a public partial type in the private group, and #1037 does not file that case. | See *Finding 1* below. |
| 2 | medium | `sdd/specs/shared/element-documentation.md`, SHARED-DOC-007: coverage sentence | FC-002 | same requirement | See *Finding 2* below. An included `<Summary>` documents the element, although SHARED-DOC-007 covers that input and says it must not. | See *Finding 2* below. |
| 3 | low | `src/Reihitsu.Analyzer.Test/Base/AnalyzerTestsBase{TAnalyzer}.cs` (`ApplyDocumentationModeParseToTestProject`) and the local `ApplyParseDocumentationMode` functions in `RH8001NonPrivateClassesMustBeDocumentedAnalyzerTests` and `RH8013NonPrivateEnumMembersMustBeDocumentedAnalyzerTests` | code quality: logic duplicated from existing code | PR-introduced | The new base helper repeats the body of the three local functions, and all three stay in classes this diff edits: `VerifyNoDiagnosticForClassWithMalformedAmpersandInDocumentation` and `VerifyNoDiagnosticForClassWithUnclosedTagInDocumentation` (RH8001), and `VerifyNoDiagnosticForEnumMemberWithMalformedAmpersandInDocumentation` (RH8013). The Parse-mode setup now has four copies. | Call the base helper from those three tests and delete the local functions. No `[Requirement]` tag changes, so those tests stay outside the open questions. |
| 4 | low | `src/Reihitsu.Analyzer.Test/Base/InMemoryXmlReferenceResolver.cs` | repository convention (`.editorconfig`) | PR-introduced | The new file has no UTF-8 BOM. `.editorconfig` sets `charset = utf-8-bom` for `[*.cs]`. In the same folder, 8 of 11 files have a BOM. Nothing enforces the rule, and 207 of 1672 tracked `src/**/*.cs` files already lack a BOM, so I rated this low. | Save the file with a BOM. |

### Finding 1 — SHARED-DOC-002 puts a part of a public partial type in the private group

Counterexample, observed in both `Diagnose` and `Parse` mode:

```csharp
/// <summary>Outer</summary>
public class Outer
{
    /// <summary>Visible</summary>
    public partial class Visible
    {
    }

    partial class Visible
    {
    }
}
```

- Real output: RH8002 ("private classes") reports the second `Visible`, and RH8001 reports nothing. The generated `T:Outer.Visible` has the summary.
- Every type kind of this batch behaves the same way:
  - `internal partial struct` with a modifier-less second part: RH8004;
  - `public partial interface`: RH8006;
  - `public partial record`: RH8008;
  - `internal partial record struct`: RH8010.
- Why the spec is wrong here:
  - SHARED-DOC-002's coverage is "every declaration except a static constructor declared in an interface". So the requirement covers this part and states that it belongs to the private group.
  - Its `Why:` says "Without a modifier, C# makes an element declared in a class, struct, or record private". That is false for a part of a partial type: C# gives the type the accessibility that any of its parts declares.
  - #1037 Q3 has only file-level parts, and it lists SHARED-DOC-001/005/006/007 but not SHARED-DOC-002. Under its reading (B), this part would be reported by the private rule, and the question does not show that.
  - So #982's "output that looks wrong is left out of the spec … and filed" is not met for this input.
- Partial constructors cannot reach this case: CS8799 requires identical accessibility modifiers on both parts.
- Suggested change:
  - Exclude "a partial type declared in several parts" in SHARED-DOC-002's coverage sentence, as SHARED-DOC-001 and SHARED-DOC-005 already do.
  - Add the routing case to #1037, in Q3 or as a new question touching SHARED-DOC-002 and RH8001–RH8010.
  - No test asserts this case, so no tag changes.

### Finding 2 — SHARED-DOC-007 covers an included `<Summary>`, which the code accepts

Counterexample:

```csharp
/// <include file='upper.xml' path='doc/members/member[@name="A"]/*'/>
public class UpperIncluded
{
}
```

The file `upper.xml` selects `<Summary>From the file</Summary>`.

- Real output: nothing is reported. `XmlDocumentationExpander.HasTag` matches the expanded element names with `OrdinalIgnoreCase`.
- Why the spec is wrong here:
  - SHARED-DOC-007's coverage is "what SHARED-DOC-005 covers, and an included file that is well-formed XML". SHARED-DOC-005's lower-case condition is a property of the documentation comments, not of the included file.
  - So the input is covered. The normative sentence "whose selected content has neither element at the top level, MUST NOT make the declaration documented" is false for it.
- #1037 Q5 already lists SHARED-DOC-007, so the decision itself is filed. Only the coverage sentence is wrong.
- Suggested change:
  - Make the coverage read "an included file that is well-formed XML and writes the tag names `summary` and `inheritdoc` in lower case".
  - Optionally, note on #1037 Q5 that included content is matched case-insensitively too.

## Guard scope

The diff changes no production predicate. The rows below compare each decision the specs state with the code that implements it, and with the counterpart that decides the same thing elsewhere.

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which documentation comment belongs to a declaration (SHARED-DOC-004) | `DirectDocumentationSyntaxChecker.GetDocumentationComment`: `declaration.GetLeadingTrivia()`, which is the leading trivia of the first token (`[`, a modifier, a keyword, the identifier, or `~`); its first `DocumentationCommentTriviaSyntax` | Compiler attachment (CS1587). FMT-DOC-001/002 `DocumentationCommentFormattingPhase` moves a comment only within the leading trivia of the same token. | They agree within the coverage sentence: a single comment, or comments separated only by whitespace. Observed: D1–D6 shapes, `#nullable` after the comment, a comment after an attribute list, between modifiers, and inside `#if NEVER`. The separated shapes disagree; they are excluded and filed as #1037 Q1. The formatter never moves a comment across a token boundary, so the analyzer's decision is unchanged. | ok |
| Top-level `<summary>`/`<inheritdoc>` (SHARED-DOC-005) | `GetDirectTags`: XML elements and empty elements directly in `documentationComment.Content`, by `LocalName`, `OrdinalIgnoreCase` | Fallback `XmlDocumentationExpander.HasTag`: `expanded.Elements()`, by `LocalName`, `OrdinalIgnoreCase` | Both paths stop at the same top-level boundary. Observed: nested in `<remarks>` is reported; text before `<summary>` is not reported. Case-insensitive matching is excluded for comments (Q5) but not for included files. | finding 2 |
| Later comment and included file (SHARED-DOC-006, SHARED-DOC-007) | `GetDocumentationCommentXml(expandIncludes: true)` of the symbol, read with `XElement.Parse`, `null` on `XmlException` | The compiler's generated documentation | They agree for whitespace-joined comments (a `/** */` comment then `///` agrees too), for resolved includes, for an include nested in `<remarks>` (reported), for a path that selects the whole `<member>` (reported), and for an include in the later comment (not reported). Malformed XML is excluded and filed as Q2. | ok |
| Group (SHARED-DOC-002, SHARED-DOC-003) | `IsPurePrivateDeclaration`: the declaration's own five modifiers; `HasImplicitPrivateAccessibility`: `declaration.Parent` is a `TypeDeclarationSyntax` other than `InterfaceDeclarationSyntax` | The C# declared accessibility of the symbol, which joins the parts of a partial type | They agree for: `private protected`; `new class` in a class; `private class` in an interface; an enum in an interface (RH8011/RH8013); `file`; implicit in a namespace or at file level. They disagree for a modifier-less part of a partial type whose other part has a modifier (finding 1). The interface static constructor is excluded and filed as Q4. An extension-block parent is a hint. | finding 1 |
| Documentation mode (SHARED-DOC-008) | `WrapWithDocumentationModeCheck`: `Node.SyntaxTree.Options` is `CSharpParseOptions` with `DocumentationMode == None` | `RH0002DocumentationModeMustNotBeNoneAnalyzer` uses the same condition | agree | ok |
| Location (RH80nn-002) | `GetDiagnosticLocation`: `Identifier.GetLocation()`, which includes the `@` and excludes the `~`, type parameters, the parameter list, the underlying type, and the value | — | agree; expression-bodied constructors and destructors observed too | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Selecting the declaration's documentation comment | `rg 'OfType<DocumentationCommentTriviaSyntax>\|GetStructure\(\) is DocumentationCommentTriviaSyntax'` finds: `DirectDocumentationSyntaxChecker.GetDocumentationComment`; `DocumentationCommentFormattingPhase` (it detects parsed trivia, which is a different decision); and the `RH8401…CodeFixProvider` (an ancestor lookup, also a different decision) | one owner |
| Documented check | `DocumentationAnalysisUtilities.HasRequiredDocumentation`, two overloads (`MemberDeclarationSyntax`, `EnumMemberDeclarationSyntax`) with identical bodies, in one owner type; RH8013 has its own tests for the enum-member path | one owner, parity tested |
| Private/non-private group | `DocumentationAnalysisUtilities.MatchesAccessibilityGroup` / `IsPurePrivateDeclaration` / `HasImplicitPrivateAccessibility`; `rg PrivateKeyword` finds no other copy in the documentation analyzers. `NamingAccessibilityClassifier` belongs to a different spec. | one owner |
| Documentation mode `None` | `DocumentationModeAnalysisContextExtensions` (owner); `RH0002…Analyzer` reports the mode itself; `RH8402…Analyzer` has its own check but does not use SHARED-DOC | one owner for SHARED-DOC users |
| Parse-mode test setup | `AnalyzerTestsBase.ApplyDocumentationModeParseToTestProject` (new), three local `ApplyParseDocumentationMode` functions, `RH0002…Tests.ApplyDocumentationMode` | finding 3 |

## Assertion adequacy

Every test uses `AnalyzerTestsBase<TAnalyzer>.Verify` with markup. The verifier fails on any unexpected diagnostic of the analyzer under test and checks each exact span.

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| R01 `…DocumentedAfterAttributeList` / `…BeforeAttributeList`, and the R13 pair | Only the first token's leading trivia counts | Before is reported, or after is not | `Verify` | adequate, both sides |
| R01 `…FollowedBy{OrdinaryComment,BlockComment,ConditionalDirective,RegionDirective,PragmaDirective,DisabledText}`, R13 `…ConditionalDirective`/`…OrdinaryComment` | Comments, directives, and disabled text after the last documentation comment do not separate it | A diagnostic on `TestClass` or `Red` | `Verify` | adequate; directive and comment are separate tests (FC-005) |
| R01 `…SummaryNestedInRemarks`, `…InheritdocNestedInRemarks`, `…PlainTextDocumentation`, `…EmptySummaryElement`, `…BlockDocumentationComment`, and the R13 counterparts | Top level versus nested; an empty element counts | A nested element accepted, or `<summary/>` rejected | `Verify` | adequate |
| R01 `…SummaryInLaterDocumentationComment` (+ `InParseMode`) / `…WhenNoDocumentationCommentHasSummary`, and the R13 pair | A whitespace-joined later comment counts; two `<remarks>` comments do not | The later summary ignored, or remarks accepted | `Verify` (+ Parse) | adequate; only the blank-line shape, as the coverage sentence requires |
| R01 `…SummaryFromIncludedFile` (+ `InParseMode`), `…InheritdocFromIncludedFile` / `…IncludedFileWithoutSummary`, `…IncludedSummaryNestedInRemarks`, `…UnresolvedIncludedFile`, and the R13 pair | A resolved include counts at the top level; an unresolved include, a nested element, or no summary does not | The positive tests fail when the resolver does not supply the file; the negative tests fail when the include is accepted blindly | `Verify` + `CreateIncludedDocumentationTransform` | adequate as a pair. The resolver returns `null` for any path it was not given, which is the "missing file" path. |
| R01, R13 `VerifyDiagnosticForUndocumented…InParseMode`; `VerifyNoDiagnosticsWhenDocumentationModeIsNone` in R01–R19 and RH8107 | `Parse` mode is checked; `None` mode is not | `Parse` silent, or `None` reporting. Every `None` source has an undocumented element. The test implementer swapped the transform to `None` and saw the Parse tests fail. | `Verify` + transform | adequate. The Parse side is tested on 2 of 19 rules; the shared registration path is the static proof. |
| Group tests (R01–R18, for example `…FileLocalClass…`, `…PublicClassNestedInPrivateClass`, `…ClassWithoutModifierNestedInInterface`, `…ClassesWithoutModifierInRecordAndRecordStruct`, `…PrivateProtected…`, `…StaticConstructorsOfClassStructRecordAndRecordStruct`, `…PrivateAndStaticConstructors`) | Own modifiers plus the direct container decide the group; the container's accessibility does not | The other group's rule reports, or this rule misses the declaration | `Verify` | adequate. Each test checks one analyzer; the sibling rule's test checks the other side. |
| `RH8001-002` `…VerbatimIdentifierCoversTheAtSign`, and the generic, positional, underlying-type, value, and `~` location tests | The span is exactly the identifier token | A wider or narrower span | `Verify` markup | adequate |
| Single-part partial tests tagged SHARED-DOC-005 | A one-part partial type is a single declaration | — | `Verify` | adequate; inside the coverage sentence |

## Failure classes dismissed

- FC-001: the diff changes no production guard. The spec coverage sentences were checked as guards: finding 1 (too narrow, so it covers a looks-wrong case) and finding 2.
- FC-003: both sides are covered for every qualifier listed in *Assertion adequacy*.
- FC-004, FC-006, FC-007, FC-008, FC-009, FC-018, FC-030: the run has no code-fix provider and no rewrite.
- FC-010, FC-011, FC-012, FC-028, FC-029: the run has no formatter change, and none of these rules uses `FormatNode`.
- FC-013: FMT-DOC-001 to FMT-DOC-003 keep a moved comment in the leading trivia of the same token, so the analyzer's decision does not change.
- FC-014: these rules ask for prose; no formatter phase can write it.
- FC-019, FC-020, FC-021: comment, block comment, `#if`/`#endif`, `#region`, `#pragma`, and `#if NEVER` disabled text are tested after a documentation comment. Between two documentation comments is excluded and filed as Q1. Between modifiers was observed as reported, matching SHARED-DOC-004; see *Hints*.
- FC-022: a literal cannot be leading trivia of a declaration.
- FC-023: a verbatim identifier is tested. A Unicode identifier takes the same `Identifier` token path.
- FC-024: partial types and constructors are excluded and filed as Q3, apart from the group, which is finding 1. `file` types are tested. The interface static constructor is excluded and filed as Q4. Explicit interface implementations cannot be of these kinds.
- FC-025: expression-bodied constructors and destructors were observed as reported on the identifier, the same path as block bodies.
- FC-026: repeated `Service` constructors in one class are pinned by markup positions.
- FC-027: nesting in struct, record, record struct, interface, and private containers is tested; an enum in an interface was observed.
- FC-031: the summaries of the new helpers and tests describe what the code does.
- FC-032: no compiled production change. The harness DLL is newer than the last analyzer source change.
- FC-033: these rules read trivia kinds and XML structure, and a blank line is `EndOfLineTrivia` under LF and CRLF alike, so line endings cannot change the result.
- FC-034, FC-035: the tests derive from spec examples run against the unchanged analyzers, not from a patched copy.
- FC-036: no rename. FC-037: no proof that a change is comment-only is claimed.

## Hints

- `HasImplicitPrivateAccessibility` treats an `ExtensionBlockDeclarationSyntax` parent as implicitly private, while SHARED-DOC-002 says non-private ("otherwise"). Observed: RH8002 on `class N` inside `extension(int i)`. That code does not compile (CS9282) for any kind in this batch. It matters for thoenissen/Reihitsu#983–#985, as `03-spec-author.md` already notes.
- A documentation comment between modifiers (`public /// <summary>…</summary>` then `class C`) is reported (CS1587). That matches SHARED-DOC-004's first sentence, but no test has this FC-021 shape.
- `/// <x:summary>…</x:summary>` counts as documented, because `GetTagName` uses `LocalName`. The compiler reports CS1570 (undeclared prefix), so this is malformed XML, which the coverage sentence already excludes. It could be added to #1037 Q2 as another shape.
- Pre-existing, outside this diff:
  - `DocumentationAnalysisUtilities` and `XmlDocumentationExpander` each have a private `GetDeclaredSymbol` with the same body;
  - the two `HasRequiredDocumentation` overloads repeat each other.
- The same included-documentation XML literal is repeated in eight RH8001 and RH8013 tests. It could be one shared constant.
- `00-lead.md` still says "Roles skipped: … auditor" in its *Lane* section. That line was written before the lane widened, and the *Lane widened* section lists the auditor.
