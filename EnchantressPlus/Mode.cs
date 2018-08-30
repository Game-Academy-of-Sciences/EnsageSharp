﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Ensage;
using Ensage.Common.Threading;
using Ensage.SDK.Extensions;
using Ensage.SDK.Orbwalker.Modes;
using Ensage.SDK.Service;
using Ensage.SDK.TargetSelector;

using SharpDX;

using PlaySharp.Toolkit.Helper.Annotations;

namespace EnchantressPlus
{
    [PublicAPI]
    internal class Mode : KeyPressOrbwalkingModeAsync
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private EnchantressPlus Main { get; }

        private UpdateMode UpdateMode { get; }

        private ITargetSelectorManager TargetSelector { get; }

        private float LastCastAttempt { get; set; }

        public Mode(
            IServiceContext context, 
            Key key,
            Config config) : base(context, key)
        {
            Config = config;
            Menu = config.Menu;
            Main = config.Main;
            UpdateMode = config.UpdateMode;
        }

        public override async Task ExecuteAsync(CancellationToken token)
        {
            var target = UpdateMode.Target;

            if (target != null && (!Menu.BladeMailItem || !target.HasModifier("modifier_item_blade_mail_reflect")))
            {
                var StunDebuff = target.Modifiers.FirstOrDefault(x => x.IsStunDebuff);
                var HexDebuff = target.Modifiers.FirstOrDefault(x => x.Name == "modifier_sheepstick_debuff");
                var AtosDebuff = target.Modifiers.FirstOrDefault(x => x.Name == "modifier_rod_of_atos_debuff");
                var MultiSleeper = Config.MultiSleeper;

                // Blink
                var Blink = Main.Blink;
                if (Blink != null
                    && Menu.ItemsToggler.Value.IsEnabled(Blink.ToString())
                    && Owner.Distance2D(Game.MousePosition) > Menu.BlinkActivationItem
                    && Owner.Distance2D(target) > 600
                    && Blink.CanBeCasted)
                {
                    var blinkPos = target.Position.Extend(Game.MousePosition, Menu.BlinkDistanceEnemyItem);
                    if (Owner.Distance2D(blinkPos) < Blink.CastRange)
                    {
                        Blink.UseAbility(blinkPos);
                        await Await.Delay(Blink.GetCastDelay(blinkPos), token);
                    }
                }

                if (!target.IsMagicImmune() && !target.IsInvulnerable()
                    && !target.HasAnyModifiers("modifier_abaddon_borrowed_time", "modifier_item_combo_breaker_buff")
                    && !target.HasAnyModifiers("modifier_winter_wyvern_winters_curse_aura", "modifier_winter_wyvern_winters_curse"))
                {
                    if (!target.IsBlockingAbilities())
                    {
                        // Hex
                        var Hex = Main.Hex;
                        if (Hex != null
                            && Menu.ItemsToggler.Value.IsEnabled(Hex.ToString())
                            && Hex.CanBeCasted
                            && Hex.CanHit(target)
                            && (StunDebuff == null || !StunDebuff.IsValid || StunDebuff.RemainingTime <= 0.3f)
                            && (HexDebuff == null || !HexDebuff.IsValid || HexDebuff.RemainingTime <= 0.3f))
                        {
                            Hex.UseAbility(target);
                            await Await.Delay(Hex.GetCastDelay(target), token);
                        }

                        // Orchid
                        var Orchid = Main.Orchid;
                        if (Orchid != null
                            && Menu.ItemsToggler.Value.IsEnabled(Orchid.ToString())
                            && Orchid.CanBeCasted
                            && Orchid.CanHit(target))
                        {
                            Main.Orchid.UseAbility(target);
                            await Await.Delay(Main.Orchid.GetCastDelay(target), token);
                        }

                        // Bloodthorn
                        var Bloodthorn = Main.Bloodthorn;
                        if (Bloodthorn != null
                            && Menu.ItemsToggler.Value.IsEnabled(Bloodthorn.ToString())
                            && Bloodthorn.CanBeCasted
                            && Bloodthorn.CanHit(target))
                        {
                            Bloodthorn.UseAbility(target);
                            await Await.Delay(Bloodthorn.GetCastDelay(target), token);
                        }

                        // Nullifier
                        var Nullifier = Main.Nullifier;
                        if (Nullifier != null
                            && Menu.ItemsToggler.Value.IsEnabled(Nullifier.ToString())
                            && Nullifier.CanBeCasted
                            && Nullifier.CanHit(target)
                            && (StunDebuff == null || !StunDebuff.IsValid || StunDebuff.RemainingTime <= 0.5f)
                            && (HexDebuff == null || !HexDebuff.IsValid || HexDebuff.RemainingTime <= 0.5f))
                        {
                            Nullifier.UseAbility(target);
                            await Await.Delay(Nullifier.GetCastDelay(target), token);
                        }

                        // RodofAtos
                        var RodofAtos = Main.RodofAtos;
                        if (RodofAtos != null
                            && Menu.ItemsToggler.Value.IsEnabled(RodofAtos.ToString())
                            && RodofAtos.CanBeCasted
                            && RodofAtos.CanHit(target)
                            && (StunDebuff == null || !StunDebuff.IsValid || StunDebuff.RemainingTime <= 0.5f)
                            && (AtosDebuff == null || !AtosDebuff.IsValid || AtosDebuff.RemainingTime <= 0.5f))
                        {
                            RodofAtos.UseAbility(target);
                            await Await.Delay(RodofAtos.GetCastDelay(target), token);
                        }

                        // Enchant
                        var Enchant = Main.Enchant;
                        if (Menu.AbilityToggler.Value.IsEnabled(Enchant.ToString())
                            && Enchant.CanBeCasted
                            && Enchant.CanHit(target))
                        {
                            Enchant.UseAbility(target);
                            await Await.Delay(Enchant.GetCastDelay(target), token);
                        }

                        // HurricanePike
                        var HurricanePike = Main.HurricanePike;
                        if (HurricanePike != null
                            && Menu.ItemsToggler.Value.IsEnabled(HurricanePike.ToString())
                            && HurricanePike.CanBeCasted
                            && HurricanePike.CanHit(target))
                        {
                            HurricanePike.UseAbility(target);
                            await Await.Delay(HurricanePike.GetCastDelay(target), token);
                        }

                        // HeavensHalberd
                        var HeavensHalberd = Main.HeavensHalberd;
                        if (HeavensHalberd != null
                            && Menu.ItemsToggler.Value.IsEnabled(HeavensHalberd.ToString())
                            && HeavensHalberd.CanBeCasted
                            && HeavensHalberd.CanHit(target))
                        {
                            HeavensHalberd.UseAbility(target);
                            await Await.Delay(HeavensHalberd.GetCastDelay(target), token);
                        }

                        // Veil
                        var Veil = Main.Veil;
                        if (Veil != null
                            && Menu.ItemsToggler.Value.IsEnabled(Veil.ToString())
                            && Veil.CanBeCasted
                            && Veil.CanHit(target))
                        {
                            Veil.UseAbility(target.Position);
                            await Await.Delay(Veil.GetCastDelay(target), token);
                        }

                        // Ethereal
                        var Ethereal = Main.Ethereal;
                        if (Ethereal != null
                            && Menu.ItemsToggler.Value.IsEnabled(Ethereal.ToString())
                            && Ethereal.CanBeCasted
                            && Ethereal.CanHit(target))
                        {
                            Ethereal.UseAbility(target);
                            MultiSleeper.Sleep(Ethereal.GetHitTime(target), "Ethereal");
                            await Await.Delay(Ethereal.GetCastDelay(target), token);
                        }

                        // Shivas
                        var Shivas = Main.Shivas;
                        if (Shivas != null
                            && Menu.ItemsToggler.Value.IsEnabled(Shivas.ToString())
                            && Shivas.CanBeCasted
                            && Owner.Distance2D(target) <= Shivas.Radius)
                        {
                            Shivas.UseAbility();
                            await Await.Delay(Shivas.GetCastDelay(), token);
                        }

                        if (!MultiSleeper.Sleeping("Ethereal") || target.IsEthereal())
                        {
                            // Dagon
                            var Dagon = Main.Dagon;
                            if (Dagon != null
                                && Menu.ItemsToggler.Value.IsEnabled("item_dagon_5")
                                && Dagon.CanBeCasted
                                && Dagon.CanHit(target))
                            {
                                Dagon.UseAbility(target);
                                await Await.Delay(Dagon.GetCastDelay(target), token);
                            }
                        }

                        // UrnOfShadows
                        var UrnOfShadows = Main.UrnOfShadows;
                        if (UrnOfShadows != null
                            && Menu.ItemsToggler.Value.IsEnabled(UrnOfShadows.ToString())
                            && UrnOfShadows.CanBeCasted
                            && UrnOfShadows.CanHit(target))
                        {
                            UrnOfShadows.UseAbility(target);
                            await Await.Delay(UrnOfShadows.GetCastDelay(target), token);
                        }

                        // SpiritVessel
                        var SpiritVessel = Main.SpiritVessel;
                        if (SpiritVessel != null
                            && Menu.ItemsToggler.Value.IsEnabled(SpiritVessel.ToString())
                            && SpiritVessel.CanBeCasted
                            && SpiritVessel.CanHit(target))
                        {
                            SpiritVessel.UseAbility(target);
                            await Await.Delay(SpiritVessel.GetCastDelay(target), token);
                        }
                    }
                    else
                    {
                        Config.LinkenBreaker.Handler.RunAsync();
                    }
                }
                
                // Necronomicon
                var Necronomicon = Main.Necronomicon;
                if (Necronomicon != null
                    && Menu.ItemsToggler.Value.IsEnabled("item_necronomicon_3")
                    && Necronomicon.CanBeCasted
                    && Owner.Distance2D(target) <= Owner.AttackRange(Context.Owner))
                {
                    Necronomicon.UseAbility();
                    await Await.Delay(Necronomicon.GetCastDelay(), token);
                }

                if (target.IsInvulnerable() || target.IsAttackImmune())
                {
                    Orbwalker.Move(Game.MousePosition);
                }
                else
                {
                    if (Menu.OrbwalkerItem.Value.SelectedValue.Contains("Default"))
                    {
                        Orbwalker.OrbwalkingPoint = Vector3.Zero;

                        if (!ImpetusCast(target))
                        {
                            Orbwalker.OrbwalkTo(target);
                        }
                    }
                    else if (Menu.OrbwalkerItem.Value.SelectedValue.Contains("Distance"))
                    {
                        var ownerDis = Math.Min(Owner.Distance2D(Game.MousePosition), 230);
                        var ownerPos = Owner.Position.Extend(Game.MousePosition, ownerDis);
                        var pos = target.Position.Extend(ownerPos, Menu.MinDisInOrbwalkItem);

                        if (!ImpetusCast(target))
                        {
                            Orbwalker.OrbwalkTo(target);
                        }

                        Orbwalker.OrbwalkingPoint = pos;
						Orbwalker.OrbwalkingPoint = Vector3.Zero;
                    }
                    else if (Menu.OrbwalkerItem.Value.SelectedValue.Contains("Free"))
                    {
                        if (Owner.Distance2D(target) < Owner.AttackRange(target) && target.Distance2D(Game.MousePosition) < Owner.AttackRange(target) 
                            || Owner.HasModifier("modifier_item_hurricane_pike_range"))
                        {
                            Orbwalker.OrbwalkingPoint = Vector3.Zero;

                            if (!ImpetusCast(target))
                            {
                                Orbwalker.OrbwalkTo(target);
                            }
                        }
                        else
                        {
                            Orbwalker.Move(Game.MousePosition);
                        }
                    }
                }
            }
            else
            {
                Orbwalker.Move(Game.MousePosition);
            }
        }

        private bool ImpetusCast(Hero target)
        {
            var Impetus = Main.Impetus;
            var modifierHurricanePike = Owner.HasModifier("modifier_item_hurricane_pike_range");
            
            if (!Impetus.IsReady || Owner.IsMuted() || Owner.IsSilenced() || !Menu.AbilityToggler.Value.IsEnabled(Impetus.ToString()))
            {
                if (modifierHurricanePike)
                {
                    return Orbwalker.Attack(target);
                }

                return false;
            }

            // Impetus Autocast
            if (modifierHurricanePike)
            {
                if (!Impetus.Ability.IsAutoCastEnabled)
                {
                    Impetus.Ability.ToggleAutocastAbility();
                }

                return Orbwalker.Attack(target);
            }
            else if (Impetus.Ability.IsAutoCastEnabled)
            {
                Impetus.Ability.ToggleAutocastAbility();
            }

            // Impetus
            if (Orbwalker.CanAttack(target))
            {
                var time = Game.RawGameTime;
                if ((time - LastCastAttempt) > 0.1f)
                {
                    Impetus.UseAbility(target);
                    LastCastAttempt = time;
                }
                
                return true;
            }

            return false;
        }
    }
}
