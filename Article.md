---
layout: post
title:  "プラグインによる拡張が可能なプログラムを書いてみよう"
date:   2019-12-22 9:30:00 +0900
author: Ayumi Goto
---

# はじめに

`プラグイン`という言葉をご存知でしょうか。

`既存のアプリケーションに対して、機能を拡張するソフトウェア`のことを指します。例えばChromeにあるChrome拡張もプラグインの一種です。

Photoshop、Chrome、Unityなど、特にこういったデスクトップアプリケーションの多くは、プラグインを組み込むことができる仕組みが提供されており、提供元では用意しきれない便利な機能をアプリケーションに追加することができるようになっています。

多くの基幹システム（例えばSalesforceなど）でも、プラグインによってカスタマイズを可能にし、お客さんの数多の要望を叶える仕組みが備わっています。

そんなプラグインによる拡張の仕組みを、今回簡単なプログラムを通して、学んでいきたいと思います。

# どういうプログラムを作るか

今回は下記のような単純なプログラムと、それを拡張するプラグインを作っていきたいと思います。

- 2つの整数を受け取って`何らかの計算`を行い、計算結果を整数で返すプログラム
- `何らかの計算`はプラグインで拡張可能にする

# 作ってみよう

`プラグインによる拡張の仕組みを作ろう`となると、なんだかややこしそうで難しそうな気がしますが、要点をまとめるとこんなに単純です（特に、プログラマの人が見たら簡単で拍子抜けするくらいです）

1. プラグインを呼び出すインターフェースを決める
2. ホストプログラムを作成する（プラグインの探索と実行）
3. プラグインを実装する、配置する

ひとつひとつ、プログラムコードも交えて説明しましょう。
また、今回は .NET Core で開発しているので、言語はC#を用います。

## 1. プラグインを呼び出すインターフェースを決める

プラグインの仕組みを作るには、まずはこの`インターフェースを決める`から始まります。

今回は整数2つを受け取って整数を返す計算処理をプラグインで拡張可能にするため、下記のようなインターフェースになります（識別するためのプラグイン名も含めましょう）

```cs
namespace Calculator.Core
{
    public interface ICalculator
    {
        /// <summary>
        /// プラグイン名
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 計算処理
        /// <summary>
        int Calculate(int a, int b);
    }
}
```

## 2. ホストプログラムを作成する（プラグインの探索と実行）

今回用意したホストプログラムは単純です。

- ユーザーから整数2つを受け取る
- プラグイン一覧を表示し、ユーザーにプラグインを選択させる
- プラグインによる計算結果を画面に表示する

という感じです。画面を用意するのも面倒ですので、コンソールアプリケーションです。
また、プラグインはホストプログラムと同フォルダの`plugins`フォルダに配置するというルールにしました。

```cs
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
```

## 3. プラグインを実装する、配置する

まずは整数を足し算するプラグインを作ってみましょう

```cs
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
```

めっちゃ単純ですね。これでプラグインが完成です。これをビルドし、ホストプログラムの`plugins`フォルダに配置します。

## 実行

実行してみましょう。

```ps
> dotnet run
ひとつめの整数を入力してください: 5
ふたつめの整数を入力してください: 12

0: 足し算
計算に使用するプラグインを選択してください: 0

結果は 17です
```

5と12が足し算されて17が表示されました！！

## 更にプラグインを追加してみる

下記2つのプラグインを更に追加してみます。

__引き算プラグイン__

```cs
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
```

__掛け算プラグイン__

```cs
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
```

`plugins`フォルダにそれぞれ配置します。配置後の`plugins`フォルダはこんな感じになっています。

```
plugins
  Calculator.Add
    Calculator.Plugins.Add.dll
  Calculator.Mul
    Calculator.Plugins.Mul.dll
  Calculator.Sub
    Calculator.Plugins.Sub.dll
```

もう一度実行してみましょう。

```ps
> dotnet run
ひとつめの整数を入力してください: 5
ふたつめの整数を入力してください: 10

0: 足し算
1: 掛け算
2: 引き算
計算に使用するプラグインを選択してください: 1

結果は 50です
```

```ps
> dotnet run
ひとつめの整数を入力してください: 5
ふたつめの整数を入力してください: 10

0: 足し算
1: 掛け算
2: 引き算
計算に使用するプラグインを選択してください: 2

結果は -5です
```

`plugins`フォルダに配置するだけで、プラグインが追加され、使えるようになっています！

## 終わりに

いかがでしたでしょうか、めっちゃ簡単だったかと思います。もしかしたら色々プラグインにしてみたくなったんじゃないでしょうか（行き過ぎると、すべての関数をプラグイン化したくなるかもしれません）

コードだけ見ていると分かりづらいですが、ホストプログラムから各プラグインのクラスは参照しておらず、その存在は知りません（依存関係が逆転している）それでも、実行時にプラグインを読み込み、実行する、ということができています。

また、今回は .NET Core(C#) で書きましたが、どの言語でも要点は同じです。プラグインのインターフェースを定め、ホストプラグラムではプラグインを探索し、それを実行時に読み込み、利用する。です。

注意点として、実運用では実行時のセキュリティには気を配ったほうが良いでしょう。悪意のあるプラグインや脆弱性のあるプラグインがあった場合、不正にファイルが破壊されたり、データが消されたりなど、セキュリティホールに繋がります。プラグインに対してホストプログラムと同等の権限を与えるということは、通常しないでしょう。

ぜひ皆さんの好きな言語でも、プラグインの仕組みを作ってみてください！

[今回作成したサンプルプログラムはこちら](https://github.com/balmychan/calculator)
