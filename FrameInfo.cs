// Copyright (c) 2015 Bart³omiej Wo³k (bartlomiejwolk@gmail.com)
//  
// This file is part of the FileLogger extension for Unity.
// Licensed under the MIT license. See LICENSE file in the project root folder.

using System;
using System.Diagnostics;
using System.Reflection;
// todo remove
using Debug = UnityEngine.Debug;

namespace FileLogger {

    /// <summary>
    ///     Gets info about selected StackTrace frame. StackTrace is created at
    ///     construction time.
    /// </summary>
    public class FrameInfo {
        #region METHODS

        public FrameInfo(int frameIndex) {
            frame = stackTrace.GetFrame(frameIndex);

            // Frame can be null.
            try {
                method = frame.GetMethod();
                classType = method.DeclaringType;
            }
            catch (NullReferenceException e) {
                Debug.LogWarning("Frame not found: " + e);
            }
        }

        #endregion METHODS

        #region FIELDS

        private Type classType;
        private StackFrame frame;
        private MethodBase method;
        private StackTrace stackTrace = new StackTrace();

        #endregion FIELDS

        #region PROPERTIES

        public string ClassName {
            get {
                // Make sure that there's a frame to get the info from.
                if (frame != null) {
                    return classType.Name;
                }
                return "[Class info is not available]";
            }
        }

        public int FrameCount {
            get { return stackTrace.FrameCount; }
        }

        public string MethodName {
            get {
                if (frame != null) {
                    return method.Name;
                }
                return "[Method info is not available]";
            }
        }

        public string MethodSignature {
            get {
                if (frame != null) {
                    return method.ToString();
                }
                return "[Method info is not available]";
            }
        }

        public string QualifiedClassName {
            get {
                // Make sure that there's a frame to get the info from.
                if (frame != null) {
                    return classType.ToString();
                }
                return "[Class info is not available]";
            }
        }

        #endregion PROPERTIES
    }

}