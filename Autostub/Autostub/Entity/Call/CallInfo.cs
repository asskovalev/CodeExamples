using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Autostub.Entity;
using Autostub.Entity.Entity.Call;
using Autostub.Entity.Repository;

namespace Autostub.Entity.Call
{
	public class CallInfo : IReadable<CallInfo>, IRenderable
	{
		private const string NodeName = "call";
		private const string CallName = "name";
		private const string ModuleName = "module";

		public string MethodFullName { get; set; }
		public string Module { get; set; }

		private TypeAliasMap TypeMap { get; set; }

		public CallParameterInfo[] Parameters { get; private set; }

		public CallInfo(TypeAliasMap typemap)
		{
			TypeMap = typemap;
		}


		public CallParameterInfo[] GetInputs()
		{
			return Parameters.Where(p => p.ParameterType == ParameterType.In).ToArray();
		}

		public CallParameterInfo[] GetOutputs()
		{
			return Parameters.Where(p => p.ParameterType == ParameterType.Out).ToArray();
		}

		public CallParameterInfo[] GetAllOutputs()
		{
			var inputs = GetInputs();
			var outputs = GetOutputs().ToDictionary(o => o.Name);

			return inputs
				.Select(i => outputs.ContainsKey(i.Name)
					? outputs[i.Name]
					: i)
				.ToArray();
		}

		public CallParameterInfo GetResult()
		{
			return Parameters.First(p => p.ParameterType == ParameterType.Result);
		}

		public void SetParameters(IList<CallParameterInfo> input, IList<CallParameterInfo> output, CallParameterInfo result)
		{
			Parameters = input
				.Concat(output)
				.Concat(new[] { result })
				.ToArray();
		}

		private CallParameterInfo CreateParameter(ParameterType parameterType, string name, Type type, object value)
		{
			var result = new CallParameterInfo(TypeMap) {ParameterType = parameterType, Name = name};
			result.AssignValue(type, value);
			return result;
		}

		public CallParameterInfo CreateInParameter(string name, Type type, object value)
		{
			var result = CreateParameter(ParameterType.In, name, type, value);
			return result;
		}

		public CallParameterInfo CreateOutParameter(string name, Type type, object value)
		{
			var result = CreateParameter(ParameterType.Out, name, type, value);
			return result;
		}

		public CallParameterInfo CreateResultParameter(Type type, object value)
		{
			var result = CreateParameter(ParameterType.Result, "result", type, value);
			return result;
		}

		public XElement Render()
		{
			return new XElement(NodeName,
				new XAttribute(CallName, MethodFullName),
				new XAttribute(ModuleName, Module),
				Parameters.Select(p => p.Render()));
		}

		public CallInfo Read(XElement src)
		{
			if (src == null)
			{
				throw new ArgumentException("src is not null");
			}

			var atrCallName = src.Attribute(CallName);
			var atrModuleName = src.Attribute(ModuleName);

			if (atrCallName == null || atrModuleName == null)
			{
				throw new Exception(
					string.Format(
						"CallInfo error. XElement CallName or ModuleName is null./n atrModuleName is null {0}./n atrCallName is null = {1}",
						atrModuleName == null, CallName == null));
			}

			MethodFullName = atrCallName.Value;
			Module = atrModuleName.Value;
			Parameters = src.Elements()
				.Select(p => new CallParameterInfo(TypeMap).Read(p))
				.ToArray();

			return this;
		}

	}
}
