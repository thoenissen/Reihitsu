Role: auditor
Commit: ec5a8f756734a467a7f38725d398573de2b0fb99
Verdict: findings (#1075: pass; #1261: findings)

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

State: checkout is `ec5a8f7`, the working tree is clean, `origin/main` is `73374f6` (the SHA in the prompt) and is an ancestor of the head. `git diff origin/main...HEAD` outside `sdd/work/` touches exactly the files `04`–`09` name: 5 production files (RH5011 and RH5030 fixes, RH5025 and RH5027 analyzers, `ReihitsuFormatter.cs`), 9 test files, 7 spec files. `scripts/lane-check.sh bug-fix`: confirmed. The cleaner's build result and the roles' focused test results are trusted; I re-ran nothing from the suites.

Extra evidence gathered with `scripts/apply-fix.sh` (LF and CRLF) on throw-away fixtures in the scratchpad, on the head:
- RH5405 `M(a, b); /* x */ // trail` (two comments) → written once, inside the block.
- RH5405 `if (a) M(a, b); /* c */ b++;` → `M(a, b); /* c */⏎} b++;` (comment once).
- RH5405 `M(a, b); /* x */ ` with trailing whitespace → `} ` (see Hints).
- RH5405 `if … // trail⏎else if (…)⏎ M(a, b); // two` → each comment once.
- RH5403 `if (a) { M(a, b); } // trail` → `} // trail`, unchanged path (the marked token stays last).
- RH5011 `break; /* a⏎b */ /* c⏎d */⏎case 2:` → blank line below `d */`, one iteration.
- RH5011 / RH5030 with a directive line below the comment: no diagnostic.
- RH5002 `a++; /* a⏎b */ if (…)` and RH5021 `var x = 1; /* a⏎b */ a++;` → fixed in one application (see Hints).

## Scope

Head `ec5a8f756734a467a7f38725d398573de2b0fb99`, base `origin/main` `73374f6ef4c1692f27db2bc836f16ad58afbb4e3`. Full audit, the first of the run.

## thoenissen/Reihitsu#1075

### Findings
_N/A — no confirmed finding._

Checked against the specs:
- **RH5025-006.** `IsOpeningBraceOfInterpolation` excludes exactly the `OpenBraceToken` of an `InterpolationSyntax`. That covers `$"""`, `$$"""` (`{{`), `$@"` and `@$"`. A `{` inside the hole's code is still checked. I enumerated the class by token position. A hole `{` that starts a line always has literal text above it, because a hole cannot start a line in a regular `$"` string, and the line above a nested hole is the inner literal's text or its opening line. The mirror rules hold, traced by reading the code:
  - RH5022 checks the line below a line that ends with `{`. Below a hole `{` lies hole code.
  - RH5024 checks the line above a line that starts with `}`. Above a hole `}` lies hole code.
  - RH5026 already uses `GetLineIndicesBeginningWithToken`.
- **RH5027-001.** `StartsWithFooter` turns the unchanged qualifier into a condition: whitespace, or whitespace plus the block body's own `}`, in front of `while`. Every shape in the new negative example and the two controls agrees.
  - `do if (a) { … ⏎⏎} while` (a `}` that is not the do's block body) is no longer reported. The normative text demands that ("the closing brace of the statement's block body"), and RH5024 reports that blank line.
  - `IsWhitespaceOnly` has the same semantics as `GetLineIndicesBeginningWithToken`'s `IsNullOrWhiteSpace`.
- **RH5011-004/-007.** `FindLineEndingTrivia` is shared by `CanRegisterCodeFix` and `ApplyCodeFixAsync`, so registration and application cannot disagree.
  - A line break that is not in the semicolon's trailing trivia cannot sit in the next token's leading trivia, because Roslyn's trailing trivia runs up to and including the first end-of-line. The search therefore returns either the line end of the comment's last line, or none, when code follows on that line.
  - The same-line branch (RH5011-005/006) is reached only when no line-ending trivia exists. It is unchanged.
  - The inserted line break copies the ended line's break, as RH5011-004 does on `main`.
- **RH5030-003.** The guard `AreSeparatedByEndOfLine(closeBrace, nextToken)` uses the same `nextToken` as `ApplyCodeFixAsync`. The analyzer reports only pairs on different lines (`RequiresBlankLineAfterClosingBrace`), so the guard withholds exactly the "every line break inside a comment" member. `BlankLineCodeFixUtilities` is untouched, so RH3208 is unchanged.
- **RH5023-002.** Not changed (maintainer answer B).

#### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH5025: line above a brace is literal text | the brace token's parent (`InterpolationSyntax.OpenBraceToken`) | RH5025 fix (deletes the reported line, no own guard); `GetLineIndicesBeginningWithToken` (whitespace in front of the token) | The exemption drops only hole braces; a `{` in hole code stays in the set | holds |
| RH5027: the line holds the footer | `whileLine.Start`..`while`, and `whileLine.Start`..block `}`..`while` | RH5027 fix (deletes the reported line); RH5024 (`}` that starts a line) | Both use whitespace-only text in front of the token; RH5024 covers the `} /* c */ while` blank line the spec hands it | holds |
| RH5011: where the blank line goes / withhold | semicolon trailing trivia ∪ next token leading trivia, first `EndsLine` | analyzer base (blank line after the break line; convergence checked by one-iteration tests); RH5011-004's "ends outside a comment" | Same predicate (`SyntaxTriviaUtilities.EndsLine`) in registration and application; one-iteration tests show the analyzer accepts the result | holds |
| RH5030: withhold when every break is in a comment | trailing trivia of `}` ∪ leading trivia of the next token (`AreSeparatedByEndOfLine`) | RH5030 analyzer (`lastLine < nextLine`); `InsertBlankLineBefore` (inserts at the start of the next token's leading trivia) | The guard is false exactly where the inserted line break would land on the comment's last line | holds |

#### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| "A line break outside a comment ends the line" | `SyntaxTriviaUtilities.EndsLine` / `AreSeparatedByEndOfLine` (`src/Reihitsu.Core/SyntaxTriviaUtilities.cs:37,49`); RH5011 and RH5030 reuse them | one owner |
| "Is this `{` a hole brace" | new private `RH5025…Analyzer.IsOpeningBraceOfInterpolation`; `FormattingTextAnalysisUtilities.IsInsideInterpolatedString` is wider (true for a lambda `{` in a hole, which RH5025-006 still checks) | justified: different question |
| "Line starts with token X" | `FormattingTextAnalysisUtilities.GetLineIndicesBeginningWithToken` (RH5022/24/25/26); RH5027 `StartsWithFooter` via `IsWhitespaceOnly` | justified: RH5027 needs the do-block `}` case, and its semantics are the same |
| "Insert a blank line in front of a statement" | `BlankLineCodeFixUtilities.InsertBlankLineBefore` (RH5030, RH3208); `StatementShouldBePrecededByABlankLineCodeFixProviderBase` (RH5001–RH5021, RH5029) | pre-existing two owners, not changed by the diff; see Hints |
| Literal and hole text (`GetStringLineIndices`, `GetNonFormattableLineIndices`) | `src/Reihitsu.Core/FormattingTextAnalysisUtilities.cs` | untouched, as RH5023 answer B and thoenissen/Reihitsu#951 require |

#### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5011 `VerifyFixInsertsBlankLineBelow…` (a1, a3, a5, a6, directive, spec example) | blank line below the comment's last line; one application | blank line inside the comment; a second iteration needed | `Verify(source, fixed)` with 1 incremental / 1 Fix All iteration where convergence is asserted | adequate |
| RH5011 `…WithCarriageReturnLineFeed` | exact CRLF text, then no diagnostic | LF inserted into a CRLF document; the rule still reports | `ApplyCodeFixAsync` + `Assert.AreEqual`, then `Verify(actual)` | adequate |
| RH5011 `VerifyNoFixIsOffered…` (a2, a4 LF/CRLF, spec example) | reported, no action registered | an action is registered | `VerifyNoCodeFixIsOffered` | adequate |
| RH5011 `VerifyFixAllHandlesSpanningCommentNextToPlainBreakAndWithheldBreak` | Fix All fixes two, leaves the withheld one reported | withheld diagnostic gone or rewritten | `Verify` with `FixedState`/`BatchFixedState.MarkupHandling = Allow` (decision 11) | adequate: Allow asserts the remaining diagnostic, which is stronger than the default |
| RH5011 `VerifyFixInsertsBlankLineBelowBreakLineAboveMultiLineCommentOnNextLine` | RH5011-004 boundary (FC-003) | blank line moved below the comment | `Verify` | adequate |
| RH5025 `VerifyEmptyLineAboveHoleBrace…` (`$"""` LF/CRLF, `$$"""`, `$@"`/`@$"`) and the lambda control | hole brace not reported; lambda `{` still reported and fixed | report on a hole line; no report on the lambda line | `Verify(source)`; `MarkupMode.None` for `$$` | adequate |
| RH5027 seven not-reported shapes and the reported control | qualifier of RH5027-001 | report on a non-footer line; no report on the control | `Verify(source)` / `Verify(source, fixed)` | adequate |
| RH5030 `VerifyNoFixIsOffered…` (b2 LF/CRLF, b3, b6, b7), the b1/b4/directive controls, Fix All | withheld exactly when every break is in a comment; one iteration otherwise | action offered on b2; b1 needs two passes | `VerifyNoCodeFixIsOffered`, `Verify` with 1 iteration, Fix All with `MarkupHandling = Allow` | adequate |

#### Failure classes dismissed
- FC-006, FC-018, FC-023, FC-024, FC-025, FC-029, FC-036: no rename, no expression-body, semantic or naming rewrite in these four rules.
- FC-008: Fix All is covered for RH5011 and RH5030, with a withheld member.
- FC-009: withheld inputs register no action; offered actions always insert a line break.
- FC-010, FC-011, FC-012, FC-013, FC-014: no formatter phase changes for this issue. RH5025/RH5027 report less and RH5011 inserts a blank line that formatting keeps. FC-013 was not traced further: no layout rule's fix result changes.
- FC-026: the tokens are located by diagnostic span or syntax, not by text search.
- FC-028, FC-030: these fixes do not call the formatter.
- FC-032, FC-035, FC-037: no stale result, no patched-copy expectation, and no text-only claim.
- FC-001, FC-002, FC-003, FC-004, FC-005, FC-015, FC-016, FC-017, FC-019, FC-020, FC-021, FC-022, FC-031, FC-033, FC-034: traced above. They hold, apart from the sibling under Hints.

#### Visible output
- The RH5011 inserting outputs and the withheld RH5011/RH5030 outputs follow the maintainer's decision in thoenissen/Reihitsu#1075: "inserts the blank line after the comment's end, or withholds", and for RH5030 "or withheld". `00-lead.md` records that decision under *Maintainer clarifications*, and decision 4 picks the split.
- The a5 placement (the blank line above `// note`) follows unchanged RH5011-004: "a comment on the lines below stays directly above the code it precedes".
- The RH5025 and RH5027 "not reported" outputs are demanded by unchanged SHARED-DIAG-003 and by RH5027-001's unchanged normative text.
- No rule page changed.

#### Architecture findings
_None._

## thoenissen/Reihitsu#1261

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Analyzer.Test/Formatting/RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzerTests.cs`, `VerifyFixKeepsTextOfEnclosingRegionDirectives` | FC-017, FC-034 | – | PR-introduced | The test says it shows that the fix keeps the region directives' text. The issue's instance is the region phase rewriting region text in a node-level fix, which FMT-PIPE-028 prevents. The test cannot fail on that mechanism, for two reasons. First, `class A` is a direct child of the compilation unit, so `FormatNodeInDocumentAsync` writes the target's leading trivia, `#region legacy`, back verbatim (`keepsLeadingTrivia`, `ReihitsuFormatter.cs`). Second, `#endregion` sits in the end-of-file token's leading trivia, outside the target. The expected text would therefore come out even if the node-level entry point ran the region phase. The test is a characterization that the leading trivia of a top-level type is kept, not of region text. | Keep it under a summary that states what it shows, or drop it. Decision 7 allowed a characterization only "if cheap", and `ReihitsuFormatterNodeRegionTests` already covers FMT-PIPE-028. A shape that would falsify the mechanism needs both directives inside the formatted target, such as RH5417's region in an accessor list. Its output carries blank lines that were decided wrong (thoenissen/Reihitsu#1140), so that shape is not a cheap replacement. |

Checked against the specs:
- **SHARED-BRACE-002 and FMT-PIPE-010.** `FormatNodeInDocumentAsync` marks the detached target's last token. When the result's last token is not that token, `WriteBackTrailingTrivia` drops from the written-back trivia as many leading comments as the marked token still carries.
  - The behavior differs from `main` only where the marked token survives and is no longer last.
  - I enumerated the transforms that change the root's last token:
    - `ControlFlowBraceTransform` (if/else bodies, nested `if`, `else if`) keeps the whole trailing trivia on the body (`BlockWrapTriviaUtilities.GetCarriedTrailingTrivia`). Those comments are therefore exactly the original trailing comments.
    - The expression-body transforms (`ExpressionBodyToBlockConverter` builds a new `;`/`}` with `SyntaxFactory.Token`) and `EmptyTypeDeclarationSemicolonTransform` (new `;`) drop the marked token. The full write-back stays as on `main`, and existing RH5412 tests with `{ } // note` still pass on that branch.
    - `SwitchCaseBracePhase` around a section target is the brace member again.
  - No phase moves a comment onto the marked `;`: the only token behind it is the inserted `}`, whose leading trivia is empty. So the comment count cannot remove a comment of another origin.
  - No pipeline code reads annotations (`rg ContainsAnnotations|HasAnnotation` in `src/Reihitsu.Formatter` and `src/Reihitsu.Core`: no hits outside `ReihitsuFormatter.cs`).
- **`FormatNodeInDocumentWithContextAsync`.** It keeps the old write-back. Its callers are RH5111 (declarator, parameter, or enum member in a context), RH7101 (the split-field path), RH3208 (a local function whose new `}`/`;` are fresh tokens), and the chain helper (a chain expression). None of them passes a target whose last token ends up inside the result. The FMT-PIPE-010 sentence that leaves that case undecided is accurate (AGENTS.md, *Supported inputs*).
- **Region text (decision 7).** Not reproduced. It is covered by FMT-PIPE-028 and `ReihitsuFormatterNodeRegionTests`, but see finding 1.

#### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Write the target's trailing comments once | trailing trivia of every token that carries the last-token annotation, compared with the result's last token | `BlockWrapTriviaUtilities.GetCarriedTrailingTrivia` (which comments stay on the body); `ControlFlowBraceTransform.CanWrap` (directive in trailing trivia → no wrap, so the marked token stays last) | The transform keeps all trailing comments on the body, and the write-back removes that many leading comments: the same set. A directive keeps the marked token last, which is the unchanged branch | holds |

#### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Write the target's trailing trivia back after node-level formatting | `ReihitsuFormatter.WriteBackTrailingTrivia` (`FormatNodeInDocumentAsync`); the inline `WithTrailingTrivia(originalLastToken.TrailingTrivia)` in `FormatNodeInDocumentWithContextAsync` (`ReihitsuFormatter.cs` ~331); `FormatSplitFieldInContext` (own path) | Two owners that now differ. The diff justifies it: FMT-PIPE-010 scopes write-once to `FormatNodeInDocumentAsync` and states that no caller reaches the case for the other. Not a finding; see Hints |
| Where a wrapped body's trailing comment lives | `ControlFlowBraceTransform` (keeps it on the body); the entry point no longer writes it again; `StatementBracesCodeFixProviderBase` unchanged (no third owner, decision 9) | one owner |

#### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5405 `VerifyFixWritesTrailingCommentOfBodyOnce` (LF, CRLF), `…OnLineOfIfOnce`, `…MultiLineTrailingCommentOfBodyOnce`, `…NestedIfBodyOnce`, `VerifyFixAllWritesTrailingCommentsOfBothBranchesOnce` | comment once, inside the block; `b++` keeps its line | `} // trail`; comment moved behind `}` | `Verify(source, fixed)`, exact text, incremental and Fix All | adequate |
| RH5405 `VerifyFixKeepsTrailingCommentOfIfBodyFollowedByElseOnce` | boundary: the body is not the target's last token (FC-003) | comment lost or duplicated | `Verify` | adequate |
| RH5406, RH5407 `…Once` (LF, CRLF) | same invariant for the sibling rules (FC-016) | duplicate behind `}` | `Verify` | adequate |
| `ReihitsuFormatterEntryPointTests.FormatNodeInDocumentAsyncWritesTrailingCommentOfWrappedBodyOnce` | FMT-PIPE-010 example | duplicate; `b++` moved | `Assert.AreEqual` exact text, LF and CRLF | adequate |
| `…WritesTrailingCommentOfBracedTargetOnce` | boundary: the marked token stays last | comment lost behind `}` | same | adequate |
| RH5412 `VerifyFixKeepsTextOfEnclosingRegionDirectives` | region text kept by a node-level fix (FMT-PIPE-028) | none possible: the leading trivia is restored verbatim and `#endregion` is outside the target | `Verify` | **weaker than claimed** — finding 1 |

#### Failure classes dismissed
- FC-005, FC-020: `CanWrap` refuses a directive in the body's trivia, so the marked token stays last and the write-back is `main`'s.
- FC-006, FC-018, FC-022, FC-023, FC-025, FC-036: no semantic, rename or literal rewrite. The expression-body kinds drop the marked token, so their write-back is unchanged; this is covered by the RH3208 byte-identical check in `05` and by the RH5412 `// note` tests.
- FC-007, FC-009: one-application exact-text tests; an offered fix always braces.
- FC-008: Fix All with both branches reported is covered.
- FC-010, FC-012: no phase changed. The result equals document formatting for the tested shapes (`02`, `03`).
- FC-011: no loop added.
- FC-013, FC-014: no rule added. The output agrees with document formatting.
- FC-024, FC-029: not reachable through brace wrapping.
- FC-030: the target is unchanged (the owning `if`).
- FC-032, FC-035, FC-037: no stale result, no patched-copy expectation, and no text-only claim.
- FC-001, FC-002, FC-003, FC-004, FC-015, FC-016, FC-017, FC-019, FC-021, FC-026, FC-027, FC-028, FC-031, FC-033, FC-034: traced above. They hold, apart from finding 1.

#### Visible output
- The RH5405, RH5406 and RH5407 expected outputs are today's fix output without the duplicate comment. That is the maintainer's decision in thoenissen/Reihitsu#1261 (comment text stays as written), recorded under *Maintainer clarifications*. Every input avoids the #1209 outputs (decision 8).
- The RH5412 output (`class A;` between unchanged directives) is demanded by SHARED-FIX-003, FMT-PIPE-028 and SHARED-EMPTY-003.
- The formatter entry-point expectations are demanded by FMT-PIPE-010 as clarified and by unchanged spacing requirements (`M(a,b)` → `M(a, b)`).
- No rule page changed.

#### Architecture findings
_None._ Decision 9's two-owner finding is resolved: the transform owns the comment, and the entry point no longer writes it a second time.

## Whole run

### Code quality and comments
- XML summaries of every changed method describe the new code (FC-031): `FindLineEndingTrivia`, `ApplyCodeFixAsync`/`CanRegisterCodeFix` (RH5011), `CanRegisterCodeFix` (RH5030), `IsOpeningBraceOfInterpolation`, `StartsWithFooter`/`OnDoStatement`, `DetachWithAnnotatedLastToken`, `WriteBackTrailingTrivia`, and `RemoveLeadingComments`.
- No `TODO`, no commented-out code, and no tracker number in `src` (`git diff … -- src | rg 'TODO|#[0-9]{3,}'`: none).
- The new members are private, and no dependency was added. Scope: nothing outside the paths the handovers name; the lane is confirmed.

### Hints
- **RH5030/RH3208 vs the preceded-by-blank-line base.** Two owners insert a blank line in front of a statement. Both exist on `main`, and the diff does not change that structure.
  - `StatementShouldBePrecededByABlankLineCodeFixProviderBase` rebuilds the gap when `TokenGapAnalysis` needs two line breaks. It fixes `a++; /* a⏎b */ if (…)` (RH5002) and `var x = 1; /* a⏎b */ a++;` (RH5021) in one application. Verified with `scripts/apply-fix.sh`, LF and CRLF.
  - `BlankLineCodeFixUtilities.InsertBlankLineBefore` cannot fix that shape. RH5030 now withholds it (RH5030-003, within the maintainer's choice in #1075), and RH3208 still corrupts it (the follow-up candidate already in `00-lead.md`).
  - Follow-up candidate that names the structural cause: one owner for "blank line in front of a statement" would fix RH3208 and could later narrow RH5030-003's guard. Narrowing it would be a visible change, so it is for the maintainer.
- **Trailing whitespace behind the comment.** `if (a)⏎ M(a, b); /* x */ ` (with trailing whitespace) now gives `} ` behind the closing brace. On `main` it gave `} /* x */ `, with the same trailing whitespace. The write-back after the last phase copies whitespace the cleanup phase never sees (SHARED-FIX-007). This is pre-existing, and the run moved it only within the line.
- **Code behind the target on the same line.** `if (a) M(a, b); /* c */ b++;` gives `} b++;`; on `main` it was `} /* c */ b++;`. FMT-PIPE-010's "keeps its line" reads as the target's last line. This is pre-existing layout, and only the duplicate is gone.
- **`FormatNodeInDocumentWithContextAsync`.** It still carries its own copy of the old three-line write-back. That is justified by FMT-PIPE-010 as it stands. A future caller that passes a brace-wrapped target there would get the duplicate back.
- **Bookkeeping.** `00-lead.md` decisions 4, 5, 6, 8 and 9 still say `pending` in their `Commit` column.
