using UnityEngine;

public interface IClimbableVine
{
	int TotalPositions();

	Vector2 Pos(int index);

	float Rad(int index);

	float Mass(int index);

	void Push(int index, Vector2 movement);

	void BeingClimbedOn(Creature crit);

	bool CurrentlyClimbable();
}
