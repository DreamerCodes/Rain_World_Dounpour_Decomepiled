using System.Collections.Generic;
using System.Linq;
using Menu;
using UnityEngine;

namespace Modding.Passages;

public abstract class CustomPassage
{
	public virtual WinState.EndgameID ID => null;

	public virtual string DisplayName => "Missing Name";

	public virtual string SpriteName => null;

	public virtual MenuScene.SceneID Scene => MenuScene.SceneID.MainMenu;

	public virtual bool IsNotched => false;

	public virtual Color[] NotchColors => null;

	public virtual WinState.EndgameID[] RequiredPassages => null;

	public virtual int ExpeditionScore => 40;

	public virtual bool RequiresCombat => false;

	public virtual bool IsAvailableForSlugcat(SlugcatStats.Name name)
	{
		return true;
	}

	public virtual WinState.EndgameTracker CreateTracker()
	{
		return null;
	}

	public virtual bool RequirementsMet(List<WinState.EndgameTracker> trackers)
	{
		if (RequiredPassages != null)
		{
			return RequiredPassages.All((WinState.EndgameID requirement) => trackers.Any((WinState.EndgameTracker tracker) => tracker.ID == requirement && tracker.GoalAlreadyFullfilled));
		}
		return true;
	}

	public virtual void OnWin(WinState winState, RainWorldGame game, WinState.EndgameTracker tracker)
	{
	}

	public virtual void OnDeath(WinState winState, WinState.EndgameTracker tracker)
	{
	}

	public virtual void ApplyHooks()
	{
	}
}
