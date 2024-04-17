using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class DaddyTentacle : Tentacle
{
	public class Task : ExtEnum<Task>
	{
		public static readonly Task Locomotion = new Task("Locomotion", register: true);

		public static readonly Task Hunt = new Task("Hunt", register: true);

		public static readonly Task ExamineSound = new Task("ExamineSound", register: true);

		public static readonly Task Grabbing = new Task("Grabbing", register: true);

		public Task(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public int tentacleNumber;

	public Vector2 tentacleDir;

	public float awayFromBodyRotation;

	public bool atGrabDest;

	private int foundNoGrabPos;

	public float chunksGripping;

	public int stun;

	private int secondaryGrabBackTrackCounter;

	private bool lastBackTrack;

	public IntVector2 secondaryGrabPos;

	public Vector2 preliminaryGrabDest;

	public Vector2 idealGrabPos;

	public bool neededForLocomotion;

	public Tracker.CreatureRepresentation huntCreature;

	public NoiseTracker.TheorizedSource checkSound;

	public int soundCheckCounter;

	public int soundCheckTimer;

	public Vector2? examineSoundPos;

	public BodyChunk grabChunk;

	public float sticky;

	public int[] chunksStickSounds;

	public Task task;

	private List<IntVector2> scratchPath;

	public Vector2 huntDirection;

	private IntVector2[] _cachedRays1 = new IntVector2[200];

	private readonly List<IntVector2> _cachedRays2 = new List<IntVector2>(10);

	public DaddyLongLegs daddy => owner as DaddyLongLegs;

	public void SwitchTask(Task newTask)
	{
		if (newTask != Task.Hunt)
		{
			huntCreature = null;
		}
		if (newTask != Task.ExamineSound)
		{
			checkSound = null;
			examineSoundPos = null;
		}
		if (newTask != Task.Grabbing && grabChunk != null)
		{
			room.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Release_Creature, grabChunk.pos);
			grabChunk = null;
		}
		if (newTask == Task.Locomotion && task != Task.Locomotion)
		{
			List<IntVector2> path = null;
			UpdateClimbGrabPos(ref path);
		}
		if (newTask == Task.ExamineSound)
		{
			soundCheckTimer = 0;
			soundCheckCounter = 0;
		}
		task = newTask;
	}

	public DaddyTentacle(DaddyLongLegs daddy, BodyChunk chunk, float length, int tentacleNumber, Vector2 tentacleDir)
		: base(daddy, chunk, length)
	{
		this.tentacleNumber = tentacleNumber;
		this.tentacleDir = tentacleDir;
		tProps = new TentacleProps(stiff: false, rope: true, shorten: false, 0.5f, 0f, 0f, 0f, 0f, 3.2f, 10f, 0.25f, 5f, 15, 60, 12, 20);
		if (ModManager.MMF)
		{
			tChunks = new TentacleChunk[Math.Max(3, (int)(length / 40f))];
		}
		else
		{
			tChunks = new TentacleChunk[(int)(length / 40f)];
		}
		for (int i = 0; i < tChunks.Length; i++)
		{
			tChunks[i] = new TentacleChunk(this, i, (float)(i + 1) / (float)tChunks.Length, 3f);
		}
		chunksStickSounds = new int[tChunks.Length];
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		SwitchTask(Task.Locomotion);
	}

	public override void Update()
	{
		base.Update();
		if (grabChunk != null && (grabChunk.owner.room == null || grabChunk.owner.room != daddy.room))
		{
			stun = 10;
			grabChunk = null;
		}
		if (daddy.dead)
		{
			neededForLocomotion = true;
			grabChunk = null;
			limp = true;
		}
		if (stun > 0)
		{
			stun--;
			grabChunk = null;
		}
		if (Mathf.Pow(UnityEngine.Random.value, 0.35f) > (daddy.State as DaddyLongLegs.DaddyState).tentacleHealth[tentacleNumber])
		{
			stun = Math.Max(stun, (int)Mathf.Lerp(-4f, 14f, Mathf.Pow(UnityEngine.Random.value, 0.5f + 20f * Mathf.Max(0f, (daddy.State as DaddyLongLegs.DaddyState).tentacleHealth[tentacleNumber]))));
		}
		if (grabChunk != null)
		{
			float num = Vector2.Distance(base.Tip.pos, grabChunk.pos);
			float num2 = (base.Tip.rad + grabChunk.rad) / 4f;
			Vector2 vector = Custom.DirVec(base.Tip.pos, grabChunk.pos);
			float num3 = grabChunk.mass / (grabChunk.mass + 0.01f);
			float num4 = 1f;
			base.Tip.pos += vector * (num - num2) * num3 * num4;
			base.Tip.vel += vector * (num - num2) * num3 * num4;
			grabChunk.pos -= vector * (num - num2) * (1f - num3) * num4;
			grabChunk.vel -= vector * (num - num2) * (1f - num3) * num4;
			if (grabChunk.owner is Player && UnityEngine.Random.value < Mathf.Lerp(0f, 1f / (daddy.SizeClass ? 20f : 10f), (grabChunk.owner as Player).GraspWiggle))
			{
				stun = Math.Max(stun, UnityEngine.Random.Range(1, daddy.SizeClass ? 7 : 17));
				grabChunk = null;
			}
		}
		limp = !daddy.Consious || stun > 0;
		for (int i = 0; i < tChunks.Length; i++)
		{
			tChunks[i].vel *= 0.9f;
			if (limp)
			{
				tChunks[i].vel.y -= 0.5f;
			}
			if (stun > 0 && !daddy.dead)
			{
				tChunks[i].vel += Custom.RNV() * 10f;
			}
		}
		if (limp)
		{
			for (int j = 0; j < tChunks.Length; j++)
			{
				tChunks[j].vel.y -= 0.7f;
			}
			return;
		}
		atGrabDest = false;
		if (backtrackFrom > -1)
		{
			secondaryGrabBackTrackCounter++;
			if (!lastBackTrack)
			{
				secondaryGrabBackTrackCounter += 20;
			}
		}
		lastBackTrack = backtrackFrom > -1;
		Vector2 pos = daddy.mainBodyChunk.pos;
		for (int k = 1; k < daddy.bodyChunks.Length; k++)
		{
			pos += daddy.bodyChunks[k].pos;
		}
		pos /= (float)daddy.bodyChunks.Length;
		awayFromBodyRotation = Custom.AimFromOneVectorToAnother(pos, connectedChunk.pos);
		chunksGripping = 0f;
		if (!neededForLocomotion)
		{
			bool flag = !daddy.safariControlled || (daddy.inputWithDiagonals.HasValue && daddy.inputWithDiagonals.Value.pckp);
			if (task != Task.Grabbing && flag)
			{
				LookForCreaturesToHunt();
				if (huntCreature == null && checkSound == null)
				{
					LookForSoundsToExamine();
				}
			}
		}
		else if (task != Task.Locomotion)
		{
			SwitchTask(Task.Locomotion);
		}
		if (task == Task.Hunt && (huntCreature == null || huntCreature.deleteMeNextFrame))
		{
			SwitchTask(Task.Locomotion);
		}
		else if (task != Task.Hunt && huntCreature != null)
		{
			huntCreature = null;
		}
		if (task == Task.ExamineSound && (checkSound == null || checkSound.slatedForDeletion))
		{
			SwitchTask(Task.Locomotion);
		}
		else if (task != Task.ExamineSound && checkSound != null)
		{
			checkSound = null;
		}
		if (task == Task.Grabbing && (grabChunk == null || grabChunk.owner.room != room || (ModManager.MMF && !daddy.Consious)))
		{
			SwitchTask(Task.Locomotion);
		}
		else if (task != Task.Grabbing && grabChunk != null)
		{
			room.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Release_Creature, grabChunk.pos);
			grabChunk = null;
		}
		if (task == Task.Locomotion)
		{
			Climb(ref scratchPath);
		}
		else if (task == Task.Hunt)
		{
			Hunt(ref scratchPath);
		}
		else if (task == Task.ExamineSound)
		{
			ExamineSound(ref scratchPath);
		}
		else if (task == Task.Grabbing)
		{
			MoveGrabDest(pos + Custom.DirVec(pos, grabChunk.pos) * 20f, ref scratchPath);
			Vector2 p = pos;
			bool flag2 = room.VisualContact(grabChunk.pos, pos);
			for (int num5 = tChunks.Length - 1; num5 >= 0; num5--)
			{
				Vector2 p2 = base.FloatBase;
				if (num5 > 0)
				{
					p2 = tChunks[num5 - 1].pos;
					if (!flag2 && !room.VisualContact(grabChunk.pos, tChunks[num5 - 1].pos))
					{
						p = tChunks[num5].pos;
						flag2 = true;
					}
				}
				tChunks[num5].vel += Custom.DirVec(tChunks[num5].pos, p2) * 1.2f;
				if (tChunks[num5].phase > -1f || room.GetTile(tChunks[num5].pos).Solid)
				{
					room.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Release_Creature, grabChunk.pos);
					grabChunk = null;
					SwitchTask(Task.Locomotion);
					break;
				}
			}
			if (task == Task.Grabbing)
			{
				grabChunk.vel += (Vector2)Vector3.Slerp(Custom.DirVec(grabChunk.pos, p), Custom.DirVec(base.Tip.pos, tChunks[tChunks.Length - 2].pos), 0.5f) * Custom.LerpMap(grabPath.Count, 3f, 18f, 0.65f, 0.25f) * (daddy.SizeClass ? 1f : 0.45f) / grabChunk.mass;
			}
		}
		for (int l = 0; l < tChunks.Length; l++)
		{
			float num6 = (float)l / (float)(tChunks.Length - 1);
			if (num6 < 0.2f)
			{
				tChunks[l].vel += Custom.DegToVec(awayFromBodyRotation) * Mathf.InverseLerp(0.2f, 0f, num6) * 5f;
			}
			for (int m = l + 1; m < tChunks.Length; m++)
			{
				PushChunksApart(l, m);
			}
		}
		Touch();
	}

	private void Touch()
	{
		bool flag = false;
		bool flag2 = !daddy.safariControlled || (daddy.inputWithDiagonals.HasValue && daddy.inputWithDiagonals.Value.pckp);
		for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
		{
			if (!(room.abstractRoom.creatures[i].realizedCreature != null && !room.abstractRoom.creatures[i].realizedCreature.inShortcut && room.abstractRoom.creatures[i].realizedCreature != daddy && !room.abstractRoom.creatures[i].tentacleImmune && flag2))
			{
				continue;
			}
			Creature realizedCreature = room.abstractRoom.creatures[i].realizedCreature;
			for (int j = 0; j < tChunks.Length; j++)
			{
				for (int k = 0; k < realizedCreature.bodyChunks.Length; k++)
				{
					if (!Custom.DistLess(tChunks[j].pos, realizedCreature.bodyChunks[k].pos, tChunks[j].rad + realizedCreature.bodyChunks[k].rad))
					{
						continue;
					}
					if (daddy.eyesClosed < 1 || UnityEngine.Random.value < 0.05f)
					{
						daddy.AI.tracker.SeeCreature(realizedCreature.abstractCreature);
						if (daddy.graphicsModule != null)
						{
							Tracker.CreatureRepresentation creatureRep = daddy.AI.tracker.RepresentationForObject(realizedCreature, AddIfMissing: false);
							(daddy.graphicsModule as DaddyGraphics).FeelSomethingWithTentacle(creatureRep, tChunks[j].pos);
						}
					}
					if (realizedCreature.abstractCreature.creatureTemplate.AI && realizedCreature.abstractCreature.abstractAI.RealAI != null && realizedCreature.abstractCreature.abstractAI.RealAI.tracker != null)
					{
						realizedCreature.abstractCreature.abstractAI.RealAI.tracker.SeeCreature(daddy.abstractCreature);
					}
					CollideWithCreature(j, realizedCreature.bodyChunks[k]);
					if (!neededForLocomotion && realizedCreature.newToRoomInvinsibility < 1 && grabChunk == null && j == tChunks.Length - 1 && (daddy.SizeClass || daddy.digestingCounter < 1) && (daddy.eyesClosed < 1 || UnityEngine.Random.value < (daddy.SizeClass ? 0.5f : 0.15f)) && (task == Task.Hunt || !IsCreatureCaughtEnough(realizedCreature.abstractCreature)))
					{
						flag = true;
						if (!(Vector2.Distance(tChunks[j].vel, realizedCreature.bodyChunks[k].vel) < Mathf.Lerp(1f, 8f, sticky)))
						{
							break;
						}
						bool flag3 = false;
						if (daddy.AI.tracker.RepresentationForObject(realizedCreature, AddIfMissing: false) != null && daddy.AI.DynamicRelationship(daddy.AI.tracker.RepresentationForObject(realizedCreature, AddIfMissing: false)).type == CreatureTemplate.Relationship.Type.Eats)
						{
							flag3 = true;
						}
						for (int l = 0; l < tChunks.Length && flag3; l++)
						{
							if (tChunks[l].phase > -1f || room.GetTile(tChunks[l].pos).Solid)
							{
								flag3 = false;
							}
						}
						if (!flag3)
						{
							break;
						}
						grabChunk = realizedCreature.bodyChunks[k];
						room.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Grab_Creature, tChunks[j].pos, 1f, 1f);
						SwitchTask(Task.Grabbing);
						return;
					}
					if (neededForLocomotion || (!(task == Task.Locomotion) && !(task == Task.ExamineSound)) || IsCreatureCaughtEnough(realizedCreature.abstractCreature))
					{
						break;
					}
					Tracker.CreatureRepresentation creatureRepresentation = daddy.AI.tracker.RepresentationForObject(realizedCreature, AddIfMissing: false);
					if (creatureRepresentation == null || !(daddy.AI.DynamicRelationship(creatureRepresentation).type == CreatureTemplate.Relationship.Type.Eats))
					{
						break;
					}
					bool flag4 = false;
					for (int m = 0; m < daddy.tentacles.Length; m++)
					{
						if (flag4)
						{
							break;
						}
						if (daddy.tentacles[m].huntCreature == creatureRepresentation)
						{
							flag4 = true;
						}
					}
					if (!flag4)
					{
						huntCreature = creatureRepresentation;
						if (checkSound != null)
						{
							checkSound.Destroy();
							checkSound = null;
						}
						SwitchTask(Task.Hunt);
					}
					break;
				}
			}
		}
		if (flag)
		{
			sticky = Mathf.Min(1f, sticky + 1f / 30f);
		}
		else
		{
			sticky = Mathf.Max(0f, sticky - 1f / 60f);
		}
	}

	private void CollideWithCreature(int tChunk, BodyChunk creatureChunk)
	{
		if (backtrackFrom <= -1 || backtrackFrom > tChunk)
		{
			float num = Vector2.Distance(tChunks[tChunk].pos, creatureChunk.pos);
			float num2 = (tChunks[tChunk].rad + creatureChunk.rad) / 4f;
			Vector2 vector = Custom.DirVec(tChunks[tChunk].pos, creatureChunk.pos);
			float num3 = creatureChunk.mass / (creatureChunk.mass + 0.01f);
			float num4 = 0.8f;
			tChunks[tChunk].pos += vector * (num - num2) * num3 * num4;
			tChunks[tChunk].vel += vector * (num - num2) * num3 * num4;
			creatureChunk.pos -= vector * (num - num2) * (1f - num3) * num4;
			creatureChunk.vel -= vector * (num - num2) * (1f - num3) * num4;
		}
	}

	private void LookForCreaturesToHunt()
	{
		if (neededForLocomotion || daddy.AI.preyTracker.TotalTrackedPrey == 0)
		{
			return;
		}
		Tracker.CreatureRepresentation creatureRepresentation = daddy.AI.preyTracker.GetTrackedPrey(UnityEngine.Random.Range(0, daddy.AI.preyTracker.TotalTrackedPrey));
		if (daddy.safariControlled)
		{
			if (huntDirection == Vector2.zero)
			{
				huntDirection = Custom.RNV() * 80f;
			}
			if (daddy.inputWithDiagonals.HasValue && daddy.inputWithDiagonals.Value.AnyDirectionalInput)
			{
				huntDirection = new Vector2(daddy.inputWithDiagonals.Value.x, daddy.inputWithDiagonals.Value.y) * 80f;
			}
			Creature creature = null;
			float num = float.MaxValue;
			float current = Custom.VecToDeg(huntDirection);
			for (int i = 0; i < daddy.room.abstractRoom.creatures.Count; i++)
			{
				if (daddy.abstractCreature != daddy.room.abstractRoom.creatures[i] && daddy.room.abstractRoom.creatures[i].realizedCreature != null)
				{
					float target = Custom.AimFromOneVectorToAnother(daddy.mainBodyChunk.pos, daddy.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos);
					float num2 = Custom.Dist(daddy.mainBodyChunk.pos, daddy.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos);
					if (Mathf.Abs(Mathf.DeltaAngle(current, target)) < 22.5f && num2 < num)
					{
						num = num2;
						creature = daddy.room.abstractRoom.creatures[i].realizedCreature;
					}
				}
			}
			if (creature != null)
			{
				creatureRepresentation = daddy.AI.tracker.RepresentationForCreature(creature.abstractCreature, addIfMissing: true);
			}
		}
		for (int j = 0; j < daddy.tentacles.Length; j++)
		{
			if (daddy.tentacles[j].huntCreature == creatureRepresentation)
			{
				return;
			}
		}
		if (!IsCreatureCaughtEnough(creatureRepresentation.representedCreature) && creatureRepresentation.BestGuessForPosition().room == daddy.abstractCreature.pos.room && !(Vector2.Distance(room.MiddleOfTile(creatureRepresentation.BestGuessForPosition()), base.FloatBase) > idealLength + 40f))
		{
			if (checkSound != null)
			{
				checkSound.Destroy();
				checkSound = null;
			}
			huntCreature = creatureRepresentation;
			SwitchTask(Task.Hunt);
		}
	}

	private bool IsCreatureCaughtEnough(AbstractCreature crit)
	{
		int num = 0;
		for (int i = 0; i < daddy.tentacles.Length; i++)
		{
			if (daddy.tentacles[i].grabChunk != null && daddy.tentacles[i].grabChunk.owner is Creature && (daddy.tentacles[i].grabChunk.owner as Creature).abstractCreature == crit)
			{
				num++;
			}
		}
		if ((float)num >= crit.creatureTemplate.bodySize * (daddy.SizeClass ? 1.5f : 2.5f))
		{
			return true;
		}
		return false;
	}

	private void LookForSoundsToExamine()
	{
		if (!daddy.SizeClass || neededForLocomotion || daddy.AI.noiseTracker.sources.Count == 0)
		{
			return;
		}
		int num = int.MaxValue;
		int num2 = -1;
		for (int i = 0; i < daddy.AI.noiseTracker.sources.Count; i++)
		{
			if (!Custom.DistLess(daddy.AI.noiseTracker.sources[i].pos, base.FloatBase, idealLength * 0.85f) || daddy.AI.noiseTracker.sources[i].age >= num || daddy.AI.noiseTracker.sources[i].creatureRep != null)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < daddy.tentacles.Length; j++)
			{
				if (flag)
				{
					break;
				}
				if (daddy.tentacles[j].checkSound == daddy.AI.noiseTracker.sources[i] || (daddy.AI.noiseTracker.sources[i].creatureRep != null && daddy.tentacles[j].huntCreature == daddy.AI.noiseTracker.sources[i].creatureRep))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				num = daddy.AI.noiseTracker.sources[i].age;
				num2 = i;
			}
		}
		if (num2 > -1)
		{
			checkSound = daddy.AI.noiseTracker.sources[num2];
			SwitchTask(Task.ExamineSound);
		}
	}

	private void Hunt(ref List<IntVector2> path)
	{
		if (huntCreature.BestGuessForPosition().room != daddy.abstractCreature.pos.room || huntCreature.deleteMeNextFrame)
		{
			SwitchTask(Task.Locomotion);
			return;
		}
		if (huntCreature.VisualContact)
		{
			MoveGrabDest(huntCreature.representedCreature.realizedCreature.mainBodyChunk.pos, ref path);
		}
		else
		{
			if (huntCreature.BestGuessForPosition().TileDefined)
			{
				MoveGrabDest(room.MiddleOfTile(huntCreature.BestGuessForPosition()), ref path);
			}
			for (int i = 0; i < tChunks.Length; i++)
			{
				if (!(huntCreature is Tracker.ElaborateCreatureRepresentation))
				{
					continue;
				}
				for (int j = 0; j < (huntCreature as Tracker.ElaborateCreatureRepresentation).ghosts.Count; j++)
				{
					if (room.GetTilePosition(tChunks[i].pos) == (huntCreature as Tracker.ElaborateCreatureRepresentation).ghosts[j].coord.Tile)
					{
						(huntCreature as Tracker.ElaborateCreatureRepresentation).ghosts[j].Push();
					}
				}
			}
		}
		if ((float)grabPath.Count * 20f > idealLength || neededForLocomotion)
		{
			float num = float.MaxValue;
			int num2 = -1;
			for (int k = 0; k < daddy.tentacles.Length; k++)
			{
				if (daddy.tentacles[k].task == Task.Locomotion && !daddy.tentacles[k].neededForLocomotion && (daddy.tentacles[k].idealLength > idealLength || neededForLocomotion) && !daddy.tentacles[k].atGrabDest && Mathf.Abs(daddy.tentacles[k].idealLength - (float)grabPath.Count * 20f) < num)
				{
					num = Mathf.Abs(daddy.tentacles[k].idealLength - (float)grabPath.Count * 20f);
					num2 = k;
				}
			}
			if (num2 > -1)
			{
				daddy.tentacles[num2].huntCreature = huntCreature;
				daddy.tentacles[num2].task = Task.Hunt;
				huntCreature = null;
				UpdateClimbGrabPos(ref path);
				return;
			}
		}
		if (Vector2.Distance(room.MiddleOfTile(huntCreature.BestGuessForPosition()), base.FloatBase) > idealLength * 1.5f)
		{
			huntCreature = null;
			UpdateClimbGrabPos(ref path);
			return;
		}
		for (int l = 0; l < tChunks.Length; l++)
		{
			if (backtrackFrom == -1 || backtrackFrom > l)
			{
				if (base.grabDest.HasValue && room.VisualContact(tChunks[l].pos, floatGrabDest.Value))
				{
					tChunks[l].vel += Vector2.ClampMagnitude(floatGrabDest.Value - tChunks[l].pos, 20f) / 20f * 1.2f;
				}
				else
				{
					tChunks[l].vel += Vector2.ClampMagnitude(room.MiddleOfTile(segments[tChunks[l].currentSegment]) - tChunks[l].pos, 20f) / 20f * 1.2f;
				}
			}
		}
	}

	private void Climb(ref List<IntVector2> path)
	{
		float t = Custom.LerpMap(daddy.stuckCounter, 50f, 200f, 0.5f, 0.95f);
		idealGrabPos = base.FloatBase + (Vector2)Vector3.Slerp(tentacleDir, daddy.moveDirection, t) * idealLength * 0.7f;
		Vector2 vector = base.FloatBase + (Vector2)Vector3.Slerp(Vector3.Slerp(tentacleDir, daddy.moveDirection, t), Custom.RNV(), Mathf.InverseLerp(20f, 200f, foundNoGrabPos)) * idealLength * Custom.LerpMap(Math.Max(foundNoGrabPos, daddy.stuckCounter), 20f, 200f, 0.7f, 1.2f);
		int num;
		for (num = SharedPhysics.RayTracedTilesArray(base.FloatBase, vector, _cachedRays1); num >= _cachedRays1.Length; num = SharedPhysics.RayTracedTilesArray(base.FloatBase, vector, _cachedRays1))
		{
			Custom.LogWarning($"DaddyTentacle Climb ray tracing limit exceeded, extending cache to {_cachedRays1.Length + 100} and trying again!");
			Array.Resize(ref _cachedRays1, _cachedRays1.Length + 100);
		}
		bool flag = false;
		for (int i = 0; i < num - 1; i++)
		{
			if (room.GetTile(_cachedRays1[i + 1]).IsSolid())
			{
				ConsiderGrabPos(Custom.RestrictInRect(vector, room.TileRect(_cachedRays1[i]).Shrink(1f)), idealGrabPos);
				flag = true;
				break;
			}
			if (room.GetTile(_cachedRays1[i]).horizontalBeam || room.GetTile(_cachedRays1[i]).verticalBeam)
			{
				ConsiderGrabPos(room.MiddleOfTile(_cachedRays1[i]), idealGrabPos);
				flag = true;
			}
		}
		if (flag)
		{
			foundNoGrabPos = 0;
		}
		else
		{
			foundNoGrabPos++;
		}
		bool flag2 = secondaryGrabBackTrackCounter < 200 && SecondaryGrabPosScore(secondaryGrabPos) > 0f;
		for (int j = 0; j < tChunks.Length; j++)
		{
			if (backtrackFrom != -1 && backtrackFrom <= j)
			{
				continue;
			}
			StickToTerrain(tChunks[j]);
			if (!base.grabDest.HasValue)
			{
				continue;
			}
			if (!atGrabDest && Custom.DistLess(tChunks[j].pos, floatGrabDest.Value, 20f))
			{
				atGrabDest = true;
			}
			if (tChunks[j].currentSegment <= grabPath.Count || !flag2)
			{
				tChunks[j].vel += Vector2.ClampMagnitude(floatGrabDest.Value - tChunks[j].pos, 20f) / 20f * 1.2f;
			}
			else if (j > 1 && segments.Count > grabPath.Count && flag2)
			{
				float num2 = Mathf.InverseLerp(grabPath.Count, segments.Count, tChunks[j].currentSegment);
				Vector2 vector2 = Custom.DirVec(tChunks[j - 2].pos, tChunks[j].pos) * (1f - num2) * 0.6f;
				vector2 += Custom.DirVec(tChunks[j].pos, room.MiddleOfTile(base.grabDest.Value)) * Mathf.Pow(1f - num2, 4f) * 2f;
				vector2 += Custom.DirVec(tChunks[j].pos, room.MiddleOfTile(secondaryGrabPos)) * Mathf.Pow(num2, 4f) * 2f;
				vector2 += Custom.DirVec(tChunks[j].pos, base.FloatBase) * Mathf.Sin(num2 * (float)Math.PI) * 0.3f;
				tChunks[j].vel += vector2.normalized * 1.2f;
				if (j == tChunks.Length - 1)
				{
					tChunks[j].vel += Vector2.ClampMagnitude(room.MiddleOfTile(secondaryGrabPos) - tChunks[j].pos, 20f) / 20f * 4.2f;
				}
			}
		}
		if (base.grabDest.HasValue)
		{
			ConsiderSecondaryGrabPos(base.grabDest.Value + new IntVector2(UnityEngine.Random.Range(-20, 21), UnityEngine.Random.Range(-20, 21)));
		}
		if (!base.grabDest.HasValue || !atGrabDest)
		{
			UpdateClimbGrabPos(ref path);
		}
	}

	private void ExamineSound(ref List<IntVector2> path)
	{
		if (!Custom.DistLess(checkSound.pos, base.FloatBase, idealLength * 1.1f) || checkSound.slatedForDeletion)
		{
			SwitchTask(Task.Locomotion);
			return;
		}
		soundCheckTimer--;
		if (soundCheckTimer < 1 || !examineSoundPos.HasValue || Custom.DistLess(examineSoundPos.Value, base.Tip.pos, 20f))
		{
			if (examineSoundPos.HasValue)
			{
				soundCheckTimer = UnityEngine.Random.Range(40, 180);
				soundCheckCounter++;
				if (soundCheckCounter > 17)
				{
					checkSound.Destroy();
					SwitchTask(Task.Locomotion);
					return;
				}
				examineSoundPos = null;
			}
			SharedPhysics.RayTracedTilesArray(checkSound.pos, checkSound.pos + Custom.RNV() * 150f, _cachedRays2);
			int num = _cachedRays2.Count;
			for (int i = 0; i < _cachedRays2.Count - 1; i++)
			{
				if (room.GetTile(_cachedRays2[i + 1]).Solid)
				{
					num = i;
					break;
				}
			}
			for (int num2 = _cachedRays2.Count - 1; num2 > num; num2--)
			{
				_cachedRays2.RemoveAt(num2);
			}
			while (_cachedRays2.Count > 0)
			{
				int index = _cachedRays2.Count - 1;
				if (room.aimap.getTerrainProximity(_cachedRays2[index]) < 2 || ((room.GetTile(_cachedRays2[index]).horizontalBeam || room.GetTile(_cachedRays2[index]).verticalBeam) && UnityEngine.Random.value < 0.05f))
				{
					examineSoundPos = Custom.RestrictInRect(room.MiddleOfTile(_cachedRays2[index]) + Custom.DirVec(base.FloatBase, room.MiddleOfTile(_cachedRays2[index])) * 20f, room.TileRect(_cachedRays2[index]).Shrink(1f));
					break;
				}
				_cachedRays2.RemoveAt(index);
			}
		}
		if (examineSoundPos.HasValue)
		{
			MoveGrabDest(examineSoundPos.Value, ref path);
		}
		for (int j = 0; j < tChunks.Length; j++)
		{
			if (backtrackFrom == -1 || backtrackFrom > j)
			{
				if (base.grabDest.HasValue && room.VisualContact(tChunks[j].pos, floatGrabDest.Value))
				{
					tChunks[j].vel += Vector2.ClampMagnitude(floatGrabDest.Value - tChunks[j].pos, 20f) / 20f * 1.2f;
				}
				else
				{
					tChunks[j].vel += Vector2.ClampMagnitude(room.MiddleOfTile(segments[tChunks[j].currentSegment]) - tChunks[j].pos, 20f) / 20f * 1.2f;
				}
			}
		}
	}

	private void StickToTerrain(TentacleChunk chunk)
	{
		if (floatGrabDest.HasValue && !Custom.DistLess(chunk.pos, floatGrabDest.Value, 200f))
		{
			return;
		}
		int num = (int)Mathf.Sign(chunk.pos.x - room.MiddleOfTile(chunk.pos).x);
		Vector2 vector = new Vector2(0f, 0f);
		IntVector2 tilePosition = room.GetTilePosition(chunk.pos);
		for (int i = 0; i < 8; i++)
		{
			if (room.GetTile(tilePosition + new IntVector2(Custom.eightDirectionsDiagonalsLast[i].x * num, Custom.eightDirectionsDiagonalsLast[i].y)).Solid)
			{
				if (Custom.eightDirectionsDiagonalsLast[i].x != 0)
				{
					vector.x = room.MiddleOfTile(chunk.pos).x + (float)(Custom.eightDirectionsDiagonalsLast[i].x * num) * (20f - chunk.rad);
				}
				if (Custom.eightDirectionsDiagonalsLast[i].y != 0)
				{
					vector.y = room.MiddleOfTile(chunk.pos).y + (float)Custom.eightDirectionsDiagonalsLast[i].y * (20f - chunk.rad);
				}
				break;
			}
		}
		if (vector.x == 0f && room.GetTile(chunk.pos).verticalBeam)
		{
			vector.x = room.MiddleOfTile(chunk.pos).x;
		}
		if (vector.y == 0f && room.GetTile(chunk.pos).horizontalBeam)
		{
			vector.y = room.MiddleOfTile(chunk.pos).y;
		}
		if (chunk.tentacleIndex > tChunks.Length / 2)
		{
			if (vector.x != 0f || vector.y != 0f)
			{
				if (chunksStickSounds[chunk.tentacleIndex] > 10)
				{
					owner.room.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Grab_Terrain, chunk.pos, Mathf.InverseLerp(tChunks.Length / 2, tChunks.Length - 1, chunk.tentacleIndex), 1f);
				}
				if (chunksStickSounds[chunk.tentacleIndex] > 0)
				{
					chunksStickSounds[chunk.tentacleIndex] = 0;
				}
				else
				{
					chunksStickSounds[chunk.tentacleIndex]--;
				}
			}
			else
			{
				if (chunksStickSounds[chunk.tentacleIndex] < -10)
				{
					owner.room.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Release_Terrain, chunk.pos, Mathf.InverseLerp(tChunks.Length / 2, tChunks.Length - 1, chunk.tentacleIndex), 1f);
				}
				if (chunksStickSounds[chunk.tentacleIndex] < 0)
				{
					chunksStickSounds[chunk.tentacleIndex] = 0;
				}
				else
				{
					chunksStickSounds[chunk.tentacleIndex]++;
				}
			}
		}
		if (vector.x != 0f)
		{
			chunk.vel.x += (vector.x - chunk.pos.x) * 0.1f;
			chunk.vel.y *= 0.9f;
		}
		if (vector.y != 0f)
		{
			chunk.vel.y += (vector.y - chunk.pos.y) * 0.1f;
			chunk.vel.x *= 0.9f;
		}
		if (vector.x != 0f || vector.y != 0f)
		{
			chunksGripping += 1f / (float)tChunks.Length;
		}
	}

	private void ConsiderGrabPos(Vector2 testPos, Vector2 idealGrabPos)
	{
		if (GrabPosScore(testPos, idealGrabPos) > GrabPosScore(preliminaryGrabDest, idealGrabPos))
		{
			preliminaryGrabDest = testPos;
		}
	}

	private float GrabPosScore(Vector2 testPos, Vector2 idealGrabPos)
	{
		float num = 100f / Vector2.Distance(testPos, idealGrabPos);
		if (base.grabDest.HasValue && room.GetTilePosition(testPos) == base.grabDest.Value)
		{
			num *= 1.5f;
		}
		for (int i = 0; i < 4; i++)
		{
			if (room.GetTile(testPos + Custom.fourDirections[i].ToVector2() * 20f).Solid)
			{
				num *= 2f;
				break;
			}
		}
		return num;
	}

	private void ConsiderSecondaryGrabPos(IntVector2 testPos)
	{
		if (!room.GetTile(testPos).Solid && SecondaryGrabPosScore(testPos) > SecondaryGrabPosScore(secondaryGrabPos))
		{
			secondaryGrabBackTrackCounter = 0;
			secondaryGrabPos = testPos;
		}
	}

	private float SecondaryGrabPosScore(IntVector2 testPos)
	{
		if (!base.grabDest.HasValue)
		{
			return 0f;
		}
		if (testPos.FloatDist(base.BasePos) < 7f)
		{
			return 0f;
		}
		float num = idealLength - (float)grabPath.Count * 20f;
		if (Vector2.Distance(room.MiddleOfTile(testPos), floatGrabDest.Value) > num)
		{
			return 0f;
		}
		if (!SharedPhysics.RayTraceTilesForTerrain(room, base.grabDest.Value, testPos))
		{
			return 0f;
		}
		float num2 = 0f;
		for (int i = 0; i < 8; i++)
		{
			if (room.GetTile(testPos + Custom.eightDirections[i]).Solid)
			{
				num2 += 1f;
			}
		}
		if (room.GetTile(testPos).horizontalBeam || room.GetTile(testPos).verticalBeam)
		{
			num2 += 1f;
		}
		if (num2 > 0f && testPos == secondaryGrabPos)
		{
			num2 += 1f;
		}
		if (num2 == 0f)
		{
			return 0f;
		}
		num2 += testPos.FloatDist(base.BasePos) / 10f;
		return num2 / (1f + Mathf.Abs(num * 0.75f - Vector2.Distance(room.MiddleOfTile(testPos), floatGrabDest.Value)) + Vector2.Distance(room.MiddleOfTile(testPos), room.MiddleOfTile(segments[segments.Count - 1])));
	}

	public float ReleaseScore()
	{
		float num = float.MaxValue;
		for (int i = tChunks.Length / 2; i < tChunks.Length; i++)
		{
			if (Custom.DistLess(tChunks[i].pos, idealGrabPos, num))
			{
				num = Vector2.Distance(tChunks[i].pos, idealGrabPos);
			}
		}
		return num;
	}

	public void UpdateClimbGrabPos(ref List<IntVector2> path)
	{
		if (huntCreature == null)
		{
			MoveGrabDest(preliminaryGrabDest, ref path);
		}
	}

	protected override IntVector2 GravityDirection()
	{
		if (!(UnityEngine.Random.value < 0.5f))
		{
			return new IntVector2(0, -1);
		}
		return new IntVector2((!(base.Tip.pos.x < connectedChunk.pos.x)) ? 1 : (-1), -1);
	}
}
