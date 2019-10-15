using System;
using Xunit;
using TestLibrary;

namespace TestLibrary.Tests
{
    public class UnitTest1
    {
        [Theory]
        [InlineData(3, 5, 8)]
        public void Add_test(int x, int y, int expected)
        {
            var c = new Calculator();
            int sum = c.Add(x, y);
            Assert.Equal(expected, sum);
        }
    }
}
