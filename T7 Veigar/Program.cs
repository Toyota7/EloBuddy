using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace Veigarino
{
    class ΤοΠιλλ
    {
        private static void Main(string[] args) { Loading.OnLoadingComplete += OnLoad; }
        public static AIHeroClient myhero { get { return ObjectManager.Player; } }
        private static Menu menu, combo, harass, laneclear, misc, draw, pred, sequence1, jungleclear;
        public static Spell.Targeted ignt = new Spell.Targeted(myhero.GetSpellSlotFromName("summonerdot"), 600);
        public static Item potion { get; private set; }
        public static Item Biscuit { get; private set; }
        public static void OnLoad(EventArgs arg)
        {
            //   DemSpells.Q.AllowedCollisionCount = 1;
            if (Player.Instance.ChampionName != "Veigar") { return; }
            Chat.Print("<font color='#0040FF'>T7</font><font color='#A901DB'> Veigar</font> : Loaded!(v1.7b)");
            Chat.Print("<font color='#04B404'>By </font><font color='#FF0000'>T</font><font color='#FA5858'>o</font><font color='#FF0000'>y</font><font color='#FA5858'>o</font><font color='#FF0000'>t</font><font color='#FA5858'>a</font><font color='#0040FF'>7</font><font color='#FF0000'> <3 </font>");
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnLevelUp += OnLvlUp;
            Gapcloser.OnGapcloser += OnGapcloser;
            DatMenu();
            Game.OnTick += OnTick;
            Player.LevelSpell(SpellSlot.Q);
            potion = new Item((int)ItemId.Health_Potion);
            Biscuit = new Item((int)ItemId.Total_Biscuit_of_Rejuvenation);
        }
        private static void OnLvlUp(Obj_AI_Base guy, Obj_AI_BaseLevelUpEventArgs args)
        {
            if (!guy.IsMe) return;
            /*Q>W>E*/
            SpellSlot[] sequence1 = { SpellSlot.Unknown, SpellSlot.E, SpellSlot.W, SpellSlot.Q, SpellSlot.Q, SpellSlot.R, SpellSlot.Q, SpellSlot.W, SpellSlot.Q, SpellSlot.E, SpellSlot.R, SpellSlot.W, SpellSlot.E, SpellSlot.W, SpellSlot.W, SpellSlot.R, SpellSlot.E, SpellSlot.E };

            if (misc["autoS"].Cast<CheckBox>().CurrentValue) Player.LevelSpell(sequence1[myhero.Level]);
        }
        private static void OnTick(EventArgs args)
        {
            if (myhero.IsDead) return;

            var flags = Orbwalker.ActiveModesFlags;

            if (flags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }

            if (flags.HasFlag(Orbwalker.ActiveModes.Harass) || check(harass, "autoH") && myhero.ManaPercent > slider(harass, "minMH"))
            {
                Harass();
            }
            if (flags.HasFlag(Orbwalker.ActiveModes.LaneClear) || check(laneclear, "AutoL") && slider(laneclear, "LcM") <= myhero.ManaPercent)
            {
                Laneclear();
            }

            if (flags.HasFlag(Orbwalker.ActiveModes.JungleClear) || check(laneclear, "AutoL") && slider(laneclear, "LcM") <= myhero.ManaPercent)
            {
                Laneclear();
            }

            if (flags.HasFlag(Orbwalker.ActiveModes.JungleClear) && myhero.ManaPercent > slider(jungleclear, "JMIN"))
            {
                Jungleclear();
            }

            if (laneclear["Qlk"].Cast<KeyBind>().CurrentValue && slider(laneclear, "LcM") <= myhero.ManaPercent) QStack();

            Misc();
        }
        private static float ComboDMG(AIHeroClient target)
        {
            if (target != null)
            {
                float cdmg = 0;

                if (DemSpells.Q.IsReady() && combo["useQ"].Cast<CheckBox>().CurrentValue) { cdmg += QDamage(target); }
                if (DemSpells.W.IsReady() && combo["useW"].Cast<CheckBox>().CurrentValue) { cdmg += WDamage(target); }
                if (DemSpells.R.IsReady() && combo["useR"].Cast<CheckBox>().CurrentValue) { cdmg += UltDamage(target); }

                return cdmg;
            }
            return 0;
        }
        private static int comb(Menu submenu, string sig)
        {
            return submenu[sig].Cast<ComboBox>().CurrentValue;
        }
        private static bool check(Menu submenu, string sig)
        {
            return submenu[sig].Cast<CheckBox>().CurrentValue;
        }
        private static int slider(Menu submenu, string sig)
        {
            return submenu[sig].Cast<Slider>().CurrentValue;
        }
        private static float QDamage(Obj_AI_Base target)
        {
            var index = DemSpells.Q.Level - 1;

            var QDamage = new[] { 70, 110, 150, 190, 230 }[index] +
                          (0.6f * myhero.FlatMagicDamageMod);

            return myhero.CalculateDamageOnUnit(target, DamageType.Magical, (float)QDamage);
        }
        private static float WDamage(AIHeroClient target)
        {
            var index = DemSpells.W.Level - 1;

            var WDamage = new[] { 100, 150, 200, 250, 300 }[index] + myhero.FlatMagicDamageMod;

            return myhero.CalculateDamageOnUnit(target, DamageType.Magical, (float)WDamage);
        }
        private static float WDamage(Obj_AI_Base monster)
        {
            var index = DemSpells.W.Level - 1;

            var WDamage = new[] { 100, 150, 200, 250, 300 }[index] + myhero.FlatMagicDamageMod;

            return myhero.CalculateDamageOnUnit(monster, DamageType.Magical, (float)WDamage);
        }
        private static float UltDamage(AIHeroClient target)
        {
            var level = DemSpells.R.Level;

            var damage = new float[] { 0, 175, 250, 325 }[level] + (((100 - target.HealthPercent) * 1.5) / 100) * new float[] { 0, 175, 250, 325 }[level] +
                0.75 * myhero.FlatMagicDamageMod;
            return myhero.CalculateDamageOnUnit(target, DamageType.Magical, (float)damage);
        }

        public static void Harass()
        {
            var target = TargetSelector.GetTarget(1000, DamageType.Magical, Player.Instance.Position);

            if (target != null)
            {
                var Qpred = DemSpells.Q.GetPrediction(target);
                var Wpred = DemSpells.W.GetPrediction(target);

                if (check(combo, "useQ") && DemSpells.Q.IsReady() && target.IsValidTarget(DemSpells.Q.Range) && !target.IsZombie
                    && !target.IsInvulnerable)
                {
                    if (!Qpred.Collision || Qpred.CollisionObjects.Count() < 2)
                    {
                        DemSpells.Q.Cast(Qpred.CastPosition);
                    }
                }

                if (check(combo, "useW") && DemSpells.W.IsReady() && target.IsValidTarget(DemSpells.W.Range) && !target.IsZombie
                    && !target.IsInvulnerable)
                {
                    switch (comb(harass, "HWMODE"))
                    {
                        case 0:
                            if (Wpred.HitChancePercent >= slider(pred, "Whit") || Wpred.HitChance == HitChance.Immobile ||
                               (target.HasBuffOfType(BuffType.Slow) && Wpred.HitChance == HitChance.High))
                            {
                                DemSpells.W.Cast(Wpred.CastPosition);
                            }
                            break;
                        case 1:
                            DemSpells.W.Cast(target.Position);
                            break;
                        case 2:
                            if (target.HasBuffOfType(BuffType.Stun))
                            {
                                DemSpells.W.Cast(target.Position);
                            }
                            else if (Wpred.HitChance == HitChance.Immobile)
                            {
                                DemSpells.W.Cast(Wpred.CastPosition);
                            }
                            break;
                    }

                }
            }
        }
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(1000, DamageType.Magical, Player.Instance.Position);

            if (target != null)
            {
                var Qpred = DemSpells.Q.GetPrediction(target);
                var Wpred = DemSpells.W.GetPrediction(target);
                var Epred = Prediction.Position.PredictUnitPosition(target, 500);
                var Epred2 = DemSpells.E.GetPrediction(target);

                if (check(combo, "useQ") && DemSpells.Q.IsReady() && target.IsValidTarget(DemSpells.Q.Range) && !target.IsZombie
                    && !target.IsInvulnerable)
                {
                    if (!Qpred.Collision || Qpred.CollisionObjects.Count() < 2)
                    {
                        DemSpells.Q.Cast(Qpred.CastPosition);
                    }
                }

                if (check(combo, "useE") && DemSpells.E.IsReady() && target.IsValidTarget(DemSpells.E.Range - 30) && !target.IsZombie
                    && !target.IsInvulnerable)
                {
                    if (check(combo, "Es") && target.HasBuffOfType(BuffType.Stun)) return;
                    switch (comb(combo, "CEMODE"))
                    {
                        case 0:
                            if (Epred2.HitChancePercent >= slider(pred, "Ehit")) DemSpells.E.Cast(Epred2.CastPosition);
                            break;
                        case 1:
                            if (Epred.Distance(myhero.Position) < DemSpells.E.Range)
                            {
                                switch (target.IsFleeing)
                                {
                                    case true:
                                        DemSpells.E.Cast(Epred.To3D().Shorten(myhero.Position, 182));
                                        break;
                                    case false:
                                        DemSpells.E.Cast(Epred.Extend(myhero.Position, 182).To3D());
                                        break;
                                }
                            }
                            break;
                        case 2:
                            if (myhero.CountEnemiesInRange(DemSpells.E.Range + (DemSpells.E.Radius / 2)) >= slider(combo, "CEAOE"))
                            {
                                foreach (var enemy in EntityManager.Heroes.Enemies.Where(x => !x.IsDead && x.IsValid && !x.IsAlly &&
                                                                                         x.Distance(myhero.Position) <= DemSpells.E.Range))
                                {
                                    if (enemy.CountEnemiesInRange(175) >= slider(combo, "CEAOE")) DemSpells.E.Cast(enemy.Position);
                                }
                            }
                            break;
                    }
                }

                if (check(combo, "useW") && DemSpells.W.IsReady() && target.IsValidTarget(DemSpells.W.Range) && !target.IsZombie
                    && !target.IsInvulnerable)
                {
                    switch (comb(combo, "CWMODE"))
                    {
                        case 0:
                            if (Wpred.HitChancePercent >= slider(pred, "Whit") || Wpred.HitChance == HitChance.Immobile ||
                               (target.HasBuffOfType(BuffType.Slow) && Wpred.HitChance == HitChance.High))
                            {
                                DemSpells.W.Cast(Wpred.CastPosition);
                            }
                            break;
                        case 1:
                            DemSpells.W.Cast(target.Position);
                            break;
                        case 2:
                            if (target.HasBuffOfType(BuffType.Stun))
                            {
                                DemSpells.W.Cast(target.Position);
                            }
                            else if (Wpred.HitChance == HitChance.Immobile)
                            {
                                DemSpells.W.Cast(Wpred.CastPosition);
                            }
                            break;
                    }
                }

                if (check(combo, "useR") && DemSpells.R.IsReady() &&
                    DemSpells.R.IsInRange(target.Position) && ComboDMG(target) > target.Health &&
                    UltDamage(target) > target.Health && !target.HasBuffOfType(BuffType.SpellImmunity) && !target.IsInvulnerable && !target.HasUndyingBuff())
                {
                    if ((float)(ComboDMG(target) - UltDamage(target)) > target.Health) return;
                    DemSpells.R.Cast(target);
                }

                if (check(combo, "igntC") && ignt.IsReady() && ComboDMG(target) < target.Health &&
                    ignt.IsInRange(target.Position) && myhero.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite) > target.Health &&
                    !misc["autoign"].Cast<CheckBox>().CurrentValue && !target.HasUndyingBuff()) ignt.Cast(target);
            }
        }
        private static void QStack()
        {
            if (!DemSpells.Q.IsReady() || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) return;

            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, myhero.Position, DemSpells.Q.Range)
                                                          .OrderBy(x => x.Distance(myhero.Position));

            if (minions != null)
            {
                foreach (var minion in minions.Where(x => !x.IsDead && x.IsValidTarget(DemSpells.Q.Range - 10) && x.Health < QDamage(x) - 10 &&
                                                     Prediction.Health.GetPrediction(x, (int)(((x.Distance(myhero.Position) / DemSpells.Q.Speed) * 1000) + 250)) > 30))
                {
                    var pred = DemSpells.Q.GetPrediction(minion);
                    var collisions = pred.CollisionObjects.ToList();

                    switch (comb(laneclear, "Qlm"))
                    {
                        case 0:
                            if (pred.Collision && collisions.Count() <= 1)
                            {
                                DemSpells.Q.Cast(pred.CastPosition);

                            }
                            else
                            {
                                DemSpells.Q.Cast(pred.CastPosition);
                            }
                            break;
                        case 1:
                            if (pred.Collision && (collisions.Count() == 1 &&
                                collisions.FirstOrDefault().Health < QDamage(collisions.FirstOrDefault()) - 10))
                            {
                                DemSpells.Q.Cast(pred.CastPosition);
                            }
                            else if (collisions.Count() == 2 && collisions[0].Health < QDamage(collisions[0]) - 10 &&
                                                               collisions[1].Health < QDamage(collisions[1]) - 10)
                            {
                                DemSpells.Q.Cast(pred.CastPosition);
                            }
                            break;
                    }
                }
            }
        }
        private static void Laneclear()
        {
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, myhero.Position, DemSpells.W.Range);

            if (check(laneclear, "LQ") && DemSpells.Q.IsReady() && !laneclear["Qlk"].Cast<KeyBind>().CurrentValue)
            {
                var Qpred = EntityManager.MinionsAndMonsters.GetLineFarmLocation(minion, DemSpells.Q.Width, (int)DemSpells.Q.Range);

                if (Qpred.HitNumber >= 1) DemSpells.Q.Cast(Qpred.CastPosition);
            }

            if (check(laneclear, "LW") && minion != null && DemSpells.W.IsReady())
            {
                var Wpred = EntityManager.MinionsAndMonsters.GetCircularFarmLocation(minion, DemSpells.W.Width, (int)DemSpells.W.Range);

                if (Wpred.HitNumber >= slider(laneclear, "Wmm")) DemSpells.W.Cast(Wpred.CastPosition);
            }
        }

        private static void Jungleclear()
        {
            var Monsters = EntityManager.MinionsAndMonsters.GetJungleMonsters(myhero.Position, 1800f);

            if (Monsters != null)
            {

                if (check(jungleclear, "JQ") && DemSpells.Q.IsLearned && DemSpells.Q.IsReady())
                {
                    switch (comb(jungleclear, "JQMODE"))
                    {
                        case 0:
                            foreach (var monster in Monsters.Where(x => !x.IsDead && x.IsValidTarget(DemSpells.Q.Range) && x.Health > 30))
                            {
                                DemSpells.Q.Cast(monster.Position);
                            }
                            break;
                        case 1:
                            foreach (var monster in Monsters.Where(x => !x.IsDead && x.IsValidTarget(DemSpells.Q.Range) && x.Health > 30 &&
                                                                        !x.Name.ToLower().Contains("mini")))
                            {
                                var pred = DemSpells.Q.GetPrediction(monster);
                                if (pred.CollisionObjects.Count() < 2) DemSpells.Q.Cast(pred.CastPosition);
                            }
                            break;
                    }
                }

                if (check(jungleclear, "JW") && DemSpells.W.IsLearned && DemSpells.W.IsReady())
                {
                    var mobs = Monsters.Where(x => !x.IsDead && x.IsValidTarget(DemSpells.W.Range) && x.Health > 30);


                    if (mobs != null && mobs.Count() >= slider(jungleclear, "JWMIN"))
                    {
                        var pred = EntityManager.MinionsAndMonsters.GetCircularFarmLocation(mobs, DemSpells.W.Width, (int)DemSpells.W.Range);

                        if (pred.HitNumber >= slider(jungleclear, "JWMIN")) DemSpells.W.Cast(pred.CastPosition);
                    }
                }

                if (check(jungleclear, "JE") && DemSpells.E.IsLearned && DemSpells.E.IsReady())
                {
                    var mobs = Monsters.Where(x => !x.IsDead && x.IsValidTarget(DemSpells.E.Range) && x.Health > 10);


                    if (mobs != null && mobs.Count() >= slider(jungleclear, "JEMIN"))
                    {
                        var pred = EntityManager.MinionsAndMonsters.GetCircularFarmLocation(mobs, DemSpells.E.Width, (int)DemSpells.E.Range);

                        if (pred.HitNumber >= slider(jungleclear, "JEMIN")) DemSpells.E.Cast(pred.CastPosition);
                    }
                }
            }
        }
        private static void Misc()
        {
            if (check(misc, "sh"))
            {
                myhero.SetSkinId(comb(misc, "sID"));
            }

            if (check(misc, "AUTOPOT") && (!myhero.HasBuff("RegenerationPotion") || !myhero.HasBuff("ItemMiniRegenPotion")) &&
                myhero.HealthPercent <= slider(misc, "POTMIN"))
            {
                if (Item.HasItem(potion.Id) && Item.CanUseItem(potion.Id)) potion.Cast();

                else if (Item.HasItem(Biscuit.Id) && Item.CanUseItem(Biscuit.Id)) Biscuit.Cast();
            }

            if (check(misc, "KSJ") && DemSpells.W.IsLearned && DemSpells.W.IsReady())
            {
                foreach (var monster in EntityManager.MinionsAndMonsters.GetJungleMonsters().Where(x => !x.Name.ToLower().Contains("mini") && !x.IsDead &&
                                                                                                   x.Health > 200 && x.IsValidTarget(DemSpells.W.Range) &&
                                                                                                   (x.Name.ToLower().Contains("dragon") ||
                                                                                                    x.Name.ToLower().Contains("baron") ||
                                                                                                    x.Name.ToLower().Contains("herald"))))
                {
                    var pred = DemSpells.W.GetPrediction(monster);

                    if (Prediction.Health.GetPrediction(monster, DemSpells.W.CastDelay + 100) > monster.Health)
                    {
                        switch (monster.Name.ToLower().Contains("herald"))
                        {
                            case true:
                                if (pred.HitChancePercent >= 85) DemSpells.W.Cast(pred.CastPosition);
                                break;
                            case false:
                                DemSpells.W.Cast(monster.Position);
                                break;
                        }
                    }
                }
            }

            var target = TargetSelector.GetTarget(1000, DamageType.Magical, Player.Instance.Position);

            if (target != null)
            {
                var Qpred = DemSpells.Q.GetPrediction(target);
                var Wpred = DemSpells.W.GetPrediction(target);

                if (check(misc, "ksQ") && QDamage(target) > target.Health && !target.HasUndyingBuff() &&
                    target.IsValidTarget(DemSpells.Q.Range - 10) && DemSpells.Q.IsReady() && !target.IsInvulnerable)
                {
                    if (target.HasBuffOfType(BuffType.Stun) ||
                        Qpred.HitChancePercent >= slider(pred, "Qhit"))
                    {
                        DemSpells.Q.Cast(target.Position);
                    }
                }
                if (check(misc, "ksW") && WDamage(target) > target.Health && !target.HasUndyingBuff() &&
                    target.IsValidTarget(DemSpells.W.Range) && DemSpells.W.IsReady() && !target.IsInvulnerable)
                {

                    if (Wpred.HitChancePercent >= slider(pred, "Whit") || Wpred.HitChance == HitChance.Immobile ||
                        (target.HasBuffOfType(BuffType.Slow) && Wpred.HitChance == HitChance.High))
                    {
                        DemSpells.W.Cast(Wpred.CastPosition);
                    }
                }

                if (check(misc, "ksR") && UltDamage(target) > target.Health &&
                    target.IsValidTarget(DemSpells.R.Range) && DemSpells.R.IsReady() &&
                    !target.IsInvulnerable && !target.HasUndyingBuff() && !target.HasBuffOfType(BuffType.SpellImmunity))
                {
                    switch (target.HasBuffOfType(BuffType.SpellShield))
                    {
                        case true:
                            if ((target.MagicShield + target.Health) < UltDamage(target)) DemSpells.R.Cast(target);
                            else break;
                            break;
                        case false:
                            DemSpells.R.Cast(target);
                            break;
                    }
                }

                if (check(misc, "autoign") && ignt.IsReady() && target.IsValidTarget(ignt.Range) &&
                    myhero.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite) > target.Health &&
                    !target.IsInvulnerable && !target.HasUndyingBuff())
                {
                    ignt.Cast(target);
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {
            Circle.Draw(SharpDX.Color.Fuchsia, 375, myhero.Position);
            if (draw["drawQ"].Cast<CheckBox>().CurrentValue && DemSpells.Q.Level > 0 && !myhero.IsDead && !draw["nodraw"].Cast<CheckBox>().CurrentValue)
            {

                if (draw["nodrawc"].Cast<CheckBox>().CurrentValue) { Circle.Draw(DemSpells.Q.IsOnCooldown ? SharpDX.Color.Transparent : SharpDX.Color.Fuchsia, DemSpells.Q.Range, myhero.Position); }

                else if (!draw["nodrawc"].Cast<CheckBox>().CurrentValue) { Circle.Draw(SharpDX.Color.Fuchsia, DemSpells.Q.Range, myhero.Position); }

            }


            if (draw["drawW"].Cast<CheckBox>().CurrentValue && DemSpells.W.Level > 0 && !myhero.IsDead && !draw["nodraw"].Cast<CheckBox>().CurrentValue)
            {

                if (draw["nodrawc"].Cast<CheckBox>().CurrentValue) { Circle.Draw(DemSpells.W.IsOnCooldown ? SharpDX.Color.Transparent : SharpDX.Color.Fuchsia, DemSpells.W.Range, myhero.Position); }

                else if (!draw["nodrawc"].Cast<CheckBox>().CurrentValue) { Circle.Draw(SharpDX.Color.Fuchsia, DemSpells.W.Range, myhero.Position); }

            }

            if (draw["drawE"].Cast<CheckBox>().CurrentValue && DemSpells.E.Level > 0 && !myhero.IsDead && !draw["nodraw"].Cast<CheckBox>().CurrentValue)
            {

                if (draw["nodrawc"].Cast<CheckBox>().CurrentValue) { Circle.Draw(DemSpells.E.IsOnCooldown ? SharpDX.Color.Transparent : SharpDX.Color.Fuchsia, DemSpells.E.Range, myhero.Position); }

                else if (!draw["nodrawc"].Cast<CheckBox>().CurrentValue) { Circle.Draw(SharpDX.Color.Fuchsia, DemSpells.E.Range, myhero.Position); }

            }

            if (draw["drawR"].Cast<CheckBox>().CurrentValue && DemSpells.R.Level > 0 && !myhero.IsDead && !draw["nodraw"].Cast<CheckBox>().CurrentValue)
            {

                if (draw["nodrawc"].Cast<CheckBox>().CurrentValue) { Circle.Draw(DemSpells.R.IsOnCooldown ? SharpDX.Color.Transparent : SharpDX.Color.Fuchsia, DemSpells.R.Range, myhero.Position); }

                else if (!draw["nodrawc"].Cast<CheckBox>().CurrentValue) { Circle.Draw(SharpDX.Color.Fuchsia, DemSpells.R.Range, myhero.Position); }

            }

            if (draw["drawAA"].Cast<CheckBox>().CurrentValue && !myhero.IsDead && !draw["nodraw"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(SharpDX.Color.LightYellow, myhero.AttackRange, myhero.Position);
            }

            foreach (var enemy in EntityManager.Heroes.Enemies)
            {
                if (draw["drawk"].Cast<CheckBox>().CurrentValue && !draw["nodraw"].Cast<CheckBox>().CurrentValue &&
                    enemy.IsVisible && enemy.IsHPBarRendered && !enemy.IsDead && ComboDMG(enemy) > enemy.Health)
                {
                    Drawing.DrawText(Drawing.WorldToScreen(enemy.Position).X, Drawing.WorldToScreen(enemy.Position).Y - 30,
                                     Color.Green, "Killable With Combo");
                }
                else if (draw["drawk"].Cast<CheckBox>().CurrentValue && !draw["nodraw"].Cast<CheckBox>().CurrentValue &&
                         enemy.IsVisible && enemy.IsHPBarRendered && !enemy.IsDead &&
                            ComboDMG(enemy) + myhero.GetSummonerSpellDamage(enemy, DamageLibrary.SummonerSpells.Ignite) > enemy.Health)
                {
                    Drawing.DrawText(Drawing.WorldToScreen(enemy.Position).X, Drawing.WorldToScreen(enemy.Position).Y - 30,
                                     Color.Green, "Combo + Ignite");

                }
            }

            if (draw["drawStacks"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawText(Drawing.WorldToScreen(myhero.Position).X - 50, Drawing.WorldToScreen(myhero.Position).Y + 10,
                                 Color.Red, laneclear["Qlk"].Cast<KeyBind>().CurrentValue ? "Auto Stacking: ON" : "Auto Stacking: OFF");
            }

            if (draw["drawStackCount"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawText(Drawing.WorldToScreen(myhero.Position).X - 25, Drawing.WorldToScreen(myhero.Position).Y + 25,
                                 Color.Red, "Count: " + myhero.GetBuffCount("veigarphenomenalevilpower").ToString());
            }

        }

        private static void OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender != null && sender.IsEnemy && DemSpells.E.IsInRange(sender.Position) && DemSpells.E.IsReady() &&
                sender.IsValidTarget() && misc["gapmode"].Cast<ComboBox>().CurrentValue != 0)
            {
                var gpred = DemSpells.E.GetPrediction(sender);
                switch (misc["gapmode"].Cast<ComboBox>().CurrentValue)
                {
                    case 1:
                        if (!sender.IsFleeing && sender.IsFacing(myhero))
                            DemSpells.E.Cast(myhero.Position);
                        break;
                    case 2:
                        if (gpred != null && gpred.HitChancePercent >= pred["Ehit"].Cast<Slider>().CurrentValue)
                        {
                            DemSpells.E.Cast(gpred.CastPosition);
                        }
                        break;
                }
            }
        }

        private static void DatMenu()
        {

            menu = MainMenu.AddMenu("T7 Veigar", "veigarxd");
            combo = menu.AddSubMenu("Combo", "combo");
            harass = menu.AddSubMenu("Harass", "harass");
            laneclear = menu.AddSubMenu("Laneclear", "lclear");
            jungleclear = menu.AddSubMenu("Jungleclear", "jclear");
            draw = menu.AddSubMenu("Drawings", "draw");
            misc = menu.AddSubMenu("Misc", "misc");
            pred = menu.AddSubMenu("Prediction", "pred");

            menu.AddGroupLabel("Welcome to T7 Veigar And Thank You For Using!");
            menu.AddLabel("Version 1.7b 31/7/2016");
            menu.AddLabel("Author: Toyota7");
            menu.AddSeparator();
            menu.AddLabel("Please Report Any Bugs And If You Have Any Requests Feel Free To PM Me <3");

            combo.AddGroupLabel("Spells");
            combo.Add("useQ", new CheckBox("Use Q in Combo", true));
            combo.Add("useW", new CheckBox("Use W in Combo", true));
            combo.Add("useE", new CheckBox("Use E in Combo", true));
            combo.Add("useR", new CheckBox("Use R in Combo", true));
            combo.Add("igntC", new CheckBox("Use Ignite", true));
            combo.AddSeparator();
            combo.AddLabel("W Mode:");
            combo.Add("CWMODE", new ComboBox("Select Mode", 0, "With Prediciton", "Without Prediction", "Only On Stunned Enemies"));
            combo.AddSeparator();
            combo.AddLabel("E Options:");
            combo.Add("CEMODE", new ComboBox("E Mode: ", 0, "Target On The Center", "Target On The Edge(stun)", "AOE"));
            combo.Add("CEAOE", new Slider("Min Champs For AOE Function", 2, 1, 5));
            combo.Add("Es", new CheckBox("Dont Use E On Immobile Enemies", true));

            harass.AddGroupLabel("Spells");
            harass.Add("hQ", new CheckBox("Use Q", true));
            harass.Add("hW", new CheckBox("Use W", false));
            harass.AddSeparator();
            harass.AddLabel("W Mode:");
            harass.Add("HWMODE", new ComboBox("Select Mode", 2, "With Prediciton", "Without Prediction(Not Recommended)", "Only On Stunned Enemies"));
            harass.AddSeparator();
            harass.AddLabel("Min Mana To Harass");
            harass.Add("minMH", new Slider("Stop Harass At % Mana", 40, 0, 100));
            harass.AddSeparator();
            harass.AddLabel("Auto Harass");
            harass.Add("autoH", new CheckBox(" Use Auto harass", false));

            laneclear.AddGroupLabel("Spells");
            laneclear.Add("LQ", new CheckBox("Use Q", true));
            laneclear.Add("LW", new CheckBox("Use W", false));
            laneclear.AddSeparator();
            laneclear.AddLabel("Q Stacking");
            laneclear.Add("Qlk", new KeyBind("Auto Stacking", true, KeyBind.BindTypes.PressToggle, 'F'));
            laneclear.Add("Qlm", new ComboBox("Select Mode", 0, "LastHit 1 Minion", "LastHit 2 Minions"));
            laneclear.AddSeparator();
            laneclear.AddLabel("Min W Minions");
            laneclear.Add("Wmm", new Slider("Min minions to use W", 2, 1, 6));
            laneclear.AddSeparator();
            laneclear.AddLabel("Stop Laneclear At % Mana");
            laneclear.Add("LcM", new Slider("%", 50, 0, 100));
            laneclear.AddSeparator();
            laneclear.AddLabel("Auto Laneclear");
            laneclear.Add("AutoL", new CheckBox("Auto Laneclear", false));

            jungleclear.AddGroupLabel("Spells");
            jungleclear.Add("JQ", new CheckBox("Use Q", false));
            jungleclear.Add("JQMODE", new ComboBox("Q Mode", 1, "All Monsters", "Big Monsters"));
            jungleclear.AddSeparator();
            jungleclear.Add("JW", new CheckBox("Use W", true));
            jungleclear.Add("JWMIN", new Slider("Min Monsters To Hit With W", 2, 1, 4));
            jungleclear.AddSeparator();
            jungleclear.Add("JE", new CheckBox("Use E To Trap Monsters", false));
            jungleclear.Add("JEMIN", new Slider("Min Monsters To Trap With E", 3, 1, 4));
            jungleclear.AddSeparator();
            jungleclear.Add("JMIN", new Slider("Min Mana % To Jungleclear", 10, 0, 100));

            draw.Add("nodraw", new CheckBox("Disable All Drawings", false));
            draw.AddSeparator();
            draw.Add("drawQ", new CheckBox("Draw Q Range", true));
            draw.Add("drawW", new CheckBox("Draw W Range", true));
            draw.Add("drawE", new CheckBox("Draw E Range", true));
            draw.Add("drawR", new CheckBox("Draw R Range", true));
            draw.AddSeparator();
            draw.Add("drawAA", new CheckBox("Draw AA Range", false));
            draw.Add("drawk", new CheckBox("Draw Killable Enemies", false));
            draw.Add("nodrawc", new CheckBox("Draw Only Ready Spells", false));
            draw.Add("drawStacks", new CheckBox("Draw Auto Stack Mode", true));
            draw.Add("drawStackCount", new CheckBox("Draw Stack Count", false));

            misc.AddGroupLabel("Killsteal");
            misc.Add("ksQ", new CheckBox("Killsteal with Q", false));
            misc.Add("ksW", new CheckBox("Killsteal with W(With Prediction)", false));
            misc.Add("ksR", new CheckBox("Killsteal with R", false));
            misc.Add("autoign", new CheckBox("Auto Ignite If Killable", true));
            misc.AddSeparator();
            misc.Add("KSJ", new CheckBox("Steal Dragon/Baron/Rift Herald With W", false));
            misc.AddSeparator();
            misc.Add("AUTOPOT", new CheckBox("Auto Potion", true));
            misc.Add("POTMIN", new Slider("Min Health % To Activate Potion", 50, 1, 100));
            misc.AddSeparator();
            misc.AddGroupLabel("Gapcloser");
            misc.Add("gapmode", new ComboBox("Use E On Gapcloser                                               Mode:", 2, "Off", "Self", "Enemy(Pred)"));
            misc.AddSeparator();
            misc.AddGroupLabel("Auto Level Up Spells");
            misc.Add("autoS", new CheckBox("Activate Auto Level Up Spells", true));
            misc.AddSeparator();
            misc.AddGroupLabel("Skin Hack");
            misc.Add("sh", new CheckBox("Activate Skin hack"));
            misc.Add("sID", new ComboBox("Skin Hack", 0, "Default", "White Mage", "Curling", "Veigar Greybeard", "Leprechaun", "Baron Von", "Superb Villain", "Bad Santa", "Final Boss"));


            pred.AddGroupLabel("Q HitChance");
            pred.Add("Qhit", new Slider("% Hitchance", 85, 0, 100));
            pred.AddSeparator();
            pred.AddGroupLabel("W HitChance");
            pred.Add("Whit", new Slider("% Hitchance", 85, 0, 100));
            pred.AddSeparator();
            pred.AddGroupLabel("E HitChance");
            pred.Add("Ehit", new Slider("% Hitchance", 85, 0, 100));

        }
    }

    public static class DemSpells
    {
        public static Spell.Skillshot Q { get; private set; }
        public static Spell.Skillshot W { get; private set; }
        public static Spell.Skillshot E { get; private set; }
        public static Spell.Targeted R { get; private set; }

        static DemSpells()
        {
            Q = new Spell.Skillshot(SpellSlot.Q, 950, SkillShotType.Linear, 250, 2000, 70);
            W = new Spell.Skillshot(SpellSlot.W, 900, SkillShotType.Circular, 1350, 0, 112);
            E = new Spell.Skillshot(SpellSlot.E, 700, SkillShotType.Circular, 500, 0, 375);
            R = new Spell.Targeted(SpellSlot.R, 650);
        }
    }
}
