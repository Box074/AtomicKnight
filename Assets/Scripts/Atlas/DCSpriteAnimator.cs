using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(DCSpriteRenderer))]
[ExecuteAlways]
public class DCSpriteAnimator : MonoBehaviour
{
    public DCSpriteRenderer Renderer
    {
        get
        {
            if(m_sr == null)
            {
                m_sr = GetComponent<DCSpriteRenderer>();
            }
            return m_sr;
        }
    }
    private DCSpriteRenderer m_sr;
    private DCAtlas Atlas => Renderer?.atlas?.atlas;

    public UnityEvent<DCSpriteAnimator, DCAtlas, DCAtlas.Clip> onComplete;
    public UnityEvent<DCSpriteAnimator, DCAtlas, DCAtlas.Clip, int> onNextFrame;

    public int fps = 24;
    public bool loop = false;

    public float speed = 1;

    public string curClipName;
    public int curClipFrame;
    public bool isPlaying = false;

    private DCAtlas.Clip m_curClip;
    private int? m_curFrame;

    private float m_time;

    public void Stop()
    {
        m_time = 0;
        m_curClip = null;
        m_curFrame = null;
        isPlaying = false;
    }

    public class WaitForClipEnd : CustomYieldInstruction
    {
        public override bool keepWaiting => m_isPlaying;

        private string m_clipName;
        private bool m_isPlaying;

        private void OnComplete(DCSpriteAnimator animation, DCAtlas atlas, DCAtlas.Clip clip)
        {
            if(clip.name == m_clipName || string.IsNullOrEmpty(m_clipName))
            {
                m_isPlaying = false;
                animation.onComplete.RemoveListener(OnComplete);
            }
        }

        public WaitForClipEnd(DCSpriteAnimator animator, string clipName)
        {
            m_clipName = clipName;
            m_isPlaying = true;

            animator.onComplete.AddListener(OnComplete);
        }
    }

    public class WaitForClipFrame : CustomYieldInstruction
    {
        public override bool keepWaiting => m_isPlaying;

        private string m_clipName;
        private int m_frame;
        private bool m_isPlaying;

        private void OnNextFrame(DCSpriteAnimator animation, DCAtlas atlas, 
            DCAtlas.Clip clip, int frame)
        {
            var targetFrame = frame;
            if(frame < 0)
            {
                targetFrame = clip.frames.Count + frame;
            }
            if ((clip.name == m_clipName || string.IsNullOrEmpty(m_clipName)) && m_frame == targetFrame)
            {
                m_isPlaying = false;
                animation.onNextFrame.RemoveListener(OnNextFrame);
            }
        }

        public WaitForClipFrame(DCSpriteAnimator animator, string clipName, int frame)
        {
            m_clipName = clipName;
            m_frame = frame;
            m_isPlaying = true;

            animator.onNextFrame.AddListener(OnNextFrame);
        }
    }

    public void Play(string name, bool loop = false, int startFrame = 0)
    {
        Stop();

        this.loop = loop;
        m_time = 1f / fps;
        curClipFrame = startFrame;
        curClipName = name;
        isPlaying = true;

        Renderer.Renderer.gameObject.SetActive(true);
        Renderer.Renderer.enabled = true;

        onNextFrame.Invoke(this, Atlas, m_curClip, 0);
    }

    public IEnumerator WaitForClip(string name = null)
    {
        return new WaitForClipEnd(this, name);
    }

    public IEnumerator WaitForFrame(int frame, string name = null)
    {
        return new WaitForClipFrame(this, name, frame);
    }

    private void Awake()
    {
        m_sr = GetComponent<DCSpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Atlas == null)
        {
            return;
        }
        m_time -= (Time.deltaTime * speed);
        if(m_curClip == null || m_curClip.name != curClipName)
        {
            m_curFrame = null;
            m_curClip = Atlas.clips.FirstOrDefault(x => x.name == curClipName);
        }
        if(m_curClip == null)
        {
            if(!string.IsNullOrEmpty(curClipName))
            {
                Debug.LogWarning("Not found clip: " + curClipName);
            }
            return;
        }
        if(curClipFrame < 0)
        {
            curClipFrame = 0;
        }
        if(curClipName == "Armature_AS_rotorobot_agro_run" && curClipFrame < 3)
        {
            curClipFrame= 3;
        }
        if(curClipFrame >= m_curClip.frames.Count)
        {
            var clip = m_curClip;
            Stop();
            if(loop)
            {
                Play(curClipName, loop, 0);
                Update();
                return;
            }
            onComplete.Invoke(this, Atlas, clip);
            if(Application.isPlaying || !Application.isEditor) SendMessage("OnAnimationFinish", clip, SendMessageOptions.DontRequireReceiver);
        }

        if(m_curClip == null)
        {
            return;
        }

        if (m_curFrame == null || m_curFrame != curClipFrame)
        {
            m_curFrame = curClipFrame;

            Renderer.curSpriteId = m_curClip.frames[curClipFrame];
        }

        if(m_time <= 0 && isPlaying && (!Application.isEditor || Application.isPlaying))
        {
            curClipFrame++;
            m_time = 1f / fps;
            onNextFrame.Invoke(this, Atlas, m_curClip, curClipFrame);
        }
    }
}
