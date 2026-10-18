Role: spec author
Commit: 757ff46da7d581134b239a5e9f33403dcea5f694
Verdict: spec written

The specs are in commit 806f24b. No change doc: every requirement writes down current behavior (a clarification, `sdd/README.md`, *When a change doc is required*). No production code, test, or rule page was touched. The rule pages under `documentation/rules/` for the 16 rules were read against the specs and need no change: none contradicts a requirement, and none describes a case left out below.

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## How the statements were established

- Read `TargetAttributeListShapeAnalyzerBase`, `TargetAttributeListShapeCodeFixProviderBase`, the 16 analyzers and code-fix providers, `AttributeTargetUtilities`, and the existing test classes `RH55xx…ShapeRulesAnalyzerTests`.
- Ran fixtures through `scripts/apply-fix.sh` for every rule, LF and CRLF (inputs and outputs are quoted in the questions below).
- Ran every C# block of the new and changed specs through the shipped analyzers and code fixes with a scratch verifier (Roslyn `CompilationWithAnalyzers` plus `CodeAction.GetOperationsAsync`, one fix per diagnostic until none is left). It checks the reported spans against the `{|RH####:…|}` markup, before/after pairs, "is not reported", and "no code fix is offered": 0 failures across all 16 specs and `shared/attribute-targets.md`, the existing SHARED-ATTR-001 to SHARED-ATTR-012 blocks included. The script is not in the repository.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RuleDocumentation"`: 26 passed. `RequirementCoverageTests` will fail until the new requirements are tagged.

## Changed requirements

`sdd/specs/shared/attribute-targets.md`:

- Header: code owners `TargetAttributeListShapeAnalyzerBase` and `TargetAttributeListShapeCodeFixProviderBase` added (also in `sdd/specs/README.md`). Clarified: the intro now covers the list-shape rules. The non-goal "How many attributes one list holds … the attribute list-shape rules" is folded into the first non-goal. The type-parameter non-goal now names placement only.
- SHARED-ATTR-013 — A list with several attributes is reported — added
- SHARED-ATTR-014 — Accessor lists of an inline auto-property are not split — added
- SHARED-ATTR-015 — The lists of one target are merged into the first — added
- SHARED-ATTR-016 — The list-shape diagnostic is on the attribute list — added
- SHARED-ATTR-017 — The split fix gives each attribute a list on its own line — added (scoped, see Q1 and Q2)
- SHARED-ATTR-018 — The merge fix moves the attributes into the first list — added
- SHARED-ATTR-019 — The split fix is withheld for a list with a comment or directive inside — added
- SHARED-ATTR-020 — The merge fix is withheld for a comment or directive around the group — added

New analyzer specs. All are added. The split rules all have the same four requirements:

- RH5502, RH5504, RH5506, RH5508, RH5510, RH5512, RH5516, RH5518, RH5520, RH5522, RH5526, RH5528: `-001` what is reported, `-002` diagnostic location, `-003` the fix, `-004` fix withheld.
- RH5514: `-001` reported, `-002` accessor lists not checked, `-003` location, `-004` fix, `-005` withheld.
- RH5524 and RH5529 (merge rules): `-001` reported, `-002` location, `-003` fix, `-004` withheld.
- RH5531: `-001` split in a multi-line accessor list, `-002` merge on an inline auto-property, `-003` method lists of other declarations not checked, `-004` location, `-005` fix (split or merge), `-006` withheld.

All 75 new IDs are in `sdd/specs/requirement-ids.txt`. Only lines were added; none was moved or removed.

## Examples for the tests

Every new requirement above has an example that is the seed for its test: SHARED-ATTR-013 to SHARED-ATTR-020, plus all of RH5502-001…004, RH5504-001…004, RH5506-001…004, RH5508-001…004, RH5510-001…004, RH5512-001…004, RH5514-001…005, RH5516-001…004, RH5518-001…004, RH5520-001…004, RH5522-001…004, RH5524-001…004, RH5526-001…004, RH5528-001…004, RH5529-001…004, and RH5531-001…006.

Facts for tagging:

- None of the existing list-shape tests carries a `[Requirement]` tag yet.
- Each split-rule test class's `GetFixAllScenario` sets `NumberOfFixAllIterations = 2` (see Q3).
- The shared requirements are covered through tests of the rules that use them, as for SHARED-ATTR-001 to SHARED-ATTR-012.

## Open questions to file

### Q1 — The split fix's layout depends on where the list is

File in: `SDD open questions: sdd/specs/shared/attribute-targets.md`. thoenissen/Reihitsu#1030 is that issue and is closed; reopen it or open a new one, as the lead decides.

**Touches:** SHARED-ATTR-017, which now covers only a list that starts its line, is followed on its line by nothing but a comment, and whose line and the next line are indented with spaces at the depth of the innermost enclosing brace. Also the fix requirements RH55xx-003 of the split rules, RH5514-004, and RH5531-005.

**Input and real output** (each run through `scripts/apply-fix.sh`; every fixture also declares `class A : System.Attribute { } class B : System.Attribute { } class D2 : System.Attribute { }`):

- RH5506: `[A, B] class C` becomes `[A]` / `[B]` / `class C`. The declaration is moved onto its own line.
- RH5514, a local function in a block (`case 1:` section, or a lambda block in an object initializer): `[A, B] void L()` becomes `[A]` / `[B] void L()`. The declaration is not moved.
- RH5514: `[A] [B, D2] void M()` becomes `[A]` / `[B]` / `[D2]` / `void M()`. The list in front of the reported one is broken off as well. But `[A, B] [D2] void N()` becomes `[A]` / `[B]` / `[D2] void N()`. `[return: A] [A, B] int R() => 0;` becomes `[return: A]` / `[A]` / `[B]` / `int R() => 0;`, so the `return:` list, which belongs to RH5528/RH5527, is moved too.
- RH5514: `System.Action a = [A, B] () => { };` inside a method becomes `System.Action a = [A]` / `        [B] () => { };`, with `[B]` at the statement's indentation.
- RH5528: `int Q { [return: A, B] get => 1; }` becomes `int Q {` / `        [return: A]` / `        [return: B]` / `        get => 1; }`. RH5518 does the same with `[field: A, B]`.
- RH5506: `class O { [A, B] class C { } }` (inside a namespace) becomes `class O {` / `        [A]` / `        [B]` / `        class C { } }`.
- RH5514: `/* c */ [A, B]` / `void N()` becomes `/* c */` / `[A]` / `[B]` / `void N()`. The comment is separated from the list.
- RH5514: a list and its method both indented 8 spaces inside a class (one level too deep) become `    [A]` / `    [B]` / `    void M()`, while the method's braces stay at 8. With tab indentation (`\t[A, B]` / `\tvoid M()`), both lines are rewritten with 4 spaces, and the braces keep their tabs.

**Why it looks wrong:** what the fix changes besides the separators depends on the context, not on the rule. A class-level declaration is moved onto its own line, but a local function is not. A neighbouring list of another rule is moved in one case and not in another. A lambda expression is broken across lines. Whitespace on lines the diagnostic is not about is rewritten. The Why of SHARED-ATTR-008 and SHARED-ATTR-010 is "the fix makes only the change the diagnostic asks for".

**Decision needed:**

- Reading A: the fix changes only the reported list. Each attribute after the first starts a new line at the indentation of the reported list's line, and nothing around the list moves. `[A, B] class C` becomes `[A]` / `[B] class C`, and the placement rule (RH5505) then reports the last list.
- Reading B: the fix lays out the reported list and what follows it the way the formatter does (FMT-BREAK-027): every list and the declaration on their own lines at the canonical indentation. `[A, B] void L()` then becomes `[A]` / `[B]` / `void L()` as well.
- Reading C: the current context-dependent output is intended.
- Recommendation: Reading A. It matches the stated scope of the placement fixes, and the placement rules already report what is left.

### Q2 — The split fix writes LF line breaks into a CRLF document

File in: `SDD open questions: sdd/specs/shared/attribute-targets.md` (same issue as Q1).

**Touches:** SHARED-ATTR-017 (its line-ending sentence holds only inside its scope) and FMT-PIPE-007.

**Input:** the inputs of Q1 that move code onto new lines, saved with CRLF line endings. Examples: `[A, B] class C` (RH5506), `System.Action a = [A, B] () => { };` (RH5514), `/* c */ [A, B]` (RH5514), the over-indented and tab-indented methods (RH5514), `int S { [field: A, B] get => 1; }` (RH5518), and `int Q { [return: A, B] get => 1; }` (RH5528).

**Real output:** `scripts/apply-fix.sh` reports `line-ending-drift`. For example, RH5506 `basic.cs` ends with `CRLF=14, LF=3`: the line breaks the fix adds outside SHARED-ATTR-017's scope are LF. Inside the scope (list already on its own line at its depth) no drift occurs. The CRLF test `VerifyInsertedLineBreakUsesDetectedCarriageReturnLineFeedEndOfLine` in the RH5506 test class covers only that case.

**Why it looks wrong:** the fix leaves mixed line endings in the document. SHARED-ATTR-008 and the existing CRLF test state the intent that inserted line breaks use the document's line ending.

**Decision needed:**

- Reading A: every line break the split fix inserts uses the line ending detected as FMT-PIPE-007 specifies, in every layout (bug fix, then a clarification of SHARED-ATTR-017).
- Reading B: only the line breaks between the new lists must; others are not specified.
- Recommendation: Reading A.

### Q3 — Fix All of a split rule needs two passes for two lists on one declaration

File in: `SDD open questions: sdd/specs/shared/code-fixes.md`. thoenissen/Reihitsu#1066 is that issue and is closed; reopen it or open a new one.

**Touches:** SHARED-FIX-002 ("correct every reported diagnostic … in one Fix All application"). The rules affected are RH5502, RH5504, RH5506, RH5508, RH5510, RH5512, RH5514, RH5516, RH5518, RH5520, RH5522, RH5526, RH5528, and RH5531 in split mode.

**Input** (the `GetFixAllScenario` of `RH5506ClassAttributeListsMustFollowShapeRulesAnalyzerTests`):

```csharp
[First, Second] [Third, Fourth]
internal class Example { }
```

**Real output:** the first Fix All pass corrects one of the two lists. A second pass is needed for the other. Each of the 14 split-rule test classes passes only because its scenario sets `NumberOfFixAllIterations = 2`, and its comment says the batch fixer discards one of the two conflicting changes. The merge rules RH5524 and RH5529 converge in one pass. Several test classes outside this batch also set `NumberOfFixAllIterations` (for example RH5030, RH5103, RH5107, RH6003, RH6014, RH7207); whether they are the same case was not checked.

**Why it looks wrong:** SHARED-FIX-002 requires one application, and `FixAllCorrectsEveryDiagnostic`, which carries SHARED-FIX-002, passes here only with two.

**Note:** this violates an existing requirement rather than raising a new reading. It matches no decided principle (P7 covers a single fix, not Fix All). Proposed: a bug against SHARED-FIX-002, possibly one cross-cutting issue for every test class that needs `NumberOfFixAllIterations` greater than 1.

## Instances of decided issues (left out of the specs)

### Instance of thoenissen/Reihitsu#1093: the specifier, not the attached declaration, decides the list shape

Left out of RH5524-001, RH5529-001, and RH5514-001. These specs cover only lists attached to a parameter or type parameter, and do not cover `typevar:` on a method.

- `[param: A] [param: B]` / `void N(int x)`: RH5524 reports `[param: B]`, and its fix gives `[param: A, B] void N(int x)`. It merges the lists and also joins the method onto the list's line.
- `int P { get; [param: A] [param: B] set; }`: RH5524 merges them into `[param: A, B] set;`. In a multi-line accessor list, `[param: A] [param: B]` / `set;` becomes `[param: A, B] set;`.
- `[typevar: A] [typevar: B]` / `void N<T>()`: RH5529 reports the second list, and its fix gives `[typevar: A, B] void N<T>()`.
- `[typevar: A, B]` / `void O<T>()` is reported by no list-shape rule, while `[A, B]` in the same place is reported by RH5514.

### Instance of thoenissen/Reihitsu#1093: the same accessor layout gets a different verdict by target

Left out of RH5531-001 and RH5531-002. These specs cover only a multi-line owner and an accessor that SHARED-ATTR-006 exempts.

- On a one-line property or indexer whose accessor has a body or expression body, RH5531 requires merged lists. `int Q { [A] [B] get => 1; }` is reported and merged, and `int T { [A, B] get => 1; }` is not reported. At the same place, RH5528 reports and splits `int Q { [return: A, B] get => 1; }`, and RH5518 reports and splits `int S { [field: A, B] get => 1; }`.
- The same applies to a one-line accessor list that contains a comment. `int S { [A] [B] get; /* c */ set; }` is reported by RH5531 and merged to `int S { [A, B] get; /* c */ set; }`, although SHARED-ATTR-006 does not exempt it.
- The same applies to a one-line event with accessor bodies. `event System.Action E { [A] [B] add { } remove { } }` is reported by RH5531 and merged.
- Possibly related, for the lead to judge: on an inline auto-property, `int P { [A] [B] get; set; }` is reported by RH5531. `int P { [return: A] [return: B] get; set; }` is reported by no rule.

No instance of thoenissen/Reihitsu#1094 and no case matching P1, P2, P3, P5, P6, P7, or P8 was found. The merge rules never merge a `type:` list on a non-type declaration (SHARED-ATTR-015), which agrees with P1: merging it would start applying an attribute the compiler ignores today.

## Non-goals written

- Each rule spec names where its lists are placed (the RH55odd placement rule) and the neighbouring list-shape rule as non-goals.
- RH5531 names `return:` accessor attributes (RH5528) and owner attributes (RH5516, RH5520).
- RH5524 and RH5529 point at each other.
