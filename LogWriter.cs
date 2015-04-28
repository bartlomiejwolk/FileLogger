using UnityEngine;
using System.Collections;
using System;
using System.IO;

// todo rename namespace
namespace OneDayGame.LoggingTools {

    /// Holds and writes to file logged messages.
    public class LogWriter {

        /// Event called on file write.
        public delegate void WriteEventHandler(object sender, EventArgs e);
        public static event WriteEventHandler WriteEvent;

        ///private bool prevLoggingEnabled = false;
        private StreamWriter writer;
        /// Log cache as a list. Each element is one log msg.
        private string[] logCache;
        /// Initial size of the cache array.
        ///
        /// This value is also used to resize the array when it's filled up.
        private int initArraySize = 1000000;
        /// 'logCacheArray' index for next log message.
        private int logIdx;
        /// Amount of messages logged during lifetime of the object.
        private int loggedMessages;

        public int InitArraySize {
            set { initArraySize = value; }
        }
        public int LoggedMessages {
            get { return loggedMessages; }
            set { loggedMessages = value; }
        }

        /// Save log message to cache.
        public void Add(string message, bool echoToConsole) {
            // Initialize array if not initialized already.
            if (
                    logCache == null ||
                    logCache.Length == 0) {
                logCache = new string[initArraySize];
            }
            // Cache message.
            logCache[logIdx] = message;
            // Handle "Echo To Console" inspector option.
            if (echoToConsole) {
                UnityEngine.Debug.Log(message);
            }
            logIdx += 1;
            loggedMessages += 1;
            // Resize array when needed.
            if (logIdx == logCache.Length) {
                Array.Resize(
                        ref logCache,
                        logCache.Length + initArraySize);
                UnityEngine.Debug.Log("Array resized to: " + logCache.Length);
            }
        }

        // todo move to Logger class
        public void WriteAll(string filePath, bool append) {
            // Create stream writer used to write log cache to file.
            using (writer = new StreamWriter(filePath, append)) {
                for (int i = 0; i < logIdx; i++) {
                    // write log to the file.
                    writer.WriteLine(logCache[i]);
                }
            }
            // Inform that file has been saved.
            DateTime now = DateTime.Now;
            UnityEngine.Debug.Log(string.Format("[{0:H:mm:ss}] Logs written: {1}.", now, loggedMessages));
            // Next logs save again at the beginning of the 'logCacheArray' field.
            logIdx = 0;
            loggedMessages = 0;
        }

        // todo move to Logger class
        public void WriteLast(string filePath) {
            // Create stream writer used to write log cache to file.
            using (writer = new StreamWriter(filePath, true)) {
                int lastCachedMsgIdx = logIdx - 1;
                // Write to file last cached message.
                writer.WriteLine(logCache[lastCachedMsgIdx]);
            }
            // Fire event.
            EventArgs e = new EventArgs();
            OnWriteEvent(e);
        }

        protected void OnWriteEvent(EventArgs e) {
            if (WriteEvent != null) {
                WriteEvent(this, e);
            }
        }
    }
}
