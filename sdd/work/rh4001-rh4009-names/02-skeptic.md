Role: skeptic
Commit: fc9ee76
Verdict: proceed (with a text amendment: the EF Core reason in the RH4001 spec, change doc, and rule page states a false fact)

Issue: thoenissen/Reihitsu#1085 (`agent-filed`). The two behaviors are maintainer decisions recorded in the issue body (question round 3, 2026-10-18) and binding; I checked the premise, the spec diff (9de335e), and the change doc against them. My prompt carried no intended fix or verdict.

## Premise

Both defects are real and reached by the analyzer in the IDE (RH4001 also by its code fix).

**RH4001.** Caller: `RH4001TypeNameShouldMatchFileNameCodeFixProvider` → `RH4001TypeNameShouldMatchFileNameHelper.GetRenamedFileName`, which builds `<expected stem><suffix from first dot><extension>` and never looks at the prefix that `GetComparableFileName`/`StripTimestampPrefix` remove for the comparison. Fixture run (`scripts/apply-fix.sh RH4001 <dir>`, LF and CRLF identical, every fixture `fixed`, 1 iteration), each file holding `public class SomeMigration { }` unless noted:

| Fixture | Today's rename |
|---|---|
| `20210501083748_InitialCreate.cs` | `SomeMigration.cs` |
| `20210501083748_InitialCreate.Part1.cs` (`partial class SomeMigration`) | `SomeMigration.Part1.cs` |
| `20210501083749_InitialCreate.cs` (`class Cache<T>`) | `Cache{T}.cs` |
| `2021050108374_InitialCreate.cs` (13 digits) | `SomeMigration.cs` |
| `202105010837480_InitialCreate.cs` (15 digits, no `_` at index 14) | `SomeMigration.cs` |
| `20210501083750_.cs` (nothing behind the prefix) | `SomeMigration.cs` |
| `20210501083751_20210501083748_X.cs` (two prefixes) | `SomeMigration.cs` |

The issue's severity claim is wrong, and the run's texts repeat it. The issue says that dropping the prefix "breaks Entity Framework's migration order". EF Core does not read file names. `MigrationsAssembly` (dotnet/efcore `src/EFCore.Relational/Migrations/Internal/MigrationsAssembly.cs`) orders migrations by `[Migration("<id>")]`: `let id = t.GetCustomAttribute<MigrationAttribute>()?.Id orderby id`. The file names matter only to the scaffolder: `MigrationsScaffolder` writes `<id>.cs` and `<id>.Designer.cs` and finds them again by those names for `migrations remove` (`migration.GetId() + codeGenerator.FileExtension`). That lookup fails after any rename, with the prefix or without it, because the part behind the prefix changes too. So keeping the prefix keeps the file sorted among the migration files in creation order and next to its `.Designer.cs`, which keeps the prefix. It changes nothing EF Core does at runtime. The maintainer's decision stands. Only the stated reason is false, in three places:

- `sdd/specs/analyzer/RH4001.md`, RH4001-008 `Why:` — "Entity Framework Core orders its migrations by the timestamp prefix … a fix that drops it moves the migration out of that order."
- `sdd/changes/2026-11/rh4001-rh4009-names.md`, `## Why` — the same claim ("breaks the project's migration history").
- `documentation/rules/RH4001.md:55` — "The code fix keeps the prefix, so a migration keeps its place in the order of migrations". This is a visible rule-page sentence that a user would read as EF's order.

Recommended amendment for the spec author. In the Why, say that EF Core names a migration's files after its ID, the timestamp prefix plus the name, and that the files sort by that prefix in creation order, so a fix that drops it moves the file away from the other migrations and from its `.Designer.cs`. On the rule page, either drop the "so …" clause or write "so the file stays in order among the other migration files". This rewords the reason for a decision the maintainer already took, and the behavior does not change. I count it as covered by maintainer answer 1 (lead decision 5 is the same kind). If the lead disagrees, it is a rule-page wording question for the next batch.

**RH4009.** `RH4009NamespaceNotAllowedAnalyzer.OnNamespaceDeclarationSyntax` compares `GetNamespaceName(namespaceSyntax.Name)`, which is the declaration's own name and never the enclosing declarations' names. I ran disposable focused tests in a separate worktree, since deleted, using `AnalyzerTestsBase<RH4009NamespaceNotAllowedAnalyzer>.Verify` with `reihitsu.json` as an additional file:

- `namespace A\n{\n    namespace B\n    {\n    }\n}` with `["A", "A.B"]` → today `warning RH4009` at `(3,15)` on `B`.
- The same code with `["A", "B"]` → today nothing is reported (the test passed with no expected diagnostic).

Both match the issue. The rule page already says "The comparison uses the full declared namespace", so the code contradicts the user documentation as well.

## Mechanism and candidates

RH4001: the fix builds the new name from the type and the suffix only. The prefix that the comparison strips (`StripTimestampPrefix`) is never put back. Dispatch: the analyzer reports one diagnostic per file on the first type (`BaseTypeDeclarationSyntax` or `DelegateDeclarationSyntax`). The only code path that changes is the new name, `GetRenamedFileName`. The collision check `TargetDocumentAlreadyExists` reads the same `GetNewFileName`. The `document.FilePath is null` branch of `GetNewFileName` cannot be reached, because the analyzer returns early when `context.Tree.FilePath` is empty and so no diagnostic reaches the fix.

RH4009: the name compared is the `Name` of the declaration node alone. Dispatch: `RegisterSyntaxNodeAction` for `SyntaxKind.NamespaceDeclaration` and `SyntaxKind.FileScopedNamespaceDeclaration`. A namespace's parent can be one of:
- `CompilationUnitSyntax`: RH4009-002, unchanged.
- `NamespaceDeclarationSyntax`: RH4009-003, new.
- `FileScopedNamespaceDeclarationSyntax`: `namespace A; namespace B { }` parses B as a child of the file-scoped declaration, with no parse error.
- `NamespaceDeclarationSyntax` holding a file-scoped one: `namespace A { namespace B; }`.

I verified the last two shapes. Both produce compiler error `CS8955` ("Source file can not contain both file-scoped and normal namespace declarations") at the inner name, so the code never builds. Lead decision 4 holds: no spec or test has to decide them. A namespace inside a type is a syntax error and does not reach the analyzer as a namespace declaration in a valid file.

## Breaking inputs

Failure classes that can reach the changed code: FC-003 (both sides of the prefix boundary), FC-023 (verbatim, Unicode-escaped, digit-only names), FC-027 (more than one nesting level, siblings, a dotted enclosing name), FC-015 (the prefix predicate and the enclosing-name value must be the same as the comparison's), FC-002 (the "nested" qualifier). FC-007/FC-009 hold: a renamed `20210501083748_SomeMigration.cs` declaring `SomeMigration` is not reported, and every case still renames the file. FC-004/019–021 do not apply (no trivia is edited), FC-018 does not apply (the file is renamed, not the type), and FC-008 does not apply (no Fix All provider).

Inputs the test implementer must cover (an existing test that already pins one is named; `TimestampPrefixedFileMismatch` pins only the diagnostic, not the fix result):

| # | Input | Expected | Existing test |
|---|---|---|---|
| R1 | `20210501083748_InitialCreate.cs`, `class SomeMigration` | renamed to `20210501083748_SomeMigration.cs` (RH4001-008) | none found |
| R2 | `20210501083748_InitialCreate.Part1.cs`, `partial class SomeMigration` | `20210501083748_SomeMigration.Part1.cs` | none found |
| R3 | `20210501083748_InitialCreate.cs`, `class Cache<T>` | `20210501083748_Cache{T}.cs` | none found |
| R4 | `2021050108374_InitialCreate.cs` (13 digits) | `SomeMigration.cs` (other side of the boundary) | none found (`NonDigitPrefixWithUnderscoreIsNotStripped` is analyzer-only) |
| R5 | `V2021050108374_InitialCreate.cs` | `SomeMigration.cs` | none found |
| R6 | prefixed file plus a document `20210501083748_SomeMigration.cs` in the same folder | no fix offered (RH4001-006) | none found; the non-prefixed collisions are pinned by `NoFixIsOfferedWhenDocumentWithTheNewFileNameExists` and its siblings |
| R7 | prefixed file plus a document `SomeMigration.cs` in the same folder | fix offered, renames to `20210501083748_SomeMigration.cs` | none found |
| N1 | `namespace A { namespace B { } }`, `["A", "A.B"]` | nothing reported | none found |
| N2 | same, `["A", "B"]` | `B` reported on `B`; a file-level `namespace B { }` with that list is not reported | none found |
| N3 | `namespace A { namespace B.C { namespace D { } namespace E { } } }`, `["A", "A.B.C", "A.B.C.D"]` | only `E` | none found |
| N4 | `namespace A { namespace B { } }`, `["A.B"]` | only `A` | none found |
| N5 | enclosing name written with a verbatim prefix, a Unicode escape, or whitespace or a comment inside it, e.g. `namespace @A { namespace B { } }` with `["A", "A.B"]` | nothing reported (the enclosing value is taken as RH4009-002 takes it; FC-023/FC-015) | none found |

Optional, rare (not required): a 15-digit `202105010837480_InitialCreate.cs` → `SomeMigration.cs`; `20210501083750_.cs` → `20210501083750_SomeMigration.cs`; two prefixes `20210501083751_20210501083748_X.cs` → `20210501083751_SomeMigration.cs`, since RH4001-008 keeps only the first prefix and replaces the rest. All three follow from RH4001-008's wording as written.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH4001 analyzer (comparison, location) | must stay |
| `RH4001TypeNameShouldMatchFileNameHelper` (new file name) | must change |
| RH4001 code fix (collision check) | must change only through the new name; the check itself stays |
| RH4001 Fix All | not involved (none) |
| RH4009 analyzer | must change (full name for nested declarations); location stays |
| RH4009 code fix | not involved (none) |
| Rule pages RH4001 / RH4009 | changed by the spec author; RH4001's reason clause needs the amendment above |
| Resource texts, package README, `AnalyzerPackageMetadataTests` | not involved (titles and messages unchanged) |
| Formatter, `Reihitsu.Core`, CLI | not involved; the formatter does not judge file names or allow-lists, so there is no P11/SHARED-DIAG-010 conflict |
| Self-hosting scan | not involved: no `reihitsu.json` in the repository sets `allowedNamespaceDeclarations`, and no source file has a timestamp prefix |

## Refuse option

- RH4001: withhold the fix when the file name has the prefix, with a one-line guard on the same predicate. The requested change costs a few lines in one helper and no new type, so both options are about the same size. Prefixed migration files are common in EF projects, though a type mismatch in them is uncommon. The maintainer chose to keep the prefix, so refusing is out.
- RH4009: there is no fix to withhold. "Do not report nested declarations" would itself be a visible change, and the maintainer decided the full-name comparison.
- Declaring a shape unsupported: not an option. Both rules are analyzer-only concerns with no formatter phase or precheck involved.
- Simpler options: none. There is no shared full-namespace helper to reuse (`rg` over `src` finds none). `NamespaceCasingHelper.GetLocations` already gives the value of each name part.

## Output questions

_None._ Both behaviors are maintainer answers 1 and 2. Of the consequences, `["A.B"]` reporting the outer `A` and the location on the nested name are both today's output under the unchanged RH4009-002 and the unchanged location. The rule-page reason clause is a wording correction of a sentence that states a decided behavior (see Premise).

## Internal choices

1. **What counts as a digit in the prefix.** The analyzer uses `char.IsDigit`. A fixture run shows that `٢٠٢١٠٥٠١٠٨٣٧٤٨_InitialCreate.cs` (Arabic-Indic digits) and `２０２１０５０１０８３７４８_InitialCreate.cs` (fullwidth digits), each declaring `class InitialCreate`, are `no-diagnostic`, so they count as prefixed today. RH4001-008 refers to "the 14-digit prefix of RH4001-002", so the fix has to keep exactly what the comparison strips. Recommendation: the fix and the comparison use one predicate (FC-015). The spec need not decide ASCII against Unicode digits: nobody writes such names, and either outcome converges.
2. **Mixed file-scoped and block-scoped nesting** (CS8955): leave it unspecified, as lead decision 4 does. Whatever the implementation reports there is on code that does not compile.

## Issue goals

- "Done when: Both inputs behave as decided, with regression tests": keep it.
- The issue's framing ("breaks Entity Framework's migration order") is overstated, as shown under Premise. This changes no goal or visible output beyond the reason-text amendment. The PR should not repeat the claim.

## Architecture findings

_None._

## Follow-up candidates

_None._ After either rename (today's or the new one), EF Core's `migrations remove` cannot find the renamed file by its migration ID. That follows from renaming the file at all, which RH4001-005 decides; it is not a defect against any spec.
