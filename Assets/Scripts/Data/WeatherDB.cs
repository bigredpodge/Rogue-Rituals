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

                    devil.DamageHP((devil.MaxHP / 12));
                }
            }
        },
        {WeatherID.ashstorm, new Weather() {
                Name = "Ashstorm",
                StartMessage = "Sulphurous ash swirls around the battlefield.",
                ContinueMessage = "The hot ash blinds and burns.",
                EndMessage = "The hot ash settles.",
                NaturalBrands = new List<DevilBrand> { DevilBrand.Heat },

                OnAfterRound = (Devil devil) => {
                    Weather currentWeather = Weathers[WeatherID.ashstorm];
                    if (currentWeather.NaturalBrands.Contains(devil.Base.Brand1) || currentWeather.NaturalBrands.Contains(devil.Base.Brand2))
                        return;

                    devil.DamageHP((devil.MaxHP / 12));

                    if (devil.StatBoosts[Stat.Accuracy] > -1) {
                        StatBoost accuracyDown = new StatBoost {
                            stat = Stat.Accuracy,
                            boost = -1
                        };
                        devil.ApplyBoost(accuracyDown);
                    }
                }
            }
        },
        {WeatherID.moonlight, new Weather() {
                Name = "Moonlight",
                StartMessage = "The battlefield is illuminated by moonlight.",
                ContinueMessage = "The full moon shines brightly.",
                EndMessage = "The moonlight fades from view.",
                NaturalBrands = new List<DevilBrand> { DevilBrand.Lunacy },

                OnAfterRound = (Devil devil) => {
                    Weather currentWeather = Weathers[WeatherID.moonlight];
                    if (currentWeather.NaturalBrands.Contains(devil.Base.Brand1) || currentWeather.NaturalBrands.Contains(devil.Base.Brand2)) {
                        devil.HealHP(devil.MaxHP / 12);

                        if (devil.StatBoosts[Stat.Strength] < 1) {
                            StatBoost strengthUp = new StatBoost {
                                stat = Stat.Strength,
                                boost = 1
                            };
                            devil.ApplyBoost(strengthUp);
                        } 
                    }   
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
    none, plague, ashstorm, moonlight
}

