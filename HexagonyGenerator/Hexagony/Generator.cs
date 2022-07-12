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
    private static Hexagon? TryWriteProcedures(List<Procedure> procedures, int size, bool firstIsMain)
    {
        var enumerator = new HexagonColumnsEnumerator(size, firstIsMain);

        if (procedures.All(procedure => procedure.Write(enumerator)))
        {
            var hxg = new Hexagon(size);
            enumerator = new HexagonColumnsEnumerator(size, firstIsMain, hxg);
            foreach (var procedure in procedures)
                procedure.Write(enumerator);
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
