namespace HexagonyGenerator.SourceCode.Parser;

using Rune = System.Text.Rune;

class Lexer
{
    private readonly IEnumerator<Token> _tokenStream;

    public Lexer(string code)
    {
        _tokenStream = Scan(code).GetEnumerator();
        _tokenStream.MoveNext();
    }

    public Token? Read(TokenType type, bool force = true, string? value = null)
    {
        var token = _tokenStream.Current;
        if (token.Type == type && (value == null || token.Text == value))
        {
            _tokenStream.MoveNext();
            return token;
        }
        return force ? throw new ParserException($"Unexpected token '{token.Text}'", token) : null;
    }

    private static IEnumerable<Token> Scan(string code)
    {
        var reader = new RuneReader(code);
        while (reader.HasData)
        {
            if (Rune.IsWhiteSpace(reader.Current))
            {
                reader.Read();
                continue;
            }

            int start = reader.CurrentPosition;
            Rune current = reader.Current;
            int lineId = reader.LineId;
            int column = reader.Column;
            reader.Read();

            TokenType type = TokenType.Invalid;

            switch (current.Value)
            {
                case '{':
                    type = TokenType.LBrace;
                    break;
                case '}':
                    type = TokenType.RBrace;
                    break;
                case '(':
                    type = TokenType.LParen;
                    break;
                case ')':
                    type = TokenType.RParen;
                    break;
                case ';':
                    type = TokenType.Semicolon;
                    break;
                case ',':
                    type = TokenType.Comma;
                    break;
                case '<':
                case '>':
                    reader.Read('=');
                    type = TokenType.ComparisonOperator;
                    break;
                case '=':
                    type = reader.Read('=') ? TokenType.ComparisonOperator : TokenType.Assignment;
                    break;
                case '!':
                    type = reader.Read('=') ? TokenType.ComparisonOperator : TokenType.BooleanOperator;
                    break;
                case '&':
                case '|':
                    if (reader.Read((char)current.Value))
                        type = TokenType.BooleanOperator;
                    break;
                case '/':
                    if (reader.Read('/')) // line comment
                    {
                        while (reader.HasData && reader.Column != 0)
                            reader.Read();
                        continue;
                    }
                    else if (reader.Read('*')) // block comment
                    {
                        bool good = false;
                        while (reader.HasData)
                        {
                            if (reader.Read('*'))
                            {
                                if (reader.Read('/'))
                                {
                                    good = true;
                                    break;
                                }
                            }
                            else
                            {
                                reader.Read();
                            }
                        }
                        if (good)
                            continue;
                        throw new ParserException("Unclosed block comment", lineId, column);
                    }
                    goto case '*';
                case '+':
                case '-':
                    if (reader.Read((char)current.Value))
                    {
                        type = TokenType.IncDec;
                        break;
                    }
                    goto case '*';
                case '*':
                case '%':
                case '^':
                    type = reader.Read('=') ? TokenType.Assignment : TokenType.ArithmeticOperator;
                    break;
                case '\'':
                case '\"':
                {
                    char quote = (char)current.Value;
                    type = quote == '\'' ? TokenType.Character : TokenType.String;
                    int length = 0;
                    while (!reader.Read(quote))
                    {
                        if (reader.Read('\\'))
                        {
                            if (!reader.ReadAny("'\"\\0abfnrtv"))
                                throw new ParserException($"Unrecognized escape sequence: \\{reader.Current}", lineId, column + 1);
                        }
                        else
                        {
                            if (reader.ReadAny("\0\n\r"))
                                throw new ParserException("Newline in literal", lineId, column);
                            reader.Read();
                        }
                        length++;
                    }
                    if (type == TokenType.Character)
                    {
                        if (length == 0)
                            throw new ParserException("Empty character literal", lineId, column);
                        else if (length > 1)
                            throw new ParserException("Too many characters in character literal", lineId, column);
                    }
                    break;
                }
                default:
                    if (ValueParser.IsDecDigit(current.Value))
                    {
                        if (current.Value == '0' && reader.ReadAny(ValueParser.PrefixChars))
                        {
                            var prefixChar = code[start + 1];
                            var digitChecker = ValueParser.GetDigitChecker(prefixChar);
                            while (digitChecker(reader.Current.Value))
                                reader.Read();
                            if (reader.CurrentPosition - start == 2)
                                throw new ParserException($"Invalid number: '0{prefixChar}'", lineId, column);
                        }
                        else
                        {
                            while (ValueParser.IsDecDigit(reader.Current.Value))
                                reader.Read();
                        }
                        type = TokenType.Integer;
                    }
                    else if (Rune.IsLetter(current) || current.Value == '_')
                    {
                        while (Rune.IsLetterOrDigit(reader.Current) || reader.Current.Value == '_')
                            reader.Read();
                        type = TokenType.Identifier;
                    }
                    break;
            }

            if (type == TokenType.Invalid)
                throw new ParserException($"Unexpected character '{current}'", lineId, column);

            var text = type < TokenType.Eof ? _cached[(int)type] : code[start..reader.CurrentPosition];
            if (type == TokenType.Identifier && Keyword.Is(text))
                type = TokenType.Keyword;
            yield return new Token(type, lineId, column, text);
        }

        yield return new Token(TokenType.Eof, reader.LineId, reader.Column, "(end of file)");
    }

    private static readonly string[] _cached = { "{", "}", "(", ")", ",", ";" };
}
