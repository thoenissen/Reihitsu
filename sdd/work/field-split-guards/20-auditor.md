Role: auditor
Commit: 1cd4ec2e07a02a8ee4356489006e4fb64903517e
Verdict: pass, text cleanup

My prompt carried no intended fix, no suspected cause, and no hoped-for verdict. I read `00-lead.md` (answers 1 and 2, decisions 14 to 16), `15-auditor.md`, and handovers 16 to 19 as claims, and checked them against the diff, the specs, the code, and probes.

## Scope

- Head `1cd4ec2e07a02a8ee4356489006e4fb64903517e` is the checkout; the working tree was clean before this handover.
- Base `origin/main` is `026fb648e4457763a9b15d559500f2eba07dcda5`; `git rev-parse origin/main` matches the prompt, and `git merge-base --is-ancestor` confirms the branch contains it.
- `scripts/lane-check.sh bug-fix`: `confirmed — claimed bug-fix, diff needs bug-fix`.
- Delta against `f133f2fc3c32440cd675d4192ca88d0655731b33` (the structural change of decision 14; not a repair round). The scope did not grow. The diff is what 16 to 18 describe:
  - specs: FMT-STRUCT-022, FMT-STRUCT-030, FMT-PIPE-031, RH7101-007 (16);
  - tests: `FieldDeclarationSplitTests.cs`, `ReihitsuFormatterSplitsFieldDeclarationTests.cs`, `RH7101DoNotCombineFieldsAnalyzerTests.cs` (17, 18);
  - production: comments only, in `FieldDeclarationSplitTransform.cs` and `ReihitsuFormatter.cs` (18).
- `scripts/verify-text-only.sh --base f133f2f --head HEAD` reports both production files `text-only` (1097 and 3040 tokens identical; changed trivia: comments, documentation, layout). The guard code is byte-identical to the code audited in `09-auditor.md` and `15-auditor.md`, so their evidence for everything outside finding #2 and the architecture finding is reused.
- Probes, outside the repository in `/tmp/claude-0/-home-user-Reihitsu/97d9a81b-01a7-58f5-89e4-4ec128033620/scratchpad/a20/` (CLI via `scripts/format.sh`, two passes) and `/tmp/claude-0/-home-user-Reihitsu/97d9a81b-01a7-58f5-89e4-4ec128033620/scratchpad/probe/` (a console program against the Release `Reihitsu.Formatter.dll` and Roslyn, printing the type's trailing trivia, the first declarator's leading trivia, and `SplitsFieldDeclaration` in `Parse` and `None`).

### Previous findings
| # | Status | Why |
|---|---|---|
| 1 | closed | Closed in `15-auditor.md`; nothing in this delta reopens it. The `///`/`/** */`-behind-the-type members stay on the split side in FMT-STRUCT-030's derivation and in the tests. |
| 2 | closed | FMT-STRUCT-030 now puts every comment that starts behind the first `///` or `/** */` on the type's line (Parse/Diagnose) on the first declarator's side, ordinary comments included, which is what `CarriesCommentInRepeatedPart` does. FMT-PIPE-031 and RH7101-007 no longer state a contrary list. Every failing member that #2 listed (`/** c */ /* d */` same line; `/** c */ // d⏎`; `/** c */ /* d */⏎`) is pinned on the split side in the formatter and RH7101 tests, and the members #2 named as holding are pinned on the other side. |

## thoenissen/Reihitsu#963

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | low | `sdd/specs/formatter/structural-transforms.md`, FMT-STRUCT-030, the boundary paragraph ("The trailing trivia of the type's last token runs to the end of the type's line") and its first derived bullet ("every comment that starts on a later line than the type's last token") | FC-002 (wording) | – | same requirement | The lexer ends the type's trailing trivia at the first line break that is not inside a comment, not at the end of the type's line. A `/* */` comment that starts on the type's line and spans lines carries the trailing trivia onto the line where it ends. Probe, `Parse` and `None`: `private int /* a⏎   b */ /* c */⏎    _k, _l;` gives type trailing trivia `/* a⏎   b */`, `/* c */`, EOL, and first-declarator leading trivia of whitespace only. So `/* c */` starts on a later line than `int` but is in the repeated part, while bullet 1 assigns it to the first declarator. No verdict changes: the comment that spans lines is itself an ordinary comment in the repeated part (or, in `None`, a `/** */` one), so the declaration is never split either way (`SplitsFieldDeclaration` false, CLI output unsplit, second pass `Formatted 0 of 1`). FMT-STRUCT-022's placement never applies. The error is in an explanatory derivation of the one boundary statement, about a comment that never decides the outcome. | In FMT-STRUCT-030 only: "runs to the first line break behind the type that no comment spans", and in bullet 1, "every comment that starts behind that line break". No test, code, or other spec changes. |

Why `pass, text cleanup` and why `Repeats` is `–`: #3 is confined to spec prose. It does not reach compiled behavior, diagnostics, public API documentation, or rule metadata. For every input, the decision the requirement makes (split or not, fix offered or not) agrees with the code. That makes it different from #1 and #2, where the spec and the code split different inputs differently. The text also stands in the single statement that decision 14 created, so it is corrected in one place.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Comment in the repeated part (`CarriesCommentInRepeatedPart`, code unchanged) | Leading and trailing trivia of every token from the first token to the token before the first declarator's first token, without the first token's leading trivia; the first declarator's leading trivia is never inspected | (1) FMT-STRUCT-030 condition 2: "the trivia of the tokens in front of the first declarator … without the declaration's leading trivia … and without the first declarator's leading trivia". (2) `CreateSplitField`: the first declarator is carried over unchanged, and every later field repeats the tokens in front of it | (1) Exact. Its derivation of which comments that is agrees for every member that decides an outcome. The one non-deciding misstatement is #3. (2) Exact, as in 09. | holds |
| RH7101 fix offered | `SplitsFieldDeclaration` only (`RH7101DoNotCombineFieldsCodeFixProvider.cs:109`) | `Splits` inside the structural phase | agree, unchanged since 09 | holds |

Members probed or traced against the ownership statement on this head:
- `/*** c */` behind the type in `Parse`: the probe shows trailing trivia of the type, and the declaration is not split. It agrees with FMT-STRUCT-030 read as the compiler parses it. See *Hints* for the text definition in SHARED-COMMENT-001.
- `/** c */ /*** d */`: split, both comments in the first field.
- `/**/` behind the type: not split.
- `//// c⏎`: not split. `////` is not a `///` comment (SHARED-COMMENT-001).
- `/** a⏎ b */ /* c */` in `Parse`: split, both comments in the first field. Second pass stable.

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether a field declaration is split | `rg -n "SplitsFieldDeclaration\|FieldDeclarationSplitTransform\.Splits\|CarriesCommentInRepeatedPart\|IsDocumented\(" src --glob '!*Test*'`: one owner, `FieldDeclarationSplitTransform.Splits`. `ReihitsuFormatter.SplitsFieldDeclaration` delegates to it, and the RH7101 fix and `ReihitsuFormatter.cs:292` call it. | one owner |
| Statement of the repeated-part boundary | `rg -n "type's line\|first declarator\|repeated part\|every generated field repeats\|every field repeats" sdd documentation src`, excluding `sdd/work`. Normative statement: FMT-STRUCT-030 only. FMT-STRUCT-022, FMT-PIPE-031 and RH7101-007 refer to it and list no members; their examples illustrate. The `Splits`, `CarriesCommentInRepeatedPart`, `SplitsFieldDeclaration` and `CreateSplitField` comments state the ownership span and point to FMT-STRUCT-030. No rule page states it. Change docs under `sdd/changes/2026-11/` mention first declarators only in unrelated, unchanged records. | one statement; the architecture finding of 15 is resolved |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| FDS `SpecExampleCommentBehindTheFirstDocumentationStyleCommentOnTheTypesLineBelongsToTheFirstDeclarator` | The new FMT-STRUCT-030 example: the first two declarations are split with the comments only in the first field; the third is not split | Any split difference, a copied comment, a layout change, CRLF divergence, an unstable second pass, or an unstable phase | `AssertRuleResult`, explicit `Parse` | adequate |
| FDS `…KeepsTheDeclarationUnsplitInDocumentationModeNone` | In `None`, all three stay whole | A split | `AssertRuleResult` (fixed point), explicit `None` | adequate (other side, FC-003) |
| FDS six split-side siblings (Diagnose `/* d */⏎`, spanning `/* d⏎ e */`, `/** d */`, `/// c⏎ /* d */`, attributed, generic `// d` Diagnose) | Split, comments in the first field only, repeated tokens without the comments | Same as above | `AssertRuleResult`, explicit mode where the mode matters | adequate. 17 reports that all of them fail under a line-based guard. |
| FDS `OrdinaryCommentThatDoesNotFollowADocumentationStyleCommentOnTheTypesLineKeepsTheDeclarationUnsplit` (5 rows) | The declaration stays whole | Any change in the declarator list | `AssertDeclarationIsNotSplit` (LF, CRLF, second pass). The default parse options give `Parse`, which the summary names. | adequate. Only the split is decided, and `None` is covered on the predicate. 17 reports that 3 of the rows fail under a guard that ignores the type's trailing trivia. The two non-type-slot rows are correctly unaffected by that mutation. |
| `ReihitsuFormatterSplitsFieldDeclarationTests`: the two extended FMT-PIPE-031 example tests, the 9-row Parse/Diagnose/None split-side test, and the 6-row every-mode other-side test | The predicate's answer per mode | A wrong boolean in any mode | Direct call; the private `AssertSplitsOnlyWhenDocumentationCommentsAreParsed` is the unchanged former body (18, S4144) | adequate |
| RH7101 `SpecExampleOrdinaryCommentBehindADocumentationStyleCommentBehindTheTypeDoesNotWithholdTheFix`, `LineCommentBehind…InDocumentationModeDiagnose` | Fix offered; output equals the spec and the formatter | No action, or different text under LF or CRLF | `Verify` with `Parse` plus `AssertSingleFixUnderBothLineEndings` (explicit mode) | adequate |
| RH7101 `…WithholdsTheFixInDocumentationModeNone`, `SpecExampleOrdinaryCommentInFrontOf…WithholdsTheFix` (3 modes) | Reported, no action | An action, or a missing diagnostic | `VerifyNoCodeFixIsOffered(…, mode)` | adequate. LF only, and the offer is line-ending independent (as in 15). |
| RH7101 `FixAllSplitsTheDeclarationWhoseOrdinaryCommentFollowsADocumentationStyleCommentBehindTheType` | Fix All splits the owned-comment declaration and the plain one, and keeps the other whole with its diagnostic | A wrong split, or a lost diagnostic | `Verify` with `MarkupMode.Allow` on the fixed states | adequate |

### Failure classes dismissed
The guard code is unchanged. The first audit's dismissals hold byte for byte. The delta for this step:
- FC-001, FC-002: span and statement agree on every outcome. The non-deciding wording gap is #3.
- FC-003: both sides are tested for every new member, in every mode.
- FC-004, FC-005, FC-019 to FC-021: no rewrite changed. `CarriesDirective` is untouched.
- FC-006, FC-022 to FC-029, FC-036: no rewrite or semantics change.
- FC-007 to FC-009, FC-030: the RH7101 offer is still `SplitsFieldDeclaration` alone. The new RH7101 tests run the iterative and Fix All paths.
- FC-010 to FC-012: no phase changed. Every probe's second pass was stable.
- FC-013, FC-014: no analyzer or formatter disagreement introduced. RH7101 follows the formatter predicate.
- FC-015, FC-016: one owner, one statement (*Policy ownership*).
- FC-017: helpers match the requirements (*Assertion adequacy*).
- FC-018: no renames.
- FC-031: every changed summary and inline comment describes the unchanged code. See *Comments*.
- FC-032: no compiled production change since the last results.
- FC-033: every new test either sets its documentation mode or names the default `Parse` in its summary. CRLF is covered on the formatter side and on the RH7101 offered side.
- FC-034: the tests use the exact shapes of finding #2.
- FC-035: the expected values are main's unchanged outputs and the spec examples. 17's two mutations show the tests discriminate in both directions.
- FC-037: the text-only claim is proven by `verify-text-only.sh`.
- The tab-width class matrix cannot reach this change: there is no column computation.

## thoenissen/Reihitsu#1384

_Nothing to audit in this delta._ `IsDocumented`, FMT-STRUCT-030's first condition, RH7101-006, FMT-STRUCT-022's aligned example, and the RH7103 regression are untouched since `09-auditor.md`, where they hold.

## Whole run

### Visible output is decided
| Expected value | Covered by |
|---|---|
| `Parse`/`Diagnose`: a comment behind the first `///` or `/** */` on the type's line is split with the doc-style comment, kept in the first field only; RH7101 fix offered with the same output | Answer B as decision 14 applies it to the comments riding behind the doc-style one. The output is main's unchanged output (15), and the layout inside the first field is the one the maintainer chose from (decision 13). |
| An ordinary comment in front of the doc-style comment, or a run behind a modifier or attribute list: not split; fix withheld, every mode | FMT-STRUCT-024 (unchanged), which forbids copying or separating a comment in the repeated part |
| `None`: none of these split; fix withheld | FMT-STRUCT-024 (unchanged), as above. Same as answer B's `None` side. |
| FMT-PIPE-031 `_i, _j` true in `Parse`, false in `None`; `_k, _l` false in both | Follows the two rows above, through the predicate that FMT-PIPE-031 ties to the split |

No rule page changed. No `undecided output`.

### Comments and conventions
- `Splits`, `CarriesCommentInRepeatedPart`, the `CreateSplitField` inline comment, and the public `SplitsFieldDeclaration` summary describe the code. They state the ownership span without enumerating comment kinds, and point to FMT-STRUCT-030.
- There is no `TODO`, no commented-out code, and no tracker number in the changed source or tests.
- FMT-STRUCT-030's `Why` cites FMT-PIPE-004 (idempotency) for the line-boundary argument, which is the right requirement.

### Scope
No edit outside the run.

## Architecture findings

_None._ The finding of `15-auditor.md` is resolved. The repeated-part boundary is now stated once, in FMT-STRUCT-030, in the form the guard decides it (trivia ownership). FMT-STRUCT-022, FMT-PIPE-031, RH7101-007 and the four code comments refer to that statement instead of restating it. #3 is a wording error inside that single statement and creates no second form.

## Hints

- Pre-existing, outside the run: SHARED-COMMENT-001 (`sdd/specs/shared/comment-placement.md`, the "by its text" sentence) counts a `/* */` comment that starts with `/**` and is not `/**/` as a `/** */` comment. The compiler does not parse `/*** c */` as a documentation comment in `Parse` (probe: it is `MultiLineCommentTrivia` in the type's trailing trivia). So for `/***`, the text definition and the compiler disagree, although the requirement says the verdict must not depend on parsing. FMT-STRUCT-030 is decided by compiler attachment, so the field split is consistent. A reader who takes "`/** */` comment" in its derived bullets by SHARED-COMMENT-001's text would misplace `/***`. It is a follow-up candidate for SHARED-COMMENT-001, not this run's. Optionally, FMT-STRUCT-030 could say "a comment the compiler parses as a `///` or `/** */` documentation comment" when #3 is fixed.
- The two follow-up candidates already in `00-lead.md` (continuation column of a wrapped declarator list; the documentation-comment phase moving a doc-style run off the type's line) are visible again in the new examples and are unchanged.
