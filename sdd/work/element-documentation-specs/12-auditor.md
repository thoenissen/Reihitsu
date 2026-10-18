Role: auditor
Commit: 6aed45a723000c75ce98aeaa42d849100c8a4043
Verdict: pass

This is the repair round after `07-auditor.md`. The lead's message carried only the head and base SHAs and the handover list: no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore.

## Scope

- This audit covers only the repair delta `bd63778..6aed45a`, against the first audit at `bd63778f2b4ebfe9d72cb942289dea1f37ac9f9b`.
- State checks:
  - After `git pull`, the checkout is at `6aed45a`, and the tree is clean.
  - `git ls-remote origin refs/heads/main` returns `d83f2ae0b88a68fc626936144a7d7b1e01b861a4`, and that commit is an ancestor of HEAD.
  - `scripts/lane-check.sh tooling --head worktree` reports `confirmed — claimed tooling, diff needs tooling`.
- What the delta contains, matching `08`–`11`:
  - `sdd/specs/shared/element-documentation.md`: the coverage sentences of SHARED-DOC-002, SHARED-DOC-005, and SHARED-DOC-007, and SHARED-DOC-002's `Why:`;
  - the BOM of `InMemoryXmlReferenceResolver.cs`;
  - the RH8001 and RH8013 test classes;
  - work docs.
- The scope did not grow:
  - The one change outside my findings is the namespace narrowing of SHARED-DOC-005. It belongs to the same requirement set and the same question class as finding 2 (tag-name matching), and it is spec-only.
  - No production file, ledger line, index row, example, or `[Requirement]` attribute changed.
- Everything not in the delta is byte-identical to the first audit, so its evidence still holds.

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| _None._ | | | | | | |

### Status of the first audit's findings

| # | Status | Evidence |
|---|---|---|
| 1 (high, SHARED-DOC-002 and partial types) | closed | See below. |
| 2 (medium, SHARED-DOC-007 and an included `<Summary>`) | closed | See below. |
| 3 (low, duplicated Parse-mode setup) | closed | See below. |
| 4 (low, missing BOM) | closed | See below. |

**Finding 1 — closed.**
- The coverage now reads "every declaration except a static constructor declared in an interface and a partial type declared in several parts". It excludes the same shape that SHARED-DOC-001 and SHARED-DOC-005 exclude.
- The `Why:` no longer claims that a modifier-less part of a partial type is private. The added sentence states the C# rule, not the analyzer's output, so it is not a note about the open question.
- A partial type in one part stays covered. A partial constructor stays covered: CS8799 makes every part carry the same modifiers, so its group cannot split.
- #1037 now has "Q3, continued", with the counterexample, both readings, and SHARED-DOC-002 on its "Touches" line.
- No test tagged SHARED-DOC-002 declares a type in several parts.

**Finding 2 — closed.**
- The coverage now requires an included file that writes `summary` and `inheritdoc` in lower case, without a prefix, and in no namespace.
- The `upper.xml` counterexample is now outside SHARED-DOC-007, and #1037 Q5 holds it, together with `<INHERITDOC/>` in included content.

**Finding 3 — closed.**
- The three tests now pass `ApplyDocumentationModeParseToTestProject`. `rg ApplyParseDocumentationMode src` finds nothing.
- `git diff bd63778..HEAD -- src` changes no `[Requirement]`, test-method, or `Verify(source…)` line.
- The removed `Microsoft.CodeAnalysis` and `Microsoft.CodeAnalysis.CSharp` usings have no remaining users in either file, and the cleaner's Release build reports 0 warnings and 0 errors.

**Finding 4 — closed.** `InMemoryXmlReferenceResolver.cs` now starts with `ef bb bf`, exactly once, and no other byte changed. The two edited test classes still start with exactly one BOM.

**First audit's hint on `<x:summary>` — addressed.** The undeclared-prefix shape is in #1037 Q2.

### What the repair moved

**SHARED-DOC-005: "without a namespace prefix, and in no XML namespace".**
- I ran it in my scratch harness (Roslyn 5.9.0, the Debug analyzer of this tree), in `Diagnose` and `Parse` mode.
- These were not reported and raised no CS1570, so they are now excluded and filed in Q5:
  - `/// <x:summary xmlns:x="urn:x">…`;
  - `/// <summary xmlns="urn:x">…`;
  - an included `<summary xmlns="urn:x">`.
- A plain included `<summary>` was not reported either. It is still covered, which confirms the boundary on the covered side.
- SHARED-DOC-006 narrows with SHARED-DOC-005 through "what SHARED-DOC-005 covers".
- `rg 'xmlns|<[A-Za-z]+:(summary|inheritdoc)|<(Summary|SUMMARY|Inheritdoc|INHERITDOC)'` over the R01–R19 test classes finds nothing, so no tagged test now lies outside the coverage.

**Counterparts of the changed coverage sentences.**
- `rg -l 'SHARED-DOC-00[257]' sdd/specs` finds only RH8001–RH8019 and the ledger.
- Those rule specs reference the shared requirements without restating their coverage, so they need no change.
- RH8107 uses only SHARED-DOC-008. FMT-DOC-008 and RH8107-001 do not reference SHARED-DOC-005.
- Coverage sentences are an existing spec pattern on `main` (for example RH4105, RH4107, RH4109).

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Group, SHARED-DOC-002 coverage | The declaration's own modifiers and its direct container (`IsPurePrivateDeclaration`, `HasImplicitPrivateAccessibility`), unchanged | C# declared accessibility, which joins the parts of a partial type | They agree on every covered input. The one shape where they disagree, a partial type in several parts, is now excluded and filed as Q3, continued. | ok |
| Tag-name match, SHARED-DOC-005 to SHARED-DOC-007 coverage | `GetDirectTags` and `XmlDocumentationExpander.HasTag`: `LocalName`, `OrdinalIgnoreCase`, in the comment and in the expanded documentation, unchanged | Compiler-generated documentation and tools, which match case-sensitively and by namespace | They agree on every covered input. Upper case, prefixes, and namespaces, in comments and in included files, are now excluded and filed as Q5 (undeclared prefix: Q2). | ok |
| Parse-mode setup in tests | `ApplyDocumentationModeParseToTestProject` | — | one helper | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Parse-mode test setup | `AnalyzerTestsBase{TAnalyzer}.ApplyDocumentationModeParseToTestProject`; `RH0002DocumentationModeMustNotBeNoneAnalyzerTests.ApplyDocumentationMode` takes the mode as a parameter, in a class this run does not edit | one owner in the run's files |
| Production policies | unchanged since the first audit | as in `07-auditor.md` |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH8001 `VerifyNoDiagnosticForClassWithMalformedAmpersandInDocumentation`, `VerifyNoDiagnosticForClassWithUnclosedTagInDocumentation`; RH8013 `VerifyNoDiagnosticForEnumMemberWithMalformedAmpersandInDocumentation` | Malformed documentation in `Parse` mode does not crash and reports nothing. No `[Requirement]` tag, because the cases belong to open question Q2. | An exception or a diagnostic | `Verify` + the base Parse helper, which is identical to the removed local functions | unchanged strength |

## Failure classes dismissed

- FC-001 / FC-002 / FC-003: the moved boundaries are the coverage sentences. Both sides were checked: single-part versus multi-part partial types; lower-case, namespace-free names versus upper-case, prefixed, or namespaced names, in a comment and in an included file.
- FC-015: covered by finding 3, closed.
- FC-016: the sibling shapes of the namespace narrowing were checked: a later comment (through SHARED-DOC-006) and included content (SHARED-DOC-007).
- FC-031: the summaries of the touched tests still describe them.
- FC-032: no compiled production change.
- FC-033 to FC-035: no new test.
- FC-037: no proof that a change is comment-only is claimed.
- FC-004 to FC-014, FC-017 to FC-030, FC-036: the first audit's dismissals stand, because the delta does not reach their code.

## Hints

- Disclosure: in the first audit I wrote my harness into the session scratchpad folder `harness`, which the spec author had used before, and replaced its files. That is why `08-spec-author.md` worked from a private copy. It is outside the repository and has no effect on the tree.
- These first-audit hints stay open and are not findings:
  - the extension-block parent, for thoenissen/Reihitsu#983–#985;
  - no test for a comment between modifiers;
  - the repeated included-XML literal;
  - the pre-existing duplicated `GetDeclaredSymbol` and `HasRequiredDocumentation` bodies.
