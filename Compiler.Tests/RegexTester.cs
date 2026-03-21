using FluentAssertions;
using JASON_Compiler;
using Xunit;

namespace Compiler.Tests
{
    public class RegexTester
    {
        /***** Test Identifiers *****/
        [Fact]
        public void TestIdentifier()
        {
            Scanner scanner = new Scanner();

            // case 1: valid
            scanner.isIdentifier("isValid").Should().BeTrue();

            // case 2: starts with digit should be false
            scanner.isIdentifier("1notValid").Should().BeFalse();

            // case 3: contains digits valid
            scanner.isIdentifier("is1Valid").Should().BeTrue();

            // case 4: contains punctuation isn't valid as per the language specs
            scanner.isIdentifier("is-not-valid").Should().BeFalse();
        }
        
        /***** Test Strings *****/
        [Fact]
        public void TestString()
        {
            Scanner scanner = new Scanner();
            
            scanner.isString(@"""123blabla123""").Should().BeTrue();
            scanner.isString(@"""""").Should().BeTrue();

            scanner.isString(@"123""123blabla123""").Should().BeFalse();
            scanner.isString(@"""123blabla123""123").Should().BeFalse();
        }
        
        /***** Test Numbers *****/
        [Fact]
        public void TestNumbers()
        {
            Scanner scanner = new Scanner();

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
        }

        /***** Test operators & punctuation *****/
        [Fact]
        public void TestSymbols()
        {
            Scanner scanner = new Scanner();
            
            // note: only two tests needed (equivalnce partioning)
            scanner.isOperator("+").Should().BeTrue();
            scanner.isOperator("a").Should().BeFalse();
        }
    }
}