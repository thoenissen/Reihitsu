Role: spec author
Commit: 43d59fa
Verdict: #353: spec written; #354: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the maintainer's recorded answers. This round amends the specs and the change doc to batch 3 (12D, 13A, 14A, 15A, 16A) and to lead decisions 4 and 5, and sets the change doc to `Status: Accepted` as the lead asked (sdd/README.md has the lead set it in a squad run; done here on the lead's instruction).

Files changed:

- `sdd/changes/2026-11/line-count-rules.md` — `Status: Accepted`; *What* and visible change 5 name both properties; *Decisions* gains one section per batch-3 answer (12D, 13A, 14A, 15A, 16A), the "Pending batch 3" notes are gone, and the wrong rationale of option A ("which `Microsoft.NET.Test.Sdk` sets and `dotnet test` reads"; "a list of frameworks misses one" as the only reason) is replaced: `IsTestProject` alone is now a rejected alternative (12A) with the Microsoft.Testing.Platform reason; 12B, 12C, 13B, 14B/C, 15B, 16B are listed as rejected alternatives.
- `sdd/specs/shared/line-count.md` — SHARED-LINES-002 changed (decision 4: a default applies "whenever the setting has no valid value", the cases as examples, the unreadable file added; empty file and `{ } junk` added to the example); SHARED-LINES-003 changed (12D: either property `true` is test code; the package declares both; corrected `Why:`; example covers both properties, one `true` with the other `false`, and the helper library).
- `sdd/specs/analyzer/RH2008.md`, `sdd/specs/analyzer/RH2111.md` — RH2008-001 and RH2111-001 clarified: examples at the defaults in both kinds of code (14A), a configured production setting leaving the test default (15A), an invalid setting keeping the default (16A).
- `sdd/specs/analyzer/RH0001.md` — RH0001-007 `Why:` states that a reported setting keeps its default (16A); `Uses` gains SHARED-LINES-002.
- `documentation/rules/RH2008.md`, `documentation/rules/RH2111.md` — the test-project sentence is the one drafted with 12D; the configuration sentence says each setting stands on its own and that a setting left out or reported by RH0001 keeps its default (15A, 16A).

## thoenissen/Reihitsu#353

### Changed requirements

- SHARED-LINES-002 — A declaration is reported when its count is greater than its limit — changed (wording, decision 4; 16A)
- SHARED-LINES-003 — Test code is the code of a test project — changed (12D: `IsTestProject` or `IsTestingPlatformApplication`)
- RH2008-001 — The limits are `design.maximumMethodLines` and `design.maximumTestMethodLines` — clarified (default and independence examples; 14A, 15A, 16A)
- RH0001-007 — The `design` section holds the line limits — clarified (16A in `Why:`; 13A unchanged)

### Examples for the tests

SHARED-LINES-002 (every configuration listed in its example, empty file and `{ } junk` included), SHARED-LINES-003 (every project shape in its example), RH2008-001 (60/61 and 100/101 without configuration; production setting alone in a test project; `"40"`), RH0001-007. Earlier lists of 01- and 03-spec-author.md stand for the rest.

What the later roles need for the 12D mechanism:

- **Package.** `src/Reihitsu.Analyzer.Package/buildTransitive/Reihitsu.Analyzer.targets` gains, beside the existing `RazorComponent` item group:
  ```xml
  <ItemGroup>
    <CompilerVisibleProperty Include="IsTestProject" />
    <CompilerVisibleProperty Include="IsTestingPlatformApplication" />
  </ItemGroup>
  ```
  The csproj already packs this file to `buildTransitive` (`Reihitsu.Analyzer.Package.csproj:31`), which NuGet imports for direct and transitive references. No spec test covers the file; the auditor checks it by hand. The skeptic verified on SDK 11.0.100 that these items put both keys into the generated `.GeneratedMSBuildEditorConfig.editorconfig` (04-skeptic.md table).
- **Analyzer.** Read both keys from `AnalyzerConfigOptionsProvider.GlobalOptions` (`TryGetValue("build_property.IsTestProject", …)`, same for `IsTestingPlatformApplication`); test code when either value equals `true` case-insensitively (trimmed or not is not specified — a value MSBuild writes carries no whitespace; compare with `string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)`). Lead decision 3: the limit resolution belongs with the one shared counter.
- **Verifier tests, both directions (maintainer's condition on 12D).** Pass the property as a global config: `test.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", "is_global = true\nbuild_property.IsTestingPlatformApplication = true\n"))` — the pattern of `RH7101DoNotCombineFieldsAnalyzerTests.cs:1776`. With `{ "design": { "maximumMethodLines": 2, "maximumTestMethodLines": 3 } }` and the three-line `M` of SHARED-LINES-003, for RH2008 and the analogous type case for RH2111:
  - `IsTestProject = true` → not reported; `IsTestingPlatformApplication = true` → not reported; `IsTestProject = false` with `IsTestingPlatformApplication = true` and the reverse → not reported; `True`/`TRUE` → not reported.
  - No global config, both `false`, empty values, or another value such as `yes` → reported at limit 2.
  - The pair "same code, key present → not reported / key absent → reported" is what proves the key reaches `GlobalOptions`; each property needs its own such pair. No Reihitsu analyzer reads `GlobalOptions` today, so this is new plumbing.
- **Defaults** (14A): FC-003 pairs at 60/61, 100/101, 500/501, 1000/1001, generated in the test. **Independence** (15A): production setting in a test project, test setting in a production project. **Invalid** (16A): `"40"`, `0`, `50.0`, `5e1`, `null`, empty file, `{ } junk`, each with a 61-line production body (reported) and a 60-line one (not reported).
- `RuleDocumentationExampleTests` still needs both rules opted out (03-spec-author.md).

### Output questions

_None._ Batch 3 is answered; nothing open.

### Internal choices

- Lead decision 5 (an extension library of Microsoft.Testing.Platform counts as test code) is stated once, in the change doc's 12D decision; the spec's normative sentence keys on the property and implies it without naming it, and the rule pages leave it out. Alternative: a sentence in SHARED-LINES-003, which the skeptic advised against.
- SHARED-LINES-003's `Why:` states that an analyzer loaded without the package's build files sees neither property and judges production code; that is the reason this repository needs its own `CompilerVisibleProperty` once the rules are turned on here (follow-up already in `00-lead.md`).
- The unreadable file is named in SHARED-LINES-002 without a requirement reference: RH0001-003 covers only the empty and the malformed file.
- The rule pages say a setting "that RH0001 reports as invalid" keeps its default, so a user who sees RH0001 knows the rule still runs. Alternative: leave the page at "a setting you leave out", since an invalid setting is a corner case — kept because 16A was an output question the maintainer decided.
- RH0001's `Uses` row gains SHARED-LINES-002, which RH0001-007 now references.

### Follow-up candidates

_None_ new.

## thoenissen/Reihitsu#354

### Changed requirements

- RH2111-001 — The limits are `design.maximumTypeLines` and `design.maximumTestTypeLines` — clarified (500/501, 1000/1001; production setting alone in a test project; `0` keeps the default)
- SHARED-LINES-002, SHARED-LINES-003, RH0001-007 — see #353

### Examples for the tests

RH2111-001's new example, and the SHARED-LINES-003 cases written as a type over `maximumTypeLines`/`maximumTestTypeLines`.

### Output questions

_None._

### Internal choices

As under #353.

### Follow-up candidates

_None._

## Whole run

- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"`: 45 passed, 3 failed — the three expected until the analyzers exist (`AnalyzerSpecHeaderMatchesItsRule`, `EveryCodeOwnerNamesAnExistingType`, `EveryRequirementOfAnEnforcedSpecIsCovered`), unchanged from 03-spec-author.md.
- No new C# example block was added; the SHARED-LINES-003 example code is unchanged. The added examples are prose with generated bodies.
- 11A instructions for rulesets and `Reihitsu.SelfHosting.Test` in 03-spec-author.md *Whole run* stand unchanged.
- Code owner of `shared/line-count.md` remains `—` until the shared counter type exists (lead decision 3; the implementer fills it in the header and in `sdd/specs/README.md`).
