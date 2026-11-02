# 15 Cleaner — F4 doc summary of TokenGapNormalizer.EnsureTokenStartsLine

Rewrote the summary: it now says the method moves a token onto its own line when its line starts behind other content (the previous token or the last line of a multi-line comment), leaves it when the line already starts behind a `//`/`///` comment, directive, or end-of-line, and places the break in front of the first content glued to the token. Wrapped like the surrounding lines (120 columns). No code changed.

Proof: `--strict-docs` FAILS only because the file's public API documentation changed (the flag treats any public doc change as behavioral); the plain run is the real text-only check. Format: 0 of 1 files changed. Release build: 0 warnings, 0 errors.
