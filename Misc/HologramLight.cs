using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class HologramLight : UpdatableAndDeletable, IDrawable
{
	public Vector2 pos;

	public Vector2 lastPos;

	private Vector2 lastDisplace;

	private Vector2 displace;

	private Vector2 displaceGoal;

	private Vector2 lastPushAroundPos;

	private Vector2 pushAroundPos;

	public Vector2 projPos;

	public Vector2 lastProjPos;

	private float power;

	private float lastPower;

	public int outOfRangeCounter;

	public int notNeededCounter;

	private float flicker;

	private float lastFlicker;

	private float flickerFac;

	public Player player;

	private Overseer overseer;

	public Vector2 inFront;

	private bool powerDownAndKill;

	public bool respawn;

	public int activeLinger;

	public LightSource lightsource;

	public bool projectorActive
	{
		get
		{
			if (overseer.room == room)
			{
				return overseer.mode != Overseer.Mode.Zipping;
			}
			return false;
		}
	}

	public Vector2 OverseerEyePos(float timeStacker)
	{
		if (overseer.graphicsModule == null || overseer.room == null)
		{
			return Vector2.Lerp(overseer.mainBodyChunk.lastPos, overseer.mainBodyChunk.pos, timeStacker);
		}
		return (overseer.graphicsModule as OverseerGraphics).DrawPosOfSegment(0f, timeStacker);
	}

	public HologramLight(Player player, Overseer overseer)
	{
		this.player = player;
		this.overseer = overseer;
		overseer.AI.communication.holoLight = this;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastPos = pos;
		lastPower = power;
		lastDisplace = displace;
		lastFlicker = flicker;
		lastProjPos = projPos;
		lastPushAroundPos = pushAroundPos;
		projPos = OverseerEyePos(1f);
		pushAroundPos *= 0.8f;
		if (overseer.extended > 0f)
		{
			pushAroundPos += (overseer.firstChunk.pos - overseer.firstChunk.lastPos) * overseer.extended;
		}
		if (!Custom.DistLess(projPos, pos, 1700f))
		{
			outOfRangeCounter++;
		}
		else
		{
			outOfRangeCounter -= 10;
		}
		outOfRangeCounter = Custom.IntClamp(outOfRangeCounter, 0, 200);
		displace = Vector2.Lerp(Custom.MoveTowards(displace, displaceGoal, 2f), displaceGoal, 0.05f);
		if (UnityEngine.Random.value < 1f / 15f)
		{
			displaceGoal += Custom.RNV() * UnityEngine.Random.value * 20f;
			if (displaceGoal.magnitude > 60f)
			{
				displaceGoal = Custom.RNV() * UnityEngine.Random.value * UnityEngine.Random.value * 60f;
			}
		}
		if (UnityEngine.Random.value < Custom.LerpMap(Vector2.Distance(pos, projPos), 500f, 1200f, 0.005f, 1f / 30f))
		{
			flickerFac = UnityEngine.Random.value;
		}
		flickerFac = Mathf.Max(0f, flickerFac - 1f / 30f);
		if (UnityEngine.Random.value < 0.6f * flickerFac)
		{
			flicker = Mathf.Lerp(flicker, 0.5f + 0.5f * flickerFac, UnityEngine.Random.value);
		}
		else
		{
			flicker = Mathf.Max(0f, flicker - 0.1f);
		}
		if (projectorActive)
		{
			activeLinger = 20;
		}
		else if (activeLinger > 0)
		{
			activeLinger--;
		}
		if (lightsource == null)
		{
			lightsource = new LightSource(pos, environmentalLight: false, new Color(1f, 0.8f, 0.3f), this);
			room.AddObject(lightsource);
		}
		else if (lightsource.slatedForDeletetion || lightsource.room != room)
		{
			lightsource.Destroy();
			lightsource = null;
		}
		if (player.room == room && !player.enteringShortCut.HasValue && activeLinger > 0 && !powerDownAndKill)
		{
			if (respawn)
			{
				if (power <= 0f && lastPower <= 0f)
				{
					respawn = false;
					pos = GoalPos();
					inFront *= 0f;
				}
				else
				{
					power = Mathf.Max(0f, power - 1f / 30f);
				}
			}
			else
			{
				inFront = Vector2.Lerp(inFront, player.mainBodyChunk.pos - player.mainBodyChunk.lastPos, 0.1f);
				pos = Vector2.Lerp(Custom.MoveTowards(pos, GoalPos(), 7f), GoalPos(), 0.1f);
				power = Mathf.Min((projectorActive ? 1f : 0.9f) * Mathf.InverseLerp(200f, 150f, outOfRangeCounter) * Mathf.InverseLerp(80f, 50f, notNeededCounter), power + 1f / 30f);
			}
		}
		else
		{
			respawn = true;
			power = Mathf.Max(0f, power - 1f / 30f);
			if (powerDownAndKill && power <= 0f && lastPower <= 0f)
			{
				Destroy();
			}
			if (!projectorActive)
			{
				activeLinger = 0;
			}
		}
		float num = Needed(player);
		if (num < 0.1f)
		{
			notNeededCounter++;
		}
		else if (num > 0.4f)
		{
			notNeededCounter -= 10;
		}
		else if (num > 0.2f)
		{
			notNeededCounter--;
		}
		notNeededCounter = Custom.IntClamp(notNeededCounter, 0, 500);
		if ((player.room != null && player.room != room) || overseer.AI.communication.holoLight != this || notNeededCounter >= 500)
		{
			powerDownAndKill = true;
		}
	}

	public Vector2 GoalPos()
	{
		return Vector2.Lerp(player.mainBodyChunk.pos, player.bodyChunks[1].pos, 0.3f) + Vector2.ClampMagnitude(inFront * 20f, 60f) * power;
	}

	public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[10];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].color = new Color(1f, 0.8f, 0.3f);
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["HoloGrid"];
		for (int i = 1; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i] = new FSprite("pixel");
			sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["Hologram"];
			sLeaser.sprites[i].color = new Color(0.87f, 0.72f, 0.35f);
			sLeaser.sprites[i].anchorY = 0f;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (room != null)
		{
			float num = Mathf.Lerp(lastFlicker, flicker, timeStacker);
			float num2 = Mathf.Lerp(lastPower, power, timeStacker) * (1f - Mathf.Pow(num, 2f + UnityEngine.Random.value * 2f));
			float num3 = Mathf.Lerp(lastPower, power, timeStacker) * (1f - 0.05f * num);
			Vector2 vector = ((overseer.room == room) ? OverseerEyePos(timeStacker) : Vector2.Lerp(lastProjPos, projPos, timeStacker));
			Vector2 b = Vector2.Lerp(lastPos, pos, timeStacker) + Vector2.Lerp(lastDisplace, displace, timeStacker);
			b = Vector2.Lerp(vector, b, num3) + Vector2.Lerp(lastPushAroundPos, pushAroundPos, timeStacker);
			num2 *= Custom.LerpMap(Vector2.Distance(b, vector), 500f, 1200f, 1f, 0.75f, 0.6f);
			Vector2 vector2 = Vector2.ClampMagnitude(vector - b, 240f) / 240f;
			sLeaser.sprites[0].x = b.x - camPos.x;
			sLeaser.sprites[0].y = b.y - camPos.y;
			sLeaser.sprites[0].scaleX = Mathf.Lerp(15f, 20f, num2 * num3) + Mathf.Sin(num2 * num3 * (float)Math.PI) * 30f * ((ModManager.MMF && MMF.cfgLargeHologramLight.Value) ? 2f : 1f);
			sLeaser.sprites[0].scaleY = Mathf.Lerp(8f, 20f, num2 * num3) * ((ModManager.MMF && MMF.cfgLargeHologramLight.Value) ? 2f : 1f);
			sLeaser.sprites[0].color = new Color(Mathf.InverseLerp(-1f, 1f, vector2.x), Mathf.InverseLerp(-1f, 1f, vector2.y), num3, num2);
			float num4 = 8f * Mathf.Lerp(6f, 16f, num2 * num3) * ((ModManager.MMF && MMF.cfgLargeHologramLight.Value) ? 2f : 1f);
			if (lightsource != null)
			{
				lightsource.HardSetPos(b);
				lightsource.HardSetRad(num4 * 2f);
				lightsource.HardSetAlpha(Mathf.InverseLerp(0.75f, 1f, num2) * 0.1f);
			}
			for (int i = 1; i < sLeaser.sprites.Length; i++)
			{
				if (num2 < 0.5f)
				{
					sLeaser.sprites[i].isVisible = false;
					continue;
				}
				Vector2 vector3 = b + Custom.RNV() * Mathf.Pow(UnityEngine.Random.value, 0.65f) * num4;
				float num5 = 0.75f;
				if (room.GetTile(vector3).Solid)
				{
					num5 = 0f;
				}
				else if (UnityEngine.Random.value < Custom.LerpMap(num3, 0.5f, 1f, 0.85f, 0.25f))
				{
					num5 = 0.75f;
				}
				else if (UnityEngine.Random.value < 0.5f && room.GetTile(vector3).verticalBeam)
				{
					vector3.x = room.MiddleOfTile(vector3).x + ((UnityEngine.Random.value < 0.5f) ? (-2f) : 2f);
					num5 = 1f;
				}
				else if (UnityEngine.Random.value < 0.5f && room.GetTile(vector3).horizontalBeam)
				{
					vector3.y = room.MiddleOfTile(vector3).y + ((UnityEngine.Random.value < 0.5f) ? (-2f) : 2f);
					num5 = 1f;
				}
				else
				{
					int num6 = UnityEngine.Random.Range(0, 4);
					for (int j = 0; j < 4; j++)
					{
						if (!Custom.DistLess(vector, vector3 + Custom.fourDirections[num6].ToVector2() * 20f, num4))
						{
							num5 = 0f;
							break;
						}
						if (room.GetTile(vector3 + Custom.fourDirections[num6].ToVector2() * 20f).Solid)
						{
							vector3 = room.MiddleOfTile(vector3 + Custom.fourDirections[num6].ToVector2() * 20f) - Custom.fourDirections[num6].ToVector2() * 10f;
							num5 = 1f;
							break;
						}
					}
				}
				if (num5 > 0f)
				{
					Vector2 vector4 = Vector2.Lerp(vector3, vector, Mathf.Pow(UnityEngine.Random.value, 3f - 1.5f * num2));
					sLeaser.sprites[i].isVisible = true;
					sLeaser.sprites[i].x = vector4.x - camPos.x;
					sLeaser.sprites[i].y = vector4.y - camPos.y;
					sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(vector4, vector3);
					sLeaser.sprites[i].scaleY = Vector2.Distance(vector4, vector3);
					sLeaser.sprites[i].alpha = num5 * Mathf.Pow(UnityEngine.Random.value, 0.2f) * Mathf.InverseLerp(0.5f, 0.6f, num2);
				}
				else
				{
					sLeaser.sprites[i].isVisible = false;
				}
			}
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public static float Needed(Player player)
	{
		if (player.room == null || player.room.Darkness(player.mainBodyChunk.pos) < 0.85f || player.dead || player.room.game.cameras[0].room != player.room)
		{
			return 0f;
		}
		for (int i = 0; i < player.grasps.Length; i++)
		{
			if (player.grasps[i] != null)
			{
				if (player.grasps[i].grabbed is Lantern)
				{
					return 0f;
				}
				if (player.grasps[i].grabbed is LanternMouse && (player.grasps[i].grabbed as LanternMouse).State.battery > 200)
				{
					return 0f;
				}
			}
		}
		float num = Mathf.InverseLerp(0.85f, 0.92f, player.room.Darkness(player.mainBodyChunk.pos));
		for (int j = 0; j < player.room.lightSources.Count; j++)
		{
			if (player.room.lightSources[j].Rad > 120f && player.room.lightSources[j].Alpha > 0.2f && Custom.DistLess(player.mainBodyChunk.pos, player.room.lightSources[j].Pos, player.room.lightSources[j].rad + 200f))
			{
				num -= Mathf.InverseLerp(player.room.lightSources[j].rad + 200f, player.room.lightSources[j].rad * 0.8f, Vector2.Distance(player.mainBodyChunk.pos, player.room.lightSources[j].Pos));
				if (num <= 0f)
				{
					return 0f;
				}
			}
		}
		return num;
	}

	public static void TryCreate(Player player)
	{
		if (player.room == null)
		{
			return;
		}
		Overseer overseer = null;
		for (int i = 0; i < player.room.abstractRoom.creatures.Count; i++)
		{
			if (player.room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.Overseer && (player.room.abstractRoom.creatures[i].abstractAI as OverseerAbstractAI).playerGuide)
			{
				if (player.room.abstractRoom.creatures[i].realizedCreature != null && player.room.abstractRoom.creatures[i].realizedCreature.Consious && player.room.abstractRoom.creatures[i].realizedCreature.room == player.room && (player.room.abstractRoom.creatures[i].realizedCreature as Overseer).AI.communication != null)
				{
					overseer = player.room.abstractRoom.creatures[i].realizedCreature as Overseer;
				}
				break;
			}
		}
		if (overseer != null)
		{
			if (overseer.AI.communication.holoLight == null)
			{
				player.room.AddObject(new HologramLight(player, overseer));
			}
		}
		else
		{
			BringGuideToPlayerEvent.BringGuide(player.room.world, -0.8f);
		}
	}

	public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Water");
		}
		FSprite[] sprites = sLeaser.sprites;
		foreach (FSprite fSprite in sprites)
		{
			fSprite.RemoveFromContainer();
			newContatiner.AddChild(fSprite);
		}
	}
}
