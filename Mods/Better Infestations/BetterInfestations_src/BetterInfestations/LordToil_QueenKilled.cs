using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace BetterInfestations
{
	public class LordToil_QueenKilled : LordToil
	{
		public override void UpdateAllDuties()
		{
            for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
                if (!HiveUtility.queenKindDef.Contains(lord.ownedPawns[i].kindDef))
                {
                    lord.ownedPawns[i].mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("BI_QueenKilledDuty"));
                }
			}
		}
	}
}
