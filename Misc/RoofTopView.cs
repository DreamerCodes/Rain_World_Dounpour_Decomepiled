using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class RoofTopView : BackgroundScene
{
	public class Floor : BackgroundSceneElement
	{
		public string assetName;

		private float fromDepth;

		private float toDepth;

		private Vector2 elementSize;

		private RoofTopView RoofTopScene => scene as RoofTopView;

		public Floor(RoofTopView roofTopScene, string assetName, Vector2 pos, float fromDepth, float toDepth)
			: base(roofTopScene, pos, toDepth)
		{
			this.assetName = assetName;
			this.fromDepth = fromDepth;
			this.toDepth = toDepth;
			scene.LoadGraphic(assetName, crispPixels: true, clampWrapMode: false);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite(assetName);
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["BkgFloor"];
			sLeaser.sprites[0].anchorY = 1f;
			elementSize = Futile.atlasManager.GetElementWithName(assetName).sourcePixelSize;
			sLeaser.sprites[0].scaleX = 1400f / elementSize.x;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = scene.DrawPos(pos, toDepth, camPos, rCam.hDisplace);
			Vector2 vector2 = scene.DrawPos(pos, fromDepth, camPos, rCam.hDisplace);
			sLeaser.sprites[0].x = rCam.game.rainWorld.screenSize.x / 2f;
			sLeaser.sprites[0].y = vector.y;
			sLeaser.sprites[0].scaleY = (vector.y - vector2.y) / elementSize.y;
			sLeaser.sprites[0].color = new Color(1f / fromDepth, 1f / toDepth, 0f);
			Shader.SetGlobalVector(RainWorld.ShadPropWorldCamPos, camPos - scene.sceneOrigo);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class Building : BackgroundSceneElement
	{
		private float scale;

		private Vector2 elementSize;

		private string assetName;

		public Building(RoofTopView roofTopScene, string assetName, Vector2 pos, float depth, float scale)
			: base(roofTopScene, pos, depth)
		{
			base.depth = depth;
			this.scale = scale;
			this.assetName = assetName;
			roofTopScene.LoadGraphic(assetName, crispPixels: false, clampWrapMode: true);
			elementSize = Futile.atlasManager.GetElementWithName(assetName).sourceSize;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite(assetName);
			sLeaser.sprites[0].scale = scale;
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["House"];
			sLeaser.sprites[0].anchorY = 0f;
			if ((scene as RoofTopView).isLC)
			{
				sLeaser.sprites[0].anchorY = -0.02f;
				sLeaser.sprites[0].anchorX = 0.5f;
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = DrawPos(camPos, rCam.hDisplace);
			sLeaser.sprites[0].x = vector.x;
			sLeaser.sprites[0].y = vector.y;
			sLeaser.sprites[0].color = new Color(elementSize.x * scale / 4000f, elementSize.y * scale / 1500f, 1f / depth);
			if ((scene as RoofTopView).isLC)
			{
				sLeaser.sprites[0].scale = 5f;
				sLeaser.sprites[0].color = new Color(elementSize.x * scale / 4000f, elementSize.y * scale / 1500f, 1f / (depth / 20f));
			}
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class Rubble : BackgroundSceneElement
	{
		public float scale = 1f;

		private string assetName;

		private float randomValue;

		private RoofTopView roofTopScene => scene as RoofTopView;

		public Rubble(RoofTopView roofTopScene, string assetName, Vector2 pos, float depth, int index)
			: base(roofTopScene, pos, depth)
		{
			base.depth = depth;
			this.assetName = assetName;
			roofTopScene.LoadGraphic(assetName, crispPixels: true, clampWrapMode: false);
			randomValue = roofTopScene.room.game.SeededRandom(index);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite(assetName);
			sLeaser.sprites[0].scaleY = 1f / depth;
			sLeaser.sprites[0].scaleX = scale;
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["DistantBkgObjectRepeatHorizontal"];
			sLeaser.sprites[0].anchorY = 0f;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = DrawPos(camPos, rCam.hDisplace);
			sLeaser.sprites[0].x = rCam.game.rainWorld.screenSize.x / 2f;
			sLeaser.sprites[0].y = vector.y;
			sLeaser.sprites[0].color = new Color(roofTopScene.AtmosphereColorAtDepth(depth), 1f / depth, 1f / scale, randomValue);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class DistantBuilding : BackgroundSceneElement
	{
		public string assetName;

		public float atmosphericalDepthAdd;

		private RoofTopView RoofTopScene => scene as RoofTopView;

		public DistantBuilding(RoofTopView roofTopScene, string assetName, Vector2 pos, float depth, float atmosphericalDepthAdd)
			: base(roofTopScene, pos, depth)
		{
			this.assetName = assetName;
			this.atmosphericalDepthAdd = atmosphericalDepthAdd;
			scene.LoadGraphic(assetName, crispPixels: true, clampWrapMode: false);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite(assetName);
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["DistantBkgObject"];
			sLeaser.sprites[0].anchorY = 0f;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = DrawPos(camPos, rCam.hDisplace);
			sLeaser.sprites[0].x = vector.x;
			sLeaser.sprites[0].y = vector.y;
			sLeaser.sprites[0].color = new Color(RoofTopScene.AtmosphereColorAtDepth(depth), 0f, 0f);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class Smoke : BackgroundSceneElement
	{
		private float flattened;

		private float alpha;

		private float shaderInputColor;

		private float randomOffset;

		private bool shaderType;

		public Smoke(RoofTopView roofTopView, Vector2 pos, float depth, int index, float flattened, float alpha, float shaderInputColor, bool shaderType)
			: base(roofTopView, pos, depth)
		{
			this.flattened = flattened;
			this.alpha = alpha;
			this.shaderInputColor = shaderInputColor;
			this.shaderType = shaderType;
			randomOffset = UnityEngine.Random.value;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("smoke1");
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders[shaderType ? "Dust" : "CloudDistant"];
			sLeaser.sprites[0].anchorY = 0f;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			float num = 2f;
			float y = DrawPos(camPos, rCam.hDisplace).y;
			sLeaser.sprites[0].scaleY = flattened * num;
			sLeaser.sprites[0].scaleX = num;
			sLeaser.sprites[0].color = new Color(shaderInputColor, randomOffset, Mathf.Lerp(flattened, 1f, 0.5f), alpha);
			sLeaser.sprites[0].x = 683f;
			sLeaser.sprites[0].y = y;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class DistantGhost : BackgroundSceneElement
	{
		public Vector2 lastHandleA;

		public Vector2 handleA;

		public Vector2 lastHandleB;

		public Vector2 handleB;

		public Vector2 getToHandleA;

		public Vector2 getToHandleB;

		public Vector2 headLastPos;

		public Vector2 headPos;

		public Vector2 headVel;

		public Vector2 gravitateDir;

		public int segs = 20;

		private RoofTopView roofTopView => scene as RoofTopView;

		private float DisplayDepth => Custom.LerpMap(depth, 2.2f, 10.5f, 1.5f, 25f);

		public DistantGhost(RoofTopView roofTopView, Vector2 pos, float depth, int index)
			: base(roofTopView, pos, depth)
		{
			handleB = Custom.DegToVec(Mathf.Lerp(-45f, 45f, UnityEngine.Random.value));
			handleA = Custom.DegToVec(180f + Mathf.Lerp(-45f, 45f, UnityEngine.Random.value));
			headPos = new Vector2(0f, 200f);
			headLastPos = pos;
			gravitateDir = Custom.RNV();
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			headLastPos = headPos;
			headPos += headVel;
			Vector2 vector = new Vector2(0f, 200f);
			headVel *= 0.95f;
			headVel += Vector2.ClampMagnitude(vector - headPos, 10f) / 200f;
			headVel += gravitateDir * 0.05f;
			gravitateDir = (gravitateDir + Custom.RNV() * 0.1f).normalized;
			lastHandleA = handleA;
			lastHandleB = handleB;
			handleA = Vector3.Slerp(handleA, getToHandleA, 0.01f);
			handleB = Vector3.Slerp(handleB, getToHandleB, 0.01f);
			getToHandleA = (getToHandleA + Custom.RNV() * 0.1f).normalized;
			if (getToHandleA.y > 0f)
			{
				getToHandleA.y = 0f;
			}
			if (UnityEngine.Random.value < 1f / 120f)
			{
				getToHandleA = Custom.DegToVec(180f + Mathf.Lerp(-45f, 45f, UnityEngine.Random.value));
			}
			getToHandleB = (getToHandleB + Custom.RNV() * 0.1f).normalized;
			if (getToHandleB.y < 0f)
			{
				getToHandleB.y = 0f;
			}
			if (UnityEngine.Random.value < 1f / 120f)
			{
				getToHandleB = Custom.DegToVec(Mathf.Lerp(-45f, 45f, UnityEngine.Random.value));
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[2];
			Color color = Color.Lerp(new Color(0f, 0f, 0f), roofTopView.atmosphereColor, roofTopView.AtmosphereColorAtDepth(DisplayDepth * 2f));
			sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segs, pointyTip: false, customColor: false);
			sLeaser.sprites[0].color = color;
			sLeaser.sprites[0].alpha = Custom.LerpMap(depth, 2.2f, 7.5f, 0.5f, 0.15f) * 0.5f;
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Background"];
			sLeaser.sprites[1] = new FSprite("Futile_White");
			sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
			sLeaser.sprites[1].scale = 8f / DisplayDepth;
			sLeaser.sprites[1].color = color;
			sLeaser.sprites[1].alpha = Custom.LerpMap(depth, 2.2f, 7.5f, 0.75f, 0.15f) * 0.5f;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = DrawPos(camPos, rCam.hDisplace);
			Vector2 vector2 = vector + Vector2.Lerp(headLastPos, headLastPos, timeStacker) / DisplayDepth;
			Vector2 cA = vector2 + Vector2.Lerp(lastHandleA, handleA, timeStacker) * 100f / DisplayDepth;
			Vector2 cB = vector + Vector2.Lerp(lastHandleB, handleB, timeStacker) * 100f / DisplayDepth;
			Vector2 vector3 = vector2;
			float num = 5f;
			for (int i = 0; i < segs; i++)
			{
				float num2 = (float)i / (float)(segs - 1);
				Vector2 vector4 = Custom.Bezier(vector2, cA, vector, cB, num2);
				Vector2 vector5 = Custom.DirVec(vector3, vector4);
				Vector2 vector6 = Custom.PerpendicularVector(vector5);
				float num3 = Vector2.Distance(vector3, vector4);
				float num4 = Mathf.Lerp(4f, 0f, num2);
				num4 += Mathf.Sin((float)Math.PI * Mathf.InverseLerp(0f, 0.15f, num2)) * 12f;
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, vector4 - vector5 * num3 * 0.3f - vector6 * (num4 + num) / (2f * DisplayDepth));
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, vector4 - vector5 * num3 * 0.3f + vector6 * (num4 + num) / (2f * DisplayDepth));
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector4 - vector6 * (num4 / DisplayDepth));
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector4 + vector6 * (num4 / DisplayDepth));
				vector3 = vector4;
				num = num4;
				if (i == 1)
				{
					sLeaser.sprites[1].x = vector4.x;
					sLeaser.sprites[1].y = vector4.y;
				}
			}
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class DustpuffSpawner : UpdatableAndDeletable
	{
		public class DustPuff : CosmeticSprite
		{
			private float life;

			private float lastLife;

			private float lifeTime;

			private float size;

			private Vector2 motion;

			public DustPuff(Vector2 pos, float size)
			{
				base.pos = pos;
				lastPos = pos;
				this.size = size;
				lastLife = 1f;
				life = 1f;
				lifeTime = Mathf.Lerp(40f, 120f, UnityEngine.Random.value) * Mathf.Lerp(0.5f, 1.5f, size);
				motion = new Vector2(0.5f, 0.25f);
			}

			public override void Update(bool eu)
			{
				base.Update(eu);
				pos.y += motion.x;
				pos.x += motion.y;
				lastLife = life;
				life -= 1f / lifeTime;
				if (lastLife < 0f)
				{
					Destroy();
				}
			}

			public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
			{
				motion = (room.snow ? new Vector2(0f, 0f) : motion);
				sLeaser.sprites = new FSprite[1];
				sLeaser.sprites[0] = new FSprite("Futile_White");
				sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders[room.snow ? "SporesSnow" : "Spores"];
				AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Background"));
				base.InitiateSprites(sLeaser, rCam);
			}

			public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
			{
				sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
				sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
				sLeaser.sprites[0].scale = 10f * Mathf.Pow(1f - Mathf.Lerp(lastLife, life, timeStacker), 0.35f) * Mathf.Lerp(0.5f, 1.5f, size);
				sLeaser.sprites[0].alpha = Mathf.Lerp(lastLife, life, timeStacker);
				base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			}

			public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
			{
				if (rCam.room.snow)
				{
					Vector2 vector = pos - rCam.room.cameraPositions[rCam.room.game.cameras[0].currentCameraPosition];
					sLeaser.sprites[0].color = new Color(vector.x / 1400f, vector.y / 800f, 0f);
				}
				else
				{
					sLeaser.sprites[0].color = palette.texture.GetPixel(9, 5);
				}
				base.ApplyPalette(sLeaser, rCam, palette);
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			for (int i = 0; i < room.game.Players.Count; i++)
			{
				if (room.game.Players[i].realizedCreature == null || room.game.Players[i].realizedCreature.room != room)
				{
					continue;
				}
				for (int j = 0; j < room.game.Players[i].realizedCreature.bodyChunks.Length; j++)
				{
					if (room.game.Players[i].realizedCreature.bodyChunks[j].ContactPoint.y < 0)
					{
						if (room.game.Players[i].realizedCreature.bodyChunks[j].lastContactPoint.y >= 0 && room.game.Players[i].realizedCreature.bodyChunks[j].lastPos.y - room.game.Players[i].realizedCreature.bodyChunks[j].pos.y > 5f)
						{
							room.AddObject(new DustPuff(room.game.Players[i].realizedCreature.bodyChunks[j].pos + new Vector2(0f, 0f - room.game.Players[i].realizedCreature.bodyChunks[j].rad), Custom.LerpMap(room.game.Players[i].realizedCreature.bodyChunks[j].lastPos.y - room.game.Players[i].realizedCreature.bodyChunks[j].pos.y, 5f, 10f, 0.5f, 1f)));
						}
						else if (UnityEngine.Random.value < 0.1f && Mathf.Abs(room.game.Players[i].realizedCreature.bodyChunks[j].lastPos.x - room.game.Players[i].realizedCreature.bodyChunks[j].pos.x) > 3f)
						{
							room.AddObject(new DustPuff(room.game.Players[i].realizedCreature.bodyChunks[j].pos + new Vector2(0f, 0f - room.game.Players[i].realizedCreature.bodyChunks[j].rad), 0.25f * UnityEngine.Random.value));
						}
					}
				}
			}
		}
	}

	public class DustWave : BackgroundSceneElement
	{
		public string assetName;

		public float atmosphericalDepthAdd;

		private float scale;

		private Vector2 offset;

		public Vector2 lastPos;

		public bool isTopmost;

		private RoofTopView RoofTopScene => scene as RoofTopView;

		public DustWave(RoofTopView roofTopScene, string assetName, Vector2 pos, float depth, float atmosphericalDepthAdd)
			: base(roofTopScene, pos, depth)
		{
			isTopmost = false;
			this.assetName = assetName;
			this.atmosphericalDepthAdd = atmosphericalDepthAdd;
			scale = 1f - (base.depth - 20f) / 380f;
			offset = new Vector2(UnityEngine.Random.value, UnityEngine.Random.value);
			scene.LoadGraphic(assetName, crispPixels: true, clampWrapMode: false);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite(assetName);
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders[(ModManager.MMF && room.game.rainWorld.options.quality == Options.Quality.LOW) ? "DustWaveLow" : "DustWave"];
			sLeaser.sprites[0].anchorY = 0.15f;
			sLeaser.sprites[0].anchorX = 0.5f;
			sLeaser.sprites[0].scale = 2f + scale * scale * 10f;
			sLeaser.sprites[0].color = new Color(offset.x, offset.y, 1f - scale);
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = DrawPos(camPos, rCam.hDisplace);
			sLeaser.sprites[0].x = vector.x;
			sLeaser.sprites[0].y = vector.y;
			sLeaser.sprites[0].isVisible = room.DustStormIntensity < 1f && (room.DustStormIntensity > 0f || isTopmost);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public RoomSettings.RoomEffect effect;

	private float floorLevel = 26f;

	public Color atmosphereColor = new Color(0.16078432f, 0.23137255f, 27f / 85f);

	public Simple2DBackgroundIllustration daySky;

	public Simple2DBackgroundIllustration duskSky;

	public Simple2DBackgroundIllustration nightSky;

	public bool isLC;

	private List<DustWave> dustWaves;

	public RoofTopView(Room room, RoomSettings.RoomEffect effect)
		: base(room)
	{
		this.effect = effect;
		sceneOrigo = RoomToWorldPos(room.abstractRoom.size.ToVector2() * 10f);
		room.AddObject(new DustpuffSpawner());
		daySky = new Simple2DBackgroundIllustration(this, "Rf_Sky", new Vector2(683f, 384f));
		if (ModManager.MSC && room.game.IsStorySession && room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
		{
			duskSky = new Simple2DBackgroundIllustration(this, "Rf_DuskSky-Rivulet", new Vector2(683f, 384f));
		}
		else
		{
			duskSky = new Simple2DBackgroundIllustration(this, "Rf_DuskSky", new Vector2(683f, 384f));
		}
		nightSky = new Simple2DBackgroundIllustration(this, "Rf_NightSky", new Vector2(683f, 384f));
		isLC = ModManager.MSC && ((room.world.region != null && room.world.region.name == "LC") || base.room.abstractRoom.name.StartsWith("LC_"));
		if (isLC && (base.room.abstractRoom.name == "LC_entrancezone" || base.room.abstractRoom.name == "LC_shelter_above"))
		{
			isLC = false;
		}
		string text = "";
		bool flag = false;
		if ((ModManager.MSC && room.world.region != null && room.world.region.name == "DM") || base.room.abstractRoom.name.StartsWith("DM_"))
		{
			text = "_DM";
			flag = true;
		}
		if (base.room.dustStorm)
		{
			dustWaves = new List<DustWave>();
			float num = 2500f;
			float num2 = 0f;
			dustWaves.Add(new DustWave(this, "RF_CityA" + text, new Vector2(PosFromDrawPosAtNeutralCamPos(new Vector2(300f + (flag ? (-300f) : 0f), 0f), 0f - num2).x, floorLevel / 4f - num * 40f), 370f, 0f));
			dustWaves.Add(new DustWave(this, "RF_CityA" + text, new Vector2(PosFromDrawPosAtNeutralCamPos(new Vector2(300f + (flag ? 300f : 0f), 0f), 0f - num2).x, floorLevel / 5f - num * 30f), 290f, 0f));
			dustWaves.Add(new DustWave(this, "RF_CityA" + text, new Vector2(PosFromDrawPosAtNeutralCamPos(new Vector2(300f + (flag ? (-300f) : 0f), 0f), 0f - num2).x, floorLevel / 6f - num * 20f), 210f, 0f));
			dustWaves.Add(new DustWave(this, "RF_CityA" + text, new Vector2(PosFromDrawPosAtNeutralCamPos(new Vector2(300f + (flag ? (-300f) : 0f), 0f), 0f - num2).x, floorLevel / 7f - num * 10f), 130f, 0f));
			DustWave item = new DustWave(this, "RF_CityA" + text, new Vector2(PosFromDrawPosAtNeutralCamPos(new Vector2(300f + (flag ? (-300f) : 0f), 0f), 0f - num2).x, floorLevel / 8f), 50f, 0f)
			{
				isTopmost = true
			};
			dustWaves.Add(item);
			foreach (DustWave dustWave in dustWaves)
			{
				AddElement(dustWave);
			}
		}
		if (isLC)
		{
			daySky = new Simple2DBackgroundIllustration(this, "AtC_Sky", new Vector2(683f, 384f));
			duskSky = new Simple2DBackgroundIllustration(this, "AtC_DuskSky", new Vector2(683f, 384f));
			nightSky = new Simple2DBackgroundIllustration(this, "AtC_NightSky", new Vector2(683f, 384f));
			AddElement(nightSky);
			AddElement(duskSky);
			AddElement(daySky);
			floorLevel = base.room.world.RoomToWorldPos(new Vector2(0f, 0f), base.room.abstractRoom.index).y - 30992.8f;
			floorLevel *= 22f;
			floorLevel = 0f - floorLevel;
			float num3 = base.room.world.RoomToWorldPos(new Vector2(0f, 0f), base.room.abstractRoom.index).x - 11877f;
			num3 *= 0.01f;
			Shader.SetGlobalVector(RainWorld.ShadPropAboveCloudsAtmosphereColor, atmosphereColor);
			Shader.SetGlobalVector(RainWorld.ShadPropMultiplyColor, Color.white);
			Shader.SetGlobalVector(RainWorld.ShadPropSceneOrigoPosition, sceneOrigo);
			AddElement(new Building(this, "city2", new Vector2(PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 200f - num3).x, floorLevel * 0.2f - 170000f), 420.5f, 2f));
			AddElement(new Building(this, "city1", new Vector2(PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 70f - num3 * 0.5f).x, floorLevel * 0.25f - 116000f), 340f, 2f));
			AddElement(new Building(this, "city3", new Vector2(PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 70f - num3 * 0.5f).x, floorLevel * 0.3f - 85000f), 260f, 2f));
			AddElement(new Building(this, "city2", new Vector2(PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 40f - num3 * 0.5f).x, floorLevel * 0.35f - 42000f), 180f, 2f));
			AddElement(new Building(this, "city1", new Vector2(PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 90f - num3 * 0.2f).x, floorLevel * 0.4f + 5000f), 100f, 2f));
			AddElement(new Floor(this, "floor", new Vector2(0f, floorLevel * 0.2f - 90000f), 400.5f, 500.5f));
			return;
		}
		AddElement(nightSky);
		AddElement(duskSky);
		AddElement(daySky);
		Shader.SetGlobalVector(RainWorld.ShadPropMultiplyColor, Color.white);
		AddElement(new Floor(this, "floor", new Vector2(0f, floorLevel), 1f, 12f));
		Shader.SetGlobalVector(RainWorld.ShadPropAboveCloudsAtmosphereColor, atmosphereColor);
		Shader.SetGlobalVector(RainWorld.ShadPropSceneOrigoPosition, sceneOrigo);
		for (int i = 0; i < 16; i++)
		{
			float f = (float)i / 15f;
			AddElement(new Rubble(this, "Rf_Rubble", new Vector2(0f, floorLevel), Mathf.Lerp(1.5f, 8f, Mathf.Pow(f, 1.5f)), i));
		}
		AddElement(new DistantBuilding(this, "Rf_HoleFix", new Vector2(-2676f, 9f), 1f, 0f));
		if (!ModManager.MSC || text == "")
		{
			AddElement(new Building(this, "city2", new Vector2(PosFromDrawPosAtNeutralCamPos(new Vector2(1780f, 0f), 11.5f).x, floorLevel), 11.5f, 3f));
			AddElement(new Building(this, "city1", new Vector2(PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 10.5f).x, floorLevel), 10.5f, 3f));
		}
		AddElement(new DistantBuilding(this, "RF_CityA" + text, new Vector2(PosFromDrawPosAtNeutralCamPos(new Vector2(300f + (flag ? (-300f) : 0f), 0f), 8.5f).x, floorLevel - 25.5f), 8.5f, 0f));
		AddElement(new DistantBuilding(this, "RF_CityB" + text, new Vector2(PosFromDrawPosAtNeutralCamPos(new Vector2(515f + (flag ? (-300f) : 0f), 0f), 6.5f).x, floorLevel - 13f), 6.5f, 0f));
		AddElement(new DistantBuilding(this, "RF_CityC" + text, new Vector2(PosFromDrawPosAtNeutralCamPos(new Vector2(400f + (flag ? (-300f) : 0f), 0f), 5f).x, floorLevel - 8.5f), 5f, 0f));
		LoadGraphic("smoke1", crispPixels: false, clampWrapMode: false);
		AddElement(new Smoke(this, new Vector2(0f, floorLevel + 560f), 7f, 0, 2.5f, 0.1f, 0.8f, shaderType: false));
		AddElement(new Smoke(this, new Vector2(0f, floorLevel), 4.2f, 0, 0.2f, 0.1f, 0f, shaderType: true));
		AddElement(new Smoke(this, new Vector2(0f, floorLevel + 28f), 2f, 0, 0.5f, 0.1f, 0f, shaderType: true));
		AddElement(new Smoke(this, new Vector2(0f, floorLevel + 14f), 1.2f, 0, 0.75f, 0.1f, 0f, shaderType: true));
	}

	public float AtmosphereColorAtDepth(float depth)
	{
		return Mathf.Clamp(depth / 15f, 0f, 1f) * 0.9f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if ((!(room.game.cameras[0].effect_dayNight > 0f) || room.world.rainCycle.timer < room.world.rainCycle.cycleLength) && (!ModManager.Expedition || !room.game.rainWorld.ExpeditionMode))
		{
			return;
		}
		float num = 1320f;
		float num2 = (float)room.world.rainCycle.dayNightCounter / num;
		float num3 = ((float)room.world.rainCycle.dayNightCounter - num) / num;
		float num4 = ((float)room.world.rainCycle.dayNightCounter - num) / (num * 1.25f);
		Color a = new Color(0.16078432f, 0.23137255f, 27f / 85f);
		Color color = new Color(0.0627451f, 0.38431373f, 0.3019608f);
		Color color2 = new Color(0.04882353f, 0.0527451f, 0.06843138f);
		Color color3 = new Color(0.75686276f, 7f / 15f, 39f / 85f);
		Color color4 = new Color(4f / 51f, 12f / 85f, 18f / 85f);
		if (ModManager.MSC && room.game.IsStorySession && room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
		{
			color = new Color(0.78039217f, 0.41568628f, 0.39607844f);
			color3 = new Color(1f, 0.79f, 0.47f);
		}
		Color? color5 = null;
		Color? color6 = null;
		if (num2 > 0f && num2 < 1f)
		{
			daySky.alpha = 1f - num2;
			color5 = Color.Lerp(a, color, num2);
			color6 = Color.Lerp(Color.white, color3, num2);
		}
		if (num2 >= 1f)
		{
			daySky.alpha = 0f;
			if (num3 > 0f && num3 < 1f)
			{
				duskSky.alpha = 1f - num3;
				color5 = Color.Lerp(color, color2, num3);
			}
			if (num3 >= 1f)
			{
				duskSky.alpha = 0f;
				color5 = color2;
			}
			if (num4 > 0f && num4 < 1f)
			{
				color6 = Color.Lerp(color3, color4, num4);
			}
			if (num4 >= 1f)
			{
				color6 = color4;
			}
		}
		if (color5.HasValue)
		{
			atmosphereColor = color5.Value;
			Shader.SetGlobalVector(RainWorld.ShadPropAboveCloudsAtmosphereColor, atmosphereColor);
		}
		if (color6.HasValue)
		{
			Shader.SetGlobalVector(RainWorld.ShadPropMultiplyColor, color6.Value);
		}
	}

	public override void Destroy()
	{
		base.Destroy();
	}
}
