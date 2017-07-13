using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;


namespace T7_Kled
{
    class Base
    {     
        public static AIHeroClient myhero { get { return ObjectManager.Player; } }
        public static Menu menu, combo, harass, laneclear, jungleclear, misc, draw, pred;
        
        public static readonly string ChampionName = "Kled";
        public static readonly string Version = "1.1";
        public static readonly string Date = "13/7/2017";
        public static Item tiamat { get; set; }
        public static Item rhydra { get; set; }
        public static Item thydra { get; set; }
        public static Item cutl { get; set; }
        public static Item blade { get; set; }
        public static Item yomus { get; set; }
        public static Item Potion { get; set; }
        public static Item Biscuit { get; set; }
        public static Item RPotion { get; set; }

        public static Spell.Skillshot Q1 { get; set; }
        public static Spell.Skillshot Q2 { get; set; }
        public static Spell.Skillshot E { get; set; }
        public static Spell.Targeted Ignite { get; set; }



        #region Methods
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

        public static void QCast(AIHeroClient target)
        {
            if (HasMount())
            {
                var qpred = Q1.GetPrediction(target);

                if (qpred.CollisionObjects.Where(x => x is AIHeroClient).Count() == 0 && qpred.HitChancePercent >= slider(pred, "Q1Pred"))
                {
                    Q1.Cast(qpred.CastPosition);
                }

            }
            else
            {
                var qpred = Q2.GetPrediction(target);

                if (!qpred.Collision && qpred.HitChancePercent >= slider(pred, "Q2Pred"))
                {
                    Q2.Cast(qpred.CastPosition);
                }
            }
        }

        public static float QDamage(AIHeroClient target)
        {
            int index = myhero.Spellbook.GetSpell(SpellSlot.Q).Level - 1;

            var Q1Damage = (new[] { 25, 50, 75, 100, 125 }[index] * (0.6f * myhero.TotalAttackDamage)) +
                           (new[] { 50, 100, 150, 200, 250 }[index] * (1.2f * myhero.TotalAttackDamage));

            var Q2Damage = new[] { 30, 45, 60, 75, 90 }[index] * (0.8f * myhero.TotalAttackDamage);

            return myhero.CalculateDamageOnUnit(target, DamageType.Physical, HasMount() ? Q1Damage : Q2Damage);
        }

        public static bool HasMount()
        {
            return myhero.GetAutoAttackRange() > 150;
        }

        public static void ItemManager(AIHeroClient target)
        {
            if (target != null && target.IsValidTarget() && check(combo, "ITEMS"))
            {
                if (tiamat.IsOwned() && tiamat.IsReady() && tiamat.IsInRange(target.Position) && tiamat.Cast())
                    return; 

                if (rhydra.IsOwned() && rhydra.IsReady() && rhydra.IsInRange(target.Position) && rhydra.Cast())
                    return; 

                if (thydra.IsOwned() && thydra.IsReady() && target.Distance(myhero.Position) < Player.Instance.GetAutoAttackRange() && !Orbwalker.CanAutoAttack &&
                    thydra.Cast())
                    return;

                if (cutl.IsOwned() && cutl.IsReady() && cutl.IsInRange(target.Position) && cutl.Cast(target))
                    return; 

                if (blade.IsOwned() && blade.IsReady() && blade.IsInRange(target.Position) && blade.Cast(target))
                    return;

                if (yomus.IsOwned() && yomus.IsReady() && target.Distance(myhero.Position) < 1000 && yomus.Cast())
                    return;
            }
        }
        #endregion
    }

    static class Extensions
    {
        public static bool ValidTarget(this AIHeroClient hero, uint range)
        {
            return !hero.HasBuff("UndyingRage") && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("ChronoShift") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("bansheesveil") && !hero.HasBuff("fioraw") &&
                   !hero.IsInvulnerable && !hero.IsDead && hero.IsValidTarget(range) && !hero.IsZombie &&
                   !hero.HasBuffOfType(BuffType.Invulnerability) && !hero.HasBuffOfType(BuffType.SpellImmunity) && !hero.HasBuffOfType(BuffType.SpellShield);
        }
    }
}
