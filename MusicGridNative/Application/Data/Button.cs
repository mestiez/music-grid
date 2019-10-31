using System;

namespace MusicGrid
{
    public struct Button
    {
        public string Label { get; set; }
        public Action Action { get; set; }
        public bool Interactive { get; set; }

        public Button(string label, Action action, bool interactive = true)
        {
            Label = label;
            Action = action;
            Interactive = interactive;
        }

        public static readonly Button VerticalSeparator = new Button("|", default, false);
        public static readonly Button HorizontalSeparator = new Button("ー", default, false);
        public static readonly Button Spacer = new Button("", default, false);
    }
}
