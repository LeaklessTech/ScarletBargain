using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameObjectListVariable", menuName = "Scriptable Objects/GameObjectListVariable")]
public class GameObjectListVariable : ScriptableObject
{
    public List<GameObject> GameObjects;
}
