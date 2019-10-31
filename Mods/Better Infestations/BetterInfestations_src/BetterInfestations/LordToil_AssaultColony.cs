using RimWorld;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace BetterInfestations
{
	public class LordToil_AssaultColony : LordToil
	{
        public float distToQueenToAttack = 32f;

        public override bool ForceHighStoryDanger
		{
			get
			{
				return true;
			}
		}

		public override bool AllowSatisfyLongNeeds
		{
			get
			{
				return false;
			}
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
			}
            Queen queen = HiveUtility.FindQueen(lord.ownedPawns.RandomElement());
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                if (queen != null)
                {
                    if (!HiveUtility.queenKindDef.Contains(lord.ownedPawns[i].kindDef))
                    {
                        lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
                    }
                    else
                    {
                        lord.ownedPawns[i].mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("BI_QueenDuty"), queen, distToQueenToAttack);
                    }
                }
            }
        }
	}
}
