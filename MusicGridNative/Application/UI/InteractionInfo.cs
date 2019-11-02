using SFML.Window;

namespace MusicGrid
{
    public struct InteractionInfo
    {
        public Mouse.Button? Pressed { get; set; }
        public Mouse.Button? Held { get; set; }
        public Mouse.Button? Released { get; set; }

        public bool FirstServed { get; set; }

        public bool HasInteracted => Pressed.HasValue || Held.HasValue || Released.HasValue;
    }
}
