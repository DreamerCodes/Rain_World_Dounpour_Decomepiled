using UnityEngine;

namespace MoreSlugcats;

public class FiltrationPowerController
{
	private int counter;

	private float powerOutput;

	private World world;

	private float outtageIntensity;

	private bool deadFilter;

	private bool fullTurnOff;

	private int triggerThreshold;

	private SlugcatStats.Name saveStateNumber;

	private float powerGoal;

	public float ElectricPower => powerOutput;

	public bool PowerOn => powerGoal > 0.6f;

	public bool PowerOff => powerGoal < 0.2f;

	public FiltrationPowerController(World world)
	{
		this.world = world;
		powerOutput = 0f;
		counter = 0;
		if (world.game.IsStorySession)
		{
			saveStateNumber = world.game.GetStorySession.saveStateNumber;
		}
		powerGoal = Random.Range(0f, 1f);
		fullTurnOff = true;
		outtageIntensity = Random.Range(0.8f, 1f);
		if (saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear || saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer || saveStateNumber == SlugcatStats.Name.Red)
		{
			fullTurnOff = false;
			outtageIntensity = Random.Range(0.1f, 0.3f);
			if (saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				outtageIntensity = Random.Range(0f, 0.1f);
			}
			if (saveStateNumber == SlugcatStats.Name.Red)
			{
				outtageIntensity = Random.Range(0.51f, 0.6f);
			}
		}
		if (saveStateNumber == SlugcatStats.Name.White || saveStateNumber == SlugcatStats.Name.Yellow || saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet || saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			fullTurnOff = true;
			deadFilter = true;
			outtageIntensity = 1f;
		}
	}

	public void Update()
	{
		if (deadFilter)
		{
			counter = 0;
			powerGoal = 0f;
			powerOutput = 0f;
			return;
		}
		counter++;
		if (counter >= triggerThreshold)
		{
			counter = 0;
			if (saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				triggerThreshold = Random.Range(600, 900);
			}
			else if (saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				triggerThreshold = Random.Range(500, 1100);
			}
			else if (saveStateNumber == SlugcatStats.Name.Red)
			{
				triggerThreshold = Random.Range(700, 1600);
			}
			else
			{
				triggerThreshold = Random.Range(900, 1800);
			}
			bool flag;
			if (PowerOn)
			{
				flag = false;
				if (outtageIntensity < 0.5f || Random.value >= outtageIntensity * 0.6f)
				{
					if (fullTurnOff)
					{
						powerGoal = 0f;
					}
					else
					{
						powerGoal = Random.Range(0f, 0.1f);
					}
				}
			}
			else if (PowerOff)
			{
				powerGoal = Random.Range(0.9f, 1f) * Mathf.Lerp(1f, 0.3f, outtageIntensity);
				flag = true;
			}
			else
			{
				powerGoal = Random.Range(0f, 1f);
				flag = powerGoal >= 0.5f;
			}
			if (!flag)
			{
				triggerThreshold *= (int)Mathf.Clamp(8f * outtageIntensity, 0.1f, 99999f);
			}
		}
		else
		{
			powerGoal += Random.Range(-0.01f, 0.01f) * (1f - outtageIntensity);
			powerGoal = Mathf.Clamp(powerGoal, 0f, 1f);
			powerGoal *= Mathf.Lerp(0.999f, 0.985f, outtageIntensity);
		}
		powerOutput = Mathf.Lerp(powerOutput, powerGoal, Mathf.Lerp(0.12f, 0.05f, outtageIntensity));
	}
}
