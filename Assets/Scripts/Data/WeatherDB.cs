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

                OnAfterRound = (Devil devil) => {
                    if (devil.Base.Brand1 == DevilBrand.Filth || devil.Base.Brand2 == DevilBrand.Filth || devil.Base.Brand1 == DevilBrand.Squall || devil.Base.Brand2 == DevilBrand.Squall)
                        return;

                    devil.DamageHP((devil.MaxHP / 16));
                }
            }
        },
    };
}

public enum WeatherID {
    none, plague
}

