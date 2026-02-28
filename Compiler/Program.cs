namespace Compiler;

using System.IO;
using System.Collections.Generic;

static class Program
{
    public static void Main(string[] args)
    {
        List<Token> tokens = new List<Token>();

        // Ensure file exists before opening to prevent crash
        if (!File.Exists("data.txt")) 
        {
            Console.WriteLine("Error: data.txt not found.");
            return;
        }

        using StreamReader reader = new StreamReader("data.txt");
        
        string line;
        int lineNo = 0;

        while ((line = reader.ReadLine()) != null)
        {
            int col = 0;
            // Iterate through the line until we reach the end
            while (col < line.Length)
            {
                // Handle whitespace
                if (char.IsWhiteSpace(line[col]))
                {
                    col++;
                    continue;
                }

                string bestMatch = string.Empty;
                int bestLength = 0;

                // Inner Loop: Try to find the longest token starting at 'col'
                // 'len' represents the LENGTH of the substring we are testing
                for (int len = 1; col + len <= line.Length; len++)
                {
                    string candidate = line.Substring(col, len);
                    TokenType type = Token.CalculateType(candidate);

                    if (type != TokenType.Unknown)
                    {
                        // Valid so far, save it and keep extending (Greedy)
                        bestMatch = candidate;
                        bestLength = len;
                    }
                    else
                    {
                        // We hit an unknown. 
                        // If we already found a valid match (e.g., "int"), 
                        // and "intX" is unknown, stop here.
                        if (bestLength > 0) break;
                    }
                }

                if (bestLength > 0)
                {
                    // 1. Create and Add the token
                    Token token = new Token(lineNo, col);
                    token.Type = Token.CalculateType(bestMatch);
                    // token.Value = bestMatch; // Useful for debugging
                    tokens.Add(token);

                    // 2. Advance 'col' by the length of the token we found
                    col += bestLength;
                }
                else
                {
                    // No valid token found starting at this char (Lexical Error).
                    // Skip 1 char to avoid infinite loop.
                    Console.WriteLine($"Unexpected character at line {lineNo}, col {col}: '{line[col]}'");
                    col++;
                }
            }
            lineNo++;
        }

        // Add EOF token
        Token terminator = new Token(lineNo, 0);
        terminator.Type = TokenType.EOF;
        tokens.Add(terminator);

        foreach (var token in tokens)
        {
            Console.WriteLine($"{token} = {(int)token.Type}");
        }
    }
}