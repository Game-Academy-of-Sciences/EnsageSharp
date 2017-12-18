﻿using System.Collections.Generic;
using System.Linq;

using Ensage;
using Ensage.SDK.Abilities;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Prediction;

namespace LegionCommanderPlus.Features
{
    internal class DamageCalculation
    {
        private MenuManager Menu { get; }

        private LegionCommanderPlus Main { get; }

        private UpdateMode UpdateMode { get; }

        private Unit Owner { get; }

        public DamageCalculation(Config config)
        {
            Menu = config.Menu;
            Main = config.Main;
            UpdateMode = config.UpdateMode;
            Owner = config.Main.Context.Owner;

            UpdateManager.Subscribe(OnUpdate);
        }

        public void Dispose()
        {
            UpdateManager.Unsubscribe(OnUpdate);
        }

        private void OnUpdate()
        {
            var heroes = EntityManager<Hero>.Entities.Where(x => x.IsValid && !x.IsIllusion).ToList();

            DamageList.Clear();

            foreach (var target in heroes.Where(x => x.IsAlive && x.IsEnemy(Owner)).ToList())
            {
                List<BaseAbility> abilities = new List<BaseAbility>();

                var damageOdds = 0.0f;

                if (target.IsVisible)
                {
                    // Veil
                    var veil = Main.Veil;
                    if (veil != null && veil.Ability.IsValid && Menu.AutoKillStealToggler.Value.IsEnabled(veil.ToString()))
                    {
                        abilities.Add(veil);
                    }

                    // Ethereal
                    var ethereal = Main.Ethereal;
                    if (ethereal != null && ethereal.Ability.IsValid && Menu.AutoKillStealToggler.Value.IsEnabled(ethereal.ToString()))
                    {
                        abilities.Add(ethereal);
                    }

                    // Shivas
                    var shivas = Main.Shivas;
                    if (shivas != null && shivas.Ability.IsValid && Menu.AutoKillStealToggler.Value.IsEnabled(shivas.ToString()))
                    {
                        abilities.Add(shivas);
                    }

                    // Dagon
                    var dagon = Main.Dagon;
                    if (dagon != null && dagon.Ability.IsValid && Menu.AutoKillStealToggler.Value.IsEnabled("item_dagon_5"))
                    {
                        abilities.Add(dagon);
                    }

                    // Overwhelming Odds
                    var overwhelmingOdds = Main.OverwhelmingOdds;
                    if (overwhelmingOdds.Ability.Level > 0 && Menu.AutoKillStealToggler.Value.IsEnabled(overwhelmingOdds.ToString()))
                    {
                        var input = new PredictionInput
                        {
                            Owner = Owner,
                            AreaOfEffect = overwhelmingOdds.HasAreaOfEffect,
                            AreaOfEffectTargets = UpdateMode.OverwhelmingOddsUnits,
                            CollisionTypes = overwhelmingOdds.CollisionTypes,
                            Delay = overwhelmingOdds.CastPoint + overwhelmingOdds.ActivationDelay,
                            Speed = overwhelmingOdds.Speed,
                            Range = float.MaxValue,
                            Radius = overwhelmingOdds.Radius,
                            PredictionSkillshotType = overwhelmingOdds.PredictionSkillshotType
                        };

                        var castPosition = overwhelmingOdds.GetPredictionOutput(input.WithTarget(target)).CastPosition;
                        var damageUnits = overwhelmingOdds.GetDamage(castPosition, target);

                        if (overwhelmingOdds.IsReady && !Owner.IsStunned() && !Owner.IsMuted() && !Owner.IsSilenced())
                        {
                            if (Owner.Distance2D(castPosition) <= overwhelmingOdds.CastRange)
                            {
                                damageOdds += damageUnits;
                            }
                        }
                    }
                }

                var damageCalculation = new Combo(abilities.ToArray());
                var damageReduction = -DamageReduction(target, heroes);
                var damageBlock = MagicalDamageBlock(target, heroes);

                var damage = DamageHelpers.GetSpellDamage((damageCalculation.GetDamage(target) + damageOdds) + damageBlock, 0, damageReduction);

                if (target.IsInvulnerable() || target.HasAnyModifiers(BlockModifiers))
                {
                    damage = 0.0f;
                }

                DamageList.Add(new Damage(target, damage, target.Health));
            }
        }

        private string[] BlockModifiers { get; } =
        {
            "modifier_abaddon_borrowed_time",
            "modifier_item_combo_breaker_buff",
            "modifier_winter_wyvern_winters_curse_aura",
            "modifier_winter_wyvern_winters_curse",
            "modifier_templar_assassin_refraction_absorb",
            "modifier_oracle_fates_edict",
            "modifier_dark_willow_shadow_realm_buff"
        };

        private float DamageReduction(Hero target, List<Hero> heroes)
        {
            var value = 0.0f;

            // Bristleback
            var bristleback = target.GetAbilityById(AbilityId.bristleback_bristleback);
            if (bristleback != null && bristleback.Level != 0)
            {
                var brist = bristleback.Owner as Hero;
                if (brist.FindRotationAngle(Owner.Position) > 1.90f)
                {
                    value -= bristleback.GetAbilitySpecialData("back_damage_reduction") / 100f;
                }
                else if (brist.FindRotationAngle(Owner.Position) > 1.20f)
                {
                    value -= bristleback.GetAbilitySpecialData("side_damage_reduction") / 100f;
                }
            }

            // Modifier Centaur Stampede
            if (target.HasModifier("modifier_centaur_stampede"))
            {
                var centaur = heroes.FirstOrDefault(x => x.IsEnemy(Owner) && x.HeroId == HeroId.npc_dota_hero_centaur);
                if (centaur.HasAghanimsScepter())
                {
                    var ability = centaur.GetAbilityById(AbilityId.centaur_stampede);

                    value -= ability.GetAbilitySpecialData("damage_reduction") / 100f;
                }
            }

            // Modifier Kunkka Ghostship
            if (target.HasModifier("modifier_kunkka_ghost_ship_damage_absorb"))
            {
                var kunkka = heroes.FirstOrDefault(x => x.IsEnemy(Owner) && x.HeroId == HeroId.npc_dota_hero_kunkka);
                var ability = kunkka.GetAbilityById(AbilityId.kunkka_ghostship);

                value -= ability.GetAbilitySpecialData("ghostship_absorb") / 100f;
            }

            // Modifier Wisp Overcharge
            if (target.HasModifier("modifier_wisp_overcharge"))
            {
                var wisp = heroes.FirstOrDefault(x => x.IsEnemy(Owner) && x.HeroId == HeroId.npc_dota_hero_wisp);
                var ability = wisp.GetAbilityById(AbilityId.wisp_overcharge);

                value += ability.GetAbilitySpecialData("bonus_damage_pct") / 100f;
            }

            // Modifier Bloodseeker Bloodrage
            if (target.HasModifier("modifier_bloodseeker_bloodrage") || Owner.HasModifier("modifier_bloodseeker_bloodrage"))
            {
                var bloodseeker = heroes.FirstOrDefault(x => x.HeroId == HeroId.npc_dota_hero_bloodseeker);
                var ability = bloodseeker.GetAbilityById(AbilityId.bloodseeker_bloodrage);

                value += ability.GetAbilitySpecialData("damage_increase_pct") / 100f;
            }

            // Modifier Medusa Mana Shield
            if (target.HasModifier("modifier_medusa_mana_shield"))
            {
                var ability = target.GetAbilityById(AbilityId.medusa_mana_shield);

                if (target.Mana >= 50)
                {
                    value -= ability.GetAbilitySpecialData("absorption_tooltip") / 100f;
                }
            }

            // Modifier Ursa Enrage
            if (target.HasModifier("modifier_ursa_enrage"))
            {
                var ability = target.GetAbilityById(AbilityId.ursa_enrage);
                value -= ability.GetAbilitySpecialData("damage_reduction") / 100f;
            }

            // Modifier Chen Penitence
            if (target.HasModifier("modifier_chen_penitence"))
            {
                var chen = heroes.FirstOrDefault(x => x.IsAlly(Owner) && x.HeroId == HeroId.npc_dota_hero_chen);
                var ability = chen.GetAbilityById(AbilityId.chen_penitence);

                value += ability.GetAbilitySpecialData("bonus_damage_taken") / 100f;
            }

            // Modifier Shadow Demon Soul Catcher
            if (target.HasModifier("modifier_shadow_demon_soul_catcher"))
            {
                var shadowDemon = heroes.FirstOrDefault(x => x.IsAlly(Owner) && x.HeroId == HeroId.npc_dota_hero_shadow_demon);
                var ability = shadowDemon.GetAbilityById(AbilityId.shadow_demon_soul_catcher);

                value += ability.GetAbilitySpecialData("bonus_damage_taken") / 100f;
            }

            // Modifier Pangolier Shield Crash
            var shieldCrash = target.Modifiers.FirstOrDefault(x => x.Name == "modifier_pangolier_shield_crash_buff");
            if (shieldCrash != null)
            {
                value -= shieldCrash.StackCount / 100f;
            }

            return value;
        }

        private float DamageBlock(Hero hero, List<Hero> heroes)
        {
            var value = 0.0f;

            // Modifier Abaddon Aphotic Shield
            if (hero.HasModifier("modifier_abaddon_aphotic_shield"))
            {
                var abaddon = heroes.FirstOrDefault(x => x.IsEnemy(Owner) && x.HeroId == HeroId.npc_dota_hero_abaddon);
                var ability = abaddon.GetAbilityById(AbilityId.abaddon_aphotic_shield);

                value -= ability.GetAbilitySpecialData("damage_absorb");

                var talent = abaddon.GetAbilityById(AbilityId.special_bonus_unique_abaddon);
                if (talent != null && talent.Level > 0)
                {
                    value -= talent.GetAbilitySpecialData("value");
                }
            }

            return value;
        }

        private float MagicalDamageBlock(Hero target, List<Hero> heroes)
        {
            var value = 0.0f;

            // Modifier Hood Of Defiance Barrier
            if (target.HasModifier("modifier_item_hood_of_defiance_barrier"))
            {
                var item = target.GetItemById(AbilityId.item_hood_of_defiance);
                if (item != null)
                {
                    value -= item.GetAbilitySpecialData("barrier_block");
                }
            }

            // Modifier Pipe Barrier
            if (target.HasModifier("modifier_item_pipe_barrier"))
            {
                var pipehero = heroes.FirstOrDefault(x => x.IsEnemy(Owner) && x.Inventory.Items.Any(v => v.Id == AbilityId.item_pipe));
                if (pipehero != null)
                {
                    var ability = pipehero.GetItemById(AbilityId.item_pipe);

                    value -= ability.GetAbilitySpecialData("barrier_block");
                }
            }

            // Modifier Infused Raindrop
            if (target.HasModifier("modifier_item_infused_raindrop"))
            {
                var item = target.GetItemById(AbilityId.item_infused_raindrop);
                if (item != null && item.Cooldown <= 0)
                {
                    value -= item.GetAbilitySpecialData("magic_damage_block");
                }
            }

            // Modifier Ember Spirit Flame Guard
            if (target.HasModifier("modifier_ember_spirit_flame_guard"))
            {
                var ability = target.GetAbilityById(AbilityId.ember_spirit_flame_guard);
                if (ability != null)
                {
                    value -= ability.GetAbilitySpecialData("absorb_amount");

                    var emberSpirit = ability.Owner as Hero;
                    var talent = emberSpirit.GetAbilityById(AbilityId.special_bonus_unique_ember_spirit_1);
                    if (talent != null && talent.Level > 0)
                    {
                        value -= talent.GetAbilitySpecialData("value");
                    }
                }
            }

            return value + DamageBlock(target, heroes);
        }

        public List<Damage> DamageList { get; } = new List<Damage>();

        public class Damage
        {
            public Hero GetTarget { get; }

            public float GetDamage { get; }

            public uint GetHealth { get; }

            public Damage(Hero target, float damage, uint health)
            {
                GetTarget = target;
                GetDamage = damage;
                GetHealth = health;
            }
        }
    }  
}
