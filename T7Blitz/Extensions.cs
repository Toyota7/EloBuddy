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
    static class Extensions
    {
        public static bool ValidTarget(this AIHeroClient hero, int range)
        {
            return !hero.HasBuff("UndyingRage") && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("ChronoShift") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("bansheesveil") && !hero.HasBuff("fioraw") &&
                   !hero.IsInvulnerable && !hero.IsDead && hero.IsValidTarget(range) && !hero.IsZombie &&
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

        public static bool HasPowerFist(this AIHeroClient hero)
        {
            return hero.HasBuff(Base.ESelfBuffName);
        }

        public static Vector3 GetPositionAfter(this Obj_AI_Base target, int milliseconds = 250)
        {
            return Prediction.Position.PredictUnitPosition(target, milliseconds).To3D();
        }
    }
}
