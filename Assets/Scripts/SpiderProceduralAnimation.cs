using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpiderProceduralAnimation : MonoBehaviour
{
    [Header("GameObject Assignment")]
    public GameObject spider;
    public GameObject[] legTargets;
    public GameObject[] legCubes;

    [Header("Leg Movement Properties")]
    public float moveDistance = 0.7f;
    public float stepHeight = 0.15f;
    public float spiderJitterCutOff = 0f;
    public float overStepMultiplier = 4f;
    public int waitTimeBetweenSteps = 0;
    public int bodySmoothness = 8;
    public int legSmoothness = 4;

    Vector3 velocity;
    Vector3 lastVelocity = Vector3.one;
    Vector3 lastSpiderPosition;
    Vector3 lastBodyUp;
    Vector3[] legPositions;
    Vector3[] legOriginalPositions;
    List<int> oppositeLegIndex = new List<int>();
    List<int> nextIndexToMove = new List<int>();
    List<int> indexMoving = new List<int>();
    bool currentLeg = true;

    /**
     * Initializes leg positions and opposite-leg pairings.
     */
    void Start()
    {
        lastBodyUp = transform.up;
        legPositions = new Vector3[legTargets.Length];
        legOriginalPositions = new Vector3[legTargets.Length];

        for (int i = 0; i < legTargets.Length; i++)
        {
            Vector3 position = legTargets[i].transform.position;
            legPositions[i] = position;
            legOriginalPositions[i] = position;

            if (currentLeg) 
            {
                oppositeLegIndex.Add(i + 1);
                currentLeg = false;
            }
            else
            {
                oppositeLegIndex.Add(i - 1);
                currentLeg = true;
            }
        }

        lastSpiderPosition = spider.transform.position;
        RotateBody();
    }

    /**
     * Updates spider velocity, triggers leg stepping, and rotates body.
     */
    void FixedUpdate()
    {
        velocity = (spider.transform.position - lastSpiderPosition + bodySmoothness * lastVelocity) 
                    / (bodySmoothness + 1f);

        MoveLegs();
        RotateBody();

        lastSpiderPosition = spider.transform.position;
        lastVelocity = velocity;
    }

    /**
     * Checks each leg for step threshold and initiates step if needed.
     */
    void MoveLegs()
    {
        for (int i = 0; i < legTargets.Length; i++)
        {
            if (Vector3.Distance(legTargets[i].transform.position, legCubes[i].transform.position) >= moveDistance)
            {
                if (!nextIndexToMove.Contains(i) && !indexMoving.Contains(i)) nextIndexToMove.Add(i);
            }
            else if (!indexMoving.Contains(i))
            {
                legTargets[i].transform.position = legOriginalPositions[i];
            }
        }

        if (nextIndexToMove.Count == 0 || indexMoving.Count > 0) return;

        Vector3 targetPosition = legCubes[nextIndexToMove[0]].transform.position 
                                + Mathf.Clamp(velocity.magnitude * overStepMultiplier, 0.0f, 1.5f) 
                                * (legCubes[nextIndexToMove[0]].transform.position 
                                - legTargets[nextIndexToMove[0]].transform.position) 
                                + velocity * overStepMultiplier;
        StartCoroutine(Step(nextIndexToMove[0], targetPosition, false));
    }

    /**
     * Initiates a mirrored step for the opposite leg.
     *
     * @param index Index of the opposite leg to move.
     */
    void MoveOppositeLeg(int index)
    {
        Vector3 targetPosition = legCubes[index].transform.position 
                                + Mathf.Clamp(velocity.magnitude * overStepMultiplier, 0.0f, 1.5f) 
                                * (legCubes[index].transform.position - legTargets[index].transform.position) 
                                + velocity * overStepMultiplier;
        StartCoroutine(Step(index, targetPosition, true));
    }

    /**
     * Animates a leg stepping from its current position to a new one.
     *
     * @param index Index of the leg to move.
     * @param moveTo Target world position for the step.
     * @param isOpposite True if the leg is part of an opposite-leg pair step.
     */
    IEnumerator Step(int index, Vector3 moveTo, bool isOpposite)
    {
        if (!isOpposite) MoveOppositeLeg(oppositeLegIndex[index]);
        if (nextIndexToMove.Contains(index)) nextIndexToMove.Remove(index);
        if (!indexMoving.Contains(index)) indexMoving.Add(index);

        Vector3 startPosition = legOriginalPositions[index];

        for (int i = 1; i <= legSmoothness; i++)
        {
            legTargets[index].transform.position = Vector3.Lerp(startPosition, moveTo + 
                                                    new Vector3(0, Mathf.Sin(i 
                                                    / (float)(legSmoothness + spiderJitterCutOff) 
                                                    * Mathf.PI) * stepHeight, 0), 
                                                    (i / legSmoothness + spiderJitterCutOff));
            yield return new WaitForFixedUpdate();
        }

        legOriginalPositions[index] = moveTo;

        for (int i = 1; i <= waitTimeBetweenSteps; i++) yield return new WaitForFixedUpdate();

        if (indexMoving.Contains(index)) indexMoving.Remove(index);
    }

    /**
     * Rotates the spider body to align with the average plane formed by leg pairs.
     */
    void RotateBody()
    {
        Vector3 v1 = legTargets[0].transform.position - legTargets[1].transform.position;
        Vector3 v2 = legTargets[2].transform.position - legTargets[3].transform.position;
        Vector3 normal = Vector3.Cross(v1, v2).normalized;
        Vector3 up = Vector3.Lerp(lastBodyUp, normal, 1f / (float)(bodySmoothness));
        transform.up = up;
        lastBodyUp = up;
    }
}