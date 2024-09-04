using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Locale", menuName = "Devil/Create New Locale")]
[System.Serializable]
public class Locale : ScriptableObject {
    [SerializeField] List<DevilBase> commonDevilBases;
    public List<DevilBase> CommonDevilBases => commonDevilBases;
    [SerializeField] List<DevilBase> uncommonDevilBases;
    public List<DevilBase> UncommonDevilBases => uncommonDevilBases;
    [SerializeField] List<DevilBase> rareDevilBases;
    public List<DevilBase> RareDevilBases => rareDevilBases;
    [SerializeField] List<LocaleID> nextAvailableLocales;
    public List<LocaleID> NextAvailableLocales => nextAvailableLocales;
    [SerializeField] Material material;
    public Material Material => material;

    LocaleID localeID;
    public LocaleID LocaleID {
        get  { return localeID; }
        set { localeID = value; }
    }

    public Devil GetRandomWildDevil(List<DevilBase> devilBases, int difficulty) {
        var dBase = devilBases[Random.Range(0, devilBases.Count)];
        var dLevel = Mathf.FloorToInt(((difficulty * 5) / 8) + 4 + Mathf.FloorToInt(difficulty / 10) * Random.Range(0.75f, 1.25f));
        var wildDevil = new Devil(dBase, dLevel);
        return wildDevil;
    }

}

public enum LocaleID {
    //below enum list must be in the same order as the Game Controller's locales list in inspector.
    Dungeon,
    Sewers,
    Library,
    Forest
}

