#define DEBUG_LOGGER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using AnimationPathAnimator;
using FileLogger;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace FileLogger {

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
        private AppendOptions appendOptions = AppendOptions.Timestamp
                                              | AppendOptions.ClassName
                                              | AppendOptions.CallerClassName;

        /// <summary>
        /// Keeps info about Logger methods state (enabled/disabled).
        /// Disabled methods won't produce output.
        /// </summary>
        [SerializeField]
        private EnabledMethods enabledMethods = EnabledMethods.LogString
                                                | EnabledMethods.LogCall
                                                | EnabledMethods.LogResult;

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
            private bool enableOnPlay = true;
#pragma warning restore 649
        /// Output file name/path.
        [SerializeField]
        private string filePath = "log.txt";

        /// If true, display fully qualified class name.
        [SerializeField]
        private bool qualifiedClassName = true;

        [SerializeField]
        private bool indentLine = true;

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

        private ObjectIDGenerator objectIDGenerator;

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

        /// <summary>
        /// Generates and remembers GUID of all objects that call logger passing "this" param.
        /// </summary>
        private ObjectIDGenerator ObjectIDGenerator {
            get {
                if (objectIDGenerator != null) {
                    return objectIDGenerator;
                }

                objectIDGenerator = new ObjectIDGenerator();
                return objectIDGenerator;
            }
            set { objectIDGenerator = value; }
        }

        /// <summary>
        /// Allows specify what additional information will be included in
        /// a single log line.
        /// </summary>
        public AppendOptions AppendOptions {
            get { return appendOptions; }
            set { appendOptions = value; }
        }

        /// <summary>
        /// Keeps info about Logger methods state (enabled/disabled).
        /// Disabled methods won't produce output.
        /// </summary>
        public EnabledMethods EnabledMethods {
            get { return enabledMethods; }
            set { enabledMethods = value; }
        }

        public bool LogInRealTime {
            get { return logInRealTime; }
            set { logInRealTime = value; }
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
            if (LogInRealTime) {
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
            //        methodEnabled == true
            //        && inGameLabel) {
            //    Logger.Instance.DisplayLabel();
            //}
        }

        private void Reset() {
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

        [Conditional("DEBUG_LOGGER")]
        public static void LogCall() {
            LogCall(null);
        }

        [Conditional("DEBUG_LOGGER")]
        public static void LogCall(object objectReference) {
           Log(
                stackInfo => stackInfo.MethodSignature,
                FlagsHelper.IsSet(Instance.EnabledMethods, EnabledMethods.LogCall),
                objectReference);
        }

        [Conditional("DEBUG_LOGGER")]
        public static void LogResult(object result) {
            // Compose log message.
            var message = string.Format("[RESULT: {0}]", result);

            // Log message.
            Log(
                stackInfo => message,
                FlagsHelper.IsSet(Instance.EnabledMethods, EnabledMethods.LogResult),
                null);
        }

        /// <summary>
        /// Logs stack trace.
        /// </summary>
        [Conditional("DEBUG_LOGGER")]
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
                null);
        }

        [Conditional("DEBUG_LOGGER")]
        public static void LogString(
            string format,
            params object[] paramList) {

            // Compose log message.
            var message = string.Format(format, paramList);

            // Log message.
            Log(
                stackInfo => message,
                FlagsHelper.IsSet(Instance.EnabledMethods, EnabledMethods.LogString),
                null);
        }

        /// Start Logger.
        [Conditional("DEBUG_LOGGER")]
        public static void StartLogging(
            string filePath = "log.txt",
            bool append = false) {

            Instance.filePath = filePath;
            Instance.append = append;
            Instance.LoggingEnabled = true;
        }

        /// Stop Logger.
        [Conditional("DEBUG_LOGGER")]
        public static void StopLogging() {
            Instance.LoggingEnabled = false;

            // There's no need to write cached messages since logging was made
            // in real time.
            if (Instance.LogInRealTime) return;

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

        /// <summary>
        /// Base method used to create and save a log message.
        /// </summary>
        /// <param name="composeMessage"></param>
        /// <param name="methodEnabled"></param>
        /// <param name="showTimestamp"></param>
        /// <param name="indentMessage"></param>
        /// <param name="appendClassName"></param>
        /// <param name="appendCallerClassName"></param>
        /// <param name="objectReference"></param>
        private static void Log(
            Func<FrameInfo, string> composeMessage,
            bool methodEnabled,
            object objectReference) {

            if (!methodEnabled) return;
            if (!Instance.LoggingEnabled) return;

            // Get info from call stack.
            var stackInfo = new FrameInfo(3);

            // Filter by class name.
            if (!ClassInFilter(stackInfo.ClassName)) return;
            // Filter by method name.
            if (!MethodInFilter(stackInfo.MethodName)) return;

            // Log message to write.
            var outputMessage = new StringBuilder();

            // Add timestamp.
            HandleShowTimestamp(outputMessage);
            // Indent message.
            HandleIndentMessage(stackInfo, outputMessage);
            // Append message returned by callback.
            outputMessage.Append(composeMessage(stackInfo));
            // Append class name.
            HandleAppendClassName(outputMessage, stackInfo);
            // Append object GUID.
            HandleAppendGUID(objectReference, outputMessage);
            // Append caller class name.
            HandleAppendCallerClassName(outputMessage);

            // Add log message to the cache.
            Instance.logWriter.Add(
                outputMessage.ToString(),
                Instance.echoToConsole);

            // Append message to the log file.
            if (Instance.LogInRealTime) {
                Instance.logWriter.WriteLast(Instance.filePath);
            }
        }

        /// <summary>
        /// Adds timestamp to the log message.
        /// </summary>
        /// <param name="showTimestamp"></param>
        /// <param name="outputMessage"></param>
        private static void HandleShowTimestamp(StringBuilder outputMessage) {
            if (!FlagsHelper.IsSet(
                Instance.AppendOptions,
                AppendOptions.Timestamp)) return;

            outputMessage.Append(GetCurrentTimestamp());
            outputMessage.Append(" ");
        }

        /// <summary>
        /// Indents log message.
        /// </summary>
        /// <param name="indentMessage"></param>
        /// <param name="frameInfo"></param>
        /// <param name="outputMessage"></param>
        private static void HandleIndentMessage(
            FrameInfo frameInfo,
            StringBuilder outputMessage) {

            if (!Instance.indentLine) return;

            for (var i = 0; i < frameInfo.FrameCount; i++) {
                outputMessage.Append("| ");
            }
        }

        /// <summary>
        /// Appends GUID of the object that called the Logger method.
        /// </summary>
        /// <param name="objectReference"></param>
        /// <param name="outputMessage"></param>
        private static void HandleAppendGUID(
            object objectReference,
            StringBuilder outputMessage) {

            if (objectReference == null) return;

            bool firstTime;
            long objectID = Instance.ObjectIDGenerator.GetId(
                objectReference,
                out firstTime);

            outputMessage.Append(string.Format(" (GUID: {0})", objectID));
        }

        /// <summary>
        /// Helper method.
        /// Appends caller class name to the output message.
        /// </summary>
        /// <param name="outputMessage"></param>
        private static void HandleAppendCallerClassName(StringBuilder outputMessage) {
            if (!FlagsHelper.IsSet(
                    Instance.AppendOptions,
                    AppendOptions.CallerClassName)) return;

            // Get info from call stack.
            var callerStackInfo = new FrameInfo(5);

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
        /// <param name="frameInfo"></param>
        private static void HandleAppendClassName(
            StringBuilder outputMessage,
            FrameInfo frameInfo) {

            if (!FlagsHelper.IsSet(
                Instance.AppendOptions,
                AppendOptions.ClassName)) return;

            if (Instance.qualifiedClassName) {
                // Append fully qualified class name.
                outputMessage.Append(
                    ", @ " + frameInfo.QualifiedClassName + "");
            }
            else {
                // Append class name.
                outputMessage.Append(", @ " + frameInfo.ClassName + "");
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