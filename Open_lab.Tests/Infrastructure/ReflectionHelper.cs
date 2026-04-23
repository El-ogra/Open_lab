using System.Reflection;
using System.Threading.Tasks;

namespace Open_lab.Tests.Infrastructure
{
    public static class ReflectionHelper
    {
        public static Task InvokePrivateAsync(this object instance, string methodName, params object?[] parameters)
        {
            var method = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
                throw new TargetException($"Method '{methodName}' not found on type '{instance.GetType().Name}'.");

            var result = method.Invoke(instance, parameters);
            if (result is Task task)
                return task;

            return Task.CompletedTask;
        }

        public static Task<T> InvokePrivateAsync<T>(this object instance, string methodName, params object?[] parameters)
        {
            var method = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
                throw new TargetException($"Method '{methodName}' not found on type '{instance.GetType().Name}'.");

            var result = method.Invoke(instance, parameters);
            if (result is Task<T> task)
                return task;

            throw new System.InvalidOperationException($"Method '{methodName}' did not return Task<{typeof(T).Name}>.");
        }

        public static void InvokePrivate(this object instance, string methodName, params object?[] parameters)
        {
            var method = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
                throw new TargetException($"Method '{methodName}' not found on type '{instance.GetType().Name}'.");

            method.Invoke(instance, parameters);
        }
    }
}
