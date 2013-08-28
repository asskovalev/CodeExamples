using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Histogram.Common
{
	public class HistogramElement
	{
		public int Value { get; set; }
		public int Count { get; set; }

		public override string ToString()
		{
			return string.Format("{0}:{1}", Value, Count);
		}

		public static HistogramElement FromString(string src)
		{
			if (!src.Contains(':'))
				throw new ArgumentOutOfRangeException("src", "source string should fit the form Value:Count");
			var parts = src.Split(':');
			return new HistogramElement()
			{
				Value = Convert.ToInt32(parts[0].Trim()),
				Count = Convert.ToInt32(parts[1].Trim())
			};
		}
	}
}
