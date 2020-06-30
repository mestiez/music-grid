using System;
using System.Linq;

namespace MusicGrid
{
    public class KeybindExecutor : Entity
    {
        public void Bind(string[] keys, string script)
        {
            try
            {
                Configuration.CurrentConfiguration.Keybinds = Configuration.CurrentConfiguration.Keybinds.Append(
                            new Configuration.Keybind(keys.Select(k => (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), k)).ToArray(),
                            script)
                            ).ToArray();
            }
            catch (ArgumentException e)
            {
                ConsoleEntity.Log(e, this);
            }
            
        }

        public override void Update()
        {
            if (ConsoleEntity.Main.ConsoleIsOpen || World.GetEntitiesByType<UiControllerEntity>().Any(u => u.UserIsInputting)) return;

            foreach (var keybind in Configuration.CurrentConfiguration.Keybinds)
                if (keybind.IsSatisfied())
                    World.Lua.Execute(keybind.Script);
        }
    }
}
