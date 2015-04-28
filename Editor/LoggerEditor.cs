using UnityEngine;
using System;
using System.Collections;
using UnityEditor;
using ATP.ReorderableList;
using SyntaxTree.VisualStudio.Unity.Messaging;

namespace mlogger {

    [CustomEditor(typeof(Logger))]
    public class LoggerEditor : Editor {

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
        private SerializedProperty fullyQualifiedClassName;
        private SerializedProperty showTimestamp;
        private SerializedProperty indentMessage;
        private SerializedProperty classFilter;
        private SerializedProperty methodFilter;
        #endregion

        #region UNITY MESSAGES
        private void OnEnable() {
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
            fullyQualifiedClassName =
                serializedObject.FindProperty("fullyQualifiedClassName");
            showTimestamp = serializedObject.FindProperty("showTimestamp");
            indentMessage = serializedObject.FindProperty("indentMessage");
            classFilter = serializedObject.FindProperty("classFilter");
            methodFilter = serializedObject.FindProperty("methodFilter");
        }

        public override void OnInspectorGUI() {
            // todo create Script property
            Logger script = (Logger)target;

            serializedObject.Update();

            // TODO Set longer label width.

            DrawFilePathField();

            EditorGUILayout.Space();

            DrawEnableOnPlayToggle();
            DrawLogInRealTimeToggle();
            DrawEchoToConsoleToggle();

            EditorGUILayout.Space();

            DrawIndentLineToggle();
            DrawFullyQualifiedNameToggle();

            EditorGUILayout.Space();

            // todo remove param.
            DrawEnabledMethodsDropdown(script);
            // todo remove param.
            DrawAppendDropdown(script);

            EditorGUILayout.Space();

            DrawMyClassHelpBox();
            ReorderableListGUI.Title("Class Filter");
            ReorderableListGUI.ListField(classFilter);

            DrawOnEnableHelpBox();
            ReorderableListGUI.Title("Method Filter");
            ReorderableListGUI.ListField(methodFilter);

            // todo remove param.
            DrawStartStopButton(script);

            // Save changes
            serializedObject.ApplyModifiedProperties();
        }
        #endregion
        #region INSPECTOR

        private void DrawStartStopButton(Logger script) {
            // todo add button to continue logging after pause
            // todo extract what's inside to DrawDrawStartStopButton()
            if (loggingEnabled.boolValue == false) {
                loggingEnabled.boolValue = GUILayout.Toggle(
                    loggingEnabled.boolValue,
                    "Start Logging",
                    "Button");

                // If value was changed..
                if (loggingEnabled.boolValue != script.LoggingEnabled) {
                    // Fire event.
                    Utilities.InvokeMethodWithReflection(
                        script,
                        "OnStateChanged",
                        null);
                }
            }
            else if (Application.isPlaying && enableOnPlay.boolValue) {
                loggingEnabled.boolValue = GUILayout.Toggle(
                    loggingEnabled.boolValue,
                    "Pause Logging",
                    "Button");

                // If value was changed..
                if (loggingEnabled.boolValue != script.LoggingEnabled) {
                    loggingEnabled.boolValue = false;
                    script.LogCache.Add("[PAUSE]", true);

                    // Fire event.
                    Utilities.InvokeMethodWithReflection(
                        script,
                        "OnStateChanged",
                        null);
                }
            }
            else {
                loggingEnabled.boolValue = GUILayout.Toggle(
                    loggingEnabled.boolValue,
                    "Stop Logging",
                    "Button");

                // If value was changed..
                if (loggingEnabled.boolValue != script.LoggingEnabled) {
                    loggingEnabled.boolValue = false;
                    script.LogCache.WriteAll(script.FilePath, false);

                    // Fire event.
                    Utilities.InvokeMethodWithReflection(
                        script,
                        "OnStateChanged",
                        null);
                }
            }
        }

        private static void DrawOnEnableHelpBox() {

            EditorGUILayout.HelpBox(
                "Example: OnEnable",
                UnityEditor.MessageType.Info);
        }

        private static void DrawMyClassHelpBox() {

            EditorGUILayout.HelpBox(
                "Example: MyClass",
                UnityEditor.MessageType.Info);
        }

        private static void DrawAppendDropdown(Logger script) {

            script.AppendOptions = (AppendOptions) EditorGUILayout.EnumMaskField(
                new GUIContent(
                    "Append",
                    ""),
                script.AppendOptions);
        }

        private static void DrawEnabledMethodsDropdown(Logger script) {

            script.EnabledMethods = (EnabledMethods) EditorGUILayout.EnumMaskField(
                new GUIContent(
                    "Enabled Methods",
                    "Select Logger methods that should be active. Inactive " +
                    "methods won't product output."),
                script.EnabledMethods);
        }

        private void DrawFullyQualifiedNameToggle() {

            EditorGUILayout.PropertyField(
                fullyQualifiedClassName,
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

    }
}
