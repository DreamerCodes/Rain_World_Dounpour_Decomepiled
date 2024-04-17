using System;
using System.Collections.Generic;
using System.Globalization;
using JollyCoop;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class PlayerGraphics : GraphicsModule
{
	private class PlayerObjectLooker
	{
		public PlayerGraphics owner;

		public PhysicalObject currentMostInteresting;

		public int timeLookingAtThis;

		private Vector2? lookAtPoint;

		private float lookAtPointInterest;

		public bool looking
		{
			get
			{
				if (currentMostInteresting != null)
				{
					return true;
				}
				if (lookAtPoint.HasValue)
				{
					return true;
				}
				return false;
			}
		}

		public Vector2 mostInterestingLookPoint
		{
			get
			{
				if (lookAtPoint.HasValue)
				{
					return lookAtPoint.Value;
				}
				if (currentMostInteresting == null)
				{
					return owner.head.pos + owner.lookDirection * 100f;
				}
				if (currentMostInteresting is Creature)
				{
					return (currentMostInteresting as Creature).DangerPos;
				}
				return currentMostInteresting.firstChunk.pos;
			}
		}

		public PlayerObjectLooker(PlayerGraphics owner)
		{
			this.owner = owner;
		}

		public void Update()
		{
			timeLookingAtThis++;
			if (currentMostInteresting != null && currentMostInteresting.slatedForDeletetion)
			{
				currentMostInteresting = null;
			}
			float num = HowInterestingIsThisObject(currentMostInteresting) + ((timeLookingAtThis < 8) ? 0.5f : (-0.5f));
			if (lookAtPoint.HasValue)
			{
				num = lookAtPointInterest + ((timeLookingAtThis < 8) ? 0.5f : (-0.5f));
			}
			foreach (UpdatableAndDeletable update in owner.player.room.updateList)
			{
				if (update != owner.player && update is PhysicalObject && HowInterestingIsThisObject(update as PhysicalObject) > num)
				{
					timeLookingAtThis = 0;
					currentMostInteresting = update as PhysicalObject;
					num = HowInterestingIsThisObject(update as PhysicalObject);
				}
			}
			if (num < 0.2f && UnityEngine.Random.value < 0.5f)
			{
				LookAtNothing();
			}
		}

		public void LookAtNothing()
		{
			currentMostInteresting = null;
			timeLookingAtThis = 0;
			lookAtPoint = null;
		}

		public void LookAtObject(PhysicalObject obj)
		{
			currentMostInteresting = obj;
			timeLookingAtThis = 0;
		}

		public void LookAtPoint(Vector2 point, float interest)
		{
			lookAtPointInterest = interest;
			lookAtPoint = point;
			timeLookingAtThis = 0;
		}

		private float HowInterestingIsThisObject(PhysicalObject obj)
		{
			if (obj == null || obj.room != owner.player.room)
			{
				return 0f;
			}
			if (obj is Weapon && (obj as Weapon).mode == Weapon.Mode.OnBack)
			{
				return 0f;
			}
			if (owner.player.slugOnBack != null && owner.player.slugOnBack.slugcat != null && owner.player.slugOnBack.slugcat == obj)
			{
				return 0f;
			}
			for (int i = 0; i < 2; i++)
			{
				if (owner.player.grasps[i] != null && owner.player.grasps[i].grabbed == obj)
				{
					return 0f;
				}
			}
			float num = 1f;
			if (obj is Creature)
			{
				CreatureTemplate.Relationship relationship = owner.player.abstractCreature.creatureTemplate.CreatureRelationship((obj as Creature).abstractCreature.creatureTemplate);
				num += relationship.intensity;
				if (relationship.type == CreatureTemplate.Relationship.Type.Afraid && (obj as Creature).Consious)
				{
					num += 3f + relationship.intensity * 3f;
				}
			}
			else if (obj is Oracle)
			{
				num += 1000f;
			}
			if (Custom.DistLess(owner.player.mainBodyChunk.pos, obj.bodyChunks[0].pos, 400f) && owner.player.room.VisualContact(owner.player.mainBodyChunk.pos, obj.bodyChunks[0].pos))
			{
				num *= Mathf.Lerp(obj.bodyChunks[0].vel.magnitude + 1f, 2f, 0.5f);
			}
			return num / Mathf.Lerp(Mathf.Pow(Vector2.Distance(owner.player.mainBodyChunk.pos, obj.bodyChunks[0].pos), 1.5f), 1f, 0.995f);
		}
	}

	public class AxolotlScale : BodyPart
	{
		public float length;

		public float width;

		public AxolotlScale(GraphicsModule cosmetics)
			: base(cosmetics)
		{
		}

		public override void Update()
		{
			base.Update();
			if (owner.owner.room.PointSubmerged(pos))
			{
				vel *= 0.5f;
			}
			else
			{
				vel *= 0.9f;
			}
			lastPos = pos;
			pos += vel;
		}
	}

	public class AxolotlGills
	{
		public class SpritesOverlap : ExtEnum<SpritesOverlap>
		{
			public static readonly SpritesOverlap Behind = new SpritesOverlap("Behind", register: true);

			public static readonly SpritesOverlap BehindHead = new SpritesOverlap("BehindHead", register: true);

			public static readonly SpritesOverlap InFront = new SpritesOverlap("InFront", register: true);

			public SpritesOverlap(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public AxolotlScale[] scaleObjects;

		public float[] backwardsFactors;

		public int graphic;

		public float graphicHeight;

		public float rigor;

		public float scaleX;

		public bool colored;

		public Vector2[] scalesPositions;

		public PlayerGraphics pGraphics;

		public int numberOfSprites;

		public int startSprite;

		public RoomPalette palette;

		public SpritesOverlap spritesOverlap;

		public Color baseColor;

		public Color effectColor;

		public AxolotlGills(PlayerGraphics pGraphics, int startSprite)
		{
			this.pGraphics = pGraphics;
			this.startSprite = startSprite;
			rigor = 0.5873646f;
			float num = 1.310689f;
			colored = true;
			graphic = 3;
			graphicHeight = Futile.atlasManager.GetElementWithName("LizardScaleA" + graphic).sourcePixelSize.y;
			int num2 = 3;
			scalesPositions = new Vector2[num2 * 2];
			scaleObjects = new AxolotlScale[scalesPositions.Length];
			backwardsFactors = new float[scalesPositions.Length];
			float num3 = 0.1542603f;
			float num4 = 0.1759363f;
			for (int i = 0; i < num2; i++)
			{
				float y = 0.03570603f;
				float num5 = 0.659981f;
				float num6 = 0.9722961f;
				float num7 = 0.3644831f;
				if (i == 1)
				{
					y = 0.02899241f;
					num5 = 0.76459f;
					num6 = 0.6056554f;
					num7 = 0.9129724f;
				}
				if (i == 2)
				{
					y = 0.02639332f;
					num5 = 0.7482835f;
					num6 = 0.7223744f;
					num7 = 0.4567381f;
				}
				for (int j = 0; j < 2; j++)
				{
					scalesPositions[i * 2 + j] = new Vector2((j != 0) ? num5 : (0f - num5), y);
					scaleObjects[i * 2 + j] = new AxolotlScale(pGraphics);
					scaleObjects[i * 2 + j].length = Mathf.Lerp(2.5f, 15f, num * num6);
					scaleObjects[i * 2 + j].width = Mathf.Lerp(0.65f, 1.2f, num3 * num);
					backwardsFactors[i * 2 + j] = num4 * num7;
				}
			}
			numberOfSprites = ((!colored) ? scalesPositions.Length : (scalesPositions.Length * 2));
			spritesOverlap = SpritesOverlap.InFront;
		}

		public void Update()
		{
			for (int i = 0; i < scaleObjects.Length; i++)
			{
				Vector2 pos = pGraphics.owner.bodyChunks[0].pos;
				Vector2 pos2 = pGraphics.owner.bodyChunks[1].pos;
				float num = 0f;
				float num2 = 90f;
				int num3 = i % (scaleObjects.Length / 2);
				float num4 = num2 / (float)(scaleObjects.Length / 2);
				if (i >= scaleObjects.Length / 2)
				{
					num = 0f;
					pos.x += 5f;
				}
				else
				{
					pos.x -= 5f;
				}
				Vector2 vector = Custom.rotateVectorDeg(Custom.DegToVec(0f), (float)num3 * num4 - num2 / 2f + num + 90f);
				float f = Custom.VecToDeg(pGraphics.lookDirection);
				Vector2 vector2 = Custom.rotateVectorDeg(Custom.DegToVec(0f), (float)num3 * num4 - num2 / 2f + num);
				Vector2 a = Vector2.Lerp(vector2, Custom.DirVec(pos2, pos), Mathf.Abs(f));
				if (scalesPositions[i].y < 0.2f)
				{
					a -= vector * Mathf.Pow(Mathf.InverseLerp(0.2f, 0f, scalesPositions[i].y), 2f) * 2f;
				}
				a = Vector2.Lerp(a, vector2, Mathf.Pow(backwardsFactors[i], 1f)).normalized;
				Vector2 vector3 = pos + a * scaleObjects[i].length;
				if (!Custom.DistLess(scaleObjects[i].pos, vector3, scaleObjects[i].length / 2f))
				{
					Vector2 vector4 = Custom.DirVec(scaleObjects[i].pos, vector3);
					float num5 = Vector2.Distance(scaleObjects[i].pos, vector3);
					float num6 = scaleObjects[i].length / 2f;
					scaleObjects[i].pos += vector4 * (num5 - num6);
					scaleObjects[i].vel += vector4 * (num5 - num6);
				}
				scaleObjects[i].vel += Vector2.ClampMagnitude(vector3 - scaleObjects[i].pos, 10f) / Mathf.Lerp(5f, 1.5f, rigor);
				scaleObjects[i].vel *= Mathf.Lerp(1f, 0.8f, rigor);
				scaleObjects[i].ConnectToPoint(pos, scaleObjects[i].length, push: true, 0f, new Vector2(0f, 0f), 0f, 0f);
				scaleObjects[i].Update();
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			for (int num = startSprite + scalesPositions.Length - 1; num >= startSprite; num--)
			{
				sLeaser.sprites[num] = new FSprite("LizardScaleA" + graphic);
				sLeaser.sprites[num].scaleY = scaleObjects[num - startSprite].length / graphicHeight;
				sLeaser.sprites[num].anchorY = 0.1f;
				if (colored)
				{
					sLeaser.sprites[num + scalesPositions.Length] = new FSprite("LizardScaleB" + graphic);
					sLeaser.sprites[num + scalesPositions.Length].scaleY = scaleObjects[num - startSprite].length / graphicHeight;
					sLeaser.sprites[num + scalesPositions.Length].anchorY = 0.1f;
				}
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			if (pGraphics.owner == null)
			{
				return;
			}
			for (int num = startSprite + scalesPositions.Length - 1; num >= startSprite; num--)
			{
				Vector2 p = new Vector2(sLeaser.sprites[9].x + camPos.x, sLeaser.sprites[9].y + camPos.y);
				float f = 0f;
				float num2 = 0f;
				if (num < startSprite + scalesPositions.Length / 2)
				{
					p.x -= 5f;
				}
				else
				{
					num2 = 180f;
					p.x += 5f;
				}
				sLeaser.sprites[num].x = p.x - camPos.x;
				sLeaser.sprites[num].y = p.y - camPos.y;
				sLeaser.sprites[num].rotation = Custom.AimFromOneVectorToAnother(p, Vector2.Lerp(scaleObjects[num - startSprite].lastPos, scaleObjects[num - startSprite].pos, timeStacker)) + num2;
				sLeaser.sprites[num].scaleX = scaleObjects[num - startSprite].width * Mathf.Sign(f);
				if (colored)
				{
					sLeaser.sprites[num + scalesPositions.Length].x = p.x - camPos.x;
					sLeaser.sprites[num + scalesPositions.Length].y = p.y - camPos.y;
					sLeaser.sprites[num + scalesPositions.Length].rotation = Custom.AimFromOneVectorToAnother(p, Vector2.Lerp(scaleObjects[num - startSprite].lastPos, scaleObjects[num - startSprite].pos, timeStacker)) + num2;
					sLeaser.sprites[num + scalesPositions.Length].scaleX = scaleObjects[num - startSprite].width * Mathf.Sign(f);
					if (num < startSprite + scalesPositions.Length / 2)
					{
						sLeaser.sprites[num + scalesPositions.Length].scaleX *= -1f;
					}
				}
				if (num < startSprite + scalesPositions.Length / 2)
				{
					sLeaser.sprites[num].scaleX *= -1f;
				}
			}
			for (int num3 = startSprite + scalesPositions.Length - 1; num3 >= startSprite; num3--)
			{
				sLeaser.sprites[num3].color = baseColor;
				if (colored)
				{
					sLeaser.sprites[num3 + scalesPositions.Length].color = Color.Lerp(effectColor, baseColor, pGraphics.malnourished / 1.75f);
				}
			}
		}

		public void SetGillColors(Color baseCol, Color effectCol)
		{
			baseColor = baseCol;
			if (pGraphics.useJollyColor)
			{
				effectColor = JollyColor(pGraphics.player.playerState.playerNumber, 2);
			}
			else if (CustomColorsEnabled())
			{
				effectColor = CustomColorSafety(2);
			}
			else
			{
				effectColor = effectCol;
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			this.palette = palette;
			for (int num = startSprite + scalesPositions.Length - 1; num >= startSprite; num--)
			{
				sLeaser.sprites[num].color = baseColor;
				if (colored)
				{
					sLeaser.sprites[num + scalesPositions.Length].color = effectColor;
				}
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			for (int i = startSprite; i < startSprite + numberOfSprites; i++)
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public class RopeSegment
	{
		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public int index;

		public bool claimedForBend;

		public PlayerGraphics pGraph;

		public RopeSegment(int index, PlayerGraphics pGraph)
		{
			this.index = index;
			this.pGraph = pGraph;
		}

		public void Update()
		{
			if (claimedForBend)
			{
				return;
			}
			lastPos = pos;
			pos += vel;
			vel *= 0.98f;
			int num = index;
			int num2 = index;
			while (num > 0)
			{
				num--;
				if (pGraph.ropeSegments[num].claimedForBend)
				{
					break;
				}
			}
			while (num2 < pGraph.ropeSegments.Length - 1)
			{
				num2++;
				if (pGraph.ropeSegments[num2].claimedForBend)
				{
					break;
				}
			}
			Vector2 vector = Vector2.Lerp(pGraph.ropeSegments[num].pos, pGraph.ropeSegments[num2].pos, Mathf.InverseLerp(num, num2, index));
			if (pGraph.player.tongue.mode == Player.Tongue.Mode.Retracted)
			{
				pos = vector;
				return;
			}
			vel += (vector - pos) * 0.2f;
			pos = Vector2.Lerp(pos, vector, 0.4f);
		}
	}

	public class Tentacle
	{
		public Vector2[,] segments;

		public float conRad;

		public Vector2? posB;

		public Vector2? rootDirB;

		public int startSprite;

		public PlayerGraphics pGraphics;

		public Vector2 wind;

		public float lengthFactor;

		public float length;

		public int activeUpdateTime;

		public Tentacle(PlayerGraphics pGraphics, int startSprite, float length, Vector2? posB)
		{
			this.startSprite = startSprite;
			this.pGraphics = pGraphics;
			this.posB = posB;
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState((int)length);
			if (posB.HasValue)
			{
				IntVector2 tilePosition = pGraphics.player.room.GetTilePosition(posB.Value);
				for (int i = 0; i < 4; i++)
				{
					if (pGraphics.player.room.GetTile(tilePosition + Custom.fourDirections[i]).Solid)
					{
						rootDirB = -Custom.fourDirections[i].ToVector2();
						posB = ((Custom.fourDirections[i].x != 0) ? new Vector2?(new Vector2(pGraphics.player.room.MiddleOfTile(tilePosition).x - rootDirB.Value.x * 20f, posB.Value.y)) : new Vector2?(new Vector2(posB.Value.x, pGraphics.player.room.MiddleOfTile(tilePosition).y - rootDirB.Value.y * 20f)));
					}
				}
			}
			segments = new Vector2[(int)Mathf.Clamp(length / 20f, 1f, 200f), 3];
			for (int j = 0; j < segments.GetLength(0); j++)
			{
				_ = (float)j / (float)(segments.GetLength(0) - 1);
				if (posB.HasValue)
				{
					segments[j, 0] = posB.Value;
				}
				segments[j, 1] = segments[j, 0];
				segments[j, 2] = Custom.RNV() * UnityEngine.Random.value;
			}
			this.length = length;
			conRad = length / (float)segments.GetLength(0) * 1.5f;
			UnityEngine.Random.state = state;
		}

		public void ActiveUpdate()
		{
			lengthFactor = Mathf.Lerp(lengthFactor, 1f, 0.01f);
			activeUpdateTime++;
			if (activeUpdateTime == 1)
			{
				for (int i = 1; i < segments.GetLength(0); i++)
				{
					_ = (float)i / (float)(segments.GetLength(0) - 1);
					segments[i, 0] = segments[0, 0];
					segments[i, 1] = segments[0, 1];
				}
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Midground");
			}
			sLeaser.sprites[startSprite].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[startSprite]);
			sLeaser.sprites[startSprite + 1].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[startSprite + 1]);
		}

		private void Connect(int A, int B)
		{
			Vector2 normalized = (segments[A, 0] - segments[B, 0]).normalized;
			float num = Vector2.Distance(segments[A, 0], segments[B, 0]);
			float num2 = Mathf.InverseLerp(0f, conRad, num);
			segments[A, 0] += normalized * (conRad - num) * 0.5f * num2;
			segments[A, 2] += normalized * (conRad - num) * 0.5f * num2;
			segments[B, 0] -= normalized * (conRad - num) * 0.5f * num2;
			segments[B, 2] -= normalized * (conRad - num) * 0.5f * num2;
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			(sLeaser.sprites[startSprite] as TriangleMesh).isVisible = true;
			(sLeaser.sprites[startSprite + 1] as TriangleMesh).isVisible = true;
			Vector2 vector = Vector2.Lerp(segments[0, 1], segments[0, 0], timeStacker);
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				Vector2 vector2 = Vector2.Lerp(segments[i, 1], segments[i, 0], timeStacker);
				Vector2 normalized = (vector - vector2).normalized;
				Vector2 vector3 = Custom.PerpendicularVector(normalized);
				float num = Vector2.Distance(vector, vector2) / 5f;
				float num2 = Rad((float)i / (float)(segments.GetLength(0) - 1));
				if (i != 0)
				{
					(sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4, vector - normalized * num - vector3 * 1.5f * num2 - camPos);
					(sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector - normalized * num + vector3 * 1.5f * num2 - camPos);
				}
				else
				{
					(sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4, vector - vector3 - camPos);
					(sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 - camPos);
				}
				if (i != segments.GetLength(0) - 1)
				{
					(sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 + normalized * num - vector3 * 3.5f * num2 - camPos);
					(sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + normalized * num + vector3 * 3.5f * num2 - camPos);
				}
				else
				{
					(sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 - camPos);
					(sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 - camPos);
				}
				for (int j = 0; j < 4; j++)
				{
					(sLeaser.sprites[startSprite + 1] as TriangleMesh).MoveVertice(i * 4 + j, (sLeaser.sprites[startSprite] as TriangleMesh).vertices[i * 4 + j]);
				}
				vector = vector2;
			}
		}

		public void InactiveDrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			(sLeaser.sprites[startSprite] as TriangleMesh).isVisible = false;
			(sLeaser.sprites[startSprite + 1] as TriangleMesh).isVisible = false;
		}

		public void InactiveUpdate()
		{
			if (lengthFactor <= 0.01f)
			{
				lengthFactor = 0f;
			}
			else
			{
				lengthFactor = Mathf.Lerp(lengthFactor, 0f, 0.01f);
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[startSprite] = TriangleMesh.MakeLongMesh(segments.GetLength(0), pointyTip: false, customColor: true);
			sLeaser.sprites[startSprite + 1] = TriangleMesh.MakeLongMesh(segments.GetLength(0), pointyTip: false, customColor: false);
			sLeaser.sprites[startSprite + 1].shader = rCam.room.game.rainWorld.Shaders["GhostSkin"];
			sLeaser.sprites[startSprite + 1].alpha = 1f / (float)segments.GetLength(0);
			for (int i = 0; i < (sLeaser.sprites[startSprite] as TriangleMesh).verticeColors.Length; i++)
			{
				float f = (float)i / (float)((sLeaser.sprites[startSprite] as TriangleMesh).verticeColors.Length - 1);
				(sLeaser.sprites[startSprite] as TriangleMesh).verticeColors[i] = MeshColor(f);
			}
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Items"));
		}

		public Color MeshColor(float f)
		{
			f = Mathf.Abs(f - 0.5f) * 2f;
			return Custom.HSL2RGB(Custom.Decimal(Mathf.Lerp(0.4f, 0.1f, 0.5f + 0.5f * Mathf.Pow(f, 3f))), Custom.Decimal(Mathf.Lerp(0.4f, 0.1f, 0.5f + 0.5f * Mathf.Pow(f, 3f))), Custom.LerpMap(f, 0.7f, 1f, 0.1f, 0.02f));
		}

		private float Rad(float f)
		{
			return Mathf.Lerp(0.2f, 1f, Mathf.Pow(Mathf.Clamp(Mathf.Sin(f * (float)Math.PI), 0f, 1f), 0.5f));
		}

		public void SetPosition(Vector2 pos)
		{
			posB = pos;
			segments[0, 0] = pos;
			segments[0, 1] = pos;
		}

		public void Update()
		{
			if (pGraphics.player.room == null)
			{
				return;
			}
			conRad = length * lengthFactor / (float)segments.GetLength(0) * 1.5f;
			wind += Custom.RNV() * 0.2f * UnityEngine.Random.value;
			wind = Vector2.ClampMagnitude(wind, 1f);
			for (int i = 2; i < segments.GetLength(0); i++)
			{
				Vector2 vector = Custom.DirVec(segments[i - 2, 0], segments[i, 0]);
				segments[i - 2, 2] -= vector * 0.15f;
				segments[i, 2] += vector * 0.15f;
			}
			for (int j = 0; j < segments.GetLength(0); j++)
			{
				float num = (float)j / (float)(segments.GetLength(0) - 1);
				segments[j, 1] = segments[j, 0];
				segments[j, 0] += segments[j, 2];
				segments[j, 2] *= 0.999f;
				if (pGraphics.player.room.aimap != null && pGraphics.player.room.aimap.getTerrainProximity(segments[j, 0]) < 4)
				{
					IntVector2 tilePosition = pGraphics.player.room.GetTilePosition(segments[j, 0]);
					Vector2 vector2 = new Vector2(0f, 0f);
					for (int k = 0; k < 4; k++)
					{
						if (!pGraphics.player.room.GetTile(tilePosition + Custom.fourDirections[k]).Solid && !pGraphics.player.room.aimap.getAItile(tilePosition + Custom.fourDirections[k]).narrowSpace)
						{
							float num2 = 0f;
							for (int l = 0; l < 4; l++)
							{
								num2 += (float)pGraphics.player.room.aimap.getTerrainProximity(tilePosition + Custom.fourDirections[k] + Custom.fourDirections[l]);
							}
							vector2 += Custom.fourDirections[k].ToVector2() * num2;
						}
					}
					segments[j, 2] += vector2.normalized * Custom.LerpMap(pGraphics.player.room.aimap.getTerrainProximity(segments[j, 0]), 0f, 3f, 2f, 0.2f);
				}
				segments[j, 2] += wind * 0.005f;
				if (num > 0.5f && posB.HasValue)
				{
					segments[j, 2] += Vector2.ClampMagnitude(posB.Value - segments[j, 0], 40f) / 420f * Mathf.InverseLerp(0.75f, 1f, num);
				}
			}
			for (int num3 = segments.GetLength(0) - 1; num3 > 0; num3--)
			{
				Connect(num3, num3 - 1);
			}
			for (int m = 1; m < segments.GetLength(0); m++)
			{
				Connect(m, m - 1);
			}
		}
	}

	public class CosmeticPearl
	{
		public PlayerGraphics pGraphics;

		public int numberOfSprites;

		public int startSprite;

		public bool visible;

		public float lastGlimmer;

		public float glimmer;

		public float glimmerProg;

		public float glimmerSpeed;

		public int glimmerWait;

		public Color? highlightColor;

		public Color color;

		public float darkness;

		public float globalAlpha;

		public bool scarVisible;

		public CosmeticPearl(PlayerGraphics pGraphics, int startSprite)
		{
			this.pGraphics = pGraphics;
			this.startSprite = startSprite;
			numberOfSprites = 3;
			glimmerProg = 1f;
			visible = false;
			scarVisible = false;
			globalAlpha = 0f;
		}

		public void Update()
		{
			lastGlimmer = glimmer;
			glimmer = Mathf.Sin(glimmerProg * (float)Math.PI) * UnityEngine.Random.value;
			if (glimmerProg < 1f)
			{
				glimmerProg = Mathf.Min(1f, glimmerProg + glimmerSpeed);
				return;
			}
			if (glimmerWait > 0)
			{
				glimmerWait--;
				return;
			}
			glimmerWait = UnityEngine.Random.Range(20, 40);
			glimmerProg = 0f;
			glimmerSpeed = 1f / Mathf.Lerp(5f, 15f, UnityEngine.Random.value);
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[startSprite] = new FSprite("JetFishEyeA");
			sLeaser.sprites[startSprite].anchorX = 0.5f;
			sLeaser.sprites[startSprite].anchorY = 0.5f;
			sLeaser.sprites[startSprite + 1] = new FSprite("Futile_White");
			sLeaser.sprites[startSprite + 1].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
			sLeaser.sprites[startSprite + 2] = new FSprite("BodyPearl");
			sLeaser.sprites[startSprite + 2].anchorX = 0.5f;
			sLeaser.sprites[startSprite + 2].anchorY = 0.5f;
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Midground");
			}
			for (int i = 0; i < numberOfSprites; i++)
			{
				newContatiner.AddChild(sLeaser.sprites[startSprite + i]);
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			color = new Color(0.25f, 0.04f, 0.1f);
			highlightColor = new Color(1f, 0.4f, 1f);
			darkness = 0f;
			sLeaser.sprites[startSprite + 2].color = Color.white;
			sLeaser.sprites[startSprite + 2].alpha = 0.25f;
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp((pGraphics.owner.bodyChunks[0].lastPos + pGraphics.owner.bodyChunks[1].lastPos) / 2f, (pGraphics.owner.bodyChunks[0].pos + pGraphics.owner.bodyChunks[1].pos) / 2f, timeStacker);
			float num = Mathf.Lerp(lastGlimmer, glimmer, timeStacker);
			float rotation = Custom.AimFromOneVectorToAnother(pGraphics.owner.bodyChunks[1].pos, pGraphics.owner.bodyChunks[0].pos);
			if (!visible && sLeaser.sprites[startSprite].isVisible)
			{
				sLeaser.sprites[startSprite].isVisible = false;
				sLeaser.sprites[startSprite + 1].isVisible = false;
			}
			if (visible && !sLeaser.sprites[startSprite].isVisible)
			{
				sLeaser.sprites[startSprite].isVisible = true;
				sLeaser.sprites[startSprite + 1].isVisible = true;
			}
			sLeaser.sprites[startSprite].x = vector.x - camPos.x;
			sLeaser.sprites[startSprite].y = vector.y - camPos.y;
			Vector2 normalized = pGraphics.player.mainBodyChunk.vel.normalized;
			sLeaser.sprites[startSprite].x += pGraphics.lookDirection.x + normalized.x;
			sLeaser.sprites[startSprite].y += pGraphics.lookDirection.y + normalized.y;
			sLeaser.sprites[startSprite].rotation = rotation;
			sLeaser.sprites[startSprite].scaleX = 1f;
			if (pGraphics.player.bodyMode == Player.BodyModeIndex.Crawl)
			{
				sLeaser.sprites[startSprite].scaleX = 0.5f;
			}
			else if (pGraphics.player.mainBodyChunk.vel.x != 0f)
			{
				sLeaser.sprites[startSprite].scaleX = Custom.LerpMap(Mathf.Abs(pGraphics.player.mainBodyChunk.vel.x), 0f, 4f, 1f, 0.75f);
			}
			sLeaser.sprites[startSprite].scaleX *= 1f - Mathf.Abs(pGraphics.lookDirection.x) * 0.35f;
			sLeaser.sprites[startSprite + 1].x = vector.x - camPos.x;
			sLeaser.sprites[startSprite + 1].y = vector.y - camPos.y;
			sLeaser.sprites[startSprite].color = Color.Lerp(Custom.RGB2RGBA(this.color * Mathf.Lerp(1f, 0.2f, darkness), 1f), new Color(1f, 1f, 1f), num);
			if (highlightColor.HasValue)
			{
				Color color = Color.Lerp(highlightColor.Value, new Color(1f, 1f, 1f), num);
				sLeaser.sprites[startSprite + 1].color = color;
			}
			if (num > 0.9f && pGraphics.player.firstChunk.submersion == 1f)
			{
				sLeaser.sprites[startSprite].color = new Color(0f, 0.003921569f, 0f);
			}
			sLeaser.sprites[startSprite + 1].alpha = num * 0.5f * globalAlpha;
			sLeaser.sprites[startSprite + 1].scale = 20f * num * 1f / 16f;
			sLeaser.sprites[startSprite].alpha = globalAlpha;
			if (pGraphics.player.room != null && pGraphics.player.room.game.IsStorySession && pGraphics.player.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad > 0 && (!ModManager.CoopAvailable || !(pGraphics.player.room.game.StoryCharacter != MoreSlugcatsEnums.SlugcatStatsName.Spear)))
			{
				float num2 = pGraphics.player.room.game.GetStorySession.saveState.miscWorldSaveData.cyclesSinceSSai;
				sLeaser.sprites[startSprite + 2].color = Color.white;
				if (num2 < 10f)
				{
					if (num2 > 1f)
					{
						sLeaser.sprites[startSprite + 2].alpha = (1f - num2 / 10f) * 0.2f;
					}
					else
					{
						sLeaser.sprites[startSprite + 2].alpha = 0.25f;
					}
					if (num2 < 2f && UnityEngine.Random.value < 0.0075f / (num2 + 1f) && pGraphics.player.room != null)
					{
						WaterDrip waterDrip = new WaterDrip(new Vector2(sLeaser.sprites[startSprite].x + camPos.x, sLeaser.sprites[startSprite].y + camPos.y), Custom.RNV() * 3f * UnityEngine.Random.value, waterColor: false);
						pGraphics.player.room.AddObject(waterDrip);
						if (num2 == 0f)
						{
							waterDrip.WhiteDrip();
						}
					}
					scarVisible = true;
				}
			}
			sLeaser.sprites[startSprite + 2].isVisible = scarVisible;
			sLeaser.sprites[startSprite + 2].x = sLeaser.sprites[startSprite].x;
			sLeaser.sprites[startSprite + 2].y = sLeaser.sprites[startSprite].y;
			sLeaser.sprites[startSprite + 2].scaleX = sLeaser.sprites[startSprite].scaleX;
			sLeaser.sprites[startSprite + 2].scaleY = sLeaser.sprites[startSprite].scaleY;
			sLeaser.sprites[startSprite + 2].rotation = sLeaser.sprites[startSprite].rotation;
		}
	}

	public struct PlayerSpineData
	{
		public float f;

		public Vector2 pos;

		public Vector2 outerPos;

		public Vector2 dir;

		public Vector2 perp;

		public float depthRotation;

		public float rad;

		public PlayerSpineData(float f, Vector2 pos, Vector2 outerPos, Vector2 dir, Vector2 perp, float depthRotation, float rad)
		{
			this.f = f;
			this.pos = pos;
			this.outerPos = outerPos;
			this.dir = dir;
			this.perp = perp;
			this.depthRotation = depthRotation;
			this.rad = rad;
		}
	}

	public class TailSpeckles
	{
		public PlayerGraphics pGraphics;

		public int numberOfSprites;

		public int startSprite;

		public int rows;

		public int lines;

		public float spearProg;

		public int spearLine;

		public int spearRow;

		public int spearType;

		public TailSpeckles(PlayerGraphics pGraphics, int startSprite)
		{
			this.pGraphics = pGraphics;
			this.startSprite = startSprite;
			rows = 5;
			lines = 3;
			numberOfSprites = rows * lines + 1;
			newSpearSlot();
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			for (int i = startSprite; i < startSprite + numberOfSprites; i++)
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			for (int i = 0; i < rows; i++)
			{
				float f = Mathf.InverseLerp(0f, rows - 1, i);
				float s = Mathf.Lerp(0.4f, 0.95f, Mathf.Pow(f, 0.8f));
				PlayerSpineData playerSpineData = pGraphics.SpinePosition(s, timeStacker);
				Color color = ((pGraphics.player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup) ? pGraphics.player.ShortCutColor() : SlugcatColor(pGraphics.CharacterForColor));
				float num = 0.8f * Mathf.Pow(f, 0.5f);
				float num2 = (0.8f - num) * spearProg;
				Color color2 = Color.Lerp(color, Color.Lerp(new Color(1f, 1f, 1f), color, 0.3f), 0.2f + num + num2);
				for (int j = 0; j < lines; j++)
				{
					float num3 = ((float)j + ((i % 2 != 0) ? 0f : 0.5f)) / (float)(lines - 1);
					num3 = -1f + 2f * num3;
					if (num3 < -1f)
					{
						num3 += 2f;
					}
					else if (num3 > 1f)
					{
						num3 -= 2f;
					}
					num3 = Mathf.Sign(num3) * Mathf.Pow(Mathf.Abs(num3), 0.6f);
					Vector2 vector = playerSpineData.pos + playerSpineData.perp * (playerSpineData.rad + 0.5f) * num3;
					sLeaser.sprites[startSprite + i * lines + j].x = vector.x - camPos.x;
					sLeaser.sprites[startSprite + i * lines + j].y = vector.y - camPos.y;
					sLeaser.sprites[startSprite + i * lines + j].color = new Color(1f, 0f, 0f);
					sLeaser.sprites[startSprite + i * lines + j].rotation = Custom.VecToDeg(playerSpineData.dir);
					sLeaser.sprites[startSprite + i * lines + j].scaleX = Custom.LerpMap(Mathf.Abs(num3), 0.4f, 1f, 1f, 0f);
					sLeaser.sprites[startSprite + i * lines + j].scaleY = 1f;
					if (spearProg > 0f)
					{
						if (i == spearRow && j == spearLine)
						{
							sLeaser.sprites[startSprite + i * lines + j].scaleX *= 1f + spearProg * 2f;
							sLeaser.sprites[startSprite + i * lines + j].scaleY *= 1f + spearProg * 2f;
						}
						else if ((i == spearRow + 1 && j == spearLine) || (i == spearRow - 1 && j == spearLine) || (i == spearRow && j == spearLine + 1) || (i == spearRow && j == spearLine - 1))
						{
							sLeaser.sprites[startSprite + i * lines + j].scaleX *= 1f + spearProg;
							sLeaser.sprites[startSprite + i * lines + j].scaleY *= 1f + spearProg;
						}
					}
					if (ModManager.CoopAvailable && pGraphics.useJollyColor)
					{
						sLeaser.sprites[startSprite + i * lines + j].color = JollyColor(pGraphics.player.playerState.playerNumber, 2);
					}
					else if (CustomColorsEnabled())
					{
						sLeaser.sprites[startSprite + i * lines + j].color = CustomColorSafety(2);
					}
					else if (pGraphics.CharacterForColor == SlugcatStats.Name.White || pGraphics.CharacterForColor == SlugcatStats.Name.Yellow)
					{
						sLeaser.sprites[startSprite + i * lines + j].color = Color.gray;
					}
					else
					{
						sLeaser.sprites[startSprite + i * lines + j].color = color2;
					}
					if (i == spearRow && j == spearLine)
					{
						sLeaser.sprites[startSprite + lines * rows].x = vector.x - camPos.x;
						sLeaser.sprites[startSprite + lines * rows].y = vector.y - camPos.y;
						if (ModManager.CoopAvailable && pGraphics.useJollyColor)
						{
							sLeaser.sprites[startSprite + lines * rows].color = JollyColor(pGraphics.player.playerState.playerNumber, 2);
						}
						else if (CustomColorsEnabled())
						{
							sLeaser.sprites[startSprite + lines * rows].color = CustomColorSafety(2);
						}
						else
						{
							sLeaser.sprites[startSprite + lines * rows].color = Color.white;
						}
						Vector2 v = Custom.PerpendicularVector(playerSpineData.dir);
						if (v.normalized.y > 0.35f)
						{
							v.y *= -1f;
							v.x *= -1f;
						}
						float rotation = Custom.VecToDeg(v);
						sLeaser.sprites[startSprite + lines * rows].rotation = rotation;
						sLeaser.sprites[startSprite + lines * rows].scaleY = (0f - spearProg) * 0.5f;
						sLeaser.sprites[startSprite + lines * rows].element = Futile.atlasManager.GetElementWithName("BioSpear" + (spearType % 3 + 1));
					}
				}
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < lines; j++)
				{
					sLeaser.sprites[startSprite + i * lines + j] = new FSprite("tinyStar");
				}
			}
			sLeaser.sprites[startSprite + rows * lines] = new FSprite("BioSpear" + (spearType % 3 + 1));
			sLeaser.sprites[startSprite + rows * lines].anchorY = 0f;
		}

		public void newSpearSlot()
		{
			spearLine = UnityEngine.Random.Range(0, lines - 1);
			spearRow = UnityEngine.Random.Range(0, rows - 1);
		}

		public void setSpearProgress(float p)
		{
			spearProg = Mathf.Clamp(p, 0f, 1f);
			if (spearProg == 0f)
			{
				spearType = UnityEngine.Random.Range(0, 3);
			}
		}
	}

	public class Gown
	{
		private PlayerGraphics owner;

		private int divs;

		public Vector2[,,] clothPoints;

		public bool visible;

		public bool needsReset;

		public Gown(PlayerGraphics owner)
		{
			divs = 11;
			this.owner = owner;
			clothPoints = new Vector2[divs, divs, 3];
			visible = false;
			needsReset = true;
		}

		public void Update()
		{
			if (!visible || owner.player.room == null)
			{
				needsReset = true;
				return;
			}
			if (needsReset)
			{
				Custom.Log("GownReset");
				for (int i = 0; i < divs; i++)
				{
					for (int j = 0; j < divs; j++)
					{
						clothPoints[i, j, 1] = owner.player.bodyChunks[1].pos;
						clothPoints[i, j, 0] = owner.player.bodyChunks[1].pos;
						clothPoints[i, j, 2] *= 0f;
					}
				}
				needsReset = false;
			}
			Vector2 vector = Vector2.Lerp(owner.head.pos, owner.player.bodyChunks[1].pos, 0.75f);
			if (owner.player.bodyMode == Player.BodyModeIndex.Crawl)
			{
				vector += new Vector2(0f, 4f);
			}
			Vector2 vector2 = default(Vector2);
			if (owner.player.bodyMode == Player.BodyModeIndex.Stand)
			{
				vector += new Vector2(0f, Mathf.Sin((float)owner.player.animationFrame / 6f * 2f * (float)Math.PI) * 2f);
				vector2 = new Vector2(0f, -11f + Mathf.Sin((float)owner.player.animationFrame / 6f * 2f * (float)Math.PI) * -2.5f);
			}
			Vector2 bodyPos = vector;
			Vector2 vector3 = Custom.DirVec(owner.player.bodyChunks[1].pos, owner.player.bodyChunks[0].pos + Custom.DirVec(default(Vector2), owner.player.bodyChunks[0].vel) * 5f) * 1.6f;
			Vector2 perp = Custom.PerpendicularVector(vector3);
			for (int k = 0; k < divs; k++)
			{
				for (int l = 0; l < divs; l++)
				{
					Mathf.InverseLerp(0f, divs - 1, k);
					float num = Mathf.InverseLerp(0f, divs - 1, l);
					clothPoints[k, l, 1] = clothPoints[k, l, 0];
					clothPoints[k, l, 0] += clothPoints[k, l, 2];
					clothPoints[k, l, 2] *= 0.999f;
					clothPoints[k, l, 2].y -= 1.1f * owner.player.EffectiveRoomGravity;
					Vector2 vector4 = IdealPosForPoint(k, l, bodyPos, vector3, perp) + vector2 * (-1f * num);
					Vector3 vector5 = Vector3.Slerp(-vector3, Custom.DirVec(vector, vector4), num) * (0.01f + 0.9f * num);
					clothPoints[k, l, 2] += new Vector2(vector5.x, vector5.y);
					float num2 = Vector2.Distance(clothPoints[k, l, 0], vector4);
					float num3 = Mathf.Lerp(0f, 9f, num);
					Vector2 vector6 = Custom.DirVec(clothPoints[k, l, 0], vector4);
					if (num2 > num3)
					{
						clothPoints[k, l, 0] -= (num3 - num2) * vector6 * (1f - num / 1.4f);
						clothPoints[k, l, 2] -= (num3 - num2) * vector6 * (1f - num / 1.4f);
					}
					for (int m = 0; m < 4; m++)
					{
						IntVector2 intVector = new IntVector2(k, l) + Custom.fourDirections[m];
						if (intVector.x >= 0 && intVector.y >= 0 && intVector.x < divs && intVector.y < divs)
						{
							num2 = Vector2.Distance(clothPoints[k, l, 0], clothPoints[intVector.x, intVector.y, 0]);
							vector6 = Custom.DirVec(clothPoints[k, l, 0], clothPoints[intVector.x, intVector.y, 0]);
							float num4 = Vector2.Distance(vector4, IdealPosForPoint(intVector.x, intVector.y, bodyPos, vector3, perp));
							clothPoints[k, l, 2] -= (num4 - num2) * vector6 * 0.05f;
							clothPoints[intVector.x, intVector.y, 2] += (num4 - num2) * vector6 * 0.05f;
						}
					}
				}
			}
		}

		private Vector2 IdealPosForPoint(int x, int y, Vector2 bodyPos, Vector2 dir, Vector2 perp)
		{
			float num = Mathf.InverseLerp(0f, divs - 1, x);
			float t = Mathf.InverseLerp(0f, divs - 1, y);
			return bodyPos + Mathf.Lerp(-1f, 1f, num) * perp * Mathf.Lerp(9f, 11f, t) + dir * Mathf.Lerp(8f, -9f, t) * (1f + Mathf.Sin((float)Math.PI * num) * 0.35f * Mathf.Lerp(-1f, 1f, t));
		}

		public Color Color(float f)
		{
			return Custom.HSL2RGB(Mathf.Lerp(0.38f, 0.32f, Mathf.Pow(f, 2f)), Mathf.Lerp(0f, 0.1f, Mathf.Pow(f, 1.1f)), Mathf.Lerp(0.7f, 0.3f, Mathf.Pow(f, 6f)));
		}

		public void InitiateSprite(int sprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[sprite] = TriangleMesh.MakeGridMesh("MoonCloakTex", divs - 1);
			for (int i = 0; i < divs; i++)
			{
				for (int j = 0; j < divs; j++)
				{
					clothPoints[i, j, 0] = owner.player.firstChunk.pos;
					clothPoints[i, j, 1] = owner.player.firstChunk.pos;
					clothPoints[i, j, 2] = new Vector2(0f, 0f);
				}
			}
		}

		public void ApplyPalette(int sprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			for (int i = 0; i < divs; i++)
			{
				for (int j = 0; j < divs; j++)
				{
					(sLeaser.sprites[sprite] as TriangleMesh).verticeColors[j * divs + i] = Color((float)i / (float)(divs - 1));
				}
			}
		}

		public void DrawSprite(int sprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[sprite].isVisible = visible && owner.player.room != null;
			if (!sLeaser.sprites[sprite].isVisible)
			{
				return;
			}
			for (int i = 0; i < divs; i++)
			{
				for (int j = 0; j < divs; j++)
				{
					(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(i * divs + j, Vector2.Lerp(clothPoints[i, j, 1], clothPoints[i, j, 0], timeStacker) - camPos);
				}
			}
		}
	}

	public TailSegment[] tail;

	public GenericBodyPart head;

	private GenericBodyPart legs;

	public Vector2[,] drawPositions;

	private Vector2 legsDirection;

	public float disbalanceAmount;

	public float balanceCounter;

	public SlugcatHand[] hands;

	private Player player;

	public float airborneCounter;

	public Vector2 lookDirection;

	public Vector2 lastLookDir;

	public int blink;

	public float lastMarkAlpha;

	public float markAlpha;

	private PlayerObjectLooker objectLooker;

	private int handEngagedInThrowing;

	private PhysicalObject thrownObject;

	private int throwCounter;

	public LightSource lightSource;

	public float spearDir;

	public float flail;

	public int swallowing;

	public float breath;

	public float lastBreath;

	public float malnourished;

	public float markBaseAlpha = 1f;

	public float currentAppliedHypothermia;

	public static Color?[][] jollyColors;

	public static List<Color> customColors;

	public AxolotlGills gills;

	public RopeSegment[] ropeSegments;

	public float rubberMarkX;

	public float rubberMarkY;

	public float rubberMouseX;

	public float rubberMouseY;

	public float rubberRadius;

	public float rubberAlphaPips;

	public float rubberAlphaEmblem;

	public int numGodPips;

	private float stretch;

	private float lastStretch;

	public float darkenFactor;

	public Tentacle[] tentacles;

	public int tentaclesVisible;

	public TailSpeckles tailSpecks;

	public CosmeticPearl bodyPearl;

	public LightSource lanternLight;

	public Gown gown;

	public int gownIndex;

	private AGCachedStrings3Dim _cachedFaceSpriteNames;

	private AGCachedStrings2Dim _cachedHeads;

	private AGCachedStrings _cachedPlayerArms;

	private AGCachedStrings _cachedLegsA;

	private AGCachedStrings _cachedLegsACrawling;

	private AGCachedStrings _cachedLegsAClimbing;

	private AGCachedStrings _cachedLegsAOnPole;

	public override bool ShouldBeCulled => false;

	public SlugcatStats.Name CharacterForColor
	{
		get
		{
			if (player.room != null && player.room.game.setupValues.arenaDefaultColors)
			{
				return player.SlugCatClass;
			}
			return player.playerState.slugcatCharacter;
		}
	}

	public bool useJollyColor
	{
		get
		{
			if (ModManager.CoopAvailable)
			{
				if (!(Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.CUSTOM))
				{
					if (Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO)
					{
						return player.IsJollyPlayer;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public bool RenderAsPup
	{
		get
		{
			if (!ModManager.CoopAvailable || !player.playerState.isPup)
			{
				if (ModManager.MSC && (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup || player.playerState.isPup))
				{
					return !player.playerState.forceFullGrown;
				}
				return false;
			}
			return true;
		}
	}

	public float RopeStretchFac
	{
		get
		{
			float num = Mathf.Lerp(player.tongue.totalRope, player.tongue.RequestRope(), 0.5f) / (player.tongue.rope.totalLength + 80f);
			num = Mathf.Pow(num, (num >= 1f) ? 0.4f : 1.6f);
			if (player.tongue.mode == Player.Tongue.Mode.AttachedToTerrain && player.tongue.mode == Player.Tongue.Mode.AttachedToTerrain)
			{
				num = Mathf.Lerp(num, 1f, 0.5f);
			}
			return num;
		}
	}

	public PlayerGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		InitCachedSpriteNames();
		player = ow as Player;
		malnourished = ((player.Malnourished || player.redsIllness != null) ? 1f : 0f);
		List<BodyPart> list = new List<BodyPart>();
		airborneCounter = 0f;
		tail = new TailSegment[4];
		if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
		{
			float num = 0.85f + 0.3f * Mathf.Lerp(player.npcStats.Wideness, 0.5f, player.playerState.isPup ? 0.5f : 0f);
			float num2 = (0.75f + 0.5f * player.npcStats.Size) * (player.playerState.isPup ? 0.5f : 1f);
			tail[0] = new TailSegment(this, 6f * num, 4f * num2, null, 0.85f, 1f, 1f, pullInPreviousPosition: true);
			tail[1] = new TailSegment(this, 4f * num, 7f * num2, tail[0], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
			tail[2] = new TailSegment(this, 2.5f * num, 7f * num2, tail[1], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
			tail[3] = new TailSegment(this, 1f * num, 7f * num2, tail[2], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
		}
		else if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			if (player.playerState.isPup)
			{
				tail[0] = new TailSegment(this, 8f, 2f, null, 0.85f, 1f, 1f, pullInPreviousPosition: true);
				tail[1] = new TailSegment(this, 6f, 3.5f, tail[0], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
				tail[2] = new TailSegment(this, 4f, 3.5f, tail[1], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
				tail[3] = new TailSegment(this, 2f, 3.5f, tail[2], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
			}
			else
			{
				tail[0] = new TailSegment(this, 8f, 4f, null, 0.85f, 1f, 1f, pullInPreviousPosition: true);
				tail[1] = new TailSegment(this, 6f, 7f, tail[0], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
				tail[2] = new TailSegment(this, 4f, 7f, tail[1], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
				tail[3] = new TailSegment(this, 2f, 7f, tail[2], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
			}
		}
		else if ((ModManager.MSC || ModManager.CoopAvailable) && player.playerState.isPup)
		{
			tail[0] = new TailSegment(this, 6f, 2f, null, 0.85f, 1f, 1f, pullInPreviousPosition: true);
			tail[1] = new TailSegment(this, 4f, 3.5f, tail[0], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
			tail[2] = new TailSegment(this, 2.5f, 3.5f, tail[1], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
			tail[3] = new TailSegment(this, 1f, 3.5f, tail[2], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
		}
		else
		{
			tail[0] = new TailSegment(this, 6f, 4f, null, 0.85f, 1f, 1f, pullInPreviousPosition: true);
			tail[1] = new TailSegment(this, 4f, 7f, tail[0], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
			tail[2] = new TailSegment(this, 2.5f, 7f, tail[1], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
			tail[3] = new TailSegment(this, 1f, 7f, tail[2], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
		}
		if (player.bool1)
		{
			tail[0] = new TailSegment(this, 7f, 1f, null, 0.85f, 1f, 1f, pullInPreviousPosition: true);
			tail[1] = new TailSegment(this, 2f, 2f, tail[0], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
			tail[2] = new TailSegment(this, 0.93500006f, 2f, tail[1], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
			tail[3] = new TailSegment(this, 0.85f, 2f, tail[2], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
		}
		for (int i = 0; i < tail.Length; i++)
		{
			list.Add(tail[i]);
		}
		hands = new SlugcatHand[2];
		for (int j = 0; j < 2; j++)
		{
			hands[j] = new SlugcatHand(this, base.owner.bodyChunks[0], j, 3f, 0.8f, 1f);
			list.Add(hands[j]);
		}
		head = new GenericBodyPart(this, 4f, 0.8f, 0.99f, base.owner.bodyChunks[0]);
		list.Add(head);
		legs = new GenericBodyPart(this, 1f, 0.8f, 0.99f, base.owner.bodyChunks[1]);
		list.Add(legs);
		legsDirection = new Vector2(0f, -1f);
		if (ModManager.MSC)
		{
			if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				gills = new AxolotlGills(this, 12);
			}
			if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				tailSpecks = new TailSpeckles(this, 12);
				bodyPearl = new CosmeticPearl(this, 12 + tailSpecks.numberOfSprites);
			}
			numGodPips = 12;
			if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				ropeSegments = new RopeSegment[20];
				for (int k = 0; k < ropeSegments.Length; k++)
				{
					ropeSegments[k] = new RopeSegment(k, this);
				}
				tentacles = new Tentacle[4];
				for (int l = 0; l < tentacles.Length; l++)
				{
					tentacles[l] = new Tentacle(this, 15 + numGodPips + l * 2, 100f, base.owner.bodyChunks[0].pos);
				}
			}
			gown = new Gown(this);
		}
		drawPositions = new Vector2[base.owner.bodyChunks.Length, 2];
		disbalanceAmount = 0f;
		balanceCounter = 0f;
		for (int m = 0; m < base.owner.bodyChunks.Length; m++)
		{
			drawPositions[m, 0] = base.owner.bodyChunks[m].pos;
			drawPositions[m, 1] = base.owner.bodyChunks[m].lastPos;
		}
		lookDirection = new Vector2(0f, 0f);
		objectLooker = new PlayerObjectLooker(this);
		if (player.AI == null && (player.slugcatStats.name == SlugcatStats.Name.Red || (ModManager.MSC && player.slugcatStats.name == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)) && player.abstractCreature.world.game.IsStorySession)
		{
			markBaseAlpha = Mathf.Pow(Mathf.InverseLerp(4f, 14f, player.abstractCreature.world.game.GetStorySession.saveState.cycleNumber), 3.5f);
		}
		bodyParts = list.ToArray();
		Custom.Log("Creating player graphics!", player.playerState.playerNumber.ToString());
		if (player.playerState.playerNumber == 0)
		{
			PopulateJollyColorArray(player.slugcatStats.name);
		}
	}

	public override void Update()
	{
		base.Update();
		lastMarkAlpha = markAlpha;
		if (ModManager.MSC)
		{
			MSCUpdate();
		}
		if (!player.dead && player.room.game.session is StoryGameSession && (player.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.theMark && !player.isNPC && !player.DreamState)
		{
			markAlpha = Custom.LerpAndTick(markAlpha, Mathf.Clamp(Mathf.InverseLerp(30f, 80f, player.touchedNoInputCounter) - UnityEngine.Random.value * Mathf.InverseLerp(80f, 30f, player.touchedNoInputCounter), 0f, 1f) * markBaseAlpha, 0.1f, 1f / 30f);
		}
		else
		{
			markAlpha = 0f;
		}
		if (player.input[1].x != player.input[0].x || player.input[1].y != player.input[0].y)
		{
			flail = Mathf.Min(1f, flail + 1f / 3f);
		}
		else
		{
			flail = Mathf.Max(0f, flail - 0.0125f);
		}
		lastBreath = breath;
		if (!player.dead)
		{
			if (player.Sleeping)
			{
				breath += 0.0125f;
			}
			else
			{
				breath += 1f / Mathf.Lerp(60f, 15f, Mathf.Pow(player.aerobicLevel, 1.5f));
			}
		}
		if (lightSource != null)
		{
			lightSource.stayAlive = true;
			lightSource.setPos = player.mainBodyChunk.pos;
			if (lightSource.slatedForDeletetion || player.room.Darkness(player.mainBodyChunk.pos) == 0f)
			{
				lightSource = null;
			}
		}
		else if (player.room.Darkness(player.mainBodyChunk.pos) > 0f && player.glowing && !player.DreamState)
		{
			lightSource = new LightSource(player.mainBodyChunk.pos, environmentalLight: false, Color.Lerp(new Color(1f, 1f, 1f), (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup) ? player.ShortCutColor() : SlugcatColor(CharacterForColor), 0.5f), player);
			lightSource.requireUpKeep = true;
			lightSource.setRad = 300f;
			lightSource.setAlpha = 1f;
			player.room.AddObject(lightSource);
		}
		if (ModManager.MMF)
		{
			Color? color = player.StomachGlowLightColor();
			if (lanternLight != null)
			{
				lanternLight.stayAlive = true;
				lanternLight.setPos = player.bodyChunks[1].pos;
				lanternLight.setAlpha = 0.09f + UnityEngine.Random.value / 50f;
				if (lanternLight.slatedForDeletetion || !color.HasValue)
				{
					lanternLight = null;
				}
			}
			else if (color.HasValue)
			{
				lanternLight = new LightSource(player.bodyChunks[1].pos, environmentalLight: true, color.Value, player);
				lanternLight.submersible = true;
				lanternLight.requireUpKeep = true;
				lanternLight.setRad = 60f;
				lanternLight.setAlpha = 0.09f + UnityEngine.Random.value / 50f;
				lanternLight.flat = true;
				player.room.AddObject(lanternLight);
			}
		}
		if (malnourished > 0f && !player.Malnourished)
		{
			malnourished = Mathf.Max(0f, malnourished - 0.005f);
		}
		if (player.bodyMode == Player.BodyModeIndex.Stand && player.input[0].x != 0)
		{
			spearDir = Mathf.Clamp(spearDir + (float)player.input[0].x * 0.1f, -1f, 1f);
		}
		else if (spearDir < 0f)
		{
			spearDir = Mathf.Min(spearDir + 0.05f, 0f);
		}
		else if (spearDir > 0f)
		{
			spearDir = Mathf.Max(spearDir - 0.05f, 0f);
		}
		if (player.room.world.rainCycle.RainApproaching < 1f && UnityEngine.Random.value > player.room.world.rainCycle.RainApproaching && UnityEngine.Random.value < 1f / 102f && (player.room.roomSettings.DangerType == RoomRain.DangerType.Rain || player.room.roomSettings.DangerType == RoomRain.DangerType.FloodAndRain))
		{
			objectLooker.LookAtPoint(new Vector2(player.room.PixelWidth * UnityEngine.Random.value, player.room.PixelHeight + 100f), (1f - player.room.world.rainCycle.RainApproaching) * 0.6f);
		}
		float num = 0f;
		if (player.Consious && objectLooker.currentMostInteresting != null && objectLooker.currentMostInteresting is Creature)
		{
			CreatureTemplate.Relationship relationship = player.abstractCreature.creatureTemplate.CreatureRelationship((objectLooker.currentMostInteresting as Creature).abstractCreature.creatureTemplate);
			if (relationship.type == CreatureTemplate.Relationship.Type.Afraid && !(objectLooker.currentMostInteresting as Creature).dead)
			{
				num = Mathf.InverseLerp(Mathf.Lerp(40f, 250f, relationship.intensity), 10f, Vector2.Distance(player.mainBodyChunk.pos, objectLooker.mostInterestingLookPoint) * (player.room.VisualContact(player.mainBodyChunk.pos, objectLooker.mostInterestingLookPoint) ? 1f : 1.5f));
				if ((objectLooker.currentMostInteresting as Creature).abstractCreature.abstractAI != null && (objectLooker.currentMostInteresting as Creature).abstractCreature.abstractAI.RealAI != null)
				{
					num *= (objectLooker.currentMostInteresting as Creature).abstractCreature.abstractAI.RealAI.CurrentPlayerAggression(player.abstractCreature);
				}
			}
		}
		if (!player.Consious)
		{
			objectLooker.LookAtNothing();
			blink = 10;
		}
		if (DEBUGLABELS != null)
		{
			DEBUGLABELS[0].label.text = player.bodyMode.ToString() + " " + player.animation.ToString();
			DEBUGLABELS[1].label.text = "XPOS: " + player.mainBodyChunk.pos.x + " YPOS: " + player.mainBodyChunk.pos.y;
			DEBUGLABELS[2].label.text = "XPOS: " + player.bodyChunks[1].pos.x + " YPOS: " + player.bodyChunks[1].pos.y;
		}
		for (int i = 0; i < base.owner.bodyChunks.Length; i++)
		{
			drawPositions[i, 1] = drawPositions[i, 0];
		}
		drawPositions[0, 0] = base.owner.bodyChunks[0].pos;
		drawPositions[1, 0] = base.owner.bodyChunks[1].pos;
		if (RenderAsPup)
		{
			drawPositions[0, 0] = Vector2.Lerp(drawPositions[0, 0], drawPositions[1, 0], 0.35f + (0.25f - ((player.npcStats != null) ? player.npcStats.Size : 0.5f) * 0.25f));
		}
		int num2 = 0;
		bool flag = false;
		float num3 = 1f;
		if (player.bodyMode == Player.BodyModeIndex.Stand)
		{
			drawPositions[0, 0].x += (float)player.flipDirection * (RenderAsPup ? 2f : 6f) * Mathf.Clamp(Mathf.Abs(base.owner.bodyChunks[1].vel.x) - 0.2f, 0f, 1f);
			drawPositions[0, 0].y += Mathf.Cos(((float)player.animationFrame + 0f) / 6f * 2f * (float)Math.PI) * (RenderAsPup ? 1.5f : 2f);
			drawPositions[1, 0].x -= (float)player.flipDirection * (1.5f - (float)player.animationFrame / 6f) * (RenderAsPup ? 0.25f : 1f);
			drawPositions[1, 0].y += 2f + Mathf.Sin(((float)player.animationFrame + 0f) / 6f * 2f * (float)Math.PI) * (RenderAsPup ? 2f : 4f);
			flag = Mathf.Abs(base.owner.bodyChunks[0].vel.x) > 2f && Mathf.Abs(base.owner.bodyChunks[1].vel.x) > 2f;
			num3 = 1f - Mathf.Clamp((Mathf.Abs(base.owner.bodyChunks[1].vel.x) - 1f) * 0.5f, 0f, 1f);
		}
		else if (player.bodyMode == Player.BodyModeIndex.Crawl)
		{
			num2 = 1;
			float num4 = Mathf.Sin((float)player.animationFrame / 21f * 2f * (float)Math.PI);
			float num5 = Mathf.Cos((float)player.animationFrame / 14f * 2f * (float)Math.PI);
			float num6 = ((player.superLaunchJump > 19) ? 0f : 1f);
			drawPositions[0, 0].x += num5 * (float)player.flipDirection * 2f;
			drawPositions[0, 0].y -= num4 * -1.5f - 3f;
			head.vel.y -= num4 * -0.5f - 0.5f;
			head.vel.x += ((base.owner.bodyChunks[0].pos.x < base.owner.bodyChunks[1].pos.x) ? (-1f) : 1f);
			drawPositions[1, 0].x += -3f * num4 * (float)player.flipDirection;
			drawPositions[1, 0].y -= num5 * 1.5f - 7f + 3f * num6;
		}
		else if (player.bodyMode == Player.BodyModeIndex.ClimbingOnBeam)
		{
			num2 = 2;
			if (player.animation == Player.AnimationIndex.GetUpOnBeam)
			{
				disbalanceAmount = 70f;
			}
			else if (player.animation == Player.AnimationIndex.GetUpToBeamTip)
			{
				disbalanceAmount = 120f;
			}
			else if (player.animation == Player.AnimationIndex.StandOnBeam)
			{
				num2 = 0;
				drawPositions[1, 0].y += 3f;
				flag = Mathf.Abs(base.owner.bodyChunks[0].vel.x) > 2f && Mathf.Abs(base.owner.bodyChunks[1].vel.x) > 2f;
				num3 = 1f - Mathf.Clamp((Mathf.Abs(base.owner.bodyChunks[1].vel.x) - 1f) * 0.3f, 0f, 1f);
				if (flag)
				{
					tail[0].vel.x -= base.owner.bodyChunks[0].vel.x * 2f;
					tail[0].vel.y += 1.5f;
					tail[1].vel.x -= base.owner.bodyChunks[0].vel.x * 0.2f;
					tail[1].vel.y += 0.5f;
				}
			}
			else if (player.animation == Player.AnimationIndex.ClimbOnBeam)
			{
				drawPositions[0, 0].x += (float)player.flipDirection * 2.5f + (float)player.flipDirection * 0.5f * Mathf.Sin((float)player.animationFrame / 20f * (float)Math.PI * 2f);
				drawPositions[1, 0].x += (float)player.flipDirection * 2.5f * Mathf.Cos((float)player.animationFrame / 20f * (float)Math.PI * 2f);
			}
		}
		else if (player.bodyMode == Player.BodyModeIndex.WallClimb)
		{
			num2 = 1;
			legsDirection.y -= 1f;
			drawPositions[0, 0].y += 2f;
			drawPositions[0, 0].x -= (float)player.flipDirection * ((base.owner.bodyChunks[1].ContactPoint.y < 0) ? 3f : 5f);
			head.vel.y -= (float)player.flipDirection * 5f;
		}
		else if (player.bodyMode == Player.BodyModeIndex.Swimming)
		{
			if (player.animation == Player.AnimationIndex.DeepSwim || player.input[0].x != 0)
			{
				drawPositions[1, 0] += Custom.PerpendicularVector(Custom.DirVec(player.bodyChunks[0].pos, player.bodyChunks[1].pos)) * Mathf.Sin(player.swimCycle * 2f * (float)Math.PI) * 5f;
			}
		}
		else if (player.bodyMode == Player.BodyModeIndex.ZeroG)
		{
			disbalanceAmount = Mathf.Max(disbalanceAmount, 70f * Mathf.InverseLerp(0.8f, 1f, flail));
		}
		else if (player.bodyMode == Player.BodyModeIndex.Default)
		{
			if (player.animation == Player.AnimationIndex.AntlerClimb)
			{
				num2 = 2;
			}
			else if (player.animation == Player.AnimationIndex.LedgeGrab)
			{
				legsDirection.y -= 1f;
				drawPositions[0, 0].x -= (float)player.flipDirection * 5f;
			}
			else
			{
				num3 = 0f;
			}
		}
		if (player.animation == Player.AnimationIndex.Roll || player.animation == Player.AnimationIndex.Flip)
		{
			float num7 = 6f;
			Vector2 vector = Custom.DirVec(player.bodyChunks[0].pos, player.bodyChunks[1].pos);
			for (int j = 0; j < tail.Length; j++)
			{
				tail[j].vel += vector * num7;
				num7 /= 1.7f;
			}
		}
		else if (player.animation == Player.AnimationIndex.CorridorTurn)
		{
			drawPositions[0, 0] += Custom.DegToVec(UnityEngine.Random.value * 360f) * 3f * UnityEngine.Random.value;
			drawPositions[1, 0] += Custom.DegToVec(UnityEngine.Random.value * 360f) * 2f * UnityEngine.Random.value;
			blink = 5;
		}
		if (player.bodyMode == Player.BodyModeIndex.Default && player.animation == Player.AnimationIndex.None && base.owner.bodyChunks[0].ContactPoint.x == 0 && base.owner.bodyChunks[0].ContactPoint.y == 0 && base.owner.bodyChunks[1].ContactPoint.x == 0 && base.owner.bodyChunks[1].ContactPoint.y == 0)
		{
			airborneCounter += base.owner.bodyChunks[0].vel.magnitude;
		}
		else
		{
			airborneCounter = 0f;
		}
		if (player.bodyMode == Player.BodyModeIndex.ClimbingOnBeam && (player.animation == Player.AnimationIndex.BeamTip || player.animation == Player.AnimationIndex.StandOnBeam))
		{
			if (Mathf.Abs(base.owner.bodyChunks[0].vel.x) > 2f)
			{
				disbalanceAmount += ((player.animation == Player.AnimationIndex.BeamTip) ? 17f : 3f);
			}
			else
			{
				disbalanceAmount -= 1f;
			}
			disbalanceAmount = Mathf.Clamp(disbalanceAmount, 0f, 120f);
			balanceCounter += 1f + disbalanceAmount / 40f * (1f + UnityEngine.Random.value);
			if (balanceCounter > 300f)
			{
				balanceCounter -= 300f;
			}
			float num8 = Mathf.Sin(balanceCounter / 300f * (float)Math.PI * 2f) / (Mathf.Abs(base.owner.bodyChunks[1].vel.x) + 1f);
			drawPositions[0, 0].x += num8 * (disbalanceAmount + 20f) * 0.08f;
			drawPositions[0, 0].y += num8 * disbalanceAmount * 0.02f;
			tail[0].vel.x += num8 * (disbalanceAmount + 20f) * 0.1f;
			tail[1].vel.x += num8 * (disbalanceAmount + 20f) * 0.04f;
		}
		if (player.bodyMode == Player.BodyModeIndex.ZeroG)
		{
			disbalanceAmount -= 1f;
			disbalanceAmount = Mathf.Clamp(disbalanceAmount, 0f, 120f);
			balanceCounter += 1f + disbalanceAmount / 40f * (1f + UnityEngine.Random.value);
			if (balanceCounter > 300f)
			{
				balanceCounter -= 300f;
			}
			float num9 = Mathf.Sin(balanceCounter / 300f * (float)Math.PI * 2f);
			Vector2 vector2 = Custom.DirVec(player.bodyChunks[1].pos, player.mainBodyChunk.pos);
			Vector2 vector3 = Custom.PerpendicularVector(vector2);
			drawPositions[0, 0] += vector3 * num9 * (disbalanceAmount + 20f) * 0.08f;
			tail[0].vel -= vector3 * num9 * (disbalanceAmount + 20f) * 0.1f + vector2 * disbalanceAmount * 0.1f;
			tail[1].vel -= vector3 * num9 * (disbalanceAmount + 20f) * 0.04f + vector2 * disbalanceAmount * 0.04f;
		}
		if (player.Consious && player.standing && num > 0.5f)
		{
			drawPositions[0, 0] += Custom.DirVec(objectLooker.mostInterestingLookPoint, player.bodyChunks[0].pos) * 3.4f * Mathf.InverseLerp(0.5f, 1f, num);
			head.vel += Custom.DirVec(objectLooker.mostInterestingLookPoint, head.pos) * 1.4f * Mathf.InverseLerp(0.5f, 1f, num);
		}
		if (num > 0f)
		{
			tail[0].vel += Custom.DirVec(objectLooker.mostInterestingLookPoint, drawPositions[1, 0]) * 5f * num;
			tail[1].vel += Custom.DirVec(objectLooker.mostInterestingLookPoint, drawPositions[1, 0]) * 3f * num;
			player.aerobicLevel = Mathf.Max(player.aerobicLevel, Mathf.InverseLerp(0.5f, 1f, num) * 0.9f);
		}
		Vector2 vector4 = base.owner.bodyChunks[0].pos;
		if (flag)
		{
			vector4 = base.owner.bodyChunks[1].pos;
			vector4.y -= 4f;
			vector4.x += (float)player.flipDirection * 16f * Mathf.Clamp(Mathf.Abs(base.owner.bodyChunks[1].vel.x) - 0.2f, 0f, 1f);
		}
		Vector2 pos = base.owner.bodyChunks[1].pos;
		float num10 = 28f;
		tail[0].connectedPoint = drawPositions[1, 0];
		for (int k = 0; k < tail.Length; k++)
		{
			tail[k].Update();
			tail[k].vel *= Mathf.Lerp(0.75f, 0.95f, num3 * (1f - base.owner.bodyChunks[1].submersion));
			tail[k].vel.y -= Mathf.Lerp(0.1f, 0.5f, num3) * (1f - base.owner.bodyChunks[1].submersion) * base.owner.EffectiveRoomGravity;
			num3 = (num3 * 10f + 1f) / 11f;
			if (!Custom.DistLess(tail[k].pos, base.owner.bodyChunks[1].pos, 9f * (float)(k + 1)))
			{
				tail[k].pos = base.owner.bodyChunks[1].pos + Custom.DirVec(base.owner.bodyChunks[1].pos, tail[k].pos) * 9f * (k + 1);
			}
			tail[k].vel += Custom.DirVec(vector4, tail[k].pos) * num10 / Vector2.Distance(vector4, tail[k].pos);
			num10 *= 0.5f;
			vector4 = pos;
			pos = tail[k].pos;
		}
		if (player.swallowAndRegurgitateCounter > 15 && player.swallowAndRegurgitateCounter % 10 == 0)
		{
			blink = Math.Max(blink, UnityEngine.Random.Range(-5, 8));
		}
		if (swallowing > 0)
		{
			swallowing--;
			blink = 5;
			drawPositions[0, 0] = Vector2.Lerp(drawPositions[0, 0], drawPositions[1, 0], 0.4f * Mathf.Sin((float)swallowing / 12f * (float)Math.PI));
		}
		else if ((player.objectInStomach != null || (ModManager.MSC && (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand || player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear))) && player.swallowAndRegurgitateCounter > 0)
		{
			if (player.swallowAndRegurgitateCounter > 30)
			{
				blink = 5;
			}
			float num11 = Mathf.InverseLerp(0f, 110f, player.swallowAndRegurgitateCounter);
			float num12 = (float)player.swallowAndRegurgitateCounter / Mathf.Lerp(30f, 15f, num11);
			if (player.standing)
			{
				drawPositions[0, 0].y += Mathf.Sin(num12 * (float)Math.PI * 2f) * num11 * 2f;
				drawPositions[1, 0].y += (0f - Mathf.Sin((num12 + 0.2f) * (float)Math.PI * 2f)) * num11 * 3f;
			}
			else
			{
				drawPositions[0, 0].y += Mathf.Sin(num12 * (float)Math.PI * 2f) * num11 * 3f;
				drawPositions[0, 0].x += Mathf.Cos(num12 * (float)Math.PI * 2f) * num11 * 1f;
				drawPositions[1, 0].y += Mathf.Sin((num12 + 0.2f) * (float)Math.PI * 2f) * num11 * 2f;
				drawPositions[1, 0].x += (0f - Mathf.Cos(num12 * (float)Math.PI * 2f)) * num11 * 3f;
			}
		}
		blink--;
		if (blink < -UnityEngine.Random.Range(2, 1800))
		{
			blink = UnityEngine.Random.Range(3, UnityEngine.Random.Range(3, 10));
		}
		if (!player.dead)
		{
			if (player.exhausted)
			{
				if (player.aerobicLevel > 0.8f)
				{
					blink = Math.Max(blink, 1);
				}
				else if (UnityEngine.Random.value < 0.02f)
				{
					blink = Math.Max(blink, UnityEngine.Random.Range(10, 20));
				}
			}
			if (player.lungsExhausted || player.exhausted)
			{
				objectLooker.LookAtNothing();
				head.vel.y += Mathf.Sin(player.swimCycle * (float)Math.PI * 2f) * (player.lungsExhausted ? 1f : 0.25f);
				drawPositions[0, 0].y += Mathf.Sin(player.swimCycle * (float)Math.PI * 2f) * (player.lungsExhausted ? 2.5f : 0.75f);
				blink = 1;
			}
		}
		if (UnityEngine.Random.value < 0.1f)
		{
			objectLooker.Update();
		}
		if (UnityEngine.Random.value < 0.0025f)
		{
			objectLooker.LookAtNothing();
		}
		lastLookDir = lookDirection;
		if (player.Consious && objectLooker.looking)
		{
			lookDirection = Custom.DirVec(head.pos, objectLooker.mostInterestingLookPoint);
		}
		else
		{
			lookDirection *= 0f;
		}
		if (num > 0.86f)
		{
			blink = 5;
			lookDirection *= -1f;
		}
		if (player.grasps[0] != null && player.grasps[0].grabbed is JokeRifle)
		{
			lookDirection = (player.grasps[0].grabbed as JokeRifle).aimDir;
		}
		if (player.standing)
		{
			if (player.input[0].x == 0)
			{
				head.vel -= lookDirection * 0.5f;
			}
			drawPositions[0, 0] -= lookDirection * 2f;
		}
		else
		{
			head.vel += lookDirection;
		}
		Vector2 vector5 = Custom.DirVec(drawPositions[1, 0], drawPositions[0, 0]) * 3f;
		if (player.bodyMode == Player.BodyModeIndex.Crawl && !RenderAsPup)
		{
			vector5.x *= 2.5f;
		}
		else if (player.bodyMode == Player.BodyModeIndex.CorridorClimb && vector5.y < 0f)
		{
			vector5.y *= 2f;
		}
		head.Update();
		head.ConnectToPoint(Vector2.Lerp(drawPositions[0, 0], drawPositions[1, 0], 0.2f) + vector5, (player.animation == Player.AnimationIndex.HangFromBeam) ? 0f : 3f, push: false, 0.2f, base.owner.bodyChunks[0].vel, 0.7f, 0.1f);
		legs.Update();
		if (player.bodyMode == Player.BodyModeIndex.CorridorClimb)
		{
			legs.ConnectToPoint(base.owner.bodyChunks[1].pos + Custom.DirVec(base.owner.bodyChunks[0].pos, base.owner.bodyChunks[1].pos) * 4f, 2f, push: false, 0.25f, base.owner.bodyChunks[1].vel, 0.5f, 0.1f);
			int num13 = Mathf.RoundToInt((270f - Custom.AimFromOneVectorToAnother(base.owner.bodyChunks[1].pos, base.owner.bodyChunks[0].pos)) / 45f);
			int num14 = 10;
			int num15 = 0;
			for (int l = 0; l < 4; l++)
			{
				if (base.owner.room.GetTile(base.owner.room.GetTilePosition(base.owner.bodyChunks[1].pos) + Custom.eightDirections[(l + num13 + 10) % 8]).Terrain != Room.Tile.TerrainType.Solid || base.owner.room.GetTile(base.owner.room.GetTilePosition(base.owner.bodyChunks[1].pos) + Custom.eightDirections[(l + num13 + 14) % 8]).Terrain != Room.Tile.TerrainType.Solid)
				{
					continue;
				}
				int num16 = 0;
				switch (l)
				{
				case 1:
					num16 = ((player.flipDirection == -1) ? 1 : 2);
					break;
				case 3:
					num16 = ((player.flipDirection == 1) ? 1 : 2);
					break;
				case 2:
					num16 = 3;
					break;
				}
				if (num16 < num14)
				{
					num14 = num16;
					switch (l)
					{
					case 0:
						num15 = 0;
						break;
					case 1:
						num15 = 45;
						break;
					case 2:
						num15 = ((player.flipDirection == -1) ? (-90) : 90);
						break;
					case 3:
						num15 = -45;
						break;
					}
				}
			}
			legsDirection += Custom.DegToVec(Custom.AimFromOneVectorToAnother(base.owner.bodyChunks[0].pos, base.owner.bodyChunks[1].pos) + (float)num15);
		}
		else if (base.owner.bodyChunks[1].ContactPoint.y == -1 || player.animation == Player.AnimationIndex.StandOnBeam)
		{
			legs.ConnectToPoint(base.owner.bodyChunks[1].pos + new Vector2(legsDirection.x * 8f, 1f), 5f, push: false, 0.25f, new Vector2(base.owner.bodyChunks[1].vel.x, -10f), 0.5f, 0.1f);
			legsDirection.x -= base.owner.bodyChunks[1].onSlope;
			legsDirection.y -= 1f;
		}
		else if (player.animation == Player.AnimationIndex.BeamTip)
		{
			legs.ConnectToPoint(base.owner.bodyChunks[1].pos + new Vector2(0f, -8f), 0f, push: false, 0.25f, new Vector2(0f, -10f), 0.5f, 0.1f);
			legsDirection += Custom.DirVec(drawPositions[0, 0], base.owner.room.MiddleOfTile(base.owner.bodyChunks[1].pos) + new Vector2(0f, -10f));
		}
		else if (player.animation == Player.AnimationIndex.ClimbOnBeam)
		{
			Vector2 vector6 = new Vector2((float)(-player.flipDirection) * (5f - Mathf.Sin((float)player.animationFrame / 20f * (float)Math.PI * 2f)), -16f - 5f * Mathf.Cos((float)player.animationFrame / 20f * (float)Math.PI * 2f));
			legs.ConnectToPoint(base.owner.bodyChunks[0].pos + vector6, 0f, push: false, 0.25f, new Vector2(0f, 0f), 0.5f, 0.1f);
			legsDirection.y -= 1f;
		}
		else if (player.animation == Player.AnimationIndex.ZeroGSwim || player.animation == Player.AnimationIndex.ZeroGPoleGrab)
		{
			legs.ConnectToPoint(base.owner.bodyChunks[1].pos + Custom.DirVec(base.owner.bodyChunks[0].pos, base.owner.bodyChunks[1].pos) * 4f, 4f, push: false, 0f, base.owner.bodyChunks[1].vel, 0.2f, 0f);
			legsDirection = Custom.DirVec(base.owner.bodyChunks[0].pos, base.owner.bodyChunks[1].pos);
			legs.vel += legsDirection * 0.2f;
		}
		else
		{
			legs.ConnectToPoint(base.owner.bodyChunks[1].pos + new Vector2(legsDirection.x * 8f, (player.animation == Player.AnimationIndex.HangFromBeam) ? (-5f) : (-2f)), 4f, push: false, 0.25f, new Vector2(base.owner.bodyChunks[1].vel.x, -10f), 0.5f, 0.1f);
			legsDirection += base.owner.bodyChunks[1].vel * 0.01f;
			legsDirection.y -= 0.05f;
		}
		legsDirection.Normalize();
		if (player.Consious)
		{
			if (throwCounter > 0 && thrownObject != null)
			{
				hands[handEngagedInThrowing].reachingForObject = true;
				hands[handEngagedInThrowing].absoluteHuntPos = thrownObject.firstChunk.pos;
				if (Custom.DistLess(hands[handEngagedInThrowing].pos, thrownObject.firstChunk.pos, 40f))
				{
					hands[handEngagedInThrowing].pos = thrownObject.firstChunk.pos;
				}
				else
				{
					hands[handEngagedInThrowing].vel += Custom.DirVec(hands[handEngagedInThrowing].pos, thrownObject.firstChunk.pos) * 6f;
				}
				hands[1 - handEngagedInThrowing].vel -= Custom.DirVec(hands[handEngagedInThrowing].pos, thrownObject.firstChunk.pos) * 3f;
				throwCounter--;
			}
			else if (player.handOnExternalFoodSource.HasValue)
			{
				int num17 = ((!(player.handOnExternalFoodSource.Value.x < player.mainBodyChunk.pos.x)) ? 1 : 0);
				hands[num17].reachingForObject = true;
				if (player.eatExternalFoodSourceCounter < 3)
				{
					hands[num17].absoluteHuntPos = head.pos;
					blink = Math.Max(blink, 3);
				}
				else
				{
					hands[num17].absoluteHuntPos = player.handOnExternalFoodSource.Value;
				}
				drawPositions[0, 0] += Custom.DirVec(drawPositions[0, 0], player.handOnExternalFoodSource.Value) * 5f;
				head.vel += Custom.DirVec(drawPositions[0, 0], player.handOnExternalFoodSource.Value) * 2f;
			}
			else if ((player.grasps[0] != null && player.grasps[0].grabbed is TubeWorm) || (player.grasps[1] != null && player.grasps[1].grabbed is TubeWorm))
			{
				for (int m = 0; m < player.grasps.Length; m++)
				{
					if (player.grasps[m] != null && player.grasps[m].grabbed is TubeWorm)
					{
						hands[m].mode = Limb.Mode.HuntRelativePosition;
						hands[m].relativeHuntPos = new Vector2(5f * ((m == 0) ? (-1f) : 1f), -10f);
					}
				}
			}
			else if (player.spearOnBack != null && player.spearOnBack.counter > 5)
			{
				int num18 = -1;
				for (int n = 0; n < 2; n++)
				{
					if (num18 != -1)
					{
						break;
					}
					if ((player.spearOnBack.HasASpear && player.grasps[n] == null) || (!player.spearOnBack.HasASpear && player.grasps[n] != null && player.grasps[n].grabbed is Spear))
					{
						num18 = n;
					}
				}
				if (num18 > -1)
				{
					if (player.grasps[num18] != null && player.grasps[num18].grabbed is Weapon)
					{
						(player.grasps[num18].grabbed as Weapon).ChangeOverlap(newOverlap: false);
					}
					hands[num18].reachingForObject = true;
					hands[num18].mode = Limb.Mode.HuntRelativePosition;
					if (player.spearOnBack.HasASpear)
					{
						hands[num18].relativeHuntPos = Vector3.Slerp(new Vector2(((num18 == 0) ? (-1f) : 1f) * 20f, -30f) * Mathf.Sin(Mathf.InverseLerp(9f, 20f, player.spearOnBack.counter) * (float)Math.PI), new Vector2(0f, 1f), Mathf.InverseLerp(9f, 20f, player.spearOnBack.counter));
					}
					else
					{
						hands[num18].relativeHuntPos = Vector3.Slerp(new Vector2(((num18 == 0) ? (-1f) : 1f) * 30f, -20f) * Mathf.Lerp(1f, 0.2f, Mathf.Abs(player.spearOnBack.flip)), new Vector2(1f, 1f), Mathf.InverseLerp(14f, 20f, player.spearOnBack.counter));
					}
					drawPositions[0, 0] += Custom.DirVec(hands[num18].absoluteHuntPos, drawPositions[0, 0]) * 0.7f;
					head.vel += Custom.DirVec(hands[num18].absoluteHuntPos, head.pos) * 1.5f;
				}
			}
			else if (player.FoodInStomach < player.MaxFoodInStomach && objectLooker.currentMostInteresting != null && num2 < 2 && ((objectLooker.currentMostInteresting is Fly && (objectLooker.currentMostInteresting as Fly).PlayerAutoGrabable) || num > 0.8f) && player.AllowGrabbingBatflys() && Custom.DistLess(player.mainBodyChunk.pos, objectLooker.mostInterestingLookPoint, 80f) && player.room.VisualContact(player.mainBodyChunk.pos, objectLooker.mostInterestingLookPoint))
			{
				int num19 = -1;
				for (int num20 = 0; num20 < 2; num20++)
				{
					if (player.grasps[num20] == null && hands[1 - num20].reachedSnapPosition)
					{
						num19 = num20;
					}
				}
				if (objectLooker.currentMostInteresting is Fly && (objectLooker.currentMostInteresting as Fly).PlayerAutoGrabable && player.input[0].x != 0 && objectLooker.currentMostInteresting.bodyChunks[0].pos.x < player.mainBodyChunk.pos.x == player.input[0].x > 0)
				{
					num19 = -1;
				}
				if (num19 > -1)
				{
					hands[num19].reachingForObject = true;
					hands[num19].absoluteHuntPos = objectLooker.mostInterestingLookPoint;
					if (num == 0f)
					{
						drawPositions[0, 0] += Custom.DirVec(drawPositions[0, 0], objectLooker.mostInterestingLookPoint) * 5f;
						head.vel += Custom.DirVec(drawPositions[0, 0], objectLooker.mostInterestingLookPoint) * 2f;
					}
				}
			}
		}
		for (int num21 = 0; num21 < 2; num21++)
		{
			hands[num21].Update();
			if (float.IsNaN(hands[num21].pos.x) || float.IsNaN(hands[num21].pos.y))
			{
				hands[num21].pos = player.firstChunk.pos;
			}
		}
		if (player.sleepCurlUp > 0f)
		{
			float num22 = Mathf.Sign(player.bodyChunks[0].pos.x - player.bodyChunks[1].pos.x);
			Vector2 vector7 = (player.bodyChunks[0].pos + player.bodyChunks[1].pos) / 2f;
			drawPositions[0, 0] = Vector2.Lerp(drawPositions[0, 0], vector7, player.sleepCurlUp * 0.2f);
			drawPositions[1, 0] = Vector2.Lerp(drawPositions[1, 0], vector7, player.sleepCurlUp * 0.2f);
			drawPositions[0, 0].y += 2f * player.sleepCurlUp;
			drawPositions[1, 0].y += 2f * player.sleepCurlUp;
			drawPositions[1, 0].x -= 3f * num22 * player.sleepCurlUp;
			for (int num23 = 0; num23 < tail.Length; num23++)
			{
				float num24 = (float)num23 / (float)(tail.Length - 1);
				tail[num23].vel *= 1f - 0.2f * player.sleepCurlUp;
				tail[num23].pos = Vector2.Lerp(tail[num23].pos, drawPositions[1, 0] + new Vector2((Mathf.Sin(num24 * (float)Math.PI) * 25f - num24 * 10f) * (0f - num22), Mathf.Lerp(5f, -15f, num24)), 0.1f * player.sleepCurlUp);
			}
			head.vel *= 1f - 0.4f * player.sleepCurlUp;
			head.pos = Vector2.Lerp(head.pos, vector7 + new Vector2(num22 * 5f, -3f), 0.5f * player.sleepCurlUp);
			PlayerBlink();
			for (int num25 = 0; num25 < 2; num25++)
			{
				hands[num25].absoluteHuntPos = vector7 + new Vector2(num22 * 10f, -20f);
			}
		}
		if (player.Adrenaline > 0f)
		{
			float num26 = Mathf.Pow(player.Adrenaline, 0.2f);
			drawPositions[0, 0] += Custom.RNV() * UnityEngine.Random.value * num26 * 2f;
			drawPositions[0, 1] += Custom.RNV() * UnityEngine.Random.value * num26 * 2f;
			head.pos += Custom.RNV() * UnityEngine.Random.value * num26 * 1f;
			if (UnityEngine.Random.value < 0.05f)
			{
				blink = Math.Max(blink, 3);
			}
		}
	}

	public override void Reset()
	{
		for (int i = 0; i < base.owner.bodyChunks.Length; i++)
		{
			drawPositions[i, 0] = base.owner.bodyChunks[i].pos;
			drawPositions[i, 1] = base.owner.bodyChunks[i].pos;
		}
		for (int j = 0; j < tail.Length; j++)
		{
			tail[j].Reset(base.owner.bodyChunks[1].pos);
		}
		head.Reset(base.owner.bodyChunks[0].pos);
		legs.Reset(base.owner.bodyChunks[1].pos);
		hands[0].Reset(base.owner.bodyChunks[0].pos);
		hands[1].Reset(base.owner.bodyChunks[0].pos);
		if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			for (int k = 0; k < ropeSegments.Length; k++)
			{
				ropeSegments[k].pos = player.mainBodyChunk.pos;
				ropeSegments[k].lastPos = player.mainBodyChunk.pos;
				ropeSegments[k].vel *= 0f;
			}
		}
	}

	public override void SuckedIntoShortCut(Vector2 shortCutPosition)
	{
		for (int i = 0; i < tail.Length; i++)
		{
			tail[i].lastPos = tail[i].pos;
			tail[i].vel *= 0.5f;
			tail[i].pos = (tail[i].pos * 5f + shortCutPosition) / 6f;
		}
		head.vel *= 0.5f;
		legs.vel *= 0.5f;
		head.lastPos = head.pos;
		head.pos = Vector2.Lerp(head.pos, shortCutPosition, 1f / 3f);
		legs.lastPos = legs.pos;
		legs.pos = Vector2.Lerp(legs.pos, shortCutPosition, 1f / 3f);
	}

	public void NudgeDrawPosition(int drawPos, Vector2 nudge)
	{
		drawPositions[drawPos, 0] += nudge;
	}

	public void BiteFly(int hand)
	{
		head.vel += Custom.DirVec(head.pos, hands[hand].pos) * 2f;
		NudgeDrawPosition(0, new Vector2(0f, 1f));
		hands[hand].pos = drawPositions[0, 0];
		LookAtObject(player.grasps[hand].grabbed);
		for (int i = 0; i < UnityEngine.Random.Range(0, 3); i++)
		{
			player.room.AddObject(new WaterDrip(head.pos, Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * 4f, waterColor: false));
		}
		if (blink < 5)
		{
			blink = 5;
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		if (!base.owner.room.game.DEBUGMODE)
		{
			if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				sLeaser.sprites = new FSprite[13 + gills.numberOfSprites];
			}
			else if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				sLeaser.sprites = new FSprite[17 + numGodPips + tentacles.Length * 2];
			}
			else if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				sLeaser.sprites = new FSprite[13 + tailSpecks.numberOfSprites + bodyPearl.numberOfSprites];
			}
			else if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer && !player.DreamState)
			{
				sLeaser.sprites = new FSprite[14];
			}
			else
			{
				sLeaser.sprites = new FSprite[ModManager.MSC ? 13 : 12];
			}
			sLeaser.sprites[0] = new FSprite("BodyA");
			sLeaser.sprites[0].anchorY = 0.7894737f;
			if (RenderAsPup)
			{
				sLeaser.sprites[0].scaleY = 0.5f;
			}
			sLeaser.sprites[1] = new FSprite("HipsA");
			TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[13]
			{
				new TriangleMesh.Triangle(0, 1, 2),
				new TriangleMesh.Triangle(1, 2, 3),
				new TriangleMesh.Triangle(4, 5, 6),
				new TriangleMesh.Triangle(5, 6, 7),
				new TriangleMesh.Triangle(8, 9, 10),
				new TriangleMesh.Triangle(9, 10, 11),
				new TriangleMesh.Triangle(12, 13, 14),
				new TriangleMesh.Triangle(2, 3, 4),
				new TriangleMesh.Triangle(3, 4, 5),
				new TriangleMesh.Triangle(6, 7, 8),
				new TriangleMesh.Triangle(7, 8, 9),
				new TriangleMesh.Triangle(10, 11, 12),
				new TriangleMesh.Triangle(11, 12, 13)
			};
			TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, customColor: false);
			sLeaser.sprites[2] = triangleMesh;
			if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				sLeaser.sprites[3] = new FSprite("HeadB0");
			}
			else
			{
				sLeaser.sprites[3] = new FSprite("HeadA0");
			}
			sLeaser.sprites[4] = new FSprite("LegsA0");
			sLeaser.sprites[4].anchorY = 0.25f;
			sLeaser.sprites[5] = new FSprite("PlayerArm0");
			sLeaser.sprites[5].anchorX = 0.9f;
			sLeaser.sprites[5].scaleY = -1f;
			sLeaser.sprites[6] = new FSprite("PlayerArm0");
			sLeaser.sprites[6].anchorX = 0.9f;
			sLeaser.sprites[7] = new FSprite("OnTopOfTerrainHand");
			sLeaser.sprites[8] = new FSprite("OnTopOfTerrainHand");
			sLeaser.sprites[8].scaleX = -1f;
			sLeaser.sprites[9] = new FSprite("FaceA0");
			sLeaser.sprites[11] = new FSprite("pixel");
			sLeaser.sprites[11].scale = 5f;
			sLeaser.sprites[10] = new FSprite("Futile_White");
			sLeaser.sprites[10].shader = rCam.game.rainWorld.Shaders["FlatLight"];
			if (ModManager.MSC)
			{
				if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer && sLeaser.sprites.Length > 12)
				{
					sLeaser.sprites[12] = new FSprite("MushroomA");
				}
				if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
				{
					gills.InitiateSprites(sLeaser, rCam);
				}
				if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
				{
					tailSpecks.InitiateSprites(sLeaser, rCam);
					bodyPearl.InitiateSprites(sLeaser, rCam);
				}
				if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
				{
					sLeaser.sprites[12] = TriangleMesh.MakeLongMesh(ropeSegments.Length - 1, pointyTip: false, customColor: true);
					sLeaser.sprites[13] = new FSprite("Futile_White");
					sLeaser.sprites[13].shader = rCam.game.rainWorld.Shaders["FlatLight"];
					sLeaser.sprites[14] = new FSprite("guardEye");
					for (int i = 0; i < numGodPips; i++)
					{
						sLeaser.sprites[15 + i] = new FSprite("WormEye");
					}
					sLeaser.sprites[15 + numGodPips + tentacles.Length * 2] = new FSprite("Futile_White");
					sLeaser.sprites[15 + numGodPips + tentacles.Length * 2].shader = rCam.game.rainWorld.Shaders["GhostDistortion"];
					for (int j = 0; j < tentacles.Length; j++)
					{
						tentacles[j].InitiateSprites(sLeaser, rCam);
					}
				}
				if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer && player.dissolved > 0f)
				{
					for (int k = 0; k < sLeaser.sprites.Length; k++)
					{
						if (sLeaser.sprites[k] != null && sLeaser.sprites[k].shader == FShader.defaultShader)
						{
							sLeaser.sprites[k].shader = rCam.game.rainWorld.Shaders["Hologram"];
						}
					}
				}
				if (gown != null)
				{
					gownIndex = sLeaser.sprites.Length - 1;
					gown.InitiateSprite(gownIndex, sLeaser, rCam);
				}
			}
			AddToContainer(sLeaser, rCam, null);
		}
		else
		{
			sLeaser.sprites = new FSprite[2];
			for (int l = 0; l < 2; l++)
			{
				FSprite fSprite = new FSprite("pixel");
				sLeaser.sprites[l] = fSprite;
				rCam.ReturnFContainer("Midground").AddChild(fSprite);
				fSprite.x = -10000f;
				fSprite.color = new Color(1f, 0.7f, 1f);
				fSprite.scale = base.owner.bodyChunks[l].rad * 2f;
				fSprite.shader = FShader.Basic;
			}
		}
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (!rCam.room.game.DEBUGMODE)
		{
			float num = 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(lastBreath, breath, timeStacker) * (float)Math.PI * 2f);
			if ((malnourished > 0f && !player.Malnourished) || Mathf.Abs((base.owner as Player).abstractCreature.Hypothermia - currentAppliedHypothermia) > 0.01f)
			{
				ApplyPalette(sLeaser, rCam, rCam.currentPalette);
			}
			Vector2 vector = Vector2.Lerp(drawPositions[0, 1], drawPositions[0, 0], timeStacker);
			Vector2 vector2 = Vector2.Lerp(drawPositions[1, 1], drawPositions[1, 0], timeStacker);
			Vector2 vector3 = Vector2.Lerp(head.lastPos, head.pos, timeStacker);
			if (player.aerobicLevel > 0.5f)
			{
				vector += Custom.DirVec(vector2, vector) * Mathf.Lerp(-1f, 1f, num) * Mathf.InverseLerp(0.5f, 1f, player.aerobicLevel) * 0.5f;
				vector3 -= Custom.DirVec(vector2, vector) * Mathf.Lerp(-1f, 1f, num) * Mathf.Pow(Mathf.InverseLerp(0.5f, 1f, player.aerobicLevel), 1.5f) * 0.75f;
			}
			float num2 = Mathf.InverseLerp(0.3f, 0.5f, Mathf.Abs(Custom.DirVec(vector2, vector).y));
			sLeaser.sprites[0].x = vector.x - camPos.x;
			sLeaser.sprites[0].y = vector.y - camPos.y - player.sleepCurlUp * 4f + Mathf.Lerp(0.5f, 1f, player.aerobicLevel) * num * (1f - num2);
			sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(vector2, vector);
			if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				sLeaser.sprites[0].scaleX = 1.4f + Mathf.Lerp(Mathf.Lerp(Mathf.Lerp(-0.05f, -0.15f, malnourished), 0.05f, num) * num2, 0.15f, player.sleepCurlUp);
			}
			else if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				sLeaser.sprites[0].scaleX = 0.76f + Mathf.Lerp(Mathf.Lerp(Mathf.Lerp(-0.05f, -0.15f, malnourished), 0.05f, num) * num2, 0.15f, player.sleepCurlUp);
			}
			else if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
			{
				sLeaser.sprites[1].scaleX = 0.9f + 0.2f * Mathf.Lerp(player.npcStats.Wideness, 0.5f, player.playerState.isPup ? 0.5f : 0f) + Mathf.Lerp(Mathf.Lerp(Mathf.Lerp(-0.05f, -0.15f, malnourished), 0.05f, num) * num2, 0.15f, player.sleepCurlUp);
			}
			else
			{
				sLeaser.sprites[0].scaleX = 1f + Mathf.Lerp(Mathf.Lerp(Mathf.Lerp(-0.05f, -0.15f, malnourished), 0.05f, num) * num2, 0.15f, player.sleepCurlUp);
			}
			sLeaser.sprites[1].x = (vector2.x * 2f + vector.x) / 3f - camPos.x;
			sLeaser.sprites[1].y = (vector2.y * 2f + vector.y) / 3f - camPos.y - player.sleepCurlUp * 3f;
			sLeaser.sprites[1].rotation = Custom.AimFromOneVectorToAnother(vector, Vector2.Lerp(tail[0].lastPos, tail[0].pos, timeStacker));
			sLeaser.sprites[1].scaleY = 1f + player.sleepCurlUp * 0.2f;
			if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				sLeaser.sprites[1].scaleX = 1.6f + player.sleepCurlUp * 0.2f + 0.05f * num - 0.05f * malnourished;
			}
			else if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				sLeaser.sprites[1].scaleX = 0.76f + player.sleepCurlUp * 0.2f + 0.05f * num - 0.05f * malnourished;
			}
			else if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
			{
				sLeaser.sprites[0].scaleX = 0.9f + 0.2f * Mathf.Lerp(player.npcStats.Wideness, 0.5f, player.playerState.isPup ? 0.5f : 0f) + player.sleepCurlUp * 0.2f + 0.05f * num - 0.05f * malnourished;
			}
			else
			{
				sLeaser.sprites[1].scaleX = 1f + player.sleepCurlUp * 0.2f + 0.05f * num - 0.05f * malnourished;
			}
			Vector2 vector4 = (vector2 * 3f + vector) / 4f;
			float num3 = 1f - 0.2f * malnourished;
			float num4 = 6f;
			for (int i = 0; i < 4; i++)
			{
				Vector2 vector5 = Vector2.Lerp(tail[i].lastPos, tail[i].pos, timeStacker);
				Vector2 normalized = (vector5 - vector4).normalized;
				Vector2 vector6 = Custom.PerpendicularVector(normalized);
				float num5 = Vector2.Distance(vector5, vector4) / 5f;
				if (i == 0)
				{
					num5 = 0f;
				}
				(sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4, vector4 - vector6 * num4 * num3 + normalized * num5 - camPos);
				(sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4 + 1, vector4 + vector6 * num4 * num3 + normalized * num5 - camPos);
				if (i < 3)
				{
					(sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4 + 2, vector5 - vector6 * tail[i].StretchedRad * num3 - normalized * num5 - camPos);
					(sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4 + 3, vector5 + vector6 * tail[i].StretchedRad * num3 - normalized * num5 - camPos);
				}
				else
				{
					(sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4 + 2, vector5 - camPos);
				}
				num4 = tail[i].StretchedRad;
				vector4 = vector5;
			}
			float num6 = Custom.AimFromOneVectorToAnother(Vector2.Lerp(vector2, vector, 0.5f), vector3);
			int num7 = Mathf.RoundToInt(Mathf.Abs(num6 / 360f * 34f));
			if (player.sleepCurlUp > 0f)
			{
				num7 = 7;
				num7 = Custom.IntClamp((int)Mathf.Lerp(num7, 4f, player.sleepCurlUp), 0, 8);
			}
			Vector2 vector7 = Vector2.Lerp(lastLookDir, lookDirection, timeStacker) * 3f * (1f - player.sleepCurlUp);
			if (player.sleepCurlUp > 0f)
			{
				sLeaser.sprites[9].scaleX = Mathf.Sign(vector.x - vector2.x);
				sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName(DefaultFaceSprite(sLeaser.sprites[9].scaleX, Custom.IntClamp((int)Mathf.Lerp(num7, 1f, player.sleepCurlUp), 0, 8)));
				sLeaser.sprites[9].rotation = num6 * (1f - player.sleepCurlUp);
				num6 = Mathf.Lerp(num6, 45f * Mathf.Sign(vector.x - vector2.x), player.sleepCurlUp);
				vector3.y += 1f * player.sleepCurlUp;
				vector3.x += Mathf.Sign(vector.x - vector2.x) * 2f * player.sleepCurlUp;
				vector7.y -= 2f * player.sleepCurlUp;
				vector7.x -= 4f * Mathf.Sign(vector.x - vector2.x) * player.sleepCurlUp;
			}
			else if (base.owner.room != null && base.owner.EffectiveRoomGravity == 0f)
			{
				num7 = 0;
				sLeaser.sprites[9].rotation = num6;
				if (player.Consious)
				{
					sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName(DefaultFaceSprite(sLeaser.sprites[9].scaleX, 0));
				}
				else
				{
					sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName(player.dead ? "FaceDead" : "FaceStunned");
				}
			}
			else if (player.Consious)
			{
				if ((player.bodyMode == Player.BodyModeIndex.Stand && player.input[0].x != 0) || player.bodyMode == Player.BodyModeIndex.Crawl)
				{
					if (player.bodyMode == Player.BodyModeIndex.Crawl)
					{
						num7 = 7;
						sLeaser.sprites[9].scaleX = Mathf.Sign(vector.x - vector2.x);
					}
					else
					{
						num7 = 6;
						sLeaser.sprites[9].scaleX = ((num6 < 0f) ? (-1f) : 1f);
					}
					vector7.x = 0f;
					sLeaser.sprites[9].y += 1f;
					sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName(DefaultFaceSprite(sLeaser.sprites[9].scaleX, 4));
				}
				else
				{
					Vector2 vector8 = vector3 - vector2;
					vector8.x *= 1f - vector7.magnitude / 3f;
					vector8 = vector8.normalized;
					if (Mathf.Abs(vector7.x) < 0.1f)
					{
						sLeaser.sprites[9].scaleX = ((num6 < 0f) ? (-1f) : 1f);
					}
					else
					{
						sLeaser.sprites[9].scaleX = Mathf.Sign(vector7.x);
					}
					sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName(DefaultFaceSprite(sLeaser.sprites[9].scaleX, Mathf.RoundToInt(Mathf.Abs(Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), vector8) / 22.5f))));
				}
				sLeaser.sprites[9].rotation = 0f;
			}
			else
			{
				vector7 *= 0f;
				num7 = 0;
				sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName(player.dead ? "FaceDead" : "FaceStunned");
				sLeaser.sprites[9].rotation = num6;
			}
			if (ModManager.CoopAvailable && player.bool1)
			{
				sLeaser.sprites[0].scaleX += 0.35f;
				sLeaser.sprites[1].rotation += 0.1f;
				sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName(DefaultFaceSprite(sLeaser.sprites[9].scaleX, 5));
				sLeaser.sprites[9].rotation = num6 + 0.2f;
				vector3.y -= 1.9f;
				num6 = Mathf.Lerp(num6, 45f * Mathf.Sign(vector.x - vector2.x), 0.7f);
				vector7.x -= 0.2f;
			}
			sLeaser.sprites[3].x = vector3.x - camPos.x;
			sLeaser.sprites[3].y = vector3.y - camPos.y;
			sLeaser.sprites[3].rotation = num6;
			sLeaser.sprites[3].scaleX = ((num6 < 0f) ? (-1f) : 1f);
			if (RenderAsPup && ModManager.MSC)
			{
				sLeaser.sprites[3].element = Futile.atlasManager.GetElementWithName(_cachedHeads[2, num7]);
			}
			else if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				sLeaser.sprites[3].element = Futile.atlasManager.GetElementWithName(_cachedHeads[1, num7]);
			}
			else
			{
				sLeaser.sprites[3].element = Futile.atlasManager.GetElementWithName(_cachedHeads[0, num7]);
			}
			if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				sLeaser.sprites[3].scaleX *= 0.85f;
			}
			else if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
			{
				sLeaser.sprites[3].scaleX *= 0.9f + 0.2f * Mathf.Lerp(player.npcStats.Wideness, 0.5f, player.playerState.isPup ? 0.5f : 0f);
			}
			sLeaser.sprites[9].x = vector3.x + vector7.x - camPos.x;
			sLeaser.sprites[9].y = vector3.y + vector7.y - 2f - camPos.y;
			Vector2 vector9 = Vector2.Lerp(legs.lastPos, legs.pos, timeStacker);
			sLeaser.sprites[4].x = vector9.x - camPos.x;
			sLeaser.sprites[4].y = vector9.y - camPos.y;
			sLeaser.sprites[4].rotation = Custom.AimFromOneVectorToAnother(legsDirection, new Vector2(0f, 0f));
			sLeaser.sprites[4].isVisible = true;
			string elementName = "LegsAAir0";
			if (player.bodyMode == Player.BodyModeIndex.Stand)
			{
				elementName = _cachedLegsA[player.animationFrame];
				sLeaser.sprites[4].scaleX = ((player.flipDirection > 0) ? 1 : (-1));
			}
			else if (player.bodyMode == Player.BodyModeIndex.Crawl)
			{
				elementName = _cachedLegsACrawling[player.animationFrame / 2];
				sLeaser.sprites[4].scaleX = ((player.flipDirection > 0) ? 1 : (-1));
			}
			else if (player.bodyMode == Player.BodyModeIndex.CorridorClimb)
			{
				int num8 = player.animationFrame;
				if (num8 > 6)
				{
					num8 %= 6;
					sLeaser.sprites[4].scaleX = -1f;
				}
				else
				{
					sLeaser.sprites[4].scaleX = 1f;
				}
				elementName = _cachedLegsAClimbing[num8];
			}
			else if (player.bodyMode == Player.BodyModeIndex.ClimbingOnBeam)
			{
				if (player.animation == Player.AnimationIndex.StandOnBeam)
				{
					elementName = "LegsAOnPole" + ((player.animationFrame < 7) ? player.animationFrame : 0);
					sLeaser.sprites[4].scaleX = ((player.flipDirection > 0) ? 1 : (-1));
				}
				else if (player.animation == Player.AnimationIndex.BeamTip)
				{
					elementName = "LegsAPole";
				}
				else if (player.animation == Player.AnimationIndex.ClimbOnBeam)
				{
					elementName = "LegsAVerticalPole";
					sLeaser.sprites[4].scaleX = ((player.flipDirection > 0) ? 1 : (-1));
					sLeaser.sprites[4].y = Mathf.Clamp(sLeaser.sprites[4].y, vector2.y - 6f - camPos.y, vector2.y + 4f - camPos.y);
					sLeaser.sprites[7].element = Futile.atlasManager.GetElementWithName("OnTopOfTerrainHand");
					sLeaser.sprites[8].element = Futile.atlasManager.GetElementWithName("OnTopOfTerrainHand");
				}
				else if (player.animation == Player.AnimationIndex.HangFromBeam)
				{
					sLeaser.sprites[7].element = Futile.atlasManager.GetElementWithName("OnTopOfTerrainHand2");
					sLeaser.sprites[8].element = Futile.atlasManager.GetElementWithName("OnTopOfTerrainHand2");
				}
			}
			else if (player.bodyMode == Player.BodyModeIndex.WallClimb)
			{
				elementName = "LegsAWall";
				sLeaser.sprites[4].scaleX = ((player.flipDirection > 0) ? 1 : (-1));
			}
			else if (player.bodyMode == Player.BodyModeIndex.Default)
			{
				if (player.animation == Player.AnimationIndex.LedgeGrab)
				{
					elementName = "LegsAWall";
					sLeaser.sprites[4].scaleX = ((player.flipDirection > 0) ? 1 : (-1));
				}
			}
			else if (player.bodyMode == Player.BodyModeIndex.Swimming)
			{
				if (player.animation == Player.AnimationIndex.DeepSwim)
				{
					sLeaser.sprites[4].isVisible = false;
				}
			}
			else if (player.bodyMode == Player.BodyModeIndex.ZeroG && player.animation == Player.AnimationIndex.ZeroGPoleGrab)
			{
				sLeaser.sprites[7].element = Futile.atlasManager.GetElementWithName("OnTopOfTerrainHand");
				sLeaser.sprites[8].element = Futile.atlasManager.GetElementWithName("OnTopOfTerrainHand");
			}
			sLeaser.sprites[4].element = Futile.atlasManager.GetElementWithName(elementName);
			for (int j = 0; j < 2; j++)
			{
				Vector2 vector10 = Vector2.Lerp(hands[j].lastPos, hands[j].pos, timeStacker);
				if (hands[j].mode != Limb.Mode.Retracted)
				{
					sLeaser.sprites[5 + j].x = vector10.x - camPos.x;
					sLeaser.sprites[5 + j].y = vector10.y - camPos.y;
					float num9 = 4.5f / ((float)hands[j].retractCounter + 1f);
					if ((player.animation == Player.AnimationIndex.StandOnBeam || player.animation == Player.AnimationIndex.BeamTip) && disbalanceAmount <= 40f && hands[j].mode == Limb.Mode.HuntRelativePosition)
					{
						num9 *= disbalanceAmount / 40f;
					}
					if (player.animation == Player.AnimationIndex.HangFromBeam)
					{
						num9 *= 0.5f;
					}
					num9 *= Mathf.Abs(Mathf.Cos(Custom.AimFromOneVectorToAnother(vector2, vector) / 360f * (float)Math.PI * 2f));
					Vector2 vector11 = ((!ModManager.MSC || !(player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)) ? (vector + Custom.RotateAroundOrigo(new Vector2((-1f + 2f * (float)j) * num9, -3.5f), Custom.AimFromOneVectorToAnother(vector2, vector))) : (vector + Custom.RotateAroundOrigo(new Vector2((-1f + 2f * (float)j) * (num9 * 0.6f), -3.5f), Custom.AimFromOneVectorToAnother(vector2, vector))));
					sLeaser.sprites[5 + j].element = Futile.atlasManager.GetElementWithName(_cachedPlayerArms[Mathf.RoundToInt(Mathf.Clamp(Vector2.Distance(vector10, vector11) / 2f, 0f, 12f))]);
					sLeaser.sprites[5 + j].rotation = Custom.AimFromOneVectorToAnother(vector10, vector11) + 90f;
					if (player.bodyMode == Player.BodyModeIndex.Crawl)
					{
						sLeaser.sprites[5 + j].scaleY = ((vector.x < vector2.x) ? (-1f) : 1f);
					}
					else if (player.bodyMode == Player.BodyModeIndex.WallClimb)
					{
						sLeaser.sprites[5 + j].scaleY = ((player.flipDirection == -1) ? (-1f) : 1f);
					}
					else
					{
						sLeaser.sprites[5 + j].scaleY = Mathf.Sign(Custom.DistanceToLine(vector10, vector, vector2));
					}
					if (player.animation == Player.AnimationIndex.HangUnderVerticalBeam)
					{
						sLeaser.sprites[5 + j].scaleY = ((j == 0) ? 1f : (-1f));
					}
				}
				sLeaser.sprites[5 + j].isVisible = hands[j].mode != Limb.Mode.Retracted && ((player.animation != Player.AnimationIndex.ClimbOnBeam && player.animation != Player.AnimationIndex.ZeroGPoleGrab) || !hands[j].reachedSnapPosition);
				if ((player.animation == Player.AnimationIndex.ClimbOnBeam || player.animation == Player.AnimationIndex.HangFromBeam || player.animation == Player.AnimationIndex.GetUpOnBeam || player.animation == Player.AnimationIndex.ZeroGPoleGrab) && hands[j].reachedSnapPosition)
				{
					sLeaser.sprites[7 + j].x = vector10.x - camPos.x;
					sLeaser.sprites[7 + j].y = vector10.y - camPos.y + ((player.animation != Player.AnimationIndex.ClimbOnBeam && player.animation != Player.AnimationIndex.ZeroGPoleGrab) ? 3f : 0f);
					sLeaser.sprites[7 + j].isVisible = true;
				}
				else
				{
					sLeaser.sprites[7 + j].isVisible = false;
				}
			}
			Vector2 vector12 = vector3 + Custom.DirVec(vector2, vector3) * 30f + new Vector2(0f, 30f);
			sLeaser.sprites[11].x = vector12.x - camPos.x;
			sLeaser.sprites[11].y = vector12.y - camPos.y;
			sLeaser.sprites[11].alpha = Mathf.Lerp(lastMarkAlpha, markAlpha, timeStacker);
			sLeaser.sprites[10].x = vector12.x - camPos.x;
			sLeaser.sprites[10].y = vector12.y - camPos.y;
			sLeaser.sprites[10].alpha = 0.2f * Mathf.Lerp(lastMarkAlpha, markAlpha, timeStacker);
			sLeaser.sprites[10].scale = 1f + Mathf.Lerp(lastMarkAlpha, markAlpha, timeStacker);
			if (ModManager.MSC)
			{
				if (player.room != null && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
				{
					gills.DrawSprites(sLeaser, rCam, timeStacker, camPos);
				}
				if (player.room != null && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
				{
					tailSpecks.DrawSprites(sLeaser, rCam, timeStacker, camPos);
					bodyPearl.DrawSprites(sLeaser, rCam, timeStacker, camPos);
				}
				if (player.room != null && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
				{
					float b = Mathf.Lerp(lastStretch, stretch, timeStacker);
					vector = Vector2.Lerp(ropeSegments[0].lastPos, ropeSegments[0].pos, timeStacker);
					vector += Custom.DirVec(Vector2.Lerp(ropeSegments[1].lastPos, ropeSegments[1].pos, timeStacker), vector) * 1f;
					float num10 = 0f;
					for (int k = 1; k < ropeSegments.Length; k++)
					{
						float num11 = (float)k / (float)(ropeSegments.Length - 1);
						vector2 = ((k < ropeSegments.Length - 2) ? Vector2.Lerp(ropeSegments[k].lastPos, ropeSegments[k].pos, timeStacker) : new Vector2(sLeaser.sprites[9].x + camPos.x, sLeaser.sprites[9].y + camPos.y));
						Vector2 vector13 = Custom.PerpendicularVector((vector - vector2).normalized);
						float num12 = 0.2f + 1.6f * Mathf.Lerp(1f, b, Mathf.Pow(Mathf.Sin(num11 * (float)Math.PI), 0.7f));
						Vector2 vector14 = vector - vector13 * num12;
						Vector2 vector15 = vector2 + vector13 * num12;
						float num13 = Mathf.Sqrt(Mathf.Pow(vector14.x - vector15.x, 2f) + Mathf.Pow(vector14.y - vector15.y, 2f));
						if (!float.IsNaN(num13))
						{
							num10 += num13;
						}
						(sLeaser.sprites[12] as TriangleMesh).MoveVertice((k - 1) * 4, vector14 - camPos);
						(sLeaser.sprites[12] as TriangleMesh).MoveVertice((k - 1) * 4 + 1, vector + vector13 * num12 - camPos);
						(sLeaser.sprites[12] as TriangleMesh).MoveVertice((k - 1) * 4 + 2, vector2 - vector13 * num12 - camPos);
						(sLeaser.sprites[12] as TriangleMesh).MoveVertice((k - 1) * 4 + 3, vector15 - camPos);
						vector = vector2;
					}
					if (player.tongue.Free || player.tongue.Attached)
					{
						sLeaser.sprites[12].isVisible = true;
					}
					else
					{
						sLeaser.sprites[12].isVisible = false;
					}
				}
				if (player.room != null && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
				{
					if (rCam.room.game.setupValues.arenaDefaultColors || player.playerState.playerNumber >= 2 || !rCam.room.game.IsArenaSession || rCam.room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
					{
						sLeaser.sprites[9].color = Color.white;
					}
					if (sLeaser.sprites.Length > 12)
					{
						sLeaser.sprites[12].rotation = sLeaser.sprites[9].rotation;
						sLeaser.sprites[12].scaleX = 1f;
						if (player.animation == Player.AnimationIndex.Flip)
						{
							Vector2 vector16 = Custom.DegToVec(sLeaser.sprites[9].rotation) * 4f;
							sLeaser.sprites[12].x = sLeaser.sprites[9].x + vector16.x;
							sLeaser.sprites[12].y = sLeaser.sprites[9].y + vector16.y;
						}
						else
						{
							int num14 = 0;
							string name = sLeaser.sprites[9].element.name;
							if (name[name.Length - 2] == 'C')
							{
								num14 = int.Parse(name[name.Length - 1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
								sLeaser.sprites[12].scaleX = 1f - (float)num14 / 8f;
								sLeaser.sprites[12].x = sLeaser.sprites[9].x + 3f + 4f * ((float)num14 / 8f);
							}
							else if (name[name.Length - 2] == 'D')
							{
								num14 = int.Parse(name[name.Length - 1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
								sLeaser.sprites[12].x = sLeaser.sprites[9].x + 3f * (1f - (float)num14 / 8f);
							}
							else
							{
								sLeaser.sprites[12].x = sLeaser.sprites[9].x + 3f * (1f - (float)num14 / 8f);
							}
							sLeaser.sprites[12].y = sLeaser.sprites[9].y + 3f;
						}
					}
				}
				Color color = rCam.currentPalette.blackColor;
				if ((rCam.room.game.setupValues.arenaDefaultColors || rCam.room.game.IsStorySession || (rCam.room.game.IsArenaSession && rCam.room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)) && !CustomColorsEnabled())
				{
					if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel || player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer || player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
					{
						color = Color.Lerp(new Color(1f, 1f, 1f), color, 0.3f);
					}
				}
				else if (rCam.room.game.IsArenaSession && !rCam.room.game.setupValues.arenaDefaultColors && (player.playerState.playerNumber == 3 || (player.playerState.playerNumber == 2 && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)))
				{
					color = Color.Lerp(new Color(1f, 1f, 1f), color, 0.3f);
				}
				if (useJollyColor)
				{
					color = JollyColor(player.playerState.playerNumber, 1);
				}
				if (CustomColorsEnabled())
				{
					color = CustomColorSafety(1);
				}
				if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
				{
					color = ((base.owner.abstractPhysicalObject.ID.RandomSeed != 1000 && base.owner.abstractPhysicalObject.ID.RandomSeed != 1001) ? Color.Lerp((!player.npcStats.Dark) ? rCam.currentPalette.blackColor : (new Color(1f, 1f, 1f, 2f) - rCam.currentPalette.blackColor), new Color(1f - color.r, 1f - color.b, 1f - color.g), player.npcStats.EyeColor * 0.25f) : rCam.currentPalette.blackColor);
				}
				if (malnourished > 0f)
				{
					float num15 = (player.Malnourished ? malnourished : Mathf.Max(0f, malnourished - 0.005f));
					color = Color.Lerp(color, Color.Lerp(Color.white, rCam.currentPalette.fogColor, 0.5f), 0.2f * num15 * num15);
				}
				if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup && player.room != null && player.room.world.game.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
				{
					color = Color.red;
				}
				sLeaser.sprites[9].color = color;
				if (player.room != null && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
				{
					if (darkenFactor <= 0f)
					{
						sLeaser.sprites[15 + numGodPips + tentacles.Length * 2].isVisible = false;
					}
					else
					{
						sLeaser.sprites[15 + numGodPips + tentacles.Length * 2].isVisible = true;
						sLeaser.sprites[15 + numGodPips + tentacles.Length * 2].x = sLeaser.sprites[1].x;
						sLeaser.sprites[15 + numGodPips + tentacles.Length * 2].y = sLeaser.sprites[1].y;
						sLeaser.sprites[15 + numGodPips + tentacles.Length * 2].scale = 15f * darkenFactor;
					}
					for (int l = 0; l < tentacles.Length; l++)
					{
						if (l < tentaclesVisible)
						{
							tentacles[l].DrawSprites(sLeaser, rCam, timeStacker, camPos);
						}
						else
						{
							tentacles[l].InactiveDrawSprites(sLeaser, rCam, timeStacker, camPos);
						}
					}
					if (player.killFac > 0f || player.forceBurst)
					{
						sLeaser.sprites[13].isVisible = true;
						sLeaser.sprites[13].x = sLeaser.sprites[3].x + player.burstX;
						sLeaser.sprites[13].y = sLeaser.sprites[3].y + player.burstY + 60f;
						float f = Mathf.Lerp(player.lastKillFac, player.killFac, timeStacker);
						sLeaser.sprites[13].scale = Mathf.Lerp(50f, 2f, Mathf.Pow(f, 0.5f));
						sLeaser.sprites[13].alpha = Mathf.Pow(f, 3f);
					}
					else
					{
						sLeaser.sprites[13].isVisible = false;
					}
					if (player.killWait > player.lastKillWait || player.killWait == 1f || player.forceBurst)
					{
						rubberMouseX += (player.burstX - rubberMouseX) * 0.3f;
						rubberMouseY += (player.burstY - rubberMouseY) * 0.3f;
					}
					else
					{
						rubberMouseX *= 0.15f;
						rubberMouseY *= 0.25f;
					}
					if (Mathf.Sqrt(Mathf.Pow(sLeaser.sprites[3].x - rubberMarkX, 2f) + Mathf.Pow(sLeaser.sprites[3].y - rubberMarkY, 2f)) > 100f)
					{
						rubberMarkX = sLeaser.sprites[3].x;
						rubberMarkY = sLeaser.sprites[3].y;
					}
					else
					{
						rubberMarkX += (sLeaser.sprites[3].x - rubberMarkX) * 0.15f;
						rubberMarkY += (sLeaser.sprites[3].y - rubberMarkY) * 0.25f;
					}
					sLeaser.sprites[14].x = rubberMarkX;
					sLeaser.sprites[14].y = rubberMarkY + 60f;
					float num16;
					if (player.monkAscension)
					{
						sLeaser.sprites[9].color = Custom.HSL2RGB(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
						sLeaser.sprites[10].alpha = 0f;
						sLeaser.sprites[11].alpha = 0f;
						sLeaser.sprites[14].color = sLeaser.sprites[9].color;
						num16 = 1f;
					}
					else
					{
						num16 = 0f;
					}
					float num17;
					if ((player.godTimer < player.maxGodTime || player.monkAscension) && !player.hideGodPips)
					{
						num17 = 1f;
						float num18 = 15f;
						if (!player.monkAscension)
						{
							num18 = 6f;
						}
						rubberRadius += (num18 - rubberRadius) * 0.045f;
						if (rubberRadius < 5f)
						{
							rubberRadius = num18;
						}
						float num19 = player.maxGodTime / (float)numGodPips;
						for (int m = 0; m < numGodPips; m++)
						{
							float num20 = num19 * (float)m;
							float num21 = num19 * (float)(m + 1);
							if (player.godTimer <= num20)
							{
								sLeaser.sprites[15 + m].scale = 0f;
							}
							else if (player.godTimer >= num21)
							{
								sLeaser.sprites[15 + m].scale = 1f;
							}
							else
							{
								sLeaser.sprites[15 + m].scale = (player.godTimer - num20) / num19;
							}
							if (player.karmaCharging > 0 && player.monkAscension)
							{
								sLeaser.sprites[15 + m].color = sLeaser.sprites[9].color;
							}
							else
							{
								sLeaser.sprites[15 + m].color = SlugcatColor(CharacterForColor);
							}
						}
					}
					else
					{
						num17 = 0f;
					}
					sLeaser.sprites[14].x = rubberMarkX + rubberMouseX;
					sLeaser.sprites[14].y = rubberMarkY + 60f + rubberMouseY;
					rubberAlphaEmblem += (num16 - rubberAlphaEmblem) * 0.05f;
					rubberAlphaPips += (num17 - rubberAlphaPips) * 0.05f;
					sLeaser.sprites[14].alpha = rubberAlphaEmblem;
					sLeaser.sprites[10].alpha *= 1f - rubberAlphaPips;
					sLeaser.sprites[11].alpha *= 1f - rubberAlphaPips;
					for (int n = 15; n < 15 + numGodPips; n++)
					{
						sLeaser.sprites[n].alpha = rubberAlphaPips;
						Vector2 vector17 = new Vector2(sLeaser.sprites[14].x, sLeaser.sprites[14].y);
						vector17 += Custom.rotateVectorDeg(Vector2.one * rubberRadius, (float)(n - 15) * (360f / (float)numGodPips));
						sLeaser.sprites[n].x = vector17.x;
						sLeaser.sprites[n].y = vector17.y;
					}
				}
				if (gown != null)
				{
					gown.DrawSprite(gownIndex, sLeaser, rCam, timeStacker, camPos);
				}
			}
			else
			{
				Color color2 = rCam.currentPalette.blackColor;
				if (useJollyColor)
				{
					color2 = JollyColor(player.playerState.playerNumber, 1);
				}
				if (ModManager.MMF && CustomColorsEnabled())
				{
					color2 = CustomColorSafety(1);
				}
				if (malnourished > 0f)
				{
					float num22 = (player.Malnourished ? malnourished : Mathf.Max(0f, malnourished - 0.005f));
					color2 = Color.Lerp(color2, Color.Lerp(Color.white, rCam.currentPalette.fogColor, 0.5f), 0.2f * num22 * num22);
				}
				if (CharacterForColor == SlugcatStats.Name.Night)
				{
					Color color3 = SlugcatColor(CharacterForColor);
					Color color4 = new Color(color3.r, color3.g, color3.b);
					if (malnourished > 0f)
					{
						color4 = Color.Lerp(color4, Color.gray, 0.4f * (player.Malnourished ? malnourished : Mathf.Max(0f, malnourished - 0.005f)));
					}
					color2 = Color.Lerp(new Color(1f, 1f, 1f), color4, 0.3f);
				}
				sLeaser.sprites[9].color = color2;
			}
		}
		else
		{
			for (int num23 = 0; num23 < 2; num23++)
			{
				Vector2 vector18 = Vector2.Lerp(base.owner.bodyChunks[num23].lastPos, base.owner.bodyChunks[num23].pos, timeStacker);
				sLeaser.sprites[num23].x = vector18.x - camPos.x;
				sLeaser.sprites[num23].y = vector18.y - camPos.y;
			}
		}
		if (ModManager.MSC)
		{
			if (darkenFactor > 0f)
			{
				for (int num24 = 0; num24 < sLeaser.sprites.Length; num24++)
				{
					if (num24 != 9)
					{
						Color color5 = ((player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup) ? player.ShortCutColor() : SlugcatColor(CharacterForColor));
						sLeaser.sprites[num24].color = new Color(Mathf.Min(1f, color5.r * (1f - darkenFactor) + 0.01f), Mathf.Min(1f, color5.g * (1f - darkenFactor) + 0.01f), Mathf.Min(1f, color5.b * (1f - darkenFactor) + 0.01f));
					}
				}
			}
			if (player.dissolved > 0f)
			{
				for (int num25 = 0; num25 < sLeaser.sprites.Length; num25++)
				{
					if (sLeaser.sprites[num25].alpha > 0f)
					{
						sLeaser.sprites[num25].alpha = 1f - player.dissolved;
					}
				}
			}
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Color color = ((ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup) ? player.ShortCutColor() : SlugcatColor(CharacterForColor));
		Color color2 = new Color(color.r, color.g, color.b);
		if (malnourished > 0f)
		{
			float num = (player.Malnourished ? malnourished : Mathf.Max(0f, malnourished - 0.005f));
			color2 = Color.Lerp(color2, Color.gray, 0.4f * num);
		}
		if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup && rCam.room.world.game.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
		{
			color2 = new Color(0.01f, 0.01f, 0.01f);
		}
		if (CharacterForColor == SlugcatStats.Name.Night)
		{
			color2 = Color.Lerp(palette.blackColor, Custom.HSL2RGB(0.63055557f, 0.54f, 0.5f), Mathf.Lerp(0.08f, 0.04f, palette.darkness));
		}
		color2 = HypothermiaColorBlend(color2);
		currentAppliedHypothermia = player.Hypothermia;
		if (ModManager.MMF && (base.owner as Player).AI == null)
		{
			RainWorld.PlayerObjectBodyColors[player.playerState.playerNumber] = color2;
		}
		if (gills != null)
		{
			Color effectCol = new Color(0.87451f, 0.17647f, 0.91765f);
			if (!rCam.room.game.setupValues.arenaDefaultColors && !ModManager.CoopAvailable)
			{
				switch (player.playerState.playerNumber)
				{
				case 0:
					if (rCam.room.game.IsArenaSession && rCam.room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType != MoreSlugcatsEnums.GameTypeID.Challenge)
					{
						effectCol = new Color(0.25f, 0.65f, 0.82f);
					}
					break;
				case 1:
					effectCol = new Color(0.31f, 0.73f, 0.26f);
					break;
				case 2:
					effectCol = new Color(0.6f, 0.16f, 0.6f);
					break;
				case 3:
					effectCol = new Color(0.96f, 0.75f, 0.95f);
					break;
				}
			}
			gills.SetGillColors(color2, effectCol);
			gills.ApplyPalette(sLeaser, rCam, palette);
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			if (i != 9)
			{
				sLeaser.sprites[i].color = color2;
			}
		}
		sLeaser.sprites[11].color = Color.Lerp(color, Color.white, 0.3f);
		sLeaser.sprites[10].color = color;
		if (!ModManager.MSC)
		{
			return;
		}
		if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer && sLeaser.sprites.Length > 12)
		{
			if (ModManager.CoopAvailable && useJollyColor)
			{
				sLeaser.sprites[12].color = JollyColor(player.playerState.playerNumber, 2);
			}
			else if (CustomColorsEnabled())
			{
				sLeaser.sprites[12].color = CustomColorSafety(2);
			}
			else
			{
				sLeaser.sprites[12].color = new Color(0.27059f, 0.15686f, 0.23529f);
				if (rCam.room.game.IsArenaSession && !rCam.room.game.setupValues.arenaDefaultColors)
				{
					switch (player.playerState.playerNumber)
					{
					case 0:
						if (rCam.room.game.IsArenaSession && rCam.room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType != MoreSlugcatsEnums.GameTypeID.Challenge)
						{
							sLeaser.sprites[12].color = new Color(0.651f, 0.5569f, 0.5922f);
						}
						break;
					case 1:
						sLeaser.sprites[12].color = Color.Lerp(color2, new Color(0.5647f, 0.3451f, 0.0118f), 0.5f);
						break;
					case 2:
						sLeaser.sprites[12].color = new Color(0.4353f, 0.302f, 0.4f);
						break;
					case 3:
						sLeaser.sprites[12].color = new Color(0.098f, 0.0314f, 0.1765f);
						break;
					}
				}
			}
		}
		if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			bodyPearl.ApplyPalette(sLeaser, rCam, palette);
		}
		if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			float a = 0.95f;
			float b = 1f;
			float sl = 1f;
			float a2 = 0.75f;
			float b2 = 0.9f;
			if (CustomColorsEnabled() || useJollyColor)
			{
				Vector3 one = Vector3.one;
				if (useJollyColor)
				{
					Color color3 = JollyColor(player.playerState.playerNumber, 2);
					one = new Vector3(color3.r, color3.g, color3.b);
				}
				else
				{
					one = Custom.RGB2HSL(CustomColorSafety(2));
				}
				if ((double)one.x > 0.95)
				{
					b = one.x;
					a = one.x - 0.05f;
				}
				else
				{
					a = one.x;
					b = one.x + 0.05f;
				}
				sl = one.y;
				if ((double)one.z > 0.85)
				{
					b2 = one.z;
					a2 = one.z - 0.15f;
				}
				else
				{
					a2 = one.z;
					b2 = one.z + 0.15f;
				}
			}
			for (int j = 0; j < (sLeaser.sprites[12] as TriangleMesh).verticeColors.Length; j++)
			{
				float num2 = Mathf.Clamp(Mathf.Sin((float)j / (float)((sLeaser.sprites[12] as TriangleMesh).verticeColors.Length - 1) * (float)Math.PI), 0f, 1f);
				if (useJollyColor || CustomColorsEnabled())
				{
					(sLeaser.sprites[12] as TriangleMesh).verticeColors[j] = Color.Lerp(palette.fogColor, JollyColor(player.playerState.playerNumber, 2), 0.7f);
				}
				else
				{
					(sLeaser.sprites[12] as TriangleMesh).verticeColors[j] = Color.Lerp(palette.fogColor, Custom.HSL2RGB(Mathf.Lerp(a, b, num2), sl, Mathf.Lerp(a2, b2, Mathf.Pow(num2, 0.15f))), 0.7f);
				}
			}
		}
		if (gown != null)
		{
			gown.ApplyPalette(gownIndex, sLeaser, rCam, palette);
		}
	}

	public static Color SlugcatColor(SlugcatStats.Name i)
	{
		if (ModManager.CoopAvailable)
		{
			int num = 0;
			if (i == JollyEnums.Name.JollyPlayer2)
			{
				num = 1;
			}
			if (i == JollyEnums.Name.JollyPlayer3)
			{
				num = 2;
			}
			if (i == JollyEnums.Name.JollyPlayer4)
			{
				num = 3;
			}
			if ((Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO && num > 0) || Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.CUSTOM)
			{
				return JollyColor(num, 0);
			}
			i = Custom.rainWorld.options.jollyPlayerOptionsArray[num].playerClass ?? i;
		}
		if (CustomColorsEnabled())
		{
			return CustomColorSafety(0);
		}
		return DefaultSlugcatColor(i);
	}

	public static Color DefaultSlugcatColor(SlugcatStats.Name i)
	{
		if (i == SlugcatStats.Name.White)
		{
			return new Color(1f, 1f, 1f);
		}
		if (i == SlugcatStats.Name.Yellow)
		{
			return new Color(1f, 1f, 23f / 51f);
		}
		if (i == SlugcatStats.Name.Red)
		{
			return new Color(1f, 23f / 51f, 23f / 51f);
		}
		if (i == SlugcatStats.Name.Night)
		{
			return Custom.HSL2RGB(0.63055557f, 0.54f, 0.2f);
		}
		if (ModManager.MSC)
		{
			if (i == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
			{
				return new Color(0.09f, 0.14f, 0.31f);
			}
			if (i == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				return new Color(0.56863f, 0.8f, 0.94118f);
			}
			if (i == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				return new Color(0.43922f, 0.13725f, 0.23529f);
			}
			if (i == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				return new Color(0.66667f, 0.9451f, 0.33725f);
			}
			if (i == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				return new Color(0.31f, 0.18f, 0.41f);
			}
			if (i == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				return new Color(0.94118f, 0.75686f, 0.59216f);
			}
		}
		return new Color(1f, 1f, 1f);
	}

	public static List<string> DefaultBodyPartColorHex(SlugcatStats.Name slugcatID)
	{
		List<string> list = new List<string>();
		Color col = DefaultSlugcatColor(slugcatID);
		list.Add(Custom.colorToHex(col));
		if (ModManager.MSC && (slugcatID == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel || slugcatID == MoreSlugcatsEnums.SlugcatStatsName.Artificer || slugcatID == MoreSlugcatsEnums.SlugcatStatsName.Spear))
		{
			list.Add("FFFFFF");
		}
		else
		{
			list.Add("101010");
		}
		if (ModManager.MSC && slugcatID == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
		{
			list.Add("DF2DEA");
		}
		if (ModManager.MSC && slugcatID == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			list.Add("45283C");
		}
		if (ModManager.MSC && slugcatID == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			list.Add("FF80A6");
		}
		if (ModManager.MSC && slugcatID == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			list.Add("FFFFFF");
		}
		return list;
	}

	public static List<string> ColoredBodyPartList(SlugcatStats.Name slugcatID)
	{
		List<string> list = new List<string>();
		list.Add("Body");
		list.Add("Eyes");
		if (ModManager.MSC && slugcatID == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
		{
			list.Add("Gills");
		}
		if (ModManager.MSC && slugcatID == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			list.Add("Scar");
		}
		if (ModManager.MSC && slugcatID == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			list.Add("Tongue");
		}
		if (ModManager.MSC && slugcatID == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			list.Add("Spears");
		}
		return list;
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			if (ModManager.MSC && i == gownIndex)
			{
				newContatiner = rCam.ReturnFContainer("Items");
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
			else if (ModManager.MSC)
			{
				if ((i < 13 || player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint) && (i != 12 || player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Artificer) && (i < 12 || player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Spear))
				{
					if (i == 9 && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer && sLeaser.sprites.Length > 12)
					{
						newContatiner.AddChild(sLeaser.sprites[12]);
					}
					if (i == 2 && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
					{
						bodyPearl.AddToContainer(sLeaser, rCam, newContatiner);
					}
					if (i == 3 && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
					{
						tailSpecks.AddToContainer(sLeaser, rCam, newContatiner);
					}
					if ((i <= 6 || i >= 9) && i <= 9)
					{
						newContatiner.AddChild(sLeaser.sprites[i]);
					}
					else
					{
						rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[i]);
					}
				}
			}
			else if ((i > 6 && i < 9) || i > 9)
			{
				rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[i]);
			}
			else
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
		if (!ModManager.MSC)
		{
			return;
		}
		if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
		{
			gills.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Midground"));
		}
		if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[12]);
			for (int j = 13; j < 15 + numGodPips; j++)
			{
				rCam.ReturnFContainer("HUD2").AddChild(sLeaser.sprites[j]);
			}
			rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[15 + numGodPips + tentacles.Length * 2]);
			for (int k = 0; k < tentacles.Length; k++)
			{
				tentacles[k].AddToContainer(sLeaser, rCam, newContatiner);
			}
		}
	}

	public void LookAtObject(PhysicalObject obj)
	{
		objectLooker.LookAtObject(obj);
	}

	public void LookAtPoint(Vector2 point, float interest)
	{
		objectLooker.LookAtPoint(point, interest);
	}

	public void LookAtNothing()
	{
		objectLooker.LookAtNothing();
	}

	public void ThrowObject(int hand, PhysicalObject obj)
	{
		throwCounter = 5;
		thrownObject = obj;
		handEngagedInThrowing = hand;
	}

	public static void PopulateJollyColorArray(SlugcatStats.Name reference)
	{
		jollyColors = new Color?[4][];
		JollyCustom.Log("Initializing colors... reference " + reference);
		for (int i = 0; i < jollyColors.Length; i++)
		{
			jollyColors[i] = new Color?[3];
			if (Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.CUSTOM)
			{
				LoadJollyColorsFromOptions(i);
			}
			else
			{
				if (!(Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO))
				{
					continue;
				}
				JollyCustom.Log("Need to generate colors for player " + i);
				if (i == 0)
				{
					List<string> list = DefaultBodyPartColorHex(reference);
					jollyColors[0][0] = Color.white;
					jollyColors[0][1] = Color.black;
					jollyColors[0][2] = Color.green;
					if (list.Count >= 1)
					{
						jollyColors[0][0] = Custom.hexToColor(list[0]);
					}
					if (list.Count >= 2)
					{
						jollyColors[0][1] = Custom.hexToColor(list[1]);
					}
					if (list.Count >= 3)
					{
						jollyColors[0][2] = Custom.hexToColor(list[2]);
					}
				}
				else
				{
					Color color = JollyCustom.GenerateComplementaryColor(JollyColor(i - 1, 0));
					jollyColors[i][0] = color;
					HSLColor hSLColor = JollyCustom.RGB2HSL(JollyCustom.GenerateClippedInverseColor(color));
					float num = hSLColor.lightness + 0.45f;
					hSLColor.lightness *= num;
					hSLColor.saturation *= num;
					jollyColors[i][1] = hSLColor.rgb;
					HSLColor hSLColor2 = JollyCustom.RGB2HSL(JollyCustom.GenerateComplementaryColor(hSLColor.rgb));
					hSLColor2.saturation = Mathf.Lerp(hSLColor2.saturation, 1f, 0.8f);
					hSLColor2.lightness = Mathf.Lerp(hSLColor2.lightness, 1f, 0.8f);
					jollyColors[i][2] = hSLColor2.rgb;
					JollyCustom.Log("Generating auto color for player " + i);
				}
			}
		}
	}

	public static void LoadJollyColorsFromOptions(int playerNumber)
	{
		JollyCustom.Log($"Loading color from options for player: {playerNumber}. Body: {jollyColors[playerNumber][0]}");
		jollyColors[playerNumber][0] = Custom.rainWorld.options.jollyPlayerOptionsArray[playerNumber].GetBodyColor();
		jollyColors[playerNumber][1] = Custom.rainWorld.options.jollyPlayerOptionsArray[playerNumber].GetFaceColor();
		jollyColors[playerNumber][2] = Custom.rainWorld.options.jollyPlayerOptionsArray[playerNumber].GetUniqueColor();
	}

	public static Color JollyBodyColorMenu(SlugcatStats.Name slugName, SlugcatStats.Name reference)
	{
		if (Custom.rainWorld.options.jollyColorMode != Options.JollyColorMode.DEFAULT && (jollyColors == null || jollyColors[0][0] != SlugcatColor(reference)))
		{
			PopulateJollyColorArray(reference);
		}
		if (slugName == JollyEnums.Name.JollyPlayer1 && Custom.rainWorld.options.jollyColorMode != Options.JollyColorMode.CUSTOM)
		{
			slugName = reference;
		}
		return SlugcatColor(slugName);
	}

	public static Color JollyFaceColorMenu(SlugcatStats.Name slugName, SlugcatStats.Name reference, int playerNumber)
	{
		if (Custom.rainWorld.options.jollyColorMode != Options.JollyColorMode.DEFAULT && (jollyColors == null || jollyColors[0][0] != SlugcatColor(reference)))
		{
			PopulateJollyColorArray(reference);
		}
		Color color = Color.black;
		if (slugName == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel || slugName == MoreSlugcatsEnums.SlugcatStatsName.Artificer || slugName == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			color = Color.Lerp(new Color(1f, 1f, 1f), color, 0.3f);
		}
		if (playerNumber == 0 && Custom.rainWorld.options.jollyColorMode != Options.JollyColorMode.CUSTOM)
		{
			return color;
		}
		if ((Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO && playerNumber > 0) || Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.CUSTOM)
		{
			return JollyColor(playerNumber, 1);
		}
		return color;
	}

	public static Color JollyUniqueColorMenu(SlugcatStats.Name slugName, SlugcatStats.Name reference, int playerNumber)
	{
		if (Custom.rainWorld.options.jollyColorMode != Options.JollyColorMode.DEFAULT && (jollyColors == null || jollyColors[0][0] != DefaultSlugcatColor(reference)))
		{
			PopulateJollyColorArray(reference);
		}
		Color result = Color.white;
		if (slugName == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
		{
			result = new Color(0.87451f, 0.17647f, 0.91765f);
		}
		else if (slugName == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			result = new Color(0.27059f, 0.15686f, 0.23529f);
		}
		else if (slugName == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			result = Color.gray;
		}
		if (playerNumber == 0 && Custom.rainWorld.options.jollyColorMode != Options.JollyColorMode.CUSTOM)
		{
			return result;
		}
		if ((Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO && playerNumber > 0) || Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.CUSTOM)
		{
			return JollyColor(playerNumber, 2);
		}
		return result;
	}

	public static Color JollyColor(int playerNumber, int bodyPartIndex)
	{
		if (jollyColors == null)
		{
			return Color.gray;
		}
		return jollyColors[playerNumber][bodyPartIndex] ?? Color.grey;
	}

	public static Color CustomColorSafety(int staticColorIndex)
	{
		return new Color(Mathf.Clamp(customColors[staticColorIndex].r, 0.01f, 0.99f), Mathf.Clamp(customColors[staticColorIndex].g, 0.01f, 0.99f), Mathf.Clamp(customColors[staticColorIndex].b, 0.01f, 0.99f));
	}

	public static bool CustomColorsEnabled()
	{
		if (ModManager.MMF && customColors != null)
		{
			return !ModManager.CoopAvailable;
		}
		return false;
	}

	public bool SaintFaceCondition()
	{
		if (player.room != null && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			if (player.monkAscension)
			{
				return player.killWait >= 0.02f;
			}
			return true;
		}
		return false;
	}

	public void EchoSkin(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		for (int i = 0; i < 9; i++)
		{
			sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["GhostSkin"];
		}
	}

	public void UndoEchoSkin(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		for (int i = 0; i < 9; i++)
		{
			sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["Basic"];
		}
	}

	public void BiteStruggle(int hand)
	{
		head.vel += Custom.DirVec(head.pos, hands[hand].pos);
		NudgeDrawPosition(0, new Vector2(0f, 1f));
		hands[hand].pos = drawPositions[0, 0] + new Vector2(0f, -8f) + Custom.RNV();
		LookAtObject(player.grasps[hand].grabbed);
		if (blink < 5)
		{
			blink = 5;
		}
	}

	private void PlayerBlink()
	{
		if (UnityEngine.Random.value < 1f / 30f)
		{
			blink = Math.Max(2, blink);
		}
		if (ModManager.MSC && player.room != null && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			blink = Math.Min(2, blink);
		}
		else if (player.sleepCurlUp == 1f)
		{
			blink = Math.Max(2, blink);
		}
	}

	private void InitCachedSpriteNames()
	{
		_cachedFaceSpriteNames = new AGCachedStrings3Dim(new string[2] { "Face", "PFace" }, new string[5] { "A", "B", "C", "D", "E" }, 9);
		_cachedHeads = new AGCachedStrings2Dim(new string[3] { "HeadA", "HeadB", "HeadC" }, 18);
		_cachedPlayerArms = new AGCachedStrings("PlayerArm", 13);
		_cachedLegsA = new AGCachedStrings("LegsA", 31);
		_cachedLegsACrawling = new AGCachedStrings("LegsACrawling", 31);
		_cachedLegsAClimbing = new AGCachedStrings("LegsAClimbing", 31);
		_cachedLegsAOnPole = new AGCachedStrings("LegsAOnPole", 31);
	}

	public string DefaultFaceSprite(float eyeScale, int imgIndex)
	{
		if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup && player.room != null && player.room.world.game.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
		{
			return _cachedFaceSpriteNames[0, 4, imgIndex];
		}
		int num = 0;
		if (RenderAsPup && ModManager.MSC)
		{
			num = 1;
		}
		int j = ((blink > 0 || SaintFaceCondition()) ? 1 : ((ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer && !player.DreamState && num != 1) ? ((!(eyeScale < 0f)) ? 2 : 3) : 0));
		return _cachedFaceSpriteNames[num, j, imgIndex];
	}

	[Obsolete("Use two parameter function instead.")]
	public string DefaultFaceSprite(float eyeScale)
	{
		if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup && player.room != null && player.room.world.game.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
		{
			return "FaceE";
		}
		string text = "Face";
		if (RenderAsPup && ModManager.MSC)
		{
			text = "PFace";
		}
		string text2 = ((blink > 0 || SaintFaceCondition()) ? "B" : ((!ModManager.MSC || !(player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer) || player.DreamState || !(text != "PFace")) ? "A" : ((!(eyeScale < 0f)) ? "C" : "D")));
		return text + text2;
	}

	public void MSCUpdate()
	{
		if (base.owner.room != null && base.owner.room.game.IsStorySession && !player.playerState.isPup)
		{
			gown.visible = (base.owner.room.game.session as StoryGameSession).saveState.wearingCloak;
		}
		else
		{
			gown.visible = false;
		}
		gown.Update();
		if (player.room != null && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
		{
			gills.Update();
		}
		if (player.room != null && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			bodyPearl.Update();
			if (tailSpecks.spearProg > 0.1f)
			{
				blink = 5;
			}
		}
		if (player.room != null && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			if (tentaclesVisible > 0 && darkenFactor == 0f)
			{
				darkenFactor = 0.01f;
			}
			for (int i = 0; i < tentacles.Length; i++)
			{
				if (darkenFactor > 0f)
				{
					tentacles[i].Update();
					tentacles[i].SetPosition(player.mainBodyChunk.pos + Custom.DegToVec(45f + 90f * (float)i) * 8f);
					if (i >= tentaclesVisible)
					{
						tentacles[i].InactiveUpdate();
					}
					else
					{
						tentacles[i].ActiveUpdate();
					}
				}
			}
			lastStretch = stretch;
			stretch = RopeStretchFac;
			List<Vector2> list = new List<Vector2>();
			for (int num = player.tongue.rope.TotalPositions - 1; num > 0; num--)
			{
				list.Add(player.tongue.rope.GetPosition(num));
			}
			list.Add(player.mainBodyChunk.pos);
			float num2 = 0f;
			for (int j = 1; j < list.Count; j++)
			{
				num2 += Vector2.Distance(list[j - 1], list[j]);
			}
			float num3 = 0f;
			for (int k = 0; k < list.Count; k++)
			{
				if (k > 0)
				{
					num3 += Vector2.Distance(list[k - 1], list[k]);
				}
				AlignRope(num3 / num2, list[k]);
			}
			for (int l = 0; l < ropeSegments.Length; l++)
			{
				ropeSegments[l].Update();
			}
			for (int m = 1; m < ropeSegments.Length; m++)
			{
				ConnectRopeSegments(m, m - 1);
			}
			for (int n = 0; n < ropeSegments.Length; n++)
			{
				ropeSegments[n].claimedForBend = false;
			}
		}
		if ((player.isSlugpup || ModManager.CoopAvailable) && player.onBack != null)
		{
			for (int num4 = 0; num4 < 2; num4++)
			{
				hands[num4].mode = Limb.Mode.Retracted;
			}
		}
	}

	private void ConnectRopeSegments(int A, int B)
	{
		Vector2 vector = Custom.DirVec(ropeSegments[A].pos, ropeSegments[B].pos);
		float num = Vector2.Distance(ropeSegments[A].pos, ropeSegments[B].pos);
		float num2 = player.tongue.rope.totalLength / (float)ropeSegments.Length * 0.1f;
		if (!ropeSegments[A].claimedForBend)
		{
			ropeSegments[A].pos += vector * (num - num2) * 0.5f;
			ropeSegments[A].vel += vector * (num - num2) * 0.5f;
		}
		if (!ropeSegments[B].claimedForBend)
		{
			ropeSegments[B].pos -= vector * (num - num2) * 0.5f;
			ropeSegments[B].vel -= vector * (num - num2) * 0.5f;
		}
	}

	private void AlignRope(float f, Vector2 alignPos)
	{
		int num = Custom.IntClamp((int)(f * (float)ropeSegments.Length), 0, ropeSegments.Length - 1);
		ropeSegments[num].lastPos = ropeSegments[num].pos;
		ropeSegments[num].pos = alignPos;
		ropeSegments[num].vel *= 0f;
		ropeSegments[num].claimedForBend = true;
	}

	public PlayerSpineData SpinePosition(float s, float timeStacker)
	{
		float num = Mathf.InverseLerp(0f, 1f, s);
		int num2 = Mathf.FloorToInt(num * (float)tail.Length - 1f);
		int num3 = Mathf.FloorToInt(num * (float)tail.Length);
		if (num3 > tail.Length - 1)
		{
			num3 = tail.Length - 1;
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		Vector2 vector = Vector2.Lerp(tail[num2].lastPos, tail[num2].pos, timeStacker);
		float stretchedRad = tail[num2].StretchedRad;
		Vector2 vector2 = Vector2.Lerp(tail[Math.Min(num3 + 1, tail.Length - 1)].lastPos, tail[Math.Min(num3 + 1, tail.Length - 1)].pos, timeStacker);
		Vector2 vector3 = Vector2.Lerp(tail[num3].lastPos, tail[num3].pos, timeStacker);
		float stretchedRad2 = tail[num3].StretchedRad;
		float t = Mathf.InverseLerp(num2 + 1, num3 + 1, num * (float)tail.Length);
		Vector2 normalized = Vector2.Lerp(vector3 - vector, vector2 - vector3, t).normalized;
		if (normalized.x == 0f && normalized.y == 0f)
		{
			normalized = (tail[tail.Length - 1].pos - tail[tail.Length - 2].pos).normalized;
		}
		Vector2 vector4 = Custom.PerpendicularVector(normalized);
		float num4 = Mathf.Lerp(stretchedRad, stretchedRad2, t);
		float f = 0f;
		f = Mathf.Pow(Mathf.Abs(f), Mathf.Lerp(1.2f, 0.3f, Mathf.Pow(s, 0.5f))) * Mathf.Sign(f);
		Vector2 outerPos = Vector2.Lerp(vector, vector3, t) + vector4 * f * num4;
		return new PlayerSpineData(s, Vector2.Lerp(vector, vector3, t), outerPos, normalized, vector4, f, num4);
	}
}
