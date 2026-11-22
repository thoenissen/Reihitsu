Role: spec author
Commit: a2e48df
Verdict: #971: spec written; #1072: spec written; #1451: spec written; #1434: spec written

Follow-up round: the maintainer answered output batch 1 with A for questions 1–12 (`00-lead.md`, *Maintainer answers*). The prompt carried no intended fix or verdict beyond that answer. Each answer is recorded in the `Decisions` of its change doc, and all three change docs are `Status: Accepted` (`sdd/README.md` gives that step to the lead; done here on the lead's instruction). Questions 1–9 were already written into the spec on their recommendation; questions 10–12 needed spec amendments, listed below. No new requirement IDs; no rule page changes.

`scripts/test.sh --project architecture`: 197 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`, for SHARED-CASING-013 and SHARED-CASING-014 only, unchanged from `01-spec-author.md` (the test implementer tags them).

## thoenissen/Reihitsu#971

### Changed requirements
- SHARED-CASING-006 — changed (question 10 A): the camelCase step lowercases the leading uppercase run of the **first word**, as the word step wrote it, before joining; the run never reaches into the second word. *Why* gains the sentence that a `_` boundary is never merged into an acronym. The RH4106 leading-acronym example gains `URLs` → `_urLs` (question 1 A, skeptic's row). New example "a leading run that ends with the first word": `A_B`, `A_B_Cd`, `m_HTTPClient`, `X_Y_Z`, `XML_Parser` → `_aB`, `_aBCd`, `_mHTTPClient`, `_xYZ`, `_xmlParser`; measured with `scripts/apply-fix.sh RH4106` on `a2e48df`, identical to today under LF and CRLF.
- Change doc `sdd/changes/2026-11/casing-fix-name-derivation.md`: What says "of the first word"; visible change 1 gains the `URLs` row and the unchanged `A_B`/`m_HTTPClient`/`X_Y_Z`/`I_O_Stream` line; the old decision "measured on the joined name" is replaced by "taken from the first word only"; answers 1–5 and 10 recorded.

### Examples for the tests
- SHARED-CASING-006: both leading-acronym examples (now with `URLs`) and the new first-word block (characterization of today's output; guards against the joined reading).

### Output questions
_None._

### Internal choices
_None._ (The earlier "joined name" choice is withdrawn; question 10 decided it.)

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1072

### Changed requirements
- None in this round. Change doc `sdd/changes/2026-11/casing-fix-keyword-names.md`: answer 6 A recorded; Accepted.

### Examples for the tests
- Unchanged from `01-spec-author.md` (SHARED-CASING-013).

### Output questions
_None._

### Internal choices
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1451

### Changed requirements
- None in this round; questions 2–5 were already written on their recommendation. Answers recorded in `sdd/changes/2026-11/casing-fix-name-derivation.md` (the doc it shares with #971).

### Examples for the tests
- Unchanged from `01-spec-author.md`.

### Output questions
_None._

### Internal choices
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1434

### Changed requirements
- SHARED-CASING-014 — changed:
  - question 11 A: a member implements an interface member only when its own type implements the interface; a member that implements one only because a derived type lists the interface is not dictated and is reported. New example (RH4103, `A`/`B : A, IMeta` with `IMeta` in a referenced assembly): `A.process` reported, fix withheld (SHARED-CASING-009) — this is what `RH4103MethodNameCasingAnalyzerTests.NoFixIsOfferedWhenADerivedTypeImplementsAReferencedAssemblyInterfaceWithTheMethod` already asserts; it keeps its assertion and should gain a `SHARED-CASING-014` tag.
  - question 12 A: *Why* states that a dictated signature's tuple names are those of the implemented member after type-argument substitution, and that a name supplied by a type argument written in this compilation is reported where it is written, once. New example (RH4116, `Pairs : IEnumerable<(string key, int value)>`): reported in the base list, not in `GetEnumerator`'s signature. Not measured (RH4116 has no fix; derived from the analyzer per the skeptic).
- Change doc `sdd/changes/2026-11/casing-names-dictated-elsewhere.md`: What gains both boundaries; visible changes 4 (tuple via type argument) and 5 (unchanged, derived-type implementer); two decisions and two rejected alternatives added; answers 7, 8, 9, 11, 12 recorded.

### Examples for the tests
- SHARED-CASING-014: the two new examples, besides those listed in `01-spec-author.md`. The existing-test list in `02-skeptic.md` (*Breaking inputs*, #1434) holds with question 7–9 answered A: the contradicted tests change their assertions.

### Output questions
_None._

### Internal choices
_None._

### Follow-up candidates
_None_ beyond the RH4120 candidate already in `00-lead.md`.
