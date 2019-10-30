using System;
using System.Reflection;

namespace MusicGrid
{
    public interface ILuaConsole
    {
        object[] Execute(string code);
        T GetGlobal<T>(string name);
        void LinkFunction(string name, object target, MethodBase method);
        void LinkFunction(string name, object target, Action action);
        void LinkFunction<T>(string name, object target, Action<T> action);
        void LinkFunction<T>(string name, object target, Func<T> function);
        void SetGlobal(string name, object value);
    }
}