using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

namespace ATP.LoggingTools {

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
                if (GUILayout.Button("Start Logging")) {
                    LoggerInstance.LoggingEnabled = true;
                }
            }
            else if (Application.isPlaying && LoggerInstance.EnableOnPlay) {
                if (GUILayout.Button("Pause Logging")) {
                    LoggerInstance.LoggingEnabled = false;
                    LoggerInstance.LogCache.Add("[PAUSE]", true);
                }
            }
            else if (Application.isPlaying
                    && LoggerInstance.EnableOnPlay
                    && !LoggerInstance.LoggingEnabled) {

                if (GUILayout.Button("Continue Logging")) {
                    LoggerInstance.LoggingEnabled = true;
                }
            }
            else {
                if (GUILayout.Button("Stop Logging")) {
                    LoggerInstance.LoggingEnabled = false;
                    LoggerInstance.LogCache.WriteAll(
                            LoggerInstance.FilePath,
                            false);
                }
            }
            if (GUILayout.Button("->", GUILayout.Width(30))) {
                EditorGUIUtility.PingObject(LoggerInstance);
                Selection.activeGameObject = LoggerInstance.gameObject;
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
