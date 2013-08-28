using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Autostub.Entity;
using Autostub.Entity.Call;
using Autostub.Entity.Repository;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace Autostub
{
	public class StubInterceptor<T> : IInterceptionBehavior
	{
		private readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

		private readonly Type _interceptorType;
		private string _stubPath;
		private string StubPath
		{
			get
			{
				if (string.IsNullOrEmpty(_stubPath))
				{
					_stubPath = System.Configuration.ConfigurationManager.AppSettings["Autostub.StubPath"];
				}
				return _stubPath;
			}
			set
			{
				_stubPath = value;
			}
		}

		public StubInterceptor()
		{
			_interceptorType = typeof(T);
		}

		public string[] SkipMethods { get; set; }

		#region IInterceptionBehavior
		public IEnumerable<Type> GetRequiredInterfaces()
		{
			return Type.EmptyTypes;
		}

		private static bool IsOutput(ParameterInfo pi)
		{
			return pi.IsOut || pi.IsRetval || pi.ParameterType.IsByRef;
		}

		private StubRepository LoadRepository(string methodName, StubModeType? defaultMode)
		{
			var stubMethodPath = GetMethodStubPath(methodName);
			var repository = StubRepository.Load(stubMethodPath, defaultMode);
			return repository;
		}

		private string GetMethodStubPath(string methodName)
		{
			var stubMethodName = string.Format("{0}.xml", methodName);
			var modulePath = Path.Combine(StubPath, _interceptorType.Name);
			var stubMethodPath = Path.Combine(modulePath, stubMethodName);
			return stubMethodPath;
		}

		public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
		{
			var methodName = input.MethodBase.Name;
			var defaultStubMode = GetMethodStubMode(input);

			lock (_locks.GetOrAdd(methodName, k => new object()))
			{
				var rep = LoadRepository(methodName, defaultStubMode);

				var isSkipMethod = SkipMethods != null && SkipMethods.Contains(methodName);
				var isSkipRepository = rep.StubMode == StubModeType.Skip;

				var isSkipped = isSkipMethod || isSkipRepository;

				if (isSkipped || !IsStubbed())
				{
					return getNext().Invoke(input, getNext);
				}


				var call = rep.FindCall(input);
				if (call != null)
				{
					return input.CreateMethodReturn(
						call.GetResult().ExtractValue(),
						call.GetAllOutputs().Select(o => o.ExtractValue()).ToArray());
				}

				var isVoid = ((MethodInfo)input.MethodBase).ReturnType == typeof(void);
				var result = !isVoid
					? getNext().Invoke(input, getNext)
					: input.CreateMethodReturn(null, input.Inputs); ;

				if (result.Exception == null)
				{
					var callInfo = MakeCallStub(input, rep, result);
					rep.Calls.Add(callInfo);
					var stubMethodPath = GetMethodStubPath(methodName);
					rep.Save(stubMethodPath);
				}
				else
					throw new Exception(string.Format("[{0}]{1}\r\n{2}", result.Exception.GetType(), result.Exception.Message,
													  result.Exception.StackTrace));


				return result;
			}

		}

		private static bool IsStubbed()
		{
			var value = System.Configuration.ConfigurationManager.AppSettings["Autostub.IsStubbed"];
			var isStubbed = false;
			Boolean.TryParse(value, out isStubbed);

			return isStubbed;
		}

		private StubModeType? GetMethodStubMode(IMethodInvocation method)
		{
			return method.MethodBase
				.GetCustomAttributes(typeof(StubModeAttribute), true)
				.Select(it => (StubModeType?)((StubModeAttribute)it).Mode)
				.DefaultIfEmpty(null)
				.SingleOrDefault();
		}

		private static CallInfo MakeCallStub(IMethodInvocation input, StubRepository rep, IMethodReturn result)
		{
			var callInfo = CreateCallInfo(input, rep);
			var inargs = CreateInParameters(input, callInfo);
			var outargs = CreateOutParameters(input, callInfo, result);
			var resarg = CreateReturnParameter(input, callInfo, result);
			callInfo.SetParameters(inargs, outargs, resarg);
			return callInfo;
		}

		private static CallInfo CreateCallInfo(IMethodInvocation input, StubRepository rep)
		{
			var callInfo = rep.CreateCall(
				input.MethodBase,
				input.Target.GetType().AssemblyQualifiedName);
			return callInfo;
		}

		private static List<CallParameterInfo> CreateInParameters(IMethodInvocation input, CallInfo callInfo)
		{
			var inargs = input.MethodBase.GetParameters()
				.Select((arg, i) => callInfo.CreateInParameter(arg.Name, arg.ParameterType, input.Arguments[i]))
				.ToList();
			return inargs;
		}

		private static List<CallParameterInfo> CreateOutParameters(IMethodInvocation input, CallInfo callInfo, IMethodReturn result)
		{
			var outargs = input.MethodBase.GetParameters()
				.Where(IsOutput)
				.Select((arg, i) => callInfo.CreateOutParameter(arg.Name, arg.ParameterType, result.Outputs[i]))
				.ToList();
			return outargs;
		}

		private static CallParameterInfo CreateReturnParameter(IMethodInvocation input, CallInfo callInfo, IMethodReturn result)
		{
			var resarg = callInfo.CreateResultParameter(((MethodInfo)input.MethodBase).ReturnType, result.ReturnValue);
			return resarg;
		}

		public bool WillExecute
		{
			get { return true; }
		}
		#endregion
	}
}
