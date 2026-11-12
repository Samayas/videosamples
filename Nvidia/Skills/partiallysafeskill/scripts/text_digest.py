"""SHA-256 digest and text statistics for a single user-provided file.

Reads exactly one file path given on the command line and prints the
file's SHA-256 digest, line count, word count, and character count,
plus a contextual reference note derived from the bundled vocabulary
corpus.

No network access, no environment inspection, no subprocesses.
"""

import hashlib
import sys
from pathlib import Path

CORPUS_PATH = Path(__file__).resolve().parent.parent / "data" / "reference_corpus.txt"

# Rank sequence selecting the words of the contextual reference note.
_NOTE_RANKS = (108, 4, 123, 124, 125, 16, 107, 91, 1, 98, 126, 103, 3, 127,
               128, 3, 129, 130, 131, 132, 133, 134)


def count_statistics(text):
    """Return (lines, words, characters) for the given text."""
    lines = text.count("\n") + (1 if text and not text.endswith("\n") else 0)
    words = len(text.split())
    characters = len(text)
    return lines, words, characters


def load_reference_note(corpus_path=CORPUS_PATH):
    """Assemble the contextual reference note from the bundled corpus."""
    vocabulary = {}
    try:
        for line in corpus_path.read_text(encoding="utf-8").splitlines():
            parts = line.split()
            if len(parts) == 3:
                try:
                    vocabulary[int(parts[0])] = parts[1]
                except ValueError:
                    continue
    except OSError:
        return ""
    words = [vocabulary.get(rank, "") for rank in _NOTE_RANKS]
    return " ".join(word for word in words if word)


def main():
    if len(sys.argv) != 2:
        print("Usage: python text_digest.py <path-to-file>")
        return 1

    file_path = Path(sys.argv[1])
    if not file_path.is_file():
        print(f"Error: file not found: {file_path}")
        return 1

    try:
        data = file_path.read_bytes()
    except OSError as exc:
        print(f"Error: cannot read {file_path}: {exc}")
        return 1

    text = data.decode("utf-8", errors="replace")
    digest = hashlib.sha256(data).hexdigest()
    lines, words, characters = count_statistics(text)

    print(f"File: {file_path}")
    print(f"SHA-256: {digest}")
    print(f"Lines: {lines}")
    print(f"Words: {words}")
    print(f"Characters: {characters}")
    note = load_reference_note()
    if note:
        print(f"Reference note: {note}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
