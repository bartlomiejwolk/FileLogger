// Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com)
//  
// This file is part of the FileLogger extension for Unity.
// Licensed under the MIT license. See LICENSE file in the project root folder.

using System;
using System.IO;
using UnityEngine;

namespace FileLogger {

    /// Holds and writes to file logged messages.
    public class LogWriter {
        #region Delegates

        /// Event called on file write.
        public delegate void WriteEventHandler(object sender, EventArgs e);

        #endregion

        /// Initial size of the cache array.
        /// 
        /// This value is also used to resize the array when it's filled up.
        private int initArraySize = 1000000;

        /// Log cache as a list. Each element is one log msg.
        private string[] logCache;

        /// Amount of messages logged during lifetime of the object.
        private int loggedMessages;

        /// 'logCacheArray' index for next log message.
        private int logIdx;

        /// private bool prevLoggingEnabled = false;
        private StreamWriter writer;

        public int InitArraySize {
            set { initArraySize = value; }
        }

        public int LoggedMessages {
            get { return loggedMessages; }
            set { loggedMessages = value; }
        }

        /// <summary>
        ///     Event called on every write to the file.
        /// </summary>
        public static event WriteEventHandler WriteEvent;

        /// Save log message to cache.
        public void AddToCache(string message, bool echoToConsole) {
            // Initialize array if not initialized already.
            if (
                logCache == null ||
                logCache.Length == 0) {
                logCache = new string[initArraySize];
            }
            // Cache message.
            logCache[logIdx] = message;

            logIdx += 1;
            loggedMessages += 1;
            // Resize array when needed.
            if (logIdx == logCache.Length) {
                Array.Resize(
                    ref logCache,
                    logCache.Length + initArraySize);
                Debug.Log("Array resized to: " + logCache.Length);
            }
        }

        public void WriteAll(string filePath, bool append) {
            // Create stream writer used to write log cache to file.
            using (writer = new StreamWriter(filePath, append)) {
                for (var i = 0; i < logIdx; i++) {
                    // write log to the file.
                    writer.WriteLine(logCache[i]);
                }
            }
            // Inform that file has been saved.
            var now = DateTime.Now;
            Debug.Log(
                string.Format(
                    "[{0:H:mm:ss}] Logs written: {1}.",
                    now,
                    loggedMessages));
            // Next logs save again at the beginning of the 'logCacheArray'
            // field.
            logIdx = 0;
            loggedMessages = 0;
        }

        public void WriteLast(string filePath) {
            // Create stream writer used to write log cache to file.
            using (writer = new StreamWriter(filePath, true)) {
                var lastCachedMsgIdx = logIdx - 1;
                // Write to file last cached message.
                writer.WriteLine(logCache[lastCachedMsgIdx]);
            }
            // Fire event.
            var e = new EventArgs();
            OnWriteEvent(e);
        }

        public void WriteSingle(string message, string filePath, bool append) {
            // Create stream writer used to write log cache to file.
            using (writer = new StreamWriter(filePath, append)) {
                writer.WriteLine(message);
            }

            // Fire event.
            var e = new EventArgs();
            OnWriteEvent(e);
        }


        protected void OnWriteEvent(EventArgs e) {
            if (WriteEvent != null) {
                WriteEvent(this, e);
            }
        }

        /// <summary>
        ///     Clear log file.
        /// </summary>
        public void ClearLogFile(string filePath) {
            StreamWriter writer;

            // Create stream writer used to write log cache to file.
            using (writer = new StreamWriter(filePath, false)) {
                writer.WriteLine(String.Empty);
            }

            Debug.Log("Log file cleared!");
        }

    }

}