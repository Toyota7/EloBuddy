using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;

namespace Ignite_Helper
{
    class Program
    {
        private static void Main(string[] args) { Loading.OnLoadingComplete += OnLoad; }
        public static AIHeroClient myhero { get { return ObjectManager.Player; } }
        public static Spell.Targeted ignt = new Spell.Targeted(myhero.GetSpellSlotFromName("summonerdot"), 600);
        private static Menu menu;
        public static void OnLoad(EventArgs args)
        {
            if (ignt.Slot == SpellSlot.Unknown) return;
            Chat.Print("<font color='#ff0000'>Ignite</font>Helper : Loaded!");
            Chat.Print("<font color='#04B404'>By </font><font color='#FF0000'>T</font><font color='#FA5858'>o</font><font color='#FF0000'>y</font><font color='#FA5858'>o</font><font color='#FF0000'>t</font><font color='#FA5858'>a</font><font color='#0040FF'>7</font><font color='#FF0000'> <3 </font>");
            Menu();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw =>
            {
                if (menu["draw"].Cast<CheckBox>().CurrentValue && ignt.IsReady())
                {
                    Circle.Draw(SharpDX.Color.Red, ignt.Range, myhero.Position);
                }
            };
        }

        private static void OnUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(600, DamageType.True, Player.Instance.Position);

            float IgniteDMG = 50 + (20 * myhero.Level);                   
              
            if (target != null &&
                menu["active"].Cast<CheckBox>().CurrentValue && ignt.IsReady() && target.IsValidTarget(ignt.Range) &&
                IgniteDMG > (target.TotalShieldHealth() + target.HPRegenRate))
            {
                if (myhero.IsRanged && myhero.IsAttackingPlayer && myhero.IsFacing(target) && target.Distance(myhero.Position) > (myhero.AttackRange) * 0.75f)
                    return; //Avoid Wasting It

                ignt.Cast(target);
            }          
        }

        private static void Menu()
        {
            menu = MainMenu.AddMenu("Ignite Helper", "ignitemenu");
            menu.AddGroupLabel("Welcome to Ignite Helper and thank you for using! :)");
            menu.AddGroupLabel("Author: Toyota7");
            menu.AddGroupLabel("Version: 1.2");
            menu.AddSeparator();
            menu.Add("active", new CheckBox("Use Ignite", true));
            menu.Add("draw", new CheckBox("Draw ignite Range", false));
        }
    }
}
