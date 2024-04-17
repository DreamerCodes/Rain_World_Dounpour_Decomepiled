using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class AboveCloudsView : BackgroundScene
{
	public abstract class Cloud : BackgroundSceneElement
	{
		public float randomOffset;

		public Color skyColor;

		public int index;

		public AboveCloudsView AboveCloudsScene => scene as AboveCloudsView;

		public Cloud(AboveCloudsView aboveCloudsScene, Vector2 pos, float depth, int index)
			: base(aboveCloudsScene, pos, depth)
		{
			randomOffset = UnityEngine.Random.value;
			this.index = index;
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			skyColor = palette.skyColor;
		}
	}

	public class CloseCloud : Cloud
	{
		private float cloudDepth;

		public CloseCloud(AboveCloudsView aboveCloudsScene, Vector2 pos, float cloudDepth, int index)
			: base(aboveCloudsScene, pos, aboveCloudsScene.CloudDepth(cloudDepth), index)
		{
			this.cloudDepth = cloudDepth;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[2];
			sLeaser.sprites[0] = new FSprite("pixel");
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Background"];
			sLeaser.sprites[0].anchorY = 0f;
			sLeaser.sprites[0].scaleX = 1400f;
			sLeaser.sprites[0].x = 683f;
			sLeaser.sprites[0].y = 0f;
			sLeaser.sprites[1] = new FSprite("clouds" + (index % 3 + 1));
			sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["Cloud"];
			sLeaser.sprites[1].anchorY = 1f;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			float y = scene.RoomToWorldPos(rCam.room.cameraPositions[rCam.currentCameraPosition]).y;
			float num = Mathf.InverseLerp(base.AboveCloudsScene.startAltitude, base.AboveCloudsScene.endAltitude, y);
			float num2 = cloudDepth;
			if (num > 0.5f)
			{
				num2 = Mathf.Lerp(num2, 1f, Mathf.InverseLerp(0.5f, 1f, num) * 0.5f);
			}
			depth = Mathf.Lerp(base.AboveCloudsScene.cloudsStartDepth, base.AboveCloudsScene.cloudsEndDepth, num2);
			float num3 = Mathf.Lerp(10f, 2f, num2);
			float y2 = DrawPos(new Vector2(camPos.x, camPos.y + base.AboveCloudsScene.yShift), rCam.hDisplace).y;
			y2 += Mathf.Lerp(Mathf.Pow(cloudDepth, 0.75f), Mathf.Sin(cloudDepth * (float)Math.PI), 0.5f) * Mathf.InverseLerp(0.5f, 0f, num) * 600f;
			y2 -= Mathf.InverseLerp(0.18f, 0.1f, num) * Mathf.Pow(1f - cloudDepth, 3f) * 100f;
			float num4 = Mathf.Lerp(1f, Mathf.Lerp(0.75f, 0.25f, num), num2);
			if (base.AboveCloudsScene.SIClouds)
			{
				y2 += -100f + 200f * num2;
				num4 = 1f;
			}
			sLeaser.sprites[0].scaleY = y2 - 150f * num3 * num4;
			sLeaser.sprites[1].scaleY = num4 * num3;
			sLeaser.sprites[1].scaleX = num3;
			sLeaser.sprites[1].color = new Color(num2 * 0.75f, randomOffset, Mathf.Lerp(num4, 1f, 0.5f), 1f);
			sLeaser.sprites[1].x = 683f;
			sLeaser.sprites[1].y = y2;
			sLeaser.sprites[0].color = Color.Lerp(skyColor, base.AboveCloudsScene.atmosphereColor, num2 * 0.75f);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class DistantCloud : Cloud
	{
		private float distantCloudDepth;

		public DistantCloud(AboveCloudsView aboveCloudsScene, Vector2 pos, float distantCloudDepth, int index)
			: base(aboveCloudsScene, pos, aboveCloudsScene.DistantCloudDepth(distantCloudDepth), index)
		{
			this.distantCloudDepth = distantCloudDepth;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[2];
			sLeaser.sprites[0] = new FSprite("pixel");
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Background"];
			sLeaser.sprites[0].anchorY = 0f;
			sLeaser.sprites[0].scaleX = 1400f;
			sLeaser.sprites[0].x = 683f;
			sLeaser.sprites[0].y = 0f;
			sLeaser.sprites[1] = new FSprite("clouds" + (index % 3 + 1));
			sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["CloudDistant"];
			sLeaser.sprites[1].anchorY = 1f;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			float value = scene.RoomToWorldPos(rCam.room.cameraPositions[rCam.currentCameraPosition]).y + base.AboveCloudsScene.yShift;
			if (Mathf.InverseLerp(base.AboveCloudsScene.startAltitude, base.AboveCloudsScene.endAltitude, value) < 0.33f)
			{
				sLeaser.sprites[1].isVisible = false;
				sLeaser.sprites[0].isVisible = false;
				return;
			}
			sLeaser.sprites[1].isVisible = true;
			sLeaser.sprites[0].isVisible = true;
			float num = 2f;
			float y = DrawPos(new Vector2(camPos.x, camPos.y + base.AboveCloudsScene.yShift), rCam.hDisplace).y;
			float num2 = Mathf.Lerp(0.3f, 0.01f, distantCloudDepth);
			if (index == 8)
			{
				num2 *= 1.5f;
			}
			sLeaser.sprites[0].scaleY = y - 150f * num * num2;
			sLeaser.sprites[1].scaleY = num2 * num;
			sLeaser.sprites[1].scaleX = num;
			sLeaser.sprites[1].color = new Color(Mathf.Lerp(0.75f, 0.95f, distantCloudDepth), randomOffset, Mathf.Lerp(num2, 1f, 0.5f), 1f);
			sLeaser.sprites[1].x = 683f;
			sLeaser.sprites[1].y = y;
			sLeaser.sprites[0].color = Color.Lerp(skyColor, base.AboveCloudsScene.atmosphereColor, Mathf.Lerp(0.75f, 0.95f, distantCloudDepth));
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class FlyingCloud : Cloud
	{
		private float flattened;

		private float alpha;

		private float shaderInputColor;

		public FlyingCloud(AboveCloudsView aboveCloudsScene, Vector2 pos, float depth, int index, float flattened, float alpha, float shaderInputColor)
			: base(aboveCloudsScene, pos, depth, index)
		{
			this.flattened = flattened;
			this.alpha = alpha;
			this.shaderInputColor = shaderInputColor;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("flyingClouds1");
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["CloudDistant"];
			sLeaser.sprites[0].anchorY = 1f;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			float y = scene.RoomToWorldPos(rCam.room.cameraPositions[rCam.currentCameraPosition]).y;
			if (Mathf.InverseLerp(base.AboveCloudsScene.startAltitude, base.AboveCloudsScene.endAltitude, y) < 0.33f)
			{
				sLeaser.sprites[0].isVisible = false;
				return;
			}
			sLeaser.sprites[0].isVisible = true;
			float num = 2f;
			float y2 = DrawPos(camPos, rCam.hDisplace).y;
			sLeaser.sprites[0].scaleY = flattened * num;
			sLeaser.sprites[0].scaleX = num;
			sLeaser.sprites[0].color = new Color(shaderInputColor, randomOffset, Mathf.Lerp(flattened, 1f, 0.5f), alpha);
			sLeaser.sprites[0].x = 683f;
			sLeaser.sprites[0].y = y2;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class DistantBuilding : BackgroundSceneElement
	{
		public string assetName;

		public float atmosphericalDepthAdd;

		public float alpha;

		public bool useNonMultiplyShader;

		private AboveCloudsView AboveCloudsScene => scene as AboveCloudsView;

		public DistantBuilding(AboveCloudsView aboveCloudsScene, string assetName, Vector2 pos, float depth, float atmosphericalDepthAdd)
			: base(aboveCloudsScene, pos, depth)
		{
			this.assetName = assetName;
			this.atmosphericalDepthAdd = atmosphericalDepthAdd;
			alpha = 1f;
			scene.LoadGraphic(assetName, crispPixels: true, clampWrapMode: false);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite(assetName);
			if (useNonMultiplyShader)
			{
				sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["DistantBkgObjectAlpha"];
			}
			else
			{
				sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["DistantBkgObject"];
			}
			sLeaser.sprites[0].anchorY = 0f;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = DrawPos(new Vector2(camPos.x, camPos.y + AboveCloudsScene.yShift), rCam.hDisplace);
			sLeaser.sprites[0].x = vector.x;
			sLeaser.sprites[0].y = vector.y;
			sLeaser.sprites[0].alpha = alpha;
			sLeaser.sprites[0].color = new Color(Mathf.Pow(Mathf.InverseLerp(0f, 600f, depth + atmosphericalDepthAdd), 0.3f) * 0.9f, 0f, 0f);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class Fog : FullScreenSingleColor
	{
		private AboveCloudsView AboveCloudsScene => scene as AboveCloudsView;

		public Fog(AboveCloudsView aboveCloudsScene)
			: base(aboveCloudsScene, default(Color), 1f, singlePixelTexture: true, float.MaxValue)
		{
			depth = 0f;
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			if (!room.game.IsArenaSession)
			{
				float value = scene.RoomToWorldPos(camPos).y + AboveCloudsScene.yShift;
				if (ModManager.MSC && AboveCloudsScene.OEClouds)
				{
					float a = Mathf.InverseLerp(0f, 66900f, scene.RoomToWorldPos(camPos).x) * 0.5f;
					a = Mathf.Lerp(a, 0.6f, Mathf.InverseLerp(21207.4f, 19887.3f, scene.RoomToWorldPos(camPos).y));
					alpha = a;
				}
				else if (AboveCloudsScene.SIClouds)
				{
					alpha = Mathf.InverseLerp(16000f, 9000f, value) * 0.6f;
				}
				else
				{
					alpha = Mathf.InverseLerp(22000f, 18000f, value) * 0.6f;
				}
			}
			else
			{
				alpha = 0f;
			}
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			color = palette.skyColor;
			base.ApplyPalette(sLeaser, rCam, palette);
		}
	}

	public class DistantLightning : BackgroundSceneElement
	{
		public int index;

		public string assetName;

		public float minusDepthForLayering;

		private bool restoredDepth;

		public int wait;

		public int tinyThunderWait;

		public int tinyThunder;

		public int tinyThunderLength;

		public int thunder;

		public int thunderLength;

		public float randomLevel;

		public float power;

		public int randomLevelChange;

		private float lastIntensity;

		private float intensity;

		public bool nonPositionBasedIntensity;

		public float intensityMultiplier;

		private AboveCloudsView AboveCloudsScene => scene as AboveCloudsView;

		public float ThunderFac => 1f - (float)thunder / (float)thunderLength;

		public float TinyThunderFac => 1f - (float)tinyThunder / (float)tinyThunderLength;

		public DistantLightning(AboveCloudsView aboveCloudsScene, string assetName, Vector2 pos, float depth, float minusDepthForLayering)
			: base(aboveCloudsScene, pos, depth - minusDepthForLayering)
		{
			this.minusDepthForLayering = minusDepthForLayering;
			this.assetName = assetName;
			scene.LoadGraphic(assetName, crispPixels: true, clampWrapMode: false);
			tinyThunderWait = 5;
			intensityMultiplier = 1f;
			nonPositionBasedIntensity = false;
		}

		public void Reset()
		{
			wait = (int)(Mathf.Lerp(10f, 440f, UnityEngine.Random.value) * Mathf.Lerp(1.5f, 1f, 1f));
			power = Mathf.Lerp(0.7f, 1f, UnityEngine.Random.value);
			thunderLength = UnityEngine.Random.Range(1, (int)Mathf.Lerp(10f, 32f, power));
		}

		public override void Update(bool eu)
		{
			if (!restoredDepth)
			{
				depth += minusDepthForLayering;
				restoredDepth = true;
			}
			randomLevelChange--;
			if (randomLevelChange < 1)
			{
				randomLevelChange = UnityEngine.Random.Range(1, 6);
				randomLevel = UnityEngine.Random.value;
			}
			if (wait > 0)
			{
				wait--;
				if (wait < 1)
				{
					thunder = thunderLength;
				}
			}
			else
			{
				thunder--;
				if (thunder < 1)
				{
					Reset();
				}
			}
			if (tinyThunderWait > 0)
			{
				tinyThunderWait--;
				if (tinyThunderWait < 1)
				{
					tinyThunderWait = UnityEngine.Random.Range(10, 80);
					tinyThunderLength = UnityEngine.Random.Range(5, tinyThunderWait);
					tinyThunder = tinyThunderLength;
				}
			}
			lastIntensity = intensity;
			float a = 0f;
			float b = 0f;
			if (thunder > 0)
			{
				a = Mathf.Pow(randomLevel, Mathf.Lerp(3f, 0.1f, Mathf.Sin(ThunderFac * (float)Math.PI)));
			}
			if (tinyThunder > 0)
			{
				tinyThunder--;
				b = Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(3f, 0.1f, Mathf.Sin(TinyThunderFac * (float)Math.PI))) * 0.4f;
			}
			intensity = Mathf.Max(a, b);
			base.Update(eu);
		}

		public float LightIntensity(float timeStacker)
		{
			float num = Mathf.Lerp(lastIntensity, intensity, timeStacker);
			if (UnityEngine.Random.value < 1f / 3f)
			{
				num = Mathf.Lerp(num, (UnityEngine.Random.value < 0.5f) ? 1f : 0f, UnityEngine.Random.value * num);
			}
			return Custom.SCurve(num, 0.5f) * intensityMultiplier;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite(assetName);
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Background"];
			sLeaser.sprites[0].anchorY = 0f;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = DrawPos(new Vector2(camPos.x, camPos.y + AboveCloudsScene.yShift), rCam.hDisplace);
			float value = scene.RoomToWorldPos(camPos).y + AboveCloudsScene.yShift;
			sLeaser.sprites[0].x = vector.x;
			sLeaser.sprites[0].y = vector.y;
			if (nonPositionBasedIntensity)
			{
				sLeaser.sprites[0].alpha = LightIntensity(timeStacker) * 0.2f;
			}
			else
			{
				sLeaser.sprites[0].alpha = LightIntensity(timeStacker) * 0.2f * Mathf.InverseLerp(27600f, 31400f, value);
			}
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public RoomSettings.RoomEffect effect;

	public List<Cloud> clouds;

	public float startAltitude = 20000f;

	public float endAltitude = 31400f;

	public float cloudsStartDepth = 5f;

	public float cloudsEndDepth = 40f;

	public float distantCloudsEndDepth = 200f;

	public Color atmosphereColor = new Color(0.16078432f, 0.23137255f, 27f / 85f);

	public Fog generalFog;

	private bool SIClouds;

	public Simple2DBackgroundIllustration daySky;

	public Simple2DBackgroundIllustration duskSky;

	public Simple2DBackgroundIllustration nightSky;

	public float yShift;

	public bool animateSMEndingScroll;

	public float endingScrollVelocity;

	private float betweenEndScrollVelo;

	private bool OEClouds;

	public DistantBuilding spireLights;

	public DistantLightning pebblesLightning;

	private int editObject;

	public AboveCloudsView(Room room, RoomSettings.RoomEffect effect)
		: base(room)
	{
		this.effect = effect;
		SIClouds = (room.world.region != null && room.world.region.name == "SI") || base.room.abstractRoom.name.StartsWith("SI_");
		OEClouds = ModManager.MSC && ((room.world.region != null && room.world.region.name == "OE") || base.room.abstractRoom.name.StartsWith("OE_"));
		if (SIClouds)
		{
			startAltitude = 9000f;
			endAltitude = 26400f;
		}
		if (ModManager.MSC)
		{
			if (room.world.region != null && (room.world.region.name == "LM" || room.world.region.name == "SL"))
			{
				startAltitude = 21000f;
				endAltitude = 32400f;
			}
			if (OEClouds)
			{
				startAltitude = 0f;
				endAltitude = 36900f;
			}
			OEshiftView();
		}
		if (!base.room.game.IsArenaSession || effect.type != RoomSettings.RoomEffect.Type.AboveCloudsView)
		{
			sceneOrigo = new Vector2(2514f, (startAltitude + endAltitude) / 2f);
		}
		else
		{
			Custom.Log("arena sky view is :", effect.amount.ToString());
			float num = 10000f - effect.amount * 30000f;
			sceneOrigo = new Vector2(2514f, num);
			startAltitude = num - 5500f;
			endAltitude = num + 5500f;
			SIClouds = false;
		}
		clouds = new List<Cloud>();
		LoadGraphic("clouds1", crispPixels: false, clampWrapMode: false);
		LoadGraphic("clouds2", crispPixels: false, clampWrapMode: false);
		LoadGraphic("clouds3", crispPixels: false, clampWrapMode: false);
		LoadGraphic("flyingClouds1", crispPixels: false, clampWrapMode: false);
		generalFog = new Fog(this);
		AddElement(generalFog);
		nightSky = new Simple2DBackgroundIllustration(this, "AtC_NightSky", new Vector2(683f, 384f));
		if (ModManager.MSC && base.room.game.IsStorySession && base.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
		{
			duskSky = new Simple2DBackgroundIllustration(this, "AtC_DuskSky-Rivulet", new Vector2(683f, 384f));
		}
		else
		{
			duskSky = new Simple2DBackgroundIllustration(this, "AtC_DuskSky", new Vector2(683f, 384f));
		}
		daySky = new Simple2DBackgroundIllustration(this, "AtC_Sky", new Vector2(683f, 384f));
		AddElement(nightSky);
		AddElement(duskSky);
		AddElement(daySky);
		if (SIClouds)
		{
			AddElement(new DistantBuilding(this, "AtC_Spire1", PosFromDrawPosAtNeutralCamPos(new Vector2(517f, -148f), 50f), 50f, -30f));
		}
		float num10;
		float num11;
		float num12;
		int num13;
		object obj;
		if (OEClouds)
		{
			if (base.room.abstractRoom.name == "OE_TREETOP" || base.room.abstractRoom.name == "OE_JUNGLE02")
			{
				float num2 = -775f;
				float num3 = -230f;
				float num4 = -890f;
				float num5 = -40f;
				float depth = 25f;
				spireLights = new DistantBuilding(this, "AtC_LocalSpire_Lights", PosFromDrawPosAtNeutralCamPos(new Vector2(1500f + num2, -380f + num3), depth), depth, -10f);
				spireLights.useNonMultiplyShader = true;
				spireLights.alpha = 0f;
				AddElement(new DistantBuilding(this, "AtC_LocalSpire", PosFromDrawPosAtNeutralCamPos(new Vector2(1500f + num2, -380f + num3), depth), depth, -10f));
				AddElement(spireLights);
				if (base.room.abstractRoom.name == "OE_TREETOP")
				{
					AddElement(new DistantBuilding(this, "AtC_RetainingWall", PosFromDrawPosAtNeutralCamPos(new Vector2(1785f + num2, -300f + num3), depth), depth, -9f));
				}
				else
				{
					AddElement(new DistantBuilding(this, "AtC_RetainingWall", PosFromDrawPosAtNeutralCamPos(new Vector2(1740f + num2, -100f + num3), depth), depth, -9f));
				}
				depth = 400f;
				AddElement(new DistantBuilding(this, "AtC_FivePebbles", PosFromDrawPosAtNeutralCamPos(new Vector2(960f + num4, -330f + num5), depth), depth, -100f));
				pebblesLightning = new DistantLightning(this, "AtC_FivePebblesLight", PosFromDrawPosAtNeutralCamPos(new Vector2(960f + num4, -300f + num5), depth), depth, 370f);
				pebblesLightning.nonPositionBasedIntensity = true;
				pebblesLightning.intensityMultiplier = 0f;
				AddElement(pebblesLightning);
			}
			else if (base.room.abstractRoom.name == "OE_SPIRE")
			{
				float num6 = 0f;
				float num7 = 280f;
				float num8 = -810f;
				float num9 = 130f;
				float depth2 = 40f;
				AddElement(new DistantBuilding(this, "AtC_RetainingWall", PosFromDrawPosAtNeutralCamPos(new Vector2(1600f + num6, -380f + num7), depth2), depth2, -10f));
				depth2 = 900f;
				AddElement(new DistantBuilding(this, "AtC_FivePebbles", PosFromDrawPosAtNeutralCamPos(new Vector2(960f + num8, -330f + num9), depth2), depth2, -100f));
			}
		}
		else
		{
			if (ModManager.MSC && (!SIClouds || base.room.abstractRoom.name == "SI_A07" || base.room.abstractRoom.name == "SI_SAINTINTRO"))
			{
				num10 = 0f;
				num11 = 0f;
				num12 = 1f;
				if (SIClouds)
				{
					num10 = 200f;
				}
				if (base.room.game.IsStorySession)
				{
					num13 = ((base.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint) ? 1 : 0);
					if (num13 != 0)
					{
						obj = "SAINT";
						goto IL_0748;
					}
				}
				else
				{
					num13 = 0;
				}
				obj = "";
				goto IL_0748;
			}
			float depth3 = 160f;
			AddElement(new DistantBuilding(this, "AtC_Structure1", PosFromDrawPosAtNeutralCamPos(new Vector2(-520f, -85f), depth3), depth3, -20f));
			AddElement(new DistantLightning(this, "AtC_Light1", PosFromDrawPosAtNeutralCamPos(new Vector2(-520f, -119f), depth3), depth3, 55f));
			depth3 = 350f;
			AddElement(new DistantBuilding(this, "AtC_Structure2", PosFromDrawPosAtNeutralCamPos(new Vector2(88f, -37f), depth3), depth3, 0f));
			AddElement(new DistantLightning(this, "AtC_Light2", PosFromDrawPosAtNeutralCamPos(new Vector2(88f, -53f), depth3), depth3, 250f));
			depth3 = 600f;
			AddElement(new DistantBuilding(this, "AtC_Structure3", PosFromDrawPosAtNeutralCamPos(new Vector2(-316f, -24f), depth3), depth3, -100f));
			AddElement(new DistantLightning(this, "AtC_Light3", PosFromDrawPosAtNeutralCamPos(new Vector2(-316f, -32f), depth3), depth3, 80f));
			depth3 = 700f;
			AddElement(new DistantBuilding(this, "AtC_Structure4", PosFromDrawPosAtNeutralCamPos(new Vector2(-648f, -21f), depth3), depth3, -200f));
			depth3 = 800f;
			AddElement(new DistantBuilding(this, "AtC_Structure5", PosFromDrawPosAtNeutralCamPos(new Vector2(156f, -22f), depth3), depth3, -350f));
			depth3 = 850f;
			AddElement(new DistantBuilding(this, "AtC_Structure6", PosFromDrawPosAtNeutralCamPos(new Vector2(-242f, -20f), depth3), depth3, -350f));
			depth3 = 80f;
			AddElement(new DistantBuilding(this, "AtC_Spire1", PosFromDrawPosAtNeutralCamPos(new Vector2(-587f, -134f), depth3), depth3, -60f));
			depth3 = 100f;
			AddElement(new DistantBuilding(this, "AtC_Spire2", PosFromDrawPosAtNeutralCamPos(new Vector2(-653f, -57f), depth3), depth3, 10f));
			depth3 = 155f;
			AddElement(new DistantBuilding(this, "AtC_Spire3", PosFromDrawPosAtNeutralCamPos(new Vector2(0f, -46f), depth3), depth3, 0f));
			depth3 = 190f;
			AddElement(new DistantBuilding(this, "AtC_Spire4", PosFromDrawPosAtNeutralCamPos(new Vector2(-224f, -20f), depth3), depth3, 80f));
			depth3 = 360f;
			AddElement(new DistantBuilding(this, "AtC_Spire5", PosFromDrawPosAtNeutralCamPos(new Vector2(-276f, -24f), depth3), depth3, -100f));
			depth3 = 280f;
			AddElement(new DistantBuilding(this, "AtC_Spire6", PosFromDrawPosAtNeutralCamPos(new Vector2(-39f, -33f), depth3), depth3, 0f));
			depth3 = 370f;
			AddElement(new DistantBuilding(this, "AtC_Spire7", PosFromDrawPosAtNeutralCamPos(new Vector2(155f, -7f), depth3), depth3, -85f));
			depth3 = 380f;
			AddElement(new DistantBuilding(this, "AtC_Spire8", PosFromDrawPosAtNeutralCamPos(new Vector2(-380f, 3f), depth3), depth3, -50f));
			depth3 = 395f;
			AddElement(new DistantBuilding(this, "AtC_Spire9", PosFromDrawPosAtNeutralCamPos(new Vector2(-207f, -1f), depth3), depth3, -50f));
		}
		goto IL_11e9;
		IL_0748:
		string text = (string)obj;
		float depth4 = 160f * num12;
		AddElement(new DistantBuilding(this, "AtC_Structure1" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(-520f + num10, -85f + num11), depth4), depth4, -20f));
		if (num13 == 0)
		{
			AddElement(new DistantLightning(this, "AtC_Light1", PosFromDrawPosAtNeutralCamPos(new Vector2(-520f + num10, -119f + num11), depth4), depth4, 55f));
		}
		depth4 = 350f * num12;
		AddElement(new DistantBuilding(this, "AtC_Structure2" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(88f + num10, -37f + num11), depth4), depth4, 0f));
		if (num13 == 0)
		{
			AddElement(new DistantLightning(this, "AtC_Light2", PosFromDrawPosAtNeutralCamPos(new Vector2(88f + num10, -53f + num11), depth4), depth4, 250f));
		}
		depth4 = 600f * num12;
		AddElement(new DistantBuilding(this, "AtC_Structure3" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(-316f + num10, -24f + num11), depth4), depth4, -100f));
		if (num13 == 0)
		{
			AddElement(new DistantLightning(this, "AtC_Light3", PosFromDrawPosAtNeutralCamPos(new Vector2(-316f + num10, -32f + num11), depth4), depth4, 80f));
		}
		depth4 = 700f * num12;
		AddElement(new DistantBuilding(this, "AtC_Structure4" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(-648f + num10, -21f + num11), depth4), depth4, -200f));
		depth4 = 800f * num12;
		AddElement(new DistantBuilding(this, "AtC_Structure5" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(156f + num10, -22f + num11), depth4), depth4, -350f));
		depth4 = 850f * num12;
		AddElement(new DistantBuilding(this, "AtC_Structure6" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(-242f + num10, -20f + num11), depth4), depth4, -350f));
		depth4 = 80f * num12;
		AddElement(new DistantBuilding(this, "AtC_Spire1" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(-587f + num10, -134f + num11), depth4), depth4, -60f));
		depth4 = 100f * num12;
		AddElement(new DistantBuilding(this, "AtC_Spire2" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(-653f + num10, -57f + num11), depth4), depth4, 10f));
		depth4 = 155f * num12;
		AddElement(new DistantBuilding(this, "AtC_Spire3" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(0f + num10, -46f + num11), depth4), depth4, 0f));
		depth4 = 190f * num12;
		AddElement(new DistantBuilding(this, "AtC_Spire4" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(-224f + num10, -20f + num11), depth4), depth4, 80f));
		depth4 = 360f * num12;
		AddElement(new DistantBuilding(this, "AtC_Spire5" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(-276f + num10, -24f + num11), depth4), depth4, -100f));
		depth4 = 280f * num12;
		AddElement(new DistantBuilding(this, "AtC_Spire6" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(-39f + num10, -33f + num11), depth4), depth4, 0f));
		depth4 = 370f * num12;
		AddElement(new DistantBuilding(this, "AtC_Spire7" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(155f + num10, -7f + num11), depth4), depth4, -85f));
		depth4 = 380f * num12;
		AddElement(new DistantBuilding(this, "AtC_Spire8" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(-380f + num10, 3f + num11), depth4), depth4, -50f));
		depth4 = 395f * num12;
		AddElement(new DistantBuilding(this, "AtC_Spire9" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(-207f + num10, -1f + num11), depth4), depth4, -50f));
		depth4 = 120f * num12;
		AddElement(new DistantBuilding(this, "AtC_Structure1" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(670f + num10, -85f + num11), depth4), depth4, -20f));
		if (num13 == 0)
		{
			AddElement(new DistantLightning(this, "AtC_Light1", PosFromDrawPosAtNeutralCamPos(new Vector2(670f + num10, -119f + num11), depth4), depth4, 55f));
		}
		depth4 = 160f * num12;
		AddElement(new DistantBuilding(this, "AtC_Spire2" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(470f + num10, -57f + num11), depth4), depth4, 10f));
		depth4 = 365f * num12;
		AddElement(new DistantBuilding(this, "AtC_Spire5" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(220f + num10, -24f + num11), depth4), depth4, -100f));
		depth4 = 285f * num12;
		AddElement(new DistantBuilding(this, "AtC_Spire6" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(520f + num10, -33f + num11), depth4), depth4, 0f));
		depth4 = 375f * num12;
		AddElement(new DistantBuilding(this, "AtC_Spire7" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(340f + num10, -7f + num11), depth4), depth4, -85f));
		depth4 = 380f * num12;
		AddElement(new DistantBuilding(this, "AtC_Spire8" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(600f + num10, 3f + num11), depth4), depth4, -50f));
		depth4 = 390f * num12;
		AddElement(new DistantBuilding(this, "AtC_Spire9" + text, PosFromDrawPosAtNeutralCamPos(new Vector2(175f + num10, -1f + num11), depth4), depth4, -50f));
		goto IL_11e9;
		IL_11e9:
		if (effect.type == RoomSettings.RoomEffect.Type.AboveCloudsView)
		{
			int num14 = 7;
			for (int i = 0; i < num14; i++)
			{
				float cloudDepth = (float)i / (float)(num14 - 1);
				AddElement(new CloseCloud(this, new Vector2(0f, 0f), cloudDepth, i));
			}
		}
		if (!SIClouds || (ModManager.MSC && (base.room.abstractRoom.name == "SI_A07" || base.room.abstractRoom.name == "SI_SAINTINTRO")))
		{
			int num15 = 11;
			for (int j = 0; j < num15; j++)
			{
				float num16 = (float)j / (float)(num15 - 1);
				AddElement(new DistantCloud(this, new Vector2(0f, -40f * cloudsEndDepth * (1f - num16)), num16, j));
			}
			AddElement(new FlyingCloud(this, PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 75f), 355f), 355f, 0, 0.35f, 0.5f, 0.9f));
			AddElement(new FlyingCloud(this, PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 43f), 920f), 920f, 0, 0.15f, 0.3f, 0.95f));
		}
		Shader.SetGlobalVector(RainWorld.ShadPropAboveCloudsAtmosphereColor, atmosphereColor);
		Shader.SetGlobalVector(RainWorld.ShadPropMultiplyColor, Color.white);
	}

	private float CloudDepth(float f)
	{
		return Mathf.Lerp(cloudsStartDepth, cloudsEndDepth, f);
	}

	private float DistantCloudDepth(float f)
	{
		return Mathf.Lerp(cloudsEndDepth, distantCloudsEndDepth, Mathf.Pow(f, 1.5f));
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (ModManager.MSC)
		{
			OEshiftView();
			if (animateSMEndingScroll)
			{
				if (yShift < 7500f)
				{
					if (endingScrollVelocity < 20f)
					{
						endingScrollVelocity = Mathf.Max(1f, endingScrollVelocity * 1.05f);
					}
					else
					{
						endingScrollVelocity = Mathf.Max(1f, endingScrollVelocity * 1.01f);
					}
				}
				else if (yShift > 12500f)
				{
					endingScrollVelocity = Mathf.Lerp(betweenEndScrollVelo, 0f, (yShift - 12500f) / 7500f);
				}
				else
				{
					betweenEndScrollVelo = endingScrollVelocity;
				}
				if (room.game.StoryCharacter != MoreSlugcatsEnums.SlugcatStatsName.Saint)
				{
					yShift += endingScrollVelocity;
				}
				room.game.cameras[0].hardLevelGfxOffset.y = yShift * 0.35f;
			}
		}
		if ((room.game.cameras[0].effect_dayNight > 0f && room.world.rainCycle.timer >= room.world.rainCycle.cycleLength) || (ModManager.Expedition && room.game.rainWorld.ExpeditionMode))
		{
			float num = 1320f;
			float num2 = (float)room.world.rainCycle.dayNightCounter / num;
			float num3 = ((float)room.world.rainCycle.dayNightCounter - num) / num;
			float num4 = ((float)room.world.rainCycle.dayNightCounter - num) / (num * 1.25f);
			Color a = new Color(0.16078432f, 0.23137255f, 27f / 85f);
			Color color = new Color(44f / 85f, 0.3254902f, 0.40784314f);
			Color color2 = new Color(0.04882353f, 0.0527451f, 0.06843138f);
			Color color3 = new Color(1f, 0.79f, 0.47f);
			Color color4 = new Color(4f / 51f, 12f / 85f, 18f / 85f);
			if (ModManager.MSC && room.game.IsStorySession && room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				color = new Color(0.7564706f, 0.3756863f, 0.3756863f);
				color3 = new Color(1f, 0.79f, 0.47f);
			}
			Color? color5 = null;
			Color? color6 = null;
			if (spireLights != null)
			{
				if (num3 > 0f)
				{
					spireLights.alpha = Mathf.Min(1f, num3);
				}
				else
				{
					spireLights.alpha = 0f;
				}
			}
			if (pebblesLightning != null)
			{
				if (num3 > 0f)
				{
					pebblesLightning.intensityMultiplier = Mathf.Min(1f, num3);
				}
				else
				{
					pebblesLightning.intensityMultiplier = 0f;
				}
			}
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
		if (!room.game.devToolsActive && !ModManager.DevTools)
		{
			return;
		}
		if (Input.GetKey("t"))
		{
			int num5 = 0;
			for (int i = 0; i < elements.Count; i++)
			{
				if (elements[i] is DistantBuilding)
				{
					if (num5 == editObject)
					{
						Vector2 input = (Vector2)Futile.mousePosition - room.game.rainWorld.screenSize / 2f;
						elements[i].pos = PosFromDrawPosAtNeutralCamPos(input, elements[i].depth);
						Custom.Log((elements[i] as DistantBuilding).assetName, input.ToString());
						break;
					}
					num5++;
				}
			}
		}
		if (Input.GetKey("y"))
		{
			editObject = (int)(Futile.mousePosition.x / 50f);
			Custom.LogImportant("EDIT: " + editObject);
		}
	}

	public override void AddElement(BackgroundSceneElement element)
	{
		if (element is Cloud)
		{
			clouds.Add(element as Cloud);
		}
		base.AddElement(element);
	}

	public override void Destroy()
	{
		base.Destroy();
	}

	public void OEshiftView()
	{
		if (OEClouds && room.game.cameras[0].room == room)
		{
			Vector2 pos = room.game.cameras[0].pos;
			float x = room.world.RoomToWorldPos(pos, room.abstractRoom.index).x;
			AbstractRoom abstractRoom = room.world.GetAbstractRoom("GATE_SB_OE");
			float x2 = room.world.RoomToWorldPos(new Vector2(0f, 0f), abstractRoom.index).x;
			AbstractRoom abstractRoom2 = room.world.GetAbstractRoom("OE_RUIN02");
			float t = Mathf.InverseLerp(room.world.RoomToWorldPos(new Vector2(0f, 0f), abstractRoom2.index).x, x2, x);
			yShift = Mathf.Lerp(-9370f, -26900f, t);
		}
	}
}
