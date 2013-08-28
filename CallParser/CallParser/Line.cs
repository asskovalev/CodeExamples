using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CallParser
{
	public enum LineType
	{
		Head,
		Id,
		Duration,
		Method,
		Start,
		End,
		Other,
		Separator
	}

	public class Line
	{
		public LineType Type { get; set; }
		public int LineNumber { get; set; }
		public string Data { get; set; }

		public static Line Head     (int linenum, string data) { return new Line { LineNumber = linenum, Type = LineType.Head,     Data = data }; }
		public static Line Id       (int linenum, string data) { return new Line { LineNumber = linenum, Type = LineType.Id,       Data = data }; }
		public static Line Duration (int linenum, string data) { return new Line { LineNumber = linenum, Type = LineType.Duration, Data = data }; }
		public static Line Method   (int linenum, string data) { return new Line { LineNumber = linenum, Type = LineType.Method,   Data = data }; }
		public static Line Start    (int linenum, string data) { return new Line { LineNumber = linenum, Type = LineType.Start,    Data = data }; }
		public static Line End      (int linenum, string data) { return new Line { LineNumber = linenum, Type = LineType.End,      Data = data }; }
		public static Line Other    (int linenum)              { return new Line { LineNumber = linenum, Type = LineType.Other }; }
		public static Line Separator(int linenum)              { return new Line { LineNumber = linenum, Type = LineType.Separator }; }
	}
}
