using UnityEngine;

namespace HUD;

public interface IOwnAHUD
{
	int CurrentFood { get; }

	Player.InputPackage MapInput { get; }

	bool RevealMap { get; }

	Vector2 MapOwnerInRoomPosition { get; }

	bool MapDiscoveryActive { get; }

	int MapOwnerRoom { get; }

	HUD.OwnerType GetOwnerType();

	void PlayHUDSound(SoundID soundID);

	void FoodCountDownDone();
}
