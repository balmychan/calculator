using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using Calculator.Core;

namespace Calculator.Host
{
    class Program
    {
        static void Main(string[] args)
        {    
            // 2つの整数をユーザーから得る
            Console.Write("ひとつめの整数を入力してください: ");
            int a = int.Parse(System.Console.ReadLine());
            Console.Write("ふたつめの整数を入力してください: ");
            int b = int.Parse(System.Console.ReadLine());
            Console.WriteLine();

            // どのプラグインをユーザーから得る
            var plugins = LoadPlugins();
            for(var i = 0; i < plugins.Count(); i++)
            {
                var plugin = plugins.ElementAt(i);
                Console.WriteLine($"{i}: {plugin.Name}");
            }
            Console.Write("計算に使用するプラグインを選択してください: ");
            int index = int.Parse(System.Console.ReadLine());
            var targetPlugin = plugins.ElementAt(index);
            Console.WriteLine();

            // プラグインを実行する
            var result = targetPlugin.Calculate(a, b);

            // 結果を表示する
            Console.WriteLine($"結果は {result}です");
        }

        /// <summary>
        /// プラグインを探索し、プラグインのインスタンス一覧を返す
        /// </summary>
        static IEnumerable<ICalculator> LoadPlugins()
        {
            var plugins = GetPluginPaths()
                .Select(path => AssemblyLoadContext.Default.LoadFromAssemblyPath(path))
                .SelectMany(asm => asm.GetTypes())
                .Where(type => typeof(ICalculator).IsAssignableFrom(type))
                .Select(type => Activator.CreateInstance(type) as ICalculator)
                ;
            return plugins;
        }

        /// <summary>
        /// プラグインのDLLのパスを返す
        /// <summary>
        static IEnumerable<string> GetPluginPaths()
        {
            var asm = Assembly.GetExecutingAssembly();
            var pluginRootPath = Path.Combine(Path.GetDirectoryName(asm.Location), @"plugins");
            var pluginDirs = Directory.EnumerateDirectories(pluginRootPath);
            var pluginPattern = new Regex(@"Calculator\.Plugin.*\.dll");
            var pluginPaths = pluginDirs
                .SelectMany(pluginDir => Directory.EnumerateFiles(pluginDir))
                .Where(filePath => pluginPattern.IsMatch(filePath))
                ;
            return pluginPaths;
        }
    }
}
