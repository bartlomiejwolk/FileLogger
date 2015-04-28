#define DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using OneDayGame.LoggingTools;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace ATP.LoggingTools {

    public enum AppendOptions {

        Timestamp = 1,
        ClassName = 2,
        CallerClassName = 4

    }

    /// Logs to a file function calls, return values or any string.
    /// 
    /// Comment out the DEBUG directive to disable all calls to this class.
    /// For Editor classes you must define DEBUG directive explicitly.
    /// \todo Remove LogPrimitive() methods and use LogStringArray and
    /// LogIntArray() instead.
    public sealed class Logger : GameComponent {

        public static event EventHandler StateChanged;

        private static Logger instance;

        /// todo all class fields should be static.
        /// If append messages to the file or overwrite.
        private bool append;

        /// If append caller class name to every log message.
        [SerializeField]
        private bool appendCallerClassName = true;

        /// If append class name to every log message.
        [SerializeField]
        private bool appendClassName = true;

        /// Class filter.
        /// 
        /// List of classes that will be logged.
        /// \remark Empty list disables class filtering.
        [SerializeField]
        private List<string> classFilter = new List<string>();

        [SerializeField]
        private bool echoToConsole = false;

        [SerializeField]
        private bool enableLogCall = true;

        // TODO Add to inspector.
        [SerializeField]
        private bool enableLogDictionary = true;

        // TODO Add to inspector.
        [SerializeField]
#pragma warning disable 169
            private bool enableLogList = true;
#pragma warning restore 169
        [SerializeField]
        private bool enableLogResult = true;

        /// \todo Change to false.
        [SerializeField]
        private bool enableLogStackTrace = true;

        [SerializeField]
        private bool enableLogString = true;

        /// Enable logging when in play mode.
        [SerializeField]
#pragma warning disable 649
            private bool enableOnPlay;
#pragma warning restore 649
        /// Output file name/path.
        [SerializeField]
        private string filePath = "log.txt";

        /// If true, display fully qualified class name.
        /// \todo Rename to qualifiedClassName.
        [SerializeField]
        private bool fullyQualifiedClassName = true;

        [SerializeField]
        private bool indentMessage = true;

        //[SerializeField]
        //private bool inGameLabel;

        /// Initial size of the cache array.
        /// 
        /// When this array fills-up, it'll be resized by the same amount.
        [SerializeField]
        private int initArraySize = 1000000;

        private Cache logCache = new Cache();
        // When false, no logging is done.
        [SerializeField]
        private bool loggingEnabled;

        [SerializeField]
        private bool logInRealTime = false;

        /// Method filter.
        /// 
        /// List of methods that will be logged.
        /// \remark Empty list disables method filtering.
        [SerializeField]
        private List<string> methodFilter = new List<string>();

        /// \todo Rename to addTimestamp.
        [SerializeField]
        private bool showTimestamp = true;

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

        public Cache LogCache {
            get { return logCache; }
        }

        public bool LoggingEnabled {
            get { return loggingEnabled; }
            set { loggingEnabled = value; }
        }

        [Conditional("DEBUG")]
        public static void LogCall() {
            Log(
                stackInfo => stackInfo.MethodSignature,
                Instance.enableLogCall,
                Instance.showTimestamp,
                Instance.indentMessage,
                Instance.appendClassName,
                Instance.appendCallerClassName);
        }

        public static void LogDictionary<TKey, TValue>(
            Dictionary<TKey, TValue> dict,
            string info) {
            var message = new StringBuilder();

            if (info.Length != 0) {
                message.Append(info);
                message.Append("\n");
            }

            foreach (var entry in dict) {
                message.Append(entry.Key + ": " + entry.Value);
                // TODO Don't append new line after last dict. element.
                message.Append("\n");
            }

            // Log message.
            Log(
                stackInfo => message.ToString(),
                Instance.enableLogDictionary,
                false,
                false,
                false,
                false);
        }

        public static void LogResult(object result) {
            // Compose log message.
            var message = string.Format("[RESULT: {0}]", result);

            // Log message.
            Log(
                stackInfo => message,
                Instance.enableLogResult,
                Instance.showTimestamp,
                Instance.indentMessage,
                Instance.appendClassName,
                Instance.appendCallerClassName);
        }

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
                Instance.enableLogString,
                Instance.showTimestamp,
                Instance.indentMessage,
                Instance.appendClassName,
                Instance.appendCallerClassName);
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
            Instance.logCache.WriteAll(
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

        // TODO Refactor.
        private static void Log(
            Func<StackInfo, string> composeMessage,
            bool enableLoggingForMethod,
            bool showTimestamp,
            bool indentMessage,
            bool appendClassName,
            bool appendCallerClassName) {
            if (enableLoggingForMethod == false) {
                return;
            }

            // Return if Logger instance does not exists.
            if (Instance == null) {
                return;
            }

            // Return if user stopped logging from the inspector.
            if (Instance.LoggingEnabled == false) {
                return;
            }

            // Get info from call stack.
            var stackInfo = new StackInfo(3);

            // Filter by class name.
            if (ClassInFilter(stackInfo.ClassName) == false) {
                return;
            }

            // Filter by method name.
            if (MethodInFilter(stackInfo.MethodName) == false) {
                return;
            }

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

            // Apend class name if enabled in the inspector.
            if (appendClassName) {
                if (Instance.fullyQualifiedClassName) {
                    // Append fully qualified class name.
                    outputMessage.Append(
                        ", @ " + stackInfo.QualifiedClassName + "");
                }
                else {
                    // Append class name.
                    outputMessage.Append(", @ " + stackInfo.ClassName + "");
                }
            }

            // Apend caller class name if enabled in the inspector.
            if (appendCallerClassName) {
                // Get info from call stack.
                var callerStackInfo = new StackInfo(4);

                if (Instance.fullyQualifiedClassName) {
                    // Append fully qualified caller class name.
                    outputMessage.Append(
                        ", <- " + callerStackInfo.QualifiedClassName + "");
                }
                else {
                    outputMessage.Append(
                        ", <- " + callerStackInfo.ClassName + "");
                }
            }

            // Add log message to the cache.
            Instance.logCache.Add(
                outputMessage.ToString(),
                Instance.echoToConsole);

            if (Instance.logInRealTime) {
                Instance.logCache.WriteLast(Instance.filePath);
            }
        }

        //private void DisplayLabel() {
        //    MeasureIt.Set("Logs captured", logCache.LoggedMessages);
        //}
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
            logCache.InitArraySize = initArraySize;
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
                logCache.WriteAll(filePath, append);
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

        // todo move to region
        private void OnStateChanged() {
            var handler = StateChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

    }

}