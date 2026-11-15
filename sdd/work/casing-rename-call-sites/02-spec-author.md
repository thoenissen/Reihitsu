Role: spec author
Commit: 6fd06fa
Verdict: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict; it named the maintainer's answer (A) and decision 4, which this handover follows. Lane bug fix: the change records a withheld class (clarification), so no change doc is written. Rule pages are unchanged: `general-notes.md` ("A diagnostic without a code fix") covers a withheld fix.

## Changed requirements

- SHARED-CASING-009 — The fix is withheld when the new name would collide — clarified. A second normative paragraph withholds the fix when the rename would change which symbol an existing reference to a symbol outside the rename cascade binds to, in any document: **rewritten** (the renamer qualifies with `this.`/`base.`/type/namespace, turns an extension call into a static call, or casts an argument; R1) or **left as written** (the reference then binds to the renamed symbol, e.g. hiding an inherited member; R2). Both sides of the qualifiers are stated: a reference to a cascade symbol is renamed and does not count; a reference to another symbol whose binding stays unchanged does not withhold the fix. The Why gains the reason, and five examples are added.
- SHARED-CASING-012 — Why only: the clause "even where the renamer qualifies it to keep its binding" implied a fix that rewrites another symbol's `nameof`; it now says such a rename is withheld by SHARED-CASING-009. No behavior change of SHARED-CASING-012.

No new requirement ID; `requirement-ids.txt` and `sdd/specs/README.md` are unchanged.

## Examples for the tests

SHARED-CASING-009, the five new examples (each run on 6fd06fa with `scripts/apply-fix.sh`, LF and CRLF):

- RH4103 `toList` extension + `values.ToList()` in the same namespace — today fixed, call rewritten to `Enumerable.ToList(values)`; must become no fix.
- Control: same with `User` in namespace `M` that does not import `N` — fixed today, call left as written; must stay fixed.
- RH4103 `write(int)` next to `Write(object)`, `box.Write(1)` — today rewritten to `box.Write((object)1)`; must become no fix.
- RH4105 parameter `Name` next to field `name` used unqualified — today rewritten to `this.name = name.Trim();`; must become no fix.
- RH4103 `Derived.process()` hiding `Base.Process()`, `derived.Process()` in `User` — today fixed with the call text unchanged (R2); must become no fix.

The skeptic's other members (01-skeptic.md, *Breaking inputs*: f07, f09, f11, f12, f13, f16–f29) and the Fix All mode remain the test implementer's to cover; the spec names them in the "The same holds" sentences without a block each.

## Output questions

_None._ The maintainer answered A.

## Internal choices

- Extend SHARED-CASING-009 (decision 4) rather than add a new requirement; alternative: a new SHARED-CASING-013.
- The withheld class is stated by its effect (a changed binding of a reference outside the cascade) with the renamer's rewrites listed as the observed forms, not as a closed list; alternative: a closed list of rewrite shapes, which would leave R2 and unlisted rewrites undecided.
- "In whichever document of the solution it stands" is written although the skeptic ran only same-document fixtures: it follows from the maintainer's answer (no casing fix changes code beyond the cascade) and from the renamer's solution-wide scope.

## Follow-up candidates

_None._
