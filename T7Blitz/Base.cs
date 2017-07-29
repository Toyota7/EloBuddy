using System;
using System.Linq;
using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace T7_Blitzcrank
{
    class Base
    {
        #region Declerations

        public static AIHeroClient myhero { get { return ObjectManager.Player; } }
        public static AIHeroClient EnemyADC { get; set; }

        public static Menu menu, combo, harass, laneclear, jungleclear, misc, draw, qsett;

        public static readonly string ChampionName = "Blitzcrank", Version = "1.1", Date = "29/7/17", QTargetBuffName = "rocketgrab2", ESelfBuffName = "PowerFist";
        public static readonly string[] ADCNames = new string[] { "Ashe","Caitlyn","Corki","Draven","Ezreal","Graves","Jhin","Jinx","Kalista","Kog'Maw","Lucian",
                                                                  "Miss Fortune","Quinn","Sivir","Tristana","Twitch","Urgot","Varus","Vayne" };
        public static string[] EnemyPlayerNames;

        public static Item Potion { get; set; }
        public static Item Biscuit { get; set; }
        public static Item RPotion { get; set; }

        public static Spell.Skillshot Q { get; set; }
        public static Spell.Active W { get; set; }
        public static Spell.Active E { get; set; }
        public static Spell.Active R { get; set; }

        #endregion

        #region Methods

        public static void CheckPrediction()
        {
            string CorrectPrediction = "SDK Prediction";

            if (Prediction.Manager.PredictionSelected != CorrectPrediction)
            {
                Prediction.Manager.PredictionSelected = CorrectPrediction;
                Console.WriteLine("SupportAIO: Prediction Has Been Changed");
            }
        }

        public static void KnockupTarget()
        {
            var target = EntityManager.Heroes.Enemies.Where(x => x.Distance(myhero.Position) < 300).FirstOrDefault();

            if (target == null) return;

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && myhero.HasPowerFist())
            {
                Orbwalker.DisableMovement = false;
                Orbwalker.DisableAttacking = false;

                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.None) || !myhero.HasPowerFist())
            {
                Orbwalker.DisableMovement = true;
                Orbwalker.DisableAttacking = true;
            }
        }

        public static AIHeroClient GetEnemyADC()
        {
            foreach (var name in EntityManager.Heroes.Enemies.Select(x => x.ChampionName))
            {
                if (ADCNames.Contains(name)) return EntityManager.Heroes.Enemies.FirstOrDefault(x => x.ChampionName == name);
            }

            return null;
        }

        public static AIHeroClient GetTarget()
        {
            var selection = comb(misc, "FOCUS");


            switch (selection)
            {
                case 0:
                    if (EnemyADC != null && EnemyADC.ValidTarget((int)Q.Range + 250))
                    {
                        return EnemyADC;
                    }
                    else return TargetSelector.GetTarget(Q.Range + 100, DamageType.Magical, Player.Instance.Position);
                case 1:
                    return TargetSelector.GetTarget(Q.Range + 100, DamageType.Magical, Player.Instance.Position);
                case 2:
                    var target = EntityManager.Heroes.Enemies.FirstOrDefault(x => x.ChampionName == EnemyPlayerNames[comb(misc, "CFOCUS")]);

                    if (target != null && target.ValidTarget((int)Q.Range + 250))
                    {
                        return target;
                    }
                    else return TargetSelector.GetTarget(Q.Range + 100, DamageType.Magical, Player.Instance.Position);
            }

            return null;
        }

        public static bool check(Menu submenu, string sig)
        {
            return submenu[sig].Cast<CheckBox>().CurrentValue;
        }

        public static int slider(Menu submenu, string sig)
        {
            return submenu[sig].Cast<Slider>().CurrentValue;
        }

        public static int comb(Menu submenu, string sig)
        {
            return submenu[sig].Cast<ComboBox>().CurrentValue;
        }

        public static bool key(Menu submenu, string sig)
        {
            return submenu[sig].Cast<KeyBind>().CurrentValue;
        }
        #endregion
    }
}
