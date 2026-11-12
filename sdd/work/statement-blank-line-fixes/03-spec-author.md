Role: spec author
Commit: 6e81e97
Verdict: #946: spec written; #972: spec written; #1096: spec written; #1076: spec written; #1626: nothing to write; #1625: spec written

Follow-up amendment: carries the maintainer's answers 1A, 2A, 3A (`00-lead.md`, *Maintainer answers*) and the spec-wording items of decisions #5, #6, #10, #12 into the specs. No change doc: every behavior written here is either today's output (RH5005/RH5006 location, RH5011 `///`, the base's move behind a comment), a decided bug (#1076, #1096), or the maintainer's answer 3A for #1625. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Code owners: no row changed. The single owner of the blank line in front of a statement (#1626) is not decided yet — the implementer picks the type — so the *Code owners* rows of `shared/statement-blank-lines.md`, `shared/blank-line-after-brace-fixes.md`, and `sdd/specs/README.md` stay as they are and are a repair-round item once the type exists.

Rule pages: none changed. Nothing reported or how to fix it changes for the user in the normal case.

## thoenissen/Reihitsu#946

### Changed requirements

- RH5005-002 — The diagnostic is on the `using` keyword — clarified (decision #5): also for `await using (…)`; the diagnostic does not include `await`; the fix still inserts in front of `await` (SHARED-STMT-004). Title shortened from "… that starts the statement". Example added (`await {|RH5005:using|} (resource)` → blank line above `await`).
- RH5006-002 — The diagnostic is on the `foreach` keyword — clarified the same way, example with `await {|RH5006:foreach|}` over an `IAsyncEnumerable<int>`.

### Examples for the tests

RH5005-002, RH5006-002 (one application, LF and CRLF; also the same-line form and a comment line above, per 02-skeptic's matrix), SHARED-STMT-004.

### Output questions

_None._

### Internal choices

- The `await` examples sit in RH5005-002/RH5006-002 rather than SHARED-STMT-004, because the markup decides the location, which is each rule's own. Alternative: an `await` example in SHARED-STMT-004.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#972

### Changed requirements

- SHARED-STMT-004 — clarified (decision #12, comment wording): the comment cases are now three bullets. A comment that ends on the statement's line in front of it and starts a line — the statement's own line, or an earlier line when it spans several — gets the blank line above the line it starts on. This decides 02-skeptic's row `⏎/* a⏎b */ if` by text. A comment that starts behind the statement before it, holding every line break between the two statements, makes the fix move the statement below the comment's last line, with no whitespace left at its end. That third bullet writes down today's base output (verified with `scripts/apply-fix.sh RH5002`: `a++; /* a⏎b */ if (a > 7) { }` → `a++; /* a⏎b */⏎⏎if (a > 7) { }`, LF and CRLF, one iteration), and it is the shape answer 3A refers to.
- New example in SHARED-STMT-004 with all three comment shapes (RH5002).

### Examples for the tests

SHARED-STMT-004 — the comment example: the block comment and the multi-line comment that starts its own line are #972's failing rows (two blank lines on `main`); the third block is a boundary test that passes on `main`. Cover RH5002 and one other rule on the base (RH5008 or RH5029), LF and CRLF, plus the other failing rows of 02-skeptic's #972 matrix.

### Output questions

_None._

### Internal choices

- Wrote the third bullet as a requirement of the base, not only of RH3208, so SHARED-AFTERBRACE-002 can point at one stated output. Alternative: leave the base's behavior unwritten — rejected, answer 3A is defined as "the output of the single owner".

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1096

### Changed requirements

- SHARED-STMT-004 — clarified for answer 2A: the moved statement's column has two cases. Outside an object initializer or anonymous object, it is the column of the statement's nesting depth (FMT-INDENT-003). Inside one, it is the column at which the statements of its block or switch section start their lines as written; the fix does not predict the formatter's layout of the initializer. Behind a multi-line `case … when` label, it is the column of the section's statements (such as `break;`), not one read from the label's continuation line. The Why gives the reason for not predicting.
- SHARED-STMT-004 — the sentence "This requirement does not decide a switch section whose label stands on the line of the switch statement's opening brace" is deleted (decision #12, `sdd/README.md` *Open questions*: no note on an undecided case). The follow-up candidate in `00-lead.md` carries the question.
- SHARED-STMT-004 — the plain-method `case … when` example, which passes on `main`, is replaced by the failing initializer shape (RH5029 inside a lambda in an object initializer: `case` at column 20, continuation and `break;` at 24; `main` puts `var y` at 28, the spec puts it at 24). The closing sentence notes that the same input in a method body gives the same column.
- SHARED-BLANK-001 — clarified (decision #12): the `///` sentence is widened to "the line directly below a trivia whose own text ends with its line break — a `///` documentation comment or a preprocessor directive". A second example is added: `#endif⏎⏎// note⏎if` is not reported by RH5002. The Why now says why those lines count (the trivia carries its own line break). It also names the analyzers of RH5001–RH5019, RH5021, and RH5029 as users of `TokenGapAnalysis`, per decision #8. If the implementer reports that the structural change exceeds its budget and the analyzers keep `ContainsBlankLine`, that Why sentence has to go back to naming only the code fixes. That is a repair-round item for me.
- RH5011-001 — changed for answer 1A: the clause "holds no documentation comment" is deleted. A comment between `break` and the next statement, a documentation comment included, is not a blank line. A `break;⏎/// doc⏎a++;` line is added to the example, and the Why gains one sentence on `///` inside a method body. With 1A, #1096's RH5011 half (`break;⏎/// doc⏎⏎a++;` not reported) rests on SHARED-BLANK-001 alone. The SHARED-BLANK-001 `///` example still says RH5011 reports nothing there, which stays true.

### Examples for the tests

- SHARED-STMT-004 — the initializer example. It is the regression test: `RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests.VerifyDiagnosticOverIndentsWhenLabelSpansMultipleLinesAndPrecedingStatementSharesItsContinuationLine` pins the wrong column today. Add the other failing rows of 02-skeptic's Q1 matrix (`case 1⏎    when …`, two labels, a three-line guard, anonymous object, RH5002). Every test needs a statement such as `break;` that starts its own line in the section, since the column is read from it. The plain-method shape stays as a boundary test.
- SHARED-BLANK-001 — the `///` example and the new `#endif` example, each once with an empty line and once with a line of spaces. Also the `#pragma` rows of 02-skeptic's Q2 matrix, for RH5002 and for RH5011 (`// c⏎#pragma warning disable X⏎⏎a++;` after `break;`). Cover `///` under both documentation modes (FC-040).
- RH5011-001 — the new `/// doc` line: reported, fix gives `break;⏎⏎/// doc⏎a++;`. The existing RH5011-007 test (`/** a⏎b */`) is unchanged.

### Output questions

_None._

### Internal choices

- The initializer column is worded as "the column at which the statements of its block or switch section start their lines as written", the maintainer's answer, rather than as an algorithm, for example "the label's first-token line plus one level" or decision #9's "add the level only on a line that starts with the label's first token". The two agree on every row of the skeptic's matrix, where a label's continuation is indented one level deeper than `case`. They differ only for a continuation indented differently, and there the spec asks for the column of `break;`. Alternative: write decision #9's algorithm as the normative text. Rejected, because it would read the column from the continuation line, which is what the answer rejects.
- SHARED-BLANK-001 names `///` and directives, not disabled text: the line below disabled text is always a directive line, never blank. Alternative: list disabled text too.

### Follow-up candidates

_None_ beyond those already in `00-lead.md`.

## thoenissen/Reihitsu#1076

### Changed requirements

- RH5023-001 — clarified (decision #12): the end-of-file example now also shows `"class C\n{\n}\n\n    "` — a last line holding only spaces and no line break — as not reported.
- RH5026-005 — A blank line below a region directive is not reported — **added** (decisions #6, #10). Not reported when the nearest non-blank line above the blank line above `else`/`catch`/`finally` holds an active `#region` or `#endregion`. Example: `}⏎#region R⏎⏎else`. `SHARED-BLANK-006` and `SHARED-FIX-004` added to *Uses*.
- RH5027-005 — A blank line below a region directive is not reported — **added** (decisions #6, #10). Not reported below an active `#region`, nor below an active `#endregion` unless the footer's line begins with `}`. A blank line between `#endregion` and `} while` stays reported, as SHARED-BLANK-006 exempts `#endregion` only in front of `}`; RH5024-001 reports it too. The example shows `#region R⏎⏎while (…);` and `#region S⏎⏎} while (…);`. `SHARED-BLANK-006` and `SHARED-FIX-004` added to *Uses*.

IDs registered in `sdd/specs/requirement-ids.txt`.

### Examples for the tests

RH5023-001 (the spaces case, with no line break at the end), RH5026-005, RH5027-005 (both blocks, plus a boundary: `#endregion⏎⏎} while` stays reported by RH5027), and an RH5032 round trip on the RH5026-005/RH5027-005 inputs showing no cycle.

### Output questions

_None._

### Internal choices

- RH5026-005 needs no `}` exception: RH5026 considers only a line that begins with the keyword (RH5026-002), so the `#endregion`-before-`}` exemption of SHARED-BLANK-006 cannot apply.
- RH5027-001's sentence "A blank line above `} while` is reported by RH5024 as well" was left as written. RH5027-005 states the region exception and its relation to RH5024-001. Alternative: qualify the -001 sentence too.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1626

Nothing to write: one owner is internal. Its visible consequence is RH3208's output under SHARED-AFTERBRACE-002 (#1625). Code owners stay until the implementer has decided the type; see the top of this handover.

### Follow-up candidates

_None_ new. RH5030-003's Why ("a decision about where it goes relative to the comment that the author makes") now sits next to SHARED-AFTERBRACE-002 and SHARED-STMT-004, which make that decision. This is the existing follow-up "one answer for a statement behind a multi-line comment" (02-skeptic, decision #11); I did not change RH5030-003.

## thoenissen/Reihitsu#1625

### Changed requirements

- SHARED-AFTERBRACE-002 — A statement behind a comment that holds every line break moves below it — **added** for answer 3A. When every line break between the closing brace and the next statement lies inside a comment, the RH3208 fix moves the statement onto a new line below the comment's last line, with exactly one blank line, indented like its block's statements. No whitespace stays at the end of the comment's last line, and the comment keeps its text and place. The RH5030 fix withholds there (RH5030-003, unchanged). The Why points to SHARED-STMT-004's third comment bullet and the formatter for the same output. The example is the issue's input. `SHARED-STMT-004` and `RH5030-003` added to *Uses*.
- RH3208-002 — clarified: points to SHARED-AFTERBRACE-002 for that gap; `SHARED-AFTERBRACE-002` added to *Uses*.

IDs registered.

### Examples for the tests

SHARED-AFTERBRACE-002 — the issue's input, LF and CRLF, one application, Fix All. Boundaries from 02-skeptic's #1625 table: ` /* a */ L();` and ` L();` keep `L();` on the brace's line, and ` /* a⏎b */⏎L();` gives one blank line. Every existing RH5030-003 test stays as it is.

### Output questions

_None._

### Internal choices

- The requirement is in the shared after-brace spec rather than in RH3208, because the answer ties it to the #1626 single owner, which serves RH5030 too. Its text names RH3208 as the fix that moves and RH5030 as the one that withholds, so narrowing RH5030-003 later changes one sentence. Alternative: RH3208-004.

### Follow-up candidates

_None._

## Whole run

- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 50 passed, 1 failed (`EveryRequirementOfAnEnforcedSpecIsCovered`: RH5011-008, RH5025-007, RH5026-005, RH5027-005, SHARED-AFTERBRACE-002 have no tagged tests yet; expected until the test implementer adds them).
- Files changed: `sdd/specs/shared/statement-blank-lines.md`, `sdd/specs/shared/blank-line-spacing.md`, `sdd/specs/shared/blank-line-after-brace-fixes.md`, `sdd/specs/analyzer/RH3208.md`, `RH5005.md`, `RH5006.md`, `RH5011.md`, `RH5023.md`, `RH5026.md`, `RH5027.md`, `sdd/specs/requirement-ids.txt`.
