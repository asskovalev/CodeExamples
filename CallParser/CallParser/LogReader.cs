using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;

namespace CallParser
{
	public class LogFileInfo
	{
		public string Filename { get; set; }

		public LogFileInfo(string filename)
		{
			Filename = filename;
		}

		public IEnumerable<string> GetLines()
		{
			return File.ReadLines(Filename);
		}
	}

	public class LogReader
	{
		const string PatternMethod     = "Method call: ";
		const string PatternId         = "Call identity: ";
		const string PatternDuration   = "Call duration = ";
		const string PatternStartTime  = "Start time = ";
		const string PatternSeparator  = "=======================================";
		const string PatternEndTime    = "End time = ";
		const string PatternHead       = @"(\d\d\d\d-\d\d-\d\d \d\d:\d\d:\d\d[.,]\d\d\d) \[([^\s]*)\] ([^\s]*) (.*)";
		const string DefaultDateFormat = @"yyyy.MM.dd HH:mm:ss,fff";

		static LineType[] PatternItem = new LineType[] { 
			LineType.Head, 
			LineType.Separator,
			LineType.Method,
			LineType.Id,
			LineType.Duration,
			LineType.Start, 
			LineType.End,
			LineType.Separator };

		public static IEnumerable<LogItem> ParseLines(IEnumerable<string> filenames)
		{
			return filenames
				.Select(filename => filename
					.With(File.ReadLines)
					.Select((str, num) => LogReader.ToLine(num, str))
					.With(LogReader.GroupItemLines)
					.Where(LogReader.IsLinesAcceptable)
					.Select(it => ToItem(filename, it)))
				.SelectMany(it => it);
		}

		static Line ToLine(int linenum, string it)
		{
			if (it.StartsWith(PatternEndTime))   return Line.End      (linenum, it.Substring(PatternEndTime.Length));
			if (it.StartsWith(PatternMethod))    return Line.Method   (linenum, it.Substring(PatternMethod.Length));
			if (it.StartsWith(PatternId))        return Line.Id       (linenum, it.Substring(PatternId.Length));
			if (it.StartsWith(PatternDuration))  return Line.Duration (linenum, it.Substring(PatternDuration.Length).Split(' ')[0]);
			if (it.StartsWith(PatternStartTime)) return Line.Start    (linenum, it.Substring(PatternStartTime.Length));
			if (it == PatternSeparator)          return Line.Separator(linenum);

			var is_start = Regex.Matches(it, PatternHead);
			if (is_start.Count == 1)
				return Line.Head(linenum, is_start[0].Groups[3].Value);

			return Line.Other(linenum);
		}

		static bool IsLinesAcceptable(List<Line> lines)
		{
			return lines.Select(l => l.Type).SequenceEqual(PatternItem);
		}

		static IEnumerable<List<Line>> GroupItemLines(IEnumerable<Line> src)
		{
			var acc = new List<Line>();
			var counter = 0;
			foreach (var it in src)
			{
				if (it.Type == LineType.Head)
				{
					if (counter > 0)
					{
						yield return acc;
						acc = new List<Line>();
					}
					acc.Add(it);
					counter++;
				}

				else if (it.Type != LineType.Other)
					acc.Add(it);
			}
		}

		static LogItem ToItem(string filename, IEnumerable<Line> src)
		{
			var result = new LogItem();
			var sepCounter = 0;
			var dataStart = 0;
			var dataEnd = 0;
			foreach (var item in src)
			{
				switch (item.Type)
				{
					case LineType.Head:      result.Level    = item.Data; break;
					case LineType.Method:    result.Method   = item.Data; break;
					case LineType.Id:        result.Id       = item.Data; break;
					case LineType.Duration:  result.Duration = ParseDuration(item.Data); break;
					case LineType.Start:     result.Start    = ParseDate(item.Data); break;
					case LineType.End:
						dataStart = item.LineNumber + 1;
						break;
					case LineType.Separator:
						if (sepCounter == 1)
							dataEnd = item.LineNumber;
						sepCounter++; break;
					default: break;
				}
			}

			result.DataFileName = filename;
			result.DataLineNumber = dataStart;
			result.DataLineCount = dataEnd - dataStart;

			return result;
		}

		static DateTime ParseDate(string src)
		{
			return DateTime.ParseExact(src, DefaultDateFormat, Thread.CurrentThread.CurrentCulture);
		}

		static Int32 ParseDuration(string src)
		{
			return Int32.Parse(src);
		}
	}
}
