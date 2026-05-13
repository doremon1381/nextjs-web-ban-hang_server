---
name: doc-version
description: Apply, bump, and maintain version metadata + changelog on every Markdown document in this repo. Invoke this skill whenever you create or substantially edit any .md file under docs/, the repo root (README.md, CLAUDE.md), or anywhere else in the project. Required policy — do not skip on doc writes.
origin: custom
---

# Document Version Management

Every Markdown document in this repository carries a machine-readable version block and a human-readable changelog. This skill is the single source of truth for that policy.

## When to activate

Activate this skill **before finishing a turn** if any of the following are true:

- You created a new `.md` file in this repo.
- You edited an existing `.md` file with anything beyond a pure typo/formatting fix.
- The user asked you to "rewrite", "revise", "update", or "rework" a doc.
- The user explicitly invoked `/doc-version`.

Do **not** activate for:

- `.md` files inside `node_modules/`, `.next/`, or other generated/vendored paths.
- Files you only read.
- Pure whitespace / lint-style fixes (unless the user explicitly asks for a PATCH bump).

If you are about to write/edit a doc and you are unsure whether this applies — assume it does.

## Convention

### Frontmatter block

The **very first lines** of every doc are a YAML frontmatter block:

```yaml
---
version: 1.0.0
updated: 2026-05-13
status: draft | active | superseded
---
```

- `version` — semantic version string `MAJOR.MINOR.PATCH` (see rules below).
- `updated` — ISO date `YYYY-MM-DD` of the latest change.
- `status` — one of:
  - `draft` — work-in-progress, conclusions may flip
  - `active` — current source of truth
  - `superseded` — kept for history; another doc replaced it (link to it in the body)

The `# Title` heading goes immediately after the closing `---`.

### Changelog section

The **last section** of every doc is `## Changelog`, ordered newest-first:

```markdown
## Changelog

- **1.1.0** (2026-05-13) — Added Phase 0 dependency-audit step; trimmed Auth section.
- **1.0.0** (2026-05-10) — Initial version.
```

Each entry is one line. If a change needs more than one line, the prose belongs in the doc body — the changelog is an index, not a narrative.

### Version rules (semantic for docs)

- **MAJOR** (`X.0.0`) — the doc's purpose, audience, or core conclusions change. A reader who knew the previous version must re-read. Examples: scope rewrite, reversal of a recommendation, switch from "plan" to "post-mortem".
- **MINOR** (`1.X.0`) — a meaningful section is added, a decision is added/changed, scope is extended. A reader who knew the previous version benefits from re-reading the affected section.
- **PATCH** (`1.0.X`) — clarifications, corrections, examples, fixes to stale claims. No decision changes. A reader who knew the previous version does not need to re-read.

When ambiguous, default to **MINOR** and say so in the changelog entry (e.g., "Bumped MINOR by default; consolidated examples.").

New docs start at `1.0.0` if they are an immediate source of truth, or `0.1.0` if explicitly a draft (set `status: draft`).

## Steps to apply

1. **Read** the target doc. Locate any existing frontmatter and `## Changelog` section.
2. **Detect** the current version. If none, treat this as the first versioned write.
3. **Decide** the bump level based on the diff you are about to write (or just wrote):
   - Scope/conclusions changed → MAJOR
   - New section/decision → MINOR
   - Polish/correction only → PATCH
4. **Update** the frontmatter:
   - Bump `version`
   - Set `updated` to today's date in `YYYY-MM-DD` (use the `currentDate` value provided by the harness; do not guess)
   - Adjust `status` if it changed (e.g., `draft` → `active`)
5. **Prepend** a new entry to the `## Changelog` section in the format `- **X.Y.Z** (YYYY-MM-DD) — <one-line summary>.` Keep the summary to one sentence; lead with the verb (Added, Replaced, Removed, Clarified, Fixed).
6. **Verify** the doc still has exactly one frontmatter block at the top and exactly one `## Changelog` section at the bottom. No duplicates.

## Backfilling existing docs

When you first touch a doc that has no frontmatter but carries an inline status note (e.g., the line `> Status: revision 2 (2026-05-13).` near the top of `NEXTJS_MIGRATION_PLAN.md`):

1. Convert the inline note into proper frontmatter. Map the existing revision number → version (e.g., revision 2 → `2.0.0`).
2. Remove the now-redundant inline status note from the body.
3. Seed the changelog with what you can infer from git history or in-doc notes:
   - If a date is mentioned, use it
   - If the date is unknown, write `(date unknown)` for that entry — do NOT fabricate dates
4. Add a new entry for the current change on top.

Backfill is opportunistic — do it on docs you are already editing. Do not preemptively backfill every doc in a single sweep unless the user asks.

## Worked example

A doc currently looks like:

```markdown
# Migration Plan

> Status: revision 2 (2026-05-13). Replaces the earlier 5-week plan…

## 1. Current state
…
```

After a substantial edit that adds a new "Risks" section, the doc becomes:

```markdown
---
version: 2.1.0
updated: 2026-05-13
status: active
---

# Migration Plan

## 1. Current state
…

## 7. Risks
…

## Changelog

- **2.1.0** (2026-05-13) — Added Risks section covering Tailwind v4 pinning and Unsplash rate limits.
- **2.0.0** (2026-05-13) — Replaced the 5-week plan with a trimmed 5-7 day plan; rewrote in English.
- **1.0.0** (date unknown) — Initial Vietnamese-language migration plan.
```

## Hard rules

- Never finish a turn that wrote or edited a `.md` file without leaving its version block and changelog in a valid state.
- Never bump backwards.
- Never edit a past changelog entry to alter history; add a new entry that corrects/supersedes it.
- Never use today's date for entries you did not personally write in this turn.
- Frontmatter is YAML — keys lowercase, no quotes around dates or simple strings.

## If the user disagrees

If the user explicitly tells you to skip versioning for a specific edit, do so for that edit only — do not treat the override as a permanent policy change. The policy itself only changes if the user updates this `SKILL.md` (or memory) directly.
