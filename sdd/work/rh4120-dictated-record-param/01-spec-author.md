Role: spec author
Commit: b071ad0
Verdict: change doc written

Change doc: `sdd/changes/2026-11/record-parameter-dictated-name.md` (Status: Draft). Its one visible output change is the case the maintainer already answered (option A on thoenissen/Reihitsu#1939, summarized in `00-lead.md`); the doc records the answer under *Decisions*. The prompt carried no intended fix or suspected cause; the issue's pointer to `CasingAnalyzerBase.IsDictatedMember` and `RecordPositionalParameterUtilities` is an implementation proposal and was not written into the spec.

## Changed requirements

- SHARED-CASING-014 — A name that another compilation or a native entry point dictates is not reported — changed: new bullet "the name of a positional parameter of a record whose property — the one the compiler declares for the parameter — is dictated"; a boundary sentence that a positional parameter for which the compiler declares no property (the record declares or inherits a member of its name) is not exempted, whatever that member overrides or implements; one Why sentence; one new example (RH4114, RH4120).
- RH4120-001 — Record primary constructor parameter names are in PascalCase — changed: "A parameter whose name SHARED-CASING-014 exempts MUST NOT be reported."; `Uses` gains SHARED-CASING-014.

No new requirement ID; `requirement-ids.txt` unchanged. No rule page changed: the exemption is an exception, not the normal case (`documentation/rules/RH4120.md` left alone). No resource text changed.

## Examples for the tests

- SHARED-CASING-014, example "positional parameters of records, for RH4114 and RH4120" — needs a referenced assembly (or another project) declaring `INamed` and `MetaRecord`:
  - not reported: `record Person(string name) : INamed` (implicit implementation), `record struct Point(string name) : INamed`, `record Item(string title) : MetaRecord` (override of an abstract property; the compiler synthesizes the override — confirmed by a two-project build in which `MetaBase m = new Item("t"); m.title` runs and returns the parameter value);
  - still reported: `Local.code` (interface `ILocal` in source), `Holder.name` (`Holder` does not implement `INamed`; only `Sub` does), `Sub.other`, `Derived.name` (`Derived : Person(name)` inherits `name`, so no property is declared for its parameter).
- RH4120-001 — the existing examples stay; the counterpart the issue asks for (a parameter whose property implements no referenced-assembly member is still reported) is `Local.code` / `Holder.name` above.

How established: the full example compiles (`dotnet build` of a scratch app referencing a scratch library holding `INamed` and `MetaRecord`). Today's output of its in-compilation part (Person without the interface, ILocal/Local, Holder, Sub, Derived) through `scripts/apply-fix.sh RH4120`: "no-fix-offered" under LF and CRLF, i.e. diagnostics reported. The runner cannot add a referenced assembly, so the "before" of the exempted cases is read from `RH4120RecordPrimaryConstructorParameterCasingAnalyzer.GetLocations`, which yields every record positional parameter except `_` by syntax alone. `scripts/test.sh --project architecture`: 198 passed.

## Output questions

_None._ The one visible change is covered by the maintainer's answer on thoenissen/Reihitsu#1939. The extension to an override of a referenced-assembly abstract property (`Item.title`) has the same cause and is within the answer's "dictated, as for properties under SHARED-CASING-014"; the lead may still list it if it reads the answer more narrowly (it concerns only `implements`).

## Internal choices

- The case is stated as a bullet of SHARED-CASING-014 and referenced from RH4120-001, instead of a new RH4120 requirement that would restate the definition of a dictated member.
- Only the property the compiler declares decides: a parameter for which no property is declared (it inherits a dictated member of its name, `Derived.name`) stays reported — unchanged behavior, so nothing visible is in question. Alternative: exempt it too (rejected in the change doc).

## Follow-up candidates

_None._
