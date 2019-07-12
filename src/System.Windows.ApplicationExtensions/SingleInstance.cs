using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Windows.ApplicationExtensions
{
    /// <summary>
    ///     This class checks to make sure that only one instance of
    ///     this application is running at a time.
    /// </summary>
    /// <remarks>
    ///     Note: this class should be used with some caution, because it does no
    ///     security checking. For example, if one instance of an app that uses this class
    ///     is running as Administrator, any other instance, even if it is not
    ///     running as Administrator, can activate it with command line arguments.
    ///     For most apps, this will not be much of an issue.
    /// </remarks>
    public static class SingleInstance<TApplication>
        where TApplication : Application, ISingleInstanceApp
    {
        #region Private Fields

        private static Mutex singleInstanceMutex;

        private static readonly FileSystemWatcher watcher = new FileSystemWatcher();
        #endregion


        #region Public Methods

        public static bool InitializeAsFirstInstance(string uniqueName)
        {
            // Build unique application Id and the IPC channel name.
            string applicationIdentifier = uniqueName + Environment.UserName;


            // Create mutex based on unique application Id to check if this is the first instance of the application. 
            singleInstanceMutex = new Mutex(true, applicationIdentifier, out bool firstInstance);
            if (firstInstance)
            {
                CreateRemoteService();
            }
            else
            {
                SignalFirstInstance(Environment.CommandLine);
            }

            return firstInstance;
        }

        public static void Cleanup()
        {
            if (singleInstanceMutex != null)
            {
                singleInstanceMutex.Close();
                singleInstanceMutex = null;
            }
        }

        #endregion

        #region Private Methods


        private static void CreateRemoteService()
        {
            watcher.Path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            watcher.Changed += Watcher_Changed;
            watcher.EnableRaisingEvents = true;
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (Application.Current == null)
            {
                return;
            }
            if (e.Name != "cmd.args") return;
            try
            {
                using (var reader = new StreamReader(e.FullPath))
                {
                    var str = reader.ReadToEnd();
                    ((TApplication)Application.Current).SignalExternalCommandLineArgs(NativeMethods.CommandLineToArgvW(str));
                }
                File.Delete(e.FullPath);
            }
            catch
            {
                //ignore
            }
        }

        private static void SignalFirstInstance(string args)
        {
            try
            {
                using (var writer = new StreamWriter(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "cmd.args"), false))
                {
                    writer.Write(args);
                }
            }
            catch
            {
                //ignore
            }
        }

        #endregion

    }
}
