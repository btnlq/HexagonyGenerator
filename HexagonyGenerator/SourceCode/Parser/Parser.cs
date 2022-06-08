namespace HexagonyGenerator.SourceCode.Parser;

using Bytecode;

class Parser
{
    public static SourceCode.Program Parse(string code) => new Parser(code).ParseProgram();

    private readonly Lexer _lexer;
    private readonly Stack _stack = new();

    private Parser(string code)
    {
        _lexer = new Lexer(code);
    }

    private Token Read(TokenType type) => _lexer.Read(type, true)!;
    private Token? TryRead(TokenType type, string? value = null) => _lexer.Read(type, false, value);
    private bool TryRead(TokenType type, out Token token)
    {
        token = _lexer.Read(type, false)!;
        return token != null;
    }

    private SourceCode.Program ParseProgram()
    {
        Block block = ParseBlock(topLevel: true);
        return new(block);
    }

    private Block ParseBlock(string? blockName = null, bool topLevel = false, IsLoop? isLoop = null)
    {
        Block block = new(isLoop);
        _stack.PushFrame(block, blockName);

        if (topLevel || TryRead(TokenType.LBrace) != null)
        {
            var lastToken = topLevel ? TokenType.Eof : TokenType.RBrace;
            while (TryRead(lastToken) == null)
                block.Add(ParseStatement());
        }
        else
        {
            block.Add(ParseStatement());
        }

        _stack.PopFrame();
        return block;
    }

    private IEnumerable<IStatement> ParseStatement()
    {
        return TryRead(TokenType.Keyword, out var token)
            ? token.Text switch
            {
                Keyword.While => ParseWhile(),
                Keyword.For => ParseFor(),
                Keyword.Break => ParseGoto(GotoType.Break, token),
                Keyword.Continue => ParseGoto(GotoType.Continue, token),
                Keyword.Exit => ParseExit(),
                Keyword.If => ParseConditional(),
                Keyword.WriteInt => ParseIntWriting(false, token),
                Keyword.WritelnInt => ParseIntWriting(true, token),
                Keyword.WriteByte => ParseByteWriting(token),
                _ => throw new ParserException($"Unexpected '{token.Text}'", token),
            }
            : ParseAssignment();
    }

    private Variable GetVariable(Token variableToken, bool create = false) =>
        _stack.GetVariable(variableToken.Text, create) ??
        throw new ParserException($"Undefined variable '{variableToken.Text}'", variableToken);

    private IEnumerable<IStatement> ParseGoto(GotoType type, Token keywordToken)
    {
        Block block = TryRead(TokenType.Identifier, out Token blockNameToken)
            ? _stack.GetBlock(blockNameToken.Text) ??
                throw new ParserException($"No enclosing loops labeled '{blockNameToken.Text}'", blockNameToken)
            : _stack.GetTopLoop() ??
                throw new ParserException($"No enclosing loop", keywordToken);
        Read(TokenType.Semicolon);

        Goto @goto = new(type, block);
        return type == GotoType.Continue && block.IsLoop?.OnContinue != null ? block.IsLoop.OnContinue.Append(@goto) : @goto;
    }

    private Exit ParseExit()
    {
        Read(TokenType.Semicolon);
        return new();
    }

    private IBooleanExpression ParseOrExpression()
    {
        var expression = ParseAndExpression();

        while (TryRead(TokenType.BooleanOperator, "||") != null)
            expression = BooleanExpression.Or(expression, ParseAndExpression());

        return expression;
    }

    private IBooleanExpression ParseAndExpression()
    {
        var expression = ParseNotExpression();

        while (TryRead(TokenType.BooleanOperator, "&&") != null)
            expression = BooleanExpression.And(expression, ParseNotExpression());

        return expression;
    }

    private IBooleanExpression ParseNotExpression()
    {
        if (TryRead(TokenType.BooleanOperator, "!") != null)
        {
            Read(TokenType.LParen);
            var expression = ParseOrExpression();
            Read(TokenType.RParen);
            return BooleanExpression.Not(expression);
        }

        return BooleanExpression.Comparison(ParseComparison());
    }

    private Comparison ParseComparison()
    {
        var left = ParseArithmeticSum();
        var opText = Read(TokenType.ComparisonOperator).Text;
        var right = ParseArithmeticSum();

        var op = opText switch
        {
            ">"  => ComparisonOp.Gt,
            ">=" => ComparisonOp.Ge,
            "<"  => ComparisonOp.Lt,
            "<=" => ComparisonOp.Le,
            "==" => ComparisonOp.Eq,
            "!=" => ComparisonOp.Ne,
            _ => throw new UnexpectedDefaultException(),
        };

        return new Comparison(left, op, right);
    }

    private IEnumerable<IStatement> ParseConditional()
    {
        Read(TokenType.LParen);
        var condition = ParseOrExpression();
        Read(TokenType.RParen);
        var trueBlock = ParseBlock();
        bool hasElse = TryRead(TokenType.Keyword, Keyword.Else) != null;
        var falseBlock = hasElse ? ParseBlock() : null;

        return condition.ToStatement(trueBlock, falseBlock);
    }

    private IEnumerable<IStatement> ParseWhile()
    {
        var blockName = TryRead(TokenType.Identifier)?.Text ?? "";
        Read(TokenType.LParen);
        var condition = ParseOrExpression();
        Read(TokenType.RParen);
        var block = ParseBlock(blockName, isLoop: new());
        return Desugar.While(condition, block);
    }

    private T? ParseOptional<T>(System.Func<T> parser, TokenType last) where T : class
    {
        T? t;
        if (TryRead(last) == null)
        {
            t = parser();
            Read(last);
        }
        else
            t = null;
        return t;
    }

    private IEnumerable<IStatement> ParseFor()
    {
        var blockName = TryRead(TokenType.Identifier)?.Text;
        Read(TokenType.LParen);
        var initializer = ParseOptional(ParseAssignmentExpression, TokenType.Semicolon);
        var condition = ParseOptional(ParseOrExpression, TokenType.Semicolon);
        var iterator = ParseOptional(ParseAssignmentExpression, TokenType.RParen);
        var block = ParseBlock(blockName, isLoop: new(iterator));
        return Desugar.For(initializer, condition, iterator, block);
    }

    private IEnumerable<IStatement> ParseIntWriting(bool newLine, Token token)
    {
        SimpleActionList actions = new();

        Read(TokenType.LParen);
        if (TryRead(TokenType.RParen) != null)
        {
            if (!newLine)
                throw new ParserException($"`{Keyword.WriteInt}` requires at least 1 argument", token);
        }
        else
        {
            while (true)
            {
                if (TryRead(TokenType.String, out Token str))
                {
                    var text = Unescape(str.Text);
                    var bytes = System.Text.Encoding.UTF8.GetBytes(text, 1, text.Length - 2);
                    foreach (var _byte in bytes)
                        actions.Add(new Writing(VariableType.Byte, new Integer(_byte)));
                }
                else
                {
                    ISymbol symbol = ParseArithmeticSum().ToSymbol(actions);
                    actions.Add(new Writing(VariableType.Int, symbol));
                }

                if (TryRead(TokenType.RParen) != null)
                    break;
                Read(TokenType.Comma);
            }
        }
        Read(TokenType.Semicolon);

        if (newLine)
            actions.Add(new Writing(VariableType.Byte, new Integer('\n')));

        return actions.Statements;
    }

    private IEnumerable<IStatement> ParseByteWriting(Token token)
    {
        SimpleActionList actions = new();

        Read(TokenType.LParen);
        while (true)
        {
            ISymbol symbol = ParseArithmeticSum().ToSymbol(actions);
            if (symbol is Integer integer && (integer.Value < 0 || integer.Value > 255))
                throw new ParserException($"Integer out of range 0..255: {integer.Value}", token);
            actions.Add(new Writing(VariableType.Byte, symbol));

            if (TryRead(TokenType.RParen) != null)
                break;
            Read(TokenType.Comma);
        }
        Read(TokenType.Semicolon);

        return actions.Statements;
    }

    private IEnumerable<IStatement> ParseAssignment()
    {
        var assignment = ParseAssignmentExpression();
        Read(TokenType.Semicolon);
        return assignment;
    }

    private IEnumerable<IStatement> ParseAssignmentExpression()
    {
        Token destToken = Read(TokenType.Identifier);
        if (TryRead(TokenType.Assignment, out var assignOpToken))
        {
            var opText = assignOpToken.Text;
            var expr = ParseArithmeticSum();
            bool simple = opText == "=";
            var dest = GetVariable(destToken, simple);
            if (!simple)
            {
                var op = opText[..1] switch
                {
                    "+" => BinOp.Add,
                    "-" => BinOp.Sub,
                    "*" => BinOp.Mul,
                    "/" => BinOp.Div,
                    "%" => BinOp.Mod,
                    "^" => throw new ParserException("Power operator operands must be constants", assignOpToken),
                    _ => throw new UnexpectedDefaultException()
                };
                expr = ArithmeticExpression.Create(ArithmeticExpression.Create(dest), op, expr);
            }
            var actions = new SimpleActionList();
            expr.AssignTo(dest, actions);
            return actions.Statements;
        }
        else
        {
            var opText = Read(TokenType.IncDec).Text;
            var op = opText == "++" ? BinOp.Add : BinOp.Sub;
            var dest = GetVariable(destToken);
            return new Assignment(dest, dest, op, new Integer(1)).AsStatement();
        }
    }

    private IArithmeticExpression ParseArithmeticSum()
    {
        var expression = ParseArithmeticTerm();

        while (true)
        {
            if (TryRead(TokenType.ArithmeticOperator, "+") != null)
                expression = ArithmeticExpression.Create(expression, BinOp.Add, ParseArithmeticTerm());
            else if (TryRead(TokenType.ArithmeticOperator, "-") != null)
                expression = ArithmeticExpression.Create(expression, BinOp.Sub, ParseArithmeticTerm());
            else
                break;
        }

        return expression;
    }

    private IArithmeticExpression ParseArithmeticTerm()
    {
        var expression = ParseArithmeticPower();

        while (true)
        {
            if (TryRead(TokenType.ArithmeticOperator, "*") != null)
                expression = ArithmeticExpression.Create(expression, BinOp.Mul, ParseArithmeticPower());
            else if (TryRead(TokenType.ArithmeticOperator, "/") != null)
                expression = ArithmeticExpression.Create(expression, BinOp.Div, ParseArithmeticPower());
            else if (TryRead(TokenType.ArithmeticOperator, "%") != null)
                expression = ArithmeticExpression.Create(expression, BinOp.Mod, ParseArithmeticPower());
            else
                break;
        }

        return expression;
    }

    private IArithmeticExpression ParseArithmeticPower()
    {
        var expression = ParseArithmeticPrimary();

        var token = TryRead(TokenType.ArithmeticOperator, "^");
        if (token != null)
            expression = ArithmeticExpression.CreatePower(expression, ParseArithmeticPower(), out string error) ??
                throw new ParserException(error, token);

        return expression;
    }

    private IArithmeticExpression ParseArithmeticPrimary()
    {
        if (TryRead(TokenType.LParen) != null)
        {
            var expression = ParseArithmeticSum();
            Read(TokenType.RParen);
            return expression;
        }

        return ArithmeticExpression.Create(ParseSymbol());
    }

    private ISymbol ParseSymbol()
    {
        return
            TryRead(TokenType.Keyword, Keyword.ReadByte) != null ? new Reading(VariableType.Byte) :
            TryRead(TokenType.Keyword, Keyword.ReadInt) != null ? new Reading(VariableType.Int) :
            TryRead(TokenType.Identifier, out var variableToken) ? GetVariable(variableToken) :
            ParseInteger();
    }

    private Integer ParseInteger()
    {
        bool hasSign = TryRead(TokenType.ArithmeticOperator, "-") != null;

        Value value = TryRead(TokenType.Integer, out Token token) ?
            ValueParser.Parse(token.Text) :
            char.ConvertToUtf32(Unescape(Read(TokenType.Character).Text), 1);
        if (hasSign)
            value = -value;
        return new(value);
    }

    private static string Unescape(string str)
    {
        var sb = new System.Text.StringBuilder();
        bool escape = false;
        foreach (char c in str)
        {
            char res;
            if (escape)
            {
                res = "'\"\\\0\a\b\f\n\r\t\v"["'\"\\0abfnrtv".IndexOf(c)];
                escape = false;
            }
            else if (c == '\\')
            {
                escape = true;
                continue;
            }
            else
            {
                res = c;
            }
            sb.Append(res);
        }
        return sb.ToString();
    }
}
