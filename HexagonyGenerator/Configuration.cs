namespace HexagonyGenerator;

enum ValueEncoding
{
    /// <summary>
    /// Use only ASCII commands to generate big numbers.
    /// </summary>
    Bytes,
    /// <summary>
    /// Generate big numbers with unicode symbols.
    /// </summary>
    Chars,
    /// <summary>
    /// Same as `Chars` but also replace commands like "put(10)" (requires 3 commands)
    /// with "put(266)" (requires only 2 commands, but one of them is non-ASCII).
    /// </summary>
    ForceChars,
}

/*
    -- Code --  -------- Hexagony --------
                Bytes   Chars   ForceChars
    x = 7832;   N32     ẘ       ẘ          (N is 78, ẘ is 7832)
    put(10);    0;     0;     Ċ;         ( is 1, Ċ is 266)
*/