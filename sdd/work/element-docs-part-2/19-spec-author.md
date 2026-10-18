Role: spec author
Commit: 964c2a5
Verdict: text written

Text-only repair of `18-auditor.md` finding 1. The lead's message carried no intended fix or hoped-for verdict.

## Re-derivation

I wrote the fixture with `printf`, so its bytes are literal (checked with `od -c`):
- `extension(int va` + U+200B + `lue)` with `<param name="value">`;
- `extension(long va` + the six characters of a backslash-u escape for U+200B + `lue)` with `<param name="value">`.

The scratch harness (Roslyn 5.9.0, `LanguageVersion.Latest`, Debug `Reihitsu.Analyzer.dll`) shows no RH8032 and no compiler warning for either, in both `Diagnose` and `Parse` mode. The code compares with the identifier's value, from which format characters are removed. The definition in SHARED-DOC-012 named only the removal of `@` and the decoding of escapes.

## Change

`sdd/specs/shared/element-documentation.md`, SHARED-DOC-012, wording only:
- **Normative sentence:** "The parameter's name is the value of its identifier: without the `@` of a verbatim identifier, with a Unicode escape sequence decoded, and with format characters (Unicode category Cf) removed."
- **`Why:`:** "… without the `@` that only lets a keyword be used as a name, with its escape sequences decoded, and with its format characters removed, as C# does when it compares identifiers."

What did not change:
- No behavior, ID, example, coverage sentence, test, or rule page changed.
- The coverage exclusion for a `name` *attribute* with an escape or a format character (Q5, thoenissen/Reihitsu#1130) is unchanged.
- No existing test asserts or contradicts the parameter-side format-character shape.

## Checks

- **Literal backslash-u sequences intact:** `grep -c` finds 4 lines with a backslash-u sequence in the shared spec before the edit and 4 after. `od -c` of the `extension(ushort …)` line shows the bytes `\ u 0 0 6 1`.
- **Spec tests:** `SpecConsistencyTests` and `SpecStructureTests` pass.

## Open questions

Unchanged from `09-spec-author.md`, with the Q5 addition of `14-spec-author.md`, already appended to thoenissen/Reihitsu#1130.
