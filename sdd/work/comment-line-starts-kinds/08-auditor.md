Role: auditor
Commit: 748c9341471a380974563bf70675fac3fc58d9c3
Verdict: findings

My prompt named the issue, the slug, the handovers, the head and the base. It carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Head `748c934`: it is checked out, and the tree is clean. Base `origin/main` = `5c5b70169b0bf0a05ff3253ce7510c64b2f740e7`, confirmed with `git ls-remote origin refs/heads/main`. The base is an ancestor of the head, merged in `748c934`. That merge brought in only `sdd/README.md` (the P11 section).
- Full audit of `git diff 5c5b701` (33 files): 13 RH51xx/RH52xx/RH53xx specs, `shared/comment-placement.md`, the ledger, 8 analyzer test files, 2 decisions entries, and the `sdd/work/` handovers. No production, formatter, or tooling code changed. The diff matches handovers 01–07.
- `scripts/lane-check.sh tooling --base 5c5b701 --head HEAD` gives `confirmed — claimed tooling, diff needs tooling`.
- I trusted the cleaner's build (0 warnings) and the focused results: 327/327 touched-rule tests, architecture 72/72. No finding turns on re-running them.
- Probes ran through `scripts/apply-fix.sh` (LF and CRLF) and `scripts/format.sh`, with fixtures only in the session scratchpad.

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | high | `sdd/specs/shared/comment-placement.md`, SHARED-COMMENT-011, second sentence ("a token behind it on the comment's first line MUST NOT be reported for its position: neither for not starting its line nor for its column"); applies through the reference sentence in RH5201-001, RH5301-002, RH5303-002, RH5110-001, RH5206-001 and RH5308-002 | FC-002 | PR-introduced | **The exemption covers every token on the comment-led line, not only the token directly behind the comment.** P12, as the maintainer decided it (`00-lead.md` clarification 1), says "not reported for the position of **the** token behind it". Read as written, the requirement turns conforming behavior into bugs and contradicts SHARED-INIT-001. Probes on the head, LF and CRLF, show the code doing what P12 intends: (a) RH5201 reports `.ToList()` in `var list = items.Where(x => x > 0)⏎                        /* all */ .Select(x => x).ToList();` and fixes it onto its own line, while SHARED-COMMENT-011 + RH5201-001 now say `.ToList()` MUST NOT be reported for not starting its line. (b) RH5303 reports `2` in `{⏎ /* a */ 1, 2⏎ }`, with the comment in the element column (RH5303-002: "on a line on which an earlier element starts"). (c) RH5301 reports `B = 2` in `/* a */ A = 1, B = 2`, with the comment in the member column. In (c), SHARED-INIT-001, which RH5301 also lists, says B has no comment in its leading trivia and "MUST start at the expected column". So RH5301's own spec now holds two requirements that contradict each other on B. A follow-up run for thoenissen/Reihitsu#1147 or #1362 Q5–Q7 that derives tests from this text would silence real diagnostics. | Narrow the sentence to the token directly behind the comment, for example "the first token behind it on the comment's first line", and match SHARED-INIT-001's "the token". Add the other side to the example: a later token on the same line is judged as without the comment. Shape (a) or (b) conforms today, so the example rule of the 2026-10-21 decision allows it. |
| 2 | medium | Tests of SHARED-COMMENT-011 (RH5201, RH5203, RH5204, RH5301, RH5303 test classes) | FC-003 | same requirement | The tests cover a comment-led line (not reported), a comment on its own line above (reported), and a comment behind code above (reported). No test pins the boundary of finding 1: a later token on a comment-led line, comment in the right column, still reported. An implementation that skips every token behind a line-leading comment passes all 22 tagged tests. | Once finding 1 is reworded, add a tagged test on that side, for example RH5201 with `/* all */ .Select(x => x).ToList();` (reported on `.ToList()` and fixed), or RH5303 with `/* a */ 1, 2`. Both conform today (probes above). |
| 3 | high | `sdd/specs/analyzer/RH5304.md` (`Uses`, RH5304-002) | FC-016 | same requirement | **RH5304 is missing from the specs that list SHARED-COMMENT-011.** Issue thoenissen/Reihitsu#1139 asks for P12 in "every rule that judges where a line starts", and its "Done when" requires that those specs reference it. RH5304-002 judges the column of a nested collection initializer's closing brace, and the line and column each element starts at. That is the same check as RH5303-002, which is listed, and it goes through the same SHARED-INIT-001. It is in scope by the run's own test ("in which column a line starts", `01-spec-author.md`). Neither skeptic's candidate table includes RH5304, so the escalation-5 question (`squad/decisions/2026-10-22-1139-p12-scope-rh53xx.md`) never put it to the maintainer. No behavior is wrong today. Probe: RH5304 does not report `L = {⏎ /* one */ 1,⏎ 2⏎ }` with the comment in the element column, nor `/* end */ }` with the comment in the opening brace's column; a misaligned element is reported and fixed. The existing tests `VerifyNoDiagnosticForCommentPrefixedOrdinaryElements` and `VerifyNoDiagnosticForCommentPrefixedNestedCollectionClosingBrace` already pin both shapes. | The lead puts RH5304 to the maintainer, because the decisions entry names the list. If it is added: list SHARED-COMMENT-011 under RH5304's `Uses`, add the reference sentence to RH5304-002, and tag the two existing tests. |

`pass, text cleanup` does not apply. Finding 1 changes the meaning of an enforced requirement, finding 2 is a missing test, and finding 3 is a spec scope gap.

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| SHARED-COMMENT-011: which token a line-leading comment exempts | Every token "behind it on the comment's first line" | P12 text ("the token behind it"). SHARED-INIT-001 ("the token" whose leading trivia holds the comment; every other token at its own column). RH5201-001, RH5303-002 and RH5308-001 check later tokens. | Disagree for the second and later tokens on the line | Finding 1 |
| SHARED-COMMENT-011: "a comment … that starts its line" | Defers to SHARED-BLANK-002 | SHARED-BLANK-002. `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex`. `SyntaxTokenPositionUtilities.IsFirstOnLine`, which needs an `EndOfLineTrivia` in the token's leading trivia or the previous token's trailing trivia. | Agree. A comment after a line break starts its line in both. A token behind the tail of a multi-line comment is outside both ("on the comment's first line"; #1362 Q4). | ok |
| SHARED-COMMENT-011: "stands where the rule asks the line to start" | Comment column = the rule's expected column | SHARED-INIT-001, bullet 2 (a same-line comment starts at the expected column, and the token is not checked), for one comment. FMT-INDENT-006 for a comment before a closing brace. | One comment: agree; the RH5301/RH5303 tests are tagged with both. Two comments: SHARED-INIT-001 is silent, SHARED-COMMENT-011 decides, and the code reports (#1362 Q7). Closing brace: the formatter leaves `/* Keep close. */ };` at the brace column (probe), as SHARED-INIT-001 asks. | ok, non-conformance tracked |
| SHARED-COMMENT-011: "a comment that does not start its line … does not make the line start with a comment" | The comment behind code on the line above, or on a line of its own above | SHARED-CHAIN-004 (any comment in the gap), RH5305-001 (any comment in front on the bracket's line), RH5306-001 (any comment) | The rules' broader exemptions stand on their own. The sentence only leaves such lines outside the predicate, so no contradiction (skeptic F1 resolved). | ok |
| SHARED-COMMENT-001: "documents a declaration" | Positions where the compiler reads documentation (no CS1587) | RH5034-001 and its position list (type, delegate, member, enum member; not namespace, accessor, parameter, local function, statement). `CommentPositionUtilities.IsOrdinaryComment` excludes by kind or text in every position. | Spec texts agree. The code does not implement "documents nothing" (#1362 Q1, the 2026-10-21 decision). | ok, non-conformance tracked |
| SHARED-COMMENT-001: doc-mode independence | Text-based kind in `DocumentationMode.None` | `DocumentationAnalysisUtilities.IsDocumentationLine` (exactly three slashes), `CommentPositionUtilities.IsMultiLineDocumentationText` (`/**` but not `/**/`) | Agree on kind | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| A comment-led line is judged by its first element (spec side) | SHARED-COMMENT-011. SHARED-INIT-001 (RH5301, RH5303, RH5304). RH5201-002's own block-comment sentence. RH5204-001 (line indentation). The broader exemptions of SHARED-CHAIN-004, RH5305-001 and RH5306-001. | SHARED-INIT-001 and SHARED-COMMENT-011 overlap for RH5301/RH5303. Escalation 5 justifies it, and parity holds for one comment, except for later tokens on the line (finding 1). The two-comment divergence is filed in #1362 Q7, with a note that both change together. |
| Trivia starts its line (code) | `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex` (SHARED-BLANK owner). `SyntaxTokenPositionUtilities.IsFirstOnLine` (general utility, used by `FluentChainLink.StartsLine`). Private `AttributeTargetUtilities.StartsLine`. `RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.TryGetActualIndentation` (line text). `RH5201MethodChainsShouldBeAlignedAnalyzer.GetLineStartColumn`. `InitializerLayoutAnalysisUtilities` (SHARED-INIT owner). | Pre-existing and unchanged: the diff changes no code. The spec side has one owner, SHARED-BLANK-002, which SHARED-COMMENT-011 now references instead of restating. Not a finding of this PR. |
| Ordinary comment vs documentation comment (code) | `CommentPositionUtilities.IsOrdinaryComment` (SHARED-COMMENT owner). `DocumentationAnalysisUtilities.IsDocumentationLine`. The formatter's private `TokenGapNormalizer.IsOrdinaryComment`, which is kind-only. | Pre-existing and unchanged. SHARED-COMMENT-001 binds only RH5033–RH5049, which all go through `CommentPositionUtilities`. See Hints. |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5204 `VerifyNoDiagnosticForMemberBehindBlockCommentStartingTheLine`, `…WithoutDocumentationParsing`, `…WithCrLf` | `/* a */   private` with the comment at the scope column is not reported | Any RH5204 diagnostic. Without the comment, `private` sits at column 14, not 4, and is reported. | `Verify(source)`, analyzer only, no diagnostics expected | adequate; covers doc mode and CRLF |
| RH5204 `VerifyNoDiagnosticForMemberBehindDelimitedDocumentationCommentStartingTheLine`, `…WithoutDocumentationParsing` | "Of any kind": a `/** */` leading the line counts like `/* */` | Any diagnostic (`private` at column 34 without the comment) | same | adequate (FC-002, both modes) |
| RH5204 `VerifyMemberBelowBlockCommentLineIsDetectedAndFixed` | "On the comment's first line": a comment on its own line above does not exempt the member | No diagnostic, or the fix moving `/* a */` | `Verify(source, fixed, diag)`, exact output | adequate (FC-003) |
| RH5201 `VerifyNoDiagnosticForLinkBehindBlockCommentStartingTheLineInTheAnchorColumn`, `…WithCrLf` | A link behind a comment in the anchor column is reported neither by -001 nor by -002 | Any diagnostic (`.ToList()` at column 36 instead of 24 without the comment) | analyzer only | adequate; the boundary of finding 1 is missing (finding 2) |
| RH5201 `VerifyBlockCommentStartingTheLinkLineIsAlignedWithTheLink` (tag added) | The same, with no extra spaces | Any diagnostic | analyzer only | adequate |
| RH5203 `VerifyArgumentBelowTrailingCommentIsDetectedAndFixed`, `…WithCrLf` | A comment behind code on the line above leaves the argument judged | No diagnostic, or the fix moving `/* one */` | exact fixed output | adequate (FC-003) |
| RH5203 `VerifyArgumentBelowCommentLineIsDetected` | A comment on its own line above leaves the argument judged | No diagnostic | analyzer only. The fix is not asserted because it moves the comment to column 0, which is thoenissen/Reihitsu#1144 (verified on GitHub: the RH5203 instance `M(1,⏎ // two⏎ 2);`). | adequate for the diagnostic; the omission is justified |
| RH5301 `VerifyNoDiagnosticForDelimitedDocumentationCommentPrefixedAssignment`, `…WithoutDocumentationParsing`; `VerifyNoDiagnosticForCommentPrefixedAssignmentWithCrLf` | A member behind a comment of any kind in the member column is not reported | Any diagnostic (member at column 42–43 instead of 24 without the comment) | analyzer only | adequate |
| RH5301 `VerifyNoDiagnosticForCommentPrefixed{OpeningBrace,ClosingBrace,Assignment}`; RH5303 `…CollectionOpeningBrace`, `…CollectionClosingBrace`, `…ComplexElementTokens` (tags added) | The same, for braces, members and complex-element tokens. SHARED-INIT-001 decides these identically; it is not broader. | Any diagnostic. Comments checked at `new` or at the member/element column. | analyzer only | adequate; not green for another reason (FC-033) |
| RH5303 `VerifyNoDiagnosticForCommentPrefixedCollectionElement` | An element behind a comment in the element column is not reported | Any diagnostic (`1` at column 45 instead of 25 without the comment) | analyzer only | adequate |
| RH5033 `VerifyQuadrupleSlashCommentAfterAttributeIsReported` | `//// note` behind an attribute list is reported with documentation parsed (the harness default, Diagnose) | No diagnostic | analyzer only, exact location | adequate |
| RH5033 `VerifyEmptyBlockCommentAfterAttributeIsReported`, `…WithoutDocumentationParsingIsReported` (trimmed and renamed) | `/**/` is an ordinary comment in both modes | No diagnostic | analyzer only | adequate. The summaries match the trimmed data. |
| RH5034 `VerifyNoDiagnosticForBlockDocumentationCommentAfterDocumentationComment`, `…WithoutDocumentationParsing` | A `/** */` that documents the declaration is not an ordinary comment, in both modes | A diagnostic on `/** <remarks> */`, which an over-reporting P13 implementation would produce | analyzer only | adequate |
| Removed: RH5033 ×3, RH5038 ×2, and the `/** <summary> */` halves of the two mixed tests | — | — | — | As decided (escalation 4). RH5033 went from 23 to 21 tests (3 removed, 1 added). `ApplyDocumentationModeParseToTestProject` is still used by the RH80xx tests. No remaining RH5033–RH5049 test asserts that a `///` or `/** */` which documents nothing goes unreported: every remaining one is in RH5034 and stands in a documenting position, or behind a declaration's first token. |

## Failure classes dismissed

- FC-001: no code guard changed. The spec exemption's span is too wide rather than too narrow, which is finding 1 under FC-002.
- FC-004, FC-005, FC-019, FC-020: no rewrite or fix changed. A directive fills its whole line, so it cannot lead a line ahead of a token.
- FC-006, FC-018, FC-022–FC-026, FC-028, FC-029, FC-036: no rewrite, rename, literal handling, or detached formatting is involved.
- FC-007, FC-008, FC-009, FC-030: no fix behavior changed. The fix outputs the new tests pin (RH5203, RH5204) are existing behavior that converges in one pass. The `no-progress` observations of the skeptic are pre-existing follow-up candidates.
- FC-010, FC-011, FC-012: no formatter change.
- FC-013: no analyzer or formatter change. On the spec example, I ran `scripts/format.sh` twice: the output is idempotent, and RH5201 and RH5204 do not report it. The formatter doing more is P11 (see Hints for the blank line it inserts).
- FC-014: no new analyzer.
- FC-015: covered under Policy ownership. No new owner was introduced in code.
- FC-017: covered under Assertion adequacy.
- FC-021: a `/* */` ending mid-line (#1362 Q4) is excluded by "on the comment's first line" and is untagged. RH5301 `VerifyCommentsInFrontOfTokensAnchorTheirLines` was deliberately left untagged.
- FC-027: "every enclosing scope" is the scope question, covered by finding 3.
- FC-031: every new or renamed test's summary describes its data. The diff changes no production comments.
- FC-032: no compiled production change, so earlier results stay valid.
- FC-033: CRLF and `DocumentationMode.None` variants are present where the requirement names them. RH5112 and RH5306 shapes are untagged because SHARED-CHAIN-004 and RH5306-001 decide them more broadly. The tags shared with SHARED-INIT-001 are equal in scope, not broader.
- FC-034: no bug reproduction. The issue's RH5110, RH5203 and RH5206 examples are non-conforming and are tracked by thoenissen/Reihitsu#1147 by decision.
- FC-035: expected values come from the spec examples and probes of unpatched code.
- FC-037: no text-only proof is claimed.
- No `TODO`, tracker reference, or commented-out code is added (rg over the added lines of `src/` and `sdd/specs/`). No edit is outside the run's scope, and none of the files owned by concurrent runs (`00-lead.md` item 8) is touched.

## Hints

- **Pre-existing, unrelated to the diff (FMT-BLANK-011).** The formatter inserts a blank line inside the wrapped chain of the SHARED-COMMENT-011 RH5201 example: `items.Where(x => x > 0)⏎⏎/* all */   .ToList();`. The result is idempotent and analyzer-clean. It is the same mechanism the skeptic saw for RH5302/RH5308. It is a follow-up candidate if no issue tracks it.
- Issue thoenissen/Reihitsu#1362 Q5 names RH5302-001 and RH5308-001 as touched. The deciding requirements, which carry the reference sentence, are RH5302-002 and RH5308-002. That is the lead's issue text.
- Handover counts:
  - `05-test-implementer.md` says 10 new not-reported SHARED-COMMENT-011 tests. There are 11: 5 RH5204, 2 RH5201, 3 RH5301, 1 RH5303. By the columns in the table above, each of the 11 depends on its leading comment.
  - `07-spec-checker.md` says "all 19 requirements" but lists 16. The diff carries those 16.
- RH5303-003 (complex elements, SHARED-INIT-002) has no reference sentence, though the RH5303-003 test `VerifyNoDiagnosticForCommentPrefixedComplexElementTokens` is now tagged SHARED-COMMENT-011. `Uses` is spec-wide and SHARED-INIT-001 decides the same, so no reader sees the opposite. The spec author's N4 convention.
- RH5112's listing of SHARED-COMMENT-011 never decides anything. RH5112 never reports a token for not starting its line or for its column, and SHARED-CHAIN-004 decides every comment-led first link.
- When #1362 Q1 is fixed, the formatter's kind-only `TokenGapNormalizer.IsOrdinaryComment` is a second classifier next to `CommentPositionUtilities.IsOrdinaryComment`.
- `07-spec-checker.md` finding 1 is still open for the lead: confirm that decided principles P12 and P13 need no change doc, and record it in the run report.
