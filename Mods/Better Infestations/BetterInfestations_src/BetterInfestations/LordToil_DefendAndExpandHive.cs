using System.Linq;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace BetterInfestations
{
    public class LordToil_DefendAndExpandHive : LordToil
    {
        public float distToQueenToAttack = 32f;

        public override void UpdateAllDuties()
        {
            List<Pawn> pawns = lord.ownedPawns;
            if (pawns != null && pawns.Any())
            {
                Queen queen = HiveUtility.FindQueen(pawns.RandomElement());
                foreach (Pawn pawn in pawns)
                {
                    if (queen != null)
                    {
                        if (HiveUtility.queenKindDef.Contains(pawn.kindDef))
                        {
                            pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("BI_QueenDuty"), queen, distToQueenToAttack);
                        }
                        else if (HiveUtility.megaspiderKindDef.Contains(pawn.kindDef))
                        {
                            pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("BI_MegaspiderDuty"), queen, distToQueenToAttack);
                        }
                        else if (HiveUtility.spelopedeKindDef.Contains(pawn.kindDef))
                        {
                            pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("BI_SpelopedeDuty"), queen, distToQueenToAttack);
                        }
                        else if (HiveUtility.megascarabKindDef.Contains(pawn.kindDef))
                        {
                            pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("BI_MegascarabDuty"), queen, distToQueenToAttack);
                        }
                    }
                }
            }
        }
    }
}
