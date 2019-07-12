using System.Collections.Generic;

namespace System.Windows.ApplicationExtensions
{
    public interface ISingleInstanceApp
    {
        bool SignalExternalCommandLineArgs(IList<string> args);
    }
}
