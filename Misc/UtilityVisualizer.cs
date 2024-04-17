using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class UtilityVisualizer
{
	public class Bar
	{
		public int number;

		public UtilityVisualizer vis;

		public UtilityComparer.UtilityTracker utracker;

		public Color col;

		public FSprite[] sprites;

		public FLabel name;

		public FLabel name2;

		public Bar(int number, UtilityVisualizer vis, UtilityComparer.UtilityTracker utracker, Color col, string nameString)
		{
			this.number = number;
			this.vis = vis;
			this.utracker = utracker;
			this.col = col;
			Vector2 vector = vis.dispPos + new Vector2(0f, (float)(-number) * 20f);
			sprites = new FSprite[4];
			for (int i = 0; i < 4; i++)
			{
				sprites[i] = new FSprite("pixel");
				sprites[i].anchorX = 0f;
				sprites[i].scaleY = 10f;
				sprites[i].x = vector.x;
				sprites[i].y = vector.y;
				Futile.stage.AddChild(sprites[i]);
			}
			sprites[0].scaleX = 100f;
			sprites[0].color = new Color(0f, 0f, 0f);
			sprites[0].alpha = 0.5f;
			sprites[1].color = new Color(0f, 0f, 0f);
			sprites[2].color = col;
			sprites[2].alpha = 0.5f;
			sprites[3].color = col;
			sprites[3].scaleY = 2f;
			name = new FLabel(Custom.GetFont(), nameString);
			name.alignment = FLabelAlignment.Right;
			name.color = col;
			name.anchorX = 1f;
			name.x = vector.x - 10f + 0.01f;
			name.y = vector.y + 0.01f;
			name2 = new FLabel(Custom.GetFont(), nameString);
			name2.alignment = FLabelAlignment.Right;
			name2.color = new Color(0f, 0f, 0f);
			name2.anchorX = 1f;
			name2.x = vector.x - 10f + 1f + 0.01f;
			name2.y = vector.y - 1f + 0.01f;
			Futile.stage.AddChild(name2);
			Futile.stage.AddChild(name);
		}

		public void Update()
		{
			sprites[3].scaleX = 100f * utracker.module.Utility() * utracker.weight;
			sprites[2].scaleX = 100f * utracker.SmoothedUtility();
			sprites[1].scaleX = 100f * utracker.weight;
		}
	}

	public UtilityComparer uc;

	public List<Bar> bars;

	public FSprite highestBar;

	public FSprite creaturePointer;

	public Vector2 dispPos;

	public FLabel name;

	public FLabel name2;

	public UtilityVisualizer(UtilityComparer uc)
	{
		this.uc = uc;
		dispPos = new Vector2(100f, 768f - 150f * (0.5f + (float)uc.AI.creature.world.game.numberOfUtilityVisualizers));
		bars = new List<Bar>();
		Color[] array = new Color[8]
		{
			new Color(1f, 0f, 0f),
			new Color(0f, 1f, 0f),
			new Color(0f, 0f, 1f),
			new Color(1f, 1f, 0f),
			new Color(0f, 1f, 1f),
			new Color(1f, 0f, 1f),
			new Color(0.7f, 0.7f, 0.7f),
			new Color(0.3f, 0.3f, 0.3f)
		};
		for (int i = 0; i < uc.uTrackers.Count; i++)
		{
			bars.Add(new Bar(i, this, uc.uTrackers[i], array[i], uc.uTrackers[i].module.GetType().ToString()));
		}
		highestBar = new FSprite("pixel");
		highestBar.scaleY = (float)uc.uTrackers.Count * 20f + 10f;
		highestBar.anchorY = 1f;
		highestBar.y = dispPos.y + 10f;
		Futile.stage.AddChild(highestBar);
		creaturePointer = new FSprite("pixel");
		creaturePointer.anchorY = 0f;
		creaturePointer.x = dispPos.x + 50f;
		creaturePointer.y = dispPos.y + 10f;
		Futile.stage.AddChild(creaturePointer);
		name = new FLabel(Custom.GetFont(), "");
		name.x = dispPos.x - 10f + 0.01f;
		name.y = dispPos.y + 0.01f + 20f;
		name2 = new FLabel(Custom.GetFont(), "");
		name2.color = new Color(0f, 0f, 0f);
		name2.x = dispPos.x - 10f + 1f + 0.01f;
		name2.y = dispPos.y - 1f + 0.01f + 20f;
		Futile.stage.AddChild(name2);
		Futile.stage.AddChild(name);
		uc.AI.creature.world.game.numberOfUtilityVisualizers++;
	}

	public void Update()
	{
		float num = 0f;
		int num2 = -1;
		for (int i = 0; i < bars.Count; i++)
		{
			bars[i].Update();
			if (uc.uTrackers[i].SmoothedUtility() > num)
			{
				num = uc.uTrackers[i].SmoothedUtility();
				num2 = i;
			}
		}
		if (num2 > -1 && uc.uTrackers[num2] == uc.highestUtilityTracker)
		{
			highestBar.x = dispPos.x + 100f * num;
			highestBar.color = bars[num2].col;
			name.color = bars[num2].col;
			name.text = uc.uTrackers[num2].module.GetType().ToString();
			name2.text = uc.uTrackers[num2].module.GetType().ToString();
		}
		else
		{
			highestBar.x = dispPos.x;
			highestBar.color = new Color(1f, 1f, 1f);
			name.color = new Color(1f, 1f, 1f);
			name.text = "NONE";
			name2.text = "NONE";
		}
		if (uc.AI.creature.Room.realizedRoom == uc.AI.creature.world.game.cameras[0].room)
		{
			creaturePointer.scaleY = Vector2.Distance(dispPos + new Vector2(50f, 10f), uc.AI.creature.realizedCreature.mainBodyChunk.pos - uc.AI.creature.world.game.cameras[0].pos);
			creaturePointer.rotation = Custom.AimFromOneVectorToAnother(dispPos + new Vector2(50f, 10f), uc.AI.creature.realizedCreature.mainBodyChunk.pos - uc.AI.creature.world.game.cameras[0].pos);
			creaturePointer.isVisible = true;
		}
		else
		{
			creaturePointer.isVisible = false;
		}
	}
}
