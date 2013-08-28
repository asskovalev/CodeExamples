using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Histogram.Common
{
	public static class Extensions
	{
		public static IEnumerable<List<T>> SplitBy<T>(this IEnumerable<T> src, int batchSize)
		{
			var acc = new List<T>(batchSize);
			foreach (var item in src)
			{
				acc.Add(item);
				if (acc.Count == batchSize)
				{
					yield return acc;
					acc = new List<T>();
				}
			}
		}

		public static void Foreach<T>(this IEnumerable<T> src, Action<T> fn)
		{
			foreach (var item in src)
			{
				fn(item);
			}
		}

		public static R With<T, R>(this T src, Func<T, R> fn)
		{
			return fn(src);
		}

		public static T Do<T>(this T src, Action<T> fn)
		{
			fn(src);
			return src;
		}
	}
}
