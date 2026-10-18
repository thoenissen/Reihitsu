# May the RH8020–RH8034 run edit the text of SHARED-DOC-001, SHARED-DOC-002, and SHARED-DOC-005?

| | |
|---|---|
| Date | 2026-10-18 |
| Escalation | none of 1–10; asked under the run instruction not to change another spec's existing requirement without approval |
| Run | issue #983 |

## Question

Specifying RH8020–RH8029 needs two edits to existing shared requirements, with behavior and IDs unchanged. SHARED-DOC-001 and SHARED-DOC-005 would exclude from their coverage every partial element declared in several parts (type, constructor, method, property, indexer, event), not only types and constructors, because today's per-part verdict for partial members contradicts principle P8 and is filed as an open question instead. SHARED-DOC-002 would add the extension block to the containers in which a member without an access modifier is private, with an RH8021 example. Options: keep all edits; revert them and state the coverage and the extension-block group in each rule spec; or keep only the coverage edits.

## Answer

Keep all edits.

## Reason

None given.
