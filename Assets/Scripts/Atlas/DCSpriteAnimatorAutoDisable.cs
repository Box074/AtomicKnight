using UnityEngine;

[RequireComponent(typeof(DCSpriteAnimator))]
public class DCSpriteAnimatorAutoDisable : MonoBehaviour
{

    void OnAnimationFinish(DCAtlas.Clip clip)
    {
        var sr = GetComponent<DCSpriteRenderer>();
        sr.Renderer.gameObject.SetActive(false);
        GetComponent<DCSpriteAnimator>().Stop();
    }
}
