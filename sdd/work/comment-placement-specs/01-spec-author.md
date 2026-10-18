Role: spec author
Commit: f073e8e
Verdict: spec written

No change doc: the run specifies existing behavior only. The prompt carried no intended fix or verdict.

## Changed requirements

New shared spec `sdd/specs/shared/comment-placement.md` (prefix SHARED-COMMENT):

- SHARED-COMMENT-001 — Only ordinary comments are checked — added
- SHARED-COMMENT-002 — A comment inside a preprocessor directive is not checked — added
- SHARED-COMMENT-003 — Each comment is reported on its own — added
- SHARED-COMMENT-004 — A comment inside a region is reported — added
- SHARED-COMMENT-005 — A gap at a region boundary or before an element belongs to the region — added
- SHARED-COMMENT-006 — A gap that a nested construct lays out belongs to that construct — added
- SHARED-COMMENT-007 — What counts as a binary operator — added
- SHARED-COMMENT-008 — Inside a region, a comment before a binary operator stays valid — added
- SHARED-COMMENT-009 — Inside a region, a comment before a later call of a chain stays valid — added (carries a coverage clause that keeps the open question below out of 004 and 009)

New rule specs `sdd/specs/analyzer/RH####.md` (all: Layout, Warning, enabled, Code fix None, owner = the rule's analyzer only):

- RH5033-001 — A comment after an attribute list of a declaration is reported; RH5033-002 — Assembly and module attributes are not checked
- RH5034-001 — A comment after the documentation comment of a declaration is reported; RH5034-002 — Only type, delegate, and member declarations are checked
- RH5035-001 — A comment inside a parameter list is reported; RH5035-002 — A comment between a declaration and its parameter list is reported
- RH5036-001 — A comment between a statement header and its body is reported; RH5036-002 — Other statements with a body are not checked
- RH5037-001 — A comment before a continuation keyword is reported
- RH5038-001 — A comment inside an argument list is reported
- RH5039-001 — A comment inside a statement header is reported
- RH5040-001 — A comment around the arrow of an expression-bodied member is reported; RH5040-002 — Other arrows are not checked
- RH5041-001 — A comment next to a declarator separator is reported; RH5041-002 — A comment before the end of a declaration is reported
- RH5042-001 — A comment around an assignment operator is reported; RH5042-002 — The `=` of a name is not checked
- RH5043-001, RH5044-001, RH5045-001, RH5046-001 — a comment inside the base list / attribute list / next to a conditional operator / inside a type parameter, type argument, or tuple type list is reported
- RH5047-001 — A comment inside an auto-property accessor list is reported; RH5047-002 — A comment before an auto-property accessor list is reported
- RH5049-001 — A comment after a binary operator is reported

RH5048 is not specified. `sdd/specs/README.md` has 17 new rows; `sdd/specs/requirement-ids.txt` has the 33 new IDs at their sorted positions (insertions only). No rule page changed: the pages under `documentation/rules/` already agree with the specs.

Code owners of the shared spec: `CommentPositionAnalyzerBase` and `CommentRegionAnalyzerBase` (as the maintainer named), plus `CommentPositionUtilities` and `FluentChainAnalysisHelper`. I added the last two because the rulebook gives a type to the spec whose behavior it implements: `CommentPositionUtilities` implements 001, 004–008 and the statement-header list, and `FluentChainAnalysisHelper` implements the later-link test of 009. Outside its own file, each is called only by these analyzers and RH5048. This goes beyond what the maintainer named; drop the two if the maintainer wants the shorter list.

How the statements were established: I read the analyzers, both base classes, `CommentPositionUtilities`, `FluentChainAnalysisHelper`, `SyntaxIndentationUtilities.IsIndentingScope`, and all 16 test classes. Every example block was then checked by a throwaway file-based app (`/tmp/claude-0/-home-user-Reihitsu/94033b8c-43c8-5bb8-9963-d2b62ecb37ee/scratchpad/probe/probe.cs`, driven by `/tmp/claude-0/-home-user-Reihitsu/94033b8c-43c8-5bb8-9963-d2b62ecb37ee/scratchpad/tools/verify.py`). The app strips the markup and runs every RH5033–RH5049 analyzer, and the script compares the reported comment texts with the `{|RH####:…|}` markup for the rule the block belongs to. All 39 blocks match, and all compile without errors (only unused-member warnings). The prose under SHARED-COMMENT-001 about `DocumentationMode.None` was checked the same way: only `/**/` and `//// note` are reported. `SpecConsistencyTests` and `SpecStructureTests` pass (26/26). `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` fails on exactly the 33 new IDs, because no test is tagged yet.

## Examples for the tests

Every new requirement has an example. Facts from reading the existing tests, so tags and new tests can be placed (an existing test covers a requirement only where its input matches the requirement's case):

- Existing tests cover most of the cases: the same-line, before/after, and per-construct tests of each class; the documentation-mode tests in RH5033 and RH5034 (SHARED-COMMENT-001); `VerifyNoDiagnosticForCommentInsideDirective` (SHARED-COMMENT-002); the nested-construct, pattern, query, and chain tests in RH5038 and RH5039 (SHARED-COMMENT-005, 006, 007, 008, 009).
- Cases in the examples that no existing test has: two comments in one gap (SHARED-COMMENT-003); a comment before a separator after an element that starts with an anonymous object (SHARED-COMMENT-005); `items ??` before `[1]` versus inside `[`…`]` (SHARED-COMMENT-006); `as` and `<<` (SHARED-COMMENT-007); `!.Trim()` as a later call (SHARED-COMMENT-009); RH5033 type parameter, accessor, enum member, local function, and lambda attribute lists (named in the requirement, verified by probe, not in the example); RH5034-002 namespace and local function; RH5035 `static` lambda modifier and indexer `this`/`[`; RH5036-002 `switch`; RH5037 `else if`; RH5038 constructor-initializer and primary-constructor base arguments, array size; RH5039 `catch` and `do`-`while` headers; RH5040 `Sum` with a comment before `+`; RH5041 `for` initializer, event field terminator on the same line, `const` local, `return` terminator; RH5042 object-initializer member, enum member, `_values = []`; RH5043 `D<T>` around-the-list comments; RH5044 same-line `/* obsolete */` and attribute `+`; RH5045 `?` and `:` on one line; RH5047 `Computed`/event accessor negatives and the interface indexer; RH5049 `as`.

## Open questions to file

Belongs to a new issue `SDD open questions: sdd/specs/shared/comment-placement.md` (a sub-issue of thoenissen/Reihitsu#912; none exists yet, because the spec is new). Touches SHARED-COMMENT-004 and SHARED-COMMENT-009; the cases are left out of both through the coverage clause in SHARED-COMMENT-009.

Question: inside a region (RH5035, RH5038, RH5039, RH5043, RH5044, RH5046, RH5047), which comments before a link of a member-access chain stay valid? The rule pages and the `CommentRegionAnalyzerBase` summary say "before a later call of a method chain". The analyzer instead counts every conditional-access `?` as a link, whether or not a call follows it, and it does not recognize the `!` of a `!?.` link. Input, inside `object Use(object v) => v; void M(dynamic a)`, each as an argument of `Use(…)` with the comment on its own line before the last link:

- `a.B()` / `// c` / `?.Name` — not reported. `.Name` is not a call, and the same input with `.Name` is reported (`VerifyCommentBeforeMemberAccessThatIsNotCalledIsReported`).
- `a.B()` / `// c` / `?[0]` — not reported, though an element access is not a call; with `[0]` it is reported (`VerifyCommentBeforeElementAccessInChainIsReported`). Pinned by `VerifyNoDiagnosticForCommentBeforeConditionalElementAccessInArgument`.
- `a?.P` / `// c` / `?.Q` — not reported, though the chain has no call at all.
- `a?.P` / `// c` / `?.Q()`, `a?.P` / `// c` / `.Q()`, and `a?[0]` / `// c` / `?.Q()` — not reported, though `Q()` is the chain's first call. The same input without a conditional access in front (`a.P` / `// c` / `?.Q()`) is reported.
- `a.B()` / `// c` / `!?.C()` — reported, though `C()` is a later call; `!.C()` and `?.C()` in the same position are not reported. Pinned by `VerifyCommentBeforeNullForgivingConditionalLinkInArgumentIsReported`.

Why it looks wrong: the exemption is meant for a comment that describes the later call after it. Under SHARED-CHAIN-001, `!?.` is a link operator, a conditional element access is an attached part and not a link, and the prefix ends at the first invoked link. The comment analyzers follow a different model from the one the chain rules use. Competing readings: (a) the exemption follows SHARED-CHAIN-001, so a comment before a later invoked link (including `!?.`) is valid and every other position is reported, which changes all six inputs; (b) the current behavior is intended and gets specified as it is. Recommendation: (a), so the comment rules and the chain rules agree on what a link is; the two pinned tests would then assert the opposite.

## Follow-up candidates

None.
