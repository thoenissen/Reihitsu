Role: spec author
Commit: 4738ef2
Verdict: spec written

No change doc: every requirement states existing behavior (lane *specs and tests for existing behavior*). No production code, tests, or fixtures were touched. The prompt carried no intended fix or verdict to ignore.

## How the statements were established

Every example in the new specs and in SHARED-DOC-014 to SHARED-DOC-016 was compiled and run against the analyzers of commit 4738ef2 (documentation mode `Diagnose`, `LanguageVersion.Latest`, `XmlFileResolver` next to the source for the `docs.xml` examples) with a scratch harness outside the repository; for every block the reported spans equal the `{|RH####:…|}` markup exactly. The negative blocks that end in "is not reported" were run too and report nothing; RH8202-004's block reports the element through its line break and gets no fix action. The fix examples of RH8201-005, RH8202-003, and RH8204-003 were applied (single fix and batch Fix All, LF document) and the result equals the after block character for character. The compiler's own CS1570–CS1574/CS1711/CS1712 warnings were printed alongside to see how the compiler binds `name` attributes. `scripts/apply-fix.sh` was run for RH8201, RH8202, RH8204 under LF and CRLF. `SpecConsistencyTests`, `SpecStructureTests`, and `RuleDocumentationConventionTests` pass; `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` fails only on the 37 new IDs below, which have no tagged test yet.

Answer to thoenissen/Reihitsu#1132 (observed): inside SHARED-DOC-012's coverage, RH8101/RH8108 and RH8102/RH8109 agree with each other and with RH8032/RH8033 — `name="class"` documents `@class`, `name="@class"` does not (compiler: CS1572), a `a` declaration is documented by `name="a"`. They differ only outside that coverage: `name=" a "`, `name="a&#x200B;b"` (declaration `ab`), and `name="a b"` are reported by RH8101 (and the type-parameter shape by RH8108) but accepted by RH8102/RH8109, and the compiler binds them without a warning. So RH8101 and RH8108 use SHARED-DOC-012; RH8102 and RH8109 use its comparison through SHARED-DOC-015; the differences are filed below as instances of #1153. The helper extraction (#1132's third question) was not done.

## Changed requirements

- SHARED-DOC-014 — A parameter, type-parameter, or return-value description without text — added (defined by reference to SHARED-DOC-010; used by RH8104, RH8106, RH8111)
- SHARED-DOC-015 — Parameter descriptions follow the order of the parameters — added (used by RH8102, RH8109)
- SHARED-DOC-016 — A parameter description names its parameter — added (used by RH8103, RH8110)
- `shared/element-documentation.md` intro paragraph: one sentence added naming what SHARED-DOC-014 to SHARED-DOC-016 define; no existing requirement, non-goal, or ID changed.
- RH8101-001 — An undocumented parameter is reported — added
- RH8101-002 — The diagnostic is on the parameter name — added
- RH8102-001 — A `<param>` element that does not follow the parameters is reported — added
- RH8102-002 — The diagnostic is on the element — added
- RH8103-001 — A `<param>` element without a name is reported — added
- RH8103-002 — The diagnostic is on the element — added
- RH8104-001 — A `<param>` element without text is reported — added
- RH8104-002 — The diagnostic is on the element — added
- RH8105-001 — A missing return value description is reported — added
- RH8105-002 — The diagnostic is on the return type — added
- RH8106-001 — A `<returns>` element without text is reported — added
- RH8106-002 — The diagnostic is on the element — added
- RH8108-001 — An undocumented type parameter is reported — added
- RH8108-002 — The diagnostic is on the type parameter name — added
- RH8109-001 — A `<typeparam>` element that does not follow the type parameters is reported — added
- RH8109-002 — The diagnostic is on the element — added
- RH8110-001 — A `<typeparam>` element without a name is reported — added
- RH8110-002 — The diagnostic is on the element — added
- RH8111-001 — A `<typeparam>` element without text is reported — added
- RH8111-002 — The diagnostic is on the element — added
- RH8201-001 — A documented member that inherits its documentation is reported — added
- RH8201-002 — A member that only resembles an inherited one is not reported — added
- RH8201-003 — An implicit implementation is not reported when its interfaces may depend on `#if` — added
- RH8201-004 — The diagnostic covers the documentation comment — added
- RH8201-005 — The fix replaces the comment with `<inheritdoc/>` — added
- RH8202-001 — A `<value>` element on a property or indexer is reported — added
- RH8202-002 — The diagnostic is on the element — added
- RH8202-003 — The fix removes the element — added (uses RH8107-006, the requirement owning `DocumentationCommentCodeFixUtilities`; nothing moved out of RH8107)
- RH8202-004 — The fix is withheld for an element without an end tag — added (uses RH8107-007)
- RH8203-001 — `<inheritdoc>` on a declaration that inherits nothing is reported — added
- RH8203-002 — The diagnostic is on the element or on the declaration's name — added
- RH8204-001 — A `<placeholder>` element is reported — added
- RH8204-002 — The diagnostic is on the element — added
- RH8204-003 — The fix replaces the element with its content — added

Files: `sdd/specs/analyzer/RH8101.md` … `RH8106.md`, `RH8108.md` … `RH8111.md`, `RH8201.md` … `RH8204.md` (new); `sdd/specs/shared/element-documentation.md`, `sdd/specs/requirement-ids.txt` (37 IDs inserted, nothing moved), `sdd/specs/README.md` (14 rows). `XmlDocumentationElementOrderingUtilities` is not claimed by any spec. Code owners are only the rules' analyzers and providers.

User documentation, derived from the specs: `documentation/rules/RH8201.md` gains an *Exceptions* bullet for RH8201-003; `documentation/rules/RH8202.md`'s description now says properties and indexers only, and that a `<value>` inside `<code>`/`<example>` is not reported (RH8202-001). The other twelve pages already match their specs.

Judgment calls the skeptic may want to break:

- RH8201-003 specifies the whole-file `#if` guard as intended (any `#if` in a file declaring the type, a partial part, or one of `AllInterfaces` suppresses an implicit implementation). It is deliberately pinned by `VerifyNoDiagnosticWhenFileOfTypeContainsUnrelatedConditionalRegion` and its neighbours; I did not judge it wrong, but it is a broad guard.
- Left out of coverage without filing, because I found no basis to call the output wrong: a `name` attribute whose value is not a C# identifier (`name="1x"` is treated as no name by RH8102/RH8103), malformed XML for the syntax-based rules, and a `<param>`/`<typeparam>` on declarations a rule does not check (properties, interfaces for RH8103/RH8104, constructors and enums for RH8110/RH8111; written into the requirements as current behavior).

## Examples for the tests

Every ID listed above under *Changed requirements*; each has an `Example:` block or points to one. Existing tests that already show the behavior (to tag rather than duplicate) are in the 14 `src/Reihitsu.Analyzer.Test/Documentation/RH81xx*/RH82xx*Tests.cs` classes; gaps I noticed with no existing test: RH8101/RH8108 explicit interface implementation and `<inheritdoc>`/include exemptions, RH8105 explicit implementation/`<inheritdoc>`/later comment/include, RH8106 explicit implementation, RH8102/RH8103 explicit implementation, RH8202 explicit implementation and method, RH8203 constructor/static constructor/destructor/enum/delegate/field and the identifier location (later comment, include), RH8204 later comment/sample/nested, SHARED-DOC-015 unnamed element keeping its position, SHARED-DOC-016 `Name=` attribute and `name=" "`.

## Open questions to file

No open-questions issue exists yet for any spec below; each heading names the issue the block belongs to.

### `SDD open questions: sdd/specs/shared/element-documentation.md` (new issue, sub-issue of #912)

**S1 — The element rules read only the first documentation comment.** Instance of thoenissen/Reihitsu#1153 by extension of its #1129 Q3 decision (RH8030 checks the summary of the documentation the compiler produces); if the lead reads #1129 Q3 as covering the summary only, this is a new question.
- Input: `/// <summary>S</summary>`, an empty line, `/// <param name="a"></param>` on `void E(int a)`; the same with `/// <param>Later</param>` (RH8103), `/// <param name="z">Later</param>` on `void G(int a, int b)` (RH8102), `/// <typeparam name="B"></typeparam>` on `void N<B>()` (RH8111), `/// <returns></returns>` on `int L()` (RH8106), `/// <value>Later</value>` on `int F { get; }` (RH8202).
- Real output: nothing reported by RH8102, RH8103, RH8104, RH8106, RH8109, RH8110, RH8111, RH8202.
- Why it looks wrong: the compiler joins both comments into one documentation entry, and RH8101/RH8105/RH8108 (via SHARED-DOC-011/012) already read the later comment, so RH8101 counts `<param name="z">` there while RH8102 never checks it.
- Touches: SHARED-DOC-014, SHARED-DOC-015, SHARED-DOC-016, RH8104-001, RH8106-001, RH8111-001, RH8202-001 (all exclude a second comment from coverage).

**S2 — A description on a declaration without parameters is not checked for a match.** New question.
- Input: `/// <param name="a">A</param>` on `public void O()`; `/// <param name="z">Z</param>` on a class without a primary constructor; `/// <typeparam name="T">T</typeparam>` on non-generic `class NonGeneric` and on non-generic `void O()`.
- Real output: RH8102 and RH8109 report nothing (compiler: CS1572 / CS1711). With one parameter more, an element after the last parameter is reported (`<param name="c">` on `M(int a, int b)`).
- Why it looks wrong: the same surplus element is reported or not depending only on whether the declaration has at least one parameter.
- Touches: SHARED-DOC-015 (coverage "declares at least one parameter"), RH8102-001, RH8109-001.

**S3 — Tag names `returns`, `value`, and `placeholder` are matched case-insensitively.** New question: P8 (#1091) decides the case of `summary`/`inheritdoc` and P8b (#1153) of `param`/`typeparam`; nothing decides these, and RH8107-001 already specifies case-insensitive `returns` for RH8107.
- Input: `/// <summary>S</summary> /// <RETURNS>Upper</RETURNS>` on `int L()`; `/// <RETURNS></RETURNS>` on `int M()`; `/// <VALUE>Upper</VALUE>` on a property; `/// Saves <PlaceHolder/> c` in a summary.
- Real output: RH8105 not reported; RH8106, RH8202, RH8204 reported.
- Why it looks wrong: the editor and documentation tools match tag names case-sensitively, so `<RETURNS>` documents nothing that the reader sees, yet RH8105 accepts it.
- Touches: RH8105-001, RH8106-001, RH8202-001, RH8204-001 (all cover lower case only), RH8107-001.

**Instances of thoenissen/Reihitsu#1153 (P8b, P8c)** — one-line comments for the lead:
- RH8101/RH8108: `<PARAM name="value">` / `<TYPEPARAM name="T">` count as documentation (not reported); RH8102/RH8103/RH8104/RH8109/RH8110/RH8111 check `<PARAM>`/`<TYPEPARAM>` elements as if lower case (e.g. RH8103 reports `<PARAM>Upper</PARAM>`). P8b: tag lower case.
- RH8101: `name=" a "` on `E(int a, int b)`, `name="a&#x200B;b"` on `B(int ab)`, `name="a b"` on `F(int a)` → `a`/`ab` reported; compiler binds them (no CS1572/CS1573). RH8108: `name=" T "` on `M<T>()` → `T` reported. P8b: name as the compiler reads it.
- RH8101: `void N(int a, __arglist)` documented with a summary → `__arglist` reported. P8b: an unnamed parameter is not reported.
- RH8104/RH8106/RH8111 (via SHARED-DOC-014 → SHARED-DOC-010): `<param name="a">&#x200B;</param>` on `H(int a)` not reported. P8c: Cf characters are no text.

**Instances of thoenissen/Reihitsu#1091 (P8)** — one-line comments for the lead:
- `<INHERITDOC/>` exempts the parameters (RH8101 `G(int value)`), type parameters (RH8108), and the return value (RH8105); RH8201 treats it as `<inheritdoc>` (override `Other()` not reported); RH8203 reports it on `void Upper()`. P8: tag case-sensitive.
- RH8201: `/// <summary>Stops</summary>`, empty line, `/// <inheritdoc/>` on `override void Stop()` → reported, and the fix adds a second `/// <inheritdoc/>`. P8: the documentation the compiler produces has `<inheritdoc>`.
- Merged partial parts: RH8101 on `partial void M(int value);` documented with `<param name="value">` plus `partial void M(int value) { }` documented without it → `value` reported on both parts; RH8201 reports the documented defining part of a partial method implementing an interface member but not the documented implementing part (pinned by `VerifyDiagnosticForDocumentedDefiningPartialInterfaceImplementation`/`VerifyNoDiagnosticForDocumentedImplementingPartialInterfaceImplementation`). Every new spec excludes partial elements in several parts from coverage.

### `SDD open questions: sdd/specs/analyzer/RH8106.md` (new issue, sub-issue of #912)

**Q1 — RH8106 and RH8107 both report a `<returns>` on a `void` member.** New question.
- Input: `/// <returns/>` on `public void H() { }`.
- Real output: RH8106 (no text) and RH8107 (must not be documented) on the same span.
- Why it looks wrong: one rule asks for text in an element the other asks to remove; RH8107's fix clears both, RH8106's advice would be undone.
- Touches: RH8106-001 (covers only non-`void` return types), RH8107-001.

**Q2 — Only the first top-level `<returns>` is checked.** New question.
- Input: `/// <returns>Value</returns>` then `/// <returns></returns>` on `int F()`.
- Real output: nothing reported (the empty second element is ignored); with the order swapped, the empty first one is reported. RH8104/RH8111 check every top-level `<param>`/`<typeparam>`.
- Why it looks wrong: the verdict on an empty `<returns>` depends on its position, and the sibling rules check every element.
- Touches: RH8106-001 (covers at most one top-level `<returns>`).

### `SDD open questions: sdd/specs/analyzer/RH8201.md` (new issue, sub-issue of #912)

**Q1 — In a CRLF document the fix rewrites neighbouring line breaks to `\n`.** Decided by principle P9 (#912) — SHARED-FIX-006 once #1138 writes it.
- Input: the RH8201-005 example (or a class `C : B` with one documented `override void Run()`), all line breaks `\r\n`.
- Real output: `/// <inheritdoc/>` ends with `\r\n`, but the line break after the preceding `{`, the one after `// Ordinary comment`, and the one after the moved `// Trailing` become `\n` (`scripts/apply-fix.sh RH8201` reports `[line-ending-drift]`, e.g. `CRLF=21, LF=1` for a single member). LF documents are unaffected.
- Touches: RH8201-005 (covers documents with `\n` only).

### `SDD open questions: sdd/specs/analyzer/RH8204.md` (new issue, sub-issue of #912)

**Q1 — The fix re-serializes the placeholder's content.** New question (SHARED-FIX-003 allows changing text inside the reported span, so no decided requirement covers it).
- Input: `/// Saves <placeholder>a &#32; <see cref='C'/> b</placeholder>`; `/// Saves the <placeholder>current` + newline + `/// customer</placeholder> now` in a document with `\r\n` line breaks.
- Real output: `/// Saves a   <see cref="C" /> b` (character reference decoded, quotes and spacing of the nested element rewritten); the line break inside the multi-line content becomes `\n` (`[line-ending-drift]`).
- Why it looks wrong: the fix is meant to remove the tags and keep the text as written; it changes user-written content and the document's line ending.
- Touches: RH8204-003 (covers text without references or elements; multi-line only with `\n`).

**Q2 — Fix All needs two applications for nested placeholders.** Instance of a decided requirement: SHARED-FIX-002 (Fix All corrects everything in one application; the class is tracked by #1143).
- Input: `/// Saves <placeholder>a <placeholder>x</placeholder> b</placeholder> and <placeholder>y</placeholder>`.
- Real output: three diagnostics; one batch Fix All leaves one `<placeholder>` diagnostic, a second application clears it.
- Touches: RH8204-002, SHARED-FIX-002.

**Q3 — A `<placeholder>` in a `///` comment that documents nothing is reported.** Decided by principle P13 (#912) (`///` that documents nothing is an ordinary comment; P6 for `/** */`).
- Input: `/// <placeholder>Inside body</placeholder>` above `var x = 1;` inside a method body; `/// <placeholder>Orphan</placeholder>` at the end of a file (both CS1587).
- Real output: RH8204 reports both.
- Touches: RH8204-001 (covers only comments that precede a declaration).

## Follow-up candidates (not filed, for the run report)

- thoenissen/Reihitsu#1132 Q3: one shared `name`-matching helper for RH8032, RH8033, RH8101, RH8108 (refactor, own PR, maintainer's call).
- No new-rule candidate under P11 was found.
