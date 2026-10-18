Role: skeptic
Commit: 3d1f74ec7c722bfcef16998627319246296a7d42
Verdict: proceed narrower

Second round. As the lead asked, it checks only the revision in 01ea580 and the handover `03-spec-author.md`. Method: 29 new fixtures through `scripts/apply-fix.sh` (LF and CRLF; RH5514, RH5506, RH5529, and five other rules for "reported by no rule"), one snippet compiled with `dotnet build` for CS0657, and `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"` (26 passed). Fixtures were in the scratchpad and are deleted. The lead's message carried no intended fix or hoped-for verdict.

## Premise

### (a) SHARED-ATTR-017's narrowed scope

These inputs are inside the scope and give the specified output under LF and CRLF:

- the class header at column 0 with its brace at 4;
- a method header at 2 with its brace at 4;
- an `if` header at 12 with its brace at 8;
- a lambda block under a switch expression;
- a braced switch section;
- a lambda block in an anonymous object, in a `with` initializer, and in an argument on a new line without continuation;
- a multi-line `/* */` block comment and a `/** */` doc comment at the list's column;
- a `///` continuation line indented with a tab;
- a trailing `// c   ` (its spaces belong to the comment and are kept);
- two trailing block comments;
- trailing spaces on the declaration line;
- an empty blank line, or a comment line at the list's column, between the list and the declaration.

Still broken, in the scope as written:

| Input (RH5514 unless stated) | Output |
|---|---|
| `namespace N {` / `class C {` / `/// <summary>` at 8, `/// M` at 6, `/// </summary>` at 8 / `[A, B]` at 8 / `void M()` at 8 | the `/// M` line moves to column 8; CRLF: drift |
| RH5506, top level: `/// <summary>` at 0, `/// C` at 4, `/// </summary>` at 0 / `[A, B]` / `class C` | the `/// C` line moves to column 0 (no CRLF drift) |

A `///` documentation comment is one comment that starts at the list's column, so condition 2 ("every comment there starts at the list's column") admits both inputs. Only if each `///` line counts as a comment of its own are they outside the scope. The scope must say which, for example "every comment line".

Unclear in the scope as written, between the list and the token after it:

| Input | Output |
|---|---|
| `[A, B]` / `// c` at column 0 / `    void M()` | the comment moves to column 4; CRLF: drift |
| `[A, B]` / a whitespace-only line / `    void M()` | the line is emptied; CRLF: drift |

Read strictly, condition 1 ("the first token after the list starts the next line") puts both outside the scope. But that reading also excludes the inputs `03-spec-author.md` reports as probed inside it: "a comment line between the list and the declaration" and, by the same words, a blank line. Read loosely, both inputs above are inside the scope and broken. Condition 3's "the line after it" has the same ambiguity: the comment line, or the declaration's line. The scope should say what may lie between the list and the first token after it. In every probe, empty lines and comment lines at the list's column were safe.

### (b) The RH5514-001 example

Correct. `[method: A, B]` / `void N()` is a valid placement, is reported by RH5514, and is split by its fix (run).

The same defect remains in RH5518-001. Its examples `int Q { [field: A, B] get; set; }` (not reported) and `int S { {|RH5518:[field: A, B]|} get => 1; }` put a `field:` list on a `get` accessor. The compiler ignores that list: `dotnet build` reports CS0657 ("valid attribute locations for this declaration are 'method, return'"). The `method:` list on a property in the old RH5514 example fails the same way (CS0657 confirmed). Under thoenissen/Reihitsu#1093's change to SHARED-ATTR-002, these lists may become RH5531's. The reason the revision gives for RH5514 applies here too. RH5518-001's reference to SHARED-ATTR-014 exists only for such lists, and so do the references in RH5502-001, RH5504-001, RH5516-001, and RH5520-001 (none of those shows an example). For the spec author to decide.

### (c) `AttributeTargetUtilities` as code owner

Correct under `sdd/README.md` (*Spec format*).

- Its target resolution, inline-accessor test, grouping, merge, and merge guard are specified by SHARED-ATTR-001 to -003, -006, -015, -018, and -020.
- No other spec names it.
- `CasingUtilities` in `shared/casing.md` is the precedent.
- The index row and the header match, and the architecture tests pass.

The follow-up the spec author names is real but outside this run's scope: `formatter/line-breaks.md` calls `AttributeTargetUtilities` through `AttributeTargetFormattingRewriter` but lists no SHARED-ATTR requirement under `Uses`.

## Mechanism and candidates

_N/A_ (no bug fix in this lane).

## Breaking inputs

In addition to `02-skeptic.md`:

- FC-004 and FC-021: a `///` documentation comment whose continuation lines sit at another column than its first line.
- FC-004 and FC-021: a comment line, and a whitespace-only line, between the list and the declaration.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `shared/attribute-targets.md` | must change: SHARED-ATTR-017 scope wording (above) |
| `analyzer/RH5518.md` | may change: the `field:`-on-accessor examples (above) |
| Analyzers, code fixes, `Reihitsu.Core`, formatter, rule pages | not involved |

## Simpler option

_None._

## Questions for the maintainer

(d) Classification and wording of the revised questions.

**Q1 (split-fix layout, the former Q2 folded in): correctly classified, the readings are complete, no recommendation, right issue.** Two corrections are needed before it is filed:

- The intro says each listed input, saved with CRLF, gives `line-ending-drift`. That is false for the row "`[A, B] void L()`, a local function in a switch section or in a lambda block of an object initializer" (run for both, plus a plain method block). The output is `[A]` / `[B] void L()` with no drift, because the only break inserted is the fix's own, with the detected ending.
- The first row says "the class braces stay at 0". The method's own braces also stay at 4 while `void M()` moves to 8, which is what makes that output visibly broken. Worth stating.

The table row "`int Q { [return: A, B] get => 1; }` (RH5528; RH5518 alike with `field:`)" relies, for RH5518, on the CS0657 placement above. The `return:` input alone carries the point.

**Q2 (Fix All needs two passes, the former Q3): correctly classified (violation of SHARED-FIX-002), right issue.** One correction is needed: the list of "other test classes that set an explicit count" includes RH5030, RH5103, RH5107, RH6003, RH6014, and RH7207. Each of these sets `NumberOfFixAllIterations = 1`, which is exactly one pass and agrees with SHARED-FIX-002. Only RH3001, RH3002, RH3004, RH3102, RH5104, RH5110, RH5204, RH7004, RH7102, RH7103, RH7109, and RH7310 set 2 or more. The RH5524 and RH5529 test classes set no count, so they use the library default of at most one pass: agreed.

**The thoenissen/Reihitsu#1093 comment and its details: correct and self-contained.** I re-checked the one statement that was not yet run: a `typevar:` list on an accessor behaves as the comment says. `[typevar: A] [typevar: B]` / `set;` in a multi-line accessor list is merged by RH5529 to `[typevar: A, B] set;`. `[typevar: A, B]` / `set;` is reported by none of RH5502, RH5514, RH5516, RH5524, RH5528, RH5529, and RH5531.

A fact the comment could add, because #1093's change doc must list it: the normative sentences of RH5502-001, RH5504-001, RH5514-001, RH5516-001, RH5518-001, RH5520-001, and RH5528-001 ("every list with the `…:` specifier that is not attached to a parameter or type parameter") rely on SHARED-ATTR-002 as it stands. They are affected requirements of that change as well.
