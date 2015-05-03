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
                                              | AppendOptions.CallerClassName
                                              | AppendOptions.MethodName
                                              | AppendOptions.ParentClassName;

        /// <summary>
        ///     List of classes that will be logged. Empty list disables
        ///     class filtering.
        /// </summary>
        [SerializeField]
        private List<string> classFilter = new List<string>();

        [SerializeField]
        private bool echoToConsole = true;

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
        private string fileName = "log.txt";

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
        private bool logInRealTime = true;

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

        public string FileName {
            get { return fileName; }
        }

        public bool LoggingEnabled {
            get { return loggingEnabled; }
            set {
                var prevValue = loggingEnabled;
                loggingEnabled = value;

                if (prevValue != value) {
                    OnStateChanged(value);
                    HandleStateChange(value);
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
            HandleExitPlayMode();
        }

        private void HandleExitPlayMode() {

            if (LogInRealTime) return;
            if (!EnableOnPlay) return;

            // Write single message to the file.
            logWriter.WriteAll(FileName, Append);
        }

        private void Start() {
            HandleEnableOnPlay();
        }

        #endregion UNITY MESSAGES

        #region EVENT INVOCATORS

        private void OnStateChanged(bool state) {
            var handler = StateChanged;
            if (handler != null) handler(this, state);
        }

        #endregion EVENT INVOCATORS

        #region EVENT HANDLERS
        void HandleStateChange(bool newState) {
            // There's no need to write cached messages since logging was made
            // in real time.
            if (Instance.LogInRealTime) return;

            // Save messages to file on logger stop.
            if (!newState) LogWriter.WriteAll(FileName, Append);
        }
        #endregion

        #region METHODS
        private void HandleEnableOnPlay() {
            if (!enableOnPlay) return;

            loggingEnabled = true;
            if (ClearOnPlay) LogWriter.ClearLogFile(FileName);
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
        public static void LogResult(object objectReference, object result) {
            DoLogResult(result, objectReference);
        }

        private static void DoLogResult(object objectRererence, object result) {
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

        [Conditional("DEBUG_LOGGER")]
        public static void LogString(
            string format,
            params object[] paramList) {

            DoLogString(format, null, paramList);
        }

        [Conditional("DEBUG_LOGGER")]
        public static void LogString(
            object objectReference,
            string format,
            params object[] paramList) {

            DoLogString(format, objectReference, paramList);
        }

        private static void DoLogString(
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
        /// <param name="fileName"></param>
        /// <param name="append"></param>
        [Conditional("DEBUG_LOGGER")]
        public static void StartLogging(
            string fileName = "log.txt",
            bool append = false) {

            Instance.fileName = fileName;
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
                Instance.FileName,
                Instance.Append);
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
                AppendOptions.ParentClassName)) return;

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
                AppendOptions.CallerClassName)) return;

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
                    Instance.fileName,
                    true);
            }
        }

        private static void HandleEchoToConsole(StringBuilder outputMessage) {
            if (Instance.EchoToConsole) {
                UnityEngine.Debug.Log(outputMessage.ToString());
            }
        }

        public void ClearLogFile() {
            LogWriter.ClearLogFile(FileName);
        }

        private static bool MethodInFilter(string methodName) {
            // No methods were specified in the filter.
            if (Instance.methodFilter.Count == 0) return true;

            // Return false if method is not listed in the class filter.
            return Instance.methodFilter.Contains(methodName);
        }

        #endregion METHODS
    }

}