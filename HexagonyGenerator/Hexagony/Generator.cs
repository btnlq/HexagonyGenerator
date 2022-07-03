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

    private Generator() { }

    public static Hexagon Generate(Program program)
    {
        Generator generator = new();
        generator.WriteProgram(program);
        return generator.hxg;
    }

    private void WriteProgram(Program program)
    {
        if (Configuration.OptimizeSize)
        {
            var heights = new int[program.Procedures.Count];
            foreach (var procedure in program.Procedures)
                heights[procedure.Index] = new Procedure(procedure).EstimateSize();
            program.OrderByDescending(heights);
        }

        var procedures = program.Procedures.ConvertAll(procedure => new Procedure(procedure));

        for (int size = procedures.Count * 3 - 1; ; size++)
        {
            var enumerator = new HexagonColumnsEnumerator(size);
            if (procedures.All(procedure => procedure.Write(enumerator)))
            {
                enumerator = new HexagonColumnsEnumerator(size, hxg);
                foreach (var procedure in procedures)
                    procedure.Write(enumerator);
                break;
            }
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
    }
}
