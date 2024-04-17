using System.Collections.Generic;
using UnityEngine;

public class UtilityComparer : AIModule
{
	public class UtilityTracker
	{
		public float weight;

		public float continuationBonus;

		public AIModule module;

		public FloatTweener.FloatTween smoother;

		public float smoothedUtility;

		public float exponent = 1f;

		public UtilityTracker(AIModule module, FloatTweener.FloatTween smoother, float weight, float continuationBonus)
		{
			this.weight = weight;
			this.smoother = smoother;
			this.continuationBonus = continuationBonus;
			this.module = module;
			smoothedUtility = 0f;
		}

		public void UpdateSmoothing()
		{
			if (smoother != null)
			{
				smoothedUtility = smoother.Tween(smoothedUtility, UnSmoothedUtility());
			}
		}

		private float UnSmoothedUtility()
		{
			if (module == null)
			{
				return 0f;
			}
			return Mathf.Pow(module.Utility(), exponent) * weight;
		}

		public float SmoothedUtility()
		{
			if (smoother != null)
			{
				return smoothedUtility;
			}
			return UnSmoothedUtility();
		}
	}

	public bool visualize;

	public UtilityVisualizer vis;

	public List<UtilityTracker> uTrackers;

	public UtilityTracker highestUtilityTracker;

	public UtilityComparer(ArtificialIntelligence AI)
		: base(AI)
	{
		uTrackers = new List<UtilityTracker>();
		visualize = false;
	}

	public void AddComparedModule(AIModule module, FloatTweener.FloatTween smoother, float weight, float continuationBonus)
	{
		uTrackers.Add(new UtilityTracker(module, smoother, weight, continuationBonus));
	}

	public override void Update()
	{
		if (visualize)
		{
			if (vis == null)
			{
				vis = new UtilityVisualizer(this);
			}
			else
			{
				vis.Update();
			}
		}
		float num = float.MinValue;
		highestUtilityTracker = null;
		for (int i = 0; i < uTrackers.Count; i++)
		{
			uTrackers[i].UpdateSmoothing();
			if (uTrackers[i].SmoothedUtility() > num)
			{
				num = uTrackers[i].SmoothedUtility();
				highestUtilityTracker = uTrackers[i];
			}
		}
	}

	public float GetSmoothedNonWeightedUtility(AIModule module)
	{
		for (int i = 0; i < uTrackers.Count; i++)
		{
			if (uTrackers[i].module == module)
			{
				return uTrackers[i].SmoothedUtility() / uTrackers[i].weight;
			}
		}
		return 0f;
	}

	public UtilityTracker GetUtilityTracker(AIModule module)
	{
		for (int i = 0; i < uTrackers.Count; i++)
		{
			if (uTrackers[i].module == module)
			{
				return uTrackers[i];
			}
		}
		return null;
	}

	public AIModule HighestUtilityModule()
	{
		if (highestUtilityTracker == null)
		{
			return null;
		}
		return highestUtilityTracker.module;
	}

	public float HighestUtility()
	{
		if (highestUtilityTracker == null)
		{
			return 0f;
		}
		return highestUtilityTracker.SmoothedUtility();
	}
}
