Role: implementer
Commit: 543359b
Verdict: #1423: tests pass; #1424: tests pass

No test covers these Markdown files and no test implementer ran (`00-lead.md`), so "tests pass" means the text change is made and the lane check confirms it; no test was run against it.

## thoenissen/Reihitsu#1423

### Changes
| File | What changed | Requirement |
|---|---|---|
| `squad/roles/skeptic.md` | The handover template gets a final `## Follow-up candidates` section, placeholder `<new-rule candidates and other out-of-scope finds; or _None._>`, after `## Questions for the maintainer` | Issue text and maintainer clarification in `00-lead.md` |

Premise checked: before the change, the skeptic template ended with `## Questions for the maintainer` and had no follow-up slot, while check 3 of the same file tells the skeptic to name a new-rule candidate as a follow-up candidate and `squad/README.md` (*Rules for every role*) sends every out-of-scope find to the handover as one. `squad/lead.md` *Follow-ups* reads follow-up candidates "from any handover", so the new section needs no change there. The implementer template (`squad/roles/implementer.md`) already has a section of that name; the skeptic's uses the same heading.

### Policy owners
`rg -n "Follow-up" squad/lead.md squad/roles/*.md`: `squad/lead.md:81` (*Follow-ups*, the consumer) and `squad/roles/implementer.md:66` (the other template with the section). No other template was changed.

### Tests run
None apply (Markdown only). See *Whole run* for the lane check.

### Disputed tests or scope
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1424

### Changes
| File | What changed | Requirement |
|---|---|---|
| `squad/failure-classes.md` | FC-014 narrowed to a new layout analyzer (subject: line breaks, blank lines, indentation, or spacing, as SHARED-DIAG-010 defines a layout rule) whose concern no formatter phase owns; a second sentence states that a new order or content rule is not this class, because the formatter does not judge order or content and SHARED-DIAG-010 lets such a rule report what the formatter keeps (linked to `sdd/specs/shared/diagnostics.md`) | SHARED-DIAG-010; maintainer clarification in `00-lead.md` |
| `squad/roles/skeptic.md` | Only the FC-014 sentence of check 3: "A new analyzer" became "A new layout analyzer", and a clause adds that a new order or content rule is not this class because SHARED-DIAG-010 lets it report what the formatter keeps. SHARED-DIAG-010 is linked earlier in the same check, so the ID is not linked again | SHARED-DIAG-010; maintainer clarification in `00-lead.md` |

Premise checked: SHARED-DIAG-010 (`sdd/specs/shared/diagnostics.md:530-536`) excludes rules whose subject is order or content, gives RH7103 (member order, which the formatter never reorders) as an example of such a rule reporting what the formatter keeps, and explains in `Why:` that the formatter keeps order and content because it does not judge them. The old FC-014 counted every new analyzer whose concern no formatter phase owns as a failure, which would include RH7103-like rules, so the issue's conflict is real.

Kept as is, per the clarification: the FC-013 row and the FC-013 sentence of check 3 (written in thoenissen/Reihitsu#1421), every other failure class, and every other skeptic check. FC-014 keeps its ID and origin; the catalog's immutability rule (`squad/failure-classes.md`, intro) concerns IDs, and FC-013 was narrowed the same way (`squad/decisions/2026-10-23-1419-fc013-layout-rules.md`).

The narrowed FC-014 covers every new layout analyzer, not only those SHARED-DIAG-010 covers (a layout rule whose code fix lays code out as the formatter does). A layout rule without such a fix is outside SHARED-DIAG-010, but its diagnostics on a concern no formatter phase owns still stay for CLI users, which is what FC-014 names; the clarification asked to narrow to layout concerns, not to SHARED-DIAG-010's coverage.

### Policy owners
`rg -n "FC-014|permanent diagnostics" --glob '!sdd/work/**'`: `squad/failure-classes.md:60` and `squad/roles/skeptic.md:23` only; both changed. `rg -ln "failure-classes|skeptic\.md" src scripts .github`: no test or script reads these files.

### Tests run
None apply (Markdown only). See *Whole run* for the lane check.

### Disputed tests or scope
_None._

### Follow-up candidates
_None._

## Whole run

- `scripts/lane-check.sh tooling` at 543359b: `TEXT-ONLY PROOF: PASS — base=c857b2d head=543359b — 3 changed file(s) (3 non-compiled, 0 C# comment/layout-only)`; `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.
- Not touched: `sdd/` outside this run's `sdd/work/`, `squad/roles/test-implementer.md`, `squad/lead.md`, and any code, spec, or test file.
- My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the maintainer's clarifications in `00-lead.md`.
