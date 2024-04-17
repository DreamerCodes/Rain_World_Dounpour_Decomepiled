using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Menu;

public abstract class PlayerResultMenu : Menu
{
	public List<PlayerResultBox> resultBoxes;

	public int allResultBoxesInPlaceCounter = -1;

	public ArenaSitting ArenaSitting;

	public List<ArenaSitting.ArenaPlayer> result;

	public int counter;

	public Vector2 topMiddle;

	private bool alreadyPlayedSoundThisFrame;

	public bool NumbersTick => counter % 2 == 0;

	public bool KillsTick => counter % 4 == 0;

	public float AllBoxesInPlaceFac(float timeStacker)
	{
		return Mathf.InverseLerp(5f, 40f, (float)allResultBoxesInPlaceCounter + timeStacker);
	}

	public void PlaySingleSound(SoundID sound)
	{
		if (!alreadyPlayedSoundThisFrame)
		{
			alreadyPlayedSoundThisFrame = true;
			PlaySound(sound);
		}
	}

	public PlayerResultMenu(ProcessManager manager, ArenaSitting ArenaSitting, List<ArenaSitting.ArenaPlayer> result, ProcessManager.ProcessID processID)
		: base(manager, processID)
	{
		resultBoxes = new List<PlayerResultBox>();
		this.ArenaSitting = ArenaSitting;
		this.result = result;
		topMiddle = new Vector2(683f, 550.01f);
		topMiddle.y += Custom.LerpMap(result.Count, 1f, 4f, -100f, 0f);
	}

	public override void Update()
	{
		base.Update();
		counter++;
		alreadyPlayedSoundThisFrame = false;
		if (allResultBoxesInPlaceCounter > -1)
		{
			allResultBoxesInPlaceCounter++;
		}
	}
}
