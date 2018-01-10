﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Ensage;
using Ensage.Common.Threading;
using Ensage.SDK.Extensions;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;

namespace SkywrathMagePlus.Features
{
    internal class LinkenBreaker
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private Abilities Abilities { get; set; }

        private Unit Owner { get; }

        public TaskHandler Handler { get; }

        public LinkenBreaker(Config config)
        {
            Config = config;
            Menu = config.Menu;
            Abilities = config.Abilities;
            Owner = config.Main.Context.Owner;

            Handler = UpdateManager.Run(ExecuteAsync, false, false);
        }

        private async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                var target = Config.UpdateMode.Target;
                if (target == null)
                {
                    return;
                }

                List<KeyValuePair<string, uint>> breakerChanger = new List<KeyValuePair<string, uint>>();

                if (target.IsLinkensProtected())
                {
                    breakerChanger = Menu.LinkenBreakerChanger.Value.Dictionary.Where(
                        x => Menu.LinkenBreakerToggler.Value.IsEnabled(x.Key)).OrderByDescending(x => x.Value).ToList();
                }
                else if (target.IsSpellShieldProtected())
                {
                    breakerChanger = Menu.AntiMageBreakerChanger.Value.Dictionary.Where(
                        x => Menu.AntiMageBreakerToggler.Value.IsEnabled(x.Key)).OrderByDescending(x => x.Value).ToList();
                }

                foreach (var order in breakerChanger)
                {
                    // Eul
                    var eul = Abilities.Eul;
                    if (eul != null
                        && eul.ToString() == order.Key
                        && eul.CanBeCasted)
                    {
                        if (eul.CanHit(target))
                        {
                            eul.UseAbility(target);
                            await Await.Delay(eul.GetCastDelay(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // ForceStaff
                    var forceStaff = Abilities.ForceStaff;
                    if (forceStaff != null
                        && forceStaff.ToString() == order.Key
                        && forceStaff.CanBeCasted)
                    {
                        if (forceStaff.CanHit(target))
                        {
                            forceStaff.UseAbility(target);
                            await Await.Delay(forceStaff.GetCastDelay(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // Orchid
                    var orchid = Abilities.Orchid;
                    if (orchid != null
                        && orchid.ToString() == order.Key
                        && orchid.CanBeCasted)
                    {
                        if (orchid.CanHit(target))
                        {
                            orchid.UseAbility(target);
                            await Await.Delay(orchid.GetCastDelay(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // Bloodthorn
                    var bloodthorn = Abilities.Bloodthorn;
                    if (bloodthorn != null
                        && bloodthorn.ToString() == order.Key
                        && bloodthorn.CanBeCasted)
                    {
                        if (bloodthorn.CanHit(target))
                        {
                            bloodthorn.UseAbility(target);
                            await Await.Delay(bloodthorn.GetCastDelay(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // Nullifier
                    var nullifier = Abilities.Nullifier;
                    if (nullifier != null
                        && nullifier.ToString() == order.Key
                        && nullifier.CanBeCasted)
                    {
                        if (nullifier.CanHit(target))
                        {
                            nullifier.UseAbility(target);
                            await Await.Delay(nullifier.GetCastDelay(target) + nullifier.GetHitTime(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // RodofAtos
                    var rodofAtos = Abilities.RodofAtos;
                    if (rodofAtos != null
                        && rodofAtos.ToString() == order.Key
                        && rodofAtos.CanBeCasted)
                    {
                        if (rodofAtos.CanHit(target))
                        {
                            rodofAtos.UseAbility(target);
                            await Await.Delay(rodofAtos.GetCastDelay(target) + rodofAtos.GetHitTime(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // Hex
                    var hex = Abilities.Hex;
                    if (hex != null
                        && hex.ToString() == order.Key
                        && hex.CanBeCasted)
                    {
                        if (hex.CanHit(target))
                        {
                            hex.UseAbility(target);
                            await Await.Delay(hex.GetCastDelay(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // ArcaneBolt
                    var arcaneBolt = Abilities.ArcaneBolt;
                    if (arcaneBolt.ToString() == order.Key
                        && arcaneBolt.CanBeCasted)
                    {
                        if (arcaneBolt.CanHit(target))
                        {
                            arcaneBolt.UseAbility(target);
                            await Await.Delay(arcaneBolt.GetCastDelay(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // AncientSeal
                    var ancientSeal = Abilities.AncientSeal;
                    if (ancientSeal.ToString() == order.Key
                        && ancientSeal.CanBeCasted)
                    {
                        if (ancientSeal.CanHit(target))
                        {
                            ancientSeal.UseAbility(target);
                            await Await.Delay(ancientSeal.GetCastDelay(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // canceled
            }
            catch (Exception e)
            {
                Config.Main.Log.Error(e);
            }
        }
    }
}
