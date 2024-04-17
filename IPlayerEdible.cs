public interface IPlayerEdible
{
	int BitesLeft { get; }

	int FoodPoints { get; }

	bool Edible { get; }

	bool AutomaticPickUp { get; }

	void BitByPlayer(Creature.Grasp grasp, bool eu);

	void ThrowByPlayer();
}
