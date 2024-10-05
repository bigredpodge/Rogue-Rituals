using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition
{
    public ConditionID Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; }
    public Action<Devil> OnStart { get; set; }
    public Action<Devil> OnRefresh { get; set; }
    public Func<Devil, bool> OnBeforeMove { get; set; }
    public Action<Devil> OnAfterTurn { get; set; }
    
}
