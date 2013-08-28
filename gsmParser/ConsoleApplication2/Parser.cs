using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConsoleApplication2
{
	internal static class Parser
	{
		public static Exception ResultOk(string str, int ln) { return null; }
		public static Exception ResultFail(string str, int ln) { return new IOException("InvalidFileFormat"); }

		public static Func<string, KeyValuePair<bool, string>> And(
			Func<string, KeyValuePair<bool, string>> clause1,
			Func<string, KeyValuePair<bool, string>> clause2
			)
		{
			return str =>
			{
				var result = clause1(str);
				if (result.Key)
					return clause2(str);
				else return result;
			};
		}

		public static Func<string, KeyValuePair<bool, string>> Or(
			Func<string, KeyValuePair<bool, string>> clause1,
			Func<string, KeyValuePair<bool, string>> clause2
			)
		{
			return str =>
			{
				var result = clause1(str);
				if (!result.Key)
					return clause2(str);
				else return result;
			};
		}

		public static Func<string, int, Exception> When(
			Func<string, KeyValuePair<bool, string>> predicate,
			Func<string, int, Exception> consequent,
			Func<string, int, Exception> alternative)
		{
			return (str, line) =>
			{
				var result = predicate(str);
				return result.Key
					? consequent(result.Value, line)
					: alternative(str, line);
			};
		}

		public static Func<string, KeyValuePair<bool, string>> IsStarts(string signature)
		{
			return str =>
				  str.StartsWith(signature)
					  ? new KeyValuePair<bool, string>(true, str.Substring(signature.Length))
					  : new KeyValuePair<bool, string>(false, str);
		}

		public static Func<string, KeyValuePair<bool, string>> IsContains(string signature)
		{
			return str =>
				  str.IndexOf(signature) > -1
					  ? new KeyValuePair<bool, string>(true, str.Substring(signature.Length))
					  : new KeyValuePair<bool, string>(false, str);
		}

		public static Func<string, int, Exception> Starts(string signature)
		{
			return When(IsStarts(signature), ResultOk, ResultFail);
		}
	}

}
