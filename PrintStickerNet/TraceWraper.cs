using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace PrintSticker
{
    /// <summary>
    /// Summary description for TraceWraper.
    /// </summary>

    public class TraceWraper : System.Diagnostics.TraceListener
    {
        public TraceWraper() {
        }
        ~TraceWraper() {
            Dispose(false);
        }
        public override void Close()
        {
            base.Close();
        }
        public override void Write(string str)
        {
        }
        public override void WriteLine(string str) {
            _Form1.WriteLog(str);                        
        }
    }
}

