using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using SharpDX;
using SharpDX.Direct3D9;

namespace AutoDeward
{
    internal class Deward
    {
        private static Item quellingBlade;
        private static Item tango;
        private static Team enemyTeam = Team.Neutral;

        private static int sleepTime;


        public static void Init()
        {
            Game.OnUpdate += GameOnUpdate;
        }

        private static void GameOnUpdate(EventArgs eventArgs)
        {
            var me = ObjectMgr.LocalHero;

            if (!Game.IsInGame || me == null) return;
            if (sleepTime > 0)
            {
                sleepTime--;
                return;
            }

            enemyTeam = me.Team == Team.Dire ? Team.Radiant : Team.Dire;

            quellingBlade = me.Inventory.Items.FirstOrDefault(i => i.ClassID == ClassID.CDOTA_Item_QuellingBlade);
            tango =
                me.Inventory.Items.FirstOrDefault(
                    i => i.ClassID == ClassID.CDOTA_Item_Tango || i.ClassID == ClassID.CDOTA_Item_Tango_Single);

            var canDeward = (quellingBlade != null || tango != null);

            var wards = ObjectMgr.GetEntities<Unit>()
                .Where(u => (u.ClassID == ClassID.CDOTA_NPC_Observer_Ward || u.ClassID == ClassID.CDOTA_NPC_Observer_Ward_TrueSight) && u.Team == enemyTeam && u.IsAlive && Vector3.Distance(me.Position, u.Position) < 475).ToList();

            if (canDeward)
            {
                Item dewardItem = quellingBlade;

                // is using a tango worth it?
                // tango heals 230 if used on ward, 115 if used on tree
                // TODO: check if already under tango effect
                if (tango != null && quellingBlade != null)
                {
                    var hpMissing = me.MaximumHealth - me.Health;
                    if (hpMissing > 120)
                    {
                        dewardItem = tango;
                    }
                }
                else if (quellingBlade == null)
                {
                    dewardItem = tango;
                }


                if (wards.Count > 0 && dewardItem.Cooldown == 0)
                {
                    dewardItem.UseAbility(wards[0]);
                    Console.WriteLine("Dewarded a ward");
                    sleepTime = 10;
                }
            }
        }
    }
}