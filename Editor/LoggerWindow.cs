// Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com)
//  
// This file is part of the AnimationPath Animator extension for Unity.
// Licensed under the MIT license. See LICENSE file in the project root folder.

#define DEBUG_LOGGER

using UnityEditor;
using UnityEngine;

namespace FileLogger {

    public class LoggerWindow : EditorWindow {

        private Logger loggerInstance;

        public Logger LoggerInstance {
            get {
                if (loggerInstance == null) {
                    loggerInstance = FindObjectOfType<Logger>();
                    if (loggerInstance == null) {
                        var loggerGO = new GameObject();
                        loggerGO.AddComponent<Logger>();
                        loggerInstance = loggerGO.GetComponent<Logger>();
                    }
                }
                return loggerInstance;
            }
        }

        [MenuItem("Window/FileLogger")]
        public static void Init() {
            var window =
                (LoggerWindow) GetWindow(typeof (LoggerWindow));
            window.title = "Logger";
            window.minSize = new Vector2(100f, 60f);
        }

        private void OnGUI() {
            EditorGUILayout.BeginHorizontal();

            // Draw Start/Stop button.
            LoggerInstance.LoggingEnabled =
                InspectorControls.DrawStartStopButton(
                    LoggerInstance.LoggingEnabled,
                    LoggerInstance.EnableOnPlay,
                    null,
                    () => LoggerInstance.LogWriter.Add("[PAUSE]", true),
                    () => Logger.StopLogging());

            // Draw -> button.
            if (GUILayout.Button("->", GUILayout.Width(30))) {
                EditorGUIUtility.PingObject(LoggerInstance);
                Selection.activeGameObject = LoggerInstance.gameObject;
            }

            EditorGUILayout.EndHorizontal();

            Repaint();
        }

    }

}