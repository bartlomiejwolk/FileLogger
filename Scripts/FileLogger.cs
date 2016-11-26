// Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com)
//  
// This file is part of the FileLogger extension for Unity.
// Licensed under the MIT license. See LICENSE file in the project root folder.

#define FILELOGGER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using FileLoggerTool.Enums;
using UnityEngine;
using Debug = UnityEngine.Debug;

// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable FieldCanBeMadeReadOnly.Local
#pragma warning disable 649
#pragma warning disable 169

namespace FileLoggerTool.Scripts
{
    /// <summary>
    ///     Allows logging custom messages to a file.
    /// </summary>
    /// <remarks>
    ///     Comment out the DEBUG directive to disable all calls to this class. For
    ///     Editor classes you must define DEBUG directive explicitly.
    /// </remarks>
    public sealed class FileLogger : MonoBehaviour
    {
        #region CONST

        public const string Version = "v0.1.0";

        // Index of the frame in which the logger method was called.
        private const int FrameIndex = 3;

        #endregion

        #region EVENT INVOCATORS

        private void OnStateChanged(bool state)
        {
            var handler = StateChanged;
            if (handler != null) handler(this, state);
        }

        #endregion EVENT INVOCATORS

        #region EVENT HANDLERS

        private void HandleLoggingOnOff(bool loggingEnabled)
        {
            // Save messages to file on logger stop.
            if (!WriteToFile) {}
            // There's no need to write cached messages since logging was made
            // in real time.
            else if (Instance.WriteInRealTime) {}
            else if (loggingEnabled) {}
            else
            {
                LogWriter.WriteAll(FileName, Append);
            }
        }

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
        public event StateChangedEventHandler StateChanged;

        #endregion EVENTS

        #region FIELDS

        private static FileLogger _instance;

        /// <summary>
        ///     If append messages to the file or overwrite.
        /// </summary>
        [SerializeField]
        private bool _append;

        /// <summary>
        ///     Allows specify what additional information will be included in a
        ///     single log line.
        /// </summary>
        [SerializeField]
        private AppendOptions _displayOptions = (AppendOptions) 65535;

        /// <summary>
        ///     List of classes that will be logged. Empty list disables
        ///     class filtering.
        /// </summary>
        [SerializeField]
        private List<string> _classFilter = new List<string>();

        [SerializeField]
        private bool _echoToConsole;

        /// <summary>
        ///     Keeps info about Logger methods state (enabled/disabled). Disabled
        ///     methods won't produce output.
        /// </summary>
        [SerializeField]
        private EnabledMethods _enabledMethods = (EnabledMethods) 65535;

        /// <summary>
        ///     Enable logging when in play mode.
        /// </summary>
        [SerializeField]
        private bool _enableOnPlay = true;

        /// <summary>
        ///     Output file name/path.
        /// </summary>
        [SerializeField]
        private string _fileName = "log.txt";

        [SerializeField]
        private bool _indentLine = true;

        /// <summary>
        ///     Initial size of the cache array.
        ///     When this array fills-up, it'll be resized by the same amount.
        /// </summary>
        [SerializeField]
        private int _initArraySize = 1000000;

        /// <summary>
        ///     When false, no logging is done.
        /// </summary>
        [SerializeField]
        private bool _loggingEnabled;

        [SerializeField]
        private bool _writeToFile = true;

        [SerializeField]
        private bool _writeInRealTime = true;

        private readonly LogWriter _logWriter = new LogWriter();

        /// <summary>
        ///     List of methods that will be logged.
        ///     Empty list disables method filtering.
        /// </summary>
        [SerializeField]
        private List<string> _methodFilter = new List<string>();

        [SerializeField]
        private MessageCategories _enabledMessageCategories = (MessageCategories)65535;

        private ObjectIDGenerator _objectIdGenerator;

        /// <summary>
        ///     If true, display fully qualified class name.
        /// </summary>
        [SerializeField]
        private bool _qualifiedClassName = true;

        [SerializeField]
        private bool _clearOnPlay = true;

        [SerializeField]
        private FilterType _classFilterType = FilterType.Disabled;

        [SerializeField]
        private FilterType _methodFilterType = FilterType.Disabled;

        [SerializeField]
        private bool _logUnityMessages = true;

        #endregion FIELDS

        #region PROPERTIES

        public static FileLogger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<FileLogger>();
                    if (_instance == null)
                    {
                        var singleton = new GameObject();
                        _instance = singleton.AddComponent<FileLogger>();
                        singleton.name = "(singleton) Logger";
                    }
                }
                return _instance;
            }
        }

        public bool Append
        {
            get { return _append; }
            set { _append = value; }
        }

        /// <summary>
        ///     Allows specify what additional information will be included in a
        ///     single log line.
        /// </summary>
        public AppendOptions DisplayOptions
        {
            get { return _displayOptions; }
            set { _displayOptions = value; }
        }

        /// <summary>
        ///     Keeps info about Logger methods state (enabled/disabled). Disabled
        ///     methods won't produce output.
        /// </summary>
        public EnabledMethods EnabledMethods
        {
            get { return _enabledMethods; }
            set { _enabledMethods = value; }
        }

        public bool EnableOnPlay
        {
            get { return _enableOnPlay; }
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public bool LoggingEnabled
        {
            get { return _loggingEnabled; }
            set
            {
                Debug.Log("LogginEnabled set to: " + value);
                var prevValue = _loggingEnabled;
                _loggingEnabled = value;

                if (prevValue != value)
                {
                    OnStateChanged(value);
                    HandleLoggingOnOff(value);
                }
            }
        }

        public bool WriteInRealTime
        {
            get { return _writeInRealTime; }
            set { _writeInRealTime = value; }
        }

        public LogWriter LogWriter
        {
            get { return _logWriter; }
        }

        /// <summary>
        ///     Generates and remembers GUID of all objects that call logger
        ///     passing "this" param.
        /// </summary>
        private ObjectIDGenerator ObjectIdGenerator
        {
            get
            {
                if (_objectIdGenerator != null)
                {
                    return _objectIdGenerator;
                }

                _objectIdGenerator = new ObjectIDGenerator();
                return _objectIdGenerator;
            }
        }

        public bool EchoToConsole
        {
            get { return _echoToConsole; }
            set { _echoToConsole = value; }
        }

        public bool ClearOnPlay
        {
            get { return _clearOnPlay; }
            set { _clearOnPlay = value; }
        }

        public bool WriteToFile
        {
            get { return _writeToFile; }
        }

        public MessageCategories EnabledMessageCategories
        {
            get { return _enabledMessageCategories; }
            set { _enabledMessageCategories = value; }
        }

        #endregion PROPERTIES

        #region UNITY MESSAGES

        private void Awake()
        {
            Debug.Log("FileLogger.Awake()");
            if (_instance == null)
            {
                // If I am the first instance, make me the Singleton
                _instance = this;
            }
            else
            {
                // If a Singleton already exists and you find another reference
                // in scene, destroy it!
                if (this != _instance)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            DontDestroyOnLoad(this);
            // Initialize 'cache' object.
            _logWriter.InitArraySize = _initArraySize;
            HandleEnableOnPlay();
            HandleLogUnityMessagesOption();
        }

        private void HandleLogUnityMessagesOption()
        {
            if (_logUnityMessages)
            {
                Application.logMessageReceived += OnApplicationOnLogMessageReceived;
            }
        }

        private void OnApplicationOnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            Log(condition, MessageCategories.Unity);
        }

        private void OnDestroy()
        {
            HandleExitPlayMode();
            Application.logMessageReceived -= OnApplicationOnLogMessageReceived;
        }

        private void HandleExitPlayMode()
        {
            if (_logWriter.CachedMessages > 0)
            {
                // Write single message to the file.
                _logWriter.WriteAll(FileName, Append);
            }
        }

        private void Start()
        {
        }

        #endregion UNITY MESSAGES

        #region METHODS

        private bool IsCategoryEnabled(MessageCategories category)
        {
            var categoryEnabled = (category & Instance._enabledMessageCategories) != 0;
            return categoryEnabled;
        }

        private void HandleEnableOnPlay()
        {
            if (!_enableOnPlay)
            {
                return;
            }
            _loggingEnabled = true;
            if (!WriteToFile) {}
            else if (!ClearOnPlay) {}
            else
            {
                ClearLogFile();
            }
        }

        public void LogCall()
        {
            LogCall(LogType.Log);
        }

        public void LogCall(MessageCategories categories)
        {
            LogCall(LogType.Log, categories);
        }

        public void LogCall(LogType logType)
        {
            DoLogCall(logType, MessageCategories.Generic, null);
        }

        public void LogCall(LogType logType, object objectReference)
        {
            DoLogCall(logType, MessageCategories.Generic, objectReference);
        }

        private void DoLogCall(LogType logType, MessageCategories category, object objectReference)
        {
            // Return if method is disabled.
            if (!FlagsHelper.IsSet(
                Instance.EnabledMethods,
                EnabledMethods.LogCall)) return;

            // Get info from call stack.
            var stackInfo = new FrameInfo(FrameIndex + 2);

            Log(
                logType,
                category,
                stackInfo.MethodSignature,
                stackInfo,
                objectReference);
        }

        public void LogResult(LogType logType, object result, MessageCategories category = MessageCategories.Generic)
        {
            DoLogResult(logType, category, result, null);
        }

        [Conditional("FILELOGGER")]
        public void LogResult(LogType logType, object objectReference, object result, MessageCategories category = MessageCategories.Generic)
        {
            DoLogResult(logType, category, result, objectReference);
        }

        private void DoLogResult(LogType logType, MessageCategories category, object objectRererence, object result)
        {
            // Return if method is disabled.
            if (!FlagsHelper.IsSet(
                Instance.EnabledMethods,
                EnabledMethods.LogResult)) return;

            // Compose log message.
            var message = string.Format("[RESULT: {0}]", result);

            // Get info from call stack.
            var stackInfo = new FrameInfo(FrameIndex - 1);

            // Log message.
            Log(
                logType,
                category,
                message,
                stackInfo,
                objectRererence);
        }

        public void LogString(LogType logType, string message, MessageCategories category = MessageCategories.Generic)
        {
            DoLogString(logType, category, message, null);
        }

        public void LogStringForObject(LogType logType, string message, object objectReference, MessageCategories categories = MessageCategories.Generic)
        {
            DoLogString(logType, categories, message, objectReference);
        }

        private void DoLogString(
            LogType logType,
            MessageCategories category,
            string message,
            object objectReference)
        {
            // Return if method is disabled.
            if (!FlagsHelper.IsSet(
                Instance.EnabledMethods,
                EnabledMethods.LogString)) return;

            // Get info from call stack.
            var stackInfo = new FrameInfo(FrameIndex - 1);

            // Log message.
            Log(
                logType,
                category,
                message,
                stackInfo,
                objectReference);
        }

        /// <summary>
        ///     Start Logger.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="append"></param>
        public void StartLogging(
            string fileName = "log.txt",
            bool append = false)
        {
            Instance._fileName = fileName;
            Instance._append = append;
            Instance.LoggingEnabled = true;
        }

        /// <summary>
        ///     Stop Logger.
        /// </summary>
        public void StopLogging()
        {
            Instance.LoggingEnabled = false;

            // There's no need to write cached messages since logging was made
            // in real time.
            if (Instance.WriteInRealTime) return;

            // Write single message to the file.
            Instance._logWriter.WriteAll(
                Instance.FileName,
                Instance.Append);
        }

        /// <summary>
        ///     Add timestamp to a single log message.
        /// </summary>
        /// <returns></returns>
        private string GetCurrentTimestamp()
        {
            var now = DateTime.Now;
            var timestamp =
                string.Format("[{0:H:mm:ss:fff}]", now);

            return timestamp;
        }

        /// <summary>
        ///     Helper method. Appends caller class name to the output message.
        /// </summary>
        /// <param name="outputMessage"></param>
        private void HandleAppendCallerClassName(
            StringBuilder outputMessage)
        {
            if (!FlagsHelper.IsSet(
                Instance.DisplayOptions,
                AppendOptions.ParentClassName)) return;

            // Get info from call stack.
            var callerStackInfo = new FrameInfo(FrameIndex + 7);

            if (Instance._qualifiedClassName)
            {
                // Append fully qualified caller class name.
                outputMessage.Append(
                    ", <- " + callerStackInfo.QualifiedClassName + "");
            }
            else
            {
                outputMessage.Append(
                    ", <- " + callerStackInfo.ClassName + "");
            }
        }

        /// <summary>
        ///     Helper method. Appends class name to the output message.
        /// </summary>
        /// <param name="outputMessage"></param>
        /// <param name="frameInfo"></param>
        private void HandleAppendClassName(
            StringBuilder outputMessage,
            FrameInfo frameInfo)
        {
            if (!FlagsHelper.IsSet(
                Instance.DisplayOptions,
                AppendOptions.CallerClassName)) return;

            if (Instance._qualifiedClassName)
            {
                // Append fully qualified class name.
                outputMessage.Append(
                    ", @ " + frameInfo.QualifiedClassName + "");
            }
            else
            {
                // Append class name.
                outputMessage.Append(", @ " + frameInfo.ClassName + "");
            }
        }

        /// <summary>
        ///     Appends GUID of the object that called the Logger method.
        /// </summary>
        /// <param name="objectReference"></param>
        /// <param name="outputMessage"></param>
        private void HandleAppendGuid(
            object objectReference,
            StringBuilder outputMessage)
        {
            if (objectReference == null) return;

            bool firstTime;
            var objectId = Instance.ObjectIdGenerator.GetId(
                objectReference,
                out firstTime);

            outputMessage.Append(string.Format(" (GUID: {0})", objectId));
        }

        /// <summary>
        ///     Appends caller method name to the log message.
        /// </summary>
        /// <param name="outputMessage"></param>
        /// <param name="stackInfo"></param>
        private void HandleAppendMethodName(
            StringBuilder outputMessage,
            FrameInfo stackInfo)
        {
            if (!FlagsHelper.IsSet(
                Instance.DisplayOptions,
                AppendOptions.MethodName))
            {
                return;
            }

            outputMessage.Append(string.Format(".{0}", stackInfo.MethodName));
        }

        /// <summary>
        ///     Indents log message.
        /// </summary>
        /// <param name="frameInfo"></param>
        /// <param name="outOutputMessage"></param>
        private void HandleIndentMessage(
            FrameInfo frameInfo,
            StringBuilder outOutputMessage)
        {
            if (!Instance._indentLine) return;

            for (var i = 0; i < frameInfo.FrameCount; i++)
            {
                outOutputMessage.Append("| ");
            }
        }

        /// <summary>
        ///     Adds timestamp to the log message.
        /// </summary>
        /// <param name="outOutputMessage"></param>
        private void HandleShowTimestamp(StringBuilder outOutputMessage)
        {
            if (!FlagsHelper.IsSet(
                Instance.DisplayOptions,
                AppendOptions.Timestamp))
            {
                return;
            }
            outOutputMessage.Append(GetCurrentTimestamp());
            outOutputMessage.Append(" ");
        }

        /// <summary>
        ///     Base method used to create and save a log message.
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="category"></param>
        /// <param name="message"></param>
        /// <param name="frameInfo"></param>
        /// <param name="objectReference"></param>
        private void Log(
            LogType logType,
            MessageCategories category,
            string message,
            FrameInfo frameInfo,
            object objectReference)
        {
            // todo use variables to store method return value
            if (!Debug.isDebugBuild) {}
            else if (!Instance.LoggingEnabled) {}
            // filter by category
            else if (!IsCategoryEnabled(category)) {}
            // filter by class name.
            else if (!ClassEnabledForLogging(frameInfo.ClassName)) {}
            // filter by method name.
            else if (!MethodEnabledForLogging(frameInfo.MethodName)) {}
            else
            {
                // Log message to write.
                var outputMessage = new StringBuilder();
                HandleShowTimestamp(outputMessage);
                HandleShowMessageCategory(category, outputMessage);
                HandleIndentMessage(frameInfo, outputMessage);
                // Append message returned by callback.
                outputMessage.Append(message);
                HandleAppendClassName(outputMessage, frameInfo);
                HandleAppendMethodName(outputMessage, frameInfo);
                HandleAppendGuid(objectReference, outputMessage);
                HandleAppendCallerClassName(outputMessage);
                HandleEchoToConsole(outputMessage, logType);
                HandleLogInRealTime(outputMessage);
                // There's no need to write cached messages since logging was made
                // in real time.
                if (Instance.WriteInRealTime) {}
                else
                {
                    // Add log message to the cache.
                    Instance._logWriter.AddToCache(
                        outputMessage.ToString(),
                        Instance.EchoToConsole);
                }
            }
        }

        private void HandleShowMessageCategory(MessageCategories categories, StringBuilder outputMessage)
        {
            var showCategoryEnabled = FlagsHelper.IsSet(Instance.DisplayOptions,
                AppendOptions.MessageCategory);
            if (!showCategoryEnabled)
            {
            }
            else
            {
                var categoryInfo = "[" + categories + "] ";
                outputMessage.Append(categoryInfo);
                outputMessage.Append(" ");
            }
        }

        // Appends message to the log file.
        private void HandleLogInRealTime(StringBuilder outputMessage)
        {
            if (!Instance.WriteToFile)
            {
            }
            else if (!Instance.WriteInRealTime)
            {
            }
            else
            {
                Instance._logWriter.WriteSingle(
                    outputMessage.ToString(),
                    Instance._fileName,
                    true);
            }
        }

        private void HandleEchoToConsole(StringBuilder outputMessage, LogType logType)
        {
            if (Instance.EchoToConsole)
            {
                Debug.logger.Log(logType, outputMessage.ToString());
            }
        }

        public void ClearLogFile()
        {
            LogWriter.ClearLogFile(FileName);
        }

        private bool ClassEnabledForLogging(string className)
        {
            if (Instance._classFilterType == FilterType.Disabled)
            {
                return true;
            }

            // No classes were specified in the filter.
            if (Instance._classFilter.Count == 0)
            {
                return true;
            }

            if (Instance._classFilterType == FilterType.Include)
            {
                return Instance._classFilter.Contains(className);
            }

            if (Instance._classFilterType == FilterType.Exclude)
            {
                return !Instance._classFilter.Contains(className);
            }

            return true;
        }

        private bool MethodEnabledForLogging(string methodName)
        {
            if (Instance._methodFilterType == FilterType.Disabled)
            {
                return true;
            }

            // No methods were specified in the filter.
            if (Instance._methodFilter.Count == 0)
            {
                return true;
            }

            if (Instance._methodFilterType == FilterType.Include)
            {
                return Instance._methodFilter.Contains(methodName);
            }

            if (Instance._methodFilterType == FilterType.Exclude)
            {
                return !Instance._methodFilter.Contains(methodName);
            }

            return true;
        }

        #endregion METHODS

        // todo remove default value (some for other methods)
        public void Log(string message, MessageCategories category = MessageCategories.Generic)
        {
            var stackInfo = new FrameInfo(FrameIndex);
            Log(LogType.Log, category, message, stackInfo, null);
        }

        public void LogError(string message, MessageCategories category)
        {
            var stackInfo = new FrameInfo(FrameIndex);
            Log(LogType.Error, category, message, stackInfo, null);
        }
    }
}