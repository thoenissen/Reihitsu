Role: spec author
Commit: fa14973
Verdict: text written

Both findings of `10-spec-checker.md` were checked by running the shipped analyzers with my scratch probe, before anything was changed.

Afterwards:
- The RH7311 example blocks still reproduce: 2 blocks, 0 mismatches.
- `SpecConsistencyTests` and `SpecStructureTests` pass (26 tests).
- `RuleDocumentationConventionTests` passes (3 tests).
- No requirement ID changed, and no protected file was edited. The open questions stay out of the specs.

| # | Re-derived | Change |
|---|---|---|
| 1 — `general-notes.md`, the rules that check region directives in excluded code | **RH7310:** an empty region wholly inside `#if false`, with no blank line between its directives, is reported. With a blank line between them it is not, because that blank line is excluded text, which counts as content. So RH7310 does check such directives; the skeptic's earlier A10 input happened to contain a blank line. **RH7304:** not reported in any of three inputs (empty, blank line, excluded member), because excluded code holds no token to align with. The directive is still examined, so the input does not decide whether RH7304 "checks" it. **RH7309 and RH7311** count regions in excluded code as well (my earlier probe). A complete list would therefore be long, and part of it undecidable. | The sentence no longer names rules: "The `#region` and `#endregion` directives inside such a branch are an exception: the compiler still reads them, so several of the region rules check them there as well." |
| 2 — RH7311-002, "a region of a nested type" | Yes. A nested enum's `#region Values` that repeats a class region's description is reported. This is open question Q2 of thoenissen/Reihitsu#1187. | The sentence now names the kinds: "a region of a nested class, struct, interface, record, or record struct". The nested enum falls outside it. |
