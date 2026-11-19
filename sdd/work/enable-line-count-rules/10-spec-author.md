Role: spec author
Commit: fadbd15a
Verdict: text written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Scope: decision 11 and a check of the stage-03 documents against the code at fadbd15a.

## Files changed

- `AGENTS.md` (*Key conventions*, decision 11) — `DiagnosticAnalyzerBase<TAnalyzer>` → `DiagnosticAnalyzerBase` (the class is not generic), and the help link is described as what the base builds: "from the rule ID as the link to `documentation/rules/RH####.md`" (the URI is the GitHub URL of that page). No other `DiagnosticAnalyzerBase<` reference exists in `*.md` or `*.cs` outside `sdd/work/`.
- `sdd/changes/2026-11/enable-line-count-rules.md` (Accepted; corrections to match the code, no decision changed):
  - *What*: the statement "The build passes [the root `reihitsu.json`] to the analyzers of every project" was wrong after decision 13; it now says the self-hosting scan reads it and the build does not pass it yet, and why. The `IsTestProject` bullet names the mechanism of decision 2 (the build imports the package's `buildTransitive/Reihitsu.Analyzer.targets`). The restructuring bullet states decision 20 (test classes split by topic, not into partial parts).
  - *Decisions*, 3C: the published properties are named (`Title`, `MessageFormat`), the unit test also checks that they are the resources the ID names (as `AnalyzerResourcesTests.EveryAnalyzerPublishesTheResourcesItsDiagnosticIdNames` does), and tests read the message through their test base's `MessageFormat`.
  - *Decisions*, test-project recognition: one file states the property list (decision 2); how the self-hosting scan recognizes a test project (decision 3).
  - New decision *The build enforces the rules once `binaries/` is rebuilt* (decisions 4, 13): `binaries/Reihitsu.Analyzer.dll` contains neither RH2008/RH2111 nor the `design` section (checked: no `RH2008`/`RH2111` string in it), so the scan is the check until the rebuild; passing the file then is the follow-up the lead holds.
  - *Alternatives*: passing the root `reihitsu.json` to the build now; partial parts for test classes (decision 20); a note on the "rules only in the scan" alternative.
- `.claude/commands/add-resource-texts.md` — checked against `DiagnosticAnalyzerBase` (`Title`, `MessageFormat`, resource names derived from the ID, `AnalyzerResources` holds only `ResourceManager`) and the tests: it matched. Two precisions: the checking test is named (`AnalyzerResourcesTests`, which also checks the texts are the resources the ID names), and analyzer tests reach the property through `AnalyzerTestsBase<TAnalyzer>.MessageFormat`.

No spec, requirement ID, or rule page changed. `scripts/test.sh --project architecture`: 198 passed.

## Changed requirements

_None._

## Examples for the tests

_None._

## Output questions

_None._

## Internal choices

- The `AGENTS.md` sentence describes the help link as built "from the rule ID as the link to" the page rather than "as" the relative path, because the code writes the full GitHub URL. Alternative: change only the type name — rejected, the sentence would still describe the URI inexactly.
- The change doc stays `Accepted`: the corrections record decisions 2, 3, 13, and 20 the lead already took and change no visible output.

## Follow-up candidates

_None_ beyond the one the lead holds (pass the root `reihitsu.json` to the build once `binaries/` is rebuilt); the change doc now names it.
