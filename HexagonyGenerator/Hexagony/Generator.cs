namespace HexagonyGenerator.Hexagony;

using Bytecode;
using Shape = Writer.Shape;

class Generator
{
    private static Hexagon? TryWriteProcedures(Writer.Writer writer, Shape shape)
    {
        return writer.CanWrite(shape) ? writer.Write(shape) : null;
    }

    public static Hexagon Generate(Program program)
    {
        var procedures = program.Procedures.ConvertAll(Compiler.Compiler.Compile);
        int count = procedures.Count;
        var writer = new Writer.Writer(procedures, program.Start.Index);

        int minBigSize = count > 3 ? 3 * count / 2 : count + 2;
        int minSmallSize = Math.Max(6, count);

        Hexagon? hxg;

        for (int size = Math.Min(minBigSize, minSmallSize); ; size++)
        {
            if (size >= minBigSize && (hxg = TryWriteProcedures(writer, new Shape(size, true))) != null)
                break;
            if (size >= minSmallSize && (hxg = TryWriteProcedures(writer, new Shape(size, false))) != null)
                break;
        }

        return hxg;
    }
}
