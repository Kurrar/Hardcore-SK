using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
    public class JobGiver_InsectButcher : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (HiveUtility.JobGivenRecentTick(pawn, "BI_InsectButcher"))
            {
                return null;
            }
            if (!ButcherCheck(pawn))
            {
                return null;
            }
            Thing thing = JobGiver_InsectGather.FindTarget(pawn, true);
            if (thing == null || !JobGiver_InsectGather.WithinHive(pawn, pawn, false))
            {
                return null;
            }
            Job job = new Job(DefDatabase<JobDef>.GetNamed("BI_InsectButcher"), thing)
            {
                ignoreForbidden = true,
                count = 1
            };
            return job;
        }
        public static bool ButcherCheck(Pawn pawn)
        {
            bool canButcher = true;
            Queen queen = HiveUtility.FindQueen(pawn);
            if (queen != null)
            {
                List<Pawn> spawnedInsects = queen.spawnedInsects;
                List<ThingDef> defList = new List<ThingDef>();
                List<string> list = spawnedInsects?.Select(x => x.def.defName).ToList() ?? new List<string>();
                defList = list.Select(s => DefDatabase<ThingDef>.GetNamedSilentFail(s)).OfType<ThingDef>().ToList();

                if (HiveUtility.megaspiderKindDef.Contains(pawn.kindDef))
                {
                    if (defList.Contains(ThingDef.Named("Spelopede")) || defList.Contains(ThingDef.Named("Megascarab")))
                    {
                        canButcher = false;
                    }
                }
                else if (HiveUtility.spelopedeKindDef.Contains(pawn.kindDef))
                {
                    if (!defList.Contains(ThingDef.Named("Megaspider")) && defList.Contains(ThingDef.Named("Megascarab")))
                    {
                        canButcher = false;
                    }
                }
            }
            return canButcher;
        }
    }
}