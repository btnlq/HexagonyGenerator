namespace HexagonyGenerator.Hexagony.Interpreter;

using System.IO;
using System.Text;

class Writer
{
    private readonly MemoryStream _output = new();

    public string GetOutput()
    {
        _output.Position = 0;
        using var reader = new StreamReader(_output);
        return reader.ReadToEnd();
    }

    public void WriteByte(Value value)
    {
        var b = (byte)(value & 0xFF);
        _output.WriteByte(b);
    }

    public void WriteInt(Value value)
    {
        var bytes = Encoding.UTF8.GetBytes(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        _output.Write(bytes);
    }
}
