using UnityEngine;

[CreateAssetMenu(menuName = "Spawning/Spawnable Prop", fileName = "SpawnableProp")]
public class SpawnableProp : ScriptableObject
{
    [Header("Prefab")]
    public GameObject Prefab;

    [Header("Placement")]
    public float YOffset = 0f;

    [Tooltip("Random local jitter on XZ plane (meters).")]
    public Vector2 JitterXZ = new Vector2(0.15f, 0.15f);

    [Tooltip("Snap Y-rotation to 90° increments.")]
    public bool SnapRotation90 = true;

    [Header("Selection Weighting")]
    [Min(0f)]
    [Tooltip("Relative chance to be chosen vs other props.")]
    public float Weight = 1f;

    [Header("Collision Bounds (optional)")]
    [Tooltip("If true, compute bounds from all Renderers in the prefab (instantiated offscreen).")]
    public bool UseRendererBounds = true;

    [Tooltip("If UseRendererBounds = false, these half extents are used for OverlapBox.")]
    public Vector3 ManualHalfExtents = new Vector3(0.4f, 0.6f, 0.4f);

    [Min(0f)]
    [Tooltip("Extra padding added to half extents for safety.")]
    public float BoundsPadding = 0.02f;

    [Header("Uniqueness & Guarantees")]
    [Tooltip("If true, at most one of this prop per room.")]
    public bool AtMostOnePerRoom = false;

    [Tooltip("GUARANTEES at least one of this prop spawns in EVERY room that has a floor.")]
    public bool MustSpawnInEveryRoom = false;
}