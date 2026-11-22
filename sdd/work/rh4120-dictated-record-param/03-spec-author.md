Role: spec author
Commit: d847534
Verdict: spec written

Follow-up after the maintainer's answers 1A and 2B (`00-lead.md`, *Maintainer answers* item 2) and lead decision 3. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond those recorded answers. Change doc `sdd/changes/2026-11/record-parameter-dictated-name.md` is now `Status: Accepted`.

## Changed requirements

- SHARED-CASING-014 — A name that another compilation or a native entry point dictates is not reported — changed (2B): the record-parameter bullet now exempts a positional parameter "whose member is dictated: the property the compiler declares for the parameter, or, when the compiler declares none because the record declares or inherits a member of the parameter's name, that member". The boundary sentence now says a parameter whose member is not dictated is reported, also when that member is declared in another compilation (option C not chosen). Why extended; the example gains `MetaPerson` in the referenced assembly, `Derived` and `Annotated` (not reported), and `Inherited : MetaPerson(name)` (reported).
- RH4120-001 — Record primary constructor parameter names are in PascalCase — clarified (decision 3): the coverage statement names every positional parameter, whether or not the compiler declares a property for it; one Why sentence; a new example `B`/`D`/`P` (parameters matching an inherited or a declared, non-dictated member, reported) gives both sides of "the compiler declares a property" (FC-002).

RH4120-003 unchanged: its fix for a parameter that matches a member stays withheld by SHARED-CASING-007's match-change bullet, which already states it. No new requirement ID; no rule page change (an exception, not the normal case); no resource text.

Change doc: *What*, *Why*, and the unchanged list rewritten to "the member the parameter stands for"; *Visible output changes* gains case 2 (`Person`/`Derived`/`Annotated`/`Inherited`, before and after); *Decisions* gains "An override counts like an implementation" (1A), "A parameter that matches a dictated member is not reported" (2B, replacing "Only the property the compiler declares decides"), and "RH4120-001 covers every positional parameter" (decision 3); *Alternatives* lists 1B, 2A, and 2C as rejected by the maintainer, replacing the earlier "also exempt an inherited dictated member" entry.

## Examples for the tests

- SHARED-CASING-014, example "positional parameters of records, for RH4114 and RH4120" (referenced assembly with `INamed`, `MetaRecord`, `MetaPerson`): not reported `Person.name`, `Point.name`, `Item.title`, `Derived.name`, `Annotated.name`; reported `Local.code`, `Holder.name`, `Sub.other`, `Inherited.name`. The skeptic's table (02, cases 1–16) holds the further axes; cases 13 and 14 are now "not reported", case 15 stays "reported".
- RH4120-001, new example `B`/`D`/`P`: all three reported (today's behavior, already pinned by `NoFixIsOfferedForEitherOfTwoMatchingPositionalParametersOfABaseAndADerivedRecord` and `NoFixIsOfferedWhenTheRenameWouldSplitTheParameterFromItsDeclaredProperty`, which the test implementer may also tag RH4120-001).

How established: the whole SHARED-CASING-014 example plus the RH4120-001 example compiled in a scratch two-project build (`Meta` + `App`, .NET 11); reflection confirmed the compiler declares no property for `Derived`, `Inherited`, `D` and declares the body's `name`/`total` for `Annotated`/`P`. `scripts/apply-fix.sh RH4120` on the RH4120-001 example: no-fix-offered under LF and CRLF (reported, no fix). `scripts/test.sh --project architecture`: 198 passed. Scratch deleted.

## Output questions

_None._

## Internal choices

- 2B is stated by widening the existing SHARED-CASING-014 bullet ("the member the parameter stands for") rather than adding a second bullet; alternative: a separate bullet for the declared-or-inherited case.
- "Merely declared in another compilation" (2C rejected) is expressed through the existing definition of *dictated* — a member that overrides or implements nothing outside its own compilation is not dictated — with `Inherited : MetaPerson(name)` as the example, not as a new rule.
- RH4120-003 left unchanged; SHARED-CASING-007 already withholds the fix when a rename changes which parameters match a member.

## Follow-up candidates

_None._
