﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Ensage;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Extensions;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;
using Ensage.SDK.Prediction;

namespace LegionCommanderPlus.Features
{
    internal class AutoKillSteal
    {
        private MenuManager Menu { get; }

        private LegionCommanderPlus Main { get; }

        private UpdateMode UpdateMode { get; }

        private LinkenBreaker LinkenBreaker { get; }

        private Extensions Extensions { get; }

        private DamageCalculation DamageCalculation { get; }

        private MultiSleeper MultiSleeper { get; }

        private Unit Owner { get; }

        private DamageCalculation.Damage Damage { get; set; }

        private TaskHandler Handler { get; }

        private IUpdateHandler UpdateHandler { get; set; }
        
        public AutoKillSteal(Config config)
        {
            Menu = config.Menu;
            Main = config.Main;
            UpdateMode = config.UpdateMode;
            LinkenBreaker = config.LinkenBreaker;
            Extensions = config.Extensions;
            DamageCalculation = config.DamageCalculation;
            MultiSleeper = config.MultiSleeper;
            Owner = config.Main.Context.Owner;

            Handler = UpdateManager.Run(ExecuteAsync, true, false);

            if (config.Menu.AutoKillStealItem)
            {
                Handler.RunAsync();
            }

            config.Menu.AutoKillStealItem.PropertyChanged += AutoKillStealChanged;

            UpdateHandler = UpdateManager.Subscribe(Stop, 0, false);
        }

        public void Dispose()
        {
            Menu.AutoKillStealItem.PropertyChanged -= AutoKillStealChanged;

            if (Menu.AutoKillStealItem)
            {
                Handler?.Cancel();
            }
        }

        private void AutoKillStealChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateManager.Unsubscribe(Stop);

            if (Menu.AutoKillStealItem)
            {
                Handler.RunAsync();
            }
            else
            {
                Handler?.Cancel();
            }
        }

        private async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                if (Game.IsPaused || !Owner.IsValid || !Owner.IsAlive || Owner.IsStunned())
                {
                    return;
                }

                if (Menu.AutoKillWhenComboItem && Menu.ComboKeyItem)
                {
                    return;
                }

                var damageCalculation = DamageCalculation.DamageList.Where(x => (x.GetHealth - x.GetDamage) / x.GetTarget.MaximumHealth <= 0.0f).ToList();
                Damage = damageCalculation.OrderByDescending(x => x.GetHealth).OrderByDescending(x => x.GetTarget.Player.Kills).FirstOrDefault();

                if (Damage == null)
                {
                    return;
                }

                if (!UpdateHandler.IsEnabled)
                {
                    UpdateHandler.IsEnabled = true;
                }

                var target = Damage.GetTarget;

                if (!Cancel(target) || Extensions.ComboBreaker(target, false))
                {
                    return;
                }

                if (!target.IsBlockingAbilities())
                {
                    // Veil
                    var veil = Main.Veil;
                    if (veil != null
                        && Menu.AutoKillStealToggler.Value.IsEnabled(veil.ToString())
                        && veil.CanBeCasted
                        && veil.CanHit(target))
                    {
                        veil.UseAbility(target.Position);
                        await Task.Delay(veil.GetCastDelay(target.Position), token);
                    }

                    // Ethereal
                    var ethereal = Main.Ethereal;
                    if (ethereal != null
                        && Menu.AutoKillStealToggler.Value.IsEnabled(ethereal.ToString())
                        && ethereal.CanBeCasted
                        && ethereal.CanHit(target))
                    {
                        ethereal.UseAbility(target);
                        MultiSleeper.Sleep(ethereal.GetHitTime(target), "ethereal");
                        await Task.Delay(ethereal.GetCastDelay(target), token);
                    }

                    // Shivas
                    var shivas = Main.Shivas;
                    if (shivas != null
                        && Menu.AutoKillStealToggler.Value.IsEnabled(shivas.ToString())
                        && shivas.CanBeCasted
                        && shivas.CanHit(target))
                    {
                        shivas.UseAbility();
                        await Task.Delay(shivas.GetCastDelay(), token);
                    }

                    if (!MultiSleeper.Sleeping("ethereal") || target.IsEthereal())
                    {
                        // Dagon
                        var dagon = Main.Dagon;
                        if (dagon != null
                            && Menu.AutoKillStealToggler.Value.IsEnabled("item_dagon_5")
                            && dagon.CanBeCasted
                            && dagon.CanHit(target))
                        {
                            dagon.UseAbility(target);
                            await Task.Delay(dagon.GetCastDelay(target), token);
                        }
                    }
                }
                else
                {
                    LinkenBreaker.Handler.RunAsync();
                }

                if (!MultiSleeper.Sleeping("ethereal") || target.IsEthereal())
                {
                    // Overwhelming Odds
                    var overwhelmingOdds = Main.OverwhelmingOdds;
                    if (Menu.AutoKillStealToggler.Value.IsEnabled(overwhelmingOdds.ToString()) && overwhelmingOdds.CanBeCasted)
                    {
                        var input = new PredictionInput
                        {
                            Owner = Owner,
                            AreaOfEffect = overwhelmingOdds.HasAreaOfEffect,
                            AreaOfEffectTargets = UpdateMode.OverwhelmingOddsUnits,
                            CollisionTypes = overwhelmingOdds.CollisionTypes,
                            Delay = overwhelmingOdds.CastPoint + overwhelmingOdds.ActivationDelay,
                            Speed = overwhelmingOdds.Speed,
                            Range = overwhelmingOdds.CastRange,
                            Radius = overwhelmingOdds.Radius,
                            PredictionSkillshotType = overwhelmingOdds.PredictionSkillshotType
                        };

                        var castPosition = overwhelmingOdds.GetPredictionOutput(input.WithTarget(target)).CastPosition;
                        if (Owner.Distance2D(castPosition) <= overwhelmingOdds.CastRange)
                        {
                            overwhelmingOdds.UseAbility(castPosition);
                            await Task.Delay(overwhelmingOdds.GetCastDelay(castPosition), token);
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
                Main.Log.Error(e);
            }
        }

        private bool Cancel(Hero target)
        {
            return !Owner.IsInvisible()
                && !target.IsMagicImmune()
                && !target.IsInvulnerable()
                && !target.HasAnyModifiers("modifier_dazzle_shallow_grave", "modifier_necrolyte_reapers_scythe")
                && !Reincarnation(target);
        }

        private bool Reincarnation(Hero target)
        {
            var reincarnation = target.GetAbilityById(AbilityId.skeleton_king_reincarnation);
            return reincarnation != null && reincarnation.Cooldown == 0 && reincarnation.Level > 0;
        }

        private void Stop()
        {
            if (Damage == null)
            {
                UpdateHandler.IsEnabled = false;
                return;
            }

            var stop = EntityManager<Hero>.Entities.Any(x => !x.IsAlive && x == Damage.GetTarget);
            if (stop && Owner.Animation.Name.Contains("cast"))
            {
                Owner.Stop();
                UpdateHandler.IsEnabled = false;
            }
        }
    }
}