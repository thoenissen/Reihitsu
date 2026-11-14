Role: auditor
Commit: 9bd5966
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

State verified: the checkout is 9bd5966 with a clean tree. `git ls-remote origin refs/heads/main` returns 8a26419228353ec26062c404565abb5c9c68ae10, which is an ancestor of HEAD. The diff (`git diff 8a26419 9bd5966`) is what handovers 01–10 describe:
- six spec files;
- `sdd/work/`;
- the tests;
- four production files: `src/Reihitsu.Core/UsingDirectiveOrderingUtilities.cs` and the RH7203, RH7204, and RH7206 analyzers.

There is no change doc.

Every visible output change is covered except one. Dropping a leading `global::` from the sort key also changes the order of `global::` names that are written plainly, and no issue, maintainer answer, or handover names that change (finding #1).

## Scope

Full audit of head 9bd5966 against base 8a26419228353ec26062c404565abb5c9c68ae10.

## thoenissen/Reihitsu#1105

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Core/UsingDirectiveOrderingUtilities.cs:91-96` (`GetSortKey` → `GetNameValue` → `StripGlobalQualifier`), spec `sdd/specs/shared/using-directive-ordering.md` SHARED-USING-004 ("`global :: System.IO` … sort key `System.IO`"); tests `UsingDirectiveNameValueTests.GlobalQualifierWithWhitespaceBelongsToTheSystemGroup`, `RH7207…AnalyzerTests.BlockGroupedAndOrderedByTheValueOfItsNamesIsNotReported`, RH7207-002 example | FC-016 | – | undecided output | The non-alias sort key now leaves out a leading `global::`. On `main` the key was `Name.ToString()`, `global::` included. Only the root dropped it (SHARED-USING-004, SHARED-USING-006). This changes output for the plainly written `global::` qualifier, which has no `@`, whitespace, comment, or escape, and which none of the three issues, the Q1 answer (`@` dropped, escapes decoded), or Decisions 2 and 5 name. The implementer's handover records it only as an internal note ("no existing test locked the old key"). **Run here:**<br>1. Formatter on `using System;⏎using global::System.IO;⏎using System.Collections;`:<br>- `main` gives `global::System.IO; System; System.Collections`.<br>- Head gives `System; System.Collections; global::System.IO`.<br>2. `scripts/apply-fix.sh RH7203` on `using global::System.IO;⏎using System.Collections;`:<br>- `main`: `no-diagnostic` (LF, CRLF).<br>- Head: `fixed`, which moves `System.Collections` first.<br>**Same mechanism, traced:**<br>- `using static global::System.Math; using static System.Console;` is reported by RH7206 and reordered by the formatter on head, and not on `main`.<br>- The global section (`global using global::System.IO;`) behaves the same way.<br>- Holds: alias directives, whose key is the alias name, and every group/root decision, which already stripped `global::` on `main`.<br>The stripping is not needed for #1105: keeping `global::` in the trivia-free value would still be idempotent and analyzer–formatter consistent. The added tests' expected values for `global :: System.IO` / `global :: System.Xml` depend on it, and no test pins the plain `global::System.IO` spelling. | The lead decides whether unchanged SHARED-USING-007 ("the imported namespace or type name") together with the Q1 answer ("ordered by its compiler-interpreted value") covers this. If they do not, ask it as an output question: `global::` kept in the key, as on `main`, or dropped. Either way, name the change under the PR's Review notes and pin the plain spelling: RH7203 on `global::System.IO` vs `System.Collections`, RH7206 on `static global::System.Math` vs `static System.Console`, and a formatter test. If the answer is to keep `main`'s key, `GetSortKey` uses the trivia-free value without stripping, and the two `global ::` expectations and the SHARED-USING-004 sentence follow. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Name value (`GetNameValue`) | `ValueText` of every `DescendantTokens()` of `UsingDirectiveSyntax.Name`. Trivia inside the name is excluded, structured trivia is not descended into, and a leading `global::` is stripped | SHARED-USING-004: value without `@`, with escapes decoded, without trivia between the tokens, including around `::` and inside type-argument lists | The tokens of the name are exactly what the spec names. The leading/trailing trivia of the name (a comment between `using` and the name) stays outside, as on `main` | holds, except the sort-key use of the `global::` strip (#1) |
| System membership (`IsSystemNamespaceUsing`) vs root (`GetRootNamespace`) | Both read `GetNameValue`. `System` / `System.*` vs the first segment | RH7201, RH7203's kind filter, `GetNamespaceGroupOrderKey` | The same value string, so `root == "System"` ⇔ `IsSystemNamespaceUsing` for regular directives. FC-016: both reads moved | holds |
| Non-name alias last (`Name == null ? 1 : 0` in `ComputeCanonicalOrder`) | `UsingDirectiveSyntax.Name` is null exactly when `NamespaceOrType` is not a `NameSyntax`: predefined, tuple, array, nullable, pointer, or function-pointer types | `AreInSameGroup` (root `""` only for `Name == null` in parseable code), RH7204's group, RH7207's blank-line check | Both partition on the same fact. The key sits after the kind, so regular and static directives are unaffected. Group boundaries in the rewriter (`UsingDirectiveOrderingRewriter.cs:139,154`) and in RH7207 come from `AreInSameGroup`, so `int` gets its own group behind `System` aliases | holds |
| Out of order within group (`GetDirectivesOutOfOrderWithinTheirGroup`) | Every earlier directive of the scope with `AreInSameGroup` (section, kind, root) and `CompareSortKeys` < 0 | `ComputeCanonicalOrder`: section, kind, name-null, group key, root ordinal, then `GetSortKey` `OrdinalIgnoreCase`, then source index | Same group definition and same comparer. Ties: strict `< 0` in both, so ties keep source order (SHARED-USING-007) | holds |
| RH7203 kind filter | `GetUsingDirectiveGroup is SystemNamespace or OtherNamespace` (regular) | `main`'s pre-partition by `SystemNamespace`/`OtherNamespace` | `AreInSameGroup`'s root comparison already separates the `System` root from the others, so dropping the pre-partition changes nothing | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Name value, root, `System` membership | `UsingDirectiveOrderingUtilities.GetNameValue/GetRootNamespace/IsSystemNamespaceUsing` only. `rg '"System\.?"\|"global::"'` and `rg 'NamespaceOrType\|\.Name\??\.ToString'` over production show no other reader of a using name: `UsingDirectiveOrderingSafety`, `UsingLeadingTriviaBuilder`, `UsingTrailingTriviaBuilder`, and RH5114 read trivia or layout only | one owner |
| Order within a group (SHARED-USING-007) | Core `ComputeCanonicalOrder` (RH7207, formatter via `UsingGrouping`) and Core `GetDirectivesOutOfOrderWithinTheirGroup` (RH7203, RH7204, RH7206) | One file, same `AreInSameGroup` + `CompareSortKeys`. The private loops are deleted (Decision 3) |
| Group order (non-name alias last) | `ComputeCanonicalOrder` only. `OrderUsings`/`OrderSubset` (no production caller, already a follow-up candidate) delegate to it | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7201 `SystemDirectiveWithWhitespace…`, `Verbatim…`, `…Comment…`, `GlobalQualified…`, `UnicodeEscaped…`, global-section test | A `System` directive spelled differently is reported after another root | A missing or extra diagnostic | `Verify` (exact diagnostic set) | adequate |
| RH7201 `CodeFixPlacesVerbatim…` (+CRLF), `CodeFixOfSystemDirectiveWithWhitespace…Converges` | The fix converges and places `@System.IO` in the `System` group | The fix leaves a diagnostic, or gives different output | `Verify(source, fixed)` with re-analysis; `ApplyCodeFixAsync` + `Verify(result)` | adequate; the convergence test pins no output, as 04 states |
| RH7203/RH7204/RH7206 value tests and every-earlier tests, plus the in-order counterparts | Every directive sorted before any earlier one of its group is reported, and only those | An extra or missing diagnostic on either side (FC-003) | `Verify` | adequate |
| RH7203 `MicrosoftUsingsAreFixedWithoutMovingSortedSystemGroup` (corrected) | Three directives sort before the earlier `…CSharp` | Count ≠ 3 | `Verify(source, fixed, 3 diagnostics)` | adequate (P18 clarification) |
| RH7207 non-name alias, value-block tests, fix (+CRLF), Fix All | A non-name alias forms the last group; a value-organized block is clean | A report on an organized block, or a fix that moves `int` first | `Verify`, `VerifyCodeFixWithEndOfLine`, Fix All scenario with 2 diagnostics in 2 blocks | adequate |
| RH7207 formatter tests | Formatter output is clean for RH7207 | A diagnostic on the formatter output | `VerifyFormatter` (2 passes, LF+CRLF) | adequate |
| `UsingDirectiveNameValueTests` (19) | Exact full-pipeline output; idempotent; every phase stable | Any difference after pass 1 or pass 2, LF or CRLF | `AssertRuleResult` | adequate; `System` stays literal in the raw strings (checked) |
| Plain `global::System.IO` sort key | — | — | — | missing; part of #1 |

### Failure classes dismissed
- FC-001, FC-002, FC-003, FC-015, FC-016: traced in the tables above. The only failing member is #1.
- FC-004, FC-005, FC-019, FC-020, FC-021: no gap is joined or collapsed. Directives move as whole nodes with their trivia, and blocks with a directive are refused by `CanSafelyReorder` (unchanged). A comment inside a name moves with its node (`NameWithCommentBetweenItsTokensBelongsToTheSystemGroup`).
- FC-006: reordering using directives within a scope never changes meaning; no rewrite of names.
- FC-007, FC-008, FC-009: the fixes run the formatter's using phase, and the RH7201/RH7207 fix tests re-analyze their output. The Fix All test covers 2 blocks. The skeptic's 33 `apply-fix` fixtures showed no non-convergence.
- FC-010, FC-011, FC-012: `AssertRuleResult` runs 2 passes plus phase stability, LF and CRLF. Grouping by value removes the using phase's dependence on the horizontal-spacing phase's later normalization.
- FC-013, FC-014: analyzer and formatter share one policy in Core, and the RH7207 `VerifyFormatter` tests pin parity.
- FC-017: see Assertion adequacy.
- FC-018, FC-036: no rename.
- FC-022: no literal is involved.
- FC-023: verbatim and Unicode identifiers are the subject and covered on every surface. A `@global::` or escaped `global` qualifier, whose value text is also stripped, was not traced. It is a shape people do not write, and it is not reported.
- FC-024, FC-025, FC-026, FC-027, FC-029: no member or expression rewrite. Nested namespace scopes are covered by the Fix All namespace block.
- FC-028: the using phase is not reached from the node-level entry points.
- FC-030: unchanged; the fixes organize the block they report.
- FC-031: the summaries of `IsSystemNamespaceUsing`, `GetRootNamespace`, `GetSortKey`, `ComputeCanonicalOrder`, the new members, and the three `OnUsingScope` methods describe the new code. The test comments in `GetFixAllScenario` were updated.
- FC-032: the implementer's full project runs follow the Core change, and the cleaner's Release build is on the final code.
- FC-033, FC-034: CRLF and the issues' own instances are tested.
- FC-035: expected values come from the specs and, for spacing, from `main` runs of single directives (04).
- FC-037: no text-only claim.
- FC-038 to FC-043: no shape is declared, and the precheck is unchanged.

### Architecture findings
_None._

### Hints
- `GetDirectivesOutOfOrderWithinTheirGroup` compares every pair in an in-order block, the common case. Each `AreInSameGroup` call rebuilds up to four name values (`GetUsingTypeOrder` → `IsSystemNamespaceUsing`, and `GetRootNamespace` ×2) with a `StringBuilder`, three analyzers per scope. For about 40 directives that is a few thousand small allocations per analysis. It is not a defect. Precomputing root and kind next to `sortKeys` would make it linear in allocations.

## thoenissen/Reihitsu#1266

### Findings
_None._ RH7203, RH7204, and RH7206 compare with every earlier directive of the group (SHARED-USING-007; maintainer's P18 clarification on the issue). This is traced under #1105's Guard scope, and tests cover the issue's instance, the earlier-higher member, and in-order counterparts for all three rules and the global section. The issue's "other ordering rules checked" goal: RH7201, RH7202, and RH7205 use whole-section flags and RH7207 uses the canonical order. Both are confirmed by reading their `OnUsingScope` methods.

### Guard scope
See #1105 (rows "Out of order within group" and "RH7203 kind filter").

### Policy ownership
See #1105.

### Assertion adequacy
See #1105.

### Failure classes dismissed
See #1105.

### Architecture findings
_None._ Decision 3 removed the three private copies (FC-015).

### Hints
_None._

## thoenissen/Reihitsu#1267

### Findings
_None._ SHARED-USING-004's last-group rule holds through `ComputeCanonicalOrder`'s name-null key:
- the predefined, tuple, array, and nullable targets;
- behind another root's alias group;
- in the global section;
- RH7207 and its fix (LF, CRLF, Fix All);
- the formatter, which is idempotent through `AssertRuleResult`.

RH7204 needs no change and is characterized. Two non-name aliases keep `main`'s order (Decision 4); no test locks it.

### Guard scope
See #1105 (row "Non-name alias last").

### Policy ownership
See #1105.

### Assertion adequacy
See #1105.

### Failure classes dismissed
See #1105. Pointer and function-pointer targets are not tested. They share the `Name == null` dispatch with the four tested kinds, and the key reads only that fact.

### Architecture findings
_None._

### Hints
_None._

## Whole run

- Scope: every changed path is in the run's components (Core ordering utilities, RH7203/RH7204/RH7206, their tests, RH7201/RH7207 tests, formatter regression tests, and the six specs). There is no `TODO` and no tracker number in the source diff.
- Executed for #1 (fixtures in the scratchpad, removed afterwards; `main` from a temporary worktree, removed): `scripts/format.sh` once on head and once on `main`, and `scripts/apply-fix.sh RH7203` on head and on `main`. The results are quoted in #1. No test was re-run: the implementer's, cleaner's, and test implementer's focused results are trusted.
