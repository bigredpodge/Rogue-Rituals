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
                NaturalBrands = new List<DevilBrand> { DevilBrand.Filth, DevilBrand.Squall, DevilBrand.Discord },

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
                NaturalBrands = new List<DevilBrand> { DevilBrand.Heat, DevilBrand.Squall },

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
                NaturalBrands = new List<DevilBrand> { DevilBrand.Lunacy, DevilBrand.Insight },

                OnAfterRound = (Devil devil) => {
                    if (devil.Base.Brand1 == DevilBrand.Lunacy || devil.Base.Brand2 == DevilBrand.Lunacy) {
                        devil.HealHP(devil.MaxHP / 12);

                        if (devil.StatBoosts[Stat.Strength] < 1) {
                            StatBoost strengthUp = new StatBoost {
                                stat = Stat.Strength,
                                boost = 1
                            };
                            devil.ApplyBoost(strengthUp);
                        } 
                    } 

                    if (devil.Base.Brand1 == DevilBrand.Insight || devil.Base.Brand2 == DevilBrand.Insight) {
                        devil.HealHP(devil.MaxHP / 12);

                        if (devil.StatBoosts[Stat.Discipline] < 1) {
                            StatBoost disciplineUp = new StatBoost {
                                stat = Stat.Discipline,
                                boost = 1
                            };
                            devil.ApplyBoost(disciplineUp);
                        } 
                    }    
                }
            }
        },
        {WeatherID.bloodrain, new Weather() {
                Name = "Blood Rain",
                StartMessage = "The battlefield is showered in blood.",
                ContinueMessage = "The blood rains without pause.",
                EndMessage = "The blood drizzles and stops.",
                NaturalBrands = new List<DevilBrand> { DevilBrand.Allure, DevilBrand.Rage },

                //Drunk/Berserk deal 1.5x damage
            }
        },
        {WeatherID.tempest, new Weather() {
                Name = "Tempest",
                StartMessage = "The battlefield is swept with a forceful tempsest.",
                ContinueMessage = "The winds rage and howl without relent.",
                EndMessage = "The tempest finally calms.",
                NaturalBrands = new List<DevilBrand> { DevilBrand.Squall, DevilBrand.Misery },

                OnAfterRound = (Devil devil) => {
                    Weather currentWeather = Weathers[WeatherID.bloodrain];
                    if (currentWeather.NaturalBrands.Contains(devil.Base.Brand1) || currentWeather.NaturalBrands.Contains(devil.Base.Brand2))
                        return;

                    devil.DamageHP((devil.MaxHP * (3 / 24)));

                    if (devil.Statuses.Count == 0) {
                        int randomNum = Random.Range(0, 9);
                        if (randomNum == 0)
                            devil.SetStatus(ConditionID.dsp);
                        else if (randomNum == 1)
                            devil.SetStatus(ConditionID.fbl);
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
    none, plague, ashstorm, moonlight, bloodrain, tempest
}

