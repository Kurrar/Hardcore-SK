using Verse.AI.Group;
using RimWorld;

namespace BetterInfestations
{
	public class LordJob_DefendAndExpandHive : LordJob
	{
		public override bool CanBlockHostileVisitors
		{
			get
			{
				return false;
			}
		}

		public override bool AddFleeToil
		{
			get
			{
				return false;
			}
		}

		public LordJob_DefendAndExpandHive()
		{
		}

		public override StateGraph CreateGraph()
		{
            StateGraph stateGraph = new StateGraph();
            LordToil_DefendAndExpandHive lordToil_DefendAndExpandHive = new LordToil_DefendAndExpandHive();
            lordToil_DefendAndExpandHive.distToQueenToAttack = 16f;
            stateGraph.StartingToil = lordToil_DefendAndExpandHive;

            LordToil_AssaultColony lordToil_AssaultColony = new LordToil_AssaultColony();
            stateGraph.AddToil(lordToil_AssaultColony);
            Transition transition = new Transition(lordToil_DefendAndExpandHive, lordToil_AssaultColony, false, true);
            transition.AddTrigger(new Trigger_Memo(Queen.MemoAttackedByEnemy));
            transition.AddPostAction(new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition, false);

            LordToil_QueenKilled lordToil_QueenKilled = new LordToil_QueenKilled();
            stateGraph.AddToil(lordToil_QueenKilled);
            Transition transition2 = new Transition(lordToil_DefendAndExpandHive, lordToil_QueenKilled);
            transition2.AddTrigger(new Trigger_Memo(Queen.MemoDestroyedNonRoofCollapse));
            transition2.AddTrigger(new Trigger_Memo(Queen.MemoDeSpawned));
            transition2.AddPostAction(new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition2);

            LordToil_HiveDefense lordToil_HiveDefense = new LordToil_HiveDefense();
            stateGraph.AddToil(lordToil_HiveDefense);
            Transition transition3 = new Transition(lordToil_DefendAndExpandHive, lordToil_HiveDefense);
            transition3.AddTrigger(new Trigger_PawnHarmed(0.1f, false, Map.ParentFaction));
            transition3.AddPostAction(new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition3);

            Transition transition4 = new Transition(lordToil_HiveDefense, lordToil_DefendAndExpandHive);
            transition4.AddTrigger(new Trigger_TicksPassedWithoutHarmOrMemos(1200, new string[]
            {
                Insect.MemoAttackedByEnemy
            }));
            transition4.AddPostAction(new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition4);

            Transition transition5 = new Transition(lordToil_AssaultColony, lordToil_DefendAndExpandHive);
            transition5.AddTrigger(new Trigger_TicksPassedWithoutHarmOrMemos(7500, new string[]
            {
                Queen.MemoAttackedByEnemy
            }));
            transition5.AddPostAction(new TransitionAction_EndAttackBuildingJobs());
            stateGraph.AddTransition(transition5);
            return stateGraph;
        }
	}
}
