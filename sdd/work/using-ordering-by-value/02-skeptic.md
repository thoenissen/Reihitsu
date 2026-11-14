Role: skeptic
Commit: 4789434
Verdict: #1105: proceed, with one output question (Unicode escape); #1266: proceed; #1267: proceed narrower (RH7204 is already correct for the issue's instance)

The spec diff is the one in 8d25ac5; 4789434 changes only `00-lead.md`, so code and specs are the same. My prompt carried no intended fix and no suspected cause. It asked for my verdict on one internal choice; I give it below as evidence, not as a hoped-for answer.

Every observation below comes from running `scripts/format.sh` twice, or `scripts/apply-fix.sh RH72xx` (LF and CRLF), on fixtures in the scratchpad, which I deleted afterwards. Statuses are quoted as the scripts printed them.

## thoenissen/Reihitsu#1105

### Premise

The premise is real, and it reaches further than the issue says. Callers that reach it:
- every analyzer run of RH7201, RH7203, RH7204, RH7206, and RH7207 on a file;
- whole-document formatting and the CLI;
- the RH7201–RH7207 code fixes, through `ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync` (FMT-PIPE-030).

Observed on `main`'s code:
- RH7201 gives `no-diagnostic` for `using Microsoft.Win32;` followed by `using System . IO;`, `using @System.IO;`, `using System/* x */.IO;`, `using global :: System.IO;`, or `using global::@System.IO;`. Plain `System.IO` is `fixed`.
- RH7203 gives `no-diagnostic` for `System . IO` or `@System.IO` after `System.Linq`. It gives a false positive for `System.Collections` followed by `System.@Linq`. The fix then produces `using System.@Linq; using System.Collections;`.
- RH7204 gives `no-diagnostic` for `B = System.Text` followed by `A = System . IO` or `A = @System.IO`.
- RH7206 gives `no-diagnostic` for `static System . Math`, `static @System.Math`, or `static System.Collections.Generic.Comparer< int >` in front of `static System.Console` (or `Comparer<byte>`).
- RH7207 gives `no-diagnostic` for `using System;⏎⏎using @System.IO;` and for `using System;⏎⏎using System . IO;`. It reports the value-organized block `System; @System.IO; System/* x */.Linq; ⏎ Microsoft.Win32`, and its fix splits that block into four groups.
- Formatter, pass 1:
  - `Microsoft.Win32; @System.IO; System` → `System; ⏎ @System.IO; ⏎ Microsoft.Win32`, three groups.
  - `static @System.Math; static System.Console` → two groups.
  - `global :: System.IO` → its own group in front of `Microsoft`.
  - `System/* x */.IO` → its own group behind `Microsoft`.
- **Formatter is not idempotent (FMT-PIPE-004) on `main`.** `Microsoft.Win32; System . IO; System` gives `System; ⏎ Microsoft.Win32; ⏎ System.IO` on pass 1. Pass 2 moves `System.IO` up into the `System` group. `static …Comparer< int >; static …Comparer<byte>` gives `Comparer<int>; Comparer<byte>` on pass 1 and swaps them on pass 2. The using phase groups by spelling, and the later horizontal-spacing phase then changes that spelling. The issue does not mention this defect.

### Verdict on the internal choice: by-value comparison also binds the formatter

The formatter's output change is **demanded by requirements unchanged on `main`**. It is not an output question. The evidence:

1. **There is one implementation.** `UsingGrouping.ComputeCanonicalOrder` and `UsingGrouping.AreInSameGroup` (formatter) only delegate to `UsingDirectiveOrderingUtilities` in `Reihitsu.Core`. RH7207 calls the same `ComputeCanonicalOrder` and `AreInSameGroup` directly. SHARED-DIAG-002 (unchanged) binds RH7207 to the value. RH7207 cannot follow it while the formatter stays on spelling unless the policy is forked, which would be a second statement of one rule (FC-015).
2. **SHARED-USING-004 on `main`** says "Analyzer and formatter MUST" put two directives in one group exactly when their roots agree. That is one definition for both surfaces, and DIAG-002 fixes it to the value for the analyzer side.
3. **FMT-USING-001 on `main`** requires the formatter to produce the canonical order of SHARED-USING-001…007. Its Why says: "the formatter and the RH72xx code fixes must produce the order the analyzer checks."
4. **SHARED-FIX-001 and SHARED-FIX-005.** The RH72xx fixes are the formatter's using phase (FMT-PIPE-030). Suppose the analyzers moved to the value and the formatter stayed on spelling:
   - The RH7201 fix on `Microsoft.Win32; System . IO` would keep `System . IO` behind `Microsoft`, because the spelled root `System ` sorts after `Microsoft`. RH7201 would report again: the fix does not converge (FC-007).
   - The RH7207 fix on `System; ⏎ @System.IO` would keep the blank line, so the fix is offered and changes nothing (FC-009).
5. **FMT-PIPE-004** (idempotency) already fails on `main` for the whitespace member and the generic-argument member, as shown above. Only grouping by value makes the using phase's decision independent of what the horizontal-spacing phase normalizes later.

No spec example and no test on `main` locks a spelling-based output. A search of `src/*Test*` for using directives with `@`, ` . `, `/*`, ` :: ` found none. The spec author's Why in SHARED-USING-004 states this argument correctly. I recommend that the lead decide it without asking and name the moved directives under the PR's review notes. The formatter's code needs no change: the change is in `Reihitsu.Core`.

### Mechanism and candidates

**Mechanism.** `UsingDirectiveOrderingUtilities` takes the root (`GetRootNamespace`, `StripGlobalQualifier`), the `System` check (`IsSystemNamespaceUsing`), and the non-alias sort key (`GetSortKey`) from `NameSyntax.ToString()`. That is the source text from the name's first token through its last, interior trivia included. So any difference in a token's text, or in the trivia between the tokens, that leaves the name's value unchanged still changes group and order, for every analyzer and the formatter at once.

Membership is decidable: a directive is affected exactly when its name's `ToString()` differs from the value string.

**Dispatch.** `UsingDirectiveSyntax.Name`/`NamespaceOrType` and the `NameSyntax` hierarchy:
- `IdentifierNameSyntax`;
- `QualifiedNameSyntax`;
- `AliasQualifiedNameSyntax` (`global::`);
- `GenericNameSyntax` (static type names and alias targets with type arguments).

**Read sites.** Two independent reads of the root must both move:
- `IsSystemNamespaceUsing`, used by RH7201 and by RH7203's group filter through `GetUsingDirectiveGroup`;
- `GetRootNamespace`, used by `AreInSameGroup`, `ComputeCanonicalOrder`, RH7203, RH7204, RH7206, RH7207, and the formatter.

`GetSortKey` reads the alias name through `Identifier.ValueText`, so the alias name is already compared by value.

### Breaking inputs

Failure classes:
- **FC-023**: verbatim and Unicode identifiers.
- **FC-015 / FC-016**: two separate reads of the root; fixing one leaves the other on spelling.
- **FC-010 / FC-017**: whole-pipeline idempotency needs a two-pass assertion.
- **FC-007 / FC-009**: fix convergence.
- **FC-032**: fix expectations move with the Core change.
- **FC-034**: test the reported shape itself.
- **FC-021**: comment inside the name.
- **FC-033**: CRLF; `apply-fix.sh` showed no line-ending drift on any fixture.

Class matrix. Spelling variant × kind (regular, static type name, alias target, alias name) × read (root/`System` check, sort key) × section (local, global):

| Member | Surface | Today | Must cover |
|---|---|---|---|
| `@System.IO` after `Microsoft.Win32` (regular) | RH7201, formatter | not reported; own group | yes |
| `System . IO` after `Microsoft.Win32` | RH7201, formatter | not reported; not idempotent | yes, two passes |
| `System/* x */.IO` after `Microsoft.Win32` | RH7201, formatter | not reported; own group behind `Microsoft` (comment kept as written) | yes, formatter too |
| `global :: System.IO` | RH7201, formatter | not reported; own group | yes |
| `global::@System.IO` | RH7201 | not reported | yes |
| `global using @System.IO;` (global section) | RH7201, formatter | same mechanism | one test |
| `System.@Linq` after `System.Collections` | RH7203, formatter (sort key) | false positive; reordered | yes |
| `System . IO` / `@System.IO` after `System.Linq` | RH7203 | not reported | yes |
| alias target `System . IO` / `@System.IO` after `B = System.Text` | RH7204 | not reported | yes |
| static `@System.Math` / `System . Math` before `System.Console` | RH7206, formatter | not reported; two groups | yes |
| static `Comparer< int >` before `Comparer<byte>` (type-argument trivia) | RH7206, formatter | not reported; not idempotent | yes, two passes |
| `System;⏎⏎@System.IO` / `System;⏎⏎System . IO` | RH7207 | not reported | yes |
| `System; @System.IO; System/* x */.Linq; ⏎ Microsoft.Win32` | RH7207 + fix | reported; "fixed" into 4 groups | yes, not reported |
| `System.IO` (Unicode escape) | RH7201, formatter | not reported; own group behind `Microsoft` | undecided, see Output questions |
| alias name `@B` / `B` | RH7204 | already compared by `ValueText` (holds) | no |
| comment between `using` and the name (leading trivia of the name) | all | not part of `ToString()` (holds) | no |

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Reihitsu.Core` `UsingDirectiveOrderingUtilities` (`IsSystemNamespaceUsing`, `GetRootNamespace`, `GetSortKey`) | must change |
| RH7201, RH7203, RH7204, RH7206, RH7207 analyzers | behavior changes through Core; their files need not change for #1105 |
| RH7202, RH7205 analyzers | not involved (they read only the kind) |
| RH7201–RH7207 code fixes, Fix All (`BatchFixer`) | code not involved; output changes through the formatter phase (FMT-PIPE-030) |
| Formatter using phase (`UsingGrouping`, `UsingDirectiveOrderingRewriter`, `UsingLeadingTriviaBuilder`) | code not involved; output changes through Core, as demanded above |
| Other formatter phases (horizontal spacing) | must stay |
| Rule pages RH7201–RH7207 | must stay. RH7207's "spelled exactly `System`, so a root such as `system` is not treated as the framework group" is about case, as its *so* clause shows. `@System` is a corner case a rule page does not list. |
| Tests | analyzer tests per rule, formatter tests with two-pass idempotency, Core tests in `src/Reihitsu.Core.Test/UsingDirectiveOrderingUtilitiesTests.cs` |

### Refuse option

- **Refuse.** The analyzers stay silent and the formatter leaves alone any block whose names carry `@` or interior trivia. This is a cheap check, but it contradicts SHARED-DIAG-002 unchanged ("MUST give the same result"). It also leaves the FMT-PIPE-004 failure in place unless the guard also stops the phase. Not admissible.
- **Requested change.** Derive one value string per name in Core and use it at the three read sites. That is a few dozen production lines, no new type, and no new mechanism. The input class is rare: `@`, spaces, or comments in a using name.
- **Declare unsupported.** The shape would be a comment between the first and the last token of a using directive's name (or type name, or alias target). It would delete no special case, because the using phase has none for it, so under `AGENTS.md` it is not declared. `@` and whitespace delete nothing either. I name it and do not recommend it.
- **Simpler option.** None.

### Output questions

**1 · Using ordering (RH7201 · System using directives must be placed before other using directives; RH7207 · Using directives should be organized into groups; formatter) — Unicode escape in a using name**

Input:
```csharp
using Microsoft.Win32;
using System.IO;
using System;
```

Before (`main`): RH7201 does not report `System.IO`. The formatter gives:
```csharp
using System;

using Microsoft.Win32;

using System.IO;
```

After, A (recommended): RH7201 reports `System.IO`, and the formatter gives:
```csharp
using System;
using System.IO;

using Microsoft.Win32;
```

After, B: unchanged from *Before*. The escape counts as spelling that is kept, so the directive is not in the `System` group.

Why A: the compiler imports `System.IO` either way. The alias name in this same ordering is already compared with escapes decoded, and RH4001, RH4009, and the parameter documentation rules decode escapes too.

Why it is asked: SHARED-DIAG-002 names only `@`, whitespace, and comments. The other specs state escape decoding separately, and the spec diff's SHARED-USING-004 also lists only `@`, whitespace, and comments. An implementation built on `Identifier.ValueText` would silently give A. One that strips `@` from text would give B. So the mechanism would decide a visible output unless the output is decided first. The case is rare. If the lead considers it outside #1105's named spellings, it becomes a follow-up issue carrying this question, and the run's tests must then not lock either output.

### Internal choices

- **Spec wording.** SHARED-USING-004 (diff) says "without the whitespace and comments written between its parts". I recommend "between its tokens", or one example each for `global :: System.IO` and a generic static type name such as `Comparer< int >`, so that `global ::` and type-argument trivia read as covered. Both are failing members above. FMT-PIPE-004 and the analyzer–formatter agreement already demand the output for the generic member, so nothing visible is in question.
- **Comment member in the formatter.** The spec author left the comment-inside-name member without a formatter example, because of horizontal spacing. The run shows that the formatter keeps `System/* x */.IO` as written, so the test implementer can write a formatter test for it from SHARED-USING-004's wording. The issue's "Done when" lists it for the formatter.

### Issue goals

- "RH7201, RH7203, RH7207, and the formatter" is too narrow. The comment on the issue adds RH7204 and RH7206, and the run's specs cover them. Keep that.
- Add the formatter's idempotency failure (two-pass test) as an explicit result. The issue does not know about it.
- No goal to drop.

### Architecture findings

_None for #1105._ The policy has one owner in Core. Its two reads of the root (`IsSystemNamespaceUsing` and `GetRootNamespace`) sit in the same file and must change together (FC-016).

### Follow-up candidates

- `UsingDirectiveOrderingUtilities.OrderUsings` has no production caller; only `Reihitsu.Core.Test` calls it. It is a dead-code cleanup candidate, not a defect.

## thoenissen/Reihitsu#1266

### Premise

The premise is real, and plain, common code reaches it: any analyzer run on the file. Observed:
- RH7203: `Microsoft.B; Alpha.X; Microsoft.A` gives `no-diagnostic`. With `using static System.Math;` between instead, it is `fixed`, as the issue says.
- RH7204: `B = Microsoft.Win32.Registry; C2 = System.Text; A = Microsoft.Win32.RegistryKey` gives `no-diagnostic`.
- RH7206: `static Microsoft.Win32.Registry; static System.Math; static Microsoft.Win32.ComponentModel` gives `no-diagnostic`.

SHARED-USING-007's second paragraph, unchanged on `main`, already demands a comparison with every earlier directive of the group. The rule specs on `main` ("directly follows") contradicted it, and the spec diff aligns them. No visible output is in question.

Severity is lower than the issue implies: RH7207 (default on) already reports each such block, and its fix organizes it.

### Mechanism and candidates

**Mechanism.** RH7203, RH7204, and RH7206 each keep only the immediately preceding directive of their kind (RH7203: of the same `SystemNamespace`/`OtherNamespace` class) and compare only when that directive has the same root. So a directive is never compared with an earlier directive of its group that stands in front of its immediate predecessor of the same kind, whether that predecessor belongs to another root or to the same group.

**Dispatch.** The three `OnUsingScope`/`AnalyzeGroup` loops. RH7201, RH7202, and RH7205 use a "seen" flag over the whole section, so they already compare with every earlier (or later) directive. That is the issue's "Done when" check, by code reading. RH7207 compares the block with `ComputeCanonicalOrder`.

### Breaking inputs

Classes: FC-015 / FC-016 (three private copies, see Architecture findings) and FC-003 (both sides: an adjacent predecessor of the same group that is lower, and one that is higher).

| Member | Rule | Today |
|---|---|---|
| another root of the same kind between (`Microsoft.B; Alpha.X; Microsoft.A`) | RH7203 | not reported |
| same, aliases | RH7204 | not reported |
| same, statics | RH7206 | not reported |
| a **non-name alias** between (`B = System.Text; P = int; A = System.IO`) | RH7204 | not reported (P's root is `""`). By code reading, not run |
| an earlier higher key, with a lower adjacent one (`Microsoft.C; Microsoft.A; Microsoft.B`: B) | RH7203 | B not reported (A is) |
| same within the `System` group (`System.Text; System.Collections; System.IO`: IO) | RH7203 | IO not reported. By code reading |
| same, aliases (`C2 = System.Text; A = System.IO; B = System.Linq`: B) | RH7204 | B not reported (A is) |
| same, statics (`System.Math; System.Console; System.Convert`: Convert) | RH7206 | Convert not reported (Console is) |
| a `System`-group directive or another kind between | RH7203 | already reported (holds) |
| global section | all three | same mechanism; one test |

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH7203, RH7204, RH7206 analyzers | must change |
| RH7201, RH7202, RH7205, RH7207 analyzers | must stay |
| Code fixes, Fix All, formatter | not involved (the fix already organizes the whole block; new diagnostics converge in one application) |
| Rule pages | not involved (none says "directly above") |

### Refuse option

Not applicable: the defect is a missing report, not a fix or a rewrite. The requested change is small, in three loops. _None_ simpler.

### Output questions

_None._

### Internal choices

_None._

### Issue goals

"The other ordering rules are checked for the same pattern" is met by the code reading above for RH7201, RH7202, RH7205, and RH7207. No goal to drop.

### Architecture findings

**A policy with more than one owner (FC-015).** The order within a group (SHARED-USING-007) is decided in four places:
- by `UsingDirectiveOrderingUtilities.ComputeCanonicalOrder` in Core, which RH7207 and the formatter use;
- by three private, near-identical loops in RH7203, RH7204, and RH7206.

The copies drifted. With `Microsoft.B; Alpha.X; Microsoft.A`, RH7207 reports the block while RH7203 does not report `Microsoft.A`. RH7203 also partitions by a second group notion (`GetUsingDirectiveGroup`'s `SystemNamespace`/`OtherNamespace`) before it applies `AreInSameGroup`. A shared check in Core would stay within the run's components and well under 100 lines. The lead decides between a narrow fix and the shared check.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1267

### Premise

The premise is real. The CLI and whole-document formatting reach it, and so do RH7207 and its fix, for any C# 12 alias of a type that is not a name. Observed:
- RH7207 reports `A = System.Text; ⏎ B = int`, and its fix moves `B = int` first.
- The fix does the same for `int[]`, `int?`, `System.Int32?`, and `(int X, int Y)` (`fixed` in each case).
- RH7207 gives `no-diagnostic` for `B = int; ⏎ A = System.Text`.
- The formatter turns `A = System.Text; ⏎ B = int` into `B = int; ⏎ A = System.Text`, and moves `P = (int X, int Y)` in front of `A = System.Text`.

SHARED-USING-004 on `main` already demands the last position, with an `int` example whose formatter output `main` does not produce. So the formatter output change is demanded and puts nothing in question.

### Mechanism and candidates

**Mechanism.** For an alias whose target is not a `NameSyntax`, `UsingDirectiveSyntax.Name` is null. `GetRootNamespace` then returns `""`, and `GetNamespaceGroupOrderKey` returns `""`, the order key of the `System` group. The ordinal root tie-break (`""` < `"System"`) in `ComputeCanonicalOrder` then places that group first among the aliases instead of last.

**Dispatch.** The target types of an alias that are not a `NameSyntax`:
- `PredefinedTypeSyntax`;
- `TupleTypeSyntax`;
- `ArrayTypeSyntax`;
- `NullableTypeSyntax`;
- `PointerTypeSyntax`;
- `FunctionPointerTypeSyntax`.

### Breaking inputs

Classes: FC-010 / FC-017 (the "Done when" asks for idempotency: two passes) and FC-032 (RH7207 fix expectations).

| Member | Today |
|---|---|
| `int` behind a `System` alias | RH7207 reports a correct block; fix and formatter put `int` first |
| tuple `(int X, int Y)` | same |
| array `int[]` | same |
| nullable `int?` and nullable of a name `System.Int32?` | same |
| pointer `int*` (`using unsafe`) and function pointer | same mechanism (`Name` null); not run, because they need unsafe code. One test is optional |
| behind an alias group of another root (`Z = Microsoft.X`) | same mechanism; must come after it too |
| global section (`global using B = int;`) | same mechanism; one test |
| block with no name alias (regular + static + `B = int`) | holds (kind order) |
| RH7204 `B = System.Text; A = int` | `no-diagnostic`: already correct (characterization) |
| two non-name aliases (`B = int; A = (int X, int Y)`) | RH7204 reports A today. Outside SHARED-USING-004 by its own scope note; must stay as it is, so that no undecided output changes |

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Reihitsu.Core` `ComputeCanonicalOrder` (group order of a target that is not a name) | must change |
| RH7207 analyzer, its fix, formatter using phase | output changes through Core; code not involved |
| RH7204 analyzer | must stay (already correct for one non-name alias) |
| RH7201–RH7203, RH7205, RH7206 | not involved |
| Rule page RH7207 | must stay ("alias directives last" stays true) |

### Refuse option

- **Refuse.** RH7207 and the formatter leave a block with a non-name alias as written. That contradicts SHARED-USING-004 unchanged, and its example. The requested change is a few lines in one ordering key, with no new type.
- **Declare unsupported.** Not an option: no special case would be deleted.
- **Simpler option.** _None._

### Output questions

_None._

### Internal choices

_None._

### Issue goals

- "Requirements: … SHARED-USING-005" is not relevant: the `System`-first rule is not involved. Drop it.
- RH7204 needs only a characterization test, not a code change.
- "Tuple targets behave the same" and "formatter idempotent on the result" stay.

No visible output changes from these adjustments.

### Architecture findings

_None._

### Follow-up candidates

_None._

## Whole run

- Fixture runs: `scripts/format.sh` twice on 10 formatter fixtures, and `scripts/apply-fix.sh` for RH7201, RH7203, RH7204, RH7206, and RH7207 on 33 fixtures. No `not-converged`, `no-progress`, or line-ending drift on `main`. All fixtures were deleted.
- The repository's own source holds no using directive of any affected shape, so the self-hosting scans are unaffected.
- No change doc is needed: every output change above is demanded by requirements unchanged on `main` (SHARED-DIAG-002, SHARED-USING-004, SHARED-USING-007, FMT-USING-001, FMT-PIPE-004, SHARED-FIX-001). The one exception is the Unicode-escape question.
