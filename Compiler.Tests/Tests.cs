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
        private List<String> tokenList = new List<string>();
        
        [Fact]
        public void TestReservedKeywords()
        {
            var mockService = new Mock<Scanner>();
            
            string solution =
                @"
                   int float string read write repeat 
                   until if else then end return endl
                 ";
            
            /*
             * Sets up a stub method to replace the token list
             * It doesn't tokenize but instead passes the "tokens" as strings
             */
            mockService.Setup(x => x.FindTokenClass(It.IsAny<String>())).Callback<String>(
                (passedValue) =>
            {
                tokenList.Add(passedValue);
            });
            
            mockService.Object.StartScanning(solution);
            
            /*
             * Extracts the words from the solution to compare
             */
            List<String> ansList = solution.Split(null as char[], StringSplitOptions.RemoveEmptyEntries).ToList();
            
            tokenList.Should().BeEquivalentTo(ansList);
        }
    }
}