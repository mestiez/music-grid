using NLua;
using System;
using System.Linq;
using System.Reflection;

namespace MusicGridNative
{
    public class LuaConsole
    {
        private readonly Lua state;

        public LuaConsole()
        {
            state = new Lua();
            ConsoleEntity.Show("Lua initialised");
        }

        public void SetGlobal(string name, object value)
        {
            state[name] = value;
        }

        public T GetGlobal<T>(string name)
        {
            return (T)state[name];
        }

        public void LinkFunction(string name, object target, MethodBase method)
        {
            state.RegisterFunction(name, target, method);
        }

        public void LinkFunction<T>(string name, object target, Action<T> action)
        {
            state.RegisterFunction(name, target, action.Method);
        }

        public void LinkFunction<T>(string name, object target, Func<T> function)
        {
            state.RegisterFunction(name, target, function.Method);
        }

        public object[] Execute(string code)
        {
            var result = state.DoString(code);
            return result;
        }
    }
}
