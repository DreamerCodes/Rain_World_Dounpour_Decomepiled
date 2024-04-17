using UnityEngine;

public interface IOwnProjectedCircles
{
	Vector2 CircleCenter(int index, float timeStacker);

	Room HostingCircleFromRoom();

	bool CanHostCircle();
}
