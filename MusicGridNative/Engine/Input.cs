using OpenTK;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicGrid
{
    public static class Input
    {
        private static RenderWindow window;
        private static NativeWindow nativeWindow;

        public static float MouseWheelDelta { get; private set; }

        public static Vector2f MousePosition { get; private set; }
        public static Vector2f PreviousMousePosition { get; private set; }
        public static Vector2f MouseDelta { get; private set; }

        public static Vector2i ScreenMousePosition { get; private set; }
        public static Vector2i PreviousScreenMousePosition { get; private set; }
        public static Vector2i ScreenMouseDelta { get; private set; }

        private static readonly HashSet<OpenTK.Input.Key> PressedKeys = new HashSet<OpenTK.Input.Key>();
        private static readonly HashSet<OpenTK.Input.Key> HeldKeys = new HashSet<OpenTK.Input.Key>();
        private static readonly HashSet<OpenTK.Input.Key> ReleasedKeys = new HashSet<OpenTK.Input.Key>();

        private static readonly HashSet<Mouse.Button> PressedButtons = new HashSet<Mouse.Button>();
        private static readonly HashSet<Mouse.Button> HeldButtons = new HashSet<Mouse.Button>();
        private static readonly HashSet<Mouse.Button> ReleasedButtons = new HashSet<Mouse.Button>();

        public static Vector2u WindowSize => window.Size;
        public static bool WindowHasFocus => window.HasFocus();

        public static string TextEntered { get; private set; }

        public static event EventHandler<SizeEventArgs> WindowResized;
        public static event EventHandler WindowClosed;

        public static void SetWindow(RenderWindow window, NativeWindow nativeWindow)
        {
            if (window != null)
            {
                window.MouseWheelScrolled -= MouseWheelScrolled;
                window.MouseMoved -= MouseMoved;

                nativeWindow.KeyDown -= NativeWindow_KeyDown;
                nativeWindow.KeyUp -= NativeWindow_KeyUp;
                window.TextEntered -= OnTextEntered;

                window.MouseButtonPressed -= MouseButtonPressed;
                window.MouseButtonReleased -= MouseButtonReleased;

                window.Resized -= OnWindowResized;

                window.LostFocus -= ResetEventHandler;
                window.GainedFocus -= ResetEventHandler;

                window.Closed -= OnWindowClose;

                nativeWindow.FileDrop -= OnFileDrop;
            }

            Reset();

            Input.window = window;
            Input.nativeWindow = nativeWindow;

            window.MouseWheelScrolled += MouseWheelScrolled;
            window.MouseMoved += MouseMoved;

            nativeWindow.KeyDown += NativeWindow_KeyDown;
            nativeWindow.KeyUp += NativeWindow_KeyUp;
            window.TextEntered += OnTextEntered;

            window.MouseButtonPressed += MouseButtonPressed;
            window.MouseButtonReleased += MouseButtonReleased;

            window.Resized += OnWindowResized;

            window.LostFocus += ResetEventHandler;
            window.GainedFocus += ResetEventHandler;

            window.Closed += OnWindowClose;

            nativeWindow.FileDrop += OnFileDrop;
        }

        private static void NativeWindow_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.IsRepeat) return;
            HeldKeys.Remove(e.Key);
            ReleasedKeys.Add(e.Key);
        }

        private static void NativeWindow_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.IsRepeat) return;
            HeldKeys.Add(e.Key);
            PressedKeys.Add(e.Key);
        }

        private static void OnWindowClose(object sender, EventArgs e)
        {
            WindowClosed?.Invoke(sender, EventArgs.Empty);
        }

        public static void Reset()
        {
            TextEntered = "";

            MouseDelta = MousePosition - PreviousMousePosition;
            PreviousMousePosition = MousePosition;

            ScreenMouseDelta = ScreenMousePosition - PreviousScreenMousePosition;
            PreviousScreenMousePosition = ScreenMousePosition;

            MouseWheelDelta = 0;

            PressedButtons.Clear();
            ReleasedButtons.Clear();

            PressedKeys.Clear();
            ReleasedKeys.Clear();
        }

        private static void ResetEventHandler(object sender, EventArgs e)
        {
            Reset();
        }

        private static void OnFileDrop(object sender, OpenTK.Input.FileDropEventArgs e)
        {
            ConsoleEntity.Log(e.FileName);
        }

        private static void OnWindowResized(object sender, SizeEventArgs e) => WindowResized?.Invoke(sender, e);

        public static bool IsKeyHeld(OpenTK.Input.Key key) => HeldKeys.Contains(key);
        public static bool IsKeyPressed(OpenTK.Input.Key key) => PressedKeys.Contains(key);
        public static bool IsKeyReleased(OpenTK.Input.Key key) => ReleasedKeys.Contains(key);

        public static bool IsAnyKeyHeld => HeldKeys.Any();
        public static bool IsAnyKeyPressedd => PressedKeys.Any();
        public static bool IsAnyKeyReleased => ReleasedKeys.Any();

        public static bool IsButtonHeld(Mouse.Button button) => HeldButtons.Contains(button);
        public static bool IsButtonPressed(Mouse.Button button) => PressedButtons.Contains(button);
        public static bool IsButtonReleased(Mouse.Button button) => ReleasedButtons.Contains(button);

        public static bool IsAnyButtonHeld => HeldButtons.Any();
        public static bool IsAnyButtonPressed => PressedButtons.Any();
        public static bool IsAnyButtonReleased => ReleasedButtons.Any();

        public static Mouse.Button? PressedButton => IsAnyButtonPressed ? new Mouse.Button?(PressedButtons.First()) : null;
        public static Mouse.Button? HeldButton => IsAnyButtonHeld ? new Mouse.Button?(HeldButtons.First()) : null;
        public static Mouse.Button? ReleasedButton => IsAnyButtonReleased ? new Mouse.Button?(ReleasedButtons.First()) : null;

        public static void SetCursor(Cursor.CursorType type)
        {
            window.SetMouseCursor(new Cursor(type));
        }

        private static void OnTextEntered(object sender, TextEventArgs e)
        {
            TextEntered += e.Unicode;
        }

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
            // HeldKeys.Remove(e.Code);
            // ReleasedKeys.Add(e.Code);
        }

        private static void KeyPressed(object sender, KeyEventArgs e)
        {
            //HeldKeys.Add(e.Code);
            // PressedKeys.Add(e.Code);
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