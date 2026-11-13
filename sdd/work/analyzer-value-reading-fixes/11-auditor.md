Role: auditor
Commit: 52531f03c4c70a0c5100465a8ad4bf576f87f6bc
Verdict: #1071: pass; #1268: pass; #974: pass; #1289: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `52531f0` (checked out, clean tree), base `origin/main` = `c2866eaf294a9040b995a2b75de7587bdccc5efe` (fetched; same as the prompt), merged into head. `scripts/lane-check.sh bug-fix`: confirmed. The diff (`c2866ea..52531f0`, without `sdd/work/`) is 5 specs, 6 test files, and 5 production files, which is what handovers 03–09 describe. The cleaner's Release build (e85cfc0) postdates the last compiled change (0eb9d98); only spec text changed after it.

RH2109 probes: a throw-away console project in the scratchpad that references the Release `Reihitsu.Analyzer.dll` from the analyzer test output (built after d3c005c) and calls the private `FindCodeDirectives` by reflection on `.razor` fixtures in the scratchpad. The fixtures for the tests the run already has (two blocks, `@functions` before `@code`, `#if` in a block, `user@code {`) gave the results the tests expect, so the harness reads the shipped code. `main` reports every `@code\s*\{` (one regex over the file), so on `main` each directive below is reported.

## thoenissen/Reihitsu#1071

### Findings
_N/A — none._ RH4001: `GetExpectedFileNameStem` reads `ValueText` for the type name and each type-parameter name. The analyzer comparison, the fix's rename (`GetRenamedFileName`), and the RH4001-006 taken-name check (`RH4001TypeNameShouldMatchFileNameCodeFixProvider.cs:68-69`) all go through it, so the three agree. RH4009: `GetNamespaceName` joins the `ValueText` parts that `NamespaceCasingHelper.GetLocations` yields. The location is still `Name.GetLocation()`, the whole name as written, and entries are compared as written. RH4001-002/-003/-005/-006 and RH4009-002 hold for every example and for the skeptic's matrices (`@` and Unicode escape × type name and type parameter × analyzer and fix; `@` on the first or a later part, spaces, a line break, a comment, an escape × block-scoped and file-scoped).

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH4001 expected stem | type `Identifier` and every `TypeParameter.Identifier`, `ValueText` | analyzer `GetComparableFileName` comparison, fix rename, RH4001-006 check: all call this one helper | same helper, same span | holds |
| RH4009 declared name | `QualifiedNameSyntax`/`IdentifierNameSyntax` parts, `ValueText`, joined with `.` | `NamespaceCasingHelper.GetLocations` (RH4007, RH4008, RH7002) | reused, not copied; an `AliasQualifiedNameSyntax` namespace name only occurs in code that does not compile | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Namespace name by value | `src/Reihitsu.Analyzer/Core/NamespaceCasingHelper.cs` `GetLocations`; RH4009 `GetNamespaceName` only joins its output. `rg -n "Name\.ToString\(\)" src/Reihitsu.Analyzer src/Reihitsu.Analyzer.CodeFixes` finds nothing else | one owner |
| Expected file name of a type | `RH4001TypeNameShouldMatchFileNameHelper` (`GetExpectedFileNameStem`, `GetRenamedFileName`). `rg -n "GetFileNameWithoutExtension"` also finds `PartialMemberUtilities.cs:254` (generated-file suffixes, a different policy) | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH4001 `Verbatim…`/`UnicodeEscaped…MatchesFileNamedAfterItsValue` (5) | a name's value matches the file stem | any diagnostic | `Verify` (exact diagnostic set) | adequate |
| RH4001 `…IsReportedInFileNamedWithTheVerbatimPrefix` (2), `UnicodeEscapedTypeNameIsReportedInFileNamedAfterAnotherType` | other side of the boundary (FC-003), on the name span | missing diagnostic or another span | same | adequate |
| RH4001 `CodeFixRenames…` (4) | renamed document name, content unchanged | another name or changed content | `SingleCodeFixTestsBase` with `FixedState` path and re-analysis | adequate |
| RH4001 `NoFixIsOfferedWhenDocumentWithFileNameOfVerbatimTypeNameValueExists` | fix withheld | an offered fix changes the fixed state | fixed state = test state | adequate |
| RH4009 value tests (9) and `EntryWithVerbatimPrefixMatchesNoNamespace` | compared by value; entry compared as written; location on the whole written name | extra, missing, or moved diagnostic | `Verify`/`VerifyWithAllowList` with `{|RH4009:…|}` spans | adequate |

## thoenissen/Reihitsu#1268

### Findings
_N/A — none._ The pre-filter `objectCreation.Type is not IdentifierNameSyntax && GetSimpleTypeName(...) != "NotImplementedException"` lets every single identifier through to `IsNotImplementedException`. A `using` alias can only be written as a single identifier (`@NIE` and an escaped alias name included, since `GetSimpleTypeName` and the semantic check need no text match). Every qualified (`S.`, `global::System.`) or alias-qualified (`S::`) form keeps `NotImplementedException` as its rightmost identifier, so no alias form is filtered out. RH2003-001/-003/-004 hold.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Syntactic pre-filter of explicit creation | the kind of `ObjectCreationExpressionSyntax.Type`; for a non-identifier type, its rightmost identifier's `ValueText` | `IsNotImplementedException` (semantic) and SHARED-DIAG-002's alias sentence | every input the semantic check accepts passes the pre-filter | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Simple type name pre-filter | `GetSimpleTypeName` only in this analyzer (`rg -n "GetSimpleTypeName" src`) | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyCreationThroughUsingAliasOfTheTypeIsReported`, `…TypeAndNamespaceAliases…`, `…WithArgumentsAndInitializer…`, `…VerbatimAliasName…`, `…AliasDeclaredInNamespace…`, `…GlobalAliasOfAnotherFile…` | alias creation reported on the written type | missing diagnostic or another span | `Verify` with `{|RH2003:…|}` | adequate |
| `VerifyCreationThroughAliasOfAnotherTypeIsNotReported`, `VerifyAliasNamedNotImplementedExceptionOfAnotherTypeIsNotReported`, `…DerivedType…` | other side (FC-003) | extra diagnostic | same | adequate |

## thoenissen/Reihitsu#1289

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Analyzer/Rules/Design/RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer.cs` `FindCodeDirectives` (the `@*` and `<!--` branches) | FC-002, FC-016 | – | PR-introduced | The scanner treats every `@*` and every `<!--` outside a lexed `@code`/`@functions` block as the start of a comment and skips to the next `*@`/`-->`, or to the end of the file. Neither is a comment in the C# of the hosts it does not lex, and `@*` after a letter is text by SHARED-DIAG-003. So a real directive later in the file is no longer reported; `main` reports it, and RH2109-001 requires it. Counterexample, `Counter.razor`: `@{` / `    var s = "<!--";` / `}` / `@code {` / `}` → nothing reported (`main`: (4,1)). Failing members (all probed; each followed by a line-starting `@code {` that is then missed): `"<!--"` in a `@{ }` string; `"@*"` in a `@{ }` string; `@M("<!--")` implicit expression; `@("@*")` explicit expression; `// <!--` comment in an `@if { }` block; `<p>user@*x</p>` (`@` after a letter, SHARED-DIAG-003); `<input value="<!--" />` attribute value; `"<!--"` in a `<script>` string. A `-->` later in the file does not help: the directive between the opener and that `-->` is still missed. Members that hold: the same openers inside a lexed `@code`/`@functions` block (only `@*` at a C# token start counts there); `@@` (skips only two characters). The last two failing members depend on how Razor's markup tokenizer treats an attribute value and script text; the C# members do not. No test covers an opener outside a lexed block followed by a real directive. | The owner re-derives. The shapes are rare, and the defect is a missed diagnostic, not an unsafe fix. So the choice is the lead's: either recognize a comment opener only where Razor sees one (at least not inside the C# of `@{ }`, control-flow, and expression hosts, and not after a letter or digit), which overlaps the decision-7 follow-up, or record the regression as a decision with a follow-up. Either way, add a test with a real directive after such an opener that pins the chosen outcome. |
| 2 | high | same file, `SkipCSharpBlock` | FC-027 | – | PR-introduced | The block extent is found by lexing everything from the block's `{` as C#. Razor markup inside a `@code`/`@functions` block (a templated delegate `@<p>…</p>`, as in a Blazor `RenderFragment`) is lexed as C# too. A `{` or `/*` in its text keeps the block open to the end of the file, so every later directive is missed; `main` reports them. Counterexample: `@code {` / `    RenderFragment F => @<p>{</p>;` / `}` / `@code {` / `}` → only (1,1) (`main`: (1,1) and (4,1)). Failing members (probed): `{` in markup text; `/*` in markup text. Members that hold: `'` and `"` in markup text (Roslyn ends an unterminated char or string literal at the line end); a template without these characters. Not a regression: a `}` in markup text closes the block early, so a later line-starting `@code {` inside that block's C# literal is reported, as on `main`. Each failing member needs a second `@code` block (or a `@code` after a `@functions` block) after the template. | Same choice as #1, for the lead. Either keep a templated-delegate `@<` from being lexed as C# (skip to its closing tag), or record it as a decision with a follow-up. A test pins the outcome. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Razor/HTML comment and `@@` escape (`StartsWith(..., "@*")`, `"<!--"`, `"@@"`) | every position outside a lexed `@code`/`@functions` block, whatever the context and the preceding character | SHARED-DIAG-003: a Razor or HTML comment in markup; `@` after a letter or digit is text | the guard also fires inside C# of unlexed hosts, in attribute values and script text, and after a letter | #1 |
| Directive at line start (`StartsLine`) | spaces and tabs back to `\n`/`\r` or the file start | RH2109-001 "starts a line, after optional white space" (Q3) | agree; LF, CRLF, and tab cases tested | holds |
| Block extent (`SkipCSharpBlock`) | everything from `{` to the matching `}` token, lexed as C# | Razor's block extent, where `@<tag>…</tag>` inside a block is markup | disagree for markup holding `{` or `/*` | #2 |
| Razor comment inside a block | `@*` only at a C# token's start | SHARED-DIAG-003: a C# comment or literal wins over Razor text inside it | agree (`"@*"`, `"<!--"` in a block hold) | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Razor directive and comment scanning | only RH2109 (`rg -n "@code\|<!--\|ParseTokens" src --glob '!*Test*'`; the other `ParseTokens` users, `Reihitsu.Core/TokenSeparationUtilities.cs:76` and `CodeFixes/Core/ExcludedCodeUtilities.cs:119`, serve unrelated purposes) | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyCodeBlockTextIn…IsNotReported` (3 reworked), `VerifySharedExample…` (LF, CRLF), `…MultiLineRazorComment…` (LF, CRLF), `…MultiLineHtmlComment…`, `…RazorCommentWithinCodeBlock…`, `…DocumentationBlockComment…`, `…VerbatimString…`, `…InterpolatedVerbatimString…`, `…InterpolatedRawString…` (markup off), `…BlockCommentOfFunctionsBlock…` | text is not reported, the real directive is, at its exact span | an extra, missing, or moved diagnostic | `Verify` with `RazorCodeBlockSpan` (exact span, exact count) | adequate |
| `VerifyClosingBraces…`, `VerifyOpeningBraces…`, `VerifyNestedBraces…`, `VerifyCommentAndStringStartsInsideLiteralsStartNothing`, `VerifyCSharpCommentStartInMarkupDoesNotHideDirective` | block extent ends at the matching brace; a directive after the block stays reported | a later directive missed, or a text line reported | same | adequate for what they cover; no test has an opener outside a lexed block (#1) or markup inside a block (#2) |
| `VerifyCodeBlockInTheMiddleOfALineIsNotReported`, `VerifyCodeBlockIndentedWithTabIsReported`, `VerifyCodeWithoutBraceIsNotReported`, `VerifyEscapedCodeBlockTextAtStartOfLineIsNotReported` | Q3 and both sides of `StartsLine` (FC-003) | as above | same | adequate |

## thoenissen/Reihitsu#974

### Findings
_N/A — none._ `ReadToEndOfDocument` takes a copy of the reader positioned on the root's first token, `Skip()`s the root value, and `Read()`s once more. `Utf8JsonReader` with default options (final block, `CommentHandling.Disallow`, no multiple values) returns `false` when only JSON white space (space, tab, LF, CR) follows and throws `JsonException` at the first other byte. It runs before the root-kind check and before section validation, so `GetConfiguration`'s catch reports only the syntax error and leaves `Configuration` null. RH0001-003 ("not validated further") holds for object and non-object roots. RH4009-001 (Q2) and RH8402-001 hold, because both consumers and the RH8402 fix read `Configuration` and see null. An error inside the root throws at the same position as before, because the copy reaches it first with the same reader state. The CLI loads no `reihitsu.json` (decision 2).

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Content after the root value | whole document after the root value, through `Utf8JsonReader` | RH0001-003 "not valid JSON … anything other than JSON white space after the root value, whatever kind of value the root is" | same white-space set; runs for every root kind | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Reading `reihitsu.json` | `ConfigurationManager` only (`rg -n "Utf8JsonReader\(\|JsonDocument" src --glob '!*Test*'`); consumers RH0001, RH4009, RH8402 analyzer and fix | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH0001 `…AfterRootObjectIsInvalidJson` (token, second value, `//`, `/* */`, later line LF/CRLF, U+00A0 after a non-ASCII root) | one syntax error at the first offending character | other position, other message, or no diagnostic | `VerifyConfiguration` (exact location and message) | adequate |
| `ContentAfterRootObject/Array/StringReportsOnlyInvalidJson` | not validated further | the root's own problem also or instead reported | same | adequate |
| `JsonWhiteSpaceAfterRootObjectIsValid`, `UnknownSectionIsReportedWhenOnlyWhiteSpaceFollowsRootObject`, `RootArrayFollowedOnlyByWhiteSpaceIsReportedAsNotAnObject` | other side (FC-003) | a syntax error reported | same | adequate |
| RH4009 `ConfigurationWith…AfterItsRootObjectIsInactive` (3), `…OnlyWhiteSpace…StaysInEffect`; RH8402 `ConfigurationWithContentAfterItsRootObject` | consumers inactive on trailing content, active on white space | a diagnostic, or a missing one | `Verify`, `VerifySpecificationFile` | adequate |

## Whole run

### Visible output is decided
Every expected value the diff adds or changes is covered.
- RH4001/RH4009 `@` and trivia: unchanged SHARED-DIAG-002. Unicode escapes: Q1. RH4009 entry compared as written: unchanged RH4009-002 "exactly one of the entries" together with SHARED-DIAG-002 (decision 5).
- RH2003 aliases: SHARED-DIAG-002's alias sentence.
- RH2109 comment, escape, and literal cases: SHARED-DIAG-003. Mid-line `@code`: Q3. The tab-indented case: RH2109-001.
- RH0001 trailing content: RH0001-003, unchanged in normative meaning ("not valid JSON", "not validated further").
- RH4009 with trailing content: Q2. RH8402: RH8402-001.

No rule page changed.

### Code quality and comments
Changed summaries describe the new code (`GetExpectedFileNameStem`, `OnNamespaceDeclarationSyntax`, `GetNamespaceName`, the pre-filter comment, `_blockDirectivePattern`, `FindCodeDirectives`, `SkipCSharpBlock`, `ReadToEndOfDocument` with its `<exception>`). The added lines carry no `TODO`, no commented-out code, and no tracker number. The changes stay in scope: the lane check passed, and every path belongs to the four issues. The `ReadToEndOfDocument` copy parses the configuration twice, which is negligible for a configuration file.

### Cost of special cases
Nothing serves only a rare input class. No formatter or code-fix path is involved, and no shape is declared. RH2109's Razor-comment-inside-block branch serves commented-out code in a `@code` block, which people write.

## Failure classes dismissed
- FC-001: the RH4001, RH4009, RH2003, and configuration guards were traced above, and each inspects the full span of its decision. RH2109's is reported under FC-002 (#1).
- FC-003: both sides are tested for every changed condition (tables above).
- FC-004, FC-005, FC-019, FC-020, FC-021: no rule edits tokens or trivia; RH4001's fix renames a document and keeps its content. A `#if`/`#else` with braces inside a `@code` block was probed and holds.
- FC-006, FC-022: RH4001 renames only. Literals matter only to RH2109, and its raw, verbatim, interpolated, and char literals inside lexed blocks are tested and hold.
- FC-007, FC-008, FC-009, FC-018, FC-030: RH4001's fix converges in one application (re-analysis in `Verify`), has no Fix All, and is withheld under RH4001-006 with a `@` name. No symbol is renamed, and the other rules have no fix.
- FC-010 to FC-014, FC-028, FC-029, FC-038 to FC-043: no formatter change, no declared shape, no layout rule.
- FC-015: one owner per policy (tables above).
- FC-016: siblings checked. RH4001 covers both identifier arms and type parameters. RH4009 uses the shared helper. RH2003 has its one pre-filter, and the implicit path never had one. The configuration loader handles every root kind. RH2109's siblings are reported in #1.
- FC-017: assertions are adequate (tables above).
- FC-023: verbatim and escaped identifiers are tested for RH4001 and RH4009, plus `@NIE` for RH2003.
- FC-024, FC-025, FC-026, FC-036: not reachable. These rules have no member-kind, partial, or same-token-on-a-line axis, and nothing renames a symbol.
- FC-027: RH2003 alias scopes (namespace block, global alias in another file) are tested. RH2109's nesting is reported in #2.
- FC-031: comments checked (above).
- FC-032: no result predates the last compiled change.
- FC-033: CRLF is tested for RH2109 and RH0001. The markup-handling normalization of `$$` was found and fixed in 06.
- FC-034: the issues' own shapes are the reproductions.
- FC-035: expected values come from the specs. The reproductions failed on the pre-fix head (04).
- FC-037: no text-only claim.

## Architecture findings
_None._ #1 and #2 are local to one analyzer, which approximates Razor's own tokenizer. No two components of the repository share the decision.

## Hints
- The probe harness and fixtures are in `/tmp/claude-0/-home-user-Reihitsu/0f8f32bb-d47f-54fd-bcc1-8f17dd4f4945/scratchpad/probe`, `/tmp/claude-0/-home-user-Reihitsu/0f8f32bb-d47f-54fd-bcc1-8f17dd4f4945/scratchpad/fx`, `/tmp/claude-0/-home-user-Reihitsu/0f8f32bb-d47f-54fd-bcc1-8f17dd4f4945/scratchpad/fx2`, and `/tmp/claude-0/-home-user-Reihitsu/0f8f32bb-d47f-54fd-bcc1-8f17dd4f4945/scratchpad/fx3` (session scratchpad, not in the repository).
- `StartsLine` accepts only spaces and tabs as leading white space. Other white space before a line-starting `@code` (form feed, U+00A0) is treated as "behind other characters". That is rare, and Razor's own rule for it was not checked.
