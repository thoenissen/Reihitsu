Role: skeptic
Commit: e907deb27117899a246ecd597a6823c62d8dd2e4
Verdict: proceed narrower

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

The lane writes current behavior, so the premise is that every new requirement is true of the code. I read `TargetAttributeListShapeAnalyzerBase`, `TargetAttributeListShapeCodeFixProviderBase`, the overrides of the 16 analyzers and of the RH5531 provider, `AttributeTargetUtilities` (target resolution, `HaveSameTarget`, `CanMergeAttributeListGroup`, `MergeAttributeListGroup`), the trivia guards in `SyntaxNodeUtilities`, and the formatter's `AttributeTargetFormattingRewriter`. I ran about 110 fixtures through `scripts/apply-fix.sh` (LF and CRLF; `--max-iterations 1` where a single application mattered), ran 15 fixtures through `scripts/format.sh` twice (pass 2 was a no-op), and ran `scripts/test.sh --project analyzer` on the RH5506 test class and the RH5524 Fix All test (8 passed) and the architecture `SpecConsistencyTests|SpecStructureTests` (26 passed). Fixtures were in the scratchpad and are deleted.

**Holds.** SHARED-ATTR-013, -014, -015, -016, -018, -019, -020 and every rule requirement that only references them agree with the code and with every probe. The merge fix touched nothing outside the merged group in every probe, including a tab-indented parameter line, a parameter line at an odd column, a class at column 0 inside a namespace, and CRLF. The split fix gave the specified output for a well-indented list in every split rule (one in-scope fixture per rule, with a comment, a blank line, a doc comment, `#nullable` at column 0 before a top-level list, a switch section, a lambda block in an argument, a K&R lambda, a continuation-indented lambda, an object initializer, an array initializer, top-level statements, a file-scoped namespace), with no line-ending drift.

**Does not hold: SHARED-ATTR-017 inside its own stated scope.** Each input below starts its line, is followed by nothing on its line, and has its line and the next indented with spaces four more than the line of the innermost enclosing opening brace, so the scope paragraph admits it. The output breaks "nothing else changes", "each list after the first starts a new line at the indentation of the reported list", or the line-ending sentence (RH5514, `class A/B/D2 : System.Attribute` declared in every fixture):

| Input (inside `class C { … }` unless stated) | Output |
|---|---|
| `namespace N {` / `class C {` at column 0 / `    [A, B]` / `    void M()` | `[A]`, `[B]`, `void M()` all moved to column 8; braces stay. CRLF: one LF. |
| `namespace N {` / `  class C {` at column 2 / `      [A, B]` / `      void M()` | all three lines moved to column 8. CRLF: one LF. |
| `#pragma warning disable CS0001` at column 0 / `    [A, B]` / `    void M()` | LF arm correct; CRLF arm ends with one LF. |
| `    #pragma warning disable CS0001` / `    [A, B]` / `    void M()` | the `#pragma` line moved to column 0; CRLF: one LF. |
| `#region R` at column 0 / `    [A, B]` / `    void M()` | the `#region` line moved to column 4; CRLF: two LF. |
| `// c` at column 0 / `    [A, B]` / `    void M()` | the comment moved to column 4; CRLF drift. |
| `        /// <summary>M</summary>` / `    [A, B]` / `    void M()` | the doc comment moved to column 4; CRLF drift. |
| `    [A, B]   ` (trailing spaces) / `    void M()` | trailing spaces removed; CRLF: one LF. |

So the scope as written is wrong: the fix's layout around the list is decided relative to the absolute nesting depth, not to the brace's line, and every comment, directive, or trailing whitespace on the lines around the list counts. Routed back to the spec author: SHARED-ATTR-017 must be narrowed so that none of these inputs is inside it (or each is shown with its real output, if the maintainer settles Q1 first). The examples of SHARED-ATTR-017 and RH55xx-003/RH5514-004/RH5531-005 are all inside the narrower ground and stay valid.

**Why the split fix behaves this way (context for Q1–Q3, established from the code plus the outputs).** `ApplySplitCodeFixAsync` replaces only the owner's attribute list and inserts only the detected line ending; it reuses every token outside the list unchanged. Its new lists come from `SyntaxFactory.AttributeList(…)`, whose bracket tokens carry elastic trivia; the provider overwrites only the first list's leading, the later lists' leading, and the last list's trailing trivia. Every change outside the list and every LF in a CRLF output therefore appears after the provider returns, in the code action's own post-processing, which runs Roslyn's formatter over elastic trivia. Inferred, not run: that formatter takes indentation size, tabs, and the new-line string from the host's formatting options (`.editorconfig`, defaulting to 4 spaces and `Environment.NewLine`), so the out-of-list output can differ by editor configuration and OS, and on Windows the Q2 inputs would likely get CRLF inside an LF document. CI runs on Linux only. This is failure class FC-030 (a fix formats more than the node it fixes).

**Minor.** The RH5514-001 example `{|RH5514:[method: A, B]|} int P { get; set; }` puts a `method:` list on a property, which the compiler ignores (CS0657). Today it is reported by RH5514 under SHARED-ATTR-002. thoenissen/Reihitsu#1093's change doc rewrites SHARED-ATTR-002 so that the attached declaration decides, and that may move this list to RH5516. Every other specifier example in the batch is a valid C# placement (`field:` and `property:` on a property, `event:` on an event field, `return:` on a method or accessor). A `method:` list on a method or an event field would keep the example out of thoenissen/Reihitsu#1093's way. For the spec author to decide.

**Note for the spec checker.** `AttributeTargetUtilities` implements SHARED-ATTR-001 to -003, -006, -015, -018 and -020 and is a code owner of no spec. The attribute-targets spec of thoenissen/Reihitsu#980 treated it the same way. Under `sdd/README.md` (*Spec format*, the code-owner rule) it may belong to `shared/attribute-targets.md`.

## Mechanism and candidates

_N/A_: no bug fix in this lane. The candidate set for coverage, taken from the dispatch code and not from the issue:

- Owners (`AttributeTargetUtilities.GetAttributeLists`): compilation unit, every `MemberDeclarationSyntax` (types, records, delegates, enum members, fields, event fields, events, properties, indexers, methods, operators, conversion operators, constructors, destructors), `AccessorDeclarationSyntax`, `LocalFunctionStatementSyntax`, `LambdaExpressionSyntax` (simple and parenthesized), `ParameterSyntax`, `TypeParameterSyntax`. Lists on other nodes (statements, function-pointer parameters, incomplete members) resolve to no target.
- Targets (`TryResolveTarget`): no specifier; `assembly`, `module`, `field`, `event`, `method`, `param`, `property`, `return`, `typevar`; `type` (resolves through the owner); an unknown specifier (no target). Layout target (`TryResolveLayoutTarget`): a parameter or type-parameter owner overrides every specifier.
- Shape mode: split everywhere, merge for RH5524 and RH5529, and for RH5531 on a single-line owner (`IsAttributeListOnSingleLinePropertyAccessor`, without the body and comment conditions of SHARED-ATTR-006). Split exemption: `IsAttributeListOnInlineAutoPropertyAccessor` (single-line owner, no accessor body, no comment or directive in the accessor list). Merge grouping: `HaveSameTarget` (named target, `type:` on a non-type owner never groups).

## Breaking inputs

- FC-030, FC-033, FC-017: the split-fix tests must assert the whole document, under LF and CRLF, so that a change outside the list fails the test. The existing CRLF test (`VerifyInsertedLineBreakUsesDetectedCarriageReturnLineFeedEndOfLine`, RH5506) covers only a list at column 0 in front of `internal class`.
- FC-004, FC-019, FC-020, FC-021: for the split fix, a comment in front of the list on its own line and on the list's line, a trailing comment, a doc comment, and `#if`/`#pragma`/`#region` in front of the list (the table above). For the merge fix, the SHARED-ATTR-020 boundaries: a comment before the first list (allowed), between lists, after the last list on its line, in the leading trivia of the token after the group, and after that token (allowed); a directive in front of the first list and inside the group.
- FC-002, FC-003: SHARED-ATTR-014 on both sides: an inline auto-property; the same property with an expression-bodied accessor, with a comment between accessors, and on several lines. SHARED-ATTR-015: `type:` on a type declaration and on a parameter, interleaved lists of another target, a single list of the target.
- FC-027: the enclosing scopes listed under *Holds*, for at least one split rule, plus enum members, accessor lists, and lambda, local-function, delegate, operator, indexer, and primary-constructor parameters for RH5524.
- FC-008: Fix All with two reported lists on one line (every current split scenario is this shape and needs `NumberOfFixAllIterations = 2`), and with two reported lists on separate lines. Whether the second converges in one pass was not established: no existing entry point runs Fix All on a fixture. A scenario with default iterations would show it.
- FC-026: two identical lists on one line, such as `[A, B] [A, B]`.
- FC-013: the formatter agrees with every written requirement I probed; the two disagreements found are the thoenissen/Reihitsu#1093 cases below.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `TargetAttributeListShapeAnalyzerBase` and the 16 analyzers | must stay |
| `TargetAttributeListShapeCodeFixProviderBase` and the 16 providers | must stay |
| Fix All (`WellKnownFixAllProviders.BatchFixer`) | must stay; Q3 |
| `AttributeTargetUtilities`, `SyntaxNodeUtilities` (`Reihitsu.Core`) | must stay |
| Formatter (`AttributeTargetFormattingRewriter`, FMT-BREAK-027) | not involved; no spec or code change |
| `shared/attribute-targets.md` | must change: SHARED-ATTR-017 scope (above) |
| `analyzer/RH5514.md` | may change: the `method:`-on-property example (above) |
| Rule pages `documentation/rules/RH55xx.md` | not involved |
| Tests | must change: tags and new tests (test implementer) |

## Simpler option

For SHARED-ATTR-017: narrow the scope to ground the probes show is stable (enclosing braces and the list at four spaces per nesting level, no directive in front of the list, comment lines in front at the list's column, no trailing whitespace). Trying to list every condition under which Roslyn's formatter leaves the text alone would not give a stable scope: those conditions are Roslyn's rules and the host's options (inferred).

## Questions for the maintainer

Each item the spec author lists is judged below. I reproduced every claimed input and output of Q1, Q2, Q3 and of both thoenissen/Reihitsu#1093 sections, except where a line says otherwise.

**Q1 (split-fix layout depends on context): real, an open question, filed in the right issue** (`SDD open questions: sdd/specs/shared/attribute-targets.md`; thoenissen/Reihitsu#1030 is closed). Additional facts for the question:

- Reading C, "the current output is intended", would specify Roslyn's formatter output under the host's options, not code in this repository (mechanism above). It cannot be written as a stable requirement.
- The question should carry the in-scope examples from the table above (re-indented comment, `#pragma`, and `#region` lines; absolute-depth re-indentation). Under SHARED-ATTR-017's current wording they are inside the requirement, not outside it.
- `/* c */ [A, B]` → `/* c */` / `[A]` / `[B]`, and the re-indented comment and directive lines, relocate user comments and directives (FC-004). `AGENTS.md` (*Code-fix coverage*) says a rewrite that relocates them must withhold itself. P1 as decided covers changed comment or directive *text*, so I do not mark these `Decided by principle P1`. The lead may cite the `AGENTS.md` rule next to them.
- Precedent the question should cite: thoenissen/Reihitsu#1094 (from thoenissen/Reihitsu#1030 Q2) decided that the RH5513 placement fix also breaks before the list "as the formatter does", and that the fix output matches the formatter's layout in one application. The spec author recommends Reading A (change only the reported list), which runs against that precedent. Reading B follows it. I take no side. Both readings and the precedent belong in the question.

**Q2 (LF written into CRLF): real, but it depends on Q1. Fold it into Q1 or file it as conditional on Q1.** Under Q1 Reading A, the only line breaks the fix writes are the ones between the new lists, and those already use the detected ending: Q2 then has nothing left to decide. Only under Readings B and C do other breaks exist. Reading B of Q2 ("others are not specified") leaves those breaks to `Environment.NewLine` (inferred), so the line ending would depend on the user's OS. The question should say so. In-scope CRLF drift (the table above) is a spec error in SHARED-ATTR-017 to repair in this run, not part of Q2.

**Q3 (Fix All needs two passes): real, correctly classified as a violation of SHARED-FIX-002, and correctly filed under `shared/code-fixes.md`, not attribute-targets.** For the testing library, an explicit `NumberOfFixAllIterations = 2` is an exact count, so the passing tests prove that exactly two passes are needed. Additions:

- The same violation exists outside this batch in a rule whose spec is already merged and enforced: RH7103's scenario sets `NumberOfFixAllIterations = 2` while `FixAllCorrectsEveryDiagnostic` carries SHARED-FIX-002. Other test classes with an explicit count of 2 or more: RH3001, RH3002, RH3004, RH3102, RH5104, RH5110, RH5204, RH7004, RH7102, RH7109, RH7310 (not examined). That supports the cross-cutting issue the spec author proposes.
- For this batch, the conflict lies in text outside both reported lists. Applied alone, the fix of the first list rewrites the gap between the two lists (`[A, B] [D2] class C` → `[A]` / `[B]` / `[D2] class C`). The fix of the second list rewrites the same gap (`[A] [B, D2] class C` → `[A]` / `[B]` / `[D2]` / `class C`). Only the out-of-list layout of Q1 touches that gap, so the question should name Q1 as a probable common cause. Not verified: whether Fix All converges in one pass for lists on separate lines.
- Not P7, which is about a single fix: agreed.

**Instance of thoenissen/Reihitsu#1093, "the specifier, not the attached declaration, decides the list shape": real, classification correct.** The merge fix joins the declaration onto the list's line (`[param: A] [param: B]` / `void N(int x)` → `[param: A, B] void N(int x)`; `[typevar: A] [typevar: B]` / `void N<T>()` likewise; a multi-line `[param: A] [param: B]` / `set;` → `[param: A, B] set;`). That is the placement thoenissen/Reihitsu#1093 (thoenissen/Reihitsu#1030 Q1) decided. The formatter gives the same output today. One case to add: `[param: A, B]` / `void O(int x)` is, like `[typevar: A, B]`, reported by no list-shape rule, while `[A, B]` in the same place is reported by RH5514. The one-line comment on thoenissen/Reihitsu#1093 should say that "laid out like any other list on that declaration" also decides the list shape (split versus merged). thoenissen/Reihitsu#1093's decision text and its *Done when* name placement inputs only.

**Instance of thoenissen/Reihitsu#1093, "the same accessor layout gets a different verdict by target": real, classification correct, and resolvable without a new reading.** The formatter expands and splits every one of these: `int Q { [A] [B] get => 1; }`, `int T { [A, B] get => 1; }`, `int S { [A] [B] get; /* c */ set; }`, and the one-line event all become a multi-line accessor list with `[A]` / `[B]` on separate lines. The `return:` and `field:` variants are split the same way. So RH5531's merge fix output is undone by the formatter, and RH5531-clean `int T { [A, B] get => 1; }` is changed by it (FC-013). thoenissen/Reihitsu#1093 decided "the same verdict regardless of target, following SHARED-ATTR-006 and the formatter", and that gives one answer here: the RH5528 and RH5518 verdict.

**"Possibly related" case left to the lead: also an instance of thoenissen/Reihitsu#1093.** On an inline auto-property, `int P { [A] [B] get; set; }` is reported by RH5531, and `int P { [return: A] [return: B] get; set; }` is reported by no rule, yet the formatter merges it to `[return: A, B]` (run). The `field:` variant gets no diagnostic either (RH5531 run; RH5518 reports only lists with several attributes); the formatter was not run on it. This is the same layout getting a different verdict by target, with an analyzer-clean input changed by the formatter. The comment should note that following the decision gives a list-shape diagnostic to code that no rule reports today.

No case matches P1, P2, P3, P5, P6, or P7: agreed (see the P1 note under Q1).
