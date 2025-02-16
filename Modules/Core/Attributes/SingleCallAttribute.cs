using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace NDBotUI.Modules.Core.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class SingleCallAttribute : Attribute
{
    private static readonly ConcurrentDictionary<MethodInfo, bool> CalledMethods = new();

    public static bool InvokeIfNotCalled(MethodInfo method, object[] parameters)
    {
        if (CalledMethods.ContainsKey(method))
        {
            return false; // Method đã được gọi trước đó
        }

        if (!method.IsStatic)
        {
            throw new InvalidOperationException("SingleCallAttribute only supports static methods");
        }

        CalledMethods[method] = true;
        method.Invoke(null, parameters);
        return true;
    }
}