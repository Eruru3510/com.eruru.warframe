using Eruru.Json;

namespace com.eruru.warframe {

	public class WarframeStatusInvasion {

		public string ID;
		[JsonField (typeof (NodeTranslator))]
		public string Node;
		public string AttackingFaction;
		public string DefendingFaction;
		[JsonField (typeof (InvasionCompletionRound))]
		public float Completion;
		public bool Completed;
		[JsonField (typeof (RewardTranslator))]
		public string AttackerReward;
		[JsonField (typeof (RewardTranslator))]
		public string DefenderReward;

	}

}