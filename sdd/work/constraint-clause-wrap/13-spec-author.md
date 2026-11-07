Role: spec author
Commit: 00c8512
Verdict: spec written

Repair round 1. This round carries decision 8 (`00-lead.md`) and the auditor's spec-wording hint (`12-auditor.md`, finding #1 and *Hints*) into FMT-BREAK-037 and the change doc. The prompt carried no intended fix, suspected cause, or hoped-for verdict. No requirement ID was added, so `sdd/specs/requirement-ids.txt` is unchanged. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 51/51 pass.

Evidence: `scripts/format.sh` on a worktree of `main` e3924e2 (removed afterwards) and on the head, with the fixtures of the new example. On `main`, `M`–`P` stay unchanged, and `S` puts `IDisposable` at column 4. On the head, `M`, `N`, and `P` are split (finding #1 (a), (b), (d)), and `S` leaves `new()` behind the comment. The parameter-list counterpart on the head matches the new wording on both sides. `void N(int a, /* x⏎ y */ int b, int c)` stays on one line. `void O(int a, /* x⏎ y */ int b,⏎ int c)` gets a line break behind `y */`, with `int b` and `int c` aligned.

## Changed requirements

- **FMT-BREAK-037 — changed (decision 8, auditor hint).**
  - A new paragraph defines "line break" for the whole requirement as SHARED-BLANK-008 counts it. A line break inside the text of a multi-line comment is not one. This holds for the joins, for whether the clause is wrapped, and for whether the colon, a constraint, or a comma starts a line.
  - The join step now says "a line break between the type parameter and the colon MUST be joined", where it used to say "the colon MUST join the line". The old wording asked for a join where a multi-line comment alone puts the colon on another line of the text.
  - The type-argument-list exclusion is now explicit (auditor hint). The joins cover the gap in front of `<`, but the gaps from `<` to `>` belong to FMT-BREAK-014/015. A line break those requirements keep, such as one inside a wrapped tuple type argument, stays, and the constraint then spans two lines (Q5 wraps the clause). This replaces "every line break inside one constraint … a type argument list (FMT-BREAK-014) … MUST be joined", which read broader than the code and the cited requirements.
  - The "wrapped" conditions are restated as line breaks that separate tokens: colon and first constraint, colon and type parameter (kept by a comment, directive, or disabled text), or two tokens of the constraints.
  - "Every later constraint MUST start a line" now says that a line break separates it from the token in front of it. Behind a multi-line comment that stands behind a comma, the line break goes behind the comment, and the comment stays behind the comma (FMT-BREAK-001).
  - "Changes nothing" for a clause that is not wrapped now also covers a clause that multi-line comments spread over several lines of the text.
  - The `Why` gains the line-start reasoning and the parameter-list counterpart.
  - New example `M`/`N`/`O`/`P`/`S`: the auditor's cases (a)–(d) stay as written, and `S` is the wrapped side of the qualifier (FC-002).
- **FMT-INDENT-032 — unchanged.** It places only lines that start with the colon, a constraint, or a comma, and its contributor already decides on line-start facts (`12-auditor.md`, *Guard scope*, last row). The phrase "when the constraints span several lines" places nothing in (a)–(d), because no such line starts there.
- **Change doc** `sdd/changes/2026-11/constraint-clause-wrap.md`. `Status` stays `Accepted`.
  - The *What* join and wrap bullets are reworded the same way, with the type-argument-list exclusion.
  - New *What* bullet "Line-start facts decide".
  - New entry 16, for case `S`. Its "before" is the real `main` output at e3924e2, and the entries' intro now reads "entries 14–16 at e3924e2".
  - New *Unchanged* bullet for (a)–(d), which `main` also leaves as written.
  - New *Decisions* section "Line-start facts decide whether a clause is wrapped", marked as the lead's reading of Q1 after the audit.
  - One rejected alternative is added.

## Results for the auditor's cases

| Case | Input | Output under FMT-BREAK-037 now |
|---|---|---|
| (a) | `where T /* a⏎ b */ : class, new()` | unchanged: no line break separates the colon from `T` or the constraints |
| (b) | `where T : /* a⏎ b */ class, new()` | unchanged (decision 8) |
| (c) | `where T : class /* a⏎ b */, new()` | unchanged: no line break in front of or behind the comma |
| (d) | `where T : class, /* a⏎ b */ new(), System.IDisposable` | unchanged, as the parameter list `void N(int a, /* x⏎ y */ int b, int c)` |
| (d) wrapped side | `… /* a⏎ b */ new(),⏎ IDisposable` | `b */⏎ new(),⏎ IDisposable`, both at the constraint column (example `S`, entry 16) |
| (e) | `IFoo</* a⏎ b */ T>` | unchanged by this round: a comment keeps a line break inside a type argument list, so the constraint spans two lines and wraps the clause (Q5), as the auditor found it holds |

## Examples for the tests

- FMT-BREAK-037: the new example `M`, `N`, `O`, `P` (unchanged) and `S` (wrapped), one test each, which covers (a)–(d) and the wrapped counterpart. Case (e) is already covered by the existing Q5 tests. Per the worklist, it may get its own multi-line-comment test.

## Output questions

_None._ Decision 8 decides (a)–(d): no line break separates their parts, so Q1 leaves them on one line. `S` follows from Q1 and decision 8. The clause is wrapped by the ordinary line break in front of `IDisposable`, and under line-start facts `new()` does not yet start a line, so it gets one. The comment stays behind its comma, as in the existing example `R` and in the parameter-list counterpart. Entry 16 is a new visible-output entry. The lead decides whether it needs asking. I see no competing reading that a maintainer answer leaves open. The literal reading the auditor described would also put `new()` on its own line in `S`.

## Internal choices

- **"Line break" is defined once in FMT-BREAK-037 by reference to SHARED-BLANK-008, and not restated in each sentence.** Alternative: the phrase "starts its line (SHARED-BLANK-008)". I rejected it because SHARED-BLANK-008's "starts its line" excludes a comment in front of the token on its line, while the contributor and the parameter-list layout use `IsFirstOnLine`, which allows one. Separation by a counted line break matches both.
- **FMT-INDENT-032 left unchanged** (see above). Alternative: reword its trigger to "a constraint or a comma starts a line". I rejected it because the reworded trigger would change no output and only add wording.
- **The parameter-list counterpart is cited in the `Why`, not the switch-section rule FMT-BREAK-033.** Both put a token behind a multi-line comment that starts behind the previous token on a line of its own, but the parameter list is the model the change doc already follows.

## Follow-up candidates

_None new._ Follow-up 9 in `00-lead.md` (`where⏎ T`) and the auditor's spacing hint (`IFoo</* a⏎ b */ T>`) are unchanged by this round.
