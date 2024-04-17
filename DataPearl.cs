using System;
using System.Globalization;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class DataPearl : PlayerCarryableItem, IDrawable
{
	public class AbstractDataPearl : AbstractConsumable
	{
		public class DataPearlType : ExtEnum<DataPearlType>
		{
			public static readonly DataPearlType Misc = new DataPearlType("Misc", register: true);

			public static readonly DataPearlType Misc2 = new DataPearlType("Misc2", register: true);

			public static readonly DataPearlType CC = new DataPearlType("CC", register: true);

			public static readonly DataPearlType SI_west = new DataPearlType("SI_west", register: true);

			public static readonly DataPearlType SI_top = new DataPearlType("SI_top", register: true);

			public static readonly DataPearlType LF_west = new DataPearlType("LF_west", register: true);

			public static readonly DataPearlType LF_bottom = new DataPearlType("LF_bottom", register: true);

			public static readonly DataPearlType HI = new DataPearlType("HI", register: true);

			public static readonly DataPearlType SH = new DataPearlType("SH", register: true);

			public static readonly DataPearlType DS = new DataPearlType("DS", register: true);

			public static readonly DataPearlType SB_filtration = new DataPearlType("SB_filtration", register: true);

			public static readonly DataPearlType SB_ravine = new DataPearlType("SB_ravine", register: true);

			public static readonly DataPearlType GW = new DataPearlType("GW", register: true);

			public static readonly DataPearlType SL_bridge = new DataPearlType("SL_bridge", register: true);

			public static readonly DataPearlType SL_moon = new DataPearlType("SL_moon", register: true);

			public static readonly DataPearlType SU = new DataPearlType("SU", register: true);

			public static readonly DataPearlType UW = new DataPearlType("UW", register: true);

			public static readonly DataPearlType PebblesPearl = new DataPearlType("PebblesPearl", register: true);

			public static readonly DataPearlType SL_chimney = new DataPearlType("SL_chimney", register: true);

			public static readonly DataPearlType Red_stomach = new DataPearlType("Red_stomach", register: true);

			public DataPearlType(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public DataPearlType dataPearlType;

		public bool hidden;

		public AbstractDataPearl(World world, AbstractObjectType objType, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableData, DataPearlType dataPearlType)
			: base(world, objType, realizedObject, pos, ID, originRoom, placedObjectIndex, consumableData)
		{
			this.dataPearlType = dataPearlType;
		}

		public string BaseToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}<oA>{5}", ID.ToString(), type.ToString(), pos.SaveToString(), originRoom, placedObjectIndex, dataPearlType);
		}

		public override string ToString()
		{
			string baseString = BaseToString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "<oA>", unrecognizedAttributes);
		}

		public override void Consume()
		{
			base.Consume();
			hidden = false;
		}
	}

	public class PearlDebris : CosmeticSprite
	{
		private DataPearl pearl;

		private int mySeed;

		private float spriteDown;

		public PearlDebris(DataPearl pearl)
		{
			this.pearl = pearl;
			pos = pearl.firstChunk.pos;
			lastPos = pos;
			mySeed = (int)(pos.x + pos.y);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (pearl.AbstractPearl.hidden)
			{
				pos = pearl.firstChunk.pos;
				return;
			}
			vel *= 0.8f;
			if (!room.GetTile(pos).Solid)
			{
				vel.y -= 0.5f;
			}
			else
			{
				vel *= 0.2f;
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState(mySeed);
			sLeaser.sprites[0] = new FSprite("Pebble" + UnityEngine.Random.Range(5, 8));
			float value = UnityEngine.Random.value;
			sLeaser.sprites[0].scaleX = Mathf.Lerp(0.5f, 1.5f, value) * 1.2f;
			sLeaser.sprites[0].scaleY = Mathf.Lerp(0.5f, 1.5f, 1f - value) * 1.2f;
			sLeaser.sprites[0].rotation = UnityEngine.Random.value * 360f;
			spriteDown = Mathf.Lerp(4f, 8f, UnityEngine.Random.value);
			UnityEngine.Random.state = state;
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Items"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
			sLeaser.sprites[0].x = vector.x - camPos.x;
			sLeaser.sprites[0].y = vector.y - camPos.y - spriteDown;
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			base.ApplyPalette(sLeaser, rCam, palette);
			sLeaser.sprites[0].color = Color.Lerp(palette.blackColor, palette.fogColor, palette.fogAmount * 0.1f);
		}
	}

	public float lastGlimmer;

	public float glimmer;

	public float glimmerProg;

	public float glimmerSpeed;

	public int glimmerWait;

	private float darkness;

	public Vector2 hiddenPos;

	public Color? highlightColor;

	public bool uniquePearlCountedAsPickedUp;

	public bool forceReapplyPalette;

	public AbstractDataPearl AbstractPearl => abstractPhysicalObject as AbstractDataPearl;

	public override float ThrowPowerFactor => 0.5f;

	public DataPearl(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.4f;
		surfaceFriction = 0.4f;
		collisionLayer = 2;
		base.waterFriction = 0.98f;
		base.buoyancy = 0.4f;
		base.firstChunk.loudness = 3f;
		glimmerProg = 1f;
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (!AbstractPearl.isConsumed && AbstractPearl.placedObjectIndex >= 0 && AbstractPearl.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
		{
			base.firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[AbstractPearl.placedObjectIndex].pos);
		}
		else
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
		}
		NewRoom(placeRoom);
		if (AbstractPearl.hidden)
		{
			hiddenPos = base.firstChunk.pos;
			placeRoom.AddObject(new PearlDebris(this));
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (AbstractPearl.hidden)
		{
			if (!Custom.DistLess(base.firstChunk.pos, hiddenPos, 20f))
			{
				AbstractPearl.hidden = false;
			}
			if (room.abstractRoom.creatures.Count > 0)
			{
				Creature realizedCreature = room.abstractRoom.creatures[UnityEngine.Random.Range(0, room.abstractRoom.creatures.Count)].realizedCreature;
				if (realizedCreature != null && !realizedCreature.dead && realizedCreature.Template.bodySize > 0.5f)
				{
					for (int i = 0; i < realizedCreature.bodyChunks.Length; i++)
					{
						if (!Custom.DistLess(realizedCreature.bodyChunks[i].pos, realizedCreature.bodyChunks[i].lastPos, 2f) && Custom.DistLess(base.firstChunk.pos, realizedCreature.bodyChunks[i].pos, realizedCreature.bodyChunks[i].rad + 5f))
						{
							AbstractPearl.hidden = false;
							base.firstChunk.vel += Custom.DegToVec(Mathf.Lerp(-80f, 80f, UnityEngine.Random.value)) * Mathf.Lerp(3f, 5f, UnityEngine.Random.value);
							room.PlaySound(SoundID.SS_AI_Marble_Hit_Floor, base.firstChunk, loop: false, 0.5f + 0.5f * UnityEngine.Random.value, 1f);
							break;
						}
					}
				}
			}
		}
		lastGlimmer = glimmer;
		glimmer = Mathf.Sin(glimmerProg * (float)Math.PI) * UnityEngine.Random.value;
		if (AbstractPearl.dataPearlType != AbstractDataPearl.DataPearlType.PebblesPearl)
		{
			if (glimmerProg < 1f)
			{
				glimmerProg = Mathf.Min(1f, glimmerProg + glimmerSpeed);
			}
			else if (glimmerWait > 0)
			{
				glimmerWait--;
			}
			else
			{
				glimmerWait = UnityEngine.Random.Range(20, 40);
				glimmerProg = 0f;
				glimmerSpeed = 1f / Mathf.Lerp(5f, 15f, UnityEngine.Random.value);
			}
		}
		if (grabbedBy.Count > 0)
		{
			if ((ModManager.MMF && MMF.cfgKeyItemTracking.Value) || !(abstractPhysicalObject as AbstractConsumable).isConsumed)
			{
				(abstractPhysicalObject as AbstractConsumable).Consume();
			}
			if (ModManager.MMF && MMF.cfgKeyItemTracking.Value && room.game.session is StoryGameSession && AbstractPhysicalObject.UsesAPersistantTracker(AbstractPearl))
			{
				(room.game.session as StoryGameSession).AddNewPersistentTracker(AbstractPearl);
			}
			if (grabbedBy[0].grabber is Player && !uniquePearlCountedAsPickedUp)
			{
				if ((!ModManager.MSC) ? ((int)AbstractPearl.dataPearlType >= 2 && ((room.game.session is StoryGameSession && (room.game.session as StoryGameSession).saveState.miscWorldSaveData.EverMetMoon && (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.theMark && (room.game.session as StoryGameSession).saveState.miscWorldSaveData.SLOracleState.SpeakingTerms) || room.game.StoryCharacter == SlugcatStats.Name.Red) && room.game.GetStorySession.playerSessionRecords != null) : (PearlIsNotMisc(AbstractPearl.dataPearlType) && room.game.session is StoryGameSession && SlugcatStats.PearlsGivePassageProgress(room.game.session as StoryGameSession)))
				{
					room.game.GetStorySession.playerSessionRecords[(grabbedBy[0].grabber as Player).playerState.playerNumber].pearlsFound.Add(AbstractPearl.dataPearlType);
				}
				uniquePearlCountedAsPickedUp = true;
			}
		}
		base.firstChunk.collideWithObjects = grabbedBy.Count < 1;
		base.firstChunk.collideWithTerrain = grabbedBy.Count < 1;
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (firstContact && speed > 2f)
		{
			room.PlaySound(SoundID.SS_AI_Marble_Hit_Floor, base.firstChunk, loop: false, Custom.LerpMap(speed, 0f, 8f, 0.2f, 1f), 1f);
		}
	}

	public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[3];
		sLeaser.sprites[0] = new FSprite("JetFishEyeA");
		sLeaser.sprites[1] = new FSprite("tinyStar");
		sLeaser.sprites[2] = new FSprite("Futile_White");
		sLeaser.sprites[2].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
		AddToContainer(sLeaser, rCam, null);
	}

	public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (ModManager.MSC && forceReapplyPalette)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
			forceReapplyPalette = false;
		}
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		float num = Mathf.Lerp(lastGlimmer, glimmer, timeStacker);
		if (AbstractPearl.hidden)
		{
			vector.y -= 5f;
			num *= 1.2f;
		}
		sLeaser.sprites[1].x = vector.x - camPos.x - 0.5f;
		sLeaser.sprites[1].y = vector.y - camPos.y + 1.5f;
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		sLeaser.sprites[2].x = vector.x - camPos.x;
		sLeaser.sprites[2].y = vector.y - camPos.y;
		sLeaser.sprites[0].color = Color.Lerp(Custom.RGB2RGBA(color * Mathf.Lerp(1f, 0.2f, darkness), 1f), new Color(1f, 1f, 1f), num);
		if (highlightColor.HasValue)
		{
			Color b = Color.Lerp(highlightColor.Value, new Color(1f, 1f, 1f), num);
			sLeaser.sprites[2].color = b;
			sLeaser.sprites[1].color = Color.Lerp(Custom.RGB2RGBA(highlightColor.Value * Mathf.Lerp(1f, 0.5f, darkness), 1f), b, num);
		}
		else
		{
			sLeaser.sprites[1].color = Color.Lerp(Custom.RGB2RGBA(color * Mathf.Lerp(1.3f, 0.5f, darkness), 1f), new Color(1f, 1f, 1f), Mathf.Lerp(0.5f + 0.5f * num, 0.2f + 0.8f * num, darkness));
		}
		if (num > 0.9f && base.firstChunk.submersion == 1f)
		{
			sLeaser.sprites[0].color = new Color(0f, 0.003921569f, 0f);
			sLeaser.sprites[1].color = new Color(0f, 0.003921569f, 0f);
		}
		sLeaser.sprites[2].alpha = num * 0.5f;
		sLeaser.sprites[2].scale = 20f * num * ((AbstractPearl.dataPearlType != AbstractDataPearl.DataPearlType.Misc && AbstractPearl.dataPearlType != AbstractDataPearl.DataPearlType.Misc2) ? 1.35f : 1f) / 16f;
		sLeaser.sprites[1].isVisible = true;
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		if ((abstractPhysicalObject as AbstractDataPearl).dataPearlType == AbstractDataPearl.DataPearlType.CC || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == AbstractDataPearl.DataPearlType.DS || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == AbstractDataPearl.DataPearlType.GW || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == AbstractDataPearl.DataPearlType.HI || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == AbstractDataPearl.DataPearlType.LF_bottom || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == AbstractDataPearl.DataPearlType.LF_west || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == AbstractDataPearl.DataPearlType.SB_filtration || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == AbstractDataPearl.DataPearlType.SH || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == AbstractDataPearl.DataPearlType.SI_top || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == AbstractDataPearl.DataPearlType.SI_west || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == AbstractDataPearl.DataPearlType.SL_bridge || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == AbstractDataPearl.DataPearlType.SL_moon || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == AbstractDataPearl.DataPearlType.SB_ravine || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == AbstractDataPearl.DataPearlType.SU || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == AbstractDataPearl.DataPearlType.UW || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == AbstractDataPearl.DataPearlType.SL_chimney || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == AbstractDataPearl.DataPearlType.Red_stomach)
		{
			color = UniquePearlMainColor((abstractPhysicalObject as AbstractDataPearl).dataPearlType);
			highlightColor = UniquePearlHighLightColor((abstractPhysicalObject as AbstractDataPearl).dataPearlType);
		}
		else if (ModManager.MSC && ((abstractPhysicalObject as AbstractDataPearl).dataPearlType == MoreSlugcatsEnums.DataPearlType.SI_chat3 || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == MoreSlugcatsEnums.DataPearlType.SI_chat4 || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == MoreSlugcatsEnums.DataPearlType.SI_chat5 || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == MoreSlugcatsEnums.DataPearlType.Spearmasterpearl || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == MoreSlugcatsEnums.DataPearlType.SU_filt || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == MoreSlugcatsEnums.DataPearlType.DM || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == MoreSlugcatsEnums.DataPearlType.LC || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == MoreSlugcatsEnums.DataPearlType.OE || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == MoreSlugcatsEnums.DataPearlType.MS || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == MoreSlugcatsEnums.DataPearlType.RM || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == MoreSlugcatsEnums.DataPearlType.Rivulet_stomach || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == MoreSlugcatsEnums.DataPearlType.LC_second || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == MoreSlugcatsEnums.DataPearlType.CL || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == MoreSlugcatsEnums.DataPearlType.VS || (abstractPhysicalObject as AbstractDataPearl).dataPearlType == MoreSlugcatsEnums.DataPearlType.BroadcastMisc))
		{
			color = UniquePearlMainColor((abstractPhysicalObject as AbstractDataPearl).dataPearlType);
			highlightColor = UniquePearlHighLightColor((abstractPhysicalObject as AbstractDataPearl).dataPearlType);
		}
		else if ((abstractPhysicalObject as AbstractDataPearl).dataPearlType == AbstractDataPearl.DataPearlType.Misc2)
		{
			color = new Color(1f, 0.6f, 0.9f);
			highlightColor = new Color(1f, 1f, 1f);
		}
		else if ((abstractPhysicalObject as AbstractDataPearl).dataPearlType == AbstractDataPearl.DataPearlType.PebblesPearl)
		{
			int num = UnityEngine.Random.Range(0, 3);
			if (rCam.room.world.game.IsStorySession)
			{
				num = (abstractPhysicalObject as PebblesPearl.AbstractPebblesPearl).color;
			}
			switch (Mathf.Abs(num))
			{
			case 1:
				color = new Color(0.7f, 0.7f, 0.7f);
				break;
			case 2:
				if (num < 0)
				{
					color = new Color(1f, 0.47843137f, 0.007843138f);
				}
				else
				{
					color = new Color(0.01f, 0.01f, 0.01f);
				}
				break;
			default:
				if (num < 0)
				{
					color = new Color(0f, 0.45490196f, 0.6392157f);
				}
				else
				{
					color = new Color(1f, 0.47843137f, 0.007843138f);
				}
				break;
			}
		}
		else
		{
			color = new Color(0.7f, 0.7f, 0.7f);
		}
		if (ModManager.MSC && rCam.room.game.IsStorySession && rCam.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint && (abstractPhysicalObject as AbstractDataPearl).dataPearlType != MoreSlugcatsEnums.DataPearlType.CL && (abstractPhysicalObject as AbstractDataPearl).dataPearlType != AbstractDataPearl.DataPearlType.LF_west)
		{
			color = Color.Lerp(color, new Color(0.7f, 0.7f, 0.7f), 0.76f);
			highlightColor = new Color(1f, 1f, 1f);
		}
		if ((abstractPhysicalObject as AbstractDataPearl).dataPearlType == AbstractDataPearl.DataPearlType.PebblesPearl)
		{
			darkness = 0f;
		}
		else
		{
			darkness = rCam.room.Darkness(base.firstChunk.pos);
		}
	}

	public static Color UniquePearlMainColor(AbstractDataPearl.DataPearlType pearlType)
	{
		if (ModManager.MSC)
		{
			if (pearlType == AbstractDataPearl.DataPearlType.SI_west || pearlType == AbstractDataPearl.DataPearlType.SI_top || pearlType == MoreSlugcatsEnums.DataPearlType.SI_chat3 || pearlType == MoreSlugcatsEnums.DataPearlType.SI_chat4 || pearlType == MoreSlugcatsEnums.DataPearlType.SI_chat5)
			{
				return new Color(0.01f, 0.01f, 0.01f);
			}
			if (pearlType == MoreSlugcatsEnums.DataPearlType.Spearmasterpearl)
			{
				return new Color(0.04f, 0.01f, 0.04f);
			}
			if (pearlType == MoreSlugcatsEnums.DataPearlType.SU_filt)
			{
				return new Color(1f, 0.75f, 0.9f);
			}
			if (pearlType == MoreSlugcatsEnums.DataPearlType.DM)
			{
				return new Color(0.95686275f, 47f / 51f, 0.20784314f);
			}
			if (pearlType == MoreSlugcatsEnums.DataPearlType.LC)
			{
				return Custom.HSL2RGB(0.34f, 1f, 0.2f);
			}
			if (pearlType == MoreSlugcatsEnums.DataPearlType.LC_second)
			{
				return new Color(0.6f, 0f, 0f);
			}
			if (pearlType == MoreSlugcatsEnums.DataPearlType.OE)
			{
				return new Color(28f / 51f, 0.36862746f, 0.8f);
			}
			if (pearlType == MoreSlugcatsEnums.DataPearlType.MS)
			{
				return new Color(0.8156863f, 76f / 85f, 23f / 85f);
			}
			if (pearlType == MoreSlugcatsEnums.DataPearlType.RM)
			{
				return new Color(0.38431373f, 0.18431373f, 0.9843137f);
			}
			if (pearlType == MoreSlugcatsEnums.DataPearlType.Rivulet_stomach)
			{
				return new Color(0.5882353f, 74f / 85f, 32f / 51f);
			}
			if (pearlType == MoreSlugcatsEnums.DataPearlType.CL)
			{
				return new Color(0.48431373f, 29f / 102f, 1f);
			}
			if (pearlType == MoreSlugcatsEnums.DataPearlType.VS)
			{
				return new Color(0.53f, 0.05f, 0.92f);
			}
			if (pearlType == MoreSlugcatsEnums.DataPearlType.BroadcastMisc)
			{
				return new Color(0.9f, 0.7f, 0.8f);
			}
		}
		if (pearlType == AbstractDataPearl.DataPearlType.CC)
		{
			return new Color(0.9f, 0.6f, 0.1f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.DS)
		{
			return new Color(0f, 0.7f, 0.1f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.GW)
		{
			return new Color(0f, 0.7f, 0.5f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.HI)
		{
			return new Color(0.007843138f, 10f / 51f, 1f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.LF_bottom)
		{
			return new Color(1f, 0.1f, 0.1f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.LF_west)
		{
			return new Color(1f, 0f, 0.3f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.SB_filtration)
		{
			return new Color(0.1f, 0.5f, 0.5f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.SH)
		{
			return new Color(0.2f, 0f, 0.1f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.SI_top)
		{
			return new Color(0.01f, 0.01f, 0.01f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.SI_west)
		{
			return new Color(0.01f, 0.01f, 0.01f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.SL_bridge)
		{
			return new Color(0.4f, 0.1f, 0.9f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.SL_moon)
		{
			return new Color(0.9f, 0.95f, 0.2f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.SB_ravine)
		{
			return new Color(0.01f, 0.01f, 0.01f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.SU)
		{
			return new Color(0.5f, 0.6f, 0.9f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.UW)
		{
			return new Color(0.4f, 0.6f, 0.4f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.SL_chimney)
		{
			return new Color(1f, 0f, 0.55f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.Red_stomach)
		{
			return new Color(0.6f, 1f, 0.9f);
		}
		return new Color(0.7f, 0.7f, 0.7f);
	}

	public static Color? UniquePearlHighLightColor(AbstractDataPearl.DataPearlType pearlType)
	{
		if (ModManager.MSC)
		{
			if (pearlType == MoreSlugcatsEnums.DataPearlType.SI_chat3)
			{
				return new Color(0.4f, 0.1f, 0.6f);
			}
			if (pearlType == MoreSlugcatsEnums.DataPearlType.SI_chat4)
			{
				return new Color(0.4f, 0.6f, 0.1f);
			}
			if (pearlType == MoreSlugcatsEnums.DataPearlType.SI_chat5)
			{
				return new Color(0.6f, 0.1f, 0.4f);
			}
			if (pearlType == MoreSlugcatsEnums.DataPearlType.Spearmasterpearl)
			{
				return new Color(0.95f, 0f, 0f);
			}
			if (pearlType == MoreSlugcatsEnums.DataPearlType.RM)
			{
				return new Color(1f, 0f, 0f);
			}
			if (pearlType == MoreSlugcatsEnums.DataPearlType.LC_second)
			{
				return new Color(0.8f, 0.8f, 0f);
			}
			if (pearlType == MoreSlugcatsEnums.DataPearlType.CL)
			{
				return new Color(1f, 0f, 0f);
			}
			if (pearlType == MoreSlugcatsEnums.DataPearlType.VS)
			{
				return new Color(1f, 0f, 1f);
			}
			if (pearlType == MoreSlugcatsEnums.DataPearlType.BroadcastMisc)
			{
				return new Color(0.4f, 0.9f, 0.4f);
			}
		}
		if (pearlType == AbstractDataPearl.DataPearlType.CC)
		{
			return new Color(1f, 1f, 0f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.GW)
		{
			return new Color(0.5f, 1f, 0.5f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.HI)
		{
			return new Color(0.5f, 0.8f, 1f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.SH)
		{
			return new Color(1f, 0.2f, 0.6f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.SI_top)
		{
			return new Color(0.1f, 0.4f, 0.6f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.SI_west)
		{
			return new Color(0.1f, 0.6f, 0.4f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.SL_bridge)
		{
			return new Color(1f, 0.4f, 1f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.SB_ravine)
		{
			return new Color(0.6f, 0.1f, 0.4f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.UW)
		{
			return new Color(1f, 0.7f, 1f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.SL_chimney)
		{
			return new Color(0.8f, 0.3f, 1f);
		}
		if (pearlType == AbstractDataPearl.DataPearlType.Red_stomach)
		{
			return new Color(1f, 1f, 1f);
		}
		return null;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			if (i < 2)
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
			else if (ModManager.MSC && this is SpearMasterPearl && i >= (this as SpearMasterPearl).TailSprite && i < (this as SpearMasterPearl).TailSprite + (this as SpearMasterPearl).cords.GetLength(0))
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
			else
			{
				rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public static bool PearlIsNotMisc(AbstractDataPearl.DataPearlType pearlType)
	{
		if (pearlType != AbstractDataPearl.DataPearlType.Misc && pearlType != AbstractDataPearl.DataPearlType.Misc2 && (!ModManager.MSC || pearlType != MoreSlugcatsEnums.DataPearlType.BroadcastMisc))
		{
			return pearlType != AbstractDataPearl.DataPearlType.PebblesPearl;
		}
		return false;
	}

	public override void PickedUp(Creature upPicker)
	{
		abstractPhysicalObject.destroyOnAbstraction = false;
		room.PlaySound(SoundID.Slugcat_Pick_Up_Misc_Inanimate, base.firstChunk);
	}
}
