Role: spec author
Commit: 4df0e39
Verdict: text written

This is the repair round for the two documentation findings in `06-spec-checker.md`. I derived both changes from RH5527-001, RH5530-001, and SHARED-ATTR-006 as they stand, and from the code (`AttributeTargetUtilities.IsAttributeListOnInlineAutoPropertyAccessor`, and `RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer.ResolvePlacementMode`). Neither the prompt nor the checker's suggestion was taken as the wording. Specs, tests, `requirement-ids.txt`, and `sdd/specs/README.md` are unchanged.

## Changed requirements

None. Only rule pages changed:

- **`documentation/rules/RH5530.md`** (finding 1)
  - The Description now names properties, indexers, and events, and `get`, `set`, `init`, `add`, and `remove` accessors, as RH5530-001 does.
  - I removed the Description sentence and the Exceptions text that said every single-line property is "never reported". That is the wider exemption still open in thoenissen/Reihitsu#1029, and no spec states it.
  - The Exceptions section is now a one-item bullet list stating only what SHARED-ATTR-006 states: an accessor attribute on a one-line auto-property or auto-indexer is not reported, and a property attribute on its own line above does not change that.
  - The page now says nothing about single-line declarations whose accessors have bodies or a comment, as the spec does.
- **`documentation/rules/RH5527.md`** (finding 2)
  - Added an Exceptions section with one bullet, the same SHARED-ATTR-006 case: `internal int Value { [return: First] get; set; }` is not reported.
  - I did not add the same exception to RH5501, RH5503, RH5515, RH5517, or RH5519. There it applies only to `assembly:`, `module:`, `property:`, `field:`, or `event:` lists on an accessor, which the C# compiler rejects as attribute locations. That is a corner case the rule-doc conventions say to leave out. The checker found those pages consistent.

Both pages follow `/create-rule-doc`:

- The Exceptions section is a short bullet list.
- The formatter is not mentioned, and nothing from `general-notes.md` is repeated (a comment inside the accessor list is the general comment limitation).
- The footer is unchanged, and so are the Violation and Correction examples.

## Evidence

- Scratchpad harness, both pages' exception shapes on an interface indexer and on a property with a property attribute on the line above:
  - `int this[int i] { [return: A] get; set; }` and `[B]` / `int Value { [return: A] get; set; }` give 0 RH5527 diagnostics.
  - `int this[int i] { [A] get; set; }` and `[B]` / `int Value { [A] get; set; }` give 0 RH5530 diagnostics.
  - The RH5530 `add` case is the verified RH5530-001 example.
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RuleDocumentationConventionTests|FullyQualifiedName~RuleDocumentationExampleTests"`: Passed, 5 of 5.

## Examples for the tests

None. No requirement changed.

## Open questions to file

_None._

Note for the lead: question 1 in thoenissen/Reihitsu#1029 cites the RH5530 page's old *Exceptions* text ("A property whose declaration is single line … is never reported") as documented support for reading (b). That sentence is no longer on the page. The issue may want a comment saying the page now stays silent on that case until the question is decided, and that reading (b) would bring the sentence back.
