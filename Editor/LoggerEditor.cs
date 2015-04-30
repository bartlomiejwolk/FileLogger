﻿#define DEBUG_LOGGER

using ATP.ReorderableList;
using UnityEditor;
using UnityEngine;

namespace FileLogger {

    [CustomEditor(typeof(Logger))]
    public class LoggerEditor : Editor {

        private Logger Script { get; set; }

        #region SERIALIZED PROPERTIES
        private SerializedProperty filePath;
        private SerializedProperty logInRealTime;
        private SerializedProperty echoToConsole;
        private SerializedProperty loggingEnabled;
        private SerializedProperty enableOnPlay;
        private SerializedProperty qualifiedClassName;
        private SerializedProperty indentLine;
        private SerializedProperty classFilter;
        private SerializedProperty methodFilter;
        #endregion

        #region UNITY MESSAGES
        private void OnEnable() {
            Script = (Logger) target;

            filePath = serializedObject.FindProperty("filePath");
            logInRealTime = serializedObject.FindProperty("logInRealTime");
            echoToConsole = serializedObject.FindProperty("echoToConsole");
            loggingEnabled = serializedObject.FindProperty("loggingEnabled");
            enableOnPlay = serializedObject.FindProperty("enableOnPlay");
                serializedObject.FindProperty("appendCallerClassName");
            qualifiedClassName =
                serializedObject.FindProperty("qualifiedClassName");
            indentLine = serializedObject.FindProperty("indentLine");
            classFilter = serializedObject.FindProperty("classFilter");
            methodFilter = serializedObject.FindProperty("methodFilter");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawFilePathField();

            EditorGUILayout.Space();

            GUILayout.Label("Logging Options", EditorStyles.boldLabel);

            DrawEnableOnPlayToggle();
            DrawLogInRealTimeToggle();
            DrawEchoToConsoleToggle();

            EditorGUILayout.Space();

            GUILayout.Label("Message Options", EditorStyles.boldLabel);

            DrawIndentLineToggle();
            DrawFullyQualifiedNameToggle();
            DrawAppendDropdown();

            EditorGUILayout.Space();

            GUILayout.Label("Filters", EditorStyles.boldLabel);
            DrawEnabledMethodsDropdown();

            EditorGUILayout.Space();

            DrawMyClassHelpBox();
            ReorderableListGUI.Title("Class Filter");
            ReorderableListGUI.ListField(classFilter);

            DrawOnEnableHelpBox();
            ReorderableListGUI.Title("Method Filter");
            ReorderableListGUI.ListField(methodFilter);

            EditorGUILayout.BeginHorizontal();
            HandleDrawingStartStopButton();
            DrawClearLogFileButton();
            EditorGUILayout.EndHorizontal();

            // Save changes
            serializedObject.ApplyModifiedProperties();
        }
        #endregion
        #region INSPECTOR
        private void DrawClearLogFileButton() {
            // Don't allow reseting log file while logging.
            if (Script.LoggingEnabled) return;

            if (GUILayout.Button(
                "Clear Log File",
                GUILayout.Width(100))) {

                Script.ClearLogFile();
            }
        }

        private void HandleDrawingStartStopButton() {
            loggingEnabled.boolValue =
                InspectorControls.DrawStartStopButton(
                    Script.LoggingEnabled,
                    Script.EnableOnPlay,
                    FireOnStateChangedEvent,
                    () => Script.LogWriter.Add("[PAUSE]", true),
                    //() => Script.LogWriter.WriteAll(Script.FilePath, false));
                    () => Logger.StopLogging());
        }

        private void DrawOnEnableHelpBox() {
            EditorGUILayout.HelpBox(
                "Example: OnEnable",
                UnityEditor.MessageType.Info);
        }

        private void DrawMyClassHelpBox() {

            EditorGUILayout.HelpBox(
                "Example: MyClass",
                UnityEditor.MessageType.Info);
        }

        private void DrawAppendDropdown() {
            Script.AppendOptions = (AppendOptions) EditorGUILayout.EnumMaskField(
                new GUIContent(
                    "Display",
                    ""),
                Script.AppendOptions);
        }

        private void DrawEnabledMethodsDropdown() {
            Script.EnabledMethods = (EnabledMethods) EditorGUILayout.EnumMaskField(
                new GUIContent(
                    "Enabled Methods",
                    "Select Logger methods that should be active. Inactive " +
                    "methods won't product output."),
                Script.EnabledMethods);
        }

        private void DrawFullyQualifiedNameToggle() {
            EditorGUILayout.PropertyField(
                qualifiedClassName,
                new GUIContent(
                    "Full Class Name",
                    "If enabled, class name will be fully qualified."));
        }

        private void DrawIndentLineToggle() {

            EditorGUILayout.PropertyField(
                indentLine,
                new GUIContent(
                    "Indent On",
                    ""));
        }

        private void DrawEchoToConsoleToggle() {

            EditorGUILayout.PropertyField(
                echoToConsole,
                new GUIContent(
                    "Echo To Console",
                    "Echo logged messages also to the Unity's console. " +
                    "It can be really slow."));
        }

        private void DrawLogInRealTimeToggle() {

            EditorGUILayout.PropertyField(
                logInRealTime,
                new GUIContent(
                    "Log In Real Time",
                    "Each log message will be written to the file " +
                    "in real time instead of when logging stops."));
        }

        private void DrawEnableOnPlayToggle() {

            EditorGUILayout.PropertyField(
                enableOnPlay,
                new GUIContent(
                    "Enable On Play",
                    "Start logger when entering play mode."));
        }

        private void DrawFilePathField() {

            EditorGUILayout.PropertyField(
                filePath,
                new GUIContent(
                    "File Path",
                    "File path to save the generated log file."));
        }
        #endregion

        #region METHODS
        private void FireOnStateChangedEvent() {
            Utilities.InvokeMethodWithReflection(
                Script,
                "OnStateChanged",
                null);
        }

        [MenuItem("Component/FileLogger")]
        private static void AddLoggerComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(Logger));
            }
        }

        #endregion

    }
}
