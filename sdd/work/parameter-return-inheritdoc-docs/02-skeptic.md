Role: skeptic
Commit: 5964fee
Verdict: proceed narrower

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The spec author's list of "judgment calls" was treated as claims to test, not as a verdict.

## Premise

Lane: specs and tests for existing behavior. There is no bug premise to check, so the question is whether the spec diff (`git diff origin/main -- sdd/specs documentation`) states what the code at 5964fee does, whether it presents cases that look wrong as intended, and which inputs the code decides that the specs do not mention. Production code has not changed since `origin/main` 78af33e.

How I checked: I read the reporting code of all 14 analyzers, `DocumentationAnalysisUtilities`, `DirectDocumentationSyntaxChecker`, `XmlDocumentationExpander`, `InterfaceImplementationUtilities`, and the RH8201/RH8202/RH8204 providers. For analyzer-only rules no script exists (`scripts/apply-fix.sh` refuses an ID that has no provider), so I ran fixtures through the spec author's existing scratch analyzer runner, copied unchanged: documentation mode `Diagnose`, `LanguageVersion.Latest`, an `XmlFileResolver` next to the fixture, and the compiler's CS15xx/CS17xx warnings printed alongside. The fixes went through `scripts/apply-fix.sh <ID> <dir>` under LF and CRLF. All fixtures were in the session scratchpad and have been deleted.

Most of the diff holds. Inputs I re-ran that match the specs:

- `name="@event"` on `int @event`: RH8102 reports the element and RH8101 reports `@event`.
- `name="class"` on `int @class`: RH8101, RH8102, and RH8103 report nothing.
- `<PARAM name="a">Upper</PARAM>`: RH8103 does not report it, because the parser still reads its `name` attribute.
- RH8203:
  - `<inheritdoc/>` on a destructor and on a static constructor is reported.
  - `<inheritdoc/>` on a struct's parameterless constructor is not reported.
  - In the later-comment case, the diagnostic lands on the field's first variable (`a`), the event's first variable (`E1`), the operator token (`+`), and the `operator` keyword of a conversion.
- RH8105 locates its diagnostic on `ref readonly int`.
- The RH8204 fix keeps `a < b` (fallback path) and `"quoted"<tab>tab` unchanged.

The findings below need a change to the specs before tests are tagged.

### A. Spec statements the code contradicts (the spec changes; nothing to ask)

**A1 — RH8203-001's list of inheriting declarations is missing explicit operator implementations.**
- Input: `class V : IAdd<V>` with `/// <inheritdoc/>` on `static V IAdd<V>.operator +(V left, V right)` and on `static explicit IAdd<V>.operator int(V value)`, both `static abstract` members of `IAdd<T>`.
- Real output: neither is reported.
- The spec lists explicit implementations only for "method, property, indexer, or event", and operators only for "overrides … or implicitly implements". Read that way, both declarations inherit nothing and must be reported.
- In the code, `CanInheritDocumentation` reaches `OverridesOrImplementsMember`, whose `ExplicitInterfaceImplementations` check catches both.
- Nothing about this looks wrong: the spec text just has to include explicitly implemented operators and conversion operators.

**A2 — RH8203-001's "the element considered" is wrong when the documentation has two top-level `<inheritdoc>` elements.**
- Input: on `public void Stop()` in a class without a base list, `/// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>`, where the included content is `<inheritdoc cref="Run"/>`, followed by `/// <inheritdoc/>`.
- Real output: not reported. The spec says the considered element is the direct `<inheritdoc/>`, which has no `cref`, so it should be reported.
- The analyzer also returns when the first `<inheritdoc>` of the expanded documentation has a `cref`.
- This only happens with at least two top-level `<inheritdoc>` elements in the produced documentation. Either state it or exclude that shape from coverage.

**A3 — RH8202-001 can be read two ways, with different output.**
- The sentence: "otherwise the first `<value>` element nested inside another element, unless it lies inside a `<code>` or an `<example>` element".
- Input: on a property, `/// <example><code><value>Sample</value></code></example>` followed by `/// <remarks><value>Nested</value></remarks>`.
- Real output: `<value>Nested</value>` is reported, because `GetFirstDocumentingTag` takes the first nested element that is not inside a sample.
- The spec has to say that reading. This is not a maintainer question, because the code decides.

**A4 — RH8201-005 says the fix "MUST keep everything else in the member's leading trivia" and "stays as the user wrote it". In a document with `\n` line breaks, which the requirement covers, the fix re-lays out the leading trivia instead.** Observed with `scripts/apply-fix.sh RH8201`:
- Directives before the comment: `    #if DEBUG` / `    #endif` followed by `/// <summary>…` → both directives move to column 0.
- A directive between the comment and the member: `    #pragma warning disable CS0618` → column 0. The same happens to `    #pragma warning restore CS0618` in the next member's leading trivia once that member is fixed.
- A comment after the comment: `      // Odd   indentation` → indented 4 spaces, with its interior spaces kept.
- An attribute list after the comment: `      [System.Obsolete]` → indented 4 spaces.
- A block comment followed by indented directives: `/** <summary>Stops here</summary> */` followed by indented `#if DEBUG` / `#endif` → the directives move to column 0.
- The member on the line of the closing `*/`: `/** <summary>Stops here</summary> */ public override void Stop()` → `/// <inheritdoc/>`, then the member on its own line, indented like the member. The requirement does not describe this case.
- Code below the leading trivia is not touched: `public   override int Stop(int a,int b)` and `return a+b;` stay as written.
- The existing test `VerifyDirectiveIsPreservedWhenMultiLineDocumentationCommentIsReplaced` only uses a directive that is already at column 0.
- This is FC-030 (a fix formats more than it fixes), and I read it as an instance of P9 (#912), the fix-scope principle that SHARED-FIX-006 (#1138) will state.
- RH8201-005's coverage and its `Why:` have to be narrowed to leading trivia that this layout leaves unchanged. The input belongs in the `analyzer/RH8201.md` open-questions issue next to Q1, marked P9.

**A5 — RH8204-003's coverage admits `>`, and the fix escapes it.**
- Input: `/// Runs when <placeholder>a > b</placeholder> holds`.
- Real output: `/// Runs when a &gt; b holds`, under LF and CRLF.
- The covered set ("text without character references, entity references, and XML elements") includes this text, but the fix does not replace the element with its content here.
- Exclude `>` from coverage, and add the input to RH8204 Q1, which has the same re-serialization mechanism.

### B. Cases that look wrong but are written as intended (leave them out and file them, per `sdd/README.md`, *Open questions*)

**B1 — RH8202 skips explicit interface implementations.**
- RH8202-001 says it "MUST NOT report a declaration that explicitly implements an interface member", and no `Why:` justifies that.
- Input: `/// <summary>Count</summary> /// <value>Count</value>` on `int INotifier.Count => 0;`.
- Real output: RH8202 reports nothing, because of the `NeedsDocumentation` guard. RH8201 reports the comment.
- Every other content rule in the batch checks explicit implementations: RH8102–RH8104, RH8106, RH8109–RH8111. RH8106-001's `Why:` gives the reason that applies equally here ("a `<returns>` element that it does carry is shown to the reader").
- The maintainer's #1129 Q1 decision (recorded in #1153) is that SHARED-DOC-009 exempts only missing documentation, and that an empty summary on an explicit implementation is reported.
- No test pins the current behavior.
- Leave the sentence out of RH8202-001 and file it in the `analyzer/RH8202.md` issue, either as a new question or as an instance of #1129 Q1 if the lead reads that decision broadly.

**B2 — RH8202's one-element-per-declaration reporting needs two fix applications.** RH8202-001 says "the first such element MUST be reported".
- Input: a property with two top-level `<value>` elements, or with `<value>First</value>` inside `<summary>` and `<value>Second</value>` inside `<remarks>`.
- Real output from `scripts/apply-fix.sh RH8202`: `fixed (1 action(s), 2 iteration(s))` under LF and CRLF.
- This looks like an instance of P7 (#912) / SHARED-FIX-001: "everything the rule reports at the place it checks" has to be cleared in one application.
- Exclude more than one `<value>` outside samples from RH8202-001's coverage, and file it marked P7.
- Follow-up candidate, out of scope: RH8107 has the same shape (`GetFirstDocumentingTag`).

**B3 — RH8203's verdict on a field-like event with several declarators depends on their order.**
- Input: `/// <inheritdoc/>` on `public event EventHandler First, Fourth;`, where only `First` implements `INotifier.First`.
- Real output: not reported. With `Fourth, First`, it is reported on the element.
- The code decides by the first declarator only (`GetDeclaredSymbol` falls back to `Variables[0]`). RH8201-001 requires each declarator to inherit. RH8203-001 says nothing about it.
- Cover only one declarator, and file it in the `analyzer/RH8203.md` issue as a new question.

**B4 — RH8102 (SHARED-DOC-015) reports `<param name="value">` on an indexer with a setter, which the compiler binds.**
- Input: `/// <param name="index">Index</param> /// <param name="value">Value</param>` on `public int this[int index] { get => 0; set { } }`.
- Real output: RH8102 reports the `value` element. The compiler gives no CS1572.
- On a get-only indexer the compiler gives CS1572, and RH8102 reports the element too.
- SHARED-DOC-015 covers this input as "an element whose position is after the last parameter".
- Exclude a `value` element on an indexer with a `set` or `init` accessor, and file it in the shared spec's issue. It may be an instance of #1153 (P8b, "as the compiler binds"); that is the lead's call.

**B5 — "MUST NOT be checked" sentences pin an arbitrary boundary that comes from the registered syntax kinds, and none has a `Why:`.**
- RH8103-001 excludes interfaces, properties, and fields. RH8110-001 excludes constructors, properties, and enums. RH8202-001 excludes methods.
- `<typeparam/>` on a non-generic `void Run()` is reported, but `<typeparam>Unused</typeparam>` on a constructor is not.
- `<param>Unused</param>` on a class without a primary constructor is reported, but the same element on an interface or a property is not.
- This is the same family as S2 (a description on a declaration without that kind of parameter). Turn the sentences into coverage boundaries without a MUST NOT, and add these inputs to S2 in the shared spec's issue.
- For RH8202: before this run, the rule page said the rule "forbids the `<value>` tag in XML documentation". The run narrowed the page to properties and indexers to match the code. That is a narrowing of the user-facing contract, and the PR description should say so.

### C. Precision and filing notes

- **C1 — Two more inputs for RH8204 Q3 (P13).** A `///` comment after an attribute list (`[Obsolete]` then `/// <summary>Runs <placeholder>soon</placeholder></summary>`) and one between modifiers (`public` / `/// <placeholder>…</placeholder>` / `void Stop()`). The compiler gives CS1587 on both, and RH8204 reports both. The coverage sentence "does not precede a declaration" is vague for these. "Is not a documentation comment of a declaration as SHARED-DOC-004 defines it" would be exact.
- **C2 — S3 may already be decided.** #1091 lists #1037 Q5 ("tag names are case-sensitive and namespace-free") under P8. If the maintainer meant tag names in general, S3 is an instance of P8. Either way it conflicts with the merged RH8107-001 (case-insensitive `returns`). Lead's call.
- **C3 — S1 is a new question.** #1129 Q3 decides only which summary RH8030 checks. The spec author raised the same doubt.
- **C4 — SHARED-DOC-014 inherits future changes to SHARED-DOC-010.** It is defined by reference to SHARED-DOC-010, so when #1153 (P8c) rewrites SHARED-DOC-010, the change reaches `<param>`, `<typeparam>`, and `<returns>` too, although P8c was decided for summaries. The #1153 comment should say this, so the change doc decides it explicitly.
- **C5 — RH8203's "same parameter types in the same order" is looser than it reads.** `HasMatchingBaseConstructor` compares only parameter types with `SymbolEqualityComparer.Default`, so it ignores `ref`/`in`/`out`, `params`, and nullability, and a `private` base constructor counts. This is from reading the code, not from a run. State it precisely or exclude it.
- **C6 — RH8202 → RH8107-006/007 is the first cross-rule `Uses` reference.** No analyzer spec on `main` lists another rule's requirement under `Uses`, and `sdd/README.md` says behavior that more than one spec relies on is specified once under `shared/`. Moving the requirement out of RH8107 needs escalation (`00-lead.md`). Whether referencing it instead needs the maintainer's approval is the lead's call.
- **C7 — Non-identifier `name` values.** `name="1x"` (RH8103) and `name="1T"` (RH8110) are reported as having no name. They were left out of coverage without filing. Under P8b ("name read as the compiler reads it") they are a low-priority candidate for the #1153 comment.

## Mechanism and candidates

_N/A_ — no bug premise. The dispatch sets that define which inputs each rule decides, taken from the registrations:

| Kind list | Rules |
|---|---|
| `ParameterOwnerKinds` (method, constructor, delegate, indexer, operator, conversion operator, class, struct, record, record struct) | RH8101–RH8104 |
| `TypeParameterOwnerKinds` (class, struct, interface, record, record struct, delegate, method) | RH8108–RH8111 |
| `ReturnValueOwnerKinds` (method, delegate, operator, conversion operator) | RH8105, RH8106 |
| `DocumentableDeclarationKinds` | RH8203 |
| Seven member kinds (method, property, event, field-like event, indexer, operator, conversion operator) | RH8201 |
| Property and indexer | RH8202 |
| `XmlElement` and `XmlEmptyElement` anywhere in structured trivia | RH8204 |

## Breaking inputs

- **FC-024** (explicit implementations, static interface members):
  - RH8203: explicit operator and conversion operator implementations (A1).
  - The explicit-implementation exemptions of RH8101, RH8105, and RH8108, which have no test yet (spec author's gap list).
  - Explicit implementations are checked by RH8102–RH8104, RH8106, and RH8109–RH8111.
- **FC-019, FC-020, FC-030**: RH8201-005 tests only what stays covered after narrowing (A4). No test may assert that indented directives or comments next to the comment are kept as written.
- **FC-017, FC-033**: RH8201-005 and RH8204-003 are covered for `\n` only. No test asserts CRLF output for them.
- **FC-007, FC-008**: RH8202 with two `<value>` elements (B2) and RH8204 nested placeholders under Fix All (RH8204 Q2) stay out of the tests, because they are filed.
- **FC-016**: the shared requirements need a covering test in both sibling rules: SHARED-DOC-015 for RH8102 and RH8109, SHARED-DOC-016 for RH8103 and RH8110, and SHARED-DOC-014 for RH8104, RH8106, and RH8111.
- **FC-023**: `@class`, `@event`, and `a` in the SHARED-DOC-012 and SHARED-DOC-015 examples.
- **FC-027**: multi-line `<param>`, `<typeparam>`, and `<returns>` elements, and `/** */` comments, for RH8104, RH8106, and RH8111.
- **Characterization tests for claims no test covers yet**:
  - RH8203 on a destructor, a static constructor, a struct's parameterless constructor, an enum, a delegate, and a field.
  - The RH8203-002 name locations (field, field-like event, operator, conversion operator, indexer).
  - RH8105 `ref readonly`.
- **FC-013 and FC-014 do not apply.** These are existing rules about documentation content, and no formatter phase owns or emits that content.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH8101–RH8111, RH8201–RH8204 analyzers | must stay (lane) |
| RH8201, RH8202, RH8204 code fixes, including Fix All | must stay (lane) |
| `analyzer/RH8201.md`, `RH8202.md`, `RH8203.md`, `RH8204.md` | must change (A1–A5, B1–B3, B5, C1) |
| `analyzer/RH8103.md`, `RH8110.md` | must change (B5) |
| `shared/element-documentation.md` (SHARED-DOC-015) | must change (B4 coverage) |
| `documentation/rules/RH8201.md` | must stay; the *Exceptions* bullet matches RH8201-003 and the code |
| `documentation/rules/RH8202.md` | must change only if B1 or B5 changes what the page may claim; otherwise the narrowing is named in the PR |
| Other twelve rule pages | must stay |
| Tests (`src/Reihitsu.Analyzer.Test/Documentation/RH81xx`/`RH82xx`) | must change (tags and characterization tests) |
| `Reihitsu.Formatter`, `Reihitsu.Core`, CLI | not involved |

## Simpler option

_None._ Narrowing coverage and filing the cases is already the smallest change. No requirement needs a new ID or a moved one.

## Questions for the maintainer

_None_ block this run. B1–B5 are open questions that the squad files per `sdd/README.md` (*Open questions*). A2 and A3 are decided by the code. C2, C3, and C6 are classification and structure calls for the lead.
