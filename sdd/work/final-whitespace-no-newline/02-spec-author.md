Role: spec author
Commit: 98baf45b5f4682aa2dd9895ba4f203733641b79f
Verdict: spec written

My prompt named the asked change (the lead's decisions 3–5 in `00-lead.md`); it carried no suspected cause or hoped-for verdict beyond those recorded decisions. The clarification writes down the output the maintainer decided in thoenissen/Reihitsu#923 Q3; no change doc (bug-fix lane, #923 Q1 precedent).

## Changed requirements

- FMT-CLEAN-005 — The file does not end with a line break or whitespace — clarified (widened): whitespace trivia after the last token is removed whether or not a final line break follows; covers the skeptic's failing members 1–9 (directly behind the last token, LF/CRLF/BOM/tab; behind a block comment on the token's line; behind a block comment on its own line; comment-only file). States the condition with both sides (FC-002): only whitespace trivia and line breaks up to the end of the file; gaps on a continuing line, whitespace inside a `//` comment's or a directive's text, and a final directive's line break are not covered.
- Non-goals — the end-of-file part ("whitespace behind the last token of a file that has no final line break" and "and behind the last token") removed; the comment, documentation-line, and directive-line parts (#959, #960 item 1) stay, with a pointer that end-of-file whitespace trivia is FMT-CLEAN-005's.
- Intro sentence — now names "line breaks and whitespace at the end of a file".
- FMT-CLEAN-006, FMT-CLEAN-007, FMT-CLEAN-008, FMT-CLEAN-009 — unchanged.

No new requirement ID; `requirement-ids.txt` unchanged.

## Examples for the tests

FMT-CLEAN-005 (new prose example): `}` + three spaces, no line break → `}` (LF and CRLF; tab variant → `}`); `} /* c */` + three spaces → `} /* c */`; `}`, blank line, `/* end */` + three spaces → ends `/* end */`; `/* only */` + three spaces → `/* only */`. Holding side: `int y = x + 1;` keeps its gaps; `// end` + three spaces keeps the spaces. Each must equal the output of the same input with a final line break appended, and a second pass changes nothing. Inputs must be built from explicit strings with the trailing spaces visible (FC-033). The examples are prose, not blocks, because the difference is trailing whitespace.

## Output questions

_None._ #923 Q3 decides every member.

## Internal choices

1. Widened FMT-CLEAN-005 rather than FMT-CLEAN-001 (lead's decision 4; keeps the issue's test tag).
2. Comment-only file (member 9) is stated inside FMT-CLEAN-005 ("also in a file that holds no token but comments") rather than left out; alternative: leave it unspecified as rare. Same mechanism and same #923 Q3 rule, so stated.
3. Left `pipeline.md` FMT-PIPE text "MAY remove whitespace that ends a line and line breaks at the end of the file (FMT-CLEAN-001, FMT-CLEAN-005)" unchanged: the whitespace at the end of a file is whitespace that ends the last line, so it already admits the change.

## Follow-up candidates

_None_ beyond the two already recorded in `00-lead.md` (update #960; FMT-CLEAN-007 vs directive-only file), which I did not touch.
