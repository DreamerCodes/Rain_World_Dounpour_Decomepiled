public interface IUseItemTracker
{
	bool TrackItem(AbstractPhysicalObject obj);

	void SeeThrownWeapon(PhysicalObject obj, Creature thrower);
}
