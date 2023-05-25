using System.Text;
using UnityEditor;
using UnityEngine;

namespace Wsh.View {

    [CustomEditor(typeof(ViewConfigDefine))]
    public class ViewConfigDefineEditor : UnityEditor.Editor {

        private ViewConfigDefine define;
        private SerializedProperty viewConfigListDefine;

        private void OnEnable() {
            viewConfigListDefine = serializedObject.FindProperty("ViewConfigListDefine");
            define = (ViewConfigDefine)target;
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(viewConfigListDefine, true);

            if(EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(define);
            }
        }
    }

}