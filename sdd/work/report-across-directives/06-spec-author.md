Role: spec author
Commit: d4bb0b1
Verdict: spec written

Repair round for decision 8 of `00-lead.md` and disputed test 2 of `05-implementer.md`. The prompt named the defect and asked for every example of the run with the same defect; it carried no suspected cause or hoped-for verdict beyond that, and the fix was chosen from the compiler's output below.

## How the defect was searched

Every `csharp` block holding `#if`, `#elif`, or `#else` in the specs and the change doc this run touches (94 blocks) was compiled with `csc` (`-langversion:preview`, markup removed) once without symbols and once for each symbol its conditions name. Errors that appear in one build and not in another:

| Example | Build | Error | Handling |
|---|---|---|---|
| RH7102-001, `#if DEBUG readonly #else const #endif` | `DEBUG` | CS8983, field initializer in a struct without a constructor | Changed |
| Change doc, *Member order — RH7102, RH7103*, `S` (before and after blocks) | `DEBUG` | CS8983, same | Changed |
| Change doc, *Inherited documentation — RH8201* (before and after blocks) | `FEATURE` | CS0535, `C` does not implement `ICloneable.Clone()` | Changed |
| Change doc, *Accessor order — RH7107, RH7108*; RH7108-001 example | `NEVER` | CS0065, event without `remove` | Not changed: `#if !NEVER` is the always-active idiom; no example describes a build that defines `NEVER` (the RH7311 tests use `NEVER` as the undefined symbol) |
| RH2001, two examples (`#if NEVER … unused`, `#if !RELEASE … Value`) | `NEVER`, `RELEASE` | CS1001/CS1002, CS0103 | Not changed: unchanged by this run, and neither describes that build |

The other blocks compile alike in every build; some fail in every build only on attribute or base types the snippet does not declare (CS0246), which is the specs' usual shape and not build-dependent.

## Changed examples

- `sdd/specs/analyzer/RH7102.md`, RH7102-001, second example: `public struct S` → `public class S`. In a build with `DEBUG`, `K` is now a readonly field of a class, which compiles; without `DEBUG` it is a constant after `_x`, reported with no fix. The prose ("reported in that build, and no code fix is offered … a readonly instance field, and nothing is reported") still holds unchanged.
- `sdd/changes/2026-11/report-across-conditional-directives.md`, *Member order — RH7102, RH7103*: `public struct S` → `public class S` in the before and the after block. `T` stays a struct (no initializer; it compiles in both builds).
- Same change doc, *Inherited documentation — RH8201*: the before and after blocks gain, after `Dispose`, the member the `FEATURE` build needs, as RH8201-003's own example already writes it:

  ```csharp
  #if FEATURE
      /// <inheritdoc/>
      public object Clone() => this;
  #endif
  ```

  The diagnostic on `Dispose` and the missing fix are unchanged.

## Checks

- `csc` on the changed blocks, without symbols, with `DEBUG`, and with `FEATURE`: 0 errors each.
- `scripts/apply-fix.sh RH7102` on the RH7102-001 example and the change doc's member-order example: `no-fix-offered` under LF and CRLF.
- `scripts/apply-fix.sh RH8201` on the change doc's RH8201 example: `no-fix-offered` under LF and CRLF.
- No requirement ID, `Uses` row, or code owner changed, so the architecture tests were not rerun.

## Changed requirements

- RH7102-001 — A constant after a field of the same accessibility is reported — clarified (example only).

## Examples for the tests

RH7102-001 (second example): the test `ConstantWhoseModifiersAnIfSplitsIntoAnInstanceFieldIsReportedWithoutFix` (LF and CRLF rows) follows it, `struct` → `class`; its `DEBUG` half then expects no diagnostic, its other half reported with no fix. If a test was derived from the change doc's RH8201 example and runs a `FEATURE` build, it takes the `Clone` member too.

## Output questions

_None._ The reported output of every changed example is unchanged.

## Internal choices

- RH7102: `class` rather than an explicit constructor in the struct. Alternative: keep `struct` and add `public S() { }`; it adds a member that has nothing to do with the rule.
- RH8201: the missing `Clone` added inside `#if FEATURE`, copied from RH8201-003. Alternative: drop `ICloneable` for an interface without members; it would make the change doc diverge from the spec example.
- `#if !NEVER` examples left as they are: the symbol stands for "never defined", so no example describes that build.

## Follow-up candidates

- RH2001 spec, example with `#if !RELEASE` around `Value` and `int Read() => Value;` outside the group: does not compile in a build that defines `RELEASE` (CS0103). Unchanged by this run and the example describes no `RELEASE` build; low value, spec-text only.
