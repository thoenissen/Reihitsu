Role: auditor
Commit: 396887f776fe938dacc896043f105f5178b47bb9
Verdict: #971: pass; #1072: pass; #1451: findings; #1434: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Read: `00-lead.md` (*Maintainer answers*: batch 1, Q1–Q12 all A; *Decisions taken* 1–2), handovers `01`–`10`, the three change docs, the spec diff (`shared/casing.md`, `shared/code-fixes.md`, RH4005, RH4102–RH4105, RH4112–RH4117, RH4121), the production diff, the test diff, and outside the diff `CasingCodeFixProviderBase{T}` (both rename paths), `CasingRenameSafetyUtilities.GetNativeEntryPoint`, `RH4005InterfaceNameCasingAnalyzer`, `RH4118TupleElementCasingAnalyzer`, and every caller of `CasingUtilities` (`rg "CasingUtilities\.(To|Is)"`).

Probes ran in a detached worktree of `396887f` under the scratchpad (removed afterwards): temporary MSTest classes in `Reihitsu.Analyzer.Test` and `Reihitsu.Core.Test`, run with `scripts/test.sh --filter`, and `scripts/apply-fix.sh RH4105` over a scratch fixture directory. Nothing in the shared tree was changed besides this handover.

## thoenissen/Reihitsu#971

### Scope
Full audit. Head `396887f776fe938dacc896043f105f5178b47bb9`, base `origin/main` `75cc39fa61efdb2aa564ba57f36e86127b33485b` (matches `git fetch`; merged: `git merge-base --is-ancestor` true). The diff is what `04`–`06` describe (`CasingUtilities.ToCamelCase`/`ToUnderlineCamelCase` through `SplitIntoPascalCaseWords` + `LowercaseLeadingUppercaseRun`).

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_N/A — none._ Traced against SHARED-CASING-006 (second paragraph, first-word reading of Q10 A): `HTTPClient`→`httpClient`, `URLs`→`urLs`, `HTTP2Client`→`http2Client`, `ABc`→`aBc`, `IOStream`→`ioStream`, `IDs`→`iDs`, `ID`→`id` (word all uppercase first), `A_B`→`aB`, `m_HTTPClient`→`mHTTPClient`, `XML_Parser`→`xmlParser`, `ORDER‿LINE`→`order‿LINE` (Pc ends the run, word not all uppercase). Every result satisfies `IsCamelCase`, so the fix converges in one pass (FC-007). PascalCase targets are untouched (`ToPascalCase` never calls the run step). RH2001 reaches the change only through `ToUnderlineCamelCase`.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Leading uppercase run of a camelCase target (`LowercaseLeadingUppercaseRun`) | `words[0]` only, after the PascalCase word step | SHARED-CASING-006 "the run never reaches into the second word"; `IsCamelCase` of the result | Same span (first word); every result is camelCase | ok |
| Run end / last-letter exception | `char.IsUpper` run, combining marks skipped once a letter is counted; exception when ≥2 letters and `char.IsLower` follows | Spec: "two or more letters and a lowercase letter of the same word follows directly" | Agree (a digit, Nl, Pc, Lo after the run lowercases the whole run, as the spec's "lowercase letter" qualifier demands — FC-002 checked) | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Word splitting and case derivation | `rg -n "IsLetterOrDigit\|IsUpper\(\|ToLowerInvariant\|ToUpperInvariant" src` (non-formatter): only `CasingUtilities`; RH4005 analyzer/fix prefix tests and RH7302 region capitalization decide other questions | one owner |
| Callers of the derivation | `rg "CasingUtilities\.To"`: RH4105, RH4106, RH4107, RH4115, RH4117, RH2001 (camel/_camel); PascalCase fixes; RH4121 `ToTypeParameterName` | all go through the shared helper |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| Core `ToCamelCaseLowercasesTheLeadingUppercaseRunOfTheFirstWord`, `ToUnderlineCamelCase…`, `ToCamelCaseTakesTheLeadingUppercaseRunFromTheFirstWordOnly`, `ToPascalCaseKeepsALeadingAcronym` | exact derived name per row, both sides of the first-word boundary (FC-003) | any other string | `Assert.AreEqual` | adequate |
| `RH4106…FixWritesALeadingAcronymInLowercase`, `…FromTheFirstWordOnly`, RH4105/RH4107/RH4115/RH4117 `FixWritesALeadingAcronymInLowercase`, RH4110 `FixKeepsALeadingAcronym`, RH2001 `VerifyFixWritesALeadingAcronymOfTheFieldNameInLowercase` | exact fixed document; re-analysis clean | different name or leftover diagnostic | `Verify` | adequate |
| `RH4106…FixAllSkipsAFieldWithALeadingAcronymThatCollidesWithAnotherField` (reshaped, decision 2) | single fix renames `HTTPClient`, withholds `HttpClient`; Fix All skips both, renames `Count` | Fix All renaming either colliding field | `Verify` with `BatchFixedState`, `NumberOfFixAllIterations = -2` | adequate; the `-2` follows the existing convention for a skipped group and the final state is exact |

### Visible output is decided
Every changed expected value maps to Q1 (leading acronym, RH2001 backing field, `URLs`), Q10 (first-word run), or SHARED-CASING-010 (unchanged) for the collision test.

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._

### Hints
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1072

### Scope
Full audit, same SHAs. Diff: `CasingRenameSafetyUtilities.WritesNameAsKeywordAsync`/`CountVerbatimIdentifiers`, called from `CasingCodeFixProviderBase{T}.RenameWithoutConflictAsync`.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_N/A — none._ Both registration (`IsRenameUnsafeAsync` → `RenameWithoutConflictAsync`) and Fix All (`:633`, and `CandidatesConflictAsync`) pass through the new check; `ApplyCodeFixAsync` (`:141`) only applies a rename the check already accepted. Probed with `apply-fix.sh RH4105`: `int M(int @Count) { return @Count; }` → `count` (no false withholding for a verbatim non-keyword name, LF and CRLF); `Await` in a non-async method referenced inside an async lambda → `no-fix-offered` (the reference would need `@await`), as SHARED-CASING-013 states for a reference.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| The rename writes the new name with `@` (SHARED-CASING-013) | Every identifier token spelled `@<new>` in every changed document, counted before and after | Spec: "with the verbatim prefix `@` at the declaration or at a reference" | Agree; reads the Renamer's result, no keyword list (FC-044) | ok |
| The rename writes the bare new name where the text reads it as a keyword | Every identifier token spelled `<new>` in each changed document, re-parsed with the tree's own options | Spec: "a contextual keyword that is a keyword where the declaration or a reference stands" | Wider than the renamed tokens, but pre-existing tokens came from a text parse and re-parse identically; no false positive found (`Var`, `Nameof`, `Yield`, `Dynamic`, `Async`, `@Count`) | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Keyword / verbatim-prefix decision for a rename | `rg -n '"@" \+\|IsReservedKeyword\|GetKeywordKind' src/Reihitsu.Analyzer*`: only `WritesNameAsKeywordAsync` | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH4105 `NoFixIsOffered…VerbatimPrefix`, `…AwaitInsideAnAsyncLocalFunction`, `…VerbatimParameter…ReservedKeyword`; RH4115 `…ReservedKeyword`, `…AwaitInsideAnAsyncLambda`, `…QueryExpression…`; RH4117 `…ReservedKeyword` | no action offered; diagnostic stays | an action that writes `@x` or a bare keyword | `Verify(source, source)` (the framework rejects an offered no-op action) | adequate |
| RH4105/RH4115 `FixRenamesToAContextualKeywordThatNeedsNoVerbatimPrefix` | other side of the boundary (FC-003) | withholding `var`/`nameof`/`await`/`yield`/`dynamic`/`async` | `Verify` | adequate |
| RH4105 `FixAllRenamesTheOtherParameters…`, RH4117 `Object`/`Total` | Fix All skips only the keyword candidate (FC-008) | all or none renamed | `Verify` with batch state | adequate |

### Visible output is decided
Q6 A; the `@Class`, `Select`-in-a-query, and async-local-function members are rare withheld fixes the SHARED-CASING-013 text decides (nothing in question per `squad/README.md`; the PR names them under *Review notes*).

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._

### Hints
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1451

### Scope
Full audit, same SHAs. Diff: `CasingUtilities.IsWordCharacter`/`IsCombiningMark`/`ToPascalCaseWord`/`ToTypeParameterName`, `RH4005InterfaceNameCasingCodeFixProvider.OnTransformIdentifier`.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | low | `src/Reihitsu.Core/CasingUtilities.cs:399` (`ToPascalCaseWord`, `char.ToUpperInvariant(input[start])`) | FC-023 | – | same requirement | SHARED-CASING-006 says a letter number and a combining mark "are kept as written", but the first character of every word after the first is upper-cased whatever its category. Two characters in the covered set have an uppercase mapping: the lowercase Roman numerals U+2170–U+217F (Nl) and U+0345 COMBINING GREEK YPOGEGRAMMENI (Mn). Probed with `CasingUtilities.ToPascalCase` on `396887f`: `class_ⅻ` gives `ClassⅫ` (U+216B), not `Classⅻ`; `order_ⅻx` gives `OrderⅫx`; `a_` + U+0345 + `b` gives `A` + U+0399 GREEK CAPITAL LETTER IOTA + `b` — a combining mark the author wrote becomes a letter. Reached through every PascalCase, camelCase, and `_camelCase` fix on such a name. Members that hold: a later word starting with a digit, Pc, an uncased letter, or any other Mn/Mc (no case mapping, so unchanged). The first word is outside coverage (letter number) or cannot start with a mark. Rare input; the derived names compile. | Apply the upper-casing of a word's first character only to a letter (a cheap category check), or have the spec author state that the first character of every word is upper-cased whatever its category; either removes the contradiction. |

Traced and holding: SHARED-CASING-001/002/003 (`IsWordCharacter` = letter/digit plus Nl, Mn, Mc, Pc other than `_`; `_` the only separator); the all-uppercase test ignores marks, and Nl/Pc keep a word from being all uppercase (`CAFÉ_NAME`→`CaféName`, `CLASSⅫ_NAME`→`CLASSⅫName`); RH4005-002 (`i_document`→`IDocument`, `I_reader`→`IReader`, `I_O_Stream`→`IOStream`, `i__writer`→`IWriter`; `I`, `i`, `I_`, `i_` return the old name and are withheld; `i2Factory`/`i18nService` derive a name the analyzer still reports and are withheld by SHARED-CASING-008); RH4121-003 (`T_Key`, `t_value`, `T_1`, `T_`, `T__Key`, `T_T`; `C<T_, T>` collides, SHARED-CASING-009). Surrogate pairs cannot reach the change: Roslyn does not accept supplementary characters in identifiers. Lt and Cf are outside the covered set as the spec states.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Word character (`IsWordCharacter`) | one char: `_` excluded; `IsLetterOrDigit` or Nl/Mn/Mc/Pc | SHARED-CASING-001 category list; `SplitIntoPascalCaseWords` splits on its negation | Agree (Lt accepted through `IsLetterOrDigit`, outside the spec's covered set, unchanged from `main`) | ok |
| Case of a word's characters (`ToPascalCaseWord`) | rest of the word, marks skipped for the all-uppercase test | SHARED-CASING-006 "kept as written" for Nl and marks | First character is upper-cased regardless of category | finding #2 |
| RH4005 prefix (fix) | char 0 and char 1 (`IsUpper`/`IsDigit`/`_`/end), then leading `_` | RH4005 analyzer `IsInterfacePascalCase` (`I` + uppercase + `IsPascalCase`) | A digit after the prefix is a prefix for the fix, not a valid name for the analyzer; the spec (Q2 A) routes it to SHARED-CASING-008, and the fix is withheld | ok |
| RH4121 prefix (`ToTypeParameterName`) | char 0, char 1 (`IsUpper`/`IsDigit`/`_`), then leading `_` | `IsTypeParameterName` (`T` + PascalCase or digits) | Agree; every derived name satisfies `IsTypeParameterNameSuffix` or the input is returned | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Identifier-character categories | `rg "GetUnicodeCategory\|IsLetterOrDigit" src` (non-formatter): only `CasingUtilities` | one owner |
| `I` prefix | RH4005 analyzer (validity) and RH4005 fix (prefix to keep) — two questions, both stated in RH4005 | justified, unchanged structure |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| Core `Is*CaseAccepts…`, `ToPascalCaseKeepsEveryIdentifierCharacterInItsWord`, `ToCamelCaseCountsACombiningMark…`, `ToTypeParameterNameDropsTheUnderscores…` | exact bool / exact string per row, with negatives (`café`, `Order_Line`, leading `Ⅻ`) | any other value | `Assert.AreEqual` | adequate; no row has a later word starting with a cased Nl or U+0345 (finding #2) |
| RH4002 `CombiningMarks…`, `ConnectorPunctuation…`, `FixKeeps…`; RH4110, RH4105, RH4106 Unicode tests; RH4005 `FixDropsTheUnderscores…`, `NoFixIsOfferedWhenNoUsableNameCanBeDerived` rows; RH4121 `FixDropsTheUnderscores…`, `…DerivesTheNameOfAnotherTypeParameter` | exact diagnostics / exact fixed document | reported accepted name, dropped character, doubled prefix | `Verify` | adequate |

### Visible output is decided
Q2, Q3, Q4, Q5 A; `First＿Name`, `i18nService`, `I_O_Stream`, `T__Key` are members the answered requirement text decides; `C<T_, T>` follows unchanged SHARED-CASING-009.

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._

### Hints
- `T‿Key` (U+203F after the `T`) derives `TT‿Key`: connector punctuation other than `_` does not separate a prefix (RH4121-003 names only `_`), so the prefix is doubled. `main` gave `TTKey` (doubled too, with the character dropped). Spec-consistent and rare; not a finding.

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1434

### Scope
Full audit, same SHAs. Diff: `CasingAnalyzerBase.IsDictatedMember`/`HasNativeEntryPointFromName`/`IsDictatedName` hook and the overrides in RH4102, RH4103, RH4104, RH4112, RH4113, RH4114, RH4116.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Analyzer/Rules/Naming/RH4116TupleElementCasingAnalyzer.cs` `IsDictatedName` (signature selection `member switch`, and `Declaration.Variables.FirstOrDefault()`) | FC-016, FC-024, FC-002 | – | same requirement | SHARED-CASING-014 (second bullet) and RH4116-001 require that an element name of a tuple type in the signature of any dictated member is not reported. The signature is selected per syntax kind, and two members of the class fall through. (a) Operators and conversion operators: `OperatorDeclarationSyntax` and `ConversionOperatorDeclarationSyntax` hit `_ => []`, so their signatures are never inspected. Probe, with a referenced assembly declaring `public interface IOps<T> where T : IOps<T> { static abstract (int first, int second) operator +(T a, T b); static abstract implicit operator (int low, int high)(T value); }`: `public struct P : IOps<P> { public static (int first, int second) operator +(P a, P b) => default; public static implicit operator (int low, int high)(P value) => default; }` reports RH4116 on `first`, `second`, `low`, `high` (4 diagnostics, expected 0); the explicit form `static (int first, int second) IOps<P>.operator +(P a, P b) => default;` reports `first`, `second` (expected 0). `IsDictatedMember` itself would answer true for these symbols; only the syntax selection misses them. (b) A field-like event with several declarators: only the first declarator is asked. With `interface IMeta { event System.Action<(int first, int second)> changed; }` in a referenced assembly, `public event Action<(int first, int second)> local, changed;` reports `first`, `second` although the shared type is in the signature of the dictated `changed`. Members that hold (probed or tested): method return and parameter types, explicit method implementation, property type, indexer type and parameter type, field-like event with one declarator, event with accessors, nested positions (type argument, array element, tuple in tuple). | Select the signature by the cause — the tuple type lies in the member declaration outside its body, expression body, accessor list, initializer, and attribute lists — or at least cover every `BaseMethodDeclarationSyntax` that can implement (method, operator, conversion operator, with `OperatorDeclarationSyntax.ReturnType` / `ConversionOperatorDeclarationSyntax.Type`) and treat an event field as dictated when any declarator is; add tests for an implicit and an explicit operator implementation, a conversion operator, and the non-first event declarator. |

Traced and holding: `IsDictatedMember` walks `OverriddenMethod/Property/Event` through every level and, at each level, the explicit implementations plus `FindImplementationForInterfaceMember` on the member's own containing type — compiler results, not a model (FC-044); the boundary is `OriginalDefinition.ContainingAssembly`, so another project of the solution counts as outside (Q8 A) and a member implementing only on behalf of a derived type stays reported (Q11 A); partial parts compare through `PartialMemberUtilities.GetDefiningPartSymbol`. RH4102 matches the declarator by span; RH4103 skips explicit implementations by syntax as before; RH4104 asks only the native question. `HasNativeEntryPointFromName` binds the attribute and reads whether an `EntryPoint` named argument is written (spec: "whatever the argument's value"); `nameof` and literal entry points stay reported.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Dictated member (`IsDictatedMember`) | override chain; per level, explicit implementations and the own type's interface map, filtered by `member.Name` | Fix side: cascade withholding (`CasingRenameSafetyUtilities`, SHARED-CASING-009) for a reported source member whose rename reaches a dictated one | Agree: an exempt member is never the declaration a fix starts from; a cascade into it from a reported member is withheld (`NoFixIsOfferedWhenADerivedTypeImplements…`, `…CascadesToAnOverrideOfAReferencedAssemblyMember`) or, across projects, renamed with it as SHARED-CASING-007/014 state | ok |
| Tuple in a dictated signature (RH4116) | return/parameter types of `MethodDeclarationSyntax`; type and indexer parameters of `BasePropertyDeclarationSyntax`; type of `EventFieldDeclarationSyntax`, asking its first declarator | SHARED-CASING-014 bullet 2: every dictated member's return type, parameter types, property/indexer/event type | Operators, conversion operators, and non-first event declarators are outside the inspected span | finding #1 |
| Native entry point from the name (analyzer) | the declaration's attribute lists; first attribute bound to `System.Runtime.InteropServices.DllImportAttribute`/`LibraryImportAttribute`; `NameEquals` `EntryPoint` | Fix side `GetNativeEntryPoint` (`EntryPointName ?? method.Name`), compared before and after a cascade rename | Agree: an exempt method's entry point is its name, so any cascade rename into it changes the entry point and is withheld | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Override / interface-implementation walk for "declared outside" | `rg "FindImplementationForInterfaceMember\|OverriddenMethod\|OverriddenProperty\|OverriddenEvent" src`: `CasingAnalyzerBase` (new), `InterfaceImplementationUtilities` and `OverrideMemberUtilities` (region names), `DocumentationAnalysisUtilities` (inheritdoc), RH3101, RH7405–RH7408 | different decisions; one owner of "dictated" |
| `DllImport`/`LibraryImport` entry point | `CasingAnalyzerBase.HasNativeEntryPointFromName` (is an `EntryPoint` written) and `CasingRenameSafetyUtilities.GetNativeEntryPoint` (which name a symbol binds) | two decisions in two assemblies (the analyzer cannot reference CodeFixes); parity shown above | justified |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH4103 `MethodsWhoseNamesAReferencedAssemblyDictatesAreNotReported`, `…StaticAbstract…`, `…ReferencedProject…RenamedWithIt`, native tests, rewritten cascade tests, `NoFixIsOfferedWhenADerivedTypeImplements…` (kept), `FixRenamesAnOverrideChainDeclaredInSourceTogether` (kept) | exact diagnostic set; fix withheld or exact cross-project rename | a reported dictated name, an unreported in-compilation chain | analyzer `Verify` / `Verify(source, source)` / additional project | adequate, both sides (FC-003) |
| RH4102 field-like/accessor/interface events; RH4112–RH4114 property overrides; RH4114 partial property | exact diagnostic set | a reported dictated name | analyzer `Verify` | adequate |
| RH4104 `AnExternLocalFunctionIsReportedOnlyWhenItsAttributeNamesAnEntryPoint`, `…EntryPointIsNameofItself` | exempt without `EntryPoint`; reported and fixed/withheld with one | wrong side of the `EntryPoint` boundary | `Verify` | adequate |
| RH4116 override, explicit implementation, type-argument (Q12 A), nested positions | exact diagnostic set | a reported dictated element name, or an unreported base-list name | analyzer `Verify` | adequate for what it covers; operators, conversion operators, and multi-declarator events have no test (finding #1) |

### Visible output is decided
Q7, Q8, Q9, Q11, Q12 A, and the unchanged SHARED-CASING-009 for the rewritten cascade example.

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._ Finding #1 is a list-shaped selection inside one component, not two owners of one decision.

### Hints
- RH4118 (tuple-expression names) reports a `return (first: 1, second: 2);` inside a dictated member; such names are optional (omitting them is the resolution), so they are not dictated and SHARED-CASING-014 rightly leaves them to RH4118. Not a finding.

### Follow-up candidates
_None_ beyond the RH4120 candidate already in `00-lead.md`.

## Whole run

- State: checkout `396887f776fe938dacc896043f105f5178b47bb9`; `origin/main` `75cc39fa61efdb2aa564ba57f36e86127b33485b`, merged; diff matches `04`–`10` (12 production files, the listed tests, specs, change docs).
- `scripts/lane-check.sh behavior-change --base 75cc39f…`: `LANE CHECK: confirmed — claimed behavior-change, diff needs behavior-change`.
- Comments (FC-031): changed summaries describe the new code (`RenameWithoutConflictAsync`, `ToTypeParameterName`, `OnTransformIdentifier`, `SplitIntoPascalCaseWords`, `IsDictatedMember`, RH4116 `IsDictatedName`); no `TODO`, no commented-out code, no tracker numbers (`rg "TODO|#[0-9]{3,4}"` over the changed `src`/`tests` files: none).
- Rule pages: none changed, and no example on the RH4002, RH4005, RH4102–RH4107, RH4112–RH4117, RH4121 pages involves an affected case (`rg` for `DllImport`, `override`, keywords, `_document`, `T_`, `é`).
- Cost of special cases: none. The second arm of `WritesNameAsKeywordAsync` (re-parse) reads the Renamer's result for the whole class of positions instead of a list; no formatter phase or shape that could be declared unsupported is involved.
- Scope: no edit outside the run's components.
- Builds and test results of the cleaner (`07`) and implementer (`05`) trusted; re-ran only the probes above.

### Failure classes dismissed
- FC-001, FC-002: checked per guard above; FC-002 is part of finding #1 (the "signature of a dictated member" qualifier became a syntax-kind list).
- FC-003: both sides tested for every changed condition (see the tables).
- FC-004, FC-005, FC-019, FC-020, FC-021, FC-026: the fixes rename through `Renamer.RenameSymbolAsync` only and the analyzers judge names; no gap, trivia, or token move is touched.
- FC-006: a rename to a keyword is now withheld (#1072); other renames are guarded by the unchanged compile-error check.
- FC-007: every derived camelCase/PascalCase/`_camelCase` name of a covered input satisfies its style predicate, so one pass silences the diagnostic.
- FC-008: Fix All tested with a withheld keyword candidate and a new acronym collision.
- FC-009: SHARED-CASING-013 withholds before an action is registered; tests assert no action.
- FC-010–FC-014, FC-028–FC-030, FC-038–FC-043: no formatter phase or entry point is involved.
- FC-015: one owner per policy (tables above).
- FC-016: siblings checked — RH4111 (private properties cannot be dictated), RH4105/RH4121 (non-goal), RH4118 (hint), RH4120 (follow-up); finding #1 is the sibling-shape miss inside RH4116.
- FC-017: assertions exact throughout.
- FC-018, FC-036: unchanged rename path.
- FC-022, FC-025, FC-027 (scopes: lambda, local function, query covered for #1072), FC-029: no literal, expression-body, or layout change.
- FC-023: finding #2; verbatim identifiers covered (`@Class`, `@Count` probe).
- FC-024: partial property, explicit implementation, static abstract method covered; static abstract operators are finding #1.
- FC-031, FC-037: see above; no text-only claim is made.
- FC-032: no formatter change, so earlier analyzer results stay valid.
- FC-033–FC-035: expected values come from the spec examples; probes used the spec's own shapes.
- FC-044: dictated members read `OverriddenX` and `FindImplementationForInterfaceMember`; the keyword check reads the Renamer's result.
