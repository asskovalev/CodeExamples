using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;

namespace CallParser
{
	public static class Program
	{			
		const string PatternDefaultNs = "Foris.TelCrm.ConvergentSale.";

		static void Main(string[] args)
		{
			if (args.Length != 1)
			{
				PrintHelp();
				return;
			}

			var logdir = args[0] == "!test"
				? "TestData"
				: args[0];

			var all = Directory
				.EnumerateFiles(logdir)
				.With(LogReader.ParseLines)
				.ToList();

			var bld = new LogTreeNode();
			all.ForEach(it => bld.Attach(it));
			var tree = bld.GetContent();

			tree.Children.ForEach(PrintItem);
		}

		static void PrintItem(TreeLogItem item) { PrintItem(item, 0); Console.WriteLine(); }
		static void PrintItem(TreeLogItem item, int level)
		{
			var lvl_offset = new string(' ', (level + 1) * 2);
			if (item.Unresolved)
			{
				Console.WriteLine(string.Format("{0}{1}{2}{3}",
					item.Start,
					item.Duration.ToString().PadLeft(5),
					lvl_offset,
					"Unresolved"));
			}
			else
			{
				Console.WriteLine(string.Format("{0}{1}{2}{3}",
					item.Start,
					item.Duration.ToString().PadLeft(5),
					lvl_offset,
					item.Method.Replace(PatternDefaultNs, "")));
			}
			if (item.Children != null)
				item.Children.OrderBy(it => it.Start).ToList().ForEach(it => PrintItem(it, level + 1));
		}

		static void PrintHelp()
		{
			Console.WriteLine("Usage: CallParser <path-to-logdir> to parse");
			Console.WriteLine("       CallParser !test to test");
		}
	}
}
