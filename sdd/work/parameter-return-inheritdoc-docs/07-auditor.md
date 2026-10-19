Role: auditor
Commit: b09b6f0
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The coordinator's mid-run message only moved the head and the base; it is reflected below.

## Scope

Full audit. Head b09b6f0, a merge of `origin/main` 3bdffeb (read with `git ls-remote` by the lead; `git merge-base --is-ancestor` confirms it is merged). The audit started on be92621 against base 78af33e. The merge brought in only RH5409–RH5417 specs and tests. `git diff --name-only 78af33e b09b6f0 -- src` lists test files only, so no production code differs from the code the probes ran against, and every probe below holds for b09b6f0 (FC-032). `git diff --stat 3bdffeb b09b6f0` shows 41 files, +5960/−2, the same diff the handovers describe. The `requirement-ids.txt` merge removed no line, the file is sorted, and each of the 37 IDs of the run appears there once and is tagged on at least one test. `scripts/lane-check.sh tooling` reports confirmed.

How I checked:
- Static tracing of all 14 analyzers, the RH8201/RH8202/RH8204 providers, `DocumentationAnalysisUtilities`, `DirectDocumentationSyntaxChecker`, `XmlDocumentationExpander`, `InterfaceImplementationUtilities`, and `DocumentationCommentCodeFixUtilities`.
- A scratch analyzer runner outside the repository, built against the analyzer DLL from the test output (documentation mode `Diagnose`, or `Parse` on request).
- `scripts/apply-fix.sh` for RH8201, RH8202, and RH8204 under LF and CRLF.
- A script confirming that every `csharp` block of the 14 specs and SHARED-DOC-014 to SHARED-DOC-016 appears verbatim in a test of the rule it marks. 34 of 35 blocks do. The 35th is RH8202-004's unmarked block, whose test adds the span that the prose describes.

All fixtures were in the session scratchpad. No test was re-run: the cleaner's build and the roles' focused results stand, and no finding depends on a test result.

## Findings
| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | high | `sdd/specs/analyzer/RH8204.md` RH8204-003 coverage, and RH8204-002's location; code: `RH8204DoNotUsePlaceholderElementsCodeFixProvider.StripPlaceholderTags` | FC-006, FC-009, FC-016 | PR-introduced (spec text); exposes a pre-existing defect | **The input:** `/// <remarks>Fast</remarks>` / `/// <placeholder>Later text` / `public void Run()`. The placeholder has no end tag (CS1570). RH8204 reports `<placeholder>Later text\n`.<br>**What the fix does** (LF and CRLF): `XElement.Parse` fails, and the fallback finds no `<` after the start tag, so it returns `""`. The fix deletes the span together with its line break, which yields `    ///     public void Run()`. The method header becomes comment text, and `apply-fix.sh` reports `invalid-result` (the result no longer parses).<br>**Why the spec covers it:** RH8204-003's covered set ("text without character references, entity references, XML elements, and `>` characters") admits this content, and nothing in RH8204-001 to RH8204-003 requires an end tag. Yet the spec promises "replace … with its content" and "remove only the start and end tags".<br>**Related:** the result also breaks SHARED-FIX-003 (a compile error the code did not have). RH8107 and RH8202 withhold their fix through `DocumentationCommentCodeFixUtilities.CanRemoveElement`; RH8204 has no such guard. RH8204-002 ("from the start tag through the end tag") does not describe this span. | **Spec author:** exclude a `<placeholder>` without its own end tag from RH8204's coverage (or state its location). Add the input to the RH8204 open-questions block as an instance of the decided SHARED-FIX-003.<br>**Lead:** name the provider bug to the maintainer as a follow-up candidate. No code change in this lane. |
| 2 | medium | RH8204-003 coverage | FC-002 | same requirement | Three more covered inputs where the fix neither replaces the element with its content nor removes only its tags:<br>(a) `/// Saves the<placeholder> </placeholder>customer` becomes `/// Saves thecustomer`. `XElement.Parse` without `PreserveWhitespace` drops white-space-only text.<br>(b) `/// Saves <placeholder>current customer` / `/// </summary>`: Roslyn closes the placeholder with `</summary>`, and the fix leaves `/// Saves current customer` / `/// `, which deletes the summary's end tag.<br>(c) `/// Handles <placeholder hint="1>2">a & b</placeholder> now` becomes `/// Handles 2">a & b now`. The fallback cuts at the first `>`, which is inside the attribute. | Narrow the coverage to:<br>- content with at least one character that is not white space;<br>- an element closed by its own `</placeholder>`;<br>- no attribute value containing `>`.<br>Add (a) to (c) to RH8204 Q1. |
| 3 | medium | `sdd/specs/analyzer/RH8202.md` RH8202-001 coverage ("well-formed XML apart from a missing end tag of the reported element"), RH8202-002, RH8202-004 | FC-002, FC-009 | same requirement | `/// <remarks><value>The size</remarks>` on a property is covered: the documentation is well-formed apart from the missing `</value>`. But Roslyn closes `<value>` with `</remarks>`, so `EndTag.IsMissing` is false. The diagnostic spans `<value>The size</remarks>`. The fix is offered and leaves `/// <remarks>` without its end tag (LF and CRLF).<br>This contradicts RH8202-004 (no fix without an end tag) and RH8202-002 (the span ends with the content when the end tag is missing). | Narrow the coverage to a reported element that is closed by its own `</value>`, or that runs unclosed to the end of the comment. Add the input to the RH8202 open-questions block.<br>Follow-up candidate, not in scope: RH8107-007 on `main` has the same gap through the same helper. |
| 4 | medium | `sdd/specs/analyzer/RH8201.md` RH8201-005 coverage ("start their own line at the member's indentation") | FC-030, FC-002 | same requirement | The coverage admits a member that is indented consistently but unusually.<br>**The input:** in `class C : B`, `      // Ordinary comment` / `      /// <summary>Runs here</summary>` / `      public override void Run()` with its braces at column 6; and `  /// <summary>Stops here</summary>` / `  [System.Obsolete]` / `  public override void Stop()` at column 2.<br>**What `apply-fix.sh RH8201` does (LF):**<br>- `Run`: the comment, `/// <inheritdoc/>`, and the header line move to column 4, while the braces stay at 6.<br>- `Stop`: `/// <inheritdoc/>` and the attribute move to column 4, while the header stays at 2.<br>This contradicts "at the position of the comment", and it is the RH8201 Q1 mechanism (P9). | Bound the coverage to the indentation of the member's nesting level, which is what the fix lays out (four spaces per level). Add the input to RH8201 Q1. |
| 5 | low | RH8201-005 statement and coverage | — | same requirement | Both speak of "attribute lists … in the member's leading trivia". An attribute list is part of the declaration and never trivia: its first token carries the comment as leading trivia. | Reword: the fix keeps the ordinary comments and directives in the leading trivia, and the attribute lists that follow the comment. |
| 6 | low | Examples of RH8103-001 and RH8202-001, copied verbatim into `RH8103…VerifySpecificationExampleReportsParameterElementsWithoutName` and `RH8202…VerifySpecificationExampleReportsValueElementsOnPropertiesAndIndexers` | — | PR-introduced | Both end with an empty line before the class's closing brace. It is left over from the cases removed in the repair round. | Remove the line in the spec and in the test together. |
| 7 | low | XML summaries of `RH8101…VerifyNoDiagnosticForParametersOfExplicitInterfaceImplementation` and `RH8108…VerifyNoDiagnosticForTypeParametersOfExplicitInterfaceImplementation` | FC-031 | PR-introduced | Each summary calls the reported public sibling "an implicit implementation". The interface member is implemented explicitly in the same type, so the public `ToString(string, IFormatProvider)` and `Convert<T>()` implement nothing. | Say "a public method with the same signature". |

Findings 1 to 4 are spec-coverage repairs plus open-question entries. None needs production code, and none needs a changed test assertion: no test covers inputs 1 to 4. Findings 5 and 6 touch the spec text, and 6 and 7 touch test files.

## Guard scope
_N/A — the diff adds or changes no predicate, guard, or exemption in production code. The coverage boundaries the specs state were traced against the code's predicates under the specs check; findings 1 to 4 are the boundaries that disagree._

Coverage boundaries traced and found to agree with the code:
- the top-level vs nested element choice of `GetDirectTags`, `GetFirstDirectTag`, `GetFirstDocumentingTag`, and `XElement.Elements()`;
- the case-sensitive `name` attribute: the parser builds `XmlNameAttributeSyntax` only for `name`, so `Name="d"` reads as no name;
- the `#if` file set of `HasConditionalInterfaces`: the declaring trees of the type and of `AllInterfaces`. A base class's file is irrelevant, because a derived type that does not list the interface again maps no member of its own to it;
- RH8203's `CanInheritDocumentation` cases: an enum with a base list is reported; implicit `static abstract` operators and conversion operators are not; constructors of records and structs with parameters are reported. All probed.

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Description without text (SHARED-DOC-010/014) | `DocumentationAnalysisUtilities.IsEmpty`/`HasMeaningfulContent`, used by RH8030, RH8031, RH8104, RH8106, RH8111 | One owner; SHARED-DOC-014's "exactly when a summary …" holds by construction |
| Positional pairing (SHARED-DOC-015) | Private loops in `RH8102…OnDeclaration` and `RH8109…OnDeclaration` | Two pre-existing copies. By reading they are identical: `GetDirectTags`, `GetNameAttributeValue`, white-space skip, and index ≥ length or `Ordinal` mismatch. Parity holds; see the hints. |
| Missing name (SHARED-DOC-016) | One-line filters in RH8103 and RH8110 over the shared `GetNameAttributeValue` | Identical; pass |
| `name` matched by value (SHARED-DOC-012 vs SHARED-DOC-015) | RH8101/RH8108 (`XElement` attribute, `Ordinal` set), RH8102/RH8109 (`XmlNameAttributeSyntax` value text), RH8032/RH8033 | Known divergence outside coverage, filed under #1153 and #1132 Q3. Probe: `name="&#97;"` is read as `a` by both RH8101 and RH8102. |
| Withhold a doc-element fix without an end tag | `DocumentationCommentCodeFixUtilities.CanRemoveElement` (RH8107, RH8202); none in RH8204 | Divergent; finding 1 (FC-016) |
| `<inheritdoc>` present | `XmlDocumentationExpander.HasInheritdoc` (top level, `OrdinalIgnoreCase`; RH8101, RH8105, RH8108); `GetFirstDirectTag`/`GetFirstExpandedElement` (RH8203); private `ContainsInheritDoc` in RH8201 (any depth, `InvariantCultureIgnoreCase`) | The different depth is a decision RH8201-001's `Why:` justifies. The comparison mode differs only outside coverage (lower-case tag names). Pass. |
| Member inherits documentation | `DocumentationAnalysisUtilities.CanInheritDocumentation`/`OverridesOrImplementsMember` (RH8203; RH8201 for every member except a field-like event), plus RH8201's private all-declarator rule and `#if` guard | They agree inside both coverages. The multi-declarator divergence is filed (RH8203 Q1). Pass. |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 33 `VerifySpecificationExample…` tests | The spec example yields exactly the marked diagnostics, with the message | An extra, missing, or moved diagnostic | `Verify` with `{\|#n:…\|}` markup and `Diagnostics(id, message, n)`. The examples are verbatim (mechanical check). | Adequate |
| Fix examples (RH8201-005, RH8202-003, RH8204-003) and `VerifyFixKeepsColumnZeroDirectivesAroundComment` | Exact fixed text, with no diagnostic left | Any character or line-ending difference; Fix All needing more than one pass | Code-fix verifier: incremental and Fix All, exact text | Adequate. RH8204's nested example is analyzer-only by design (Q2). |
| `RH8202…InCarriageReturnLineFeedDocument` | CRLF is kept on every remaining line | An LF in the output | Code-fix verifier compares exactly; the test implementer's mutation confirmed this | Adequate (FC-017, FC-033) |
| `RH8202…WithholdsFixForValueElementWithoutEndTag` | Diagnostic reported, with no action | An offered action | `VerifyNoCodeFixIsOffered`, which asserts that a diagnostic exists | Adequate |
| `RH8102…VerifyNoDiagnosticForEmptyOrWhiteSpaceNameKeepingItsPosition`, `RH8109…IncludingExplicitImplementations` | An unnamed element keeps its position | An implementation that skips unnamed elements reports two diagnostics, not one | `Verify` | Adequate |
| Negative tests (`Verify(source)` with no markup) | Nothing is reported | Any diagnostic | `Verify` | Adequate |
| `RH8104…VerifyParameterElementWithNoBreakSpaceOrTabIsWithoutText` | `&#9;` and `&#160;` count as no text; `*` is text | Tab or NBSP counted as text | `Verify` | Adequate for its whitespace claim. The `*` half is weak; see the hints. |
| Tag-only additions to existing tests | Each tagged test lies inside the coverage of its tag | — | — | Checked. RH8201-005 tags sit on directives at column 0 and conventional indentation only; the out-of-coverage tests are untagged, as `04-test-implementer.md` states. |

## Failure classes dismissed
- FC-001, FC-002: no production guard changed. The coverage qualifiers of the specs were traced against the code instead (findings 2–4).
- FC-003: both sides are tested: nested vs top level, first vs later comment, with and without `cref`, include vs direct.
- FC-004, FC-005, FC-019, FC-020, FC-021: only RH8201's fix edits leading trivia. Inside the narrowed coverage the tests keep comments, directives, and attributes. Two shapes were probed: a `/* Block */` after `*/` and a multi-line `/** */` followed by `// Trailing`. Both end up on their own line after `/// <inheritdoc/>`, indented like the member. Finding 4 is relayout, not loss. RH8202 and RH8204 edit inside the XML of one comment, where no directive or ordinary comment can occur.
- FC-007: RH8201's output does not trigger RH8203, because the "inherits" predicate is shared and every declarator ⊂ the first declarator. It does not trigger RH8101, RH8105, or RH8108 either: `<inheritdoc>` exempts them. RH8202 with two `<value>` elements is filed (Q2, P7).
- FC-008: nested placeholders under Fix All are filed (RH8204 Q2). The other rules run a Fix All scenario.
- FC-010 to FC-014, FC-028: no formatter change. No formatter phase emits or owns `<param>`, `<typeparam>`, `<returns>`, `<value>`, `<placeholder>`, or `<inheritdoc>` content. The three fixes do not call `ReihitsuFormatter`.
- FC-015: see the policy table.
- FC-018: no naming fix.
- FC-022: no literal lies in an edited span.
- FC-023: `@class` and `@event` are tested. The character-reference names (`&#97;`) were probed and are consistent.
- FC-024: explicit implementations are tested in every rule. Explicit and implicit `static abstract` operators and conversion operators were probed for RH8201 and RH8203. Partial members are outside coverage.
- FC-025, FC-026, FC-029, FC-036: these shapes do not occur in the changed tests or in what the fixes edit.
- FC-027: multi-line elements and `/** */` comments are tested for RH8104, RH8106, and RH8111.
- FC-031: no production method changed. The test summaries were read; finding 7.
- FC-032: production code is unchanged up to b09b6f0 (see Scope).
- FC-034: the examples are verbatim.
- FC-035: the expected values come from spec blocks derived by running the unchanged code. That is the purpose of this lane; the test implementer's mutations show the assertions can fail.
- FC-037: no text-only proof is claimed.
- SHARED-DOC-008 Parse half: RH8101, RH8105, RH8108, and RH8203 give the same results under `Parse` and `Diagnose` on later-comment shapes (probe).

## Hints
- Kind coverage relies on sibling rules:
  - RH8104-001 names delegates, constructors, indexers, structs, records, and record structs, and RH8109-001 and RH8111-001 name structs and records, but no test of those rules uses those kinds.
  - The shared `ParameterOwnerKinds` and `TypeParameterOwnerKinds` lists are pinned by the RH8101–RH8103, RH8108, and RH8110 tests, so only a per-rule registration change would go unnoticed.
- `RH8104…NoBreakSpaceOrTab…` places `*` in a `///` comment, where it is text under any reading. SHARED-DOC-010's mid-line `*` rule for `/** */` comments is pinned only by RH8030, which uses the same helper.
- Pre-existing: the summary of `RH8201…VerifyTrailingCommentIsPreservedWhenMultiLineDocumentationCommentIsReplaced` is truncated ("… asserted by") and describes another test. The run only added a tag.
- Follow-up candidates:
  - RH8107-007 has finding 3's mismatched-end-tag gap.
  - RH8204's provider could reuse `CanRemoveElement` (finding 1).
  - The RH8102/RH8109 and RH8103/RH8110 private copies could join the shared `name`-helper work of #1132 Q3.
