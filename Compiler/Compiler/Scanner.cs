using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

public enum Token_Class
{
    INT_T, FLOAT_T, STRING_T, READ_T, WRITE_T, REPEAT_T, UNTIL_T, IF_T, ELSEIF_T, 
    ELSE_T, THEN_T, END_T, RETURN_T, ENDL_T, 
    INT_LITERAL_T, FLOAT_LITERAL_T, STRING_LITERAL_T, IDENTIFIER_T,
    PLUS_T, MINUS_T, MULTIPLY_T, DIVIDE_T, ASSIGN_T, LESS_THAN_T, GREATER_THAN_T,
    EQUAL_T, NOT_EQUAL_T, LOGIC_AND_T, LOGIC_OR_T, 
    L_CURLY_BRACKET_T, R_CURLY_BRACKET_T, L_PAREN_BRACKET_T, R_PAREN_BRACKET_T, SEMICOLON_T,
}

namespace JASON_Compiler
{
    public class Token
    {
        public string lex;
        public Token_Class token_type;

        public Token(Token_Class token_type, string lex)
        {
            this.lex = lex;
            this.token_type = token_type;
        }

        public Token() { }
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();

        public Scanner()
        {
            ReservedWords.Add("int", Token_Class.INT_T);
            ReservedWords.Add("float", Token_Class.FLOAT_T);
            ReservedWords.Add("string", Token_Class.STRING_T);
            ReservedWords.Add("read", Token_Class.READ_T);
            ReservedWords.Add("write", Token_Class.WRITE_T);
            ReservedWords.Add("repeat", Token_Class.REPEAT_T);
            ReservedWords.Add("until", Token_Class.UNTIL_T);
            ReservedWords.Add("if", Token_Class.IF_T);
            ReservedWords.Add("elseif", Token_Class.ELSEIF_T);
            ReservedWords.Add("else", Token_Class.ELSE_T);
            ReservedWords.Add("then", Token_Class.THEN_T);
            ReservedWords.Add("end", Token_Class.END_T);
            ReservedWords.Add("return", Token_Class.RETURN_T);
            ReservedWords.Add("endl", Token_Class.ENDL_T);

            Operators.Add("+", Token_Class.PLUS_T);
            Operators.Add("-", Token_Class.MINUS_T);
            Operators.Add("*", Token_Class.MULTIPLY_T);
            Operators.Add("/", Token_Class.DIVIDE_T);

            Operators.Add(":=", Token_Class.ASSIGN_T);
            Operators.Add("<", Token_Class.LESS_THAN_T);
            Operators.Add(">", Token_Class.GREATER_THAN_T);
            Operators.Add("=", Token_Class.EQUAL_T);
            Operators.Add("<>", Token_Class.NOT_EQUAL_T);
            Operators.Add("&&", Token_Class.LOGIC_AND_T);
            Operators.Add("||", Token_Class.LOGIC_OR_T);
            Operators.Add("{", Token_Class.L_CURLY_BRACKET_T);
            Operators.Add("}", Token_Class.R_CURLY_BRACKET_T);
            Operators.Add("(", Token_Class.L_PAREN_BRACKET_T);
            Operators.Add(")", Token_Class.R_PAREN_BRACKET_T);
            Operators.Add(";", Token_Class.SEMICOLON_T);
        }

        public void StartScanning(string sourceCode)
        {
            for (int i = 0; i < sourceCode.Length; i++)
            {
                int j = i;
                char currentChar = sourceCode[i];
                StringBuilder currentLexime = new StringBuilder(currentChar.ToString());

                if (Char.IsWhiteSpace(currentChar))
                    continue;

                // Any letter in the english language
                // Underscores are not allowed in tiny language
                if (Char.IsLetter(currentChar))
                {
                    while (j + 1 < sourceCode.Length)
                    {
                        currentChar = sourceCode[++j];
                        if (Char.IsLetterOrDigit(currentChar))
                        {
                            currentLexime.Append(currentChar);
                        }
                        else
                        {
                            break;
                        }
                    }

                    FindTokenClass(currentLexime.ToString());
                    i = j - 1;
                }

                // Any digit
                else if (Char.IsDigit(currentChar))
                {
                    while (j + 1 < sourceCode.Length)
                    {
                        currentChar = sourceCode[++j];
                        if (Char.IsDigit(currentChar) || currentChar == '.')
                        {
                            currentLexime.Append(currentChar);
                        }
                        else
                        {
                            break;
                        }
                    }

                    FindTokenClass(currentLexime.ToString());
                    i = j - 1;
                }

                else if (currentChar == '"')
                {
                    while (j + 1 < sourceCode.Length)
                    {
                        currentChar = sourceCode[++j];
                        if (currentChar != '"')
                        {
                            currentLexime.Append(currentChar);
                        }
                        else
                        {
                            currentLexime.Append(currentChar);
                            break;
                        }
                    }

                    FindTokenClass(currentLexime.ToString());
                    i = j;
                }

                else if (currentChar == '/')
                {
                    // 1. Safe Look-ahead for comment
                    if (i + 1 < sourceCode.Length && sourceCode[i + 1] == '*')
                    {
                        j++; // Move j to the '*'

                        // iterate until we find * before /
                        while (j + 1 < sourceCode.Length)
                        {
                            while (j + 1 < sourceCode.Length && sourceCode[++j] != '/')
                            { } // loop until j stands at /

                            if (sourceCode[j - 1] == '*')
                            {
                                break;
                            }
                        }
                        i = j; // Skip the whole comment
                    }
                    else // 2. If it's NOT a comment, it MUST be division!
                    {
                        FindTokenClass("/");
                        // we don't change 'i' here. The outer loop will naturally increment it.
                    }
                }

                else if (Char.IsSymbol(currentChar) || Char.IsPunctuation(currentChar))
                {
                    if (i + 1 < sourceCode.Length)
                    {
                        string possibleOperator = currentChar.ToString() + sourceCode[i+1].ToString();
                        if (Operators.ContainsKey(possibleOperator))
                        {
                            FindTokenClass(possibleOperator);
                            i = i + 1;
                            continue;
                        }
                    }
                    FindTokenClass(currentChar.ToString());
                }

            }

            JASON_Compiler.TokenStream = Tokens;
        }
        public virtual void FindTokenClass(string Lex)
        {
            Token_Class TC;
            Token Tok = new Token();
            Tok.lex = Lex;

            //Is it a reserved word?
            if (ReservedWords.ContainsKey(Tok.lex))
            {
                Tok.token_type = ReservedWords[Tok.lex];
                Tokens.Add(Tok);
                return;
            }

            //Is it an identifier?
            if (isIdentifier(Tok.lex))
            {
                Tok.token_type = Token_Class.IDENTIFIER_T;
                Tokens.Add(Tok);
                return;
            }

            //Is it a Constant?
            if (isConstant(Tok.lex))
            {
                if (Tok.lex.Contains("."))
                {
                    Tok.token_type = Token_Class.FLOAT_LITERAL_T;
                }
                else {
                    Tok.token_type = Token_Class.INT_LITERAL_T; 
                }
                Tokens.Add(Tok);
                return;
            }

            //Is it an operator?
            if (isOperator(Tok.lex))
            {
                Tok.token_type = Operators[Tok.lex];
                Tokens.Add(Tok);
                return;
            }

            // Is it a String?
            if (isString(Tok.lex))
            {
                Tok.token_type = Token_Class.STRING_LITERAL_T;
                Tokens.Add(Tok);
                return;
            }

            //Is it an undefined?
            Errors.Error_List.Add(Tok.lex);
        }

        public bool isIdentifier(string lex)
        {
            return Regex.IsMatch(lex, "^[a-zA-Z][a-zA-Z0-9]*$");
        }

        public bool isConstant(string lex)
        {
            return Regex.IsMatch(lex, "^[0-9]+([.][0-9]+)?$");
        }
        public bool isString(string lex)
        {
            return Regex.IsMatch(lex, @"^""[^""]*""$");
        }

        public bool isOperator(string lex)
        {
            return Operators.ContainsKey(lex);
        }
    }
}

