using System.Collections.Generic;
using Noise;
using RWCustom;
using UnityEngine;

public class NoiseTracker : AIModule
{
	public class TheorizedSource
	{
		public NoiseTracker noiseTracker;

		public Vector2 pos;

		public int age;

		public bool slatedForDeletion;

		public Tracker.CreatureRepresentation creatureRep;

		public float LASTSOUNDSTRENGTH;

		public TheorizedSource(NoiseTracker noiseTracker, Vector2 pos, Tracker.CreatureRepresentation creatureRep)
		{
			this.noiseTracker = noiseTracker;
			this.pos = pos;
			this.creatureRep = creatureRep;
			age = 0;
		}

		public void Update()
		{
			age++;
			if (creatureRep != null && creatureRep.deleteMeNextFrame)
			{
				creatureRep = null;
			}
			if (age > noiseTracker.forgetTime)
			{
				Destroy();
			}
			LASTSOUNDSTRENGTH = Mathf.Max(10f, LASTSOUNDSTRENGTH * 0.6f);
		}

		public void Destroy()
		{
			slatedForDeletion = true;
		}

		public float NoiseMatch(InGameNoise noise)
		{
			if (creatureRep == null)
			{
				return Vector2.Distance(pos, noise.pos) + 200f + (float)age / 5f;
			}
			return noiseTracker.NoiseMatch(noise, creatureRep) + (float)age / 5f;
		}

		public void Refresh(InGameNoise noise)
		{
			pos = noise.pos;
			age = 0;
			LASTSOUNDSTRENGTH = noise.strength;
			if (creatureRep == null)
			{
				return;
			}
			creatureRep.HeardThisCreature();
			if (creatureRep is Tracker.SimpleCreatureRepresentation simpleCreatureRepresentation && noiseTracker.room.aimap.TileOrNeighborsAccessibleToCreature(noiseTracker.room.GetTilePosition(noise.pos), creatureRep.representedCreature.creatureTemplate))
			{
				simpleCreatureRepresentation.lastSeenCoord = noiseTracker.room.GetWorldCoordinate(noise.pos);
			}
			else
			{
				if (!(creatureRep is Tracker.ElaborateCreatureRepresentation elaborateCreatureRepresentation))
				{
					return;
				}
				float num = float.MaxValue;
				Tracker.Ghost ghost = null;
				for (int i = 0; i < elaborateCreatureRepresentation.ghosts.Count; i++)
				{
					if (elaborateCreatureRepresentation.ghosts[i].coord.room == noiseTracker.room.abstractRoom.index && Custom.DistLess(noise.pos, noiseTracker.room.MiddleOfTile(elaborateCreatureRepresentation.ghosts[i].coord), num))
					{
						num = Vector2.Distance(noise.pos, noiseTracker.room.MiddleOfTile(elaborateCreatureRepresentation.ghosts[i].coord));
						ghost = elaborateCreatureRepresentation.ghosts[i];
					}
				}
				if (ghost == null)
				{
					return;
				}
				if (noiseTracker.room.aimap.TileOrNeighborsAccessibleToCreature(noiseTracker.room.GetTilePosition(noise.pos), creatureRep.representedCreature.creatureTemplate))
				{
					if (num < 100f)
					{
						ghost.Move(noiseTracker.room.GetWorldCoordinate(noise.pos), noise.pos);
						if (ghost.vel.magnitude == 0f)
						{
							ghost.vel = Custom.RNV();
						}
						ghost.lastCoord = new WorldCoordinate(-1, -1, -1, -1);
					}
					else
					{
						ghost = ghost.Clone(noiseTracker.room.GetWorldCoordinate(noise.pos), 0);
					}
				}
				for (int j = 0; j < elaborateCreatureRepresentation.ghosts.Count; j++)
				{
					elaborateCreatureRepresentation.ghosts[j].generation++;
				}
				ghost.generation = 0;
				elaborateCreatureRepresentation.bestGhostDirty = true;
			}
		}
	}

	public Tracker tracker;

	public Room room;

	public List<TheorizedSource> sources;

	public bool ignoreSeenNoises = true;

	public int forgetTime;

	public float hearingSkill;

	public float mysteriousNoises;

	public int mysteriousNoiseCounter;

	public TheorizedSource soundToExamine;

	public WorldCoordinate ExaminePos => room.GetWorldCoordinate(soundToExamine.pos);

	public NoiseTracker(ArtificialIntelligence AI, Tracker tracker)
		: base(AI)
	{
		this.tracker = tracker;
		sources = new List<TheorizedSource>();
		tracker.noiseTracker = this;
		forgetTime = 1200;
	}

	public override float Utility()
	{
		if (soundToExamine == null)
		{
			return 0f;
		}
		return Mathf.Pow(Mathf.InverseLerp(1f, 20f, mysteriousNoises), 0.5f);
	}

	public override void NewRoom(Room room)
	{
		if (this.room != room)
		{
			this.room = room;
			sources.Clear();
			UpdateExamineSound();
		}
	}

	public override void Update()
	{
		for (int num = sources.Count - 1; num >= 0; num--)
		{
			if (sources[num].slatedForDeletion)
			{
				sources.RemoveAt(num);
				UpdateExamineSound();
			}
			else
			{
				sources[num].Update();
			}
		}
		if (soundToExamine != null && (Custom.ManhattanDistance(AI.creature.pos, ExaminePos) < 3 || (Custom.ManhattanDistance(AI.creature.pos, ExaminePos) < 10 && AI.VisualContact(soundToExamine.pos, 0f))))
		{
			soundToExamine.Destroy();
			UpdateExamineSound();
		}
		if (mysteriousNoiseCounter > 0)
		{
			mysteriousNoiseCounter--;
		}
		else if (mysteriousNoises > 0f)
		{
			Mathf.Max(0f, mysteriousNoises -= 0.0125f);
		}
	}

	public void ClearAllUnkown()
	{
		for (int i = 0; i < sources.Count; i++)
		{
			if (sources[i].creatureRep == null)
			{
				sources[i].Destroy();
			}
		}
	}

	public void HeardNoise(InGameNoise noise)
	{
		if (!Custom.DistLess(AI.creature.realizedCreature.mainBodyChunk.pos, noise.pos, noise.strength * (1f - AI.creature.realizedCreature.Deaf) * hearingSkill * (1f - room.BackgroundNoise) * ((AI.creature.realizedCreature.room.PointSubmerged(noise.pos) || AI.creature.realizedCreature.room.GetTile(AI.creature.realizedCreature.mainBodyChunk.pos).DeepWater) ? 0.2f : 1f)))
		{
			return;
		}
		if (ignoreSeenNoises)
		{
			if (AI.VisualContact(noise.pos, 0f))
			{
				return;
			}
			Tracker.CreatureRepresentation creatureRepresentation = tracker.RepresentationForObject(noise.sourceObject, AddIfMissing: false);
			if (creatureRepresentation == null)
			{
				for (int i = 0; i < noise.sourceObject.grabbedBy.Count; i++)
				{
					if (creatureRepresentation != null)
					{
						break;
					}
					creatureRepresentation = tracker.RepresentationForObject(noise.sourceObject.grabbedBy[i].grabber, AddIfMissing: false);
				}
			}
			if (creatureRepresentation != null && creatureRepresentation.VisualContact)
			{
				return;
			}
		}
		float num = float.MaxValue;
		TheorizedSource theorizedSource = null;
		for (int j = 0; j < sources.Count; j++)
		{
			float num2 = sources[j].NoiseMatch(noise);
			if (num2 < num && num2 < ((sources[j].creatureRep != null) ? Custom.LerpMap(sources[j].creatureRep.TicksSinceSeen, 20f, 600f, 200f, 1000f) : 300f))
			{
				num = num2;
				theorizedSource = sources[j];
			}
		}
		if (theorizedSource != null)
		{
			if (theorizedSource.creatureRep == null && theorizedSource.age > 10)
			{
				mysteriousNoises += HowInterestingIsThisNoiseToMe(noise);
				mysteriousNoiseCounter = 200;
			}
			theorizedSource.Refresh(noise);
		}
		else
		{
			Tracker.CreatureRepresentation creatureRepresentation2 = null;
			num = float.MaxValue;
			int num3 = 0;
			for (int k = 0; k < tracker.CreaturesCount; k++)
			{
				float num4 = NoiseMatch(noise, tracker.GetRep(k));
				if (num4 < num && num4 < Custom.LerpMap(tracker.GetRep(k).TicksSinceSeen, 20f, 600f, 200f, 1000f))
				{
					num = num4;
					creatureRepresentation2 = tracker.GetRep(k);
				}
				if (!tracker.GetRep(k).VisualContact)
				{
					num3++;
				}
			}
			if (num > Custom.LerpMap(num3, 0f, tracker.maxTrackedCreatures, 1000f, 300f))
			{
				creatureRepresentation2 = null;
			}
			if (creatureRepresentation2 == null)
			{
				mysteriousNoises += HowInterestingIsThisNoiseToMe(noise);
				mysteriousNoiseCounter = 200;
			}
			theorizedSource = new TheorizedSource(this, noise.pos, creatureRepresentation2);
			sources.Add(theorizedSource);
			theorizedSource.Refresh(noise);
		}
		UpdateExamineSound();
		if (AI is IAINoiseReaction)
		{
			(AI as IAINoiseReaction).ReactToNoise(theorizedSource, noise);
		}
	}

	public float HowInterestingIsThisNoiseToMe(InGameNoise noise)
	{
		if (noise.sourceObject is Creature && (noise.sourceObject as Creature).Template.TopAncestor().type == AI.creature.creatureTemplate.TopAncestor().type)
		{
			return noise.interesting * 0.1f;
		}
		if (noise.sourceObject is Rock)
		{
			return noise.interesting * 5f;
		}
		return noise.interesting;
	}

	private float NoiseMatch(InGameNoise noise, Tracker.CreatureRepresentation critRep)
	{
		if (critRep.VisualContact && ignoreSeenNoises)
		{
			return float.MaxValue;
		}
		float num = Vector2.Distance(noise.pos, room.MiddleOfTile(critRep.BestGuessForPosition()));
		if (critRep is Tracker.ElaborateCreatureRepresentation)
		{
			for (int i = 0; i < (critRep as Tracker.ElaborateCreatureRepresentation).ghosts.Count; i++)
			{
				if ((critRep as Tracker.ElaborateCreatureRepresentation).ghosts[i].coord.room == room.abstractRoom.index && Custom.DistLess(noise.pos, room.MiddleOfTile((critRep as Tracker.ElaborateCreatureRepresentation).ghosts[i].coord), num))
				{
					num = Vector2.Distance(noise.pos, room.MiddleOfTile((critRep as Tracker.ElaborateCreatureRepresentation).ghosts[i].coord));
				}
			}
		}
		return num;
	}

	private void UpdateExamineSound()
	{
		soundToExamine = null;
		float num = float.MaxValue;
		for (int i = 0; i < sources.Count; i++)
		{
			if (!sources[i].slatedForDeletion)
			{
				float num2 = Vector2.Distance(AI.creature.realizedCreature.mainBodyChunk.pos, sources[i].pos);
				if (AI.pathFinder != null && !AI.pathFinder.CoordinateReachableAndGetbackable(AI.creature.realizedCreature.room.GetWorldCoordinate(sources[i].pos)))
				{
					num2 += 500f;
				}
				num2 += Custom.LerpMap(sources[i].age, forgetTime / 2, forgetTime, 0f, 400f);
				if (sources[i].creatureRep == null && num2 < num)
				{
					num = num2;
					soundToExamine = sources[i];
				}
			}
		}
	}
}
