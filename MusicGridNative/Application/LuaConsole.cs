using NLua;
using System;
using System.Reflection;

namespace MusicGrid
{
    public class LuaConsole : ILuaConsole
    {
        private readonly Lua state;

        public LuaConsole()
        {
            state = new Lua();
            ConsoleEntity.Log("Lua initialised: " + state.State.Status, this);
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

        public void LinkFunction(string name, object target, Action action)
        {
            state.RegisterFunction(name, target, action.Method);
        }

        public void LinkFunction<T>(string name, object target, Func<T> function)
        {
            state.RegisterFunction(name, target, function.Method);
        }

        public object[] Execute(string code)
        {
            object[] result;
            try
            {
                result = state.DoString(code);
            }
            catch (Exception e)
            {
                result = new[] { e.Message };
            }
            return result;
        }
    }
}
