Role: spec checker
Commit: 07f1642
Verdict: consistent

Re-check after the repair round (`git diff 4df0e39 HEAD`): two rule pages and the handovers `07-spec-author.md` and `08-cleaner.md` changed. No spec, test, ledger, or index file changed. The prompt's note on the RH5530 single-line exemption was treated as context, not as a verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
- Finding 1 of `06-spec-checker.md`: resolved. The RH5530 Description now names properties, indexers, and events and the `get`, `set`, `init`, `add`, and `remove` accessors, as RH5530-001 does.
- Finding 2 of `06-spec-checker.md`: resolved. `documentation/rules/RH5527.md` has an Exceptions bullet for a `[return: …]` list on an accessor of a one-line auto-property or auto-indexer, which is what RH5527-001 and SHARED-ATTR-006 state. The shape is a property or indexer, so the page matches SHARED-ATTR-006, which names no events.
- RH5530 single-line exemption: the page does not contradict any requirement. RH5530-001 requires a report only when the owner spans more than one line apart from its own attribute lists, and SHARED-ATTR-006 exempts the inline auto-property shape. The page's "single line is never reported" is true of the shapes the spec states. It also covers single-line declarations with accessor bodies or a comment, which the spec leaves unstated while thoenissen/Reihitsu#1029 is open. The page therefore adds to the spec without conflicting with it.
- Rule-page conventions: both pages keep a short Exceptions bullet list, mention no formatter, repeat nothing from `general-notes.md`, and keep the footer and the Violation and Correction examples.
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RuleDocumentationConventionTests|FullyQualifiedName~RuleDocumentationExampleTests"` -> "Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5".
- `scripts/test.sh --project architecture` -> "Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72".
- Nothing left behind: `sdd/work/` holds only `attribute-placement-specs/`, which the lead deletes before publishing. The working tree is clean.
