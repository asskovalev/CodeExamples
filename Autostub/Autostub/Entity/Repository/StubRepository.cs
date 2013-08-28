using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Autostub.Entity;
using Autostub.Entity.Call;
using Autostub.Entity.Entity.Call;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace Autostub.Entity.Repository
{
	public class StubRepository : IReadable<StubRepository>, IRenderable
	{
		private const string NodeName = "stub";
		private const string CallsName = "calls";
		private const string StubModeName = "type";
		private const string DescrName = "description";

		public TypeAliasMap TypeMap { get; set; }
		public string Description { get; set; }

		private StubModeType? stubMode;
		public StubModeType StubMode
		{
			get { return stubMode ?? StubModeType.MethodSignature; }
			set { stubMode = value; }
		}

		public List<CallInfo> Calls { get; set; }

		public CallInfo CreateCall(MethodBase methodBase, string module)
		{
			string methodFullName = GetMethodFullName(methodBase);

			var result = new CallInfo(TypeMap) { MethodFullName = methodFullName, Module = module };
			return result;
		}

		public CallInfo FindCall(IMethodInvocation invocationInfo)
		{
			switch (StubMode)
			{
				case StubModeType.MethodName:
					return FindCallByName(invocationInfo.MethodBase);
				case StubModeType.MethodSignature:
					return FindCallBySignature(invocationInfo.MethodBase);
				case StubModeType.MethodArguments:
					return FindCallByArguments(invocationInfo);
				default:
					throw new InvalidOperationException("Unknown StubMode: " + StubMode.ToString());
			}
		}

		private CallInfo FindCallByName(MethodBase method)
		{
			return Calls.FirstOrDefault(c => c.MethodFullName == GetMethodFullName(method));
		}

		#region поиск метода в репозитории
		private CallInfo FindCallBySignature(MethodBase method)
		{
			var methodSig = method.GetParameters().Select(pi => pi.ParameterType).ToArray();

			var call = Calls.FirstOrDefault(c =>
				{
					string methodFullName = GetMethodFullName(method);

					if (methodFullName != c.MethodFullName)
					{
						return false;
					}


					var s1 = c.Parameters.Where(p => p.ParameterType == ParameterType.In)
						.Select(p => TypeMap[p.TypeAlias]).ToArray();

					if (s1.Length != methodSig.Length)
						return false;

					return s1
						.Select((t, i) => methodSig[i].IsAssignableFrom(t) || methodSig[i].Name.Replace("&", "") == t.Name)
						.All(x => x);
				});
			return call;
		}

		private CallInfo FindCallByArguments(IMethodInvocation invocationInfo)
		{
			var method = (MethodInfo)invocationInfo.MethodBase;
			var methodSig = method.GetParameters().Select(pi => pi.ParameterType).ToArray();
			var c = Calls.FirstOrDefault(call =>
			{
				string methodFullName = GetMethodFullName(method);

				if (methodFullName != call.MethodFullName)
				{
					return false;
				}

				var s1 = call.Parameters.Where(p => p.ParameterType == ParameterType.In)
					.Select(p => TypeMap[p.TypeAlias]).ToArray();

				if (s1.Length != methodSig.Length)
					return false;

				if (!s1
					.Select((t, i) => methodSig[i].IsAssignableFrom(t) || methodSig[i].Name.Replace("&", "") == t.Name)
					.All(x => x))
					return false;

				var inputs = call.GetInputs().Select(inp => inp.Value).ToArray();
				var actualInputs = invocationInfo.MethodBase.GetParameters()
					.Select((arg, i) => call.CreateInParameter(arg.Name, arg.ParameterType, invocationInfo.Arguments[i]))
					.Select(p => p.Value)
					.ToArray();

				if (!s1
					.Select((t, i) => inputs[i].Value == actualInputs[i].Value)
					.All(x => x))
					return false;

				return true;
			});
			return c;
		}

		#endregion
		public XElement Render()
		{
			return new XElement(NodeName,
				new XAttribute(StubModeName, StubMode.ToString()),
				new XElement(DescrName, Description),
				TypeMap.Render(),
				new XElement(CallsName,
					Calls.Select(c => c.Render()).ToArray()));
		}

		public StubRepository Read(XElement src)
		{

			var typeMapNode = src.Element(TypeAliasMap.NodeName);
			var typeMap = new TypeAliasMap();

			if (typeMapNode != null)
				typeMap.Read(src.Element(TypeAliasMap.NodeName));

			TypeMap = typeMap;


			var atrCallsName = src.Element(CallsName);
			Calls = new List<CallInfo>();
			if (atrCallsName != null)
			{
				Calls = atrCallsName.Elements("call")
					.Select(c => new CallInfo(typeMap).Read(c))
					.ToList();
			}


			var descrNode = src.Element(DescrName);
			if (descrNode != null)
				Description = descrNode.Value;


			if (src.Attribute(StubModeName) != null)
			{
				StubModeType mode;
				if (Enum.TryParse<StubModeType>(src.Attribute(StubModeName).Value, out mode))
					StubMode = mode;
			}

			return this;
		}
        
		public static StubRepository Load(string path, StubModeType? defaultMode)
		{
			var result = new StubRepository();
			if (File.Exists(path))
				result.Read(XDocument.Load(path).Root);
			else
			{
				result.TypeMap = new TypeAliasMap();
				result.Calls = new List<CallInfo>();
                result.StubMode = defaultMode ?? StubModeType.MethodSignature;
			}
			return result;
		}

		public void Save(string path)
		{
			var doc = new XDocument();
			doc.Add(Render());
			doc.Save(path);
		}

		public static string GetMethodFullName(MethodBase methodBase)
		{
			string result = methodBase.Name;

			if (methodBase.IsGenericMethod)
			{
				result += '[' + String.Join(", ", methodBase.GetGenericArguments().Select(GetTypeName)) + ']';
			}

			return result;
		}

		private static string GetTypeName(Type type)
		{
			if (!type.IsGenericType)
			{
				return type.Name;
			}

			string genericTypeName = type.GetGenericTypeDefinition().Name;

			genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));

			var genericArguments = type.GetGenericArguments();
			string genericArgs = string.Join(", ", genericArguments.Select(GetTypeName));
			return genericTypeName + '[' + genericArgs + ']';
		}
	}
}
