using System;
using System.Collections;
using UnityEngine;
using Wsh.UIAnimation;

namespace Wsh.View {
    public class BaseView : MonoBehaviour {
        
        public const int SHOW_ANIMATION_GROUP = 100;
        public const int CLOSE_ANIMATION_GROUP = 200;

        public event Action OnBeforeAnimationShowEvent;
        public event Action OnAfterAnimationShowEvent;
        public event Action OnBeforeAnimationCloseEvent;
        public event Action OnAfterAnimationCloseEvent;

        public string ViewName { get { return m_viewName; } }

        public float OpenTime { get { return m_openTime; } }

        private string m_viewName;
        private float m_openTime;
        private ViewConfigContentClass m_viewDefine;
        protected ViewManager m_viewMgr;
        protected UIBaseAnimation[] m_uIAnimations;

        //debug
        private float m_lastImageMaskDebugAlpha;

        public void OnStart(ViewManager viewMgr, ViewConfigContentClass viewDefine) {
            m_viewMgr = viewMgr;
            m_viewName = viewDefine.viewName;
            m_viewDefine = viewDefine;
            m_uIAnimations = transform.gameObject.GetComponentsInChildren<UIBaseAnimation>();
            m_openTime = Time.realtimeSinceStartup;
        }

        protected Transform Find(string path) {
            var tf = transform.Find(path);
            if(tf != null) {
                return tf;
            }
            return null;
        }

        protected T TryGetComponent<T>(string path) where T : MonoBehaviour {
            var tf = Find(path);
            if(tf != null) {
                return tf.GetComponent<T>();
            }
            return null;
        }

        IEnumerator WaitFor(float time, Action<BaseView> onComplete) {
            yield return new WaitForSeconds(time);
            onComplete?.Invoke(this);
        }

        private void PlayAnimation(int group, Action<BaseView> onComplete=null) {
            float maxDuration = UIAnimationUtils.PlayAnimation(m_uIAnimations, group);
            if(maxDuration > 0) {
                StartCoroutine(WaitFor(maxDuration, onComplete));
            } else {
                onComplete?.Invoke(this);
            }
        }

        public virtual void OnInit(params object[] pm) {
            m_lastImageMaskDebugAlpha = m_viewMgr.GetImageMaskDebugAlpha();
            m_viewMgr.UpdateImageMaskDebug(m_viewDefine.debugMaskAlpha);
            m_viewMgr.LockInput();
            OnBeforeAnimationShowEvent?.Invoke();
            PlayAnimation(SHOW_ANIMATION_GROUP, view => {
                OnAfterAnimationShowEvent?.Invoke();
                OnAfterStartAnimation(view);
                m_viewMgr.UnlockInput();
            });
        }

        public virtual void OnClose(Action<BaseView> onComplete) {
            m_viewMgr.LockInput();
            m_viewMgr.UpdateImageMaskDebug(m_lastImageMaskDebugAlpha);
            OnBeforeAnimationCloseEvent?.Invoke();
            PlayAnimation(CLOSE_ANIMATION_GROUP, view => {
                m_viewMgr.UnlockInput();
                OnAfterAnimationCloseEvent?.Invoke();
                onComplete?.Invoke(view);
            });
        }

        public void Close() {
            m_viewMgr.CloseView(this);
        }

        public virtual void OnAfterStartAnimation(BaseView view) {

        }
    }
}