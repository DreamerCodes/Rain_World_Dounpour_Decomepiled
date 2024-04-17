using System;
using System.IO;
using System.Text;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class Love : CosmeticSprite
{
	public class WireframeMesh
	{
		private Love love;

		private Vector2[,] cubeVerts;

		public int startSprite;

		public int numberOfSprites;

		public Vector2 pos;

		public float rotation;

		public float fadePulse;

		public float yOff;

		private float pixelScaleFactor;

		public float alpha;

		public WireframeMesh(Love love, int startSprite, Vector2 pos)
		{
			this.love = love;
			this.startSprite = startSprite;
			this.pos = pos;
			numberOfSprites = 108;
			cubeVerts = new Vector2[3, 8];
			rotation = 0f;
			fadePulse = 0f;
			yOff = 0f;
			pixelScaleFactor = 0f;
		}

		private Vector2[] diamondVertices(float x_center, float y_center, float length, float rotation)
		{
			float num = length / Mathf.Sqrt(2f);
			float num2 = rotation % ((float)Math.PI * 2f);
			float num3 = num2 / (float)Math.PI;
			if (num2 >= (float)Math.PI)
			{
				num3 = 1f - (num2 - (float)Math.PI) / (float)Math.PI;
			}
			return new Vector2[4]
			{
				new Vector2(x_center - num * num3, y_center),
				new Vector2(x_center, y_center + num),
				new Vector2(x_center + num * num3, y_center),
				new Vector2(x_center, y_center - num)
			};
		}

		private Vector2[] cubeVertices(float x_center, float y_center, float length, float depth, float rotation)
		{
			_ = length / Mathf.Sqrt(2f);
			float num = rotation % ((float)Math.PI * 2f);
			float num2 = num / (float)Math.PI;
			if (num >= (float)Math.PI)
			{
				num2 = 1f - (num - (float)Math.PI) / (float)Math.PI;
			}
			float x_center2 = x_center - depth * (1f - num2);
			float x_center3 = x_center + depth * (1f - num2);
			Vector2[] array = new Vector2[8];
			Vector2[] array2 = diamondVertices(x_center2, y_center, length, rotation);
			for (int i = 0; i < 4; i++)
			{
				array[i] = array2[i];
			}
			array2 = diamondVertices(x_center3, y_center, length, rotation);
			for (int j = 0; j < 4; j++)
			{
				array[j + 4] = array2[j];
			}
			return array;
		}

		private void drawLine(RoomCamera.SpriteLeaser sLeaser, int ind, Vector2 start, Vector2 end, Color color, float alpha)
		{
			sLeaser.sprites[ind].x = start.x;
			sLeaser.sprites[ind].y = start.y;
			sLeaser.sprites[ind].rotation = Custom.VecToDeg(Custom.DirVec(start, end));
			sLeaser.sprites[ind].scaleX = pixelScaleFactor;
			sLeaser.sprites[ind].scaleY = Custom.Dist(start, end) * pixelScaleFactor;
			sLeaser.sprites[ind].color = color;
			sLeaser.sprites[ind].alpha = alpha;
		}

		private void drawCubeWire(RoomCamera.SpriteLeaser sLeaser, int startInd, float x_center, float y_center, float length, float depth, float rotation, float alpha, Color main_color, Color sub_color, int[] sub_faces, int[] sub_edges)
		{
			Vector2[] array = cubeVertices(x_center, y_center, length, depth, rotation);
			for (int i = 0; i < 4; i++)
			{
				Color color = main_color;
				float num = 1f;
				for (int j = 0; j < sub_faces.Length; j++)
				{
					if (sub_faces[j] == i)
					{
						color = sub_color;
						num = 0.25f;
						break;
					}
				}
				drawLine(sLeaser, startInd + i, array[i], array[(i + 1) % 4], color, alpha * num);
			}
			for (int k = 0; k < 4; k++)
			{
				Color color2 = main_color;
				float num2 = 1f;
				for (int l = 0; l < sub_edges.Length; l++)
				{
					if (sub_edges[l] == k)
					{
						color2 = sub_color;
						num2 = 0.25f;
						break;
					}
				}
				drawLine(sLeaser, startInd + k + 4, array[k], array[k + 4], color2, alpha * num2);
			}
			for (int m = 0; m < 4; m++)
			{
				Color color3 = main_color;
				float num3 = 1f;
				for (int n = 0; n < sub_faces.Length; n++)
				{
					if (sub_faces[n] == m)
					{
						color3 = sub_color;
						num3 = 0.25f;
						break;
					}
				}
				drawLine(sLeaser, startInd + m + 8, array[m + 4], array[(m + 1) % 4 + 4], color3, alpha * num3);
			}
		}

		private void drawFullWire(RoomCamera.SpriteLeaser sLeaser, int startInd, float x_center, float y_center, float length, float depth, float rotation, float alpha)
		{
			Color main_color = new Color(1f, 57f / 85f, 0.76862746f);
			Color sub_color = new Color(7f / 15f, 2f / 51f, 0.29803923f);
			float num = length / Mathf.Sqrt(2f);
			float num2 = rotation % ((float)Math.PI * 2f);
			float num3 = num2 / (float)Math.PI;
			if (num2 >= (float)Math.PI)
			{
				num3 = 1f - (num2 - (float)Math.PI) / (float)Math.PI;
			}
			drawCubeWire(sLeaser, startInd, x_center, y_center - num, length, depth, rotation, alpha, main_color, sub_color, new int[2] { 0, 1 }, new int[2] { 0, 2 });
			drawCubeWire(sLeaser, startInd + 12, x_center - num + num * (1f - num3), y_center, length, depth, rotation, alpha, main_color, sub_color, new int[1] { 2 }, new int[1] { 3 });
			drawCubeWire(sLeaser, startInd + 24, x_center + num - num * (1f - num3), y_center, length, depth, rotation, alpha, main_color, sub_color, new int[1] { 3 }, new int[1] { 3 });
		}

		public void Update()
		{
			rotation -= 0.047996555f;
			if (rotation < 0f)
			{
				rotation = (float)Math.PI * 4f;
			}
			yOff = Mathf.Sin(rotation / 2f) * 10f;
			if (rotation % (float)Math.PI < (float)Math.PI / 2f)
			{
				fadePulse = rotation % (float)Math.PI / ((float)Math.PI / 2f);
			}
			else
			{
				fadePulse = 1f - (rotation % (float)Math.PI - (float)Math.PI / 2f) / ((float)Math.PI / 2f);
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			for (int i = startSprite; i < startSprite + numberOfSprites; i++)
			{
				sLeaser.sprites[i] = new FSprite("Futile_White");
				sLeaser.sprites[i].anchorY = 0f;
				if (i == startSprite)
				{
					pixelScaleFactor = 1f / sLeaser.sprites[i].element.sourceRect.width;
				}
				sLeaser.sprites[i].scale = pixelScaleFactor;
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			float num = 32f;
			float num2 = 16f;
			drawFullWire(sLeaser, startSprite, pos.x - camPos.x, pos.y - camPos.y - yOff, num + fadePulse * 6f, num2 + fadePulse * 6f, rotation, (1f - fadePulse) * alpha);
			drawFullWire(sLeaser, startSprite + 36, pos.x - camPos.x, pos.y - camPos.y - yOff, num - fadePulse * 6f, num2 - fadePulse * 6f, rotation, (1f - fadePulse) * alpha);
			drawFullWire(sLeaser, startSprite + 72, pos.x - camPos.x, pos.y - camPos.y - yOff, num, num2, rotation, 1f * alpha);
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			for (int i = startSprite; i < startSprite + numberOfSprites; i++)
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public class RebootTransition : CosmeticSprite
	{
		public int timer;

		public float animLength;

		public float[] rots;

		public float[] yscales;

		public float[] xscales;

		public float[] xrate;

		public float[] yrate;

		public int[] delays;

		public RebootTransition(Vector2 position)
		{
			pos = position;
			animLength = 120f;
		}

		public override void Update(bool eu)
		{
			timer++;
			if (timer > 1)
			{
				for (int i = 0; i < rots.Length; i++)
				{
					if (timer >= delays[i])
					{
						xscales[i] += xrate[i];
						yscales[i] += yrate[i];
					}
				}
			}
			base.Update(eu);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[26];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["FlatLight"];
			sLeaser.sprites[0].color = new Color(0f, 0f, 0f);
			sLeaser.sprites[0].scale = 0f;
			rots = new float[sLeaser.sprites.Length - 1];
			yscales = new float[sLeaser.sprites.Length - 1];
			xscales = new float[sLeaser.sprites.Length - 1];
			yrate = new float[sLeaser.sprites.Length - 1];
			xrate = new float[sLeaser.sprites.Length - 1];
			delays = new int[sLeaser.sprites.Length - 1];
			for (int i = 1; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i] = new FSprite("Futile_White");
				sLeaser.sprites[i].anchorY = 0f;
				sLeaser.sprites[i].scaleX = 0f;
				sLeaser.sprites[i].scaleY = 0f;
				sLeaser.sprites[i].color = new Color(0f, 0f, 0f);
				rots[i - 1] = UnityEngine.Random.Range(0, 360);
				xscales[i - 1] = 0f;
				yscales[i - 1] = 0f;
				xrate[i - 1] = UnityEngine.Random.Range(0.005f, 0.15f);
				yrate[i - 1] = UnityEngine.Random.Range(8f, 35f);
				delays[i - 1] = UnityEngine.Random.Range(0, (int)(animLength * 0.4f));
				sLeaser.sprites[i].rotation = rots[i - 1];
			}
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("HUD2"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].x = pos.x - camPos.x;
			sLeaser.sprites[0].y = pos.y - camPos.y;
			sLeaser.sprites[0].scale = 250f * ((float)timer / animLength);
			for (int i = 1; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].x = pos.x - camPos.x;
				sLeaser.sprites[i].y = pos.y - camPos.y;
				sLeaser.sprites[i].scaleX = xscales[i - 1];
				sLeaser.sprites[i].scaleY = yscales[i - 1];
				sLeaser.sprites[i].rotation = rots[i - 1];
			}
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public new Vector2 pos;

	public WireframeMesh mesh;

	public bool pendingAssemble;

	public AtlasAnimator animator;

	public int timeUntilReboot;

	public bool activated;

	public float activateFlicker;

	public int rebootStatus;

	public RebootTransition rebootTrans;

	public int NumberOfSprites
	{
		get
		{
			if (mesh != null)
			{
				return mesh.numberOfSprites;
			}
			return 1;
		}
	}

	public bool WasRebooted()
	{
		string text = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/" + Encoding.ASCII.GetString(new byte[3] { 70, 69, 90 });
		if (Directory.Exists(text))
		{
			int num = 0;
			while (File.Exists(text + "/SaveSlot" + num))
			{
				BinaryReader binaryReader = new BinaryReader(new FileStream(text + "/SaveSlot" + num, FileMode.Open));
				binaryReader.ReadInt64();
				binaryReader.ReadInt64();
				binaryReader.ReadInt64();
				binaryReader.ReadBoolean();
				binaryReader.ReadBoolean();
				binaryReader.ReadBoolean();
				binaryReader.ReadBoolean();
				binaryReader.ReadBoolean();
				binaryReader.ReadBoolean();
				int num2 = binaryReader.ReadInt32();
				for (int i = 0; i < num2; i++)
				{
					if (binaryReader.ReadBoolean())
					{
						binaryReader.ReadString();
					}
					binaryReader.ReadBoolean();
				}
				if (binaryReader.ReadBoolean())
				{
					binaryReader.ReadString();
				}
				binaryReader.ReadInt32();
				binaryReader.ReadSingle();
				binaryReader.ReadSingle();
				binaryReader.ReadSingle();
				binaryReader.ReadInt64();
				num2 = binaryReader.ReadInt32();
				for (int j = 0; j < num2; j++)
				{
					if (binaryReader.ReadBoolean())
					{
						binaryReader.ReadString();
					}
				}
				binaryReader.ReadInt32();
				binaryReader.ReadInt32();
				binaryReader.ReadInt32();
				binaryReader.ReadInt32();
				binaryReader.ReadInt32();
				binaryReader.ReadInt32();
				num2 = binaryReader.ReadInt32();
				for (int k = 0; k < num2; k++)
				{
					if (binaryReader.ReadBoolean())
					{
						binaryReader.ReadString();
					}
				}
				num2 = binaryReader.ReadInt32();
				for (int l = 0; l < num2; l++)
				{
					binaryReader.ReadInt32();
				}
				num2 = binaryReader.ReadInt32();
				for (int m = 0; m < num2; m++)
				{
					if (binaryReader.ReadBoolean())
					{
						binaryReader.ReadString();
					}
				}
				num2 = binaryReader.ReadInt32();
				for (int n = 0; n < num2; n++)
				{
					if (binaryReader.ReadBoolean())
					{
						binaryReader.ReadString();
					}
				}
				if (binaryReader.ReadBoolean())
				{
					binaryReader.ReadString();
				}
				binaryReader.ReadBoolean();
				if (binaryReader.ReadBoolean())
				{
					binaryReader.ReadSingle();
				}
				binaryReader.ReadBoolean();
				binaryReader.ReadBoolean();
				binaryReader.ReadBoolean();
				binaryReader.ReadBoolean();
				binaryReader.ReadBoolean();
				num2 = binaryReader.ReadInt32();
				for (int num3 = 0; num3 < num2; num3++)
				{
					if (binaryReader.ReadBoolean())
					{
						binaryReader.ReadString();
					}
					int num4 = binaryReader.ReadInt32();
					for (int num5 = 0; num5 < num4; num5++)
					{
						binaryReader.ReadInt32();
						binaryReader.ReadInt32();
						binaryReader.ReadInt32();
					}
					num4 = binaryReader.ReadInt32();
					for (int num6 = 0; num6 < num4; num6++)
					{
						binaryReader.ReadInt32();
						binaryReader.ReadInt32();
						binaryReader.ReadInt32();
					}
					num4 = binaryReader.ReadInt32();
					for (int num7 = 0; num7 < num4; num7++)
					{
						binaryReader.ReadInt32();
					}
					num4 = binaryReader.ReadInt32();
					for (int num8 = 0; num8 < num4; num8++)
					{
						binaryReader.ReadInt32();
					}
					num4 = binaryReader.ReadInt32();
					for (int num9 = 0; num9 < num4; num9++)
					{
						binaryReader.ReadInt32();
					}
					num4 = binaryReader.ReadInt32();
					for (int num10 = 0; num10 < num4; num10++)
					{
						binaryReader.ReadInt32();
					}
					num4 = binaryReader.ReadInt32();
					for (int num11 = 0; num11 < num4; num11++)
					{
						binaryReader.ReadInt32();
					}
					num4 = binaryReader.ReadInt32();
					for (int num12 = 0; num12 < num4; num12++)
					{
						binaryReader.ReadInt32();
						binaryReader.ReadInt32();
					}
					if (binaryReader.ReadBoolean())
					{
						binaryReader.ReadSingle();
					}
					if (binaryReader.ReadBoolean())
					{
						binaryReader.ReadString();
					}
					binaryReader.ReadBoolean();
					binaryReader.ReadInt32();
					binaryReader.ReadInt32();
					binaryReader.ReadInt32();
					binaryReader.ReadInt32();
					binaryReader.ReadInt32();
					binaryReader.ReadInt32();
					num4 = binaryReader.ReadInt32();
					for (int num13 = 0; num13 < num4; num13++)
					{
						binaryReader.ReadInt32();
					}
					binaryReader.ReadInt32();
				}
				binaryReader.ReadBoolean();
				if (binaryReader.ReadBoolean())
				{
					return true;
				}
				num++;
			}
		}
		return false;
	}

	public Love(Vector2 pos)
	{
		this.pos = pos;
		mesh = new WireframeMesh(this, 0, this.pos);
		animator = null;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (base.slatedForDeletetion)
		{
			return;
		}
		if (room.game.rainWorld.progression != null && room.game.rainWorld.progression.miscProgressionData != null && room.game.rainWorld.progression.miscProgressionData.hasDoneHeartReboot)
		{
			Destroy();
		}
		if (mesh != null)
		{
			mesh.Update();
		}
		if (animator != null)
		{
			animator.Update();
		}
		if (activated)
		{
			if (activateFlicker == 0f)
			{
				GenerateLove();
				if (WasRebooted() && rebootStatus == 0)
				{
					rebootStatus = 1;
				}
				if (rebootStatus == 0)
				{
					room.PlayCustomSoundDisembodied("love", 0f, 1f, 1.2f);
				}
			}
			activateFlicker += 1f;
		}
		if (animator == null && activateFlicker >= 40f && rebootStatus == 1)
		{
			pendingAssemble = true;
		}
		if (mesh != null && pendingAssemble && Mathf.Abs(mesh.rotation - 9.574778f) < 0.03f)
		{
			Vector2 vector = new Vector2(pos.x + 217f, pos.y + 150f);
			AtlasAnimator atlasAnimator = new AtlasAnimator(0, vector, "loved", "loved", 121, loop: false, reverse: true);
			atlasAnimator.animSpeed = 1f;
			Love obj = new Love(pos, atlasAnimator);
			room.AddObject(obj);
			Destroy();
		}
		if (animator != null && animator.atlas == "loved" && animator.frame == 1f)
		{
			Vector2 vector2 = new Vector2(pos.x + 207f, pos.y + 37f);
			AtlasAnimator atlasAnimator2 = new AtlasAnimator(0, vector2, "love", "love", 208, loop: true, reverse: true);
			atlasAnimator2.animSpeed = 1f;
			atlasAnimator2.frame -= 5f;
			Love obj2 = new Love(pos, atlasAnimator2);
			room.AddObject(obj2);
			Destroy();
		}
		if (timeUntilReboot > 0)
		{
			timeUntilReboot--;
			if (timeUntilReboot == 0)
			{
				Reboot();
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[NumberOfSprites];
		if (mesh != null)
		{
			mesh.InitiateSprites(sLeaser, rCam);
		}
		if (animator != null)
		{
			if (animator.atlas == "loved")
			{
				room.PlayCustomSoundDisembodied("love", 0f, 1f, 1.2f);
			}
			animator.InitiateSprites(sLeaser, rCam);
		}
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
	{
		if (mesh != null)
		{
			mesh.AddToContainer(sLeaser, rCam, newContainer);
		}
		if (animator != null)
		{
			animator.AddToContainer(sLeaser, rCam, newContainer);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (mesh != null)
		{
			if (!activated)
			{
				mesh.alpha = 0f;
			}
			else if (activateFlicker < 120f)
			{
				mesh.alpha = UnityEngine.Random.value * (activateFlicker / 120f);
			}
			else
			{
				mesh.alpha = 1f;
			}
			mesh.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
		if (animator != null)
		{
			animator.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public void Reboot()
	{
		if (animator == null)
		{
			return;
		}
		if (room.game.rainWorld.progression != null && room.game.rainWorld.progression.miscProgressionData != null)
		{
			room.game.rainWorld.progression.miscProgressionData.hasDoneHeartReboot = true;
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
			{
				room.game.rainWorld.progression.miscProgressionData.UpdateSaintStomach(firstAlivePlayer.realizedCreature as Player);
			}
			room.game.rainWorld.progression.miscProgressionData.beaten_Saint = true;
			room.game.rainWorld.progression.miscProgressionData.SetTokenCollected(MoreSlugcatsEnums.SandboxUnlockID.FireBug);
			room.game.rainWorld.progression.miscProgressionData.SetTokenCollected(MoreSlugcatsEnums.SandboxUnlockID.FireEgg);
			room.game.rainWorld.progression.miscProgressionData.SetTokenCollected(MoreSlugcatsEnums.SandboxUnlockID.HellSpear);
			room.game.rainWorld.progression.miscProgressionData.SetTokenCollected(MoreSlugcatsEnums.LevelUnlockID.HR);
			room.game.rainWorld.progression.SaveProgression(saveMaps: false, saveMiscProg: true);
			room.game.rainWorld.progression.WipeSaveState(MoreSlugcatsEnums.SlugcatStatsName.Saint);
		}
		room.game.rainWorld.processManager.RequestMainProcessSwitch(ProcessManager.ProcessID.IntroRoll);
	}

	public void GenerateLove()
	{
		_ = Application.dataPath;
		int[] array = new int[6] { 1024, 294264, 335038, 629270, 653667, 1440145 };
		string text = Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "mods" + Path.DirectorySeparatorChar + "moreslugcats" + Path.DirectorySeparatorChar + "Atlases").ToLowerInvariant();
		string text2 = Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "mods" + Path.DirectorySeparatorChar + "moreslugcats" + Path.DirectorySeparatorChar + "LoadedSoundEffects").ToLowerInvariant();
		string path = AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + "Love");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		if (!Directory.Exists(text2))
		{
			Directory.CreateDirectory(text2);
		}
		byte[] array2 = File.ReadAllBytes(path);
		byte[] array3 = new byte[array[1] - array[0]];
		Array.Copy(array2, array[0], array3, 0, array3.Length);
		File.WriteAllBytes(text + Path.DirectorySeparatorChar + "love.png", array3);
		byte[] array4 = new byte[array[2] - array[1]];
		Array.Copy(array2, array[1], array4, 0, array4.Length);
		File.WriteAllBytes(text + Path.DirectorySeparatorChar + "love.txt", array4);
		byte[] array5 = new byte[array[3] - array[2]];
		Array.Copy(array2, array[2], array5, 0, array5.Length);
		File.WriteAllBytes(text + Path.DirectorySeparatorChar + "loved.png", array5);
		byte[] array6 = new byte[array[4] - array[3]];
		Array.Copy(array2, array[3], array6, 0, array6.Length);
		File.WriteAllBytes(text + Path.DirectorySeparatorChar + "loved.txt", array6);
		byte[] array7 = new byte[array[5] - array[4]];
		Array.Copy(array2, array[4], array7, 0, array7.Length);
		File.WriteAllBytes(text2 + Path.DirectorySeparatorChar + "love.wav", array7);
		byte[] array8 = new byte[array2.Length - array[5]];
		Array.Copy(array2, array[5], array8, 0, array8.Length);
		File.WriteAllBytes(text2 + Path.DirectorySeparatorChar + "loveover.wav", array8);
	}

	public static void CleanAtlas()
	{
		string text = Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "mods" + Path.DirectorySeparatorChar + "moreslugcats" + Path.DirectorySeparatorChar + "Atlases").ToLowerInvariant();
		string path = text + Path.DirectorySeparatorChar + "love.png";
		string path2 = text + Path.DirectorySeparatorChar + "love.txt";
		string path3 = text + Path.DirectorySeparatorChar + "loved.png";
		string path4 = text + Path.DirectorySeparatorChar + "loved.txt";
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		if (File.Exists(path2))
		{
			File.Delete(path2);
		}
		if (File.Exists(path3))
		{
			File.Delete(path3);
		}
		if (File.Exists(path4))
		{
			File.Delete(path4);
		}
	}

	public static void CleanSounds()
	{
		string text = Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "mods" + Path.DirectorySeparatorChar + "moreslugcats" + Path.DirectorySeparatorChar + "LoadedSoundEffects").ToLowerInvariant();
		string path = text + Path.DirectorySeparatorChar + "love.wav";
		string path2 = text + Path.DirectorySeparatorChar + "loveover.wav";
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		if (File.Exists(path2))
		{
			File.Delete(path2);
		}
	}

	public Love(Vector2 pos, AtlasAnimator animator)
	{
		this.pos = pos;
		this.animator = animator;
	}

	public void InitiateReboot()
	{
		if (timeUntilReboot == 0 && animator != null)
		{
			timeUntilReboot = 120;
			room.PlayCustomSoundDisembodied("loveover", 0f, 1f, 1.2f);
			if (rebootTrans == null)
			{
				rebootTrans = new RebootTransition(pos);
				room.AddObject(rebootTrans);
			}
		}
	}

	public void Activate()
	{
		if (!activated && mesh != null)
		{
			activated = true;
			mesh.rotation = (float)Math.PI;
		}
	}
}
