using RWCustom;
using UnityEngine;

public class Ant : CosmeticInsect
{
	private Vector2 dir;

	private Vector2 lastDir;

	private float dp;

	private Vector2 runDir;

	private Vector2 spawnPos;

	private bool running;

	private int runCounter;

	public Ant(Room room, Vector2 pos)
		: base(room, pos, Type.Ant)
	{
		if (Random.value < 0.65f)
		{
			dp = ((Random.value < 0.5f) ? (11f / 30f) : 0.7f);
		}
		else
		{
			Mathf.Lerp(1f / 30f, 29f / 30f, Random.value);
		}
		spawnPos = pos;
	}

	public override void Update(bool eu)
	{
		lastDir = runDir;
		vel *= 0.8f;
		base.Update(eu);
	}

	public override void Act()
	{
		base.Act();
		if (running)
		{
			vel += runDir * 0.4f;
			if (wantToBurrow)
			{
				runCounter = 100;
				runDir = Vector3.Slerp(runDir, new Vector2(0f, -1f), 0.05f * Random.value);
				if (pos.x < 0f)
				{
					Destroy();
				}
			}
			else
			{
				runDir += Custom.RNV() * Random.value * 0.9f;
				runDir = Vector2.Lerp(runDir, Custom.DirVec(pos, spawnPos), Mathf.InverseLerp(300f, 400f, Vector2.Distance(pos, spawnPos)));
				runDir.Normalize();
			}
			dir = Custom.DirVec(lastPos, pos);
		}
		runCounter--;
		if (runCounter < 1)
		{
			running = !running;
			runCounter = Random.Range(5, running ? 200 : 80);
		}
	}

	public override void EmergeFromGround(Vector2 emergePos)
	{
		base.EmergeFromGround(emergePos);
		pos = emergePos + new Vector2(0f, 3f);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("pixel");
		sLeaser.sprites[0].scale = 5f;
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["SpecificDepth"];
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		float num = Mathf.Lerp(lastInGround, inGround, timeStacker);
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		vector = Custom.ApplyDepthOnVector(vector - camPos, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * (2f / 3f)), dp * 30f);
		vector.y -= 5f * num;
		sLeaser.sprites[0].x = vector.x;
		sLeaser.sprites[0].y = vector.y;
		sLeaser.sprites[0].rotation = Custom.VecToDeg(Vector3.Slerp(lastDir, dir, timeStacker));
		sLeaser.sprites[0].scaleX = 2f;
		sLeaser.sprites[0].scaleY = 4f;
		sLeaser.sprites[0].alpha = 1f - dp;
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		sLeaser.sprites[0].color = Color.Lerp(Color.Lerp(palette.texture.GetPixel((int)(30f * dp), 2), palette.blackColor, 0.5f), palette.fogColor, dp * (0.3f + 0.2f * palette.fogAmount));
	}
}
