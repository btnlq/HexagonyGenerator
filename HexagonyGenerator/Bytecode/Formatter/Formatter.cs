namespace HexagonyGenerator.Bytecode.Formatter;

class Formatter
{
    public static string Format(Program program) => new Formatter(program).Text;

    private readonly System.Text.StringBuilder _sb = new();
    private readonly Procedure _start;

    private string Text => _sb.ToString();

    private void AppendName(Procedure procedure)
    {
        if (procedure == _start)
            _sb.Append("start");
        else if (procedure == Procedure.Exit)
            _sb.Append("exit");
        else
            _sb.Append("proc_").Append(procedure.Index);
    }

    private void AppendVariable(Variable variable)
    {
        int index = variable.Location;
        if (index < 26)
            _sb.Append((char)(index + 'a'));
        else
            _sb.Append((char)(index % 26 + 'a')).Append(index / 26);
    }

    private void AppendInteger(Integer integer, bool asChar = false)
    {
        if (asChar)
        {
            Value value = integer.Value;
            if (value == 10)
                _sb.Append(@"'\n'");
            else if (value == '\'')
                _sb.Append(@"'\''");
            else if (32 <= value && value <= 126)
                _sb.Append('\'').Append((char)value).Append('\'');
            else
                _sb.Append(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        else
            _sb.Append(integer.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    private void AppendSymbol(ISymbol symbol, bool asChar = false)
    {
        switch (symbol)
        {
            case Variable variable:
                AppendVariable(variable);
                break;
            case Integer integer:
                AppendInteger(integer, asChar);
                break;
            case Reading reading:
                _sb.Append(reading.Type == VariableType.Int ?
                    SourceCode.Parser.Keyword.ReadInt :
                    SourceCode.Parser.Keyword.ReadByte);
                break;
            default:
                throw new UnexpectedDefaultException();
        }
    }

    private void AppendOp(BinOp op)
    {
        _sb.Append(op switch
        {
            BinOp.Add => '+',
            BinOp.Sub => '-',
            BinOp.Mul => '*',
            BinOp.Div => '/',
            BinOp.Mod => '%',
            _ => throw new UnexpectedDefaultException()
        });
    }

    private Formatter(Program program)
    {
        _start = program.Start;

        foreach (var procedure in program.Procedures)
        {
            AppendName(procedure);
            _sb.Append(':').AppendLine();
            foreach (var action in procedure.Actions)
            {
                _sb.Append(' ', 2);
                switch (action)
                {
                    case Assignment assignment:
                        AppendVariable(assignment.Dest);
                        if (assignment.Left is Variable variable && assignment.Dest.Location == variable.Location
                            && assignment.Right != null)
                        {
                            _sb.Append(' ');
                            AppendOp(assignment.Operator);
                            _sb.Append("= ");
                            AppendSymbol(assignment.Right);
                        }
                        else
                        {
                            _sb.Append(" = ");
                            AppendSymbol(assignment.Left);
                            if (assignment.Right != null)
                            {
                                _sb.Append(' ');
                                AppendOp(assignment.Operator);
                                _sb.Append(' ');
                                AppendSymbol(assignment.Right);
                            }
                        }
                        break;
                    case Writing writing:
                        _sb.Append(writing.Type == VariableType.Int ?
                            SourceCode.Parser.Keyword.WriteInt :
                            SourceCode.Parser.Keyword.WriteByte);
                        _sb.Append('(');
                        AppendSymbol(writing.Symbol, writing.Type == VariableType.Byte);
                        _sb.Append(')');
                        break;
                    default:
                        throw new UnexpectedDefaultException();
                }
                _sb.AppendLine();
            }

            _sb.Append(' ', 2).Append("goto ");
            switch (procedure.Continuation)
            {
                case Continuation continuation:
                    AppendName(continuation.Next);
                    break;
                case ConditionalContinuation continuation:
                    AppendVariable(continuation.ConditionVar);
                    _sb.Append(' ');
                    _sb.Append(continuation.Type switch
                    {
                        ConditionType.Positive => ">",
                        ConditionType.Negative => "<",
                        ConditionType.Nonzero => "!=",
                        _ => throw new UnexpectedDefaultException(),
                    });
                    _sb.Append(" 0 ? ");
                    AppendName(continuation.TrueBranch);
                    _sb.Append(" : ");
                    AppendName(continuation.FalseBranch);
                    break;
                default:
                    throw new UnexpectedDefaultException();
            }
            _sb.AppendLine();
        }
    }
}
