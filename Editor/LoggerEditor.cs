using UnityEngine;
using System;
using System.Collections;
using UnityEditor;
using ATP.ReorderableList;
using SyntaxTree.VisualStudio.Unity.Messaging;

namespace ATP.LoggingTools {

    [CustomEditor(typeof(Logger))]
    public class LoggerEditor : GameComponentEditor {

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

        public override void OnEnable() {
            base.OnEnable();

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
            base.OnInspectorGUI();
            Logger script = (Logger)target;
            serializedObject.Update();

            // TODO Set longer label width.

            EditorGUILayout.PropertyField(
                    filePath,
                    new GUIContent(
                        "File Path",
                        "File path to save the generated log file."));

            EditorGUILayout.Space();

            //EditorGUILayout.PropertyField(
            //        initArraySize,
            //        new GUIContent(
            //            "Array Size",
            //            "Initial and expand size of the log cache array.")); 
            EditorGUILayout.PropertyField(
                    enableOnPlay,
                    new GUIContent(
                        "Enable On Play",
                        "Start logger when entering play mode."));
            //EditorGUILayout.PropertyField(
            //        inGameLabel,
            //        new GUIContent(
            //            "In-game Label",
            //            "Display in-game label with current number of " +
            //            "cached logs."));
            EditorGUILayout.PropertyField(
                    logInRealTime,
                    new GUIContent(
                        "Log In Real Time",
                        "Each log message will be written to the file " +
                        "in real time instead of when logging stops."));

            EditorGUILayout.PropertyField(
                    echoToConsole,
                    new GUIContent(
                        "Echo To Console",
                        "Echo logged messages also to the Unity's console. " +
                        "It can be really slow."));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(
                    enableLogCall,
                    new GUIContent(
                        "Enable LogCall()",
                        ""));
            EditorGUILayout.PropertyField(
                    enableLogResult,
                    new GUIContent(
                        "Enable LogResult()",
                        ""));
            EditorGUILayout.PropertyField(
                    enableLogString,
                    new GUIContent(
                        "Enable LogString()",
                        ""));

            EditorGUILayout.Space();


            EditorGUILayout.PropertyField(
                    indentMessage,
                    new GUIContent(
                        "Indent On",
                        ""));
            EditorGUILayout.PropertyField(
                    showTimestamp,
                    new GUIContent(
                        "Add Timestamp",
                        ""));
            EditorGUILayout.PropertyField(
                    appendClassName,
                    new GUIContent(
                        "Append Class Name",
                        "Append class name to every log message."));
            EditorGUILayout.PropertyField(
                    appendCallerClassName,
                    new GUIContent(
                        "Append Caller Name",
                        "Append caller class name to every log message."));
            EditorGUILayout.PropertyField(
                    fullyQualifiedClassName,
                    new GUIContent(
                        "Qualified Class Name",
                        "If enabled, class name will be fully qualified."));

            script.AppendOptions = (AppendOptions) EditorGUILayout.EnumMaskField(
                new GUIContent(
                    "Append",
                    ""),
                script.AppendOptions);

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                    "Example: MyClass",
                    UnityEditor.MessageType.Info);
            ReorderableListGUI.Title("Class Filter");
            ReorderableListGUI.ListField(classFilter);
            EditorGUILayout.HelpBox(
                    "Example: OnEnable",
                    UnityEditor.MessageType.Info);
            ReorderableListGUI.Title("Method Filter");
            ReorderableListGUI.ListField(methodFilter);

            // todo add button to continue logging after pause
            if (loggingEnabled.boolValue == false) {
                if (GUILayout.Button("Start Logging")) {
                    loggingEnabled.boolValue = true;

                    // Fire event.
                    Utilities.InvokeMethodWithReflection(
                        script,
                        "OnStateChanged",
                        null);
                }
            }
            else if (Application.isPlaying && enableOnPlay.boolValue) {
                if (GUILayout.Button("Pause Logging")) {
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
                if (GUILayout.Button("Stop Logging")) {
                    loggingEnabled.boolValue = false;
                    script.LogCache.WriteAll(script.FilePath, false);

                    // Fire event.
                    Utilities.InvokeMethodWithReflection(
                        script,
                        "OnStateChanged",
                        null);
                }
            }

            // Save changes
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed) {
                EditorUtility.SetDirty(script);
            }
        }
    }
}
