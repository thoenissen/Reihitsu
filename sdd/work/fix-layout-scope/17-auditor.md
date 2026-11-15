Role: auditor
Commit: 1552682ddb966d43d14885601b5f58ebd140a713
Verdict: pass — #1140: pass (#2 closed); #1687: pass (#1 closed); #1748: pass (not touched by the repair)

The prompt carried no intended fix, suspected cause, or hoped-for verdict. M1–M9 and D1–D27 in `00-lead.md` are taken as binding.

## Scope

- Repair delta `8333c46..1552682`, against findings #1 and #2 of `11-auditor.md`.
- Head `1552682` is checked out with a clean tree. Base `origin/main` = `4838e9a03f0736d9e4593286d0f251f3a92fb704`. `git ls-remote origin refs/heads/main` returns the same SHA, and it is an ancestor of the head (merged). `scripts/lane-check.sh behavior-change` confirms the lane.
- The diff matches 12–16:
  - production: 5 files, +32/−16 (RH2001, RH5110, RH5111, the empty-type base, and RH5113 comments only);
  - tests: 6 test files (10 new SHARED-FIX-007 tests, 2 new RH5113-004 tests, 1 retagged);
  - spec: RH5113-004, its ID, and the change doc's *Affected requirements* and *Decisions*;
  - `sdd/work/`.
- **Scope grew, but only in the spec.** The repair adds one requirement, RH5113-004. I audited it in full: its text, `Uses`, ID, change-doc entry, code, and tests, and how it fits with SHARED-FIX-010/011. Nothing outside the repair diff changed, so the first audit's evidence for the rest of the tree stands.
- I trusted the cleaner's Release build (0 warnings) and the roles' focused test results, and re-ran no test.
- I ran probes with `scripts/apply-fix.sh` under LF and CRLF. The fixtures are in `/tmp/claude-0/-home-user-Reihitsu/ba68e92d-4c14-5cfe-a5c5-81fb8298ae73/scratchpad/au17/`, outside the repository.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Every member listed now holds, on both sides of the shared line, under LF and CRLF. **Tests in 13:** RH2001, RH5110, RH5111 property target, RH5412, RH5413. **Probes:** the same rules, plus RH5414, RH5415, and RH5416 on one line with siblings in front and behind (each separated, then `public interface I;`, `public record R;`, `public record struct R;`); RH5110 on a delegate, a nested generic class, an interface method, and a method with a body; RH5111 on an attributed property and a property with a property behind it; RH2001 with an initializer and siblings on both sides (`private int _value = 3;` on its own line). **Not broken, as decided:** a line that does not begin with a member of the same list, such as `class C { private int Value { get; set; } … }`, `class C { public void M<T>() where T : class { } }`, or `record R(int X) { private int _a; public int P … }`, stays shared. That is the helper's rare class (D27), and the formatted node is the same as before the repair. |
| 2 | closed | The repair took the second option of the suggested change. **Spec:** RH5113-004 records the withholding as a decision. **Comments:** the `CanApplyCodeFix` summary no longer mirrors the formatter, and the `FormatsThroughFormatter` override gains a `<remarks>` naming RH5113-004 (as `EmptyParenthesesCodeFixProviderBase` and `TypeMemberOrderingCodeFixProviderBase` already do). **Test:** retagged RH5113-004, with a corrected summary. **New tests:** the declared-shape counterexample from finding #2 (`int /* c */ _a⏎;`) is a new no-fix test, paired with the offered case. **Output:** both guards existed on `main` (`git show 4838e9a:…RH5113…CodeFixProvider.cs` lines 113 and 138), so nothing visible changes. |

## Findings

_None._

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH5110 `member` switch arm (`MemberDeclarationSyntax` → helper, other → `FormatNodeInDocumentAsync`) | The node `FindDeclaration` returns | The helper's `lineBeginsWithMember`/`SharesLineWithSiblingMember`, which need a member of the same list | A local function, which is not a member, keeps its old path. Probe `d.cs` changed only the local function, and `L`'s shared line stayed. A delegate, a type, and an interface method are members and take the helper (probes `a`–`c`) | ok |
| RH5111 `formattingNode is PropertyDeclarationSyntax` → helper | The innermost assignment, declarator, property, parameter, or enum member | `GetFormattingContextNode` returns the property itself for a property (the `_` arm), so the replaced call was `FormatNodeInDocumentAsync(property)`, which is the helper's default step | Same formatted node; only the shared-line break is added | ok |
| RH5111 / RH5110 / RH2001 / base `FindUnsupportedSyntax` at registration vs the node the helper formats (FC-043) | The original member (RH2001: the built field) | The annotated member the helper passes to the formatter | It differs only at the whitespace-only boundary gaps (`SharesLineWithSiblingMember` requires them), and every declared shape is a comment, directive, or disabled-text shape. Same answer | ok |
| RH5113 `FormatsThroughFormatter`, `FindUnsupportedSyntax(declarationNode)` | Document errors; the declaration | RH5113-004 | It is now a stated decision of the rule, not a borrowed shared guard. Its span equals what RH5113-004 names ("as `FindUnsupportedSyntax` answers for the declaration") | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Line break around a member that shares its line (SHARED-FIX-007) | `rg -n "FormatNodeInDocument(WithContext)?Async\(\|FormatMemberOnLinesOfItsOwnAsync\(\|ConvertBodyAsync\(" src/Reihitsu.Analyzer.CodeFixes`. **Through the helper:** the bases (empty type: new; expression body; attribute placement) and RH2001 (new), RH3202, RH3203, RH5110 (new, members), RH5111 (new, property), RH5113, RH5401, RH5404, RH5408, RH5417. **Whole-member formatting outside the helper:** only RH7101, which separates siblings on both sides on the head (probe `r7101`). **The rest format non-member nodes:** RH3004, RH3208, RH5101, RH5102, RH5205, RH5206, RH5301–RH5308, RH5403, the RH5111 non-property targets, and the chain helper | One owner for the break behind, and for the break in front apart from the attribute base's private copy (D23/FU5, already decided). No whole-member formatting call is left unrouted. |
| RH5113 withholding | RH5113-004 (spec), `RH5113…CodeFixProvider` (code) | one owner |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 10 `…ALineOfItsOwn` tests (RH2001, RH5110, RH5111, RH5412, RH5413; sibling in front and behind) | The changed member and its sibling each start a line; the member is laid out from its line's column | The sibling left on the line (13 shows all 20 runs failing on exactly that before the repair), or the RH5110 clause and block laid out from the shared column | `VerifyCodeFixWithEndOfLine`: exact text, LF and CRLF, re-analysis of the result | adequate |
| `RH5113…SpecExampleNoFixIsOfferedForADeclarationWithACommentInsideItsVariableDeclaration` | Reported, no fix, for a declared shape outside the semicolon gap | A registered action; or no diagnostic | `VerifyNoCodeFixIsOffered`, which verifies the marked diagnostic and no action, under LF and CRLF | adequate. The gap holds only a line break, so only the RH5113-004 guard can withhold the fix |
| `RH5113…SpecExampleFixIsOfferedForTheDeclarationWithoutTheComment` | The control: the fix is offered without the comment | No fix; or a different result | `VerifyCodeFixWithEndOfLine` | adequate |
| `RH5113…NoFixIsOfferedInAFileWithASyntaxError` (retagged) | Withheld with a syntax error, offered without it | A fix offered with the error, or none without it | `VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded` | adequate |

## Visible output is decided (check 10)

- **D25 tests.** Every expected value is demanded by the unchanged SHARED-FIX-007 *Shared line* bullet (the RH5417 example there).
- **RH5113 tests.** They keep `main`'s output: the fix is withheld with a syntax error and for a refused declaration, as on `main` under SHARED-FIX-010/011. The offered case (`int _a;`) follows RH5113-003, which the first audit already covered.
- **Rule pages.** None changed. `RH5113.md` needs nothing, because `general-notes.md` covers a diagnostic without a fix.
- **Result.** No undecided output.

## Failure classes dismissed

- **FC-001 / FC-002 / FC-015 / FC-043.** See the tables above.
- **FC-003 (one side of a boundary).** Each D25 member is tested on both sides. Lines that do not begin with a same-list member were probed. They stay shared, as D27 decides.
- **FC-004 / FC-019 / FC-020 / FC-021 (trivia in the gap).** The helper breaks only a whitespace-only gap, so a comment or directive between the siblings leaves the line shared (D18), and nothing is dropped. With the attributed RH5111 property, the attribute list kept its place and the formatter laid it out on its own line as the property's attribute.
- **FC-005.** RH5113's comment guard is unchanged. The analyzer never reports a directive or disabled text in the gap (first audit).
- **FC-006 / FC-022 – FC-025 / FC-036 / FC-018.** No token, literal, or rename logic changed. RH2001 still renames through `Renamer` before the replacement.
- **FC-007 (convergence).** Every probe converged in one iteration per diagnostic, under LF and CRLF.
- **FC-008 (Fix All).** RH2001, RH5111, and the empty-type base use `BatchFixer`. Overlapping edits arise only when two reported members share one line, which D24 dropped as a rare class. `BatchFixer` drops a conflicting action rather than merging it, so it corrupts nothing. RH5110 uses `DocumentOrderFixAllProvider`.
- **FC-009.** Each routed fix still changes the reported member.
- **FC-010 – FC-014.** No formatter code changed in the repair.
- **FC-016 / FC-027.** See finding #1's closure and the ownership row. RH7101 is the only other fix that formats a whole member, and it holds.
- **FC-017.** See *Assertion adequacy*.
- **FC-026.** The helper re-resolves the member through an annotation. RH2001 resolves the field through its own annotation.
- **FC-028.** `FormatNode` is not used in the repair.
- **FC-029.** Not involved.
- **FC-030.** The formatted node of each routed fix is unchanged (see *Hints* for RH5110 on an empty generic type).
- **FC-031.** Every changed summary describes the new code: RH2001, RH5110, RH5111, the base, and the RH5113 summary, remarks, and inline comment. The RH5113 test summary is corrected.
- **FC-032 – FC-035 / FC-037.**
  - 14 and 15 ran tests after the last compiled change.
  - The seeds are the first audit's own inputs.
  - Expected values come from the spec (13 confirms no patched copy was used).
  - No text-only claim is made.
- **FC-038 – FC-041.** No shape is declared, and the precheck is untouched.
- **FC-042.** The one guard that only serves declared shapes, RH5113's `FindUnsupportedSyntax`, is now RH5113-004's stated decision (D26). FU7 carries the question of deleting it.

## Architecture findings

_None new._ The in-front break still has two owners (D23/FU5).

## Hints

- **RH5111 declarator target.** `private int _a; private int _b =⏎    1;` becomes `private int _a; private int _b = 1;`, and the field stays on the shared line, while the property in the same shape is now separated. The fix formats the declarator, not the whole member, so this falls outside #1687's class ("a fix that formats a whole member"). It is FU1's reading question, like the enum members. The lead may add it to FU1 as a second example.
- **RH5110 on an empty generic type.** `private int _a; public class Inner<T> where T : class { }` becomes `public class Inner<T>⏎        where T : class;`: the fix also writes the semicolon body, which is RH5412's change. The repair does not cause this, because the formatted node is the same as before. The cause is the whole-declaration formatting D6 left as on `main` (FU3).
- **RH5110 tab case.** In a tab-indented line, the fix writes `\tpublic void M<T>()⏎     where T : class⏎ {⏎ }`, the same with or without a sibling. That is the unchanged SHARED-FIX-007 tab bullet (FMT-PIPE-017), not a repair effect.
- **RH5113 wording.** The `CanApplyCodeFix` summary says that removing the gap "would absorb the semicolon into the comment". That holds for `//`; a `/* */` comment would be deleted instead. The guard is right for both. This is wording only, and the previous summary said the same.
- **`FormatsThroughFormatter` on RH5113.** The property still reads `true` for a fix that formats nothing. The new `<remarks>` explains why, so this is acceptable. FU7 removes it if the maintainer chooses to offer the fix.
