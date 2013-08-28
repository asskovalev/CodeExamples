using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CallParser
{
	public class LogTreeNode
	{
		private Dictionary<string, LogTreeNode> Children;
		protected LogItem Value;

		public LogTreeNode()
		{
			Children = new Dictionary<string, LogTreeNode>();
		}

		public void Attach(LogItem item)
		{
			var path = item.Id.Split('.');
			var child = GetChild(path);
			child.Value = item;
		}

		public TreeLogItem GetContent()
		{
			var result = Value == null
				? new TreeLogItem()
				: new TreeLogItem(Value);

			result.Children = Children.Values
				.Select(it => it.GetContent())
				.OrderBy(it => it.Start)
				.ToList();

			return result;
		}

		private LogTreeNode GetChild(string[] path)
		{
			if (path.Length == 0)
				return this;

			if (!Children.ContainsKey(path[0]))
				Children[path[0]] = new LogTreeNode();

			return Children[path[0]].GetChild(path.Skip(1).ToArray());
		}
	}
}
