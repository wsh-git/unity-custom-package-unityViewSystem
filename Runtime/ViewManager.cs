using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wsh.UIAnimation;

namespace Wsh.View {

    public class ViewManager : MonoBehaviour {

        public RectTransform CanvasRectTransform { get { return m_canvasRectTransform; } }
        public Camera UICamera { get{ return m_camera; } }

        private Action<string, GameObject, Action<GameObject>> InstantiateAsyncFunc;
        private Func<GameObject, GameObject, GameObject> InstantiateFunc;

        [SerializeField]private Camera m_camera;
        [SerializeField]private RectTransform m_canvasRectTransform;
        [SerializeField]private GameObject m_viewRoot;
        [SerializeField]private GameObject m_msgRoot;
        [SerializeField]private GameObject m_loadingRoot;
        [SerializeField]private UIAnimationGroupPlayer m_greenMsgAnimationPlayer;
        [SerializeField]private UIAnimationGroupPlayer m_redMsgAnimationPlayer;
        [SerializeField]private Text m_textGreenMsg;
        [SerializeField]private Text m_textRedMsg;
        [SerializeField]private Image m_imageMaskRaycast;
        [SerializeField]private Image m_imageMaskDebug;
        
        private List<BaseView> m_viewList;
        private int m_inputLockNumber;
        private Dictionary<Type, ViewConfigContentClass> m_viewConfigDic;
        private bool m_isDarkMode;

        public static void InitAsync(string uiRootPrefabPath, Vector3 rootPosition, ViewConfigDefine viewConfigDefine, Action<string, GameObject, Action<GameObject>> instantiateAsyncFunc, Func<GameObject, GameObject, GameObject> instantiateFunc, Action<ViewManager> onComplete) {
            instantiateAsyncFunc(uiRootPrefabPath, null, root => {
                root.transform.position = rootPosition;
                ViewManager viewManger = root.GetComponent<ViewManager>();
                viewManger.Init(viewConfigDefine, instantiateAsyncFunc, instantiateFunc);
                onComplete?.Invoke(viewManger);
            });
        }

        private void Init(ViewConfigDefine viewConfigDefine, Action<string, GameObject, Action<GameObject>> instantiateAsyncFunc, Func<GameObject, GameObject, GameObject> instantiateFunc) {
            DontDestroyOnLoad(gameObject);
            InstantiateAsyncFunc = instantiateAsyncFunc;
            InstantiateFunc = instantiateFunc;
            m_inputLockNumber = 0;
            m_viewList = new List<BaseView>();
            InitViewConfigs(viewConfigDefine);
        }
        
        private void InitViewConfigs(ViewConfigDefine viewConfigDefine) {
            m_viewConfigDic = new Dictionary<Type, ViewConfigContentClass>();
            for(int i = 0; i < viewConfigDefine.ViewConfigListDefine.Count; i++) {
                var configData = viewConfigDefine.ViewConfigListDefine[i];
                var classType = configData.GetClassType();
                if(classType != null && !m_viewConfigDic.ContainsKey(classType)) {
                    m_viewConfigDic.Add(classType, configData);
                }   
            }
        }

        private ViewConfigContentClass GetViewDefine(Type type) {
            return m_viewConfigDic[type];
        }

        public List<T> GetViews<T>() where T : BaseView {
            ViewConfigContentClass viewDefine = GetViewDefine(typeof(T));
            var list = m_viewList.FindAll(v => v.ViewName == viewDefine.viewName);
            if(list != null && list.Count > 0) {
                List<T> ls = new List<T>();
                for(int i = 0; i < list.Count; i++) {
                    ls.Add(list[i] as T);
                }
                return ls;
            }
            return null;
        }

        public T GetView<T>() where T : BaseView {
            var list = GetViews<T>();
            if(list != null && list.Count > 0) {
                if(list.Count > 1) {
                    list.Sort((v1, v2) => {
                        if(v1.OpenTime > v2.OpenTime) {
                            return 1;
                        } else {
                            return -1;
                        }
                    });
                }
                return list[0];
            }
            return null;
        }

        public BaseView GetLatestView() {
            if(m_viewList.Count > 0) {
                return m_viewList[m_viewList.Count-1];
            }
            return null;
        }

        private void RemoveView(BaseView view) {
            int removeIndex = -1;
            for(int i = 0; i < m_viewList.Count; i++) {
                if(m_viewList[i] == view) {
                    removeIndex = i;
                }
            }
            if(removeIndex != -1) { m_viewList.RemoveAt(removeIndex); }
        }

        private void DestroyView(BaseView view) {
            RemoveView(view);
            Destroy(view.gameObject);
        }

        public void CloseView(BaseView view) {
            view.OnClose(DestroyView);
        }

        public void ShowViewAsync<T>(Action<T> onComplete, params object[] pm) where T: BaseView {
            Type type = typeof(T);
            ViewConfigContentClass viewDefine = GetViewDefine(type);
            var root = viewDefine.isLoading ? m_loadingRoot : m_viewRoot;
            InstantiateAsyncFunc(viewDefine.prefabPath, root, go => {
                T v = go.AddComponent<T>() as T;
                v.OnStart(this, viewDefine);
                v.OnInit(pm);
                m_viewList.Add(v);
                onComplete?.Invoke(v);
            });
        }

        public T ShowView<T>(GameObject prefab, params object[] pm) where T: BaseView {
            Type type = typeof(T);
            ViewConfigContentClass viewDefine = GetViewDefine(type);
            var root = viewDefine.isLoading ? m_loadingRoot : m_viewRoot;
            var go = InstantiateFunc(prefab, root);
            T v = go.AddComponent<T>() as T;
            v.OnStart(this, viewDefine);
            v.OnInit(pm);
            m_viewList.Add(v);
            return v;
        }

        private void SetMessage(UIAnimationGroupPlayer animationPlayer, Text textMsg, string msg) {
            textMsg.text = msg;
            animationPlayer.Play();
        }

        public void GreenMessage(string msg) {
            SetMessage(m_greenMsgAnimationPlayer, m_textGreenMsg, msg);
        }
        
        public void RedMessage(string msg) {
            SetMessage(m_redMsgAnimationPlayer, m_textRedMsg, msg);
        }

        public void SetDarkMode(bool isDarkMode) {
            m_isDarkMode = isDarkMode;
        }

        public void UpdateImageMaskDebug(float alpha) {
            if(m_isDarkMode) {
                ViewUtils.SetImageAlpha(m_imageMaskDebug, alpha);
            }
        }

        public float GetImageMaskDebugAlpha() {
            return m_imageMaskDebug.color.a;
        }

        private void SetEventSystemLockState(bool islock) {
            m_imageMaskRaycast.raycastTarget = islock;
        }

        public void UnlockInput() {
            m_inputLockNumber--;
            if(m_inputLockNumber < 0) UnityEngine.Debug.LogError("Unlock input number error");
            if(m_inputLockNumber == 0) {
                SetEventSystemLockState(false);
            }
        }

        public void LockInput() {
            if(m_inputLockNumber == 0) {
                SetEventSystemLockState(true);
            }
            m_inputLockNumber++;
        }

        public bool IsLockInput() {
            return m_inputLockNumber > 0;
        }
    }
}