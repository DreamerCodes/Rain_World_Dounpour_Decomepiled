using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class TubeWormGraphics : GraphicsModule
{
	public class RopeSegment
	{
		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public int index;

		public bool claimedForBend;

		public TubeWormGraphics wormGraph;

		public RopeSegment(int index, TubeWormGraphics wormGraph)
		{
			this.index = index;
			this.wormGraph = wormGraph;
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
				if (wormGraph.ropeSegments[num].claimedForBend)
				{
					break;
				}
			}
			while (num2 < wormGraph.ropeSegments.Length - 1)
			{
				num2++;
				if (wormGraph.ropeSegments[num2].claimedForBend)
				{
					break;
				}
			}
			Vector2 vector = Vector2.Lerp(wormGraph.ropeSegments[num].pos, wormGraph.ropeSegments[num2].pos, Mathf.InverseLerp(num, num2, index));
			if (wormGraph.worm.tongues[0].mode == TubeWorm.Tongue.Mode.Retracted && wormGraph.worm.tongues[1].mode == TubeWorm.Tongue.Mode.Retracted)
			{
				pos = vector;
				return;
			}
			vel += (vector - pos) * 0.2f;
			pos = Vector2.Lerp(pos, vector, 0.4f);
		}
	}

	public RopeSegment[] ropeSegments;

	private float stretch;

	private float lastStretch;

	private Color color;

	public StaticSoundLoop moveLoop;

	private List<Vector2> positionsList = new List<Vector2>();

	public TubeWorm worm => base.owner as TubeWorm;

	public TubeWormGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		bodyParts = new BodyPart[4];
		for (int i = 0; i < 4; i++)
		{
			bodyParts[i] = new GenericBodyPart(this, 5f, 0.5f, 0.99f, worm.mainBodyChunk);
		}
		ropeSegments = new RopeSegment[20];
		for (int j = 0; j < ropeSegments.Length; j++)
		{
			ropeSegments[j] = new RopeSegment(j, this);
		}
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(worm.abstractCreature.ID.RandomSeed);
		color = Custom.HSL2RGB(Mathf.Lerp(0.52f, 0.68f, UnityEngine.Random.value), Mathf.Lerp(0.4f, 0.9f, UnityEngine.Random.value), Mathf.Lerp(0.15f, 0.3f, UnityEngine.Random.value));
		UnityEngine.Random.state = state;
		moveLoop = new StaticSoundLoop(SoundID.Tube_Worm_Inch_Along_Tongue_LOOP, ow.firstChunk.pos, ow.room, 1f, 1f);
	}

	public override void Update()
	{
		lastStretch = stretch;
		stretch = worm.RopeStretchFac;
		for (int i = 0; i < bodyParts.Length; i++)
		{
			bodyParts[i].Update();
		}
		moveLoop.Update();
		moveLoop.pos = base.owner.firstChunk.pos;
		moveLoop.volume = Mathf.InverseLerp(0f, 0.029f, Mathf.Abs(worm.lastWalk - worm.walkCycle));
		bodyParts[1].pos = worm.bodyChunks[0].pos;
		bodyParts[1].vel = worm.bodyChunks[0].vel;
		bodyParts[2].pos = worm.bodyChunks[1].pos;
		bodyParts[2].vel = worm.bodyChunks[1].vel;
		float num = 10f;
		if (worm.tongues[0].mode != TubeWorm.Tongue.Mode.Retracted)
		{
			bodyParts[0].pos = worm.bodyChunks[0].pos + Custom.DirVec(worm.bodyChunks[0].pos, worm.tongues[0].rope.AConnect) * num * Custom.LerpMap(worm.totalRope, 10f, 500f, 0.7f, 1.7f);
			bodyParts[0].vel = bodyParts[0].lastPos - bodyParts[0].pos;
		}
		else
		{
			Vector2 vector = Custom.DirVec(bodyParts[0].pos, bodyParts[1].pos);
			float num2 = Vector2.Distance(bodyParts[0].pos, bodyParts[1].pos);
			bodyParts[0].pos += vector * (num2 - num) * 0.5f;
			bodyParts[0].vel += vector * (num2 - num) * 0.5f;
			bodyParts[0].vel += Custom.DirVec(worm.bodyChunks[1].pos, worm.bodyChunks[0].pos);
		}
		if (worm.tongues[1].mode != TubeWorm.Tongue.Mode.Retracted)
		{
			bodyParts[3].pos = worm.bodyChunks[1].pos + Custom.DirVec(worm.bodyChunks[1].pos, worm.tongues[1].rope.AConnect) * num * Custom.LerpMap(worm.totalRope, 10f, 500f, 0.7f, 1.7f);
			bodyParts[3].vel = bodyParts[3].lastPos - bodyParts[3].pos;
		}
		else
		{
			Vector2 vector2 = Custom.DirVec(bodyParts[3].pos, bodyParts[2].pos);
			float num3 = Vector2.Distance(bodyParts[3].pos, bodyParts[2].pos);
			bodyParts[3].pos += vector2 * (num3 - num) * 0.5f;
			bodyParts[3].vel += vector2 * (num3 - num) * 0.5f;
			bodyParts[3].vel += Custom.DirVec(worm.bodyChunks[0].pos, worm.bodyChunks[1].pos);
		}
		if (Custom.DistLess(worm.bodyChunks[1].pos, bodyParts[0].pos, 14f))
		{
			bodyParts[0].pos = worm.bodyChunks[1].pos + Custom.DirVec(worm.bodyChunks[1].pos, bodyParts[0].pos) * 14f;
		}
		if (Custom.DistLess(worm.bodyChunks[0].pos, bodyParts[3].pos, 14f))
		{
			bodyParts[3].pos = worm.bodyChunks[0].pos + Custom.DirVec(worm.bodyChunks[0].pos, bodyParts[3].pos) * 14f;
		}
		int num4 = 0;
		if (worm.tongues[0].mode != TubeWorm.Tongue.Mode.Retracted)
		{
			for (int num5 = worm.tongues[0].rope.TotalPositions - 1; num5 > 0; num5--)
			{
				AddToPositionsList(num4++, worm.tongues[0].rope.GetPosition(num5));
			}
			AddToPositionsList(num4++, bodyParts[0].pos);
		}
		if (worm.tongues[0].mode != TubeWorm.Tongue.Mode.Retracted == (worm.tongues[1].mode != TubeWorm.Tongue.Mode.Retracted))
		{
			for (int j = 1; j < 3; j++)
			{
				AddToPositionsList(num4++, bodyParts[j].pos);
			}
		}
		if (worm.tongues[1].mode != TubeWorm.Tongue.Mode.Retracted)
		{
			AddToPositionsList(num4++, bodyParts[3].pos);
			for (int k = 1; k < worm.tongues[1].rope.TotalPositions; k++)
			{
				AddToPositionsList(num4++, worm.tongues[1].rope.GetPosition(k));
			}
		}
		float num6 = 0f;
		for (int l = 1; l < num4; l++)
		{
			num6 += Vector2.Distance(positionsList[l - 1], positionsList[l]);
		}
		float num7 = 0f;
		for (int m = 0; m < num4; m++)
		{
			if (m > 0)
			{
				num7 += Vector2.Distance(positionsList[m - 1], positionsList[m]);
			}
			AlignRope(num7 / num6, positionsList[m]);
		}
		for (int n = 0; n < ropeSegments.Length; n++)
		{
			ropeSegments[n].Update();
		}
		for (int num8 = 1; num8 < ropeSegments.Length; num8++)
		{
			ConnectRopeSegments(num8, num8 - 1);
		}
		for (int num9 = 0; num9 < ropeSegments.Length; num9++)
		{
			ropeSegments[num9].claimedForBend = false;
		}
		base.Update();
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < ropeSegments.Length; i++)
		{
			ropeSegments[i].pos = worm.mainBodyChunk.pos;
			ropeSegments[i].lastPos = worm.mainBodyChunk.pos;
			ropeSegments[i].vel *= 0f;
		}
	}

	private void AddToPositionsList(int index, Vector2 pos)
	{
		while (positionsList.Count < index + 1)
		{
			positionsList.Add(Vector2.zero);
		}
		positionsList[index] = pos;
	}

	private void ConnectRopeSegments(int A, int B)
	{
		Vector2 vector = Custom.DirVec(ropeSegments[A].pos, ropeSegments[B].pos);
		float num = Vector2.Distance(ropeSegments[A].pos, ropeSegments[B].pos);
		float num2 = (worm.tongues[0].rope.totalLength + worm.tongues[1].rope.totalLength) / (float)ropeSegments.Length * 0.1f;
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

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[5];
		sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(4, pointyTip: false, customColor: false);
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["TubeWorm"];
		sLeaser.sprites[1] = TriangleMesh.MakeLongMesh(4, pointyTip: false, customColor: false);
		sLeaser.sprites[2] = TriangleMesh.MakeLongMesh(ropeSegments.Length - 1, pointyTip: false, customColor: true);
		for (int i = 3; i < 5; i++)
		{
			sLeaser.sprites[i] = new FSprite("Circle20");
			sLeaser.sprites[i].scaleX = 0.2f;
			sLeaser.sprites[i].anchorY = 0f;
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		for (int num = sLeaser.sprites.Length - 1; num >= 0; num--)
		{
			newContatiner.AddChild(sLeaser.sprites[num]);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		float num = Mathf.Lerp(worm.lastWalk, worm.walkCycle, timeStacker);
		if (num < 0f)
		{
			num += Mathf.Round(num + 1f);
		}
		num -= Mathf.Floor(num);
		sLeaser.sprites[0].alpha = num;
		Vector2 vector = Vector2.Lerp(bodyParts[0].lastPos, bodyParts[0].pos, timeStacker);
		vector += Custom.DirVec(Vector2.Lerp(bodyParts[1].lastPos, bodyParts[1].pos, timeStacker), vector) * 10f;
		for (int i = 0; i < bodyParts.Length; i++)
		{
			Vector2 vector2 = Vector2.Lerp(bodyParts[i].lastPos, bodyParts[i].pos, timeStacker);
			Vector2 b = ((i < 3) ? Vector2.Lerp(bodyParts[i + 1].lastPos, bodyParts[i + 1].pos, timeStacker) : (vector2 + Custom.DirVec(vector, vector2) * 10f));
			Vector2 normalized = (vector - vector2).normalized;
			Vector2 vector3 = Custom.PerpendicularVector(normalized);
			float num2 = Vector2.Distance(vector2, vector) / 4f;
			float num3 = Vector2.Distance(vector2, b) / 4f;
			float num4 = SegmentStretchFac(i, timeStacker);
			float num5 = 5f * num4;
			float num6 = 5f * num4;
			switch (i)
			{
			case 0:
				num5 = 3.5f * num4;
				break;
			case 3:
				num6 = 3.5f * num4;
				break;
			}
			for (int j = 0; j < 2; j++)
			{
				(sLeaser.sprites[j] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * num5 - normalized * num2 - camPos);
				(sLeaser.sprites[j] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * num5 - normalized * num2 - camPos);
				(sLeaser.sprites[j] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * num6 + normalized * num3 - camPos);
				(sLeaser.sprites[j] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * num6 + normalized * num3 - camPos);
			}
			vector = vector2;
		}
		float b2 = Mathf.Lerp(lastStretch, stretch, timeStacker);
		vector = Vector2.Lerp(ropeSegments[1].lastPos, ropeSegments[1].pos, timeStacker);
		vector += Custom.DirVec(Vector2.Lerp(ropeSegments[2].lastPos, ropeSegments[2].pos, timeStacker), vector) * 1f;
		for (int k = 1; k < ropeSegments.Length; k++)
		{
			float num7 = (float)k / (float)(ropeSegments.Length - 1);
			Vector2 vector4 = Vector2.Lerp(ropeSegments[k].lastPos, ropeSegments[k].pos, timeStacker);
			Vector2 vector5 = Custom.PerpendicularVector((vector - vector4).normalized);
			float num8 = 0.2f + 1.6f * Mathf.Lerp(1f, b2, Mathf.Pow(Mathf.Sin(num7 * (float)Math.PI), 0.7f));
			(sLeaser.sprites[2] as TriangleMesh).MoveVertice((k - 1) * 4, vector - vector5 * num8 - camPos);
			(sLeaser.sprites[2] as TriangleMesh).MoveVertice((k - 1) * 4 + 1, vector + vector5 * num8 - camPos);
			(sLeaser.sprites[2] as TriangleMesh).MoveVertice((k - 1) * 4 + 2, vector4 - vector5 * num8 - camPos);
			(sLeaser.sprites[2] as TriangleMesh).MoveVertice((k - 1) * 4 + 3, vector4 + vector5 * num8 - camPos);
			vector = vector4;
		}
		vector = Vector2.Lerp(ropeSegments[0].lastPos, ropeSegments[0].pos, timeStacker);
		Vector2 vector6 = Vector2.Lerp(ropeSegments[1].lastPos, ropeSegments[1].pos, timeStacker);
		sLeaser.sprites[3].x = vector.x - camPos.x;
		sLeaser.sprites[3].y = vector.y - camPos.y;
		sLeaser.sprites[3].scaleY = (Vector2.Distance(vector, vector6) + 1f) / 20f;
		sLeaser.sprites[3].rotation = Custom.AimFromOneVectorToAnother(vector, vector6);
		vector = Vector2.Lerp(ropeSegments[ropeSegments.Length - 1].lastPos, ropeSegments[ropeSegments.Length - 1].pos, timeStacker);
		vector6 = Vector2.Lerp(ropeSegments[ropeSegments.Length - 2].lastPos, ropeSegments[ropeSegments.Length - 2].pos, timeStacker);
		sLeaser.sprites[4].x = vector.x - camPos.x;
		sLeaser.sprites[4].y = vector.y - camPos.y;
		sLeaser.sprites[4].scaleY = (Vector2.Distance(vector, vector6) + 1f) / 20f;
		sLeaser.sprites[4].rotation = Custom.AimFromOneVectorToAnother(vector, vector6);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		sLeaser.sprites[1].color = palette.blackColor;
		sLeaser.sprites[0].color = color;
		for (int i = 3; i < 5; i++)
		{
			sLeaser.sprites[i].color = Color.Lerp(palette.fogColor, Custom.HSL2RGB(0.95f, 1f, 0.865f), 0.7f);
		}
		for (int j = 0; j < (sLeaser.sprites[2] as TriangleMesh).verticeColors.Length; j++)
		{
			float num = Mathf.Clamp(Mathf.Sin((float)j / (float)((sLeaser.sprites[2] as TriangleMesh).verticeColors.Length - 1) * (float)Math.PI), 0f, 1f);
			(sLeaser.sprites[2] as TriangleMesh).verticeColors[j] = Color.Lerp(palette.fogColor, Custom.HSL2RGB(Mathf.Lerp(0.95f, 1f, num), 1f, Mathf.Lerp(0.75f, 0.9f, Mathf.Pow(num, 0.15f))), 0.7f);
		}
	}

	private float SegmentStretchFac(int seg, float timeStacker)
	{
		float num = 0f;
		if (seg > 0)
		{
			num += 10f / Vector2.Distance(Vector2.Lerp(bodyParts[seg].lastPos, bodyParts[seg].pos, timeStacker), Vector2.Lerp(bodyParts[seg - 1].lastPos, bodyParts[seg - 1].pos, timeStacker));
		}
		if (seg < 3)
		{
			num += 10f / Vector2.Distance(Vector2.Lerp(bodyParts[seg].lastPos, bodyParts[seg].pos, timeStacker), Vector2.Lerp(bodyParts[seg + 1].lastPos, bodyParts[seg + 1].pos, timeStacker));
		}
		if (seg > 0 && seg < 3)
		{
			num /= 2f;
		}
		if (num < 1f)
		{
			return Mathf.Pow(num, 0.6f);
		}
		return Custom.LerpMap(num, 1f, 3f, 1f, 1.5f);
	}
}
