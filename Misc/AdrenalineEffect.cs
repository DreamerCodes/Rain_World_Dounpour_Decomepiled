using System;
using UnityEngine;

public class AdrenalineEffect : CosmeticSprite
{
	private float intensity;

	private float lastIntensity;

	private Player player;

	private Vector2 smoothedVel;

	private float sinCounter;

	public AdrenalineEffect(Player player)
	{
		intensity = 0f;
		lastIntensity = 0f;
		this.player = player;
		pos = player.mainBodyChunk.pos;
		lastPos = pos;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		pos = player.mainBodyChunk.pos;
		smoothedVel = Vector2.Lerp(smoothedVel, player.mainBodyChunk.vel, 0.2f);
		lastIntensity = intensity;
		sinCounter += 1f / Mathf.Lerp(50f, 10f, Mathf.InverseLerp(3f, 20f, smoothedVel.magnitude));
		if (sinCounter > 1f)
		{
			sinCounter -= 1f;
		}
		float num = 0.1f + 0.9f * Mathf.Pow(Mathf.InverseLerp(0.1f, 20f, smoothedVel.magnitude), 2f);
		intensity = (1f - num) * Mathf.InverseLerp(0.1f, 20f, smoothedVel.magnitude);
		intensity += num * (0.5f + 0.5f * Mathf.Sin(sinCounter * (float)Math.PI * 2f));
		intensity = Mathf.Pow(intensity, Mathf.InverseLerp(18f, 3f, smoothedVel.magnitude));
		intensity *= player.Adrenaline;
		if ((player.room != null && player.room != room) || player.Adrenaline == 0f)
		{
			Destroy();
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["Adrenaline"];
		sLeaser.sprites[0].color = new Color(0f, 0.5f, 1f);
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
		Vector2 normalized = smoothedVel.normalized;
		sLeaser.sprites[0].color = new Color(Mathf.InverseLerp(-1f, 1f, normalized.x), Mathf.InverseLerp(-1f, 1f, 0f - normalized.y), Mathf.InverseLerp(0.2f, 6f, smoothedVel.magnitude));
		sLeaser.sprites[0].alpha = Mathf.Lerp(lastIntensity, intensity, timeStacker);
		sLeaser.sprites[0].scale = Mathf.Lerp(110f, 80f, Mathf.Lerp(lastIntensity, intensity, timeStacker)) / 8f;
		sLeaser.sprites[0].isVisible = intensity > 0f;
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("HUD");
		}
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}
}
