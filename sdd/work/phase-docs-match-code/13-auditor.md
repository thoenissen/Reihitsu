Role: auditor
Commit: 6543206
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore. It stated that the "no lane" lane-check result comes from the comment-only remarks edit and was accepted under escalation 7; I checked that against `00-lead.md` and the decisions entry rather than taking it from the prompt, and it is not a finding.

## Scope

Repair delta against eb6470e (my first audit covered 7eec426; eb6470e only adds `08-auditor.md`). Head 6543206 is checked out. `origin/main` is 3da05b708385a227ad7b833d39982442ae286ac6 per the prompt's `git ls-remote`, and it is an ancestor of HEAD. `git diff eb6470e...HEAD` holds what the handovers `09`–`12` and `00-lead.md` describe:

- `sdd/specs/formatter/pipeline.md`: the FMT-PIPE-008 `FormatNode` bullet, a new descriptive list of which tree Roslyn reports, and the reworded last example paragraph;
- `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterLanguageVersionTests.cs`: three summaries reworded, ten new `[Requirement("FMT-PIPE-008")]` tests, the private helper `AssertFormatNodeOfDerivedNode`, and `using System;`;
- `src/Reihitsu.Formatter/ReihitsuFormatter.cs`: two lines of the `FormatNode` `<remarks>` (commit 4c06e3b, the only commit in the range that touches the file);
- the escalation-7 decisions entry and the run's `sdd/work/` handovers.

The repair did not grow into another surface and did not change a requirement's meaning: FMT-PIPE-008's normative clause still names `node.SyntaxTree.Options`, which is what escalation 4 decided. So this is a delta audit, not a full one. I trusted the cleaner's Release build and the focused test results in `10-test-implementer.md` and `12-spec-checker.md` and re-ran no repository test.

Evidence I gathered myself:

- `scripts/verify-text-only.sh --base 02420ce --head 4c06e3b`: `TEXT-ONLY PROOF: PASS — 2 changed file(s) (1 non-compiled, 1 C# comment/layout-only); xml-doc impact: 1 file(s), including public API documentation`. `ReihitsuFormatter.cs`: 1446 tokens identical, directives and disabled text identical, changed trivia: documentation. This is the proof of FC-037 for the one production edit.
- A Roslyn probe: a scratch console project outside the repository, `Microsoft.CodeAnalysis.CSharp` 5.9.0 (the version the repository pins). It prints `((CSharpParseOptions)node.SyntaxTree.Options).SpecifiedLanguageVersion`, which is exactly what `ReihitsuFormatter.FormatNode` passes to `LanguageVersionResolver.Resolve` (`ReihitsuFormatter.cs:127`). Input: `public class Empty { void M() { } }` parsed as C# 11, so that the token-list and node-list edits have a target. Results, against the spec's list:
  - "Edit that produces a new root" keeps C# 11, for every named member and its spelled-out variants: `ReplaceTrivia`, `ReplaceNodes`, `ReplaceTokens`, `ReplaceSyntax`, `RemoveNode`, `RemoveNodes`, `InsertNodesBefore/After`, `InsertTokensBefore/After`, `InsertTriviaBefore/After`, `WithLeadingTrivia`, `WithTrailingTrivia`, `WithoutTrivia`, `NormalizeWhitespace`. A node inside the result (`.Members[0]` of `NormalizeWhitespace` and of `ReplaceToken`) also reports C# 11, which matches "and so do the nodes inside it".
  - "Default parse options" holds: `WithIdentifier`, `AddModifiers`, `AddMembers`, `WithMembers` with a changed list, `WithAdditionalAnnotations`. A node inside a `WithIdentifier` result also reports the default, which matches "every node inside such a node". A later edit (`ReplaceToken`, `NormalizeWhitespace`, `WithLeadingTrivia`) of a `WithIdentifier` result keeps the default, which matches "a later edit of the previous kind keeps the default parse options". `ParseCompilationUnit` without options, and a `ReplaceToken` of it, also report the default.
  - "`Parse…` with parse options" holds: `ParseExpression`, `ParseStatement`, `ParseTypeName`, and `ParseMemberDeclaration` with C# 11 options; a node inside the parsed member and a `WithLeadingTrivia` of it. `CSharpSyntaxTree.Create(node, C# 11)`, including a node inside its root, also holds.

  No statement of the new list is contradicted by a probed shape.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 (a) FMT-PIPE-008 `FormatNode` wording | closed | The bullet now states only the normative source, the reported `SyntaxTree`'s options. The list assigns every probed shape of the first audit to one side, and the derived-node split it names is the one the probe shows. The phrase "without a tree of its own" is gone. The example paragraph names one shape on each side of every case. |
| 1 (b) `ReihitsuFormatter.FormatNode` `<remarks>` | closed | The remarks now read "of the tree that Roslyn reports as the node's syntax tree, whether or not the node has a parent; a node that reports a tree with the default parse options is formatted for the newest supported version". That is true for every probed shape. The edit is proven text-only (above). |
| 1 (c) test summary of `FormatNodeFormatsDetachedNodeForTheNewestSupportedVersion` | closed | The summary now names its input ("a node parsed without parse options"). The two sibling summaries that said "which has no tree of its own" were corrected as well. |

I searched for further members of finding #1's class, meaning other statements of which parse options `FormatNode` uses. I ran `rg -i "detached|tree of its own|default parse options"` over `src`, `sdd/specs`, `documentation`, `AGENTS.md`, and `squad/decisions`, and `rg FormatNode` over every Markdown file filtered for version and parse-option wording. Every remaining hit is about something else: FMT-PIPE-012 and the `<remarks>` sentence about guards, FMT-CLEAN-008 about trailing line breaks, `AGENTS.md:141` about which entry point a code fix uses, the `indentLevel` parameter documentation, and the decisions entries that quote the old text as history. No new member fails, so nothing repeats finding #1's class. The test method name is in *Hints*.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

## Guard scope

_N/A — the repair adds or changes no predicate, guard, or exemption in code. The one decision it describes, the `FormatNode` version source, is read in one place (`ReihitsuFormatter.cs:127`, `node.SyntaxTree.Options`), and the spec bullet, the list, the remarks, and the test summaries now state the same span: the reported tree's options, parent or not._

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Language-version resolution | `LanguageVersionResolver.Resolve`, called from the five entry points of `ReihitsuFormatter` (`node.SyntaxTree.Options` at `:127`, `document.Project.ParseOptions` at `:190` and `:285`, and the tree options of the document and tree entry points); unchanged by the repair | one owner |
| Statements of the `FormatNode` version source | the FMT-PIPE-008 bullet, the list, and the example paragraph; the `FormatNode` `<remarks>`; the summaries in `ReihitsuFormatterLanguageVersionTests` | they now agree with each other and with the code; none of them decides anything |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 5 "keeps" derived-node tests (`…ReturnedByWithoutTrivia`, `…ReturnedByReplaceToken`, `…ReturnedByNormalizeWhitespace`, `…RootReturnedByReplaceNode`, `…ReturnedByUnchangingWithIdentifier`) | a node derived by a new-root edit, or by a no-op `With…`, is formatted for its source tree's C# 11 | `FormatNode` overrides or ignores the reported tree's options, for example by forcing the newest version for nodes without a parent: the class would become `class Empty;` | `AssertFormatNodeOfDerivedNode`. Under LF and CRLF it first asserts that the derived node's text equals the input, so only the version can make a difference, then asserts the exact output. The no-op test also asserts `AreSame` | adequate. The "default" tests drive the same helper and the same formatting path to the opposite result, so a "keeps" test cannot pass because formatting silently did nothing |
| 4 "default" derived-node tests (`…ChangedByWithIdentifier…`, `…ChangedByWithAdditionalAnnotations…`, `…CompilationUnitBuiltAroundParsedClass…`, `…EditOfChangedNode…`) | a node that reports default options is formatted for the newest version | the formatter reads the version from anywhere other than the reported tree, for example a cached original tree: the class would stay braced | same helper, exact `class Empty;` under LF and CRLF | adequate. `ReplaceNode` and `CompilationUnit().AddMembers` each put a changed class into a compilation unit and get opposite results, which pins the split from both sides |
| `FormatNodeFollowsTheParseOptionsOfRootCreatedWithCSharpSyntaxTreeCreate` | the root of a tree created with options follows those options | the C# 11 root converts | exact text under LF and CRLF, for C# 11 and C# 12 | adequate. The C# 12 side alone cannot tell C# 12 from the default, but the C# 11 side can |
| 3 summary-only edits | unchanged | – | unchanged | unchanged from the first audit |

The test implementer left out a second `FormatNode` pass and the CRLF case of the parsed-without-options test, and gave the reasons in `10-test-implementer.md`. FMT-PIPE-008 states neither, so leaving them out does not make an assertion weaker than the guarantee.

## Failure classes dismissed

- FC-001, FC-003, FC-015: the repair changes no guard, predicate, or policy in code.
- FC-002, FC-016, FC-031: these reached the change in the first audit as finding #1. Every member has been rechecked and is closed, and the class search above found no new member.
- FC-004, FC-005, FC-019 to FC-021: no gap, join, or trivia code changed. The repair touches no switch or spacing test.
- FC-006, FC-022 to FC-027, FC-029, FC-036: no rewrite was added or changed. FC-006's language-version gate is the subject of the tests, which describe existing output.
- FC-007 to FC-009, FC-018, FC-030: no code fix was touched. The only production caller of `FormatNode`, RH5401 `CanCollapse`, passes a node inside a tree, which is the first case of the list.
- FC-010 to FC-012: there is no formatter change. A second pass is not part of FMT-PIPE-008 (see *Assertion adequacy*).
- FC-013, FC-014: no analyzer surface is touched.
- FC-028: the repair concerns `FormatNode` only. The node-in-document entry points keep the project's options, as the spec states separately, and no test asserts that the two families agree.
- FC-032, FC-035: no compiled production change. The expected values come from the spec author's list and examples, which are taken from Roslyn probes against the unchanged formatter. The test implementer did not write them from a patched copy.
- FC-033: `AssertFormatNodeOfDerivedNode` runs under LF and CRLF and asserts its input text, so the harness normalizes nothing away.
- FC-034: the tests use the spec's own empty-class input.
- FC-037: the comment-only claim for `ReihitsuFormatter.cs` is proven by `scripts/verify-text-only.sh` (above), not by `grep`.
- FC-017: covered in *Assertion adequacy*.

## Architecture findings

_None._ The first audit considered whether the restated version source is "one rule stated in more than one place". It concluded that the code is the only owner and that public XML remarks restate behavior by convention. That holds after the repair, and no member of the class recurred.

## Hints

- The test method name `FormatNodeFormatsDetachedNodeForTheNewestSupportedVersion` predates the run on `main`, and the repair kept it. Read as a general claim, "a detached node is formatted for the newest version" is the clause narrowed under escalation 4. Read as a label of its own scenario, it is accurate, and its corrected summary qualifies the scenario. I do not count it as a member of finding #1, for two reasons: test names in this class label their own input, as `FormatNodeKeepsEmptyClassBracedForCSharp11Tree` does, and the sibling `FormatNodeKeepsParseOptionsOfDetachedNodeParsedWithOlderOptions` shows that "detached" alone does not decide the version. A rename such as `FormatNodeFormatsNodeParsedWithoutParseOptionsForTheNewestSupportedVersion` would remove the ambiguity, if the lead wants it in a later run.
- The FMT-PIPE-008 list describes Roslyn behavior that the formatter does not own, and it names its members without claiming to be complete. Shapes it does not name follow the same split in the probe: `WithoutLeadingTrivia`, `WithoutTrailingTrivia`, and `WithTriviaFrom` keep the options; `WithoutAnnotations` and `WithMembers` with a changed list get the default. A `Microsoft.CodeAnalysis.CSharp` upgrade that moved a shape across the split would fail one of the ten pinned tests only if that shape is one of the pinned ones.
- The skeptic's follow-up candidate from the first audit still applies: an architecture test that resolves the identifiers in the ownership table's guards column.
