using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.ApplicationExtensions
{
    public abstract class SingleInstanceApp : Application, ISingleInstanceApp
    {
        private readonly string _name;

        protected SingleInstanceApp(string uniqueAppName)
        {
            _name = uniqueAppName;
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SingleInstance<SingleInstanceApp>.Cleanup();
            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!SingleInstance<SingleInstanceApp>.InitializeAsFirstInstance(_name))
            {
                Shutdown();
                return;
            }
            base.OnStartup(e);
        }

        public abstract bool SignalExternalCommandLineArgs(IList<string> args);
    }
}
