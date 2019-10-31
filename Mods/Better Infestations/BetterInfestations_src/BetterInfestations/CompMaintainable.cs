using RimWorld;
using Verse;

namespace BetterInfestations
{
	public class CompMaintainable : ThingComp
	{
		public int ticksSinceMaintain;

		public CompProperties_Maintainable Props
		{
			get
			{
				return (CompProperties_Maintainable)props;
			}
		}

		public MaintainableStage CurStage
		{
			get
			{
				if (ticksSinceMaintain < Props.ticksHealthy)
				{
					return MaintainableStage.Healthy;
				}
				if (ticksSinceMaintain < Props.ticksHealthy + Props.ticksNeedsMaintenance)
				{
					return MaintainableStage.NeedsMaintenance;
				}
				return MaintainableStage.Damaging;
			}
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref ticksSinceMaintain, "ticksSinceMaintain", 0, false);
		}

		public override void CompTick()
		{
			base.CompTick();
			ticksSinceMaintain++;
			if (Find.TickManager.TicksGame % 250 == 0)
			{
				CheckTakeDamage();
			}
		}

		public override void CompTickRare()
		{
			base.CompTickRare();
			ticksSinceMaintain += 250;
			CheckTakeDamage();
		}

		private void CheckTakeDamage()
		{
			if (CurStage == MaintainableStage.Damaging)
			{
                if (parent != null)
                {
                    parent.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, Props.damagePerTickRare, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
                }
			}
		}

		public void Maintained()
		{
			ticksSinceMaintain = 0;
		}

		public override string CompInspectStringExtra()
		{
			MaintainableStage curStage = CurStage;
			if (curStage == MaintainableStage.NeedsMaintenance)
			{
				return "DueForMaintenance".Translate();
			}
			if (curStage != MaintainableStage.Damaging)
			{
				return null;
			}
			return "DeterioratingDueToLackOfMaintenance".Translate();
		}
	}
}
