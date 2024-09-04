using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Item/Create new scroll item")]
public class ScrollItem : ItemBase
{
[SerializeField] MoveBase move;
public MoveBase Move => move;

}


