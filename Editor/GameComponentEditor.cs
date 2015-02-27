using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;

namespace ATP.LoggingTools {

    [CustomEditor(typeof (GameComponent))]
    public class GameComponentEditor : Editor {

        public virtual void OnEnable() {
        }

        public virtual void OnDisable() {
        }

        public virtual void OnDestroy() {
        }

        public override void OnInspectorGUI() {
        }

        public virtual void OnSceneGUI() {
        }

    }

}
