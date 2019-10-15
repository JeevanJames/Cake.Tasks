using System;
using System.Linq;

namespace TestLibrary
{
    public sealed class Calculator
    {
        public int Add(int x, params int[] nums)
        {
            int sum = x + nums.Sum();
            return sum;
        }
    }
}
