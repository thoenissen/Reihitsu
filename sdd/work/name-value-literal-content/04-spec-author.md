Role: spec author
Commit: b4a7cf8
Verdict: spec written

This revision applies the maintainer's answer recorded in `squad/decisions/2026-10-18-shared-diag-003-literal-value.md`. It also keeps the earlier answers in `2026-10-18-shared-diag-003-comment-subject-rules.md` and `2026-10-18-using-rules-name-by-value-filing.md`, and weighs the non-blocking findings in `03-skeptic.md`. Everything in `01-spec-author.md` and `02-spec-author.md` still holds unless this file says otherwise. My prompt carried no intended fix or hoped-for verdict. No change doc is needed: this is still a clarification. `documentation/rules/` is unchanged, for the reason given in `01-spec-author.md`.

## Changed requirements

- **SHARED-DIAG-003 — Only the text of a literal or a comment is content — clarified**
  - **The fix clause is gone.** The normative sentence now covers only what is reported: a rule "MUST NOT report anything inside it". "Checked and fixed like any other code" now reads "checked like any other code". A new non-goal points the fix side to `shared/code-fixes.md`. It names no requirement ID, because SHARED-FIX-003 (thoenissen/Reihitsu#1068) does not exist on this branch, and a non-goal pointer is not a `Uses` entry.
  - **New paragraph on value and spelling.** The text of a literal protects the literal's value. How the literal spells that value is the literal's own syntax. Examples given: an escape such as `\t` or `{{`, and the raw-string indentation the compiler removes. The exemption paragraph adds: "A rule whose subject is a literal, such as RH5202, checks how the literal spells its value." Per the decision, the fix-side half ("a fix may respell") is not stated here.
  - **Why** now says why spelling may be judged by a rule about literals: the value stays the same however it is spelled.
  - **The example blocks are now diagnostics-only, each a single block**, because the requirement covers only what is reported (`sdd/README.md`, analyzer spec format):
    - RH6002: the second line is now `string.Concat("// c"{|RH6002:,|}"d")`. This is FC-022: comment-looking text inside a literal does not hide the comma after it.
    - RH6003.
  - **The negative block now covers RH5023 only.** The vacuous RH6021 part is dropped (skeptic, FC-017), and so is the format-clause line that went with it.
  - **New RH5601 block (`→` marks a tab):**
    - Not reported: a tab in the text of a regular, verbatim, UTF-8, single-line raw, and character literal; in the text of an interpolated string; in a format clause (`{2:→D3}`); and in `//`, `/* */`, `///`, and `/** */` comments.
    - Reported: a tab in a hole's expression (`{1→+ 2}`) and in its alignment clause (`{1,→5}`).
    - This gives the literal-kind, comment-kind, format-clause, and hole qualifiers an example on each side (FC-002, FC-003). It replaces the RH6021 format-clause case with a falsifiable one.
  - **New RH5202 block**, the literal-subject side of the exemption: a raw string whose closing `"""` is not in the column of its opening quotes is reported, with the whole literal as the span. An aligned one is not reported.
  - **Bullet wording:** "a line … is part of the literal, not a blank line" now reads "is part of the literal's text". The comment bullet says the same for comments. This no longer restates SHARED-BLANK-001's definition of a blank line (skeptic, wording).
- **SHARED-DIAG-002 — A name is compared by its value — clarified**
  - **Value definition.** It was "a qualified name as its identifiers joined by `.`". It now reads "a qualified name without the whitespace and comments written between its parts". The old wording gave no value for `global::System.IO` or `System.Nullable<int>` (skeptic, FC-002). The new one removes only trivia and `@`, which is the maintainer's P3 wording. It takes no stance on whether `global::` is part of the value, which is #1105 territory.
  - **The qualified-name example now uses RH2003 instead of RH3103.** RH3103 binds through the semantic model and compares no name, so its test could not fail on a by-spelling comparison (skeptic). RH2003 compares the created type's syntactic name by `ValueText` before it binds:
    - `new @NotImplementedException()`, `new System . NotImplementedException()`, and `new System./* c */NotImplementedException()` are each reported.
    - The span is the type name.
    - A comparison by spelling drops the first case from the report.
  - **The RH8101 block is unchanged.**

`sdd/specs/requirement-ids.txt`, the header, the code owners, `Uses`, and `sdd/specs/README.md` are unchanged.

## How it was checked

- **Every example block passes today.**
  - Eight throwaway analyzer tests in a temporary worktree of `b4a7cf8`, one per block, using `{|RH####:…|}` markup with real tab characters: 8 passed.
  - Mutation check: I removed the RH5202 markup and one RH5601 marker, and both tests failed.
  - The worktree is deleted. Nothing from it is committed.
- **Architecture tests.** `scripts/test.sh --project architecture`, filtered to `SpecConsistencyTests`, `SpecStructureTests`, `RequirementCoverageTests`, and `RuleDocumentation`: 38 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, which names only SHARED-DIAG-002 and SHARED-DIAG-003. Their covering tests are the test implementer's job.

## Skeptic findings weighed and not adopted

- **The "derives other text from it, such as a file name" qualifier has no example.** Kept without one. RH4001 is the only such rule and it breaks the requirement today (#1071), so no passing example exists. Its falsifiable test arrives with that fix.
- **A contextual keyword (`@nameof(…)`) and RH3103's `Identifier.Text: "nameof"` check.** No added scope sentence. The skeptic found no compiling input where comparing by value and by spelling differ, so any qualifier I added could have no falsifiable example.
- **`/// <param name="@class">` for `int @class`.** Not added. Today's behavior follows the compiler (CS1572/CS1573), as the requirement's Why already says, and an extra example would read as if the attribute's text were a C# name.
- **RH3003 (reports `""`) and RH3204.** Not named in the spec.
  - RH3003 reports the whole literal and nothing inside its text, so the requirement does not touch it.
  - RH3204's report covers the whole interpolated string. Its `{{` → `{` rewrite is a fix-side matter, left to SHARED-FIX-003.
- **RH5601's own carve-outs:** a tab next to a string or character literal is not reported, and a tab in a comment on a directive line is reported (RH5601-001/002). The first conflicts with the "whitespace before and after a literal is code" bullet. The skeptic lists RH5601 among the rules tracked in #1071, #1074, and #1075, so per `00-lead.md` it is not a new open question. Neither case appears in an example here. The RH5601 block puts no tab next to a literal.

## Examples for the tests

Every block passes today. Tag each test with the requirement ID.

- **SHARED-DIAG-002:**
  - the RH8101 block;
  - the RH2003 block.
- **SHARED-DIAG-003:**
  - the RH6002 block;
  - the RH6003 block;
  - the RH5601 block (tabs; `→` in the spec);
  - the RH5023 negative block;
  - the RH3201 block;
  - the RH5202 block.

As before, the issue's own examples (`class @class`, `namespace A . B`, `"a";···`, `$"{new int[]{1}.Length}"`, the empty line inside a raw string) appear only as prose with no rule named. They break the requirements today and are tracked in #1071, #1074, #1075, and #1105. Do not turn them into tests in this run.

## Open questions to file

_None._
