using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "GenericSummoner", menuName = "Summoner/Create New Generic Summoner")]
public class GenericSummoner : ScriptableObject {

    [SerializeField] string name;
    [SerializeField] List<DevilBase> possibleDevils;
    public string Name => name;
    public List<DevilBase> PossibleDevils => possibleDevils;

    public Devil GetDevilFromSummoner(int difficulty) {
        var dBase = possibleDevils[UnityEngine.Random.Range(0, possibleDevils.Count)];
        var dLevel = Mathf.FloorToInt(((difficulty * 5) / 8) + 6 + Mathf.FloorToInt(difficulty / 10) * UnityEngine.Random.Range(0.75f, 1.25f));
        var summonedDevil = new Devil(dBase, dLevel);
        return summonedDevil;
    }
}