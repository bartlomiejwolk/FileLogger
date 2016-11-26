// Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com)
//  
// This file is part of the FileLogger extension for Unity.
// Licensed under the MIT license. See LICENSE file in the project root folder.

#define FILELOGGER

using FileLoggerTool.Enums;
using FileLoggerTool.Include.ReorderableList.Editor;
using UnityEditor;
using UnityEngine;

namespace FileLoggerTool.Editor {

    // todo rename to FileLoggerEditor
    [CustomEditor(typeof (FileLogger))]
    public class LoggerEditor : UnityEditor.Editor {

        private FileLogger Script { get; set; }

        #region SERIALIZED PROPERTIES

        private SerializedProperty _classFilter;
        private SerializedProperty _echoToConsole;
        private SerializedProperty _logUnityMessages;
        private SerializedProperty _enableOnPlay;
        private SerializedProperty _fileName;
        private SerializedProperty _indentLine;
        private SerializedProperty _loggingEnabled;
        private SerializedProperty _writeToFile;
        private SerializedProperty _logInRealTime;
        private SerializedProperty _methodFilter;
        private SerializedProperty _qualifiedClassName;
        private SerializedProperty _append;
        private SerializedProperty _clearOnPlay;
        private SerializedProperty _classFilterType;
        private SerializedProperty _methodFilterType;

        #endregion SERIALIZED PROPERTIES

        #region UNITY MESSAGES

        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawVersionNo();
            DrawFilePathField();

            EditorGUILayout.Space();

            GUILayout.Label("Logging Options", EditorStyles.boldLabel);

            DrawEnableOnPlayToggle();
            DrawWriteToFileToggle();
            DrawClearOnPlayToggle();
            DrawWriteInRealTimeToggle();
            DrawAppendToggle();
            DrawEchoToConsoleToggle();
            DrawLogUnityMessagesToggle();

            EditorGUILayout.Space();

            GUILayout.Label("Message Options", EditorStyles.boldLabel);

            DrawIndentLineToggle();
            DrawFullyQualifiedNameToggle();
            DrawAppendDropdown();

            EditorGUILayout.Space();
            
            GUILayout.Label("Filter Settings", EditorStyles.boldLabel);
            DrawEnabledMethodsDropdown();
            DrawEnabledMessageCategoriesDropdown();

            EditorGUILayout.Space();

            GUILayout.Label("Class Filter", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_classFilterType);
            DrawClassFilterHelpBox();
            ReorderableListGUI.Title("Class Filter");
            ReorderableListGUI.ListField(_classFilter);

            GUILayout.Label("Method Filter", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_methodFilterType);
            DrawMethodFilterHelpBox();
            ReorderableListGUI.Title("Method Filter");
            ReorderableListGUI.ListField(_methodFilter);

            EditorGUILayout.BeginHorizontal();
            HandleDrawingStartStopButton();
            DrawClearLogFileButton();
            EditorGUILayout.EndHorizontal();

            // save changes
            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable() {
            Script = (FileLogger) target;

            _fileName = serializedObject.FindProperty("_fileName");
            _writeToFile = serializedObject.FindProperty("_writeToFile");
            _logInRealTime = serializedObject.FindProperty("_writeInRealTime");
            _echoToConsole = serializedObject.FindProperty("_echoToConsole");
            _logUnityMessages = serializedObject.FindProperty("_logUnityMessages");
            _loggingEnabled = serializedObject.FindProperty("_loggingEnabled");
            _enableOnPlay = serializedObject.FindProperty("_enableOnPlay");
            serializedObject.FindProperty("_appendCallerClassName");
            _qualifiedClassName =
                serializedObject.FindProperty("_qualifiedClassName");
            _indentLine = serializedObject.FindProperty("_indentLine");
            _classFilter = serializedObject.FindProperty("_classFilter");
            _methodFilter = serializedObject.FindProperty("_methodFilter");
            _append = serializedObject.FindProperty("_append");
            _clearOnPlay = serializedObject.FindProperty("_clearOnPlay");
            _classFilterType = serializedObject.FindProperty("_classFilterType");
            _methodFilterType = serializedObject.FindProperty("_methodFilterType");
        }

        #endregion UNITY MESSAGES

        #region INSPECTOR
        private void HandleDrawingStartStopButton() {
            _loggingEnabled.boolValue =
                InspectorControls.DrawStartStopButton(
                    Script.LoggingEnabled,
                    Script.EnableOnPlay,
                    null);
        }

        private void DrawClearOnPlayToggle() {
            EditorGUI.BeginDisabledGroup(!_writeToFile.boolValue);

            _clearOnPlay.boolValue = EditorGUILayout.Toggle(
            new GUIContent(
                "Clear On Play",
                "Clear log file on enter play mode."),
            _clearOnPlay.boolValue);

            EditorGUI.EndDisabledGroup();
        }


        private void DrawAppendDropdown() {
            Script.DisplayOptions =
                (AppendOptions) EditorGUILayout.EnumMaskField(
                    new GUIContent(
                        "Display",
                        "Additional info that should be attached to a single" +
                        "log message."),
                    Script.DisplayOptions);
        }

        private void DrawAppendToggle() {
            EditorGUI.BeginDisabledGroup(_logInRealTime.boolValue);

            _append.boolValue = EditorGUILayout.Toggle(
                new GUIContent(
                    "Always Append",
                    "Always append messages to the log file."),
                _append.boolValue);

            EditorGUI.EndDisabledGroup();
        }

        private void DrawClearLogFileButton() {
            // Don't allow reseting log file while logging.
            if (Script.LoggingEnabled) return;

            if (GUILayout.Button(
                "Clear Log File",
                GUILayout.Width(100))) {

                Script.ClearLogFile();
            }
        }

        private void DrawEchoToConsoleToggle() {
            EditorGUILayout.PropertyField(
                _echoToConsole,
                new GUIContent(
                    "Echo To Console",
                    "Echo logged messages also to the Unity's console. " +
                    "It can be really slow."));
        }

        private void DrawLogUnityMessagesToggle()
        {
            // any message logged by FileLogger to the console would then
            // be received by Application.logMessagesReceived and logged again and again..
            if (_echoToConsole.boolValue)
            {
                _logUnityMessages.boolValue = false;
            }

            EditorGUI.BeginDisabledGroup(_echoToConsole.boolValue);

            EditorGUILayout.PropertyField(
                _logUnityMessages,
                new GUIContent(
                    "Log Unity Messages",
                    "Log Unity log messages. If true, Echo to Console option will be disabled. "));

            EditorGUI.EndDisabledGroup();
        }

        private void DrawEnabledMethodsDropdown() {
            Script.EnabledMethods =
                (EnabledMethods) EditorGUILayout.EnumMaskField(
                    new GUIContent(
                        "Enabled Methods",
                        "Select Logger methods that should be active. Inactive "
                        +
                        "methods won't log anything."),
                    Script.EnabledMethods);
        }

        private void DrawEnabledMessageCategoriesDropdown()
        {
            var desc = new GUIContent("Message Categories", "Only messages that belong to selected categories will be logged.");
            Script.EnabledMessageCategories = (MessageCategories) EditorGUILayout.EnumMaskField(
                desc, Script.EnabledMessageCategories);
        }

        private void DrawEnableOnPlayToggle() {
            EditorGUILayout.PropertyField(
                _enableOnPlay,
                new GUIContent(
                    "Enable On Play",
                    "Start logger on enter play mode."));
        }

        private void DrawFilePathField() {

            EditorGUILayout.PropertyField(
                _fileName,
                new GUIContent(
                    "File Name",
                    "File name for the generated log file."));
        }

        private void DrawFullyQualifiedNameToggle() {
            EditorGUILayout.PropertyField(
                _qualifiedClassName,
                new GUIContent(
                    "Full Class Name",
                    "If enabled, class name will be fully qualified."));
        }

        private void DrawIndentLineToggle() {

            EditorGUILayout.PropertyField(
                _indentLine,
                new GUIContent(
                    "Indent On",
                    "Indent log messages accordingly to the call stack."));
        }

        private void DrawWriteToFileToggle()
        {
            EditorGUILayout.PropertyField(
                _writeToFile,
                new GUIContent(
                    "Write To File",
                    "If disabled, logger won't do any operations on a file."));
        }

        private void DrawWriteInRealTimeToggle() {
            EditorGUI.BeginDisabledGroup(!_writeToFile.boolValue);

            EditorGUILayout.PropertyField(
                    _logInRealTime,
                    new GUIContent(
                        "Write In Real Time",
                        "Each log message will be written to the file " +
                        "in real time instead of when logging stops."));

            EditorGUI.EndDisabledGroup();
        }

        private void DrawClassFilterHelpBox() {

            EditorGUILayout.HelpBox(
                "Example: MyClass",
                UnityEditor.MessageType.Info);
        }

        private void DrawMethodFilterHelpBox() {
            EditorGUILayout.HelpBox(
                "Example: OnEnable",
                UnityEditor.MessageType.Info);
        }
        private void DrawVersionNo() {
            EditorGUILayout.LabelField(FileLogger.Version);
        }

        #endregion INSPECTOR

        #region METHODS
        [MenuItem("Component/FileLogger")]
        private static void AddLoggerComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof (FileLogger));
            }
        }

        #endregion METHODS
    }

}