using RimWorld;
using System;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace BetterInfestations
{
    public class Insect : Pawn
	{
        public bool worker = false;
        public bool stealFood = false;
        public bool targetColonyFood = false;

        public static readonly string MemoAttackedByEnemy = "InsectAttacked";

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(MainDesc(true));
            if (carryTracker != null && carryTracker.CarriedThing != null)
            {
                stringBuilder.Append("Carrying".Translate() + ": ");
                stringBuilder.AppendLine(carryTracker.CarriedThing.LabelCap);
            }
            string text = null;
            Lord lord = this.GetLord();
            if (lord != null && lord.LordJob != null)
            {
                text = lord.LordJob.GetReport();
            }
            if (jobs.curJob != null)
            {
                try
                {
                    string text2 = jobs.curDriver.GetReport().CapitalizeFirst();
                    if (!text.NullOrEmpty())
                    {
                        text = text + ": " + text2;
                    }
                    else
                    {
                        text = text2;
                    }
                }
                catch (Exception arg)
                {
                    Log.Error("JobDriver.GetReport() exception: " + arg, false);
                }
            }
            if (!text.NullOrEmpty())
            {
                stringBuilder.AppendLine(text);
            }
            stringBuilder.Append(InspectStringPartsFromComps());
            return stringBuilder.ToString().TrimEndNewlines();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref worker, "worker", false, false);
            Scribe_Values.Look(ref stealFood, "stealFood", false, false);
            Scribe_Values.Look(ref targetColonyFood, "targetColonyFood", false, false);
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (!stealFood && !targetColonyFood || stealFood && targetColonyFood)
            {
                Pawn pawn = this as Pawn;
                if (pawn != null)
                {
                    Lord lord = pawn.GetLord();
                    if (lord != null)
                    {
                        if (dinfo.Def.ExternalViolenceFor(this) && dinfo.Instigator != null && dinfo.Instigator.Faction != null && dinfo.Instigator.Faction == Faction.OfPlayer)
                        {
                            lord.ReceiveMemo(MemoAttackedByEnemy);
                        }
                    }
                }
            }
            base.PostApplyDamage(dinfo, totalDamageDealt);
        }
    }
}
