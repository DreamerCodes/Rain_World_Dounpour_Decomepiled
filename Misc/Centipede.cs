using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class Centipede : InsectoidCreature, IPlayerEdible
{
	public class CentipedeState : HealthState
	{
		public bool[] shells;

		public bool meatInitated;

		public CentipedeState(AbstractCreature creature)
			: base(creature)
		{
		}

		public override string ToString()
		{
			bool flag = false;
			string text = HealthBaseSaveString();
			if (shells != null)
			{
				for (int i = 0; i < shells.Length; i++)
				{
					if (!shells[i])
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				string text2 = "";
				for (int j = 0; j < shells.Length; j++)
				{
					text2 += (shells[j] ? "1" : "0");
				}
				text = text + "<cB>Shells<cC>" + text2;
			}
			if (meatInitated)
			{
				text += "<cB>MEATINIT";
			}
			foreach (KeyValuePair<string, string> unrecognizedSaveString in unrecognizedSaveStrings)
			{
				text = text + "<cB>" + unrecognizedSaveString.Key + "<cC>" + unrecognizedSaveString.Value;
			}
			return text;
		}

		public override void LoadFromString(string[] s)
		{
			base.LoadFromString(s);
			for (int i = 0; i < s.Length; i++)
			{
				switch (Regex.Split(s[i], "<cC>")[0])
				{
				case "Shells":
				{
					string text = Regex.Split(s[i], "<cC>")[1];
					shells = new bool[text.Length];
					for (int j = 0; j < text.Length && j < shells.Length; j++)
					{
						shells[j] = text[j] == '1';
					}
					break;
				}
				case "MEATINIT":
					meatInitated = true;
					break;
				}
			}
			unrecognizedSaveStrings.Remove("Shells");
			unrecognizedSaveStrings.Remove("MEATINIT");
		}

		public override void CycleTick()
		{
			base.CycleTick();
			if (!base.alive || shells == null)
			{
				return;
			}
			for (int i = 0; i < shells.Length; i++)
			{
				if (UnityEngine.Random.value < 0.2f)
				{
					shells[i] = UnityEngine.Random.value < 0.985f;
				}
			}
		}
	}

	public CentipedeAI AI;

	public bool flying;

	public bool wantToFly;

	public int flyModeCounter;

	public float wingsStartedUp;

	public bool outsideLevel;

	public int shockGiveUpCounter;

	public Rope[] connectionRopes;

	public bool bodyDirection;

	public Vector2 moveToPos;

	public int noFollowConCounter;

	public int directionChangeBlock;

	public int changeDirCounter;

	public bool moving;

	public float bodyWave;

	public float shockCharge;

	public float doubleGrabCharge;

	public bool visionDirection;

	private int bites = 5;

	public float size;

	public LightSource Glower;

	private BodyChunk GlowerHead;

	public int shellJustFellOff = -1;

	public bool Centiwing => base.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Centiwing;

	public bool Small => base.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.SmallCentipede;

	public bool Red => base.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.RedCentipede;

	public int HeadIndex
	{
		get
		{
			if (base.grasps[0] != null && base.grasps[1] == null)
			{
				return base.bodyChunks.Length - 1;
			}
			if (base.grasps[0] == null && base.grasps[1] != null)
			{
				return 0;
			}
			if (!bodyDirection)
			{
				return base.bodyChunks.Length - 1;
			}
			return 0;
		}
	}

	public BodyChunk HeadChunk => base.bodyChunks[HeadIndex];

	public override Vector2 VisionPoint => base.bodyChunks[(!visionDirection) ? (base.bodyChunks.Length - 1) : 0].pos;

	public CentipedeState CentiState => base.abstractCreature.state as CentipedeState;

	public bool AquaCenti
	{
		get
		{
			if (ModManager.MSC)
			{
				return base.abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti;
			}
			return false;
		}
	}

	public bool AquacentiSwim
	{
		get
		{
			if (AquaCenti && base.Submersion > 0.5f)
			{
				return !flying;
			}
			return false;
		}
	}

	public int BitesLeft => bites;

	public int FoodPoints => 2;

	public bool Edible => Small;

	public bool AutomaticPickUp => Small;

	public static float GenerateSize(AbstractCreature abstrCrit)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(abstrCrit.ID.RandomSeed);
		float result = Mathf.Lerp(0f, 1f, Mathf.Pow(UnityEngine.Random.value, 1.5f));
		if (abstrCrit.creatureTemplate.type == CreatureTemplate.Type.SmallCentipede)
		{
			result = 0f;
		}
		else if (abstrCrit.creatureTemplate.type == CreatureTemplate.Type.Centiwing)
		{
			result = Mathf.Lerp(0.5f, 0.65f, UnityEngine.Random.value);
		}
		else if (ModManager.MSC && abstrCrit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti)
		{
			result = Mathf.Lerp(0.9f, 1.8f, UnityEngine.Random.value);
		}
		else if (abstrCrit.creatureTemplate.type == CreatureTemplate.Type.RedCentipede)
		{
			result = 1f;
		}
		UnityEngine.Random.state = state;
		if (abstrCrit.creatureTemplate.type == CreatureTemplate.Type.Centipede && abstrCrit.spawnData != null && abstrCrit.spawnData.Length > 2)
		{
			string s = abstrCrit.spawnData.Substring(1, abstrCrit.spawnData.Length - 2);
			try
			{
				result = float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			catch
			{
			}
		}
		return result;
	}

	public Centipede(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		size = GenerateSize(abstractCreature);
		if (Small)
		{
			bites = 5;
		}
		else if (!CentiState.meatInitated)
		{
			if (Centiwing)
			{
				abstractCreature.state.meatLeft = 3;
			}
			else if (Red)
			{
				abstractCreature.state.meatLeft = 9;
			}
			else
			{
				abstractCreature.state.meatLeft = Mathf.RoundToInt(Mathf.Lerp(2.3f, 7f, size));
			}
			CentiState.meatInitated = true;
		}
		base.bodyChunks = new BodyChunk[Red ? 18 : (Small ? 5 : ((int)Mathf.Lerp(7f, 17f, size)))];
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			float num = (float)i / (float)(base.bodyChunks.Length - 1);
			float num2 = Mathf.Lerp(Mathf.Lerp(2f, 3.5f, size), Mathf.Lerp(4f, 6.5f, size), Mathf.Pow(Mathf.Clamp(Mathf.Sin((float)Math.PI * num), 0f, 1f), Mathf.Lerp(0.7f, 0.3f, size)));
			if (Red)
			{
				num2 += 1.5f;
			}
			if (Centiwing)
			{
				num2 = Mathf.Lerp(num2, Mathf.Lerp(2f, 3.5f, size), 0.4f);
			}
			if (Small)
			{
				num2 = Mathf.Lerp(1.5f, 3f, Mathf.Pow(Mathf.Clamp(Mathf.Sin((float)Math.PI * num), 0f, 1f), 0.5f));
			}
			base.bodyChunks[i] = new BodyChunk(this, i, new Vector2(0f, 0f), num2, Mathf.Lerp(3f / 70f, 11f / 34f, Mathf.Pow(size, 1.4f)));
			base.bodyChunks[i].loudness = 0f;
		}
		if (Red)
		{
			for (int j = 0; j < base.bodyChunks.Length; j++)
			{
				base.bodyChunks[j].mass += 0.02f + 0.08f * Mathf.Clamp01(Mathf.Sin(Mathf.InverseLerp(0f, base.bodyChunks.Length - 1, j) * (float)Math.PI));
			}
		}
		base.mainBodyChunkIndex = base.bodyChunks.Length / 2;
		if (!Small && (CentiState.shells == null || CentiState.shells.Length != base.bodyChunks.Length))
		{
			CentiState.shells = new bool[base.bodyChunks.Length];
			for (int k = 0; k < CentiState.shells.Length; k++)
			{
				CentiState.shells[k] = UnityEngine.Random.value < 0.985f;
			}
		}
		bodyChunkConnections = new BodyChunkConnection[base.bodyChunks.Length * (base.bodyChunks.Length - 1) / 2];
		int num3 = 0;
		for (int l = 0; l < base.bodyChunks.Length; l++)
		{
			for (int m = l + 1; m < base.bodyChunks.Length; m++)
			{
				float num4 = 0f;
				if (AquaCenti)
				{
					num4 = 0.7f;
				}
				bodyChunkConnections[num3] = new BodyChunkConnection(base.bodyChunks[l], base.bodyChunks[m], base.bodyChunks[l].rad + base.bodyChunks[m].rad, BodyChunkConnection.Type.Push, 1f - num4, -1f);
				num3++;
			}
		}
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = (AquaCenti ? 0.78f : 1.05f);
		flying = Centiwing;
		collisionRange = 150f;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new CentipedeGraphics(this);
		}
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		connectionRopes = new Rope[base.bodyChunks.Length - 1];
		for (int i = 0; i < connectionRopes.Length; i++)
		{
			connectionRopes[i] = new Rope(newRoom, base.bodyChunks[i].pos, base.bodyChunks[i + 1].pos, 1f);
		}
	}

	public override void Update(bool eu)
	{
		if (AquaCenti)
		{
			if (Glower == null)
			{
				GlowerHead = HeadChunk;
				Glower = new LightSource(GlowerHead.pos, environmentalLight: false, new Color(0.7f, 0.7f, 1f), this);
				room.AddObject(Glower);
				Glower.HardSetAlpha(0f);
				Glower.HardSetRad(0f);
				Glower.submersible = true;
			}
			else
			{
				if (GlowerHead == HeadChunk && !base.Stunned && shockCharge < 0.2f && base.Consious)
				{
					if (Glower.rad < 300f)
					{
						Glower.HardSetRad(Glower.rad + 11f);
					}
					if (Glower.Alpha < 0.5f)
					{
						Glower.HardSetAlpha(Glower.Alpha + 0.2f);
					}
				}
				else
				{
					if (Glower.rad > 0f)
					{
						Glower.HardSetRad(Glower.rad - 5f);
					}
					if (Glower.Alpha > 0f)
					{
						Glower.HardSetAlpha(Glower.Alpha - 0.05f);
					}
					if (Glower.Alpha <= 0f && Glower.rad <= 0f)
					{
						room.RemoveObject(Glower);
						Glower = null;
					}
				}
				if (Glower != null)
				{
					Glower.HardSetPos(GlowerHead.pos);
				}
			}
		}
		visionDirection = !visionDirection;
		if (grabbedBy.Count > 0 && !base.dead && Small)
		{
			shockCharge += 1f / 60f;
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				base.bodyChunks[i].vel += Custom.RNV() * UnityEngine.Random.value * 2f;
			}
			if (shockCharge >= 1f)
			{
				Creature grabber = grabbedBy[0].grabber;
				bool num = ModManager.MSC && grabbedBy[0].grabber is Player && (grabbedBy[0].grabber as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint;
				Shock(grabbedBy[0].grabber);
				if (num)
				{
					(grabber as Player).SaintStagger(680);
				}
				Stun(14);
				for (int j = 0; j < base.bodyChunks.Length; j++)
				{
					base.bodyChunks[j].vel += Custom.RNV() * UnityEngine.Random.value * 7f;
				}
			}
		}
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		for (int k = 0; k < base.grasps.Length; k++)
		{
			if (base.grasps[k] != null)
			{
				UpdateGrasp(k);
			}
		}
		if (!enteringShortCut.HasValue)
		{
			for (int l = 0; l < connectionRopes.Length; l++)
			{
				connectionRopes[l].Update(base.bodyChunks[l].pos, base.bodyChunks[l + 1].pos);
				float totalLength = connectionRopes[l].totalLength;
				float num2 = base.bodyChunks[l].rad + base.bodyChunks[l + 1].rad;
				if (totalLength > num2)
				{
					float num3 = base.bodyChunks[l].mass / (base.bodyChunks[l].mass + base.bodyChunks[l + 1].mass);
					float num4 = 1f;
					Vector2 vector = Custom.DirVec(base.bodyChunks[l].pos, connectionRopes[l].AConnect);
					base.bodyChunks[l].vel += vector * (totalLength - num2) * num4 * num3;
					base.bodyChunks[l].pos += vector * (totalLength - num2) * num4 * num3;
					vector = Custom.DirVec(base.bodyChunks[l + 1].pos, connectionRopes[l].BConnect);
					base.bodyChunks[l + 1].vel += vector * (totalLength - num2) * num4 * (1f - num3);
					base.bodyChunks[l + 1].pos += vector * (totalLength - num2) * num4 * (1f - num3);
				}
			}
			for (int num5 = connectionRopes.Length - 2; num5 >= 0; num5--)
			{
				connectionRopes[num5].Update(base.bodyChunks[num5].pos, base.bodyChunks[num5 + 1].pos);
				float totalLength2 = connectionRopes[num5].totalLength;
				float num6 = base.bodyChunks[num5].rad + base.bodyChunks[num5 + 1].rad;
				if (totalLength2 > num6)
				{
					float num7 = base.bodyChunks[num5].mass / (base.bodyChunks[num5].mass + base.bodyChunks[num5 + 1].mass);
					float num8 = 1f;
					Vector2 vector2 = Custom.DirVec(base.bodyChunks[num5].pos, connectionRopes[num5].AConnect);
					base.bodyChunks[num5].vel += vector2 * (totalLength2 - num6) * num8 * num7;
					base.bodyChunks[num5].pos += vector2 * (totalLength2 - num6) * num8 * num7;
					vector2 = Custom.DirVec(base.bodyChunks[num5 + 1].pos, connectionRopes[num5].BConnect);
					base.bodyChunks[num5 + 1].vel += vector2 * (totalLength2 - num6) * num8 * (1f - num7);
					base.bodyChunks[num5 + 1].pos += vector2 * (totalLength2 - num6) * num8 * (1f - num7);
				}
			}
			for (int m = 0; m < base.bodyChunks.Length - 2; m++)
			{
				base.bodyChunks[m].vel += Custom.DirVec(base.bodyChunks[m + 2].pos, base.bodyChunks[m].pos) * Mathf.Lerp(1f, 6f, shockCharge) * Mathf.Lerp(1f, 2f, size);
				base.bodyChunks[m + 2].vel += Custom.DirVec(base.bodyChunks[m].pos, base.bodyChunks[m + 2].pos) * Mathf.Lerp(1f, 6f, shockCharge) * Mathf.Lerp(1f, 2f, size);
			}
		}
		for (int n = 0; n < base.grasps.Length; n++)
		{
			if (base.grasps[n] != null)
			{
				UpdateGrasp(n);
			}
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		if (base.dead && grabbedBy.Count == 0)
		{
			for (int num9 = 0; num9 < base.bodyChunks.Length; num9++)
			{
				if (base.bodyChunks[num9].ContactPoint.y != 0)
				{
					base.bodyChunks[num9].vel.x *= 0.1f;
				}
			}
		}
		else if ((base.State as HealthState).health < 0.75f)
		{
			if (UnityEngine.Random.value * 0.75f > (base.State as HealthState).health && base.stun > 0)
			{
				base.stun--;
			}
			if (UnityEngine.Random.value > (base.State as HealthState).health && UnityEngine.Random.value < 1f / 3f)
			{
				Stun(4);
				if ((base.State as HealthState).health <= 0f && UnityEngine.Random.value < 1f / Mathf.Lerp(500f, 10f, 0f - (base.State as HealthState).health))
				{
					Die();
				}
			}
			if (!base.dead)
			{
				for (int num10 = 0; num10 < base.bodyChunks.Length; num10++)
				{
					if (UnityEngine.Random.value > (base.State as HealthState).health * 2f)
					{
						base.bodyChunks[num10].vel += Custom.RNV() * Mathf.Pow(UnityEngine.Random.value, Custom.LerpMap((base.State as HealthState).health, 0.75f, 0f, 3f, 0.1f, 2f)) * 4f * Mathf.InverseLerp(0.75f, 0f, (base.State as HealthState).health);
					}
				}
			}
		}
		if (base.Consious)
		{
			if (AquacentiSwim)
			{
				Act();
			}
			else if (base.Submersion > 0.5f && !flying)
			{
				Swim();
			}
			else
			{
				Act();
			}
		}
		shockCharge = Mathf.Max(0f, shockCharge - 1f / 120f);
	}

	private void Swim()
	{
		WorldCoordinate worldCoordinate = room.GetWorldCoordinate(HeadChunk.pos);
		worldCoordinate.y = Math.Max(worldCoordinate.y, room.defaultWaterLevel);
		MovementConnection movementConnection = default(MovementConnection);
		if (base.safariControlled)
		{
			if (inputWithoutDiagonals.HasValue)
			{
				MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
				if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					type = MovementConnection.MovementType.ShortCut;
				}
				if (inputWithoutDiagonals.Value.x != 0 || inputWithoutDiagonals.Value.y != 0)
				{
					movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithoutDiagonals.Value.x, inputWithoutDiagonals.Value.y) * Mathf.Max(80f, size * 240f)), 2);
				}
			}
		}
		else
		{
			movementConnection = (AI.pathFinder as StandardPather).FollowPath(worldCoordinate, actuallyFollowingThisPath: true);
		}
		if (!(movementConnection != default(MovementConnection)))
		{
			return;
		}
		if (movementConnection.destinationCoord.y > movementConnection.startCoord.y)
		{
			if (room.aimap.TileAccessibleToCreature(HeadChunk.pos, base.Template))
			{
				Act();
			}
			HeadChunk.vel.y += 1f;
			return;
		}
		HeadChunk.vel += Custom.DirVec(movementConnection.StartTile.ToVector2(), movementConnection.DestTile.ToVector2()) * 1.2f;
		if (room.aimap.TileAccessibleToCreature(HeadChunk.pos, base.Template))
		{
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				base.bodyChunks[i].vel += Custom.DirVec(movementConnection.StartTile.ToVector2(), movementConnection.DestTile.ToVector2()) * 0.05f + Custom.RNV() * UnityEngine.Random.value * 4f;
				if (ModManager.MSC)
				{
					base.bodyChunks[i].vel.y += Mathf.Clamp(room.WaterLevelDisplacement(base.bodyChunks[i].pos), -5f, 5f) * 0.05f;
				}
				else
				{
					base.bodyChunks[i].vel.y += Mathf.Clamp(room.FloatWaterLevel(base.bodyChunks[i].pos.x) - base.bodyChunks[i].pos.y, -5f, 5f) * 0.05f;
				}
			}
		}
		if (!moving)
		{
			return;
		}
		for (int j = 0; j < base.bodyChunks.Length; j++)
		{
			if (j > 0 && j < base.bodyChunks.Length - 1)
			{
				base.bodyChunks[j].vel += Custom.DirVec(base.bodyChunks[j].pos, base.bodyChunks[j + ((!bodyDirection) ? 1 : (-1))].pos) * 0.2f * Mathf.Lerp(0.5f, 1.5f, size);
			}
		}
	}

	private void Act()
	{
		if (AquacentiSwim)
		{
			flyModeCounter = 100;
		}
		if (Centiwing)
		{
			if (wantToFly)
			{
				if (flyModeCounter == 100)
				{
					flying = true;
				}
				flyModeCounter = Math.Min(100, flyModeCounter + 1);
			}
			else
			{
				if (flyModeCounter < 90)
				{
					flying = false;
				}
				flyModeCounter = Math.Max(0, flyModeCounter - 1);
			}
			float num = Mathf.InverseLerp(80f, 100f, flyModeCounter);
			if (wingsStartedUp < num)
			{
				wingsStartedUp = Mathf.Min(1f, wingsStartedUp + 0.025f);
			}
			else
			{
				wingsStartedUp = Mathf.Max(0f, wingsStartedUp - 0.025f);
			}
			wingsStartedUp = Mathf.Lerp(wingsStartedUp, num, 0.05f);
			if (base.abstractCreature.pos.y < -10)
			{
				flyModeCounter = 100;
				wantToFly = true;
				flying = true;
				wingsStartedUp = 1f;
				for (int i = 0; i < base.bodyChunks.Length; i++)
				{
					base.bodyChunks[i].vel.y += 3f;
				}
				if ((float)base.abstractCreature.pos.x < -10f)
				{
					for (int j = 0; j < base.bodyChunks.Length; j++)
					{
						base.bodyChunks[j].vel.x += 2f;
					}
				}
				else if (base.abstractCreature.pos.x > room.TileWidth + 10)
				{
					for (int k = 0; k < base.bodyChunks.Length; k++)
					{
						base.bodyChunks[k].vel.x -= 2f;
					}
				}
				moveToPos = new Vector2(room.PixelWidth / 2f, room.PixelHeight / 2f);
				outsideLevel = true;
			}
			else
			{
				outsideLevel = false;
			}
		}
		if (directionChangeBlock > 0)
		{
			directionChangeBlock -= (moving ? 1 : 0);
		}
		else if (!flying && !base.safariControlled)
		{
			if ((AI.pathFinder as CentipedePather).TileClosestToGoal(room.GetWorldCoordinate(base.bodyChunks[bodyDirection ? (base.bodyChunks.Length - 1) : 0].pos), room.GetWorldCoordinate(base.bodyChunks[(!bodyDirection) ? (base.bodyChunks.Length - 1) : 0].pos)))
			{
				changeDirCounter++;
				bool flag = room.aimap.getTerrainProximity(base.bodyChunks[(!bodyDirection) ? (base.bodyChunks.Length - 1) : 0].pos) > 1 || room.aimap.getTerrainProximity(base.bodyChunks[bodyDirection ? (base.bodyChunks.Length - 1) : 0].pos) > 1;
				if (changeDirCounter > ((!Centiwing) ? (flag ? 10 : 2) : (flag ? 40 : 10)))
				{
					bodyDirection = !bodyDirection;
					directionChangeBlock = 40;
					changeDirCounter = 0;
				}
			}
			else
			{
				changeDirCounter = 0;
			}
		}
		AI.Update();
		moving = AI.run > 0f && Custom.ManhattanDistance(room.GetWorldCoordinate(HeadChunk.pos), AI.pathFinder.GetDestination) > 2;
		if (flying || AquacentiSwim)
		{
			moving = true;
		}
		if (base.safariControlled)
		{
			moving = false;
			if (inputWithoutDiagonals.HasValue && (inputWithoutDiagonals.Value.x != 0 || inputWithoutDiagonals.Value.y != 0))
			{
				moving = true;
			}
			if ((!inputWithoutDiagonals.Value.pckp || grabbedBy.Count > 0) && (base.grasps[0] != null || base.grasps[1] != null))
			{
				LoseAllGrasps();
				shockCharge = 0f;
			}
			else if (inputWithoutDiagonals.Value.pckp && grabbedBy.Count == 0 && base.Consious)
			{
				if (base.grasps[0] == null && base.grasps[1] == null)
				{
					for (int l = 0; l < base.abstractCreature.Room.creatures.Count; l++)
					{
						if (base.abstractCreature.Room.creatures[l].realizedCreature == null)
						{
							continue;
						}
						Creature realizedCreature = base.abstractCreature.Room.creatures[l].realizedCreature;
						for (int m = 0; m < 2; m++)
						{
							if (base.grasps[m] != null)
							{
								continue;
							}
							int num2 = ((m != 0) ? (base.bodyChunks.Length - 1) : 0);
							for (int n = 0; n < realizedCreature.bodyChunks.Length; n++)
							{
								if (realizedCreature.abstractCreature != base.abstractCreature && Custom.DistLess(base.bodyChunks[num2].pos, realizedCreature.bodyChunks[n].pos, 50f + realizedCreature.bodyChunks[n].rad) && !realizedCreature.dead)
								{
									Grab(realizedCreature, m, n, Grasp.Shareability.NonExclusive, 1f, overrideEquallyDominant: false, pacifying: false);
									room.PlaySound(SoundID.Centipede_Attach, base.bodyChunks[num2]);
									break;
								}
							}
						}
					}
				}
				if (base.grasps[0] != null || base.grasps[1] != null)
				{
					int num3 = 0;
					if (base.grasps[0] == null)
					{
						num3 = 1;
					}
					if (Custom.DistLess(base.bodyChunks[0].pos, base.bodyChunks[base.bodyChunks.Length - 1].pos, 15f))
					{
						Shock(base.grasps[num3].grabbed);
						shockCharge = 0f;
						LoseAllGrasps();
					}
				}
			}
		}
		if (moving)
		{
			MovementConnection movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(HeadChunk.pos), actuallyFollowingThisPath: true);
			if (base.safariControlled && (movementConnection == default(MovementConnection) || !base.AllowableControlledAIOverride(movementConnection.type)) && inputWithoutDiagonals.HasValue)
			{
				MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
				if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					type = MovementConnection.MovementType.ShortCut;
				}
				if (inputWithoutDiagonals.Value.x != 0 || inputWithoutDiagonals.Value.y != 0)
				{
					movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithoutDiagonals.Value.x, inputWithoutDiagonals.Value.y) * Mathf.Max(80f, size * 240f)), 2);
				}
				if (inputWithoutDiagonals.Value.jmp && !lastInputWithoutDiagonals.Value.jmp)
				{
					bodyDirection = !bodyDirection;
				}
				if (inputWithoutDiagonals.Value.y < 0)
				{
					base.GoThroughFloors = true;
				}
				else
				{
					base.GoThroughFloors = false;
				}
			}
			moving = movementConnection != default(MovementConnection);
			if (movementConnection != default(MovementConnection))
			{
				if (shortcutDelay < 1 && (movementConnection.type == MovementConnection.MovementType.ShortCut || movementConnection.type == MovementConnection.MovementType.NPCTransportation))
				{
					enteringShortCut = movementConnection.StartTile;
					if (base.safariControlled)
					{
						bool flag2 = false;
						List<IntVector2> list = new List<IntVector2>();
						ShortcutData[] shortcuts = room.shortcuts;
						for (int num4 = 0; num4 < shortcuts.Length; num4++)
						{
							ShortcutData shortcutData = shortcuts[num4];
							if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != movementConnection.StartTile)
							{
								list.Add(shortcutData.StartTile);
							}
							if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == movementConnection.StartTile)
							{
								flag2 = true;
							}
						}
						if (flag2)
						{
							if (list.Count > 0)
							{
								list.Shuffle();
								NPCTransportationDestination = room.GetWorldCoordinate(list[0]);
							}
							else
							{
								NPCTransportationDestination = movementConnection.destinationCoord;
							}
						}
					}
					else if (movementConnection.type == MovementConnection.MovementType.NPCTransportation)
					{
						NPCTransportationDestination = movementConnection.destinationCoord;
					}
					return;
				}
				if (movementConnection.destinationCoord.TileDefined)
				{
					base.GoThroughFloors = movementConnection.DestTile.y < movementConnection.StartTile.y;
					moveToPos = room.MiddleOfTile(movementConnection.DestTile);
					if (movementConnection.DestTile.x != movementConnection.StartTile.x)
					{
						moveToPos.y += (float)VerticalSitSurface(moveToPos) * 5f;
					}
					if (movementConnection.DestTile.y != movementConnection.StartTile.y)
					{
						moveToPos.x += (float)HorizontalSitSurface(moveToPos) * 5f;
					}
					if (Centiwing || AquaCenti)
					{
						if (!RatherClimbThanFly(movementConnection.DestTile) && !flying)
						{
							moving = false;
						}
						if (!base.safariControlled && flying && AI.pathFinder.GetDestination.room == room.abstractRoom.index && AI.pathFinder.GetDestination.TileDefined && room.VisualContact(HeadChunk.pos, room.MiddleOfTile(AI.pathFinder.GetDestination)))
						{
							moveToPos = room.MiddleOfTile(AI.pathFinder.GetDestination);
						}
						else
						{
							MovementConnection movementConnection2 = movementConnection;
							bool flag3 = false;
							for (int num5 = 0; num5 < 10; num5++)
							{
								if (base.safariControlled)
								{
									movementConnection2 = movementConnection;
									if (num5 > 0)
									{
										break;
									}
								}
								else
								{
									movementConnection2 = (AI.pathFinder as StandardPather).FollowPath(movementConnection2.destinationCoord, actuallyFollowingThisPath: false);
								}
								if (!(movementConnection2 != default(MovementConnection)) || movementConnection2.destinationCoord == AI.pathFinder.GetDestination)
								{
									break;
								}
								if (movementConnection2.destinationCoord.TileDefined && room.VisualContact(HeadChunk.pos, room.MiddleOfTile(movementConnection2.destinationCoord)))
								{
									if (flying)
									{
										moveToPos = room.MiddleOfTile(movementConnection2.DestTile);
									}
									else if (!flag3 && !RatherClimbThanFly(movementConnection2.DestTile))
									{
										flag3 = true;
									}
								}
							}
							if (!flying && !RatherClimbThanFly(movementConnection.DestTile))
							{
								wantToFly = true;
							}
							else if (flying && RatherClimbThanFly(movementConnection.DestTile))
							{
								wantToFly = false;
							}
						}
					}
				}
				noFollowConCounter = 0;
			}
			else
			{
				noFollowConCounter++;
				if (noFollowConCounter > 40 && base.bodyChunks.Length != 0)
				{
					int num6 = UnityEngine.Random.Range(0, base.bodyChunks.Length);
					if (AccessibleTile(room.GetTilePosition(base.bodyChunks[num6].pos)))
					{
						moveToPos = base.bodyChunks[num6].pos;
					}
				}
			}
		}
		if (flying && AI.pathFinder.GetDestination.room == room.abstractRoom.index && AI.pathFinder.GetDestination.TileDefined && RatherClimbThanFly(AI.pathFinder.GetDestination.Tile) && room.GetTilePosition(HeadChunk.pos).FloatDist(AI.pathFinder.GetDestination.Tile) < 3f)
		{
			wantToFly = false;
		}
		if (base.grasps[0] == null && base.grasps[1] == null)
		{
			if (flying || AquacentiSwim)
			{
				if (AquaCenti)
				{
					base.buoyancy = 0.78f;
				}
				Fly();
			}
			else
			{
				Crawl();
			}
			doubleGrabCharge = Mathf.Max(0f, doubleGrabCharge - 0.025f);
			shockGiveUpCounter = Math.Max(0, shockGiveUpCounter - 2);
		}
		else
		{
			if (AquaCenti)
			{
				base.buoyancy = 0.15f;
			}
			if (base.grasps[0] == null || base.grasps[1] == null)
			{
				for (int num7 = 0; num7 < base.grasps.Length; num7++)
				{
					if (base.grasps[num7] == null || !(base.grasps[num7].grabbed is Creature) || !AI.DoIWantToShockCreature((base.grasps[num7].grabbed as Creature).abstractCreature))
					{
						continue;
					}
					moveToPos = base.grasps[num7].grabbedChunk.pos;
					if (room.VisualContact(HeadChunk.pos, base.grasps[num7].grabbedChunk.pos))
					{
						continue;
					}
					if (HeadIndex == 0)
					{
						for (int num8 = base.bodyChunks.Length - 1; num8 >= 0; num8--)
						{
							if (room.VisualContact(base.bodyChunks[num8].pos, HeadChunk.pos))
							{
								moveToPos = base.bodyChunks[num8].pos;
								break;
							}
						}
						continue;
					}
					for (int num9 = 0; num9 < base.bodyChunks.Length; num9++)
					{
						if (room.VisualContact(base.bodyChunks[num9].pos, HeadChunk.pos))
						{
							moveToPos = base.bodyChunks[num9].pos;
							break;
						}
					}
				}
			}
			HeadChunk.vel += Custom.DirVec(HeadChunk.pos, moveToPos) * Mathf.Pow(doubleGrabCharge, 2f) * 6f * Mathf.Lerp(0.7f, 1.3f, size);
			doubleGrabCharge = Mathf.Min(1f, doubleGrabCharge + 0.0125f);
			if (doubleGrabCharge > 0.9f)
			{
				shockGiveUpCounter = Math.Min(110, shockGiveUpCounter + 1);
				if (shockGiveUpCounter >= 110)
				{
					Stun(12);
					shockGiveUpCounter = 30;
					LoseAllGrasps();
				}
			}
		}
		if (AI.preyTracker.MostAttractivePrey == null || AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature == null || AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.collisionLayer == collisionLayer)
		{
			return;
		}
		for (int num10 = 0; num10 < AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.bodyChunks.Length; num10++)
		{
			for (int num11 = 0; num11 < base.bodyChunks.Length; num11++)
			{
				if (Custom.DistLess(AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.bodyChunks[num10].pos, base.bodyChunks[num11].pos, AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.bodyChunks[num10].rad + base.bodyChunks[num11].rad))
				{
					Collide(AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature, num11, num10);
				}
			}
		}
	}

	private void Crawl()
	{
		int num = 0;
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			if (!AccessibleTile(room.GetTilePosition(base.bodyChunks[i].pos)))
			{
				continue;
			}
			num++;
			base.bodyChunks[i].vel *= 0.7f;
			base.bodyChunks[i].vel.y += base.gravity * Mathf.Pow(Mathf.Max(0f, Mathf.Lerp((base.State as HealthState).ClampedHealth, 1f, UnityEngine.Random.value)), 0.25f);
			if (i > 0 && !AccessibleTile(room.GetTilePosition(base.bodyChunks[i - 1].pos)))
			{
				base.bodyChunks[i].vel *= 0.3f;
				base.bodyChunks[i].vel.y += base.gravity * Mathf.Pow(Mathf.Max(0f, Mathf.Lerp((base.State as HealthState).ClampedHealth, 1f, UnityEngine.Random.value)), 0.25f);
			}
			if (i < base.bodyChunks.Length - 1 && !AccessibleTile(room.GetTilePosition(base.bodyChunks[i + 1].pos)))
			{
				base.bodyChunks[i].vel *= 0.3f;
				base.bodyChunks[i].vel.y += base.gravity * Mathf.Pow(Mathf.Max(0f, Mathf.Lerp((base.State as HealthState).ClampedHealth, 1f, UnityEngine.Random.value)), 0.25f);
			}
			if (i <= 0 || i >= base.bodyChunks.Length - 1)
			{
				continue;
			}
			if (moving)
			{
				if (AccessibleTile(room.GetTilePosition(base.bodyChunks[i + ((!bodyDirection) ? 1 : (-1))].pos)))
				{
					base.bodyChunks[i].vel += Custom.DirVec(base.bodyChunks[i].pos, base.bodyChunks[i + ((!bodyDirection) ? 1 : (-1))].pos) * 1.5f * Mathf.Lerp(0.5f, 1.5f, size * (base.State as HealthState).ClampedHealth) * (Red ? 1.25f : 1f);
				}
				base.bodyChunks[i].vel -= Custom.DirVec(base.bodyChunks[i].pos, base.bodyChunks[i + (bodyDirection ? 1 : (-1))].pos) * 0.8f * Mathf.Lerp(0.7f, 1.3f, size * (base.State as HealthState).ClampedHealth);
				continue;
			}
			Vector2 vector = ((base.bodyChunks[i].pos - base.bodyChunks[i - 1].pos).normalized + (base.bodyChunks[i + 1].pos - base.bodyChunks[i].pos).normalized) / 2f;
			if (Mathf.Abs(vector.x) > 0.5f)
			{
				base.bodyChunks[i].vel.y -= (base.bodyChunks[i].pos.y - (room.MiddleOfTile(base.bodyChunks[i].pos).y + (float)VerticalSitSurface(base.bodyChunks[i].pos) * (10f - base.bodyChunks[i].rad))) * Mathf.Lerp(0.01f, 0.6f, Mathf.Pow(size * (base.State as HealthState).ClampedHealth, 1.2f));
			}
			if (Mathf.Abs(vector.y) > 0.5f)
			{
				base.bodyChunks[i].vel.x -= (base.bodyChunks[i].pos.x - (room.MiddleOfTile(base.bodyChunks[i].pos).x + (float)HorizontalSitSurface(base.bodyChunks[i].pos) * (10f - base.bodyChunks[i].rad))) * Mathf.Lerp(0.01f, 0.6f, Mathf.Pow(size * (base.State as HealthState).ClampedHealth, 1.2f));
			}
		}
		if (num > 0 && !Custom.DistLess(HeadChunk.pos, moveToPos, 10f))
		{
			if (Small)
			{
				HeadChunk.vel += Custom.DirVec(HeadChunk.pos, moveToPos) * Custom.LerpMap(num, 0f, base.bodyChunks.Length, 3f, 1f);
			}
			else
			{
				HeadChunk.vel += Custom.DirVec(HeadChunk.pos, moveToPos) * Custom.LerpMap(num, 0f, base.bodyChunks.Length, 6f, 3f) * Mathf.Lerp(0.7f, 1.3f, size * (base.State as HealthState).health) * (Centiwing ? 0.7f : 1f);
			}
		}
		if (Centiwing && num == 0)
		{
			flyModeCounter += 10;
			wantToFly = true;
		}
		if (AquaCenti)
		{
			HeadChunk.vel.Scale(new Vector2(0.2f, 0.2f));
		}
	}

	private int VerticalSitSurface(Vector2 pos)
	{
		if (room.GetTile(pos + new Vector2(0f, -20f)).Solid)
		{
			return -1;
		}
		if (room.GetTile(pos + new Vector2(0f, 20f)).Solid)
		{
			return 1;
		}
		return 0;
	}

	private int HorizontalSitSurface(Vector2 pos)
	{
		if (room.GetTile(pos + new Vector2(-20f, 0f)).Solid && !room.GetTile(pos + new Vector2(20f, 0f)).Solid)
		{
			return -1;
		}
		if (room.GetTile(pos + new Vector2(20f, 0f)).Solid && !room.GetTile(pos + new Vector2(-20f, 0f)).Solid)
		{
			return 1;
		}
		return 0;
	}

	private void Fly()
	{
		if (AquaCenti)
		{
			bodyWave += Mathf.Clamp(Vector2.Distance(HeadChunk.pos, AI.tempIdlePos.Tile.ToVector2()) / 80f, 0.1f, 1f);
		}
		else
		{
			bodyWave += 1f;
		}
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			float num = (float)i / (float)(base.bodyChunks.Length - 1);
			if (!bodyDirection)
			{
				num = 1f - num;
			}
			float num2 = Mathf.Sin((bodyWave - num * Mathf.Lerp(12f, 28f, size)) * (float)Math.PI * 0.11f);
			base.bodyChunks[i].vel *= 0.9f;
			base.bodyChunks[i].vel.y += base.gravity * wingsStartedUp;
			if (i <= 0 || i >= base.bodyChunks.Length - 1)
			{
				continue;
			}
			Vector2 vector = Custom.DirVec(base.bodyChunks[i].pos, base.bodyChunks[i + ((!bodyDirection) ? 1 : (-1))].pos);
			Vector2 vector2 = Custom.PerpendicularVector(vector);
			base.bodyChunks[i].vel += vector * 0.5f * Mathf.Lerp(0.5f, 1.5f, size);
			if (AquaCenti && AI.behavior == CentipedeAI.Behavior.Idle)
			{
				base.bodyChunks[i].vel *= Mathf.Clamp(Vector2.Distance(HeadChunk.pos, AI.tempIdlePos.Tile.ToVector2()) / 40f, 0.02f, 1f);
				if (Vector2.Distance(HeadChunk.pos, AI.tempIdlePos.Tile.ToVector2()) < 20f)
				{
					base.bodyChunks[i].vel *= 0.28f;
				}
			}
			base.bodyChunks[i].pos += vector2 * 2.5f * num2;
		}
		if (room.aimap.getTerrainProximity(moveToPos) > 2)
		{
			if (AquacentiSwim)
			{
				HeadChunk.vel += Custom.DirVec(HeadChunk.pos, moveToPos + Custom.DegToVec(bodyWave * 5f) * 10f) * 5f * Mathf.Lerp(0.7f, 1.3f, size);
			}
			else
			{
				HeadChunk.vel += Custom.DirVec(HeadChunk.pos, moveToPos + Custom.DegToVec(bodyWave * 10f) * 60f) * 4f * Mathf.Lerp(0.7f, 1.3f, size);
			}
		}
		else if (AquacentiSwim)
		{
			HeadChunk.vel += Custom.DirVec(HeadChunk.pos, moveToPos) * 2f * Mathf.Lerp(0.2f, 0.8f, size);
		}
		else
		{
			HeadChunk.vel += Custom.DirVec(HeadChunk.pos, moveToPos) * 4f * Mathf.Lerp(0.7f, 1.3f, size);
		}
	}

	private bool AccessibleTile(Vector2 testPos)
	{
		return AccessibleTile(room.GetTilePosition(testPos));
	}

	private bool AccessibleTile(IntVector2 testPos)
	{
		if (Centiwing && !flying)
		{
			return RatherClimbThanFly(testPos);
		}
		if (testPos.y != room.defaultWaterLevel)
		{
			return room.aimap.TileAccessibleToCreature(testPos, base.Template);
		}
		return ClimbableTile(testPos);
	}

	public bool ClimbableTile(Vector2 testPos)
	{
		return ClimbableTile(room.GetTilePosition(testPos));
	}

	public bool ClimbableTile(IntVector2 testPos)
	{
		if (Centiwing && !flying)
		{
			return RatherClimbThanFly(testPos);
		}
		if (!room.GetTile(testPos).wallbehind && !room.GetTile(testPos).verticalBeam && !room.GetTile(testPos).horizontalBeam)
		{
			return room.aimap.getTerrainProximity(testPos) < 2;
		}
		return true;
	}

	public bool RatherClimbThanFly(IntVector2 testPos)
	{
		if (!room.GetTile(testPos).verticalBeam && !room.GetTile(testPos).horizontalBeam)
		{
			return room.aimap.getTerrainProximity(testPos) < 2;
		}
		return true;
	}

	public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos appPos, Vector2 direction)
	{
		if (Red && chunk != null && chunk.index >= 0 && chunk.index < CentiState.shells.Length && (chunk.index == shellJustFellOff || CentiState.shells[chunk.index]))
		{
			if (chunk.index == shellJustFellOff)
			{
				shellJustFellOff = -1;
			}
			return false;
		}
		return base.SpearStick(source, dmg, chunk, appPos, direction);
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
	{
		shellJustFellOff = -1;
		if (!Small && hitChunk != null && hitChunk.index >= 0 && hitChunk.index < CentiState.shells.Length)
		{
			if (CentiState.shells[hitChunk.index])
			{
				if (directionAndMomentum.HasValue && (!Centiwing || UnityEngine.Random.value < 0.85f))
				{
					bool flag = true;
					if (type == DamageType.Electric || type == DamageType.Water)
					{
						flag = false;
					}
					else if (Red && damage >= 0.95f)
					{
						flag = true;
					}
					else if (UnityEngine.Random.value < Mathf.Lerp(0.9f, 0.1f, Mathf.Pow(size, 0.4f)))
					{
						flag = false;
					}
					else if (damage < Mathf.Lerp(0.4f, 1.1f, UnityEngine.Random.value))
					{
						flag = false;
					}
					else if (flag && UnityEngine.Random.value < 0.85f)
					{
						int num = 0;
						for (int i = 0; i < base.bodyChunks.Length; i++)
						{
							if (CentiState.shells[i])
							{
								num++;
							}
						}
						flag = (float)num / (float)base.bodyChunks.Length > (base.dead ? 0.33f : 0.55f);
					}
					if (flag && room != null)
					{
						shellJustFellOff = hitChunk.index;
						CentiState.shells[hitChunk.index] = false;
						if (base.graphicsModule != null)
						{
							for (int j = 0; j < ((!Red) ? 1 : 3); j++)
							{
								CentipedeShell centipedeShell = new CentipedeShell(hitChunk.pos, directionAndMomentum.Value * Mathf.Lerp(0.7f, 1.6f, UnityEngine.Random.value) + Custom.RNV() * UnityEngine.Random.value * ((j == 0) ? 3f : 6f), (base.graphicsModule as CentipedeGraphics).hue, (base.graphicsModule as CentipedeGraphics).saturation, hitChunk.rad * 1.8f * (1f / 14f) * 1.2f, hitChunk.rad * 1.3f * (1f / 11f) * 1.2f);
								if (base.abstractCreature.IsVoided())
								{
									centipedeShell.lavaImmune = true;
								}
								room.AddObject(centipedeShell);
							}
						}
						if (Red)
						{
							room.PlaySound(SoundID.Red_Centipede_Shield_Falloff, hitChunk);
						}
					}
				}
			}
			else
			{
				damage *= 1f + 0.45f * Mathf.Pow(size, 0.65f);
				if (Red)
				{
					stunBonus *= 1.5f;
				}
			}
		}
		if (!Red)
		{
			stunBonus = ((!AquaCenti) ? (stunBonus * (1f - (base.State as HealthState).health)) : (stunBonus * 2f));
		}
		else
		{
			if (hitChunk != null && room != null && hitChunk.index >= 0 && hitChunk.index < CentiState.shells.Length && (CentiState.shells[hitChunk.index] || hitChunk.index == shellJustFellOff))
			{
				if (type == DamageType.Bite || type == DamageType.Stab)
				{
					type = DamageType.Blunt;
				}
				int num2 = (int)Math.Min(Math.Max((damage * 15f + stunBonus) / base.Template.baseStunResistance / 2f, (int)(damage / base.Template.baseDamageResistance * 15f)), 25f);
				if (source != null && num2 > 5)
				{
					for (int k = 0; k < num2; k++)
					{
						room.AddObject(new Spark(source.pos + Custom.DegToVec(UnityEngine.Random.value * 360f) * 5f * UnityEngine.Random.value, source.vel * -0.1f + Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(0.2f, 0.4f, UnityEngine.Random.value) * source.vel.magnitude, new Color(1f, 1f, 1f), null, 10, 170));
					}
					room.AddObject(new StationaryEffect(source.pos, new Color(1f, 1f, 1f), null, StationaryEffect.EffectType.FlashingOrb));
					room.PlaySound(SoundID.Lizard_Head_Shield_Deflect, hitChunk);
				}
				damage *= 0.05f;
				stunBonus *= 0.2f;
			}
			else if (type == DamageType.Explosion)
			{
				damage *= 1.1f;
			}
			if (source != null && source.owner is Spear)
			{
				for (int l = 0; l < abstractPhysicalObject.stuckObjects.Count; l++)
				{
					if (abstractPhysicalObject.stuckObjects[l] is AbstractPhysicalObject.AbstractSpearStick && abstractPhysicalObject.stuckObjects[l].A != source.owner.abstractPhysicalObject && (abstractPhysicalObject.stuckObjects[l] as AbstractPhysicalObject.AbstractSpearStick).B == abstractPhysicalObject && (abstractPhysicalObject.stuckObjects[l] as AbstractPhysicalObject.AbstractSpearStick).chunk == hitChunk.index)
					{
						damage *= ((hitChunk.index == 0 || hitChunk.index == base.bodyChunks.Length - 1) ? 0.2f : 0.5f);
						break;
					}
				}
			}
			if (hitChunk.index == 0 || hitChunk.index == base.bodyChunks.Length - 1)
			{
				damage *= 0.5f;
			}
			stunBonus *= 1f - (base.State as HealthState).health * 0.5f;
		}
		float num3 = size;
		if (AquaCenti)
		{
			num3 /= 2f;
		}
		base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage * Mathf.Lerp(1.3f, 0.075f, Mathf.Pow(num3, 0.5f)), stunBonus);
	}

	public void UpdateGrasp(int g)
	{
		if (!base.safariControlled && UnityEngine.Random.value < 0.025f && (!(base.grasps[g].grabbed is Creature) || !AI.DoIWantToShockCreature((base.grasps[g].grabbed as Creature).abstractCreature) || !base.Consious || base.grasps[1 - g] != null))
		{
			ReleaseGrasp(g);
			return;
		}
		BodyChunk bodyChunk = base.bodyChunks[(g != 0) ? (base.bodyChunks.Length - 1) : 0];
		float num = Vector2.Distance(bodyChunk.pos, base.grasps[g].grabbedChunk.pos);
		if (num > 50f + base.grasps[g].grabbedChunk.rad)
		{
			ReleaseGrasp(g);
			return;
		}
		Vector2 vector = Custom.DirVec(bodyChunk.pos, base.grasps[g].grabbedChunk.pos);
		float rad = base.grasps[g].grabbedChunk.rad;
		float num2 = 0.95f;
		float num3 = base.grasps[g].grabbedChunk.mass / (base.grasps[g].grabbedChunk.mass + bodyChunk.mass);
		bodyChunk.pos -= (rad - num) * vector * num3 * num2;
		bodyChunk.vel -= (rad - num) * vector * num3 * num2;
		base.grasps[g].grabbedChunk.pos += (rad - num) * vector * (1f - num3) * num2;
		base.grasps[g].grabbedChunk.vel += (rad - num) * vector * (1f - num3) * num2;
		if (base.grasps[1 - g] != null)
		{
			return;
		}
		BodyChunk bodyChunk2 = base.bodyChunks[(g == 0) ? (base.bodyChunks.Length - 1) : 0];
		for (int i = 0; i < base.grasps[g].grabbed.bodyChunks.Length; i++)
		{
			if (Custom.DistLess(bodyChunk2.pos, base.grasps[g].grabbed.bodyChunks[i].pos, bodyChunk2.rad + base.grasps[g].grabbed.bodyChunks[i].rad + 10f))
			{
				BodyChunk bodyChunk3 = base.grasps[g].grabbed.bodyChunks[i];
				vector = Custom.DirVec(bodyChunk2.pos, bodyChunk3.pos);
				rad = bodyChunk3.rad;
				num2 = 0.95f;
				num3 = bodyChunk3.mass / (base.grasps[g].grabbedChunk.mass + bodyChunk2.mass);
				bodyChunk2.pos -= (rad - num) * vector * num3 * num2;
				bodyChunk2.vel -= (rad - num) * vector * num3 * num2;
				bodyChunk3.pos += (rad - num) * vector * (1f - num3) * num2;
				bodyChunk3.vel += (rad - num) * vector * (1f - num3) * num2;
				shockCharge += 1f / Mathf.Lerp(100f, 5f, size);
				if (!base.safariControlled && shockCharge >= 1f)
				{
					Shock(base.grasps[g].grabbed);
					shockCharge = 0f;
				}
				break;
			}
		}
	}

	public void Shock(PhysicalObject shockObj)
	{
		room.PlaySound(SoundID.Centipede_Shock, base.mainBodyChunk.pos);
		if (base.graphicsModule != null)
		{
			(base.graphicsModule as CentipedeGraphics).lightFlash = 1f;
			for (int i = 0; i < (int)Mathf.Lerp(4f, 8f, size); i++)
			{
				room.AddObject(new Spark(HeadChunk.pos, Custom.RNV() * Mathf.Lerp(4f, 14f, UnityEngine.Random.value), new Color(0.7f, 0.7f, 1f), null, 8, 14));
			}
		}
		for (int j = 0; j < base.bodyChunks.Length; j++)
		{
			base.bodyChunks[j].vel += Custom.RNV() * 6f * UnityEngine.Random.value;
			base.bodyChunks[j].pos += Custom.RNV() * 6f * UnityEngine.Random.value;
		}
		for (int k = 0; k < shockObj.bodyChunks.Length; k++)
		{
			shockObj.bodyChunks[k].vel += Custom.RNV() * 6f * UnityEngine.Random.value;
			shockObj.bodyChunks[k].pos += Custom.RNV() * 6f * UnityEngine.Random.value;
		}
		if (AquaCenti)
		{
			if (shockObj is Creature)
			{
				if (shockObj is Player && (shockObj as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
				{
					(shockObj as Player).PyroDeath();
				}
				else
				{
					(shockObj as Creature).Violence(base.mainBodyChunk, new Vector2(0f, 0f), (shockObj as Creature).mainBodyChunk, null, DamageType.Electric, 2f, 200f);
					room.AddObject(new CreatureSpasmer(shockObj as Creature, allowDead: false, (shockObj as Creature).stun));
					(shockObj as Creature).LoseAllGrasps();
				}
			}
			if (shockObj.Submersion > 0f)
			{
				room.AddObject(new UnderwaterShock(room, this, HeadChunk.pos, 14, 80f, 1f, this, new Color(0.7f, 0.7f, 1f)));
			}
			return;
		}
		if (shockObj is Creature)
		{
			if (Small)
			{
				(shockObj as Creature).Stun(120);
				room.AddObject(new CreatureSpasmer(shockObj as Creature, allowDead: false, (shockObj as Creature).stun));
				(shockObj as Creature).LoseAllGrasps();
			}
			else if (shockObj.TotalMass < base.TotalMass)
			{
				if (ModManager.MSC && shockObj is Player && (shockObj as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
				{
					(shockObj as Player).PyroDeath();
				}
				else
				{
					(shockObj as Creature).Die();
					room.AddObject(new CreatureSpasmer(shockObj as Creature, allowDead: true, (int)Mathf.Lerp(70f, 120f, size)));
				}
			}
			else
			{
				(shockObj as Creature).Stun((int)Custom.LerpMap(shockObj.TotalMass, 0f, base.TotalMass * 2f, 300f, 30f));
				room.AddObject(new CreatureSpasmer(shockObj as Creature, allowDead: false, (shockObj as Creature).stun));
				(shockObj as Creature).LoseAllGrasps();
				Stun(6);
				shockGiveUpCounter = Math.Max(shockGiveUpCounter, 30);
				AI.annoyingCollisions = Math.Min(AI.annoyingCollisions / 2, 150);
			}
		}
		if (shockObj.Submersion > 0f)
		{
			room.AddObject(new UnderwaterShock(room, this, HeadChunk.pos, 14, Mathf.Lerp(ModManager.MMF ? 0f : 200f, 1200f, size), 0.2f + 1.9f * size, this, new Color(0.7f, 0.7f, 1f)));
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (!base.Consious || !(otherObject is Creature) || base.safariControlled)
		{
			return;
		}
		AI.tracker.SeeCreature((otherObject as Creature).abstractCreature);
		if (!Small && !(otherObject is Centipede))
		{
			AI.AnnoyingCollision((otherObject as Creature).abstractCreature);
		}
		if (AI.DoIWantToShockCreature((otherObject as Creature).abstractCreature))
		{
			for (int i = 0; i < 2; i++)
			{
				if (myChunk != ((i != 0) ? (base.bodyChunks.Length - 1) : 0) || base.grasps[i] != null)
				{
					continue;
				}
				bool flag = true;
				for (int j = 0; j < grabbedBy.Count && flag; j++)
				{
					if (grabbedBy[j].grabber == otherObject)
					{
						flag = false;
					}
				}
				if (Centiwing && (room.aimap.getAItile(base.bodyChunks[myChunk].pos).fallRiskTile.y < 0 || UnityEngine.Random.value < 0.5f))
				{
					flag = false;
				}
				if (shockGiveUpCounter > 0)
				{
					flag = false;
				}
				if (flag)
				{
					room.PlaySound(SoundID.Centipede_Attach, base.bodyChunks[myChunk]);
					Grab(otherObject, i, otherChunk, Grasp.Shareability.NonExclusive, 1f, overrideEquallyDominant: false, pacifying: false);
				}
			}
		}
		if (otherObject is Centipede && (otherObject as Centipede).size > size)
		{
			AI.CheckRandomIdlePos();
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
	}

	public override void Die()
	{
		base.Die();
	}

	public override void Stun(int st)
	{
		if (UnityEngine.Random.value < 0.5f || Centiwing)
		{
			LoseAllGrasps();
		}
		if (Centiwing)
		{
			st *= 2;
		}
		base.Stun(st);
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		Vector2 vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].pos = newRoom.MiddleOfTile(pos) + Custom.RNV() * UnityEngine.Random.value * 2f;
			base.bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
			base.bodyChunks[i].vel = vector * 8f;
		}
		if (ModManager.MMF && MMF.cfgSafeCentipedes.Value && (base.grasps[0] != null || base.grasps[1] != null))
		{
			LoseAllGrasps();
			Stun(5);
		}
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
	}

	public override Color ShortCutColor()
	{
		if (base.abstractCreature.IsVoided())
		{
			return RainWorld.SaturatedGold;
		}
		if (AquaCenti)
		{
			return Custom.HSL2RGB(Mathf.Lerp(0.5f, 0.6f, 0.5f), 0.9f, 0.5f);
		}
		if (ModManager.MSC && Small && base.abstractCreature.superSizeMe)
		{
			return Custom.HSL2RGB(Mathf.Lerp(0.28f, 0.38f, 0.5f), 0.5f, 0.5f);
		}
		if (Centiwing)
		{
			return Custom.HSL2RGB(Mathf.Lerp(0.28f, 0.38f, 0.5f), 0.5f, 0.5f);
		}
		if (Red)
		{
			return Color.red;
		}
		return Custom.HSL2RGB(Mathf.Lerp(0.04f, 0.1f, 0.5f), 0.9f, 0.5f);
	}

	public void BitByPlayer(Grasp grasp, bool eu)
	{
		bites--;
		Die();
		room.PlaySound((bites == 0) ? SoundID.Slugcat_Eat_Centipede : SoundID.Slugcat_Bite_Centipede, base.mainBodyChunk.pos);
		base.firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
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
