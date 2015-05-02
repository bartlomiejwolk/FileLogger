// Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com)
//  
// This file is part of the FileLogger extension for Unity.
// Licensed under the MIT license. See LICENSE file in the project root folder.

#define DEBUG_LOGGER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

namespace FileLogger {

    /// <summary>
    ///     Allows logging custom messages to a file.
    /// </summary>
    /// <remarks>
    ///     Comment out the DEBUG directive to disable all calls to this class. For
    ///     Editor classes you must define DEBUG directive explicitly.
    /// </remarks>
    [ExecuteInEditMode]
    public sealed class Logger : MonoBehaviour {

        #region CONST
        public const string VERSION = "v0.1.0";
        #endregion

        #region EVENTS
        /// <summary>
        ///     Delegate for <c>StateChanged</c> event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="state">If logging is enabled.</param>
        public delegate void StateChangedEventHandler(object sender, bool state);

        /// <summary>
        ///     Event called when logger is started, stopped, paused or reasumed.
        /// </summary>
        public static event StateChangedEventHandler StateChanged;

        #endregion EVENTS
        #region FIELDS

        private static Logger instance;

        /// <summary>
        ///     If append messages to the file or overwrite.
        /// </summary>
        [SerializeField]
        private bool append;

        /// <summary>
        ///     Allows specify what additional information will be included in a
        ///     single log line.
        /// </summary>
        [SerializeField]
        private AppendOptions displayOptions = AppendOptions.Timestamp
                                              | AppendOptions.ClassName
                                              | AppendOptions.MethodName
                                              | AppendOptions.CallerClassName;

        /// <summary>
        ///     List of classes that will be logged. Empty list disables
        ///     class filtering.
        /// </summary>
        [SerializeField]
        private List<string> classFilter = new List<string>();

        [SerializeField]
        private bool echoToConsole;

        /// <summary>
        ///     Keeps info about Logger methods state (enabled/disabled). Disabled
        ///     methods won't produce output.
        /// </summary>
        [SerializeField]
        private EnabledMethods enabledMethods = EnabledMethods.LogString
                                                | EnabledMethods.LogCall
                                                | EnabledMethods.LogResult;

        [SerializeField]
        private bool enableLogStackTrace = true;

        /// <summary>
        ///     Enable logging when in play mode.
        /// </summary>
        [SerializeField]
#pragma warning disable 649
            private bool enableOnPlay = true;

#pragma warning restore 649

        /// <summary>
        ///     Output file name/path.
        /// </summary>
        [SerializeField]
        private string filePath = "log.txt";

        [SerializeField]
        private bool indentLine = true;

        /// <summary>
        ///     Initial size of the cache array.
        ///     When this array fills-up, it'll be resized by the same amount.
        /// </summary>
        [SerializeField]
        private int initArraySize = 1000000;

        /// <summary>
        ///     When false, no logging is done.
        /// </summary>
        [SerializeField]
        private bool loggingEnabled;

        [SerializeField]
        private bool logInRealTime;

        private LogWriter logWriter = new LogWriter();

        /// <summary>
        ///     List of methods that will be logged.
        ///     Empty list disables method filtering.
        /// </summary>
        [SerializeField]
        private List<string> methodFilter = new List<string>();

        private ObjectIDGenerator objectIDGenerator;

        /// <summary>
        ///     If true, display fully qualified class name.
        /// </summary>
        [SerializeField]
        private bool qualifiedClassName = true;

        [SerializeField]
        private bool clearOnPlay = true;

        #endregion FIELDS

        #region PROPERTIES

        public static Logger Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<Logger>();
                    if (instance == null) {
                        var singleton = new GameObject();
                        instance = singleton.AddComponent<Logger>();
                        singleton.name = "(singleton) Logger";

                        // Tell unity not to destroy this object when loading
                        // a new scene!
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

        /// <summary>
        ///     Allows specify what additional information will be included in a
        ///     single log line.
        /// </summary>
        public AppendOptions DisplayOptions {
            get { return displayOptions; }
            set { displayOptions = value; }
        }

        /// <summary>
        ///     Keeps info about Logger methods state (enabled/disabled). Disabled
        ///     methods won't produce output.
        /// </summary>
        public EnabledMethods EnabledMethods {
            get { return enabledMethods; }
            set { enabledMethods = value; }
        }

        public bool EnableOnPlay {
            get { return enableOnPlay; }
        }

        public string FilePath {
            get { return filePath; }
        }

        public bool LoggingEnabled {
            get { return loggingEnabled; }
            set {
                var prevValue = loggingEnabled;
                loggingEnabled = value;

                if (prevValue != value) {
                    OnStateChanged(value);
                }
            }
        }

        public bool LogInRealTime {
            get { return logInRealTime; }
            set { logInRealTime = value; }
        }

        public LogWriter LogWriter {
            get { return logWriter; }
        }

        /// <summary>
        ///     Generates and remembers GUID of all objects that call logger
        ///     passing "this" param.
        /// </summary>
        private ObjectIDGenerator ObjectIDGenerator {
            get {
                if (objectIDGenerator != null) {
                    return objectIDGenerator;
                }

                objectIDGenerator = new ObjectIDGenerator();
                return objectIDGenerator;
            }
        }

        public bool EchoToConsole {
            get { return echoToConsole; }
            set { echoToConsole = value; }
        }

        public bool ClearOnPlay {
            get { return clearOnPlay; }
            set { clearOnPlay = value; }
        }
        #endregion PROPERTIES

        #region UNITY MESSAGES

        private void Awake() {
            if (instance == null) {
                // If I am the first instance, make me the Singleton
                instance = this;
                DontDestroyOnLoad(this);
            }
            else {
                // If a Singleton already exists and you find another reference
                // in scene, destroy it!
                if (this != instance) {
                    Destroy(gameObject);
                }
            }

            // Initialize 'cache' object.
            logWriter.InitArraySize = initArraySize;
        }

        private void OnDestroy() {
            // todo extract method
            if (LogInRealTime) return;
            if (!EnableOnPlay) return;
            // Write only on exit play mode.
            if (!Application.isPlaying) return;

            // Write single message to the file.
            logWriter.WriteAll(FilePath, Append);
        }

        private void Start() {
            UnityEngine.Debug.Log("Start");
            HandleEnableOnPlay();
        }

        private void OnEnable() {
            UnityEngine.Debug.Log("OnEnable");
            SubscribeToEvents();
        }

        private void OnDisable() {
            UnsubscribeFromEvents();
        }
        #endregion UNITY MESSAGES

        #region EVENT INVOCATORS

        private void OnStateChanged(bool state) {
            var handler = StateChanged;
            if (handler != null) handler(this, state);
        }

        #endregion EVENT INVOCATORS

        #region EVENT HANDLERS
        void Logger_StateChanged(object sender, bool state) {
            // There's no need to write cached messages since logging was made
            // in real time.
            if (Instance.LogInRealTime) return;

            // Save messages to file on logger stop.
            if (!state) LogWriter.WriteAll(FilePath, Append);
        }
        #endregion

        #region METHODS
        private void HandleEnableOnPlay() {
            if (!enableOnPlay) return;
            if (!Application.isPlaying) return;

            loggingEnabled = true;
            if (ClearOnPlay) ClearLogFile();
        }

        private void UnsubscribeFromEvents() {
            UnityEngine.Debug.Log("UnsubscribeFromEvents");
            StateChanged -= Logger_StateChanged;
        }

        private void SubscribeToEvents() {
            UnityEngine.Debug.Log("SubscribeToEvents");
            StateChanged += Logger_StateChanged;
        }


        [Conditional("DEBUG_LOGGER")]
        public static void LogCall() {
            DoLogCall(null);
        }

        [Conditional("DEBUG_LOGGER")]
        public static void LogCall(object objectReference) {
            DoLogCall(objectReference);
        }

        private static void DoLogCall(object objectReference) {
            // Return if method is disabled.
            if (!FlagsHelper.IsSet(
                Instance.EnabledMethods,
                EnabledMethods.LogCall)) return;

            // Get info from call stack.
            var stackInfo = new FrameInfo(3);

            Log(
                stackInfo.MethodSignature,
                stackInfo,
                objectReference);
        }

        [Conditional("DEBUG_LOGGER")]
        public static void LogResult(object result) {
            DoLogResult(result, null);
        }

        [Conditional("DEBUG_LOGGER")]
        public static void LogResult(object result, object objectReference) {
            DoLogResult(result, objectReference);
        }

        private static void DoLogResult(object result, object objectRererence) {
            // Return if method is disabled.
            if (!FlagsHelper.IsSet(
                Instance.EnabledMethods,
                EnabledMethods.LogResult)) return;

            // Compose log message.
            var message = string.Format("[RESULT: {0}]", result);

            // Get info from call stack.
            var stackInfo = new FrameInfo(3);

            // Log message.
            Log(
                message,
                stackInfo,
                objectRererence);
        }

        /// <summary>
        ///     Logs stack trace.
        /// </summary>
        [Conditional("DEBUG_LOGGER")]
        public static void LogStackTrace() {
            if (!Instance.enableLogStackTrace) return;

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

            // Get info from call stack.
            var stackInfo = new FrameInfo(3);

            Log(
                message.ToString(),
                stackInfo,
                null);
        }

        [Conditional("DEBUG_LOGGER")]
        public static void LogString(
            string format,
            params object[] paramList) {

            DoLogString(format, null, paramList);
        }

        [Conditional("DEBUG_LOGGER")]
        public static void LogString(
            string format,
            object objectReference,
            params object[] paramList) {

            DoLogString(format, objectReference, paramList);
        }

        public static void DoLogString(
            string format,
            object objectReference,
            params object[] paramList) {

            // Return if method is disabled.
            if (!FlagsHelper.IsSet(
                Instance.EnabledMethods,
                EnabledMethods.LogString)) return;

            // Compose log message.
            var message = string.Format(format, paramList);

            // Get info from call stack.
            var stackInfo = new FrameInfo(3);

            // Log message.
            Log(
                message,
                stackInfo,
                objectReference);
        }

        /// <summary>
        ///     Start Logger.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="append"></param>
        [Conditional("DEBUG_LOGGER")]
        public static void StartLogging(
            string filePath = "log.txt",
            bool append = false) {

            Instance.filePath = filePath;
            Instance.append = append;
            Instance.LoggingEnabled = true;
        }

        /// <summary>
        ///     Stop Logger.
        /// </summary>
        [Conditional("DEBUG_LOGGER")]
        public static void StopLogging() {
            Instance.LoggingEnabled = false;

            // There's no need to write cached messages since logging was made
            // in real time.
            if (Instance.LogInRealTime) return;

            // Write single message to the file.
            Instance.logWriter.WriteAll(
                Instance.FilePath,
                Instance.Append);
        }

        /// <summary>
        ///     Clear log file.
        /// </summary>
        // todo move to LogWriter class
        [Conditional("DEBUG_LOGGER")]
        public void ClearLogFile() {
            StreamWriter writer;
            // Create stream writer used to write log cache to file.
            using (writer = new StreamWriter(FilePath, false)) {
                writer.WriteLine("");
            }

            UnityEngine.Debug.Log("Log file cleared!");
        }

        private static bool ClassInFilter(string className) {
            if (Instance.classFilter.Count != 0) {
                // Return if class is not listed in the class filter. You can
                // set class filter in the inspector.
                if (Instance.classFilter.Contains(className)) {
                    return true;
                }
                return false;
            }
            // Filtering is disabled so every class is treated as being in the
            // filter.
            return true;
        }

        /// <summary>
        ///     Add timestamp to a single log message.
        /// </summary>
        /// <returns></returns>
        private static string GetCurrentTimestamp() {
            var now = DateTime.Now;
            var timestamp =
                string.Format("[{0:H:mm:ss:fff}]", now);

            return timestamp;
        }

        /// <summary>
        ///     Helper method. Appends caller class name to the output message.
        /// </summary>
        /// <param name="outputMessage"></param>
        private static void HandleAppendCallerClassName(
            StringBuilder outputMessage) {
            if (!FlagsHelper.IsSet(
                Instance.DisplayOptions,
                AppendOptions.CallerClassName)) return;

            // Get info from call stack.
            var callerStackInfo = new FrameInfo(6);

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
        ///     Helper method. Appends class name to the output message.
        /// </summary>
        /// <param name="outputMessage"></param>
        /// <param name="frameInfo"></param>
        private static void HandleAppendClassName(
            StringBuilder outputMessage,
            FrameInfo frameInfo) {

            if (!FlagsHelper.IsSet(
                Instance.DisplayOptions,
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

        /// <summary>
        ///     Appends GUID of the object that called the Logger method.
        /// </summary>
        /// <param name="objectReference"></param>
        /// <param name="outputMessage"></param>
        private static void HandleAppendGUID(
            object objectReference,
            StringBuilder outputMessage) {

            if (objectReference == null) return;

            bool firstTime;
            var objectID = Instance.ObjectIDGenerator.GetId(
                objectReference,
                out firstTime);

            outputMessage.Append(string.Format(" (GUID: {0})", objectID));
        }

        /// <summary>
        ///     Appends caller method name to the log message.
        /// </summary>
        /// <param name="outputMessage"></param>
        /// <param name="stackInfo"></param>
        private static void HandleAppendMethodName(
            StringBuilder outputMessage,
            FrameInfo stackInfo) {

            if (!FlagsHelper.IsSet(
                Instance.DisplayOptions,
                AppendOptions.MethodName)) return;

            outputMessage.Append(string.Format(".{0}", stackInfo.MethodName));
        }

        /// <summary>
        ///     Indents log message.
        /// </summary>
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
        ///     Adds timestamp to the log message.
        /// </summary>
        /// <param name="outputMessage"></param>
        private static void HandleShowTimestamp(StringBuilder outputMessage) {
            if (!FlagsHelper.IsSet(
                Instance.DisplayOptions,
                AppendOptions.Timestamp)) return;

            outputMessage.Append(GetCurrentTimestamp());
            outputMessage.Append(" ");
        }

        /// <summary>
        ///     Base method used to create and save a log message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="methodEnabled"></param>
        /// <param name="objectReference"></param>
        private static void Log(
            string message,
            FrameInfo frameInfo,
            object objectReference) {

            if (!Instance.LoggingEnabled) return;

            // Filter by class name.
            if (!ClassInFilter(frameInfo.ClassName)) return;
            // Filter by method name.
            if (!MethodInFilter(frameInfo.MethodName)) return;

            // Log message to write.
            var outputMessage = new StringBuilder();

            // Add timestamp.
            HandleShowTimestamp(outputMessage);
            // Indent message.
            HandleIndentMessage(frameInfo, outputMessage);
            // Append message returned by callback.
            outputMessage.Append(message);
            // Append class name.
            HandleAppendClassName(outputMessage, frameInfo);
            // Append caller method name.
            HandleAppendMethodName(outputMessage, frameInfo);
            // Append object GUID.
            HandleAppendGUID(objectReference, outputMessage);
            // Append caller class name.
            HandleAppendCallerClassName(outputMessage);

            HandleEchoToConsole(outputMessage);
            HandleLogInRealTime(outputMessage);

            // There's no need to write cached messages since logging was made
            // in real time.
            if (Instance.LogInRealTime) return;

            // Add log message to the cache.
            Instance.logWriter.AddToCache(
                outputMessage.ToString(),
                Instance.EchoToConsole);
        }

        private static void HandleLogInRealTime(StringBuilder outputMessage) {

// Append message to the log file.
            if (Instance.LogInRealTime) {
                Instance.logWriter.WriteSingle(
                    outputMessage.ToString(),
                    Instance.filePath,
                    true);
            }
        }

        private static void HandleEchoToConsole(StringBuilder outputMessage) {

            if (Instance.EchoToConsole) {
                UnityEngine.Debug.Log(outputMessage.ToString());
            }
        }

        private static bool MethodInFilter(string methodName) {
            if (Instance.methodFilter.Count != 0) {
                // Return if method is not listed in the class filter. You can
                // set class filter in the inspector.
                if (Instance.methodFilter.Contains(methodName)) {
                    return true;
                }
                return false;
            }
            return true;
        }

        #endregion METHODS

    }

}