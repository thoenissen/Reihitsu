Role: spec author
Commit: a37fab68
Verdict: spec written

The prompt named decisions 46–48 and audit 6 (#17–#19) as the input. It carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions.

Measured with `scripts/apply-fix.sh` on temporary worktrees of `origin/main` (`3578f98e`) and of the head (`a37fab68`), LF and CRLF; fixtures and my worktrees lived in the scratchpad under `sa49/`. On `main`, all of these are fixed:

- RH7309, partial struct, defining declaration `{ get; set; }` in the `IComparable` block, implementing `get => 0; set { }` in the other part;
- RH7309, the same with implementing `get => 0; set;` in the block (layout reorder in every build);
- RH7309, the same with implementing `get => 0; set { }` in the block (stays fixed);
- RH7109, `#if DEBUG [System.Serializable] #endif` on a class, moving `_c` in front of `_a`;
- RH7103, the same attribute, and `#if NET , System.IAsyncDisposable #endif` in a base list, moving a static method;
- RH2001, a conditional group of `new`, `static`, `unsafe` (get-only: `private` / `readonly` / group / `int _value;`, `line-ending-drift` under CRLF), and `static` with `{ get; set; }` (group unchanged, `int _value;`).

Head: an `#if` block holding a whole member on the lines above `static private int Value { get; }` is fixed to `private static readonly int _value;` with the block in place (the new RH2001-009 counterpart example).

## Changed requirements

- **SHARED-ORDER-007 — Initializer execution order is kept — changed (decision 46).** The backing-field paragraph now counts by accessor shape only: a property that is not abstract or extern counts as having a backing field unless every accessor has a body or expression body (or it is expression-bodied) and none of its tokens is `field`. No partial exception: the defining declaration counts, an implementing declaration with a bodiless accessor or `field` counts, an implementing `get => 0; set { }` does not. New Why sentence (over-counting can only refuse layout reorders). New RH7309 partial example (defining declaration in the block: no fix), with both counterparts in prose (implementing `get => 0; set;`: no fix; implementing `get => 0; set { }`: fix). The layout paragraph names a type with a conditional directive in its declaration (pointer to SHARED-ORDER-008).
- **SHARED-ORDER-008 — Only conditional groups of whole units keep a member move — changed (decision 47).** The type-declaration paragraph now says such a type is treated as one whose field order sets its layout: only moves that reorder its instance fields are refused; every other move stays. The whole-type-in-front exception now reads "does not make the type count as a layout type". Why updated. Examples: conditional `[StructLayout]` (RH7109, no fix) kept; conditional `[System.Serializable]` re-pointed to RH7109 moving a field (no fix); new example RH7103 with the same attribute moving a static method (fix, `main`'s output), with the base-list case in prose; the whole-type-in-front example re-pointed to RH7109 moving `_c` (fix, output), matching the existing RH7109 test.
- **RH2001-009 — retitled and changed (decision 48).** Title: "…or for a conditional directive between the modifiers". Decision 36's placement paragraph and decision 40's keyword-set paragraph are removed. New MUST NOT: any `#if`/`#elif`/`#else`/`#endif` in the leading trivia of a modifier other than the first, or of the type when a modifier precedes it, withholds the fix, whatever the branches hold, also when the order is kept and when no `readonly` is added. "A comment or directive between two modifiers that keep their order" now excludes conditional directives. New Why. The decision-36 example and the extern/unsafe/readonly example are replaced by one example (`static` get-only, `static` with `set`, `new`, `extern`, `readonly` in a struct: all no fix) and a counterpart (an `#if` block above the property: fixed, head output). `Uses` keeps SHARED-FIX-009, which the Why still cites.
- **SHARED-FIX-009 — amendment reverted.** First bullet back to "or the names in the branch". The last paragraph no longer says RH2001 reads modifier keywords. Its pre-round-2 example ("RH2001's fix for a conditional directive between two modifiers that keep their order") is no longer true under decision 48, so it now reads "such as RH2001's fix, which reads no excluded code and is withheld for every conditional directive between a property's modifiers (RH2001-009)".
- **Change doc `member-move-conditional-groups.md`.**
  - *What*: the type-declaration and backing-field sentences now state decisions 47 and 46.
  - *Visible output changes*: the RH7103 `[Serializable]` withholding is replaced by RH7109 `[Serializable]` with a field move (Before = `main`'s output). The RH7103 method move and the base list are now listed as unchanged. New entry: the defining partial declaration (RH7309, Before = fixed). New SHARED-FIX-003 bug-fix bullet: implementing `get => 0; set;`. The *Unchanged* paragraph adds the declaration directive for moves that reorder no instance field.
  - *Decisions*: decision 39's section is rewritten as 39 + 47, and a new section covers decision 46.
  - *Alternatives*: two are added (a precise partial count, and withholding every move of a type).
- **Change doc `rh2001-modifier-reorder-trivia.md`.**
  - *Affected*: drops SHARED-FIX-009 (changed). SHARED-FIX-009 is added by the other change doc, and this one no longer amends it.
  - *What*: the decision 36 and 40 bullets are replaced by one bullet on decision 48.
  - *Visible output changes*: the decision-36 "after" output is replaced. The extern entry is kept with its `main` before, and a list covers `readonly`, `unsafe`/`static`/`new`, and `static` with `set` (all no fix), stating that the earlier stage's behind-the-group output is withdrawn.
  - *Decisions*: the sections for 36 and 40 are replaced by one for 48.
  - *Alternatives*: 36/40 are added as rejected.
  - The status stays Accepted.

No rule page changed: these are withheld fixes in corner cases. `scripts/test.sh --project architecture --filter Sdd`: 51/51 pass. That run built the working tree, which included the test implementer's uncommitted test edits.

## Examples for the tests

- SHARED-ORDER-007: the partial RH7309 example (defining declaration in the block: no fix), its counterparts implementing `get => 0; set;` (no fix) and `get => 0; set { }` (fix, `main`'s output).
- SHARED-ORDER-008: RH7109 conditional `[StructLayout]` (no fix); RH7109 conditional `[System.Serializable]` with a field move (no fix); RH7103 conditional `[System.Serializable]` with a method move (fix, output), and the conditional base list (fix, `main`'s output); RH7109 whole type in front (fix, output). The RH7103 class `get => 0; #if DEBUG set; #endif` example (no fix) is the input of audit #20.
- RH2001-009: the five-property conditional example (all no fix; the decision-36 and `new` tests become `VerifyNoCodeFixIsOffered`) and the `#if` block above the property (fix).

## Output questions

_None._ Decisions 46–48 and the standing answer cover each changed output.

## Internal choices

- **Abstract and extern keep their exemption in SHARED-ORDER-007.** Decision 46 names only "no `partial` exception". Abstract and extern properties have no backing field in any build. A conditional directive between modifiers is already refused by SHARED-ORDER-008, and the check is a modifier test, not an inference. The alternative was to drop them as well, which would over-count abstract properties of a layout class.
- **Decision 47 is stated inside SHARED-ORDER-008, with a pointer from SHARED-ORDER-007's layout paragraph.** The alternative was to move the paragraph into SHARED-ORDER-007.
- **SHARED-FIX-009's last paragraph keeps an RH2001 example, reworded.** The pre-round-2 wording is false under decision 48. The alternative was to drop the example.
- **RH2001-009 scopes the refusal to directives in the leading trivia of a modifier other than the first, or of the type behind a modifier.** A directive in front of the first token stays under RH2001-004.

## Follow-up candidates

_None._
