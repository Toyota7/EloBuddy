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

namespace T7_Warwick
{
    class Base
    {
        #region Declerations

        public static AIHeroClient myhero { get { return ObjectManager.Player; } }

        public static Menu menu, combo, harass, laneclear, jungleclear, misc, draw;

        public static readonly string ChampionName = "Warwick", Version = "1.0", Date = "3/8/17", EBuffName = "WarwickE";
        public static readonly string[] BigMonsterNames = { "Baron", "Blue", "Dragon", "Red", "Gromp", "Krug" };

        public static float PreviousSpeed = 0;

        public static Item Potion { get; set; }
        public static Item Biscuit { get; set; }
        public static Item RPotion { get; set; }
        public static Item tiamat { get; set; }
        public static Item rhydra { get; set; }
        public static Item thydra { get; set; }

        public static Spell.Chargeable Q { get; set; }
        public static Spell.Active W { get; set; }
        public static Spell.Active E { get; set; }
        public static Spell.Skillshot R { get; set; }
        public static Spell.Targeted Smite { get; set; }
        public static Spell.Targeted Ignite { get; set; }

        #endregion

        #region Methods   
        public static void AutoSmite()
        {
            
        }

        public static void ItemManager()
        {
            if (tiamat.IsOwned() && tiamat.IsReady() && myhero.CountEnemies((int)tiamat.Range) > 0)
            {
                tiamat.Cast();
            }

            if (rhydra.IsOwned() && rhydra.IsReady() && myhero.CountEnemies((int)rhydra.Range) > 0)
            {
                rhydra.Cast();
            }

            if (thydra.IsOwned() && thydra.IsReady() && /*target.Distance(myhero.Position) < Player.Instance.GetAutoAttackRange()*/ myhero.CountEnemies((int)myhero.GetAutoAttackRange()) > 0 && Orbwalker.IsAutoAttacking)
            {
                thydra.Cast();
            }
        }
           
        public static float AvgComboDamage(Obj_AI_Base target)
        {
            var SmiteDmg = Smite != null ? (Smite.IsReady() ? myhero.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Smite) : 0) : 0;
            var ItemDmg = tiamat.IsOwned() || rhydra.IsOwned() ? (tiamat.IsReady() || rhydra.IsReady() ? myhero.TotalAttackDamage * 0.8f : 0) : 0; // 0.6f + 1f / 2 = 0.8f (avg distance) 

            return (Q.IsReady() ? GetQDamage(target) : 0) + (R.IsReady() ? R.GetSpellDamage(target) : 0) + (myhero.GetAutoAttackDamage(target) * 5) + SmiteDmg + ItemDmg;
        }                                                                                                   //minimum amount of aa's in a fight?

        public static float GetQDamage(Obj_AI_Base target)
        {
            var damage = (myhero.TotalAttackDamage * 1.2f) + (myhero.TotalMagicalDamage * 0.9f) + (new float[] { 0.06f, 0.07f, 0.08f, 0.09f, 0.1f}[Q.Level - 1] * target.MaxHealth);

            return myhero.CalculateDamageOnUnit(target, DamageType.Magical, damage, true, true);
        }

        public static void AdjustRange()
        {
            if ((float)R.Range != PreviousSpeed * 2f)
            {
                R = new Spell.Skillshot(SpellSlot.R, (uint)myhero.MoveSpeed * 2, SkillShotType.Linear, 200, 2150, 80);
                PreviousSpeed = myhero.MoveSpeed;
            }
        }

        public static void CheckPrediction()
        {
            string CorrectPrediction = "SDK Prediction";

            if (Prediction.Manager.PredictionSelected != CorrectPrediction)
            {
                Prediction.Manager.PredictionSelected = CorrectPrediction;
                Console.WriteLine("SupportAIO: Prediction Has Been Changed");
            }
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

    static class Extensions
    {
        public static bool ValidTarget(this AIHeroClient hero, uint SpellRange)
        {
            return !hero.HasBuff("UndyingRage") && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("ChronoShift") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("bansheesveil") && !hero.HasBuff("fioraw") &&
                   !hero.IsInvulnerable && !hero.IsDead && hero.IsValidTarget(SpellRange) && !hero.IsZombie &&
                   !hero.HasBuffOfType(BuffType.Invulnerability) && !hero.HasBuffOfType(BuffType.SpellImmunity) && !hero.HasBuffOfType(BuffType.SpellShield);
        }

        public static int CountEnemies(this AIHeroClient hero, int range)
        {
            return EntityManager.Heroes.Enemies.Where(x => x.Distance(hero.Position) < range).Count();
        }

        public static int CountAllies(this Obj_AI_Base hero, int range)
        {
            return EntityManager.Heroes.Allies.Where(x => x.Distance(hero.Position) < range).Count();
        }
        public static int CountMinions(this Obj_AI_Base hero, float range)
        {
            return EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, hero.Position, range).Count();
        }

        public static bool IsEActive(this Obj_AI_Base hero)
        {
            return hero.HasBuff(Base.EBuffName);
        }

        public static Vector3 GetPositionAfter(this Obj_AI_Base target, int milliseconds = 250)
        {
            return Prediction.Position.PredictUnitPosition(target, milliseconds).To3D();
        }
        public static float GetHealthAfter(this Obj_AI_Base target, int milliseconds = 250)
        {
            return Prediction.Health.GetPrediction(target, milliseconds);
        }

    }
}
