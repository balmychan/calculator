namespace Calculator.Core
{
    public interface ICalculator
    {
        /// <summary>
        /// プラグイン名
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 計算を実行
        /// <summary>
        int Calculate(int a, int b);
    }
}
