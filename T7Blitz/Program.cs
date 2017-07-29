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
            Spellbook.OnCastSpell += OnCastSpell;
            Orbwalker.OnPreAttack += OnPreAttack;
            Interrupter.OnInterruptableSpell += OnInterruptableSpell;
            Gapcloser.OnGapcloser += OnGapcloser;
            Game.OnTick += OnTick;
            Game.OnUpdate += AutoE => { if (myhero.HasBuff(ESelfBuffName)) KnockupTarget(); };

            Potion = new Item((int)ItemId.Health_Potion);
            Biscuit = new Item((int)ItemId.Total_Biscuit_of_Rejuvenation);
            RPotion = new Item((int)ItemId.Refillable_Potion);

            Q = new Spell.Skillshot(SpellSlot.Q, 925, SkillShotType.Linear, 250, 1750, 70)
            {
                AllowedCollisionCount = 0
            };
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Active(SpellSlot.E);
            R = new Spell.Active(SpellSlot.R, 600);

            Player.LevelSpell(SpellSlot.Q);

            EnemyPlayerNames = EntityManager.Heroes.Enemies.Select(x => x.ChampionName).ToArray();
            EnemyADC = GetEnemyADC();

            DatMenu();
            CheckPrediction();

            Chat.Print("<font color='#0040FF'>T7</font><font color='#CDD411'> " + ChampionName + "</font> : Loaded!(v" + Version + ")");
            Chat.Print("<font color='#04B404'>By </font><font color='#3737E6'>Toyota</font><font color='#D61EBE'>7</font><font color='#FF0000'> <3 </font>");
        }

        private static void OnPreAttack(AttackableUnit sender, Orbwalker.PreAttackArgs args)
        {
            var target = sender as Obj_AI_Base;
            if (target.Type == GameObjectType.AIHeroClient && target.IsEnemy && check(combo, "CAVOIDAA") && Prediction.Health.GetPrediction(target, 1700) < myhero.GetAutoAttackDamage(target) &&
                target.CountAllies(800) == 0)
            {
                args.Process = false;
            }
        }

        private static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args2)
        {
            if (sender.Owner.IsMe && args2.Slot.Equals(SpellSlot.E)) Orbwalker.ResetAutoAttack();
        }

        private static void OnBuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args8)
        {
            if (check(combo, "CEONLY") && sender.IsEnemy && !sender.IsMinion && args8.Buff.Name == QTargetBuffName)
            {
                E.Cast();
            }
        }

        private static void OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs args4)
        {
            if (!sender.IsEnemy || !sender.IsValidTarget()) return;

            if (check(misc, "RINT") && R.IsReady() && R.IsInRange(sender.GetPositionAfter(250)))
            {
                R.Cast();
            }
            else if (check(misc, "QINT") && Q.CanCast(sender) && Q.GetPrediction(sender).HitChancePercent >= slider(qsett, "QPRED"))
            {
                Q.Cast(Q.GetPrediction(sender).CastPosition);
            }

        }

        private static void OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs args5)
        {
            if (check(misc, "QGAP") && sender.IsEnemy && Q.IsReady() && Q.IsInRange(args5.End))
            {
                var qpred = Q.GetPrediction(sender);

                if (qpred != null && qpred.HitChance == HitChance.Dashing)
                {
                    Q.Cast(qpred.CastPosition);
                }
            }
        }

        private static void OnTick(EventArgs args)
        {
            if (myhero.IsDead) return;

            var flags = Orbwalker.ActiveModesFlags;

            if (flags.HasFlag(Orbwalker.ActiveModes.Combo)) Combo();

            if (flags.HasFlag(Orbwalker.ActiveModes.Harass) && myhero.ManaPercent > slider(harass, "HMIN")) Harass();

            if (flags.HasFlag(Orbwalker.ActiveModes.LaneClear) && myhero.ManaPercent > slider(laneclear, "LMIN")) Laneclear();

            if (flags.HasFlag(Orbwalker.ActiveModes.JungleClear) && myhero.ManaPercent > slider(jungleclear, "JMIN")) Jungleclear();

            if (flags.HasFlag(Orbwalker.ActiveModes.None) && (Orbwalker.DisableAttacking || Orbwalker.DisableMovement))
            {
                Orbwalker.DisableAttacking = false;
                Orbwalker.DisableMovement = false;
            }

            if (key(qsett, "FORCEQ"))
            {
                Orbwalker.MoveTo(Game.CursorPos);
            }

            Misc();
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
            }, new Random().Next(800, 1200));
        }

        private static void OnDraw(EventArgs args)
        {
            if (myhero.IsDead || check(draw, "nodraw")) return;

            if (check(draw, "drawQ") && Q.Level > 0)
            {
                Circle.Draw
                (
                    check(draw, "nodrawc") ? (Q.IsOnCooldown ? SharpDX.Color.Transparent : SharpDX.Color.Yellow) : SharpDX.Color.Yellow,
                    Q.Range,
                    myhero.Position
                );
            }

            if (check(draw, "drawR") && R.Level > 0)
            {
                Circle.Draw
                (
                    check(draw, "nodrawc") ? (R.IsOnCooldown ? SharpDX.Color.Transparent : SharpDX.Color.Yellow) : SharpDX.Color.Yellow,
                    R.Range,
                    myhero.Position
                );
            }

            AIHeroClient target = TargetSelector.GetTarget(Q.Range, DamageType.Magical, myhero.Position);

            if (target != null)
            {
                if (Q.IsReady())
                {
                    var qpred = Q.GetPrediction(target);

                    if (check(draw, "DRAWPRED"))
                    {
                        Geometry.Polygon.Rectangle Prediction = new Geometry.Polygon.Rectangle(myhero.Position.To2D(), qpred.CastPosition.To2D(), Q.Width);
                        Prediction.Draw(Color.Yellow, 1);
                    }

                    if (check(draw, "DRAWHIT"))
                    {
                        Drawing.DrawText(Drawing.WorldToScreen(myhero.Position).X - 50,
                                    Drawing.WorldToScreen(myhero.Position).Y + 10,
                                    Color.Yellow,
                                    "Hitchance %: ");
                        Drawing.DrawText(Drawing.WorldToScreen(myhero.Position).X + 37,
                                            Drawing.WorldToScreen(myhero.Position).Y + 10,
                                            Color.Green,
                                            qpred.HitChancePercent.ToString());
                    }
                }

                if (check(draw, "DRAWTARGET"))
                {
                    Circle.Draw(SharpDX.Color.Yellow, 50, target.Position);
                }

                if (check(draw, "DRAWWAY") && target.Path.Any())
                {
                    for (var i = 1; target.Path.Length > i; i++)
                    {
                        if (target.Path[i - 1].IsValid() && target.Path[i].IsValid() && (target.Path[i - 1].IsOnScreen() || target.Path[i].IsOnScreen()))
                        {
                            Drawing.DrawLine(Drawing.WorldToScreen(target.Position), Drawing.WorldToScreen(target.Path[i]), 3, Color.White);
                        }
                    }
                }
            }
        }
        #endregion

        #region Modes
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical, Player.Instance.Position);


            if (target != null && target.ValidTarget((int)Q.Range))
            {
                if (check(combo, "CQ") && Q.IsReady() && check(qsett, "Q" + target.ChampionName))
                {
                    if (check(qsett, "QCLOSE") && target.Distance(myhero.Position) < myhero.GetAutoAttackRange()) return;

                    //var Qpred = Q.GetPrediction(target);
                    var Qpred = Q.GetPrediction(target);

                    if (Qpred.HitChancePercent >= slider(qsett, "QPRED"))
                    {
                        Q.Cast(Qpred.CastPosition);
                    }
                }

                if (check(combo, "CW") && W.IsReady() && myhero.CountEnemyChampionsInRange(500) > 0)
                {
                    W.Cast();
                }

                if (check(combo, "CE") && E.IsReady() && myhero.CountEnemyChampionsInRange(300) > 0)
                {
                    E.Cast();
                }

                var MainChecks = check(combo, "CR") && R.IsReady() && !(R.IsReady() && E.IsReady()) && !myhero.HasBuff(ESelfBuffName);
                var EnemiesWithCC = check(combo, "CRAUTO") && EntityManager.Heroes.Enemies.Any(x => (x.HasBuffOfType(BuffType.Knockup) || x.HasBuffOfType(BuffType.Stun)) && R.IsInRange(x));
                var MultipleEnemies = myhero.CountEnemyHeroesInRangeWithPrediction((int)R.Range - 10, R.CastDelay) >= slider(combo, "CRMINE");
                var SingleTarget = Q.IsOnCooldown && E.IsOnCooldown && R.CanCast(target) && Prediction.Health.GetPrediction(target, R.CastDelay) >= slider(combo, "CRMINH");
                var PreventKS = check(combo, "CAVOID") && Prediction.Health.GetPrediction(target, R.CastDelay) < myhero.GetSpellDamage(target, SpellSlot.R);

                if (MainChecks && (EnemiesWithCC || MultipleEnemies || SingleTarget))
                {
                    if (PreventKS) return;

                    R.Cast();
                }
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical, Player.Instance.Position);

            if (target != null && target.ValidTarget((int)Q.Range))
            {
                if (check(harass, "HQ") && Q.IsReady() && check(qsett, "Q" + target.ChampionName))
                {
                    var Qpred = Q.GetPrediction(target);

                    if (Qpred.HitChancePercent >= slider(qsett, "QPRED") && Q.Cast(Qpred.CastPosition))
                        return;
                }

                if (check(harass, "HW") && W.IsReady() && myhero.CountEnemyChampionsInRange(myhero.GetAutoAttackRange()) >= slider(harass, "HWMIN") && W.Cast())
                    return;

                if (check(harass, "HR") && R.IsReady() && myhero.CountEnemyChampionsInRange(R.Range - 10) >= slider(harass, "HRMIN") && R.Cast())
                    return;
            }
        }

        private static void Laneclear()
        {
            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, myhero.Position, Q.Range).ToList();

            if (minions != null)
            {
                if (check(laneclear, "LQ") && Q.IsReady())
                {
                    foreach (Obj_AI_Minion minion in minions.Where(x => x.Distance(myhero.Position) < Q.Range - 75))
                    {
                        var qpred = Q.GetPrediction(minion);

                        if (qpred.HitChancePercent >= slider(qsett, "QPRED") && Q.Cast(qpred.CastPosition))
                        {
                            return;
                        }
                    }
                }

                if (check(laneclear, "LW") && W.IsReady() && minions.Where(x => x.Distance(myhero.Position) < 250).Count() >= slider(jungleclear, "JWMIN") &&
                    W.Cast())
                {
                    return;
                }

                if (check(laneclear, "LE") && E.IsReady() && minions.Any(x => x.Health > 50 && x.Distance(myhero.Position) < myhero.GetAutoAttackRange()) &&
                    E.Cast())
                {
                    return;
                }

                if (check(laneclear, "LR") && R.IsReady() && minions.Where(x => x.Distance(myhero.Position) < R.Range - 10).Count() >= slider(jungleclear, "JRMIN") &&
                    R.Cast())
                {
                    return;
                }
            }
        }

        private static void Jungleclear()
        {
            var Monsters = EntityManager.MinionsAndMonsters.GetJungleMonsters(myhero.Position, Q.Range);

            if (Monsters != null)
            {
                if (check(jungleclear, "JQ") && Q.IsReady())
                {
                    foreach (Obj_AI_Minion monster in Monsters)
                    {
                        if (comb(jungleclear, "JQMODE") == 0 && monster.BaseSkinName.Contains("Mini")) continue;

                        var qpred = Q.GetPrediction(monster);

                        if (qpred.HitChancePercent >= slider(qsett, "QPRED") && Q.Cast(qpred.CastPosition))
                        {
                            return;
                        }
                    }
                }

                if (check(jungleclear, "JW") && W.IsReady() && Monsters.Where(x => x.Distance(myhero.Position) < 250).Count() >= slider(jungleclear, "JWMIN") &&
                    W.Cast())
                {
                    return;
                }

                if (check(jungleclear, "JE") && E.IsReady())
                {
                    foreach (Obj_AI_Minion monster in Monsters.Where(x => x.Distance(myhero.Position) < myhero.GetAutoAttackRange()))
                    {
                        if (comb(jungleclear, "JEMODE") == 0 && monster.BaseSkinName.Contains("Mini")) continue;

                        if (E.Cast())
                        {
                            Orbwalker.DisableAttacking = true;
                            Orbwalker.DisableMovement = true;

                            Player.IssueOrder(GameObjectOrder.AttackUnit, monster);

                            Orbwalker.DisableAttacking = false;
                            Orbwalker.DisableMovement = false;
                        }
                    }
                }

                if (check(jungleclear, "JR") && R.IsReady() && Monsters.Where(x => x.Distance(myhero.Position) < R.Range - 10).Count() >= slider(jungleclear, "JRMIN") &&
                    R.Cast())
                {
                    return;
                }
            }
        }

        private static void Misc()
        {
            var Qtarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical, Player.Instance.Position);

            if (Qtarget != null && Qtarget.ValidTarget((int)Q.Range) && key(qsett, "FORCEQ") && Q.IsReady() && check(qsett, "Q" + Qtarget.ChampionName))
            {
                Q.Cast(Q.GetPrediction(Qtarget).CastPosition);
            }

            var target = TargetSelector.GetTarget(500, DamageType.Magical, Player.Instance.Position);

            if (target != null && target.ValidTarget(1000) && check(misc, "KSR") && R.CanCast(target) && target.Health < myhero.GetSpellDamage(target, SpellSlot.R) &&
                Prediction.Health.GetPrediction(target, R.CastDelay) > 0)
            {
                R.Cast();
            }

            if (check(misc, "AUTOPOT") && !myhero.HasBuffOfType(BuffType.Heal) && myhero.HealthPercent <= slider(misc, "POTMIN"))
            {
                if (Item.HasItem(Potion.Id) && Item.CanUseItem(Potion.Id)) Potion.Cast();

                else if (Item.HasItem(Biscuit.Id) && Item.CanUseItem(Biscuit.Id)) Biscuit.Cast();

                else if (Item.HasItem(RPotion.Id) && Item.CanUseItem(RPotion.Id)) RPotion.Cast();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee) && check(misc, "WFLEE") && W.IsReady() && W.Cast())
                return;

            if (check(misc, "skinhax")) myhero.SetSkinId((int)misc["skinID"].Cast<ComboBox>().CurrentValue);
        }

        #endregion

        #region Menu
        public static void DatMenu()
        {
            menu = MainMenu.AddMenu("T7 Blitz", "blitz");
            qsett = menu.AddSubMenu("Q Settings", "qsettings");
            combo = menu.AddSubMenu("Combo", "combo");
            harass = menu.AddSubMenu("Harass", "harass");
            laneclear = menu.AddSubMenu("Laneclear", "lclear");
            jungleclear = menu.AddSubMenu("Jungleclear", "jclear");
            draw = menu.AddSubMenu("Drawings", "draw");
            misc = menu.AddSubMenu("Misc", "misc");

            menu.AddGroupLabel("Welcome to T7 " + ChampionName + " And Thank You For Using!");
            menu.AddLabel("Version " + Version + " " + Date);
            menu.AddLabel("Author: Toyota7");

            qsett.AddGroupLabel("Q Settings");
            qsett.AddSeparator();
            qsett.AddLabel("Forced Q Casting");
            qsett.Add("FORCEQ", new KeyBind("Force Q To Cast", false, KeyBind.BindTypes.HoldActive, 'B'));
            qsett.Add("QINFO", new CheckBox("Info About Forced Q Casting Keybind", false)).OnValueChange +=
                delegate (ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
                {
                    if (args.NewValue == true)
                    {
                        Chat.Print("<font color='#A5A845'>Force Q Casting Info</font>:");
                        Chat.Print("This Keybind Will Cast Q At The Target Champion Using The Current Q Prediction,");
                        Chat.Print("Which Means That It Will Ignore Collision Checks Or Hitchance Numbers Lower Than The Ones On The Settings.");
                        Chat.Print("You Can See The Current Q Prediction Using The Addon's Drawing Functions.");
                        Chat.Print("I Also Wouldnt Recommend Using This Function Without The Addon's Prediction Drawings(You Wont See The Cast Position Otherwise!).");
                        sender.CurrentValue = false;
                    }
                };
            qsett.AddSeparator();
            qsett.AddLabel("Q Hitchance %");
            qsett.Add("QPRED", new Slider("Select Minimum Hitchance %", 65, 1, 100));
            qsett.AddSeparator();
            qsett.AddLabel("Q Targets:");
            foreach (AIHeroClient enemy in EntityManager.Heroes.Enemies)
            {
                qsett.Add("Q" + enemy.ChampionName, new CheckBox(enemy.ChampionName));
            }
            qsett.AddSeparator();
            qsett.Add("QCLOSE", new CheckBox("Dont Grap If Target Is In AA Range"));

            combo.AddLabel("Spells");
            combo.Add("CQ", new CheckBox("Use Q"));
            combo.AddLabel("(For Q Options Go To Q Settings Tab)");
            combo.AddSeparator(10);
            combo.Add("CW", new CheckBox("Use W"));
            combo.Add("CWMIN", new Slider("Min Enemies Nearby To Cast W", 1, 1, 5));
            combo.AddSeparator(10);
            combo.Add("CE", new CheckBox("Use E"));
            combo.Add("CEONLY", new CheckBox("Auto-E After Succesful Grab"));
            combo.AddSeparator(10);
            combo.Add("CR", new CheckBox("Use R"));
            combo.Add("CRAUTO", new CheckBox("Auto R On Knocked-Up/Stunned Targets"));
            combo.Add("CRMINE", new Slider("Min Enemies For R", 2, 1, 5));
            combo.Add("CRMINH", new Slider("Min Enemy Health % For R", 30, 1, 100));
            combo.AddSeparator();
            combo.Add("CAVOID", new CheckBox("Prevent R Killsteals"));
            combo.Add("CAVOIDAA", new CheckBox("Prevent AA KillSteals"));

            harass.AddLabel("Spells");
            harass.Add("HQ", new CheckBox("Use Q", false));
            harass.AddLabel("(For Q Options Go To Q Settings Tab)");
            harass.AddSeparator(10);
            harass.Add("HW", new CheckBox("Use W", false));
            harass.Add("HWMIN", new Slider("Min Enemies For W", 2, 1, 5));
            harass.AddSeparator();
            harass.Add("HR", new CheckBox("Use R", false));
            harass.Add("HRMIN", new Slider("Min Enemies For R", 2, 1, 5));
            harass.AddSeparator();
            harass.Add("HMIN", new Slider("Min Mana % To Harass", 50, 1, 100));

            laneclear.AddGroupLabel("Spells");
            laneclear.Add("LQ", new CheckBox("Use Q"));
            laneclear.AddSeparator(10);
            laneclear.Add("LW", new CheckBox("Use W"));
            laneclear.Add("LWMIN", new Slider("Min Minions For W", 3, 1, 10));
            laneclear.AddSeparator(20);
            laneclear.Add("LE", new CheckBox("Use E"));
            laneclear.AddSeparator(10);
            laneclear.Add("LR", new CheckBox("Use R", false));
            laneclear.Add("LRMIN", new Slider("Min Minions For R", 4, 1, 10));
            laneclear.AddSeparator();
            laneclear.Add("LMIN", new Slider("Min Mana % To Laneclear", 50, 1, 100));

            jungleclear.AddGroupLabel("Spells");
            jungleclear.Add("JQ", new CheckBox("Use Q"));
            jungleclear.Add("JQMODE", new ComboBox("Select Q Targets", 0, "Big Monsters", "All Monsters"));
            jungleclear.AddSeparator();
            jungleclear.Add("JW", new CheckBox("Use W"));
            jungleclear.Add("JWMIN", new Slider("Min Monsters For W", 2, 1, 4));
            jungleclear.AddSeparator();
            jungleclear.Add("JE", new CheckBox("Use E"));
            jungleclear.Add("JEMODE", new ComboBox("Select E Targets", 0, "Big Monsters", "All Monsters"));
            jungleclear.AddSeparator();
            jungleclear.Add("JR", new CheckBox("Use R"));
            jungleclear.Add("JRMIN", new Slider("Min Monsters For R", 3, 1, 4));
            jungleclear.AddSeparator();
            jungleclear.Add("JMIN", new Slider("Min Mana % To Jungleclear", 50, 1, 100));

            draw.Add("nodraw", new CheckBox("Disable All Drawings", false));
            draw.AddSeparator();
            draw.Add("drawQ", new CheckBox("Draw Q Range"));
            draw.Add("drawR", new CheckBox("Draw R Range"));
            draw.AddSeparator();
            draw.Add("nodrawc", new CheckBox("Draw Only Ready Spells", false));
            draw.AddSeparator();
            draw.AddGroupLabel("Q Drawings");
            draw.Add("DRAWPRED", new CheckBox("Draw Q Prediction", false));
            draw.AddSeparator(1);
            draw.Add("DRAWTARGET", new CheckBox("Draw Q Target", false));
            draw.AddSeparator(1);
            draw.Add("DRAWHIT", new CheckBox("Draw Q Hitchance", false));
            draw.AddSeparator(1);
            draw.Add("DRAWWAY", new CheckBox("Draw Targets Waypoint", false));

            misc.AddLabel("Focusing Settings");
            misc.Add("FOCUS", new ComboBox("Focus On: ", 0, "Enemy ADC", "All Champs(TS)", "Custom Champion")).OnValueChange += delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
            {
                if (args.NewValue == 2) misc["CFOCUS"].Cast<ComboBox>().IsVisible = true;
                else misc["CFOCUS"].Cast<ComboBox>().IsVisible = false;
            };
            misc.Add("CFOCUS", new ComboBox("Which Champion To Focus On? ", 0, EnemyPlayerNames));
            misc.AddSeparator(5);
            misc.AddLabel("Other Settings");
            misc.Add("KSR", new CheckBox("Killsteal with R"));
            misc.AddSeparator(1);
            misc.Add("WFLEE", new CheckBox("Use W To Flee"));
            misc.AddSeparator(1);
            misc.Add("QGAP", new CheckBox("Use Q On Gapclosers", false));
            misc.AddSeparator();
            misc.AddLabel("Interrupting");
            misc.Add("RINT", new CheckBox("Use R To Interrupt"));
            misc.Add("QINT", new CheckBox("Use Q To Interrupt"));
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
            misc.Add("skinID", new ComboBox("Skin Hack", 11, new string[]
            {
                "Default",
                "Rusty",
                "Goalkeeper",
                "Boom Boom",
                "Piltover Customs",
                "Definitely Not",
                "iBlitz",
                "Riot",
                "Chroma Red",
                "Chroma Blue",
                "Chroma Gray",
                "Battle Boss"
            }));
        }
        #endregion
    }

}


