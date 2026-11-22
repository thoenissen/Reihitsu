Role: auditor
Commit: c94249b9a5691e2168203b2cecce9e00ce1def11
Verdict: #971: pass; #1072: pass; #1451: pass, text cleanup; #1434: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Read: `00-lead.md` (*Maintainer answers*, *Decisions taken* 1–3), `11-auditor.md`, `12-test-implementer.md`, `13-implementer.md`, `14-cleaner.md`, `15-spec-checker.md`, the repair diff `396887f..c94249b` (production: `CasingUtilities.ToPascalCaseWord`, `RH4116TupleElementCasingAnalyzer.IsDictatedName`/`GetSignatureTypes`; tests: Core `CasingUtilitiesTests`, RH4105, RH4110, RH4116), SHARED-CASING-006, SHARED-CASING-014, RH4116, and outside the diff `CasingUtilities.SplitIntoPascalCaseWords`/`LowercaseLeadingUppercaseRun`, `CasingAnalyzerBase.IsDictatedMember`, and the RH4120 follow-up in `01-spec-author.md`.

Probes ran in a detached worktree of `c94249b` under the scratchpad (removed afterwards): one temporary MSTest class in `Reihitsu.Analyzer.Test`, run with `scripts/test.sh --project analyzer --configuration Debug --filter`. Nothing in the shared tree was changed besides this handover.

## thoenissen/Reihitsu#971

Repair delta only. The repair reaches #971 through `ToPascalCaseWord`, which builds every word of a camelCase and `_camelCase` name. The changed branch only affects a word whose first character is not a letter, and `LowercaseLeadingUppercaseRun` acts only on `char.IsUpper` characters (Lu), so every row of the first audit (`HTTPClient`, `URLs`, `HTTP2Client`, `IOStream`, `m_HTTPClient`, `XML_Parser`, `ORDER‿LINE`) derives the same name. Finding table: _N/A — none._ All other sections: unchanged from `11-auditor.md`.

## thoenissen/Reihitsu#1072

Not touched by the repair. `CasingRenameSafetyUtilities` and `CasingCodeFixProviderBase{T}` are byte-identical to `396887f`, so the first audit's evidence still applies.

## thoenissen/Reihitsu#1451

### Scope
Repair delta against `396887f`, for finding #2. Head `c94249b9a5691e2168203b2cecce9e00ce1def11`, base `origin/main` `75cc39fa61efdb2aa564ba57f36e86127b33485b`.

Previous findings: **#2 — closed.** `ToPascalCaseWord` now upper-cases a word's first character only when `char.IsLetter` is true. Every member #2 listed now holds: the lowercase Roman numerals U+2170–U+217F and U+0345 at the start of a later word, through PascalCase, camelCase, and `_camelCase`. The exhaustive Core test sweeps every BMP character of Nl, Mn, Mc, and Pc, so no unnamed member is left on that axis. I enumerated the other positions of a case-mapped, kept-as-written character, and they hold:
- Inside a later word: Nl is not `char.IsUpper`, so it keeps the word from being all uppercase, and the word is kept as written. U+0345 is skipped for that test, and `ToLowerInvariant(U+0345)` is U+0345.
- In the first word's leading uppercase run: only Lu is lowercased.
- As the first character of the first word (`ⅻ_count`): this is outside SHARED-CASING-006 coverage. The result changes from `ⅫCount` to `ⅻCount`, but both fail `IsPascalCase`/`IsCamelCase`, so the fix stays withheld (SHARED-CASING-008) and the visible output does not change.
- RH4005/RH4121 prefixes (`I_ⅻ`): both the old and the new result are still reported, so the fix is withheld either way.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 4 | low | `src/Reihitsu.Core/CasingUtilities.cs:337-341` (`SplitIntoPascalCaseWords` summary) | FC-031 | – | PR-introduced | The summary still says "the first character of each word is written in uppercase". After the repair, a word's first character is upper-cased only when it is a letter, and any other first character is kept as written. `ToPascalCaseWord`'s own summary (`:375`) was updated, but this caller's summary, which restates the same policy, was not. This is comment only: it cannot reach compiled behavior, diagnostics, or rule metadata. | Reword it to "the first character of each word is written in uppercase when it is a letter", or point it at `ToPascalCaseWord` instead of restating the policy. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Case of a word's first character (`ToPascalCaseWord`) | `input[start]`, `char.IsLetter` | SHARED-CASING-006 "a letter number … and a combining mark … are kept as written"; the rest-of-word all-uppercase test (`IsCombiningMark` / `char.IsUpper`); `LowercaseLeadingUppercaseRun` (`char.IsUpper`) | Agree. Nl, Mn, Mc, Pc, and Nd are kept. Lt and Lm count as letters, but Lt is outside SHARED-CASING-001's covered set and Lm has no uppercase mapping in the BMP | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Case of a word's characters | `rg -n "ToUpperInvariant\|ToLowerInvariant" src/Reihitsu.Core/CasingUtilities.cs`: only `ToPascalCaseWord` and `LowercaseLeadingUppercaseRun`. The summary of `SplitIntoPascalCaseWords` restates the rule in prose (finding #4) | one code owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| Core `ToPascalCase…`, `ToCamelCase…`, `ToUnderlineCamelCase…KeepsALetterNumberOrCombiningMarkThatStartsALaterWordAsWritten` | exact derived name for every listed row, including `CLASS_ⅿ` (all-uppercase first word) and `order_ⅰ_line` (a one-character Nl word) | `Ⅻ`, `Ι`, or any other string | `Assert.AreEqual` | adequate |
| Core `EveryDerivationKeepsEachCharacterThatIsKeptAsWrittenWhenItStartsALaterWord` | every BMP Nl/Mn/Mc/Pc character (except `_`) as the first character of a later word is kept, through all three derivations | any character in the failure list | `Assert.IsEmpty` with the list in the message | adequate. It is exhaustive on the axis that #2 failed on |
| RH4110 / RH4105 `FixKeepsALetterNumberOrCombiningMarkThatStartsALaterWordAsWritten` | exact fixed document, references retargeted, re-analysis clean | `OrderⅫx`, `AΙb`, or a leftover diagnostic | code-fix `Verify` | adequate, on both the PascalCase and the camelCase fix path |

### Visible output is decided
Every changed expected value comes from SHARED-CASING-006 ("kept as written"), which is unchanged (lead decision 3).

### Architecture findings
_None._

### Hints
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1434

### Scope
Repair delta against `396887f`, for finding #1. Same SHAs.

Previous findings: **#1 — closed.** Every member it listed now holds:
- implicit implementations of operators and conversion operators;
- explicit implementations (`IOps<P>.operator +`, `implicit IOps<P>.operator (…)`);
- the non-first declarator of an event field.

The test implementer also added the checked operator, the unary operator, and the C# 14 instance compound-assignment operator. My probe of an event field whose second declarator implements a referenced-assembly event (`public event Action<(int first, int second)> b, a;`) reports nothing, as required.

One member of #1's class that #1's enumeration did not name still fails. It is reported as finding #3, which repeats #1.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | high | `src/Reihitsu.Analyzer/Rules/Naming/RH4116TupleElementCasingAnalyzer.cs` `IsDictatedName` (the `member switch` signature selection, `_ => []`) | FC-016, FC-024, FC-002 | #1 | same requirement | SHARED-CASING-014 bullet 2 / RH4116-001: an element name of a tuple type in "the type of a property" that is dictated must not be reported. A record's positional parameter declares a synthesized property whose type is the parameter's type syntax. When that property implements or overrides a member of a referenced assembly, its tuple element names are dictated. `node.Ancestors()` stops at the `RecordDeclarationSyntax`, which falls through to `_ => []`, so the names are reported. **Probed on `c94249b`**, with a referenced assembly declaring `public interface INamed { (int first, int second) Pair { get; } }`: (a) `public record R((int first, int second) Pair) : INamed;` reports RH4116 on `first` and `second`. (b) `public record struct S((int first, int second) Pair) : INamed;` does the same. 4 diagnostics in total, expected 0. (c) With `public abstract record Base { public abstract (int first, int second) Pair { get; init; } }` in a referenced assembly, `public record R((int first, int second) Pair) : Base;` reports `first` and `second` (2 diagnostics, expected 0). **The names are dictated.** Writing `(int First, int Second)` instead gives CS8141 twice for (a) (property and getter) and CS8139 three times for (c) (property, getter, init). **Members that hold:** every member kind of #1 listed above; methods, properties, indexers, events with accessors, and the first event declarator (first audit); and a tuple in a generic method's constraint clause, whose names the compiler does not dictate (`where T : IEquatable<(int First, int Second)>` implementing `IEquatable<(int first, int second)>` compiles). **No other member kind can be dictated:** constructors and destructors have no tuple-typed signature, and delegates, fields, and class or struct primary-constructor parameters declare no member that implements or overrides. | This is the second member that the syntax-kind list missed (#1 suggested selecting by the cause). Ask the symbol whose signature holds the tuple type, read from the compiler. For a tuple type in a record's positional parameter, that symbol is the synthesized property: the `IPropertySymbol` of the record whose declaring syntax is that `ParameterSyntax`. Add tests for (a), (b), and (c), and for the other side: a positional parameter that implements nothing, or implements a source interface, stays reported. See *Architecture findings* for how this relates to the RH4120 follow-up. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Tuple in a dictated signature (RH4116) | The nearest `MemberDeclarationSyntax` ancestor. Return and parameter types of a method, operator, or conversion operator (`GetSignatureTypes`); type and indexer parameters of `BasePropertyDeclarationSyntax`; type of `EventFieldDeclarationSyntax`, asking every declarator | SHARED-CASING-014 bullet 2: the return type, parameter types, and property/indexer/event type of every dictated member | A record's positional parameter, whose synthesized property is a dictated property, is outside the inspected span | finding #3 |
| Event field dictated (any declarator) | `Declaration.Variables`, each passed to `IsDictatedMember` | The spec: the type of an event field is the type of each event it declares | Agree | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Dictated member | `CasingAnalyzerBase.IsDictatedMember` only. RH4116 selects which declared symbol to ask; RH4102 matches the event declarator by span (`rg -ln "IsDictatedMember" src`) | one owner of the decision |
| Which declared symbol a record positional parameter stands for | No owner today. RH4116 (finding #3) and the RH4120 follow-up (`00-lead.md`, *Follow-up candidates*) both need it | see *Architecture findings* |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH4116 `…OperatorThatImplementsAReferencedAssemblyOperator…`, `…ConversionOperator…`, `…ExplicitImplementationOfAReferencedAssemblyOperator…`, `…CompoundAssignmentOperator…`, `…EventFieldAreNotReportedWhenALaterDeclaratorIsDictated` | no diagnostic for a dictated operator, conversion, or event-declarator signature | any RH4116 diagnostic | analyzer `Verify` + `CompiledAssemblyReference` | adequate |
| RH4116 `…OperatorThatIsNotDictatedAreReported`, `…EventFieldAreReportedWhenNoDeclaratorIsDictated` | the other side (FC-003): source-interface implementations, interface declarations, and non-implementing members stay reported | a missing diagnostic | analyzer `Verify` with exact markup | adequate |
| (missing) record positional parameter | — | — | — | no test (finding #3) |

### Visible output is decided
The expected values come from SHARED-CASING-014 bullet 2 / RH4116-001, both unchanged, and Q7 A.

### Architecture findings
**Finding #3 repeats #1.** It is the same decision failing between the same components, with only the input shape changed. The decision is which declared symbol owns the signature that a tuple type stands in. The two components are RH4116's syntax-kind selection in `IsDictatedName` and `CasingAnalyzerBase.IsDictatedMember`. The first audit named operators, conversion operators, and non-first event declarators, and the repair extended the list by those cells. A record's positional parameter is the next cell. Under `squad/README.md`, *Architecture findings*, the lead decides this instead of starting another repair round.

Inputs that show it are (a)–(c) in finding #3. The RH4120 follow-up in `00-lead.md` (`public record Person(string name) : INamed;`) fails for the same cause, on the parameter's name instead of its type: a positional parameter's synthesized property is dictated, and no casing component maps the parameter to that property.

### Hints
- An explicit interface specifier that repeats a tuple type argument, such as `void IMeta<(int a, int b)>.M()` or now `static … IOps<(int a, int b)>.operator +(…)` in a type whose base list names `IMeta<(int a, int b)>`, reports the element names both in the base list and in the specifier. The specifier is outside the signature that SHARED-CASING-014 names, so this is not a finding. It is pre-existing for methods. The spec's "reported once" sentence concerns the signature, not the specifier.

### Follow-up candidates
_None_ beyond the RH4120 candidate already in `00-lead.md`. Finding #3 shares its cause (see *Architecture findings*).

## Whole run

- **State:** checkout `c94249b9a5691e2168203b2cecce9e00ce1def11`. `origin/main` is `75cc39fa61efdb2aa564ba57f36e86127b33485b` (`git fetch` agrees) and is merged (`git merge-base --is-ancestor` true). The repair diff matches `12`–`15`: two production files, four test files, and handovers. No spec, change doc, or rule page changed, so scope did not grow and a delta audit applies.
- **Lane check:** `scripts/lane-check.sh behavior-change --base 75cc39f…` gives `LANE CHECK: confirmed — claimed behavior-change, diff needs behavior-change`.
- **Comments (FC-031):** the changed summaries (`ToPascalCaseWord`, `GetSignatureTypes`, the `IsDictatedName` remarks) describe the new code. One stale caller summary remains (finding #4). No `TODO`, no commented-out code, and no tracker numbers (`git diff 396887f..HEAD -- src tests` grepped for `TODO|#[0-9]{3,4}`: none).
- **Cost of special cases:** none. `char.IsLetter` is a category check, and `GetSignatureTypes` is a selection helper.
- **Scope:** no edit outside the run's components.
- **Trusted, not re-run:** the cleaner's Release build (`14`) and the implementer's focused and default-set results (`13`). I re-ran only the probes above.

### Failure classes dismissed
- FC-001: the event-field guard asks every declarator, and the span test `type.Span.Contains(node.Span)` is unchanged.
- FC-002: part of finding #3. "Signature of a dictated member" is still a syntax-kind list.
- FC-003: both sides are tested for operators and event fields; the record side is missing (finding #3).
- FC-004, FC-005, FC-019–FC-021, FC-026: no gap, trivia, or token move; the repair changes name derivation and reporting only.
- FC-006, FC-018, FC-036: the rename path is unchanged.
- FC-007: every derived name in the new rows (`Orderⅻx`, `A`U+0345`b`, `orderⅻx`, `a`U+0345`b`) satisfies its style predicate. The RH4110/RH4105 fix tests re-analyse clean.
- FC-008, FC-009: no new co-occurrence, and no new action registration.
- FC-010–FC-014, FC-028–FC-030, FC-038–FC-043: no formatter phase or entry point is involved.
- FC-015: one owner per policy (tables above).
- FC-016: covered by finding #3, a missed sibling kind.
- FC-017: assertions are exact, and the Core sweep is exhaustive.
- FC-022, FC-025, FC-027, FC-029: no literal, expression body, nested scope, or layout is involved.
- FC-023: the Unicode axis of #2 is fully swept.
- FC-024: the static abstract operators, conversions, and C# 14 compound assignment are now covered. Records are finding #3.
- FC-031: finding #4.
- FC-032: no formatter change.
- FC-033–FC-035: expected values come from the unchanged spec text, and the probes used the spec's own shapes.
- FC-037: no text-only claim is made.
- FC-044: the record-parameter suggestion reads the compiler's synthesized property and does not model it.
