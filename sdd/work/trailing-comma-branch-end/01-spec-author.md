Role: spec author
Commit: e5edbfd99ead625e609d53e2ef5ee7bb4e5840c4
Verdict: change doc written

The prompt carried no intended fix or verdict beyond the issue's own option A; the spec diff was written for option A as the lead asked, and the change doc stays `Draft` until the maintainer answers.

Premise checked on `main` with `scripts/apply-fix.sh` (no symbols defined): RH5409/RH5410/RH5411 report the comma in front of `#endif`, `#else`, `#elif` with `no-fix-offered` (8 fixtures, LF and CRLF); `scripts/format.sh` keeps every such comma. Reached by the three code fixes, by whole-document formatting (CLI), and by RH5303's node-level formatting of a collection initializer (probed: its fix today writes `3,` in front of `#endif`; after A it writes `3`).

## Changed requirements

- SHARED-TRAIL-002 — A comma in front of a conditional directive is reported, and an `#if` behind it withholds the fix — changed (only an `#if` in the gap, read or excluded, withholds the fix; the "gap" qualifier is defined as the comma's trailing trivia plus the closing brace's leading trivia, with a side-example of an `#if` in front of the comma)
- SHARED-TRAIL-004 — The fix removes a comma whose branch ends behind it — added (ledger updated)
- FMT-STRUCT-019 — Enums have no trailing comma — changed (keeps the comma only for an `#if` in the gap; new `enum Level` example with `#else`)
- FMT-STRUCT-020 — no text change; follows FMT-STRUCT-019 by its "same trivia and directive rules", so its output changes too
- SHARED-DIAG-008 — clarified: its example clause now names "the comma in front of an `#if` directive" instead of "a conditional directive"
- Change doc: `sdd/changes/2026-11/trailing-comma-branch-end.md` (Status: Draft)

`scripts/test.sh --project architecture`: 197 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`: "SHARED-TRAIL-004: no test covers it". Expected; the test implementer's tests tag it.

## Examples for the tests

- SHARED-TRAIL-004: `#endif` in a DEBUG build (fix removes `B,`); `#else` (removes `C,`, excluded `B,` stays); `#elif` (removes `B,`); plus Fix All over two enums (change doc).
- SHARED-TRAIL-002: `#endif` followed by `#if Y` → no fix; `#if Y` inside code an `#else` excludes → no fix; comma before `#if` → no fix; `#region` → fix.
- FMT-STRUCT-019: `enum Level` (`#else` → `High`), `enum Mode` (comma before `#if` kept).
- FMT-STRUCT-020: array (`#if !X` / `2,` / `#endif` → `2`) and collection initializer (`#else` → `3`) from the change doc.
- RH5303 code fix: the change-doc input (`new List<int> { 1, #if X 2, #else 3, #endif }`) — after A, `3` without comma.
- Every example in the change doc was run through today's code: the before outputs are what `apply-fix.sh`/`format.sh` produce on `main`; the after outputs are today's output minus the one comma.

## Output questions

The issue's question (A or B) is the only one; the lead asks it. Refined from the run's evidence:

1. **The CLI does not change the issue's own example.** `reihitsu-format` formats with no symbol defined, so in `A, #if DEBUG B, #endif` the `B,` is disabled text and stays; only the comma behind `A` (in front of `#if`) is a token, and it stays. The formatter output changes on the CLI only for a comma in an active branch, e.g.:

```csharp
enum Level
{
    Low,
#if DEBUG
    Trace,
#else
    High,
#endif
}
```
Before: unchanged. After A: `High` loses its comma; `Trace,` stays. The issue's example (DEBUG defined) changes through the RH5409 fix in a project that defines `DEBUG`.

2. **A also changes RH5303's fix result** (and possibly RH5301/RH5304): see the change doc's *Code fixes that format an initializer*; before `3,`, after A `3`.

3. **Nested `#if` in an excluded branch** (`B, #else #if Y C, #endif #endif`): A as the issue words it ("no `#if` between the comma and the closing brace") withholds the fix here; the change doc lists this under "no change". It is safe to fix in every build, but recognizing that needs directive pairing, not a kind check, so withholding is the recommendation within A.

Recommendation: A, weakly, as the issue says — safe, cheap (a directive-kind check replacing `ContainsConditionalCompilationBoundary` in three call sites), small gain. B costs nothing.

## Internal choices

- Split into SHARED-TRAIL-002 (report + withhold for `#if`) and a new SHARED-TRAIL-004 (offer + remove for `#elif`/`#else`/`#endif` only), rather than one changed requirement — one behavior per heading. Alternative: fold both into 002.
- SHARED-DIAG-008's clause edited in place without an ID reference, so its `Uses` row is unchanged.
- FMT-STRUCT-020 left textually unchanged (it defers to FMT-STRUCT-019). Alternative: add an initializer example to it; the test implementer can take the change doc's.
- Rule pages RH5409/RH5410/RH5411 unchanged: a withheld fix in a corner case is not on the page.

## Follow-up candidates

_None._

Notes for the implementer stage (not spec): the three call sites of `SyntaxTriviaUtilities.ContainsConditionalCompilationBoundary` for trailing commas (`TrailingCommaCodeFixHelper.CommaPrecedesConditionalCompilationBoundary`, `EnumTrailingCommaRemovalTransform`, `InitializerTrailingCommaRemovalTransform`) are where the narrower guard goes; disabled text in the gap no longer withholds by itself. The RH5301/RH5304 claim in the change doc was not probed with a fixture; the skeptic or test implementer should confirm or drop it.
