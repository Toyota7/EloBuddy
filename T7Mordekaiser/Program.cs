using System;
using System.Globalization;
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

namespace T7MordeOP
{
    class ΤοΠιλλ
    {
        private static void Main(string[] args) { Loading.OnLoadingComplete += OnLoad; }
        public static AIHeroClient myhero { get { return ObjectManager.Player; } }
        private static Menu menu, combo, harass, laneclear,jungleclear, misc, draw, pred, sequence1, sequence2, sequence3;
        public static Spell.Targeted ignt = new Spell.Targeted(myhero.GetSpellSlotFromName("summonerdot"), 550);
        private static float RAttackDelay = 1200;
        public static void OnLoad(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Mordekaiser") { return; }
            Chat.Print("<font color='#0040FF'>T7</font><font color='#1F1F1F'> Mordekaiser</font> : Loaded!(v1.1)");
            Chat.Print("<font color='#04B404'>By </font><font color='#FF0000'>T</font><font color='#FA5858'>o</font><font color='#FF0000'>y</font><font color='#FA5858'>o</font><font color='#FF0000'>t</font><font color='#FA5858'>a</font><font color='#0040FF'>7</font><font color='#FF0000'> <3 </font>");
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnLevelUp += OnLvlUp;
            Game.OnUpdate += OnUpdate;
           // Gapcloser.OnGapcloser += OnGapcloser
            DatMenu();
            Game.OnTick += OnTick;
            Player.LevelSpell(SpellSlot.E);

        }


        private static void OnTick(EventArgs args)
        {
            if (myhero.IsDead) return;

            var flags = Orbwalker.ActiveModesFlags;
        //    Chat.Print(Ghost.Name);
            if (flags.HasFlag(Orbwalker.ActiveModes.Combo) && (myhero.Health - TotalHealthLoss()) > 100)
            {
                Combo();
            }

            if (flags.HasFlag(Orbwalker.ActiveModes.Harass) && myhero.HealthPercent > harass["hminhealth"].Cast<Slider>().CurrentValue) Harass();

            if (flags.HasFlag(Orbwalker.ActiveModes.LaneClear) && myhero.HealthPercent > laneclear["lminhealth"].Cast<Slider>().CurrentValue) Laneclear();

            if (flags.HasFlag(Orbwalker.ActiveModes.JungleClear) && myhero.HealthPercent > jungleclear["jminhealth"].Cast<Slider>().CurrentValue) Jungleclear();
           
            Misc();

        }

        private static bool check(Menu submenu, string sig)
        {
            return submenu[sig].Cast<CheckBox>().CurrentValue;
        }

        private static void OnLvlUp(Obj_AI_Base sender, Obj_AI_BaseLevelUpEventArgs args)
        {
            if (!sender.IsMe) return;

            /*E>Q>W*/
            SpellSlot[] sequence1 = { SpellSlot.Unknown, SpellSlot.Q, SpellSlot.W, SpellSlot.E,
                                        SpellSlot.Q, SpellSlot.R, SpellSlot.E, SpellSlot.Q, 
                                        SpellSlot.Q, SpellSlot.E, SpellSlot.R, SpellSlot.E, 
                                        SpellSlot.Q, SpellSlot.W, SpellSlot.W, SpellSlot.R, 
                                        SpellSlot.W , SpellSlot.W };

            if (check(misc,"autolevel")) Player.LevelSpell(sequence1[myhero.Level]);
        }

        private static float ComboDamage(AIHeroClient target)
        {
            if (target != null)
            {
                float totaldamage = 0;

                if (DemSpells.Q.IsLearned && DemSpells.Q.IsReady() ) { totaldamage += TotalQDamage(target); }
                if (DemSpells.W.IsLearned && DemSpells.W.IsReady()) { totaldamage += TotalWDamage(target); }
                if (DemSpells.E.IsLearned && DemSpells.E.IsReady() ) { totaldamage += EDamage(target); }
                if (DemSpells.R.IsLearned && DemSpells.R.IsReady() ) { totaldamage += TotalRDamage(target); }

                return totaldamage;
            }
            return 0;
        }

        private static float TotalHealthLoss()
        {
            var HealthLoss = new[] { 0, 20, 23, 26, 29, 32 }[DemSpells.Q.Level] + 
                             new[] { 0, 25, 35, 45, 55, 65 }[DemSpells.W.Level] + 
                             new[] { 0, 24, 36, 48, 60, 72 }[DemSpells.E.Level];
            return HealthLoss;
        }

        private static Obj_AI_Base Ghost
        {
            get
            {
                if (DemSpells.R.Name.ToLower() != "mordekaisercotgguide") return null;

                return ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(m => m.Distance(myhero.Position) < 10000 && m.IsAlly && m.HasBuff("mordekaisercotgpetbuff2"));
            }                       
        }

        private static float TotalQDamage(AIHeroClient target)
        {   
            var qdamage = (new[] { 0, 10, 20, 30, 40, 50 }[DemSpells.Q.Level] +
                          (new[] { 0, 0.5, 0.6, 0.7, 0.8, 0.9 }[DemSpells.Q.Level] * myhero.TotalAttackDamage) +
                          (0.6 * myhero.FlatMagicDamageMod)) +
                          (new[] { 0, 20, 40, 60, 80, 100 }[DemSpells.Q.Level] +                                //T7's Maths OP Kappa pepo :P
                          (new[] { 0, 1, 1.2, 1.4, 1.6, 1.8 }[DemSpells.Q.Level] * myhero.TotalAttackDamage) +
                          (1.2 * myhero.FlatMagicDamageMod));
            return myhero.CalculateDamageOnUnit(target, DamageType.Magical, (float)qdamage);
        }

        private static float TotalWDamage(AIHeroClient target)
        {
            var TotalWDamage = (new[] { 0, 140, 180, 220, 260, 300 }[DemSpells.W.Level] + (0.9 * myhero.FlatMagicDamageMod)) + 
                               (new[] { 0, 50, 85, 120, 155, 190 }[DemSpells.W.Level] + (0.3 * myhero.FlatMagicDamageMod));
            return myhero.CalculateDamageOnUnit(target, DamageType.Magical, (float)TotalWDamage); 
        }

        private static float EDamage(AIHeroClient target)
        {
            var edamage = new[] { 0, 35, 65, 95, 125, 155 }[DemSpells.Q.Level] + 
                          (0.6 * myhero.TotalAttackDamage) + 
                          (0.6 * myhero.FlatMagicDamageMod);
            return myhero.CalculateDamageOnUnit(target, DamageType.Magical, (float)edamage);
        }

        private static float TotalRDamage(AIHeroClient target)
        {
            var TotalRDamage = (new[] { 0, 0.25, 0.3, 0.35 }[DemSpells.R.Level] + 
                               (0.04 * (myhero.FlatMagicDamageMod / 100))) * 
                               target.MaxHealth;
            return myhero.CalculateDamageOnUnit(target, DamageType.Magical, (float)TotalRDamage);
        }

        private static void CastW()
        {
            var allies = EntityManager.Heroes.Allies.Where(x => x.Distance(myhero.Position) < 999);
            if (DemSpells.W.IsReady() && check(combo, "CW") && DemSpells.W.Name.ToLower() != "mordekaisercreepingdeath2")
            {
                
                foreach (var ally in allies.Where(a => !a.IsMe && !a.IsDead && a.CountEnemiesInRange(350) > 0))
                {
                    DemSpells.W.Cast(ally);
                }
                if (myhero.CountEnemiesInRange(400) > 0) DemSpells.W.Cast(myhero);
                
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (myhero.IsDead) return;

            if ( check( combo, "CGhost") )
            {

                var target = TargetSelector.GetTarget(4500, DamageType.Physical, Player.Instance.Position);
                if (target.IsValidTarget() && Ghost != null)
                {
                    if (!(Environment.TickCount >= RAttackDelay))
                    {
                        return;
                    }

                    if (check(combo, "GHOSTCOMBO") && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    {
                        return;
                    }
                       
                    target = TargetSelector.GetTarget(4500, DamageType.Physical, Player.Instance.Position);
                    DemSpells.R.Cast(target);
                              
                    RAttackDelay = Environment.TickCount + Ghost.AttackDelay * 1000;                 
                }
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(1000, DamageType.Magical, Player.Instance.Position);

            if (check(combo, "CQ") && DemSpells.Q.IsReady() && DemSpells.Q.IsLearned && target.Distance(myhero.Position) < myhero.AttackRange) { DemSpells.Q.Cast(); }

            CastW();

            if (check(combo, "CE") && DemSpells.E.IsReady() && DemSpells.E.IsLearned && DemSpells.E.IsInRange(target.Position))
            {
                switch (combo["EMode"].Cast<ComboBox>().CurrentValue)
                {
                    case 0 :
                        var Epred = DemSpells.E.GetPrediction(target);
                        if (Epred.HitChancePercent >= misc["EPred"].Cast<Slider>().CurrentValue) DemSpells.E.Cast(Epred.CastPosition);
                        break;
                    case 1:
                        DemSpells.E.Cast(target.Position);
                        break;
                }
            }

            if (check(combo, "CR") && DemSpells.R.IsReady() && DemSpells.R.IsLearned && DemSpells.R.IsInRange(target.Position) &&
                TotalRDamage(target) > (target.Health + (target.FlatHPRegenMod * 10)))
            {  
                if (!DemSpells.E.IsReady() || EDamage(target) < target.Health)
                {  DemSpells.R.Cast(target);  }
                 
            }

            if (check(combo, "Cignt") && ignt.IsReady() && target.Health > ComboDamage(target) && ignt.IsInRange(target.Position) &&
                myhero.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite) > target.Health &&
                !target.HasBuff("mordekaisercotgpetbuff2"))
            {
                if (target.Distance(myhero.Position) < (DemSpells.E.Range / 2))
                {
                    return;
                }
                else
                {
                    ignt.Cast(target);
                }
            }       
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(1000, DamageType.Magical, Player.Instance.Position);

            if (check(harass, "HQ") && DemSpells.Q.IsReady() && DemSpells.Q.IsLearned && target.Distance(myhero) < myhero.AttackRange) DemSpells.Q.Cast();

            if (check(harass, "HE") && DemSpells.E.IsReady() && DemSpells.E.IsLearned && DemSpells.E.IsInRange(target)) DemSpells.E.Cast(target.Position);

        }

        private static void Laneclear()
        {
            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, myhero.Position, DemSpells.W.Range);

            if (check(laneclear, "LQ") && DemSpells.Q.IsLearned && DemSpells.Q.IsReady() &&
                minions.Where(x => x.Distance(myhero) < myhero.AttackRange).Count() >= laneclear["lminmin1"].Cast<Slider>().CurrentValue) { DemSpells.Q.Cast(); }

            if (check(laneclear, "LE") && DemSpells.E.IsLearned && DemSpells.E.IsReady())
            {     
                foreach (var minion in minions.Where(x => x.Distance(myhero) < DemSpells.E.Range))
                {
                    DemSpells.E.Cast(minion.Position);
                }
            }
        }

        private static void Misc()
        {

            if (check(misc, "skinhack")) myhero.SetSkinId((int)misc["skinID"].Cast<ComboBox>().CurrentValue);
            
            var target = TargetSelector.GetTarget(1000, DamageType.Magical, Player.Instance.Position);

            if (target != null)
            {
                if (check(misc, "ksE") && EDamage(target) > target.Health &&
                    DemSpells.E.IsInRange(target) && DemSpells.E.IsReady() && DemSpells.E.IsLearned &&
                    !target.IsInvulnerable) DemSpells.E.Cast(target);

                if (check(misc, "autoign") && ignt.IsReady() &&
                    ignt.IsInRange(target) && myhero.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite) > target.Health) ignt.Cast(target);
            }

        }

        private static void Jungleclear()
        {
            foreach (var monster in EntityManager.MinionsAndMonsters.GetJungleMonsters(myhero.Position, 1000f))
            {
                if (monster.IsValidTarget())
                {
                    if (DemSpells.Q.IsReady() && check(jungleclear, "JQ")) { DemSpells.Q.Cast(); }
                    if (DemSpells.W.IsReady() && check(jungleclear, "JW")) { DemSpells.W.Cast(myhero); }
                    if (DemSpells.E.IsReady() && check(jungleclear, "JE")) { DemSpells.E.Cast(monster); } 
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {

            if (check(draw, "drawW") && DemSpells.W.Level > 0 && !myhero.IsDead && !check(draw, "nodraw"))
            {

                if (check(draw, "drawonlyrdy")) 
                { Circle.Draw(DemSpells.W.IsOnCooldown ? SharpDX.Color.Transparent : SharpDX.Color.Fuchsia, DemSpells.W.Range, myhero.Position); }

                else if (!check(draw, "drawonlyrdy")) { Circle.Draw(SharpDX.Color.Gray, DemSpells.W.Range, myhero.Position); }

            }

            if (check(draw, "drawE") && DemSpells.E.Level > 0 && !myhero.IsDead && !check(draw, "nodraw"))
            {

                if (check(draw, "drawonlyrdy")) 
                { Circle.Draw(DemSpells.E.IsOnCooldown ? SharpDX.Color.Transparent : SharpDX.Color.Gray, DemSpells.E.Range, myhero.Position); }

                else if (!check(draw, "drawonlyrdy")) { Circle.Draw(SharpDX.Color.Gray, DemSpells.E.Range, myhero.Position); }

            }

            if (check(draw, "drawR") && DemSpells.R.Level > 0 && !myhero.IsDead && !check(draw, "nodraw"))
            {

                if (check(draw, "drawonlyrdy")) 
                { Circle.Draw(DemSpells.R.IsOnCooldown ? SharpDX.Color.Transparent : SharpDX.Color.Gray, DemSpells.R.Range, myhero.Position); }

                else if (!check(draw, "drawonlyrdy")) { Circle.Draw(SharpDX.Color.Gray, DemSpells.R.Range, myhero.Position); }

            }

            if (check(draw, "DRAWGHOSTAA") && Ghost != null && !myhero.IsDead && !check(draw, "nodraw"))
            {
                Circle.Draw(SharpDX.Color.SkyBlue, Ghost.AttackRange, Ghost.Position); 
            }

            if (check(draw, "DRAWGHOSTTIME") && Ghost != null && !myhero.IsDead && !check(draw, "nodraw"))
            {
                foreach( var buff in Ghost.Buffs.Where(x => x.IsValid() && x.Name.Contains("mordekaisercotgpetbuff2")))
                {
                    var endTime = Math.Max(0, buff.EndTime - Game.Time);
                    Drawing.DrawText(Drawing.WorldToScreen(Ghost.Position).X,                   // Credits To Hellsing
                                         Drawing.WorldToScreen(Ghost.Position).Y - 30,
                                         Color.Green, "Time: " + Convert.ToString(endTime, CultureInfo.InvariantCulture));
                }
                
            }
            
            foreach (var enemy in EntityManager.Heroes.Enemies)
            {
                if (check(draw, "drawkillable") && !check(draw, "nodraw") && enemy.IsVisible &&
                    enemy.IsHPBarRendered && !enemy.IsDead && ComboDamage(enemy) > enemy.Health)
                {
                    Drawing.DrawText(Drawing.WorldToScreen(enemy.Position).X,
                                     Drawing.WorldToScreen(enemy.Position).Y - 30,
                                     Color.Green, "Killable With Combo");
                }
                else if (check(draw, "drawkillable") && !check(draw, "nodraw") && enemy.IsVisible &&
                         enemy.IsHPBarRendered && !enemy.IsDead &&
                         ComboDamage(enemy) + myhero.GetSummonerSpellDamage(enemy, DamageLibrary.SummonerSpells.Ignite) > enemy.Health)
                {
                    Drawing.DrawText(Drawing.WorldToScreen(enemy.Position).X, Drawing.WorldToScreen(enemy.Position).Y - 30, Color.Green, "Combo + Ignite");
                }
            }
        }

        private static void DatMenu()
        {
            menu = MainMenu.AddMenu("T7 Morde", "mordy");
            combo = menu.AddSubMenu("Combo", "combo");
            harass = menu.AddSubMenu("Harass", "harass");
            laneclear = menu.AddSubMenu("Laneclear", "lclear");
            jungleclear = menu.AddSubMenu("Jungleclear", "jclear");
            draw = menu.AddSubMenu("Drawings", "draw");
            misc = menu.AddSubMenu("Misc", "misc");

            menu.AddGroupLabel("Welcome to T7 Mordekaiser And Thank You For Using!");
            menu.AddGroupLabel("Version 1.1 12/6/2016");
            menu.AddGroupLabel("Author: Toyota7");
            menu.AddSeparator();
            menu.AddGroupLabel("Please Report Any Bugs And If You Have Any Requests Feel Free To PM Me <3");

            combo.AddGroupLabel("Spells");
            combo.Add("CQ", new CheckBox("Use Q in Combo", true));
            combo.Add("CW", new CheckBox("Use W in Combo", true));
            combo.Add("CE", new CheckBox("Use E in Combo", true));
            combo.Add("CR", new CheckBox("Use R in Combo", true));
            combo.Add("Cignt", new CheckBox("Use Ignite", true));
            combo.AddSeparator();
            combo.AddGroupLabel("Ghost Settings");
            combo.Add("CGhost", new CheckBox("Auto Control Ghost", true));
            combo.Add("GHOSTMODE", new ComboBox("Select Ghost Mode     =>",0,"Fight My Target","Go Attack Enemies"));
            combo.Add("GHOSTCOMBO", new CheckBox("Only Control Drag While In Combo Mode", true));
            combo.Add("GHOSTMIN", new Slider("Dont Harass If More Than X Enemies:",3,1,5));
            combo.AddSeparator();
            combo.AddGroupLabel("E Mode:");
            combo.Add("EMode", new ComboBox("Select Mode", 1, "With Prediction", "Without Prediciton"));
           

            harass.AddGroupLabel("Spells");
            harass.Add("HQ", new CheckBox("Use Q", true));
            harass.Add("HE", new CheckBox("Use E", false));
            harass.AddSeparator();
            harass.AddGroupLabel("Min Mana To Harass");
            harass.Add("hminhealth", new Slider("Stop Harass At % Health", 65, 0, 100));

            laneclear.AddGroupLabel("Spells");
            laneclear.Add("LQ", new CheckBox("Use Q", true));
       //     laneclear.Add("LW", new CheckBox("Use W", false));
            laneclear.Add("LE", new CheckBox("Use E", false));
            laneclear.AddSeparator();
            laneclear.AddGroupLabel("Spell Options");
            laneclear.Add("lminmin1", new Slider("Min Minions To Cast Q", 2, 0, 6));
         //   laneclear.Add("lminmin2", new Slider("Min Minions To Cast E", 1, 0, 6));
            laneclear.AddSeparator();
            laneclear.AddGroupLabel("Stop Laneclear At % Health");
            laneclear.Add("lminhealth", new Slider("%", 65, 0, 100));

            jungleclear.AddGroupLabel("Spells");
            jungleclear.Add("JQ", new CheckBox("Use Q", true));
            jungleclear.Add("JW", new CheckBox("Use W", true));
            jungleclear.Add("JE", new CheckBox("Use E", true));
            jungleclear.Add("jminhealth", new Slider("Stop Jungleclear At % Health",50,0,100));

            draw.Add("nodraw", new CheckBox("Disable All Drawings", false));
            draw.AddSeparator();
            draw.Add("drawW", new CheckBox("Draw W Range", true));
            draw.Add("drawE", new CheckBox("Draw E Range", true));
            draw.Add("drawR", new CheckBox("Draw R Range", true));
            draw.Add("drawkillable", new CheckBox("Draw Killable Enemies", false));
            draw.Add("drawonlyrdy", new CheckBox("Draw Only Ready Spells", false));
            draw.AddSeparator();
            draw.Add("DRAWGHOSTAA", new CheckBox("Draw Ghost's AA Range", true));
            draw.Add("DRAWGHOSTTIME", new CheckBox("Draw Ghost's Remaining Time", true));

            misc.AddGroupLabel("Killsteal");
    //        misc.Add("ksW", new CheckBox("Killsteal with W", false));
            misc.Add("ksE", new CheckBox("Killsteal with E", true));
            misc.Add("autoing", new CheckBox("Auto Ignite If Killable", false));
            misc.AddSeparator();
            misc.AddGroupLabel("Prediction");
            misc.AddGroupLabel("E :");
            misc.Add("EPred", new Slider("Select % Hitchance", 80,0,100));
            misc.AddSeparator();
            misc.AddGroupLabel("Auto Level Up Spells");
            misc.Add("autolevel", new CheckBox("Activate Auto Level Up Spells", true));
            misc.AddSeparator();
            misc.AddGroupLabel("Skin Hack");
            misc.Add("skinhack", new CheckBox("Activate Skin hack"));
            misc.Add("skinID", new ComboBox("Skin Hack", 4, "Default", "Dragon Knight", "Infernal", "Pentakill", "Lord", "King Of Clubs"));

        }
    }


    public static class DemSpells
    {
        public static Spell.Active Q { get; private set; }
        public static Spell.Targeted W { get; private set; }
        public static Spell.Skillshot E { get; private set; }
        public static Spell.Targeted R { get; private set; }

        static DemSpells()
        {
            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Targeted(SpellSlot.W , 1000);
            E = new Spell.Skillshot(SpellSlot.E, 670, SkillShotType.Cone, (int)0.25f, 2000, 12 * 2 * (int)Math.PI / 180); //Credits to Kk2 for cone data
            R = new Spell.Targeted(SpellSlot.R, 650);
        }
    }
}
