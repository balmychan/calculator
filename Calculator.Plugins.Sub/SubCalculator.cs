using System;
using Calculator.Core;

namespace Calculator.Plugins.Sub
{
    public class SubCalculator : ICalculator
    {
        public string Name { get { return "引き算"; } }

        public int Calculate(int a, int b)
        {
            return a - b;
        }
    }
}
