using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherDB
{

    public static void Init() {
        foreach (var kvp in Weathers) {
            var weatherId = kvp.Key;
            var weather = kvp.Value;

            weather.Id = weatherId;
        }
    }

    public static Dictionary<WeatherID, Weather> Weathers { get; set; } = new Dictionary<WeatherID, Weather>() {
        {WeatherID.plague, new Weather() {
                Name = "Plague",
                StartMessage = "A plague of insects descends on the battlefield.",
                ContinueMessage = "The plague of insects storms violently.",
                EndMessage = "The plague of insects dissipates.",
                NaturalBrands = new List<DevilBrand> { DevilBrand.Filth, DevilBrand.Squall },

                OnAfterRound = (Devil devil) => {
                    Weather currentWeather = Weathers[WeatherID.plague];
                    if (currentWeather.NaturalBrands.Contains(devil.Base.Brand1) || currentWeather.NaturalBrands.Contains(devil.Base.Brand2))
                        return;

                    devil.DamageHP((devil.MaxHP / 16));
                }
            }
        },
    };

    public static float GetWeatherBonus(Weather weather, Move move) {
        if (weather == null)
            return 1f;
        else if (weather.NaturalBrands.Contains(move.Base.Brand))
            return 1.5f;
        
        return 1f;
    }
}

public enum WeatherID {
    none, plague
}

