using UnityEngine;

public interface HasDanglers
{
	Vector2 DanglerConnection(int index, float timeStacker);

	Dangler.DanglerProps Props(int index);
}
