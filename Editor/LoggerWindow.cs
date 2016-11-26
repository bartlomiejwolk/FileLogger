// Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com)
//  
// This file is part of the FileLogger extension for Unity.
// Licensed under the MIT license. See LICENSE file in the project root folder.

#define FILELOGGER

using UnityEditor;
using UnityEngine;

namespace FileLoggerTool.Editor {

    public class LoggerWindow : EditorWindow
    {
        private const string _loggerGoName = "FileLogger";
        private FileLogger _fileLoggerInstance;

        public FileLogger FileLoggerInstance {
            get {
                if (_fileLoggerInstance == null) {
                    _fileLoggerInstance = FindObjectOfType<FileLogger>();
                    if (_fileLoggerInstance == null) {
                        var loggerGo = new GameObject
                        {
                            name = _loggerGoName
                        };
                        loggerGo.AddComponent<FileLogger>();
                        _fileLoggerInstance = loggerGo.GetComponent<FileLogger>();
                    }
                }
                return _fileLoggerInstance;
            }
        }

        [MenuItem("Window/FileLogger")]
        public static void Init() {
            var window =
                (LoggerWindow) GetWindow(typeof (LoggerWindow));
            window.titleContent = new GUIContent("FileLogger");
            window.minSize = new Vector2(100f, 60f);
        }

        private void OnGUI() {
            EditorGUILayout.BeginHorizontal();

            // Draw Start/Stop button.
            FileLoggerInstance.LoggingEnabled =
                InspectorControls.DrawStartStopButton(
                    FileLoggerInstance.LoggingEnabled,
                    FileLoggerInstance.EnableOnPlay,
                    null);

            // Draw -> button.
            if (GUILayout.Button("->", GUILayout.Width(30))) {
                EditorGUIUtility.PingObject(FileLoggerInstance);
                Selection.activeGameObject = FileLoggerInstance.gameObject;
            }

            EditorGUILayout.EndHorizontal();

            Repaint();
        }

    }

}