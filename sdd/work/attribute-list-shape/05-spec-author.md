Role: spec author
Commit: d0efd8e
Verdict: spec written

Second repair round, on the four repairs `04-skeptic.md` routes to the spec author. I re-derived each one rather than taking over its suggestion. The spec changes are in commit fd80400. This handover is the final, self-contained source for the open-question texts and the comment on thoenissen/Reihitsu#1093. It replaces the *Open questions to file* and *Instances* sections of `01-spec-author.md` and `03-spec-author.md`.

My prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the skeptic's suggestions as suggestions only.

## How the statements were established

- **Inputs outside the old scope.** 25 inputs went through `scripts/apply-fix.sh` (RH5514, RH5506, RH5528), LF and CRLF. They are every row of Q1 below, including the four new ones. Each output in Q1 is the one I observed.
- **Inputs inside the new scope.** 24 more went through RH5514, RH5506, RH5502, RH5518, and RH5531, LF and CRLF. They change nothing outside the reported list and do not drift:
  - a multi-line `/* */` whose continuation line is at another column, before and after the list;
  - a `/** */` doc comment;
  - a `///` comment between the list and the declaration;
  - a comment line at the list's column, and an empty line, between the list and the declaration;
  - comment and empty lines mixed on both sides;
  - `// c   ` with trailing spaces inside the comment;
  - two comments on one line;
  - trailing comments on the list's line: spaced, unspaced, two, and multi-line;
  - a previous member whose closing brace is at the wrong column;
  - a K&R lambda block;
  - a top-level class with `///` lines at column 0;
  - assembly lists with comment and blank lines;
  - a field list;
  - a multi-line accessor list.
- **The list of SHARED-ATTR-014.** For RH5502, RH5504, RH5516, RH5518, and RH5520, `int P { [<specifier>: A, B] get; set; }` is not reported. `int Q { [<specifier>: A, B] get => 1; }` and the same list in a multi-line accessor list are reported.
- **Fix All passes.** I ran Fix All (the provider's `GetFixAllProvider`, document scope) on RH5506 fixtures and counted the passes until no diagnostic was left.
- **Spec examples.** I rebuilt the scratch example verifier. It runs every C# block through the shipped analyzers and code fixes, and checks reported spans, before/after pairs, "is not reported", and "no code fix is offered". It also lists CS0657/CS0658 placements. I ran it over `shared/attribute-targets.md` and the 16 analyzer specs: 128 checks, 0 failures under LF, and the same under CRLF. Two deliberately mutated copies of RH5518 failed, as they should.
- **Architecture tests.** `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RuleDocumentation"`: 26 passed.

All fixtures and scripts were in the scratchpad.

## Changed requirements

- SHARED-ATTR-017 — The split fix gives each attribute a list on its own line — clarified. The normative sentences are unchanged and only the scope paragraph changed. Two inputs of the skeptic were inside the old scope and are now outside it: a `///` continuation line at another column, and a comment or whitespace-only line between the list and the declaration. The old wording also left open what may lie between the list and the first token after it. The scope now has five conditions:
  1. **The list's line and the next token.** The list is the first token on its line. After its closing bracket the line holds nothing or only comments. The first token after the list starts a later line.
  2. **The lines around the list.** This covers every other line from the code before the list (or the start of the file) to the first token after it. Each such line is empty, without even a space, or holds only comments with its first non-space character at the list's column. Each `///` line counts on its own. The continuation lines of `/* */` and `/** */` may be at any column.
  3. **Directives.** No directive or disabled text lies in that range.
  4. **Trailing whitespace.** No line from the code before the list to the line before the next token ends in a space or tab that is outside a comment.
  5. **Indentation.** The list, the first token after it, and the line of every enclosing opening brace are indented with spaces only, at four per scope. This replaces "the line after it", which could mean either a comment line or the declaration's line.

  Every example of SHARED-ATTR-017, of RH55xx-003, of RH5514-004, and of RH5531-005 is inside the new scope.
- RH5518-001 — Field attribute lists hold one attribute — example changed. `int Q { [field: A, B] get; set; }` and `int S { {|RH5518:[field: A, B]|} get => 1; }` were removed. Both put a `field:` list on a `get` accessor, which the compiler ignores (CS0657). I kept the normative sentence and its reference to SHARED-ATTR-014. Without the reference, the sentence would claim that `int P { [field: A, B] get; set; }` is reported, which is false.

  The same holds for RH5502-001, RH5504-001, RH5516-001, and RH5520-001. None of them shows an accessor example, so they needed no change. No example in the 16 specs now puts a list on a placement the compiler ignores, except deliberate ones:
  - `type:` on a non-type declaration (SHARED-ATTR-001, SHARED-ATTR-015, RH5531);
  - `foo:` (SHARED-ATTR-002);
  - `[property: B]` on a type parameter (RH5529-001; see *Note for the lead*).

No requirement was added or retired. The ledger, `Uses` rows, and code owners are unchanged.

## Examples for the tests

The same as in `01-spec-author.md`. Two points for the test implementer:

- RH5518-001's example lost its two accessor lines. A test of RH5518-001 must not use a `field:` list on an accessor.
- Each split-fix test of SHARED-ATTR-017 must stay inside the five scope conditions above.

## Open questions to file

Each block below can be posted on GitHub as it is. Nothing in it refers to a handover.

### Q1 — What the split fix of the attribute list-shape rules changes outside the reported list

**Target issue:** `SDD open questions: sdd/specs/shared/attribute-targets.md`. thoenissen/Reihitsu#1030 is that issue and is closed. Reopen it or open a new one, as the lead decides.

> **Touches:** SHARED-ATTR-017 and the split-fix requirements that reference it: RH5502-003, RH5504-003, RH5506-003, RH5508-003, RH5510-003, RH5512-003, RH5514-004, RH5516-003, RH5518-003, RH5520-003, RH5522-003, RH5526-003, RH5528-003, RH5531-005.
>
> SHARED-ATTR-017 specifies only lists inside a narrow scope, which every condition below must meet:
>
> - the list starts its line, and nothing but comments follows it there;
> - every line between the code before the list and the next token is empty or a comment line at the list's column;
> - there is no directive or disabled text;
> - no line ends in whitespace outside a comment;
> - indentation is spaces only, four per scope.
>
> Outside that scope, the fix changes text it does not report on. All inputs below were run through `scripts/apply-fix.sh`. Every fixture also declares `class A : System.Attribute { } class B : System.Attribute { } class D2 : System.Attribute { }`. The last column says what happens when the same input is saved with CRLF line endings. "LF drift" means the fix writes LF line breaks into the CRLF document.
>
> A list on its own line (RH5514, inside `class C` unless stated):
>
> | Input | Output | CRLF |
> |---|---|---|
> | `namespace N` / `{` / `class C` / `{` at column 0 / `    [A, B]` / `    void M()` / `    {` / `    }` | `[A]`, `[B]`, and `void M()` move to column 8; the class braces stay at 0 and the method's braces stay at 4 | LF drift |
> | the same with the class at column 2 and the list at 6 | `[A]`, `[B]`, and `void M()` move to column 8 | LF drift |
> | `#pragma warning disable CS0001` at column 0 above `    [A, B]` | nothing outside the list changes | LF drift |
> | `    #pragma warning disable CS0001` above `    [A, B]` | the `#pragma` line moves to column 0 | LF drift |
> | `#region R` at column 0 above `    [A, B]` | the `#region` line moves to column 4 | LF drift |
> | `// c` at column 0 above `    [A, B]` | the comment moves to column 4 | LF drift |
> | `        /// <summary>M</summary>` above `    [A, B]` | the doc comment moves to column 4 | LF drift |
> | `    /// <summary>` / `      /// M` / `    /// </summary>` above `    [A, B]` | the `/// M` line moves to column 4 | LF drift |
> | RH5506, top level: `/// <summary>` / `    /// C` / `/// </summary>` / `[A, B]` / `class C` | the `/// C` line moves to column 0 | LF drift |
> | `    int _f;   ` (trailing spaces), a blank line, `    [A, B]` | the trailing spaces are removed | LF drift |
> | `    int _f;`, a line of three spaces, `    [A, B]` | the whitespace-only line is emptied | LF drift |
> | `    /* c */   ` (trailing spaces) above `    [A, B]` | the trailing spaces are removed | LF drift |
> | `    [A, B]   ` (trailing spaces) / `    void M()` | the trailing spaces are removed | LF drift |
> | `    [A, B]` / `// c` at column 0 / `    void M()` | the comment moves to column 4 | LF drift |
> | `    [A, B]` / a line of three spaces / `    void M()` | the whitespace-only line is emptied | LF drift |
> | `        [A, B]` / `        void M()` (one level too deep) | `[A]`, `[B]`, and `void M()` move to column 4; the method's braces stay at 8 | LF drift |
> | `\t[A, B]` / `\tvoid M()` (tabs) | both lines are rewritten with 4 spaces; the method's braces keep their tabs | LF drift |
>
> A list that shares its line:
>
> | Input | Output | CRLF |
> |---|---|---|
> | RH5506: `[A, B] class C` | `[A]` / `[B]` / `class C`; the declaration is moved onto its own line | LF drift |
> | RH5514: `[A, B] void L()`, a local function in a `case 1:` section or in a method block | `[A]` / `[B] void L()`; the declaration is not moved | no drift |
> | RH5514: `[A] [B, D2] void M()` | `[A]` / `[B]` / `[D2]` / `void M()`; the list in front of the reported one is broken off too | LF drift |
> | RH5514: `[A, B] [D2] void N()` | `[A]` / `[B]` / `[D2] void N()` | LF drift |
> | RH5514: `[return: A] [A, B] int R() => 0;` | `[return: A]` / `[A]` / `[B]` / `int R() => 0;`; the `return:` list, which RH5527 and RH5528 check, is moved too | LF drift |
> | RH5514: `System.Action a = [A, B] () => { };` in a method | `System.Action a = [A]` / `        [B] () => { };` | LF drift |
> | RH5528: `int Q { [return: A, B] get => 1; }` | `int Q {` / `        [return: A]` / `        [return: B]` / `        get => 1; }` | LF drift |
> | RH5506: `class O { [A, B] class C { } }` in a namespace | `class O {` / `        [A]` / `        [B]` / `        class C { } }` | LF drift |
> | RH5514: `/* c */ [A, B]` / `void N()` | `/* c */` / `[A]` / `[B]` / `void N()`; the comment is separated from the list | LF drift |
>
> **Why it looks wrong:** the fix changes text it does not report on: indentation, trailing whitespace, comment and directive lines, neighbouring lists, and the line breaks of the declaration. What changes depends on the context, not on the rule: a type declaration is moved onto its own line, but a local function is not. Line breaks the fix adds outside the reported list are LF whatever the document uses. The break between the new lists does use the document's line ending, which is why the local-function row does not drift.
>
> Facts for the decision:
>
> - The provider itself rewrites only the reported list, and inserts only the line ending it detected. Everything else changes in the code action's post-processing, where Roslyn's formatter reformats around the new attribute-list tokens. The current output is therefore decided by Roslyn's rules, and, inferred but not run, by the host's formatting options: indent size, tabs, and the new-line string. CI runs on Linux only.
> - `AGENTS.md` (*Code-fix coverage*) says a rewrite that would relocate user-authored comments or directives must withhold itself for that input. These rows relocate them: the re-indented comment, doc-comment, `#pragma`, and `#region` lines, and the `/* c */` row. Principle P1 (#912) as decided covers changed comment or directive *text*, so these rows are not marked as decided by it.
> - Precedent: #1094 decided, for the RH5513 placement fix, that it also breaks before the list "as the formatter does", and that the fix output matches the formatter's layout in one application.
>
> **Readings:**
>
> - **Reading A:** the fix changes only the reported list. Each attribute after the first starts a new line at the indentation of the reported list's line, and nothing around the list moves. The only line breaks inserted are those between the new lists, in the document's line ending. `[A, B] class C` gives `[A]` / `[B] class C`, which RH5505 then reports.
> - **Reading B:** the fix output matches the formatter's layout (FMT-BREAK-027) for the reported list and what follows it, in one application, as #1094 decided for RH5513. Every line break the fix inserts uses the document's line ending (FMT-PIPE-007). The precedent does not say whether comments and directives in front of the list may be re-indented. Under this reading, that needs a decision.
> - **Reading C:** the current output is intended. That output comes from Roslyn's formatter under the host's options, not from code in this repository, so it cannot be written down as a stable requirement. Its line breaks follow the host's new-line option.

### Q2 — Fix All of a split rule needs two passes for two lists on one line

**Target issue:** `SDD open questions: sdd/specs/shared/code-fixes.md`. thoenissen/Reihitsu#1066 is that issue and is closed. Reopen it or open a new one, as the lead decides.

> **Touches:** SHARED-FIX-002 ("correct every reported diagnostic … in one Fix All application"). The rules are RH5502, RH5504, RH5506, RH5508, RH5510, RH5512, RH5514, RH5516, RH5518, RH5520, RH5522, RH5526, RH5528, and RH5531 in split mode.
>
> **Input** (the `GetFixAllScenario` of `RH5506ClassAttributeListsMustFollowShapeRulesAnalyzerTests`):
>
> ```csharp
> [First, Second] [Third, Fourth]
> internal class Example { }
> ```
>
> **Real output:**
>
> - One Fix All pass corrects one of the two lists, and a second pass corrects the other. Run on `[A, B] [D, E]` / `class C`, the first pass gives `[A]` / `[B]` / `[D, E]` / `class C`, and only the second gives `[A]` / `[B]` / `[D]` / `[E]` / `class C`.
> - Each of the 14 split-rule test classes passes only because its scenario sets `NumberOfFixAllIterations = 2`, which the testing library treats as an exact count. `FixAllCorrectsEveryDiagnostic` in `BatchCodeFixTestsBase` carries `[Requirement("SHARED-FIX-002")]`.
> - Two lists on separate lines of one declaration (`[A, B]` / `[D, E]` / `class C`) converge in one pass, and so do lists on two declarations.
> - The merge rules RH5524 and RH5529 set no count, so they use the library default of one pass.
>
> **Why it looks wrong:** SHARED-FIX-002 requires one application.
>
> **Notes:**
>
> - Probable cause: the two single fixes conflict in the gap between the two lists, which lies outside both of them. The fix of the first list rewrites that gap: `[A, B] [D2] void N()` becomes `[A]` / `[B]` / `[D2] void N()`. The fix of the second list rewrites it too: `[A] [B, D2] void M()` becomes `[A]` / `[B]` / `[D2]` / `void M()`. Only the out-of-list layout of the open question on `sdd/specs/shared/attribute-targets.md` (the split fix changing text outside the reported list) touches that gap, so that question is a probable common cause.
> - The same happens in a rule whose spec is already merged and enforced. `RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests` sets `NumberOfFixAllIterations = 2`, while `sdd/specs/analyzer/RH7103.md` states `Code fix: Fix All`.
> - These other test classes set `NumberOfFixAllIterations = 2` in at least one test and were not examined: RH3001, RH3002, RH3004, RH3102, RH5104, RH5110, RH5204, RH7004, RH7102, RH7109, RH7310.
> - RH3208, RH4106, RH4107, RH4109, RH5028, and RH8402 set `-2`, which means at most two passes. That does not by itself show that a second pass is needed.
> - Classes that set `1` agree with SHARED-FIX-002.
> - One cross-cutting issue may fit all of these.
> - Principle P7 (#912) is about a single fix, so it does not cover this. This is a violation of an existing requirement.

## Comment on thoenissen/Reihitsu#1093

To post on thoenissen/Reihitsu#1093 as is:

> Further instances from the list-shape specs (#981): "laid out like any other list on that declaration" also decides the list shape.
>
> - A `param:`/`typevar:` list on a method or accessor is today merged by RH5524/RH5529, and the declaration is joined onto its line. `[param: A, B]` or `[typevar: A, B]` there is reported by no rule.
> - On a one-line property whose accessor list SHARED-ATTR-006 does not exempt, RH5531 requires merged lists, while RH5528/RH5518 split them.
> - On an inline auto-property, `[A] [B] get;` is reported (RH5531), but `[return: A] [return: B] get;` is reported by no rule, although the formatter merges it.
>
> So following the decision also gives a list-shape diagnostic to code that no rule reports today.
>
> Affected requirements for the change doc:
>
> - RH5502-001, RH5504-001, RH5514-001, RH5516-001, RH5518-001, RH5520-001, and RH5528-001 ("every list with the `…:` specifier that is not attached to a parameter or type parameter") rely on SHARED-ATTR-002 as it stands.
> - RH5502-001, RH5504-001, RH5516-001, RH5518-001, and RH5520-001 reference SHARED-ATTR-014 only for lists on an accessor, which the compiler ignores (CS0657).

The details behind it, confirmed by running the analyzers and fixes, and the formatter where noted, are unchanged from `03-spec-author.md` (*Instances of thoenissen/Reihitsu#1093*), with one addition. The `typevar:` list on an accessor behaves as the comment says (see `04-skeptic.md`). `[typevar: A] [typevar: B]` / `set;` in a multi-line accessor list is merged by RH5529 to `[typevar: A, B] set;`, and `[typevar: A, B]` / `set;` is reported by none of the list-shape rules.

No instance of thoenissen/Reihitsu#1094, and no case matching P1, P2, P3, P5, P6, P7, or P8 (see the P1 note in Q1).

## Note for the lead (not routed, no spec change)

RH5529-001's example contains `[A] [property: B] V`, a `property:` list on a type parameter. The compiler ignores it (CS0657). The example shows current behavior: the list is not merged, because its specifier names another target (SHARED-ATTR-015). Merging it would start applying `B` to `T`, so not merging agrees with P1.

If thoenissen/Reihitsu#1093's "the attached declaration decides" is read to cover list shape too, this list is a further case for its change doc, next to the `type:` lists that SHARED-ATTR-015 already excludes. I left the example in place: SHARED-ATTR-015 states the behavior, and no type-parameter specifier other than `typevar:` is valid, so no valid example of "a list of another target" exists there. Whether to add this to the comment is the lead's call.
