using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Collections.Generic;

namespace MusicGridNative
{
    public static class Input
    {
        private static RenderWindow window;

        public static float MouseWheelDelta { get; private set; }

        public static Vector2f MousePosition { get; private set; }
        public static Vector2i ScreenMousePosition { get; private set; }

        private static HashSet<Keyboard.Key> PressedKeys = new HashSet<Keyboard.Key>();
        private static HashSet<Keyboard.Key> HeldKeys = new HashSet<Keyboard.Key>();
        private static HashSet<Keyboard.Key> ReleasedKeys = new HashSet<Keyboard.Key>();

        private static HashSet<Mouse.Button> PressedButtons = new HashSet<Mouse.Button>();
        private static HashSet<Mouse.Button> HeldButtons = new HashSet<Mouse.Button>();
        private static HashSet<Mouse.Button> ReleasedButtons = new HashSet<Mouse.Button>();

        public static void SetWindow(RenderWindow window)
        {
            if (window != null)
            {
                window.MouseWheelScrolled -= MouseWheelScrolled;
                window.MouseMoved -= MouseMoved;

                window.KeyPressed -= KeyPressed;
                window.KeyReleased -= KeyReleased;

                window.MouseButtonPressed -= MouseButtonPressed;
                window.MouseButtonReleased -= MouseButtonReleased;
            }

            Reset();

            Input.window = window;

            window.MouseWheelScrolled += MouseWheelScrolled;
            window.MouseMoved += MouseMoved;

            window.KeyPressed += KeyPressed;
            window.KeyReleased += KeyReleased;

            window.MouseButtonPressed += MouseButtonPressed;
            window.MouseButtonReleased += MouseButtonReleased;
        }

        public static void Reset()
        {
            PressedButtons.Clear();
            ReleasedButtons.Clear();

            PressedKeys.Clear();
            ReleasedKeys.Clear();
        }

        public static bool IsKeyHeld(Keyboard.Key key) => HeldKeys.Contains(key);
        public static bool IsKeyPressed(Keyboard.Key key) => PressedKeys.Contains(key);
        public static bool IsKeyReleased(Keyboard.Key key) => ReleasedKeys.Contains(key);

        public static bool IsButtonHeld(Mouse.Button button) => HeldButtons.Contains(button);
        public static bool IsButtonPressed(Mouse.Button button) => PressedButtons.Contains(button);
        public static bool IsButtonReleased(Mouse.Button button) => ReleasedButtons.Contains(button);

        private static void MouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            HeldButtons.Remove(e.Button);
            ReleasedButtons.Add(e.Button);
        }

        private static void MouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            HeldButtons.Add(e.Button);
            PressedButtons.Add(e.Button);
        }

        private static void KeyReleased(object sender, KeyEventArgs e)
        {
            HeldKeys.Remove(e.Code);
            ReleasedKeys.Add(e.Code);
        }

        private static void KeyPressed(object sender, KeyEventArgs e)
        {
            HeldKeys.Add(e.Code);
            PressedKeys.Add(e.Code);
        }

        private static void MouseMoved(object sender, MouseMoveEventArgs e)
        {
            ScreenMousePosition = new Vector2i(e.X, e.Y);
            MousePosition = window.MapPixelToCoords(ScreenMousePosition);
        }

        private static void MouseWheelScrolled(object sender, MouseWheelScrollEventArgs e)
        {
            MouseWheelDelta = e.Delta;
        }
    }
}
;