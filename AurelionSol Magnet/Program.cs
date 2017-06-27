using System;
using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace AurelionSol_Magnet
{
    class Program
    {
        static void Main(string[] args) { Loading.OnLoadingComplete += OnLoad; }
        static AIHeroClient myhero { get { return ObjectManager.Player; } }

        static readonly string WBuff = "aurelionsolwactive", WStarNames = "AurelionSol_Base_P_MissileLarge.troy", RingTransformationName = "AurelionSol_Base_P_Ring_OuterToInner.troy";

        static List<GameObject> Stars = new List<GameObject>();

        static Menu menu;

        static int RingDist = 590, StarsBR = 126;

        private static void OnLoad(EventArgs args)
        {
            if (myhero.ChampionName != "AurelionSol") return;

            Game.OnUpdate += OnUpdate;
            GameObject.OnCreate += GameObject_OnCreate;
            
            Menu();
        }
        
        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == WStarNames) Stars.Add(sender);
            
            if (sender.Name == RingTransformationName) Stars.Clear();         
        }

        private static void OnUpdate(EventArgs args)
        {
            if (myhero.IsDead) return;

            if (menu.Get<KeyBind>("KEY").CurrentValue) Magnet();
        }

        static void Magnet()
        {
            var target = TargetSelector.GetTarget(1200, DamageType.Magical, Player.Instance.Position);

            if (target != null && target.IsValid && target.IsVisible && myhero.CanMove)
            {
                var dist = myhero.Distance(target.Position);
                var stardist = target.Distance(myhero.Position.Extend(target.Position, RingDist));

                if (stardist > StarsBR - 10 && dist < RingDist)
                {
                    Player.IssueOrder(GameObjectOrder.MoveTo, myhero.Position.Shorten(target.Position, stardist));
                }
                else if (stardist > StarsBR - 10 && dist > RingDist)
                {

                    Player.IssueOrder(GameObjectOrder.MoveTo, myhero.Position.Extend(target.Position, stardist + 100).To3D());
                }
            }
            else Player.IssueOrder(GameObjectOrder.MoveTo, Game.ActiveCursorPos);
        }

        static void Menu()
        {
            menu = MainMenu.AddMenu("W Magnet", "magnet");

            menu.AddGroupLabel("Hotkey");
            menu.Add("KEY", new KeyBind("W Magnet", false, KeyBind.BindTypes.HoldActive, 'M'));
            menu.AddLabel("Hold The Key And The Script Will Choose A Target To Stick To Via TargetSelector");
        }
    }
}
