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
    private static Hexagon? TryWriteProcedures(List<Procedure> procedures, int size, bool firstIsBig)
    {
        var firstShape = new FirstShape(size, firstIsBig);
        var enumerator = new HexagonColumnsEnumerator(firstShape);
        int i = 0;
        while (i < procedures.Count && procedures[i].Write(enumerator)) i++;

        if (i == procedures.Count)
        {
            var hxg = new Hexagon(size);
            enumerator = new HexagonColumnsEnumerator(firstShape, hxg);
            foreach (var procedure in procedures)
                procedure.Write(enumerator);
            return hxg;
        }

        int firstCount = i;
        var secondShape = new SecondShape(size, !firstIsBig);
        enumerator = new HexagonColumnsEnumerator(secondShape);
        while (i < procedures.Count && procedures[i].Write(enumerator)) i++;

        if (i == procedures.Count)
        {
            var hxg = new Hexagon(size);
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

            return hxg;
        }

        return null;
    }

    public static Hexagon Generate(Program program)
    {
        var procedures = program.Procedures.ConvertAll(procedure => new Procedure(procedure));
        int count = procedures.Count;

        int minBigSize = count > 3 ? 3 * count / 2 : count + 2;
        int minSmallSize = Math.Max(6, count);

        Hexagon? hxg;

        for (int size = Math.Min(minBigSize, minSmallSize); ; size++)
        {
            if (size >= minBigSize && (hxg = TryWriteProcedures(procedures, size, true)) != null)
                break;
            if (size >= minSmallSize && (hxg = TryWriteProcedures(procedures, size, false)) != null)
                break;
        }

        hxg[1, 0] = '~';
        hxg[2, -2] = '\\';

        int start = program.Start.Index;

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

        return hxg;
    }
}
