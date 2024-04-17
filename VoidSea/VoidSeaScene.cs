using System;
using System.Collections.Generic;
using System.Linq;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace VoidSea;

public class VoidSeaScene : BackgroundScene
{
	public class DeepDivePhase : ExtEnum<DeepDivePhase>
	{
		public static readonly DeepDivePhase Start = new DeepDivePhase("Start", register: true);

		public static readonly DeepDivePhase CeilingDestroyed = new DeepDivePhase("CeilingDestroyed", register: true);

		public static readonly DeepDivePhase CloseWormsDestroyed = new DeepDivePhase("CloseWormsDestroyed", register: true);

		public static readonly DeepDivePhase DistantWormsDestroyed = new DeepDivePhase("DistantWormsDestroyed", register: true);

		public static readonly DeepDivePhase MovedIntoSecondSpace = new DeepDivePhase("MovedIntoSecondSpace", register: true);

		public static readonly DeepDivePhase EggScenario = new DeepDivePhase("EggScenario", register: true);

		public DeepDivePhase(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public abstract class VoidSeaSceneElement : BackgroundSceneElement
	{
		public bool visible = true;

		private VoidSeaScene voidSeaScene => scene as VoidSeaScene;

		public VoidSeaSceneElement(VoidSeaScene voidSeaScene, Vector2 pos, float depth)
			: base(voidSeaScene, pos, depth)
		{
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].isVisible = visible && rCam.voidSeaMode;
			}
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Background");
			}
			base.AddToContainer(sLeaser, rCam, newContatiner);
		}
	}

	public class VoidCeiling : VoidSeaSceneElement
	{
		public float scale = 50f;

		private string assetName;

		public int index;

		private VoidSeaScene voidSeaScene => scene as VoidSeaScene;

		public VoidCeiling(VoidSeaScene voidSeaScene, string assetName, Vector2 pos, float depth, int index)
			: base(voidSeaScene, pos, depth)
		{
			base.depth = depth;
			this.assetName = assetName;
			this.index = index;
			voidSeaScene.LoadGraphic(assetName, crispPixels: false, clampWrapMode: false);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite(assetName);
			if (voidSeaScene.Inverted)
			{
				sLeaser.sprites[0].scaleY = scale / depth;
			}
			else
			{
				sLeaser.sprites[0].scaleY = (0f - scale) / depth;
			}
			sLeaser.sprites[0].scaleX = scale;
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["VoidCeiling"];
			sLeaser.sprites[0].anchorY = 0f;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = DrawPos(camPos, rCam.hDisplace);
			visible = vector.y - 150f * (scale / depth) < 768f && camPos.y > -12500f;
			if (voidSeaScene.Inverted)
			{
				visible = camPos.y < voidSeaScene.room.PixelHeight + 12800f;
			}
			sLeaser.sprites[0].x = rCam.game.rainWorld.screenSize.x / 2f;
			sLeaser.sprites[0].y = vector.y;
			sLeaser.sprites[0].color = new Color(1f / depth, 1f / scale, 1f);
			if (index == 0)
			{
				Shader.SetGlobalVector(RainWorld.ShadPropWorldCamPos, camPos - scene.sceneOrigo + voidSeaScene.cameraOffset);
			}
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class VoidSprite : VoidSeaSceneElement
	{
		public Vector2[,] positions;

		private VoidSeaScene voidSeaScene => scene as VoidSeaScene;

		public float Rad => 1f;

		public VoidSprite(VoidSeaScene voidSeaScene, float depth, int index)
			: base(voidSeaScene, new Vector2(0f, 0f), depth)
		{
			base.depth = depth;
			positions = new Vector2[voidSeaScene.room.game.cameras.Length, 3];
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (voidSeaScene.deepDivePhase == DeepDivePhase.EggScenario)
			{
				for (int i = 0; i < positions.GetLength(0); i++)
				{
					positions[i, 0] += Custom.DirVec(positions[i, 0], voidSeaScene.theEgg.pos - voidSeaScene.room.game.cameras[i].pos) * 0.5f * UnityEngine.Random.value;
				}
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("pixel");
			sLeaser.sprites[0].anchorY = 0f;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			positions[rCam.cameraNumber, 2] = positions[rCam.cameraNumber, 0];
			positions[rCam.cameraNumber, 0] += (positions[rCam.cameraNumber, 1] - camPos) / depth;
			positions[rCam.cameraNumber, 1] = camPos;
			if (positions[rCam.cameraNumber, 0].x < 0f - Rad)
			{
				positions[rCam.cameraNumber, 0].x = 1400f + Rad;
				positions[rCam.cameraNumber, 0].y = UnityEngine.Random.value * 800f;
				positions[rCam.cameraNumber, 2] = positions[rCam.cameraNumber, 0];
			}
			else if (positions[rCam.cameraNumber, 0].x > 1400f + Rad)
			{
				positions[rCam.cameraNumber, 0].x = 0f - Rad;
				positions[rCam.cameraNumber, 0].y = UnityEngine.Random.value * 800f;
				positions[rCam.cameraNumber, 2] = positions[rCam.cameraNumber, 0];
			}
			if (positions[rCam.cameraNumber, 0].y < 0f - Rad)
			{
				positions[rCam.cameraNumber, 0].y = 800f + Rad;
				positions[rCam.cameraNumber, 0].x = UnityEngine.Random.value * 1400f;
				positions[rCam.cameraNumber, 2] = positions[rCam.cameraNumber, 0];
			}
			else if (positions[rCam.cameraNumber, 0].y > 800f + Rad)
			{
				positions[rCam.cameraNumber, 0].y = 0f - Rad;
				positions[rCam.cameraNumber, 0].x = UnityEngine.Random.value * 1400f;
				positions[rCam.cameraNumber, 2] = positions[rCam.cameraNumber, 0];
			}
			Vector2 vector = positions[rCam.cameraNumber, 0];
			sLeaser.sprites[0].alpha = Mathf.Lerp(0.3f, 0.65f, rCam.voidSeaGoldFilter) / ((depth + 0.5f) * 1.5f) * Mathf.InverseLerp(-3000f, -6000f, camPos.y);
			sLeaser.sprites[0].x = vector.x;
			sLeaser.sprites[0].y = vector.y;
			if (Custom.DistLess(positions[rCam.cameraNumber, 2], positions[rCam.cameraNumber, 0], 1f))
			{
				sLeaser.sprites[0].rotation = 0f;
				sLeaser.sprites[0].scaleY = 1f;
			}
			else
			{
				sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(positions[rCam.cameraNumber, 0], positions[rCam.cameraNumber, 2]);
				sLeaser.sprites[0].scaleY = Vector2.Distance(positions[rCam.cameraNumber, 0], positions[rCam.cameraNumber, 2]);
				sLeaser.sprites[0].alpha *= Custom.LerpMap(Vector2.Distance(positions[rCam.cameraNumber, 0], positions[rCam.cameraNumber, 2]), 1f, 16f, 0.75f, 0.35f);
			}
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class VoidSeaBkg : FullScreenSingleColor
	{
		public VoidSeaScene voidSea;

		public VoidSeaBkg(VoidSeaScene voidSea)
			: base(voidSea, new Color(0f, 0f, 0.003921569f), 1f, singlePixelTexture: true, float.MaxValue)
		{
			this.voidSea = voidSea;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Basic"];
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].isVisible = rCam.voidSeaMode;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			newContatiner = rCam.ReturnFContainer("Background");
			base.AddToContainer(sLeaser, rCam, newContatiner);
		}
	}

	public class VoidSeaFade : BackgroundSceneElement
	{
		public VoidSeaScene voidSea;

		public VoidSeaFade(VoidSeaScene voidSea)
			: base(voidSea, default(Vector2), 0.1f)
		{
			this.voidSea = voidSea;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new CustomFSprite("Futile_White");
			for (int i = 0; i < 4; i++)
			{
				(sLeaser.sprites[0] as CustomFSprite).verticeColors[i] = new Color(69f / 85f, 0.5568628f, 24f / 85f, (i < 2) ? 0f : 0.75f);
			}
			base.InitiateSprites(sLeaser, rCam);
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("GrabShaders"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].isVisible = !rCam.voidSeaMode;
			if (voidSea.Inverted)
			{
				(sLeaser.sprites[0] as CustomFSprite).vertices[0] = new Vector2(-100f, voidSea.room.PixelHeight - 560f - camPos.y);
				(sLeaser.sprites[0] as CustomFSprite).vertices[1] = new Vector2(1400f, voidSea.room.PixelHeight - 560f - camPos.y);
				(sLeaser.sprites[0] as CustomFSprite).vertices[2] = new Vector2(1400f, voidSea.room.PixelHeight + 140f - camPos.y);
				(sLeaser.sprites[0] as CustomFSprite).vertices[3] = new Vector2(-100f, voidSea.room.PixelHeight + 140f - camPos.y);
			}
			else
			{
				(sLeaser.sprites[0] as CustomFSprite).vertices[0] = new Vector2(-100f, 800f - camPos.y);
				(sLeaser.sprites[0] as CustomFSprite).vertices[1] = new Vector2(1400f, 800f - camPos.y);
				(sLeaser.sprites[0] as CustomFSprite).vertices[2] = new Vector2(1400f, 100f - camPos.y);
				(sLeaser.sprites[0] as CustomFSprite).vertices[3] = new Vector2(-100f, 100f - camPos.y);
			}
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class WormLightFade : FullScreenSingleColor
	{
		public VoidSeaScene voidSea;

		public WormLightFade(VoidSeaScene voidSea)
			: base(voidSea, new Color(0.5294118f, 31f / 85f, 0.18431373f), 1f, singlePixelTexture: false, 0f)
		{
			this.voidSea = voidSea;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Basic"];
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].isVisible = rCam.voidSeaMode;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			float num = 0f;
			Player player = room.game.FirstRealizedPlayer;
			if (ModManager.CoopAvailable)
			{
				player = room.game.RealizedPlayerFollowedByCamera;
			}
			if (player != null && rCam.voidSeaGoldFilter > 0f)
			{
				Vector2 a = Vector2.Lerp(player.mainBodyChunk.lastPos, player.mainBodyChunk.pos, timeStacker) - camPos;
				for (int i = 0; i < voidSea.worms.Count; i++)
				{
					if (voidSea.worms[i].depth < 11f)
					{
						Vector2 vector = Vector2.Lerp(voidSea.worms[i].chunks[0].lastPos, voidSea.worms[i].chunks[0].pos, timeStacker) - camPos;
						vector = (vector - voidSea.convergencePoint) / voidSea.worms[i].depth + voidSea.convergencePoint;
						num = Mathf.Max(num, Mathf.Pow(Mathf.InverseLerp(1800f, 350f, Vector2.Distance(a, vector) * voidSea.worms[i].depth), 0.75f) * Mathf.InverseLerp(11f, 1f, voidSea.worms[i].depth) * (1f - voidSea.worms[i].lightDimmed));
					}
				}
			}
			num *= 0.95f * rCam.voidSeaGoldFilter;
			sLeaser.sprites[0].alpha = num;
			sLeaser.sprites[0].isVisible = num > 0f;
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			base.AddToContainer(sLeaser, rCam, newContatiner);
		}
	}

	public class TheEgg : VoidSeaSceneElement
	{
		public float whiteFade;

		public float lastWhiteFade;

		public float greyFade;

		public float lastGreyFade;

		public int fadeWait;

		public float musicVolumeDirectionBoost;

		public float musicVolume;

		public int counter;

		private bool exitCommand;

		public List<float> playerDists;

		private VoidSeaScene voidSeaScene => scene as VoidSeaScene;

		public float maxAllowedDist => Custom.LerpMap(counter, 400f, 9600f, 11000f, 5000f);

		public TheEgg(VoidSeaScene voidSeaScene, Vector2 pos)
			: base(voidSeaScene, pos, 1f)
		{
			playerDists = new List<float>();
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (voidSeaScene.deepDivePhase != DeepDivePhase.EggScenario)
			{
				return;
			}
			if (voidSeaScene.playerGhosts != null && voidSeaScene.playerGhosts.ghosts.Count > 0)
			{
				counter++;
			}
			Player player = room.game.FirstRealizedPlayer;
			if (ModManager.CoopAvailable)
			{
				player = room.game.RealizedPlayerFollowedByCamera;
			}
			if (player != null)
			{
				float num = Vector2.Distance(player.mainBodyChunk.pos, pos);
				if (scene.room.game.devToolsActive && Input.GetMouseButton(0))
				{
					Custom.Log(counter.ToString(), musicVolume.ToString(), musicVolumeDirectionBoost.ToString());
				}
				musicVolume = Mathf.InverseLerp(Mathf.Lerp(6000f, 11000f, musicVolumeDirectionBoost), Mathf.Lerp(500f, 3000f, musicVolumeDirectionBoost), num) * Custom.SCurve(Mathf.InverseLerp(100f, 1600f, counter), 0.6f);
				playerDists.Insert(0, num);
				if (playerDists.Count > 100)
				{
					playerDists.RemoveAt(playerDists.Count - 1);
				}
				if (!Custom.DistLess(player.mainBodyChunk.pos, pos, maxAllowedDist))
				{
					pos = player.mainBodyChunk.pos + Custom.DirVec(player.mainBodyChunk.pos, pos) * maxAllowedDist;
					Custom.Log("move egg", pos.ToString());
				}
				if (Custom.DistLess(player.mainBodyChunk.pos, pos, 350f))
				{
					FadeToWhite();
				}
			}
			lastWhiteFade = whiteFade;
			lastGreyFade = greyFade;
			if (fadeWait > 0)
			{
				fadeWait--;
			}
			else
			{
				if (whiteFade >= 1f && greyFade == 0f)
				{
					greyFade = 0.002f;
					fadeWait = 20;
				}
				else if (whiteFade > 0f)
				{
					whiteFade = Mathf.Min(1f, whiteFade + 0.025f);
				}
				voidSeaScene.playerGhosts.originalPlayer.touchedNoInputCounter = Math.Min(voidSeaScene.playerGhosts.originalPlayer.touchedNoInputCounter, (int)(80f * (1f - whiteFade)));
				for (int i = 0; i < voidSeaScene.playerGhosts.ghosts.Count; i++)
				{
					voidSeaScene.playerGhosts.ghosts[i].creature.touchedNoInputCounter = Math.Min(voidSeaScene.playerGhosts.ghosts[i].creature.touchedNoInputCounter, (int)(80f * (1f - whiteFade)));
				}
				if (greyFade >= 1f)
				{
					if (!exitCommand)
					{
						voidSeaScene.room.game.ExitToVoidSeaSlideShow();
					}
					exitCommand = true;
				}
				else if (greyFade > 0f)
				{
					greyFade = Mathf.Min(1f, greyFade + 1f / 60f);
					if (greyFade >= 1f)
					{
						fadeWait = 20;
					}
				}
			}
			if (playerDists.Count > 1)
			{
				musicVolumeDirectionBoost = Custom.LerpAndTick(musicVolumeDirectionBoost, Mathf.InverseLerp(100f, -100f, playerDists[0] - playerDists[playerDists.Count - 1]), 0.002f, 1f / 30f);
			}
		}

		private void FadeToWhite()
		{
			if (whiteFade == 0f)
			{
				whiteFade = 0.002f;
				room.PlaySound(SoundID.Void_Sea_Swim_Into_Core, 0f, 1f, 1f);
				Custom.Log("END");
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[23];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["FlatLightNoisy"];
			sLeaser.sprites[sLeaser.sprites.Length - 3] = new FSprite("Futile_White");
			sLeaser.sprites[sLeaser.sprites.Length - 3].shader = rCam.game.rainWorld.Shaders["FlatLightNoisy"];
			sLeaser.sprites[sLeaser.sprites.Length - 2] = new FSprite("Futile_White");
			sLeaser.sprites[sLeaser.sprites.Length - 2].shader = rCam.game.rainWorld.Shaders["FlatLightNoisy"];
			sLeaser.sprites[sLeaser.sprites.Length - 1] = new FSprite("Futile_White");
			sLeaser.sprites[sLeaser.sprites.Length - 1].scaleX = 93.75f;
			sLeaser.sprites[sLeaser.sprites.Length - 1].scaleY = 56.25f;
			sLeaser.sprites[sLeaser.sprites.Length - 1].x = 700f;
			sLeaser.sprites[sLeaser.sprites.Length - 1].y = 400f;
			for (int i = 1; i < sLeaser.sprites.Length - 3; i++)
			{
				sLeaser.sprites[i] = new FSprite("Futile_White");
				sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["FlatLightNoisy"];
				sLeaser.sprites[i].color = new Color(0f, 0f, 0f);
				sLeaser.sprites[i].anchorY = 0.2f;
			}
			for (int j = 0; j < sLeaser.sprites.Length; j++)
			{
				sLeaser.sprites[j].isVisible = false;
			}
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			if (voidSeaScene.deepDivePhase != DeepDivePhase.EggScenario)
			{
				visible = false;
				return;
			}
			visible = true;
			Vector2 corner = pos;
			if (!new FloatRect(camPos.x, camPos.y, camPos.x + 1400f, camPos.y + 800f).Vector2Inside(pos))
			{
				corner = Custom.RectCollision(camPos + new Vector2(700f, 400f), pos, new FloatRect(camPos.x, camPos.y, camPos.x + 1400f, camPos.y + 800f)).GetCorner(3);
			}
			float num = Mathf.InverseLerp(10000f, 0f, Vector2.Distance(corner, pos));
			voidSeaScene.eggProximity = num;
			corner = Custom.MoveTowards(corner, pos, (1f - num) * Mathf.Lerp(150f, 200f, Mathf.Pow(num, 4f)) * 4f);
			sLeaser.sprites[0].scale = Mathf.Lerp(150f, 300f, Mathf.Pow(num, 4f));
			sLeaser.sprites[0].alpha = 0.25f * num;
			sLeaser.sprites[sLeaser.sprites.Length - 3].scale = Mathf.Lerp(0f, 150f, Mathf.Pow(Mathf.InverseLerp(0.5f, 1f, num), 4f));
			sLeaser.sprites[sLeaser.sprites.Length - 3].alpha = Mathf.InverseLerp(0.5f, 1f, num);
			sLeaser.sprites[sLeaser.sprites.Length - 2].scale = Mathf.Lerp(0f, 100f, Mathf.Pow(Mathf.InverseLerp(0.5f, 1f, num), 2f));
			sLeaser.sprites[sLeaser.sprites.Length - 2].alpha = Mathf.Pow(Mathf.InverseLerp(0.85f, 1f, num), 3f);
			sLeaser.sprites[0].x = corner.x - camPos.x;
			sLeaser.sprites[0].y = corner.y - camPos.y;
			sLeaser.sprites[sLeaser.sprites.Length - 3].x = corner.x - camPos.x;
			sLeaser.sprites[sLeaser.sprites.Length - 3].y = corner.y - camPos.y;
			sLeaser.sprites[sLeaser.sprites.Length - 2].x = corner.x - camPos.x;
			sLeaser.sprites[sLeaser.sprites.Length - 2].y = corner.y - camPos.y;
			sLeaser.sprites[sLeaser.sprites.Length - 2].isVisible = false;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			for (int i = 1; i < sLeaser.sprites.Length - 3; i++)
			{
				Player player = null;
				if (i == 1)
				{
					player = room.game.FirstRealizedPlayer;
					if (ModManager.CoopAvailable)
					{
						player = room.game.RealizedPlayerFollowedByCamera;
					}
				}
				else if (voidSeaScene.playerGhosts != null && i - 2 < voidSeaScene.playerGhosts.ghosts.Count)
				{
					player = voidSeaScene.playerGhosts.ghosts[i - 2].creature;
				}
				if (player == null)
				{
					sLeaser.sprites[i].isVisible = false;
					continue;
				}
				sLeaser.sprites[i].isVisible = true;
				Vector2 vector = Vector2.Lerp((player.graphicsModule as PlayerGraphics).drawPositions[0, 1], (player.graphicsModule as PlayerGraphics).drawPositions[0, 0], timeStacker);
				Vector2 vector2 = Vector2.Lerp((player.graphicsModule as PlayerGraphics).drawPositions[1, 1], (player.graphicsModule as PlayerGraphics).drawPositions[1, 0], timeStacker);
				Vector2 vector3 = Vector2.Lerp(corner, pos, 0.1f);
				Vector2 vector4 = (vector + vector2) / 2f - camPos;
				sLeaser.sprites[i].x = vector4.x;
				sLeaser.sprites[i].y = vector4.y;
				float f = Mathf.Abs(Vector2.Dot((vector - vector2).normalized, (vector3 - (vector + vector2) / 2f).normalized));
				sLeaser.sprites[i].scaleX = Mathf.Lerp(100f, 50f, Mathf.Pow(f, 2f)) / 16f;
				sLeaser.sprites[i].scaleY = Mathf.Lerp(600f, 700f, Mathf.Pow(f, 2f)) * Mathf.Pow(1f - num, 0.5f) / 16f;
				sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(vector3, (vector + vector2) / 2f);
				sLeaser.sprites[i].alpha = Mathf.Pow(Mathf.Sin(Mathf.Pow(num, 5f) * (float)Math.PI), 0.5f) * 0.15f * Mathf.Pow(Mathf.InverseLerp(385f, 100f, Vector2.Distance(new Vector2(400f, 400f), Custom.FlattenVectorAlongAxis(vector4 - new Vector2(400f, 400f), 90f, 0.5f) + new Vector2(400f, 400f))), 0.5f);
			}
			sLeaser.sprites[sLeaser.sprites.Length - 1].isVisible = visible && (lastWhiteFade > 0f || whiteFade > 0f);
			sLeaser.sprites[sLeaser.sprites.Length - 1].alpha = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastWhiteFade, whiteFade, timeStacker)), 1.8f);
			sLeaser.sprites[sLeaser.sprites.Length - 1].color = Color.Lerp(new Color(1f, 1f, 1f), new Color(0.5f, 0.5f, 0.5f), Custom.SCurve(Mathf.Lerp(lastGreyFade, greyFade, timeStacker), 0.5f));
		}
	}

	public class SaintEndingPhase : ExtEnum<SaintEndingPhase>
	{
		public static readonly SaintEndingPhase Inactive = new SaintEndingPhase("Inactive", register: true);

		public static readonly SaintEndingPhase WormDeath = new SaintEndingPhase("WormDeath", register: true);

		public static readonly SaintEndingPhase EchoTransform = new SaintEndingPhase("EchoTransform", register: true);

		public static readonly SaintEndingPhase Drowned = new SaintEndingPhase("Drowned", register: true);

		public SaintEndingPhase(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class MeltingItem
	{
		public PhysicalObject meltingObject;

		private int meltTimer;

		private int maxMeltTime;

		private VoidSeaScene voidScene;

		public MeltingItem(PhysicalObject obj, VoidSeaScene voidScene)
		{
			meltingObject = obj;
			this.voidScene = voidScene;
			maxMeltTime = UnityEngine.Random.Range(100, 200);
		}

		public void Update()
		{
			if (meltingObject == null)
			{
				return;
			}
			if (voidScene.Inverted)
			{
				if (meltingObject.firstChunk.pos.y > 6000f || meltTimer > 0)
				{
					meltTimer++;
				}
			}
			else
			{
				meltingObject.firstChunk.vel += new Vector2(0f, 0.5f);
				if (meltingObject.firstChunk.pos.y < 500f || meltTimer > 0)
				{
					meltTimer++;
				}
			}
			if (meltTimer > maxMeltTime)
			{
				Custom.Log($"Object melted by void sea {meltingObject.abstractPhysicalObject}");
				while (meltingObject.grabbedBy.Count > 0)
				{
					meltingObject.grabbedBy[0].Release();
				}
				for (int i = 0; i < 12; i++)
				{
					voidScene.room.AddObject(new VoidParticle(meltingObject.firstChunk.pos + Custom.RNV() * 12f, Custom.DegToVec(Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), new Vector2(0f, 1f)) + (float)UnityEngine.Random.Range(-5, 5)) * UnityEngine.Random.Range(0.25f, 3f), UnityEngine.Random.Range(20f, 80f)));
				}
				voidScene.room.RemoveObject(meltingObject);
				meltingObject.abstractPhysicalObject.Destroy();
				meltingObject.Destroy();
				meltingObject = null;
			}
			else
			{
				if (UnityEngine.Random.value < dissolved() / 10f && meltingObject.grabbedBy.Count > 0)
				{
					meltingObject.grabbedBy[0].Release();
				}
				if (dissolved() > 0f && UnityEngine.Random.value < dissolved() * 2f)
				{
					voidScene.room.AddObject(new VoidParticle(meltingObject.firstChunk.pos + Custom.RNV() * 12f, Custom.DegToVec(Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), new Vector2(0f, 1f)) + (float)UnityEngine.Random.Range(-5, 5)) * UnityEngine.Random.Range(0.25f, 3f), UnityEngine.Random.Range(20f, 80f)));
				}
			}
		}

		public static bool Valid(PhysicalObject obj, Room room)
		{
			if (obj != null && obj.Submersion > 0.5f && !obj.slatedForDeletetion)
			{
				return obj.room == room;
			}
			return false;
		}

		public float dissolved()
		{
			return (float)meltTimer / (float)maxMeltTime;
		}
	}

	public Vector2 cameraOffset;

	public bool[] lastVoidSeaModes;

	public PlayerGhosts playerGhosts;

	public List<VoidWorm> worms;

	public float voidWormsAltitude;

	public DeepDivePhase deepDivePhase = DeepDivePhase.Start;

	public bool secondSpace;

	public bool ridingWorm;

	public int eggScenarioTimer;

	public float eggProximity;

	public TheEgg theEgg;

	public bool playerInRoom;

	public DisembodiedDynamicSoundLoop wooshLoop;

	public DisembodiedDynamicSoundLoop wormsLoop;

	public DisembodiedDynamicSoundLoop eggLoop;

	public DisembodiedDynamicSoundLoop swimLoop;

	public float playerY;

	public bool playerDipped;

	public VoidSeaBkg voidSeaBackground;

	public SaintEndingPhase saintEndPhase;

	public float timeInSaintPhase;

	public int editObject;

	public SaintsJourneyIllustration storedJourneyIllustration;

	private float fadeOutSaint;

	public bool fadeOutLights;

	public int fadeOutLightsTimer;

	public float musicFadeFac;

	public FadeOut blackFade;

	public bool endingSavedFlag;

	public List<MeltingItem> meltingObjects;

	public float SwimDownMusic
	{
		get
		{
			if (secondSpace || ridingWorm)
			{
				return 0f;
			}
			if (!Inverted)
			{
				return Mathf.Pow(Mathf.InverseLerp(1100f, -500f, playerY), 0.2f) * Mathf.InverseLerp(voidWormsAltitude - 1000f, -3000f, playerY);
			}
			if (saintEndPhase != SaintEndingPhase.Inactive)
			{
				return 0f;
			}
			if (playerY < 5000f)
			{
				return Mathf.Pow(Mathf.InverseLerp(0f, 5000f, playerY), 0.2f);
			}
			return Mathf.Pow(Mathf.InverseLerp(31000f, 5000f, playerY), 0.2f);
		}
	}

	public float BigOpenMusic
	{
		get
		{
			if (secondSpace || ridingWorm)
			{
				return 0f;
			}
			if (!Inverted)
			{
				return Mathf.Pow(Custom.SCurve(Mathf.InverseLerp(-1000f, voidWormsAltitude + 2000f, playerY), 0.65f), 0.65f) * Custom.SCurve(Mathf.InverseLerp(voidWormsAltitude - 13000f, voidWormsAltitude - 3000f, playerY), 1.7f);
			}
			if (saintEndPhase != SaintEndingPhase.Inactive)
			{
				return 0f;
			}
			if (playerY < 20000f)
			{
				return Mathf.Pow(Mathf.InverseLerp(5000f, 20000f, playerY), 0.2f);
			}
			return Mathf.Pow(Mathf.InverseLerp(30000f, 20000f, playerY), 0.2f);
		}
	}

	public float WormsMusic
	{
		get
		{
			if (secondSpace || ridingWorm)
			{
				return 0f;
			}
			if (Inverted && saintEndPhase != SaintEndingPhase.Inactive)
			{
				return 0f;
			}
			return Custom.SCurve(Mathf.InverseLerp(9000f, 2000f, Mathf.Abs(voidWormsAltitude - playerY)), 0.65f);
		}
	}

	public float TheRideMusic
	{
		get
		{
			if (!ridingWorm || (int)deepDivePhase >= (int)DeepDivePhase.MovedIntoSecondSpace)
			{
				return 0f;
			}
			return 1f;
		}
	}

	public float SlugcatGhostMusic
	{
		get
		{
			if (!secondSpace || theEgg == null)
			{
				return 0f;
			}
			if (playerGhosts == null || playerGhosts.ghosts.Count <= 0)
			{
				return 0f;
			}
			return Mathf.Lerp(0.5f, 1f, theEgg.musicVolumeDirectionBoost) * (1f - theEgg.whiteFade);
		}
	}

	public float TheLightMusic
	{
		get
		{
			if (!secondSpace || theEgg == null)
			{
				return 0f;
			}
			return theEgg.musicVolume * (1f - theEgg.whiteFade);
		}
	}

	public bool Inverted
	{
		get
		{
			if (ModManager.MSC)
			{
				return room.waterInverted;
			}
			return false;
		}
	}

	public float VSS_DeathMusic
	{
		get
		{
			if (!(saintEndPhase == SaintEndingPhase.WormDeath))
			{
				return 0f;
			}
			return 1f;
		}
	}

	public float VSS_TransformWaitMusic
	{
		get
		{
			if (!(saintEndPhase == SaintEndingPhase.EchoTransform))
			{
				return 0f;
			}
			return 1f;
		}
	}

	public float VSS_TransformMusic
	{
		get
		{
			if (saintEndPhase == SaintEndingPhase.EchoTransform)
			{
				return Mathf.Min(1f, timeInSaintPhase / 1500f);
			}
			if (saintEndPhase == SaintEndingPhase.Drowned)
			{
				return Mathf.Max(0f, 1f - timeInSaintPhase / 400f);
			}
			return 0f;
		}
	}

	public VoidSeaScene(Room room)
		: base(room)
	{
		base.room = room;
		if (ModManager.MMF)
		{
			meltingObjects = new List<MeltingItem>();
			foreach (AbstractWorldEntity entity in base.room.abstractRoom.entities)
			{
				if (entity is AbstractPhysicalObject && (entity as AbstractPhysicalObject).type != AbstractPhysicalObject.AbstractObjectType.Creature && (entity as AbstractPhysicalObject).realizedObject != null && MeltingItem.Valid((entity as AbstractPhysicalObject).realizedObject, base.room))
				{
					Custom.Log($"Add premelt object {entity as AbstractPhysicalObject}");
					AddMeltObject((entity as AbstractPhysicalObject).realizedObject);
				}
			}
		}
		if (ModManager.MSC)
		{
			saintEndPhase = SaintEndingPhase.Inactive;
		}
		if (Inverted)
		{
			voidWormsAltitude = room.PixelHeight + 30000f;
			playerY = -10000f;
			sceneOrigo = new Vector2(room.PixelWidth / 2f, room.PixelHeight);
		}
		else
		{
			voidWormsAltitude = -11000f;
			playerY = 10000f;
			sceneOrigo = new Vector2(room.PixelWidth / 2f, 0f);
		}
		voidSeaBackground = new VoidSeaBkg(this);
		AddElement(voidSeaBackground);
		AddElement(new VoidSeaFade(this));
		theEgg = new TheEgg(this, new Vector2(-200000f, -200000f));
		AddElement(theEgg);
		for (int i = 0; i < 20; i++)
		{
			float f = (float)i / 19f;
			float y = 580f;
			if (Inverted)
			{
				y = 22000f;
			}
			AddElement(new VoidCeiling(this, "clouds" + (i % 3 + 1), new Vector2(0f, y), Mathf.Lerp(1.5f, 25f, Mathf.Pow(f, 1.5f)), i));
		}
		for (int j = 0; j < 150; j++)
		{
			AddElement(new VoidSprite(this, Mathf.Lerp(0.5f, 15f, Mathf.Pow(UnityEngine.Random.value, 2f)), j));
		}
		lastVoidSeaModes = new bool[room.game.cameras.Length];
		worms = new List<VoidWorm>();
		int num = 5;
		for (int k = 0; k < num; k++)
		{
			float f2 = (float)k / (float)(num - 1);
			worms.Add(new VoidWorm(this, default(Vector2), Mathf.Lerp(1f, 12f, Mathf.Pow(f2, 2f)), k == 0));
			AddElement(worms[worms.Count - 1]);
		}
		num = 10;
		for (int l = 0; l < num; l++)
		{
			float f3 = (float)l / (float)(num - 1);
			worms.Add(new VoidWorm(this, default(Vector2), Mathf.Lerp(8f, 50f, Mathf.Pow(f3, 0.75f)), mainWorm: false));
			AddElement(worms[worms.Count - 1]);
		}
		int num2 = 150;
		for (int m = 0; m < num2; m++)
		{
			float t = (float)m / (float)(num2 - 1);
			float num3 = Mathf.Lerp(1f / 33f, 0.0010204081f, t);
			float depth = Mathf.Lerp(1f / num3, Mathf.Lerp(33f, 980f, t), 0.85f);
			AddElement(new DistantWormLight(this, depth, m));
		}
		if (Inverted)
		{
			storedJourneyIllustration = new SaintsJourneyIllustration(9, this, new Vector2(683f, 384f));
			AddElement(storedJourneyIllustration);
		}
		room.game.cameras[0].voidSeaGoldFilter = 1f;
		wooshLoop = new DisembodiedDynamicSoundLoop(this);
		wooshLoop.sound = SoundID.Void_Sea_Worm_Swimby_Woosh_LOOP;
		wooshLoop.VolumeGroup = 1;
		wooshLoop.Volume = 0f;
		wormsLoop = new DisembodiedDynamicSoundLoop(this);
		wormsLoop.sound = SoundID.Void_Sea_Worms_Bkg_LOOP;
		wormsLoop.VolumeGroup = 1;
		wormsLoop.Volume = 0f;
		swimLoop = new DisembodiedDynamicSoundLoop(this);
		swimLoop.sound = SoundID.Void_Sea_Swim_LOOP;
		swimLoop.VolumeGroup = 1;
		swimLoop.Volume = 0f;
		Custom.Log("void sea spawning");
	}

	public override void Update(bool eu)
	{
		if (!playerInRoom && room.game.cameras[0].room == room && room.game.cameras[0].currentCameraPosition != 0)
		{
			Custom.Log("INIT VOID SEA");
			playerInRoom = true;
			for (int i = 0; i < room.world.NumberOfRooms; i++)
			{
				if (room.world.firstRoomIndex + i != room.abstractRoom.index)
				{
					for (int num = room.world.GetAbstractRoom(room.world.firstRoomIndex + i).entities.Count - 1; num >= 0; num--)
					{
						room.world.GetAbstractRoom(room.world.firstRoomIndex + i).entities[num].Destroy();
					}
					for (int num2 = room.world.GetAbstractRoom(room.world.firstRoomIndex + i).entitiesInDens.Count - 1; num2 >= 0; num2--)
					{
						room.world.GetAbstractRoom(room.world.firstRoomIndex + i).entitiesInDens[num2].Destroy();
					}
				}
			}
		}
		if (ModManager.MSC && fadeOutLights)
		{
			for (int j = 0; j < worms.Count; j++)
			{
				worms[j].lightAlpha = Mathf.Max(0f, worms[j].lightAlpha - 0.001f);
			}
			for (int k = 0; k < elements.Count; k++)
			{
				if (elements[k] is DistantWormLight)
				{
					(elements[k] as DistantWormLight).alpha = Mathf.Max(0f, (elements[k] as DistantWormLight).alpha - 0.001f);
				}
			}
			Player player = room.game.FirstRealizedPlayer;
			if (ModManager.CoopAvailable)
			{
				player = room.game.RealizedPlayerFollowedByCamera;
			}
			if (player != null)
			{
				fadeOutLightsTimer++;
				if (fadeOutLightsTimer == 1)
				{
					DestroyCeiling();
				}
				if (fadeOutLightsTimer == 1000)
				{
					DestroyAllWormsExceptMainWorm();
				}
				if (fadeOutLightsTimer >= 1000)
				{
					ArtificerEndUpdate(player, fadeOutLightsTimer - 1000);
				}
				else
				{
					musicFadeFac = (float)fadeOutLightsTimer / 1000f;
				}
			}
		}
		bool flag = false;
		if (!ModManager.CoopAvailable)
		{
			for (int l = 0; l < room.game.Players.Count; l++)
			{
				if (room.game.Players[l].realizedCreature == null || room.game.Players[l].realizedCreature.room != room)
				{
					continue;
				}
				flag = true;
				playerY = room.game.Players[l].realizedCreature.mainBodyChunk.pos.y;
				(room.game.Players[l].realizedCreature as Player).inVoidSea = (playerY < sceneOrigo.y && !Inverted) || (playerY > sceneOrigo.y && Inverted);
				if (!playerDipped && room.game.Players[l].realizedCreature.Submersion > 0.5f)
				{
					if (room.game.manager.musicPlayer != null)
					{
						room.game.manager.musicPlayer.RequestVoidSeaMusic(this);
					}
					if ((room.game.Players[0].realizedCreature as Player).redsIllness != null)
					{
						(room.game.Players[0].realizedCreature as Player).redsIllness.GetBetter();
					}
					if ((room.game.Players[0].realizedCreature as Player).Malnourished)
					{
						(room.game.Players[0].realizedCreature as Player).SetMalnourished(m: false);
					}
					playerDipped = true;
				}
				break;
			}
		}
		else
		{
			bool dead = (room.game.Players[0].state as PlayerState).dead;
			foreach (AbstractCreature alivePlayer in room.game.AlivePlayers)
			{
				if (alivePlayer.realizedCreature == null)
				{
					continue;
				}
				Player player2 = alivePlayer.realizedCreature as Player;
				if (player2.room != room)
				{
					continue;
				}
				flag = true;
				if (player2 != room.game.FirstRealizedPlayer && !dead)
				{
					continue;
				}
				playerY = player2.mainBodyChunk.pos.y;
				player2.inVoidSea = (playerY < sceneOrigo.y && !Inverted) || (playerY > sceneOrigo.y && Inverted);
				if (!playerDipped && !(player2.Submersion <= 0.5f))
				{
					if (room.game.manager.musicPlayer != null)
					{
						room.game.manager.musicPlayer.RequestVoidSeaMusic(this);
					}
					playerDipped = true;
					room.game.cameras[0].EnterCutsceneMode(player2.abstractCreature, RoomCamera.CameraCutsceneType.VoidSea);
					break;
				}
			}
			if (playerDipped)
			{
				foreach (Player item in (from x in room.game.NonPermaDeadPlayers
					select (Player)x.realizedCreature into y
					where y != null
					select y).ToList())
				{
					if (item.redsIllness != null)
					{
						item.redsIllness.GetBetter();
					}
					if (item.Malnourished)
					{
						item.SetMalnourished(m: false);
					}
				}
			}
		}
		if (!flag || !room.game.cameras[0].InCutscene)
		{
			playerDipped = false;
		}
		if (room.game.rainWorld.skipVoidSea && playerDipped && playerY < 1045f && room.game.manager.upcomingProcess == null)
		{
			room.game.ExitToVoidSeaSlideShow();
			Custom.Log("SKIP VOID");
		}
		if (!secondSpace)
		{
			float a = 0f;
			float num3 = float.MaxValue;
			if (Inverted)
			{
				room.game.cameras[0].virtualMicrophone.volumeGroups[2] = Mathf.InverseLerp(16000f, 40000f, room.game.cameras[0].pos.y) * (1f - musicFadeFac);
			}
			else
			{
				room.game.cameras[0].virtualMicrophone.volumeGroups[2] = Mathf.InverseLerp(-40000f, -16000f, room.game.cameras[0].pos.y) * (1f - musicFadeFac);
			}
			a = Mathf.Max(a, Mathf.InverseLerp(0f, 0.9f, room.game.cameras[0].screenShake));
			if (!secondSpace)
			{
				num3 = Mathf.Min(num3, Mathf.Abs(room.game.cameras[0].pos.y - voidWormsAltitude));
			}
			if (wooshLoop.Volume < a)
			{
				wooshLoop.Volume = Custom.LerpAndTick(wooshLoop.Volume * (1f - musicFadeFac), a, 0.07f, 0.05f);
			}
			else
			{
				wooshLoop.Volume = Custom.LerpAndTick(wooshLoop.Volume * (1f - musicFadeFac), a, 0.006f, 1f / 60f);
			}
			wooshLoop.Update();
			wormsLoop.Volume = Mathf.Pow(Custom.SCurve(Mathf.InverseLerp(8000f, 2000f, num3), 0.6f), 0.75f) * (1f - musicFadeFac);
			wormsLoop.Update();
		}
		else
		{
			room.game.cameras[0].virtualMicrophone.volumeGroups[2] = 0f;
			if (eggLoop != null && theEgg != null)
			{
				eggLoop.Volume = Mathf.Pow(Custom.SCurve(Mathf.InverseLerp(0.78f, 0.98f, eggProximity), 0.5f), 1.7f) * (1f - theEgg.whiteFade);
				eggLoop.Update();
			}
		}
		if (playerGhosts != null)
		{
			playerGhosts.Update();
		}
		base.Update(eu);
		if (ModManager.MSC)
		{
			for (int m = 0; m < room.abstractRoom.creatures.Count; m++)
			{
				if (room.abstractRoom.creatures[m].realizedCreature != null && room.abstractRoom.creatures[m].creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
				{
					VoidSeaTreatment(room.abstractRoom.creatures[m].realizedCreature as Player, 0.95f);
				}
			}
		}
		for (int n = 0; n < room.game.Players.Count; n++)
		{
			AbstractCreature abstractCreature = room.game.Players[n];
			if (abstractCreature.Room.index == room.abstractRoom.index && abstractCreature.realizedCreature != null)
			{
				if (!ModManager.CoopAvailable)
				{
					UpdatePlayerInVoidSea(abstractCreature.realizedCreature as Player);
				}
				else if (abstractCreature == room.game.cameras[0].followAbstractCreature)
				{
					UpdatePlayerInVoidSea(abstractCreature.realizedCreature as Player);
				}
				else
				{
					VoidSeaTreatment(abstractCreature.realizedCreature as Player, 0.95f);
				}
			}
		}
		Player player3 = room.game.FirstRealizedPlayer;
		if (ModManager.CoopAvailable)
		{
			player3 = room.game.RealizedPlayerFollowedByCamera;
		}
		if (player3 != null)
		{
			swimLoop.Update();
			float val = Vector2.Distance(player3.bodyChunks[1].lastPos, player3.bodyChunks[1].pos) + Vector2.Distance(player3.bodyChunks[0].lastPos, player3.bodyChunks[0].pos);
			swimLoop.Volume = Custom.LerpMap(val, 0f, 8f, 0.3f, 1f);
			swimLoop.Pitch = Custom.LerpMap(val, 0f, 7f, 0.95f, 1.05f);
		}
		if (ModManager.MSC && room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			if (storedJourneyIllustration != null)
			{
				storedJourneyIllustration.fadeCounter *= 0.9f;
			}
			SaintEndUpdate();
		}
		if (!ModManager.MMF)
		{
			return;
		}
		for (int num4 = 0; num4 < meltingObjects.Count; num4++)
		{
			if (!MeltingItem.Valid(meltingObjects[num4].meltingObject, room))
			{
				Custom.Log("remove invalidated melt");
				meltingObjects.RemoveAt(num4);
			}
			else
			{
				meltingObjects[num4].Update();
			}
		}
	}

	public void UpdatePlayerInVoidSea(Player voidSeaPlayer)
	{
		VoidSeaTreatment(voidSeaPlayer, 0.95f);
		bool flag = voidSeaPlayer.mainBodyChunk.pos.y < 240f && voidSeaPlayer.mainBodyChunk.pos.y > -40f;
		if (Inverted)
		{
			flag = voidSeaPlayer.mainBodyChunk.pos.y > room.PixelHeight + 100f && voidSeaPlayer.mainBodyChunk.pos.y < room.PixelHeight + 380f;
		}
		if (!secondSpace && flag)
		{
			float num = 2200f;
			float num2 = 2900f;
			if (Inverted)
			{
				num = 0f;
				num2 = room.PixelWidth;
			}
			if (voidSeaPlayer.mainBodyChunk.pos.x < num)
			{
				Move(voidSeaPlayer, new Vector2((num + num2) * 0.5f - voidSeaPlayer.mainBodyChunk.pos.x, 0f), moveCamera: true);
			}
			else if (voidSeaPlayer.mainBodyChunk.pos.x > num2)
			{
				Move(voidSeaPlayer, new Vector2((num + num2) * 0.5f - voidSeaPlayer.mainBodyChunk.pos.x, 0f), moveCamera: true);
			}
		}
		if ((int)deepDivePhase >= (int)DeepDivePhase.EggScenario && voidSeaPlayer.mainBodyChunk.pos.y > -10000f)
		{
			Custom.Log("second space mov");
			Vector2 vector = theEgg.pos - voidSeaPlayer.mainBodyChunk.pos;
			Move(voidSeaPlayer, new Vector2(0f, -11000f - voidSeaPlayer.mainBodyChunk.pos.x), moveCamera: true);
			theEgg.pos = voidSeaPlayer.mainBodyChunk.pos + vector;
		}
		for (int i = 0; i < room.game.cameras.Length; i++)
		{
			if (room.game.cameras[i].followAbstractCreature == voidSeaPlayer.abstractCreature)
			{
				room.game.cameras[i].voidSeaMode = (voidSeaPlayer.mainBodyChunk.pos.y < 240f && !Inverted) || (Inverted && voidSeaPlayer.mainBodyChunk.pos.y > room.PixelHeight + 100f);
				if (room.game.cameras[i].voidSeaMode && !lastVoidSeaModes[i])
				{
					cameraOffset *= 0f;
					room.game.cameras[i].pos = voidSeaPlayer.mainBodyChunk.pos - new Vector2(700f, 400f);
					room.game.cameras[i].lastPos = room.game.cameras[i].pos;
				}
				lastVoidSeaModes[i] = room.game.cameras[i].voidSeaMode;
			}
		}
		if (deepDivePhase == DeepDivePhase.EggScenario && eggScenarioTimer < 2800 && (voidSeaPlayer.input[0].x != 0 || voidSeaPlayer.input[0].y != 0))
		{
			eggScenarioTimer++;
		}
	}

	public void UpdatePlayersJolly()
	{
		for (int i = 0; i < room.game.Players.Count; i++)
		{
			if (room.game.Players[i].Room.index != room.abstractRoom.index || room.game.Players[i].realizedCreature == null)
			{
				continue;
			}
			VoidSeaTreatment(room.game.Players[i].realizedCreature as Player, 0.95f);
			bool flag = room.game.Players[i].realizedCreature.mainBodyChunk.pos.y < 240f && room.game.Players[i].realizedCreature.mainBodyChunk.pos.y > -40f;
			if (Inverted)
			{
				flag = room.game.Players[i].realizedCreature.mainBodyChunk.pos.y > room.PixelHeight + 100f && room.game.Players[i].realizedCreature.mainBodyChunk.pos.y < room.PixelHeight + 380f;
			}
			if (!secondSpace && flag)
			{
				float num = 2200f;
				float num2 = 2900f;
				if (Inverted)
				{
					num = 0f;
					num2 = room.PixelWidth;
				}
				if (room.game.Players[i].realizedCreature.mainBodyChunk.pos.x < num)
				{
					Move(room.game.Players[i].realizedCreature as Player, new Vector2((num + num2) * 0.5f - room.game.Players[i].realizedCreature.mainBodyChunk.pos.x, 0f), moveCamera: true);
				}
				else if (room.game.Players[i].realizedCreature.mainBodyChunk.pos.x > num2)
				{
					Move(room.game.Players[i].realizedCreature as Player, new Vector2((num + num2) * 0.5f - room.game.Players[i].realizedCreature.mainBodyChunk.pos.x, 0f), moveCamera: true);
				}
			}
			if ((int)deepDivePhase >= (int)DeepDivePhase.EggScenario && room.game.Players[i].realizedCreature.mainBodyChunk.pos.y > -10000f)
			{
				Custom.Log("second space mov");
				Vector2 vector = theEgg.pos - room.game.Players[i].realizedCreature.mainBodyChunk.pos;
				Move(room.game.Players[i].realizedCreature as Player, new Vector2(0f, -11000f - room.game.Players[i].realizedCreature.mainBodyChunk.pos.x), moveCamera: true);
				theEgg.pos = room.game.Players[i].realizedCreature.mainBodyChunk.pos + vector;
			}
			for (int j = 0; j < room.game.cameras.Length; j++)
			{
				if (room.game.cameras[j].followAbstractCreature == room.game.Players[i])
				{
					room.game.cameras[j].voidSeaMode = (room.game.Players[i].realizedCreature.mainBodyChunk.pos.y < 240f && !Inverted) || (Inverted && room.game.Players[i].realizedCreature.mainBodyChunk.pos.y > room.PixelHeight + 100f);
					if (room.game.cameras[j].voidSeaMode && !lastVoidSeaModes[j])
					{
						cameraOffset *= 0f;
						room.game.cameras[j].pos = room.game.Players[i].realizedCreature.mainBodyChunk.pos - new Vector2(700f, 400f);
						room.game.cameras[j].lastPos = room.game.cameras[j].pos;
					}
					lastVoidSeaModes[j] = room.game.cameras[j].voidSeaMode;
				}
			}
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (deepDivePhase == DeepDivePhase.EggScenario && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && eggScenarioTimer < 2800 && ((firstAlivePlayer.realizedCreature as Player).input[0].x != 0 || (firstAlivePlayer.realizedCreature as Player).input[0].y != 0))
			{
				eggScenarioTimer++;
			}
			break;
		}
	}

	public void VoidSeaTreatment(Player player, float swimSpeed)
	{
		if (player.room != room)
		{
			return;
		}
		for (int i = 0; i < player.bodyChunks.Length; i++)
		{
			player.bodyChunks[i].restrictInRoomRange = float.MaxValue;
			player.bodyChunks[i].vel *= Mathf.Lerp(swimSpeed, 1f, room.game.cameras[0].voidSeaGoldFilter);
			if (Inverted)
			{
				player.bodyChunks[i].vel.y += player.buoyancy;
				player.bodyChunks[i].vel.y -= player.gravity;
			}
			else
			{
				player.bodyChunks[i].vel.y -= player.buoyancy;
				player.bodyChunks[i].vel.y += player.gravity;
			}
		}
		if (!ModManager.MSC || saintEndPhase != SaintEndingPhase.EchoTransform)
		{
			player.airInLungs = 1f;
			player.lungsExhausted = false;
		}
		if (player.graphicsModule != null && (player.graphicsModule as PlayerGraphics).lightSource != null)
		{
			if (Inverted)
			{
				(player.graphicsModule as PlayerGraphics).lightSource.setAlpha = Custom.LerpMap(player.mainBodyChunk.pos.y, 2000f, 8000f, 1f, 0.2f) * (1f - eggProximity);
				(player.graphicsModule as PlayerGraphics).lightSource.setRad = Custom.LerpMap(player.mainBodyChunk.pos.y, 2000f, 8000f, 300f, 200f) * (0.5f + 0.5f * (1f - eggProximity));
			}
			else
			{
				(player.graphicsModule as PlayerGraphics).lightSource.setAlpha = Custom.LerpMap(player.mainBodyChunk.pos.y, -2000f, -8000f, 1f, 0.2f) * (1f - eggProximity);
				(player.graphicsModule as PlayerGraphics).lightSource.setRad = Custom.LerpMap(player.mainBodyChunk.pos.y, -2000f, -8000f, 300f, 200f) * (0.5f + 0.5f * (1f - eggProximity));
			}
		}
		if (deepDivePhase == DeepDivePhase.EggScenario && UnityEngine.Random.value < 0.1f)
		{
			player.mainBodyChunk.vel += Custom.DirVec(player.mainBodyChunk.pos, theEgg.pos) * 0.02f * UnityEngine.Random.value;
		}
		if (ModManager.MMF && player.Submersion > 0.5f)
		{
			if (player.grasps[0] != null && !(player.grasps[0].grabbed is Creature))
			{
				AddMeltObject(player.grasps[0].grabbed);
			}
			if (player.grasps[1] != null && !(player.grasps[1].grabbed is Creature))
			{
				AddMeltObject(player.grasps[1].grabbed);
			}
			if (player.spearOnBack != null && player.spearOnBack.HasASpear)
			{
				player.spearOnBack.DropSpear();
				AddMeltObject(player.spearOnBack.spear);
			}
		}
	}

	public void Move(Player player, Vector2 move, bool moveCamera)
	{
		if (moveCamera)
		{
			cameraOffset -= move;
		}
		for (int i = 0; i < player.bodyChunks.Length; i++)
		{
			player.bodyChunks[i].pos += move;
			player.bodyChunks[i].lastPos += move;
			player.bodyChunks[i].lastLastPos += move;
		}
		if (player.graphicsModule != null)
		{
			for (int j = 0; j < player.graphicsModule.bodyParts.Length; j++)
			{
				player.graphicsModule.bodyParts[j].pos += move;
				player.graphicsModule.bodyParts[j].lastPos += move;
			}
			if ((player.graphicsModule as PlayerGraphics).lightSource != null)
			{
				(player.graphicsModule as PlayerGraphics).lightSource.HardSetPos((player.graphicsModule as PlayerGraphics).lightSource.Pos + move);
			}
			for (int k = 0; k < (player.graphicsModule as PlayerGraphics).drawPositions.GetLength(0); k++)
			{
				(player.graphicsModule as PlayerGraphics).drawPositions[k, 0] += move;
				(player.graphicsModule as PlayerGraphics).drawPositions[k, 1] += move;
			}
		}
		if (!moveCamera)
		{
			return;
		}
		for (int l = 0; l < room.game.cameras.Length; l++)
		{
			if (room.game.cameras[l].followAbstractCreature == player.abstractCreature)
			{
				room.game.cameras[l].pos += move;
				room.game.cameras[l].lastPos += move;
			}
		}
	}

	public void DestroyAllWormsExceptMainWorm()
	{
		Custom.Log("DESTROY CLOSE WORMS");
		deepDivePhase = new DeepDivePhase(ExtEnum<DeepDivePhase>.values.GetEntry(Math.Max(deepDivePhase.Index, DeepDivePhase.CloseWormsDestroyed.Index)));
		for (int num = worms.Count - 1; num >= 0; num--)
		{
			if (!worms[num].mainWorm)
			{
				worms[num].Destroy();
				worms.RemoveAt(num);
			}
		}
		for (int i = 0; i < room.game.Players.Count; i++)
		{
			if (room.game.Players[i].realizedCreature != null && room.game.Players[i].realizedCreature.room == room)
			{
				if ((room.game.Players[i].realizedCreature as Player).spearOnBack != null)
				{
					(room.game.Players[i].realizedCreature as Player).spearOnBack.DropSpear();
				}
				if (!ModManager.MSC || (room.game.Players[i].realizedCreature as Player).SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
				{
					(room.game.Players[i].realizedCreature as Player).objectInStomach = null;
				}
			}
		}
		for (int num2 = elements.Count - 1; num2 >= 0; num2--)
		{
			if (elements[num2] is VoidWorm && elements[num2].slatedForDeletetion)
			{
				elements.RemoveAt(num2);
			}
		}
	}

	public void DestroyCeiling()
	{
		Custom.Log("DESTROY CEILING");
		deepDivePhase = new DeepDivePhase(ExtEnum<DeepDivePhase>.values.GetEntry(Math.Max(deepDivePhase.Index, DeepDivePhase.CeilingDestroyed.Index)));
		for (int num = elements.Count - 1; num >= 0; num--)
		{
			if (elements[num] is VoidCeiling)
			{
				elements[num].Destroy();
				elements.RemoveAt(num);
			}
		}
	}

	public void DestroyDistantWorms()
	{
		Custom.Log("DESTROY DISTANT WORMS");
		deepDivePhase = new DeepDivePhase(ExtEnum<DeepDivePhase>.values.GetEntry(Math.Max(deepDivePhase.Index, DeepDivePhase.DistantWormsDestroyed.Index)));
		for (int num = elements.Count - 1; num >= 0; num--)
		{
			if (elements[num] is DistantWormLight)
			{
				elements[num].Destroy();
				elements.RemoveAt(num);
			}
		}
	}

	public void MovedToSecondSpace()
	{
		Player player = room.game.FirstRealizedPlayer;
		if (ModManager.CoopAvailable)
		{
			player = room.game.RealizedPlayerFollowedByCamera;
		}
		if (player != null)
		{
			Custom.Log("SECOND SPACE");
			secondSpace = true;
			deepDivePhase = new DeepDivePhase(ExtEnum<DeepDivePhase>.values.GetEntry(Math.Max(deepDivePhase.Index, DeepDivePhase.MovedIntoSecondSpace.Index)));
			float y = -5000f - player.mainBodyChunk.pos.y;
			if (Inverted)
			{
				y = 5000f - player.mainBodyChunk.pos.y;
			}
			Move(player, new Vector2(0f, y), moveCamera: true);
			if (wooshLoop.emitter != null)
			{
				wooshLoop.emitter.Destroy();
			}
			wooshLoop = null;
			if (wormsLoop.emitter != null)
			{
				wormsLoop.emitter.Destroy();
			}
			wormsLoop = null;
			worms[0].Move(new Vector2(0f, y));
			eggLoop = new DisembodiedDynamicSoundLoop(this);
			eggLoop.sound = SoundID.Void_Sea_The_Core_LOOP;
			eggLoop.VolumeGroup = 1;
			eggLoop.Volume = 0f;
		}
	}

	public void DestroyMainWorm()
	{
		Custom.Log("DESTROY MAIN WORM");
		deepDivePhase = new DeepDivePhase(ExtEnum<DeepDivePhase>.values.GetEntry(Math.Max(deepDivePhase.Index, DeepDivePhase.EggScenario.Index)));
		DeleteMainWorm();
		Player player = room.game.FirstRealizedPlayer;
		if (ModManager.CoopAvailable)
		{
			player = room.game.RealizedPlayerFollowedByCamera;
		}
		theEgg.pos = player.mainBodyChunk.pos + Custom.DegToVec(115f) * 11000f;
		playerGhosts = new PlayerGhosts(player, this);
	}

	public void SaintEndUpdate()
	{
		if (room.game.Players.Count == 0)
		{
			return;
		}
		AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
		Player player = null;
		if (firstAlivePlayer != null)
		{
			player = firstAlivePlayer.realizedCreature as Player;
		}
		if (player == null || player.graphicsModule == null)
		{
			return;
		}
		if (saintEndPhase == SaintEndingPhase.Drowned)
		{
			player.airInLungs = 0.04f;
			if (fadeOutSaint < 0f)
			{
				if (!endingSavedFlag)
				{
					if (room.game.Players.Count > 0 && firstAlivePlayer.realizedCreature != null)
					{
						room.game.rainWorld.progression.miscProgressionData.UpdateSaintStomach(firstAlivePlayer.realizedCreature as Player);
					}
					RainWorldGame.BeatGameMode(room.game, standardVoidSea: false);
					room.game.overWorld.InitiateSpecialWarp_SingleRoom(null, "SI_SAINTINTRO");
				}
				endingSavedFlag = true;
			}
			else
			{
				fadeOutSaint -= 1f;
			}
		}
		else
		{
			if (!(saintEndPhase != SaintEndingPhase.Inactive))
			{
				return;
			}
			if (saintEndPhase == SaintEndingPhase.EchoTransform && timeInSaintPhase > 480f)
			{
				(player.graphicsModule as PlayerGraphics).darkenFactor = Mathf.Lerp((player.graphicsModule as PlayerGraphics).darkenFactor, 1f, 0.002f);
			}
			if (saintEndPhase == SaintEndingPhase.EchoTransform && timeInSaintPhase == 120f && room.game.manager.musicPlayer != null && room.game.manager.musicPlayer.song != null)
			{
				room.game.manager.musicPlayer.song.ResetSongStream();
			}
			float num = 1500f;
			float num2 = 240f;
			player.airInLungs = 1f;
			if (timeInSaintPhase > num + num2 * 5f)
			{
				player.airInLungs = Mathf.Clamp(Mathf.InverseLerp(num + num2 * 7f, num + num2 * 5f, timeInSaintPhase), 0.01f, 1f);
				if (player.airInLungs < 0.04f)
				{
					fadeOutSaint = 500f;
					saintEndPhase = SaintEndingPhase.Drowned;
					timeInSaintPhase = 0f;
					room.AddObject(new FadeOut(room, Color.white, 400f, fadeIn: false));
				}
				if (player.airInLungs < 0.03f)
				{
					player.airInLungs = 0.03f;
				}
			}
			else if (timeInSaintPhase > num)
			{
				for (int i = 0; i < 4; i++)
				{
					if (timeInSaintPhase > num + num2 * (float)i && (player.graphicsModule as PlayerGraphics).tentaclesVisible < i + 1)
					{
						(player.graphicsModule as PlayerGraphics).tentacles[(player.graphicsModule as PlayerGraphics).tentaclesVisible].SetPosition(player.firstChunk.pos);
						(player.graphicsModule as PlayerGraphics).tentaclesVisible++;
						player.mainBodyChunk.vel += Custom.RNV() * 50f;
						for (int j = 0; j < 20; j++)
						{
							float value = UnityEngine.Random.value;
							Spark spark = new Spark(player.mainBodyChunk.pos, Custom.RNV() * value * 5f, new Color(0.01f, 0.01f, 0.01f), null, 10 + (int)((1f - value) * 60f), 36 + (int)((1f - value) * 200f));
							spark.gravity = 0f;
							room.AddObject(spark);
						}
					}
				}
			}
			if ((player.graphicsModule as PlayerGraphics).darkenFactor > 0.35f && UnityEngine.Random.value < (player.graphicsModule as PlayerGraphics).darkenFactor * 0.25f)
			{
				for (int k = 0; k < 4; k++)
				{
					float value2 = UnityEngine.Random.value;
					Spark spark2 = new Spark(player.mainBodyChunk.pos, Custom.RNV() * value2, new Color(0.01f, 0.01f, 0.01f), null, 4 + (int)((1f - value2) * 20f), 18 + (int)((1f - value2) * 100f));
					spark2.gravity = 0f;
					room.AddObject(spark2);
				}
			}
			timeInSaintPhase += 1f;
		}
	}

	public void switchSaintEndPhase(SaintEndingPhase phase)
	{
		saintEndPhase = phase;
		timeInSaintPhase = 0f;
	}

	public void ArtificerEndUpdate(Player player, int timer)
	{
		if (timer == 720)
		{
			player.InitiateDissolve();
		}
		int num = 960;
		int num2 = 1400;
		if (timer > num && timer < num + num2)
		{
			if (eggLoop == null)
			{
				eggLoop = new DisembodiedDynamicSoundLoop(this);
				eggLoop.sound = SoundID.Void_Sea_The_Core_LOOP;
				eggLoop.VolumeGroup = 1;
				theEgg = null;
			}
			eggLoop.Volume = Mathf.Max(0f, (float)(timer - num) / (float)num2 * 0.9f);
			eggLoop.Update();
			player.dissolved = Mathf.Max(0.01f, (float)(timer - num) / (float)num2 * 0.4f);
		}
		else if (timer >= num + num2)
		{
			if (blackFade == null)
			{
				blackFade = new FadeOut(room, Color.black, 200f, fadeIn: false);
				room.AddObject(blackFade);
			}
			eggLoop.Volume = 0.9f - Mathf.Max(0f, (float)(timer - (num + num2)) / 200f * 0.9f);
			eggLoop.Update();
		}
		if (blackFade != null && blackFade.IsDoneFading())
		{
			if (!endingSavedFlag)
			{
				room.game.ExitToVoidSeaSlideShow();
			}
			endingSavedFlag = true;
		}
	}

	public void DeleteMainWorm()
	{
		worms[0].Destroy();
		worms.Clear();
		for (int num = elements.Count - 1; num >= 0; num--)
		{
			if (elements[num] is VoidWorm)
			{
				elements.RemoveAt(num);
			}
		}
	}

	public void AddMeltObject(PhysicalObject obj)
	{
		bool flag = true;
		foreach (MeltingItem meltingObject in meltingObjects)
		{
			if (meltingObject != null && MeltingItem.Valid(meltingObject.meltingObject, room) && meltingObject.meltingObject.abstractPhysicalObject == obj.abstractPhysicalObject)
			{
				flag = false;
				break;
			}
		}
		if (flag && MeltingItem.Valid(obj, room))
		{
			Custom.Log($"Add melt object {obj}");
			meltingObjects.Add(new MeltingItem(obj, this));
		}
	}
}
