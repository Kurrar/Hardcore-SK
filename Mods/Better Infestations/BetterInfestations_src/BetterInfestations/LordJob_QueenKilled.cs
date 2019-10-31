using Verse.AI.Group;

namespace BetterInfestations
{
	public class LordJob_QueenKilled : LordJob
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

		public LordJob_QueenKilled()
		{
		}

		public override StateGraph CreateGraph()
		{
            StateGraph stateGraph = new StateGraph();
            LordToil_QueenKilled lordToil_QueenKilled = new LordToil_QueenKilled();
            stateGraph.StartingToil = lordToil_QueenKilled;
            return stateGraph;
        }
	}
}
