Role: spec author
Commit: e07ba1a
Verdict: spec written

Repair round on `02-skeptic.md`. I ran every finding against the analyzers and providers at e07ba1a myself; production code has not changed since 4738ef2. Analyzer-only findings went through the scratch analyzer runner described in `01-spec-author.md`. A4, A5, and B2 went through `scripts/apply-fix.sh` under LF and CRLF. All 14 findings with an input reproduce exactly as the skeptic describes them, C5 included; I ran C5 although the skeptic only read it from the code. After the edits, every example in the changed specs matches its markup again. RH8202-004's negative block is reported in prose only, as before. `SpecConsistencyTests`, `SpecStructureTests`, and `RuleDocumentationConventionTests` pass.

The maintainer's answer (`squad/decisions/2026-10-19-rh8202-uses-rh8107-fix-requirements.md`) settles C6: RH8202 keeps RH8107-006 and RH8107-007 under `Uses`.

## Changed requirements

All changes are clarified in place. No ID was added, retired, or moved; the ledger and the index are unchanged.

- RH8201-005 — The fix replaces the comment with `<inheritdoc/>` — clarified (A4).
  - The fix no longer claims to keep the trivia "as the user wrote it". It keeps the ordinary comments, directives, and attribute lists in the leading trivia.
  - Coverage narrowed. Besides the comment, the leading trivia may hold only these:
    - comments and attribute lists on their own lines at the member's indentation;
    - directives at column 0;
    - after a `/** */` comment's `*/`, at most an ordinary comment.
  - `Why:` reworded.
- RH8202-001 — A `<value>` element on a property or indexer is reported — clarified (A3, B1, B2, B5).
  - The rule now reports a top-level or nested `<value>` element outside `<code>`/`<example>`. One inside a sample neither is reported nor hides one after it (A3).
  - Coverage narrowed:
    - no explicit interface implementations (B1);
    - at most one `<value>` outside samples (B2);
    - the documentation of other declarations is not covered (B5).
  - The explicit-implementation and method cases were removed from the example. The `Weight` case now shows a sample `<value>` before a nested one.
- RH8103-001 — A `<param>` element without a name is reported — clarified (B5): the "MUST NOT be checked" sentence is now a coverage boundary, and the property and interface cases were removed from the example.
- RH8110-001 — A `<typeparam>` element without a name is reported — clarified (B5): same change; the constructor case was removed from the example.
- RH8203-001 — `<inheritdoc>` on a declaration that inherits nothing is reported — clarified (A1, A2, B3, C5).
  - One bullet now covers members that are declared `override` or implement an interface member, explicitly or implicitly, `static abstract` operators and conversion operators included (A1). The example gains an explicit `IAdd<V>.operator +`, not reported, and an `operator -`, reported.
  - The element considered is the single top-level `<inheritdoc>` of the documentation.
  - Coverage narrowed:
    - at most one top-level `<inheritdoc>` across every comment and included content (A2);
    - no field-like event that declares several events (B3);
    - constructors only when the base type has no `private` constructor and no constructor that differs only in `ref`/`in`/`out`/`params` or nullable annotations (C5).
- RH8204-001 — A `<placeholder>` element is reported — clarified (C1): the coverage now says "not a documentation comment of a declaration as SHARED-DOC-004 defines it, such as one inside a method body, after an attribute list, or between modifiers".
- RH8204-003 — The fix replaces the element with its content — clarified (A5): `>` is excluded from the covered content.
- SHARED-DOC-015 — Parameter descriptions follow the order of the parameters — clarified (B4): a `<param name="value">` on an indexer with a `set` or `init` accessor is not covered.

Not changed: `documentation/rules/RH8202.md`. It says "properties and indexers" and that a `<value>` in a sample is not reported. Both statements are still true for every covered input, and the page claims nothing about explicit implementations or methods. Compared with `main`, the page now names properties and indexers only, which narrows what it promised; the PR description should say so (skeptic B5). `documentation/rules/RH8201.md` is unchanged as well; its *Exceptions* bullet still matches RH8201-003.

## Examples for the tests

The examples of all 37 IDs from `01-spec-author.md` still apply. The ones changed in this round are RH8202-001, RH8103-001, RH8110-001, and RH8203-001 (with the explicit `static abstract` operator). Tests must stay inside the narrowed coverage. In particular:

- no test asserts the leading-trivia layout of RH8201-005 beyond the covered shape, or CRLF output for RH8201 and RH8204;
- no test covers an explicit implementation, two `<value>` elements, or a method for RH8202;
- no test covers two `<inheritdoc>` elements, a field-like event with several events, or the loose constructor matching for RH8203;
- no test covers a setter indexer `value` for RH8102.

## Open questions to file

This is the complete list and replaces the one in `01-spec-author.md`. No open-questions issue exists yet for any spec named here.

### `SDD open questions: sdd/specs/shared/element-documentation.md`

**S1 — The element rules read only the first documentation comment.** New question (skeptic C3: #1129 Q3 decides only which summary RH8030 checks).
- Input: `/// <summary>S</summary>`, an empty line, then `/// <param name="a"></param>` on `void E(int a)`. The same shape with:
  - `/// <param>Later</param>` (RH8103);
  - `/// <param name="z">Later</param>` on `void G(int a, int b)` (RH8102);
  - `/// <typeparam name="B"></typeparam>` on `void N<B>()` (RH8111);
  - `/// <returns></returns>` on `int L()` (RH8106);
  - `/// <value>Later</value>` on a property (RH8202).
- Real output: nothing is reported by RH8102, RH8103, RH8104, RH8106, RH8109, RH8110, RH8111, or RH8202.
- Why it looks wrong: the compiler joins both comments into one documentation entry, and RH8101, RH8105, and RH8108 (through SHARED-DOC-011/012) already read the later comment.
- Touches: SHARED-DOC-014, SHARED-DOC-015, SHARED-DOC-016, RH8104-001, RH8106-001, RH8111-001, RH8202-001.

**S2 — Whether a description is checked depends on whether the declaration has that kind of parameter, and on which declaration kinds the rule registers.** New question.
- Input and real output:
  - `/// <param name="a">A</param>` on `void O()`, `/// <param name="z">Z</param>` on a class without a primary constructor, and `/// <typeparam name="T">T</typeparam>` on non-generic `class NonGeneric` and on non-generic `void O()`: RH8102 and RH8109 report nothing (compiler: CS1572 / CS1711). Yet `<param name="c">` after the last parameter of `M(int a, int b)` is reported.
  - `<param>Unused</param>` is reported by RH8103 on a class without a primary constructor, but not on an interface or a property.
  - `<typeparam/>` is reported by RH8110 on a non-generic `void Run()`, but `<typeparam>Unused</typeparam>` is not reported on a constructor.
- Why it looks wrong: the same surplus or unnamed element is reported or not depending only on parameter count or on the declaration kind, and nothing gives a reason for the boundary.
- Touches: SHARED-DOC-015, SHARED-DOC-016, RH8102-001, RH8103-001, RH8109-001, RH8110-001.

**S3 — The tag names `returns`, `value`, and `placeholder` are matched case-insensitively.** New question, unless the lead reads #1037 Q5 ("tag names are case-sensitive and namespace-free", listed in #1091 under P8) as covering every tag; then it is an instance of P8 (#1091). In both readings it conflicts with the merged RH8107-001, which makes `returns` case-insensitive (skeptic C2).
- Input:
  - `/// <summary>S</summary> /// <RETURNS>Upper</RETURNS>` on `int L()`;
  - `/// <RETURNS></RETURNS>` on `int M()`;
  - `/// <VALUE>Upper</VALUE>` on a property;
  - `/// Saves <PlaceHolder/> c`.
- Real output: RH8105 does not report; RH8106, RH8202, and RH8204 do.
- Touches: RH8105-001, RH8106-001, RH8202-001, RH8204-001, RH8107-001.

**S4 — A `<param name="value">` on an indexer with a setter is reported.** Instance of #1153 (P8b, "as the compiler binds"), or new if the lead reads P8b as covering only whether a parameter is documented.
- Input: `/// <param name="index">Index</param> /// <param name="value">Value</param>` on `public int this[int index] { get => 0; set { } }`.
- Real output: RH8102 reports the `value` element; the compiler gives no CS1572, because it binds the element to the setter's `value`. On a get-only indexer the compiler gives CS1572, and RH8102 reports the element too.
- Touches: SHARED-DOC-015, RH8102-001.

**Instances of thoenissen/Reihitsu#1153 (P8b, P8c)** — one-line comments:
- RH8101/RH8108 count `<PARAM name="value">` and `<TYPEPARAM name="T">`, and RH8102, RH8104, RH8109, and RH8111 check them, contrary to P8b's "tag lower case". RH8103 does not report `<PARAM name="a">`, because its `name` is still parsed.
- RH8101 reports `a`/`ab` for `name=" a "` on `E(int a, int b)`, `name="a&#x200B;b"` on `B(int ab)`, and `name="a b"` on `F(int a)`; RH8108 reports `T` for `name=" T "` on `M<T>()`. The compiler binds all of them without a warning. P8b: the name is read as the compiler reads it.
- RH8101 reports `__arglist` in a documented `void N(int a, __arglist)`. P8b: an unnamed parameter is not reported.
- RH8104, RH8106, and RH8111 do not report `<param name="a">&#x200B;</param>` on `H(int a)`. P8c: Cf characters are not text.
- Low priority (skeptic C7): `name="1x"` (RH8103) and `name="1T"` (RH8110) are reported as having no name. These inputs are excluded from coverage.
- Note for the comment (skeptic C4): SHARED-DOC-014 defines the text of `<param>`, `<typeparam>`, and `<returns>` by reference to SHARED-DOC-010, so when #1153 rewrites SHARED-DOC-010 for P8c, the change reaches those elements too. The change doc should decide that explicitly.

**Instances of thoenissen/Reihitsu#1091 (P8)** — one-line comments:
- `<INHERITDOC/>`:
  - exempts the parameters (RH8101 `G(int value)`), the type parameters (RH8108), and the return value (RH8105);
  - counts as `<inheritdoc>` for RH8201 (override `Other()` is not reported);
  - is reported by RH8203 on `void Upper()`.
- RH8201: `/// <summary>Stops</summary>`, an empty line, then `/// <inheritdoc/>` on `override void Stop()` → reported, and the fix adds a second `/// <inheritdoc/>`.
- Merged partial parts:
  - RH8101 on `partial void M(int value);` documented with `<param name="value">` and `partial void M(int value) { }` documented without it → `value` is reported on both parts;
  - RH8201 reports the documented defining part of a partial method that implements an interface member, but not the documented implementing part.

### `SDD open questions: sdd/specs/analyzer/RH8106.md`

**Q1 — RH8106 and RH8107 both report a `<returns>` on a `void` member.** New question.
- Input: `/// <returns/>` on `public void H() { }`.
- Real output: RH8106 (no text) and RH8107 (must not be documented) report the same span.
- Touches: RH8106-001, RH8107-001.

**Q2 — Only the first top-level `<returns>` is checked.** New question.
- Input: `/// <returns>Value</returns>` followed by `/// <returns></returns>` on `int F()`.
- Real output: nothing is reported. With the order swapped, the empty element is reported. RH8104 and RH8111 check every top-level element.
- Touches: RH8106-001.

### `SDD open questions: sdd/specs/analyzer/RH8201.md`

**Q1 — The fix re-lays out the member's leading trivia.** Decided by principle P9 (#912); SHARED-FIX-006 once #1138 writes it. This merges my CRLF finding with skeptic A4.
- Input and real output, LF (`scripts/apply-fix.sh RH8201`):
  - `    #if DEBUG` / `    #endif` before `/// <summary>Runs here</summary>` → both directives move to column 0;
  - `      // Odd   indentation` and `      [System.Obsolete]` after the comment → both are re-indented to 4 spaces;
  - `/** <summary>Stops here</summary> */ public override void Stop()` → `/// <inheritdoc/>`, then the member on a new line;
  - an indented `#pragma warning disable` between comment and member → column 0 (skeptic).
- Input and real output, CRLF: in the RH8201-005 example, the line breaks after the preceding `{`, after `// Ordinary comment`, and after the moved `// Trailing` become `\n` (`[line-ending-drift]`).
- Touches: RH8201-005.

### `SDD open questions: sdd/specs/analyzer/RH8202.md`

**Q1 — RH8202 does not report an explicit interface implementation.** New question, or an instance of #1153 (#1129 Q1: SHARED-DOC-009 exempts only missing documentation) if the lead reads that decision broadly.
- Input: `/// <summary>Count</summary> /// <value>Count</value>` on `int INotifier.Count => 0;`.
- Real output: RH8202 reports nothing, while RH8201 reports the comment. RH8102–RH8104, RH8106, and RH8109–RH8111 all check explicit implementations, and no test pins the exemption.
- Touches: RH8202-001.

**Q2 — Two `<value>` elements need two fix applications.** Decided by principle P7 (#912), SHARED-FIX-001.
- Input: `/// <summary>S <value>First</value></summary> /// <remarks><value>Second</value></remarks>` on `int P { get; }`, or two top-level `<value>` elements.
- Real output: one diagnostic at a time; `scripts/apply-fix.sh RH8202` → `fixed (1 action(s), 2 iteration(s))` under LF and CRLF.
- Touches: RH8202-001, RH8202-003.
- Follow-up candidate, not filed: RH8107 picks its element the same way (`GetFirstDocumentingTag`).

**Q3 — A `<value>` element on a method is not checked.** New question (skeptic B5).
- Input: `/// <summary>Runs</summary> /// <value>Not a property</value>` on `public void Run() { }`.
- Real output: nothing is reported; only properties and indexers are registered.
- Why it looks wrong: the rule page on `main` said the rule forbids `<value>` in XML documentation, and a `<value>` on a method describes nothing the method has. No `Why:` gives a reason for the boundary.
- Touches: RH8202-001.

### `SDD open questions: sdd/specs/analyzer/RH8203.md`

**Q1 — With several events in one declaration, the verdict depends on their order.** New question.
- Input: `/// <inheritdoc/>` on `public event EventHandler First, Fourth;`, where only `First` implements `INotifier.First`.
- Real output: not reported; with the order `Fifth, Second`, it is reported. Only the first event decides. RH8201-001 instead requires every event to inherit.
- Touches: RH8203-001.

**Q2 — Constructor inheritance ignores modifiers, nullability, and accessibility.** New question (skeptic C5, reproduced).
- Input: base `B` with only `private B(ref int a)` and `protected B(string a)`; derived `/// <inheritdoc/> public D(int a)` and `/// <inheritdoc/> public D(string? a)`.
- Real output: neither is reported. The parameter types match, so the analyzer treats a `private` constructor with a `ref` parameter, and a constructor that differs only in nullable annotations, as documentation to inherit.
- Touches: RH8203-001.

### `SDD open questions: sdd/specs/analyzer/RH8204.md`

**Q1 — The fix re-serializes the placeholder's content.** New question.
- Input and real output:
  - `/// Saves <placeholder>a &#32; <see cref='C'/> b</placeholder>` → `/// Saves a   <see cref="C" /> b`;
  - `/// Runs when <placeholder>a > b</placeholder> holds` → `/// Runs when a &gt; b holds`, under LF and CRLF (skeptic A5);
  - multi-line content in a CRLF document → its line break becomes `\n` (`[line-ending-drift]`).
- Touches: RH8204-003.

**Q2 — Fix All needs two applications for nested placeholders.** Instance of a decided requirement: SHARED-FIX-002 (tracked by #1143).
- Input: `/// Saves <placeholder>a <placeholder>x</placeholder> b</placeholder> and <placeholder>y</placeholder>`.
- Real output: three diagnostics; one batch Fix All leaves one, and a second application clears it.
- Touches: RH8204-002, SHARED-FIX-002.

**Q3 — A `<placeholder>` in a comment that documents nothing is reported.** Decided by principle P13 (#912), and P6 for `/** */`.
- Input:
  - `/// <placeholder>Inside body</placeholder>` above `var x = 1;` in a method body;
  - `/// <placeholder>Orphan</placeholder>` at the end of a file;
  - `[Obsolete]` followed by `/// <summary>Runs <placeholder>soon</placeholder></summary>` (skeptic C1);
  - `public` / `/// <placeholder>x</placeholder>` / `void Stop()` (skeptic C1).
- Real output: the compiler gives CS1587 on all four, and RH8204 reports all four.
- Touches: RH8204-001.

## Follow-up candidates (not filed, for the run report)

- #1132 Q3: one shared `name`-matching helper.
- RH8107 shares RH8202's one-element-per-declaration shape (RH8202 Q2).
- Moving RH8107-006/007 to `shared/`: the maintainer allows proposing it later.
- No new-rule candidate under P11.
