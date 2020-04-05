using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshairs : MonoBehaviour
{
    public LayerMask targetMask; // 타겟 layer
    public SpriteRenderer dot; // 점 렌더러
    public Color dotHighlightColor; // 적에 닿았을때 색상
    Color originalDotColor; // 원래 색상

    void Start()
    {
        Cursor.visible = false;

        originalDotColor = dot.color;
    }

    void Update()
    {
        transform.Rotate(Vector3.forward * -40 * Time.deltaTime);
    }

    public void DetectTargets(Ray ray)
    {
        if(Physics.Raycast(ray,100,targetMask))
        {
            dot.color = dotHighlightColor;
        } else
        {
            dot.color = originalDotColor;
        }
    }
}
