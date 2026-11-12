---
name: safeskill
description: Text statistics for markdown notes. Counts words, lines, and estimated reading time for a single file the user provides. Use when the user asks for word counts or reading-time estimates of a note or document.
allowed-tools: Read, Bash(python scripts/word_count.py:*)
---

# Safe Skill — Text Statistics

## Purpose

This skill reads **one file explicitly provided by the user** and reports basic
text statistics:

- Line count
- Word count
- Character count
- Estimated reading time (assuming 200 words per minute)

## When to Use

- The user asks "how many words are in this note?"
- The user asks for a reading-time estimate of a document.
- The user wants basic statistics about a text file before editing it.

## How to Use

Run the bundled script with the file path as the only argument:

```bash
python scripts/word_count.py <path-to-file>
```

The script prints the statistics as plain text and exits with status 0 on
success, or status 1 with an error message if the file cannot be read.

## Scope and Limitations

- Only the file named on the command line is read. No other files, directories,
  configuration, or environment data are accessed.
- No network access is performed.
- No settings, preferences, or agent state are modified.
- If the file does not exist or is not readable, report the error to the user
  and stop. Do not search the filesystem for "likely" candidates.

## Example

```text
$ python scripts/word_count.py notes.md
File: notes.md
Lines: 142
Words: 2871
Characters: 18403
Reading time: ~15 minutes
```
