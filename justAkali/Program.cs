#region
using System;
using System.Collections;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using System.Collections.Generic;
using System.Threading;
#endregion

namespace SF_Template
{


    internal class program
    {

        private const string Champion = "Akali"; 

        private static Orbwalking.Orbwalker Orbwalker; 

        private static Spell Q;

        private static Spell W; 

        private static Spell E; 

        private static Spell R; 

        private static List<Spell> SpellList = new List<Spell>(); 

        private static Menu Config; 

        private static Items.Item DFG;
        private static Items.Item Hextech;
        private static Items.Item ZHO; 

        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } } 


        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad; 

        }


        static void Game_OnGameLoad(EventArgs args)
        {

            if (ObjectManager.Player.BaseSkinName != Champion) return;


            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 800);

            E.SetSkillshot(700f, 0, 0, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            DFG = new Items.Item(3128, 750f);
            Hextech = new Items.Item(3146, 700f);
            ZHO = new Items.Item(3157, 0f);
   
            Config = new Menu("justAkali", "String_Name", true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Combo Menu
            Config.AddSubMenu(new Menu("Combo", "Combo")); //Creating a submenu
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true); 
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true); 
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true); 
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R")).SetValue(true);
            Config.SubMenu("Combo").AddSubMenu(new Menu("Items", "Items"));
            Config.SubMenu("Combo").SubMenu("Items").AddItem(new MenuItem("HextechUse", "Hextech")).SetValue(true);
            Config.SubMenu("Combo").SubMenu("Items").AddItem(new MenuItem("UseDFG", "DFG")).SetValue(true);
            Config.SubMenu("Combo").SubMenu("Items").AddItem(new MenuItem("MinHPDFG", "Min enemy Health DFG")).SetValue(new Slider(50, 0, 100));
            Config.SubMenu("Combo").SubMenu("Items").AddItem(new MenuItem("ZHOUse", "Zhonya")).SetValue(true);
            Config.SubMenu("Combo").SubMenu("Items").AddItem(new MenuItem("ZHOMin", "Health for Zhonya")).SetValue(new Slider(20, 0, 100));
            Config.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Harras", "Harras"));
            Config.SubMenu("Harras").AddItem(new MenuItem("UseQHarras", "Use Q")).SetValue(true);
            Config.SubMenu("Harras").AddItem(new MenuItem("UseEHarras", "Use E")).SetValue(true);
            Config.SubMenu("Harras").AddItem(new MenuItem("HarrasKey", "Harras!").SetValue(new KeyBind(67, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("KSSystem", "KSSystem")); //Creating a submenu
            Config.SubMenu("KSSystem").AddItem(new MenuItem("UseKS", "On/Off")).SetValue(true);
            Config.SubMenu("KSSystem").AddItem(new MenuItem("UseQKS", "Use Q")).SetValue(true);
            Config.SubMenu("KSSystem").AddItem(new MenuItem("UseEKS", "Use W")).SetValue(true);
            Config.SubMenu("KSSystem").AddItem(new MenuItem("UseRKS", "Use E")).SetValue(true);
            Config.SubMenu("KSSystem").AddItem(new MenuItem("UseComboKS", "Use Combo")).SetValue(true);
            Config.SubMenu("KSSystem").AddItem(new MenuItem("ItemsOnlyIn", "Items only with Combo"));
            Config.SubMenu("KSSystem").AddItem(new MenuItem("UseDFGKS", "Use DFG")).SetValue(true);
            Config.SubMenu("KSSystem").AddItem(new MenuItem("HextechUseKS", "Use Hextech")).SetValue(true);

            //Range Drawings same concept as the Combo menu
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "Draw R")).SetValue(true);
      
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("PacketCasting", "Use Packets?")).SetValue(true);

            Config.AddToMainMenu();
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Game.PrintChat("Loaded justAkali #MixX");

        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (Config.Item("ActiveCombo").GetValue<bool>()) Combo();
            if (Config.Item("HarrasKey").GetValue<bool>()) Harras();
            if (Config.Item("UseKS").GetValue<bool>()) KSSystem(); 
            if (Config.Item("ZHOUse").GetValue<bool>())
            {
                if (ZHO.IsReady() && (Player.Health / Player.MaxHealth) * 100 <= Config.Item("ZHOMin").GetValue<Slider>().Value)
                {
                    ZHO.Cast();
                }
            }
        }

        private static void Combo()
        {
            var target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);

            if (Q.IsReady() && target.IsValidTarget(Q.Range) && Config.Item("UseQCombo").GetValue<bool>())
            {
                Q.Cast(target, PacketCast());
            }
            if (E.IsReady() && target.IsValidTarget(E.Range) && Config.Item("UseECombo").GetValue<bool>())
            {
                E.Cast(target, PacketCast());
            }
            if (R.IsReady() && target.IsValidTarget(R.Range) && Config.Item("UseRCombo").GetValue<bool>())
            {
                R.Cast(target, PacketCast());
            }
            if (W.IsReady() && Config.Item("UseWCombo").GetValue<bool>())
            {
                var prediction = W.GetPrediction(target, true, 800);
                W.Cast(prediction.CastPosition, PacketCast());
            }
            if (DFG.IsReady() && Config.Item("UseDFG").GetValue<bool>() && Player.Distance(target) <= DFG.Range)
            {
                if ((target.Health / target.MaxHealth) * 100 <= Config.Item("MinHPDFG").GetValue<Slider>().Value)
                {
                    DFG.Cast(target);
                }
            }
            if (Hextech.IsReady() && Config.Item("HextechUse").GetValue<bool>() && Player.Distance(target) <= Hextech.Range)
            {
                Hextech.Cast(target);
            }
        }

        public static void Harras()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);

            if (Q.IsReady() && target.IsValidTarget(Q.Range) && Config.Item("UseQHarras").GetValue<bool>())
            {
                Q.Cast(target, PacketCast());
            }
            if (E.IsReady() && target.IsValidTarget(E.Range) && Config.Item("UseEHarras").GetValue<bool>())
            {
                E.Cast(target, PacketCast());
            }
        }

        private static bool PacketCast()
        {
            return Config.Item("PacketCasting").GetValue<bool>();
        }

        private static void KSSystem()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);

            double damageQ = Player.GetSpellDamage(target, SpellSlot.Q);
            double damageE = Player.GetSpellDamage(target, SpellSlot.E);
            double damageR = Player.GetSpellDamage(target, SpellSlot.R);
            double DFGdamage = Player.GetItemDamage(target, Damage.DamageItems.Dfg);
            double Hextechdamage = Player.GetItemDamage(target, Damage.DamageItems.Hexgun);
            double FullComboKS = damageQ + damageE + damageR;
            
            if (Q.IsReady() && target.IsValidTarget(Q.Range) && Config.Item("UseQKS").GetValue<bool>() && target.Health <= damageQ)
            {
                Q.Cast(target, PacketCast());
            }
            if (E.IsReady() && target.IsValidTarget(E.Range) && Config.Item("UseEKS").GetValue<bool>() && target.Health <= damageE)
            {
                E.Cast(target, PacketCast());
            }
            if (R.IsReady() && target.IsValidTarget(R.Range) && Config.Item("UseRKS").GetValue<bool>() && target.Health <= damageR)
            {
                R.Cast(target, PacketCast());
            }
            if (target.IsValidTarget(R.Range) && Config.Item("UseComboKS").GetValue<bool>() && target.Health <= FullComboKS)
            {
                if (R.IsReady() && target.IsValidTarget(R.Range))
                {
                    R.Cast(target, PacketCast());
                }
                if (Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target, PacketCast());
                }
                if (E.IsReady() && target.IsValidTarget(E.Range))
                {
                    E.Cast(target, PacketCast());
                }
                if (DFG.IsReady() && Config.Item("UseDFGKS").GetValue<bool>() && Player.Distance(target) <= DFG.Range)
                {
                    DFG.Cast(target);
                }
                if (Hextech.IsReady() && Config.Item("HextechUseKS").GetValue<bool>() && Player.Distance(target) <= Hextech.Range)
                {
                    Hextech.Cast(target);
                }
            }

        }

        public static void LaneClear()
        {
            var minion = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
            var minionTarget = minion.First();
            if (Q.IsReady())
            {
                Q.Cast(minionTarget, PacketCast());
            }
        }

        private static void OnDraw(EventArgs args)
        {
            bool drawQ = Config.Item("DrawQ").GetValue<bool>();
            bool drawE = Config.Item("DrawE").GetValue<bool>();
            bool drawR = Config.Item("DrawR").GetValue<bool>();

            if (drawQ) Utility.DrawCircle(Player.Position, Q.Range, Color.Coral, 5, 30, false);
            if (drawE) Utility.DrawCircle(Player.Position, E.Range, Color.DarkBlue, 5, 30, false);
            if (drawR) Utility.DrawCircle(Player.Position, R.Range, Color.DarkGreen, 5, 30, false);
        }


    }
}