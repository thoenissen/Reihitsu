Role: spec author
Commit: f6ce733
Verdict: spec written

Repair round 2, finding #1 of `09-auditor.md`, decision 14 in `00-lead.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decision 14, which this handover implements.

## Changed requirements

- RH3001-009 — A negation that touches a word in front of or behind it gets no fix — added (`sdd/specs/analyzer/RH3001.md`, `sdd/specs/requirement-ids.txt`).
- RH3001-004 — The fix compares the operand with `false` — changed: its "unless" list names RH3001-009.
- Change doc `sdd/changes/2026-11/rh3001-fix-scope.md`: RH3001-009 in *Affected requirements* and *What*, one sentence in *Why*, a new block in *Visible output changes* (before/after against `main`), a *Decisions* entry (decided by the lead under Code-fix coverage), and two rejected alternatives. Status left `Accepted` — see *Internal choices*.
- Rule page `documentation/rules/RH3001.md`: not changed (a withheld fix for a rare class; `general-notes.md` covers it).

The class, as written: the fix is withheld when the token in front of `!` is a keyword or an identifier (contextual keywords included) and nothing — no trivia at all — stands between it and `!`; or when the token behind the negation's last token is one and nothing stands between them. Whitespace, a line break, or a comment between them, or a punctuation token on that side, keeps the fix.

## Examples for the tests

RH3001-009, plus its tie to RH3001-004. Outputs below were probed with `scripts/apply-fix.sh` on the head (`f6ce733`) and on `origin/main` (`cf6d26b`); "head" is today's wrong output, which the tests turn into "no code fix offered".

Withheld (reported, `VerifyNoCodeFixIsOffered`-style), one small test each:

| Input | main | head (wrong) |
|---|---|---|
| `if (b) return!a;` | `return a == false` | `return(a == false)` |
| `yield return!a;` | `yield return a == false` | `yield return(a == false)` |
| `case!true:` (switch statement, no `when`) | `case true == false:` | `case(true == false):` |
| `_ when!y => 1,` (switch-expression arm guard) | `_ when y == false => 1` | `_ when(y == false) => 1` |
| `where!w` / `select!w` (query) | `where w` + `== false` on its own line / `select (w == false)` | `where(w == false)` / `select(w == false)` |
| `where !(w)select w` (trailing side: `)` touches `select`) | `where (w) == false` + `select w` on the next line | `where ((w) == false)select w` |
| `orderby!w` | no fix | `orderby(w == false)` (auditor) |

Fixed (the other side of the qualifier, FC-002), exact outputs:

- `if (b) return !a;` → `if (b) return a == false;`
- `if (b) return/* c */!a;` → `if (b) return/* c */a == false;` (a comment between keyword and `!` keeps the fix)
- `where !(w) select w` → `where (w) == false select w;` (space on the trailing side)
- Punctuation in front: `if(!a)` / `M(!a)` keep the fix (already covered by existing RH3001 tests; one assertion is enough).

The spec's RH3001-009 example combines `where !(w)select w`, `where!w`, `select!w`, `return!a` (withheld) and `where !(w) select w`, `return !a`, `return/* c */!b` (fixed) in two compiling fixtures; the tests may split them. `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` is red until a test carries `[Requirement("RH3001-009")]` — expected; the other 50 Sdd tests pass.

## Output questions

_None._ Withholding a fix for a rare class is decided by `AGENTS.md`, *Code-fix coverage* (decision 14).

## Internal choices

- Both sides of the negation, not only the keyword in front of `!` (decision 14 names only the front). The trailing side is the same failure (`where !(w)select w` → `where ((w) == false)select w` on head, probed), and the auditor's suggested change names it. Alternative: front only, which leaves the trailing case violating RH3001-004's MUST NOT.
- "Nothing between" means no trivia at all, not "no whitespace": a comment separates the tokens, the unparenthesized text parses, and the fix is offered (probed on head and main, same output). Alternative: "no whitespace", which would withhold `return/* c */!a` for no reason.
- "Keyword or identifier, contextual keywords included" rather than a keyword list, so the guard is one cheap token-kind check and the list in the spec is illustrative.
- Change doc kept `Accepted`: the added visible change is a withheld fix for a rare class, which the rule decides without a maintainer answer. The lead sets the status; flip it if you read `sdd/README.md` as requiring re-acceptance.
- Correction to decision 14's rationale: RH6001 does not report `return!a` (it reports a statement keyword glued to `(` only, RH6001-001). The spec's *Why* therefore cites no RH6001 report; it rests on the class being rare and on RH3007 reporting the query positions. The formatter writes `return !a` but leaves `where!w`, `select!w`, `case!true:`, and `when!y` as written (probed with `scripts/format.sh`), so no rule or formatter step covers those four.

## Follow-up candidates

_None._ (On `main`, `where!w` is fixed to `where w` with `== false` on a line of its own; this run withholds that fix, so nothing remains to file.)
