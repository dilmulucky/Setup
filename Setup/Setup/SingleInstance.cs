using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Setup
{

    public interface ISingleInstanceApp
    {
        bool SignalExternalCommandLineArgs(string args);
    }

    /// <summary>
    /// This class checks to make sure that only one instance of 
    /// this application is running at a time.
    /// </summary>
    /// <remarks>
    /// Note: this class should be used with some caution, because it does no
    /// security checking. For example, if one instance of an app that uses this class
    /// is running as Administrator, any other instance, even if it is not
    /// running as Administrator, can activate it with command line arguments.
    /// For most apps, this will not be much of an issue.
    /// </remarks>
    public static class SingleInstance<TApplication>
    where TApplication : Application, ISingleInstanceApp
    {
        #region Private Fields

        // 持续等待下次连接
        private static bool isRun = true;

        /// <summary>
        /// Suffix to the channel name.
        /// </summary>
        private const string ChannelNameSuffix = "SingeInstancePipcChannel";

        /// <summary>
        /// Application mutex.
        /// </summary>
        private static Mutex singleInstanceMutex;

        private static NamedPipeServerStream pipeServer;

        /// <summary>
        /// List of command line arguments for the application.
        /// </summary>
        private static IList<string> commandLineArgs;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets list of command line arguments for the application.
        /// </summary>
        public static IList<string> CommandLineArgs
        {
            get { return commandLineArgs; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks if the instance of the application attempting to start is the first instance. 
        /// If not, activates the first instance.
        /// </summary>
        /// <returns>True if this is the first instance of the application.</returns>
        public static bool InitializeAsFirstInstance(string uniqueName)
        {
            commandLineArgs = GetCommandLineArgs(uniqueName);

            // Build unique application Id and the IPC channel name.
            string applicationIdentifier = uniqueName + Environment.UserName;

            string channelName = String.Concat(applicationIdentifier, ChannelNameSuffix);

            // Create mutex based on unique application Id to check if this is the first instance of the application. 
            bool firstInstance;
            singleInstanceMutex = new Mutex(true, applicationIdentifier, out firstInstance);
            if (firstInstance)
            {
                CreateRemoteService(channelName);
            }
            else
            {
                SignalFirstInstance(channelName, commandLineArgs);
            }

            return firstInstance;
        }

        /// <summary>
        /// Cleans up single-instance code, clearing shared resources, mutexes, etc.
        /// </summary>
        public static void Cleanup()
        {
            isRun = false;

            if (singleInstanceMutex != null)
            {
                singleInstanceMutex.Close();
                singleInstanceMutex = null;
            }

            if (pipeServer != null)
            {
                pipeServer.Close();
                pipeServer = null;
            }

        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets command line args - for ClickOnce deployed applications, command line args may not be passed directly, they have to be retrieved.
        /// </summary>
        /// <returns>List of command line arg strings.</returns>
        private static IList<string> GetCommandLineArgs(string uniqueApplicationName)
        {
            string[] args = null;
            if (System.AppDomain.CurrentDomain == null)
            {
                // The application was not clickonce deployed, get args from standard API's
                args = Environment.GetCommandLineArgs();
            }

            if (args == null)
            {
                args = new string[] { };
            }

            return new List<string>(args);
        }

        /// <summary>
        /// Creates a remote service for communication.
        /// </summary>
        /// <param name="channelName">Application's IPC channel name.</param>
        private static void CreateRemoteService(string channelName)
        {
            Task.Run(() =>
            {
                while (isRun)
                {
                    pipeServer = new NamedPipeServerStream(channelName, PipeDirection.InOut);
                    // Wait for a client to connect
                    pipeServer.WaitForConnection();

                    try
                    {
                        StreamString ss = new StreamString(pipeServer);
                        var arg = ss.ReadString();

                        ((TApplication)Application.Current).SignalExternalCommandLineArgs(arg);
                    }
                    // Catch the IOException that is raised if the pipe is broken
                    // or disconnected.
                    catch (IOException e)
                    {
                        Console.WriteLine("ERROR: {0}", e.Message);
                    }

                    pipeServer.Close();
                }
            });
        }

        /// <summary>
        /// Creates a client channel and obtains a reference to the remoting service exposed by the server - 
        /// in this case, the remoting service exposed by the first instance. Calls a function of the remoting service 
        /// class to pass on command line arguments from the second instance to the first and cause it to activate itself.
        /// </summary>
        /// <param name="channelName">Application's IPC channel name.</param>
        /// <param name="args">
        /// Command line arguments for the second instance, passed to the first instance to take appropriate action.
        /// </param>
        private static void SignalFirstInstance(string channelName, IList<string> args)
        {
            var pipeClient =
                    new NamedPipeClientStream(".", channelName,
                        PipeDirection.InOut, PipeOptions.None,
                        TokenImpersonationLevel.Impersonation);

            pipeClient.Connect();

            var ss = new StreamString(pipeClient);

            // 需求只需要传递第一个参数即可
            ss.WriteString(args[0]);

            pipeClient.Close();
        }


        // Defines the data protocol for reading and writing strings on our stream
        public class StreamString
        {
            private Stream ioStream;
            private UnicodeEncoding streamEncoding;

            public StreamString(Stream ioStream)
            {
                this.ioStream = ioStream;
                streamEncoding = new UnicodeEncoding();
            }

            public string ReadString()
            {
                StreamReader sr = new StreamReader(ioStream);
                return sr.ReadToEnd();
            }

            public void WriteString(string outString)
            {
                StreamWriter sw = new StreamWriter(ioStream);
                sw.Write(outString);
                sw.Flush();
            }
        }

        #endregion

    }
}
