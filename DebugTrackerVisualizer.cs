using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class DebugTrackerVisualizer
{
	private struct GhostAndSprite
	{
		public DebugSprite sprite;

		public DebugSprite sprite2;

		public Tracker.Ghost ghost;

		public int lastRoom;

		public GhostAndSprite(DebugSprite sprite, DebugSprite sprite2, Tracker.Ghost ghost, int lastRoom)
		{
			this.sprite = sprite;
			this.sprite2 = sprite2;
			this.ghost = ghost;
			this.lastRoom = lastRoom;
		}
	}

	private struct SimpleGhostAndSprite
	{
		public DebugSprite sprite;

		public DebugSprite sprite2;

		public Tracker.CreatureRepresentation crit;

		public int lastRoom;

		public SimpleGhostAndSprite(DebugSprite sprite, DebugSprite sprite2, Tracker.CreatureRepresentation crit, int lastRoom)
		{
			this.sprite = sprite;
			this.sprite2 = sprite2;
			this.crit = crit;
			this.lastRoom = lastRoom;
		}
	}

	private struct NoiseSourceAndSprite
	{
		public DebugSprite sprite;

		public DebugSprite sprite2;

		public NoiseTracker.TheorizedSource noise;

		public int lastRoom;

		public NoiseSourceAndSprite(DebugSprite sprite, DebugSprite sprite2, NoiseTracker.TheorizedSource noise, int lastRoom)
		{
			this.sprite = sprite;
			this.sprite2 = sprite2;
			this.noise = noise;
			this.lastRoom = lastRoom;
		}
	}

	private Tracker tracker;

	private List<Tracker.Ghost> visualizedGhosts;

	private List<GhostAndSprite> spritesAndGhosts;

	private Color[] randomColors;

	private List<SimpleGhostAndSprite> simpleGhostsAndSprites;

	private List<NoiseSourceAndSprite> noisesAndSprites;

	public DebugTrackerVisualizer(Tracker tracker)
	{
		this.tracker = tracker;
		visualizedGhosts = new List<Tracker.Ghost>();
		spritesAndGhosts = new List<GhostAndSprite>();
		simpleGhostsAndSprites = new List<SimpleGhostAndSprite>();
		noisesAndSprites = new List<NoiseSourceAndSprite>();
		randomColors = new Color[50];
		for (int i = 0; i < randomColors.Length; i++)
		{
			randomColors[i] = new Color(Random.value, Random.value, Random.value);
		}
	}

	public void Update()
	{
		for (int i = 0; i < tracker.CreaturesCount; i++)
		{
			Tracker.CreatureRepresentation rep = tracker.GetRep(i);
			if (rep is Tracker.ElaborateCreatureRepresentation)
			{
				foreach (Tracker.Ghost ghost in (rep as Tracker.ElaborateCreatureRepresentation).ghosts)
				{
					bool flag = ghost.parent.representedCreature.Room.realizedRoom != null;
					foreach (Tracker.Ghost visualizedGhost in visualizedGhosts)
					{
						if (visualizedGhost.Equals(ghost))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						FSprite fSprite = new FSprite("pixel");
						fSprite.scale = 10f;
						FSprite fSprite2 = new FSprite("pixel");
						fSprite2.scaleX = 2f;
						fSprite2.anchorY = 0f;
						Room realizedRoom = ghost.parent.parent.AI.creature.world.GetAbstractRoom(ghost.coord.room).realizedRoom;
						if (realizedRoom != null)
						{
							spritesAndGhosts.Add(new GhostAndSprite(new DebugSprite(new Vector2(-100f, -100f), fSprite, realizedRoom), new DebugSprite(new Vector2(-100f, -100f), fSprite2, ghost.parent.representedCreature.Room.realizedRoom), ghost, ghost.parent.representedCreature.Room.index));
							realizedRoom.AddObject(spritesAndGhosts[spritesAndGhosts.Count - 1].sprite);
							realizedRoom.AddObject(spritesAndGhosts[spritesAndGhosts.Count - 1].sprite2);
							visualizedGhosts.Add(ghost);
						}
					}
				}
				continue;
			}
			bool flag2 = rep.representedCreature.Room.realizedRoom != null;
			foreach (SimpleGhostAndSprite simpleGhostsAndSprite in simpleGhostsAndSprites)
			{
				if (simpleGhostsAndSprite.crit.Equals(rep))
				{
					flag2 = false;
					break;
				}
			}
			if (flag2)
			{
				FSprite fSprite3 = new FSprite("pixel");
				fSprite3.scale = 10f;
				FSprite fSprite4 = new FSprite("pixel");
				fSprite4.scaleX = 2f;
				fSprite4.anchorY = 0f;
				Room realizedRoom2 = rep.representedCreature.Room.realizedRoom;
				if (realizedRoom2 != null)
				{
					simpleGhostsAndSprites.Add(new SimpleGhostAndSprite(new DebugSprite(new Vector2(-100f, -100f), fSprite3, realizedRoom2), new DebugSprite(new Vector2(-100f, -100f), fSprite4, rep.representedCreature.Room.realizedRoom), rep, rep.representedCreature.Room.index));
					realizedRoom2.AddObject(simpleGhostsAndSprites[simpleGhostsAndSprites.Count - 1].sprite);
					realizedRoom2.AddObject(simpleGhostsAndSprites[simpleGhostsAndSprites.Count - 1].sprite2);
				}
			}
		}
		if (tracker.noiseTracker != null)
		{
			for (int j = 0; j < tracker.noiseTracker.sources.Count; j++)
			{
				bool flag3 = true;
				for (int k = 0; k < noisesAndSprites.Count; k++)
				{
					if (noisesAndSprites[k].noise == tracker.noiseTracker.sources[j])
					{
						flag3 = false;
						break;
					}
				}
				if (flag3)
				{
					FSprite fSprite5 = new FSprite("Circle20");
					FSprite fSprite6 = new FSprite("pixel");
					fSprite6.scaleX = 2f;
					fSprite5.rotation = 45f;
					fSprite6.anchorY = 0f;
					noisesAndSprites.Add(new NoiseSourceAndSprite(new DebugSprite(new Vector2(-100f, -100f), fSprite5, tracker.AI.creature.realizedCreature.room), new DebugSprite(new Vector2(-100f, -100f), fSprite6, tracker.AI.creature.realizedCreature.room), tracker.noiseTracker.sources[j], tracker.AI.creature.realizedCreature.room.abstractRoom.index));
					tracker.AI.creature.realizedCreature.room.AddObject(noisesAndSprites[noisesAndSprites.Count - 1].sprite);
					tracker.AI.creature.realizedCreature.room.AddObject(noisesAndSprites[noisesAndSprites.Count - 1].sprite2);
				}
			}
		}
		for (int num = spritesAndGhosts.Count - 1; num >= 0; num--)
		{
			GhostAndSprite ghostAndSprite = spritesAndGhosts[num];
			ghostAndSprite.sprite.pos = ghostAndSprite.ghost.pos;
			ghostAndSprite.sprite2.pos = ghostAndSprite.ghost.pos;
			ghostAndSprite.sprite.sprite.color = randomColors[ghostAndSprite.ghost.generation % randomColors.Length];
			ghostAndSprite.sprite2.sprite.color = randomColors[ghostAndSprite.ghost.generation % randomColors.Length];
			ghostAndSprite.sprite.sprite.rotation = ghostAndSprite.ghost.moveBuffer * 360f;
			Vector2 pos = tracker.AI.creature.realizedCreature.mainBodyChunk.pos;
			if (ghostAndSprite.ghost.parent.bestGhost != ghostAndSprite.ghost && ghostAndSprite.ghost.parent.bestGhost != null)
			{
				pos = ghostAndSprite.ghost.parent.bestGhost.pos;
			}
			ghostAndSprite.sprite2.sprite.rotation = Custom.AimFromOneVectorToAnother(ghostAndSprite.ghost.pos, pos);
			ghostAndSprite.sprite2.sprite.scaleY = Vector2.Distance(ghostAndSprite.ghost.pos, pos);
			ghostAndSprite.sprite2.sprite.isVisible = ghostAndSprite.ghost.coord.room == tracker.AI.creature.pos.room;
			ghostAndSprite.sprite2.sprite.alpha = (1f / (float)ghostAndSprite.ghost.generation + 2f) / 3f;
			if (ghostAndSprite.ghost.parent.BestGuessForPosition().Equals(ghostAndSprite.ghost.coord))
			{
				ghostAndSprite.sprite.sprite.scale = 12f + Random.value * 7f;
				ghostAndSprite.sprite.sprite.alpha = 0.5f + Random.value * 0.5f;
			}
			else
			{
				ghostAndSprite.sprite.sprite.scale = 10f;
				ghostAndSprite.sprite.sprite.alpha = 1f;
			}
			if (ghostAndSprite.lastRoom != ghostAndSprite.ghost.coord.room)
			{
				if (ghostAndSprite.ghost.parent.parent.AI.creature.world.GetAbstractRoom(ghostAndSprite.lastRoom).realizedRoom != null)
				{
					ghostAndSprite.ghost.parent.parent.AI.creature.world.GetAbstractRoom(ghostAndSprite.lastRoom).realizedRoom.RemoveObject(ghostAndSprite.sprite);
					ghostAndSprite.ghost.parent.parent.AI.creature.world.GetAbstractRoom(ghostAndSprite.lastRoom).realizedRoom.RemoveObject(ghostAndSprite.sprite2);
				}
				if (ghostAndSprite.ghost.parent.parent.AI.creature.world.GetAbstractRoom(ghostAndSprite.ghost.coord.room).realizedRoom != null)
				{
					ghostAndSprite.ghost.parent.parent.AI.creature.world.GetAbstractRoom(ghostAndSprite.ghost.coord.room).realizedRoom.AddObject(ghostAndSprite.sprite);
					ghostAndSprite.ghost.parent.parent.AI.creature.world.GetAbstractRoom(ghostAndSprite.ghost.coord.room).realizedRoom.AddObject(ghostAndSprite.sprite2);
				}
				ghostAndSprite.lastRoom = ghostAndSprite.ghost.coord.room;
			}
			bool flag4 = true;
			for (int l = 0; l < tracker.CreaturesCount; l++)
			{
				Tracker.CreatureRepresentation rep2 = tracker.GetRep(l);
				if (rep2 is Tracker.ElaborateCreatureRepresentation)
				{
					foreach (Tracker.Ghost ghost2 in (rep2 as Tracker.ElaborateCreatureRepresentation).ghosts)
					{
						if (ghost2 == ghostAndSprite.ghost)
						{
							flag4 = false;
							break;
						}
					}
				}
				if (!flag4)
				{
					break;
				}
			}
			if (flag4)
			{
				ghostAndSprite.sprite.Destroy();
				ghostAndSprite.sprite2.Destroy();
				visualizedGhosts.Remove(ghostAndSprite.ghost);
				spritesAndGhosts.RemoveAt(num);
			}
		}
		for (int num2 = simpleGhostsAndSprites.Count - 1; num2 >= 0; num2--)
		{
			SimpleGhostAndSprite simpleGhostAndSprite = simpleGhostsAndSprites[num2];
			Vector2 vector = tracker.AI.creature.Room.realizedRoom.MiddleOfTile(simpleGhostAndSprite.crit.BestGuessForPosition());
			simpleGhostAndSprite.sprite.pos = vector;
			simpleGhostAndSprite.sprite2.pos = vector;
			simpleGhostAndSprite.sprite.sprite.color = randomColors[0];
			simpleGhostAndSprite.sprite2.sprite.color = randomColors[0];
			Vector2 pos2 = tracker.AI.creature.realizedCreature.mainBodyChunk.pos;
			simpleGhostAndSprite.sprite2.sprite.rotation = Custom.AimFromOneVectorToAnother(vector, pos2);
			simpleGhostAndSprite.sprite2.sprite.scaleY = Vector2.Distance(vector, pos2);
			simpleGhostAndSprite.sprite2.sprite.isVisible = simpleGhostAndSprite.crit.BestGuessForPosition().room == tracker.AI.creature.pos.room;
			if (simpleGhostAndSprite.crit.deleteMeNextFrame)
			{
				simpleGhostAndSprite.sprite.Destroy();
				simpleGhostAndSprite.sprite2.Destroy();
				simpleGhostsAndSprites.RemoveAt(num2);
			}
		}
		for (int num3 = noisesAndSprites.Count - 1; num3 >= 0; num3--)
		{
			NoiseSourceAndSprite noiseSourceAndSprite = noisesAndSprites[num3];
			noiseSourceAndSprite.sprite.pos = noiseSourceAndSprite.noise.pos;
			noiseSourceAndSprite.sprite2.pos = noiseSourceAndSprite.noise.pos;
			noiseSourceAndSprite.sprite.sprite.color = ((noiseSourceAndSprite.noise == noiseSourceAndSprite.noise.noiseTracker.soundToExamine) ? new Color(1f, 0f, 0f) : new Color(1f, 1f, 0f));
			noiseSourceAndSprite.sprite2.sprite.color = new Color(1f, 0f, 0f);
			noiseSourceAndSprite.sprite.sprite.scale = noiseSourceAndSprite.noise.LASTSOUNDSTRENGTH / 10f;
			noiseSourceAndSprite.sprite.sprite.alpha = 0.3f;
			if (noiseSourceAndSprite.noise.creatureRep != null)
			{
				Vector2 vector2 = noiseSourceAndSprite.sprite2.room.MiddleOfTile(noiseSourceAndSprite.noise.creatureRep.BestGuessForPosition());
				noiseSourceAndSprite.sprite2.sprite.rotation = Custom.AimFromOneVectorToAnother(noiseSourceAndSprite.noise.pos, vector2);
				noiseSourceAndSprite.sprite2.sprite.scaleY = Vector2.Distance(noiseSourceAndSprite.noise.pos, vector2);
				noiseSourceAndSprite.sprite2.sprite.isVisible = true;
			}
			else
			{
				noiseSourceAndSprite.sprite2.sprite.isVisible = false;
			}
			if (noiseSourceAndSprite.noise.slatedForDeletion)
			{
				noiseSourceAndSprite.sprite.Destroy();
				noiseSourceAndSprite.sprite2.Destroy();
				noisesAndSprites.RemoveAt(num3);
			}
		}
	}

	public void ClearSprites()
	{
		foreach (GhostAndSprite spritesAndGhost in spritesAndGhosts)
		{
			spritesAndGhost.sprite.Destroy();
		}
	}
}
