namespace HexagonyGenerator.SourceCode.Parser;

using Rune = System.Text.Rune;

class RuneReader
{
    private readonly string _string;
    private int _position;
    private Rune _current;

    public bool HasData => _position < _string.Length;

    public int CurrentPosition => _position;
    public Rune Current => _current;

    public int LineId { get; private set; }
    public int Column { get; private set; }

    public RuneReader(string str)
    {
        _string = str;
        _current = _string.Length > 0 ? Rune.GetRuneAt(_string, 0) : default;
    }

    public void Read()
    {
        int current = _current.Value;
        if (current == '\n' || current == '\r')
        {
            _position++;
            if (current == '\r' && _position < _string.Length && _string[_position] == '\n')
                _position++;
            LineId++;
            Column = 0;
        }
        else
        {
            _position += _current.Utf16SequenceLength;
            Column++;
        }

        _current = _position < _string.Length ? Rune.GetRuneAt(_string, _position) : default;
    }

    public bool Read(char c)
    {
        if (Current.Value == c)
        {
            Read();
            return true;
        }
        return false;
    }

    public bool ReadAny(string s)
    {
        if (Current.IsBmp && s.Contains((char)Current.Value))
        {
            Read();
            return true;
        }
        return false;
    }
}
