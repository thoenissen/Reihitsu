Role: auditor
Commit: 7260b2f573530f2531c88580a5822e4cab9961af
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Full audit of head `7260b2f573530f2531c88580a5822e4cab9961af` against base `53990ed5df40c0cb8ce6673ce31cefa2089ee59d`.
- State checks:
  - The checkout is the head SHA, with a clean working tree.
  - `git ls-remote origin refs/heads/main` and `origin/main` both give `53990ed…`, and it is an ancestor of HEAD.
- The diff matches the handovers:
  - 15 new specs (`sdd/specs/analyzer/RH8020.md` to `RH8034.md`), the extension of `sdd/specs/shared/element-documentation.md`, the index rows, and the ledger.
  - One sentence each in `documentation/rules/RH8032.md` and `RH8033.md`, and the decision record.
  - 15 test files. Their diff has 4951 added lines and 0 removed: 117 new tests, and 52 existing tests that only gained `[Requirement]` tags.
  - The handovers 00–12.
  - `git diff 53990ed HEAD -- src` touches only `src/Reihitsu.Analyzer.Test/Documentation/`. No production file changed.
- `scripts/lane-check.sh tooling` reports: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.
- I did not read thoenissen/Reihitsu#983 itself, because the prompt said not to touch GitHub. The issue's requirements are taken from `00-lead.md`:
  - Every rule has its spec: yes, 15 of 15.
  - Every requirement has a covering test: yes. `RequirementCoverageTests` passed per 10, 11, and 12.
  - The shared spec is extended only for behavior that more than one spec uses: yes. SHARED-DOC-009 to SHARED-DOC-013 are each used by two or more specs.
  - The P8 cases are left out and listed as open questions: yes. `09-spec-author.md` lists them for the lead to file; none is filed yet.
  - No production change and no retired requirement: confirmed.
  - `sdd/work/coordinator/` is untouched: confirmed.
- Method: static tracing of every analyzer and helper the specs name, plus a scratch harness outside the repository. The harness is a console program in the session scratchpad. It compiles fixtures with the `Microsoft.CodeAnalysis.CSharp.dll` and `Reihitsu.Analyzer.dll` in `src/Reihitsu.Analyzer.Test/bin/Release/net10.0`, with `LanguageVersion.Latest`, an `XmlFileResolver`, and `Diagnose` or `Parse` mode. It prints RH80xx diagnostics, CS15xx/CS17xx warnings, and `GetDocumentationCommentXml(expandIncludes: true)`.
- I re-ran no repository test. No finding depends on one; I trusted the focused results in 10, 11, and 12.

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | medium | `sdd/specs/shared/element-documentation.md` SHARED-DOC-012 (normative sentence, coverage, `Why:`); `documentation/rules/RH8032.md` and `RH8033.md`, "How to fix it" | FC-023, FC-016 | PR-introduced | A `name` attribute that writes the name with a C# Unicode escape, or contains a format character (Unicode Cf), is within SHARED-DOC-012's coverage. The spec requires the parameter to be reported ("compared character by character"; a `name` that differs "MUST NOT count"), and RH8032/RH8033 do report it. The compiler, however, binds such a `name` to the parameter: no CS1572, CS1573, or CS1711. This is the shape the run itself treats as looking wrong in shared Q5 (padded `name`: the compiler binds it, the rule reports it), where it left the case out of coverage and filed it. Here the case is written into the spec as settled behavior. The requirement's own `Why:` ("the compiler … pair[s] a `<param>` … with the parameter whose name it carries, and that name is the identifier's value") argues for the opposite verdict. The PR's new rule-page sentence ("spell the name exactly as it appears in the declaration, without the `@`…") leads to a report when the declaration uses an escape. Evidence is below. | The spec author decides; any of these: (a) leave a `name` attribute containing a Unicode escape or a format character out of SHARED-DOC-012's coverage and add the shapes to shared Q5, whose subject becomes "a `name` the compiler binds after normalizing it"; (b) say "the parameter's name" is the identifier's value, with `@` removed, escapes decoded, and format characters removed. The code already reads the parameter side that way, and the spec names only `@`. The rule-page sentence follows whichever reading stays in coverage. |
| 2 | low | `sdd/specs/analyzer/RH8030.md` RH8030-001, coverage sentence | FC-002 | PR-introduced | "covers a declaration whose leading trivia has one documentation comment". Read literally, this leaves out a declaration whose leading trivia has none, such as a doc comment placed after the attribute list (SHARED-DOC-004), or any undocumented declaration. `RH8030…VerifySummaryPlacementFollowsTheDocumentationCommentsOfTheDeclaration` (class `B`) asserts the verdict for exactly that shape, tagged RH8030-001. RH8031-001 says "at most one" for the same condition. The behavior is right: there is no comment, so nothing is reported. Only the wording excludes it. | Write "at most one documentation comment", as RH8031-001 does. |
| 3 | low | `src/Reihitsu.Analyzer.Test/Documentation/RH8030ElementDocumentationMustHaveSummaryTextAnalyzerTests.cs`, summary of `VerifyDiagnosticForSummaryOfUnicodeWhiteSpaceOnly` | FC-031 | PR-introduced | The summary says "only Unicode white space, written directly or as character references". The test contains only character references (`&#9;`, `&#x2003;`, `&#x85;`, `&#xA0;`, `&#32;`, `&#160;`). `grep -P "[^\x00-\x7F]"` finds no literal non-ASCII character in the RH8030 or RH8031 test files. Directly written ASCII white space is tested elsewhere. Directly written non-ASCII white space is not tested anywhere. The harness confirms that the code reports a literal U+00A0 and a literal U+2003, so there is no behavior gap. | Either correct the summary (character references only), or add the literal forms to the test. If the literal form is added, write it so that it survives `scripts/format.sh`. |

### Evidence for finding 1

The inputs below were run with the harness, in `Diagnose` and in `Parse` mode, with the same verdicts in both. A backslash-u sequence below is the literal six characters in the source.

```csharp
/// <summary>X</summary>
public static class Ext
{
    /// <summary>control wrong name</summary>
    /// <param name="x">V</param>
    extension(string value) { }

    /// <summary>escaped attribute</summary>
    /// <param name="v\u0061lue">V</param>
    extension(short value) { }

    /// <summary>ZWSP attribute</summary>
    /// <param name="va?lue">V</param>          // ? is U+200B
    extension(int value) { }

    /// <summary>escaped typeparam</summary>
    /// <typeparam name="\u0054">T</typeparam>
    /// <param name="values">V</param>
    extension<T>(T[] values) { }
}

/// <summary>C</summary>
public class C
{
    /// <summary>M</summary>
    /// <param name="v\u0061lue">V</param>
    /// <param name="b">V</param>
    public void M(int value, int b) { }

    /// <summary>N</summary>
    /// <param name="x">V</param>
    /// <param name="b">V</param>
    public void N(int value, int b) { }
}
```

Output:
- Control block (`name="x"`): CS1572 and RH8032.
- `v\u0061lue` block: RH8032 on `value`, no CS1572.
- U+200B block: RH8032 on `value`, no CS1572.
- `\u0054` block: RH8033 on `T`, no CS1711.
- `M`: no CS1572 and no CS1573, so the compiler pairs `v\u0061lue` with `value`.
- `N`: CS1572 for `x` and CS1573 for `value`.
- With `name="va&#x200B;lue"` there is likewise RH8032 and no CS1572.

The compiler writes the attribute text unchanged into the generated documentation (`<param name="v\u0061lue">`), and RH8032 compares that text, after XML decoding, ordinally with `Identifier.ValueText`.

A second input follows the rule page's advice: `/// <param name="\u0061">V</param> extension(int \u0061) { }` and `/// <typeparam name="\u0054">T</typeparam> … extension<\u0054>(\u0054[] values) { }`. It gives RH8032 on `\u0061` and RH8033 on `\u0054`, and no compiler warning.

On the parameter side, the code already uses the identifier's value:
- `extension(byte \u0061)` with `name="a"` is not reported.
- A parameter with a literal U+200B inside `value`, documented by `name="value"`, is not reported.
- `name="@class"` keeps CS1572 and RH8032, as SHARED-DOC-012 states.

No test in the 15 classes contains an escape or a format character in a name (`grep` for `\uXXXX`, `x200B`, `xFEFF`, `x2060`, `xAD;`: none), so no test has to change for option (a).

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Group of a member without access modifier in an extension block (SHARED-DOC-002 edit) | `MemberDeclarationSyntax.Modifiers`, then `declaration.Parent` (`HasImplicitPrivateAccessibility`: `InterfaceDeclarationSyntax` gives non-private; any other `TypeDeclarationSyntax`, `ExtensionBlockDeclarationSyntax` included, gives private; a namespace or compilation unit gives non-private) | The compiler's `DeclaredAccessibility` (Private for such a member); SHARED-DOC-002's container lists | The `TypeDeclarationSyntax` subtypes are exactly class, struct, interface, record (and record struct), and extension block; enums only hold enum members (SHARED-DOC-003). Spans agree. Both sides are tested (RH8020/RH8021, RH8022/RH8023). | ok |
| Containers of SHARED-DOC-001 (extension block added) | Registration by `SyntaxKind` only, with no parent filter (`SplitElementDocumentationAnalyzerBase`) | The container list of SHARED-DOC-001 and the kind definitions of RH8020-001 … RH8034-001 | They agree. The kind lists leave out what does not compile in an extension block: fields, events, and conversion operators (CS9282), and indexers (CS8652). | ok |
| Explicit implementations exempt (SHARED-DOC-009) | `ExplicitInterfaceSpecifier != null` on method, property, indexer, event, operator, and conversion (`IsExplicitInterfaceImplementation`) | SHARED-DOC-009's six kinds; `HasSyntaxLevelInheritance` reads the same specifier, but for the inheritance decision | Same span and kinds. `EventFieldDeclaration` cannot carry a specifier. Tested in a class and in an interface for every kind. | ok |
| Partial members out of coverage (SHARED-DOC-001/005) | No predicate: each part is decided by its own leading trivia, and the semantic fallback reads the merged symbol | Coverage sentences; shared Q1 | Deliberately unspecified. No test asserts a per-part verdict (`grep partial`: only single-part classes and `static partial class Extensions`). | ok |
| Summary without text (SHARED-DOC-010) | `IsEmpty` / `HasMeaningfulContent`: an empty element; `XmlTextSyntax` token `ValueText` with `string.IsNullOrWhiteSpace`, which equals Unicode White_Space; exterior trivia excluded; an element or CDATA is text; a comment or PI is text by `ToString()` | Normative sentence and coverage of SHARED-DOC-010 | Agree inside coverage. Harness: literal U+00A0 and U+2003 are reported. A `*` after leading U+00A0, U+2003, VT, FF, or tab is a line break (reported), matching "after its leading white space". | ok |
| No documentation comment (SHARED-DOC-011 first clause; RH8031-001) | `GetDocumentationComment`: the first `DocumentationCommentTriviaSyntax` in `declaration.GetLeadingTrivia()` | SHARED-DOC-004 | Same span: before the first token, with directives and ordinary comments not separating. For merged declarations, decided per declaration (tested). | ok |
| `<inheritdoc>` exempts parameters (SHARED-DOC-011) | `HasInheritdoc`: a direct top-level tag of the first comment, or a top-level element of the expanded documentation, local name case-insensitive | SHARED-DOC-011 (lower case only, in coverage; Q4 holds the rest) | Agree in coverage: first comment, later comment, include, and shared documentation are all tested. | ok |
| Parameter documented by name (SHARED-DOC-012) | The XML-decoded `name` of each top-level `param`/`typeparam` in the compiler's documentation, compared ordinally with `Identifier.ValueText` | The compiler's binder for `name` (CS1572/CS1573/CS1711); SHARED-DOC-012's `Why:` | Disagree for a `name` containing a C# Unicode escape or a format character: the compiler binds it, the rule reports it, and the spec keeps the case in coverage. The padded `name` disagrees the same way but is out of coverage (Q5). The `@` case agrees. | finding 1 |
| Merged extension declarations (SHARED-DOC-013) | The merged symbol's `GetDocumentationCommentXml` | The descriptive merge paragraph | Agree on the spec's pairs. Pairs I added, all consistent with the paragraph: merge for `nint`/`IntPtr`, `value`/`@value`, `[A, B]`/`[B, A]`, `[A]`/`[A()]`, and `(int, int)`/`ValueTuple<int, int>`; no merge for `[A(N = 1)]`/`[A(N = 2)]`. | ok |
| Which summary RH8030 checks | The first top-level `summary` (case-insensitive) of the first comment | RH8030-001 coverage | Agree in coverage. The wording leaves out declarations without a comment (finding 2). | finding 2 |
| Documentation mode (SHARED-DOC-008) | `RegisterSyntaxNodeActionWithDocumentationModeCheck` in all 15 analyzers | SHARED-DOC-008 | Agree. The `Parse` verdicts equal the `Diagnose` verdicts on the harness fixtures, including documented parameters that are not reported. | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Documentation comments of a declaration (SHARED-DOC-004) | `DirectDocumentationSyntaxChecker.GetDocumentationComment`. The other `OfType<DocumentationCommentTriviaSyntax>` use, `RH8401…CodeFixProvider`, finds the comment that encloses a trivia, a different decision. | single owner |
| Summary has text (SHARED-DOC-010) | `DocumentationAnalysisUtilities.IsEmpty` / `HasMeaningfulContent`, used by RH8030 and RH8031, and also by RH8104, RH8106, and RH8111. `RH8304…Analyzer` has a similar white-space test for line layout, a different decision. | single owner |
| Private/non-private group (SHARED-DOC-002) | `IsPurePrivateDeclaration` / `HasImplicitPrivateAccessibility` | single owner |
| Explicit implementation exemption (SHARED-DOC-009) | `IsExplicitInterfaceImplementation`, through `NeedsDocumentation`. `HasSyntaxLevelInheritance` uses the same specifier for inheritance. | single owner |
| Declaration documented (SHARED-DOC-005 to SHARED-DOC-007) | `DocumentationAnalysisUtilities.HasRequiredDocumentation`: two overloads with identical bodies, for members and for enum members. Only the member overload is reached by RH8020–RH8029 and RH8034. | single type; pre-existing twin overload, not reached by this run |
| `<inheritdoc>` exemption (SHARED-DOC-011) | `XmlDocumentationExpander.HasInheritdoc`, called by RH8032, RH8033, RH8101, RH8108, and `HasRequiredDocumentation` | single owner |
| Parameter documented by name (SHARED-DOC-012) | Private copies in `RH8032…Analyzer`, `RH8033…Analyzer`, `RH8101…Analyzer`, and `RH8108…Analyzer`, all identical: expanded `param`/`typeparam`, `Attribute("name")`, `StringComparer.Ordinal` against `Identifier.ValueText`. `RH8102`/`RH8109` read `name` through syntax (`GetNameAttributeValue`) for another decision. | Four owners at parity. Pre-existing and unchanged by the diff. See Hints. |
| Merge of extension declarations (SHARED-DOC-013) | The compiler, through `GetDeclaredSymbol(ExtensionBlockDeclarationSyntax)`; no private copy | single owner |

## Assertion adequacy

Every test of the 15 classes calls `AnalyzerTestsBase<TAnalyzer>.Verify` with numbered markup and `Diagnostics(id, message, n)`. That asserts the exact set of reported locations, their count, and the message: a missing or extra diagnostic fails. A test with no expectation asserts that nothing is reported. Expected values come from the spec text and examples, not from the code (FC-035). Every new spec example block appears verbatim in a test; a script compared every C# block of the 16 specs with the test sources. The combined RH8032/RH8033 blocks of SHARED-DOC-012 and SHARED-DOC-013 run per rule with that rule's marks: 3 and 2 marks, and 3 and 3 marks.

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH8020 (15 new, 5 tagged) | Non-private methods in every container are reported on the identifier, without the type parameter list, with `@` included. The group comes from the method's own modifiers. Explicit implementations, private, and local functions are not reported. SHARED-DOC-004 to SHARED-DOC-008 arms; extension members decided on their own | Any extra or missing location | Verify; None, Parse, and include transforms | adequate |
| RH8021 (8 new, 4 tagged) | The private group in class, struct, record, record struct, interface (`private`), and extension block; explicit implementations exempt; generic name span | same | Verify; None | adequate |
| RH8022 / RH8023 (8+6 new, 3+2 tagged) | The group comes from the property's modifiers, not from its accessors; every form; interface `static abstract`; extension block; explicit implementations exempt; an indexer is not a property | same | Verify; None | adequate |
| RH8024 / RH8025 (5+5 new, 2+2 tagged) | Location on `this`; group from the indexer's modifiers; explicit implementations exempt; a property is not an indexer | same | Verify; None | adequate |
| RH8026 / RH8027 (6+4 new, 2+2 tagged) | One diagnostic on the first variable; const, static, and readonly fields; events and enum members excluded; interface static fields | same | Verify; None | adequate |
| RH8028 / RH8029 (5+4 new, 4+2 tagged) | `EventDeclaration` and `EventFieldDeclaration`; first variable of a field-like declaration; explicit implementations exempt | same | Verify; None | adequate |
| RH8030 (12 new, 7 tagged) | Whole-element span for every SHARED-DOC-010 form (empty element, `///`, `/** */`, star runs, `&#32;`, `&#160;`, Unicode white space); every declaration kind; any accessibility; one diagnostic per multi-variable declaration; `<inheritdoc>` does not prevent the report; nested summary and extension declaration not reported | same | Verify; LF and `ReplaceLineEndings("\r\n")`; None; Parse | adequate. `VerifyDiagnosticForSummaryOfUnicodeWhiteSpaceOnly`: summary overstates (finding 3). `VerifySummaryPlacement…` class `B`: wording (finding 2). |
| RH8031 (9 new, 5 tagged) | Keyword location without a summary, summary location without text, LF and CRLF; the class's documentation and accessibility do not matter; SHARED-DOC-004 gap with a directive and an ordinary comment | same | Verify; CRLF; None; Parse | adequate |
| RH8032 (15 new, 3 tagged) | Exemption by no comment, first-comment, later-comment, and included `<inheritdoc>`; top-level `param` by exact case-sensitive name; `@`; included and later-comment `param`; merged declarations share documentation, and every non-merging qualifier keeps them apart; an undocumented merged block is not reported; merging across files | same | Verify; include; second file (`TestState.Sources`); Parse | adequate. The test implementer's mutation (`param` renamed in file 1) failed the cross-file test as expected. |
| RH8033 (9 new, 3 tagged) | Each undocumented type parameter is reported, with names compared case-sensitively (`name="t"`); nested `typeparam`; include; `<inheritdoc>` exemption; merge and non-merge qualifiers (name, `class`/`class?`, attribute, constraint order) | same | Verify; include; Parse | adequate |
| RH8034 (6 new, 6 tagged) | Operator-token location for binary, unary, `true`/`false`, `++`, `>>>`, checked, and compound assignment operators; `operator` keyword location for conversions; operators in a class, record, record struct, and extension block; interface operators whatever their modifiers; explicit operator and conversion exempt | same | Verify; None | adequate |

FC-033 checks:
- No test outside the SHARED-DOC-013 tests repeats an extension receiver in one source. A script scanned the extension headers of every test method in the 15 classes. The only repeats are in `RH8032…MergedExtensionDeclarationsShareDocumentation`, `…AcrossFilesOfPartialClass…`, `…UndocumentedMergedExtensionDeclaration…`, `…AnnotatedAndOblivious…`, `…MergeQualifiersOfParameters`, and `RH8033…MergeQualifiersOfTypeParameters`, and all of them are about merging.
- The SHARED-DOC-010 cases run under CRLF through an explicit `ReplaceLineEndings("\r\n")`.

## Failure classes dismissed

- FC-001 — no code guard changed. Every spec exemption and coverage boundary was traced against its code predicate (Guard scope). The one disagreement is finding 1, and the wording issue is finding 2.
- FC-002 — the qualifiers "top level", "first/later comment", "lower case", and "with a name" are tested on both sides. The one qualifier whose wording misstates its scope is finding 2.
- FC-003 — both sides tested for SHARED-DOC-010 (each form, text versus no text, `*` at line start versus after a space or mid-line), for the group of each container, for `private protected` versus `private`, and for `<inheritdoc>` present versus absent. The RH8032/RH8033 `Parse` tests assert only the reported side; the harness shows the documented side agrees (Hints).
- FC-004, FC-005, FC-019, FC-020, FC-021 — none of the 15 rules edits trivia: there is no code fix and no rewrite. SHARED-DOC-004's comment and directive gaps are tested with `#if DEBUG`/`#endif` and an ordinary comment in RH8020, RH8030, and RH8031.
- FC-006, FC-022, FC-025 to FC-029, FC-036 — no rewrite and no rename. FC-025 presence does not depend on body form: abstract, expression-bodied, block-bodied, and extern methods; auto, expression-bodied, accessor-bodied, abstract, override, and static properties; expression- and accessor-bodied indexers.
- FC-007, FC-008, FC-009, FC-018, FC-030 — none of RH8020–RH8034 has a code-fix provider (`Code fix: None`, checked by `SpecStructureTests`).
- FC-010 to FC-014 — no formatter change, and documentation presence and summary text are owned by no formatter phase. These are existing rules, so no new concern is left unowned.
- FC-015 — the policies have single owners, except SHARED-DOC-012's four identical private copies. Those are pre-existing and at parity (Policy ownership, Hints).
- FC-016 — the sibling shapes of finding 1 (`param` and `typeparam`, RH8032 and RH8033) are in the finding. The sibling kinds of each rule are tested (event/event field; operator, conversion, checked, compound; every explicit-implementation kind).
- FC-017 — every assertion asserts the exact diagnostic set.
- FC-023 — `@` agrees with the compiler and the spec. Unicode escapes and format characters are finding 1.
- FC-024 — partial members are out of coverage and untested on purpose. Explicit implementations, `file` types, and static abstract/virtual interface members are tested.
- FC-031 — the changed test summaries describe their sources, except finding 3. No production method changed.
- FC-032 — no compiled production change. 10, 11, and 12 ran after the last test edit (3d2f680), and HEAD adds only handovers since.
- FC-033 — see Assertion adequacy. The LF tests rely on the checkout's line endings (Hints).
- FC-034 — every new spec example block is a test, verbatim (script).
- FC-035 — no code was patched; expected values come from the specs.
- FC-037 — no comment-only C# change is claimed.

## Hints

- SHARED-DOC-012 now states one shared policy that four analyzers each implement privately and identically (RH8032, RH8033, RH8101, RH8108). RH8102 and RH8109 read the same `name` attribute through syntax (`DocumentationAnalysisUtilities.GetNameAttributeValue`) instead of through the compiler's documentation. When thoenissen/Reihitsu#984 specifies RH8101–RH8111, it should check that those rules read `name="@class"` and an escaped `name` the same way. Follow-up candidate; pre-existing and unchanged.
- The tests named `…WithLineFeeds` (RH8030, RH8031) do not force LF. They use the checkout's line endings, and the repository has no `.gitattributes`. On a checkout with `core.autocrlf=true`, both variants would run CRLF. This is the repository-wide pattern (RH5038–RH5049 do the same), not introduced here.
- `RH8032…VerifyDiagnosticForUndocumentedExtensionParameterInParseMode` and the RH8033 counterpart assert only the reported side in `Parse` mode. The harness shows that documented parameters are not reported in `Parse` mode either, so there is no behavior gap.
- For the lead's filing of shared Q7: C0 and C1 control characters (Unicode Cc, for example `&#x7F;` and `&#x9F;`) and U+3164 also render nothing and count as text (harness: not reported by RH8030). Q7's boundary is Cf only.
- `RH8032…VerifySpecificationExampleParameterIsDocumentedByIncludedFile` uses the spec's C# block verbatim, but its `docs.xml` also carries members `B` and `C`, which the spec's file does not. They are never selected, so this is harmless.
