using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JASON_Compiler;
using Xunit;

namespace Compiler.Tests
{
    public class IntegratedTester
    {
        /***** Tests every single reserved keyword *****/
        [Fact]
        public void TestReservedKeywords()
        {
            Scanner scanner = new Scanner();
            
            string input = 
                @"
                    int float string read write repeat
                    until if else elseif then end return endl
                 ";

            scanner.StartScanning(input);
            
            var expected = new List<Token>
            {
                new Token(Token_Class.INT_T, "int"), new Token(Token_Class.FLOAT_T, "float"),
                new Token(Token_Class.STRING_T, "string"), new Token(Token_Class.READ_T, "read"),
                new Token(Token_Class.WRITE_T, "write"), new Token(Token_Class.REPEAT_T, "repeat"),
                new Token(Token_Class.UNTIL_T, "until"), new Token(Token_Class.IF_T, "if"),
                new Token(Token_Class.ELSEIF_T, "elseif"), new Token(Token_Class.ELSE_T, "else"),
                new Token(Token_Class.THEN_T, "then"), new Token(Token_Class.END_T, "end"),
                new Token(Token_Class.RETURN_T, "return"), new Token(Token_Class.ENDL_T, "endl")
            };

            scanner.Tokens.Should().BeEquivalentTo(expected);
        }
        
        /***** Tests an example of a working code snippet *****/
        [Fact]
        public void TestBasicCode()
        {
            Scanner scanner = new Scanner();
            
            string input = @"int x = 5 + 6.5;";
            scanner.StartScanning(input);
            
            var expected = new List<Token>
            {
                new Token(Token_Class.INT_T, "int"), new Token(Token_Class.IDENTIFIER_T, "x"),
                new Token(Token_Class.EQUAL_T, "="), new Token(Token_Class.INT_LITERAL_T, "5"), 
                new Token(Token_Class.PLUS_T, "+"), new Token(Token_Class.FLOAT_LITERAL_T, "6.5"),
                new Token(Token_Class.SEMICOLON_T, ";")
            };

            scanner.Tokens.Should().BeEquivalentTo(expected);
        }
        
        /***** Tests if comments are correctly deleted *****/
        [Fact]
        public void TestComments()
        {
            Scanner scanner = new Scanner();
            
            string input = 
                @"
                    int x = 10 - 3;
                    /* comment */

                    /*
                     *
                     * Multi-Line Comment
                     *
                     */
                    float y = 5 + 2;
                 ";

            scanner.StartScanning(input);
            
            var expected = new List<Token>
            {
                new Token(Token_Class.INT_T, "int"), new Token(Token_Class.IDENTIFIER_T, "x"),
                new Token(Token_Class.EQUAL_T, "="), new Token(Token_Class.INT_LITERAL_T, "10"), 
                new Token(Token_Class.MINUS_T, "-"), new Token(Token_Class.INT_LITERAL_T, "3"),
                new Token(Token_Class.SEMICOLON_T, ";"),

                new Token(Token_Class.FLOAT_T, "float"), new Token(Token_Class.IDENTIFIER_T, "y"),
                new Token(Token_Class.EQUAL_T, "="), new Token(Token_Class.INT_LITERAL_T, "5"),
                new Token(Token_Class.PLUS_T, "+"), new Token(Token_Class.INT_LITERAL_T, "2"),
                new Token(Token_Class.SEMICOLON_T, ";")
            };

            scanner.Tokens.Should().BeEquivalentTo(expected);
        }
        
        /***** Tests single character operators *****/
        [Fact]
        public void TestSingleCharacterOperators()
        {
            Scanner scanner = new Scanner();
            
            string input = 
                @"
                    int a = 10 + 3;
                    float b = 5 - 2;
                    int c = 12 * 6;
                    float d = 1 / 2;
                 ";

            scanner.StartScanning(input);

            var expected = new List<Token>
            {
                new Token(Token_Class.INT_T, "int"), new Token(Token_Class.IDENTIFIER_T, "a"),
                new Token(Token_Class.EQUAL_T, "="), new Token(Token_Class.INT_LITERAL_T, "10"),
                new Token(Token_Class.PLUS_T, "+"), new Token(Token_Class.INT_LITERAL_T, "3"),
                new Token(Token_Class.SEMICOLON_T, ";"),

                new Token(Token_Class.FLOAT_T, "float"), new Token(Token_Class.IDENTIFIER_T, "b"),
                new Token(Token_Class.EQUAL_T, "="), new Token(Token_Class.INT_LITERAL_T, "5"),
                new Token(Token_Class.MINUS_T, "-"), new Token(Token_Class.INT_LITERAL_T, "2"),
                new Token(Token_Class.SEMICOLON_T, ";"),

                new Token(Token_Class.INT_T, "int"), new Token(Token_Class.IDENTIFIER_T, "c"),
                new Token(Token_Class.EQUAL_T, "="), new Token(Token_Class.INT_LITERAL_T, "12"),
                new Token(Token_Class.MULTIPLY_T, "*"), new Token(Token_Class.INT_LITERAL_T, "6"),
                new Token(Token_Class.SEMICOLON_T, ";"),

                new Token(Token_Class.FLOAT_T, "float"), new Token(Token_Class.IDENTIFIER_T, "d"),
                new Token(Token_Class.EQUAL_T, "="), new Token(Token_Class.INT_LITERAL_T, "1"),
                new Token(Token_Class.DIVIDE_T, "/"), new Token(Token_Class.INT_LITERAL_T, "2"),
                new Token(Token_Class.SEMICOLON_T, ";")
            };

            scanner.Tokens.Should().BeEquivalentTo(expected);
        }
        
        /*
         * Tests numbers
         * The multiple '.' should be handled inside the regex
         * The number should not be included
         */
        [Fact]
        public void TestNumbers()
        {
            Scanner scanner = new Scanner();
            
            string input = 
                @"
                    int a = 0104091;
                    float b = 15.2;
                    float c = 0.14290.142;
                 ";

            scanner.StartScanning(input);

            var expected = new List<Token>
            {
                new Token(Token_Class.INT_T, "int"), new Token(Token_Class.IDENTIFIER_T, "a"),
                new Token(Token_Class.EQUAL_T, "="), new Token(Token_Class.INT_LITERAL_T, "0104091"),
                new Token(Token_Class.SEMICOLON_T, ";"),

                new Token(Token_Class.FLOAT_T, "float"), new Token(Token_Class.IDENTIFIER_T, "b"),
                new Token(Token_Class.EQUAL_T, "="), new Token(Token_Class.FLOAT_LITERAL_T, "15.2"),
                new Token(Token_Class.SEMICOLON_T, ";"),

                new Token(Token_Class.FLOAT_T, "float"), new Token(Token_Class.IDENTIFIER_T, "c"),
                new Token(Token_Class.EQUAL_T, "="),
                new Token(Token_Class.SEMICOLON_T, ";"),
            };

            scanner.Tokens.Should().BeEquivalentTo(expected);
        }
        
        /***** Tests an example of a working code snippet missing a semicolon *****/
        /*
         * FAILED
         * Infinite loop
         * todo: fix
         */
        [Fact]
        public void TestBasicCodeNoSemiColon()
        {
            Scanner scanner = new Scanner();
            
            string input = @"int x = 5 + 6.5";
            scanner.StartScanning(input);
            
            var expected = new List<Token>
            {
                new Token(Token_Class.INT_T, "int"), new Token(Token_Class.IDENTIFIER_T, "x"),
                new Token(Token_Class.EQUAL_T, "="), new Token(Token_Class.INT_LITERAL_T, "5"), 
                new Token(Token_Class.PLUS_T, "+"), new Token(Token_Class.FLOAT_LITERAL_T, "6.5")
            };

            scanner.Tokens.Should().BeEquivalentTo(expected);
        }
        
        /*
         * Tests an example of a non-working code snippet
         * The code should still be scanned correctly
         */
        [Fact]
        public void TestAdvancedInvalidCode()
        {
            Scanner scanner = new Scanner();
            
            string input = 
                @"
                    x int 10.5 = - 3
                    float y x = int 5 2.1 /
                 ";
            scanner.StartScanning(input);
            
            var expected = new List<Token>
            {
                new Token(Token_Class.IDENTIFIER_T, "x"), new Token(Token_Class.INT_T, "int"),
                new Token(Token_Class.FLOAT_LITERAL_T, "10.5"), new Token(Token_Class.EQUAL_T, "="), 
                new Token(Token_Class.MINUS_T, "-"), new Token(Token_Class.INT_LITERAL_T, "3"),
                
                new Token(Token_Class.FLOAT_T, "float"), new Token(Token_Class.IDENTIFIER_T, "y"),
                new Token(Token_Class.IDENTIFIER_T, "x"), new Token(Token_Class.EQUAL_T, "="), 
                new Token(Token_Class.INT_T, "int"), new Token(Token_Class.INT_LITERAL_T, "5"),
                new Token(Token_Class.FLOAT_LITERAL_T, "2.1"), new Token(Token_Class.DIVIDE_T, "/")
            };

            scanner.Tokens.Should().BeEquivalentTo(expected);
        }
    }
}