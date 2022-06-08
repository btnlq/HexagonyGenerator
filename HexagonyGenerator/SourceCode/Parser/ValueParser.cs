namespace HexagonyGenerator.SourceCode.Parser;

static class ValueParser
{
    public const string PrefixChars = "xXoObB";

    public delegate bool DigitChecker(int c);

    public static bool IsBinDigit(int c) => (uint)(c - '0') < 2;
    public static bool IsOctDigit(int c) => (uint)(c - '0') < 8;
    public static bool IsDecDigit(int c) => (uint)(c - '0') < 10;
    public static bool IsHexDigit(int c) => c <= 'f' && System.Uri.IsHexDigit((char)c);

    private static readonly DigitChecker[] _digitCheckers = { IsHexDigit, IsOctDigit, IsBinDigit };
    public static DigitChecker GetDigitChecker(char c) => _digitCheckers[PrefixChars.IndexOf(c) >> 1];

    public static Value Parse(string text)
    {
        if (text.Length < 2 || text[1] <= '9')
            return Value.Parse(text, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture);

        int mode = PrefixChars.IndexOf(text[1]) >> 1;
        int length = text.Length - 2;
        byte[] bytes;

        if (mode == 2) // bin
        {
            bytes = new byte[length + 7 >> 3];
            for (int i = 0; i < length; i++)
            {
                int digit = text[length - i + 1] - '0';
                bytes[i >> 3] |= (byte)(digit << (i & 7));
            }
        }
        else if (mode == 1) // oct
        {
            bytes = new byte[length * 3 + 7 >> 3];
            for (int i = 0; i < length; i++)
            {
                int digit = text[length - i + 1] - '0';
                int pos = i * 3;
                bytes[pos >> 3] |= (byte)(digit << (pos & 7));
                if ((pos & 7) >= 6)
                    bytes[(pos >> 3) + 1] |= (byte)(digit >> 2 - (pos & 1));
            }
        }
        else // hex
        {
            bytes = new byte[length + 1 >> 1];
            for (int i = 0; i < length; i++)
            {
                int digit = System.Uri.FromHex(text[length - i + 1]);
                bytes[i >> 1] |= (byte)(digit << ((i & 1) << 2));
            }
        }

        var value = new Value(bytes, true);
        return value;
    }
}
