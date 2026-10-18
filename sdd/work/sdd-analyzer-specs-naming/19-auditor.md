Role: auditor
Commit: c38eea9ab136a4a3e14b0a53fdd8fdf9078a170f
Verdict: pass, text cleanup

My prompt named the repair range, the head, and the handovers `15-` to `18-`. It carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore. I changed no file other than this handover.

## Scope

- Head: `c38eea9ab136a4a3e14b0a53fdd8fdf9078a170f`, checked out and clean.
- Base: `git ls-remote origin refs/heads/main` gives `4efe6aea3f2db03ee4b91ed03edf56c958a8afe3`, an ancestor of the head.
- This is a repair-delta audit against `0e549e2` (the commit `14-auditor.md` audited), approved as a second repair round by `squad/decisions/2026-10-17-second-repair-round-naming-specs.md`.
- The repair diff matches `15-` to `18-`. It touches:
  - spec text in `shared/casing.md`, `RH4001.md`, `RH4005.md`, and `RH4009.md`;
  - three test files (RH4001, RH4002, RH4009);
  - the decisions entry;
  - `sdd/work/`.
- No production file, ledger line, code owner, or index row changed. The scope did not grow.
- Evidence about production code in `14-auditor.md` (guard scope, policy ownership, dismissed failure classes) is byte-identical, because no compiled production file changed, and it is reused here.
- New evidence:
  - `scripts/apply-fix.sh` over scratchpad fixtures for RH4002, RH4105, and RH4106, LF and CRLF agreeing in every arm;
  - `rg` over the specs and the tests.

  The fixtures were deleted afterwards. I re-ran no test, because no finding turns on one. I trust the cleaner's Release build and the focused results in `16-` to `18-`.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | SHARED-CASING-006 now covers only an old name whose first word starts with a letter that has both an uppercase and a lowercase form. That excludes every counterexample I gave: `__1st`, `_1_value`, `注文_明細`, `_注文`, `_2x`, and `注文`. The false sentence "The result is the PascalCase name" is gone. RH4005-002 is narrowed to "a rest already in PascalCase or a name that SHARED-CASING-006 covers". SHARED-CASING-008 reaches outside that coverage through "the steps of SHARED-CASING-006", so its `__` and `_1st` examples still hold. The inputs are bug thoenissen/Reihitsu#1016. Two residuals of the new wording are findings N1 and N2 |
| 2 | closed | RH4009-001 now reads "malformed before its root value ends". That matches the `JsonException` path of `ConfigurationManager.GetConfiguration`, which leaves a null configuration even when the list was parsed before the failure. RH4009-002 covers only a file without content after its root object, which matches the reader stopping at the root's `EndObject`. The new test `ConfigurationMalformedInsideItsRootObjectIsInactive` fails if a list parsed before the exception were used |
| 3 | closed | RH4001-006 has the other-folder example. It agrees with `TargetDocumentAlreadyExists` (`Folders.SequenceEqual`) and `ApplyCodeFixAsync` (`Path.Combine` of the document's own directory, its own folders kept). The covering test is `FixIsOfferedWhenDocumentWithTheNewFileNameIsInAnotherFolder`. SHARED-CASING-009 has the nested-type example. The covering test is `NoFixIsOfferedWhenRenamingANestedClassAddsACompileErrorInItsContainingType` |
| 4 | closed | SHARED-CASING-007 and SHARED-CASING-009 say that "directly in a namespace" includes the global namespace (a type at file level). That agrees with `ContainingSymbol is INamespaceSymbol` in `PrepareRenameAsync` |
| 5 | closed | `rg "Test0|TestClass|TestComponent|Mismatched|LaterType|EmptyNamespace|PopulatedNamespace|Second\b"` over `RH4001.md` and `shared/casing.md` finds nothing. The new names are neutral, and the covering tests mirror them |

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| N1 | low | `sdd/specs/shared/casing.md` SHARED-CASING-006, new coverage sentence | FC-002 | same requirement (introduced by the repair) | See *Finding N1* below. | See *Finding N1* below. |
| N2 | low | `sdd/specs/analyzer/RH4106.md` RH4106-004 | FC-016 | same requirement (a counterpart the repair's narrowing reaches) | See *Finding N2* below. | See *Finding N2* below. |

### Finding N1 — the coverage boundary is undecided for letters with case but no one-character mapping

**What goes wrong.** The new boundary, "starts with a letter that has both an uppercase and a lowercase form", does not decide letters that have a case partner but no one-character uppercase mapping. For example, `ß` has the capital `ẞ` (U+1E9E) but uppercases to itself; the ligatures and `ŉ` are similar. For PascalCase targets, the fix then produces a name the rule still reports.

Observed with `apply-fix.sh RH4002`:

- `class _ßx` → `class ßx`: still reported, no further fix.
- `class ß_x` → `ßx`, again ending still reported with no fix.

Controls:

- RH4106 `ß_x` → `_ßX` is corrected, because `ß` is lowercase.
- RH4105 `_ŉx` → `ŉx` is corrected.

So the input class is the same as finding 1, at the edge of the new boundary.

**Suggested change.** State the boundary in terms of the one-character case mapping. For example: "starts with a letter whose one-character uppercase and lowercase forms differ". Optionally add `_ßx` to thoenissen/Reihitsu#1016.

### Finding N2 — RH4106-004 lacks the coverage sentence the repair relies on

**What goes wrong.** The repair narrows every casing fix requirement through the sentence "This requirement covers the renames that SHARED-CASING-007 covers", which now starts with "an old name that SHARED-CASING-006 covers". `15-spec-author.md` says every rule fix requirement inherits it. RH4106-004 is the only casing fix requirement without the sentence; `grep -L` over `RH40*.md` and `RH41*.md` finds only RH4106 among the rules with a casing fix.

It is also the `_camelCase` requirement whose canonical bug shape is `__1st` → `_1st` (thoenissen/Reihitsu#1016). RH4106-004 keeps that input out only if "the `_camelCase` name that SHARED-CASING-006 derives" is read as limited to 006's coverage. Only the new "the steps of" wording in SHARED-CASING-008 draws that distinction. RH4107-002, the identical `_camelCase` fix, excludes the input explicitly.

**Suggested change.** End RH4106-004 with "This requirement covers the renames that SHARED-CASING-007 covers.", as every sibling fix requirement does.

### Why `pass, text cleanup` holds

Both findings are confined to sentences in two spec files under `sdd/specs/`. Neither can reach compiled behavior, diagnostics, public API documentation, or rule metadata:

- **N1:** it only sharpens the wording of a coverage boundary. No tagged test uses a letter of that class; `rg "ß|ŉ|ǰ|ﬀ|ﬁ|ﬂ"` over the naming tests, `CasingUtilitiesTests`, and the specs finds none. So no tag or assertion depends on it.
- **N2:** it adds the sentence that 16 sibling requirements already carry. No RH4106-tagged test uses an input outside SHARED-CASING-006's coverage; `rg "__1st|_1_|注"` over the tests finds none. So no tag moves.

The requirement IDs, the ledger, the code owners, the header rows, and the examples stay unchanged.

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| SHARED-CASING-006 coverage: first word starts with a cased letter | First non-empty part between underscores, its first character | `ConvertToPascalCase` (`char.ToUpperInvariant` on each word's first character); `ToCamelCase` / `ToUnderlineCamelCase` (`char.ToLowerInvariant`); the style checks `char.IsUpper` / `char.IsLower` | Agrees for digits and caseless letters (Lo/Lm). Undecided for letters with a case partner but no one-character mapping | N1 |
| Inheritance of SHARED-CASING-007 coverage by the rule fix requirements | The 17 casing fix requirements | The sentence "This requirement covers the renames that SHARED-CASING-007 covers" (16 of them), plus RH4005-002's own clause | RH4106-004 lacks it | N2 |
| RH4005-002 coverage: a rest that is PascalCase or covered by 006 | The rest after the prefix decision | `OnTransformIdentifier` (`ToPascalCase` early return, then `I` +) against `IsInterfacePascalCase` | Agrees; the result is `I` followed by a Lu letter (modulo N1) | ok |
| RH4009-001 "malformed before its root value ends" | Parse of `reihitsu.json` | `GetConfiguration`: `JsonException` leaves `Configuration` null; `Utf8JsonReader` with the final block throws at depth > 0 | Agrees | ok |
| RH4009-002 "no content after its root object" | Bytes after the root `EndObject` | `ParseConfiguration` breaks at the root `EndObject` and never reads further | Agrees: the case is excluded, not claimed | ok |
| SHARED-CASING-007 / -009 global-namespace sentence | Containing symbol of the renamed type | `declaredSymbol.ContainingSymbol is INamespaceSymbol` (the global namespace included) | Agrees | ok |
| SHARED-CASING-009 nested-type example | Containing type's declarations | `RenameWithoutConflictAsync`: either the renamer conflict or `HasAddedBindingErrorsAsync` over `ContainingSymbol.DeclaringSyntaxReferences`; both withhold | Agrees (the example's claim holds whichever branch fires) | ok |
| RH4001-006 other folder | `Folders` of the existing document | `TargetDocumentAlreadyExists` (`Folders.SequenceEqual`); `ApplyCodeFixAsync` keeps `document.Folders` and the document's own directory | Agrees | ok |

## Policy ownership

_N/A — the repair changes no production type. The owners recorded in `14-auditor.md` are unchanged (byte-identical production sources)._

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH4001 `FixIsOfferedWhenDocumentWithTheNewFileNameIsInAnotherFolder` (new) | A same-named document in another folder does not withhold the fix; the file is renamed in its own folder | No fix, which leaves the fixed state unequal; a rename into the other folder (the test implementer saw the verifier fail on folders with `/0/Other/Order.cs`); an edit to `Archive/Order.cs` | `Verify(source, null, …)` with an exact fixed document set, paths and folders | Adequate |
| RH4002 `NoFixIsOfferedWhenRenamingANestedClassAddsACompileErrorInItsContainingType` (new) | A nested class whose member has the new name is reported with no fix | Any applied action | `Verify(source, source)` | Adequate for the spec's claim. It does not isolate which withholding branch fires, and the spec does not ask it to |
| RH4009 `ConfigurationMalformedInsideItsRootObjectIsInactive` (new) | A file cut off inside its root object disables the rule, even though `[ "A" ]` was parsed before the failure | `Company.Product` reported | Analyzer `Verify` with an additional file | Adequate |
| RH4001 rewritten example tests (`CodeFixRenamesFile`, `CodeFixRenamesFileForGenericType`, `CodeFixRenamesRazorFile`, `CodeFixPreservesDottedFileSuffix`, `LaterSiblingNamespaceTypeIsDetectedAndFileIsRenamed`, the match tests, `TypeNameIsComparedWithTheFileNameCaseSensitively`, the RH4001-006 withheld tests) | The renamed spec examples, word for word | A different span, a file name, or a fixed document set | `{|RH4001:…|}` markup with explicit paths | Adequate. The five rewritten fix tests moved from `{|#0:…|}` plus `Diagnostics(…, RH4001MessageFormat)` to `{|RH4001:…|}`, so they no longer assert the message. The message is not a requirement (`sdd/README.md`, *Analyzer specs*), and 8 other RH4001 tests still assert it (13 before) — hint |

## Failure classes dismissed

These cover the delta only; every other class keeps its `14-auditor.md` disposition, because no production code changed.

- **FC-003:** both missing sides now have an example and a test (finding 3 closed).
- **FC-007, FC-016, FC-023:** the reported shapes are excluded and filed as bug thoenissen/Reihitsu#1016. The residuals are N1 and N2.
- **FC-002:** the global-namespace qualifier is explicit (finding 4 closed). The new first-word qualifier is N1.
- **FC-017:** the three new tests are as strong as their claims. The message-assertion drop in the rewritten RH4001 tests does not weaken a specified guarantee (hint).
- **FC-035:** the expected values of the new tests come from the spec examples run on unchanged code.
- **FC-031:** the summaries of the three new tests describe what they assert. The rewritten tests kept their summaries, which still match.
- **FC-004 to FC-006, FC-008 to FC-015, FC-018 to FC-022, FC-024 to FC-030, FC-032 to FC-034, FC-036, FC-037:** the repair touches no fix, analyzer, or formatter code, and no comment-only C# change claims text-only status.

## Hints

- The five rewritten RH4001 fix tests no longer assert the diagnostic message. Eight RH4001 tests still do, and the message is owned by the resources, not by a requirement.
- The rewritten RH4001 inputs also lost their `/// <summary>` doc comments and their `namespace TestNamespace` wrappers. The namespace side of RH4001-001 and RH4001-005 stays covered by `LaterSiblingNamespaceTypeIsDetectedAndFileIsRenamed`, the RH4001-006 tests, and the other-folder test.
- `15-spec-author.md` (*Changed requirements*) says every rule fix requirement inherits the narrowed SHARED-CASING-007 coverage through the "covers the renames" sentence. RH4106-004 is the exception (N2).
