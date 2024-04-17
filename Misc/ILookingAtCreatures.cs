public interface ILookingAtCreatures
{
	float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score);

	Tracker.CreatureRepresentation ForcedLookCreature();

	void LookAtNothing();
}
