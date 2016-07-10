using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common.Menu;
using Ensage.Common.Menu.MenuItems;
using SharpDX;
using SharpDX.Direct3D9;

namespace AutoDeward {
    internal class Deward {
        private static Item quellingBlade;
        private static Item tango;

        private static int sleepTime;

        private static Menu menu;

        private static readonly string[] breakableInvisModifiers = {
            "modifier_nyx_assassin_vendetta",
            "modifier_invoker_ghost_walk",
            "modifier_rune_invis",
            "modifier_item_invisibility_edge_windwalk",
            "modifier_item_silver_edge_windwalk",
            "modifier_bounty_hunter_wind_walk",
            "modifier_clinkz_wind_walk",
            "modifier_weaver_shukuchi"
        };

        public static void Init() {
            menu = new Menu("AutoDeward", "autodeward", true);
            menu.AddItem(new OnOffSlider("enabled", "Enabled", true));
            menu.AddItem(new OnOffSlider("dontBreakInvisibility", "Don't break invisibility", true));
            menu.AddToMainMenu();
            Game.OnUpdate += GameOnUpdate;
        }

        private static void GameOnUpdate(EventArgs eventArgs) {
            if (!menu.Item("enabled").GetValue<bool>()) return;

            var me = ObjectManager.LocalHero;

            if (!Game.IsInGame || me == null) return;
            if (sleepTime > 0) {
                sleepTime--;
                return;
            }

            sleepTime = 10;

            if (menu.Item("dontBreakInvisibility").GetValue<bool>()) {
                var modifiers = me.Modifiers;
                foreach (var modifier in modifiers) {
                    foreach (var breakableInvisModifier in breakableInvisModifiers) {
                        if (modifier.Name == breakableInvisModifier) {
                            return;
                        }
                    }
                }
            }

            quellingBlade =
                me.Inventory.Items.FirstOrDefault(
                    i => i.ClassID == ClassID.CDOTA_Item_QuellingBlade || i.ClassID == ClassID.CDOTA_Item_Battlefury);
            tango =
                me.Inventory.Items.FirstOrDefault(
                    i => i.ClassID == ClassID.CDOTA_Item_Tango || i.ClassID == ClassID.CDOTA_Item_Tango_Single);

            var units = ObjectManager.GetEntities<Unit>();

            var wards = units
                .Where(
                    u => (u.ClassID == ClassID.CDOTA_NPC_Observer_Ward || u.ClassID == ClassID.CDOTA_NPC_Observer_Ward_TrueSight) && u.Team != me.Team && u.IsAlive && Vector3.Distance(me.Position, u.Position) < 475).ToList();

            var mines = units.Where(u => u.ClassID == ClassID.CDOTA_NPC_TechiesMines && u.Team != me.Team && u.IsAlive && Vector3.Distance(me.NetworkPosition, u.NetworkPosition) < 475).ToList();

            var canDewardWard = ((quellingBlade != null || tango != null) && wards.Count > 0);
            var canDewardMine = ((quellingBlade != null) && mines.Count > 0);

            if (canDewardWard && me.IsAlive) {
                var dewardItem = quellingBlade;

                // is using a tango worth it?
                // tango heals 230 if used on ward, 115 if used on tree
                if (tango != null && quellingBlade != null && me.Modifiers.All(m => m.Name != "modifier_tango_heal")) {
                    var hpMissing = me.MaximumHealth - me.Health;
                    if (hpMissing > 115 || quellingBlade.Cooldown > 0) dewardItem = tango;
                }
                else if (quellingBlade != null && quellingBlade.Cooldown > 0) dewardItem = tango;
                else if (quellingBlade == null) dewardItem = tango;


                if (dewardItem.Cooldown == 0) {
                    dewardItem.UseAbility(wards[0]);
                }
            }

            if (canDewardMine && me.IsAlive) {
                if (quellingBlade.Cooldown == 0) {
                    quellingBlade.UseAbility(mines[0]);
                }
            }
        }
    }
}