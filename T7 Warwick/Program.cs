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
    class Program : Base
    {
        static void Main(string[] args) { Loading.OnLoadingComplete += OnLoad; }

        #region Events
        public static void OnLoad(EventArgs args)
        {
            if (Player.Instance.ChampionName != ChampionName) return;

            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnLevelUp += OnLvlUp;
            Obj_AI_Base.OnBuffGain += OnBuffGain;
            Gapcloser.OnGapcloser += OnGapcloser;
            Orbwalker.OnUnkillableMinion += OnUnkillable;
            Game.OnTick += OnTick;

            Potion = new Item((int)ItemId.Health_Potion);
            Biscuit = new Item((int)ItemId.Total_Biscuit_of_Rejuvenation);
            RPotion = new Item((int)ItemId.Refillable_Potion);
            tiamat = new Item((int)ItemId.Tiamat_Melee_Only, 400);
            rhydra = new Item((int)ItemId.Ravenous_Hydra_Melee_Only, 400);
            thydra = new Item((int)ItemId.Titanic_Hydra);

            Q = new Spell.Chargeable(SpellSlot.Q, 349, 350, 500, 25, 1500); //SData delay = 0
            W = new Spell.Active(SpellSlot.W, 4000);
            E = new Spell.Active(SpellSlot.E, 375);
            R = new Spell.Skillshot(SpellSlot.R, (uint)(Player.Instance.MoveSpeed * 1.90f), SkillShotType.Linear, 200, 2150, 80); //range = movespeed * 2.25f (base = 335) // SData delay = 100

            if (EloBuddy.SDK.Spells.SummonerSpells.PlayerHas(EloBuddy.SDK.Spells.SummonerSpellsEnum.Smite))
                Smite = new Spell.Targeted(myhero.GetSpellSlotFromName("summonersmite"), 500);
            else if (EloBuddy.SDK.Spells.SummonerSpells.PlayerHas(EloBuddy.SDK.Spells.SummonerSpellsEnum.Ignite))
                Ignite = new Spell.Targeted(myhero.GetSpellSlotFromName("summonerdot"), 600);

            Player.LevelSpell(SpellSlot.Q);

            DatMenu();
            CheckPrediction();

            Chat.Print("<font color='#0040FF'>T7</font><font color='#3870D1'> " + ChampionName + "</font> : Loaded!(v" + Version + ")");
            Chat.Print("<font color='#04B404'>By </font><font color='#3737E6'>Toyota</font><font color='#D61EBE'>7</font><font color='#FF0000'> <3 </font>");
        }

        private static void OnUnkillable(Obj_AI_Base target, Orbwalker.UnkillableMinionArgs args)
        {
            if (check(laneclear, "QUNKILL") && Q.CanCast(target) && Q.GetSpellDamage(target) > target.GetHealthAfter(25) && target.HealthPercent > 3)
            {
                Q.FastCast(target.Position);
            }
        }

        private static void OnBuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
        {
            //if (sender.IsEnemy && !sender.IsMinion && args.Buff.Caster.IsMe) Chat.Print("Enemy: {0}", args.Buff.Name);

            //else if (sender.IsMe) Chat.Print("Self: {0}", args.Buff.Name);
        }

        private static void OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs args)
        {
            if (R.CanCast(sender))
            {
                switch(comb(misc, "RINT"))
                {
                    case 0: break;
                    case 1: if (R.GetPrediction(sender).HitChance >= HitChance.High) R.Cast(R.GetPrediction(sender).CastPosition); break; //nice pred
                    case 2:
                        if (args.DangerLevel == DangerLevel.High && R.GetPrediction(sender).HitChance >= HitChance.High)
                            R.Cast(R.GetPrediction(sender).CastPosition);
                        break;
                        
                }
            }
        }

        private static void OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs args)
        {
            if (sender.IsEnemy && check(misc, "QGAP") && Q.IsReady() && Q.IsInRange(args.End))
            {
                Q.Cast(sender);
            }
        }

        private static void OnTick(EventArgs args)
        {
            if (myhero.IsDead) return;

            var flags = Orbwalker.ActiveModesFlags;

            if (flags.HasFlag(Orbwalker.ActiveModes.Combo)) Combo();

            if (flags.HasFlag(Orbwalker.ActiveModes.Harass) && myhero.ManaPercent > slider(harass, "HMIN")) Harass();

            if (flags.HasFlag(Orbwalker.ActiveModes.LaneClear) && myhero.ManaPercent > slider(laneclear, "LMIN")) Laneclear();

            if (flags.HasFlag(Orbwalker.ActiveModes.JungleClear)) Jungleclear();

            Misc();
            AdjustRange();
        }

        private static void OnLvlUp(Obj_AI_Base sender, Obj_AI_BaseLevelUpEventArgs args)
        {
            if (!sender.IsMe || !check(misc, "autolevel")) return;

            Core.DelayAction(delegate
            {
                if (myhero.Level > 1 && myhero.Level < 4)
                {
                    switch (myhero.Level)
                    {
                        case 2:
                            Player.LevelSpell(SpellSlot.E);
                            break;
                        case 3:
                            Player.LevelSpell(SpellSlot.W);
                            break;
                    }
                }
                else if (myhero.Level >= 4)
                {
                    if (myhero.Spellbook.CanSpellBeUpgraded(SpellSlot.R) && Player.LevelSpell(SpellSlot.R))
                    {
                        return;
                    }
                    else if (myhero.Spellbook.CanSpellBeUpgraded(SpellSlot.Q) && Player.LevelSpell(SpellSlot.Q))
                    {
                        return;
                    }
                    else if (myhero.Spellbook.CanSpellBeUpgraded(SpellSlot.E) && Player.LevelSpell(SpellSlot.E))
                    {
                        return;
                    }
                    else if (myhero.Spellbook.CanSpellBeUpgraded(SpellSlot.W) && Player.LevelSpell(SpellSlot.W))
                    {
                        return;
                    }
                }
            }, new Random().Next(700, 900));
        }

        private static void OnDraw(EventArgs args)
        {
            if (myhero.IsDead || check(draw, "nodraw")) return;

            if (check(draw, "drawQ") && Q.Level > 0) Q.DrawRange(check(draw, "nodrawc") ? (Q.IsOnCooldown ? SharpDX.Color.Transparent : SharpDX.Color.DarkSlateGray) : SharpDX.Color.DarkSlateGray, 1);


            if (check(draw, "drawW") && W.Level > 0) W.DrawRange(check(draw, "nodrawc") ? (W.IsOnCooldown ? SharpDX.Color.Transparent : SharpDX.Color.DarkSlateGray) : SharpDX.Color.DarkSlateGray, 1);


            if (check(draw, "drawE") && E.Level > 0) E.DrawRange(check(draw, "nodrawc") ? (E.IsOnCooldown ? SharpDX.Color.Transparent : SharpDX.Color.DarkSlateGray) : SharpDX.Color.DarkSlateGray, 1);


            if (check(draw, "drawR") && R.Level > 0) R.DrawRange(check(draw, "nodrawc") ? (R.IsOnCooldown ? SharpDX.Color.Transparent : SharpDX.Color.DarkSlateGray) : SharpDX.Color.DarkSlateGray, 1);
        }
        #endregion

        #region Modes
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(R.Range, DamageType.Physical, Player.Instance.Position);

            if (target != null && target.ValidTarget(R.Range))
            {
                if (check(combo, "CQ") && Q.CanCast(target))
                {
                    if (check(combo, "CQCHARGE"))
                        Q.StartCharging();
                    else Q.FastCast(target.Position);
                }

                if (check(combo, "CE") && myhero.CountEnemies((int)E.Range) > 0 && E.IsReady() && check(combo, "CERE") ? myhero.IsEActive() : !myhero.IsEActive())
                    E.Cast();              

                var KillabilityCheck = !(check(combo, "CRONLY") && AvgComboDamage(target) < target.GetHealthAfter((int)(target.Position.Distance(myhero.Position) / 1000) * 1000));

                if (check(combo, "CR") && R.CanCast(target) && KillabilityCheck)
                {
                    var pred = R.GetPrediction(target);

                    if (pred.HitChance == HitChance.High)
                        R.Cast(pred.CastPosition);
                }

                var SmiteModeChecks = !(comb(combo, "SUMSPELLS") == 1 && myhero.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Smite) < target.Health) && comb(combo, "SUMSPELLS") != 0;
                if (Smite != null && Smite.CanCast(target) && SmiteModeChecks && AvgComboDamage(target) > target.Health)
                {
                    Smite.Cast(target);
                }

                ItemManager();
            }

            if (check(combo, "CW") && W.IsReady() && EntityManager.Heroes.Enemies.Any(x => x.ValidTarget(W.Range) && x.Distance(myhero.Position) < slider(combo, "CWMIN") && x.Distance(myhero.Position) / myhero.MoveSpeed < 8f))
                W.Cast();
        }
        
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(R.Range, DamageType.Physical, Player.Instance.Position);

            if (target != null && target.ValidTarget(R.Range))
            {
                if (check(harass, "HQ") && Q.CanCast(target))
                    Q.FastCast(target.Position);

                if (check(harass, "HE") && E.CanCast(target))
                {
                    E.Cast();
                }
            }
        }

        private static void Laneclear()
        {
            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, myhero.Position, E.Range);

            if (minions != null)
            {
                if (check(laneclear, "LQ") && Q.IsReady())
                {
                    foreach(var minion in minions.Where(x => x.HealthPercent > 3))
                    {
                        if (comb(laneclear, "LQMODE") == 0 && !minion.BaseSkinName.ToLower().Contains("siege") && !minion.BaseSkinName.ToLower().Contains("super")) continue;

                        Q.FastCast(minion.Position);
                    }
                }

                if (check(laneclear, "LE") && E.IsReady() && myhero.CountMinions((float)E.Range) > slider(laneclear, "LEMIN") && !myhero.IsEActive())
                {
                    E.Cast();
                }   
            }
        }

        private static void Jungleclear()
        {
            var Monsters = EntityManager.MinionsAndMonsters.GetJungleMonsters(myhero.Position, Q.Range);

            if (Monsters != null)
            {
                if (myhero.ManaPercent > slider(jungleclear, "JMIN"))
                {
                    if (check(jungleclear, "JQ") && Q.IsReady())
                    {
                        foreach (var monster in Monsters)
                        {
                            if (comb(jungleclear, "JQMODE") == 0 && monster.BaseSkinName.Contains("Mini")) continue;

                            myhero.Spellbook.CastSpell(SpellSlot.Q, monster);
                        }
                    }

                    if (check(jungleclear, "JE") && E.IsReady() && Monsters.Count(x => x.Distance(myhero.Position) < (float)E.Range) > slider(jungleclear, "JEMIN") ||
                        Monsters.Where(x => !x.BaseSkinName.Contains("Mini") && E.IsInRange(x)).Any() && !myhero.IsEActive())
                    {
                        E.Cast();
                    }
                }

                if (Smite != null && Smite.IsReady() && check(jungleclear, "SMITE"))
                {
                    foreach (var monster in Monsters)
                    {
                        if (BigMonsterNames.Any(x => monster.BaseSkinName.Contains(x)) && myhero.GetSummonerSpellDamage(monster, DamageLibrary.SummonerSpells.Smite) > monster.Health)
                            Smite.Cast(monster);
                    }
                }

            }
        }

        private static void Misc()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical, Player.Instance.Position);

            if (target != null && check(misc, "QKS") && Q.CanCast(target) && GetQDamage(target) > target.Health)
                Q.Cast(target);

            if (check(misc, "AUTOPOT") && !myhero.HasBuffOfType(BuffType.Heal) && myhero.HealthPercent <= slider(misc, "POTMIN"))
            {
                if (Item.HasItem(Potion.Id) && Item.CanUseItem(Potion.Id)) Potion.Cast();

                else if (Item.HasItem(Biscuit.Id) && Item.CanUseItem(Biscuit.Id)) Biscuit.Cast();

                else if (Item.HasItem(RPotion.Id) && Item.CanUseItem(RPotion.Id)) RPotion.Cast();
            }

            if (check(misc, "skinhax")) myhero.SetSkinId((int)misc["skinID"].Cast<ComboBox>().CurrentValue);
        }

        #endregion

        #region Menu
        public static void DatMenu()
        {
            menu = MainMenu.AddMenu("T7 " + ChampionName, "urgot");
            combo = menu.AddSubMenu("Combo", "combo");
            harass = menu.AddSubMenu("Harass", "harass");
            laneclear = menu.AddSubMenu("Laneclear", "lclear");
            jungleclear = menu.AddSubMenu("Jungleclear", "jclear");
            draw = menu.AddSubMenu("Drawings", "draw");
            misc = menu.AddSubMenu("Misc", "misc");

            menu.AddGroupLabel("Welcome to T7" + ChampionName + "And Thank You For Using!");
            menu.AddLabel("Version " + Version + " " + Date);
            menu.AddLabel("Author: Toyota7");

            combo.AddLabel("Spells");
            combo.Add("CQ", new CheckBox("Use Q"));
            combo.Add("CQCHARGE", new CheckBox("Use Charged Q"));
            combo.AddSeparator(10);
            combo.Add("CW", new CheckBox("Use W"));
            combo.Add("CWMIN", new Slider("Min Distance With Enemy To Cast W", 1500, 100, 3900));
            combo.AddSeparator(10);
            combo.Add("CE", new CheckBox("Use E"));
            combo.Add("CERE", new CheckBox("Recast E", false));
            combo.AddSeparator(10);
            combo.Add("CR", new CheckBox("Use R"));
            combo.Add("CRONLY", new CheckBox("Use R Only On Killable Targets"));
            combo.AddSeparator();
            combo.Add("SUMSPELLS", new ComboBox("Use Summoner Spells", 2, "Off", "Always", "Only On Killable"));
            combo.Add("CITEMS", new CheckBox("Use Items"));

            harass.AddLabel("Spells");
            harass.Add("HQ", new CheckBox("Use Q", false));
            harass.AddSeparator(10);
            harass.Add("HE", new CheckBox("Use E", false));
            harass.AddSeparator();
            harass.Add("HMIN", new Slider("Min Mana % To Harass", 50, 1, 100));

            laneclear.AddGroupLabel("Spells");
            laneclear.Add("LQ", new CheckBox("Use Q"));
            laneclear.Add("LQMODE", new ComboBox("Select Q Targets:", 0, "Big Minions", "All Minions"));
            laneclear.Add("QUNKILL", new CheckBox("Use Q On Unkillable Minions", false));
            laneclear.AddSeparator(10);
            laneclear.Add("LE", new CheckBox("Use E"));
            laneclear.Add("LEMIN", new Slider("Min Minions For E", 2, 1, 5));
            laneclear.AddSeparator();
            laneclear.Add("LMIN", new Slider("Min Mana % To Laneclear", 50, 1, 100));

            jungleclear.AddGroupLabel("Spells");
            jungleclear.Add("JQ", new CheckBox("Use Q"));
            jungleclear.Add("JQMODE", new ComboBox("Select Q Targets", 0, "Big Monsters", "All Monsters"));
            jungleclear.AddSeparator();
            jungleclear.Add("JE", new CheckBox("Use E"));
            jungleclear.Add("JEMIN", new Slider("Min Monsters For E", 2, 1, 4));
            jungleclear.AddSeparator();
            jungleclear.Add("SMITE", new CheckBox("Auto-Smite Big Monsters"));
            jungleclear.AddSeparator(5);
            jungleclear.Add("JMIN", new Slider("Min Mana % To Jungleclear", 50, 1, 100));

            draw.Add("nodraw", new CheckBox("Disable All Drawings", false));
            draw.AddSeparator();
            draw.Add("drawQ", new CheckBox("Draw Q Range"));
            draw.Add("drawW", new CheckBox("Draw W Range"));
            draw.Add("drawE", new CheckBox("Draw E Range"));
            draw.Add("drawR", new CheckBox("Draw R Range"));
            draw.AddSeparator();
            draw.Add("nodrawc", new CheckBox("Draw Only Ready Spells", false));

            misc.AddGroupLabel("Other Settings");
            misc.Add("QKS", new CheckBox("Killsteal With Q", false));
            misc.Add("QGAP", new CheckBox("Use Q On Gapclosers", false));
            misc.AddSeparator(1);
            misc.Add("RINT", new ComboBox("Use R To Interrupt", 0, "Off", "All Spells", "Dangerous Spells"));
            misc.AddSeparator(1);
            misc.AddLabel("Auto Potion");
            misc.Add("AUTOPOT", new CheckBox("Activate Auto Potion"));
            misc.Add("POTMIN", new Slider("Min Health % To Active Potion", 25, 1, 100));
            misc.AddSeparator();
            misc.AddLabel("Auto Level Up Spells");
            misc.Add("autolevel", new CheckBox("Activate Auto Level Up Spells"));
            misc.AddSeparator();
            misc.AddLabel("Skin Hack");
            misc.Add("skinhax", new CheckBox("Activate Skin hack"));
            misc.Add("skinID", new ComboBox("Skin Hack", 6, new string[]
            {
                "Default",
                "Grey",
                "Urf the Manatee",
                "Big Bad",
                "Tundra Hunter",
                "Feral",
                "Firefang",
                "Hyena",
                "Marauder"                           
            }));
        }
        #endregion
    }
}


