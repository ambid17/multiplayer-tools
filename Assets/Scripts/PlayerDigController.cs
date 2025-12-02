using Digger.Modules.Core.Sources;
using Digger.Modules.Runtime.Sources;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDigController : NetworkBehaviour
{
    public LayerMask groundLayer;

    [SerializeField] private BrushType brushType = BrushType.Sphere;
    [SerializeField] private ActionType actionType = ActionType.Dig;
    [SerializeField] private int textureIndex = 0;
    [SerializeField] private float opacityMax = 0.5f;
    [SerializeField] private int frameInPastToPaint = 10;
    [SerializeField] private float digSizeMultiplier = 0.8f;
    private List<DigAction> digActions = new List<DigAction>();
    private Rigidbody rb;
    private DiggerMasterRuntime digger;


    void Start()
    {
        digger = FindAnyObjectByType<DiggerMasterRuntime>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!IsOwner)
            return;

        var velocity = rb.linearVelocity;
        var horizontalVelocity = new Vector3 (velocity.x, 0, velocity.z);

        if (horizontalVelocity.magnitude < PlayerController.minSpeedToGrow)
            return;

        var opacity = Mathf.Lerp(0, opacityMax, horizontalVelocity.magnitude / PlayerController.theoreticalMaxVelocity);
        var digLocation = transform.position - (velocity * Time.deltaTime * frameInPastToPaint);
        var digSize = transform.localScale.x * digSizeMultiplier;
        if (Physics.Raycast(digLocation, Vector3.down, out var hit, transform.localScale.y + 0.01f, groundLayer))
        {
            digger.ModifyAsyncBuffured(digLocation, brushType, actionType, textureIndex, opacity, digSize);
            digActions.Add(new DigAction
            {
                position = digLocation,
                size = digSize,
                opacity = opacity
            });
        }
    }
}
