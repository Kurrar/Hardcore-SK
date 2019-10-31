using Verse;
using Verse.AI;
using Verse.Sound;
using System.Collections.Generic;
using System.Diagnostics;

namespace BetterInfestations
{
	public class JobDriver_LayEgg : JobDriver
	{
        private const int LayEggTickTime = 500;
        private const TargetIndex LaySpotInd = TargetIndex.A;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(LaySpotInd, PathEndMode.OnCell);
            yield return Toils_General.Wait(LayEggTickTime, TargetIndex.None);
            yield return FinalizeToil();
        }

        public Toil FinalizeToil()
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                SpawnEgg();
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }
        public void SpawnEgg()
        {
            Queen queen = pawn as Queen;
            if (queen != null)
            {
                Egg egg = (Egg)ThingMaker.MakeThing(HiveUtility.ThingDefOfEgg, null);
                egg.SetFaction(pawn.Faction, null);
                egg.eggLayer = queen;
                GenSpawn.Spawn(egg, pawn.Position, Map, WipeMode.Vanish);
                SoundDef.Named("BI_Laying_Egg").PlayOneShot(egg);
                queen.eggReady = false;
                queen.spawnedEggs.Add(egg);
            }
        }
    }
}
