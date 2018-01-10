﻿using System;
using System.Windows.Input;

using Ensage;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;

using SharpDX;

using SkywrathMagePlus.Features;

namespace SkywrathMagePlus
{
    internal class Config : IDisposable
    {
        public SkywrathMagePlus Main { get; }

        public Vector2 Screen { get; }

        public MultiSleeper MultiSleeper { get; }

        public MenuManager Menu { get; }

        public Abilities Abilities { get; }

        public Extensions Extensions { get; }

        public UpdateMode UpdateMode { get; }

        public DamageCalculation DamageCalculation { get; }

        public LinkenBreaker LinkenBreaker { get; }

        private AutoKillSteal AutoKillSteal { get; }

        private AutoCombo AutoCombo { get; }

        private SpamMode SpamMode { get; }

        private AutoDisable AutoDisable { get; }

        private WithoutFail WithoutFail { get; }

        private AutoAbility AutoAbility { get; }

        private Mode Mode { get; }

        private Renderer Renderer { get; }

        private bool Disposed { get; set; }

        public Config(SkywrathMagePlus main)
        {
            Main = main;

            ActivatePlugins();
            Screen = new Vector2(Drawing.Width - 160, Drawing.Height);
            MultiSleeper = new MultiSleeper();

            Menu = new MenuManager(this);

            Abilities = new Abilities(this);
            Extensions = new Extensions(this);

            UpdateMode = new UpdateMode(this);
            DamageCalculation = new DamageCalculation(this);
            LinkenBreaker = new LinkenBreaker(this);
            AutoKillSteal = new AutoKillSteal(this);
            AutoCombo = new AutoCombo(this);
            SpamMode = new SpamMode(this);
            AutoDisable = new AutoDisable(this);
            WithoutFail = new WithoutFail(this);
            AutoAbility = new AutoAbility(this);

            Menu.ComboKeyItem.Item.ValueChanged += ComboKeyChanged;
            var key = KeyInterop.KeyFromVirtualKey((int)Menu.ComboKeyItem.Value.Key);
            Mode = new Mode(key, this);
            Main.Context.Orbwalker.RegisterMode(Mode);

            Renderer = new Renderer(this);
        }

        private void ActivatePlugins()
        {
            var orbwalker = Main.Context.Orbwalker;
            if (!orbwalker.IsActive)
            {
                orbwalker.Activate();
            }

            var targetSelector = Main.Context.TargetSelector;
            if (!targetSelector.IsActive)
            {
                targetSelector.Activate();
            }

            var prediction = Main.Context.Prediction;
            if (!prediction.IsActive)
            {
                prediction.Activate();
            }
        }

        private void ComboKeyChanged(object sender, OnValueChangeEventArgs e)
        {
            var keyCode = e.GetNewValue<KeyBind>().Key;
            if (keyCode == e.GetOldValue<KeyBind>().Key)
            {
                return;
            }

            var key = KeyInterop.KeyFromVirtualKey((int)keyCode);
            Mode.Key = key;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                Renderer.Dispose();

                Main.Context.Orbwalker.UnregisterMode(Mode);
                Menu.ComboKeyItem.Item.ValueChanged -= ComboKeyChanged;

                AutoAbility.Dispose();
                WithoutFail.Dispose();
                AutoDisable.Dispose();
                SpamMode.Dispose();
                AutoCombo.Dispose();
                AutoKillSteal.Dispose();
                DamageCalculation.Dispose();
                UpdateMode.Dispose();

                Main.Context.Particle.Dispose();

                Abilities.Dispose();

                Menu.Dispose();
            }

            Disposed = true;
        }
    }
}
