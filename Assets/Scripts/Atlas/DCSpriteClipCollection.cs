using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class DCSpriteClipCollection : ScriptableObject, ISerializationCallbackReceiver
{
    [Serializable]
    public class Clip
    {
        public string m_name;
        public List<DCSpriteClipAction> m_actions;
    }
    public DCAtlasInstance m_atlas;
    public List<Clip> m_clips = new List<Clip>();
    public List<AudioGroup> m_audioGroups = new List<AudioGroup>();

    [SerializeField]
    [HideInInspector]
    private string m_data;

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        if ((m_clips == null || m_clips.Count == 0) && !string.IsNullOrEmpty(m_data))
        {
            m_clips = JsonConvert.DeserializeObject<List<Clip>>(m_data);
            foreach (var v in m_clips)
            {
                foreach (var a in v.m_actions)
                {
                    if (a.m_actionType == DCSpriteClipAction.ActionType.PlayAudio)
                    {
                        if(a.m_audioGroupId >= m_audioGroups.Count)
                        {
                            Debug.LogError("Fail");
                        }
                        else
                        {
                            a.m_audioGroup = m_audioGroups[a.m_audioGroupId];
                        }
                    }
                }
            }
        }
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        m_audioGroups.Clear();
        foreach (var v in m_clips)
        {
            foreach(var a in v.m_actions)
            {
                if(a.m_actionType == DCSpriteClipAction.ActionType.PlayAudio)
                {
                    var id = m_audioGroups.IndexOf(a.m_audioGroup);
                    if(id == -1)
                    {
                        id = m_audioGroups.Count;
                        m_audioGroups.Add(a.m_audioGroup);
                    }
                    a.m_audioGroupId = id;
                }
            }
        }
        m_data = JsonConvert.SerializeObject(m_clips);
    }
}

[Serializable]
public class DCSpriteClipAction
{
    [Serializable]
    public enum ActionType
    {
        PlayClip,
        PlayClipInRange,
        SendFSMEvent,
        SendMessage,
        JumpToClip,
        ActiveChild,
        PlayAudio
    }

    public ActionType m_actionType;
    public string m_clipName;
    public bool m_loop;
    public float m_time;
    public int m_startFrame;
    public int m_stopFrame;
    public string m_eventName;
    public bool m_resetOnExit;
    [JsonIgnore]
    public AudioGroup m_audioGroup;
    public int m_audioGroupId;
    public void DoAction(DCSpriteClipCollection collection, DCSpriteClipCollection.Clip clip,
                    int actionIndex, DCSpriteAnimator animator,
                    DCSpriteRenderer renderer, DCSpriteAnimatorHost host)
    {
        DCSpriteClipAction nextAction;
        if (actionIndex >= clip.m_actions.Count - 1)
        {
            nextAction = null;
        }
        else
        {
            nextAction = clip.m_actions[actionIndex + 1];
        }
        
        IEnumerator WaitAction(object ie, Action action)
        {
            yield return ie;
            action();
        }

        void InvokeNext()
        {
            if (nextAction != null && host.CurrentClipName == clip.m_name)
            {
                //Debug.Log("Next Action");
                nextAction.DoAction(collection, clip, actionIndex + 1, animator, renderer, host);
            }
        }

        if(m_actionType == ActionType.PlayClip)
        {
            if (!string.IsNullOrEmpty(m_clipName))
            {
                animator.Play(m_clipName, m_loop, m_startFrame);
            }
            
            if(m_loop && m_time > 0)
            {
                animator.StartCoroutine(WaitAction(new WaitForSeconds(m_time), InvokeNext));
            }
            animator.StartCoroutine(WaitAction(animator.WaitForClip(m_clipName), InvokeNext));
        }
        else if(m_actionType == ActionType.PlayClipInRange)
        {
            if(!string.IsNullOrEmpty(m_clipName))
            {
                animator.Play(m_clipName, false, m_startFrame);
            }
            
            animator.StartCoroutine(WaitAction(animator.WaitForFrame(m_stopFrame, m_clipName), InvokeNext));
        }
        else if(m_actionType == ActionType.SendMessage)
        {
            animator.gameObject.SendMessage(m_eventName, m_clipName);
            InvokeNext();
        }
        else if(m_actionType == ActionType.SendFSMEvent)
        {
            //FSMUtility.SendEventToGameObject(animator.gameObject, m_eventName);
            Debug.LogError("Not Support!!!!");
            InvokeNext();
        }
        else if(m_actionType == ActionType.JumpToClip)
        {
            host.curClipName = m_clipName;
            if(m_clipName == clip.m_name)
            {
                nextAction = clip.m_actions[0];
                InvokeNext();
            }
        }
        else if(m_actionType == ActionType.ActiveChild)
        {
            var go = host.gameObjects.First(x => x.key == m_eventName).gameObject;
            var o = go.activeSelf;
            go.SetActive(m_loop);
            if(m_resetOnExit)
            {
                new ResetOnExitContext(host, () => go.SetActive(o));
            }
            InvokeNext();
        }
        else if(m_actionType == ActionType.PlayAudio)
        {
            var a = host.GetComponent<AudioSource>();
            if(a != null)
            {
                a.PlayOneShot(m_audioGroup.GetClip());
            }
            InvokeNext();
        }
    }

    private class ResetOnExitContext
    {
        private Action callback;
        private DCSpriteAnimatorHost host;
        public ResetOnExitContext(DCSpriteAnimatorHost host, Action cb)
        {
            callback = cb;
            this.host = host;
            host.OnExitClip += Event;
        }

        private void Event(string prev, DCSpriteClipCollection.Clip newClip)
        {
            host.OnExitClip -= Event;
            callback();
        }
    }
}
