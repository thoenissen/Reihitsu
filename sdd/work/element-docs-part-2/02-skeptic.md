Role: skeptic
Commit: 1e784d9671e3aab9757a6231c9b6f236fb63e20b
Verdict: proceed narrower

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

The run specifies the current behavior of RH8020–RH8034 and extends `shared/element-documentation.md`. Production code is unchanged since 53990ed (`git diff 53990ed HEAD -- src` is empty).

Code read: `SplitElementDocumentationAnalyzerBase.OnDeclaration`, `DocumentationAnalysisUtilities` (`NeedsDocumentation`, `IsExplicitInterfaceImplementation`, `HasRequiredDocumentation`, `IsPurePrivateDeclaration`, `HasImplicitPrivateAccessibility`, `IsEmpty`, `HasMeaningfulContent`, `GetDiagnosticLocation`), `DirectDocumentationSyntaxChecker` (`GetDocumentationComment`, `GetDirectTags`), `XmlDocumentationExpander`, `DocumentationModeAnalysisContextExtensions`, the 15 rule analyzers, and RH8101/RH8108 (same parameter logic as RH8032/RH8033, plus `NeedsDocumentation`).

How the facts below were established: no repository entry point runs an analyzer without a code fix on a free input (`apply-fix.sh` refuses an ID that no provider fixes), so I used my own scratch harness outside the repository, not the spec author's. It compiles one file with Roslyn 5.9.0, `LanguageVersion.Latest`, an `XmlFileResolver` on the file's folder, and the Debug `Reihitsu.Analyzer.dll` built after the last change to `src/Reihitsu.Analyzer`. It prints the RH80xx diagnostics, the compiler warnings, and `GetDocumentationCommentXml(expandIncludes: true)` with each symbol's documentation ID and declared accessibility. Mode is `Diagnose` unless stated. The fixtures have been deleted.

Confirmed as the diff states it:

- SHARED-DOC-002 (extension block). A member without a modifier in `extension(string value)` has `Accessibility.Private` and is reported by RH8021 only (`int Hidden() => 0;` gives RH8021 on `Hidden`). This answers the question in thoenissen/Reihitsu#1055. A conversion operator, event, or field in an extension block gives CS9282. An extension indexer gives CS8652 (preview). So no verdict of RH8001–RH8019 changes. There is no instance of thoenissen/Reihitsu#1092 here: the symbol's accessibility and the syntactic group agree.
- Extension-block members are decided on their own. With an undocumented block and a documented member `A`, nothing is reported for `A`. With a documented block and a member `B` that has only `<remarks>`, RH8020 reports `B`. The compiler's documentation of `B` holds only its `<remarks>`; nothing comes from the block.
- RH8032 shared-spec Q1: an unnamed receiver `extension(long)` gives one RH8032 with `Location.IsInSource == false`. Shared Q1: the `<remarks>`-only partial `M3` is accepted, and `partial void M4(); partial void M4() { }` gives RH8021 on both parts.
- RH8030 and RH8031 verdicts on whitespace-only summaries are the same under LF and CRLF.
- The operator token is the location for `operator true`, `operator false`, and `operator >>>`. An explicit interface conversion `static explicit IConv<N>.operator int(N t)` is not reported.

Spec text that the code contradicts, or that states a case that looks wrong as intended. Each needs a spec-author round before tests are derived from it:

**F1 — An extension declaration that the compiler merges with another block (RH8031-001, RH8031-002, RH8032-001, RH8033-001, SHARED-DOC-011, SHARED-DOC-012).** Two extension blocks in one class whose receivers are declared identically become one symbol. Both blocks get the same documentation ID and one merged documentation entry.

```csharp
/// <summary>X</summary>
public static class Extensions
{
    /// <summary>First</summary>
    extension(string value) { }

    /// <summary>Second</summary>
    /// <param name="value">V</param>
    extension(string value) { }

    extension(string value) { }

    /// <summary>Long</summary>
    extension(long value) { }

    /// <inheritdoc/>
    extension(long value) { }
}
```

Real output: RH8032 reports nothing. RH8031 reports the `extension` keyword of the third `string` block and of the `<inheritdoc/>` `long` block. The compiler gives no CS1573. Its documentation for every `string` block is `<summary>First</summary><summary>Second</summary><param name="value">V</param>`, and for both `long` blocks it is `<summary>Long</summary><inheritdoc/>`. The same holds for `extension<T>(T[] values)` written twice, where only the first has `<typeparam>`: RH8033 reports nothing. Parse mode gives the same verdicts.

As SHARED-DOC-011 and SHARED-DOC-012 are written, the element's documentation is the block's own comments (SHARED-DOC-004). Under that reading, `value` of the first `string` block and of the `<summary>Long</summary>` block MUST be reported, so the spec contradicts the code on an input it covers.

Merge condition observed: the receiver type must match including nullability, along with the ref kind, the parameter name, the parameter attributes, and the type-parameter names and constraints. None of these pairs merges: `string value`/`string s`, `string`/`string?`, `ref int`/`in int`/`int`, `[A] long`/`long`, `<T>`/`<U>`, and `<T>` with and without `where T : class`.

For RH8032 and RH8033 the current output follows the compiler's merged documentation. Two ways forward give the same output: (a) state that a merged declaration's documentation includes the comments of every block it is merged with; (b) leave merged declarations out of the coverage of SHARED-DOC-011 and SHARED-DOC-012 and file the case. Whether the current output looks wrong is the spec author's call. For RH8031 the per-block verdict disagrees with the merged documentation in the same way that shared Q1 (partial members) does. It is not specifiable as written: leave it out of RH8031-001's coverage, which RH8031-002 inherits, and file it. It is P8-shaped if RH8031 Q1 keeps its P8 marking (see F8).

**F2 — RH8030-001's coverage does not exclude the tag-name and XML shapes that SHARED-DOC-005 and RH8031-001 exclude.** With one documentation comment each:

| Input | RH8030 | Compiler |
|---|---|---|
| `/// <Summary></Summary>` | reported on the whole element | — |
| `/// <x:summary xmlns:x="urn:x"></x:summary>` | reported | — |
| `/// <summary>` with no end tag | reported, span `<summary>` plus the line break (no end tag) | CS1570 |
| `/// <summary></summary> a & b` | reported | CS1570 |

As written, the normative sentence either requires the report (if `<Summary>` counts as "the `<summary>` element") or forbids it. Either way it is decided by text that thoenissen/Reihitsu#1091 (from #1037 Q2 and Q5: malformed XML and case or namespace variants are not a summary) and RH8030 Q2 reading (B)/(C) treat as not a summary. The coverage needs "well-formed XML, the tag name `summary` in lower case, without a namespace prefix, and in no XML namespace". The four shapes belong with RH8030 Q2. RH8030-002's "through its end tag" also has nothing to point at for the unclosed form.

**F3 — SHARED-DOC-012 states a report that the compiler disagrees with.** `/// <param name=" value ">V</param>` on `extension(byte value)` gives RH8032 on `value`. The compiler binds it: no CS1572 there. For `void M(int a, int b)`, `<param name=" a ">` with `<param name="b">` gives no CS1573. In contrast, `<param name="x">` on `N(int a, int b)` gives CS1572 for `x` and CS1573 for `a`. SHARED-DOC-012's "compared character by character" makes that report intended, and its `Why:` ("pair … exactly as written") is false for this input. Leave a `name` value with surrounding whitespace out of coverage and file it in the shared spec's issue. The lead decides whether P3 ("compared by value … without whitespace") reaches it.

**F4 — SHARED-DOC-012 does not say whether `@` belongs to "the parameter's name".** Two inputs:

- `extension(int @class)` with `<param name="class">` is not reported, and the compiler is silent.
- `extension(string @class)` with `<param name="@class">` gives RH8032 on `@class`, and the compiler gives CS1572 for `'@class'`. The same holds for `<typeparam name="@T">` (CS1711, RH8033 on `T`).

The current behavior agrees with the compiler: the attribute is compared with the name without its `@`. The requirement should say so and keep an example on each side. The lead checks whether P3 (thoenissen/Reihitsu#912: "compares a name by its value — without the `@` prefix") is read to cover `name="@class"`. If it is, that shape leaves coverage and is filed as `Decided by principle P3 (#912)`. The RH8032 rule page ("spell the name exactly as it appears in the declaration") is wrong for `@class` either way.

**F5 — SHARED-DOC-010 names only the `///` continuation.** This input is reported by RH8030 because the leading `*` is exterior trivia:

```csharp
/**
 * <summary>
 * </summary>
 */
```

As written, `*` is "a character other than whitespace", so the requirement says the summary has text. The same shape with `* Text` is not reported. The `*` that continues a `/** */` comment belongs next to the `///`. RH8031 uses the same `IsEmpty`.

**F6 — SHARED-DOC-001 lists the containers "in a namespace, at file level, or nested in a class, struct, interface, record, record struct, or enum" without the extension block.** RH8020-001, RH8021-001, RH8022-001, RH8023-001, and RH8034-001 check extension-block members "as SHARED-DOC-001 specifies". The behavior is per declaration, as SHARED-DOC-001 intends (see Premise). Adding "extension block" there is another text edit of an existing requirement. `squad/decisions/2026-10-18-shared-doc-coverage-and-extension-block.md` approved only the listed edits, so the lead decides whether to ask or whether the rule specs carry it.

**F7 — Minor precision:**

- RH8034-001 says that conversion operators are checked in an extension block, but there they give CS9282. Only operator declarations compile in an extension block.
- RH8030-002 and RH8031-003 say "from its start tag through its end tag", which does not fit `<summary/>`.
- SHARED-DOC-010's "whitespace" should be decidable: `&#160;` counts as no text and is reported, while `&#x200B;` counts as text. That is Unicode White_Space.
- A processing instruction (`<summary><?pi ?></summary>`) counts as text. It is outside SHARED-DOC-010's coverage, and RH8030 Q2's list of markup could name it.
- The shared non-goal "RH8032, RH8033, and RH8101 to RH8111" overlaps the return-value rules RH8105–RH8107 named in the same bullet.

**F8 — Consistency of the open-question markings (for the lead).**

- RH8031 Q1 is marked `Decided by principle P8 (#912)` because "RH8031 is the presence rule". Yet RH8031 Q2 (`<inheritdoc>` reported by RH8031) is a normal question, although P8 counts a top-level `<inheritdoc>`. Either both are P8 instances or Q1's marking needs a different reason.
- RH8030 Q2 shapes `B` (dropped comment) and `F` (`<Summary>`), and the malformed shapes of F2, are the #1037 Q1/Q2/Q5 consequences listed in thoenissen/Reihitsu#1091. Shapes `A`, `C`, and `D` are genuinely open.

**F9 — Partial parts and RH8030.** RH8030-001 covers each part of a partial type on its own. `/// <summary></summary> public partial class J { }` next to a part with text is reported. The compiler merges the parts into two `<summary>` elements, so reading (B) of RH8030 Q2 ("the summary of the documentation the compiler produces") does not decide it. Consider leaving partial parts out of RH8030-001 as SHARED-DOC-001/005 now do, and naming the shape in RH8030 Q2.

## Mechanism and candidates

_N/A._ This is not a bug fix. The candidate set derived from the dispatch code is under Breaking inputs.

## Breaking inputs

Dispatch:

- `SplitElementDocumentationAnalyzerBase` registers these kinds:
  - `MethodDeclaration` (RH8020/RH8021)
  - `PropertyDeclaration` (RH8022/RH8023)
  - `IndexerDeclaration` (RH8024/RH8025)
  - `FieldDeclaration` (RH8026/RH8027)
  - `EventDeclaration` and `EventFieldDeclaration` (RH8028/RH8029)
  - `OperatorDeclaration` and `ConversionOperatorDeclaration` (RH8034, group `Any`)
- The gates run in this order:
  1. `NeedsDocumentation`: an explicit specifier on a method, property, indexer, event, operator, or conversion.
  2. The group: modifiers first, then the parent kind. `InterfaceDeclarationSyntax` gives non-private. Any other `TypeDeclarationSyntax`, `ExtensionBlockDeclarationSyntax` included, gives private.
  3. A direct `summary`/`inheritdoc` in the first `DocumentationCommentTriviaSyntax`, compared case-insensitively.
  4. The semantic fallback on `GetDocumentationCommentXml(expandIncludes: true)`.
- RH8030 registers `SummaryDocumentationKinds` (the 17 documentable kinds plus `EnumMemberDeclaration`) and skips explicit implementations.
- RH8031–RH8033 register `ExtensionBlockDeclaration`.

Failure classes that reach these specs, with the inputs the test implementer must cover:

- **FC-002 (qualifiers become conditions).** Each of the following on both sides: "top level" (nested in `<remarks>`); "first documentation comment" versus a later one (SHARED-DOC-006, SHARED-DOC-011/012); the lower-case tag name for every requirement that keeps it in coverage; "with a name" (RH8032-001).
- **FC-003 (both sides of a boundary).**
  - Empty versus text for each SHARED-DOC-010 form: `<summary/>`, `<summary></summary>`, `///` continuation, `&#32;`, `/** */` with `*` continuation once F5 is settled.
  - `<inheritdoc>` present versus absent, for RH8030 (no effect) and for RH8032/RH8033 (exempts).
  - The modifier-less group in each container: class, struct, record, record struct, interface, and extension block.
  - `private protected` (non-private) next to `private`.
- **FC-016 (sibling shapes).**
  - `EventDeclaration` and `EventFieldDeclaration` for both event rules.
  - `OperatorDeclaration`, `ConversionOperatorDeclaration`, a checked operator, and a compound assignment operator for RH8034.
  - A field or event declaration of several variables, located on the first identifier.
  - Every explicit-implementation kind of SHARED-DOC-009, in a class and in an interface.
  - The merged-extension-block shape (F1) is the sibling of the partial-member question.
- **FC-023 (verbatim identifiers).** `@class` as a receiver name, with `<param name="class">` and with `<param name="@class">` (F4). A method or property named with `@`: the diagnostic span includes the `@`.
- **FC-024 (partial, explicit implementations, `file`, static interface members).**
  - Partial methods, properties, indexers, and events are out of coverage, so no test asserts their per-part verdict.
  - Explicit implementations: in a class (private group), in an interface (non-private), and an explicit operator or conversion (RH8034).
  - `static abstract` operators and other members of an interface.
  - A member of a `file` class: the group comes from its own modifiers.
- **FC-025 (expression-bodied members).** Expression-bodied method, property, indexer, and operator. Presence must not depend on the body form.
- **FC-033 (harness normalization).**
  - Two extension blocks with an identical receiver in one test class share one documentation, so a test can pass for the wrong reason. Never repeat a receiver within one test source unless the test is about merging (F1).
  - The SHARED-DOC-010 cases under CRLF as well as LF.
  - SHARED-DOC-008 needs a `None`-mode transform (`ApplyDocumentationModeNoneToTestProject`), because the verifier's default mode is `Diagnose`.
- **FC-034 (the reported shape itself).** Seed the tests from the spec's own example blocks. Every example block in the diff uses distinct receivers, which is consistent with FC-033.

FC-001, FC-004 to FC-014, FC-018 to FC-022, FC-026 to FC-030, FC-036, and FC-037 do not reach the run: these rules have no code fix, no rewrite, no formatter phase, and no comment-only C# change.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzers RH8020–RH8034, `SplitElementDocumentationAnalyzerBase`, `DocumentationAnalysisUtilities`, `DirectDocumentationSyntaxChecker`, `XmlDocumentationExpander` | must stay (lane: no production change) |
| Code fix / Fix All | not involved: none of the 15 rules has a provider |
| Formatter phases | not involved: no phase owns documentation presence or summary text. FC-013/FC-014 are not reached by a spec-only run of existing rules |
| `Reihitsu.Core` | not involved |
| Specs `analyzer/RH8020.md`–`RH8034.md`, `shared/element-documentation.md` | must change: F1–F7, and F9 if taken |
| Rule docs `documentation/rules/RH8020.md`–`RH8034.md` | must stay, except line 20 of RH8032 and RH8033 ("spell the name exactly as it appears in the declaration"), which is wrong for `@class`/`@T` (F4) |
| Tests `src/Reihitsu.Analyzer.Test/Documentation/RH8020…`–`RH8034…Tests.cs` | must change: tag the existing tests and add the covering tests per requirement |
| `sdd/specs/requirement-ids.txt`, `sdd/specs/README.md` | must stay as the spec author left them, unless F1–F9 add requirements |

## Simpler option

_None._

## Questions for the maintainer

_None that block the run._ Each competing reading above leaves a case out of coverage and files it as an open question, as `sdd/README.md` prescribes. Candidates to file once the spec author has revised the coverage:

- F1. RH8031 part: per-block verdict versus the merged documentation, the same shape as shared Q1 and P8 if F8 keeps RH8031 under P8. RH8032/RH8033 part: only if the spec author chooses (b).
- F2 shapes and F9: add to RH8030 Q2.
- F3: shared spec issue, or P3 if the lead reads it so.
- F4 `name="@class"`: only if the lead reads P3 to cover it.
