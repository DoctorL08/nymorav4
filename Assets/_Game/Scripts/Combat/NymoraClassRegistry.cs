using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ClassRegistry", menuName = "Nymora/Class Registry")]
public class NymoraClassRegistry : ScriptableObject
{
    public List<ClassData> classes = new List<ClassData>();
}
