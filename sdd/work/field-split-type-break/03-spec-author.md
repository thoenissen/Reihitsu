Role: spec author
Commit: 8bedf48
Verdict: spec written

My prompt carried the maintainer's answer (A) and the change asked; it carried no intended fix, suspected cause, or hoped-for verdict to ignore.

Lane: tests. The text is a clarification of today's output (answer A); no change doc, no rule page change (`RH7101.md` is not affected: what is reported and how it is fixed for the user is unchanged).

## Changed requirements

- FMT-STRUCT-021 — One field per declaration — clarified: a new normative paragraph states that every field repeats the attributes, modifiers, and type with the whitespace and line breaks written between them and behind the type (first and later fields alike); a later field repeats only that head, not the declaration's leading trivia (FMT-STRUCT-022), nor the removed `,`'s trivia, nor its declarator's own leading trivia, so a line break behind a `,` stands in no field; whether a repeated break stays is FMT-BREAK's, its column FMT-INDENT's. `Why:` extended; one new example.

No requirement ID added; `requirement-ids.txt` and `sdd/specs/README.md` unchanged.

## Examples for the tests

FMT-STRUCT-021, the new example "line breaks inside and behind the head are repeated in every field". Its three declarations cover both sides of the qualifier "inside and behind the head" (FC-002):

- break behind the type, long generic type, initializer on the later declarator — repeated: `private Dictionary<string, List<int>>⏎        _map1, _map2 = new();`
- break between two modifiers, initializer on the first declarator — repeated: `private static⏎        readonly int _g = 1, _h;`
- break behind a `,`, with an attribute — not repeated: `[NonSerialized]⏎    private int _x,⏎        _y;`

Measured on 8bedf48: `scripts/format.sh` gives exactly the example's output, and a second pass changes nothing; `scripts/apply-fix.sh RH7101` gives the same output under LF and CRLF (`fixed (1 action(s), 3 iteration(s))` each). Suggested tests (test implementer's call): each declaration through the formatter under LF and CRLF, through `RH7101DoNotCombineFieldsFormatterTests`/the RH7101 fix, and a second pass (FC-010). The issue's own input `private int⏎        _g, _h;` is already pinned by the untagged `FieldDeclarationSplitTests.WhitespaceOnlyTriviaBeforeFirstDeclaratorIsUnchanged`; it should get `[Requirement("FMT-STRUCT-021")]`.

The declarator/modifier column in the example (member column, no continuation indent) is today's output and is not decided by FMT-STRUCT-021; a test asserting it pins the indentation phase's current behavior, which thoenissen/Reihitsu#953 may change.

## Output questions

_None._ (Question 1 answered A.)

## Internal choices

- Clarification written into FMT-STRUCT-021 itself rather than a new FMT-STRUCT-031 (skeptic's recommendation; the same output either way). FMT-STRUCT-022 still lists what only the first field keeps.
- FMT-STRUCT-026 left unchanged: its "the trivia a transform carries over keeps what the author wrote" already agrees with answer A.
- The example adds the `,`-break side (not repeated) so the qualifier "head" has an example on each side; the alternative was the issue's single `int` input only, which the existing test already covers.

## Follow-up candidates

_None_ beyond the two in `00-lead.md` (from 02-skeptic).
