Role: spec author
Commit: de916cf
Verdict: #1283: no spec change; #1284: spec written (clarification); #1285: change doc written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The handover states none.

`SpecConsistencyTests` and `SpecStructureTests` pass on the spec diff (`scripts/test.sh --project architecture`, 26 passed). `RequirementCoverageTests` will fail until RH3001-008 has a covering test.

## thoenissen/Reihitsu#1283

### Changed requirements

None. SHARED-FIX-008 already requires the fix to write the detected line ending into the code it changes. SHARED-FIX-006 forbids changing code outside the reported operand, so no line break outside the operand may change. RH3006-003 requires the trivia in front of the operand to stay outside the parentheses. The issue's input falls under these unchanged requirements, so the spec has no gap.

### Examples for the tests

- SHARED-FIX-008 and RH3006-003: the issue's input `return a ||` / `/* x */ b && c;` in a document whose line breaks are all CRLF. Expected: `return a ||` / `/* x */ (b && c);`, with every line break CRLF. Tag both IDs.
- The issue also asks for "a CRLF check in the fix test base". That is a test-infrastructure goal of an `agent-filed` issue, so the lead and the test implementer decide it.

### Output questions

_None._ The expected output is demanded by unchanged SHARED-FIX-008.

### Internal choices

- No RH3006 example was added for the line break in front of a commented operand. SHARED-FIX-008 covers it. The alternative was a CRLF note on RH3006-003, which would restate a shared rule.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1284

### Changed requirements

- SHARED-BODY-003 — *The fix converts only the body, as the formatter does* — clarified. It now says:
  - The block body is the node the fix replaces, and it is laid out as SHARED-FIX-007 states.
  - The rest of the member stays as written, as SHARED-FIX-006 requires: its documentation comment, attributes, modifiers, and signature, with the whitespace and comments between their tokens.
  - The whitespace between the signature's last token and `=>` goes with the arrow, so no whitespace is left at the end of the signature's line. A comment there follows FMT-STRUCT-006.
  - The line breaks and blank lines around the member stay with SHARED-FIX-006, SHARED-FIX-007, and the rule's own spec, such as RH3202-002 and RH3208-002.

  It gains an RH3202 example taken from the issue comment. `Uses` gains SHARED-FIX-006 and SHARED-FIX-007.
- RH3202 introduction — the sentence "converts it to a block body by formatting the method with the formatter" contradicted SHARED-FIX-006 *Formatting*. It now reads "replaces its expression body with the block body the formatter produces; the rest of the method stays as written (SHARED-BODY-003)". No requirement ID changes.
- SHARED-FIX-003, the RH3202 layout example — left unchanged. It already says the two blank lines "are outside the node the fix replaces (SHARED-FIX-006)", which is the wording the issue's "Done when" asks for.
- SHARED-FIX-006 already has the issue's first instance as its RH3203 example (*formatting only the replaced node*). It is unchanged.

None of this changes intended behavior. Unchanged SHARED-FIX-006 already demands each instance, so no change doc is needed. This matches the triage comment on the issue.

### Examples for the tests

- SHARED-FIX-006 (and SHARED-BODY-003): the RH3203 example under SHARED-FIX-006 *Formatting only the replaced node*, with `///<summary>x</summary>` and `[Obsolete] public  C( int value ) => ...`.
- SHARED-BODY-003: the new RH3202 example `public int  GetValue( ) => _value;`. The signature keeps `int  GetValue( )`, and no whitespace is left behind `)`.
- RH3202-002 / SHARED-FIX-003: the layout example with two blank lines above `GetValue`. The two blank lines stay.
- The issue names RH3202, RH3203, and RH3205–RH3208. SHARED-BODY-003 applies to all of them, so the test implementer decides how many rules get their own signature-spacing case.

### Output questions

_None._ Every expected output is demanded by unchanged SHARED-FIX-006.

### Internal choices

- The triage comment on #1284 says "the rule's coverage sentence is widened as a clarification". I read this as the scope sentence of SHARED-BODY-003, which now names what stays as written, together with the RH3202 introduction. The alternative was a new requirement in each of RH3202, RH3203, and RH3205–RH3208, which would restate SHARED-FIX-006.
- The SHARED-BODY-003 heading was reworded, and its ID was kept.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1285

### Lane

**The run's lane has to widen to behavior change.** RH3001-004 limited the fix to "a negation that covers less text than the syntax node directly around it". That wording put the accident into the spec, and the code matches it. Offering the fix for whole arguments and elements changes the condition of an existing requirement.

RH3001-004 also said only where the fix adds parentheses. Forbidding parentheses elsewhere is a new MUST NOT.

Under `sdd/README.md`, *When a change doc is required*, a requirement that changes with new behavior needs a change doc, so this is not a clarification. A change doc under `sdd/changes/` with shipped source puts the diff in the behavior-change lane.

Every visible output change it lists is already the maintainer's answer, recorded on thoenissen/Reihitsu#1245:

- round 7a: Q2, guard scope plus P23;
- round 7b: Q3, option A.

So the lead can set the doc to `Accepted` without asking anything.

### Changed requirements

- RH3001-004 — *The fix compares the operand with `false`* — changed:
  - The "covers less text than the node around it" condition is removed. The fix applies wherever the negation stands, including a whole argument of an invocation, object creation, element access, constructor initializer, or tuple, a whole attribute argument, and a whole collection-expression element. It is withheld only by RH3001-006, -007, or -008.
  - The `>`/`>>`/`>>>` qualifier is unchanged.
  - New: the fix MUST NOT add parentheses where the comparison binds as the negation did.
  - Two examples were added: whole-node positions, and no parentheses in `??=`, `orderby … descending`, and `select`. Prose names `>>>=`, named attribute arguments (`=` and `:`), `goto case`, and `group`.
- RH3001-008 — *A negation that is a constant pattern gets no fix* — added. The negation is the whole expression of a constant pattern: an `is` pattern, a switch-expression arm pattern, or a property subpattern. It is reported, and no fix is offered. The other side of the qualifier is the expression behind an arm's `=>`, which gets the fix. The ID is added to the ledger.
- Change doc: `sdd/changes/2026-11/rh3001-fix-scope.md` (`Status: Draft`).
- Rule page `documentation/rules/RH3001.md` is unchanged, because the normal case (`!x` → `x == false`) does not change.

### Examples for the tests

- RH3001-004: the second example (whole-node positions: `M(!x)`, `(!x, y)`, `new C(!y)`, `this[!y]`, `[A(!true)]`, `[!x, y]`). The third example (`_b ??= !x`, `orderby !w descending`, `select !w`) and the prose positions: `B >>>= !x` with a user-defined `operator >>>(T, bool)`, `[A(P = !true)]`, `[A(b: !true)]`, `goto case !true;`, `group !v by v`, and `group v by !v`.
- RH3001-008: `x is !true` and the switch-arm pattern `!true => 1` are reported without a fix. `true => !y` behind an arm's `=>` gets the fix.
- Regression check: `M(x: !y)` and `case !true:` stay fixed. `case !true:` becomes `case true == false:`, which is today's output.

### Output questions

_None._ The maintainer has answered every output listed under *Visible output changes* in the change doc (thoenissen/Reihitsu#1245, rounds 7a and 7b).

### Internal choices

- RH3001-008 is keyed on the constant-pattern syntax node, so a switch statement's `case !true:` label (a `CaseSwitchLabel`) keeps its fix. That is today's output, and the maintainer saw it as "fixed, for contrast" when deciding Q2. The alternative was to also withhold it. That would change visible output and would be an output question, which I recommend against. The change doc records this under *Decisions*.
- RH3001-004's `>`/`>>`/`>>>` qualifier and RH3001-007 are left unchanged.
- One change doc covers only #1285. #1283 and #1284 need none.

### Follow-up candidates

_None._

## Whole run

- Files changed:
  - `/home/user/Reihitsu/sdd/specs/analyzer/RH3001.md`
  - `/home/user/Reihitsu/sdd/specs/analyzer/RH3202.md`
  - `/home/user/Reihitsu/sdd/specs/shared/expression-body-conversion.md`
  - `/home/user/Reihitsu/sdd/specs/requirement-ids.txt`
  - `/home/user/Reihitsu/sdd/changes/2026-11/rh3001-fix-scope.md`
- Lane: the run claimed bug fix. Because of #1285's change doc it has to widen to behavior change; see the #1285 *Lane* section. That decision belongs to the lead.
