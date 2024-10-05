using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class DevilParty : MonoBehaviour
{
    [SerializeField] List<Devil> devils;

    public List<Devil> Devils {
        get {
            return devils;
        }
    }

    private void Start() {
        foreach (var devil in devils) {
            devil.Init();
        }
    }

    public Devil GetHealthyDevil() {
        return devils.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddDevil(Devil newDevil) {
        if (devils.Count < 6) {
            devils.Add(newDevil);
        }
        else {
            //pick devil to replace
        }
    }

    public void ReleaseDevilAt(int i) {
        devils.RemoveAt(i);
    }

}
