Role: skeptic
Commit: 62f7a21a192c7660d2d134a01908b283a075349c
Verdict: proceed narrower

This is a re-check of the revision in 62f7a21 against `02-skeptic.md`, plus the open-question list in `03-spec-author.md`. The coordinator's message carried no intended fix and no hoped-for verdict; nothing had to be ignored. GitHub was not accessed. Evidence comes from `scripts/apply-fix.sh RH#### <dir>` and `scripts/format.sh <dir>` on fixtures in the session scratchpad, outside the repository.

## Premise

### Items from `02-skeptic.md` that now hold

- **A1 RH6001-002** now names the unchecked keywords, and `fixed` is claimed in neither direction.
- **A3 RH5601-002** — the comment exemption now excludes comments on a directive line, which matches the code.
- **A6 RH6005-002** — "followed by exactly one space" no longer decides several spaces or a line break.
- **A7 RH5604-001/-002/-005** are limited to `\r\n` and `\n`, so lone `\r` is unclaimed. RH5604-003's "text of a string literal" matches the code: a line break inside an interpolation hole is trivia, so it is counted.
- **A8 RH6003-001, RH6011-001, RH6012-001** — "and the token before … ends on that line" claims only inputs the code reports and the formatter cleans. The continuation-line comment case is unclaimed.
- **B** — all four corrections are in: RH6012 `Dictionary<, >`, the RH5604 caption and convention, the RH6006/RH6007 Why plus FMT-CLEAN-001 in Uses, and RH5601's Why plus FMT-CLEAN-003. My RH6002-003 item (report only) is not in `03-spec-author.md`; the lead still has to report it to the maintainer as a finding on an existing spec.
- **C** — SHARED-WS-001's wording and Why now fit the blank-line users of `WhitespaceSpanRemovalCodeFixProviderBase`.
- **D** — RH6007-001's owner list is now open ("for example"). RH6009-001 and RH6010-001 claim `/* */` but not `/** */`.

### Items that still contradict the code or settle a listed question

1. **RH5601-001 (A2, partly fixed)** reports a tab "when neither token next to it is a string or character literal … between the tokens of a preprocessor directive". A tab between the tokens of a directive whose trivia belongs to a following literal is not reported:

   ```text
   class C
   {
       string s =
   #pragma→warning disable CS0168
           "abc";
   }
   ```

   apply-fix gives `no-diagnostic`. The tokens next to the tab are `pragma` and `warning`, so the requirement claims it. The formatter replaces that tab (FMT-CLEAN-003). The mechanism is the same as in RH5601 Q2: the check reads the token that *owns* the trivia, not the tokens next to the tab. This input belongs in Q2, and RH5601-001 must not claim it. The intro sentence ("reports every tab character in the code outside literals, comments, and disabled text") still overclaims, though it is not normative.
2. **RH5602-001 and RH5603-001 (A4/A5, partly fixed)** both use "the last character of a token", which also covers the tokens of directives and documentation comments (structured trivia):
   - RH5602-001 claims `#if DEBUG··` is reported. That is true (`fixed`), but it is the directive-line case left out per thoenissen/Reihitsu#960.
   - RH5602-001 claims `/// <summary>··` is reported: `>` ends a documentation token. It is not reported (`no-diagnostic`), so this contradicts the code.
   - RH5603-001 claims `…⏎#pragma warning restore CS1591⏎⏎` and `…⏎#endif⏎` are reported: the character before the line breaks ends a directive token. This settles RH5603 Q1 in favor of the current analyzer.

   Both requirements have to restrict "token" to tokens outside directives and documentation comments, or name those exclusions.
3. **RH6009-001 and RH6010-001** — I missed this in round 1; it was already true at a729d6c. Both claim every brace preceded by the last character of a token, and RH6009-002/RH6010-002 exempt only the interpolation's own braces. So `$"{new int[]{1}.Length}"` (preceded by `]`) and `$"{new int[] { 1}.Length}"` (preceded by `1`) are claimed as reported, but both give `no-diagnostic` (round-1 runs). The requirements contradict the code and settle RH6009 Q1 and RH6010 Q1. They must leave braces inside an interpolated string's expression unclaimed.
4. **Precision only, optional.** Both behaviors agree with the formatter.
   - RH6001-001 lists `if`, and `#if(DEBUG)` is not reported (`no-diagnostic`), because directive tokens are not visited.
   - RH6006-001 says "an opening parenthesis token", and `/// <see cref="M( int)"/>` is not reported (`no-diagnostic`). Its non-goal names only directives as "not code tokens".

## Mechanism and candidates

_N/A._

## Breaking inputs

Add to the round-1 list: the directive-before-literal input in item 1, `/// <summary>··` and `#if DEBUG··` for RH5602, and a brace inside an interpolation expression for RH6009/RH6010. None of these may be tagged with a requirement while it is unclaimed. `03-spec-author.md` lists the existing tests that must stay untagged; add any test whose input is one of the cases above.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Specs RH5601, RH5602, RH5603, RH6009, RH6010 | must change (items 1–3) |
| Other specs, analyzers, code fixes, formatter, rule pages | as in `02-skeptic.md` |

## Simpler option

_None._

## Questions for the maintainer

None blocks the run. Each listed question is checked against reproduced behavior:

| Question | Matches reproduced behavior | Stands on its own | Note |
|---|---|---|---|
| RH5601 Q1 | yes: the exact input gives `fixed` after 3 iterations; the formatter keeps all three tabs and adds `#endregion // Na→me` | yes | — |
| RH5601 Q2 | yes: the exact input gives `no-diagnostic`; the formatter gives `string s = "abc";`, `char c = 'x';`, `M("a", "b");`; a tab after `=` on the same line is reported (`fixed`) | yes | add the directive-before-literal input (item 1) |
| RH5602 Q1 | yes: `string u = @"a⏎b";··` also gives `no-diagnostic`, and the formatter removes it | yes | — |
| RH5603 Q1, Q2 | yes (round 1) | yes | Q1 is open only once item 2 is fixed |
| pipeline.md Q1 | yes: analyzer by code reading, formatter verified | yes | — |
| RH6003 Q1 | yes: the exact input formatted keeps `/* keep */ ;`, then `no-fix-offered` | yes | — |
| RH6004 Q1 | yes (round 1) | yes | — |
| RH6005 Q1 | yes: the line-break input gives `no-diagnostic` | yes | add that the formatter **keeps** the line break (`operator⏎    +(C a, C b)`), so reading (a) would also flag formatted code; reading (b) "a space" does not cover a line break either |
| RH6006 Q1 | yes (round 1) | yes | — |
| RH6007 Q1 | yes: `arr[  // d` gives `no-fix-offered`, and the formatter keeps it | **no**: "Readings as in the RH6006 question" points to another issue | state the readings in place |
| RH6009 Q1 | yes | yes | open only once item 3 is fixed |
| RH6010 Q1 | yes | **no**: "Same reasoning as the RH6009 question" | state why it looks wrong in place; open only once item 3 is fixed |
| RH6011 Q1, RH6012 Q1 | yes: the exact inputs give `no-fix-offered`, and the formatter keeps them | **no**: "Readings as in the RH6003 question" | state the readings in place |
| horizontal-spacing.md Q1 | partly. The formatter turns `void M()/** y */{` into `void M()⏎⏎    /** y */{`, and RH6009 reports that shape (`fixed`). But the stated contrast "`/* y */{` gets the space (FMT-SPACE-013)" is false for this input: `void M()/* y */{` becomes `void M()/* y */⏎    {` (brace on its own line). The contrast holds for an initializer, where `new int[]/* y */{ 1 }` becomes `new int[]/* y */ { 1 }` and `new int[]/** y */{ 1 }` becomes `new int[]⏎⏎    /** y */{ 1 }`, which RH6009 reports | yes | change the input or the contrast |

About thoenissen/Reihitsu#958: `03-spec-author.md` now records its subject as formatter spacing for lambda arrows, conditionals, element access, `::`, named arguments, and initializer braces. None of the questions is about those pairs. RH6009/RH6010 Q1 quote formatter output `new int[] {1 }`, and the missing space after `{` there may be what #958 means by "initializer braces". The questions are about the analyzer skipping these braces, so they are not duplicates; the quoted output just should not read as endorsed formatter behavior. I could not verify the claim that thoenissen/Reihitsu#920 is closed.

Both follow-up candidates are unchanged and still hold: formatter FMT-INDENT-009 after disabled text, and the `scripts/trace.sh` file mode.
