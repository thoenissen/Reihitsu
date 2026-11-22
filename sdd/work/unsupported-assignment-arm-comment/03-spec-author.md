Role: spec author
Commit: 25d703e
Verdict: change doc written

Spec commit: 0106e32. Change doc: `sdd/changes/2026-11/assignment-and-arm-comment-unsupported.md`, Status left at Draft.

The prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md` (*Maintainer answers*: 1B, 2A, 3A), `01-spec-author.md`, and `02-skeptic.md`.

## Changed requirements

- FMT-UNSUP-020 — Comments and directives behind an assignment operator or the `=` of a declaration are unsupported — changed (added in this run, rewritten now).
  - It covers every assignment operator, wherever the assignment stands (1B). That includes initializer members, ref assignments, and deconstructions with or without declared variables (2A).
  - It covers every equals-value clause: property initializer, variable declarator, enum member, and parameter default (3A).
  - New reason: `a comment or directive behind an assignment operator or the = of a declaration`.
  - It is listed after FMT-UNSUP-004 through 018. So a comment behind a declarator's `=` keeps FMT-UNSUP-004's reason, and a directive there in a multi-declarator field keeps FMT-UNSUP-010's.
  - The other side now names the using alias and the attribute argument `=`.
  - Node-level rule: the assignment or the equals-value clause holds the whole gap. An object creation behind a commented `=` is formatted (FC-041).
- FMT-UNSUP-019 — retired, folded into FMT-UNSUP-020 (1B). Its input now gets FMT-UNSUP-020's reason.
- FMT-UNSUP-003 — clarified. Its example now targets the object creation of `var d =⏎/* a */ new List<int> { 1 };`. The equals-value clause target is now refused under FMT-UNSUP-020.
- FMT-UNSUP-011, FMT-UNSUP-012, FMT-UNSUP-017 — clarified. I removed the now-refused lines from their other-side examples (`n += /* c6 */ 1;`, `1 => /* c6 */ "one"`, `1 => /* c5 */ 2`). FMT-UNSUP-011's *Why* and condition now point to FMT-UNSUP-020.
- FMT-BREAK-016 — clarified. No join behind an assignment operator or an equals-value `=` meets a comment. The joins in front of the operator or `=` keep FMT-BREAK-001's refusal. This also closes the skeptic's node-level qualification: no exception is left.
- FMT-INDENT-034 — clarified, all compound-operator examples replaced:
  - the gap paragraph now names FMT-UNSUP-020;
  - the "construct it continues" example is now `1 =>⏎3 + 4,`;
  - example 1 is now `x = ( // c⏎y + 3);` plus an arm with a line break alone;
  - the collection-element example is now `( // c⏎y + 2)`;
  - the RH5302 and RH5308 examples are now `var r = ( // c⏎x + 3) > 0 &&⏎y;`.
- FMT-INDENT-023 — clarified. The comment-led brace clause is narrowed to the complex element initializer: the comment holds the element column (33), the brace stands in column 41, and the elements are in column 45. The sentence also says why the nested-member and array cases are refused (FMT-UNSUP-020 and FMT-UNSUP-004).
- FMT-BLANK-018 — clarified, examples replaced:
  - `Child =⏎⏎/* c */ {` → `new P⏎⏎/* c */ {` (input and unchanged form);
  - `int[] G { get; } =⏎⏎/* b */ { 1, 2 };` → `return new int[]⏎⏎/* b */ { 1, 2 };`.
- FMT-BLANK-020 — clarified. The braces sentence now covers only the closing brace of a member-assigned collection initializer, since FMT-UNSUP-020 refuses a comment above its opening brace. The `L =⏎// c` part is dropped from the third example.
- FMT-PIPE-017 — clarified. Its comment example now targets the object creation, as the RH5303 fix does. A clause target is refused under FMT-UNSUP-020.
- SHARED-FIX-010 — clarified:
  - The RH5303 example now says the fix formats the object creation, which is what `RH5303...CodeFixProvider.TryGetFormatTarget` returns. The old text said it formats the equals-value clause, which was stale.
  - The RH2001 example notes that `Count`'s comment is refused in the property too.
- RH2001-007 — clarified. A directive or disabled text between the initializer's `=` and its value now withholds the fix (3A; visible, in the change doc). The FMT-UNSUP-019 mention is folded into FMT-UNSUP-020.
- RH7101-007 — clarified: the list of refused shapes, FMT-UNSUP-019 removed.
- `AGENTS.md` precheck row — the thirteenth shape is now FMT-UNSUP-020 and the fourteenth FMT-UNSUP-021. The partial-input example is now "a declaration's initializer value without the declaration and its `=`", because a bare equals-value clause can now be refused.
- *Uses* rows synced, by insertion only:
  - `FMT-UNSUP-020` added to `code-fixes.md`, `pipeline.md`, and `blank-lines.md`;
  - `FMT-UNSUP-004` added to `indentation.md`;
  - `FMT-UNSUP-019` removed from `line-breaks.md`, `indentation.md`, `RH7101.md`, and `RH2001.md`.

No rule page changes.

Change doc, rewritten:
- *Visible output changes* adds:
  - the own-line comment behind `=` (`main` inserts a blank line);
  - the compound operator;
  - the arm/member reason change;
  - the deconstruction declaration;
  - the enum member, parameter default, and declarator directive;
  - a directive behind a property `=` and behind an arm `=>`.
- *Code fixes* adds:
  - RH7101 with a parameter default;
  - RH2001 with a directive behind `=`;
  - RH3202 with `+=`;
  - RH5304 (fixed, or `no-progress`, now withheld);
  - RH5111 for a sibling enum member;
  - RH5301 and RH5303 still offered.
- *Special cases* deletes FMT-BREAK-001's refusal in all five joins behind an assignment operator or an equals-value `=` in `LineBreakAssignmentRewriter`, and FMT-UNSUP-019 with its walk.
- *Decisions* records 1B, 2A, and 3A.

How I checked:
- Every new or changed example block went through `scripts/format.sh` on this branch, which has no production change yet:
  - FMT-UNSUP-020's refused examples are fixed points. The arm/member one is refused on `main` under FMT-UNSUP-019.
  - The FMT-UNSUP-020 other side is a fixed point.
  - The FMT-INDENT-034, FMT-INDENT-023, FMT-BLANK-018, and FMT-BLANK-020 input→output pairs match.
- `scripts/apply-fix.sh` ran for RH5302 and RH5308 (new FMT-INDENT-034 examples, fixed as stated) and for the change doc's befores: RH2001, RH3202, RH7101, RH5111, RH5304.
- Every CLI location in the change doc comes from the CLI output or is counted from the block.
- `scripts/test.sh --project architecture`: 196 passed, 2 failed, both for the test implementer:
  - `EveryRequirementReferenceNamesAnActiveRequirement` — 11 tags of FMT-UNSUP-019 in `tests/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedAssignmentInArmCommentTests.cs`, to retag as FMT-UNSUP-020;
  - `EveryRequirementOfAnEnforcedSpecIsCovered` — FMT-UNSUP-020 and FMT-UNSUP-021 have no tests.

## Examples for the tests

- FMT-UNSUP-020: seven examples.
  - Statement and property, location 3,24.
  - One line and initializer members, location 12,13.
  - Compound operators and deconstructions, location 5,14.
  - Arm and anonymous member, location 7,30 (FMT-UNSUP-019's former example, new reason).
  - Enum, property directive, parameter default, and declarator directive, location 3,9.
  - Assignment directive, location 6,1.
  - Other side.
  - Also in the text: a node-level object creation behind a commented `=` is formatted, and an equals-value clause target is refused. The 004 and 010 reasons win on a declarator.
- FMT-UNSUP-021: unchanged.
- FMT-UNSUP-003: the new example. The clause is refused with FMT-UNSUP-020's reason, the statement with FMT-UNSUP-004's, and the object creation is formatted.
- FMT-PIPE-017: the object-creation target example.
- FMT-INDENT-034, FMT-INDENT-023, FMT-BLANK-018, FMT-BLANK-020: the replaced examples.
- The change doc's *Code fixes* list: one test per fix (RH7101, RH2001 ×2, RH3202, RH5206, RH5301, RH5304, RH5111), plus the still-offered RH5301 and RH5303 cases.

Existing tests the amendment touches, besides the list in `02-skeptic.md`:
- `tests/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterNodeColumnTests.cs` `EqualsValueTargetBehindTheLastLineOfAMultiLineCommentIsLaidOutFromNewWhereItStands`: retarget to the object creation.
- `UnsupportedSyntaxEntryPointTests.cs` lines ~396–410 and ~479–483: the clause is now refused.
- Every test that formats a compound operator, an enum member, or a parameter default with a comment behind it, or a directive behind a declarator's `=`, as supported input.

## Output questions

**1 · Formatter/CLI · the reason line for the widened shape**

The answers set the shape but not the wording of its CLI line. The question-1 draft showed `behind an assignment operator`. The skeptic said it should also name the property initializer, and 3A adds the enum member, the parameter, and the declarator.

Input:

```csharp
enum E
{
    A = // one
    1
}
```

Before (`main`): `1` moves to the continuation indent; nothing is reported.

After, A (recommended, the spec as written):
`error: <file>(3,9): unsupported syntax: a comment or directive behind an assignment operator or the = of a declaration. The file was left unchanged.`

After, B:
`error: <file>(3,9): unsupported syntax: a comment or directive behind an assignment operator. The file was left unchanged.`

Why A: an enum member's or a parameter's `=` is not an assignment operator to a user, and "a declaration" covers all four owners in one phrase.

**2 · Formatter · a comment behind the `=` of an attribute argument or a using alias** (not named by the answers)

Input:

```csharp
using System;
using Text = // alias
System.Text;

class NAttribute : Attribute
{
    public int Name { get; set; }
}

class C
{
    [N(Name = // name
    1)]
    void M()
    {
    }
}
```

Before (`main`):

```csharp
using System;

using Text = // alias
    System.Text;

class NAttribute : Attribute
{
    public int Name { get; set; }
}

class C
{
    [N(Name = // name
       1)]
    void M()
    {
    }
}
```

After, A (recommended, the spec as written): same as *before*. Both are the `=` of a name-equals, not of an assignment or a declaration's equals-value clause.

After, B: the input unchanged.
`error: <file>(2,14): unsupported syntax: a comment or directive behind an assignment operator or the = of a declaration. The file was left unchanged.`
Code fixes that format a node holding it, such as RH7101 on the class, are no longer offered. Declared in this work.

Why A: FMT-BREAK-016 has no join behind these two `=`, so declaring them would delete nothing. The attribute argument's line is placed by list alignment, and a shape that deletes nothing is not declared.

## Internal choices

- **The equals-value clause counts by itself, not only with its owner in the input.**
  - This replaces 01's "a property initializer counts where the property is part of the input".
  - No caller formats a bare equals-value clause: RH5111 formats the declaration, parameter list, or enum declaration around it, and RH5303 formats the object creation. Checked with `grep` over `src/Reihitsu.Analyzer.CodeFixes`.
  - So the two refusals behind an equals-value `=` can go as well. The cost is three direct-call examples and tests (FMT-UNSUP-003, FMT-PIPE-017, the SHARED-FIX-010 RH5303 text), which now target the object creation.
  - Alternative: count it only with its owner in the input, which keeps both refusals for an input no caller passes.
- **FMT-UNSUP-019 is retired rather than narrowed:** FMT-UNSUP-020 holds all of its input (1B: "folded in").
- **The FMT-INDENT-034 replacements** use a comment behind a parenthesized expression's `(` and a line break alone behind an arm's `=>`, the two remaining common ways to keep such a line.
- **FMT-INDENT-023:** the comment-led brace text is narrowed to the complex element initializer, not dropped.
- **Change doc, *Checked and kept*:** I kept the brace-column handling of FMT-INDENT-023 (`ObjectInitializerContributor`), because a complex element initializer still reaches it. The implementer deletes a branch only if it serves nothing but a nested initializer assigned to a member behind a comment.

## Follow-up candidates

_None new._ The `int P { get; } /** c */ = 1;` relocation candidate from `02-skeptic.md` stays as `00-lead.md` records it.
