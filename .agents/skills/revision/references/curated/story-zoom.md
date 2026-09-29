# Story Zoom

Curated from a previously merged skill. This file keeps the useful task guidance without preserving the old skill as an active nested skill.

## Extracted Resources

- `scripts/story-zoom/`
- `templates/story-zoom/`

## Guidance

### From `story-zoom-skill.md`

_Source topic: story-zoom_

**Purpose:** Manage multi-level story synchronization. Use when changes at one abstraction level (pitch, structure, scenes, entities, prose) need to propagate to others, or when story elements feel inconsistent across levels.

# Story-Zoom: Multi-Level Fiction Synchronization

You manage consistency across abstraction levels in fiction projects. Your role is to detect when changes at one level have created inconsistencies at other levels, and help the writer decide how to resolve them.

## Core Principle

**Every story element exists at multiple abstraction levels simultaneously. Consistency across levels is what makes stories feel coherent.**

A character's "lie" (from character-arc) must manifest in their dialogue (scene-level), must connect to theme (story-level), and must appear in their synopsis (pitch-level). When any level changes, the others must either update or be flagged as potentially out-of-sync.

## The Abstraction Levels

| Level | Name | Directory | Artifacts | Grain |
|-------|------|-----------|-----------|-------|
| L1 | Pitch | `pitch/` | tagline.md, logline.md, synopsis.md | Story essence |
| L2 | Structure | `structure/` | outline.md, beats.md, act-*.md | Story skeleton |
| L3 | Scenes | `scenes/` | scene-*.md, chapter-*.md | Story rhythm |
| L4 | Entities | `entities/` | characters/, locations/, items/, timeline.md | Story elements |
| L5 | Manuscript | `manuscript/` | chapter-*.md (actual prose) | Story surface |

## Architecture: Dumb Logger + Smart LLM

This skill works with a simple file watcher daemon that logs changes. The daemon does NO semantic understanding - it just records what files changed and when.

**You do the thinking.** When invoked, you:
1. Read the change log since last review
2. Read the changed files
3. Find related files (via wiki-links, directory structure, explicit references)
4. Use your understanding of narrative to identify what's now inconsistent
5. Propose resolutions for the writer to approve

### Why This Architecture?

## The States

### State Z1: No Story State (Cold Start)
...

# JSON output
deno run --allow-read scripts/status.ts ./story-project --json
```

## Example Interaction

**Writer:** "I've been drafting for two weeks and haven't looked at my outline. Can you check if things are still aligned?"

**Your approach:**

1. **Diagnose state:** This is Z2 (Siloed Work) - focused on one level without checking others.

2. **Read change log:** Identify which files in `manuscript/` changed over two weeks.

3. **Read changed files:** Understand what was written.

4. **Compare to structure:** Read `structure/outline.md` and relevant scene files.

5. **Identify drift:**
   - "Chapter 7 introduces a subplot that isn't in your outline"
...
```
```

**Tools for persistence:**
- `init.ts` - Creates story-state structure for a project
- `watcher.ts` - Daemon that logs file changes
- `status.ts` - Generates current state dashboard

### How It Differs from Standard Output Persistence

Story-zoom maintains **operational state tracking**, not exploration output. The `story-state/` directory is a working tool, not a record of sessions.

**This skill does NOT use `context/output-config.md`** because:
- Location is determined by `init.ts` during project setup
- State files are operational (read/write continuously)
- The watcher daemon needs a fixed known location

### Conversation vs. File

| Goes to File | Stays in Conversation |
...
```json
...


### From `MERGED.md`

_Source topic: MERGED_

# Merged skill: story-zoom
Source path: `skills/writing-craft/story-zoom`
Canonical skill: `skills/writing-craft/revision`

### From `README.md`

_Source topic: README_

# Story-Zoom Templates
Templates for initializing story-state in a new project.
## story-state/
The `story-state/` directory is created by `init.ts` and contains:
- `state.md` - Dashboard showing sync status across levels (LLM-maintained)
- `change-log.jsonl` - Append-only log of file changes (daemon-maintained)
- `last-review.json` - Timestamp of last LLM review
## Standard Story Directory Structure
```
story-project/
├── story-state/          # Auto-managed
│   ├── state.md          # Dashboard
│   ├── change-log.jsonl  # Change log
│   └── last-review.json  # Last review timestamp
├── pitch/                # L1: High-level story docs
│   ├── tagline.md
│   ├── logline.md
│   └── synopsis.md
├── structure/            # L2: Story skeleton
│   ├── outline.md
│   ├── beats.md
│   └── act-1.md, act-2.md, act-3.md
├── scenes/               # L3: Scene breakdowns
│   └── scene-01.md, scene-02.md, ...
├── entities/             # L4: Story elements
│   ├── characters/
│   │   └── protagonist.md, antagonist.md, ...
│   ├── locations/
│   │   └── main-setting.md, ...
│   └── items/
│       └── macguffin.md, ...
└── manuscript/           # L5: Actual prose
    └── chapter-01.md, chapter-02.md, ...
```
## Wiki-Link Convention
Reference entities using wiki-links: `[[entity-name]]`
The entity name should match the filename (without .md extension):
- `[[protagonist]]` → `entities/characters/protagonist.md`
- `[[main-setting]]` → `entities/locations/main-setting.md`
These create implicit bindings that story-zoom uses to find related files.

### From `state.md`

_Source topic: state_

# Story State: [Project Name]

**Last Review:** [timestamp]
**Health:** [Green/Yellow/Red]

## Level Summary

| Level | Directory | Files | Status | Notes |
|-------|-----------|-------|--------|-------|
| L1 Pitch | pitch/ | 0 | - | Tagline, logline, synopsis |
| L2 Structure | structure/ | 0 | - | Outline, beats, acts |
| L3 Scenes | scenes/ | 0 | - | Scene breakdowns |
| L4 Entities | entities/ | 0 | - | Characters, locations, items |
| L5 Manuscript | manuscript/ | 0 | - | Actual prose |

## Active Concerns

## Pending Propagations

## Recent Resolutions

- [date] Initialized story-state tracking

## Notes

Use `/story-zoom review` to analyze changes and update this dashboard.
