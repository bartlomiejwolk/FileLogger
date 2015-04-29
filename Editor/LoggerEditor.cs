using UnityEngine;
using System;
using System.Collections;
using UnityEditor;
using ATP.ReorderableList;
using SyntaxTree.VisualStudio.Unity.Messaging;

namespace mLogger {

    [CustomEditor(typeof(Logger))]
    public class LoggerEditor : Editor {

        private Logger Script { get; set; }

        #region SERIALIZED PROPERTIES
        private SerializedProperty filePath;
        private SerializedProperty initArraySize;
        private SerializedProperty inGameLabel;
        private SerializedProperty logInRealTime;
        private SerializedProperty echoToConsole;
        private SerializedProperty enableLogCall;
        private SerializedProperty enableLogResult;
        private SerializedProperty enableLogString;
        private SerializedProperty loggingEnabled;
        private SerializedProperty enableOnPlay;
        private SerializedProperty appendClassName;
        private SerializedProperty appendCallerClassName;
        private SerializedProperty qualifiedClassName;
        private SerializedProperty showTimestamp;
        private SerializedProperty indentMessage;
        private SerializedProperty classFilter;
        private SerializedProperty methodFilter;
        #endregion

        #region UNITY MESSAGES
        private void OnEnable() {
            Script = (Logger) target;

            filePath = serializedObject.FindProperty("filePath");
            initArraySize = serializedObject.FindProperty("initArraySize");
            inGameLabel = serializedObject.FindProperty("inGameLabel");
            logInRealTime = serializedObject.FindProperty("logInRealTime");
            echoToConsole = serializedObject.FindProperty("echoToConsole");
            enableLogCall = serializedObject.FindProperty("enableLogCall");
            enableLogResult = serializedObject.FindProperty("enableLogResult");
            enableLogString = serializedObject.FindProperty("enableLogString");
            loggingEnabled = serializedObject.FindProperty("loggingEnabled");
            enableOnPlay = serializedObject.FindProperty("enableOnPlay");
            appendClassName = serializedObject.FindProperty("appendClassName");
            appendCallerClassName =
                serializedObject.FindProperty("appendCallerClassName");
            qualifiedClassName =
                serializedObject.FindProperty("qualifiedClassName");
            showTimestamp = serializedObject.FindProperty("showTimestamp");
            indentMessage = serializedObject.FindProperty("indentMessage");
            classFilter = serializedObject.FindProperty("classFilter");
            methodFilter = serializedObject.FindProperty("methodFilter");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawFilePathField();

            EditorGUILayout.Space();

            DrawEnableOnPlayToggle();
            DrawLogInRealTimeToggle();
            DrawEchoToConsoleToggle();

            EditorGUILayout.Space();

            DrawIndentLineToggle();
            DrawFullyQualifiedNameToggle();

            EditorGUILayout.Space();

            DrawEnabledMethodsDropdown();
            DrawAppendDropdown();

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
                    loggingEnabled.boolValue,
                    enableOnPlay.boolValue,
                    FireOnStateChangedEvent,
                    () => Script.LogWriter.Add("[PAUSE]", true),
                    () => Script.LogWriter.WriteAll(Script.FilePath, false));
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
                    "Append",
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
                    "Qualified Class Name",
                    "If enabled, class name will be fully qualified."));
        }

        private void DrawIndentLineToggle() {

            EditorGUILayout.PropertyField(
                // todo rename to indentLine
                indentMessage,
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
        #endregion

    }
}
