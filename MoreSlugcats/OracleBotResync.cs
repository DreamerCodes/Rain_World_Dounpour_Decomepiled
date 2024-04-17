using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class OracleBotResync : CosmeticSprite
{
	public class Connection
	{
		public OracleBotResync parent;

		public Vector2 stuckAt;

		public Vector2 handle;

		public float lightUp;

		public float lastLightUp;

		public Connection(OracleBotResync parent, Vector2 stuckAt)
		{
			this.parent = parent;
			Vector2 b = stuckAt;
			b.x = Mathf.Clamp(b.x, parent.oracle.arm.cornerPositions[0].x, parent.oracle.arm.cornerPositions[1].x);
			b.y = Mathf.Clamp(b.y, parent.oracle.arm.cornerPositions[2].y, parent.oracle.arm.cornerPositions[1].y);
			this.stuckAt = Vector2.Lerp(stuckAt, b, 0.5f);
			handle = stuckAt + Custom.RNV() * Mathf.Lerp(400f, 700f, Random.value);
		}
	}

	public Oracle oracle;

	public int timer;

	private int[] timings;

	private int[] citizensIDSequence;

	public float botSlider;

	public int glyphIndex;

	private int glyphTicker;

	public Connection[] connections;

	public int lightUpIndex;

	public OracleBotResync(Oracle oracle)
	{
		this.oracle = oracle;
		glyphIndex = -1;
		timings = new int[5] { 120, 200, 500, 880, 1000 };
		Random.InitState(50);
		citizensIDSequence = new int[9];
		for (int i = 0; i < citizensIDSequence.Length; i++)
		{
			citizensIDSequence[i] = Random.Range(0, 14);
		}
		connections = new Connection[20];
		for (int j = 0; j < connections.Length; j++)
		{
			connections[j] = new Connection(this, new Vector2(oracle.room.PixelWidth / 2f, oracle.room.PixelHeight / 2f) + Custom.RNV() * Mathf.Lerp(300f, 500f, Random.value));
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		Player player = oracle.oracleBehavior.player;
		if (player != null && player.myRobot != null)
		{
			if (player.myRobot.Pos.x >= oracle.firstChunk.pos.x)
			{
				pos = new Vector2(player.myRobot.Pos.x + 50f, player.myRobot.Pos.y);
			}
			else
			{
				pos = new Vector2(player.myRobot.Pos.x - 50f, player.myRobot.Pos.y);
			}
		}
		for (int i = 0; i < connections.Length; i++)
		{
			connections[i].lastLightUp = connections[i].lightUp;
			connections[i].lightUp *= 0.9f;
			if (Vector2.Distance(connections[i].stuckAt, connections[i].handle) > 100f)
			{
				connections[i].handle = new Vector2((connections[i].stuckAt.x + connections[i].handle.x) / 2f, (connections[i].stuckAt.y + connections[i].handle.y) / 2f);
			}
		}
		timer++;
		if (timer == 1)
		{
			if (oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights) == null)
			{
				oracle.room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.DarkenLights, 0f, inherited: false));
			}
			if (oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness) == null)
			{
				oracle.room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Darkness, 0f, inherited: false));
			}
			if (oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast) == null)
			{
				oracle.room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Contrast, 0f, inherited: false));
			}
			oracle.suppressConnectionFires = true;
		}
		if (timer < timings[0])
		{
			float t = (float)timer / (float)timings[0];
			oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights).amount = Mathf.Lerp(0f, 0.35f, t);
			oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness).amount = Mathf.Lerp(0f, 0.2f, t);
			oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast).amount = Mathf.Lerp(0f, 0.05f, t);
		}
		if (timer > timings[1] && timer < timings[timings.Length - 1] && timer % 5 == 0)
		{
			connections[lightUpIndex].lightUp = 1f;
			if (player != null && player.myRobot != null)
			{
				connections[lightUpIndex].stuckAt = player.myRobot.Pos;
			}
			oracle.room.PlaySound(SoundID.SS_AI_Halo_Connection_Light_Up, 0f, 1f * (1f - oracle.noiseSuppress), 1f);
			lightUpIndex++;
			if (lightUpIndex >= connections.Length)
			{
				lightUpIndex = 0;
			}
		}
		if (timer >= timings[2] && timer < timings[3])
		{
			glyphTicker++;
			if (glyphTicker % 15 == 0)
			{
				glyphIndex++;
				if (glyphIndex < 19 && glyphIndex % 2 == 1)
				{
					room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Data_Bit, pos, 1f, 1f + Random.value * 2f);
				}
			}
		}
		if (timer >= timings[3] && timer < timings[4] && timer % 10 == 0)
		{
			if (glyphIndex == -1)
			{
				oracle.room.PlaySound(SoundID.SS_AI_Text, 0f, 1.5f, 1f);
				glyphIndex = 21;
			}
			else
			{
				glyphIndex = -1;
			}
		}
		if (timer == timings[timings.Length - 1] && player != null && player.myRobot != null)
		{
			Vector2 vector = player.myRobot.Pos;
			for (int j = 0; j < 5; j++)
			{
				oracle.room.AddObject(new Spark(vector, Custom.RNV(), Color.white, null, 16, 24));
			}
			oracle.room.AddObject(new Explosion.ExplosionLight(vector, 150f, 1f, 8, Color.white));
			oracle.room.AddObject(new ShockWave(vector, 60f, 0.1f, 8));
			oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, 0f, 1f, 1.5f + Random.value * 0.5f);
		}
		if (timer > timings[timings.Length - 1])
		{
			glyphIndex = -1;
			float t2 = (float)(timer - timings[timings.Length - 1]) / (float)timings[0];
			oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights).amount = Mathf.Lerp(0.35f, 0f, t2);
			oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness).amount = Mathf.Lerp(0.2f, 0f, t2);
			oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast).amount = Mathf.Lerp(0.05f, 0f, t2);
		}
		if (!base.slatedForDeletetion && player != null && player.myRobot != null && timer > timings[1] && timer < timings[timings.Length - 1])
		{
			player.myRobot.lockTarget = Vector2.Lerp(player.firstChunk.pos, oracle.firstChunk.pos, botSlider);
			AncientBot myRobot = player.myRobot;
			myRobot.lockTarget += Custom.PerpendicularVector(player.myRobot.lockTarget.Value) * Mathf.Lerp(3f, 65f, Mathf.Abs(botSlider - 0.5f) * 2f);
		}
		if (timer == timings[timings.Length - 1] + timings[0])
		{
			if (player != null && player.myRobot != null)
			{
				player.myRobot.lockTarget = null;
			}
			oracle.suppressConnectionFires = false;
			Destroy();
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[9 + connections.Length];
		for (int i = 0; i < 9; i++)
		{
			sLeaser.sprites[i] = new FSprite("pixel");
			sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["GateHologram"];
			sLeaser.sprites[i].scale = 2f;
			sLeaser.sprites[i].isVisible = false;
		}
		for (int j = 0; j < connections.Length; j++)
		{
			sLeaser.sprites[9 + j] = TriangleMesh.MakeLongMesh(10, pointyTip: false, customColor: false);
			sLeaser.sprites[9 + j].color = new Color(0f, 0f, 0f);
		}
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("BackgroundShortcuts"));
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = new Vector2(Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x - 16f, Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y - 18f);
		for (int i = 0; i < 9; i++)
		{
			sLeaser.sprites[i].x = vector.x + (float)(i % 3 * 18);
			sLeaser.sprites[i].y = vector.y + (float)(i / 3 * 18);
			if (glyphIndex < i * 2)
			{
				sLeaser.sprites[i].isVisible = false;
			}
			else if (glyphIndex == i * 2)
			{
				sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName("TinyGlyph" + Random.Range(0, 14));
				sLeaser.sprites[i].isVisible = true;
			}
			else if (glyphIndex > i * 2)
			{
				sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName("TinyGlyph" + citizensIDSequence[i]);
				sLeaser.sprites[i].isVisible = true;
			}
		}
		int num = 9;
		for (int j = 0; j < connections.Length; j++)
		{
			if (connections[j].lastLightUp > 0.05f || connections[j].lightUp > 0.05f)
			{
				Vector2 vector2 = connections[j].stuckAt;
				float num2 = 2f * Mathf.Lerp(connections[j].lastLightUp, connections[j].lightUp, timeStacker);
				for (int k = 0; k < 10; k++)
				{
					float f = (float)k / 9f;
					Vector2 vector3 = oracle.firstChunk.pos;
					Vector2 vector4 = Custom.DirVec(vector3, connections[j].stuckAt);
					Vector2 vector5 = Custom.Bezier(connections[j].stuckAt, connections[j].handle, vector3, vector3 + vector4 * 20f, f);
					Vector2 vector6 = Custom.DirVec(vector2, vector5);
					Vector2 vector7 = Custom.PerpendicularVector(vector6);
					float num3 = Vector2.Distance(vector2, vector5);
					(sLeaser.sprites[num + j] as TriangleMesh).MoveVertice(k * 4, vector5 - vector6 * num3 * 0.3f - vector7 * num2 - camPos);
					(sLeaser.sprites[num + j] as TriangleMesh).MoveVertice(k * 4 + 1, vector5 - vector6 * num3 * 0.3f + vector7 * num2 - camPos);
					(sLeaser.sprites[num + j] as TriangleMesh).MoveVertice(k * 4 + 2, vector5 - vector7 * num2 - camPos);
					(sLeaser.sprites[num + j] as TriangleMesh).MoveVertice(k * 4 + 3, vector5 + vector7 * num2 - camPos);
					vector2 = vector5;
				}
			}
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}
}
