namespace Compiler;

using System.Text.RegularExpressions;

public enum TokenType {
    // Special
    EOF,
    Unknown,

    // Literals
    Identifier_T,
    FloatLiteral_T,
    StringLiteral_T,

    // Keywords
    If_T,
    Else_T,
    While_T,
    Return_T,
    Int_T,
    Float_T,

    // Operators
    Plus_T,
    Minus_T,
    Multiply_T,
    Divide_T,
    Assign_T,
    Greater_T,
    Smaller_T,
    GreaterOrEqual_T,
    SmallerOrEqual_T,
    Equal_T,

    // Delimiters
    LeftParen_T,
    RightParen_T,
    LeftBrace_T,
    RightBrace_T,
    LeftBracket_T,
    RightBracket_T,
    Semicolon_T
}

public class Token
{
    private static readonly Dictionary<char, TokenType> SingleCharTokens =
        new()
        {
            ['+'] = TokenType.Plus_T,
            ['-'] = TokenType.Minus_T,
            ['*'] = TokenType.Multiply_T,
            ['='] = TokenType.Assign_T,
            ['/'] = TokenType.Divide_T,
            ['('] = TokenType.LeftParen_T,
            [')'] = TokenType.RightParen_T,
            ['{'] = TokenType.LeftBrace_T,
            ['}'] = TokenType.RightBrace_T,
            ['['] = TokenType.LeftBracket_T,
            [']'] = TokenType.RightBracket_T,
            [';'] = TokenType.Semicolon_T,
            ['<'] = TokenType.Smaller_T,
            ['>'] = TokenType.Greater_T
        };
    
    private static readonly Dictionary<string, TokenType> ReservedToken =
        new()
        {
            ["if"] = TokenType.If_T,
            ["else"] = TokenType.Else_T,
            ["while"] = TokenType.While_T,
            ["return"] = TokenType.Return_T,
            ["int"] = TokenType.Int_T,
            ["float"] = TokenType.Float_T,
            ["=="] = TokenType.Equal_T,
            ["<="] = TokenType.SmallerOrEqual_T,
            [">="] = TokenType.GreaterOrEqual_T,
        };
    
    public TokenType Type { get; set;  }
    public string Lexeme { get; }
    public int Line { get; }
    public int Column { get; }

    public Token(int line, int column)
    {
        Line = line;
        Column = column;
    }
    
    public static TokenType CalculateType(string input)
    {
        if (input.Length == 0)
        {
            return TokenType.EOF; // Terminating token
        }

        TokenType type;
        if (input.Length == 1)
        {
            if (SingleCharTokens.TryGetValue(input[0], out type))
            {
                return type;
            }
        }
        
        if (ReservedToken.TryGetValue(input, out type))
        {
            return type;
        }

        if (IsFloatLiteral(input))
        {
            return TokenType.FloatLiteral_T;
        }
        
        if (IsIdentifier(input))
        {
            return TokenType.Identifier_T;
        }
        
        return TokenType.Unknown;
    }

    /*
     * ^ , $ from start to end
     * [-|+]? -> could start with - or + or neither
     * [0-9]+ -> must have at least one single digit
     * [[.][0-9]+]? -> may or may not end with decimal
     * if it does, it must be in the form of . then at least one digit
     */
    private static bool IsFloatLiteral(string input)
    {
        return Regex.IsMatch(input, "^[-|+]?[0-9]+(\\.[0-9]+)?$");
    }
    
    /*
     * ^ , $ -> from start to end
     * [a-zA-Z_] -> must start with a letter or underscore
     * [a-zA-Z0-9_]* -> any mix of numbers, letters, and underscores after
     */
    private static bool IsIdentifier(string input)
    {
        return Regex.IsMatch(input, "^[a-zA-Z_][a-zA-Z0-9_]*$");
    }
}