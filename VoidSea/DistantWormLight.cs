using System;
using RWCustom;
using UnityEngine;

namespace VoidSea;

public class DistantWormLight : VoidSeaScene.VoidSeaSceneElement
{
	private int outsideScreen;

	private Vector2 goalPos;

	private float scale = 8f;

	public Vector2 swimDir;

	public Vector2 vel;

	public Vector2 dragPos;

	public float swimMotion;

	public float graphicalFidelity;

	private float dark;

	public float alpha;

	private VoidSeaScene voidSeaScene => scene as VoidSeaScene;

	private float rad => 350f * scale;

	public DistantWormLight(VoidSeaScene voidSeaScene, float depth, int index)
		: base(voidSeaScene, new Vector2(Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 700f * depth, voidSeaScene.voidWormsAltitude), depth)
	{
		goalPos = pos;
		swimMotion = UnityEngine.Random.value;
		graphicalFidelity = Mathf.InverseLerp(30f, 50f, depth);
		dark = 1f - 1f / (depth * 0.5f);
		dark = Mathf.Lerp(dark, 1f, Mathf.InverseLerp(25f, 32f, depth));
		dark *= 0.5f;
		dark = Mathf.Lerp(dark, 1f, Mathf.InverseLerp(35f, 980f, depth));
		alpha = 1f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (outsideScreen != 0)
		{
			pos.y = voidSeaScene.voidWormsAltitude + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 3000f;
			float num = voidSeaScene.convergencePoint.x + (voidSeaScene.convergencePoint.x + rad / depth) * (float)(-outsideScreen);
			num = (num - voidSeaScene.convergencePoint.x) * depth + voidSeaScene.convergencePoint.x;
			num += room.game.cameras[0].hDisplace - 8f;
			num = num + voidSeaScene.RoomToWorldPos(room.game.cameras[0].pos).x - voidSeaScene.sceneOrigo.x;
			pos.x = num;
			outsideScreen = 0;
			outsideScreen = 0;
		}
		AbstractCreature firstAlivePlayer = voidSeaScene.room.game.FirstAlivePlayer;
		if (voidSeaScene.room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && Custom.DistLess(pos, goalPos, 400f * scale))
		{
			goalPos = new Vector2(firstAlivePlayer.realizedCreature.mainBodyChunk.pos.x + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 1700f * depth, voidSeaScene.voidWormsAltitude + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 3000f);
		}
		if (UnityEngine.Random.value < 0.005f)
		{
			goalPos = new Vector2(pos.x + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 3000f, voidSeaScene.voidWormsAltitude + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 3000f);
		}
		swimMotion -= 1f / 60f;
		swimDir = Vector3.Slerp(swimDir, Custom.DirVec(pos, goalPos), 0.3f);
		swimDir = Custom.DegToVec(Custom.VecToDeg(swimDir) + Mathf.Sin(swimMotion * (float)Math.PI * 2f) * 20f);
		vel += 0.25f * scale * swimDir;
		vel += Custom.DirVec(dragPos, pos) * scale * 2.2f;
		pos += vel;
		vel *= 0.8f;
		dragPos -= swimDir * scale * 8f;
		dragPos = pos + Custom.DirVec(pos, dragPos) * scale * 50f;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		int num = 1;
		if (graphicalFidelity > 1f / 3f)
		{
			num++;
		}
		if (graphicalFidelity > 2f / 3f)
		{
			num++;
		}
		sLeaser.sprites = new FSprite[num];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["FlatWaterLight"];
		sLeaser.sprites[0].scale = scale * 350f / (8f * depth);
		sLeaser.sprites[0].color = new Color(1f - dark, 1f - dark, 1f - dark);
		if (num > 1)
		{
			sLeaser.sprites[1] = new FSprite("Futile_White");
			sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["FlatWaterLight"];
			sLeaser.sprites[1].scale = scale * 300f / (8f * depth);
			sLeaser.sprites[1].alpha = Mathf.InverseLerp(0f, 1f / 3f, graphicalFidelity);
			sLeaser.sprites[1].color = new Color(1f - dark, 1f - dark, 1f - dark);
		}
		if (num > 2)
		{
			sLeaser.sprites[2] = new FSprite("Futile_White");
			sLeaser.sprites[2].shader = rCam.game.rainWorld.Shaders["FlatWaterLight"];
			sLeaser.sprites[2].scale = scale * 150f / (8f * depth);
			sLeaser.sprites[2].alpha = Mathf.InverseLerp(1f / 3f, 2f / 3f, graphicalFidelity);
			sLeaser.sprites[2].color = new Color(1f - dark, 1f - dark, 1f - dark);
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = DrawPos(camPos, rCam.hDisplace);
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].x = vector.x;
			sLeaser.sprites[i].y = vector.y;
			sLeaser.sprites[i].alpha = alpha;
		}
		if (vector.x < (0f - rad / depth) * 2f)
		{
			outsideScreen = -1;
		}
		else if (vector.x > rCam.game.rainWorld.screenSize.x + rad / depth * 2f)
		{
			outsideScreen = 1;
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}
}
