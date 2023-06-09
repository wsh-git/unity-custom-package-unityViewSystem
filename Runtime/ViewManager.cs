using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField]private Text m_textGreenMsg;
        [SerializeField]private Text m_textRedMsg;
        [SerializeField]private Image m_imageMaskRaycast;
        [SerializeField]private Image m_imageMaskDebug;
        
        private List<BaseView> m_viewList;
        private int m_inputLockNumber;
        private Dictionary<Type, ViewConfigContentClass> m_viewConfigDic;
        private bool m_isDarkMode;

        private Coroutine m_greenMsgCoroutine;
        private Coroutine m_redMsgCoroutine;

        public static void InitAsync(string uiRootPrefabPath, ViewConfigDefine viewConfigDefine, Action<string, GameObject, Action<GameObject>> instantiateAsyncFunc, Func<GameObject, GameObject, GameObject> instantiateFunc, Action<ViewManager> onComplete) {
            instantiateAsyncFunc(uiRootPrefabPath, null, root => {
                root.transform.position = new Vector3(5000, 0, 0);
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
            InstantiateAsyncFunc(viewDefine.prefabPath, m_viewRoot, go => {
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
            var go = InstantiateFunc(prefab, m_viewRoot);
            T v = go.AddComponent<T>() as T;
            v.OnStart(this, viewDefine);
            v.OnInit(pm);
            m_viewList.Add(v);
            return v;
        }

        public void GreenMessage(string msg) {
            if(m_greenMsgCoroutine != null) {
                StopCoroutine(m_greenMsgCoroutine);
            }
            m_greenMsgCoroutine = StartCoroutine(IEShowGreenMessage(msg));
        }

        IEnumerator IEShowGreenMessage(string msg) {
            m_textGreenMsg.text = msg;
            m_textGreenMsg.gameObject.SetActive(true);
            yield return new WaitForSeconds(1);
            m_textGreenMsg.gameObject.SetActive(false);
        }

        public void RedMessage(string msg) {
            if(m_redMsgCoroutine != null) {
                StopCoroutine(m_redMsgCoroutine);
            }
            m_redMsgCoroutine = StartCoroutine(IEShowRedMessage(msg));
        }

        IEnumerator IEShowRedMessage(string msg) {
            m_textRedMsg.text = msg;
            m_textRedMsg.gameObject.SetActive(true);
            yield return new WaitForSeconds(1);
            m_textRedMsg.gameObject.SetActive(false);
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