using UnityEngine;

public class Utility
{
    public static bool LayerIsInMask(LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }
}