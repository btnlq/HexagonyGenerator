namespace HexagonyGenerator.Hexagony.Interpreter;

class Executor
{
    private readonly Hexagon Hexagon;
    private readonly Reader Reader;
    private readonly Writer Writer = new();

    private Executor(Hexagon hexagon, Reader reader)
    {
        Hexagon = hexagon;
        Reader = reader;
    }

    public static string Execute(Hexagon hexagon, Reader reader)
    {
        var executor = new Executor(hexagon, reader);
        executor.Execute();
        return executor.Writer.GetOutput();
    }

    private Pos Pos = Pos.Zero;
    private Dir Dir = Dir.Right;

    private readonly Dictionary<Pos, Value> Memory = new();

    private Value Get(Pos pos) => Memory.GetValueOrDefault(pos);

    private Pos MemPos = Pos.Zero;
    private Dir MemDir = Dir.Right;

    private Value Value
    {
        get => Get(MemPos);
        set => Memory[MemPos] = value;
    }

    private bool Positive => Value.Sign > 0;

    private Value LeftValue => Get(MemPos + MemDir + (Dir)(MemDir + 1));
    private Value RightValue => Get(MemPos + MemDir + (Dir)(MemDir - 1));

    private void Execute()
    {
        while (true)
        {
            int cmd = Hexagon[Pos.X, Pos.Y];
            switch (cmd)
            {
                // Special
                case Command.Nop: break;
                case Command.Exit: return;

                // Arithmetic
                case Command.Increment: ++Value; break;
                case Command.Decrement: --Value; break;
                case Command.Add: Value = LeftValue + RightValue; break;
                case Command.Sub: Value = LeftValue - RightValue; break;
                case Command.Mul: Value = LeftValue * RightValue; break;
                case Command.Div: Value = LeftValue.Div(RightValue); break;
                case Command.Mod: Value = LeftValue.Mod(RightValue); break;
                case Command.Negate: Value = -Value; break;

                // I/O
                case Command.ReadByte: Value = Reader.ReadByte(); break;
                case Command.ReadInt: Value = Reader.ReadInt(); break;
                case Command.WriteByte: Writer.WriteByte(Value); break;
                case Command.WriteInt: Writer.WriteInt(Value); break;

                // Control flow
                case '$': Pos += Dir; break;
                case '_': Dir = -Dir; break;
                case '|': Dir = 3 - Dir; break;
                case '/': Dir = 2 - Dir; break;
                case '\\': Dir = 4 - Dir; break;
                case '<': Dir = Dir == 0 ? Positive ? 5 : 1 : LeftArrowDirs[Dir]; break;
                case '>': Dir = Dir == 3 ? Positive ? 2 : 4 : RightArrowDirs[Dir]; break;

                // Memory manipulation
                case Command.TurnLeft: MemPos += MemDir; MemPos += ++MemDir; break;
                case Command.TurnRight: MemPos += MemDir; MemPos += --MemDir; break;
                case Command.TurnLeftBackwards: MemPos -= MemDir; MemPos -= --MemDir; break;
                case Command.TurnRightBackwards: MemPos -= MemDir; MemPos -= ++MemDir; break;
                case Command.ReverseMp: MemDir += 3; break;
                case Command.Copy: Value = Positive ? RightValue : LeftValue; break;

                case ']':
                case '[':
                case '#':
                case '^':
                    throw new System.NotImplementedException($"'{cmd}' is not implemented because not used by the generator"); 

                default:
                    if ('0' <= cmd && cmd <= '9')
                    {
                        int digit = cmd - '0';
                        Value = Value * 10 + (Positive ? digit : -digit);
                    }
                    else
                        Value = cmd;
                    break;
            }

            Pos += Dir;
            HandleWrapping();
        }    
    }

    private static readonly Dir[] LeftArrowDirs = { 9, 4, 3, 0, 3, 2 };
    private static readonly Dir[] RightArrowDirs = { 3, 0, 5, 9, 1, 0 };

    private void HandleWrapping()
    {
        int x = Pos.X;
        int y = Pos.Y;
        int size = Hexagon.Size;

        if (x < 0 || x > 2*size || y < -size || y > size || x+y < 0 || x+y > 2*size)
        {
            if (x == -1 && y == 0 && Dir == 2 && Get(new(0, 0)).Sign <= 0)
                Pos = new(2 * size, -size);
            else if (x == 2 * size + 1 && y == -size && Dir == 4)
                Pos = new(0, 1);
            else
                throw new System.NotImplementedException($"Instruction pointer is out of code grid: {Pos}. Wrapping is not implemented because not used by the generator");
        }
    }
}
