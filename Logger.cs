#define DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using AnimationPathAnimator;
using mLogger;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace mLogger {

    /// <summary>
    /// Allows logging custom messages to a file.
    /// </summary>
    /// <remarks>Comment out the DEBUG directive to disable all calls to this class.
    /// For Editor classes you must define DEBUG directive explicitly.</remarks>
    public sealed class Logger : MonoBehaviour {

        #region EVENTS
        /// <summary>
        /// Event called when logger is started, stopped, paused or reasumed.
        /// </summary>
        public static event EventHandler StateChanged;
        #endregion

        #region FIELDS
        private static Logger instance;

        /// <summary>
        /// Allows specify what additional information will be included in
        /// a single log line.
        /// </summary>
        [SerializeField]
        public AppendOptions AppendOptions;

        /// <summary>
        /// Keeps info about Logger methods state (enabled/disabled).
        /// Disabled methods won't produce output.
        /// </summary>
        [SerializeField]
        public EnabledMethods EnabledMethods;

        /// If append messages to the file or overwrite.
        [SerializeField]
        private bool append;

        /// Class filter.
        /// 
        /// List of classes that will be logged.
        /// \remark Empty list disables class filtering.
        [SerializeField]
        private List<string> classFilter = new List<string>();

        [SerializeField]
        private bool echoToConsole;

        [SerializeField]
        private bool enableLogStackTrace = true;

        /// Enable logging when in play mode.
        [SerializeField]
#pragma warning disable 649
            private bool enableOnPlay;
#pragma warning restore 649
        /// Output file name/path.
        [SerializeField]
        private string filePath = "log.txt";

        /// If true, display fully qualified class name.
        [SerializeField]
        private bool qualifiedClassName = true;

        [SerializeField]
        private bool indentMessage = true;

        /// Initial size of the cache array.
        /// 
        /// When this array fills-up, it'll be resized by the same amount.
        [SerializeField]
        private int initArraySize = 1000000;

        private LogWriter logWriter = new LogWriter();
        // When false, no logging is done.
        [SerializeField]
        private bool loggingEnabled;

        [SerializeField]
        private bool logInRealTime;

        /// Method filter.
        /// 
        /// List of methods that will be logged.
        /// \remark Empty list disables method filtering.
        [SerializeField]
        private List<string> methodFilter = new List<string>();

        #endregion

        /// \todo Rename to addTimestamp.
        //[SerializeField]
        //private bool showTimestamp = true;

        #region PROPERTIES
        public static Logger Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<Logger>();
                    if (instance == null) {
                        GameObject singleton = new GameObject();
                        instance = singleton.AddComponent<Logger>();
                        singleton.name = "(singleton) Logger";

                        //Tell unity not to destroy this object when loading a new scene!
                        DontDestroyOnLoad(instance.gameObject);
                    }
                }
                return instance;
            }
        }

        public bool Append {
            get { return append; }
            set { append = value; }
        }

        public bool EnableOnPlay {
            get { return enableOnPlay; }
        }

        public string FilePath {
            get { return filePath; }
        }

        public LogWriter LogWriter {
            get { return logWriter; }
        }

        public bool LoggingEnabled {
            get { return loggingEnabled; }
            set { loggingEnabled = value; }
        }
        #endregion

        #region UNITY MESSAGES

        // ReSharper disable once UnusedMember.Local
        private void Awake() {
            if (instance == null) {
                // If I am the first instance, make me the Singleton
                instance = this;
                DontDestroyOnLoad(this);
            }
            else {
                // If a Singleton already exists and you find
                // another reference in scene, destroy it!
                if (this != instance) {
                    Destroy(gameObject);
                }
            }

            // Initialize 'cache' object.
            logWriter.InitArraySize = initArraySize;
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDestroy() {
            // Don't write to file if 'logInRealTime' was selected.
            if (logInRealTime) {
                return;
            }
            // Write log to file when 'enableOnPlay' was selected.
            if (enableOnPlay) {
                // Write single message to the file.
                logWriter.WriteAll(filePath, append);
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void Start() {
            // Handle 'Enable On Play' inspector option.
            if (enableOnPlay) {
                loggingEnabled = true;

                OnStateChanged();
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void Update() {
            // Handle "In-game Label" inspector option.
            //if (
            //        loggingEnabled == true
            //        && inGameLabel) {
            //    Logger.Instance.DisplayLabel();
            //}
        }
        #endregion

        #region EVENT INVOCATORS
        private void OnStateChanged() {
            var handler = StateChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region METHODS
        public void ClearLogFile() {
            StreamWriter writer;
            // Create stream writer used to write log cache to file.
            using (writer = new StreamWriter(filePath, append)) {
                writer.WriteLine("");
            }

            UnityEngine.Debug.Log("Log file cleared!");
        }


        [Conditional("DEBUG")]
        public static void LogCall() {
            Log(
                stackInfo => stackInfo.MethodSignature,
                FlagsHelper.IsSet(Instance.EnabledMethods, EnabledMethods.LogCall),
                FlagsHelper.IsSet(
                    Instance.AppendOptions,
                    AppendOptions.Timestamp),
                Instance.indentMessage,
                FlagsHelper.IsSet(
                    Instance.AppendOptions,
                    AppendOptions.ClassName),
                FlagsHelper.IsSet(
                    Instance.AppendOptions,
                    AppendOptions.CallerClassName));
        }

        public static void LogResult(object result) {
            // Compose log message.
            var message = string.Format("[RESULT: {0}]", result);

            // Log message.
            Log(
                stackInfo => message,
                FlagsHelper.IsSet(Instance.EnabledMethods, EnabledMethods.LogResult),
                FlagsHelper.IsSet(
                    Instance.AppendOptions,
                    AppendOptions.Timestamp),
                Instance.indentMessage,
                FlagsHelper.IsSet(
                    Instance.AppendOptions,
                    AppendOptions.ClassName),
                FlagsHelper.IsSet(
                    Instance.AppendOptions,
                    AppendOptions.CallerClassName));
        }

        /// <summary>
        /// Logs stack trace.
        /// </summary>
        public static void LogStackTrace() {
            var stackTrace = new StackTrace();
            var message = new StringBuilder();
            for (var i = 1; i < stackTrace.FrameCount; i++) {
                var stackFrame = stackTrace.GetFrame(i);
                for (var j = 0; j < i; j++) {
                    message.Append("| ");
                }
                message.Append(stackFrame.GetMethod());
                if (i == stackTrace.FrameCount - 1) {
                    break;
                }
                message.Append("\n");
            }

            Log(
                stackInfo => message.ToString(),
                Instance.enableLogStackTrace,
                false,
                false,
                false,
                false);
        }

        public static void LogString(
            string format,
            params object[] paramList) {

            // Compose log message.
            var message = string.Format(format, paramList);

            // Log message.
            Log(
                stackInfo => message,
                FlagsHelper.IsSet(Instance.EnabledMethods, EnabledMethods.LogString),
                FlagsHelper.IsSet(
                    Instance.AppendOptions,
                    AppendOptions.Timestamp),
                Instance.indentMessage,
                FlagsHelper.IsSet(
                    Instance.AppendOptions,
                    AppendOptions.ClassName),
                FlagsHelper.IsSet(
                    Instance.AppendOptions,
                    AppendOptions.CallerClassName));
        }

        /// Start Logger.
        [Conditional("DEBUG")]
        public static void StartLogging(
            string filePath = "log.txt",
            bool append = false) {

            Instance.filePath = filePath;
            Instance.append = append;
            Instance.LoggingEnabled = true;
        }

        /// Stop Logger.
        [Conditional("DEBUG")]
        public static void StopLogging() {
            Instance.LoggingEnabled = false;
            // Write single message to the file.
            Instance.logWriter.WriteAll(
                Instance.filePath,
                Instance.append);
        }

        private static bool ClassInFilter(string className) {
            if (Instance.classFilter.Count != 0) {
                // Return if class is not listed in the class filter.
                // You can set class filter in the inspector.
                if (Instance.classFilter.Contains(className)) {
                    return true;
                }
                return false;
            }
            // Filtering is disabled so every class is treated as being
            // in the filter.
            return true;
        }

        /// Add timestamp to a single log message.
        /// 
        /// \return String with timestamp.
        /// \todo Make it static and put in some utility class.
        private static string GetCurrentTimestamp() {
            var now = DateTime.Now;
            var timestamp =
                string.Format("[{0:H:mm:ss:fff}]", now);

            return timestamp;
        }

        private static void Log(
            Func<StackInfo, string> composeMessage,
            bool loggingEnabled,
            bool showTimestamp,
            bool indentMessage,
            bool appendClassName,
            bool appendCallerClassName) {

            if (loggingEnabled == false) return;
            if (Instance.LoggingEnabled == false) return;

            // Get info from call stack.
            var stackInfo = new StackInfo(3);

            // Filter by class name.
            if (!ClassInFilter(stackInfo.ClassName)) return;
            // Filter by method name.
            if (!MethodInFilter(stackInfo.MethodName)) return;

            // Log message to write.
            var outputMessage = new StringBuilder();

            // Add timestamp.
            if (showTimestamp) {
                outputMessage.Append(GetCurrentTimestamp());
                outputMessage.Append(" ");
            }

            // Indent message.
            if (indentMessage) {
                for (var i = 0; i < stackInfo.FrameCount; i++) {
                    outputMessage.Append("| ");
                }
            }

            // Add message if not empty.
            outputMessage.Append(composeMessage(stackInfo));

            // Apend class name.
            if (appendClassName) {
                AppendClassName(outputMessage, stackInfo);
            }

            // Apend caller class name.
            if (appendCallerClassName) {
                AppendCallerClassName(outputMessage);
            }

            // Add log message to the cache.
            Instance.logWriter.Add(
                outputMessage.ToString(),
                Instance.echoToConsole);

            // Append message to the log file.
            if (Instance.logInRealTime) {
                Instance.logWriter.WriteLast(Instance.filePath);
            }
        }

        /// <summary>
        /// Helper method.
        /// Appends caller class name to the output message.
        /// </summary>
        /// <param name="outputMessage"></param>
        private static void AppendCallerClassName(StringBuilder outputMessage) {
            // Get info from call stack.
            var callerStackInfo = new StackInfo(4);

            if (Instance.qualifiedClassName) {
                // Append fully qualified caller class name.
                outputMessage.Append(
                    ", <- " + callerStackInfo.QualifiedClassName + "");
            }
            else {
                outputMessage.Append(
                    ", <- " + callerStackInfo.ClassName + "");
            }
        }

        /// <summary>
        /// Helper method.
        /// Appends class name to the output message.
        /// </summary>
        /// <param name="outputMessage"></param>
        /// <param name="stackInfo"></param>
        private static void AppendClassName(
            StringBuilder outputMessage,
            StackInfo stackInfo) {

            if (Instance.qualifiedClassName) {
                // Append fully qualified class name.
                outputMessage.Append(
                    ", @ " + stackInfo.QualifiedClassName + "");
            }
            else {
                // Append class name.
                outputMessage.Append(", @ " + stackInfo.ClassName + "");
            }
        }

        private static bool MethodInFilter(string methodName) {
            if (Instance.methodFilter.Count != 0) {
                // Return if method is not listed in the class filter.
                // You can set class filter in the inspector.
                if (Instance.methodFilter.Contains(methodName)) {
                    return true;
                }
                return false;
            }
            return true;
        }
#endregion
    }

}