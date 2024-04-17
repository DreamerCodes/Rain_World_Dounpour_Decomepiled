using CoralBrain;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class InspectorGraphics : GraphicsModule, IDrawable, IOwnMycelia
{
	public class InspectorHeadRopeGraphics : RopeGraphic
	{
		private int headNumber;

		private InspectorGraphics owner;

		private int spriteOffset;

		public override void Update()
		{
			int listCount = 0;
			AddToPositionsList(listCount++, owner.myInspector.heads[headNumber].FloatBase);
			for (int i = 0; i < owner.myInspector.heads[headNumber].tChunks.Length; i++)
			{
				for (int j = 1; j < owner.myInspector.heads[headNumber].tChunks[i].rope.TotalPositions; j++)
				{
					AddToPositionsList(listCount++, owner.myInspector.heads[headNumber].tChunks[i].rope.GetPosition(j));
				}
			}
			AlignAndConnect(listCount);
		}

		public override void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = owner.myInspector.mainBodyChunk.pos;
			vector += Custom.DirVec(Vector2.Lerp(segments[1].lastPos, segments[1].pos, timeStacker), vector) * 1f;
			float a = owner.RadOfSegment(0f, timeStacker) * 1.7f + 2f;
			for (int i = 0; i < segments.Length; i++)
			{
				float f = (float)i / (float)(segments.Length - 1);
				Vector2 vector2 = Vector2.Lerp(segments[i].lastPos, segments[i].pos, timeStacker);
				if (i >= segments.Length - 1)
				{
					vector2 += Custom.DirVec(vector, vector2) * 1f;
				}
				else
				{
					vector2 = Vector2.Lerp(segments[i + 1].lastPos, segments[i + 1].pos, timeStacker);
				}
				Vector2 vector3 = Custom.PerpendicularVector((vector - vector2).normalized);
				float num = owner.RadOfSegment(f, timeStacker) * 1.7f + 2f;
				(sLeaser.sprites[spriteOffset] as TriangleMesh).color = owner.myInspector.bodyColor;
				(sLeaser.sprites[spriteOffset] as TriangleMesh).alpha = owner.myInspector.lightpulse;
				(sLeaser.sprites[spriteOffset] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * Mathf.Lerp(a, num, 0.5f) - camPos);
				(sLeaser.sprites[spriteOffset] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * Mathf.Lerp(a, num, 0.5f) - camPos);
				(sLeaser.sprites[spriteOffset] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * num - camPos);
				(sLeaser.sprites[spriteOffset] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * num - camPos);
				vector = vector2;
				a = num;
			}
		}

		public override void MoveSegment(int segment, Vector2 goalPos, Vector2 smoothedGoalPos)
		{
			segments[segment].vel *= 0f;
			if (owner.myInspector.room.GetTile(smoothedGoalPos).Solid && !owner.myInspector.room.GetTile(goalPos).Solid)
			{
				FloatRect floatRect = Custom.RectCollision(smoothedGoalPos, goalPos, owner.myInspector.room.TileRect(owner.myInspector.room.GetTilePosition(smoothedGoalPos)).Grow(3f));
				segments[segment].pos = new Vector2(floatRect.left, floatRect.bottom);
			}
			else
			{
				segments[segment].pos = smoothedGoalPos;
			}
		}

		public Vector2 OnTubePos(Vector2 pos, float timeStacker)
		{
			Vector2 p = OneDimensionalTubePos(pos.y - 1f / (float)segments.Length, timeStacker);
			Vector2 p2 = OneDimensionalTubePos(pos.y + 1f / (float)segments.Length, timeStacker);
			return OneDimensionalTubePos(pos.y, timeStacker) + Custom.PerpendicularVector(Custom.DirVec(p, p2)) * pos.x;
		}

		public Vector2 OnTubeDir(float floatPos, float timeStacker)
		{
			Vector2 p = OneDimensionalTubePos(floatPos - 1f / (float)segments.Length, timeStacker);
			Vector2 p2 = OneDimensionalTubePos(floatPos + 1f / (float)segments.Length, timeStacker);
			return Custom.DirVec(p, p2);
		}

		public Vector2 OneDimensionalTubePos(float floatPos, float timeStacker)
		{
			int num = Custom.IntClamp(Mathf.FloorToInt(floatPos * (float)(segments.Length - 1)), 0, segments.Length - 1);
			int num2 = Custom.IntClamp(num + 1, 0, segments.Length - 1);
			float t = Mathf.InverseLerp(num, num2, floatPos * (float)(segments.Length - 1));
			return Vector2.Lerp(Vector2.Lerp(segments[num].lastPos, segments[num2].lastPos, t), Vector2.Lerp(segments[num].pos, segments[num2].pos, t), timeStacker);
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, int sproffset)
		{
			spriteOffset = sproffset;
			sLeaser.sprites[spriteOffset] = TriangleMesh.MakeLongMesh(segments.Length, pointyTip: false, customColor: false);
			sLeaser.sprites[spriteOffset].shader = rCam.game.rainWorld.Shaders["OverseerZip"];
			sLeaser.sprites[spriteOffset].alpha = 0.7f + 0.1f * Random.value;
		}

		public InspectorHeadRopeGraphics(InspectorGraphics owner, int head)
			: base(40)
		{
			this.owner = owner;
			headNumber = head;
		}
	}

	private Color blackColor;

	private ProjectionCircle projectionCircle;

	public Mycelium[] mycelia;

	private Color wingColor;

	private float[] wingflapCounters;

	private float bodyRotation;

	public InspectorHeadRopeGraphics[] ropeGraphics;

	public float[] JawAngle;

	private float[] JawAngleWiggler;

	public float[] blinks;

	private GenericBodyPart[] wingBodyParts;

	private float wingBodyPartDistance;

	private Inspector myInspector => base.owner as Inspector;

	public Room OwnerRoom => base.owner.room;

	private int SpritesBegin_Core => 0;

	private int SpritesTotal_Core => 4;

	private int SpritesBegin_mycelium => SpritesTotal_Core;

	private int SpritesTotal_mycelium => mycelia.Length;

	private int SpritesTotal_All => SpritesTotal_Core + SpritesTotal_mycelium + SpritesTotal_wings + SpritesTotal_heads;

	public int SpritesBegin_wings => SpritesTotal_Core + SpritesTotal_mycelium;

	public int SpritesTotal_wings => (myInspector.State as Inspector.InspectorState).Wingnumber;

	public int SpritesBegin_heads => SpritesTotal_Core + SpritesTotal_mycelium + SpritesTotal_wings;

	public int SpritesTotal_heads
	{
		get
		{
			int i = 0;
			int num = 0;
			for (; i < Inspector.headCount(); i++)
			{
				num += SpritesTotal_singlehead();
			}
			return num;
		}
	}

	public InspectorGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		ropeGraphics = new InspectorHeadRopeGraphics[Inspector.headCount()];
		for (int i = 0; i < Inspector.headCount(); i++)
		{
			ropeGraphics[i] = new InspectorHeadRopeGraphics(this, i);
		}
		cullRange = 300f;
		JawAngleWiggler = new float[Inspector.headCount()];
		mycelia = new Mycelium[(int)(10f + 15f * myInspector.room.world.game.SeededRandom(myInspector.abstractCreature.ID.RandomSeed))];
		blinks = new float[Inspector.headCount()];
		wingBodyParts = new GenericBodyPart[(myInspector.State as Inspector.InspectorState).Wingnumber];
		for (int j = 0; j < wingBodyParts.Length; j++)
		{
			wingBodyParts[j] = new GenericBodyPart(this, 15f, 0.2f, 0.1f, myInspector.firstChunk);
		}
		wingBodyPartDistance = 75f;
		for (int k = 0; k < mycelia.GetLength(0); k++)
		{
			mycelia[k] = new Mycelium(myInspector.neuronSystem, this, k, Mathf.Lerp(120f, 300f, myInspector.room.world.game.SeededRandom(myInspector.abstractCreature.ID.RandomSeed + k)), myInspector.mainBodyChunk.pos);
			mycelia[k].useStaticCulling = false;
			mycelia[k].color = myInspector.bodyColor;
			bodyRotation = 0f;
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
		base.ApplyPalette(sLeaser, rCam, palette);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[SpritesTotal_All];
		sLeaser.sprites[SpritesBegin_Core] = new FSprite("Circle20");
		sLeaser.sprites[SpritesBegin_Core + 1] = new FSprite("Circle20");
		sLeaser.sprites[SpritesBegin_Core + 2] = new FSprite("Circle20");
		sLeaser.sprites[SpritesBegin_Core + 3] = new FSprite("Circle20");
		for (int i = 0; i < SpritesTotal_mycelium; i++)
		{
			mycelia[i].InitiateSprites(SpritesBegin_mycelium + i, sLeaser, rCam);
		}
		wingflapCounters = new float[SpritesTotal_wings];
		for (int j = 0; j < SpritesTotal_wings; j++)
		{
			sLeaser.sprites[SpritesBegin_wings + j] = new FSprite("CicadaWingA");
			sLeaser.sprites[SpritesBegin_wings + j].anchorX = 0f;
			sLeaser.sprites[SpritesBegin_wings + j].scaleX = Mathf.Lerp(3.6f, 5f, myInspector.room.world.game.SeededRandom(myInspector.abstractCreature.ID.RandomSeed));
			sLeaser.sprites[SpritesBegin_wings + j].scaleY = 0.8f;
			sLeaser.sprites[SpritesBegin_wings + j].alpha = 0.3f;
			sLeaser.sprites[SpritesBegin_wings + j].shader = rCam.room.game.rainWorld.Shaders["CicadaWing"];
		}
		int num = 0;
		for (int k = 0; k < Inspector.headCount(); k++)
		{
			for (int l = 0; l < SpritesTotal_singlehead(); l++)
			{
				ropeGraphics[k].InitiateSprites(sLeaser, rCam, SpritesBegin_heads + num);
				sLeaser.sprites[SpritesBegin_heads + num + 1] = new FSprite("Circle20");
				sLeaser.sprites[SpritesBegin_heads + num + 1].scale = 0.75f;
				sLeaser.sprites[SpritesBegin_heads + num + 2] = new FSprite("FlyWing");
				sLeaser.sprites[SpritesBegin_heads + num + 2].anchorY = 0f;
				sLeaser.sprites[SpritesBegin_heads + num + 2].scaleY = 1.5f;
				sLeaser.sprites[SpritesBegin_heads + num + 3] = new FSprite("FlyWing");
				sLeaser.sprites[SpritesBegin_heads + num + 3].anchorY = 0f;
				sLeaser.sprites[SpritesBegin_heads + num + 3].scaleY = 1.5f;
				sLeaser.sprites[SpritesBegin_heads + num + 4] = new FSprite("FlyWing");
				sLeaser.sprites[SpritesBegin_heads + num + 4].anchorY = 0f;
				sLeaser.sprites[SpritesBegin_heads + num + 5] = new FSprite("FlyWing");
				sLeaser.sprites[SpritesBegin_heads + num + 5].anchorY = 0f;
			}
			num += SpritesTotal_singlehead();
			sLeaser.sprites[SpritesBegin_Eye(k)] = new FSprite("Circle20");
			sLeaser.sprites[SpritesBegin_Eye(k)].scaleY = 0.625f;
			sLeaser.sprites[SpritesBegin_Eye(k)].scaleX = 0.45f;
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (culled || myInspector.room == null)
		{
			return;
		}
		Vector2 pos = myInspector.mainBodyChunk.pos;
		for (int i = 0; i < mycelia.Length; i++)
		{
			mycelia[i].DrawSprites(SpritesBegin_mycelium + i, sLeaser, rCam, timeStacker, camPos);
			sLeaser.sprites[i].isVisible = !mycelia[i].culled;
		}
		Color bodyColor = myInspector.bodyColor;
		wingColor = Color.Lerp(bodyColor, new HSLColor(0.2f, 0.2f, 0.4f).rgb, 0.4f);
		wingColor.a = 0.2f;
		for (int j = 0; j < SpritesTotal_wings; j++)
		{
			Vector2 vector = Custom.DegToVec(360f / (float)SpritesTotal_wings * (float)j + bodyRotation) * wingBodyPartDistance;
			float num = 360f / (float)wingBodyParts.Length * (float)j;
			sLeaser.sprites[SpritesBegin_wings + j].x = pos.x - camPos.x;
			sLeaser.sprites[SpritesBegin_wings + j].y = pos.y - camPos.y;
			sLeaser.sprites[SpritesBegin_wings + j].color = wingColor;
			sLeaser.sprites[SpritesBegin_wings + j].scaleX = Mathf.Lerp(0f, Mathf.Lerp(3.6f, 6f, myInspector.room.world.game.SeededRandom(myInspector.abstractCreature.ID.RandomSeed)) * Mathf.InverseLerp(1f, 0.8f, myInspector.squeezeFac), Mathf.InverseLerp(1f, 0.55f, Vector2.Distance(wingBodyParts[j].pos, myInspector.firstChunk.pos + vector) / wingBodyPartDistance));
			wingflapCounters[j] += 0.02f;
			wingflapCounters[j] += findWingFlapIntensity(j, myInspector.mainBodyChunk.vel * -1f) / 8f;
			wingflapCounters[j] += findWingFlapIntensity(j, myInspector.flyingPower) * 3f;
			float num2 = Mathf.Sin(wingflapCounters[j]) * 8f;
			sLeaser.sprites[SpritesBegin_wings + j].rotation = num + bodyRotation - 90f;
			sLeaser.sprites[SpritesBegin_wings + j].rotation += num2;
		}
		for (int k = 0; k < Inspector.headCount(); k++)
		{
			ropeGraphics[k].DrawSprite(sLeaser, rCam, timeStacker, camPos);
		}
		for (int l = 0; l < SpritesTotal_Core; l++)
		{
			sLeaser.sprites[l].x = pos.x - camPos.x;
			sLeaser.sprites[l].y = pos.y - camPos.y;
			sLeaser.sprites[l].scale = 1.3f + Mathf.Sin(myInspector.lightpulse + (float)l / (float)l) / 4f;
			sLeaser.sprites[l].color = myInspector.bodyColor;
		}
		int num3 = 0;
		for (int m = 0; m < Inspector.headCount(); m++)
		{
			Vector2 lastPos = myInspector.heads[m].tChunks[myInspector.heads[m].tChunks.Length - 2].lastPos;
			Vector2 lastPos2 = myInspector.heads[m].Tip.lastPos;
			sLeaser.sprites[SpritesBegin_heads + num3 + 1].x = lastPos2.x - camPos.x;
			sLeaser.sprites[SpritesBegin_heads + num3 + 1].y = lastPos2.y - camPos.y;
			sLeaser.sprites[SpritesBegin_heads + num3 + 1].color = myInspector.bodyColor;
			sLeaser.sprites[SpritesBegin_heads + num3 + 1].scaleX = 0.8f;
			sLeaser.sprites[SpritesBegin_heads + num3 + 1].scaleY = 1.4f;
			sLeaser.sprites[SpritesBegin_heads + num3 + 1].anchorY = 0.3f;
			sLeaser.sprites[SpritesBegin_heads + num3 + 1].rotation = Custom.AimFromOneVectorToAnother(lastPos, lastPos2);
			Vector2 vector2 = Custom.DegToVec(Custom.AimFromOneVectorToAnother(lastPos, lastPos2));
			vector2 *= 10f;
			Color b = ((myInspector.activeEye != m || myInspector.HeadsCrippled(myInspector.activeEye)) ? new Color(0f, 0.02f, 0.2f) : Color.white);
			sLeaser.sprites[SpritesBegin_Eye(m)].x = lastPos2.x + vector2.x - camPos.x;
			sLeaser.sprites[SpritesBegin_Eye(m)].y = lastPos2.y + vector2.y - camPos.y;
			sLeaser.sprites[SpritesBegin_Eye(m)].color = Color.Lerp(sLeaser.sprites[SpritesBegin_Eye(m)].color, b, 0.2f);
			sLeaser.sprites[SpritesBegin_Eye(m)].rotation = Custom.AimFromOneVectorToAnother(lastPos, lastPos2);
			sLeaser.sprites[SpritesBegin_Eye(m)].scaleY = 0.625f;
			if ((myInspector.State as Inspector.InspectorState).headHealth[m] > 0f)
			{
				sLeaser.sprites[SpritesBegin_Eye(m)].scaleX = Mathf.Lerp(Mathf.Lerp(sLeaser.sprites[SpritesBegin_Eye(m)].scaleX, Mathf.Lerp(0.525f, 0.225f, myInspector.anger), 0.15f), 0.1f, blinks[m]);
				sLeaser.sprites[SpritesBegin_Eye(m)].scaleX = Mathf.Lerp(sLeaser.sprites[SpritesBegin_Eye(m)].scaleX, 0.25f + Mathf.Sin(myInspector.blind) / 4f, Mathf.InverseLerp(0f, 500f, myInspector.blind));
			}
			else
			{
				sLeaser.sprites[SpritesBegin_Eye(m)].scaleX = Mathf.Lerp(sLeaser.sprites[SpritesBegin_Eye(m)].scaleX, 0.125f, 0.06f);
			}
			vector2 = Custom.DegToVec(Custom.AimFromOneVectorToAnother(lastPos, lastPos2));
			vector2 *= 12f;
			Vector2 vector3 = vector2;
			sLeaser.sprites[SpritesBegin_heads + num3 + 2].x = lastPos2.x + vector3.x - camPos.x;
			sLeaser.sprites[SpritesBegin_heads + num3 + 2].y = lastPos2.y + vector3.y - camPos.y;
			sLeaser.sprites[SpritesBegin_heads + num3 + 2].color = myInspector.bodyColor;
			sLeaser.sprites[SpritesBegin_heads + num3 + 2].rotation = Custom.AimFromOneVectorToAnother(lastPos, lastPos2) + JawAngle[m];
			sLeaser.sprites[SpritesBegin_heads + num3 + 2].scaleY = 1.1f;
			sLeaser.sprites[SpritesBegin_heads + num3 + 2].scaleX = 0.8f;
			sLeaser.sprites[SpritesBegin_heads + num3 + 3].x = lastPos2.x + vector3.x - camPos.x;
			sLeaser.sprites[SpritesBegin_heads + num3 + 3].y = lastPos2.y + vector3.y - camPos.y;
			sLeaser.sprites[SpritesBegin_heads + num3 + 3].color = myInspector.bodyColor;
			sLeaser.sprites[SpritesBegin_heads + num3 + 3].rotation = Custom.AimFromOneVectorToAnother(lastPos, lastPos2) + 180f - JawAngle[m];
			sLeaser.sprites[SpritesBegin_heads + num3 + 3].scaleY = -1.1f;
			sLeaser.sprites[SpritesBegin_heads + num3 + 3].scaleX = 0.8f;
			vector2 = Custom.DegToVec(Custom.AimFromOneVectorToAnother(lastPos, lastPos2));
			vector2 *= -5f;
			vector3 = vector2;
			sLeaser.sprites[SpritesBegin_heads + num3 + 4].x = lastPos2.x + vector3.x - camPos.x;
			sLeaser.sprites[SpritesBegin_heads + num3 + 4].y = lastPos2.y + vector3.y - camPos.y;
			sLeaser.sprites[SpritesBegin_heads + num3 + 4].color = myInspector.bodyColor;
			sLeaser.sprites[SpritesBegin_heads + num3 + 4].rotation = Custom.AimFromOneVectorToAnother(lastPos, lastPos2) + JawAngle[m] - 5f;
			sLeaser.sprites[SpritesBegin_heads + num3 + 4].scaleY = 2.2f;
			sLeaser.sprites[SpritesBegin_heads + num3 + 4].scaleX = -1.3f;
			sLeaser.sprites[SpritesBegin_heads + num3 + 5].x = lastPos2.x + vector3.x - camPos.x;
			sLeaser.sprites[SpritesBegin_heads + num3 + 5].y = lastPos2.y + vector3.y - camPos.y;
			sLeaser.sprites[SpritesBegin_heads + num3 + 5].color = myInspector.bodyColor;
			sLeaser.sprites[SpritesBegin_heads + num3 + 5].rotation = Custom.AimFromOneVectorToAnother(lastPos, lastPos2) + 180f - JawAngle[m] + 5f;
			sLeaser.sprites[SpritesBegin_heads + num3 + 5].scaleY = -2.2f;
			sLeaser.sprites[SpritesBegin_heads + num3 + 5].scaleX = -1.3f;
			num3 += SpritesTotal_singlehead();
		}
		for (int n = 0; n < mycelia.GetLength(0); n++)
		{
			mycelia[n].UpdateColor(myInspector.bodyColor, 0f, SpritesBegin_mycelium + n, sLeaser);
		}
	}

	public override void Update()
	{
		base.Update();
		if (culled)
		{
			return;
		}
		for (int i = 0; i < wingBodyParts.Length; i++)
		{
			Vector2 vector = Custom.DegToVec(360f / (float)SpritesTotal_wings * (float)i + bodyRotation) * wingBodyPartDistance;
			wingBodyParts[i].ConnectToPoint(myInspector.firstChunk.pos, wingBodyPartDistance, push: false, 0.3f, myInspector.firstChunk.vel + vector / 3f, 0.25f, 0.1f);
			if (Vector2.Distance(myInspector.firstChunk.pos, wingBodyParts[i].pos) > wingBodyPartDistance * 1.1f || OwnerRoom.aimap.getAItile(myInspector.firstChunk.pos).narrowSpace)
			{
				wingBodyParts[i].pos = myInspector.firstChunk.pos;
			}
			wingBodyParts[i].Update();
		}
		for (int j = 0; j < mycelia.Length; j++)
		{
			mycelia[j].Update();
		}
		if (JawAngle == null)
		{
			JawAngle = new float[Inspector.headCount()];
		}
		for (int k = 0; k < Inspector.headCount(); k++)
		{
			if (blinks[k] > 0f)
			{
				blinks[k] += 0.1f;
				if (blinks[k] > 1f)
				{
					blinks[k] = 0f;
				}
			}
			else if (Random.value < 0.01f)
			{
				blinks[k] = 0.01f;
			}
			ropeGraphics[k].Update();
			if ((myInspector.State as Inspector.InspectorState).headHealth[k] <= 0f)
			{
				JawAngleWiggler[k] += Random.value * 0.01f;
				JawAngle[k] = Mathf.Lerp(JawAngle[k], JawAngle[k] + Mathf.Sin(JawAngleWiggler[k]) * 1.2f, 0.1f);
			}
			else
			{
				JawAngleWiggler[k] += Random.value * (0.1f + myInspector.anger);
				if (myInspector.headWantToGrabChunk[k] != null)
				{
					JawAngle[k] = Mathf.Lerp(JawAngle[k], 25f, 0.15f) + Mathf.Sin(JawAngleWiggler[k]);
				}
				if (myInspector.headGrabChunk[k] != null)
				{
					JawAngle[k] = Mathf.Lerp(JawAngle[k], -15f, 0.45f) + Mathf.Sin(JawAngleWiggler[k]);
				}
				JawAngle[k] = Mathf.Lerp(JawAngle[k], -5f, 0.1f) + Mathf.Sin(JawAngleWiggler[k]);
			}
			if (myInspector.dying > 0f && myInspector.room.ViewedByAnyCamera(myInspector.mainBodyChunk.pos, 900f))
			{
				for (int l = 0; l < 15; l++)
				{
					int num = (int)Random.Range(0f, myInspector.heads[k].tChunks.Length);
					myInspector.room.AddObject(new OverseerEffect(myInspector.heads[k].tChunks[num].pos, Custom.RNV() * Random.value * 0.1f, myInspector.bodyColor, Mathf.Lerp(200f, 15f, myInspector.dying), Mathf.Lerp(1.5f, 0.1f, myInspector.dying)));
				}
				for (int m = 0; m < 8; m++)
				{
					int num2 = (int)Random.Range(0f, myInspector.heads[k].tChunks.Length);
					myInspector.room.AddObject(new Spark(myInspector.heads[k].tChunks[num2].pos, myInspector.mainBodyChunk.vel * 0.5f + Custom.RNV() * 14f * Random.value, myInspector.bodyColor, null, 14, 21));
				}
			}
		}
		if (myInspector.Consious)
		{
			bodyRotation += myInspector.flyingPower.x * 3f;
		}
	}

	public Vector2 ConnectionPos(int index, float timeStacker)
	{
		return myInspector.mainBodyChunk.pos;
	}

	public Vector2 ResetDir(int index)
	{
		return myInspector.mainBodyChunk.vel;
	}

	public void UpdateNeuronSystemForMycelia()
	{
		for (int i = 0; i < mycelia.Length; i++)
		{
			if (mycelia[i].system != myInspector.neuronSystem)
			{
				if (mycelia[i].system != null)
				{
					mycelia[i].system.mycelia.Remove(mycelia[i]);
				}
				if (myInspector.neuronSystem != null)
				{
					myInspector.neuronSystem.mycelia.Add(mycelia[i]);
				}
				mycelia[i].system = myInspector.neuronSystem;
			}
		}
	}

	public override void Reset()
	{
		for (int i = 0; i < Inspector.headCount(); i++)
		{
			ropeGraphics[i].AddToPositionsList(0, myInspector.mainBodyChunk.pos);
			ropeGraphics[i].AddToPositionsList(1, myInspector.heads[i].Tip.pos);
			ropeGraphics[i].AlignAndConnect(2);
		}
		base.Reset();
		for (int j = 0; j < mycelia.Length; j++)
		{
			mycelia[j].Reset(myInspector.mainBodyChunk.pos);
		}
	}

	private float findWingFlapIntensity(int wing, Vector2 inputvec)
	{
		float num = 360f / (float)SpritesTotal_wings * (float)wing;
		return Mathf.InverseLerp(0f, 180f, Mathf.DeltaAngle(num + bodyRotation, inputvec.GetAngle())) * inputvec.magnitude;
	}

	public int SpritesBegin_SingleNeck(int index)
	{
		return SpritesBegin_heads + SpritesTotal_singlehead() * index;
	}

	public int SpritesTotal_singlehead()
	{
		return 7;
	}

	public int SpritesBegin_Eye(int index)
	{
		return SpritesBegin_heads + SpritesTotal_singlehead() * index + (SpritesTotal_singlehead() - 1);
	}

	private float RadOfSegment(float f, float timeStacker)
	{
		return myInspector.Rad(f) * Mathf.Pow(1f - Mathf.Lerp(myInspector.lastDying, myInspector.dying, timeStacker), 0.2f);
	}
}
