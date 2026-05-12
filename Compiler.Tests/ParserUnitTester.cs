using System.Collections.Generic;
using FluentAssertions;
using JASON_Compiler;
using Xunit;

namespace Compiler.Tests
{
    public class ParserUnitTester
    {
        /// <summary>
        /// Helper to quickly build token streams, bypassing the scanner completely.
        /// </summary>
        private List<Token> GenerateTokens(params (Token_Class type, string lexeme)[] tokens)
        {
            var list = new List<Token>();
            foreach (var t in tokens)
            {
                list.Add(new Token(t.type, t.lexeme));
            }
            return list;
        }

        /// <summary>
        /// Test Setup Helper to initialize the parser with tokens and reset the pointer.
        /// </summary>
        private Parser SetupParser(List<Token> tokens)
        {
            var parser = new Parser();
            parser.TokenStream = tokens;
            parser.InputPointer = 0;
            JASON_Compiler.Errors.Error_List.Clear();
            return parser;
        }

        /***** Test Rule: Assignment Statement *****/
        // Rule: starts with Identifier then assignment operator ":=" followed by Expression[cite: 1]
        [Fact]
        public void TestIsolatedAssignmentStatement()
        {
            // Code: x := 10;
            var tokens = GenerateTokens(
                (Token_Class.IDENTIFIER_T, "x"),
                (Token_Class.ASSIGN_T, ":="),
                (Token_Class.INT_LITERAL_T, "10"),
                (Token_Class.SEMICOLON_T, ";")
            );

            var parser = SetupParser(tokens);

            // IMPORTANT: Assign_Statement() must be marked 'public' in Parser.cs for this to work
            Node result = parser.Assign_Statement();

            result.Should().NotBeNull();
            result.Name.Should().Be("Assign_Statement");
            result.Children[0].Name.Should().Be("IDENTIFIER_T"); // x
            result.Children[1].Name.Should().Be("ASSIGN_T");     // :=
            result.Children[2].Name.Should().Be("Expression");   // 10
            result.Children[3].Name.Should().Be("SEMICOLON_T");  // ;
        }

        /***** Test Rule: Write Statement *****/
        // Rule: starts with "write" followed by an Expression or endl and ends with semi-colon[cite: 1]
        [Fact]
        public void TestIsolatedWriteStatement()
        {
            // Code: write "Hello World";
            var tokens = GenerateTokens(
                (Token_Class.WRITE_T, "write"),
                (Token_Class.STRING_LITERAL_T, "\"Hello World\""),
                (Token_Class.SEMICOLON_T, ";")
            );

            var parser = SetupParser(tokens);

            // IMPORTANT: Write_Statement() must be marked 'public' in Parser.cs
            Node result = parser.Write_Statement();

            result.Should().NotBeNull();
            result.Name.Should().Be("Write_Statement");
            result.Children[0].Name.Should().Be("WRITE_T");

            // Opt_Exp is the node you used to handle Expression vs Endl
            result.Children[1].Name.Should().Be("Opt_Exp");
            result.Children[1].Children[0].Name.Should().Be("Expression");

            result.Children[2].Name.Should().Be("SEMICOLON_T");
        }

        /***** Test Rule: Condition Statement *****/
        // Rule: starts with Condition followed by zero or more Boolean_Operator and Condition[cite: 1]
        [Fact]
        public void TestIsolatedConditionStatement()
        {
            // Code: x < 5 && y > 1
            var tokens = GenerateTokens(
                (Token_Class.IDENTIFIER_T, "x"),
                (Token_Class.LESS_THAN_T, "<"),
                (Token_Class.INT_LITERAL_T, "5"),
                (Token_Class.LOGIC_AND_T, "&&"),
                (Token_Class.IDENTIFIER_T, "y"),
                (Token_Class.GREATER_THAN_T, ">"),
                (Token_Class.INT_LITERAL_T, "1")
            );

            var parser = SetupParser(tokens);

            // IMPORTANT: Condition_Statement() must be marked 'public' in Parser.cs
            Node result = parser.Condition_Statement();

            result.Should().NotBeNull();
            result.Name.Should().Be("Condition_Statement");

            // First child is the first condition (x < 5)
            result.Children[0].Name.Should().Be("Condition");

            // Second child handles the && and the next condition (y > 1) via your Bool_stat logic
            var boolStatNode = result.Children[1];
            boolStatNode.Name.Should().Be("Bool_stat");
            boolStatNode.Children[0].Name.Should().Be("Bool_Operator"); // &&
            boolStatNode.Children[1].Name.Should().Be("Condition");     // y > 1
        }

        /***** Test Rule: Return Statement *****/
        // Rule: starts with "return" followed by Expression then ends with semi-colon[cite: 1]
        [Fact]
        public void TestIsolatedReturnStatement()
        {
            // Code: return a + b;
            var tokens = GenerateTokens(
                (Token_Class.RETURN_T, "return"),
                (Token_Class.IDENTIFIER_T, "a"),
                (Token_Class.PLUS_T, "+"),
                (Token_Class.IDENTIFIER_T, "b"),
                (Token_Class.SEMICOLON_T, ";")
            );

            var parser = SetupParser(tokens);

            // IMPORTANT: Return_Statement() must be marked 'public' in Parser.cs
            Node result = parser.Return_Statement();

            result.Should().NotBeNull();
            result.Name.Should().Be("Return_Statement");
            result.Children[0].Name.Should().Be("RETURN_T");
            result.Children[1].Name.Should().Be("Expression");
            result.Children[2].Name.Should().Be("SEMICOLON_T");
        }
    }
}