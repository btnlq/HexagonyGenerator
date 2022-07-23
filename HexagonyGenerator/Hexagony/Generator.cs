namespace HexagonyGenerator.Hexagony;

using Bytecode;

class Generator
{
    private static int CanWrite(Writer.Writer writer, int size, bool firstIsMain)
    {
        if (!writer.CanWrite(new(size, firstIsMain)))
            return -1;
        int good = 0;
        int bad = 6 * size;

        while (good + 1 < bad)
        {
            int tested = good + bad >> 1;
            if (writer.CanWrite(new(size, firstIsMain, tested)))
                good = tested;
            else
                bad = tested;
        }

        return good;
    }

    public static Hexagon Generate(Program program)
    {
        var procedures = program.Procedures.ConvertAll(Compiler.Compiler.Compile);
        var writer = new Writer.Writer(procedures, program.Start.Index);

        int count = procedures.Count;
        int minSize1 = count > 3 ? 3 * count / 2 : count + 2;
        int minSize2 = Math.Max(6, count);

        for (int size = Math.Min(minSize1, minSize2); ; size++)
        {
            int nopCount1 = size >= minSize1 ? CanWrite(writer, size, true) : -1;
            int nopCount2 = size >= minSize2 ? CanWrite(writer, size, false) : -1;

            int nopCount = Math.Max(nopCount1, nopCount2);

            if (nopCount >= 0)
                return writer.Write(new(size, nopCount1 > nopCount2, nopCount));
        }
    }
}
