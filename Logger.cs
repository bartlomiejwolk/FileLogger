#define DEBUG

using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using OneDayGame.LoggingTools;

// TODO Remove MessageType class.
namespace ATP.Logger {

    /// Logs to a file function calls, return values or any string.
    ///
    /// Comment out the DEBUG directive to disable all calls to this class.
    /// For Editor classes you must define DEBUG directive explicitly.
	/// \todo Remove LogPrimitive() methods and use LogStringArray and
	/// LogIntArray() instead.
	public class Logger : GameComponent {

		private static Logger instance;

        private Cache logCache = new Cache();

        /// Output file name/path.
        [SerializeField]
        private string filePath = "log.txt";

		// When false, no logging is done.
		[SerializeField]
		private bool loggingEnabled;

		[SerializeField]
        private bool inGameLabel;

        [SerializeField]
        private bool logInRealTime = false;

		[SerializeField]
		private bool echoToConsole = false;

		// TODO Add to inspector.
		[SerializeField]
		private bool enableLogList = true;

		// TODO Add to inspector.
		[SerializeField]
		private bool enableLogDictionary = true;

        [SerializeField]
        private bool enableLogCall = true;

        [SerializeField]
        private bool enableLogResult = true;

		/// \todo Change to false.
		[SerializeField]
		private bool enableLogStackTrace = true;

        [SerializeField]
        private bool enableLogString = true;

        /// Enable logging when in play mode.
        [SerializeField]
        private bool enableOnPlay;

        /// Initial size of the cache array.
        ///
        /// When this array fills-up, it'll be resized by the same amount.
        [SerializeField]
        private int initArraySize = 1000000;

        /// If append class name to every log message.
        [SerializeField]
        private bool appendClassName = true;

        /// If append caller class name to every log message.
        [SerializeField]
        private bool appendCallerClassName = true;

        /// If true, display fully qualified class name.
		/// \todo Rename to qualifiedClassName.
        [SerializeField]
        private bool fullyQualifiedClassName = true;

		/// \todo Rename to addTimestamp.
		[SerializeField]
		private bool showTimestamp = true;

		[SerializeField]
		private bool indentMessage = true;

        /// Class filter.
        /// 
        /// List of classes that will be logged.
        /// \remark Empty list disables class filtering.
        [SerializeField]
        private List<string> classFilter = new List<string>();

        /// Method filter.
        /// 
        /// List of methods that will be logged.
        /// \remark Empty list disables method filtering.
        [SerializeField]
        private List<string> methodFilter = new List<string>();

        /// If append messages to the file or overwrite.
        private bool append = false;

		public static Logger Instance {
			get {
				if(instance == null) {
					instance = GameObject.FindObjectOfType<Logger>();
                    if (instance == null) {
                        return null;
                    }
					//Tell unity not to destroy this object when loading a new scene!
					DontDestroyOnLoad(instance.gameObject);
				}
				return instance;
			}
		}
        public Cache LogCache {
            get { return logCache; }
        }
        public string FilePath {
            get { return filePath; }
        }
        public bool Append {
            get { return append; }
            set { append = value; }
        }
		public bool LoggingEnabled {
			get { return loggingEnabled; }
			set { loggingEnabled = value; }
		}
        public bool EnableOnPlay {
            get { return enableOnPlay; }
        }

		private void Awake() {
			if (instance == null) {
				// If I am the first instance, make me the Singleton
				instance = this;
				DontDestroyOnLoad(this);
			}
			else {
				// If a Singleton already exists and you find
				// another reference in scene, destroy it!
				if(this != instance) {
					Destroy(this.gameObject);
				}
			}

            // Initialize 'cache' object.
            logCache.InitArraySize = initArraySize;
		}

        private void Start() {
            // Handle 'Enable On Play' inspector option.
            if (enableOnPlay) {
                loggingEnabled = true;
            }
        }

        private void Update() {
            // Handle "In-game Label" inspector option.
            //if (
            //        loggingEnabled == true
            //        && inGameLabel) {
            //    Logger.Instance.DisplayLabel();
            //}
        }

		private void OnDestroy() {
            // Don't write to file if 'logInRealTime' was selected.
            if (logInRealTime == true) {
                return;
            }
            // Write log to file when 'enableOnPlay' was selected.
            if (enableOnPlay == true) {
                // Write single message to the file.
                logCache.WriteAll(filePath, append);
            }
		}

        /// Start Logger.
		[Conditional("DEBUG")]
        public static void Start(
                string filePath = "log.txt",
                bool append = false) {
            Logger.Instance.filePath = filePath;
            Logger.Instance.append = append;
			Logger.Instance.LoggingEnabled = true;
		}

        /// Stop Logger.
		[Conditional("DEBUG")]
		public static void Stop() {
			Logger.Instance.LoggingEnabled = false;
            // Write single message to the file.
            Logger.Instance.logCache.WriteAll(
                    Logger.Instance.filePath,
                    Logger.Instance.append);
		}

		[Conditional("DEBUG")]
        public static void LogCall() {
			Log((stackInfo) => { return stackInfo.MethodSignature; },
					Instance.enableLogCall,
					Instance.showTimestamp,
					Instance.indentMessage,
					Instance.appendClassName,
					Instance.appendCallerClassName);
        }

		public static void LogString(
                string format,
                params object[] paramList) {

			// Compose log message.
			string message = string.Format(format, paramList);

			// Log message.
			Log(stackInfo => { return message; },
					Logger.Instance.enableLogString,
					Logger.Instance.showTimestamp,
					Logger.Instance.indentMessage,
					Logger.Instance.appendClassName,
					Logger.Instance.appendCallerClassName);
		}

		public static void LogStackTrace() {
			StackTrace stackTrace = new StackTrace();
			StringBuilder message = new StringBuilder();
			for(int i = 1; i < stackTrace.FrameCount; i++ ) {
				StackFrame stackFrame = stackTrace.GetFrame(i);
				for (int j = 0; j < i; j++) {
					message.Append("| ");
				}
				message.Append(stackFrame.GetMethod());
				if (i == stackTrace.FrameCount - 1) {
					break;
				}
				message.Append("\n");
			}

			Log(stackInfo => { return message.ToString(); },
					Logger.Instance.enableLogStackTrace,
					false,
					false,
					false,
					false);
		}

        public static void LogResult(object result) {
			// Compose log message.
			string message = string.Format("[RESULT: {0}]", result.ToString());

			// Log message.
			Log(stackInfo => { return message; },
					Logger.Instance.enableLogResult,
					Logger.Instance.showTimestamp,
					Logger.Instance.indentMessage,
					Logger.Instance.appendClassName,
					Logger.Instance.appendCallerClassName);
		}

		public static void LogList<T>(List<T> list, string info) {
			int endIndex = list.Count - 1;
			DoLogList(list, 0, endIndex, info);
		}

		public static void LogList<T>(
				List<T> list,
				string info,
				int beginningIndex,
				int endIndex) {

			DoLogList(list, beginningIndex, endIndex, info);
		}

		public static void LogDictionary<key, value>(
				Dictionary<key, value> dict, string info) {

			StringBuilder message = new StringBuilder();

			if (info.Length != 0) {
				message.Append(info);
				message.Append("\n");
			}

			foreach (KeyValuePair<key, value> entry in dict) {
				message.Append(entry.Key + ": " + entry.Value);
				// TODO Don't append new line after last dict. element.
				message.Append("\n");
			}

			// Log message.
			Log(stackInfo => { return message.ToString(); },
					Logger.Instance.enableLogDictionary,
					false,
					false,
					false,
					false);
		}

		public static void LogPrimitive(string[] array, string info) {
			int endIndex = array.Length - 1;
			LogStringArray(array, 0, endIndex, info);
		}

		public static void LogPrimitive(
				string[] array,
				string info,
				int beginningIndex,
				int endIndex) {

			LogStringArray(array, beginningIndex, endIndex, info);
		}

		public static void LogPrimitive(int[] array, string info) {
			int endIndex = array.Length - 1;
			LogIntArray(array, 0, endIndex, info);
		}

		public static void LogPrimitive(
				int[] array,
				string info,
				int beginningIndex,
				int endIndex) {

			LogIntArray(array, beginningIndex, endIndex, info);
		}

		private static void DoLogList<T>(
				List<T> list,
				int beginningIndex,
				int endIndex,
				string info = "") {

			StringBuilder message = new StringBuilder();

			if (info.Length != 0) {
				message.Append(info);
				message.Append("\n");
			}

			for (int i = 0; i < list.Count; i++) {
				message.Append(list[i]);
				if (i == list.Count - 1) {
					break;
				}
				message.Append("\n");
			}

			// Log message.
			Log(stackInfo => { return message.ToString(); },
					Logger.Instance.enableLogList,
					false,
					false,
					false,
					false);
		}

		private static void LogStringArray(
				string[] array,
				int beginningIndex,
				int endIndex,
				string info = "") {

			StringBuilder message = new StringBuilder();

			if (info.Length != 0) {
				message.Append(info);
				message.Append("\n");
			}

			for (int i = beginningIndex; i <= endIndex; i++) {
				message.Append(array[i]);
				// Don't append new line after last message.
				if (i != endIndex) {
					message.Append("\n");
				}
			}

			// Log message.
			Log(stackInfo => { return message.ToString(); },
					Logger.Instance.enableLogList,
					false,
					false,
					false,
					false);
		}

		private static void LogIntArray(
				int[] array,
				int beginningIndex,
				int endIndex,
				string info = "") {

			StringBuilder message = new StringBuilder();

			if (info.Length != 0) {
				message.Append(info);
				message.Append("\n");
			}

			for (int i = beginningIndex; i <= endIndex; i++) {
				message.Append(array[i]);
				// Don't append new line after last message.
				if (i != endIndex) {
					message.Append("\n");
				}
			}

			// Log message.
			Log(stackInfo => { return message.ToString(); },
					Logger.Instance.enableLogList,
					false,
					false,
					false,
					false);
		}

		private static void Log(Func<StackInfo, string> composeMessage,
				bool enableLoggingForMethod,
				bool showTimestamp,
				bool indentMessage,
				bool appendClassName,
				bool appendCallerClassName) {

			if (enableLoggingForMethod == false) {
				return;
			}

            // Return if Logger instance does not exists.
            if (Logger.Instance == null) {
                return;
            }

			// Return if user stopped logging from the inspector.
			if (Logger.Instance.LoggingEnabled == false) {
				return;
			}

			// Get info from call stack.
			StackInfo stackInfo = new StackInfo(3);

			// Filter by class name.
			if (ClassInFilter(stackInfo.ClassName) == false) {
				return;
			}

			// Filter by method name.
			if (MethodInFilter(stackInfo.MethodName) == false) {
				return;
			}

			// Log message to write.
			StringBuilder outputMessage = new StringBuilder();

            // Add timestamp.
			if (showTimestamp) {
				outputMessage.Append(Logger.GetCurrentTimestamp());
				outputMessage.Append(" ");
			}

            // Indent message.
			if (indentMessage) {
				for (int i = 0; i < stackInfo.FrameCount; i++) {
					outputMessage.Append("| ");
				}
			}

			// Add message if not empty.
			outputMessage.Append(composeMessage(stackInfo));

			// Apend class name if enabled in the inspector.
            if (appendClassName == true) {
                if (Logger.Instance.fullyQualifiedClassName == true) {
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
			if (appendCallerClassName == true) {

				// Get info from call stack.
				StackInfo callerStackInfo = new StackInfo(4);

                if (Logger.Instance.fullyQualifiedClassName == true) {
                    // Append fully qualified caller class name.
					outputMessage.Append(
							", <- " + callerStackInfo.QualifiedClassName + "");
				}
				else {
                    outputMessage.Append(", <- " + callerStackInfo.ClassName + "");
                }
			}

            // Add log message to the cache.
			Logger.Instance.logCache.Add(
					outputMessage.ToString(),
					Logger.Instance.echoToConsole);

			if (Logger.Instance.logInRealTime == true) {
				Logger.Instance.logCache.WriteLast(Logger.Instance.filePath);
			}
        }

        /// Add timestamp to a single log message.
        ///
        /// \return String with timestamp.
		/// \todo Make it static and put in some utility class.
        private static string GetCurrentTimestamp() {
            DateTime now = DateTime.Now;
			string timestamp =
				string.Format("[{0:H:mm:ss:fff}]", now);

            return timestamp;
        }

        //private void DisplayLabel() {
        //    MeasureIt.Set("Logs captured", logCache.LoggedMessages);
        //}

		private static bool ClassInFilter(string className) {
            if (Logger.Instance.classFilter.Count != 0) {
                // Return if class is not listed in the class filter.
                // You can set class filter in the inspector.
                if (Logger.Instance.classFilter.Contains(className) ==
                        true) {

                    return true;
                }
				return false;
			}
			// Filtering is disabled so every class is treated as being
			// in the filter.
			return true;
		}

		private static bool MethodInFilter(string methodName) {
            if (Logger.Instance.methodFilter.Count != 0) {
                // Return if method is not listed in the class filter.
                // You can set class filter in the inspector.
                if (Logger.Instance.methodFilter.Contains(methodName) ==
                        true) {

                    return true;
                }
				return false;
			}
			return true;
		}
	}
}
