using System;
using System.Collections.Generic;

namespace Shared
{
    public struct Button
    {
        public string Label { get; set; }
        public Action Action { get; set; }
        public bool Interactive { get; set; }

        public Button(string label, Action action = default, bool interactive = true)
        {
            Label = label;
            Action = action;
            Interactive = interactive;
        }

        public static readonly Button VerticalSeparator = new Button("|", default, false);
        public static readonly Button HorizontalSeparator = new Button("ー", default, false);
        public static readonly Button Spacer = new Button("", default, false);

        public override bool Equals(object obj)
        {
            return obj is Button button &&
                   Label == button.Label &&
                   Interactive == button.Interactive;
        }

        public override int GetHashCode()
        {
            var hashCode = 1748908080;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Label);
            hashCode = hashCode * -1521134295 + Interactive.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Button left, Button right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Button left, Button right)
        {
            return !(left == right);
        }
    }
}
