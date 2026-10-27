# Code fixes are withheld where they would separate comments and directives from their code or change the program

| | |
|---|---|
| Status | Draft |
| Date | 2026-10 |
| Affected requirements | SHARED-FIX-003, SHARED-MODORDER-003, RH7004-004, RH7101-004, RH7109-005, RH7109-006, RH7306-003, RH7309-004, RH7501-004, RH7101-006, RH7303-005, RH7306-004 and RH7309-006 (added by this change) |

## What

SHARED-FIX-003 lists what a code fix must never do. It gains three conditions, and the organization fixes that violated them are withheld for the inputs concerned. The diagnostic is still reported in every case.

- **Comments and directives stay with their code.** A fix must not move a comment away from the code it stands with, and must not reorder two comments that stand with the same code. A comment behind code at the end of its line stands with that code. A comment that starts its line stands with the code or directive line below it, when no blank line lies between them. A fix also must not change which code a directive governs. A conditional or region directive governs the code between it and its partner. Any other directive governs the code from its line to the directive that ends its effect. A directive pair counts as split across a brace when the fix moves one directive across a brace while its partner stays, or adds a pair of braces with one brace between the two directives and the other outside them. A pair of braces added wholly between the two directives, or wholly outside them, does not split the pair.
- **Disabled text is content.** The text of code that an `#if` branch excludes must not change, and removing a comment, a directive, or disabled text counts as changing it. A fix whose diagnostic reports a whole region on its `#region`, as RH7310 does, still removes that region's `#endregion` with it.
- **Every build configuration counts.** A compile error or a run-time change that only a build with or without a conditional compilation symbol would show withholds the fix too.

The run-time condition is clarified as well: the instance fields of a layout type include the compiler's backing fields of auto-properties, of properties that use `field`, and of instance field-like events.

The rule specs state what this means for each fix:

| Rule | Withheld or changed | Requirement |
|---|---|---|
| RH7004 | A comment behind the using declaration stays behind the header of the `using` statement instead of moving behind the new block. A directive pair that the new block would split is a SHARED-FIX-003 case. | RH7004-004 |
| RH7101 | Withheld for a declaration with a `///` or `/** */` documentation comment in documentation mode `Parse` or `Diagnose`. In mode `None` the fix still splits. | RH7101-004, RH7101-006 |
| RH7105, RH7106 | Withheld when a comment or a directive stands between the last modifier and the token after it. | SHARED-MODORDER-003 |
| RH7109 | Withheld when the move reorders the instance fields of a struct without `LayoutKind.Auto` or `LayoutKind.Explicit`, or of a type with `[StructLayout(LayoutKind.Sequential)]`. | RH7109-005, RH7109-006 |
| RH7303 | Withheld for a reported directive whose partner lies between declarations. | RH7303-005 |
| RH7306 | The partner directive is judged by its description, as the analyzer judges it. Withheld when the description of the reported directive, or of the partner it would edit, is the word `implementation` alone. | RH7306-003, RH7306-004 |
| RH7309 | Withheld when the reorder would swap two runtime initializers of one context, swap two instance fields of a layout type, move a block away from a comment above its `#region` line (also across blank lines), or hand a documentation comment to another declaration in mode `Parse` or `Diagnose`. A comment above a block that keeps its place does not withhold it. | RH7309-004, RH7309-006 |
| RH7501 | Withheld when the `break` shares its line with other code, when the block's closing brace shares its line with the `break`, or when a directive or excluded code lies between the token in front of the `break` and the closing brace. A `break` that is the block's only statement is still moved, and the block it leaves empty stays. | RH7501-004 |

## Why

`AGENTS.md` (*Code-fix coverage*) already calls a fix unsafe when it drops, reorders, or relocates comments, directives, disabled text, or literals. It points to SHARED-FIX-003 for what that means, but SHARED-FIX-003 covered only changed text, not placement, and said nothing about disabled text. Without that wording, several organization fixes produced code that did not compile, compiled differently in another build configuration, changed initializer order or a struct's memory layout, took documentation from a declaration, or left a comment or a `#pragma` acting on other code. Their rule specs often still required the rewrite. A diagnostic without a fix is a supported end state; a fix that hands the user such a defect is not.

## Decisions

### One shared condition, with each rule's instances in its own spec

The placement, disabled-text, and configuration rules are stated once in SHARED-FIX-003. A rule spec names its own instances as "withheld" requirements with examples, and an "unless withheld" in the requirement that states the rewrite, so no rule spec reads as requiring a rewrite that SHARED-FIX-003 forbids. The layout guard of RH7109 is not stated again in `shared/ordering-move-safety.md`. RH7309-006 takes its runtime-initializer definition from SHARED-ORDER-007.

### The brace split follows the maintainer's clarification

A pair counts as split when one directive moves across a brace or one new brace lands between the two directives. Braces added wholly inside a governed span are not a split. Counting them would withhold every brace or expression-body fix inside a `#region`, although the code each directive governs stays the same.

### Comment placement is decided only for the shapes it names

The condition decides a comment at the end of a line of code and a comment that directly heads the next line. A comment that a blank line separates from the code below it, and a comment between two tokens of one line, are left to each rule. RH7309 decides its own case: a comment above a `#region` heads that block, also across blank lines.

### Narrower questions answered by the maintainer

- Backing fields count as fields of a layout type.
- The RH7303 mirror shape (`#region` in a body, `#endregion` between declarations) is withheld like the reported shape.
- A comment above an RH7309 block that keeps its place does not withhold the fix.
- RH7306 is withheld when the partner directive would be left with an empty description.
- A comment behind the last modifier withholds the RH7105 and RH7106 fixes like a directive.
- RH7501 keeps the empty block that a lone `break` leaves.

### Out of this change

- The fate of a blank line in front of the `break` of RH7501 is decided by a separate change.
- RH7309 regions that the region helper counts although they belong to a nested enum or lie inside an expression are left to a follow-up.
- Fixes outside these rules are not audited against the new conditions here. A fix that violates them is a bug against SHARED-FIX-003.

## Alternatives considered

- **Keep the fixes and move the comment or directive somewhere plausible** (the RH7309 comment above the moved block, the RH7101 documentation copied to every field, the RH7501 `break` laid out on a new line) — rejected: where a comment or documentation belongs is the author's decision, and "prefer no fix" applies.
- **Treat every brace added between two directives as a split** — rejected: it withholds common, safe fixes inside regions and `#pragma` spans.
- **Withhold RH7309 whenever any block has a comment above it** — rejected: it gives away safe ground, because nothing is relocated when the block keeps its place.
- **Count only declared fields for the layout guard** — rejected: a backing field's offset changes exactly like a declared field's.
- **Offer RH7303 for the mirror shape and RH7306 with an empty `#endregion //`** — rejected: one drops an unreported directive, the other leaves a nameless directive.
- **Fix the RH7309 region selection in the shared helper in this change** — rejected for now: it is a change to a helper with many consumers and a gap in SHARED-TYPEREGION-005, both outside the fixes this change is about.
