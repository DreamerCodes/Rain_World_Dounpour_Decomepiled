using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class CollectToken : UpdatableAndDeletable, IDrawable
{
	public class CollectTokenData : PlacedObject.ResizableObjectData
	{
		public Vector2 panelPos;

		public bool isBlue;

		public string tokenString;

		public List<SlugcatStats.Name> availableToPlayers;

		public bool isGreen;

		public bool isWhite;

		public bool isRed;

		public bool isDev;

		public MultiplayerUnlocks.LevelUnlockID LevelUnlock
		{
			get
			{
				if (isBlue || isWhite || isGreen || tokenString == null || tokenString.Length < 1)
				{
					return null;
				}
				try
				{
					return new MultiplayerUnlocks.LevelUnlockID(tokenString);
				}
				catch
				{
					return null;
				}
			}
			set
			{
				if (!isBlue && !isWhite && !isGreen)
				{
					if (value != null)
					{
						tokenString = value.value;
					}
					else
					{
						tokenString = "";
					}
				}
			}
		}

		public MultiplayerUnlocks.SandboxUnlockID SandboxUnlock
		{
			get
			{
				if (!isBlue || isGreen || isWhite || tokenString == null || tokenString.Length < 1)
				{
					return null;
				}
				try
				{
					return new MultiplayerUnlocks.SandboxUnlockID(tokenString);
				}
				catch
				{
					return null;
				}
			}
			set
			{
				if (isBlue && !isGreen && !isWhite)
				{
					if (value != null)
					{
						tokenString = value.value;
					}
					else
					{
						tokenString = "";
					}
				}
			}
		}

		public ChatlogData.ChatlogID ChatlogCollect
		{
			get
			{
				if (!isWhite || tokenString == null || tokenString.Length < 1)
				{
					return null;
				}
				return new ChatlogData.ChatlogID(tokenString);
			}
			set
			{
				if (isWhite)
				{
					if (value == null)
					{
						tokenString = string.Empty;
					}
					else
					{
						tokenString = value.value;
					}
				}
			}
		}

		public MultiplayerUnlocks.SlugcatUnlockID SlugcatUnlock
		{
			get
			{
				if (!isGreen || tokenString == null || tokenString.Length < 1)
				{
					return null;
				}
				return new MultiplayerUnlocks.SlugcatUnlockID(tokenString);
			}
			set
			{
				if (isGreen)
				{
					if (value == null)
					{
						tokenString = string.Empty;
					}
					else
					{
						tokenString = value.value;
					}
				}
			}
		}

		public MultiplayerUnlocks.SafariUnlockID SafariUnlock
		{
			get
			{
				if (!isRed || tokenString == null || tokenString.Length < 1)
				{
					return null;
				}
				return new MultiplayerUnlocks.SafariUnlockID(tokenString);
			}
			set
			{
				if (isRed)
				{
					if (value == null)
					{
						tokenString = string.Empty;
					}
					else
					{
						tokenString = value.value;
					}
				}
			}
		}

		public CollectTokenData(PlacedObject owner, bool isBlue)
			: base(owner)
		{
			this.isBlue = isBlue;
			availableToPlayers = new List<SlugcatStats.Name>();
			for (int i = 0; i < ExtEnum<SlugcatStats.Name>.values.Count; i++)
			{
				SlugcatStats.Name name = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(i));
				if (!SlugcatStats.HiddenOrUnplayableSlugcat(name))
				{
					availableToPlayers.Add(name);
				}
			}
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			isBlue = array[4] == "1";
			isGreen = ModManager.MSC && array.Length > 7 && array[7] == "1";
			isWhite = ModManager.MSC && array.Length > 8 && array[8] == "1";
			isRed = ModManager.MSC && array.Length > 9 && array[9] == "1";
			isDev = ModManager.MSC && array.Length > 10 && array[10] == "1";
			tokenString = array[5];
			if (Custom.IsDigitString(array[6]))
			{
				BackwardsCompatibilityRemix.ParsePlayerAvailability(array[6], availableToPlayers);
			}
			else
			{
				availableToPlayers.Clear();
				List<string> list = new List<string>(array[6].Split('|'));
				for (int i = 0; i < ExtEnum<SlugcatStats.Name>.values.Count; i++)
				{
					string entry = ExtEnum<SlugcatStats.Name>.values.GetEntry(i);
					SlugcatStats.Name name = new SlugcatStats.Name(entry);
					if (!SlugcatStats.HiddenOrUnplayableSlugcat(name) && !list.Contains(entry))
					{
						availableToPlayers.Add(name);
					}
				}
			}
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 7);
		}

		public override string ToString()
		{
			List<string> list = new List<string>();
			for (int i = 0; i < ExtEnum<SlugcatStats.Name>.values.Count; i++)
			{
				string entry = ExtEnum<SlugcatStats.Name>.values.GetEntry(i);
				SlugcatStats.Name name = new SlugcatStats.Name(entry);
				if (!SlugcatStats.HiddenOrUnplayableSlugcat(name) && !availableToPlayers.Contains(name))
				{
					list.Add(entry);
				}
			}
			string text = string.Join("|", list.ToArray());
			string text2 = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}", handlePos.x, handlePos.y, panelPos.x, panelPos.y, isBlue ? "1" : "0", tokenString, text);
			if (ModManager.MSC)
			{
				text2 += string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}", isGreen ? "1" : "0", isWhite ? "1" : "0", isRed ? "1" : "0", isDev ? "1" : "0");
			}
			text2 = SaveState.SetCustomData(this, text2);
			return SaveUtils.AppendUnrecognizedStringAttrs(text2, "~", unrecognizedAttributes);
		}
	}

	public class TokenStalk : UpdatableAndDeletable, IDrawable
	{
		public Vector2 hoverPos;

		public CollectToken token;

		public Vector2[,] stalk;

		public Vector2 basePos;

		public Vector2 mainDir;

		public float flip;

		public Vector2 armPos;

		public Vector2 lastArmPos;

		public Vector2 armVel;

		public Vector2 armGetToPos;

		public Vector2 head;

		public Vector2 lastHead;

		public Vector2 headVel;

		public Vector2 headDir;

		public Vector2 lastHeadDir;

		private float headDist = 15f;

		public float armLength;

		private Vector2[,] coord;

		private float coordLength;

		private float coordSeg = 3f;

		private float[,] curveLerps;

		private float keepDistance;

		private float sinCounter;

		private float lastSinCounter;

		private float lampPower;

		private float lastLampPower;

		private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

		public Color lampColor;

		public bool forceSatellite;

		public int sataFlasherLight;

		private Color lampOffCol;

		public int BaseSprite => 0;

		public int Arm1Sprite => 1;

		public int Arm2Sprite => 2;

		public int Arm3Sprite => 3;

		public int Arm4Sprite => 4;

		public int Arm5Sprite => 5;

		public int ArmJointSprite => 6;

		public int SocketSprite => 7;

		public int HeadSprite => 8;

		public int LampSprite => 9;

		public int SataFlasher => 10;

		public int TotalSprites => (ModManager.MSC ? 11 : 10) + coord.GetLength(0);

		public float alive
		{
			get
			{
				if (token == null)
				{
					return 0f;
				}
				return 0.25f + 0.75f * token.power;
			}
		}

		public int CoordSprite(int s)
		{
			return (ModManager.MSC ? 11 : 10) + s;
		}

		public TokenStalk(Room room, Vector2 hoverPos, Vector2 basePos, CollectToken token, bool blue)
		{
			this.token = token;
			this.hoverPos = hoverPos;
			this.basePos = basePos;
			if (token != null)
			{
				lampPower = 1f;
				lastLampPower = 1f;
			}
			if (blue)
			{
				lampColor = Color.Lerp(RainWorld.AntiGold.rgb, new Color(1f, 1f, 1f), 0.4f);
			}
			else
			{
				lampColor = Color.Lerp(RainWorld.GoldRGB, new Color(1f, 1f, 1f), 0.5f);
			}
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState((int)(hoverPos.x * 10f) + (int)(hoverPos.y * 10f));
			curveLerps = new float[2, 5];
			for (int i = 0; i < curveLerps.GetLength(0); i++)
			{
				curveLerps[i, 0] = 1f;
				curveLerps[i, 1] = 1f;
			}
			curveLerps[0, 3] = UnityEngine.Random.value * 360f;
			curveLerps[1, 3] = Mathf.Lerp(10f, 20f, UnityEngine.Random.value);
			flip = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
			mainDir = Custom.DirVec(basePos, hoverPos);
			coordLength = Vector2.Distance(basePos, hoverPos) * 0.6f;
			coord = new Vector2[(int)(coordLength / coordSeg), 3];
			armLength = Vector2.Distance(basePos, hoverPos) / 2f;
			armPos = basePos + mainDir * armLength;
			lastArmPos = armPos;
			armGetToPos = armPos;
			for (int j = 0; j < coord.GetLength(0); j++)
			{
				coord[j, 0] = armPos;
				coord[j, 1] = armPos;
			}
			head = hoverPos - mainDir * headDist;
			lastHead = head;
			UnityEngine.Random.state = state;
		}

		public override void Update(bool eu)
		{
			lastArmPos = armPos;
			armPos += armVel;
			armPos = Custom.MoveTowards(armPos, armGetToPos, (0.8f + armLength / 150f) / 2f);
			armVel *= 0.8f;
			armVel += Vector2.ClampMagnitude(armGetToPos - armPos, 4f) / 11f;
			lastHead = head;
			if (ModManager.MSC && token != null && token.placedObj != null && (token.placedObj.data as CollectTokenData).isWhite && (token.placedObj.data as CollectTokenData).ChatlogCollect != null && (token.placedObj.data as CollectTokenData).ChatlogCollect.Index >= ChatlogData.ChatlogID.Chatlog_Broadcast0.Index && (token.placedObj.data as CollectTokenData).ChatlogCollect.Index <= ChatlogData.ChatlogID.Chatlog_Broadcast19.Index)
			{
				sataFlasherLight += 2;
			}
			head += headVel;
			headVel *= 0.8f;
			if (token != null && token.slatedForDeletetion)
			{
				token = null;
			}
			lastLampPower = lampPower;
			lastSinCounter = sinCounter;
			sinCounter += UnityEngine.Random.value * lampPower;
			if (token != null)
			{
				lampPower = Custom.LerpAndTick(lampPower, 1f, 0.02f, 1f / 60f);
			}
			else
			{
				lampPower = Mathf.Max(0f, lampPower - 1f / 120f);
			}
			if (!Custom.DistLess(head, armPos, coordLength))
			{
				headVel -= Custom.DirVec(armPos, head) * (Vector2.Distance(armPos, head) - coordLength) * 0.8f;
				head -= Custom.DirVec(armPos, head) * (Vector2.Distance(armPos, head) - coordLength) * 0.8f;
			}
			headVel += (Vector2)Vector3.Slerp(Custom.DegToVec(GetCurveLerp(0, 0.5f, 1f)), new Vector2(0f, 1f), 0.4f) * 0.4f;
			lastHeadDir = headDir;
			Vector2 vector = hoverPos;
			if (token != null && token.expand == 0f && !token.contract)
			{
				vector = Vector2.Lerp(hoverPos, token.pos, alive);
			}
			headVel -= Custom.DirVec(vector, head) * (Vector2.Distance(vector, head) - headDist) * 0.8f;
			head -= Custom.DirVec(vector, head) * (Vector2.Distance(vector, head) - headDist) * 0.8f;
			headDir = Custom.DirVec(head, vector);
			if (UnityEngine.Random.value < 1f / Mathf.Lerp(300f, 60f, alive))
			{
				Vector2 b = basePos + mainDir * armLength * 0.7f + Custom.RNV() * UnityEngine.Random.value * armLength * Mathf.Lerp(0.1f, 0.3f, alive);
				if (SharedPhysics.RayTraceTilesForTerrain(room, armGetToPos, b))
				{
					armGetToPos = b;
				}
				NewCurveLerp(0, curveLerps[0, 3] + Mathf.Lerp(-180f, 180f, UnityEngine.Random.value), Mathf.Lerp(1f, 2f, alive));
				NewCurveLerp(1, Mathf.Lerp(10f, 20f, Mathf.Pow(UnityEngine.Random.value, 0.75f)), Mathf.Lerp(0.4f, 0.8f, alive));
			}
			headDist = GetCurveLerp(1, 0.5f, 1f);
			if (token != null)
			{
				keepDistance = Custom.LerpAndTick(keepDistance, Mathf.Sin(Mathf.Clamp01(token.glitch) * (float)Math.PI) * alive, 0.006f, alive / ((keepDistance < token.glitch) ? 40f : 80f));
			}
			headDist = Mathf.Lerp(headDist, 50f, Mathf.Pow(keepDistance, 0.5f));
			Vector2 vector2 = Custom.DirVec(Custom.InverseKinematic(basePos, armPos, armLength * 0.65f, armLength * 0.35f, flip), armPos);
			for (int i = 0; i < coord.GetLength(0); i++)
			{
				float num = Mathf.InverseLerp(-1f, coord.GetLength(0), i);
				Vector2 vector3 = Custom.Bezier(armPos, armPos + vector2 * coordLength * 0.5f, head, head - headDir * coordLength * 0.5f, num);
				coord[i, 1] = coord[i, 0];
				coord[i, 0] += coord[i, 2];
				coord[i, 2] *= 0.8f;
				coord[i, 2] += (vector3 - coord[i, 0]) * Mathf.Lerp(0f, 0.25f, Mathf.Sin(num * (float)Math.PI));
				coord[i, 0] += (vector3 - coord[i, 0]) * Mathf.Lerp(0f, 0.25f, Mathf.Sin(num * (float)Math.PI));
				if (i > 2)
				{
					coord[i, 2] += Custom.DirVec(coord[i - 2, 0], coord[i, 0]);
					coord[i - 2, 2] -= Custom.DirVec(coord[i - 2, 0], coord[i, 0]);
				}
				if (i > 3)
				{
					coord[i, 2] += Custom.DirVec(coord[i - 3, 0], coord[i, 0]) * 0.5f;
					coord[i - 3, 2] -= Custom.DirVec(coord[i - 3, 0], coord[i, 0]) * 0.5f;
				}
				if (num < 0.5f)
				{
					coord[i, 2] += vector2 * Mathf.InverseLerp(0.5f, 0f, num) * Mathf.InverseLerp(5f, 0f, i);
				}
				else
				{
					coord[i, 2] -= headDir * Mathf.InverseLerp(0.5f, 1f, num);
				}
			}
			ConnectCoord();
			ConnectCoord();
			for (int j = 0; j < coord.GetLength(0); j++)
			{
				SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(coord[j, 0], coord[j, 1], coord[j, 2], 2f, new IntVector2(0, 0), goThroughFloors: true);
				cd = SharedPhysics.HorizontalCollision(room, cd);
				cd = SharedPhysics.VerticalCollision(room, cd);
				coord[j, 0] = cd.pos;
				coord[j, 2] = cd.vel;
			}
			for (int k = 0; k < curveLerps.GetLength(0); k++)
			{
				curveLerps[k, 1] = curveLerps[k, 0];
				curveLerps[k, 0] = Mathf.Min(1f, curveLerps[k, 0] + curveLerps[k, 4]);
			}
			base.Update(eu);
		}

		private void NewCurveLerp(int curveLerp, float to, float speed)
		{
			if (!(curveLerps[curveLerp, 0] < 1f) && !(curveLerps[curveLerp, 1] < 1f))
			{
				curveLerps[curveLerp, 2] = curveLerps[curveLerp, 3];
				curveLerps[curveLerp, 3] = to;
				curveLerps[curveLerp, 4] = speed / Mathf.Abs(curveLerps[curveLerp, 2] - curveLerps[curveLerp, 3]);
				curveLerps[curveLerp, 0] = 0f;
				curveLerps[curveLerp, 1] = 0f;
			}
		}

		private float GetCurveLerp(int curveLerp, float sCurveK, float timeStacker)
		{
			return Mathf.Lerp(curveLerps[curveLerp, 2], curveLerps[curveLerp, 3], Custom.SCurve(Mathf.Lerp(curveLerps[curveLerp, 1], curveLerps[curveLerp, 0], timeStacker), sCurveK));
		}

		private void ConnectCoord()
		{
			coord[0, 2] -= Custom.DirVec(armPos, coord[0, 0]) * (Vector2.Distance(armPos, coord[0, 0]) - coordSeg);
			coord[0, 0] -= Custom.DirVec(armPos, coord[0, 0]) * (Vector2.Distance(armPos, coord[0, 0]) - coordSeg);
			for (int i = 1; i < coord.GetLength(0); i++)
			{
				if (!Custom.DistLess(coord[i - 1, 0], coord[i, 0], coordSeg))
				{
					Vector2 vector = Custom.DirVec(coord[i, 0], coord[i - 1, 0]) * (Vector2.Distance(coord[i - 1, 0], coord[i, 0]) - coordSeg);
					coord[i, 2] += vector * 0.5f;
					coord[i, 0] += vector * 0.5f;
					coord[i - 1, 2] -= vector * 0.5f;
					coord[i - 1, 0] -= vector * 0.5f;
				}
			}
			coord[coord.GetLength(0) - 1, 2] -= Custom.DirVec(head, coord[coord.GetLength(0) - 1, 0]) * (Vector2.Distance(head, coord[coord.GetLength(0) - 1, 0]) - coordSeg);
			coord[coord.GetLength(0) - 1, 0] -= Custom.DirVec(head, coord[coord.GetLength(0) - 1, 0]) * (Vector2.Distance(head, coord[coord.GetLength(0) - 1, 0]) - coordSeg);
		}

		public Vector2 EyePos(float timeStacker)
		{
			return Vector2.Lerp(lastHead, head, timeStacker) + (Vector2)Vector3.Slerp(lastHeadDir, headDir, timeStacker) * 3f;
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[TotalSprites];
			sLeaser.sprites[BaseSprite] = new FSprite("Circle20");
			sLeaser.sprites[BaseSprite].scaleX = 0.5f;
			sLeaser.sprites[BaseSprite].scaleY = 0.7f;
			sLeaser.sprites[BaseSprite].rotation = Custom.VecToDeg(mainDir);
			sLeaser.sprites[Arm1Sprite] = new FSprite("pixel");
			sLeaser.sprites[Arm1Sprite].scaleX = 4f;
			sLeaser.sprites[Arm1Sprite].anchorY = 0f;
			sLeaser.sprites[Arm2Sprite] = new FSprite("pixel");
			sLeaser.sprites[Arm2Sprite].scaleX = 3f;
			sLeaser.sprites[Arm2Sprite].anchorY = 0f;
			sLeaser.sprites[Arm3Sprite] = new FSprite("pixel");
			sLeaser.sprites[Arm3Sprite].scaleX = 1.5f;
			sLeaser.sprites[Arm3Sprite].scaleY = armLength * 0.6f;
			sLeaser.sprites[Arm3Sprite].anchorY = 0f;
			sLeaser.sprites[Arm4Sprite] = new FSprite("pixel");
			sLeaser.sprites[Arm4Sprite].scaleX = 3f;
			sLeaser.sprites[Arm4Sprite].scaleY = 8f;
			sLeaser.sprites[Arm5Sprite] = new FSprite("pixel");
			sLeaser.sprites[Arm5Sprite].scaleX = 6f;
			sLeaser.sprites[Arm5Sprite].scaleY = 8f;
			sLeaser.sprites[ArmJointSprite] = new FSprite("JetFishEyeA");
			sLeaser.sprites[LampSprite] = new FSprite("tinyStar");
			sLeaser.sprites[SocketSprite] = new FSprite("pixel");
			sLeaser.sprites[SocketSprite].scaleX = 5f;
			sLeaser.sprites[SocketSprite].scaleY = 9f;
			if (token != null && token.whiteToken)
			{
				sLeaser.sprites[HeadSprite] = new FSprite("MiniSatellite");
				sLeaser.sprites[SataFlasher] = new FSprite("Futile_White");
				sLeaser.sprites[SataFlasher].shader = rCam.room.game.rainWorld.Shaders["FlatLight"];
				sLeaser.sprites[SataFlasher].scale = 0.8f;
				sLeaser.sprites[SataFlasher].isVisible = false;
			}
			else
			{
				sLeaser.sprites[HeadSprite] = new FSprite("pixel");
				sLeaser.sprites[HeadSprite].scaleX = 4f;
				sLeaser.sprites[HeadSprite].scaleY = 6f;
				if (ModManager.MSC)
				{
					sLeaser.sprites[SataFlasher] = new FSprite("pixel");
					sLeaser.sprites[SataFlasher].isVisible = false;
				}
			}
			for (int i = 0; i < coord.GetLength(0); i++)
			{
				sLeaser.sprites[CoordSprite(i)] = new FSprite("pixel");
				sLeaser.sprites[CoordSprite(i)].scaleX = ((i % 2 == 0) ? 2f : 3f);
				sLeaser.sprites[CoordSprite(i)].scaleY = 5f;
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[BaseSprite].x = basePos.x - camPos.x;
			sLeaser.sprites[BaseSprite].y = basePos.y - camPos.y;
			Vector2 vector = Vector2.Lerp(lastHead, head, timeStacker);
			Vector2 v = Vector3.Slerp(lastHeadDir, headDir, timeStacker);
			Vector2 vector2 = Vector2.Lerp(lastArmPos, armPos, timeStacker);
			Vector2 vector3 = Custom.InverseKinematic(basePos, vector2, armLength * 0.65f, armLength * 0.35f, flip);
			sLeaser.sprites[Arm1Sprite].x = basePos.x - camPos.x;
			sLeaser.sprites[Arm1Sprite].y = basePos.y - camPos.y;
			sLeaser.sprites[Arm1Sprite].scaleY = Vector2.Distance(basePos, vector3);
			sLeaser.sprites[Arm1Sprite].rotation = Custom.AimFromOneVectorToAnother(basePos, vector3);
			sLeaser.sprites[Arm2Sprite].x = vector3.x - camPos.x;
			sLeaser.sprites[Arm2Sprite].y = vector3.y - camPos.y;
			sLeaser.sprites[Arm2Sprite].scaleY = Vector2.Distance(vector3, vector2);
			sLeaser.sprites[Arm2Sprite].rotation = Custom.AimFromOneVectorToAnother(vector3, vector2);
			sLeaser.sprites[SocketSprite].x = vector2.x - camPos.x;
			sLeaser.sprites[SocketSprite].y = vector2.y - camPos.y;
			sLeaser.sprites[SocketSprite].rotation = Custom.VecToDeg(Vector3.Slerp(Custom.DirVec(vector3, vector2), Custom.DirVec(vector2, Vector2.Lerp(coord[0, 1], coord[0, 0], timeStacker)), 0.4f));
			Vector2 p = Vector2.Lerp(basePos, vector3, 0.3f);
			Vector2 p2 = Vector2.Lerp(vector3, vector2, 0.4f);
			sLeaser.sprites[Arm3Sprite].x = p.x - camPos.x;
			sLeaser.sprites[Arm3Sprite].y = p.y - camPos.y;
			sLeaser.sprites[Arm3Sprite].rotation = Custom.AimFromOneVectorToAnother(p, p2);
			sLeaser.sprites[Arm4Sprite].x = p2.x - camPos.x;
			sLeaser.sprites[Arm4Sprite].y = p2.y - camPos.y;
			sLeaser.sprites[Arm4Sprite].rotation = Custom.AimFromOneVectorToAnother(p, p2);
			p += Custom.DirVec(basePos, vector3) * (armLength * 0.1f + 2f);
			sLeaser.sprites[Arm5Sprite].x = p.x - camPos.x;
			sLeaser.sprites[Arm5Sprite].y = p.y - camPos.y;
			sLeaser.sprites[Arm5Sprite].rotation = Custom.AimFromOneVectorToAnother(basePos, vector3);
			sLeaser.sprites[LampSprite].x = p.x - camPos.x;
			sLeaser.sprites[LampSprite].y = p.y - camPos.y;
			sLeaser.sprites[LampSprite].color = Color.Lerp(lampOffCol, lampColor, Mathf.Lerp(lastLampPower, lampPower, timeStacker) * Mathf.Pow(UnityEngine.Random.value, 0.5f) * (0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(lastSinCounter, sinCounter, timeStacker) / 6f)));
			sLeaser.sprites[ArmJointSprite].x = vector3.x - camPos.x;
			sLeaser.sprites[ArmJointSprite].y = vector3.y - camPos.y;
			sLeaser.sprites[HeadSprite].x = vector.x - camPos.x;
			sLeaser.sprites[HeadSprite].y = vector.y - camPos.y;
			if (ModManager.MSC && forceSatellite && sLeaser.sprites[HeadSprite].element.name != "MiniSatellite")
			{
				sLeaser.sprites[HeadSprite].SetElementByName("MiniSatellite");
				sLeaser.sprites[HeadSprite].scaleX = 1f;
				sLeaser.sprites[HeadSprite].scaleY = 1f;
			}
			if (ModManager.MSC && ((token != null && token.whiteToken) || forceSatellite))
			{
				sLeaser.sprites[HeadSprite].rotation = Custom.VecToDeg(v) - 90f;
				if (sataFlasherLight >= 99)
				{
					sLeaser.sprites[SataFlasher].isVisible = !sLeaser.sprites[SataFlasher].isVisible;
					sataFlasherLight = 0;
				}
				sLeaser.sprites[SataFlasher].color = Color.Lerp(Color.white, lampOffCol, UnityEngine.Random.value * 0.1f);
				sLeaser.sprites[SataFlasher].alpha = 0.9f + UnityEngine.Random.value * 0.09f;
				sLeaser.sprites[SataFlasher].x = vector.x + v.x * 5f - camPos.x;
				sLeaser.sprites[SataFlasher].y = vector.y + v.y * 5f - camPos.y;
			}
			else
			{
				sLeaser.sprites[HeadSprite].rotation = Custom.VecToDeg(v);
				if (ModManager.MSC)
				{
					sLeaser.sprites[SataFlasher].isVisible = false;
				}
			}
			Vector2 p3 = vector2;
			for (int i = 0; i < coord.GetLength(0); i++)
			{
				Vector2 vector4 = Vector2.Lerp(coord[i, 1], coord[i, 0], timeStacker);
				sLeaser.sprites[CoordSprite(i)].x = vector4.x - camPos.x;
				sLeaser.sprites[CoordSprite(i)].y = vector4.y - camPos.y;
				sLeaser.sprites[CoordSprite(i)].rotation = Custom.AimFromOneVectorToAnother(p3, vector4);
				p3 = vector4;
			}
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].color = palette.blackColor;
			}
			lampOffCol = Color.Lerp(palette.blackColor, new Color(1f, 1f, 1f), 0.15f);
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Midground");
			}
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].RemoveFromContainer();
				if (ModManager.MSC && i == SataFlasher)
				{
					rCam.ReturnFContainer("ForegroundLights").AddChild(sLeaser.sprites[SataFlasher]);
				}
				else
				{
					newContatiner.AddChild(sLeaser.sprites[i]);
				}
			}
		}
	}

	public class TokenSpark : CosmeticSprite
	{
		private float dir;

		private float life;

		private float lifeTime;

		public Color color;

		private Vector2 lastLastPos;

		private bool underWater;

		public TokenSpark(Vector2 pos, Vector2 vel, Color color, bool underWater)
		{
			base.pos = pos;
			base.vel = vel;
			this.color = color;
			this.underWater = underWater;
			lastPos = pos;
			lastLastPos = pos;
			lifeTime = Mathf.Lerp(20f, 40f, UnityEngine.Random.value);
			life = 1f;
			dir = Custom.VecToDeg(vel.normalized);
		}

		public override void Update(bool eu)
		{
			lastLastPos = lastPos;
			base.Update(eu);
			dir += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 50f;
			vel *= 0.8f;
			vel += Custom.DegToVec(dir) * Mathf.Lerp(0.2f, 0.2f, life);
			life -= 1f / lifeTime;
			if (life < 0f)
			{
				Destroy();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("pixel");
			sLeaser.sprites[0].color = color;
			sLeaser.sprites[0].anchorY = 0f;
			if (underWater)
			{
				sLeaser.sprites[0].alpha = 0.5f;
			}
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
			Vector2 vector2 = Vector2.Lerp(lastLastPos, lastPos, timeStacker);
			sLeaser.sprites[0].x = vector.x - camPos.x;
			sLeaser.sprites[0].y = vector.y - camPos.y;
			sLeaser.sprites[0].scaleY = Vector2.Distance(vector, vector2) * Mathf.InverseLerp(0f, 0.5f, life);
			sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
			sLeaser.sprites[0].isVisible = UnityEngine.Random.value < Mathf.InverseLerp(0f, 0.5f, life);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			base.AddToContainer(sLeaser, rCam, newContatiner);
		}
	}

	public Vector2 hoverPos;

	public Vector2 pos;

	public Vector2 lastPos;

	public Vector2 vel;

	public float sinCounter;

	public float sinCounter2;

	public Vector2[] trail;

	private float expand;

	private float lastExpand;

	private bool contract;

	public Vector2[,] lines;

	public bool underWaterMode;

	public Player expandAroundPlayer;

	private float glitch;

	private float lastGlitch;

	private float generalGlitch;

	public PlacedObject placedObj;

	public TokenStalk stalk;

	private bool poweredOn;

	private float power;

	private float lastPower;

	private StaticSoundLoop soundLoop;

	private StaticSoundLoop glitchLoop;

	public bool locked;

	private int lockdownCounter;

	public bool anythingUnlocked;

	public List<MultiplayerUnlocks.SandboxUnlockID> showUnlockSymbols;

	public int LightSprite => 0;

	public int MainSprite => 1;

	public int TrailSprite => 2;

	public int GoldSprite => 7;

	public int TotalSprites => 8;

	public bool blueToken => (placedObj.data as CollectTokenData).isBlue;

	public static HSLColor GreenColor => new HSLColor(0.322222f, 0.65f, 0.53f);

	public bool greenToken
	{
		get
		{
			if (ModManager.MSC)
			{
				return (placedObj.data as CollectTokenData).isGreen;
			}
			return false;
		}
	}

	public static HSLColor WhiteColor => new HSLColor(0f, 0f, 0.53f);

	public bool whiteToken
	{
		get
		{
			if (ModManager.MSC)
			{
				return (placedObj.data as CollectTokenData).isWhite;
			}
			return false;
		}
	}

	public static HSLColor RedColor => new HSLColor(0f, 1f, 0.5f);

	public bool redToken
	{
		get
		{
			if (ModManager.MSC)
			{
				return (placedObj.data as CollectTokenData).isRed;
			}
			return false;
		}
	}

	public static HSLColor DevColor => new HSLColor(0.82f, 1f, 0.47f);

	public bool devToken
	{
		get
		{
			if (ModManager.MSC)
			{
				return (placedObj.data as CollectTokenData).isDev;
			}
			return false;
		}
	}

	public Color TokenColor
	{
		get
		{
			if (redToken)
			{
				return RedColor.rgb;
			}
			if (greenToken)
			{
				return GreenColor.rgb;
			}
			if (whiteToken)
			{
				return WhiteColor.rgb;
			}
			if (!blueToken)
			{
				return RainWorld.GoldRGB;
			}
			if (devToken)
			{
				return DevColor.rgb;
			}
			return RainWorld.AntiGold.rgb;
		}
	}

	public int LineSprite(int line)
	{
		return 3 + line;
	}

	public CollectToken(Room room, PlacedObject placedObj)
	{
		this.placedObj = placedObj;
		base.room = room;
		underWaterMode = room.GetTilePosition(placedObj.pos).y < room.defaultWaterLevel;
		stalk = new TokenStalk(room, placedObj.pos, placedObj.pos + (placedObj.data as CollectTokenData).handlePos, this, blueToken);
		room.AddObject(stalk);
		pos = placedObj.pos;
		hoverPos = pos;
		lastPos = pos;
		lines = new Vector2[4, 4];
		for (int i = 0; i < lines.GetLength(0); i++)
		{
			lines[i, 0] = pos;
			lines[i, 1] = pos;
		}
		lines[0, 2] = new Vector2(-7f, 0f);
		lines[1, 2] = new Vector2(0f, 11f);
		lines[2, 2] = new Vector2(7f, 0f);
		lines[3, 2] = new Vector2(0f, -11f);
		trail = new Vector2[5];
		for (int j = 0; j < trail.Length; j++)
		{
			trail[j] = pos;
		}
		soundLoop = new StaticSoundLoop(SoundID.Token_Idle_LOOP, pos, room, 0f, 1f);
		glitchLoop = new StaticSoundLoop(SoundID.Token_Upset_LOOP, pos, room, 0f, 1f);
	}

	public override void Update(bool eu)
	{
		if ((ModManager.MMF && !AvailableToPlayer()) || (devToken && (!room.game.rainWorld.options.commentary || !room.game.rainWorld.options.DeveloperCommentaryLocalized())))
		{
			stalk.Destroy();
			Destroy();
		}
		sinCounter += UnityEngine.Random.value * power;
		sinCounter2 += (1f + Mathf.Lerp(-10f, 10f, UnityEngine.Random.value) * glitch) * power;
		float f = Mathf.Sin(sinCounter2 / 20f);
		f = Mathf.Pow(Mathf.Abs(f), 0.5f) * Mathf.Sign(f);
		soundLoop.Update();
		soundLoop.pos = pos;
		soundLoop.pitch = 1f + 0.25f * f * glitch;
		soundLoop.volume = Mathf.Pow(power, 0.5f) * Mathf.Pow(1f - glitch, 0.5f);
		glitchLoop.Update();
		glitchLoop.pos = pos;
		glitchLoop.pitch = Mathf.Lerp(0.75f, 1.25f, glitch) - 0.25f * f * glitch;
		glitchLoop.volume = Mathf.Pow(Mathf.Sin(Mathf.Clamp(glitch, 0f, 1f) * (float)Math.PI), 0.1f) * Mathf.Pow(power, 0.1f);
		lastPos = pos;
		for (int i = 0; i < lines.GetLength(0); i++)
		{
			lines[i, 1] = lines[i, 0];
		}
		lastGlitch = glitch;
		lastExpand = expand;
		for (int num = trail.Length - 1; num >= 1; num--)
		{
			trail[num] = trail[num - 1];
		}
		trail[0] = lastPos;
		lastPower = power;
		power = Custom.LerpAndTick(power, poweredOn ? 1f : 0f, 0.07f, 0.025f);
		glitch = Mathf.Max(glitch, 1f - power);
		pos += vel;
		for (int j = 0; j < lines.GetLength(0); j++)
		{
			if (stalk != null)
			{
				lines[j, 0] += stalk.head - stalk.lastHead;
			}
			if (Mathf.Pow(UnityEngine.Random.value, 0.1f + glitch * 5f) > lines[j, 3].x)
			{
				lines[j, 0] = Vector2.Lerp(lines[j, 0], pos + new Vector2(lines[j, 2].x * f, lines[j, 2].y), Mathf.Pow(UnityEngine.Random.value, 1f + lines[j, 3].x * 17f));
			}
			if (UnityEngine.Random.value < Mathf.Pow(lines[j, 3].x, 0.2f) && UnityEngine.Random.value < Mathf.Pow(glitch, 0.8f - 0.4f * lines[j, 3].x))
			{
				lines[j, 0] += Custom.RNV() * 17f * lines[j, 3].x * power;
				lines[j, 3].y = Mathf.Max(lines[j, 3].y, glitch);
			}
			lines[j, 3].x = Custom.LerpAndTick(lines[j, 3].x, lines[j, 3].y, 0.01f, 1f / 30f);
			lines[j, 3].y = Mathf.Max(0f, lines[j, 3].y - 1f / 70f);
			if (UnityEngine.Random.value < 1f / Mathf.Lerp(210f, 20f, glitch))
			{
				lines[j, 3].y = Mathf.Max(glitch, (UnityEngine.Random.value < 0.5f) ? generalGlitch : UnityEngine.Random.value);
			}
		}
		vel *= 0.995f;
		vel += Vector2.ClampMagnitude(hoverPos + new Vector2(0f, Mathf.Sin(sinCounter / 15f) * 7f) - pos, 15f) / 81f;
		vel += Custom.RNV() * UnityEngine.Random.value * UnityEngine.Random.value * Mathf.Lerp(0.06f, 0.4f, glitch);
		pos += Custom.RNV() * Mathf.Pow(UnityEngine.Random.value, 7f - 6f * generalGlitch) * Mathf.Lerp(0.06f, 1.2f, glitch);
		if (expandAroundPlayer != null)
		{
			expandAroundPlayer.Blink(5);
			if (!contract)
			{
				expand += 1f / 30f;
				if (expand > 1f)
				{
					expand = 1f;
					contract = true;
				}
				generalGlitch = 0f;
				glitch = Custom.LerpAndTick(glitch, expand * 0.5f, 0.07f, 1f / 15f);
				float num2 = Custom.SCurve(Mathf.InverseLerp(0.35f, 0.55f, expand), 0.4f);
				Vector2 b = Vector2.Lerp(expandAroundPlayer.mainBodyChunk.pos + new Vector2(0f, 40f), Vector2.Lerp(expandAroundPlayer.bodyChunks[1].pos, expandAroundPlayer.mainBodyChunk.pos + Custom.DirVec(expandAroundPlayer.bodyChunks[1].pos, expandAroundPlayer.mainBodyChunk.pos) * 10f, 0.65f), expand);
				for (int k = 0; k < lines.GetLength(0); k++)
				{
					Vector2 vector = Vector2.Lerp(lines[k, 2] * (2f + 5f * Mathf.Pow(expand, 0.5f)), Custom.RotateAroundOrigo(lines[k, 2] * (2f + 2f * Mathf.Pow(expand, 0.5f)), Custom.AimFromOneVectorToAnother(expandAroundPlayer.bodyChunks[1].pos, expandAroundPlayer.mainBodyChunk.pos)), num2);
					lines[k, 0] = Vector2.Lerp(lines[k, 0], Vector2.Lerp(pos, b, Mathf.Pow(num2, 2f)) + vector, Mathf.Pow(expand, 0.5f));
					lines[k, 3] *= 1f - expand;
				}
				hoverPos = Vector2.Lerp(hoverPos, b, Mathf.Pow(expand, 2f));
				pos = Vector2.Lerp(pos, b, Mathf.Pow(expand, 2f));
				vel *= 1f - expand;
			}
			else
			{
				generalGlitch *= 1f - expand;
				glitch = 0.15f;
				expand -= 1f / Mathf.Lerp(60f, 2f, expand);
				Vector2 vector2 = Vector2.Lerp(expandAroundPlayer.bodyChunks[1].pos, expandAroundPlayer.mainBodyChunk.pos + Custom.DirVec(expandAroundPlayer.bodyChunks[1].pos, expandAroundPlayer.mainBodyChunk.pos) * 10f, Mathf.Lerp(1f, 0.65f, expand));
				for (int l = 0; l < lines.GetLength(0); l++)
				{
					Vector2 vector3 = Custom.RotateAroundOrigo(Vector2.Lerp((UnityEngine.Random.value > expand) ? lines[l, 2] : lines[UnityEngine.Random.Range(0, 4), 2], lines[UnityEngine.Random.Range(0, 4), 2], UnityEngine.Random.value * (1f - expand)) * (4f * Mathf.Pow(expand, 0.25f)), Custom.AimFromOneVectorToAnother(expandAroundPlayer.bodyChunks[1].pos, expandAroundPlayer.mainBodyChunk.pos)) * Mathf.Lerp(UnityEngine.Random.value, 1f, expand);
					lines[l, 0] = vector2 + vector3;
					lines[l, 3] *= 1f - expand;
				}
				pos = vector2;
				hoverPos = vector2;
				if (expand < 0f)
				{
					Destroy();
					for (int m = 0; (float)m < 20f; m++)
					{
						room.AddObject(new TokenSpark(pos + Custom.RNV() * 2f, Custom.RNV() * 16f * UnityEngine.Random.value, Color.Lerp(TokenColor, new Color(1f, 1f, 1f), (blueToken || greenToken) ? (0.5f + 0.5f * UnityEngine.Random.value) : UnityEngine.Random.value), underWaterMode));
					}
					room.PlaySound(SoundID.Token_Collected_Sparks, pos);
					if (anythingUnlocked && room.game.cameras[0].hud != null && room.game.cameras[0].hud.textPrompt != null)
					{
						if (greenToken)
						{
							room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("Arena character unlocked:") + " " + room.game.manager.rainWorld.inGameTranslator.Translate((placedObj.data as CollectTokenData).SlugcatUnlock.ToString()), 20, 160, darken: true, hideHud: true);
						}
						else if (redToken)
						{
							room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("Safari region unlocked"), 20, 160, darken: true, hideHud: true);
						}
						else if (blueToken)
						{
							room.game.cameras[0].hud.textPrompt.AddMessage((showUnlockSymbols.Count > 1) ? room.game.manager.rainWorld.inGameTranslator.Translate("Sandbox items unlocked:") : room.game.manager.rainWorld.inGameTranslator.Translate("Sandbox item unlocked:"), 20, 160, darken: true, hideHud: true, 285f, showUnlockSymbols);
						}
						else
						{
							room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("New arenas unlocked"), 20, 160, darken: true, hideHud: true);
						}
					}
				}
			}
		}
		else
		{
			generalGlitch = Mathf.Max(0f, generalGlitch - 1f / 120f);
			if (UnityEngine.Random.value < 0.0027027028f)
			{
				generalGlitch = UnityEngine.Random.value;
			}
			if (!Custom.DistLess(pos, hoverPos, 11f))
			{
				pos += Custom.DirVec(hoverPos, pos) * (11f - Vector2.Distance(pos, hoverPos)) * 0.7f;
			}
			float f2 = Mathf.Sin(Mathf.Clamp(glitch, 0f, 1f) * (float)Math.PI);
			if (UnityEngine.Random.value < 0.05f + 0.35f * Mathf.Pow(f2, 0.5f) && UnityEngine.Random.value < power)
			{
				room.AddObject(new TokenSpark(pos + Custom.RNV() * 6f * glitch, Custom.RNV() * Mathf.Lerp(2f, 9f, Mathf.Pow(f2, 2f)) * UnityEngine.Random.value, GoldCol(glitch), underWaterMode));
			}
			glitch = Custom.LerpAndTick(glitch, generalGlitch / 2f, 0.01f, 1f / 30f);
			if (UnityEngine.Random.value < 1f / Mathf.Lerp(360f, 10f, generalGlitch))
			{
				glitch = Mathf.Pow(UnityEngine.Random.value, 1f - 0.85f * generalGlitch);
			}
			float num3 = float.MaxValue;
			bool flag = AvailableToPlayer();
			if (RainWorld.lockGameTimer)
			{
				flag = false;
			}
			float num4 = 140f;
			if (devToken)
			{
				num4 = 2000f;
			}
			for (int n = 0; n < room.game.session.Players.Count; n++)
			{
				if (room.game.session.Players[n].realizedCreature == null || !room.game.session.Players[n].realizedCreature.Consious || (room.game.session.Players[n].realizedCreature as Player).dangerGrasp != null || room.game.session.Players[n].realizedCreature.room != room)
				{
					continue;
				}
				num3 = Mathf.Min(num3, Vector2.Distance(room.game.session.Players[n].realizedCreature.mainBodyChunk.pos, pos));
				if (!flag)
				{
					continue;
				}
				if (Custom.DistLess(room.game.session.Players[n].realizedCreature.mainBodyChunk.pos, pos, 18f))
				{
					Pop(room.game.session.Players[n].realizedCreature as Player);
					break;
				}
				if (Custom.DistLess(room.game.session.Players[n].realizedCreature.mainBodyChunk.pos, pos, num4))
				{
					if (Custom.DistLess(pos, hoverPos, 80f))
					{
						pos += Custom.DirVec(pos, room.game.session.Players[n].realizedCreature.mainBodyChunk.pos) * Custom.LerpMap(Vector2.Distance(pos, room.game.session.Players[n].realizedCreature.mainBodyChunk.pos), 40f, num4, 2.2f, 0f, 0.5f) * UnityEngine.Random.value;
					}
					if (UnityEngine.Random.value < 0.05f && UnityEngine.Random.value < Mathf.InverseLerp(num4, 40f, Vector2.Distance(pos, room.game.session.Players[n].realizedCreature.mainBodyChunk.pos)))
					{
						glitch = Mathf.Max(glitch, UnityEngine.Random.value * 0.5f);
					}
				}
			}
			if (!flag && poweredOn)
			{
				lockdownCounter++;
				if (UnityEngine.Random.value < 1f / 60f || num3 < num4 - 40f || lockdownCounter > 30)
				{
					locked = true;
				}
				if (UnityEngine.Random.value < 1f / 7f)
				{
					glitch = Mathf.Max(glitch, UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value);
				}
			}
			if (poweredOn && (locked || (expand == 0f && !contract && UnityEngine.Random.value < Mathf.InverseLerp(num4 + 160f, num4 + 460f, num3))))
			{
				poweredOn = false;
				room.PlaySound(SoundID.Token_Turn_Off, pos);
			}
			else if (!poweredOn && !locked && UnityEngine.Random.value < Mathf.InverseLerp(num4 + 60f, num4 - 20f, num3))
			{
				poweredOn = true;
				room.PlaySound(SoundID.Token_Turn_On, pos);
			}
		}
		base.Update(eu);
	}

	private bool AvailableToPlayer()
	{
		if (room.game.StoryCharacter == null)
		{
			return false;
		}
		if ((placedObj.data as CollectTokenData).availableToPlayers.Contains(room.game.StoryCharacter))
		{
			if (devToken)
			{
				if (devToken && room.game.rainWorld.options.commentary)
				{
					return room.game.rainWorld.options.DeveloperCommentaryLocalized();
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public void Pop(Player player)
	{
		if (expand > 0f)
		{
			return;
		}
		expandAroundPlayer = player;
		expand = 0.01f;
		room.PlaySound(SoundID.Token_Collect, pos);
		if (ModManager.MSC && (placedObj.data as CollectTokenData).isDev)
		{
			player.InitChatLog(ChatlogData.ChatlogID.DevCommentaryNode);
		}
		else if (ModManager.MSC && (placedObj.data as CollectTokenData).isWhite)
		{
			if ((placedObj.data as CollectTokenData).ChatlogCollect != null)
			{
				player.InitChatLog((placedObj.data as CollectTokenData).ChatlogCollect);
			}
		}
		else if (ModManager.MSC && (placedObj.data as CollectTokenData).isRed && (placedObj.data as CollectTokenData).SafariUnlock != null)
		{
			anythingUnlocked = room.game.rainWorld.progression.miscProgressionData.SetTokenCollected((placedObj.data as CollectTokenData).SafariUnlock);
			if (!room.game.rainWorld.progression.miscProgressionData.regionsVisited.ContainsKey(room.world.name))
			{
				room.game.rainWorld.progression.miscProgressionData.regionsVisited[room.world.name] = new List<string>();
			}
			if (!room.game.rainWorld.progression.miscProgressionData.regionsVisited[room.world.name].Contains(room.game.GetStorySession.saveStateNumber.value))
			{
				room.game.rainWorld.progression.miscProgressionData.regionsVisited[room.world.name].Add(room.game.GetStorySession.saveStateNumber.value);
			}
		}
		else if (ModManager.MSC && (placedObj.data as CollectTokenData).isGreen && (placedObj.data as CollectTokenData).SlugcatUnlock != null)
		{
			anythingUnlocked = room.game.rainWorld.progression.miscProgressionData.SetTokenCollected((placedObj.data as CollectTokenData).SlugcatUnlock);
		}
		else if ((placedObj.data as CollectTokenData).isBlue && (placedObj.data as CollectTokenData).SandboxUnlock != null)
		{
			anythingUnlocked = room.game.rainWorld.progression.miscProgressionData.SetTokenCollected((placedObj.data as CollectTokenData).SandboxUnlock);
			showUnlockSymbols = MultiplayerUnlocks.TiedSandboxIDs((placedObj.data as CollectTokenData).SandboxUnlock, includeParent: true);
		}
		else if (!(placedObj.data as CollectTokenData).isBlue && (placedObj.data as CollectTokenData).LevelUnlock != null)
		{
			anythingUnlocked = room.game.rainWorld.progression.miscProgressionData.SetTokenCollected((placedObj.data as CollectTokenData).LevelUnlock);
		}
		for (int i = 0; (float)i < 10f; i++)
		{
			room.AddObject(new TokenSpark(pos + Custom.RNV() * 2f, Custom.RNV() * 11f * UnityEngine.Random.value + Custom.DirVec(player.mainBodyChunk.pos, pos) * 5f * UnityEngine.Random.value, GoldCol(glitch), underWaterMode));
		}
	}

	public Color GoldCol(float g)
	{
		if (blueToken || greenToken || redToken || devToken)
		{
			return Color.Lerp(TokenColor, new Color(1f, 1f, 1f), 0.4f + 0.4f * Mathf.Max(contract ? 0.5f : (expand * 0.5f), Mathf.Pow(g, 0.5f)));
		}
		return Color.Lerp(TokenColor, new Color(1f, 1f, 1f), Mathf.Pow(Mathf.InverseLerp(0.5f, 1f, g), 0.5f));
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[LightSprite] = new FSprite("Futile_White");
		sLeaser.sprites[LightSprite].shader = rCam.game.rainWorld.Shaders[underWaterMode ? "UnderWaterLight" : "FlatLight"];
		sLeaser.sprites[GoldSprite] = new FSprite("Futile_White");
		if (blueToken || greenToken || whiteToken || redToken || devToken)
		{
			sLeaser.sprites[GoldSprite].color = Color.Lerp(new Color(0f, 0f, 0f), RainWorld.GoldRGB, 0.2f);
			sLeaser.sprites[GoldSprite].shader = rCam.game.rainWorld.Shaders["FlatLight"];
		}
		else
		{
			sLeaser.sprites[GoldSprite].shader = rCam.game.rainWorld.Shaders["GoldenGlow"];
		}
		sLeaser.sprites[MainSprite] = new FSprite("JetFishEyeA");
		sLeaser.sprites[MainSprite].shader = rCam.game.rainWorld.Shaders["Hologram"];
		sLeaser.sprites[TrailSprite] = new FSprite("JetFishEyeA");
		sLeaser.sprites[TrailSprite].shader = rCam.game.rainWorld.Shaders["Hologram"];
		for (int i = 0; i < 4; i++)
		{
			sLeaser.sprites[LineSprite(i)] = new FSprite("pixel");
			sLeaser.sprites[LineSprite(i)].anchorY = 0f;
			sLeaser.sprites[LineSprite(i)].shader = rCam.game.rainWorld.Shaders["Hologram"];
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		float num = Mathf.Lerp(lastGlitch, glitch, timeStacker);
		float num2 = Mathf.Lerp(lastExpand, expand, timeStacker);
		float num3 = Mathf.Lerp(lastPower, power, timeStacker);
		if (room != null && !AvailableToPlayer())
		{
			num = Mathf.Lerp(num, 1f, UnityEngine.Random.value);
			num3 *= 0.3f + 0.7f * UnityEngine.Random.value;
		}
		sLeaser.sprites[GoldSprite].x = vector.x - camPos.x;
		sLeaser.sprites[GoldSprite].y = vector.y - camPos.y;
		if (blueToken || greenToken || redToken)
		{
			sLeaser.sprites[GoldSprite].alpha = 0.75f * Mathf.Lerp(Mathf.Lerp(0.8f, 0.5f, Mathf.Pow(num, 0.6f + 0.2f * UnityEngine.Random.value)), 0.7f, num2) * num3;
		}
		else
		{
			sLeaser.sprites[GoldSprite].alpha = Mathf.Lerp(Mathf.Lerp(0.8f, 0.5f, Mathf.Pow(num, 0.6f + 0.2f * UnityEngine.Random.value)), 0.7f, num2) * num3;
		}
		sLeaser.sprites[GoldSprite].scale = Mathf.Lerp(blueToken ? 110f : 100f, 300f, num2) / 16f;
		Color color = GoldCol(num);
		sLeaser.sprites[MainSprite].color = color;
		sLeaser.sprites[MainSprite].x = vector.x - camPos.x;
		sLeaser.sprites[MainSprite].y = vector.y - camPos.y;
		sLeaser.sprites[MainSprite].alpha = (1f - num) * Mathf.InverseLerp(0.5f, 0f, num2) * num3 * (underWaterMode ? 0.5f : 1f);
		sLeaser.sprites[MainSprite].isVisible = !contract && num3 > 0f;
		sLeaser.sprites[TrailSprite].color = color;
		sLeaser.sprites[TrailSprite].x = Mathf.Lerp(trail[trail.Length - 1].x, trail[trail.Length - 2].x, timeStacker) - camPos.x;
		sLeaser.sprites[TrailSprite].y = Mathf.Lerp(trail[trail.Length - 1].y, trail[trail.Length - 2].y, timeStacker) - camPos.y;
		sLeaser.sprites[TrailSprite].alpha = 0.75f * (1f - num) * Mathf.InverseLerp(0.5f, 0f, num2) * num3 * (underWaterMode ? 0.5f : 1f);
		sLeaser.sprites[TrailSprite].isVisible = !contract && num3 > 0f;
		sLeaser.sprites[TrailSprite].scaleX = ((UnityEngine.Random.value < num) ? (1f + 20f * UnityEngine.Random.value * glitch) : 1f);
		sLeaser.sprites[TrailSprite].scaleY = ((UnityEngine.Random.value < num) ? (1f + 2f * UnityEngine.Random.value * UnityEngine.Random.value * glitch) : 1f);
		sLeaser.sprites[LightSprite].x = vector.x - camPos.x;
		sLeaser.sprites[LightSprite].y = vector.y - camPos.y;
		if (underWaterMode)
		{
			sLeaser.sprites[LightSprite].alpha = Mathf.Pow(0.9f * (1f - num) * Mathf.InverseLerp(0.5f, 0f, num2) * num3, 0.5f);
			sLeaser.sprites[LightSprite].scale = Mathf.Lerp(60f, 120f, num) / 16f;
		}
		else
		{
			sLeaser.sprites[LightSprite].alpha = 0.9f * (1f - num) * Mathf.InverseLerp(0.5f, 0f, num2) * num3;
			sLeaser.sprites[LightSprite].scale = Mathf.Lerp(20f, 40f, num) / 16f;
		}
		if (blueToken || greenToken || whiteToken || redToken || devToken)
		{
			sLeaser.sprites[LightSprite].color = Color.Lerp(TokenColor, color, 0.4f);
		}
		else
		{
			sLeaser.sprites[LightSprite].color = color;
		}
		sLeaser.sprites[LightSprite].isVisible = !contract && num3 > 0f;
		for (int i = 0; i < 4; i++)
		{
			Vector2 vector2 = Vector2.Lerp(lines[i, 1], lines[i, 0], timeStacker);
			int num4 = ((i != 3) ? (i + 1) : 0);
			Vector2 vector3 = Vector2.Lerp(lines[num4, 1], lines[num4, 0], timeStacker);
			float f = 1f - (1f - Mathf.Max(lines[i, 3].x, lines[num4, 3].x)) * (1f - num);
			f = Mathf.Pow(f, 2f);
			f *= 1f - num2;
			if (UnityEngine.Random.value < f)
			{
				vector3 = Vector2.Lerp(vector2, vector3, UnityEngine.Random.value);
				if (stalk != null)
				{
					vector2 = stalk.EyePos(timeStacker);
				}
				if (expandAroundPlayer != null && (UnityEngine.Random.value < expand || contract))
				{
					vector2 = Vector2.Lerp(expandAroundPlayer.mainBodyChunk.lastPos, expandAroundPlayer.mainBodyChunk.pos, timeStacker);
				}
			}
			sLeaser.sprites[LineSprite(i)].x = vector2.x - camPos.x;
			sLeaser.sprites[LineSprite(i)].y = vector2.y - camPos.y;
			sLeaser.sprites[LineSprite(i)].scaleY = Vector2.Distance(vector2, vector3);
			sLeaser.sprites[LineSprite(i)].rotation = Custom.AimFromOneVectorToAnother(vector2, vector3);
			sLeaser.sprites[LineSprite(i)].alpha = (1f - f) * num3 * (underWaterMode ? 0.2f : 1f);
			sLeaser.sprites[LineSprite(i)].color = color;
			sLeaser.sprites[LineSprite(i)].isVisible = num3 > 0f;
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Water");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
		}
		if (blueToken || greenToken || whiteToken || redToken || devToken)
		{
			newContatiner.AddChild(sLeaser.sprites[GoldSprite]);
		}
		for (int j = 0; j < GoldSprite; j++)
		{
			bool flag = false;
			if (ModManager.MMF)
			{
				for (int k = 0; k < 4; k++)
				{
					if (j == LineSprite(k))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				newContatiner.AddChild(sLeaser.sprites[j]);
			}
		}
		if (ModManager.MMF)
		{
			for (int l = 0; l < 4; l++)
			{
				rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[LineSprite(l)]);
			}
		}
		if (!blueToken && !greenToken && !whiteToken && !redToken && !devToken)
		{
			rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[GoldSprite]);
		}
	}
}
