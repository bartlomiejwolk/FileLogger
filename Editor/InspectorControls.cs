// Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com)
//  
// This file is part of the FileLogger extension for Unity.
// Licensed under the MIT license. See LICENSE file in the project root folder.

using System;
using UnityEngine;

namespace FileLogger {

    public static class InspectorControls {

        public static bool DrawStartStopButton(
            // todo rename to loggerState
            bool oldLoggingEnabledValue,
            bool enableOnPlay,
            Action<bool> stateChangedCallback) {

            var btnText = GetStartStopButtonText(
                oldLoggingEnabledValue,
                enableOnPlay);

            // Draw button.
            var btnState = GUILayout.Toggle(
                    oldLoggingEnabledValue,
                    btnText,
                    "Button");

            // Execute callback.
            if (btnState != oldLoggingEnabledValue) {
                if (stateChangedCallback != null) {
                    stateChangedCallback(btnState);
                }
            }

            // Return button state.
            return btnState == oldLoggingEnabledValue
                ? oldLoggingEnabledValue
                : !oldLoggingEnabledValue;
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
                    return "Logging Enabled";
                case false:
                    return "Logging Disabled";
            }

            return "";
        }

        private static string GetStartStopBtnTextForPlayMode(
            bool oldLoggingEnabledValue,
            bool enableOnPlay) {

            switch (enableOnPlay) {
                case true:
                    if (oldLoggingEnabledValue) {
                        return "Logging Enabled";
                    }
                    return "Logging Paused";
                case false:
                    if (oldLoggingEnabledValue) {
                        return "Logging Enabled";
                    }
                    return "Logging Paused";
            }

            return "";
        }

    }

}