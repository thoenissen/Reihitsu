Role: spec author
Commit: 79ae25f3
Verdict: spec written

Repair round 1 (`00-lead.md` rows 21 and 22; `14-auditor.md` findings 1 and 2 and its architecture finding). My prompt carried no intended fix, suspected cause, or hoped-for verdict. No decided output changes.

## What I measured

I built `main` (`df4f828d`, scratch worktree, since removed) and the head CLI. I formatted the auditor's inputs with both, under LF and CRLF, and the two line endings agree. I ran `scripts/apply-fix.sh RH7101 --max-iterations 1` on `main` and on the head.

- **Document level, comment that spans lines behind a separating comma (auditor members `a`, `b`, `c`, `h`, and `/* a⏎ b */ /* c */`).** `main` ends the field's line behind the comment, and the next field starts its own line at the member column: `private int _x; /* a` / `       b */` / `    private int _y;`. The head gives `b */private int _y;` for every one of them, and the fix gives the same. `/* a⏎ b */ // c` agrees with `main` on the head.
- **Document level, comment that spans lines in a gap between members (`p`, `q`, and the combined input).** `main` and the head agree: `b */ private int _x;` and `d */ private int _q;` stay behind the `*/`. The head's fix moves them; `main`'s fix moved neither, but wrote `_y` in column 0.

## Was something missing or wrong?

**FMT-STRUCT-026 was wrong.** It allowed the split only a `//` comment's line break and the line break behind a moved comment. It forbade the line break behind a comment that spans lines and trailed a comma. Its Why also claimed that the line-break phase redoes "the line break in front of a field without a comment". The line-break phase does not break a gap whose only line break is inside a comment. So the spec as written gave the head's wrong output, and document-level output stopped being `main`'s.

**FMT-PIPE-029 decided findings 1 and 2 only by implication, and its qualifier "on the target's first/last line" was ambiguous (FC-002).** In the text, `_p` in `_p; /* a⏎ b */ private int _x, _y;` is on an earlier line. Under SHARED-COMMENT-012's reading, the token behind `*/` does not start its line. Neither reading was stated.

**The change doc repeated FMT-STRUCT-026's false Why**, in *The field split keeps the layout FMT-STRUCT-022 decides* and in the *What* summary.

## Changed requirements

- **FMT-STRUCT-026 — The phase writes only the separators its text needs — changed.**
  - The field split also writes a line break behind the last comment that trailed a separating comma when one of those comments spans lines, a `/* */` whose `*/` stands on a later line, so the next field starts its own line.
  - One-line comments write no such line break. Inline examples show both sides.
  - The Why is corrected. A field behind such a `*/` does not start its line (SHARED-COMMENT-012). The split writes the line break itself rather than depend on how the line-break phase treats a gap whose only line break is inside a comment. Only the line break in front of a field behind a `//` comment or behind no comment is redone by the line-break phase.
  - New example, measured on `main`: `_x, /* a⏎ b */ _y;` and `_p, /* a */ /* b⏎ c */ _q, /* d⏎ e */ /* f */ _r;`.
  - `Uses` gains SHARED-COMMENT-012.
- **FMT-PIPE-029 — The context entry point writes back every field the split makes of its target — clarified.**
  - A new gap bullet says another member stands on the target's first or last line exactly where document-level formatting starts that member, or the first field, on a new line (FMT-BREAK-007). The entry point moves what document-level formatting moves and leaves what it leaves, also when the gap holds a comment that spans lines.
  - The Why says that this is the line-break phase's decision, and that the entry point does not decide it again by a rule of its own (FMT-PIPE-001; decision 21).
  - Two new examples, measured on `main` and on the head: the combined `p`/`q` input, and the auditor's member-in-front case `m`.
  - `Uses` gains FMT-BREAK-007 and SHARED-COMMENT-012.
- **Change doc `sdd/changes/2026-11/rh7101-fix-through-context-entry-point.md` (`Status: Accepted`, left as is).**
  - The *What* summary and *The field split keeps the layout FMT-STRUCT-022 decides* now name the third piece of layout the split keeps, and correct the false sentence.
  - Case 7 gains the `p` and `m` inputs. The "before" outputs are measured with `main`'s fix; the "after" outputs equal the formatter's layout, as answer 1a–1f A and the existing case-7 rule decide.
  - Neither "after" is a new decision: in both, the later field moves to the member column, which case 8 and FMT-PIPE-029 already decide. `m` also equals `main`'s document-level output.

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"`: 37 passed.

## Examples for the tests

- **FMT-STRUCT-026.**
  - Phase-output test: `_x, /* a⏎ b */ _y;` gives `_x; /* a`, the line `       b */`, a line break, and `private int _y;`, unindented. For contrast, `_x, /* a */ _y;` gives `_x; /* a */private int _y;`.
  - Document-level test in `FieldDeclarationSplitTests`, the new example. It pins `main`'s output for the auditor's members `a`, `c`, `h`, and `/* a⏎ b */ /* c */`. Add member `b`, where `_y` is on the next line behind the comment; its output is the same as `a`'s.
- **FMT-PIPE-029.** The two new examples, at the entry point and as RH7101 fix tests. Also add the auditor's behind-only input `q`, `private int _x, _y; /* a⏎       b */ private int _q;`. `main`, the head formatter, and `main`'s fix all give `private int _x;` / `    private int _y; /* a` / `       b */ private int _q;`, and the fix must too. A test that asserts fix output == document-level output for these inputs stays correct if FMT-BREAK-007 for members ever changes (see *Internal choices*).

## Output questions

_None._ Every output above is `main`'s document-level output, or the formatter's layout, which answer 1a–1f A makes the fix result.

## Internal choices

- **FMT-PIPE-029 decides "shares the line" by reference to document-level formatting (FMT-BREAK-007), not by its own textual rule.**
  - This follows decision 21: one owner. The alternative, "no line break in the gap, also none inside a comment", would restate the line-break phase's rule as a second statement.
  - Lead, please note a pre-existing contradiction outside this run. SHARED-COMMENT-012's general clause, applied to FMT-BREAK-007, says a member behind a multi-line `*/` must start its own line. Statements, sections, and labels are its only exceptions. The formatter, on `main` and on the head, keeps `b */ private int _q;`.
  - Decision 21's "`_q` … does not share `_y`'s line" therefore agrees with the formatter, not with SHARED-COMMENT-012 as written. Open issue thoenissen/Reihitsu#1572 (`agent-filed`) already tracks this: it states members stay behind the `*/` and asks to rewrite SHARED-COMMENT-012's clause.
  - I wrote FMT-PIPE-029 so that it follows whichever way #1572 resolves. I did not restate the member rule in FMT-BREAK-007 or SHARED-COMMENT-012. No new follow-up.
- **FMT-STRUCT-026 has the split write the line break behind a comment that spans lines.** The alternative was to make the line-break phase break such a gap: SHARED-COMMENT-012 as written would demand it, but that is #1572's question. Under both readings the split's line break gives `main`'s output. It also keeps the split independent of the line-break phase's gap rule, which removes the prediction that caused finding 1.

## Follow-up candidates

_None new._ The SHARED-COMMENT-012 / FMT-BREAK-007 member contradiction is thoenissen/Reihitsu#1572.
