using UnityEngine;

namespace MoreSlugcats;

public interface IProvideWarmth
{
	Room loadedRoom { get; }

	float warmth { get; }

	float range { get; }

	Vector2 Position();
}
