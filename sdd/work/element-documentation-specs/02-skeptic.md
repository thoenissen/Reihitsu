Role: skeptic
Commit: 72c9e13e218897e7e4af1d183c8ef81bd2536c25
Verdict: proceed narrower

My prompt carried no intended fix, no suspected cause, and no hoped-for verdict. I treated the spec author's "fact for the skeptic" notes as inputs to check, not as conclusions.

## Premise

Issue thoenissen/Reihitsu#982 asks for specs of existing behavior. The code matches what `01-spec-author.md` describes:

- RH8001–RH8012 and RH8015–RH8018 go through `SplitElementDocumentationAnalyzerBase`, each with one `SyntaxKind` and either `NonPrivate` or `Private`.
- RH8019 uses `Any`.
- RH8013 and RH8014 are standalone analyzers on `EnumMemberDeclaration`.
- Every registration goes through `RegisterSyntaxNodeActionWithDocumentationModeCheck`.
- None of the rules has a code-fix provider.

The run's premise holds. The spec diff does not yet describe the code's behavior for every input it decides. In three places it writes down, as intended behavior, a case that looks wrong when measured against the spec's own `Why:`.

How I established this:

- I read `SplitElementDocumentationAnalyzerBase.OnDeclaration`, the `DocumentationAnalysisUtilities` members `HasRequiredDocumentation`, `MatchesAccessibilityGroup`, `IsPurePrivateDeclaration`, `HasImplicitPrivateAccessibility` and `GetDiagnosticLocation`, `DirectDocumentationSyntaxChecker.GetDocumentationComment`/`GetDirectTags`, `XmlDocumentationExpander.HasTag`/`ParseExpandedDocumentation`, and the `OnDeclaration` methods of RH8013 and RH8107.
- I then built a throwaway `net10.0` class library in the session scratchpad, outside the repository, and deleted it afterwards:
  - `GenerateDocumentationFile=true`, so the documentation mode is `Diagnose`;
  - the Debug `Reihitsu.Analyzer.dll` and `Reihitsu.Core.dll` from this commit, added as `<Analyzer>` items;
  - compiled by SDK 10.0.402, which ships Roslyn 5.9.0, the version the analyzer references.
- One build shows, for the same input, the RH80xx diagnostics, the compiler's CS1587 and CS1570 warnings, and the generated XML documentation.
- `scripts/apply-fix.sh` cannot run these rules, because it refuses a diagnostic ID that no code-fix provider fixes.

### Finding 1: the code contradicts SHARED-DOC-006, and SHARED-DOC-004 writes a case that looks wrong as intended

Each input below is a `public class` at file level, checked by RH8001. The first column lists the class's leading trivia from top to bottom.

| Input | RH8001 | CS1587 on | Generated `<member>` |
|---|---|---|---|
| D1 `/// <summary>` · `// c` | — | — | summary |
| D2–D6 `/// <summary>` followed by one of: `#if DEBUG`/`#endif`, `#region`, `#pragma warning`, `#if NEVER` disabled text, `/* c */` | — | — | summary |
| D7 `/// <remarks>` · `// c` · `/// <summary>` | — | the remarks comment | summary only |
| D8 `/// <summary>` · `// c` · `/// <remarks>` | — | the summary comment | remarks only |
| D9 `/// <remarks>` · blank line · `/// <summary>` | — | — | remarks and summary |
| D10 `/// <summary>` · blank line · `/// <remarks>` | — | — | summary and remarks |
| L1 `/// <remarks>` · `// c` · `/// <summary>` · `// c` · `/// <remarks>` | reported | first and second comment | last remarks only |
| L2 `/// <remarks>` · blank line · `/// <summary>` · `// c` · `/// <remarks>` | reported | first and second comment | last remarks only |
| L3 `/// <summary>` · `#region R` · `/// <remarks>` | — | the summary comment | remarks only |
| L4 `/// <summary>` · `#if DEBUG`/`#endif` · `/// <remarks>` | — | the summary comment | remarks only |
| L5 `/// <summary>` · `/* c */` · `/// <remarks>` | — | the summary comment | remarks only |
| L6 `/// <remarks>` · `#if DEBUG`/`#endif` · `/// <summary>` | — | the remarks comment | summary only |

- **L1 and L2.** SHARED-DOC-006 says a top-level `<summary>` in a later documentation comment MUST count. The code reports both inputs anyway. The coverage sentence of SHARED-DOC-006 (well-formed XML, a single declaration) does not exclude them, so the requirement is false as written. The code's output here does match the generated documentation.
- **D8, L3, L4 and L5.**
  - SHARED-DOC-004 says "the first of them is the declaration's documentation comment" and that ordinary comments and directives "MUST NOT separate them". Together with SHARED-DOC-005, that makes these four documented, and that is what the code does.
  - The compiler, however, drops that first comment with CS1587, so the generated documentation has no summary.
  - SHARED-DOC-004's own `Why:` says "A rule that read a different comment would disagree with the documentation the compiler generates", and that is exactly what happens here.
  - So the spec writes a case that looks wrong as intended. `sdd/README.md` (*Open questions*) says such a case is left out.
- **SHARED-DOC-006's `Why:` is false.** It says "The compiler joins every documentation comment that precedes a declaration". The compiler joins only comments shaped like D9 and D10, separated by nothing but blank lines.
  - The requirement's example has the D7 shape, with `// Separator` between the comments.
  - Its output (not reported) is correct, but only because the compiler drops the `<remarks>` comment, not because it joins the two.
- **One documentation comment is fine.** With a single documentation comment (D1–D6), SHARED-DOC-004 agrees with the compiler.

### Finding 2: SHARED-DOC-005 decides one side of the spec author's own second open question

`01-spec-author.md` files the question "A summary in a later documentation comment is ignored when the documentation is not well-formed XML". Its reading (B) is "malformed documentation never counts — `B` is reported too". The handover says both of its cases are left out of the spec through coverage sentences.

SHARED-DOC-005 nevertheless states that "an element without an end tag" counts, with example `I` (`/// <summary>No end tag`):

- That input is malformed documentation in the first comment, the same kind of input as the question's `B`.
- Observed: CS1570, `<!-- Badly formed XML comment ignored for member ... -->` in the generated file, and no RH8001.
- Under reading (B), example `I` would be reported, and SHARED-DOC-005 has no coverage sentence for malformed XML. So the spec already answers the question for the first comment.

The existing malformed-XML tests in `RH8001NonPrivateClassesMustBeDocumentedAnalyzerTests` and `RH8013NonPrivateEnumMembersMustBeDocumentedAnalyzerTests` check that nothing crashes. Tagging them with SHARED-DOC-005 would also lock in that side of the question.

### Finding 3: the static constructor of an interface

Observed, with no compile error for either:

- `public interface IStatic { static IStatic() { } }` is reported by RH8017.
- `public class CStatic { static CStatic() { } }` is reported by RH8018.

The cause: `IsPurePrivateDeclaration` finds no access modifier, and `HasImplicitPrivateAccessibility` returns `false` when the parent is an `InterfaceDeclarationSyntax`.

The specs disagree with each other on this input:

- SHARED-DOC-002's normative text puts it in the non-private group, so under RH8017, which matches the code: "a declaration without any of these five modifiers ... in an interface".
- SHARED-DOC-002's `Why:` says the opposite: "a static constructor, which takes no access modifier, is private".
- RH8018-001 also says the opposite: "A static constructor takes no access modifier, so it belongs to the private group".
- RH8017-001 and RH8018-001 both define a constructor declaration as one "in the body of a class, struct, record, or record struct", so neither rule's spec claims this constructor at all.

No code can ever call a static constructor, so having its rule depend on the containing declaration looks wrong.

### Finding 4: partial constructors are missing from the partial-type open question

Partial constructors are a C# 14 feature. The inputs below sit inside `/// <summary>PC</summary> public partial class PC`.

- **Defining part documented, implementing part not.** Input: `/// <summary>Defining int</summary> public partial PC(int value);` plus an undocumented `public partial PC(int value) { }`.
  - RH8017 reports the implementing part.
  - The generated documentation of `M:P.PC.#ctor(System.Int32)` contains "Defining int".
- **Defining part undocumented, implementing part documented.** Input: an undocumented `public partial PC(long value);` plus `/// <summary>Implementing long</summary> public partial PC(long value) { }`.
  - Nothing is reported.
  - The undocumented defining part is never reported.
  - A private pair (`partial Q(int value);` with no documentation on either part) behaves the same way under RH8018: the implementing part is reported, the defining part is not.
- **Why the defining part is skipped is not established.** `GetDocumentationComment` returns null for it, so the analyzer's code would report it if its action ran on that node.
- **Partial methods differ** (later batch, out of scope, for comparison only). RH8020 reports both an undocumented defining part and an undocumented implementing part.

The coverage sentence "an element with a single declaration" excludes partial constructors, so the spec itself is not wrong here. But the open question in `01-spec-author.md` names only partial types and RH8001–RH8010.

### Smaller findings

- **`<Summary>` is matched case-insensitively.**
  - Observed: the compiler writes `<Summary>Upper</Summary>` into the generated file unchanged, and RH8001 is not reported. XML element names are case-sensitive.
  - The `Why:` of SHARED-DOC-005 and of every RH80xx-001 argues from what the generated documentation and the tooltip show.
  - Matching case-insensitively is the same convention as RH8107-001 (already approved) and FMT-DOC-008. But no `Why:` gives a reason for it (see question 4).
- **"First attribute list, modifier, or keyword" is incomplete.** SHARED-DOC-004 says the leading trivia "comes before its first attribute list, modifier, or keyword". The first token can also be an identifier (the enum member `Red`, or `Service(int value)` in RH8018-001's own example) or `~` (a destructor).
- **RH8017's non-goal list is incomplete.** "Primary constructors ... RH8001, RH8003, RH8007, and RH8009" leaves out RH8002, RH8004, RH8008 and RH8010, whose types can declare primary constructors too.
- **Typos.** RH8005-001's `Why:` reads "A interface", and RH8011-001's reads "A enum".
- **RH8107.**
  - RH8107-001 now reads "the first one in the member's leading trivia, as SHARED-DOC-004 defines it".
  - The approved decision (`squad/decisions/2026-10-18-rh8107-shared-doc-reference-clauses.md`) assumes "behavior and IDs are unchanged".
  - In the D8 shape, RH8107 reports a `<returns>` element in the first comment. Its code uses `GetDocumentationComment` and `GetFirstDocumentingTag` without the compiler fallback.
  - If SHARED-DOC-004 is narrowed for Finding 1, the clause narrows RH8107-001 along with it (see question 5).
- **Reuse note for thoenissen/Reihitsu#983–#985.** This is not a defect in this batch.
  - `HasImplicitPrivateAccessibility` treats every `TypeDeclarationSyntax` parent except an interface as implicitly private, and that includes `ExtensionBlockDeclarationSyntax`.
  - SHARED-DOC-001 and SHARED-DOC-002 list only class, struct, interface, record, record struct and enum.
  - For the 19 kinds of this batch, a declaration directly inside an extension block does not compile (observed CS9282 for a class, a delegate and an enum).
  - Methods and properties from the later batches can be declared there.
- **`Parse` mode is barely verified.**
  - SHARED-DOC-008 requires checking trees in `Parse` mode, but the only `Parse` run so far is the spec author's run of SHARED-DOC-008's plain example.
  - SHARED-DOC-006 and SHARED-DOC-007 depend on `GetDocumentationCommentXml`, which has only been observed in `Diagnose` mode.

## Mechanism and candidates

This is not a bug fix. The mechanism behind Finding 1, which decides the outcome for any leading trivia:

- **The analyzer** reads the first documentation comment in the declaration's leading trivia from the syntax tree. Only when that comment has no top-level `<summary>` or `<inheritdoc>` does it fall back to the compiler's documentation of the symbol.
- **The compiler** walks back from the declaration and keeps only the documentation comments separated from each other by whitespace and line breaks alone. As soon as any other trivia stands between two documentation comments (an ordinary or block comment, a directive, or disabled text), it drops every earlier comment with CS1587.
- **The result:** whenever such trivia separates two documentation comments of one declaration, the analyzer counts a first comment that the compiler drops. It also ignores a summary in any dropped comment after the first.

Dispatch is by the trivia kinds in `MemberDeclarationSyntax.GetLeadingTrivia()`:

- **Kinds that join comments:** `SingleLineDocumentationCommentTrivia`, `MultiLineDocumentationCommentTrivia`, `WhitespaceTrivia` and `EndOfLineTrivia`.
- **Kinds that separate two documentation comments:** `SingleLineCommentTrivia`, `MultiLineCommentTrivia`, every directive trivia kind (`#if`/`#elif`/`#else`/`#endif`, `#region`/`#endregion`, `#pragma`, `#nullable`, and so on) and `DisabledTextTrivia`.
- **Observed between two documentation comments:** `//`, `/* */`, `#region` and `#if`/`#endif`.
- **Not run between two documentation comments:** `#pragma`, `#nullable` and disabled text. The mechanism puts them in the same branch, but I only observed them after a single documentation comment (D4, D5).

Candidate inputs:

- **Finding 1** affects every element kind of this batch: class, struct, interface, record (`record` and `record class`), record struct, enum, delegate, constructor, destructor and enum member. All of them read their first comment through the same `GetDocumentationComment`.
- **Finding 4** (partial declarations) affects class, struct, interface, record, record struct and constructor. Enums, delegates, destructors and enum members cannot be partial.
- **Finding 3** (interface members without an access modifier):
  - nested class, struct, interface, record, enum and delegate are public under C# and correctly non-private;
  - the static constructor is the only exception.

## Breaking inputs

These are for the test implementer, against the requirements as they will read once the spec is corrected.

- **FC-002 / FC-003.** Every qualifier needs a test on both sides:
  - top level versus nested (SHARED-DOC-005 `A` versus `G`);
  - before versus after the attribute list (SHARED-DOC-004 `A` versus `B`);
  - implicit private directly in a class, struct, record or record struct, versus a namespace, file level or an interface (SHARED-DOC-002);
  - `private protected` versus `private`;
  - a later comment separated by a blank line only (the D9 shape), once the spec settles what SHARED-DOC-006 covers.
- **FC-005 / FC-020.** A single documentation comment followed by each trivia kind before the declaration: `#if`/`#endif`, `#region`, `#pragma warning`, and inactive `#if NEVER` disabled text (D2–D5).
  - The spec's example covers only `#if DEBUG`/`#endif`.
  - A directive and a comment are separate cases.
- **FC-019 / FC-021.** `//` and `/* */` between the documentation comment and the declaration (D1, D6).
- **FC-023.** A verbatim identifier such as `public class @event` for every "-002" location requirement. The identifier token includes the `@`.
- **FC-024.**
  - Cover `file` types (SHARED-DOC-002's example) and static constructors of a class, struct and record (RH8018).
  - While these cases are open questions, no test may assert their current output: partial types, partial constructors, the interface static constructor, the D8 and L1–L5 shapes, and malformed documentation.
  - Leave the existing malformed-XML tests in `RH8001NonPrivateClassesMustBeDocumentedAnalyzerTests` and `RH8013NonPrivateEnumMembersMustBeDocumentedAnalyzerTests` without a SHARED-DOC tag, unless the corrected spec still specifies that case.
- **FC-017.** SHARED-DOC-008 has two sides: `None` versus `Parse`/`Diagnose`.
  - Every test class already has the `None` test.
  - The `Parse` side needs a test of its own.
  - SHARED-DOC-006 and SHARED-DOC-007 have only been observed in `Diagnose`.
- **FC-013 / FC-014.** Not applicable, for two reasons:
  - No formatter phase moves a documentation comment out of a declaration's leading trivia or reorders it against other comments or directives. FMT-DOC-001 to FMT-DOC-003 move one only within the same leading trivia.
  - These existing rules ask for prose that the formatter cannot write.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `sdd/specs/shared/element-documentation.md` | Must change: SHARED-DOC-006 (contradicted by L1/L2, `Why:` false, example has the separated shape); SHARED-DOC-004 (D8/L3–L5 written as intended; "first attribute list, modifier, or keyword"); SHARED-DOC-005 (malformed first comment belongs to open question 2; no reason given for case-insensitive matching); SHARED-DOC-002 `Why:` (static constructor) |
| `sdd/specs/analyzer/RH8017.md`, `RH8018.md` | Must change: the interface static constructor (Finding 3); RH8017's primary-constructor non-goal list |
| `sdd/specs/analyzer/RH8005.md`, `RH8011.md` | Must change: "A interface", "A enum" |
| Other RH80xx specs | Must stay, apart from what follows from the shared spec |
| `sdd/specs/analyzer/RH8107.md` | Must stay as approved; a narrowed SHARED-DOC-004 reaches RH8107-001 through its clause (question 5) |
| Open-question issue for the shared spec | Must change: add Finding 4 to the partial question, and Findings 1 and 3 as new questions |
| Analyzers RH8001–RH8019, `SplitElementDocumentationAnalyzerBase`, documentation helpers | Must stay (the lane has no production change) |
| Code fix, Fix All | Not involved (no provider) |
| Formatter (`DocumentationCommentFormattingPhase`, `BlankLinePhase`) | Not involved |
| `Reihitsu.Core` (`XmlDocumentationElementOrderingUtilities.GetTagName`) | Not involved |
| Rule pages `documentation/rules/RH8001.md`–`RH8019.md` | Not involved in this run (the spec author's follow-up candidate stands) |
| Tests | Must change: tags and focused tests per the corrected spec only |

## Simpler option

_None._ The code stays as it is; the fix is a narrower spec.

Two facts to narrow with:

- **Agreeing shapes.** The analyzer and the compiler decide these the same way: a single documentation comment (D1–D6), and documentation comments separated only by blank lines (D9, D10).
- **Disagreeing shapes.** They disagree whenever a comment or directive separates two documentation comments (D8, L1–L5).
  - D7 and L6 happen to agree.
  - In D7 and L6 the summary is in the comment nearest the declaration.

## Questions for the maintainer

Questions 1 to 4 are about behavior. Each is a case to leave out of the spec and to file in `SDD open questions: sdd/specs/shared/element-documentation.md`, which `00-lead.md` pre-approves. None of them blocks the run once the case is left out. Question 5 is about process and needs an answer before SHARED-DOC-004's first-comment statement changes.

1. **Documentation comments separated by a comment or directive.**
   - Inputs: D8 (`/// <summary>S</summary>` · `// c` · `/// <remarks>R</remarks>` · `public class C { }`) and L1 (from the table).
   - Current output: D8 is not reported, L1 is reported.
   - Reading (A), only comments the compiler processes count: both D8 and L1 are reported.
   - Reading (B), every documentation comment in the leading trivia counts: neither is reported.
   - Touches SHARED-DOC-004, SHARED-DOC-005, SHARED-DOC-006, RH8107-001, and through them RH8001–RH8019.
2. **The static constructor of an interface.**
   - Input: `/// <summary>I</summary> public interface I { static I() { } }`.
   - Reading (A), a static constructor is private wherever it is declared: RH8018 reports it.
   - Reading (B), a declaration without an access modifier in an interface is non-private: RH8017 reports it, which is the current output.
   - Touches SHARED-DOC-002, RH8017-001 and RH8018-001.
3. **Partial constructors** (extends the spec author's partial-type question).
   - Input: the `PC` pairs from Finding 4.
   - Current output: only an undocumented implementing part is reported.
   - Reading (A), a constructor is documented when either part is, as the compiler documents it: nothing is reported.
   - Reading (B), every part must carry its own documentation: both undocumented parts are reported, including the defining part, which is currently never reported.
   - Touches SHARED-DOC-005, RH8017-001 and RH8018-001.
4. **Lower priority: tag-name case.**
   - Input: `/// <Summary>Upper</Summary> public class C { }`.
   - Reading (A), case-insensitive as today and as in RH8107-001 and FMT-DOC-008: not reported.
   - Reading (B), case-sensitive like XML and the generated file: reported.
   - The lead decides whether this looks wrong enough to file. If it is intended, SHARED-DOC-005's `Why:` should give the reason.
5. **Process: RH8107-001's reference clause.**
   - Narrowing SHARED-DOC-004 for question 1, for example with a coverage sentence that leaves the D8 shape out, would narrow RH8107-001 too, through the clause the maintainer approved as behavior-neutral.
   - Option (A): RH8107-001 keeps covering the first comment in every shape, so the clause must reference only the part SHARED-DOC-004 still states, or be withdrawn.
   - Option (B): RH8107-001 narrows along with SHARED-DOC-004.
   - These produce different specified behavior for RH8107 in the D8 shape with a `<returns>` in the first comment.
