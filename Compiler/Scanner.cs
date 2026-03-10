using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public void StartScanning(string SourceCode)
        {
            for(int i=0; i<SourceCode.Length;i++)
            {
                int j = i;
                char currentChar = SourceCode[i];
                StringBuilder currentLexime = new StringBuilder(currentChar.ToString());

                if (currentChar == ' ' || currentChar == '\r' || currentChar == '\n')
                    continue;

                // Any letter in the english language
                // Underscores are not allowed in tiny language
                if (Char.IsLetter(currentChar))
                {
                    currentLexime.Append(currentChar);
                    while (j < SourceCode.Length)
                    {
                        currentChar = SourceCode[++j];
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
                    i = j-1;
                }

                // Any digit
                else if(currentChar >= '0' && currentChar <= '9')
                {
                   
                }

                else
                {
                   
                }
            }
            
            JASON_Compiler.TokenStream = Tokens;
        }
        void FindTokenClass(string Lex)
        {
            Token_Class TC;
            Token Tok = new Token();
            Tok.lex = Lex;
            //Is it a reserved word?
            

            //Is it an identifier?
            

            //Is it a Constant?

            //Is it an operator?

            //Is it an undefined?
        }

    

        bool isIdentifier(string lex)
        {
            bool isValid=true;
            // Check if the lex is an identifier or not.
            
            return isValid;
        }
        bool isConstant(string lex)
        {
            bool isValid = true;
            // Check if the lex is a constant (Number) or not.

            return isValid;
        }
    }
}
