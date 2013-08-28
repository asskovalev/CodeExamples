using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CallParser
{
	public class LogItem
	{
		public DateTime Start { get; set; }
		public string Method { get; set; }
		public string Level { get; set; }
		public string Id { get; set; }
		public int Duration { get; set; }

		public string DataFileName { get; set; }
		public int DataLineNumber { get; set; }
		public int DataLineCount { get; set; }

		public override string ToString()
		{
			return string.Format("{0} {1}", Start, Method);
		}
	}
}
