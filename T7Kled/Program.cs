using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using Color = System.Drawing.Color;

namespace T7_Kled
{
    class Program : Base
    {
        static void Main(string[] args) { Loading.OnLoadingComplete += OnLoad; }

        #region Events
        private static void OnLoad(EventArgs args)
        {
            if (Player.Instance.ChampionName != ChampionName) return; 
            
            Chat.Print("<font color='#0040FF'>T7</font><font color='#DEBC23'> " + ChampionName + "</font> : Loaded!(v" + Version + ")");
            Chat.Print("<font color='#04B404'>By </font><font color='#3737E6'>Toyota</font><font color='#D61EBE'>7</font><font color='#FF0000'> <3 </font>");

            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnLevelUp += OnLvlUp;
            Gapcloser.OnGapcloser += OnGapcloser;
            Game.OnTick += OnTick;

            Potion = new Item((int)ItemId.Health_Potion);
            tiamat = new Item((int)ItemId.Tiamat_Melee_Only, 400);
            cutl = new Item((int)ItemId.Bilgewater_Cutlass, 550);
            thydra = new Item((int)ItemId.Titanic_Hydra);
            blade = new Item((int)ItemId.Blade_of_the_Ruined_King, 550);
            rhydra = new Item((int)ItemId.Ravenous_Hydra_Melee_Only, 400);
            yomus = new Item((int)ItemId.Youmuus_Ghostblade);
            Biscuit = new Item((int)ItemId.Total_Biscuit_of_Rejuvenation);
            RPotion = new Item((int)ItemId.Refillable_Potion);

            Q1 = new Spell.Skillshot(SpellSlot.Q, 800, SkillShotType.Linear, 250, 3000, 40);
            Q2 = new Spell.Skillshot(SpellSlot.Q, 800, SkillShotType.Cone, 250, 1600, 90);
            E = new Spell.Skillshot(SpellSlot.E, 550, SkillShotType.Linear, 250, 950, 100);

            if (EloBuddy.SDK.Spells.SummonerSpells.PlayerHas(EloBuddy.SDK.Spells.SummonerSpellsEnum.Ignite))
            {
                Ignite = new Spell.Targeted(myhero.GetSpellSlotFromName("summonerdot"), 600);
            }

            Player.LevelSpell(SpellSlot.Q);

            DatMenu();
        }

        private static void OnTick(EventArgs args)
        {
            if (myhero.IsDead) return;

            var flags = Orbwalker.ActiveModesFlags;

            if (flags.HasFlag(Orbwalker.ActiveModes.Combo)) Combo(); 

            if (flags.HasFlag(Orbwalker.ActiveModes.Harass) || key(harass, "AUTOHARASS")) Harass();

            if (flags.HasFlag(Orbwalker.ActiveModes.LaneClear) || key(laneclear, "AUTOLANECLEAR")) Laneclear();

            if (flags.HasFlag(Orbwalker.ActiveModes.JungleClear)) Jungleclear();

            if (flags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                if (check(misc, "EFLEE") && E.IsReady()) E.Cast(myhero.Position.Extend(Game.CursorPos, E.Range).To3D());
            }

            Misc();
        }

        private static void OnLvlUp(Obj_AI_Base sender, Obj_AI_BaseLevelUpEventArgs args)
        {
            if (!sender.IsMe || !check(misc, "autolevel")) return;

            Core.DelayAction(() =>
            {
                switch (myhero.Level)
                {
                    case 2:
                        Player.LevelSpell(SpellSlot.W);
                        break;
                    case 3:
                        Player.LevelSpell(SpellSlot.E);
                        break;
                    default:
                        if (myhero.Spellbook.CanSpellBeUpgraded(SpellSlot.R) && Player.LevelSpell(SpellSlot.R))
                            return;

                        else if (myhero.Spellbook.CanSpellBeUpgraded(SpellSlot.Q) && Player.LevelSpell(SpellSlot.Q))
                            return;

                        else if (myhero.Spellbook.CanSpellBeUpgraded(SpellSlot.E) && Player.LevelSpell(SpellSlot.E))
                            return;

                        else if (myhero.Spellbook.CanSpellBeUpgraded(SpellSlot.W) && Player.LevelSpell(SpellSlot.W))
                            return;
                        break;
                }
            }, new Random().Next(500, 800));
        }

        private static void OnGapcloser(Obj_AI_Base sender, Gapcloser.GapcloserEventArgs args)
        {
            if (!check(misc, "QGAP") || !sender.IsEnemy || !Q1.IsInRange(args.End)) return;

            var spell = HasMount() ? Q1 : Q2;

            var qpred = spell.GetPrediction(sender);

            if ((HasMount() ? qpred.CollisionObjects.Where(x => x is AIHeroClient).Count() == 0 : !qpred.Collision) && qpred.HitChance == HitChance.Dashing)
            {
                spell.Cast(qpred.CastPosition);
            }           
        }

        private static void OnDraw(EventArgs args)
        {
            if (myhero.IsDead || check(draw, "nodraw")) return;

            if (check(draw, "drawQ") && Q1.Level > 0)
                Q1.DrawRange(check(draw, "nodrawc") ? (Q1.IsOnCooldown ? SharpDX.Color.Transparent : SharpDX.Color.YellowGreen) : SharpDX.Color.YellowGreen, 1);

            if (check(draw, "drawE") && E.Level > 0 && HasMount())
                E.DrawRange(check(draw, "nodrawc") ? (E.IsOnCooldown ? SharpDX.Color.Transparent : SharpDX.Color.YellowGreen) : SharpDX.Color.YellowGreen, 1);

            if (check(draw, "drawauto"))
            {
                Drawing.DrawText(Drawing.WorldToScreen(myhero.Position).X - 50,
                                     Drawing.WorldToScreen(myhero.Position).Y + 10,
                                     Color.White,
                                     "Auto Laneclear: ");
                Drawing.DrawText(Drawing.WorldToScreen(myhero.Position).X + 52,
                                 Drawing.WorldToScreen(myhero.Position).Y + 10,
                                 key(laneclear, "AUTOLANECLEAR") ? Color.Green : Color.Red,
                                 key(laneclear, "AUTOLANECLEAR") ? "ON" : "OFF");
                Drawing.DrawText(Drawing.WorldToScreen(myhero.Position).X - 50,
                                 Drawing.WorldToScreen(myhero.Position).Y + 25,
                                 Color.White,
                                 "Auto Harass: ");
                Drawing.DrawText(Drawing.WorldToScreen(myhero.Position).X + 34,
                                 Drawing.WorldToScreen(myhero.Position).Y + 25,
                                 key(harass, "AUTOHARASS") ? Color.Green : Color.Red,
                                 key(harass, "AUTOHARASS") ? "ON" : "OFF");
            }
        }
        #endregion

        #region Modes
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(1000, DamageType.Physical, Player.Instance.Position);

            if (target != null && target.ValidTarget(Q1.Range))
            {
                if (check(combo, "CQ") && Q1.CanCast(target))
                {
                    QCast(target);
                }

                if (HasMount() && check(combo, "CE") && E.CanCast(target))
                {
                    var epred = E.GetPrediction(target);

                    if (epred.CastPosition.IsUnderTurret() || (target.HasBuff("klede2target") && myhero.Position.Extend(target.Position, 450).IsUnderTurret()))
                        return;

                    else if (!target.HasBuff("klede2target") && epred.HitChancePercent >= slider(pred, "EPred")) E.Cast(epred.CastPosition);
                    
                    else if (target.HasBuff("klede2target")) E.Cast(target);
                }

                ItemManager(target);
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(1200, DamageType.Physical, Player.Instance.Position);

            if (target != null && target.ValidTarget(Q1.Range))
            {
                if (check(harass, "HQ") && Q1.CanCast(target))
                {
                    QCast(target);
                }
            }
        }

        private static void Laneclear()
        {
            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, myhero.Position, 1000);

            if (minions != null)
            {
                if (check(laneclear, "LQ") && Q1.IsReady())
                {
                    if (HasMount()) Q1.CastOnBestFarmPosition(slider(laneclear, "LQMIN"));

                    else
                    {
                        foreach (var minion in minions.Where(x => x.HealthPercent > 10 && x.IsValid))
                        {
                            var qpred = Q2.GetPrediction(minion);

                            if (comb(laneclear, "LQMODE") == 0 && !minion.Name.ToLower().Contains("siege") && !minion.Name.ToLower().Contains("super")) continue;

                            if (!qpred.Collision && qpred.HitChancePercent >= slider(pred, "Q2Pred")) Q2.Cast(qpred.CastPosition);
                        }
                    }
                }

                if (HasMount() && check(laneclear, "LE") && E.IsReady()) E.CastOnBestFarmPosition(slider(laneclear, "LEMIN"));
            }
        }

        private static void Jungleclear()
        {
            var Monsters = EntityManager.MinionsAndMonsters.GetJungleMonsters(myhero.Position, 1000).Where(x => x.IsValidTarget(Q1.Range));
            var spell = HasMount() ? Q1 : Q2;
            var hit = HasMount() ? slider(pred, "Q1Pred") : slider(pred, "Q2Pred");

            if (Monsters != null)
            {
                if (Monsters.Where(x => x.HasBuff("klede2target")).Any()) E.Cast();

                if (check(jungleclear, "JQ") && Q1.IsReady())
                {
                    if (Monsters.Where(x => !x.Name.ToLower().Contains("mini")).Any())
                    {
                        foreach (var monster in Monsters.Where(x => x.Health > 30 && !x.Name.ToLower().Contains("mini")))
                        {
                            var qpred = spell.GetPrediction(monster);

                            if (!qpred.Collision && qpred.HitChancePercent >= hit && spell.Cast(qpred.CastPosition)) return;                            
                        }
                    }
                    else
                    {
                        foreach (var monster in Monsters.Where(x => x.Health > 30))
                        {
                            var qpred = spell.GetPrediction(monster);

                            if (!qpred.Collision && qpred.HitChancePercent >= slider(pred, "Q1Pred") && spell.Cast(qpred.CastPosition)) return;
                        }
                    }
                }

                if (HasMount() && check(jungleclear, "JE") && E.IsReady())
                {
                    foreach (var monster in Monsters.Where(x => x.Health > 30 && !x.Name.Contains("Mini")))
                    {
                        var epred = E.GetPrediction(monster);

                        if (epred.HitChancePercent >= slider(pred, "EPred") && E.Cast(epred.CastPosition)) return;
                    }
                }
            }
        }

        private static void Misc()
        {
            var target = TargetSelector.GetTarget(1000, DamageType.Magical, Player.Instance.Position);

            if (target != null && target.ValidTarget(Q1.Range)) //fix this bitch
            {
                /*if (check(misc, "ksQ") && myhero.Spellbook.GetSpell(SpellSlot.Q).IsReady && Prediction.Health.GetPrediction(target, 250) > 0 &&
                    Prediction.Health.GetPrediction(target, 250) <= QDamage(target) && HasMount() ? Q1.CanCast(target) : Q2.CanCast(target))
                {
                    var qpred = HasMount() ? Q1.GetPrediction(target) : Q2.GetPrediction(target);

                    if (HasMount() && qpred.CollisionObjects.Where(x => x.IsEnemy).Count() > 0) return;

                    else if (qpred.HitChancePercent >= (HasMount() ? slider(pred, "Q1Pred") : slider(pred, "Q2Pred")) && Q1.Cast(qpred.CastPosition))
                    { return; }
                }*/

                if (Ignite != null && check(misc, "autoign") && Ignite.CanCast(target) && myhero.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite) > target.Health)
                {
                    Ignite.Cast(target);
                }
            }

            if (check(misc, "AUTOPOT") && !myhero.HasBuffOfType(BuffType.Heal) && myhero.HealthPercent <= slider(misc, "POTMIN"))
            {
                if (Item.HasItem(Potion.Id) && Item.CanUseItem(Potion.Id)) Potion.Cast();

                else if (Item.HasItem(Biscuit.Id) && Item.CanUseItem(Biscuit.Id)) Biscuit.Cast();

                else if (Item.HasItem(RPotion.Id) && Item.CanUseItem(RPotion.Id)) RPotion.Cast();
            }

            if (check(misc, "skinhax")) myhero.SetSkinId(comb(misc, "skinID"));        
        }
        #endregion

        

        public static void DatMenu()
        {
            menu = MainMenu.AddMenu("T7 " + ChampionName, ChampionName.ToLower());
            combo = menu.AddSubMenu("Combo", "combo");
            harass = menu.AddSubMenu("Harass", "harass");
            laneclear = menu.AddSubMenu("Laneclear", "lclear");
            jungleclear = menu.AddSubMenu("Jungleclear", "jclear");
            draw = menu.AddSubMenu("Drawings", "draw");
            misc = menu.AddSubMenu("Misc", "misc");
            pred = menu.AddSubMenu("Prediction", "pred");

            menu.AddGroupLabel("Welcome to T7 " + ChampionName + " And Thank You For Using!");
            menu.AddLabel("Version " + Version + " " + Date);
            menu.AddLabel("Author: Toyota7");
            menu.AddSeparator();
            menu.AddLabel("Please Report Any Bugs And If You Have Any Requests Feel Free To PM Me <3");

            combo.AddLabel("Spells");
            combo.Add("CQ", new CheckBox("Use Q"));
            combo.Add("CE", new CheckBox("Use E"));
            combo.AddSeparator();
            combo.Add("ITEMS", new CheckBox("Use Items"));

            harass.AddLabel("Spells");
            harass.Add("HQ", new CheckBox("Use Q", false));
            harass.AddSeparator();
            harass.Add("AUTOHARASS", new KeyBind("Auto Harass", false, KeyBind.BindTypes.PressToggle, 'H'));

            laneclear.AddGroupLabel("Spells");
            laneclear.Add("LQ", new CheckBox("Use Q", false));
            laneclear.Add("LQMIN", new Slider("Min Minions For Mounted Q", 3, 1, 10));
            laneclear.Add("LQMODE", new ComboBox("Dismounted Q Targets", 0,"Big Minions", "All Minions"));
            laneclear.AddSeparator();
            laneclear.Add("LE", new CheckBox("Use E", false));
            laneclear.Add("LEMIN", new Slider("Min Minions For E", 3, 1, 10));
            laneclear.AddSeparator();
            laneclear.Add("AUTOLANECLEAR", new KeyBind("Auto Laneclear", false, KeyBind.BindTypes.PressToggle, 'L'));

            jungleclear.AddGroupLabel("Spells");
            jungleclear.Add("JQ", new CheckBox("Use Q", false));            
            jungleclear.Add("JE", new CheckBox("Use E", false));

            draw.Add("nodraw", new CheckBox("Disable All Drawings", false));
            draw.AddSeparator();
            draw.Add("drawQ", new CheckBox("Draw Q Range"));
            draw.Add("drawE", new CheckBox("Draw E Range"));
            draw.AddSeparator();
            draw.Add("nodrawc", new CheckBox("Draw Only Ready Spells", false));
            draw.Add("drawauto", new CheckBox("Draw Automatic Mode's Status"));

          //  misc.AddLabel("Killsteal");
          //  misc.Add("ksQ", new CheckBox("Killsteal with Q", false));
            if (Ignite != null) misc.Add("autoign", new CheckBox("Auto Ignite If Killable"));
            misc.AddSeparator();
            misc.AddLabel("Anti-Gapcloser");
            misc.Add("QGAP", new CheckBox("Use Q On Gapclosers", false));
            misc.AddSeparator();
            misc.AddLabel("Flee");
            misc.Add("EFLEE", new CheckBox("Use E To Flee"));
            misc.AddSeparator();
            misc.AddLabel("Auto Potion");
            misc.Add("AUTOPOT", new CheckBox("Activate Auto Potion"));
            misc.Add("POTMIN", new Slider("Min Health % To Active Potion", 25, 1, 100));
            misc.AddSeparator();
            misc.AddLabel("Auto Level Up Spells");
            misc.Add("autolevel", new CheckBox("Activate Auto Level Up Spells"));
            misc.AddSeparator();
            misc.AddLabel("Skin Hack");
            misc.Add("skinhax", new CheckBox("Activate Skin hack"));
            misc.Add("skinID", new ComboBox("Skin Hack", 1, "Default", "Sir"));

            pred.AddGroupLabel("Prediction");
            pred.AddLabel("Mounted Q :");
            pred.Add("Q1Pred", new Slider("Select % Hitchance", 90, 1, 100));
            pred.AddSeparator();
            pred.AddLabel("Dismounted Q :");
            pred.Add("Q2Pred", new Slider("Select % Hitchance", 90, 1, 100));
            pred.AddSeparator();
            pred.AddLabel("E :");
            pred.Add("EPred", new Slider("Select % Hitchance", 90, 1, 100));
        }

    }
}
