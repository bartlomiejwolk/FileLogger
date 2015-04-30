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
                        GameObject loggerGO = new GameObject();
                        loggerGO.AddComponent<Logger>();
                        loggerInstance = loggerGO.GetComponent<Logger>();
                    }
                }
                return loggerInstance;
            }
        }

        [MenuItem("Window/FileLogger")]
        public static void Init() {
            LoggerWindow window =
                (LoggerWindow)GetWindow(typeof(LoggerWindow));
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
                    //() => LoggerInstance.LogWriter.WriteAll(
                    //    LoggerInstance.FilePath,
                    //    false));

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
