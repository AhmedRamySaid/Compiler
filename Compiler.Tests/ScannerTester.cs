using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JASON_Compiler;
using Moq;
using Xunit;

namespace Compiler.Tests
{
    public class ScannerTester
    {
        /***** Sets up stub function for the regex *****/
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
        
        /***** Tests every single reserved keyword *****/
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
        
        /***** Tests an example code snippet *****/
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
        
        /***** Tests if comments are correctly deleted *****/
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
        
        /***** Tests single character operators *****/
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
         * Tests numbers
         * As per documentation, multiple '.' are valid inside the scanner
         * Handled inside regex
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
    }
}