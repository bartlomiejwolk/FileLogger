using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;

namespace ATP.Logger {

    [CustomEditor(typeof (GameComponent))]
    public class GameComponentEditor : Editor {

        /* Serialized properties */
        //protected SerializedProperty _name;
        protected SerializedProperty _description;

        public virtual void OnEnable() {
            //_name = serializedObject.FindProperty("_name");
            _description = serializedObject.FindProperty("_description");
        }

        public virtual void OnDisable() {
        }

        public virtual void OnDestroy() {
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            // Display component name text field.
            //_name.stringValue = EditorGUILayout.TextField("Name", _name.stringValue);
            // Display label field.
            EditorGUILayout.LabelField("Description");
            // Display component description text field.
            _description.stringValue = EditorGUILayout.TextArea(
                _description.stringValue,
                GUILayout.MaxWidth(243));
            serializedObject.ApplyModifiedProperties();
        }

        public virtual void OnSceneGUI() {
        }

    }

}
