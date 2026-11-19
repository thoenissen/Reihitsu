Role: spec author
Commit: 7657e9e
Verdict: spec written

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond decision 15, which is a lead decision recorded in `00-lead.md` and which I carried out.

## Architecture refusal (decision 15)

**RH5401-004.**
- The bracket condition now reads: the fix MUST NOT be offered when an attribute list holds, between its brackets, a blank line, a directive, disabled text, or a comment that starts its own line. It applies whatever kind the directive is.
- The sentence "A directive or disabled text between the brackets does not withhold the fix by this condition" is gone.
- The "outside the brackets" side (FC-002) now names directives as well. It gains `E2` (offered, a `#pragma` in front of `[`). `Z` shows that one directive between the brackets is enough: its `#region` stands in front of `[`, its `#endregion` between the brackets.
- `Why:` no longer lists the formatter's reasons for changing blank lines inside a list (the old "by dropping one … and by inserting one in front of a comment", and "Blank lines and own-line comments are what these changes need"). It now says:
  - Behind a list, the fix compares counts.
  - Inside a list, no count works (`W`), so the fix does not judge the formatter's blank-line layout there and does not restate its reasons. It refuses by what stands between the brackets.
  - A comment on a line with code is left out of the refusal, and the fix is offered for it (`U`, `V`, `Y`).
- The first `Why:` sentence no longer claims "the fix does not insert or remove one". It now says the fix compares the blank lines behind each list.
- The per-gap explanation names its causes "for example" and says the fix compares the numbers without asking why they differ.
- The withheld example gains three members:
  - `I`: `#if`/`#else`/`#endif` with disabled text in an attribute's arguments.
  - `X`: the auditor's `d01`, `#region`/`#endregion` between the brackets.
  - `Z`: the auditor's `e01`.

**RH5401-003.**
- The condition sentence names directives and disabled text.
- "so its result has the blank lines there" now reads "behind the attribute lists". That holds by construction through the per-gap comparison. No claim about blank lines inside the brackets remains.
- A new example `E2` covers a directive in front of `[`, with its exact output.

**Change doc** (`Status: Accepted` kept):
- *What*: "so it never changes a blank line among or behind the attribute lists" is removed. The refusal now names directives and disabled text, and says a directive in front of `[` does not withhold the fix.
- The offered block gains `E2`.
- The unchanged block gains `X`. Its heading and prose name directives and disabled text.
- The "judges the property's own lines" decision names directives and disabled text.
- The question-6 decision:
  - It no longer names the two insertion causes as the complete reason.
  - It states the refusal by what stands between the brackets, adds `X`, and says that none of the refused inputs got a fix before.
  - "The fix's output therefore never changes a blank line among or behind the attribute lists" now reads: behind the attribute lists, the comparison keeps the blank lines as written.
- Rejected alternatives:
  - "a cheaper and complete check" now reads "cheaper and depends on no count".
  - The every-comment alternative no longer argues that "a comment on a line with code causes no blank-line change".
  - New: "refuse only blank lines and own-line comments, keep directives". It was rejected because it states the refusal by some of the formatter's reasons and misses `X`.

Rule page `documentation/rules/RH5401.md`: unchanged. The change only withholds a fix in a corner case.

## Does decision 15 withhold a fix that `main` offers?

No, so there is no question for you. `scripts/apply-fix.sh RH5401` on `origin/main` (`e3cf4cd`, temporary worktree, since removed), under LF and CRLF, returned no-fix-offered for all of these:
- the auditor's directive members `c01`, `c02`, `c10`, `c11`, `d01`, `d03`, `d04`, `d06`, `d07`, `e01`–`e04`, `f01`–`f03`;
- two attribute lists on the property's line with directives inside: `[Obsolete(⏎#if X⏎"a"⏎#else⏎"b"⏎#endif⏎)] int E`, and `[Obsolete⏎#pragma …⏎] int G`;
- `[Obsolete(⏎#region R⏎"a"⏎#endregion⏎)] int E`;
- the spec's `I`, `X`, and `Z`.

The reason is structural: a directive always starts its own line, so the list spans several lines, and `main` required the whole formatted declaration on one line. Fixtures are in `/tmp/claude-0/-home-user-Reihitsu/5c2095c3-beb1-5980-beb5-54178f424a54/scratchpad/s26/dir/`, with output in `s26/main.out` and `s26/head.out`. On the head all 18 are fixed today; they are the reproductions.

## Verification (head `7657e9e`, LF and CRLF)

I split each RH5401 example into one fixture per member, because `apply-fix` stops at the first diagnostic without a fix. They are in `/tmp/claude-0/-home-user-Reihitsu/5c2095c3-beb1-5980-beb5-54178f424a54/scratchpad/s26/mem/`, with output in `s26/mem-head.out` and `s26/mem-main.out`.
- RH5401-003: `A`, `B`, `L`, `E`, `F`, `J`, `U`, `V`, `Y`, and `E2` are offered. Each whole example block's output matches the spec's after block byte for byte, `E2` included.
- RH5401-004: `N`, `O`, `P`, `Q`, `R`, `T`, `W`, `D`, `G`, `H`, `K`, `M`, and the wrapped members are withheld. `I`, `X`, and `Z` are fixed today; they are the reproductions. On `main` all three got no fix.
- Change doc: the RH5401 offered block's output matches its after block (`E2` included). On `main` it was no-fix. In the unchanged block, `D`, `H`, `N`, `Q`, and `T` are withheld today and on `main`. `X` is fixed today (reproduction) and was no-fix on `main`.
- `scripts/test.sh --project architecture`: 198 passed.

## Changed requirements

- RH5401-004 — The fix is withheld when the property stays on several lines — changed (directives and disabled text between the brackets withhold the fix; causal list and completeness claims removed; examples `I`, `X`, `Z` added)
- RH5401-003 — The fix puts the property on one line — changed (condition names directives and disabled text; blank-line claim limited to behind the lists; example `E2` added)

## Examples for the tests

- RH5401-004 withheld block: one `VerifyNoCodeFixIsOffered` test each for `I`, `X` (the auditor's `d01`), and `Z` (the auditor's `e01`). All three are offered today.
- RH5401-003: `E2` (offered, exact output). It is offered today, so this test guards the outside-the-brackets side.

## Output questions

_None._ No input that `main` fixes loses its fix (see above). The newly withheld inputs are a rare-class refusal under decision 15.

## Internal choices

- **Example names `I`, `X`, `Z`, `E2`.** I did not reuse `S`, which was retired in 19, so that old test names stay unambiguous.
- **No claim about blank lines inside the brackets on the offered side.** For `U`, `V`, `Y`, and plain line breaks between the brackets, the spec no longer says the fix keeps the blank lines there. The probes show that it does today, but that does not hold by construction, and the lead asked to drop such claims. The alternative was to keep a statement backed only by probes.

## Follow-up candidates

_None._
