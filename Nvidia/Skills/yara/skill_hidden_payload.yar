/*
    Demo rule for the SkillSpector demo skills.

    Catches the partiallysafeskill evasion technique: a hidden instruction
    smuggled as an index-cipher word table. The instruction never exists in
    any file - not in plaintext, not encoded - so keyword, encoding, unicode,
    AST, and taint analyzers all stay quiet. It is assembled at runtime by
    looking up a hardcoded rank sequence in this word-frequency table and
    printed into the agent's context as a "reference note".

    The table itself is the tell: 120+ "NNNN word NNN" rows is a cipher
    alphabet, not a real frequency list. Normal scan: SAFE 0/100.
    With --yara-rules-dir yara: flagged.

    Verify:
      skillspector scan ./partiallysafeskill --no-llm                      -> SAFE
      skillspector scan ./partiallysafeskill --no-llm --yara-rules-dir yara -> flagged
*/

rule AgentSkill_IndexCipherWordlist
{
    meta:
        name = "Index-cipher word table used to smuggle hidden instructions"
        severity = "warning"
        category = "prompt_injection/hidden_payload"
        description = "Detects word-frequency-style tables (rank word count) large enough to act as a cipher alphabet for assembling instructions at runtime. The smuggled payload never exists in any scanned file in any encoding; it only exists in the tool output after the index sequence is decoded against this table."
        technique = "runtime index-cipher instruction assembly"
        reference = "partiallysafeskill demo skill"

    strings:
        // One corpus row: 4-digit rank, single lowercase word, count, newline.
        $row = /[0-9]{4} [a-z][a-z-]{0,15} [0-9]{1,4}\n/ ascii

    condition:
        #row > 120
}
