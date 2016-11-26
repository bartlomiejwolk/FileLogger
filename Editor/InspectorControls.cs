// Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com)
//  
// This file is part of the FileLogger extension for Unity.
// Licensed under the MIT license. See LICENSE file in the project root folder.

using System;
using UnityEngine;

namespace FileLoggerTool.Editor {

    public static class InspectorControls {

        public static bool DrawStartStopButton(
            bool loggerState,
            bool enableOnPlay,
            Action<bool> stateChangedCallback) {

            var btnText = GetStartStopButtonText(
                loggerState,
                enableOnPlay);

            // Draw button.
            var btnState = GUILayout.Toggle(
                    loggerState,
                    btnText,
                    "Button");

            // Execute callback.
            if (btnState != loggerState) {
                if (stateChangedCallback != null) {
                    stateChangedCallback(btnState);
                }
            }

            // Return button state.
            return btnState == loggerState
                ? loggerState
                : !loggerState;
        }

        private static string GetStartStopButtonText(
            bool oldLoggingEnabledValue,
            bool enableOnPlay) {

            switch (Application.isPlaying) {
                // Play mode.
                case true:
                    return GetStartStopBtnTextForPlayMode(
                        oldLoggingEnabledValue,
                        enableOnPlay);
                // Edit mode.
                case false:
                    return GetStartStopBtnTextForEditMode(
                        oldLoggingEnabledValue);
            }

            return "";
        }

        private static string GetStartStopBtnTextForEditMode(
            bool oldLoggingEnabledValue) {

            switch (oldLoggingEnabledValue) {
                case true:
                    return "Stop";
                case false:
                    return "Start";
            }

            return "";
        }

        private static string GetStartStopBtnTextForPlayMode(
            bool oldLoggingEnabledValue,
            bool enableOnPlay) {

            switch (enableOnPlay) {
                case true:
                    if (oldLoggingEnabledValue) {
                        return "Stop";
                    }
                    return "Start";
                case false:
                    if (oldLoggingEnabledValue) {
                        return "Stop";
                    }
                    return "Start";
            }

            return "";
        }

    }

}