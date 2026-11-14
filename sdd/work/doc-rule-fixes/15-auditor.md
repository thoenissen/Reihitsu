Role: auditor
Commit: f6cd7c7b067e91cef86faa9a9635bc75f702f011
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

This audits the repair delta against `31a774b`, the head of the first audit (`10-auditor.md`). Head is `f6cd7c7` (checked out), and `origin/main` is `cf6d26b`, the SHA in the prompt; it is an ancestor of head. The diff is `git diff 31a774b HEAD -- . ':(exclude)sdd/work'`: 5 files. They are the RH8203 analyzer, `XmlDocumentationExpander`, `DocumentationCommentCodeFixUtilities`, the summary in `DocumentationAnalysisUtilities`, and the RH8203 tests. That matches handovers 11–14 and decisions 20, 21, and 23. No spec, change doc, or rule page changed, and the scope did not grow. `scripts/lane-check.sh behavior-change`: confirmed. I trusted the cleaner's Release build and the implementer's analyzer run. To probe, I added scratch tests to a throwaway worktree of head outside the repository. Results: probes A, B, C, E, F, and G pass, and probe D shows the divergence recorded under Guard scope. The worktree has been removed.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | The failing members listed: later comment on a defining or implementing method part, and on property, indexer, and event parts; `<include>` on method, property, indexer, and event parts. All are now reported and tested on both parts and in both directions (`VerifyInheritdocInLaterCommentOfPartial{Method,Property,Indexer,Event}PartIsJudgedPartByPart`, `VerifyIncludedInheritdocOfPartial{Method,Property,Indexer,Event}PartIsJudgedPartByPart`). Members the enumeration did not list also hold: probe B (an `<include>` in a later comment of a part) and probe E (a later `/** */` comment). |
| 2 | closed | The summary of `GetDocumentationCommentsBehindCode` now describes the predicate: a following token that opens its syntax node. |
| 3 | closed | The dead `SingleLineDocumentationCommentTrivia` conjunct is gone. Both callers still guarantee it: RH8305 registers only on `///`, and RH8307 judges only elements whose parent is a `///` comment (`RH8307…Analyzer.cs:154`). The summary and the usings follow. |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 4 | medium | `src/Reihitsu.Analyzer/Rules/Documentation/RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.cs:116` (and `:117` for the `<include>` branch of a part); `RH8203InheritdocMustBeUsedWithInheritingClassAnalyzerTests.cs` | FC-017, FC-003 | – | PR-introduced | Decision 23 added a cref exemption of its own for the syntax branch: `laterInheritdocNode != null && HasCrefAttribute(laterInheritdocNode)`. No test exercises it. The only later-comment and included cref tests (`VerifyIncludedInheritdocWithCrefIsNotReported`, the spec example's `Restart`) use non-partial declarations, which take the other branch. Deleting line 116 would therefore leave every test green, while `/// <summary>…</summary>⏎⏎/// <inheritdoc cref="C"/>` on a partial part would become reported, against RH8203-001 ("an `<inheritdoc>` element with a `cref` attribute MUST NOT be reported"). Handover 11 justified the omission with "the cref exemption is one check for every declaration", which stopped being true once decision 23 split the branches. **Class members** (head, probes): (a) a cref in a later comment of a partial part, syntax branch, no test, holds (probe A); (b) a cref in content an `<include>` brings into a partial part, placeholder-compilation branch, no test, holds (probe C). In (b), the placeholder compilation cannot bind the cref and writes it as `cref="!:C"`, so the exemption depends on the attribute surviving that rewrite. | Add the cref side for both branches of a partial part: a later-comment `<inheritdoc cref="…"/>` and an included one, on both parts, not reported. Both are the other side of the eight new tests' boundary. |
| 5 | low | `RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.cs:83-85` (inline comment) | FC-031 | – | PR-introduced | The comment says the part's own comments "are read from its syntax, and only content an include element brings in is expanded on its own". The code does something different: when any comment of the part holds `<include>`, every comment of the part is compiled and expanded through `GetExpandedDocumentationOfPart`, and later comments are not read from syntax at all (`laterInheritdocNode` stays `null`). | Reword: later comments are read from syntax unless one of the part's comments holds an `<include>` element; then the part's whole documentation is expanded on its own. Text only. |
| 6 | low | `RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.cs:86-89` and `src/Reihitsu.Analyzer/Core/XmlDocumentationExpander.cs:63-66` | FC-015 | – | PR-introduced | The repair writes "the documentation comments of a declaration's leading trivia" twice: once in RH8203, to choose the branch and read later comments, and once in `GetExpandedDocumentationOfPart`, to build the placeholder text. Next to them is a third reader, `DirectDocumentationSyntaxChecker.GetDocumentationComment`, which takes the first of the same set. The three agree today, since all select single-line and multi-line documentation trivia from `GetLeadingTrivia()`, but nothing ties them together. | Add one helper next to `GetDocumentationComment` (all documentation comments of the leading trivia), have both new places use it, and express `GetDocumentationComment` as its first element. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whether a partial part's documentation is expanded or read from syntax | `GetTagsIncludingNested(comment, "include")` over every documentation comment in the part's leading trivia | The compiler's include expansion, which also expands nested `<include>` elements | Same set of triggers. A nested `<include>` also selects expansion. | holds |
| A later comment's `<inheritdoc>` on a partial part (syntax branch) | `GetFirstDirectTag` on comments 2..n (top level only) | Expansion branch and non-partial path: `GetFirstExpandedElement` (top-level elements of the expanded root) | Both look at the top level only. Case sensitivity differs (`OrdinalIgnoreCase` against the syntax tag name), but RH8203-001 covers only lower-case `inheritdoc`. | holds |
| Which comments belong to a partial part, compared with the compiler's view for a non-partial declaration | Every structured documentation trivia in `GetLeadingTrivia()` | The compiler's documentation collection (`GetDocumentationCommentXml`) | Agree when only whitespace separates the comments, and when a directive stands between the last comment and the declaration (probe G: partial and non-partial both reported). Differ when a `#region` separates two documentation comments (probe D: the partial part is reported, the non-partial declaration is not, because the compiler drops the comments before the directive). SHARED-DOC-004's coverage paragraph leaves that input uncovered. | holds (uncovered input) |
| Cref exemption on a partial part | `XmlCrefAttribute` on the syntax node (line 116); `cref` attribute on the expanded element (line 117) | RH8203-001's cref clause | Agree. Probes A and C pass. Neither is tested (#4). | holds; test gap #4 |
| Placeholder compilation resolves `<include>` like the real one | The tree path, parse options, and compilation options (`XmlReferenceResolver`) of the part's tree | The compiler's include expansion for the real tree, which resolves relative to the same `SyntaxTree.FilePath` | Same base path and resolver | holds |
| `CanLayOutElement` without the `///` conjunct | `IsWrittenBehindSource` only | RH8305 registration (`SingleLineDocumentationCommentTrivia`) and RH8307 parent check | Both callers guarantee a `///` comment | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| The documentation comments of a declaration's leading trivia | `rg -n "OfType<DocumentationCommentTriviaSyntax>()" src`: `DirectDocumentationSyntaxChecker.GetDocumentationComment` (first), `RH8203…Analyzer.cs:86-89` (all). `XmlDocumentationExpander.GetExpandedDocumentationOfPart:63-66` filters the same trivia by kind. | Three readers that agree, no shared owner: #6 |
| Expanded documentation of a symbol | `XmlDocumentationExpander.ParseExpandedDocumentation`, the one caller of `GetDocumentationCommentXml`. `GetExpandedDocumentationOfPart` feeds it a placeholder symbol. `CSharpCompilation.Create` appears only there. | One owner |
| Which documentation a part of a partial member has | RH8203 alone: syntax for later comments, a placeholder compilation for `<include>` (decision 23). The other symbol-XML readers stay as they are; this is a listed follow-up. | As decided (decision 23) |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyInheritdocInLaterCommentOfPartial{Method,Property,Indexer,Event}PartIsJudgedPartByPart` | A later comment's `<inheritdoc/>` counts for its own part only, is reported on the name (`this` for the indexer), and is not reported when the member implements an interface member | A missing report on `N`, `O`, or `P`; a report on `M`; a report taken from the other part; a location on the element | `Verify(source)` with exact markup | adequate (both directions `O`/`P`, FC-003, FC-024) |
| `VerifyIncludedInheritdocOfPartial{Method,Property,Indexer,Event}PartIsJudgedPartByPart` | Same, for included content, on the placeholder-compilation branch | Same. `P`'s defining part would read the implementing part's summary through the symbol. | `Verify` + `CreateIncludedDocumentationTransform` | adequate. The cref side of both branches is missing (#4). |

## Failure classes dismissed

- FC-001, FC-002: traced in Guard scope. The repair's per-part span now matches RH8203-001's later-comment and include clauses.
- FC-003: the inherits and inherits-nothing sides and both part directions are tested. The cref boundary is #4.
- FC-004, FC-005, FC-019, FC-020, FC-021: RH8203 has no fix, and the repair edits no trivia. The directive-between-comments shape is probe D, an uncovered input under SHARED-DOC-004.
- FC-006, FC-018, FC-022, FC-023, FC-025, FC-026, FC-029, FC-036: no rewrite and no rename.
- FC-007, FC-008, FC-009, FC-030: no code fix changed behavior. `CanLayOutElement` loses only a conjunct that its callers already guarantee (#3 closed).
- FC-010, FC-011, FC-012, FC-013, FC-014, FC-028: no formatter change and no new rule.
- FC-015: #6.
- FC-016: the per-part reading covers methods, properties, indexers, and events (tests). Other readers of a partial member's expanded documentation (RH8032, RH8033, RH8101, RH8105, RH8108) are a recorded follow-up candidate in `00-lead.md`.
- FC-017: #4.
- FC-024: partial methods, properties, indexers, and events (defining field-like event), both parts. A partial type or constructor is outside RH8203-001's per-part clause, and `IsPartOfPartialMember` excludes types.
- FC-027: an `<include>` nested inside another element still selects expansion (`GetTagsIncludingNested`), and a nested `<inheritdoc>` is not considered on either branch (top-level selection).
- FC-031: #5 (inline comment). The summary and remarks of `GetExpandedDocumentationOfPart` describe the code. The #2 summary is fixed.
- FC-032: the implementer ran the analyzer suite after the last production change (683f6db). The cleaner's later change is text-only, proven by `verify-text-only.sh`.
- FC-033: the include tests use the in-memory resolver. The placeholder passes the real tree's path, so a file resolver resolves against the same base. The documentation mode is that of the tree; mode `None` skips the rule.
- FC-034, FC-035: the expected values come from RH8203-001 and RH8203-002 (handover 11), and the tests failed before the change (handover 12).
- FC-037: the only text-only claim (cleaner, #2) was proven with `scripts/verify-text-only.sh`.
- FC-038 to FC-043: no unsupported shape is declared.

## Visible output is decided

The eight new tests pin RH8203 reports that unchanged RH8203-001 and RH8203-002 demand (later comment, `<include>`, location on the name). The per-part reading is covered by maintainer answer Q4 B and decision 13. The repair touches no rule page. No undecided output.

## Architecture findings

_None._ The detached compilation works around the compiler merging partial-member documentation, which is outside the repository (handover 12). The split between syntax and expansion is decision 23.

## Hints

- Probe D: when a preprocessor directive separates two documentation comments, the compiler keeps only the comments after the directive. A non-partial declaration with `/// <summary>…</summary>⏎⏎/// <inheritdoc/>⏎#region R⏎/// <remarks>…</remarks>` is therefore not reported, while a partial part with the same comments is. SHARED-DOC-004 leaves this input uncovered, and it is rare. No change suggested.
- Each RH8203 analysis of a partial part whose comment holds `<include>` now builds a compilation of one tree with no references (decision 23). The cost is limited to that shape.
