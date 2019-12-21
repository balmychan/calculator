using System;
using Calculator.Core;

namespace Calculator.Plugins.Mol
{
    public class MulCalculator : ICalculator
    {
        public string Name { get { return "掛け算"; } }

        public int Calculate(int a, int b)
        {
            return a * b;
        }
    }
}