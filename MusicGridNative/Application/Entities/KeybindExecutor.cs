namespace MusicGrid
{
    public class KeybindExecutor : Entity
    {
        public override void Update()
        {
            if (ConsoleEntity.Main.ConsoleIsOpen) return;

            foreach (var keybind in Configuration.CurrentConfiguration.Keybinds)
                if (keybind.IsSatisfied())
                    World.Lua.Execute(keybind.Script);
        }
    }
}
