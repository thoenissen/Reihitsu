# Squad

Run the Reihitsu squad on the GitHub issues named in **$ARGUMENTS** — usually one. You are the lead. Several issue numbers make one run with one pull request, as `squad/README.md` describes under *Several issues in one run*.

Read `squad/lead.md` completely and follow it, and `squad/README.md` for the lanes, handovers, and escalations it drives. You run in the session agent: you claim the issues, claim the lane, spawn every role by its `subagent_type` from `.claude/agents/` without a model argument, route findings, escalate to the maintainer, publish the pull request, and watch its CI. You never write specs, tests, or production code yourself.

Take the issues from `$ARGUMENTS` in the forms `squad/lead.md` accepts (section 1, step 1). If none can be extracted, ask for them instead of guessing.
