using Expedition;
using UnityEngine;

namespace Modding.Expedition;

public abstract class CustomBurden
{
	public virtual string ID => null;

	public virtual string DisplayName => "Missing Name";

	public virtual string Group => "Other Burdens";

	public virtual string Description => "Missing Description";

	public virtual string ManualDescription => "Missing Manual Description";

	public virtual Color Color => Color.white;

	public virtual bool UnlockedByDefault => false;

	public virtual float ScoreMultiplier => 0f;

	public virtual ExpeditionGame.BurdenTracker CreateTracker(RainWorldGame game)
	{
		return null;
	}

	public virtual void ApplyHooks()
	{
	}
}
