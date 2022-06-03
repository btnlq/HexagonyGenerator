namespace HexagonyGenerator.Hexagony.Interpreter;

using System.Text;

class Reader
{
    private readonly byte[] _input;
    private int _inputPos;

    public Reader(string input)
    {
        _input = Encoding.UTF8.GetBytes(input);
    }

    public Value ReadByte()
    {
        return _inputPos < _input.Length ? (Value)_input[_inputPos++] : Value.MinusOne;
    }

    private static bool IsDigit(byte b) => (b ^ 48) < 10;

    public Value ReadInt()
    {
        int start;

        while (true)
        {
            if (_inputPos == _input.Length)
                return Value.Zero;

            byte b = _input[_inputPos++];
            if (b == '-' || b == '+' || IsDigit(b))
            {
                start = _inputPos - 1;
                break;
            }
        }

        while (_inputPos < _input.Length && IsDigit(_input[_inputPos]))
            _inputPos++;

        int length = _inputPos - start;

        // "-" and "+" are valid integers
        if (_input[start] < '0' && length == 1)
            return Value.Zero;

        var chars = Encoding.ASCII.GetChars(_input, start, length);
        return Value.Parse(chars, System.Globalization.NumberStyles.AllowLeadingSign);
    }
}
