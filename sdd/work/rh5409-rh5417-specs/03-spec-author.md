Role: spec author
Commit: 22628d5
Verdict: spec written

This is a repair round on the skeptic's findings (`02-skeptic.md`). I re-ran each finding with `scripts/apply-fix.sh` (LF and CRLF agreed in every case) and confirmed all five. No requirement ID was added, retired, or removed: the ledger and the index are unchanged since `01-spec-author.md`, and the IDs stay dense. `scripts/test.sh --project architecture`, filtered to `SpecConsistencyTests|SpecStructureTests`, passes: 26 passed. The 21 IDs are still uncovered until the tests are tagged.

## Changed requirements

- RH5417-003 — the fix makes the braces of a member or accessor body start their own lines — clarified (F1).
  - Old wording: "put … on lines of their own".
  - New wording, FMT-BREAK-002's own: the opening and closing brace each *start* their own line, the statements are laid out as FMT-BREAK-006 does, and the accessor list's braces and accessors start their own lines (FMT-BREAK-025).
  - Dropped "formatting it changes nothing" from the Why.
  - The intro sentence of `RH5417.md` was adjusted to match.
  - Re-derived: `void M() { } int _x;` → `void M()` / `{` / `} int _x;`. `void M() { } void N() { }` → `} void N()` / `      {` / `      }`. `class C { void M() { } }` → `class C { void M()` / `{` / `} }`. `void M() { Foo(); ; }` → `Foo();;`.
  - In every case each brace starts its line, so the new wording holds. Nothing more about these inputs is specified; they are question 5 below.
- SHARED-EMPTY-004 — the fix is withheld where the formatter keeps the braces — changed wording only (F4).
  - It now references FMT-STRUCT-018 without restating its condition.
  - It says that a declaration SHARED-EMPTY-001 reports, such as one with a comment, stays reported. It does not say that a body holding a directive is reported (question 4).
  - Behavior is unchanged, so it keeps its ID.
- RH5413-001 and RH5415-001 — Why sentence — clarified (F5). Both now say "Record structs are checked by RH5416."

F5, owner of `EmptyTypeDeclarationSemicolonAnalysisUtilities`: it stays a code owner of `shared/empty-type-declarations.md`.
- `sdd/README.md` gives a type to the spec whose behavior it implements. `ShouldReport` is the reporting decision of SHARED-EMPTY-001/002.
- FMT-STRUCT does not list the type; FMT-STRUCT-018 only names `CanConvertSafely` in its Why.
- RH3202 owning `ExpressionBodyRewriteUtilities`, which FMT-STRUCT-007 names the same way, is the precedent.

F3: every body that holds a directive stays out of the spec. SHARED-EMPTY-001 covers only bodies with whitespace, line breaks, and comments. Question 4 now covers every directive kind and presumes no answer.

## Examples for the tests

Unchanged from `01-spec-author.md`, with these additions from the skeptic's probes:

- Leave untagged (characterization only):
  - any RH5417 input where the member shares its line with other code (question 5);
  - any input whose fix renames a `#region` (questions 6 and 7);
  - every directive-holding empty-type body, including `RH5416…VerifyDirectiveContainingRecordStructIsReportedWithoutCodeFix` (question 4);
  - `RH5412…VerifyBlankLineAboveCommentOfFixedClassInFileScopedNamespaceIsInserted` (question 2).
- SHARED-EMPTY-004's example blocks, comment-only, are the ones to tag.
- RH5417-003's example block is verified; tag tests of members that are alone on their lines.

## Open questions to file

Each question stands on its own. Issue names:
- `trailing-commas` = `SDD open questions: sdd/specs/shared/trailing-commas.md` (new);
- `empty-types` = `SDD open questions: sdd/specs/shared/empty-type-declarations.md` (new);
- `RH5417` = `SDD open questions: sdd/specs/analyzer/RH5417.md` (new).

### 1. Trailing-comma fixes leave whitespace on the comma's line — Decided by principle P10 (#912) — issue: trailing-commas

The RH5409/RH5410/RH5411 code fix removes only the comma's characters. Input → actual:

- `    B ,` before `}` → `    B ` (trailing space).
- `    /* c */ ,` on its own line → `    /* c */ ` (trailing space).
- `    , // c` on its own line → `     // c` (one column too far right).
- `    , }` → `     }` (one column too far right).

Why it looks wrong: formatting each result changes the line the fix edited, by removing the trailing space or re-indenting. Under P10, the code a fix changed is laid out as the formatter lays it out.

Requirements: SHARED-TRAIL-003; violates SHARED-FIX-007 (being added by #1138).

### 2. Empty-type fixes reformat code outside the empty body — Decided by principle P9 (#912) — issue: empty-types

The RH5412–RH5416 code fix formats the whole type declaration. Input → actual:

- `class C<T> : B where T : new()` / `{` / `}` → `class C<T> : B` / `    where T : new();`. The same for RH5414 with `interface I<T> : System.IDisposable where T : class`.
- `[Serializable][Obsolete]` / `class C` / `{` / `}` → the attribute lists split onto two lines.
- `    internal  sealed  class  C` / `{` / `}`, in a block namespace → `    internal sealed class C;`.
- `    ///<summary>x</summary>` above an empty class in a block namespace → rewritten to three lines.
- `namespace N;` / `// remark` / `class C` / `{` / `}` → a blank line inserted after `namespace N;`. Pinned by `RH5412…VerifyBlankLineAboveCommentOfFixedClassInFileScopedNamespaceIsInserted`.

Why it looks wrong: under P9 a fix changes only the code its diagnostic is about, the empty body.

Requirements: SHARED-EMPTY-003, FMT-PIPE-010; violates SHARED-FIX-006 (being added by #1138). Related: #1140, #969.

### 3. `class C { };` is neither reported nor converted — Decided by principle P11 (#912) — issue: empty-types

Input: `class C` / `{` / `};`. Actual: RH5412 reports nothing, and `scripts/format.sh` leaves it unchanged. The same holds for every kind RH5412–RH5416 check, because the shared check treats any semicolon after `}` as an existing semicolon declaration.

Why it looks wrong: it is an empty class with a brace body, the construct the rule names. FMT-STRUCT-017's wording ("MUST replace the empty brace body of a class …") covers it as well. Expected under P11: reported, and fixed to `class C;`.

Requirements: SHARED-EMPTY-001, FMT-STRUCT-017.

### 4. Is an otherwise empty type body that holds a directive reported? — open, no principle decides it — issue: empty-types

Inputs and actual output today (RH5412; the same for RH5413–RH5416). Each case is reported, no code fix is offered, and the formatter keeps the braces (FMT-STRUCT-018):

- `class A` / `{` / `#if NEVER` / `    int _x;` / `#endif` / `}`. In a configuration that defines `NEVER`, `A` has a member.
- `class A` / `{` / `#if DEBUG` / `#endif` / `}`. Pinned by `RH5416…VerifyDirectiveContainingRecordStructIsReportedWithoutCodeFix`.
- `class A` / `{` / `    #region R` / `    #endregion` / `}`.
- `class A` / `{` / `#pragma warning disable CS0001` / `}`.

Why it looks wrong: in the first case, no edit satisfies the rule without deleting a member of another configuration, so the warning can only be suppressed. The trailing-comma rules skip the analogous case on purpose (SHARED-TRAIL-002; "Exceptions" on the RH5409–RH5411 rule pages). Whether the other directive kinds follow is part of the same decision.

Competing readings, with the output each gives for the four inputs:

- (a) Report every one of them, without a fix — today's output for all four.
- (b) Do not report a body that holds a conditional directive or excluded code, as SHARED-TRAIL-002 does. Inputs 1 and 2 are not reported; inputs 3 and 4 are reported without a fix.
- (c) Do not report a body that holds any directive. None of the four is reported.

No recommendation: the choice between (a), (b), and (c) belongs to the maintainer. (b) mirrors the trailing-comma rules. The spec leaves every directive-holding body out of SHARED-EMPTY-001, so no reading contradicts it.

Requirements: SHARED-EMPTY-001, SHARED-EMPTY-004.

### 5. RH5417 fix for a member that shares its line with other code — open, no principle decides it — issue: RH5417

The provider formats only the member, and FMT-PIPE-010 keeps the member's trailing trivia, and its leading trivia when it does not start its line. Input → actual:

- `class C` / `{` / `    void M() { } int _x;` / `}` → `    void M()` / `    {` / `    } int _x;`
- `class C` / `{` / `    void M() { } void N() { }` / `}` → `    void M()` / `    {` / `    } void N()` / `      {` / `      }`. That takes two applications; `N`'s braces end up in column 6.
- `class C { void M() { } }` → `class C { void M()` / `          {` / `          } }`

Why it looks wrong: the closing brace starts its line but does not have it to itself, and formatting the result changes it further. In all three cases RH5417 no longer reports the member.

Competing readings:

- (a) P10 decides: the code the fix changed is laid out as the formatter lays it out. The first input gives `    void M()` / `    {` / `    }` / `    int _x;`.
- (b) P9 decides: the fix changes only M, and the gap after `}` belongs to the next member (FMT-BREAK-007), a concern RH5417 does not name. The first input stays `    } int _x;`, as today.

The lead ruled that no principle settles this, so it goes to the maintainer as an open question.

Requirements: RH5417-003, FMT-PIPE-010, FMT-BREAK-007.

### 6. RH5417 fix renames `#region` directives its diagnostic does not report — Decided by principle P1 (#912) — issue: RH5417

Input → actual:

- `int P` / `{` / `    #region getters` / `    get { return _x; }` / `    #endregion` / `}` → `#region Getters`, a blank line, the expanded `get`, a blank line, `#endregion // Getters`.
- `int P` / `{` / `    get { return _x; }` / `    set` / `    {` / `        #region store` / `        _x = value;` / `        #endregion` / `    }` / `}`, where only `get` is reported → the `set` body becomes `#region Store` … `#endregion // Store`.

Why it looks wrong: the fix changes the text of a directive its own diagnostic does not report. SHARED-FIX-003 forbids that, and `squad/decisions/2026-10-18-shared-fix-003-skeptic-scope.md` (answer 2) treats the same pattern in RH7103 as a bug.

Requirements: RH5417-003; violates SHARED-FIX-003.

### 7. Empty-type fixes rename `#region` directives their diagnostic does not report — Decided by principle P1 (#912) — issue: empty-types

Input: `using System;` / `[Obsolete]` / `#region legacy` / `#endregion` / `class A` / `{` / `}`.

Actual RH5412 fix output: `[Obsolete]`, a blank line, `#region Legacy`, a blank line, `#endregion // Legacy`, a blank line, `class A;`.

Why it looks wrong: the fix changes the text of a directive its own diagnostic does not report, which SHARED-FIX-003 forbids. The decision in `squad/decisions/2026-10-18-shared-fix-003-skeptic-scope.md` (answer 2) classes the RH7103 instance as a bug. A `#region` in the declaration's own leading trivia, above `[Obsolete]`, stays unchanged.

Requirements: SHARED-EMPTY-003; violates SHARED-FIX-003.

### For the lead's comment on #1150 (not a new question)

Unchanged from `01-spec-author.md`. RH5412–RH5416 (`EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase`) and RH5417 offer a no-op fix when the file starts with `// <AUTO-GENERATED>`; apply-fix reports `no-progress`. A syntax error does the same, by FMT-PIPE-005; the skeptic confirmed both in `ReihitsuFormatter.FormatNodeInDocumentAsync`. RH5409–RH5411 are not affected.

## Decisions needed

_None._
