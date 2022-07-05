namespace HexagonyGenerator.Hexagony;

using Bytecode;

/*
          ) \ _ . . _ . . . .
         > ~ < > ) < > ) < . .
        \ < $ . < $ . < $ . . .
       . . . _ a . _ a . _ a . .
      . . . _ b a _ b a _ b a . .
     . . . . $ b a $ b a $ b a . .
    . . . . . c b a c b a c b a . .
   . . . . . . c b a c b a c b a . .
  . . . . . . . . . . . . . . . . . .

*/

class Generator
{
    private readonly Hexagon hxg = new();
    private readonly List<Procedure> procedures;

    private Generator(Program program)
    {
        procedures = program.Procedures.ConvertAll(procedure => new Procedure(procedure));
    }

    public static Hexagon Generate(Program program)
    {
        Generator generator = new(program);
        generator.WriteProgram(program.Start.Index);
        return generator.hxg;
    }

    private bool TryWriteProcedures(int size, bool firstIsBig)
    {
        var firstShape = new FirstShape(size, firstIsBig);
        var enumerator = new HexagonColumnsEnumerator(firstShape);
        int i = 0;
        while (i < procedures.Count && procedures[i].Write(enumerator)) i++;

        if (i == procedures.Count)
        {
            enumerator = new HexagonColumnsEnumerator(firstShape, hxg);
            foreach (var procedure in procedures)
                procedure.Write(enumerator);
            return true;
        }

        int firstCount = i;
        var secondShape = new SecondShape(size, !firstIsBig);
        enumerator = new HexagonColumnsEnumerator(secondShape);
        while (i < procedures.Count && procedures[i].Write(enumerator)) i++;

        if (i == procedures.Count)
        {
            enumerator = new HexagonColumnsEnumerator(firstShape, hxg);
            for (i = 0; i < firstCount; i++) procedures[i].Write(enumerator);

            int lastColumn = enumerator.Column;

            enumerator = new HexagonColumnsEnumerator(secondShape, hxg);
            for (; i < procedures.Count; i++) procedures[i].Write(enumerator);

            if (lastColumn <= size)
            {
                hxg[0, lastColumn] = '_';
                hxg[1, lastColumn] = '>';
                hxg[size + 1, -size] = '.';
            }

            return true;
        }

        return false;
    }

    private void WriteProgram(int start)
    {
        int count = procedures.Count;

        int minBigSize = count > 3 ? 3 * count / 2 : count + 2;
        int minSmallSize = Math.Max(6, count);

        for (int size = Math.Min(minBigSize, minSmallSize); ; size++)
        {
            if (size >= minBigSize && TryWriteProcedures(size, true))
                break;
            if (size >= minSmallSize && TryWriteProcedures(size, false))
                break;
        }

        hxg[1, 0] = '~';
        hxg[2, -2] = '\\';

        if (start < 3)
        {
            hxg[0, 0] = ").("[start];
            hxg[0, 1] = '\\';
        }
        else
        {
            int y = 0;
            foreach (char c in (start - 2).ToString(System.Globalization.CultureInfo.InvariantCulture))
            {
                hxg[0, y] = c;
                y++;
                if (hxg[0, y] == '_') y++;
            }
            while (hxg[1, y - 1] != ')') y++;
            hxg[0, y] = '\\';
        }
    }
}
