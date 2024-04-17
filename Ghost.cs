using System;
using System.IO;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class Ghost : CosmeticSprite, Conversation.IOwnAConversation
{
	public class Part
	{
		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		private Vector2 randomMovement;

		public float scale;

		public Part(float scale)
		{
			this.scale = scale;
		}

		public void Update()
		{
			lastPos = pos;
			pos += vel;
			vel += randomMovement * 1.4f * scale;
			randomMovement = Vector2.ClampMagnitude(randomMovement + Custom.RNV() * UnityEngine.Random.value * 0.1f, 1f);
		}
	}

	public class Rags
	{
		public Ghost ghost;

		public int firstSprite;

		public int totalSprites;

		public Vector2[][,] segments;

		private float conRad;

		public Rags(Ghost ghost, int firstSprite)
		{
			this.ghost = ghost;
			this.firstSprite = firstSprite;
			conRad = 30f * ghost.scale;
			int num = 6;
			segments = new Vector2[num][,];
			for (int i = 0; i < segments.Length; i++)
			{
				segments[i] = new Vector2[UnityEngine.Random.Range(7, 27), 7];
			}
			totalSprites = segments.Length;
		}

		public void Reset(Vector2 resetPos)
		{
			for (int i = 0; i < segments.Length; i++)
			{
				for (int j = 0; j < segments[i].GetLength(0); j++)
				{
					segments[i][j, 0] = resetPos + Custom.RNV();
					segments[i][j, 1] = segments[i][j, 0];
					segments[i][j, 2] *= 0f;
				}
			}
		}

		public void Update()
		{
			for (int i = 0; i < segments.Length; i++)
			{
				for (int j = 0; j < segments[i].GetLength(0); j++)
				{
					segments[i][j, 1] = segments[i][j, 0];
					segments[i][j, 0] += segments[i][j, 2];
					segments[i][j, 2] *= 0.999f;
					segments[i][j, 2] += Custom.RNV() * 0.2f * ghost.scale;
					segments[i][j, 5] = segments[i][j, 4];
					segments[i][j, 4] = (segments[i][j, 4] + segments[i][j, 6] * 0.05f).normalized;
					segments[i][j, 6] = (segments[i][j, 6] + Custom.RNV() * UnityEngine.Random.value * (segments[i][j, 2].magnitude / (ghost.scale * 3f))).normalized;
				}
				for (int k = 0; k < segments[i].GetLength(0); k++)
				{
					if (k > 0)
					{
						Vector2 normalized = (segments[i][k, 0] - segments[i][k - 1, 0]).normalized;
						float num = Vector2.Distance(segments[i][k, 0], segments[i][k - 1, 0]);
						segments[i][k, 0] += normalized * (conRad - num) * 0.5f;
						segments[i][k, 2] += normalized * (conRad - num) * 0.5f;
						segments[i][k - 1, 0] -= normalized * (conRad - num) * 0.5f;
						segments[i][k - 1, 2] -= normalized * (conRad - num) * 0.5f;
						if (k > 1)
						{
							normalized = (segments[i][k, 0] - segments[i][k - 2, 0]).normalized;
							segments[i][k, 2] += normalized * 0.2f;
							segments[i][k - 2, 2] -= normalized * 0.2f;
						}
						if (k < segments[i].GetLength(0) - 1)
						{
							segments[i][k, 4] = Vector3.Slerp(segments[i][k, 4], (segments[i][k - 1, 4] + segments[i][k + 1, 4]) / 2f, 0.05f);
							segments[i][k, 6] = Vector3.Slerp(segments[i][k, 6], (segments[i][k - 1, 6] + segments[i][k + 1, 6]) / 2f, 0.05f);
						}
					}
					else
					{
						segments[i][k, 0] = AttachPos(i, 1f);
					}
				}
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			for (int i = 0; i < segments.Length; i++)
			{
				sLeaser.sprites[firstSprite + i] = TriangleMesh.MakeLongMesh(segments[i].GetLength(0), pointyTip: false, customColor: true);
				sLeaser.sprites[firstSprite + i].shader = rCam.room.game.rainWorld.Shaders["TentaclePlant"];
				sLeaser.sprites[firstSprite + i].alpha = 0.3f + 0.7f * Mathf.InverseLerp(7f, 27f, segments[i].GetLength(0));
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			for (int i = 0; i < segments.Length; i++)
			{
				float num = 0f;
				Vector2 vector = AttachPos(i, timeStacker);
				float num2 = 0f;
				for (int j = 0; j < segments[i].GetLength(0); j++)
				{
					Vector2 vector2 = Vector2.Lerp(segments[i][j, 1], segments[i][j, 0], timeStacker);
					float num3 = 14f * ghost.scale * Vector3.Slerp(segments[i][j, 5], segments[i][j, 4], timeStacker).x;
					Vector2 normalized = (vector - vector2).normalized;
					Vector2 vector3 = Custom.PerpendicularVector(normalized);
					float num4 = Vector2.Distance(vector, vector2) / 5f;
					(sLeaser.sprites[firstSprite + i] as TriangleMesh).MoveVertice(j * 4, vector - normalized * num4 - vector3 * (num3 + num) * 0.5f - camPos);
					(sLeaser.sprites[firstSprite + i] as TriangleMesh).MoveVertice(j * 4 + 1, vector - normalized * num4 + vector3 * (num3 + num) * 0.5f - camPos);
					(sLeaser.sprites[firstSprite + i] as TriangleMesh).MoveVertice(j * 4 + 2, vector2 + normalized * num4 - vector3 * num3 - camPos);
					(sLeaser.sprites[firstSprite + i] as TriangleMesh).MoveVertice(j * 4 + 3, vector2 + normalized * num4 + vector3 * num3 - camPos);
					float num5 = 0.35f + 0.65f * Custom.BackwardsSCurve(Mathf.Pow(Mathf.Abs(Vector2.Dot(Vector3.Slerp(segments[i][j, 5], segments[i][j, 4], timeStacker), Custom.DegToVec(45f + Custom.VecToDeg(normalized)))), 2f), 0.5f);
					(sLeaser.sprites[firstSprite + i] as TriangleMesh).verticeColors[j * 4] = Color.Lerp(ghost.blackColor, ghost.goldColor, (num5 + num2) / 2f);
					(sLeaser.sprites[firstSprite + i] as TriangleMesh).verticeColors[j * 4 + 1] = Color.Lerp(ghost.blackColor, ghost.goldColor, (num5 + num2) / 2f);
					(sLeaser.sprites[firstSprite + i] as TriangleMesh).verticeColors[j * 4 + 2] = Color.Lerp(ghost.blackColor, ghost.goldColor, num5);
					(sLeaser.sprites[firstSprite + i] as TriangleMesh).verticeColors[j * 4 + 3] = Color.Lerp(ghost.blackColor, ghost.goldColor, num5);
					vector = vector2;
					num = num3;
					num2 = num5;
				}
			}
		}

		public Vector2 AttachPos(int rag, float timeStacker)
		{
			return Vector2.Lerp(ghost.spine[4].lastPos, ghost.spine[4].pos, timeStacker);
		}
	}

	public class Chains
	{
		public Ghost ghost;

		public int firstSprite;

		public int totalSprites;

		public Vector2[][,] segments;

		public int[] firstSpriteOfChains;

		public Chains(Ghost ghost, int firstSprite)
		{
			this.ghost = ghost;
			this.firstSprite = firstSprite;
			int num = 2;
			segments = new Vector2[num][,];
			firstSpriteOfChains = new int[num];
			for (int i = 0; i < segments.Length; i++)
			{
				segments[i] = new Vector2[27, 7];
				firstSpriteOfChains[i] = totalSprites;
				for (int j = 0; j < segments[i].GetLength(0); j++)
				{
					if (j % 3 < 2)
					{
						segments[i][j, 4] = new Vector2(19f, 0.2f);
					}
					else
					{
						segments[i][j, 4] = new Vector2(35f, 1f);
					}
					totalSprites += 2;
				}
			}
		}

		public void Reset(Vector2 resetPos)
		{
			for (int i = 0; i < segments.Length; i++)
			{
				for (int j = 0; j < segments[i].GetLength(0); j++)
				{
					segments[i][j, 0] = resetPos + Custom.RNV();
					segments[i][j, 1] = segments[i][j, 0];
					segments[i][j, 2] *= 0f;
				}
			}
		}

		public void Update()
		{
			for (int i = 0; i < segments.Length; i++)
			{
				for (int j = 0; j < segments[i].GetLength(0); j++)
				{
					segments[i][j, 5].y = segments[i][j, 5].x;
					segments[i][j, 5].x += segments[i][j, 6].x;
					segments[i][j, 6].x *= 0.99f;
					if (UnityEngine.Random.value < 1f / 14f)
					{
						segments[i][j, 6].x += Mathf.Pow(UnityEngine.Random.value, 5f) * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f) * segments[i][j, 2].magnitude / (15.5f * ghost.scale);
					}
					segments[i][j, 1] = segments[i][j, 0];
					segments[i][j, 0] += segments[i][j, 2];
					segments[i][j, 2] *= 0.999f;
					segments[i][j, 2] += Custom.RNV() * 0.2f * ghost.scale;
					segments[i][j, 2] = Vector2.Lerp(segments[i][j, 2], Custom.DirVec(segments[i][j, 0], ghost.spine[4].pos) * (segments[i][j, 2].magnitude + 3f * ghost.scale) * 0.5f, Custom.LerpMap(Vector2.Distance(segments[i][j, 0], ghost.spine[4].pos), 250f * ghost.scale, 600f * ghost.scale, 0f, 0.1f, 17f));
				}
				AttachChain(i);
				for (int k = 1; k < segments[i].GetLength(0); k++)
				{
					Vector2 normalized = (segments[i][k, 0] - segments[i][k - 1, 0]).normalized;
					float num = Vector2.Distance(segments[i][k, 0], segments[i][k - 1, 0]);
					float num2 = segments[i][k - 1, 4].y / (segments[i][k, 4].y + segments[i][k - 1, 4].y);
					segments[i][k, 0] += normalized * (segments[i][k, 4].x - num) * num2;
					segments[i][k, 2] += normalized * (segments[i][k, 4].x - num) * num2;
					segments[i][k - 1, 0] -= normalized * (segments[i][k, 4].x - num) * (1f - num2);
					segments[i][k - 1, 2] -= normalized * (segments[i][k, 4].x - num) * (1f - num2);
				}
				AttachChain(i);
				for (int num3 = segments[i].GetLength(0) - 2; num3 >= 0; num3--)
				{
					Vector2 normalized = (segments[i][num3, 0] - segments[i][num3 + 1, 0]).normalized;
					float num = Vector2.Distance(segments[i][num3, 0], segments[i][num3 + 1, 0]);
					float num4 = segments[i][num3 + 1, 4].y / (segments[i][num3, 4].y + segments[i][num3 + 1, 4].y);
					segments[i][num3, 0] += normalized * (segments[i][num3 + 1, 4].x - num) * num4;
					segments[i][num3, 2] += normalized * (segments[i][num3 + 1, 4].x - num) * num4;
					segments[i][num3 + 1, 0] -= normalized * (segments[i][num3 + 1, 4].x - num) * (1f - num4);
					segments[i][num3 + 1, 2] -= normalized * (segments[i][num3 + 1, 4].x - num) * (1f - num4);
				}
				AttachChain(i);
			}
		}

		private void AttachChain(int r)
		{
			Vector2 normalized = (segments[r][0, 0] - AttachPos(r, 1f)).normalized;
			float num = Vector2.Distance(segments[r][0, 0], AttachPos(r, 1f));
			segments[r][0, 0] += normalized * (segments[r][0, 4].x - num);
			segments[r][0, 2] += normalized * (segments[r][0, 4].x - num);
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			for (int i = 0; i < segments.Length; i++)
			{
				for (int j = 0; j < segments[i].GetLength(0); j++)
				{
					if (segments[i][j, 4].y == 0.2f)
					{
						sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2] = new FSprite("haloGlyph-1");
						sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + 1] = new FSprite("pixel");
						continue;
					}
					sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2] = new FSprite("ghostLink");
					sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2].anchorY = -2f / 3f;
					sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + 1] = new FSprite("ghostLink");
					sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + 1].anchorY = -2f / 3f;
				}
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			for (int i = 0; i < segments.Length; i++)
			{
				Vector2 vector = AttachPos(i, timeStacker);
				for (int j = 0; j < segments[i].GetLength(0); j++)
				{
					Vector2 vector2 = Vector2.Lerp(segments[i][j, 1], segments[i][j, 0], timeStacker);
					if (segments[i][j, 4].y == 0.2f)
					{
						sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2].x = (vector2.x + vector.x) / 2f - camPos.x;
						sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2].y = (vector2.y + vector.y) / 2f - camPos.y;
						sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + 1].x = (vector2.x + vector.x) / 2f - camPos.x - 1f;
						sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + 1].y = (vector2.y + vector.y) / 2f - camPos.y + 1f;
						sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2].color = Color.Lerp(ghost.blackColor, ghost.goldColor, 0.65f);
						sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + 1].color = ghost.goldColor;
					}
					else
					{
						Vector2 vector3 = Custom.PerpendicularVector(vector, vector2);
						float ang = Mathf.Sin(Mathf.Lerp(segments[i][j, 5].y, segments[i][j, 5].x, timeStacker)) * 360f / (float)Math.PI;
						for (int k = 0; k < 2; k++)
						{
							sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + k].x = vector2.x + vector3.x * (float)(-1 + k * 2) * ghost.scale * 0.9f - camPos.x;
							sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + k].y = vector2.y + vector3.y * (float)(-1 + k * 2) * ghost.scale * 0.9f - camPos.y;
							sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + k].rotation = Custom.AimFromOneVectorToAnother(vector2, vector);
							sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + k].scaleX = Mathf.Max(0.1f, Mathf.Abs(Custom.DegToVec(ang).x));
						}
						float x = Mathf.Abs(Vector2.Dot(Custom.DegToVec(ang), Custom.DirVec(vector, vector2)));
						x = Custom.BackwardsSCurve(x, 0.3f);
						sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2].color = Color.Lerp(ghost.blackColor, ghost.goldColor, 0.65f + 0.1f * Mathf.Sin(x * (float)Math.PI * 2f));
						sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + 1].color = Color.Lerp(ghost.blackColor, ghost.goldColor, 0.1f + 0.9f * x);
					}
					vector = vector2;
				}
			}
		}

		public Vector2 AttachPos(int chain, float timeStacker)
		{
			return Vector2.Lerp(ghost.legs[chain, 2].lastPos, ghost.legs[chain, 2].pos, timeStacker);
		}
	}

	private GhostWorldPresence worldGhost;

	public PlacedObject placedObject;

	public GhostConversation currentConversation;

	private float scale;

	private float lightSpriteScale;

	public int totalStaticSprites = 11;

	public int totalSprites;

	public int behindBodySprites;

	public Part[] spine;

	public Part[,] legs;

	public int spineSegments = 11;

	public int snoutSegments = 20;

	public int spineBendPoint = 7;

	public int thighSegments = 7;

	public int lowerLegSegments = 17;

	public float flip;

	public float defaultFlip;

	public float flipFrom;

	public float flipTo;

	public float flipProg;

	public float flipSpeed;

	public float airResistance = 0.6f;

	public Rags rags;

	public Chains chains;

	public Color blackColor;

	public Color goldColor = new Color(0.5294118f, 31f / 85f, 0.18431373f);

	public float sinBob;

	public int onScreenCounter;

	public float fadeOut;

	public float lastFadeOut;

	public bool theMarkMode;

	private bool hasRequestedShutDown;

	public int conversationStartedButPlayerLeftCounter;

	public int LightSprite => 0;

	public int BodyMeshSprite => behindBodySprites;

	public int NeckConnectorSprite => behindBodySprites + 7;

	public int HeadMeshSprite => behindBodySprites + 8;

	public int DistortionSprite => behindBodySprites + 9;

	public int FadeSprite => behindBodySprites + 10;

	public RainWorld rainWorld => room.game.rainWorld;

	public int ButtockSprite(int side)
	{
		return behindBodySprites + 1 + side;
	}

	public int ThightSprite(int side)
	{
		return behindBodySprites + 3 + side;
	}

	public int LowerLegSprite(int side)
	{
		return behindBodySprites + 5 + side;
	}

	public Ghost(Room room, PlacedObject placedObject, GhostWorldPresence worldGhost)
	{
		this.placedObject = placedObject;
		pos = placedObject.pos;
		this.worldGhost = worldGhost;
		scale = 0.75f;
		lightSpriteScale = 0f;
		if (worldGhost.ghostID == GhostWorldPresence.GhostID.CC)
		{
			defaultFlip = -0.7f;
		}
		else if (worldGhost.ghostID == GhostWorldPresence.GhostID.SI)
		{
			defaultFlip = -0.4f;
		}
		else if (worldGhost.ghostID == GhostWorldPresence.GhostID.LF)
		{
			scale = 0.7f;
		}
		else if (worldGhost.ghostID == GhostWorldPresence.GhostID.SH)
		{
			scale = 0.6f;
			lightSpriteScale = 3f;
			defaultFlip = 0.3f;
		}
		else if (worldGhost.ghostID == GhostWorldPresence.GhostID.UW)
		{
			defaultFlip = 0.2f;
		}
		else if (worldGhost.ghostID == GhostWorldPresence.GhostID.SB)
		{
			scale = 0.57f;
			lightSpriteScale = 2f;
			defaultFlip = 1f;
		}
		else if (ModManager.MSC && worldGhost.ghostID == MoreSlugcatsEnums.GhostID.CL)
		{
			defaultFlip = 0.2f;
		}
		else if (ModManager.MSC && worldGhost.ghostID == MoreSlugcatsEnums.GhostID.UG)
		{
			scale = 0.6f;
		}
		spine = new Part[spineSegments];
		for (int i = 0; i < spine.Length; i++)
		{
			spine[i] = new Part(scale);
		}
		legs = new Part[2, 3];
		for (int j = 0; j < legs.GetLength(0); j++)
		{
			for (int k = 0; k < legs.GetLength(1); k++)
			{
				legs[j, k] = new Part(scale);
			}
		}
		LoadElement("ghostScales");
		LoadElement("ghostPlates");
		LoadElement("ghostBand");
		totalSprites = 1;
		rags = new Rags(this, totalSprites);
		behindBodySprites = 1 + rags.totalSprites;
		totalSprites = behindBodySprites + totalStaticSprites;
		chains = new Chains(this, totalSprites);
		totalSprites += chains.totalSprites;
		sinBob = UnityEngine.Random.value;
		Reset();
		if (ModManager.MSC && room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			room.roomSettings.RainIntensity = 0.04f;
		}
	}

	public void Reset()
	{
		for (int i = 0; i < spine.Length; i++)
		{
			spine[i].pos = pos + Custom.RNV();
			spine[i].lastPos = spine[i].pos;
			spine[i].vel *= 0f;
		}
		for (int j = 0; j < legs.GetLength(0); j++)
		{
			for (int k = 0; k < legs.GetLength(1); k++)
			{
				legs[j, k].pos = pos + Custom.RNV();
				legs[j, k].lastPos = legs[j, k].pos;
				legs[j, k].vel *= 0f;
			}
		}
		chains.Reset(pos);
		rags.Reset(pos);
		flip = defaultFlip;
		flipFrom = defaultFlip;
		flipTo = defaultFlip;
		flipProg = 1f;
		flipSpeed = 1f;
	}

	private void LoadElement(string elementName)
	{
		if (Futile.atlasManager.GetAtlasWithName(elementName) == null)
		{
			string text = AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + elementName + ".png");
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text, clampWrapMode: false, crispPixels: true);
			Futile.atlasManager.LoadAtlasFromTexture(elementName, texture2D, textureFromAsset: false);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		rags.Update();
		chains.Update();
		lastFadeOut = fadeOut;
		if (fadeOut > 0f)
		{
			fadeOut = Mathf.Min(1f, fadeOut + 0.0125f);
			if (fadeOut == 1f && !hasRequestedShutDown)
			{
				hasRequestedShutDown = true;
				room.game.GhostShutDown(worldGhost.ghostID);
			}
		}
		else if (room.game.Players.Count > 0)
		{
			if (room.ViewedByAnyCamera(pos, 100f))
			{
				onScreenCounter++;
			}
			if (room.game.Players[0].realizedCreature != null && room.game.Players[0].realizedCreature.room == room && room.game.Players[0].realizedCreature.graphicsModule != null)
			{
				(room.game.Players[0].realizedCreature.graphicsModule as PlayerGraphics).LookAtPoint(pos, 10000f);
			}
			if (ModManager.CoopAvailable)
			{
				for (int i = 1; i < room.game.Players.Count; i++)
				{
					if (room.game.Players[i].realizedCreature != null && room.game.Players[i].realizedCreature.room == room && room.game.Players[i].realizedCreature.graphicsModule != null)
					{
						(room.game.Players[i].realizedCreature.graphicsModule as PlayerGraphics).LookAtPoint(pos, 10000f);
					}
				}
			}
			if (room.game.session is StoryGameSession && ((room.game.session as StoryGameSession).saveState.deathPersistentSaveData.theMark || (ModManager.MSC && (room.game.session as StoryGameSession).saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)))
			{
				theMarkMode = true;
				if (onScreenCounter > 80 && currentConversation == null)
				{
					StartConversation();
				}
			}
			else
			{
				AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
				if (onScreenCounter > 120 || (firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == room && Custom.DistLess(firstAlivePlayer.realizedCreature.mainBodyChunk.pos, pos, 400f)))
				{
					fadeOut = 0.01f;
				}
			}
			if (currentConversation != null)
			{
				currentConversation.Update();
				if (!room.ViewedByAnyCamera(pos, 100f))
				{
					conversationStartedButPlayerLeftCounter++;
				}
				else
				{
					conversationStartedButPlayerLeftCounter = 0;
				}
				if (currentConversation.slatedForDeletion || conversationStartedButPlayerLeftCounter > 40)
				{
					fadeOut = 0.01f;
				}
			}
		}
		sinBob += 1f / Mathf.Lerp(140f, 210f, UnityEngine.Random.value);
		pos = placedObject.pos + new Vector2(0f, Mathf.Sin(sinBob * (float)Math.PI * 2f) * 18f * scale);
		flipProg = Mathf.Min(1f, flipProg + flipSpeed);
		flip = Mathf.Lerp(flipFrom, flipTo, Custom.SCurve(flipProg, 0.7f));
		if (flipProg >= 1f && UnityEngine.Random.value < 0.1f)
		{
			flipFrom = flip;
			flipTo = Mathf.Clamp((flip + defaultFlip) / 2f + Mathf.Lerp(0.05f, 0.5f, Mathf.Pow(UnityEngine.Random.value, 2.5f)) * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f), -1f, 1f);
			flipProg = 0f;
			flipSpeed = 1f / (Mathf.Lerp(30f, 220f, UnityEngine.Random.value) * Mathf.Abs(flipFrom - flipTo));
		}
		float num = 30f * scale;
		for (int j = 0; j < spine.Length; j++)
		{
			float t = (float)j / (float)(spine.Length - 1);
			Vector2 vector = Custom.FlattenVectorAlongAxis(Custom.DegToVec(Mathf.Lerp(180f, -90f, t)), -15f, 1.3f) * Mathf.Lerp(100f, 40f, t) * scale;
			vector.x *= flip;
			vector += pos;
			spine[j].vel *= airResistance;
			spine[j].Update();
			spine[j].vel += (vector - spine[j].pos) / 10f;
			if (j > 0)
			{
				Vector2 normalized = (spine[j].pos - spine[j - 1].pos).normalized;
				float num2 = Vector2.Distance(spine[j].pos, spine[j - 1].pos);
				float num3 = ((num2 < num && j == spineBendPoint) ? 0f : 0.5f);
				spine[j].pos += normalized * (num - num2) * num3;
				spine[j].vel += normalized * (num - num2) * num3;
				spine[j - 1].pos -= normalized * (num - num2) * num3;
				spine[j - 1].vel -= normalized * (num - num2) * num3;
				if (j > 1)
				{
					normalized = (spine[j].pos - spine[j - 2].pos).normalized;
					spine[j].vel += normalized * 0.2f;
					spine[j - 2].vel -= normalized * 0.2f;
				}
			}
		}
		for (int k = 0; k < legs.GetLength(0); k++)
		{
			for (int l = 0; l < legs.GetLength(1); l++)
			{
				Vector2 a;
				switch (l)
				{
				case 0:
					a = Vector2.Lerp(pos, spine[spineBendPoint - 3].pos, 0.5f) + new Vector2(Mathf.Lerp(110f, 50f, Mathf.Abs(flip)) * ((k == 0) ? (-1f) : 1f) + flip * 8f, 15f) * scale;
					break;
				case 1:
					a = Vector2.Lerp(pos, spine[0].pos, 0.5f) + new Vector2(Mathf.Lerp(-70f, -30f, Mathf.Abs(flip)) * ((k == 0) ? (-1f) : 1f) - flip * 20f, -70f) * scale;
					break;
				default:
					a = spine[0].pos + new Vector2(Mathf.Lerp(-80f, -40f, Mathf.Abs(flip)) * ((k == 0) ? (-1f) : 1f), -90f) * scale;
					a = Vector2.Lerp(a, legs[k, 1].pos + new Vector2(-20f * ((k == 0) ? (-1f) : 1f), -10f) * scale, 0.5f);
					legs[k, l].vel += Custom.DirVec(legs[k, 0].pos, legs[k, l].pos) * 2f * scale;
					break;
				}
				legs[k, l].vel *= airResistance;
				legs[k, l].Update();
				legs[k, l].vel += (a - legs[k, l].pos) / 10f;
			}
			Vector2 normalized2 = (legs[k, 0].pos - legs[k, 1].pos).normalized;
			float num4 = Vector2.Distance(legs[k, 0].pos, legs[k, 1].pos);
			float num5 = 210f * scale;
			legs[k, 0].pos += normalized2 * (num5 - num4) * 0.5f;
			legs[k, 0].vel += normalized2 * (num5 - num4) * 0.5f;
			legs[k, 1].pos -= normalized2 * (num5 - num4) * 0.5f;
			legs[k, 1].vel -= normalized2 * (num5 - num4) * 0.5f;
			normalized2 = (legs[k, 0].pos - spine[0].pos).normalized;
			num4 = Vector2.Distance(legs[k, 0].pos, spine[0].pos);
			num5 = 120f * scale;
			legs[k, 0].pos += normalized2 * (num5 - num4) * 0.5f;
			legs[k, 0].vel += normalized2 * (num5 - num4) * 0.5f;
			spine[0].pos -= normalized2 * (num5 - num4) * 0.5f;
			spine[0].vel -= normalized2 * (num5 - num4) * 0.5f;
			normalized2 = (legs[k, 1].pos - legs[k, 2].pos).normalized;
			num4 = Vector2.Distance(legs[k, 1].pos, legs[k, 2].pos);
			num5 = 40f * scale;
			legs[k, 1].pos += normalized2 * (num5 - num4) * 0.15f;
			legs[k, 1].vel += normalized2 * (num5 - num4) * 0.15f;
			legs[k, 2].pos -= normalized2 * (num5 - num4) * 0.85f;
			legs[k, 2].vel -= normalized2 * (num5 - num4) * 0.85f;
		}
	}

	private void StartConversation()
	{
		Conversation.ID id = Conversation.ID.None;
		if (worldGhost.ghostID == GhostWorldPresence.GhostID.CC)
		{
			id = Conversation.ID.Ghost_CC;
		}
		else if (worldGhost.ghostID == GhostWorldPresence.GhostID.SI)
		{
			id = Conversation.ID.Ghost_SI;
		}
		else if (worldGhost.ghostID == GhostWorldPresence.GhostID.LF)
		{
			id = Conversation.ID.Ghost_LF;
		}
		else if (worldGhost.ghostID == GhostWorldPresence.GhostID.SH)
		{
			id = Conversation.ID.Ghost_SH;
		}
		else if (worldGhost.ghostID == GhostWorldPresence.GhostID.UW)
		{
			id = Conversation.ID.Ghost_UW;
		}
		else if (worldGhost.ghostID == GhostWorldPresence.GhostID.SB)
		{
			id = Conversation.ID.Ghost_SB;
		}
		else if (ModManager.MSC && worldGhost.ghostID == MoreSlugcatsEnums.GhostID.LC)
		{
			id = MoreSlugcatsEnums.ConversationID.Ghost_LC;
		}
		else if (ModManager.MSC && worldGhost.ghostID == MoreSlugcatsEnums.GhostID.UG)
		{
			id = MoreSlugcatsEnums.ConversationID.Ghost_UG;
		}
		else if (ModManager.MSC && worldGhost.ghostID == MoreSlugcatsEnums.GhostID.SL)
		{
			id = MoreSlugcatsEnums.ConversationID.Ghost_SL;
		}
		else if (ModManager.MSC && worldGhost.ghostID == MoreSlugcatsEnums.GhostID.CL)
		{
			id = MoreSlugcatsEnums.ConversationID.Ghost_CL;
		}
		else if (ModManager.MSC && worldGhost.ghostID == MoreSlugcatsEnums.GhostID.MS)
		{
			id = MoreSlugcatsEnums.ConversationID.Ghost_MS;
		}
		if (room.game.cameras[0].hud.dialogBox == null)
		{
			room.game.cameras[0].hud.InitDialogBox();
		}
		currentConversation = new GhostConversation(id, this, room.game.cameras[0].hud.dialogBox);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[totalSprites];
		rags.InitiateSprites(sLeaser, rCam);
		chains.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites[LightSprite] = new FSprite("Futile_White");
		sLeaser.sprites[LightSprite].shader = rCam.game.rainWorld.Shaders["LightSource"];
		sLeaser.sprites[LightSprite].color = new Color(22f / 85f, 0.5137255f, 0.79607844f);
		sLeaser.sprites[LightSprite].isVisible = lightSpriteScale > 0f;
		sLeaser.sprites[DistortionSprite] = new FSprite("Futile_White");
		sLeaser.sprites[DistortionSprite].shader = rCam.game.rainWorld.Shaders["GhostDistortion"];
		sLeaser.sprites[BodyMeshSprite] = TriangleMesh.MakeLongMesh(spineBendPoint, pointyTip: false, customColor: true);
		sLeaser.sprites[HeadMeshSprite] = TriangleMesh.MakeLongMesh(spineSegments - spineBendPoint + snoutSegments, pointyTip: false, customColor: true, "ghostScales");
		sLeaser.sprites[HeadMeshSprite].shader = rCam.game.rainWorld.Shaders["GhostSkin"];
		sLeaser.sprites[NeckConnectorSprite] = new FSprite("Circle20");
		sLeaser.sprites[FadeSprite] = new FSprite("Futile_White");
		sLeaser.sprites[FadeSprite].scaleX = 87.5f;
		sLeaser.sprites[FadeSprite].scaleY = 50f;
		sLeaser.sprites[FadeSprite].x = rCam.game.rainWorld.screenSize.x / 2f;
		sLeaser.sprites[FadeSprite].y = rCam.game.rainWorld.screenSize.y / 2f;
		sLeaser.sprites[FadeSprite].isVisible = false;
		for (int i = 0; i < legs.GetLength(0); i++)
		{
			sLeaser.sprites[ThightSprite(i)] = TriangleMesh.MakeLongMesh(thighSegments, pointyTip: false, customColor: true, "ghostBand");
			sLeaser.sprites[ThightSprite(i)].shader = rCam.game.rainWorld.Shaders["GhostSkin"];
			sLeaser.sprites[LowerLegSprite(i)] = TriangleMesh.MakeLongMesh(lowerLegSegments, pointyTip: false, customColor: true, "ghostPlates");
			sLeaser.sprites[LowerLegSprite(i)].shader = rCam.game.rainWorld.Shaders["GhostSkin"];
			sLeaser.sprites[ButtockSprite(i)] = new FSprite("Circle20");
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			if (i == DistortionSprite)
			{
				rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[i]);
			}
			else if (i == LightSprite)
			{
				rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[i]);
			}
			else if (i == FadeSprite)
			{
				rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[i]);
			}
			else
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		float num = Mathf.Clamp((Mathf.Lerp(spine[spineBendPoint - 2].lastPos.x, spine[spineBendPoint - 2].pos.x, timeStacker) - Mathf.Lerp(spine[spineBendPoint + 2].lastPos.x, spine[spineBendPoint + 2].pos.x, timeStacker)) / (80f * scale), -1f, 1f);
		float num2 = 10f * scale;
		float num3 = 10f * scale;
		float num4 = Mathf.Lerp(lastFadeOut, fadeOut, timeStacker);
		sLeaser.sprites[FadeSprite].isVisible = num4 > 0f;
		if (num4 > 0f)
		{
			sLeaser.sprites[FadeSprite].alpha = Mathf.InverseLerp(0f, 0.7f, num4);
			float num5 = Custom.SCurve(Mathf.InverseLerp(0.5f, 1f, num4), 0.3f);
			sLeaser.sprites[FadeSprite].color = new Color(1f - num5, 1f - num5, 1f - num5);
		}
		rags.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		chains.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2 vector = Vector2.Lerp(spine[spine.Length - 1].lastPos, spine[spine.Length - 1].pos, timeStacker);
		Vector2 vector2 = Custom.DirVec(Vector2.Lerp(spine[spine.Length - 2].lastPos, spine[spine.Length - 2].pos, timeStacker), vector);
		vector += vector2 * 5f * scale;
		Vector2 vector3 = vector + vector2 * 190f * scale + Custom.PerpendicularVector(vector2) * 40f * scale * num;
		Vector2 vector4 = Vector2.Lerp(spine[0].lastPos, spine[0].pos, timeStacker);
		vector4 += Custom.DirVec(Vector2.Lerp(spine[1].lastPos, spine[1].pos, timeStacker), vector4);
		Vector2 vector5 = vector4;
		for (int i = 0; i < spineBendPoint; i++)
		{
			float f = (float)i / (float)(spineBendPoint - 1);
			Vector2 vector6 = Vector2.Lerp(spine[i].lastPos, spine[i].pos, timeStacker);
			float num6 = Mathf.Lerp(10f, Custom.LerpMap(num, -1f, 1f, 70f, 30f, 2f), Mathf.Sin((float)Math.PI * Mathf.Pow(f, 1.5f))) * scale;
			float num7 = Mathf.Lerp(10f, Custom.LerpMap(num, 1f, -1f, 70f, 30f, 2f), Mathf.Sin((float)Math.PI * Mathf.Pow(f, 1.5f))) * scale;
			Vector2 normalized = (vector4 - vector6).normalized;
			Vector2 vector7 = Custom.PerpendicularVector(normalized);
			float num8 = Vector2.Distance(vector4, vector6) / 5f;
			(sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(i * 4, vector4 - normalized * num8 - vector7 * (num2 + num6) * 0.5f - camPos);
			(sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector4 - normalized * num8 + vector7 * (num3 + num7) * 0.5f - camPos);
			(sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector6 + normalized * num8 - vector7 * num6 - camPos);
			(sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector6 + normalized * num8 + vector7 * num7 - camPos);
			if (i == spineBendPoint - 2)
			{
				vector5 = vector6;
			}
			vector4 = vector6;
			num2 = num6;
			num3 = num7;
		}
		Vector2 vector8 = Custom.DegToVec(180f - 90f * num);
		vector8.x = Mathf.Pow(Mathf.Abs(vector8.x), 8f) * Mathf.Sign(vector8.x);
		vector8 *= 40f * scale;
		vector8.y -= 7f * scale;
		Vector2 vector9 = (pos + new Vector2(0f, -170f) + vector + vector8 + Vector2.Lerp(spine[5].lastPos, spine[5].pos, timeStacker)) / 3f;
		sLeaser.sprites[DistortionSprite].x = vector9.x - camPos.x;
		sLeaser.sprites[DistortionSprite].y = vector9.y - camPos.y;
		sLeaser.sprites[DistortionSprite].scale = 933f * scale / 16f;
		sLeaser.sprites[LightSprite].x = vector9.x - camPos.x;
		sLeaser.sprites[LightSprite].y = vector9.y - camPos.y;
		sLeaser.sprites[LightSprite].scale = 500f * lightSpriteScale / 16f;
		vector4 = Vector2.Lerp(spine[spineBendPoint].lastPos, spine[spineBendPoint].pos, timeStacker);
		vector4 += Custom.DirVec(Vector2.Lerp(spine[spineBendPoint + 1].lastPos, spine[spineBendPoint + 1].pos, timeStacker), vector4);
		vector4 += vector8;
		for (int j = spineBendPoint; j < spineSegments + snoutSegments; j++)
		{
			float num9 = Mathf.InverseLerp(spineBendPoint, spineSegments + snoutSegments - 1, j);
			Vector2 vector10 = ((j >= spineSegments) ? Custom.Bezier(vector, vector + vector2 * 60f * scale, vector3, vector + vector2 * 150f * scale, Mathf.InverseLerp(spineSegments, spineSegments + snoutSegments - 1, j)) : Vector2.Lerp(spine[j].lastPos, spine[j].pos, timeStacker));
			vector10 += vector8;
			if (j == spineBendPoint)
			{
				sLeaser.sprites[NeckConnectorSprite].x = (vector10.x + vector5.x) / 2f - camPos.x;
				sLeaser.sprites[NeckConnectorSprite].y = (vector10.y + vector5.y) / 2f - camPos.y;
				sLeaser.sprites[NeckConnectorSprite].rotation = Custom.AimFromOneVectorToAnother(vector5, vector10);
				sLeaser.sprites[NeckConnectorSprite].scaleY = Vector2.Distance(vector5, vector10) * 1.6f / 20f;
				sLeaser.sprites[NeckConnectorSprite].scaleX = scale * 1.6f;
			}
			float num10;
			float num11;
			if (num9 < 0.15f)
			{
				num10 = 10f * scale;
				num11 = 10f * scale;
			}
			else if (num9 < 0.4f)
			{
				num10 = Mathf.Lerp(10f, 20f, Mathf.Sin(Custom.LerpMap(num9, 0.15f, 0.4f, 0f, 0.5f) * (float)Math.PI)) * scale;
				num11 = Mathf.Lerp(10f, 20f, Mathf.Sin(Custom.LerpMap(num9, 0.15f, 0.4f, 0f, 0.5f) * (float)Math.PI)) * scale;
			}
			else
			{
				num10 = SnoutContour(Mathf.InverseLerp(0.4f, 1f, num9), side: false, Mathf.Abs(num));
				num11 = SnoutContour(Mathf.InverseLerp(0.4f, 1f, num9), side: false, Mathf.Abs(num));
			}
			Vector2 normalized2 = (vector4 - vector10).normalized;
			Vector2 vector11 = Custom.PerpendicularVector(normalized2);
			float num12 = Vector2.Distance(vector4, vector10) / 5f;
			int num13 = j - spineBendPoint;
			(sLeaser.sprites[HeadMeshSprite] as TriangleMesh).MoveVertice(num13 * 4, vector4 - normalized2 * num12 - vector11 * (num2 + num10) * 0.5f - camPos);
			(sLeaser.sprites[HeadMeshSprite] as TriangleMesh).MoveVertice(num13 * 4 + 1, vector4 - normalized2 * num12 + vector11 * (num3 + num11) * 0.5f - camPos);
			(sLeaser.sprites[HeadMeshSprite] as TriangleMesh).MoveVertice(num13 * 4 + 2, vector10 + normalized2 * num12 - vector11 * num10 - camPos);
			(sLeaser.sprites[HeadMeshSprite] as TriangleMesh).MoveVertice(num13 * 4 + 3, vector10 + normalized2 * num12 + vector11 * num11 - camPos);
			vector4 = vector10;
			num2 = num10;
			num3 = num11;
		}
		float a = Custom.AimFromOneVectorToAnother(vector3, vector) / 360f;
		for (int k = 0; k < (sLeaser.sprites[HeadMeshSprite] as TriangleMesh).verticeColors.Length; k++)
		{
			float num14 = (float)k / (float)((sLeaser.sprites[HeadMeshSprite] as TriangleMesh).verticeColors.Length - 1);
			float num15;
			float num16;
			if (num14 < 0.15f)
			{
				num15 = 10f * scale;
				num16 = 10f * scale;
			}
			else if (num14 < 0.4f)
			{
				num15 = Mathf.Lerp(10f, 20f, Mathf.Sin(Custom.LerpMap(num14, 0.15f, 0.4f, 0f, 0.5f) * (float)Math.PI)) * scale;
				num16 = Mathf.Lerp(10f, 20f, Mathf.Sin(Custom.LerpMap(num14, 0.15f, 0.4f, 0f, 0.5f) * (float)Math.PI)) * scale;
			}
			else
			{
				num15 = SnoutContour(Mathf.InverseLerp(0.4f, 1f, num14), side: false, Mathf.Abs(num));
				num16 = SnoutContour(Mathf.InverseLerp(0.4f, 1f, num14), side: false, Mathf.Abs(num));
			}
			float value = (num15 + num16) / (2f * scale);
			(sLeaser.sprites[HeadMeshSprite] as TriangleMesh).verticeColors[k] = new Color(Mathf.InverseLerp(0.1f, 30f, value), Mathf.InverseLerp(-1f, 1f, num), Mathf.InverseLerp(0.25f, 0.05f, num14), a);
		}
		Vector2 vector12 = Vector2.Lerp(Vector2.Lerp(spine[0].lastPos, spine[0].pos, timeStacker), Vector2.Lerp(spine[1].lastPos, spine[1].pos, timeStacker), 0.5f);
		vector12 += Custom.DirVec(Vector2.Lerp(spine[2].lastPos, spine[2].pos, timeStacker), vector12) * 20f * scale;
		for (int l = 0; l < legs.GetLength(0); l++)
		{
			Vector2 vector13 = Vector2.Lerp(legs[l, 0].lastPos, legs[l, 0].pos, timeStacker);
			Vector2 vector14 = Vector2.Lerp(legs[l, 1].lastPos, legs[l, 1].pos, timeStacker);
			Vector2 vector15 = Vector2.Lerp(legs[l, 2].lastPos, legs[l, 2].pos, timeStacker);
			Vector2 vector16 = vector12 + Custom.DirVec(vector12, vector) * 5f * scale + Custom.DirVec(vector12, vector13) * 10f * scale;
			vector4 = vector16 + Custom.DirVec(vector13, vector16);
			sLeaser.sprites[ButtockSprite(l)].x = (vector12 + vector16).x / 2f - camPos.x;
			sLeaser.sprites[ButtockSprite(l)].y = (vector12 + vector16).y / 2f - camPos.y;
			sLeaser.sprites[ButtockSprite(l)].scaleX = scale;
			sLeaser.sprites[ButtockSprite(l)].rotation = Custom.AimFromOneVectorToAnother(vector16, Vector2.Lerp(spine[1].lastPos, spine[1].pos, timeStacker));
			sLeaser.sprites[ButtockSprite(l)].scaleY = Mathf.Max(scale / 2f, Vector2.Distance(vector16, Vector2.Lerp(spine[1].lastPos, spine[1].pos, timeStacker)) / 40f);
			for (int m = 0; m < thighSegments; m++)
			{
				float num17 = Mathf.InverseLerp(0f, thighSegments - 1, m);
				Vector2 vector17 = Vector2.Lerp(vector16, vector13 + Custom.DirVec(vector16, vector13) * 10f * scale, num17);
				float num18 = ThighContour(num17, l == 0);
				float num19 = ThighContour(num17, l == 1);
				Vector2 normalized3 = (vector4 - vector17).normalized;
				Vector2 vector18 = Custom.PerpendicularVector(normalized3);
				float num20 = Vector2.Distance(vector4, vector17) / 5f;
				(sLeaser.sprites[ThightSprite(l)] as TriangleMesh).MoveVertice(m * 4, vector4 - normalized3 * num20 - vector18 * (num2 + num18) * 0.5f - camPos);
				(sLeaser.sprites[ThightSprite(l)] as TriangleMesh).MoveVertice(m * 4 + 1, vector4 - normalized3 * num20 + vector18 * (num3 + num19) * 0.5f - camPos);
				(sLeaser.sprites[ThightSprite(l)] as TriangleMesh).MoveVertice(m * 4 + 2, vector17 + normalized3 * num20 - vector18 * num18 - camPos);
				(sLeaser.sprites[ThightSprite(l)] as TriangleMesh).MoveVertice(m * 4 + 3, vector17 + normalized3 * num20 + vector18 * num19 - camPos);
				vector4 = vector17;
				num2 = num18;
				num3 = num19;
			}
			float a2 = Custom.AimFromOneVectorToAnother(vector16, vector13) / 360f;
			for (int n = 0; n < (sLeaser.sprites[ThightSprite(l)] as TriangleMesh).verticeColors.Length; n++)
			{
				float num21 = (float)n / (float)((sLeaser.sprites[ThightSprite(l)] as TriangleMesh).verticeColors.Length - 1);
				(sLeaser.sprites[ThightSprite(l)] as TriangleMesh).verticeColors[n] = new Color(1f, Custom.LerpMap(num, -1f, 1f, 0.4f, 0.6f), ((double)num21 < 0.3 || num21 > 0.7f) ? 1f : 0f, a2);
			}
			vector4 = vector13 + Custom.DirVec(vector14, vector13);
			for (int num22 = 0; num22 < lowerLegSegments; num22++)
			{
				float num23 = Mathf.InverseLerp(0f, lowerLegSegments - 1, num22);
				Vector2 vector19 = ((!(num23 < 0.8f)) ? Vector2.Lerp(vector14, vector15, Mathf.InverseLerp(0.8f, 1f, num23)) : Vector2.Lerp(vector13, vector14, Mathf.InverseLerp(0f, 0.8f, num23)));
				float num24 = LowerLegContour(num23, l == 0, Mathf.Lerp(0.7f, num * ((l == 1) ? (-1f) : 1f), Mathf.Abs(num)));
				float num25 = LowerLegContour(num23, l == 1, Mathf.Lerp(0.7f, num * ((l == 1) ? (-1f) : 1f), Mathf.Abs(num)));
				Vector2 normalized4 = (vector4 - vector19).normalized;
				Vector2 vector20 = Custom.PerpendicularVector(normalized4);
				float num26 = Vector2.Distance(vector4, vector19) / 5f;
				(sLeaser.sprites[LowerLegSprite(l)] as TriangleMesh).MoveVertice(num22 * 4, vector4 - normalized4 * num26 - vector20 * (num2 + num24) * 0.5f - camPos);
				(sLeaser.sprites[LowerLegSprite(l)] as TriangleMesh).MoveVertice(num22 * 4 + 1, vector4 - normalized4 * num26 + vector20 * (num3 + num25) * 0.5f - camPos);
				(sLeaser.sprites[LowerLegSprite(l)] as TriangleMesh).MoveVertice(num22 * 4 + 2, vector19 + normalized4 * num26 - vector20 * num24 - camPos);
				(sLeaser.sprites[LowerLegSprite(l)] as TriangleMesh).MoveVertice(num22 * 4 + 3, vector19 + normalized4 * num26 + vector20 * num25 - camPos);
				vector4 = vector19;
				num2 = num24;
				num3 = num25;
			}
			a2 = Custom.AimFromOneVectorToAnother(vector13, vector15) / 360f;
			for (int num27 = 0; num27 < (sLeaser.sprites[LowerLegSprite(l)] as TriangleMesh).verticeColors.Length; num27++)
			{
				float value2 = (float)num27 / (float)((sLeaser.sprites[LowerLegSprite(l)] as TriangleMesh).verticeColors.Length - 1);
				(sLeaser.sprites[LowerLegSprite(l)] as TriangleMesh).verticeColors[num27] = new Color(1f, Custom.LerpMap(num, -1f, 1f, 0.4f, 0.6f), Mathf.InverseLerp(0.25f, 0.05f, value2), a2);
			}
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public float SnoutContour(float f, bool side, float sideView)
	{
		float num = ((!(f > 0.85f)) ? Custom.LerpMap(f, 0f, 0.5f, Mathf.Lerp(Custom.LerpMap(f, 0f, 0.5f, 1.5f, 2f), 1f, sideView), 1f) : (0.2f + 0.8f * Mathf.Sin(Custom.LerpMap(f, 0.85f, 1f, 0.5f, 1f) * (float)Math.PI)));
		num *= Mathf.Lerp(1f, 0.3f, sideView * f);
		return num * 10f * scale;
	}

	public float ThighContour(float f, bool side)
	{
		float num = 0f;
		if (f < 0.3f)
		{
			num = 0.2f + 0.6f * Mathf.Sin(Custom.LerpMap(f, 0f, 0.3f, 0f, 0.5f) * (float)Math.PI);
		}
		else if (side)
		{
			num = ((!(f < 0.85f)) ? (0.2f + 0.8f * Custom.BackwardsSCurve(1f - Mathf.InverseLerp(0.85f, 1f, f), 0.3f)) : Custom.LerpMap(f, 0.3f, 0.85f, 0.8f, 1f, 0.5f));
		}
		else if (f < 0.65f)
		{
			num = Custom.LerpMap(f, 0.3f, 0.65f, 0.8f, 1f, 0.5f);
		}
		else
		{
			num = Custom.LerpMap(f, 0.65f, 1f, 1f, 0.2f);
			num = Mathf.Max(num, 0.1f + 0.6f * Mathf.Sin(Custom.LerpMap(f, 0.85f, 1f, 0.5f, 1f) * (float)Math.PI));
		}
		return num * 15f * scale;
	}

	public float LowerLegContour(float f, bool side, float flip)
	{
		float num = 0f;
		num = ((f < 0.1f) ? (0.5f + 0.5f * Custom.BackwardsSCurve(Mathf.InverseLerp(0f, 0.1f, f), 0.3f)) : ((!(num < 0.8f)) ? 0.6f : Custom.LerpMap(f, 0.1f, 0.8f, 1f, 0.6f, 0.3f)));
		num = ((!side) ? Mathf.Max(num, Mathf.Sin(Mathf.Pow(Mathf.InverseLerp(0.2f, 0.5f, f), 0.6f) * (float)Math.PI)) : Mathf.Max(num, 0.5f + Mathf.Sin(Mathf.Pow(Mathf.InverseLerp(0f, 0.3f, f), 0.5f) * (float)Math.PI)));
		num += Mathf.Sin(Mathf.Pow(f, 0.5f) * (float)Math.PI) * (side ? (-1f) : 1f) * flip;
		if (f > 0.85f)
		{
			if (side)
			{
				num += Mathf.Sin(Mathf.InverseLerp(0.85f, 1f, f) * (float)Math.PI) * 0.7f;
			}
			num *= 0.3f + 0.7f * Mathf.InverseLerp(1f, 0.94f, f);
		}
		return num * 10f * scale;
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
		sLeaser.sprites[NeckConnectorSprite].color = blackColor;
		sLeaser.sprites[ButtockSprite(0)].color = blackColor;
		sLeaser.sprites[ButtockSprite(1)].color = blackColor;
		for (int i = 0; i < (sLeaser.sprites[BodyMeshSprite] as TriangleMesh).verticeColors.Length; i++)
		{
			(sLeaser.sprites[BodyMeshSprite] as TriangleMesh).verticeColors[i] = blackColor;
		}
		for (int j = 0; j < legs.GetLength(0); j++)
		{
			for (int k = 0; k < (sLeaser.sprites[ThightSprite(j)] as TriangleMesh).verticeColors.Length; k++)
			{
				(sLeaser.sprites[ThightSprite(j)] as TriangleMesh).verticeColors[k] = blackColor;
			}
		}
		base.ApplyPalette(sLeaser, rCam, palette);
	}

	public string ReplaceParts(string s)
	{
		return s;
	}

	public void SpecialEvent(string eventName)
	{
	}
}
