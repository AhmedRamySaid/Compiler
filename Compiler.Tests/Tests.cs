using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JASON_Compiler;
using Moq;
using Xunit;

namespace Compiler.Tests
{
    public class Tests
    {
        private List<string> ScanTokens(string input)
        {
            var tokens = new List<string>();
            var mockScanner = new Mock<Scanner>();

            /*
             * Sets up a stub method to replace the token list
             * It doesn't tokenize but instead passes the "tokens" as strings
             */
            mockScanner
                .Setup(x => x.FindTokenClass(It.IsAny<string>()))
                .Callback<string>(t => tokens.Add(t));

            mockScanner.Object.StartScanning(input);

            return tokens;
        }
        
        [Fact]
        public void TestReservedKeywords()
        {
            string input = 
                @"
                    int float string read write repeat
                    until if else then end return endl
                 ";

            var result = ScanTokens(input);

            /*
             * Extracts the words from the solution to compare them together
             */
            var expected = input
                .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            result.Should().BeEquivalentTo(expected);
        }
        
        [Fact]
        public void TestBasicCode()
        {
            string input = @"int x = 5 + 6;";

            var result = ScanTokens(input);

            var expected = new List<string>
            {
                "int", "x", "=", "5", "+", "6", ";"
            };

            result.Should().BeEquivalentTo(expected);
        }
        
        [Fact]
        public void TestComments()
        {
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

            var result = ScanTokens(input);

            var expected = new List<string>
            {
                "int", "x", "=", "10", "-", "3", ";",
                "float", "y", "=", "5", "+", "2", ";"
            };

            result.Should().BeEquivalentTo(expected);
        }
        
        /*
         * Fails!
         * '/' does not work
         * todo: fix
         */
        [Fact]
        public void TestSingleCharacterOperators()
        {
            string input = 
                @"
                    int a = 10 + 3;
                    float b = 5 - 2;
                    int c = 12 * 6;
                    float d = 1 / 2;
                 ";

            var result = ScanTokens(input);

            var expected = new List<string>
            {
                "int", "a", "=", "10", "+", "3", ";",
                "float", "b", "=", "5", "-", "2", ";",
                "int", "c", "=", "12", "*", "6", ";",
                "float", "d", "=", "1", "/", "2", ";"
            };

            result.Should().BeEquivalentTo(expected);
        }
        
        /*
         * As per documentation, 0.14290.142 is valid so far and should not
         * be handled inside the scanner
         */
        [Fact]
        public void TestNumbers()
        {
            string input = 
                @"
                    int a = 0104091;
                    float b = 15.2;
                    float c = 0.14290.142;
                 ";

            var result = ScanTokens(input);

            var expected = new List<string>
            {
                "int", "a", "=", "0104091", ";",
                "float", "b", "=", "15.2", ";",
                "float", "c", "=", "0.14290.142", ";"
            };

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void TestRegex()
        {
            Scanner scanner = new Scanner();
            
            /***** Test Identifiers ******/
            // Succeeded!

            // case 1: valid
            scanner.isIdentifier("isValid").Should().BeTrue();

            // case 2: starts with digit should be false
            scanner.isIdentifier("1notValid").Should().BeFalse();

            // case 3: contains digits valid
            scanner.isIdentifier("is1Valid").Should().BeTrue();

            // case 4: contains punctuation isn't valid as per the language specs
            scanner.isIdentifier("is-not-valid").Should().BeFalse();

            /***** Test Numbers *********/
            // Succeeded!

            // case 1: valid integers
            scanner.isConstant("1").Should().BeTrue(); // single digit
            scanner.isConstant("123").Should().BeTrue(); // multiple digits

            // case 2: invalid integers
            scanner.isConstant("1D").Should().BeFalse();

            // case 2: valid floats
            scanner.isConstant("123.123").Should().BeTrue();

            // case 3: invalid floats
            scanner.isConstant("123.123.123").Should().BeFalse();
            scanner.isConstant(".123").Should().BeFalse();

            /***** Test Strings *********/
            scanner.isString(@"""123blabla123""").Should().BeTrue();
            scanner.isString(@"""""").Should().BeTrue();

            scanner.isString(@"123""123blabla123""").Should().BeFalse();
            scanner.isString(@"""123blabla123""123").Should().BeFalse();


            /***** Test operators & punctuation *******/
            
            // note: only two tests needed (equivalnce partioning)
            scanner.isOperator("+").Should().BeTrue();
            scanner.isOperator("a").Should().BeFalse();

        }
    }
}