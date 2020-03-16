using System;
using System.IO;
using System.Text;

namespace MusicGrid
{
    public class EventTextWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.Default;

        public event EventHandler<(object, object)> OnWriteLine;

        public override void WriteLine(string value, object argument)
        {
            OnWriteLine?.Invoke(this, (value, argument));
            base.WriteLine(value);
        }

        public override void Write(string value, object argument)
        {
            OnWriteLine?.Invoke(this, (value, argument));
            base.Write(value);
        }

        public override void WriteLine(string value)
        {
            OnWriteLine?.Invoke(this, (value, null));
            base.WriteLine(value);
        }

        public override void Write(string value)
        {
            OnWriteLine?.Invoke(this, (value, null));
            base.Write(value);
        }
    }
}
