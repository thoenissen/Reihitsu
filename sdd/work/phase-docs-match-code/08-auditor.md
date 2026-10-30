Role: auditor
Commit: 7eec426
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore.

## Scope

Full audit, first audit of the run. Head 7eec426 is checked out. `git ls-remote origin refs/heads/main` gives 3da05b708385a227ad7b833d39982442ae286ac6, which is an ancestor of HEAD. `git diff 3da05b7...HEAD` holds what the handovers describe: `AGENTS.md`, three formatter specs, the decisions entry, three formatter test files, and `sdd/work/`. `scripts/lane-check.sh tooling` confirms the tooling lane. No production file changed. I trusted the cleaner's Release build and the focused test results in `05-test-implementer.md` and `06-cleaner.md`, and re-ran no repository test.

I checked every item of thoenissen/Reihitsu#1484 against the tree myself:

- Every guard named in the ownership table exists under that name and in the type the table gives:
  - `ExpressionBodiedTransformUtilities.RequiresExpressionBodyPreservation`, `FieldDeclarationSplitTransform.CarriesDirective`, `AccessorExpressionBodyTransform.ContainsBlockingDirective`;
  - `DocumentationCommentUtilities.IsWrittenBehindSource`, which `DocumentationCommentFormattingPhase` calls at line 165;
  - `HasCrossSectionLocalUse`, which `SwitchCaseBraceRewriter` ORs with `HasCrossSectionGotoTarget` at lines 946–947.
- `CanRewrite` and `IsAfterSourceOnSameLine` have no hit outside `binaries/` and `sdd/work/`.
- The spacing-row sentence matches FMT-SPACE-002 and `HorizontalSpacingRewriter`: when `GetDesiredSpacesAfter` returns `null`, only `CollapseMultipleTrailingSpaces` runs.
- `line-breaks.md` names no phase number.
- FMT-SWITCH-003, FMT-PIPE-005, FMT-PIPE-006, and FMT-PIPE-008 carry the clarified wording, and their tests carry the tags.

I measured the language-version claims with a scratch probe. It is a console project outside the repository that references the built Release `Reihitsu.Formatter.dll` and Microsoft.CodeAnalysis.CSharp 5.9.0. The input is `class Empty\n{\n}`, and each node is passed to `ReihitsuFormatter.FormatNode` with no `indentLevel`.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | (a) `sdd/specs/formatter/pipeline.md`, FMT-PIPE-008: the second sentence of the `FormatNode` bullet and the last example paragraph; (b) `src/Reihitsu.Formatter/ReihitsuFormatter.cs:115-117`, the public `<remarks>` of `FormatNode`; (c) `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterLanguageVersionTests.cs:427-428`, the summary of `FormatNodeFormatsDetachedNodeForTheNewestSupportedVersion` | FC-002, FC-016, FC-031 | – | same requirement: (a) is PR-introduced wording; (b) and (c) are pre-existing statements of the same decision that escalation 4 showed to be wrong | Four statements say which parse options a node without a parent carries, and none matches the code for every shape (members below). **(a)** The narrowed text splits nodes into "taken from a tree, or parsed with parse options" (keep their options) and "parsed or built without parse options" (default options). It never says where a node *derived* from such a node belongs, and both readings fail on a probed input. If derived nodes count as "taken from a tree", these get the C# 14 result (`class Empty;`) where the spec predicts the C# 11 result (stays braced): `cls.WithIdentifier(...)`, `cls.AddModifiers(...)`, `cls.WithAdditionalAnnotations(...)` on a class from a C# 11 tree, and `SyntaxFactory.ParseMemberDeclaration(src, options: C# 11).WithIdentifier(...)`. If derived nodes count as "built without parse options", these keep C# 11 where the spec predicts C# 14: `root.ReplaceNode(...)`, `cls.ReplaceToken(...)`, `cls.WithoutTrivia()`, `cls.NormalizeWhitespace()`. The observed split runs between Roslyn's `Replace*`-family edits and the generated `With*`/`Add*` methods, and the text names neither. The phrase "without a tree of its own" is also inexact: the parsed node's `SyntaxTree` carries the options, which is the very mechanism. **(b)** Public API documentation still says "a detached node carries the default parse options and is formatted for the newest supported version". That is the clause the maintainer narrowed under escalation 4. The run's own test `FormatNodeKeepsParseOptionsOfDetachedNodeParsedWithOlderOptions` falsifies it, and so do every `Replace*`-family member above. **(c)** The test summary says the same thing for its input, which is parsed without options. | Spec owner: state the rule through the node's `SyntaxTree` options, which is the normative first clause and holds for every probed shape. Then either name the derived-node behavior with an example on each side, or say that a node changed after parsing carries whatever options Roslyn gives its new tree, and pin one derived shape in a test. Comment owner: align (b) and (c) with the clause. (b) is a comment-only edit in a production file and can be proven with `scripts/verify-text-only.sh`. The issue's **Done when** says "No production file changes". Whether a proven text-only edit there fits that line, or needs the maintainer, is for the lead to decide. |

Members of the class, meaning the shape of the node passed to `FormatNode`:

- **Hold under every reading:**
  - a node inside a C# 11 tree → C# 11;
  - `ParseCompilationUnit(src, options: C# 11)` → C# 11;
  - `ParseMemberDeclaration(src, options: C# 11)` → C# 11;
  - `ParseCompilationUnit(src)` → C# 14;
  - a `SyntaxFactory`-built compilation unit → C# 14.
- **Derived nodes that keep C# 11:** `root.ReplaceNode`, `cls.ReplaceToken`, `cls.WithoutTrivia`, `cls.NormalizeWhitespace`.
- **Derived nodes that get C# 14:** `cls.WithIdentifier`, `cls.AddModifiers`, `cls.WithAdditionalAnnotations`, `ParseMemberDeclaration(C# 11).WithIdentifier`.

Statements that hold:

- the `FormatDocumentAsync` summary (`ReihitsuFormatter.cs:33`);
- the `FormatSyntaxTree` summary (`:69`);
- the remarks of both node-in-document entry points;
- every summary of a test the run added.

The only production caller of `FormatNode` is `RH5401…CodeFixProvider.CanCollapse`, and it passes a node that is inside a tree. No shipped behavior is affected; the defect is the text.

This is not `pass, text cleanup`, because (b) is public API documentation and (a) is the requirement text of FMT-PIPE-008.

## Guard scope

No predicate or guard in code is added or changed. The rows check the decisions that the clarified requirements describe against the code that makes them.

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| FMT-SWITCH-003: is a section multi-line? | `SwitchCaseBraceRewriter.IsMultiLineSection` and `HasMultiLineBodyEndingInReturnOrThrow`, using `SyntaxNodeUtilities.IsSingleLine` on the tree that `LineBreakPhase` (position 7) hands to `SwitchCaseBracePhase` (position 8) | FMT-SWITCH-008 (existing block, also measured after FMT-BREAK) | Same tree, same measurement. Joins (FMT-BREAK-015, -016), keeps (FMT-BREAK-012, chain), and adds (FMT-BREAK-021, lambda block, statement split) are each tested on both sides | holds |
| FMT-PIPE-005/006: node-in-document guards | `HasSyntaxErrors(syntaxTree)` and `IsAutoGeneratedSource(root)` on the whole document tree, never only on the target. `FormatNodeInDocumentWithContextAsync` with target == context delegates; otherwise it checks after its tree-membership early exit | the same helpers in `FormatDocumentAsync` and `FormatSyntaxTree` | Same helpers and same span, so "whichever node is the target or the context" holds. The early exit returns the document too and does not contradict the requirement | holds |
| FMT-PIPE-008: version source per entry point | `syntaxTree.Options` (document, tree); `node.SyntaxTree.Options` (`FormatNode`); `document.Project.ParseOptions` (both node-in-document entry points) | the spec bullets | Bullets 1 and 3 match. Bullet 2's first clause matches; its elaboration does not cover derived nodes | finding #1 |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Language-version resolution | `LanguageVersionResolver.Resolve`, called from all five entry points of `ReihitsuFormatter` | one owner |
| Syntax-error and generated-code guards | `ReihitsuFormatterHelpers.HasSyntaxErrors` and `IsAutoGeneratedSource`, called from four entry points | one owner |
| Which documentation comments document following code | `DocumentationCommentUtilities.DocumentsFollowingCode` (Core); `ReihitsuFormatterHelpers.DocumentsFollowingCode` only delegates to it, and its remarks say so | one owner (the table's bare name resolves to both, but they are the same decision) |
| Statements of the `FormatNode` version source | the FMT-PIPE-008 bullet and example, the `FormatNode` `<remarks>`, and test summaries in `ReihitsuFormatterLanguageVersionTests` | statements disagree; see finding #1 and *Architecture findings* |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 7 new `SwitchCaseBraceRequirementTests` (FMT-SWITCH-003) | exact output, with braces present or absent as the requirement decides | a section braced (or unbraced) differently, a moved comment, an unstable second pass, a phase that reverts the output, CRLF drift | `AssertRuleResult`: exact text under LF and CRLF, a second pass, `PhaseStabilityCheck` | adequate |
| 5 FMT-PIPE-005 and 3 FMT-PIPE-006 "returns unchanged" tests | the guard returns the input document | any formatting path runs, since `WithSyntaxRoot` always yields a new instance | `Assert.AreSame(document, result)` under LF and CRLF | adequate. Target == context covers the delegation path; target ≠ context covers the guard itself, because `C` contains `M` and the early exit is not taken |
| 2 FMT-PIPE-006 holding-side tests | a marker outside the file header does not disable formatting | the document comes back unformatted | exact document text under LF and CRLF | adequate |
| FMT-PIPE-008 document and tree tests (C# 5/6 property; C# 12 empty class) | the version comes from the project or tree options | a C# 5 project converts the property, or a C# 12 project keeps braces | exact text under LF and CRLF; `AssertFormatSyntaxTree` adds a second pass | adequate. Tree versus project cannot be told apart in an `AdhocWorkspace` (FC-033, stated by the skeptic) |
| FMT-PIPE-008 node-level tests (empty class through `FormatNodeInDocument*`, `FormatNode` from C# 11 and C# 12 trees) | the version comes from the project, or from the node's tree | the wrong version decides | exact text under LF and CRLF, both sides of each version | adequate |
| `FormatNodeKeepsParseOptionsOfDetachedNodeParsedWithOlderOptions`, `FormatNodeFollowsTheParseOptionsOfDetachedMemberDeclaration`, `FormatNodeFormatsNodeBuiltWithSyntaxFactoryForTheNewestSupportedVersion` | the narrowed `FormatNode` clause, by node shape | the formatter ignores or overrides the node's options | exact text under LF and CRLF. The built-node test first asserts that its input equals the spec input | adequate for the shapes they pin. No test pins a derived node (finding #1) |
| 3 existing tests newly tagged FMT-PIPE-008 | unchanged | – | unchanged | adequate |

## Failure classes dismissed

- FC-001, FC-003, FC-015: no guard, predicate, or policy changed in code. The decisions the clarified specs describe were traced above, and their tests take both sides.
- FC-004, FC-005, FC-019, FC-020, FC-021: no gap, join, or trivia code changed. The FMT-SWITCH-003 statement-split test keeps a `/* c */` comment in the split gap. A directive in a section switches to FMT-SWITCH-010, as `03-test-implementer.md` says.
- FC-006, FC-022 to FC-027, FC-029, FC-036: no rewrite was added or changed. The tests describe existing output.
- FC-007 to FC-009, FC-018, FC-030: no code fix was touched. The one production `FormatNode` caller (RH5401 `CanCollapse`) passes a node inside a tree, which every reading of FMT-PIPE-008 covers.
- FC-010 to FC-012: the run makes no formatter change. The switch tests run a second pass and `PhaseStabilityCheck` under LF and CRLF.
- FC-013, FC-014: no analyzer surface is touched.
- FC-028: the two families differ by spec, in guards (FMT-PIPE-012) and in version source (FMT-PIPE-008). Each family is pinned separately, and no test asserts that they agree.
- FC-032, FC-035: no compiled change. Expected values come from the spec examples or from `scripts/format.sh` on the unchanged code, which a spec-only run may record.
- FC-033: the skeptic's candidate became a test and drove escalation 4. Tree versus project for `FormatDocumentAsync` cannot be told apart, and the specs say so.
- FC-034: the FMT-SWITCH-003 test uses the issue's own input verbatim.
- FC-037: no comment-only proof was claimed. The C# diff is test code added under the tests path.
- FC-017: covered in *Assertion adequacy*.
- FC-002, FC-016, FC-031: reach the change; see finding #1.

## Architecture findings

_None._ I considered "one rule is stated in more than one place". Finding #1 restates the `FormatNode` version source in the spec, the public `<remarks>`, and a test summary, and those statements have drifted. None of them decides anything: the code (`node.SyntaxTree.Options`) is the only owner of the decision. The `<remarks>` are the XML documentation that the repository requires on public API, so restating behavior there is the convention rather than a second owner. The defect is a stale and imprecise statement, which is an ordinary FC-031/FC-002 finding. If the lead reads "stated in more than one place" to cover public XML remarks, this becomes escalation 12. The places are the three locations in finding #1.

## Hints

- The skeptic's follow-up candidate still applies: an architecture test that resolves every backticked identifier in the ownership table's guards column to a declaration. Nothing checks those names today. `PhaseOwnershipDocumentationTests` checks only the phase order.
- The skeptic noted that no test covers a `#error` directive as an error-level diagnostic for FMT-PIPE-005. That shape is not required by the reworded text, which names no new diagnostic kind.
