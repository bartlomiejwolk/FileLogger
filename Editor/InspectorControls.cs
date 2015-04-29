// Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com)
//  
// This file is part of the AnimationPath Animator extension for Unity.
// Licensed under the MIT license. See LICENSE file in the project root folder.

using System;
using UnityEngine;

namespace mLogger {

    public static class InspectorControls {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldLoggingEnabledValue"></param>
        /// <param name="enableOnPlay"></param>
        /// <param name="stateChangedCallback">Callback executed on every state change.</param>
        /// <param name="pauseCallback">Callback executed only in play mode, when on logger pause.</param>
        /// <param name="disableLoggerCallback">Callback executed on logger disable.</param>
        /// <returns></returns>
        public static bool DrawStartStopButton(
            bool oldLoggingEnabledValue,
            bool enableOnPlay,
            Action stateChangedCallback,
            Action pauseCallback,
            Action disableLoggerCallback) {

            bool newLoggingEnabledValue;

            // Editor mode, logging disabled.
            if (!oldLoggingEnabledValue && !Application.isPlaying) {
                newLoggingEnabledValue = GUILayout.Toggle(
                    oldLoggingEnabledValue,
                    "Logging Disabled",
                    "Button");

                // If value was changed..
                if (newLoggingEnabledValue != oldLoggingEnabledValue) {
                    stateChangedCallback();
                }
            }
            else if (Application.isPlaying
                     && enableOnPlay
                     && oldLoggingEnabledValue) {

                newLoggingEnabledValue = GUILayout.Toggle(
                    oldLoggingEnabledValue,
                    "Logging Enabled",
                    "Button");

                // If value was changed..
                if (newLoggingEnabledValue != oldLoggingEnabledValue) {
                    stateChangedCallback();
                    pauseCallback();
                }
            }
            // Play mode, logging disabled.
            else if (Application.isPlaying
                     && enableOnPlay
                     && !oldLoggingEnabledValue) {

                newLoggingEnabledValue = GUILayout.Toggle(
                    oldLoggingEnabledValue,
                    "Logging Paused",
                    "Button");

                // If value was changed..
                if (newLoggingEnabledValue != oldLoggingEnabledValue) {
                    stateChangedCallback();
                }
            }
            else {
                newLoggingEnabledValue = GUILayout.Toggle(
                    oldLoggingEnabledValue,
                    "Logging Enabled",
                    "Button");

                // If value was changed..
                if (newLoggingEnabledValue != oldLoggingEnabledValue) {
                    disableLoggerCallback();
                    stateChangedCallback();
                }
            }

            return newLoggingEnabledValue;
        }

    }

}