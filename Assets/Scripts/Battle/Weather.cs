using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weather
{
    public WeatherID Id { get; set; }
    public string Name { get; set; }
    public string StartMessage { get; set; }
    public string ContinueMessage { get; set; }
    public string EndMessage { get; set; }
    public List<DevilBrand> NaturalBrands { get; set; }
    public Action<Devil> OnAfterRound { get; set; }
    
}
