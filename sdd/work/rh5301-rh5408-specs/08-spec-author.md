Role: spec author
Commit: 2f1c4705aa128ae7e2760c2baa8167b5a25ab34b
Verdict: text written

This is the repair for Finding 1 of `07-spec-checker.md`.

## Finding 1 — applied

I re-derived the finding:

- The `## Description` of `documentation/rules/RH5404.md` said that the opening brace, the members, and the closing brace each appear on their own line, for every kind of type, enums included.
- RH5404-003 and the page's own "How to fix it" section say that an enum's members move between the braces but keep their layout. The fix result `enum E⏎{⏎    One, Two⏎}` confirms this; it was verified in round 03.

I replaced the sentence with:

> The opening brace and the closing brace must each appear on their own line, and so must each member of a class, struct, interface, record, or record struct; the members of an enum may stay together on one line between the braces.

Nothing else on the page changed.

## Changed requirements

_None_: the change is to the rule page only. No spec, ID, or index changed.

## Checks

`scripts/test.sh --project analyzer --filter "FullyQualifiedName~RuleDocumentationConventionTests|FullyQualifiedName~RuleDocumentationExampleTests"`: 5 passed.

## Open questions to file

Unchanged from `03-spec-author.md`.

## Decisions needed

_None._
