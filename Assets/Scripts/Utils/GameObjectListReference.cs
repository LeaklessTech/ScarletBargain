using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameObjectListReference
{
    public bool UseConstant = true;
    public List<GameObject> ConstantValue;
    public GameObjectListVariable Variable;

    public List<GameObject> GameObjects
    {
        get { return UseConstant ? ConstantValue : Variable.GameObjects; }
    }
}
