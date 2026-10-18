Role: spec author
Commit: 8a7e5ec
Verdict: spec written

Repair round on the skeptic's handover `02-skeptic.md`. Its points were re-derived, not taken over. The spec changes are in commit 01ea580. `01-spec-author.md` stays as the record of the first round. This handover replaces its *Open questions to file* and its *Instances* sections.

## How the statements were established

- I re-ran the eight in-scope counterexamples of the skeptic's table through `scripts/apply-fix.sh RH5514`, LF and CRLF. All eight reproduce as stated, and every CRLF arm reports `line-ending-drift`.
- I probed 13 more split-fix inputs (RH5514, RH5506, RH5531), LF and CRLF. The ones inside the narrowed scope change nothing outside the list and do not drift:
  - a doc comment at the list's column;
  - a comment line at the list's column after a blank line;
  - two blank lines in front of the list;
  - a trailing `// c`;
  - a comment line between the list and the declaration;
  - a switch section;
  - a lambda block in an object initializer;
  - a top-level class after another class;
  - a multi-line accessor list.

  These change something outside the list:
  - `int _f;   ` (trailing spaces) above the list: the spaces are removed, CRLF drift;
  - a whitespace-only blank line above it: emptied, CRLF drift.

  Two more inputs changed nothing but are left out of the scope anyway: a `#region R` at the list's column, and a continuation-indented lambda block. The first is a directive, the second is not at the four-spaces-per-scope depth.
- I rebuilt the scratch example verifier (it runs every spec block through the shipped analyzers and code fixes, checking reported spans, before/after, "is not reported", and "no code fix is offered"). I ran it over `shared/attribute-targets.md` and the 16 analyzer specs twice, with LF and with every block converted to CRLF: 0 failures each time, so no example drifts under CRLF.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"`: 26 passed.

## Changed requirements

- SHARED-ATTR-017 — The split fix gives each attribute a list on its own line — changed (scope narrowed; clarification, no behavior change). The normative sentences are unchanged. The scope paragraph is now a list of three conditions:
  - the list starts its line, the line ends directly after its closing bracket or after a comment that follows it, and the first token after the list starts the next line;
  - no directive, no comment at another column, and no line ending in whitespace lies between the code before the list and the list;
  - the list, the line after it, and every enclosing opening brace are indented with spaces only, at four per enclosing namespace, type, enum, block, switch statement, accessor list, initializer, or anonymous object, plus four inside switch-section statements.

  Each of the skeptic's eight inputs breaks one of these conditions:
  - the class at column 0 or column 2 breaks the brace-indentation condition;
  - `#pragma` and `#region` are directives;
  - `// c` at column 0 and `///` at column 8 are comments at another column;
  - `[A, B]   ` has a line ending in whitespace.
- RH5514-001 — Method attribute lists hold one attribute — example changed. The `method:` list now sits on a method (`[method: A, B]` / `void N()`), not on a property, where the compiler ignores it (CS0657). That placement would also be affected by the decision of thoenissen/Reihitsu#1093, which rewrites SHARED-ATTR-002. The SHARED-ATTR-017 example already used `method:` on a method.
- `shared/attribute-targets.md` header and `sdd/specs/README.md`: code owner `AttributeTargetUtilities` added. Under `sdd/README.md` (*Spec format*), ownership follows the behavior a type implements, and a general-purpose utility is one whose behavior no spec specifies. `AttributeTargetUtilities` implements the target resolution of SHARED-ATTR-001 to SHARED-ATTR-003, the inline auto-property test of SHARED-ATTR-006, and the grouping, merge, and merge guard of SHARED-ATTR-015, SHARED-ATTR-018, and SHARED-ATTR-020. It owns no other spec. Precedent: `CasingUtilities`, also in `Reihitsu.Core`, owns `shared/casing.md`.

No requirement was added or retired, and the ledger is unchanged.

## Examples for the tests

The same as in `01-spec-author.md`. SHARED-ATTR-017's two examples and every RH55xx split-fix example are inside the narrowed scope, under LF and CRLF.

## Open questions to file

### Q1 — What the split fix changes outside the reported list

File in: `SDD open questions: sdd/specs/shared/attribute-targets.md`. thoenissen/Reihitsu#1030 is that issue and is closed; reopen it or open a new one. The former Q2 (LF line breaks in a CRLF document) is folded in here. It has nothing left to decide under Reading A, and under Readings B and C it is a consequence of the reading, so it is not a separate decision.

**Touches:** SHARED-ATTR-017, which covers only the layouts listed in its scope paragraph. Also the split-fix requirements RH5502-003, RH5504-003, RH5506-003, RH5508-003, RH5510-003, RH5512-003, RH5514-004, RH5516-003, RH5518-003, RH5520-003, RH5522-003, RH5526-003, RH5528-003, and RH5531-005.

**Input and real output** (`scripts/apply-fix.sh`; every fixture also declares `class A : System.Attribute { } class B : System.Attribute { } class D2 : System.Attribute { }`). Each of the following saved with CRLF line endings gives `line-ending-drift`: the fix writes LF line breaks into the CRLF document.

A list on its own line (RH5514, inside `class C`):

| Input | Output |
|---|---|
| `namespace N` / `{` / `class C` / `{` (class at column 0) / `    [A, B]` / `    void M()` | `[A]`, `[B]`, and `void M()` move to column 8; the class braces stay at 0 |
| the same with the class at column 2 and the list at 6 | `[A]`, `[B]`, and `void M()` move to column 8 |
| `#pragma warning disable CS0001` at column 0 above `    [A, B]` | LF output unchanged outside the list |
| `    #pragma warning disable CS0001` above `    [A, B]` | the `#pragma` line moves to column 0 |
| `#region R` at column 0 above `    [A, B]` | the `#region` line moves to column 4 |
| `// c` at column 0 above `    [A, B]` | the comment moves to column 4 |
| `        /// <summary>M</summary>` above `    [A, B]` | the doc comment moves to column 4 |
| `    int _f;   ` (trailing spaces), blank line, `    [A, B]` | the trailing spaces are removed |
| a whitespace-only line above `    [A, B]` | the line is emptied |
| `    [A, B]   ` (trailing spaces) / `    void M()` | the trailing spaces are removed |
| `        [A, B]` / `        void M()` in a class at column 0 (one level too deep) | `[A]`, `[B]`, `void M()` move to column 4; the method braces stay at 8 |
| `\t[A, B]` / `\tvoid M()` (tabs) | both lines rewritten with 4 spaces; the method braces keep their tabs |

A list that shares its line:

| Input | Output |
|---|---|
| `[A, B] class C` (RH5506) | `[A]` / `[B]` / `class C`; the declaration is moved onto its own line |
| `[A, B] void L()`, a local function in a switch section or in a lambda block of an object initializer (RH5514) | `[A]` / `[B] void L()`; the declaration is not moved |
| `[A] [B, D2] void M()` (RH5514) | `[A]` / `[B]` / `[D2]` / `void M()`; the list in front is broken off too |
| `[A, B] [D2] void N()` (RH5514) | `[A]` / `[B]` / `[D2] void N()` |
| `[return: A] [A, B] int R() => 0;` (RH5514) | `[return: A]` / `[A]` / `[B]` / `int R() => 0;`; RH5528's list is moved too |
| `System.Action a = [A, B] () => { };` in a method (RH5514) | `System.Action a = [A]` / `        [B] () => { };` |
| `int Q { [return: A, B] get => 1; }` (RH5528; RH5518 alike with `field:`) | `int Q {` / `        [return: A]` / `        [return: B]` / `        get => 1; }` |
| `class O { [A, B] class C { } }` in a namespace (RH5506) | `class O {` / `        [A]` / `        [B]` / `        class C { } }` |
| `/* c */ [A, B]` / `void N()` (RH5514) | `/* c */` / `[A]` / `[B]` / `void N()` |

**Why it looks wrong:** the fix changes text it does not report on: indentation, trailing whitespace, comment and directive lines, neighbouring lists, and line breaks of the declaration. What changes depends on the context, not on the rule. Line breaks it adds outside the list are LF whatever the document uses.

Facts for the decision:

- The provider itself rewrites only the reported list and inserts only the detected line ending. Everything else appears in the code action's post-processing, where Roslyn's formatter reformats around the new attribute-list tokens. Whatever that formatter decides today therefore depends on Roslyn's rules and, inferred and not run, on the host's formatting options (indent size, tabs, the new-line string). CI runs on Linux only.
- `AGENTS.md` (*Code-fix coverage*) says a rewrite that would relocate user-authored comments or directives must withhold itself for that input. The re-indented comment, doc comment, `#pragma`, and `#region` lines and the `/* c */` case relocate them. Principle P1 as decided covers changed comment or directive *text*, so these are not marked `Decided by principle P1 (#912)`.
- Precedent: thoenissen/Reihitsu#1094 (from thoenissen/Reihitsu#1030 Q2) decided for the RH5513 placement fix that it also breaks before the list "as the formatter does", and that the fix output matches the formatter's layout in one application.

**Readings:**

- Reading A: the fix changes only the reported list. Each attribute after the first starts a new line at the indentation of the reported list's line, nothing around the list moves, and the only inserted line breaks are those between the new lists, in the document's line ending. `[A, B] class C` gives `[A]` / `[B] class C`, which RH5505 then reports.
- Reading B: the fix output matches the formatter's layout (FMT-BREAK-027) for the reported list and what follows it, in one application, as decided for RH5513 in thoenissen/Reihitsu#1094. Every line break the fix inserts uses the document's line ending (FMT-PIPE-007). The precedent does not say whether comments and directives in front of the list may be re-indented, and that needs a decision under this reading.
- Reading C: the current output is intended. It is Roslyn's formatter output under the host's options, not code in this repository, so it cannot be written as a stable requirement. Its line breaks follow the host's new-line option.

No recommendation, as the lead asked.

### Q2 — Fix All of a split rule needs two passes for two lists on one declaration

File in: `SDD open questions: sdd/specs/shared/code-fixes.md`. thoenissen/Reihitsu#1066 is that issue and is closed; reopen it or open a new one. (This was Q3 in `01-spec-author.md`.)

**Touches:** SHARED-FIX-002 ("correct every reported diagnostic … in one Fix All application"). The rules are RH5502, RH5504, RH5506, RH5508, RH5510, RH5512, RH5514, RH5516, RH5518, RH5520, RH5522, RH5526, RH5528, and RH5531 in split mode.

**Input** (`GetFixAllScenario` of `RH5506ClassAttributeListsMustFollowShapeRulesAnalyzerTests`):

```csharp
[First, Second] [Third, Fourth]
internal class Example { }
```

**Real output:** one Fix All pass corrects one of the two lists, and a second pass the other. Each of the 14 split-rule test classes passes only because its scenario sets `NumberOfFixAllIterations = 2`, which the testing library treats as an exact count. `FixAllCorrectsEveryDiagnostic` in `BatchCodeFixTestsBase` carries `[Requirement("SHARED-FIX-002")]`. The merge rules RH5524 and RH5529 converge in one pass.

**Why it looks wrong:** SHARED-FIX-002 requires one application.

**Notes:**

- The same exists in a rule whose spec is already merged and enforced. RH7103 (`sdd/specs/analyzer/RH7103.md`, `Code fix: Fix All`) sets `NumberOfFixAllIterations = 2` in its test class (confirmed). Other test classes that set an explicit count (RH3001, RH3002, RH3004, RH3102, RH5030, RH5103, RH5104, RH5107, RH5110, RH5204, RH6003, RH6014, RH7004, RH7102, RH7109, RH7207, RH7310) were not examined. One cross-cutting issue may fit.
- For this batch, the two single fixes conflict in the gap between the two lists, which is outside both lists. Only the out-of-list layout of Q1 touches that gap, so Q1 is a probable common cause. Not verified: whether Fix All converges in one pass for lists on separate lines.
- Not covered by principle P7, which is about a single fix. This violates an existing requirement.

## Instances of thoenissen/Reihitsu#1093 (left out of the specs)

Comment text for the lead to post on thoenissen/Reihitsu#1093:

> Further instances from the list-shape specs (#981): "laid out like any other list on that declaration" also decides the list shape. A `param:`/`typevar:` list on a method or accessor is today merged by RH5524/RH5529 (and the declaration joined onto its line), and `[param: A, B]`/`[typevar: A, B]` there is reported by no rule. On a one-line property whose accessor list SHARED-ATTR-006 does not exempt, RH5531 requires merged lists while RH5528/RH5518 split. On an inline auto-property, `[A] [B] get;` is reported (RH5531) but `[return: A] [return: B] get;` by no rule, although the formatter merges it. So following the decision also gives a list-shape diagnostic to code no rule reports today.

Details, all confirmed by running the analyzers and fixes, and the formatter where noted:

1. The specifier, not the attached declaration, decides the shape. Left out of RH5524-001, RH5529-001, and RH5514-001.
   - `[param: A] [param: B]` / `void N(int x)`: RH5524 reports the second list, and its fix gives `[param: A, B] void N(int x)`.
   - `int P { get; [param: A] [param: B] set; }` is merged by RH5524. In a multi-line accessor list, `[param: A] [param: B]` / `set;` becomes `[param: A, B] set;`.
   - `[typevar: A] [typevar: B]` / `void N<T>()`: RH5529 gives `[typevar: A, B] void N<T>()`.
   - New: `[param: A, B]` / `void O(int x)` is reported by none of RH5502, RH5514, RH5516, RH5524, RH5528, RH5529, and RH5531. Neither is `[typevar: A, B]` / `void O<T>()`. `[A, B]` in the same place is reported by RH5514. The formatter (`scripts/format.sh`) turns the `param:` case into `[param: A, B] void O(int x)`.
2. The same accessor layout gets a different verdict by target. Left out of RH5531-001 and RH5531-002.
   - `int Q { [A] [B] get => 1; }` is reported and merged by RH5531, and `int T { [A, B] get => 1; }` is not reported. At the same place, RH5528 reports and splits `[return: A, B]`, and RH5518 reports and splits `[field: A, B]`.
   - The same holds for `int S { [A] [B] get; /* c */ set; }` and for `event System.Action E { [A] [B] add { } remove { } }`.
   - Per the skeptic's runs, the formatter expands and splits all of these.
3. Confirmed (was "possibly related"): `int P { [return: A] [return: B] get; set; }` is reported by none of the seven rules above. The formatter merges it to `int P { [return: A, B] get; set; }` (run). `int P { [A] [B] get; set; }` is reported by RH5531 (RH5531-002).

No instance of thoenissen/Reihitsu#1094. No case matches P1, P2, P3, P5, P6, P7, or P8 (see the P1 note in Q1).

## Follow-up candidate (out of scope)

`AttributeTargetFormattingRewriter` and `AttributeTargetFormattingShared` in the formatter call `AttributeTargetUtilities`. Under `sdd/README.md` (*Spec format*), the spec of such a caller lists the requirement IDs it relies on under `Uses`. `sdd/specs/formatter/line-breaks.md` references no SHARED-ATTR requirement in its text, so its `Uses` cannot gain them without a text change to FMT-BREAK-027. That is a formatter spec change outside this run.
