Role: auditor
Commit: f133f2fc3c32440cd675d4192ca88d0655731b33
Verdict: findings

My prompt carried no intended fix, no suspected cause, and no hoped-for verdict. I read `00-lead.md` (answer 2 = B), `09-auditor.md`, and handovers 10 to 14 as claims, and checked them against the code, the specs, and runs.

## Scope

- Head `f133f2fc3c32440cd675d4192ca88d0655731b33`. This is the checkout, and the working tree was clean before this handover.
- Base `origin/main` is `026fb648e4457763a9b15d559500f2eba07dcda5`. `git rev-parse origin/main` matches the prompt, and the branch contains the base (`git merge-base --is-ancestor`).
- `scripts/lane-check.sh bug-fix` reports `confirmed — claimed bug-fix, diff needs bug-fix`.
- This is a repair delta against `4ad21a7999f9150960eda8a256aeef653f1848f1`. The scope did not grow. The diff is what handovers 10 to 13 describe:
  - specs: FMT-STRUCT-022, FMT-STRUCT-030, FMT-PIPE-031, RH7101-007;
  - three test files (11);
  - documentation comments only, in `FieldDeclarationSplitTransform.cs` and `ReihitsuFormatter.cs` (12, 13).
- `scripts/verify-text-only.sh --base 4ad21a7 --head HEAD` reports both production files as `text-only` (tokens identical; changed trivia: comments, documentation, layout). The guard's code is byte-identical to the first audit, so I reuse the first audit's evidence for everything the guard does outside finding #2.
- Fixtures are in `/tmp/claude-0/-home-user-Reihitsu/97d9a81b-01a7-58f5-89e4-4ec128033620/scratchpad/r1/`, outside the repository. I ran them with `scripts/format.sh` (the CLI path, documentation mode `Parse`, plus a second pass) and with `scripts/apply-fix.sh RH7101` (LF and CRLF).

### Previous findings
| # | Status | Why |
|---|---|---|
| 1 | closed | Every member that #1's enumeration listed is now on the split side in the specs and in the code. The members are `///` or `/** */` behind the type's last token on the type's line, in `Parse`/`Diagnose`, with the first declarator on the same line or the next. The spec changes are in FMT-STRUCT-030 bullet 3, FMT-PIPE-031, and RH7101-007, following answer B. The new tests pin these members (see *Assertion adequacy*). Its summaries were rewritten. One member that #1 did not list still fails; it is finding #2. |

## thoenissen/Reihitsu#963

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | high | `FieldDeclarationSplitTransform.CarriesCommentInRepeatedPart` (code unchanged, and its new summary); the `Splits`, `SplitsFieldDeclaration`, and `CreateSplitField` comments; set against FMT-STRUCT-030 bullets 2–3, FMT-PIPE-031, RH7101-007 | FC-002, FC-001, FC-031 | #1 | same requirement | See *Finding #2* below. | See *Finding #2* below. |

### Finding #2

**What goes wrong.** The amended FMT-STRUCT-030 states this rule: "An ordinary comment (`//`, `/* */`) behind the type on the type's line keeps the declaration from being split in every documentation mode, also when the first declarator starts the next line." Bullet 2 also lists "behind the type on the type's line" as blocking. RH7101-007 says the fix is withheld for such a comment.

The code does not draw the boundary on the line. It draws it at trivia ownership. In `Parse` and `Diagnose`, the lexer ends the type's trailing trivia at the first `/**` comment. Everything behind that comment on the type's line becomes the first declarator's leading trivia, ordinary comments included. That trivia is skipped by the guard.

The new summary of `CarriesCommentInRepeatedPart` says "An ordinary comment behind the type on the type's line … [is] trailing trivia of the type's last token and count[s]". For these inputs that is false.

Measured on the head, documentation mode `Parse`:

| Input | Formatter output (second pass: `Formatted 0 of 4`) | RH7101 (`apply-fix`, LF and CRLF) |
|---|---|---|
| `private int /** c */ /* d */ _first, _second;` | `private int⏎⏎/** c */ /* d */ _first;⏎private int _second;` | fix offered, same output |
| `private int /** c */ // d⏎        _first, _second;` | `private int⏎⏎/** c */ // d⏎_first;⏎private int _second;` | fix offered, same output |
| `private int /** c */ /* d */⏎        _first, _second;` | split the same way, with `/** c */ /* d */` on the first field | fix offered, same output |

In each case, an ordinary comment that the spec says describes the repeated part is kept on the first field only. FMT-STRUCT-030's `Why` names that result as what FMT-STRUCT-024 refuses: the comment is separated from the other fields it described. The fix is offered where RH7101-007 says it is withheld.

**Class enumeration.** The repair added one axis to #1's enumeration: the sequence of comments on the type's line. The other axes are unchanged: comment kind, documentation mode, slot, and the line the first declarator is on.

Failing members, all in `Parse` or `Diagnose`:
- a `/** */` behind the type on the type's line, followed on that line by a `/* */` (single-line, or one that starts there and spans lines) or by a `//`;
- the first declarator on the same line or on the next line.

Members that hold:
- A `/* */` or `//` directly behind the type, in every mode. It is trailing trivia of the type's last token, so the guard blocks. `private int /* d */ /** c */ _first, _second;` was measured: not split.
- Every comment after a `///`. A `///` runs to the end of the line, so nothing can follow it on the type's line.
- Two `/** */` comments in a row. Both are doc-style, which matches bullet 3.
- Every combination in mode `None`. All of it is trailing trivia of the type's last token, so the guard blocks (traced).
- A `/** */` followed by an ordinary comment behind an attribute list or a modifier. All of it is leading trivia of a repeated token, so the guard blocks (traced).
- An ordinary comment on a later line than the type's last token, behind a `///` (`private int /// c⏎ /* d */ _first, _second;`). The output is split, which bullet 3's first kind requires (measured).

**Constraint for the repair.** This is the same as for #1.
- Making the code follow the spec literally would mean blocking on an ordinary comment in the first declarator's leading trivia before its first line break. That is not idempotent.
- The documentation-comment phase moves the whole run off the type's line, even when the declaration stays whole. `/// <summary>x</summary>⏎private int /** c */ /* d */ _first, _second;` is formatted to `private int⏎⏎/** c */ /* d */ _first, _second;`.
- On the second pass, `/* d */` starts on a later line, so the declaration would be split.

The route that fits answer B is to amend the specs instead. Bullet 3's second kind would read: in `Parse`/`Diagnose`, a `///` or `/** */` comment behind the type on the type's line, and every comment behind it on that line. The ordinary-comment sentence and RH7101-007 would get the same exception. The `CarriesCommentInRepeatedPart`, `Splits`, `SplitsFieldDeclaration`, and `CreateSplitField` comments would state that boundary.

Today's output for these members is identical to `main`'s, and it is the output that answer B showed for the doc-style comment alone. The lead decides whether answer B covers a comment that rides behind the doc-style one, or whether this is an output question. A test for one member on the formatter side and one on the RH7101 side would pin it.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Comment in the repeated part (`CarriesCommentInRepeatedPart`, code unchanged) | Same as in `09-auditor.md`: the trivia of every token from the first token up to the first declarator's first token, without the first token's leading trivia and without the first declarator's leading trivia. | (1) `CreateSplitField`: exact, as in 09. (2) The amended FMT-STRUCT-030 bullets 2–3, FMT-PIPE-031, RH7101-007. | (2) Now agrees for `///` and `/** */` behind the type (closes #1), and for both sides in `None`. Still disagrees for an ordinary comment that follows a `/** */` on the type's line in `Parse`/`Diagnose`. | Finding #2 |
| RH7101 fix offered | `SplitsFieldDeclaration` only | `Splits` inside the structural phase | agree (unchanged since 09) | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether a field declaration is split | `rg -n "SplitsFieldDeclaration\|FieldDeclarationSplitTransform.Splits\|Splits\(" src --glob '!*Test*'`: one owner, `FieldDeclarationSplitTransform.Splits`, as in 09. The repair changed no code. | one owner |
| Statement of the comment boundary | Four comments now enumerate it: `CarriesCommentInRepeatedPart`, `Splits`, `SplitsFieldDeclaration`, `CreateSplitField`. Three specs state it: FMT-STRUCT-030, FMT-PIPE-031, RH7101-007. All seven repeat the same two-kind enumeration, and all seven miss the member of #2. | consistent with each other; wrong together (finding #2, *Architecture findings*) |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| FDS `SpecExampleDocumentationStyleCommentBehindTheTypeStaysWithTheFirstField`, `DelimitedDocumentationStyleComment…NextLine…`, `SingleLineDocumentationStyleCommentBehindAGenericType…Diagnose` | A doc-style comment behind the type (Parse/Diagnose) is split with the comment in the first field only | No split, a copy on `_second`, or a layout change, under LF/CRLF, on the second pass, or in a phase run alone | `AssertRuleResult` with explicit parse options (FC-033) | adequate. 11 showed all of them red under the literal-spec guard. The layout inside the first field is pinned only as the spec example's whole output; FMT-STRUCT-030 says it is not its decision. |
| FDS `SpecExample…KeepsTheDeclarationUnsplitInDocumentationModeNone` | In `None`, both declarations stay whole | A split | `AssertRuleResult` (fixed point), `None` | adequate (other side of the boundary, FC-003) |
| `ReihitsuFormatterSplitsFieldDeclarationTests`: the extended example pair and `ReturnsTrueForADocumentationStyleCommentBehindTheTypeUnlessDocumentationCommentsAreNotParsed` (6 rows × 3 modes) | The predicate's answer per mode | A wrong boolean in any mode | direct call | adequate. No row covers the member of #2. |
| RH7101 `SpecExampleDocumentationStyleCommentBehindTheTypeDoesNotWithholdTheFix`, `SingleLineDocumentationStyleCommentBehindTheTypeDoesNotWithholdTheFixInDocumentationModeDiagnose` | Fix offered, output as specified | No fix, or a different output | `Verify` with `Parse` + `AssertSingleFixUnderBothLineEndings` | adequate |
| RH7101 `DocumentationStyleCommentBehindTheTypeWithholdsTheFixInDocumentationModeNone` | Reported, no action, in `None` | An action, or a missing diagnostic | `VerifyNoCodeFixIsOffered(…, None)` | adequate. LF only; the predicate is line-ending independent, and the formatter tests cover CRLF. |

## thoenissen/Reihitsu#1384

### Findings
_None._ The repair did not touch `IsDocumented`, its summary, or the RH7103 regression test. The first audit's evidence holds byte for byte. The new FMT-STRUCT-030 text keeps the statement that a doc-style comment in the first declarator's leading trivia is not a documentation comment of the declaration (SHARED-DOC-004), as the code reads it.

### Guard scope
_N/A — the documented-declaration guard is unchanged since `09-auditor.md`, where it holds._

### Policy ownership
_N/A — unchanged since `09-auditor.md` (one owner, `IsDocumented`)._

### Assertion adequacy
_N/A — no test of this issue was added or changed in the repair._

## Whole run

### Visible output is decided
| Expected value | Covered by |
|---|---|
| `/** */` or `///` behind the type, `Parse`/`Diagnose`: split, comment in the first field only; RH7101 fix offered | Maintainer answer 2 = B (`00-lead.md`); decision 13 for the layout inside the first field, which is the output the maintainer chose from |
| The same in `None`: not split; fix withheld | FMT-STRUCT-024 and FMT-STRUCT-030 bullet 2 (ordinary comment behind the type), unchanged in substance |
| FMT-PIPE-031 `_g, _h` true in `Parse`, false in `None` | Answer B (predicate follows the split) |

No rule page changed. No test expects an output for the member of #2, so it has no `undecided output` row. Its output question, if the lead decides it needs one, is described in *Finding #2*.

### Comments and conventions
- The rewritten summaries describe the code for every member except the one in finding #2.
- No `TODO`, no commented-out code, and no tracker numbers in the changed source.

### Scope
No edit outside the run.

### Failure classes dismissed
I reuse the first audit's dismissals. The guard code is byte-identical, and the repair changed only specs, tests, and comments. Delta for this repair:
- FC-003: both sides of the doc-style boundary are now tested, in `Parse`/`Diagnose` and in `None`. The one-sided member is finding #2.
- FC-010, FC-011, FC-012: second passes on every probe output were stable (`Formatted 0 of 4`). The non-idempotency risk applies only to the code route for #2 and is stated there.
- FC-017: helpers are as strong as the amended requirements (table above).
- FC-033: every new test sets its documentation mode explicitly.
- FC-035: the new tests are green by design (answer B keeps the behavior). 11 showed that they discriminate, by making every split-side test fail under the literal-spec guard.
- FC-037: the text-only claims of 12 and 13 hold. `verify-text-only.sh` reports both production files as `text-only`.

## Architecture findings

Finding #2 repeats the class of #1. The decision is the same one, failing between the same components:

- the split guard `CarriesCommentInRepeatedPart`, which decides by trivia ownership;
- the requirement text in FMT-STRUCT-030, FMT-PIPE-031, and RH7101-007, which decides by comment kind, line, and documentation mode.

The structural cause is that one rule is stated in two forms that can drift apart. The repair enumerated the members it knew into the line-and-kind form. The lexer's attachment rule has one more consequence that the enumeration does not name: every comment behind a `/** */` on the type's line also goes to the first declarator.

The documentation-comment phase moves the first declarator's leading trivia off the type's line. Because of that, the code cannot follow a line-based statement and stay idempotent. Only the requirement text can move.

A structural fix would state the boundary once, in the form the code decides it. One way to write it: "a comment the compiler attaches to the first declarator: everything from the first `///` or `/** */` behind the type (Parse/Diagnose), and everything that starts on a later line than the type's last token". The four code comments would then point at that statement instead of re-enumerating it.

## Hints
_None_ beyond the follow-up candidates already in `00-lead.md`. The doc-phase move of a run of comments off the type's line (the constraint in #2) is the second candidate there.
