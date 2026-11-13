Role: spec author
Commit: 4a5d3ea8532fd99489ef8e8c2e96a541a9fc8700
Verdict: spec written

The prompt named the change to make (decision 23 of `00-lead.md`). That is the lead's recorded decision on auditor 21's finding 8, not a hoped-for verdict, and the spec follows it.

## Changed requirements
- SHARED-ATTR-022 — The split fix is withheld behind a `{` on the list's line that is still open there — changed (`sdd/specs/shared/attribute-targets.md`). The enumeration of owner kinds (accessor list, type, braced namespace, block of a local function) and the "no `{` directly holds" sentence are replaced by one condition: a `{` token in front of the reported list's `[` on that line whose `}` does not stand in front of the list on that line. Three qualifiers are explicit conditions, each with an example on both sides (FC-002):
  - *which `{`*: every `{` token, whatever it opens; a `{` inside a literal is not a token (offered: `string s = "{"; [A, B] void N() { }`);
  - *in front of the list on its line*: a `{` on a line above (offered: accessor list, and a lambda in a multi-line block), a `{` behind the list (offered, same literal example);
  - *not closed in front of the list*: `void M() { } [A, B] void N() { }` (offered) against `class C3 { void M() { } [A, B] void N() { } }` (withheld: the closed body of `M` does not cancel the open body of `C3`).
- SHARED-ATTR-017 — clarified: its pointer to SHARED-ATTR-022 now reads "a list behind a `{` on its line that is still open there".
- No requirement ID added or retired. The 14 split-rule requirements reference SHARED-ATTR-022 by ID and need no change. `sdd/specs/README.md` is unchanged: the owner is still the split base.

Change doc `sdd/changes/2026-11/brace-fix-layout-line-endings.md` (`Status: Accepted` kept): the *What* bullet and the *Affected requirements* entry state the new condition; the visible-output section is now "a list behind a `{` on its line that is still open there" and adds the auditor's three cases with their `main` output (measured with `scripts/apply-fix.sh RH5514` on a scratch worktree of `origin/main`, removed afterwards) and the offered lambda in a multi-line block; *Decisions* records decision 23 in the split-list section; *Alternatives considered* adds the rejected one-by-one enumeration.

## Examples for the tests
SHARED-ATTR-022: the withheld block (namespace, class, field-initializer lambda `C2`, closed-then-open `C3`, local function in a block, lambda in a block on one line `O`, local function in a switch section `P`, RH5518 and RH5531 accessor lists) and the three offered examples (`{` above, including the lambda in a multi-line block; closed `{` in front; `{` in a literal). The new tests are `C2`, `C3`, `O`, `P`, the multi-line lambda, and the literal case; the rest exist from rounds 1 and 2.

## Output questions
_None._ Every newly withheld case is a rare class withheld under decision 23; the offered sides keep the SHARED-ATTR-017 output.

## Internal choices
- The literal-brace case is stated and given an offered example, so the test implementer pins that the scan reads tokens, not text. Alternative: leave it unstated.
- A `{` in an interpolation (`$"{x}"`) is a token and counts; it is closed in any input that compiles, so the spec gives it no example of its own.
- The change doc says RH5528 behaves alike for `[return: A, B]` on such a lambda. Auditor 21 traced this but did not run it, and I did not measure it either. The test implementer may add one RH5528 withheld test; if RH5528 does not report that list, the sentence is dropped.
- Auditor 21's hint (extension blocks matching `BaseTypeDeclarationSyntax`) is moot: the condition no longer names owner kinds, and `extension(int i) { [A, B] void M() { } }` is withheld by the open `{`.

## Follow-up candidates
_None._
