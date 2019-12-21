using System;
using Calculator.Core;

namespace Calculator.Plugins.Add
{
    public class AddCalculator : ICalculator
    {
        public string Name { get { return "足し算"; } }

        public int Calculate(int a, int b)
        {
            return a + b;
        }
    }
}
