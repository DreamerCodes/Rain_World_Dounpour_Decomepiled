using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class SmallNeedleWorm : NeedleWorm, IPlayerEdible
{
	public int momTailSegment;

	public bool hasScreamed;

	public float screamCounter;

	public int bites = 5;

	public SmallNeedleWormAI SmallAI => AI as SmallNeedleWormAI;

	public BigNeedleWorm Mother
	{
		get
		{
			if (AbstractMother != null)
			{
				return AbstractMother.realizedCreature as BigNeedleWorm;
			}
			return null;
		}
	}

	public AbstractCreature AbstractMother
	{
		get
		{
			if ((base.abstractCreature.abstractAI as NeedleWormAbstractAI).mother != null && (base.abstractCreature.abstractAI as NeedleWormAbstractAI).mother.creatureTemplate.type == CreatureTemplate.Type.BigNeedleWorm)
			{
				return (base.abstractCreature.abstractAI as NeedleWormAbstractAI).mother;
			}
			return null;
		}
	}

	public bool MotherAttachable
	{
		get
		{
			if (Mother == null)
			{
				return false;
			}
			if (Mother.room == room)
			{
				return Mother.chargingAttack < 0.1f;
			}
			return false;
		}
	}

	public BigNeedleWorm HangingOnMother
	{
		get
		{
			if (Mother == null || base.grasps[0] == null)
			{
				return null;
			}
			return Mother;
		}
	}

	public int BitesLeft => bites;

	public int FoodPoints => 2;

	public bool Edible => true;

	public bool AutomaticPickUp => false;

	public override void RecreateSticksFromAbstract()
	{
		for (int num = base.abstractCreature.stuckObjects.Count - 1; num >= 0; num--)
		{
			if (base.abstractCreature.stuckObjects[num] is AbstractPhysicalObject.CreatureGripStick && base.abstractCreature.stuckObjects[num].A == base.abstractCreature)
			{
				if (base.abstractCreature.stuckObjects[num].B == AbstractMother)
				{
					if (!Grab(base.abstractCreature.stuckObjects[num].B.realizedObject, 0, Mother.bodyChunks.Length - 1, Grasp.Shareability.NonExclusive, 0f, overrideEquallyDominant: false, pacifying: false))
					{
						base.abstractCreature.stuckObjects[num].Deactivate();
					}
				}
				else
				{
					base.abstractCreature.stuckObjects[num].Deactivate();
				}
			}
		}
	}

	public SmallNeedleWorm(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		if (abstractCreature.state.dead)
		{
			hasScreamed = true;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null || enteringShortCut.HasValue)
		{
			return;
		}
		for (int i = 0; i < base.TotalSegments; i++)
		{
			AddSegmentVel(i, Custom.RNV() * 4f * Mathf.Pow(screaming, 0.2f));
		}
		flyingThisFrame = false;
		if (base.grasps[0] != null)
		{
			HangOnMom();
		}
		else if (base.Consious)
		{
			if (ModManager.MSC && base.LickedByPlayer != null)
			{
				Scream();
			}
			if (base.safariControlled && inputWithDiagonals.HasValue && inputWithDiagonals.Value.jmp && !lastInputWithDiagonals.Value.jmp)
			{
				SmallScream(motherRespond: false);
			}
			Act();
		}
		if (!base.dead && grabbedBy.Count > 0 && screamCounter == 0f && (!(grabbedBy[0].grabber is Player) || !ModManager.MSC || (grabbedBy[0].grabber as Player).SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint))
		{
			screamCounter = 0.01f;
		}
		if (screamCounter > 0f && !hasScreamed)
		{
			float num = screamCounter;
			screamCounter += 1f / 190f;
			if (num <= 0.2f && screamCounter > 0.2f)
			{
				Die();
			}
			if (screamCounter > 1f)
			{
				Scream();
			}
			else if (Mathf.Floor(num * 4f) != Mathf.Floor(screamCounter * 4f))
			{
				foreach (Grasp item in grabbedBy)
				{
					if (item != null && (!(item.grabber is Player) || !ModManager.MSC || (item.grabber as Player).SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint))
					{
						SmallScream(grabbedBy.Count > 0);
						break;
					}
				}
			}
			if (Mother != null)
			{
				Creature creature = ClosestCreature();
				if (Mother.BigAI.keepCloseToCreature == null || (creature != null && Random.value < 0.025f))
				{
					Mother.BigAI.keepCloseToCreature = creature;
				}
			}
		}
		AfterUpdate();
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (!placeRoom.abstractRoom.shelter || placeRoom.world.rainCycle.timer >= 40)
		{
			return;
		}
		if (base.State.eggSpawn)
		{
			room.AddObject(new NeedleEgg.EggHalf(this, placeRoom));
			base.State.eggSpawn = false;
		}
		List<AbstractCreature> list = new List<AbstractCreature>();
		for (int i = 0; i < placeRoom.abstractRoom.creatures.Count; i++)
		{
			if (placeRoom.abstractRoom.creatures[i].state.alive && placeRoom.abstractRoom.creatures[i].creatureTemplate.type != CreatureTemplate.Type.SmallNeedleWorm && !placeRoom.abstractRoom.creatures[i].creatureTemplate.smallCreature)
			{
				list.Add(placeRoom.abstractRoom.creatures[i]);
			}
		}
		if (list.Count > 0)
		{
			(base.abstractCreature.abstractAI as NeedleWormAbstractAI).mother = list[Random.Range(0, list.Count)];
		}
		if ((base.abstractCreature.abstractAI as NeedleWormAbstractAI).mother != null)
		{
			Custom.Log("small needle worm imprinted on:", (base.abstractCreature.abstractAI as NeedleWormAbstractAI).mother.ToString());
		}
		else
		{
			Custom.Log("no imprint");
		}
	}

	public void SmallScream(bool motherRespond)
	{
		room.PlaySound(SoundID.Small_Needle_Worm_Little_Trumpet, base.mainBodyChunk.pos, 1f, 1f);
		screaming = Mathf.Lerp(0.4f, 0.6f, Random.value);
		if (motherRespond && Mother != null)
		{
			if (grabbedBy.Count > 0)
			{
				Mother.BigAI.tracker.SeeCreature(grabbedBy[0].grabber.abstractCreature);
			}
			Mother.BigAI.SmallRespondCry();
		}
	}

	public void HangOnMom()
	{
		if (base.safariControlled && inputWithDiagonals.HasValue && inputWithDiagonals.Value.thrw && !lastInputWithDiagonals.Value.thrw)
		{
			LoseAllGrasps();
		}
		if (!MotherAttachable || !base.Consious || grabbedBy.Count > 0 || base.grasps[0].grabbed != Mother || (!room.GetTile(base.mainBodyChunk.pos).Solid && base.CollideWithTerrain && !base.safariControlled && Random.value < 1f / (Mother.Consious ? 400f : 70f)))
		{
			LoseAllGrasps();
			base.CollideWithTerrain = true;
			return;
		}
		if (!Custom.DistLess(base.mainBodyChunk.pos, Mother.tail[momTailSegment, 0], 30f))
		{
			Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, Mother.tail[momTailSegment, 0]) * (Vector2.Distance(base.mainBodyChunk.pos, Mother.tail[momTailSegment, 0]) - 30f);
			base.mainBodyChunk.pos += vector * 0.95f;
			base.mainBodyChunk.vel += vector * 0.95f;
			Mother.tail[momTailSegment, 0] -= vector * 0.05f;
			Mother.tail[momTailSegment, 2] -= vector * 0.05f;
		}
		base.CollideWithTerrain = Mother.segmentsInRopeMode[Mother.bodyChunks.Length + momTailSegment] > 0 && (Custom.DistLess(base.mainBodyChunk.pos, Mother.tail[momTailSegment, 0], 50f) || room.VisualContact(base.mainBodyChunk.pos, Mother.tail[momTailSegment, 0]));
		if (Random.value < 1f / 90f)
		{
			flying = ((Random.value < 0.5f) ? 1f : Random.value);
		}
	}

	public override void Act()
	{
		base.Act();
		bool flag = Random.value < 1f / 17f;
		if (base.safariControlled && inputWithDiagonals.HasValue && inputWithDiagonals.Value.pckp)
		{
			flag = true;
		}
		else if (base.safariControlled)
		{
			flag = false;
		}
		if (!flag || base.grasps[0] != null || !MotherAttachable || !Mother.NormalFlyingState)
		{
			return;
		}
		for (int num = Mother.tail.GetLength(0) - 2; num >= 0; num--)
		{
			flag = Random.value < Mathf.InverseLerp(0f, Mother.tail.GetLength(0) - 2, num);
			if (base.safariControlled && inputWithDiagonals.HasValue && inputWithDiagonals.Value.pckp)
			{
				flag = true;
			}
			else if (base.safariControlled)
			{
				flag = false;
			}
			if (flag && Custom.DistLess(base.mainBodyChunk.pos, Mother.tail[num, 0], 25f))
			{
				bool flag2 = true;
				for (int i = 0; i < Mother.grabbedBy.Count; i++)
				{
					if (Mother.grabbedBy[i].grabber is SmallNeedleWorm && (Mother.grabbedBy[i].grabber as SmallNeedleWorm).momTailSegment == num)
					{
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					momTailSegment = num;
					Grab(Mother, 0, Mother.bodyChunks.Length - 1, Grasp.Shareability.NonExclusive, 0f, overrideEquallyDominant: false, pacifying: false);
					break;
				}
			}
		}
	}

	public void Scream()
	{
		if (!hasScreamed)
		{
			Die();
			screaming = 1f;
			room.PlaySound(SoundID.Small_Needle_Worm_Intense_Trumpet_Scream, base.mainBodyChunk.pos, 1f, 1f);
			hasScreamed = true;
			if (Mother != null)
			{
				Mother.BigAI.BigRespondCry();
			}
			Custom.Log("LITTLE WORM SCREAM");
			Creature creature = ClosestCreature();
			if (creature != null && AbstractMother != null && AbstractMother.state.socialMemory.GetOrInitiateRelationship(creature.abstractCreature.ID) != null)
			{
				AbstractMother.state.socialMemory.GetOrInitiateRelationship(creature.abstractCreature.ID).like = -1f;
				AbstractMother.state.socialMemory.GetOrInitiateRelationship(creature.abstractCreature.ID).tempLike = -1f;
				AbstractMother.abstractAI.followCreature = creature.abstractCreature;
				Custom.Log($"mother coming after: {creature.abstractCreature}");
			}
			else
			{
				Custom.Log("mother found no target");
			}
		}
	}

	public Creature ClosestCreature()
	{
		if (grabbedBy.Count > 0)
		{
			return grabbedBy[0].grabber;
		}
		Creature result = null;
		float dst = float.MaxValue;
		for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
		{
			if (room.abstractRoom.creatures[i].realizedCreature != null && !room.abstractRoom.creatures[i].realizedCreature.dead && !room.abstractRoom.creatures[i].creatureTemplate.smallCreature && Custom.DistLess(base.mainBodyChunk.pos, room.abstractRoom.creatures[i].realizedCreature.DangerPos, dst) && room.abstractRoom.creatures[i].creatureTemplate.type != CreatureTemplate.Type.SmallNeedleWorm && room.abstractRoom.creatures[i].creatureTemplate.type != CreatureTemplate.Type.BigNeedleWorm)
			{
				dst = Vector2.Distance(base.mainBodyChunk.pos, room.abstractRoom.creatures[i].realizedCreature.DangerPos);
				result = room.abstractRoom.creatures[i].realizedCreature;
			}
		}
		return result;
	}

	public override void Die()
	{
		base.Die();
		if (screamCounter == 0f)
		{
			screamCounter = 0.01f;
		}
	}

	public void BitByPlayer(Grasp grasp, bool eu)
	{
		Scream();
		Die();
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
		}
		bites--;
		if (bites < 1)
		{
			(grasp.grabber as Player).ObjectEaten(this);
			grasp.Release();
			Destroy();
		}
	}

	public void ThrowByPlayer()
	{
	}
}
