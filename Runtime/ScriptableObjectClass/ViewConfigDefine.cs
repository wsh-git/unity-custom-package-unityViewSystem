using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wsh.View {

    [Serializable]
    public class ViewConfigContentClass {
        public string viewName;
        public string className; //"Wsh.View.ViewGameStartPage"
        public string prefabPath;
        public bool isLoading;
        public float debugMaskAlpha;
        public bool canCloseByEsc;

        public Type GetClassType() {
            //return Type.GetType(className);
            return System.Reflection.Assembly.Load("Assembly-CSharp").GetType(className);
        }

    }

    [CreateAssetMenu(fileName = "ViewConfigDefine", menuName = "Custom/ScriptableObject/ViewConfigDefine")]
    public class ViewConfigDefine : ScriptableObject {
        public List<ViewConfigContentClass> ViewConfigListDefine = new List<ViewConfigContentClass>();
    }

}