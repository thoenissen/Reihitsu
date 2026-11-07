Role: spec author
Commit: a0490b5f
Verdict: #1279: spec written; #1257, #1281, #1282, #1417, #1422: nothing to do

Lead row 50 (`00-lead.md`) for auditor finding 23 (a)–(c) (`44-auditor.md`). Bug-fix lane: the change records a deliberate gap in RH3004's fix coverage (AGENTS.md, *Code-fix coverage*), decided by the lead under the maintainer's standing instruction; no output question raised. No change doc. No rule page changed (`documentation/rules/RH3004.md`): a withheld fix in a corner case is not a normal-case change. My prompt named the outcome row 50 decided; that is the lead's recorded decision, which I followed, and it carried nothing else to set aside. Member (d) (a carrier before the anonymous method) is not covered, per row 51.

## Changed requirements

- RH3004-008 — A string or a comment that spans lines withholds the fix — changed (row 50), retitled from "A string or a comment that spans lines withholds the fix in a block body". Defines a *spanning carrier* (a string literal of any kind or a delimited comment that starts on one line and ends on a later one) and a *`#` line* (a later line of it whose first character other than lexer whitespace — `SyntaxFacts.IsWhitespace`: space, tab, vertical tab, form feed, other lexer whitespace — is `#`). Two conditions over the span from the first token (`async`/`delegate`) through the block's `}`, parameter list included: (1) both paths — any spanning carrier with a `#` line withholds; (2) block-body path — any spanning carrier withholds. Each condition is stated on both sides (path, place, line start, one line). The Why no longer says the expression body is free of the hazard; the "ends that class without knowledge of the directives around the anonymous method" claim is narrowed to lines the anonymous method holds, and says a string or comment that starts before the first token is not seen (row 51 owns that).
- RH3004-007 — A multi-line raw string literal withholds an expression body only by a `#` line — changed, retitled from "A multi-line raw string literal withholds the fix only in a block body". The expression-body path stays offered for a multi-line raw string without a `#` line; one with a `#` line withholds by RH3004-008. The Why sentence "a guard there would refuse an input the fix rewrites safely" is corrected: the expression body moves no line but still deletes and writes text, and the `#`-line hazard is RH3004-008's.

No ID added or retired; `sdd/specs/requirement-ids.txt` unchanged. RH3004-003 and RH3004-006 still read correctly (RH3004-006's "a block the fix keeps … holds no string literal and no delimited comment that spans lines (RH3004-008)" stays true).

## Examples for the tests

RH3004-008, new withheld (reported, no code fix offered):
1. finding 23 (a), expression-body path, `#define A`: `#if A` / `Func<string> f = delegate () { return @"` / `#else` / `Func<string> f = () =>` / `"; }; //";` / `#endif`;
2. finding 23 (b), block-body path, `#define A`: comment `/*` in the parameter list hiding `#else` / `var s = @"` / `*/)`, block `Console.WriteLine(x);`, `}; //";`;
3. finding 23 (c), expression-body path: as (b) with `*/) { return x; }; //";`;
4. no directive in the document, expression-body path: `return @"a⏎#b";` and `return """⏎    a⏎    #b⏎    """;`;
5. no directive, block-body path, added to the existing four-of-a-kind example: `Action<int> e = delegate (int x /* first⏎ second */) { Console.WriteLine(x); };` (multi-line comment in the parameter list without a `#` line).

RH3004-008, unchanged: the comment/chain example (20 a), the form-feed example (20 d), the block-path no-directive examples `b`, `c`, `d`, and the offered one-line-strings block with exact output.

RH3004-008, new offered: `return @"a⏎b";` as an expression body, output `Func<string> b = () => @"a⏎b";` (same layout as the old `#b` test).

RH3004-007: unchanged examples; the offered raw string (`a`, `  b`) has no `#` line and stays offered.

## Examples whose outcome changes

- `return @"a⏎#b";` on the expression-body path: offered → withheld. Test `FixRewritesASingleReturnOfAStringThatHoldsALineStartingWithHashAsAnExpressionBody` flips to `VerifyNoCodeFixIsOffered` (rename it); its offered side moves to `@"a⏎b"`.
- New inputs that `main` and the branch tip fix and the spec now withholds: findings 23 (a), (b), (c); the multi-line raw string with a `    #b` line on the expression-body path; the multi-line `/* first⏎ second */` comment in the parameter list on the block-body path.

No other example changes outcome.

## Output questions

_None._ The standing instruction applies (row 50).

## Internal choices

- Kept the two conditions in RH3004-008 instead of moving the `#`-line condition to RH3004-007 or a new ID: both share the carrier definition and the reason (SHARED-FIX-003, build configurations); RH3004-007 keeps only the raw-string layout statement and points to RH3004-008. Alternative: a new requirement for the `#`-line condition; rejected as splitting one guard over two IDs.
- "Lexer whitespace" is written out for the reader (space, tab, vertical tab, form feed, every other character the lexer skips as whitespace) and the implementer's anchor is `SyntaxFacts.IsWhitespace`, as the auditor named it. A `#` line is only a *later* line of the carrier: the line a carrier starts on begins with code or the carrier itself, never `#`, since the anonymous method holds no directive (RH3004-009).
- The (d) limit is stated as scope ("a string or a comment that starts before the anonymous method's first token is not decided by it"), not as an outcome, because the outcome there is the defect row 51 files.

## Follow-up candidates

_None new._ Member (d) is row 51 (repository-wide "same code in every build" check in Core), already in `00-lead.md`.

## Whole run

- Files: `sdd/specs/analyzer/RH3004.md`.
- `scripts/test.sh --project architecture`: 115 passed, 0 failed.
