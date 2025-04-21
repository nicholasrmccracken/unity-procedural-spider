using UnityEngine;

public class SpiderLegGrounding : MonoBehaviour
{
    GameObject raycastOrigin;
    int layerMask;

    /**
     * Initializes the raycast layer and origin.
     */
    void Start()
    {
        layerMask = LayerMask.GetMask("Ground");
        raycastOrigin = transform.parent.gameObject;
    }

    /**
     * Updates the leg position by raycasting downward toward the ground.
     */
    void Update()
    {
        RaycastHit hit;

        if (Physics.Raycast(raycastOrigin.transform.position, 
        -transform.up, out hit, Mathf.Infinity, layerMask))
        {
            transform.position = hit.point;
        }
    }
}