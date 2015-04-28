using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

namespace mLogger {

    public class LoggerWindow : EditorWindow {

        private Logger loggerInstance;

        public Logger LoggerInstance {
            get { 
                if (loggerInstance == null) {
                    loggerInstance = FindObjectOfType<Logger>();
                    if (loggerInstance == null) {
                        GameObject loggerGO = new GameObject();
                        // TODO Name this new GO "Logger".
                        loggerGO.AddComponent<Logger>();
                        loggerInstance = loggerGO.GetComponent<Logger>();
                    }
                }
                return loggerInstance;
            }
        }

        [MenuItem("Debug/Logger")]
        public static void Init() {
            LoggerWindow window =
                (LoggerWindow)EditorWindow.GetWindow(typeof(LoggerWindow));
            window.title = "Logger";
            window.minSize = new Vector2(100f, 60f);
        }

        private void OnGUI() {
            EditorGUILayout.BeginHorizontal();
            if (!LoggerInstance.LoggingEnabled) {
                // todo extract to DrawStartStopToggle()
                LoggerInstance.LoggingEnabled = GUILayout.Toggle(
                    LoggerInstance.LoggingEnabled,
                    "Start Logging",
                    "Button");

            }
            else if (Application.isPlaying
                && LoggerInstance.EnableOnPlay
                && LoggerInstance.LoggingEnabled) {

                LoggerInstance.LoggingEnabled = GUILayout.Toggle(
                    LoggerInstance.LoggingEnabled,
                    "Pause Logging",
                    "Button");

                if (!LoggerInstance.LoggingEnabled) {
                    LoggerInstance.LogWriter.Add("[PAUSE]", true);
                }
            }
            else if (Application.isPlaying
                    && LoggerInstance.EnableOnPlay
                    && !LoggerInstance.LoggingEnabled) {

                    LoggerInstance.LoggingEnabled = GUILayout.Toggle(
                    LoggerInstance.LoggingEnabled,
                    "Continue Logging",
                    "Button");

            }
            else {
                LoggerInstance.LoggingEnabled = GUILayout.Toggle(
                LoggerInstance.LoggingEnabled,
                "Stop Logging",
                "Button");

                if (!LoggerInstance.LoggingEnabled) {
                    LoggerInstance.LogWriter.WriteAll(
                            LoggerInstance.FilePath,
                            false);
                }

            }

            if (GUILayout.Button("->", GUILayout.Width(30))) {
                EditorGUIUtility.PingObject(LoggerInstance);
                Selection.activeGameObject = LoggerInstance.gameObject;
            }

            EditorGUILayout.EndHorizontal();

            Repaint();
        }
    }
}
