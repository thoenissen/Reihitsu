Role: auditor
Commit: 0c7c198045012f1875b866828ff2beeb40831aa2
Verdict: pass (#1762: pass; #1763: pass)

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

State verified: checkout is `0c7c198`; `git ls-remote origin refs/heads/main` is `1d2d29a`, which is the branch's base and an ancestor of HEAD; the diff (8 production files, +77/-7; 8 test files; specs, two change docs, ledger, `sdd/work/`) is what `05-implementer.md`, `04-test-implementer.md`, and the spec-author/spec-checker handovers describe. `scripts/lane-check.sh behavior-change` → `confirmed`.

How facts were established (fixtures under the session scratchpad, outside the repository): `scripts/format.sh` (CLI, C# 15 default) over union, class-analog, and `with`-element fixtures, a second pass (0 of N changed), and the same fixtures converted to CRLF (identical modulo `\r`); `scripts/apply-fix.sh` for RH7101, RH5404, RH5305, RH3202 over union and class-analog fixtures (LF and CRLF, all converge, 0 line-ending drift); a Roslyn 5.12 probe (`CSharpParseOptions.Default` is C# 15; a `with(…)` element parses as `WithElementSyntax` without syntax errors under C# 14 and 15). No repository test was re-run: no finding turned on one.

## thoenissen/Reihitsu#1762

### Scope
Head `0c7c198`, base `1d2d29a`; full audit.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

Against the specs: FMT-BREAK-002, -007, -011, -023, FMT-STRUCT-017 (C# 15 gate through the shared `CanConvertSafely`), FMT-STRUCT-018, FMT-STRUCT-021, FMT-PIPE-029, FMT-PIPE-031 and RH7101-002 hold in the code for every spec example and for the skeptic's input classes (one-line unions, block-bodied member, base list, constructor initializer, `readonly`/`partial`, nesting in class/struct/record/union, file-scoped and block namespaces, top level, `#region` in the body, comments and directives before `(`, before `where`, before `{`, in an empty body, `static`/`const`/`static readonly` combined fields). Additional probes beyond the tests, each equal to its class analog and stable on a second pass: `union U(int, string) { };` keeps its braces (as `class C(int a) { };`), `{ } // trailing` → `; // trailing`, a directive before an empty body keeps the braces, base list plus constraint plus empty body → `: I⏎    where T : class;`, comments inside the case-type list, attribute and doc comments on members of a one-line union, nested empty class → `class Inner;`. Issue requirements: every row of #1762's component table is addressed; the blank-line/indentation/Core check is answered by the skeptic (kind-agnostic; `AttributeTargetUtilities` out of scope by lead decision 2); "Done when" items met (specs, regression tests through `AssertRuleResult`, which covers LF, CRLF, a second pass, and per-phase stability).

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Field split applies (FMT-STRUCT-021) | `FieldDeclarationSplitTransform.Splits`: declarator count, parent kind (now incl. union), directives, doc comment | `VisitUnionDeclaration` of the same transform (dispatch); `ReihitsuFormatter.SplitsFieldDeclaration` (delegates to `Splits`); `FormatSplitFieldInContext` cast to `TypeDeclarationSyntax` (union is one); RH7101 fix gate `SplitsFieldDeclaration && FindUnsupportedSyntax(parent) == null` | Same kind set at the predicate and at the visitors; the fix asks the same predicate the phase uses | ok |
| Empty body → `;` (FMT-STRUCT-017/018) | `CanConvertSafely(node, kind, CSharp15, version)`: semicolon absent, trivia between header and `{`, inside `{`…`}` | Core `EmptyTypeDeclarationSemicolonAnalysisUtilities` (kind-agnostic, shared with RH5412–RH5416) | One owner, reused | ok |
| Braces placed (FMT-BREAK-002) | `DeclarationBraceLineBreakRewriter.Visit` type case, skipped when `OpenBraceToken.IsMissing` | `MemberLineStartRewriter.VisitUnionDeclaration`, `GenericConstraintLineBreakRewriter` adapter, `LineBreakListRewriter.VisitUnionDeclaration` | All four now name union; extension blocks still differ between them (pre-existing, lead decision 2, follow-up (c)) | ok for the run |
| `(` joins the name (FMT-BREAK-011) | `CollapseOwnedOpenParenToDeclarationLine(node, ParameterList)`, null-guarded | Class/struct/record/interface overrides (same body); comment in the gap keeps the line break (test) | Same | ok |
| `with` element spacing (FMT-SPACE-028) | `WithKeyword` whose parent is `WithElementSyntax`, as left token, same-line gaps only | `IsWithKeyword` (parent `WithExpressionSyntax`, FMT-SPACE-025); no earlier rule in `SpacingPolicy` decides the pair | Disjoint parents; `with` expression keeps 1 (existing tests) | ok (see #1763) |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which declarations count as a type for layout/shape | `DeclarationBraceLineBreakRewriter.cs:169-173`, `MemberLineStartRewriter.cs:145-200`, `LineBreakListRewriter.cs:563-620`, `GenericConstraintLineBreakRewriter.cs:76-80`, `EmptyTypeDeclarationSemicolonTransform.cs:97-177`, `FieldDeclarationSplitTransform.cs:51,170-243`, `Reihitsu.Core/AttributeTargetUtilities.cs:476-496` (`rg "ClassDeclarationSyntax\|StructDeclarationSyntax\|RecordDeclarationSyntax\|InterfaceDeclarationSyntax\|ExtensionBlockDeclarationSyntax"` in Formatter and Core) | Seven owners, pre-existing and reported by the skeptic; lead decision 2 fixes narrowly (six of seven now name union) with consolidation as follow-up (c). No new owner added by the diff. Not a new finding |
| Indentation depth of a type body | `Reihitsu.Core/SyntaxIndentationUtilities.GetIndentingScopeRange` (`BaseTypeDeclarationSyntax`) | Kind-agnostic; covers union | ok |
| Empty-body eligibility | `Reihitsu.Core/EmptyTypeDeclarationSemicolonAnalysisUtilities.CanConvertSafely` | Single owner, reused | ok |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `UnionDeclarationLayoutTests.*` (19) | Exact union layout per FMT-BREAK-002/007/011/023 | Any differing output, LF or CRLF, or a second pass / single phase changing it | `AssertRuleResult` (exact, LF+CRLF, second pass, `PhaseStabilityCheck`, phase-tree round trip) | ok |
| `UnionDeclarationStructuralTests.*` (10) | Exact empty-body and split outputs; braces kept with comment/directive | Same | `AssertRuleResult` | ok; the three `expected: null` tests (`UnionDeclaredWithASemicolonStaysUnchanged`, `EmptyUnionBodyWithADirectiveKeepsItsBraces`, and in the layout class `LaidOutUnionStaysUnchanged`, `BlockCommentBetweenUnionNameAndCaseTypeListStays`, `ConstraintClauseBehindAnEndOfLineCommentStaysUnchanged`) also passed on `main`; each still falsifies a later regression (directive/comment guard, join across a comment), so they are characterization tests, not proofs of the change, which the failing-first tests next to them provide |
| `ReihitsuFormatterSplitsFieldDeclarationTests` (2 new) | `SplitsFieldDeclaration` true for static and const combined fields in a union, false for one declarator | Wrong boolean | Direct call | ok |
| `RH7101…InAUnion*` (3) | Fix offered, splits, converges, Fix All over two diagnostics, CRLF kept | Fix not offered, different text, extra iteration, LF in output | `Verify` + `ApplyCodeFixAsync` on CRLF | ok; `CompilerDiagnostics.None` is justified (.NET 9 reference assemblies lack union types, thoenissen/Reihitsu#1766) and the fix only splits static/const fields, so it cannot hide a compile error the fix introduces in a way the exact-text comparison would miss |
| `RH5404…VerifyFixLaysOutAUnionInsideTheFixedType` | Change 7 output | Different text | `Verify` (LF) | ok (CRLF of the shared formatter path is covered by the formatter tests; `apply-fix.sh` CRLF arm identical) |
| `CliLanguageVersionEndToEndTests.UnionWithOneLineBracesIsLaidOutWithoutLangVersion` | CLI default lays out union; `--check` clean afterwards | Interim layout | CLI end-to-end | ok |

### Visible output is decided
Every new or changed expected value is change 1–7 of `union-declaration-layout.md` or the output a class gets today, which the maintainer's answer Q3 A ("a union gets the layout and rewrites of a class, and the code fixes follow", recorded in the change doc's *Decisions* and in `00-lead.md`) covers. Class analogs were run at HEAD for every test whose output the change doc does not show verbatim (region in body, end-of-line comment before `(`, directive before `where`, block comment before `where`, end-of-line comment behind `{` of an empty body, constructor initializer, nested type without blank line): each union output equals the class output. The RH5404 fix turning a nested empty union into `union V(int, string);` equals the class analog (`class V;`) and follows from Q2 A + Q3 A. No rule page changed.

### Cost of special cases
_None._ The production change adds a kind to existing dispatch sites; no new guard. Declaring a shape unsupported: not an option (deletes nothing; lead decision 5).

### Comments
`FieldDeclarationSplitTransform.Splits` summary, `ReihitsuFormatter.SplitsFieldDeclaration` summary and the `FormatSplitFieldInContext` cast comment name the union; the other changed classes' summaries name no kinds. No `TODO`, no commented-out code, no tracker numbers in source.

### Failure classes dismissed
- FC-001, FC-002, FC-003: no guard narrowed or added; the existing guards (`IsMissing`, `CanConvertSafely`, `Splits`) are reused unchanged and tested on both sides (semicolon/braced, one/several declarators, with/without comment or directive).
- FC-004, FC-005, FC-019, FC-020, FC-021: comments and directives in every gap the four line-break decisions and the two structural transforms touch are tested or probed; each keeps its place as for a class.
- FC-006: the C# 15 gate cannot be false for any caller (no union node under C# 14, contexts resolved from parse options; lead decision 3); `partial` union probed and tested.
- FC-007, FC-008: RH7101 converges in one iteration per diagnostic, Fix All tested; RH5404/RH3202 on unions converge (`apply-fix.sh`).
- FC-009: the RH7101 fix is offered only where `SplitsFieldDeclaration` is true, which the phase honors for unions.
- FC-010, FC-011, FC-012, FC-017: second pass and CRLF stable for every probe and test.
- FC-013, FC-014: analyzer gaps for unions (RH5404 registration, RH5110, RH5412–RH5416) are the rules' own and tracked in thoenissen/Reihitsu#1767 (P11); the skeptic ran the after outputs through the RH55xx fixes with no diagnostic.
- FC-015, FC-016: see *Policy ownership*; the sibling kind left out (extension block) is decision 2 / follow-up (c), not introduced by the run.
- FC-018, FC-022, FC-023, FC-026, FC-029, FC-036: no rename, literal, identifier, repeated-token, or key/value decision touched.
- FC-024, FC-025, FC-027: `partial`/`readonly`, constructor with initializer, block-bodied member, nesting in class/struct/record/union and namespaces covered.
- FC-028: no caller formats a detached union (`FormatNode`); the document-scoped path is covered by RH5404/RH7101/RH3202.
- FC-030, FC-032: RH3202 on a member of a one-line union leaves the union's braces as the class analog does; fix results that change are listed (changes 6, 7).
- FC-031: see *Comments*.
- FC-033, FC-034, FC-035: tests use the issue's own shapes (U1, U4, U6, U8, empty bodies); expected values were taken from spec examples, change-doc outputs, and unchanged-code class analogs at 4713a0c, not from the patched code.
- FC-037: no text-only claim made.
- FC-038 to FC-043: no shape declared, precheck unchanged.

### Architecture findings
_None new._ The seven-site type-kind list stands as reported by the skeptic and decided in `00-lead.md` decision 2.

### Hints
- `Reihitsu.Core/RegionDirectiveUtilities.cs:148` summary lists "class, struct, interface, record, record struct, or enum" while the code dispatches on `BaseTypeDeclarationSyntax` (already covers unions and extension blocks). Pre-existing, outside the diff; fits follow-up (c)'s list of places that restate the type-kind set.
- `EndOfLineCommentBetweenUnionNameAndCaseTypeListKeepsItsLineBreak` pins `(int, string);` at the declaration's column — the class output today and `main`'s union output, covered by Q3 A; it changes together with follow-up candidate (b) if that one is ever decided.
- `00-lead.md` *Follow-up candidates* still reads "(none yet)" while its *Log* lists (a)–(d); the issue manager reads that file.

## thoenissen/Reihitsu#1763

### Scope
Head `0c7c198`, base `1d2d29a`; full audit.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

Against the specs: FMT-SPACE-028 holds for its example (spaces around and inside, named argument, `with ()`, comment in the gap) and for probes beyond the tests: a `with` element inside a `with` expression's initializer (`x with { A = [with (4)] }` → `[with(4)]`, the expression's `with` keeps one space each side), an argument (`M2([with (4), .. xs])`), nested collection expressions (`[[with (2)], [with ( 3 )]]` → `[[with(2)], [with(3)]]`), `with( )` → `with()`, a multi-line argument list; all stable on a second pass and under CRLF. FMT-SPACE-002's order lists FMT-SPACE-028 in the `with` slot, matching `SpacingPolicy`. FMT-BREAK-022's `with(…)` example matches the code. Both "Done when" items of the issue are met.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Spaces between `with` and `(` | Left token `WithKeyword` with parent `WithElementSyntax`; the phase calls rules only for tokens on the same line | FMT-SPACE-025 `IsWithKeyword` (parent `WithExpressionSyntax`); RH3002's `WithKeyword` case (parentheses, unrelated); no spacing analyzer decides the pair (skeptic: RH6001/RH6006 report neither form) | Disjoint by parent; the right token of a `with` element's keyword is always its argument list's `(`, or a comment handled by FMT-SPACE-013/014 | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Spacing around `with` | `rg "WithKeyword\|WithElement"` in Analyzer, CodeFixes, Core, Formatter: `WithSpacingRule` (only spacing owner); `ObjectInitializerContributor` (indentation of a `with` expression's initializer), `AccessorExpressionBodyTransform` (Roslyn `WithKeyword(...)` method), RH3002 (parentheses) | One owner | ok |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `WithElementSpacingTests.*` FMT-SPACE-028 (7) | Exact `with(`/`with()` output, comment kept with the space behind it removed, already-tight form unchanged | Any space left or added, comment moved | `AssertRuleResult` (LF+CRLF, second pass, phase stability); default parse options are C# 15 (probe), so no syntax-error bypass in the helper | ok |
| `WithElementSpacingTests.Wrapped*` FMT-BREAK-022 (2) | One element per line with the `with` element first | Different layout | `AssertRuleResult` | ok |
| `RH5305…VerifyFixRemovesTheSpaceInFrontOfTheArgumentListOfAWithElement` | Change 3 of the change doc | `with (4)` kept | `Verify` (LF) | ok (CRLF arm identical under `apply-fix.sh`) |

### Visible output is decided
Every new expected value is FMT-SPACE-028's example or change 1–3 of `with-element-spacing.md`, covered by the maintainer's answer Q4 A; the FMT-BREAK-022 example is `main`'s output unchanged.

### Cost of special cases
_None._ One predicate on the token's parent; no shape to declare.

### Comments
`WithSpacingRule` class summary and the new method's summary describe the new behavior.

### Failure classes dismissed
- FC-001, FC-002, FC-003: the predicate is keyed on the parent node; both sides (`with` expression vs. element; spaced vs. tight) are tested.
- FC-004, FC-019, FC-021: comment in the gap tested; a directive needs a line break in the gap, which is no same-line gap (FMT-SPACE-001).
- FC-010, FC-012, FC-017: stable under a second pass and CRLF; no other phase reinserts the gap.
- FC-013: no analyzer reports either spelling (skeptic's runs).
- FC-030, FC-032: the RH5305 result change is listed (change 3) and tested.
- FC-005, FC-006, FC-007 to FC-009, FC-011, FC-014 to FC-016, FC-018, FC-020, FC-022 to FC-029, FC-031, FC-033 to FC-043: no guard over directives, no rewrite of semantics, no new code fix, no sibling copy of the rule, no literal/identifier/rename/detached-node/precheck involvement.

### Architecture findings
_None._

### Hints
- `[with⏎    (4)]` (and `Foo⏎    (4)`): the CLI places `(4)` at the statement's column, the RH5305 fix at the `]`'s column — a CLI/code-fix column disagreement for an argument list on the line after its name. Not reached by this change (the spacing phase only decides same-line gaps; established by tracing, not by a `main` run); it belongs with follow-up candidate (b) of `01-spec-author.md`, which should name both outputs.

## Whole run
- Lane check: `LANE CHECK: confirmed — claimed behavior-change, diff needs behavior-change`.
- Scope: no edit outside the two issues; production diff limited to the six formatter sites and comment updates in `ReihitsuFormatter.cs`.
- Cleaner's Release build (0 warnings) and the roles' focused test results trusted; no repository test re-run.
