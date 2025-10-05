using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;

public class RopeFactory : MonoBehaviour
{
    public static RopeFactory Instance;
    public GameObject ropeController;

    public float yOffset = 0.01f;
    public float xOffset = 0f;   

    void Awake()
    {
        Instance = this;
    }

    public void CreateNewRopeSegment(RopeController original, List<Transform> newNodes)
    {
        if (newNodes == null || newNodes.Count < 2) return;

        GameObject newRopeObj = Instantiate(ropeController, transform);
        SpriteShapeController ssc = newRopeObj.GetComponent<SpriteShapeController>();
        ssc.splineDetail = 3;
        // ssc.spline.isOpenEnded = true;
        newRopeObj.transform.parent = transform;
        newRopeObj.layer = 6;
        newRopeObj.transform.position = transform.position + Vector3.up * yOffset + Vector3.right * xOffset;
        newRopeObj.transform.localScale = Vector3.one;

        var spriteShape = newRopeObj.GetComponent<SpriteShapeController>();
        var spriteRenderer = newRopeObj.GetComponent<SpriteShapeRenderer>();
        var ropeCtrl = newRopeObj.GetComponent<RopeController>();
        ropeCtrl.broken = true;

        // 复制贴图和材质
        spriteShape.spriteShape = original.GetComponent<SpriteShapeController>().spriteShape;
        // spriteRenderer.material = original.GetComponent<SpriteShapeRenderer>().material;

        ropeCtrl.ropeNodes = newNodes;
        ropeCtrl.smoothness = original.smoothness;
    }
}