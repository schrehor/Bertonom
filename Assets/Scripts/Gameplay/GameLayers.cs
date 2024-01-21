using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask solidObjectLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask grassLayer;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask fovLayer;
    [SerializeField] LayerMask portalLayer;
    [SerializeField] LayerMask triggersLayer;
    [SerializeField] LayerMask ledgeLayer;
    public static GameLayers i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    public LayerMask SolidLayer { get => solidObjectLayer; }
    public LayerMask InteractableLayer { get => interactableLayer; }
    public LayerMask GrassLayer { get => grassLayer; }
    public LayerMask PlayerLayer { get => playerLayer; }
    public LayerMask FovLayer { get => fovLayer; }
    public LayerMask PortalLayer { get => portalLayer; }
    public LayerMask TriggerableLayers
    {
        get => grassLayer | fovLayer | portalLayer | triggersLayer;
    }

    public LayerMask LedgeLayer { get => ledgeLayer; }
}
