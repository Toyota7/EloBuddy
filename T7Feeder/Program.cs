using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Enumerations;
using SharpDX;

namespace T7_Feeder
{
    public class Ability
    {
        public string Name { get; set; }
        public List<SpellSlot> SpellSlots { get; set; }
    }
    class Program
    {    
        private static void Main(string[] args) { Loading.OnLoadingComplete += OnLoad; }
        public static AIHeroClient myhero { get { return ObjectManager.Player; } }
        private static Menu menu;

        private static Spell.Active Ghost = null;
        private static Spell.Active Heal = null;

        private static bool Chatted;
        private static bool TopPointReached = false;
        private static bool BotPointReached = false;
        private static bool MidPointReached = false;
        private static bool SayNoEnemies = false;
        private static bool FinishedBuild = false;

        private static readonly Vector3 OrderSpawn = new Vector3(394, 461, 171);
        private static readonly Vector3 ChaosSpawn = new Vector3(14340, 14391, 179);
        private static readonly Vector3 TopPoint = new Vector3(3142, 13402, 52.8381f);
        private static readonly Vector3 BotPoint = new Vector3(13498, 3284, 51);
        private static readonly Vector3 MidPoint = new Vector3(4131, 4155, 115);

        private static string[] Messages = { "wat", "how?" , "mate..", "-_-", "why?", "laaaaag", "oh my god this lag is unreal",
                                             "rito pls 500 ping", "sorry lag", "help pls", "nooob wtf", "team???", "i can't carry dis", 
                                             "wtf how?", "wow rito nerf pls", "omg so op", "what's up with this lag?", "is the server lagging again?",
                                             "i call black magic", "pls fix rito", "this champ is bad", "i was afk", "so lucky", "much wow" };
        private static string[] Chats = { "/all", " " };

        private static List<Ability> ChampList = new List<Ability>()
        {
            new Ability
                {
                    Name = "Blitzcrank",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                },
            new Ability
                {
                    Name = "Bard",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                },
            new Ability
                {
                    Name = "DrMundo",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.R }
                },
            new Ability
                {
                    Name = "Draven",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                },
            new Ability
                {
                    Name = "Evelynn",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                },
            new Ability
                {
                    Name = "Garen",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.Q }
                },
            new Ability
                {
                    Name = "Hecarim",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.E }
                },
            new Ability
                {
                    Name = "Karma",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.E }
                },
            new Ability
                {
                    Name = "Kayle",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                },
            new Ability
                {
                    Name = "Kennen",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.E }
                },
            new Ability
                {
                    Name = "Lulu",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                },
            new Ability
                {
                    Name = "MasterYi",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.R }
                },
            new Ability
                {
                    Name = "Nunu",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                },
            new Ability
                {
                    Name = "Olaf",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.R }
                },
            new Ability
                {
                    Name = "Orianna",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                },
            new Ability
                {
                    Name = "Poppy",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                },
            new Ability
                {
                    Name = "Quinn",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.R }
                },
            new Ability
                {
                    Name = "Rammus",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.Q }
                },
            new Ability
                {
                    Name = "Rumble",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                },
            new Ability
                {
                    Name = "Ryze",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.R }
                },
            new Ability
                {
                    Name = "Shyvana",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                },
            new Ability
                {
                    Name = "Singed",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.R }
                },
            new Ability
                {
                    Name = "Sivir",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.R }
                },
            new Ability
                {
                    Name = "Skarner",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                },
            new Ability
                {
                    Name = "Sona",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.E }
                },
            new Ability
                {
                    Name = "Teemo",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                },
            new Ability
                {
                    Name = "Trundle",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                },
            new Ability
                {
                    Name = "Twitch",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.Q }
                },
            new Ability
                {
                    Name = "Udyr",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.E }
                },
            new Ability
                {
                    Name = "Volibear",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.Q }
                },
            new Ability
                {
                    Name = "Zilean",
                    SpellSlots = new List<SpellSlot>() { SpellSlot.W, SpellSlot.E }
                }
        };

        public static void OnLoad(EventArgs args)
        {
            Chat.Print("<font color='#0040FF'>T7</font><font color='#09FF00'> Feeder</font> : Loaded!(v1.2)");
            Chat.Print("<font color='#04B404'>By </font><font color='#FF0000'>T</font><font color='#FA5858'>o</font><font color='#FF0000'>y</font><font color='#FA5858'>o</font><font color='#FF0000'>t</font><font color='#FA5858'>a</font><font color='#0040FF'>7</font><font color='#FF0000'> <3 </font>");
            Game.OnTick += OnTick;
            DatMenu();
            Thread.Sleep(15500);           
        }

        private static void OnTick(EventArgs args)
        {
            Checks();
            if (check(menu, "ABILITIES") && !myhero.IsDead) Abilities();
            if (check(menu, "SPELLS") && check(menu, "ACTIVE") && !myhero.IsDead) SummonerSpells();
            if (combocheck(menu, "MSGS") != 0) ChatOnDeath();
            if (check(menu, "AUTOBUY") && !FinishedBuild) Shopping();
            if (check(menu, "ACTIVE") && !myhero.IsDead) Feed();
        }

        private static bool check(Menu menu,string sig)
        {
            return menu[sig].Cast<CheckBox>().CurrentValue;
        }

        private static int slider(Menu menu, string sig)
        {
            return menu[sig].Cast<Slider>().CurrentValue;
        }

        private static int combocheck(Menu menu, string sig)
        {
            return menu[sig].Cast<ComboBox>().CurrentValue;
        }
        
        private static void SummonerSpells()
        {
            var ghost = myhero.Spellbook.Spells.Where(x => x.Name.Contains("Haste"));
            SpellDataInst ghosty = ghost.Any() ? ghost.First() : null;
            if (ghosty != null) Ghost = new Spell.Active(ghosty.Slot);                                                                                                                     

            var heal = myhero.Spellbook.Spells.Where(x => x.Name.Contains("Heal"));
            SpellDataInst Healy = heal.Any() ? heal.First() : null;
            if (Healy != null) Heal = new Spell.Active(Healy.Slot);                                      

            if (Healy != null && Heal.IsReady() && !myhero.HasBuff("SRHomeguardSpeed") && !myhero.IsDead) Heal.Cast();
            
            if (ghosty != null && Ghost.IsReady() && !myhero.HasBuff("SRHomeguardSpeed") && !myhero.IsDead) Ghost.Cast();            
        }

        private static void Shopping()
        { //best spaghetti ever made
            if (Shop.CanShop)
            {
                if (!Item.HasItem(ItemId.Boots_of_Speed) && !Item.HasItem(ItemId.Boots_of_Mobility) && myhero.Gold >= 300)
                    Shop.BuyItem(ItemId.Boots_of_Speed);

                else if (Item.HasItem(ItemId.Boots_of_Speed) && !Item.HasItem(ItemId.Boots_of_Mobility) && myhero.Gold >= 600)
                    Shop.BuyItem(ItemId.Boots_of_Mobility);

                else if (Item.HasItem(ItemId.Boots_of_Mobility) && !Item.HasItem(ItemId.Aether_Wisp) && myhero.Gold >= 850)
                    Shop.BuyItem(ItemId.Aether_Wisp);

                else if (Item.HasItem(ItemId.Aether_Wisp) && !Item.HasItem(ItemId.Serrated_Dirk) && myhero.Gold >= 1100)
                    Shop.BuyItem(ItemId.Serrated_Dirk);

                else if (Item.HasItem(ItemId.Serrated_Dirk) && !Item.HasItem(ItemId.Raptor_Cloak) && myhero.Gold >= 1200)
                    Shop.BuyItem(ItemId.Raptor_Cloak);

                else if (Item.HasItem(ItemId.Raptor_Cloak) && !Item.HasItem(ItemId.Zeal) && myhero.Gold >= 1200)
                    FinishedBuild = Shop.BuyItem(ItemId.Zeal);
            }
        }

        private static void Checks()
        {
            if (myhero.IsDead)
            {
                TopPointReached = false;
                BotPointReached = false;
                MidPointReached = false;
            }
            if (!myhero.IsDead) Chatted = false;

            if (!check(menu, "ACTIVE")) SayNoEnemies = false;
        }

        private static void ChatOnDeath()
        {
            if (myhero.IsDead && Chatted == false)
            {                
                    switch (combocheck(menu, "MSGS"))
                    {
                        case 1:
                            var Random1 = new Random();
                            Core.DelayAction(delegate { Chat.Say("/all " + Messages[Random1.Next(0, 23)]); }, slider(menu, "CHATDELAY") * 1000);
                            Chatted = true;
                            break;
                        case 2:
                            var Random2 = new Random();
                            Core.DelayAction(delegate { Chat.Say(Messages[Random2.Next(0, 23)]); }, slider(menu, "CHATDELAY") * 1000);
                            Chatted = true;
                            break;
                        case 3:
                            var Random3a = new Random();
                            var Random3b = new Random();
                            Core.DelayAction(delegate { Chat.Say(Chats[Random3b.Next(0, 1)] + " " + Messages[Random3a.Next(0, 23)]); }, slider(menu, "CHATDELAY") * 1000);
                            Chatted = true;
                            break;
                    }
            }
        }

        private static void Abilities()
        {
            var champ = ChampList.FirstOrDefault(h => h.Name == myhero.ChampionName);

            if (champ == null) return;
            
            foreach (var slot in champ.SpellSlots)
            {
                if (myhero.Spellbook.GetSpell(slot).IsUpgradable) Player.LevelSpell(slot);

                if (myhero.Spellbook.GetSpell(slot).IsReady) Player.CastSpell(slot, myhero.Position);          
            }
        }
        
        private static void Feed()
        {
            switch(combocheck(menu,"MODE"))
            {
                case 0:
                    if (EntityManager.Heroes.Enemies.Count < 1)
                    {
                        if(SayNoEnemies == false)
                        {
                            Chat.Print("<font color='#FF0000'>WARNING:</font> No Enemies Found To Feed!");
                            SayNoEnemies = true;
                        }
                        return;
                    }

                    if (EntityManager.Heroes.Enemies.Where(x => !x.IsDead && x.IsValidTarget())
                                                    .OrderBy(y => y.Distance(myhero.Position))
                                                    .FirstOrDefault()
                                                    .IsValidTarget())
                    {
                        Orbwalker.MoveTo(EntityManager.Heroes.Enemies.Where(x => !x.IsDead && x.IsValidTarget())
                                                                     .OrderBy(y => y.Distance(myhero.Position))
                                                                     .FirstOrDefault()
                                                                     .Position);
                    }
                    break;
                case 1:
                    if (!TopPointReached)
                    {
                        Orbwalker.MoveTo(TopPoint);
                        if (myhero.Distance(TopPoint) <= 100) TopPointReached = true;
                    }
                    else 
                    {
                        if (myhero.Team == GameObjectTeam.Order) Orbwalker.MoveTo(ChaosSpawn);
                        else Orbwalker.MoveTo(OrderSpawn);
                    }
                    break;
                case 2:
                    if (myhero.IsInShopRange())
                    {
                        if (myhero.Team == GameObjectTeam.Order) Orbwalker.MoveTo(ChaosSpawn);
                        else Orbwalker.MoveTo(OrderSpawn);
                    }
                    else
                    {
                        if (!MidPointReached)
                        {
                            Orbwalker.MoveTo(MidPoint);
                            if (myhero.Distance(MidPoint) <= 100) MidPointReached = true;
                        }
                        else
                        {
                            if (myhero.Team == GameObjectTeam.Order) Orbwalker.MoveTo(ChaosSpawn);
                            else Orbwalker.MoveTo(OrderSpawn);
                        }
                    }                   
                    break;
                case 3:
                    if (!BotPointReached)
                    {
                        Orbwalker.MoveTo(BotPoint);
                        if (myhero.Distance(BotPoint) <= 100) BotPointReached = true;
                    }
                    else
                    {
                        if (myhero.Team == GameObjectTeam.Order) Orbwalker.MoveTo(ChaosSpawn);
                        else Orbwalker.MoveTo(OrderSpawn);
                    }
                    break;

                
            }
        }

        private static void DatMenu()
        {
            menu = MainMenu.AddMenu("T7 Feeder", "feederkappa");

            menu.AddGroupLabel("Welcome to T7 Feeder And Thank You For Using! Kappa");
            menu.AddGroupLabel("Version 1.2 6/6/17");
            menu.AddGroupLabel("Author: Toyota7");
            menu.AddSeparator();
            menu.Add("ACTIVE", new CheckBox("Active", true));
            menu.Add("MODE", new ComboBox("Feed Mode =>", 2, "Closest Enemy", "Top", "Mid", "Bot"));
            menu.Add("MSGS", new ComboBox("Chat On Death",0,"Off","/all Chat","Team Chat","Random Chat"));
            menu.Add("CHATDELAY", new Slider("Chat Delay After Death(seconds)", 4, 1, 7));
            menu.Add("SPELLS", new CheckBox("Use Summoner Spells", false));
            menu.Add("ABILITIES", new CheckBox("Use Abilities", true));
            menu.Add("AUTOBUY", new CheckBox("Auto Buy Items", true));
            menu.AddSeparator();
            menu.AddGroupLabel("Please Report Any Bugs And If You Have Any Requests Feel Free To PM Me <3");
        }

    }
}
