Role: auditor
Commit: a0535a8
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

This audit covers the repair delta against 4a00ace, the head of the second audit (`19-auditor.md`). It does not repeat a full audit. The head is a0535a8 and the base is `origin/main` b4343557df67bc95a2278c405950476716fb9abf. The checkout is a0535a8 and the working tree is clean. `origin/main` after fetching is the SHA from the prompt, and it is an ancestor of the head.

`git diff 4a00ace HEAD` (without `sdd/work/`) matches handovers 20–24:

- RH5202-006 is new, RH5202-003 and RH5202-004 are clarified, the `Uses` row gains SHARED-FIX-002, and `requirement-ids.txt` gains the new ID.
- In the fix, `GetShift` was extracted, and `ChangesValueOfNestedRawString` and `StartsInOwnHole` are new. The guard is applied only in the interpolated registration branch. `ApplyCodeFixAsync`'s rewrite is unchanged apart from calling `GetShift`.
- `IsMultiLineRawStringLiteral` moved unchanged from `RawStringLineShifts` to `RawStringLineUtilities` (decision 14).
- 20 analyzer tests and one private helper were added.
- Four Core `DataRow`s now use escapes.

The last compiled change is cc662a9. The cleaner's Release build (e4b366d) and the Release DLL time stamp both come after it, and only `sdd/work/` changed after cc662a9 (FC-032). The scope did not grow beyond RH5202, its fix, and the formatter helper the fix already calls.

Evidence:
- `scripts/apply-fix.sh RH5202` over scratch fixtures, under LF and CRLF.
- A scratch file-based app (`#:project` on the CodeFixes project) that registers actions for every RH5202 diagnostic, not only the first. On each iteration it applies the first action offered on any diagnostic, then prints the final text and the raw-string token values.
- A compiler probe of a tab-only content line under a space prefix.

I deleted all fixtures and probes afterwards.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed (unchanged since `19-auditor.md`) | — |
| 2 | superseded (unchanged since `19-auditor.md`) | — |
| 3 | closed | **Exactness.** The guard withholds exactly when the shift is leftward by `d`, a nested raw literal has `k < d`, and some content line has `w > k`. That is exactly the condition under which the per-line clamp changes the value:<br>• With `k ≥ d`, every line loses `d`.<br>• With `k < d`, a text line keeps its value iff `min(d, w) = k`, that is iff `w = k`.<br>• A whitespace-only line contributes `max(0, w − k)` before the fix and `max(0, w − d)` after it, which are equal iff `w ≤ k`.<br>• A whitespace-only line whose characters differ from the prefix is CS9003, so it is not an input.<br>**Failing cells of #3, now withheld and pinned by tests:**<br>• plain, `k=4` (+CRLF);<br>• interpolated `$"""`, `k=4` (+CRLF);<br>• `k=0` (+CRLF).<br>**Members I added, withheld:**<br>• `u8` (test);<br>• a raw string in a hole of a regular `$"…"` in the hole (test);<br>• `$$"""` (probe);<br>• a raw string two raw levels deep, `$"""` → `$"""` → `"""` (probe).<br>The last two are released by the nested fixes, and the values are kept: 30 spaces and 35 spaces.<br>**Holding cells:** `k=d` (test, output equal to `main`), every rightward shift (test with a content line wider than the nested closing line), and whitespace-only `w ≤ k` (test).<br>**Release:** the Fix All pair asserts it for the first example, 2+2 iterations with the relative 30 kept. |
| 4 | closed | Rows 25, 26, 27, and 30 are ` `/` ` escapes with unchanged lengths. |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 5 | medium | `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider.cs:100` (`StartsInOwnHole(nested, line.Start) == false && …`). The qualifier comes from RH5202-006: "that starts in its text rather than in one of its own holes". The tests are in `RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests.cs` | FC-002, FC-003, FC-017 | – | PR-introduced | **Missing test.** No test pins the qualifier "starts in its text rather than in one of its own holes". `StartsInOwnHole` returns `true` only for a line of a nested interpolated multi-line raw string that the guard visits (shift `< 0`, `k < d`) and that starts inside one of that literal's `InterpolationSyntax` spans. The only tests with a nested `$"""` are the two withheld ones, and their hole (`x{n}`) is on one line. So `StartsInOwnHole` returns `false` in every test, and a mutation to a constant `false` would leave the suite green. The mutated guard would then withhold safe inputs, which is the imprecise-guard case `AGENTS.md` (*Code-fix coverage*) calls worth narrowing. **Input that decides it**, inside `string M(int n)`: `return $"""` / 25 spaces `Hello {$"""` / 4 spaces `{n` / 14 spaces `+ 1}` / 4 spaces `"""}` / 25 spaces `""";`. The outer literal shifts left by 8, and the nested `k = 4`. The line `+ 1}` starts in the nested literal's own hole with 14 > 4 whitespace characters. At the head the fix is offered (`apply-fix`: fixed, LF and CRLF) and the nested value `{n + 1}` with no leading whitespace is kept. Under the mutation it would be withheld. | Add an offered-side test for that shape: outer fix offered, exact single-fix output, nested literal still reported. Add a CRLF variant. The output equals `main`'s, because the rewrite is unchanged and RH5202-003/004 decide it, so nothing visible is in question. Tag RH5202-006 (and RH5202-004). |
| 6 | low | `sdd/specs/analyzer/RH5202.md`, RH5202-006 *Why*, last sentence: "The nested literal's own diagnostic keeps its fix, which shifts its lines to the right, and once that fix has run, the outer literal's fix is offered on the result (SHARED-FIX-002)" | FC-031 (spec text against the code) | – | PR-introduced | **The release is stated for every input, but it holds only when the nested opening quotes stand at least `d` columns from the line start.** The nested literal's fix moves its closing line to its opening column `c`. The outer guard is then still true while `c < d`, because `k' = c < d` and the wider content line keeps its excess. `c ≥ d` always holds when the nested literal opens on a content line of the outer literal (`c > K ≥ d`), as in every example and test. It does not hold when the nested literal opens on a line that starts in the outer literal's hole code at a low column. **Failing members (probe, LF):**<br>(a) `return $"""` / 25 spaces `Hello {` / 4 spaces `"""` / 6 spaces `x` / 2 spaces `"""}` / 25 spaces `""";`. The nested literal (`c=4`, `k=2`) is reported and its fix shifts it right by 2. The outer literal is still withheld afterwards, and the run ends with the outer diagnostic and no fix.<br>(b) The same with the nested closing at 4 spaces (`c = k = 4`, `x` at 6). The nested literal is aligned and not reported, so the outer diagnostic never gets a fix.<br>The behavior is correct: the shift would change the nested value (SHARED-FIX-003, Q2 = A), and a diagnostic without a fix is a supported end state. Only the spec's claim is too general. The spec's first example (`c = 32 ≥ d`) holds. | Limit the sentence to the case where the nested opening quotes stand at least as far from the line start as the shift, or say that otherwise the outer diagnostic stays without a fix. This is spec text only. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Withhold the outer fix (`ChangesValueOfNestedRawString`) | Every multi-line raw string literal among the outer literal's `DescendantNodes()`, at any depth. For each: the `SourceText` line of `Span.End` (closing width `k`), then lines `open+1 … close−1` that do not start in an own `InterpolationSyntax` | The rewrite in `ApplyCodeFixAsync`: every line with `MovesLine` true, each losing `min(d, w)`. Lines of a nested raw literal that `MovesLine` skips are empty (`w = 0`, never `> k`). A line in nested raw text is never inside non-raw text or a comment. Lines in an own hole hold code, not value | Same lines, same width helper (`GetLeadingWhitespaceLength`), same line split (`SourceText.Lines`) under LF, CRLF, and lone `\r`. The value-change condition is exact (see #3) | agree |
| The shift the guard and the rewrite use | `GetShift`: opening line of `node.Span.Start`, `quoteOffset`, closing width | `ApplyCodeFixAsync` calls the same `GetShift` | One owner | agree |
| Only interpolated outer literals are guarded | Registration branch on `InterpolatedMultiLineRawStringStartToken` | A plain or `u8` literal has no holes, so nothing can be nested | Complete | agree |
| "Starts in its own hole" (`StartsInOwnHole`) | `Contents` of the nested `InterpolatedStringExpressionSyntax`, `InterpolationSyntax` spans `[Start, End)` | RH5202-006 "starts in its text rather than in one of its own holes". A line in a deeper literal's hole lies inside the nested literal's own hole too | Agree. A line that starts at a hole's `{` in column 0 is classified as a hole line, and its width 0 cannot exceed `k` either way | agree; untested, finding #5 |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which node is a multi-line raw string literal | `RawStringLineUtilities.IsMultiLineRawStringLiteral` (`:23`), called from `RawStringLineShifts.cs:52`, `:148`, `:459` and from the fix `:82`. `rg IsMultiLineRawStringLiteral` finds no other definition and no test caller of the old location. The fix's registration branches (`:230–:240`) and the analyzer's token filter are pre-existing token-level checks that the diff does not touch | one owner, pass |
| Closing and content-line whitespace width | `RawStringLiteralUtilities.GetLeadingWhitespaceLength`, now also called from the guard (`:89`, `:101`). The formatter's spaces-only `GetLeadingSpaceCount` is the follow-up recorded under decision 6 | unchanged since `19-auditor.md`, pass |
| Line starts in a literal's own hole | `StartsInOwnHole` (private, fix). `RawStringLineShifts.IsHoleOpeningLine` decides a different question (whether a line opens a hole) for the formatter, which aligns nested literals itself (FMT-RAW-008) and has no value-preservation guard | one owner, pass |
| Shift of the fix | `GetShift`, used by registration and application | one owner, pass |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 8 withheld tests (plain, `$"""`, `k=0`, whitespace-only `w>k`, each +CRLF where listed; `u8`; depth 2) | No action for the outer literal, one action for the nested literal, both reported | An outer action registered, or a missing nested action or diagnostic | `VerifyOuterLiteralFixIsWithheldAndNestedLiteralFixIsOffered`: `Verify` with exact markup, then `GetCodeFixActionsAsync` on `First()`/`Last()` of the raw literals in document order | adequate. `First()` is the outer literal in every input, `Last()` is the nested raw literal (the regular `$"a{…}b"` of the depth test is not a raw literal) |
| `VerifyFixAllFixesRawStringNestedInHoleFirstAndThenOuterLiteralKeepingNestedValue` (+CRLF) | Release (SHARED-FIX-002), nested relative 30 kept | Value 26 (`main`), or a different iteration count | `Verify` with exact text, 2 incremental and 2 Fix All iterations for document, project, and solution | adequate |
| `k=d` (+CRLF), `w=k` (+CRLF), whitespace-only `w≤k` (+CRLF), rightward with content wider (+CRLF) | Offered, exact single-fix output, nested literal still reported | Withheld, or a different output | `Verify` + `ApplyCodeFixAsync` exact text + `Verify` | adequate |
| Missing: a line of a nested `$"""` that starts in its own hole, wider than `k`, under a leftward shift | The `StartsInOwnHole` branch keeps the fix offered | Fix withheld | — | missing: finding #5 |
| `RawStringLiteralUtilitiesTests` rows 25/26/27/30 | Each leading `char.IsWhiteSpace` counts as one | Wrong length | `DataRow`, exact | adequate; finding #4 closed |

**Visible output (check 10).**

- **Withheld side.** It is covered by maintainer answer Q2 = A together with the unchanged SHARED-FIX-003, because the rewrite would change the nested value.
- **Fix All release output.** The nested fix and then the outer fix are each RH5202-003. The expected values follow from that requirement.
- **Offered-side outputs** (`k=d`, `w=k`, whitespace-only `w≤k`, rightward). These equal `main`'s, because the rewrite and its clamp are unchanged (`19-auditor.md` measured `k=d` on `main`). RH5202-003/004 demand them.

No expected value is undecided, and no rule page changed.

## Failure classes dismissed

- FC-001: the guard inspects every line the rewrite moves within each nested raw literal (*Guard scope*).
- FC-004, FC-005, FC-019, FC-020, FC-021: the guard only decides whether the fix is registered. The rewrite is unchanged, and a directive in a hole is a parse error.
- FC-006, FC-022: the remaining value-change members of #3 are exactly the guarded ones (*Previous findings*). A whitespace-only line with different characters is CS9003, so it is not an input.
- FC-007, FC-009: the guard adds no offered action that cannot change the document. Withheld inputs end with the diagnostic and no fix, which is a supported end state (members (a) and (b) of #6 terminate).
- FC-008: BatchFixer computes each fix on the original document under the same guard. Where the outer fix and a nested fix are both offered, they edit the same line starts. The batch rejects the overlapping change set, so the nested fix runs in a later iteration on the outer fix's result, which is SHARED-FIX-002's sequential semantics. The withheld-then-released path is pinned by the Fix All pair. A nested guard that flips from offered to withheld after the outer fix leaves that diagnostic reported, as SHARED-FIX-002 allows.
- FC-010, FC-011, FC-012, FC-013, FC-028: the formatter changed only by a move of an unchanged predicate (formatter suite green, `22-implementer.md`).
- FC-014, FC-018, FC-023, FC-024, FC-025, FC-026, FC-029, FC-036: no new rule, rename, identifier, member kind, re-resolved token, or key/value layout.
- FC-015, FC-016: see *Policy ownership*. Sibling kinds `u8`, `$$`, and depth 2 hold.
- FC-027: nesting depth and literal kinds are enumerated above.
- FC-030: the fix's edits stay within the literal's line starts.
- FC-031: the summaries of `GetShift`, `ChangesValueOfNestedRawString`, `StartsInOwnHole`, `ApplyCodeFixAsync`, and the `RawStringLineUtilities` class describe the code. The spec text that does not is finding #6.
- FC-032: no compiled change after cc662a9.
- FC-033: CRLF variants exist for each withheld and offered boundary except `u8` and depth 2. The guard reads `SourceText.Lines` and does not branch on line endings or kind.
- FC-034, FC-035: the inputs are the second audit's own counterexample and the spec examples. Expected values come from RH5202-003/006, and the Fix All value was derived before the implementation and was red at 43881b1.
- FC-037: no text-only claim.

## Architecture findings

_None._ The withholding decision has one owner (the fix). The rewrite and the guard share `GetShift`, `GetLeadingWhitespaceLength`, and `SourceText.Lines`. The formatter makes no value-preservation decision for nested raw literals, because it aligns them itself.

## Hints

_None_ beyond the follow-up candidates already in `00-lead.md`.
