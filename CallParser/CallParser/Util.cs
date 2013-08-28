using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CallParser
{
	public static class Util
	{
		public static R With<T, R>(this T src, Func<T, R> fn)
			where T : class
			where R : class
		{
			if (src == null)
				return null;
			return fn(src);
		}
	}
}
