Role: spec author
Commit: 06dfbd3
Verdict: change doc written

The prompt carried no intended fix or suspected cause. The issue's "Instances" are proposals (`agent-filed`); the handover follows the maintainer decision on thoenissen/Reihitsu#1169 (question 2, P11) and the later change doc `sdd/changes/2026-11/report-across-conditional-directives.md`.

## What was established, and how

- Today on `main`, `class C { };` and the same shape for struct, interface, record, record struct, and union are not reported. Checked with `scripts/apply-fix.sh RH5412` and `RH5418` on fixtures: `no-diagnostic` under LF and CRLF. Cause in the syntax tree: Roslyn makes the `;` behind `}` the declaration's own `SemicolonToken`, and the shared check treats any declaration whose `SemicolonToken` is present as a semicolon declaration.
- Today `scripts/format.sh --dry-run` leaves `class A\n{\n};` unchanged, breaks `class B { };` into `class B\n{\n};`, and indents a `;` on its own line behind `}` by four spaces (`}\n    ;`). `class E\n{\n} /* x */;` is unchanged.
- `class A\n{\n};;` is a parse error (apply-fix reports `parse-error`), so a second semicolon is out of reach and the formatter skips the file.
- The issue's directive goals (bullets 2 and 3, and the RH5416 test change) are already settled the other way: SHARED-EMPTY-001 and SHARED-DIAG-008 report a body with a conditional directive or excluded code, without a fix (`report-across-conditional-directives.md`, and the correction comment on #1275). This run leaves them and the RH5416 test `VerifyDirectiveContainingRecordStructIsReportedWithoutCodeFix` unchanged. Recorded in the change doc under *Decisions*.
- `scripts/test.sh --project architecture`: 197 passed, 1 failed — `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered`: `SHARED-EMPTY-005: no test covers it`. Expected until the test implementer adds the covering test; no other spec-structure finding.

## Files

- `sdd/changes/2026-11/empty-type-stray-semicolon.md` — change doc, `Status: Draft`.
- `sdd/specs/shared/empty-type-declarations.md` — SHARED-EMPTY-001 changed, SHARED-EMPTY-002 example extended, SHARED-EMPTY-005 added.
- `sdd/specs/formatter/structural-transforms.md` — FMT-STRUCT-017 and FMT-STRUCT-018 changed.
- `sdd/specs/analyzer/RH5412.md`, `RH5413.md`, `RH5414.md`, `RH5415.md`, `RH5416.md`, `RH5418.md` — intro names SHARED-EMPTY-005; `Uses` gains it.
- `sdd/specs/requirement-ids.txt` — SHARED-EMPTY-005 inserted.
- Rule pages `documentation/rules/RH5412.md` to `RH5416.md` and `RH5418.md`: not changed. They say "replace the empty brace body with a trailing semicolon", which already covers `{ };`; the form is a corner case, not the normal case.

## Changed requirements

- SHARED-EMPTY-001 — A brace body without members is reported — changed: a semicolon behind the closing brace no longer keeps the declaration from being reported (the clause "and that ends with its closing brace" is gone); examples `F` and `G` added.
- SHARED-EMPTY-002 — A declaration with members or a semicolon is not reported — clarified: example `class C { int _x; };` (members, semicolon behind the brace) is not reported. Today's behavior.
- SHARED-EMPTY-005 — A semicolon behind the braces becomes the one semicolon — added: the fix removes the braces and the semicolon behind them and writes one semicolon after the header; whitespace and line breaks between `}` and `;` go with them; a comment behind that `;` follows the new one; a comment, directive, or disabled text between `}` and `;` withholds the fix (via FMT-STRUCT-018 and SHARED-EMPTY-004), still reported.
- FMT-STRUCT-017 — Empty types use a semicolon declaration — changed: converts also when a semicolon follows the closing brace; exactly one semicolon in the result; a comment behind the old semicolon follows the new one. New example.
- FMT-STRUCT-018 — An empty body with trivia keeps its braces — changed: anything other than whitespace and line breaks between the closing brace and a semicolon behind it also keeps the braces; the semicolon stays behind the closing brace. Example `Behind` added.

## Examples for the tests

- SHARED-EMPTY-001 — examples `F` (`class F\n{\n};`) and `G` (`class G { };`), reported on the name. Cover every rule of the spec: RH5412, RH5413, RH5414, RH5415, RH5416, RH5418 (the change doc's first block).
- SHARED-EMPTY-002 — `class C { int _x; };` not reported (check existing tests first, *Coverage first*).
- SHARED-EMPTY-005 — both examples: the four fixed declarations `A`–`D`, and the two withheld ones (`} /* note */;`, `} // note` then `;` on the next line).
- FMT-STRUCT-017 — the new example (`A`–`D`).
- FMT-STRUCT-018 — `class Behind\n{\n} /* note */;` unchanged.
- The change doc's RH5404 block: `class H { class I { }; }` fixed to `class H\n{\n    class I;\n}` — a code-fix result that changes through the formatter.

## Output questions

_None_ beyond the visible output changes the change doc lists, which the lead turns into output questions. Two of them are choices of this spec that the maintainer may want to see separately:

- RH5418 (unions) is included, though the issue names only RH5412–RH5416; same shared check, same gap. Alternative: exclude unions, which needs a kind-specific exception in shared code.
- A comment between `}` and `;` makes the declaration reported without a fix (before: not reported). Alternative: leave such a declaration unreported, which needs a separate reporting guard.

## Internal choices

- The fix's handling of the semicolon is a new requirement SHARED-EMPTY-005, not a sentence in SHARED-EMPTY-003: it has its own reason (one semicolon; the stray one declares nothing). Alternative: extend SHARED-EMPTY-003.
- The formatter side is a change of FMT-STRUCT-017 and FMT-STRUCT-018, not new FMT-STRUCT IDs: both requirements already own exactly this decision. Alternative: a new FMT-STRUCT requirement for the semicolon.
- The rule specs' intros name SHARED-EMPTY-005 and list it under `Uses`, so each rule spec states where its fix result comes from. Alternative: leave the rule specs alone; the shared spec is enforced either way.
- Rule pages are left unchanged (see *Files*).
- Non-goal recorded only in the change doc: a stray semicolon behind a type with members is not in scope.

## Follow-up candidates

### 1 · RH5404 · Element must not be on a single line — a type with a semicolon behind its braces

The rule's own gap (P11): RH5404 returns early for any type declaration whose `SemicolonToken` is present, the same cause as this run's issue, while the formatter breaks the declaration onto lines.

Input:
```csharp
class C { int _x; };
```

Before (`main`): not reported; `reihitsu-format` writes
```csharp
class C
{
    int _x;
};
```

After, A (recommended): reported on `C`, the fix writes the formatter's output above.

After, B: not reported (today).

Why A: the declaration is the single-line element the rule names; the trailing `;` does not change that. Verified with `scripts/apply-fix.sh RH5404` (no diagnostic) and `scripts/format.sh --dry-run`.

### 2 · Formatter · a semicolon on its own line behind a type's closing brace is indented

Outside this run once the empty case converts: it remains for a type with members, and for an empty type with a comment between `}` and `;`.

Input:
```csharp
class G
{
    int _x;
}
;
```

Before (`main`):
```csharp
class G
{
    int _x;
}
    ;
```

After, A (recommended):
```csharp
class G
{
    int _x;
};
```

After, B:
```csharp
class G
{
    int _x;
}
;
```

Why A: an indented `;` reads like an empty statement in a nested block; joining it to the brace is the shape C++-style authors write, and FMT-BREAK-009 already joins a declaration's semicolon for fields. Whether the line-break phase owns the join for a type declaration is for that run to decide.
