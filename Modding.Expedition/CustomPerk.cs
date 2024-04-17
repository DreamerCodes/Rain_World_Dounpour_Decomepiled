using UnityEngine;

namespace Modding.Expedition;

public abstract class CustomPerk
{
	public virtual string ID => null;

	public virtual string Group => "Other Perks";

	public virtual string DisplayName => "Missing Name";

	public virtual string Description => "Missing Description";

	public virtual string ManualDescription => "Missing Manual Description";

	public virtual string SpriteName => "pixel";

	public virtual Color Color => Color.white;

	public virtual bool UnlockedByDefault => false;

	public virtual bool AvailableForSlugcat(SlugcatStats.Name name)
	{
		return true;
	}

	public virtual void ApplyHooks()
	{
	}
}
