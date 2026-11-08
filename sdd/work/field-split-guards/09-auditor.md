Role: auditor
Commit: 4ad21a7999f9150960eda8a256aeef653f1848f1
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The handovers 01–08 and `00-lead.md` were read as claims and checked against the code, the specs, and runs. They were not adopted as facts.

## Scope

- Head `4ad21a7999f9150960eda8a256aeef653f1848f1`. This is the checkout, and the working tree is clean.
- Base `origin/main` is `026fb648e4457763a9b15d559500f2eba07dcda5`. `git rev-parse origin/main` matches the prompt, and the branch contains the base (`git merge-base --is-ancestor`).
- Full audit (first audit of the run). The diff is what the handovers describe:
  - production: `FieldDeclarationSplitTransform.Splits`, which gains `IsDocumented` and `CarriesCommentInRepeatedPart`;
  - the `SplitsFieldDeclaration` summary;
  - the RH7101 fix, which drops its private documentation check (decision 5);
  - the six test files named in 04/06;
  - the spec files named in 01/03;
  - `sdd/work/`.
- `scripts/lane-check.sh bug-fix` reports `confirmed — claimed bug-fix, diff needs bug-fix`.
- Fixtures for the runs below were in `/tmp/claude-0/-home-user-Reihitsu/97d9a81b-01a7-58f5-89e4-4ec128033620/scratchpad/`, outside the repository. They were run with `scripts/format.sh` (the CLI path, documentation mode `Parse`) and `scripts/apply-fix.sh RH7101` (LF and CRLF).

## thoenissen/Reihitsu#963

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/FieldDeclarationSplitTransform.cs` `CarriesCommentInRepeatedPart` (and its summary), against FMT-STRUCT-030 bullets 2–3, FMT-PIPE-031, RH7101-007 | FC-002, FC-001, FC-012 | – | same requirement | See *Finding #1* below. | See *Finding #1* below. |

### Finding #1

**What goes wrong.** FMT-STRUCT-030 says "a comment behind the type on the type's line keeps the declaration from being split". RH7101-007 says the same for the fix. The guard does not test the line. It tests trivia ownership: it skips the first declarator's leading trivia. In documentation mode `Parse` or `Diagnose`, Roslyn never puts a `///` or `/** */` comment into trailing trivia. Such a comment written behind the type on its line is therefore the first declarator's leading trivia, and the guard lets the split through.

Counterexamples, measured on the head:

- (a) `private int /** c */ _first, _second;` is formatted to `private int⏎⏎/** c */ _first;⏎private int _second;`. A blank line now stands inside the first field.
- (b) `private int /// <summary>c</summary>⏎        _first, _second;` is formatted to `private int⏎⏎/// <summary>⏎/// c⏎/// </summary>⏎_first;⏎private int _second;`.
- `apply-fix.sh RH7101` offers the fix for (a) and (b) under LF and CRLF and writes the same output. RH7101-007 says the fix is withheld.

**Class enumeration.** Axes:

- comment kind: `//`, `/* */`, `///`, `/** */`;
- documentation mode: `Parse`, `Diagnose`, `None`;
- slot: every repeated position, and the boundary between the type's last token and the first declarator;
- first declarator: on the type's line, or on the next line.

Failing members: a `///` or `/** */` behind the type's last token on the type's line, in `Parse` or `Diagnose`, with the first declarator on the same line or on the next line. The type's shape (simple, generic, array, nullable) makes no difference, because only its last token matters.

Members that hold:

- `//` and `/* */` behind the type, in every mode (trailing trivia of the type's last token; `ReturnsFalseForACombinedDeclarationWithACommentInThePartEveryFieldRepeats` rows 214–216).
- `///` and `/** */` in mode `None` (ordinary trailing comments, traced).
- `///` and `/** */` in every other repeated slot: they become the leading trivia of a token that is not the first token, so the guard blocks them. `[System.Obsolete] /** c */ private int _first, _second;` was measured: not split, and stable.
- Own-line comments in front of the first name (maintainer answer A).

**Constraint for the repair.** Making the guard follow the spec literally (count a comment in the first declarator's leading trivia that starts in front of that trivia's first line break) is not idempotent by itself.

- The documentation-comment phase moves such a comment off the type's line. It does so even when the declaration stays whole: `/// <summary>d</summary>⏎private int /** c */ _first, _second;`, kept whole by the documentation guard, is formatted to `private int⏎⏎/** c */ _first, _second;`.
- The second pass then sees an own-line comment in front of the first name, which does not block (answer A), and splits: `private int⏎⏎/** c */ _first, _second;` is formatted to `private int⏎⏎/** c */ _first;⏎private int⏎_second;`.

The owner therefore has to pick one of two routes:

- Make the code match the spec, and also keep the second pass stable, for example by keeping the comment where it stands or by blocking on the comment's kind and slot rather than on its line.
- Change FMT-STRUCT-030/RH7101-007 to put this member on the split side. Today's output is identical to `main`'s, but no maintainer answer covers this member, and the decision on #913 Q3 ("a comment between the type and the first declarator blocks the split") reads against it. That route is an output question.

Either way, the `CarriesCommentInRepeatedPart` summary ("a comment that starts on a line after the type") and the `SplitsFieldDeclaration` summary have to state the boundary the code then implements.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Comment in the part every field repeats (`CarriesCommentInRepeatedPart`) | Leading and trailing trivia of every token from the declaration's first token up to the first declarator's first token. Excluded: the first token's leading trivia and the first declarator's own leading trivia. | (1) `CreateSplitField`, which shows what is actually repeated: every token before the first declarator, with its trivia, except the declaration's leading trivia (later fields get rebuilt leading trivia); the first declarator goes to the first field only. (2) FMT-STRUCT-030 bullets 2–3 and RH7101-007, which are line-based ("behind the type on the type's line" against "starts on a later line"). | (1) exact. (2) disagrees for `///` and `/** */` on the type's line in `Parse`/`Diagnose`. | Finding #1 |
| RH7101 fix offered (`TryGetFixableFieldDeclaration`) | `ReihitsuFormatter.SplitsFieldDeclaration` only | `SplitFields` → `Splits` inside the structural phase. That runs after `ControlFlowBraceTransform`, the expression-bodied transforms, `EmptyTypeDeclarationSemicolonTransform`, and the trailing-comma removals (`StructuralTransformPhase.CreateRewriters`). None of them touches a field's attribute, modifier, type, or leading trivia, so the answer on the original tree equals the answer inside the phase. | agree | holds |
| Fix All candidates (`ApplyFixAllAsync`) | Same predicate, evaluated on the original root. Each fix then runs on the tracked current node. | `FormatSplitFieldInContext` writes only the target's fields plus whitespace in front of the neighbor's first token (`TrimLeadingWhitespace`). It adds no comment to a neighbor, so a neighbor's answer cannot change between fixes. | agree | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether a field declaration is split | `rg -n "SplitsFieldDeclaration\|FieldDeclarationSplitTransform.Splits\|Splits\(" src --glob '!*Test*'`: one owner, `FieldDeclarationSplitTransform.Splits`. Callers: `SplitFields` (two calls), `ReihitsuFormatter.SplitsFieldDeclaration` (delegates), `FormatNodeInDocumentWithContextAsync`, `RH7101DoNotCombineFieldsCodeFixProvider.TryGetFixableFieldDeclaration`. `rg "Variables.Count > 1"`: only `Splits`. | one owner |
| Comment classification | `Reihitsu.Core.SyntaxTriviaUtilities.IsCommentTrivia` (documentation kinds included), reused without a copy | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| FDS `Comment…KeepsTheDeclarationUnsplit` family, `SpecExample…` (FMT-STRUCT-030) | The declaration stays one declaration, and the rest of the layout is today's fixed point | Any split, copy, or layout change, under LF or CRLF, on the first pass, the second pass, or a phase run alone | `AssertRuleResult` (exact output, both line endings, second pass, `PhaseStabilityCheck`, `PhaseTreeRoundTrip`) | adequate |
| FDS `CommentThatSpansLinesBetweenAModifierAndTheTypeKeepsTheDeclarationUnsplit`, `DocumentedDeclarationWithADocumentedLaterDeclaratorIsNotSplit` | Not split, and stable | Declarator names differ after formatting, or the second pass differs | `AssertDeclarationIsNotSplit` | adequate for FMT-STRUCT-030, which decides only that the declaration stays whole. The outputs were measured: comments kept, stable. |
| `ReihitsuFormatterSplitsFieldDeclarationTests` rows (FMT-PIPE-031) | The predicate's answer per slot, in `Parse` and `None` | A wrong boolean | direct call | adequate. No row covers the members of finding #1. |
| RH7101 `…WithholdsTheFix` (RH7101-007) | Diagnostic reported, no action registered | Any action, or a missing diagnostic | `VerifyNoCodeFixIsOffered` (asserts reported diagnostics, then that no action is registered) | adequate |
| RH7101 `…DoesNotWithholdTheFix` | Fix offered, output as specified | A different output, or no fix | `Verify` + `AssertSingleFixUnderBothLineEndings` | adequate |
| RH7101 `FixAllSplitsOnlyTheDeclarationWithoutACommentInThePartEveryFieldRepeats` | Fix All and iterative fixes split only the unguarded declarations; the guarded one keeps its diagnostic | The guarded declaration is split, or its diagnostic is lost | `Verify` with `MarkupMode.Allow` on `FixedState`/`BatchFixedState` | adequate. 06 showed it red against `1ac2927^`. |

## thoenissen/Reihitsu#1384

### Findings
_None._ No other finding of the run belongs to this issue; the run's only finding, #1, is listed under #963.

- `IsDocumented` reads the trivia kind in the declaration's leading trivia. That matches SHARED-DOC-004, FMT-STRUCT-030 bullet 1, and the removed RH7101 copy, span for span.
- A `///` or `/**` behind the previous member's code lands in that leading trivia in `Parse`/`Diagnose`, so it is covered.
- In `None` the comment is ordinary trailing trivia of the previous member, so the split is unchanged.
- A doc-style comment after an attribute list is blocked by the comment guard in every mode.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Documented declaration (`IsDocumented`) | The declaration's leading trivia (the first token's leading trivia), documentation-comment trivia kinds | SHARED-DOC-004, which names the documentation comments as the `///`/`/** */` in the leading trivia, the mode encoded by the trivia kind. Also the removed private check in RH7101's `TryGetFixableFieldDeclaration`. | exact | holds |
| `StartsLineInLayout` documentation branch (`ReihitsuFormatter`) | The leading trivia of the member's first token | It is still reached for the member behind the target, because a documented target no longer splits | agree | holds. No dead branch. |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| A documented field declaration keeps its fields together | `FieldDeclarationSplitTransform.IsDocumented` only. The RH7101 copy was removed (decision 5). `rg "LeadingTrivia.Any(SyntaxTriviaUtilities.IsDocumentationCommentTrivia)"` also finds `BlankLineTokenCleanupRewriter.HasDocumentationCommentInLeadingTrivia`, which serves a blank-line decision, not this one. | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| FDS `Documented…AreNotSplit` family, the `…ModeDiagnose` and behind-code variants, `SpecExampleDocumentationStyleCommentsAreOrdinaryCommentsInDocumentationModeNone` | Not split in `Parse`/`Diagnose`; split in `None` | A split, or a missing split | `AssertRuleResult` with explicit parse options where needed (FC-033) | adequate |
| `StructuralTransformSeparatorTests.DocumentedDeclarationIsLeftAsOneDeclaration` and the `None` replacement | The phase output alone | A different phase output, or a different tree | `AssertSplitFields` (exact text, plus `AssertSameTree` with the same parse options) | adequate |
| `RH7103…CodeFixLeavesADocumentedCombinedFieldOfTheTypeUnsplit` (+ CRLF) | A whole-type fix leaves the documented field whole | `X, Y` split | `Verify` + `ApplyDocumentationModeParseToTestProject` | adequate. The multi-line `<summary>` avoids pinning a layout that RH7103 lists as a non-goal; red on `main` per 04. |
| `ReihitsuFormatterFieldSplitTests`: four rewritten tests, nine deleted | FMT-PIPE-029 on targets that are still reachable | – | `AssertSplitsTarget` | adequate. The deleted inputs have a documented target, for which `SplitsFieldDeclaration` is now false, so no caller passes them (AGENTS.md, *Supported inputs*). The behind-the-target side stays covered by the `MemberBehind…InTheGapBehind…` tests and `FirstFieldBehindACommentThatSpansLinesInTheGapInFrontKeepsItsPlace`. |

## Whole run

### Visible output is decided
| Expected value | Covered by |
|---|---|
| A documented declaration stays whole (formatter, RH7103 whole-type fix, RH7101 withheld) | Maintainer answer on #1384 (`00-lead.md`). FMT-STRUCT-024 and SHARED-FIX-003, unchanged. |
| A comment in a repeated slot keeps the declaration whole | FMT-STRUCT-024, unchanged, and #913 Q3. Decision 3 (own-line comment after an attribute list) rests on the unchanged FMT-STRUCT-024. |
| Own-line comment in front of the first name: split | Maintainer answer A |
| One-line `<summary>` expanded on an unsplit declaration | Decision 8: today's documentation-phase output for an unsplit declaration (its single-declarator twin on `main`) |
| FMT-STRUCT-022 realigned example (`// The coordinates`) | Unchanged behavior of FMT-STRUCT-022 for an ordinary leading comment |
| FMT-PIPE-029 rewritten tests | The same output on the behind-the-target side as before |

No rule page changed. Nothing undecided was found among the expected values. The members of finding #1 have no test, so they are not an `undecided output` row.

### Comments and conventions
- Changed summaries (`Splits`, `IsDocumented`, `SplitsFieldDeclaration`, `TryGetFixableFieldDeclaration`) describe the new code. The exception is the `CarriesCommentInRepeatedPart` line boundary, which is part of finding #1.
- No `TODO`, no commented-out code, and no tracker numbers in the changed source.

### Scope
No edit outside the run.

### Failure classes dismissed
- FC-003: both sides of every boundary are tested (leading trivia against the first token's trailing trivia; the type's trailing trivia against an own-line first-declarator comment; inside and behind the first declarator). The one-sided member is finding #1.
- FC-005, FC-019, FC-020: directives keep their own guard, `CarriesDirective`, which is unchanged and walks every trivia. The comment guards are not taken to cover directives. The existing tests `DirectiveBeforeFirstDeclaratorLeavesTheFieldIntact` and `DisabledTextBeforeFirstDeclaratorLeavesTheFieldIntact`, and the predicate rows 130–132, cover them.
- FC-006, FC-022, FC-023, FC-024, FC-025, FC-029, FC-036: no rewrite of literals, identifiers, members, or semantics. The change only refuses a split.
- FC-007, FC-009: the fix is offered exactly where `SplitsFieldDeclaration` holds, so where it can change the document. Convergence is unchanged (one iteration in the `apply-fix` runs).
- FC-008: Fix All was traced (guard-scope row) and is tested.
- FC-010, FC-011, FC-012: second passes were run on every probe output (`Formatted 0 of 5`, `0 of 4`). No loop was added. The only phase interaction that crosses the new boundary is the documentation phase moving a doc-style comment off the type's line, which is the constraint in finding #1.
- FC-013: RH7101 is an order/content rule. RH5041 now reports comments next to a separator in newly unsplit declarations (e.g. a `///` in front of a later declarator of a documented declaration), but RH5041 has no code fix, so SHARED-DIAG-010 does not cover it.
- FC-014: the formatter owns field splitting; nothing new.
- FC-015: one owner per policy (tables above).
- FC-016: all four container visitors call one predicate, and the nested, interface, struct, record, and record struct containers are tested. Event fields (`EventFieldDeclarationSyntax`) and locals are never split.
- FC-017: helpers are as strong as the requirements (tables above).
- FC-018: no rename.
- FC-021: covered: a comment between modifiers, a `/* */` spanning lines between a modifier and the type, an end-of-line `//` behind the type.
- FC-026: `token == firstDeclaratorToken` compares token identity (parent and position), not text.
- FC-027: nested types are tested. The guard is per declaration.
- FC-028: no caller formats a field declaration detached.
- FC-030: unchanged. The RH7101 fix writes back only the target's fields.
- FC-031: covered above. The unchanged `CreateSplitField` comment is listed under *Hints*.
- FC-032: the validator runs the full projects after this audit. 05 ran the full formatter, analyzer, architecture, and CLI projects on the production commit.
- FC-033: `Parse`, `Diagnose`, and `None` are set explicitly where they matter. The RH7103 test sets `Parse`.
- FC-034: the reproductions use the reported shapes: `private int /* c */ _first, _second;`, the issue-comment shapes, and the RH7103 input with a multi-line summary (see the table above).
- FC-035: the tests were red before the production commit (04, 06).
- FC-037: no text-only claim.

## Architecture findings
_None._ Finding #1 is a guard whose boundary differs from its requirement's. The documentation phase's move is a constraint on the repair, not a second owner of the decision.

## Hints
- Pre-existing layout that this run newly reaches for documented and comment-guarded combined fields: a wrapped declarator list that stays whole puts the continuation line at the member column. `/// …⏎public int X,⏎           Y;` gives `public int X,⏎    Y;`. The same happens on `main` for locals (`int a,⏎            b;` gives `int a,⏎        b;`) and for a declaration kept whole by a directive. No formatter requirement decides a declarator list's continuation indent. This is the same family as the follow-up candidate in `00-lead.md` (line break between the type and the first declarator), which the lead may widen to cover it.
- Pre-existing: the documentation-comment phase moves a doc-style comment that documents nothing onto a line of its own and writes a blank line in front of it, inside a declaration. Examples: `/** c */` behind a type, giving `private int⏎⏎/** c */ _first`; and behind an attribute list, giving `[System.Obsolete]⏎⏎/** c */ private int …`. The output is stable. It is unrelated to the split decision except as the constraint in finding #1.
- `FieldDeclarationSplitTransform.CreateSplitField` (unchanged) still says "comments between the type and declarator remain in that slot". With the new guard, only the first declarator's own leading trivia reaches that code.
