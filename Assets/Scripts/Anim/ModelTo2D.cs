using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ModelTo2D : MonoBehaviour
{
    public SkinnedMeshRenderer meshRenderer;
    public Animator animator;
    public Transform boneRoot;
    public Camera renderCamera;
    public int fps;
    public float time;
    public int pixelPerUnit;
    private int id;
    private Vector2 prevPos;
    private string curClip;
    private Queue<AnimationClip> clips;
    private float clipTime;
    private Dictionary<string, AHTileInfo> tiles;
    // Start is called before the first frame update
    void Start()
    {
        time = 1f / fps;

        clips = new Queue<AnimationClip>(animator.runtimeAnimatorController.animationClips.ToList());
        Debug.Log($"Clips Count: {clips.Count}");

        tiles = new Dictionary<string, AHTileInfo>();
        animator.Play(clips.Dequeue().name);
    }

    private bool CheckClip()
    {
        clipTime -= Time.deltaTime;

        var a = animator.GetCurrentAnimatorClipInfo(0)[0];
        if (a.clip.name != curClip)
        {
            curClip = a.clip.name;
            time = 0;
            prevPos = Vector2.zero;
            clipTime = a.clip.length;
            id = 0;

            Debug.Log($"Change Clip: {a.clip.name} ({clips.Count})");
        }

        if (clipTime <= 0)
        {
            SaveData();
            if (clips.Count == 0)
            {
                enabled = false;
                return false;
            }
            var clip = clips.Dequeue();
            id = 0;
            clipTime = clip.length;
            animator.Play(clip.name);
            return false;
        }
        return true;
    }

    private void SaveData()
    {
        File.WriteAllText(@"J:\TAAJ\AHTileInfo.json", JsonConvert.SerializeObject(tiles));
    }

    // Update is called once per frame
    void Update()
    {
        if (!CheckClip()) return;
        var bounds = meshRenderer.bounds;

        renderCamera.orthographicSize = bounds.size.y / 2;
        renderCamera.aspect = bounds.size.x / bounds.size.y;
        var pos = bounds.center;
        pos.z = -1000;
        renderCamera.transform.position = pos;

        time -= Time.deltaTime;
        var bp = boneRoot.position;
        bp.z = 0;
        Debug.DrawLine(boneRoot.position, boneRoot.position + new Vector3(0, 10, 0), Color.red);
        if (time > 0) return;
        time = 1f / fps;

        var rtex = new RenderTexture(Mathf.RoundToInt(bounds.size.x * pixelPerUnit),
            Mathf.RoundToInt(bounds.size.y * pixelPerUnit), 32);
        renderCamera.targetTexture = rtex;
        renderCamera.Render();
        renderCamera.targetTexture = null;

        var tex2D = new Texture2D(rtex.width, rtex.height);
        RenderTexture.active = rtex;
        tex2D.ReadPixels(new Rect(0, 0, rtex.width, rtex.height), 0, 0);
        tex2D.Apply();
        RenderTexture.active = null;

        rtex.Release();

        var offset = (Vector2)bp - prevPos;
        prevPos = bp;

        var clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        var rootOffset = ((Vector2)bp - (Vector2)pos) * pixelPerUnit;
        Debug.Log($"{curClip} {id} ({clip.length - clipTime}s/{clip.length}s)(fps: {id / clip.length}): {offset} {rootOffset}");

        var frendlyName = curClip.Replace('|', '_') + "_" + id++;



        tiles[frendlyName] = new AHTileInfo()
        {
            name = frendlyName,
            pivot = rootOffset,
            moveOffset = offset
        };

        File.WriteAllBytes(Path.Combine(@"J:\TAA", frendlyName + ".png"),
            tex2D.EncodeToPNG());
        Destroy(tex2D);
    }
}
