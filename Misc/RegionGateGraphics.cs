using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using Smoke;
using UnityEngine;

public class RegionGateGraphics
{
	public class DoorGraphic
	{
		public RegionGateGraphics rgGraphics;

		public RegionGate.Door door;

		public bool flip;

		private float lastClosedFac;

		private float lastPC;

		private float PC;

		private float locksLocked;

		private bool[] boltsBolted;

		public Vector2 posZ;

		public float[,] tracks;

		public float[,] poles;

		public Clamp[,] clamps;

		public float[,,] blocks;

		public float[,,] arms;

		private float[,] pansarLocks;

		public float[,] bigScrews;

		public int clampsStatus;

		public int wantedClampStatus;

		public StaticSoundLoop rustleLoop;

		public StaticSoundLoop screwTurnLoop;

		public int TotalSprites => 67;

		public bool Bubble
		{
			get
			{
				if (Closed > 0.1f)
				{
					return Closed < 0.8f;
				}
				return false;
			}
		}

		private float Closed => door.closedFac;

		private bool PolesClosed => Closed != 0f;

		private float TracksClosed => Mathf.InverseLerp(0f, 0.2f, Closed);

		private float ClampsPause => 0.2f;

		private float BlocksClosed => Mathf.InverseLerp(0.2f, 0.5f, Closed);

		private float ArmsWithdrawn => Mathf.InverseLerp(0.52f, 0.73f, Closed);

		private float PansarClosed => Mathf.InverseLerp(0.55f, 0.75f, Closed);

		private float LocksClosed => Mathf.InverseLerp(0.78f, 0.9f, Closed);

		private float GearsTurned => Mathf.InverseLerp(0.2f, 0.9f, Closed);

		private int CogSprite(int vert, int side, int cog)
		{
			return door.number * TotalSprites + vert * 4 + side * 2 + (1 - cog);
		}

		private int BehindPansarSprite(int side)
		{
			return door.number * TotalSprites + 8 + side;
		}

		private int PoleSprite(int pole)
		{
			return door.number * TotalSprites + 10 + pole;
		}

		private int TrackSprite(int side, int vertical)
		{
			return door.number * TotalSprites + 14 + (vertical + side + side);
		}

		private int CenterTrackSprite(int vertical)
		{
			return door.number * TotalSprites + 18 + vertical;
		}

		private int ClampSprite(int side, int clamp)
		{
			return door.number * TotalSprites + 20 + clamp * 2 + side;
		}

		private int BlockSprite(int side, int block)
		{
			return door.number * TotalSprites + 38 + block + side + side;
		}

		private int HandSprite(int side, int block)
		{
			return door.number * TotalSprites + 42 + block + side + side;
		}

		private int ArmSprite(int side, int block)
		{
			return door.number * TotalSprites + 46 + block + side + side;
		}

		private int BoltSprite(int bolt)
		{
			return door.number * TotalSprites + 50 + bolt;
		}

		private int PansarSprite(int side)
		{
			return door.number * TotalSprites + 54 + side;
		}

		private int PansarSegmentSprite(int segment)
		{
			return door.number * TotalSprites + 56 + segment / 2 + ((segment % 2 != 0) ? 5 : 0);
		}

		private int BigScrewSprite(int vertical)
		{
			return door.number * TotalSprites + 65 + vertical;
		}

		public DoorGraphic(RegionGateGraphics rgGraphics, RegionGate.Door door)
		{
			this.rgGraphics = rgGraphics;
			this.door = door;
			lastClosedFac = door.closedFac;
			posZ = new Vector2((15f + 9f * (float)door.number) * 20f, 340f);
			clampsStatus = 0;
			wantedClampStatus = -1 + 2 * (int)door.closedFac;
			tracks = new float[3, 3];
			poles = new float[4, 3];
			clamps = new Clamp[2, 9];
			blocks = new float[2, 2, 3];
			arms = new float[2, 2, 3];
			pansarLocks = new float[9, 4];
			boltsBolted = new bool[4];
			bigScrews = new float[2, 2];
			locksLocked = door.closedFac;
			for (int i = 0; i < 9; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					pansarLocks[i, j] = door.closedFac;
					pansarLocks[i, j + 2] = 1f - door.closedFac;
				}
			}
			for (int k = 0; k < 4; k++)
			{
				boltsBolted[k] = door.closedFac == 1f;
				poles[k, 0] = 1f - door.closedFac;
				poles[k, 1] = 1f - door.closedFac;
			}
			for (int l = 0; l < 2; l++)
			{
				for (int m = 0; m < 9; m++)
				{
					clamps[l, m] = new Clamp(this, l, m);
				}
				for (int n = 1; n < 9; n++)
				{
					clamps[l, n].previous = clamps[l, n - 1];
				}
				for (int num = 0; num < 8; num++)
				{
					clamps[l, num].next = clamps[l, num + 1];
				}
				if (l == 1)
				{
					for (int num2 = 0; num2 < 9; num2++)
					{
						clamps[1, num2].partner = clamps[0, num2];
					}
				}
				bigScrews[l, 0] = rgGraphics.gate.room.game.SeededRandom(rgGraphics.gate.room.abstractRoom.index + door.number + l) * 360f;
				bigScrews[l, 1] = bigScrews[l, 0];
			}
			if (door.number == 0)
			{
				flip = false;
			}
			else if (door.number == 2)
			{
				flip = true;
			}
			else
			{
				flip = rgGraphics.gate.room.game.SeededRandom(rgGraphics.gate.room.abstractRoom.index) > 0.5f;
			}
			rustleLoop = new StaticSoundLoop(SoundID.Gate_Clamps_Moving_LOOP, posZ, rgGraphics.gate.room, 0f, 1f);
			screwTurnLoop = new StaticSoundLoop((rgGraphics.gate is WaterGate) ? SoundID.Gate_Water_Screw_Turning_LOOP : SoundID.Gate_Electric_Screw_Turning_LOOP, posZ, rgGraphics.gate.room, 0f, 1f);
			Reset();
		}

		public void Reset()
		{
			for (int i = 0; i < 3; i++)
			{
				tracks[i, 2] = UnityEngine.Random.value * 0.5f;
			}
			for (int j = 0; j < 4; j++)
			{
				poles[j, 2] = 1f / Mathf.Lerp(10f, 60f, Mathf.Pow(UnityEngine.Random.value, 2.1f));
			}
			for (int k = 0; k < 2; k++)
			{
				for (int l = 0; l < 2; l++)
				{
					blocks[k, l, 2] = UnityEngine.Random.value * 0.4f;
					arms[k, l, 2] = 0.2f + 0.8f * UnityEngine.Random.value;
				}
			}
		}

		public void Update()
		{
			rustleLoop.Update();
			screwTurnLoop.Update();
			if (lastClosedFac == 0f && Closed > 0f)
			{
				rgGraphics.gate.room.PlaySound(SoundID.Gate_Poles_And_Rails_In, posZ, 1f * rgGraphics.QuieterSoundFactor, 1f);
			}
			else if (Closed == 0f && lastClosedFac > 0f)
			{
				rgGraphics.gate.room.PlaySound(SoundID.Gate_Poles_Out, posZ, 1f * rgGraphics.QuieterSoundFactor, 1f);
			}
			if (lastClosedFac <= 0.78f && Closed > 0.78f)
			{
				rgGraphics.gate.room.PlaySound(SoundID.Gate_Secure_Rail_Down, posZ, 1f * rgGraphics.QuieterSoundFactor, 1f);
			}
			else if (lastClosedFac >= 0.9f && Closed < 0.9f)
			{
				rgGraphics.gate.room.PlaySound(SoundID.Gate_Secure_Rail_Up, posZ, 1f * rgGraphics.QuieterSoundFactor, 1f);
			}
			float num = Mathf.InverseLerp(0.2f, 0.5f, lastClosedFac);
			if (BlocksClosed >= 0.2f && num < 0.2f)
			{
				rgGraphics.gate.room.PlaySound(SoundID.Gate_Pillows_Move_In, posZ, 1f * rgGraphics.QuieterSoundFactor, 1f);
			}
			else if (BlocksClosed >= 0.9f && num < 0.9f)
			{
				rgGraphics.gate.room.PlaySound(SoundID.Gate_Pillows_In_Place, posZ, 1f * rgGraphics.QuieterSoundFactor, 1f);
			}
			else if (BlocksClosed <= 0.9f && num > 0.9f)
			{
				rgGraphics.gate.room.PlaySound(SoundID.Gate_Pillows_Move_Out, posZ, 1f * rgGraphics.QuieterSoundFactor, 1f);
			}
			if (door.closedFac > ClampsPause && lastClosedFac <= ClampsPause)
			{
				wantedClampStatus = 1;
			}
			else if (door.closedFac < ClampsPause && lastClosedFac >= ClampsPause)
			{
				wantedClampStatus = -1;
			}
			lastClosedFac = door.closedFac;
			lastPC = PC;
			PC = PansarClosed;
			if (lastPC == 0f && PC > 0f)
			{
				rgGraphics.gate.room.PlaySound(SoundID.Gate_Panser_On, posZ, 1f * rgGraphics.QuieterSoundFactor, 1f);
			}
			else if (lastPC == 1f && PC < 1f)
			{
				rgGraphics.gate.room.PlaySound(SoundID.Gate_Panser_Off, posZ, 1f * rgGraphics.QuieterSoundFactor, 1f);
			}
			if (Closed > 0.95f)
			{
				locksLocked += 1f / 60f;
			}
			else
			{
				locksLocked = 0f;
			}
			for (int i = 0; i < 3; i++)
			{
				tracks[i, 1] = tracks[i, 0];
				tracks[i, 0] = Mathf.InverseLerp((i == 1) ? 1f : 0.7f, tracks[i, 2], TracksClosed);
				if (i == 1 && tracks[i, 0] == 0f && tracks[i, 1] > 0f)
				{
					rgGraphics.gate.room.ScreenMovement(posZ, new Vector2(0f, 0f), 0.5f);
					rgGraphics.gate.room.PlaySound(SoundID.Gate_Rails_Collide, posZ, 1f * rgGraphics.QuieterSoundFactor, 1f);
				}
			}
			for (int j = 0; j < 4; j++)
			{
				poles[j, 1] = poles[j, 0];
				if (PolesClosed)
				{
					poles[j, 0] = Mathf.Max(poles[j, 0] - 0.2f, 0f);
				}
				else
				{
					poles[j, 0] = Mathf.Min(poles[j, 0] + poles[j, 2], 1f);
				}
			}
			float num2 = pansarLocks[8, 0];
			for (int k = 0; k < 9; k++)
			{
				pansarLocks[k, 1] = pansarLocks[k, 0];
				float num3 = ((float)k + 0.5f) / 9f;
				pansarLocks[k, 0] = Mathf.Lerp(0f, num3, Mathf.Pow(LocksClosed, 1.5f - num3)) - Mathf.Pow(1f - LocksClosed, 2f) * 0.3f;
				pansarLocks[k, 3] = pansarLocks[k, 2];
				if (1f - locksLocked < num3)
				{
					pansarLocks[k, 2] = Mathf.Max(pansarLocks[k, 2] - 1f / 30f, 0f);
				}
				else
				{
					pansarLocks[k, 2] = Mathf.Min(pansarLocks[k, 2] + 0.1f, 1f);
				}
				if (k % 2 == 0 && k < 8)
				{
					if (pansarLocks[k, 2] == 0f && pansarLocks[k, 3] > 0f)
					{
						boltsBolted[k / 2] = true;
						rgGraphics.gate.room.ScreenMovement(posZ, new Vector2(0f, 0f), 0.3f);
						rgGraphics.gate.room.PlaySound(SoundID.Gate_Bolt, posZ, 1f * rgGraphics.QuieterSoundFactor, 1f);
					}
					else if (pansarLocks[k, 0] < 0f && pansarLocks[k, 1] >= 0f)
					{
						boltsBolted[k / 2] = false;
					}
				}
			}
			if (pansarLocks[8, 0] > 0.94f && (double)num2 <= 0.94)
			{
				rgGraphics.gate.room.PlaySound(SoundID.Gate_Secure_Rail_Slam, posZ, 1f * rgGraphics.QuieterSoundFactor, 1f);
			}
			clampsStatus = -2;
			int num4 = 0;
			for (int l = 0; l < 2; l++)
			{
				for (int m = 0; m < 9; m++)
				{
					clamps[l, m].Update(wantedClampStatus);
					int num5 = 0;
					if (clamps[l, m].mode == Clamp.Mode.Stacked)
					{
						num5 = -1;
					}
					else if (clamps[l, m].mode == Clamp.Mode.Locked)
					{
						num5 = 1;
					}
					else
					{
						num4++;
					}
					if (clampsStatus == -2)
					{
						clampsStatus = num5;
					}
					else if (num5 != clampsStatus)
					{
						clampsStatus = 0;
					}
				}
			}
			door.movementStalledByGraphicsModule = clampsStatus != wantedClampStatus;
			rustleLoop.volume = Mathf.Lerp(rustleLoop.volume, Mathf.Pow((float)num4 / 17f, 0.5f), 0.5f) * rgGraphics.QuieterSoundFactor;
			for (int n = 0; n < 2; n++)
			{
				for (int num6 = 0; num6 < 2; num6++)
				{
					blocks[n, num6, 1] = blocks[n, num6, 0];
					blocks[n, num6, 0] = Mathf.InverseLerp(blocks[n, num6, 2], 1f, BlocksClosed);
					arms[n, num6, 1] = arms[n, num6, 0];
					arms[n, num6, 0] = Mathf.InverseLerp(0f, arms[n, num6, 2], ArmsWithdrawn);
				}
			}
			float num7 = 0f;
			for (int num8 = 0; num8 < 2; num8++)
			{
				bigScrews[num8, 1] = bigScrews[num8, 0];
				if (rgGraphics.gate.goalDoorPositions[door.number] == 1f)
				{
					bigScrews[num8, 0] += Mathf.Sin((float)Math.PI * Mathf.InverseLerp(0f, 0.6f, Closed)) * 2.5f * ((num8 == 0 == flip) ? (-1f) : 1f);
				}
				else
				{
					bigScrews[num8, 0] -= Mathf.Pow(Mathf.Sin((float)Math.PI * Mathf.InverseLerp(0.3f, 1f, Closed)), 2f) * 3f * ((num8 == 0 == flip) ? (-1f) : 1f);
				}
				num7 = Mathf.Max(num7, Mathf.InverseLerp(0.2f, 2f, Mathf.Abs(bigScrews[num8, 0] - bigScrews[num8, 1])));
			}
			screwTurnLoop.volume = Mathf.Lerp(screwTurnLoop.volume, num7, 0.1f) * rgGraphics.QuieterSoundFactor;
			screwTurnLoop.pitch = Mathf.Lerp(0.25f, 1f, Mathf.Pow(num7, 2f));
			if (Closed > 0f && Closed < 1f)
			{
				PansarPush();
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			for (int i = 0; i < 4; i++)
			{
				sLeaser.sprites[PoleSprite(i)] = new FSprite("pixel");
				sLeaser.sprites[PoleSprite(i)].scaleX = 3f;
				sLeaser.sprites[PoleSprite(i)].scaleY = 200f;
			}
			for (int j = 0; j < 2; j++)
			{
				sLeaser.sprites[CenterTrackSprite(j)] = new FSprite("RegionGate_CenterTrack" + ((j == 0) ? "A" : "B"));
				sLeaser.sprites[CenterTrackSprite(j)].anchorY = ((j == 0) ? 1f : 0f);
				sLeaser.sprites[CenterTrackSprite(j)].shader = rgGraphics.gate.room.game.rainWorld.Shaders["ColoredSprite2"];
				sLeaser.sprites[BigScrewSprite(j)] = new FSprite("RegionGate_BigScrew");
				sLeaser.sprites[BigScrewSprite(j)].shader = rgGraphics.gate.room.game.rainWorld.Shaders["ColoredSprite2"];
				sLeaser.sprites[PansarSprite(j)] = new FSprite("RegionGate_Pansar" + (j + 1));
				sLeaser.sprites[PansarSprite(j)].anchorX = ((j == 0) ? 1f : 0f);
				sLeaser.sprites[PansarSprite(j)].shader = rgGraphics.gate.room.game.rainWorld.Shaders["ColoredSprite2"];
				sLeaser.sprites[BehindPansarSprite(j)] = new FSprite("RegionGate_Pansar" + (j + 1));
				sLeaser.sprites[BehindPansarSprite(j)].anchorX = ((j == 0) ? 1f : 0f);
				sLeaser.sprites[BehindPansarSprite(j)].shader = rgGraphics.gate.room.game.rainWorld.Shaders["ColoredSprite2"];
				for (int k = 0; k < 2; k++)
				{
					sLeaser.sprites[TrackSprite(j, k)] = new FSprite("RegionGate_Track" + ((j == 0) ? "A" : "B"));
					sLeaser.sprites[TrackSprite(j, k)].anchorY = ((j == 0) ? 1f : 0f);
					sLeaser.sprites[TrackSprite(j, k)].scaleX = ((k == 0) ? 1f : (-1f));
					sLeaser.sprites[TrackSprite(j, k)].anchorX = 1f;
					sLeaser.sprites[TrackSprite(j, k)].shader = rgGraphics.gate.room.game.rainWorld.Shaders["ColoredSprite2"];
					sLeaser.sprites[BlockSprite(j, k)] = new FSprite("RegionGate_Block" + (3 - (j + j + (1 - k)) + 1));
					sLeaser.sprites[BlockSprite(j, k)].anchorX = ((k == 0) ? 1f : 0f);
					sLeaser.sprites[BlockSprite(j, k)].anchorY = 1f;
					sLeaser.sprites[BlockSprite(j, k)].shader = rgGraphics.gate.room.game.rainWorld.Shaders["ColoredSprite2"];
					sLeaser.sprites[HandSprite(j, k)] = new FSprite("RegionGate_Hand");
					sLeaser.sprites[HandSprite(j, k)].scaleX = ((k == 0) ? 1f : (-1f));
					sLeaser.sprites[HandSprite(j, k)].shader = rgGraphics.gate.room.game.rainWorld.Shaders["ColoredSprite2"];
					sLeaser.sprites[HandSprite(j, k)].alpha = 14f / 15f;
					sLeaser.sprites[ArmSprite(j, k)] = new FSprite("RegionGate_Pixel");
					sLeaser.sprites[ArmSprite(j, k)].anchorY = 0f;
					sLeaser.sprites[ArmSprite(j, k)].scaleX = 3f;
					sLeaser.sprites[ArmSprite(j, k)].scaleY = 100f;
					sLeaser.sprites[ArmSprite(j, k)].shader = rgGraphics.gate.room.game.rainWorld.Shaders["ColoredSprite2"];
					sLeaser.sprites[ArmSprite(j, k)].alpha = 14f / 15f;
					for (int l = 0; l < 2; l++)
					{
						sLeaser.sprites[CogSprite(j, k, l)] = new FSprite("RegionGate_Cog");
						sLeaser.sprites[CogSprite(j, k, l)].shader = rgGraphics.gate.room.game.rainWorld.Shaders["ColoredSprite2"];
						sLeaser.sprites[CogSprite(j, k, l)].alpha = 1f - ((l == 0) ? 12f : 15f) / 30f;
					}
				}
				for (int m = 0; m < 9; m++)
				{
					sLeaser.sprites[ClampSprite(j, m)] = new FSprite("RegionGate_Clamp" + ((m % 2 == 0) ? "A" : "B") + (j + 1));
					sLeaser.sprites[ClampSprite(j, m)].anchorX = ((j == 0) ? 1f : 0f);
					sLeaser.sprites[ClampSprite(j, m)].anchorY = 0f;
					sLeaser.sprites[ClampSprite(j, m)].shader = rgGraphics.gate.room.game.rainWorld.Shaders["ColoredSprite2"];
				}
			}
			for (int n = 0; n < 4; n++)
			{
				sLeaser.sprites[BoltSprite(n)] = new FSprite("RegionGate_Bolt");
				sLeaser.sprites[BoltSprite(n)].alpha = 14f / 15f;
				sLeaser.sprites[BoltSprite(n)].shader = rgGraphics.gate.room.game.rainWorld.Shaders["ColoredSprite2"];
			}
			for (int num = 0; num < 9; num++)
			{
				sLeaser.sprites[PansarSegmentSprite(num)] = new FSprite((num % 2 == 0) ? "RegionGate_PansarSegment" : "RegionGate_PansarLock");
				sLeaser.sprites[PansarSegmentSprite(num)].shader = rgGraphics.gate.room.game.rainWorld.Shaders["ColoredSprite2"];
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			camPos.x += 0.25f;
			camPos.y += 0.25f;
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[CenterTrackSprite(i)].x = posZ.x - camPos.x;
				float num = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(tracks[1, 1], tracks[1, 0], timeStacker)), 0.6f);
				sLeaser.sprites[CenterTrackSprite(i)].y = posZ.y + ((i == 0) ? (num * 65f) : (-180f - num * 130f)) - camPos.y;
				sLeaser.sprites[CenterTrackSprite(i)].alpha = 1f - Mathf.Lerp(1.5f, 2.5f, num) / 30f;
				Vector2 vector = new Vector2(posZ.x, posZ.y + ((i != 0) ? 40f : (-220f)));
				for (int j = 0; j < 2; j++)
				{
					sLeaser.sprites[TrackSprite(i, j)].x = posZ.x + ((j == 0) ? (-9f) : 9f) - camPos.x;
					num = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(tracks[j * 2, 1], tracks[j * 2, 0], timeStacker)), 1.4f);
					sLeaser.sprites[TrackSprite(i, j)].y = posZ.y + ((i == 0) ? (num * 130f) : (-180f - num * 65f)) - camPos.y;
					sLeaser.sprites[TrackSprite(i, j)].alpha = 1f - Mathf.Lerp(1.5f, 2.5f, num) / 30f;
					float num2 = Mathf.Lerp(blocks[i, j, 1], blocks[i, j, 0], timeStacker);
					Vector2 vector2 = posZ;
					Vector2 vector3 = Custom.DegToVec(30f + num2 * 150f);
					vector3.y += 1f;
					vector3.x *= 20f * (float)(-1 + 2 * j) * Mathf.Lerp(num2, 1f, 0.5f);
					vector3.y *= 30f * (float)(-1 + 2 * i);
					vector3.y += 60f * Mathf.InverseLerp(0.2f, 0f, num2) * (float)(-1 + 2 * i);
					vector2 += vector3;
					if (i == 0)
					{
						vector2.y -= 90f;
					}
					sLeaser.sprites[BlockSprite(i, j)].x = vector2.x - camPos.x;
					sLeaser.sprites[BlockSprite(i, j)].y = vector2.y - camPos.y;
					float num3 = Mathf.Pow(Mathf.Sin((float)Math.PI * num2), 3f) * -2f * (float)(-1 + 2 * j) * (float)(-1 + 2 * i);
					sLeaser.sprites[BlockSprite(i, j)].rotation = num3;
					sLeaser.sprites[BlockSprite(i, j)].alpha = 1f - Mathf.Lerp(3f, 2f, Mathf.Pow(num2, 7f)) / 30f;
					float num4 = Mathf.Lerp(0f, 100f * ((i == 0) ? (-1f) : 1f), Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(arms[i, j, 1], arms[i, j, 0], timeStacker)), 0.8f));
					float y = Mathf.Lerp(-10f, -80f, (i == 0) ? 0f : 1f);
					Vector2 vector4 = vector2 + Custom.RotateAroundOrigo(new Vector2(22f * (float)(-1 + 2 * j), y), num3);
					vector4.y += num4;
					Vector2 vector5 = posZ + new Vector2((float)(-1 + 2 * j) * Mathf.Lerp(30f, -35f, Mathf.Pow(0.5f * num2 + 0.5f * Mathf.Sin(num2 * (float)Math.PI), 1f + 4f * num2)), (i == 1) ? 60f : (-240f));
					vector5.y += num4;
					float num5 = Custom.CirclesCollisionTime(vector4.x, vector4.y, vector.x, vector.y, vector5.x - vector4.x, vector5.y - vector4.y, 1f, 28f);
					if (num5 > 0f && num5 < 1f)
					{
						vector5 = Vector2.Lerp(vector4, vector5, num5);
					}
					sLeaser.sprites[ArmSprite(i, j)].x = vector4.x - camPos.x;
					sLeaser.sprites[ArmSprite(i, j)].y = vector4.y - camPos.y;
					sLeaser.sprites[HandSprite(i, j)].x = vector4.x - camPos.x;
					sLeaser.sprites[HandSprite(i, j)].y = vector4.y - camPos.y;
					sLeaser.sprites[HandSprite(i, j)].rotation = num3;
					sLeaser.sprites[ArmSprite(i, j)].rotation = Custom.AimFromOneVectorToAnother(vector4, vector5);
					sLeaser.sprites[ArmSprite(i, j)].scaleY = Vector2.Distance(vector4, vector5);
				}
				for (int k = 0; k < 9; k++)
				{
					sLeaser.sprites[ClampSprite(i, k)].x = Mathf.Lerp(clamps[i, k].lastPos.x, clamps[i, k].pos.x, timeStacker) - camPos.x;
					sLeaser.sprites[ClampSprite(i, k)].y = Mathf.Lerp(clamps[i, k].lastPos.y, clamps[i, k].pos.y, timeStacker) - camPos.y;
					sLeaser.sprites[ClampSprite(i, k)].rotation = Mathf.Lerp(clamps[i, k].lastRotat, clamps[i, k].rotat, timeStacker);
					sLeaser.sprites[ClampSprite(i, k)].alpha = 1f - clamps[i, k].depth;
				}
				float num6 = Mathf.Lerp(lastPC, PC, timeStacker);
				Vector2 vector6 = posZ;
				vector6.x += (float)(-1 + i * 2) * 7f * num6;
				vector6.x += Mathf.Sin(num6 * (float)Math.PI) * 25f * ((i == 0) ? (-1f) : 1f);
				vector6.y -= 90f;
				sLeaser.sprites[PansarSprite(i)].x = vector6.x - camPos.x;
				sLeaser.sprites[PansarSprite(i)].y = vector6.y - camPos.y;
				sLeaser.sprites[PansarSprite(i)].isVisible = num6 > 0.5f;
				sLeaser.sprites[BehindPansarSprite(i)].x = vector6.x - camPos.x;
				sLeaser.sprites[BehindPansarSprite(i)].y = vector6.y - camPos.y;
				sLeaser.sprites[PansarSprite(i)].alpha = Mathf.Lerp(0.7f, 1f, num6);
				sLeaser.sprites[BehindPansarSprite(i)].alpha = Mathf.Lerp(0.7f, 1f, num6);
				sLeaser.sprites[BigScrewSprite(i)].rotation = Mathf.Lerp(bigScrews[i, 1], bigScrews[i, 0], timeStacker);
				sLeaser.sprites[BigScrewSprite(i)].x = vector.x - camPos.x;
				sLeaser.sprites[BigScrewSprite(i)].y = vector.y - camPos.y;
				for (int l = 0; l < 2; l++)
				{
					for (int m = 0; m < 2; m++)
					{
						Vector2 vector7 = posZ;
						vector7.y -= 90f + ((i == 0) ? (-1f) : 1f) * ((m == 0) ? 150f : 175f) * ((i == 0) ? 1f : 1.2f);
						vector7.x += ((l == 0) ? (-1f) : 1f) * ((m == 0) ? 40f : 50f) * ((i == 0) ? 1f : 0.8f);
						sLeaser.sprites[CogSprite(i, l, m)].x = vector7.x - camPos.x;
						sLeaser.sprites[CogSprite(i, l, m)].y = vector7.y - camPos.y;
						sLeaser.sprites[CogSprite(i, l, m)].rotation = ((l == 0) ? (-1f) : 1f) * ((i == 0) ? 1f : (-1f)) * (GearsTurned * 0.5f + 0.5f * Mathf.Sin(GearsTurned * (float)Math.PI)) * ((m == 0) ? 90f : 210f);
					}
				}
			}
			for (int n = 0; n < 9; n++)
			{
				sLeaser.sprites[PansarSegmentSprite(n)].x = posZ.x - camPos.x;
				sLeaser.sprites[PansarSegmentSprite(n)].y = posZ.y - 180f * Mathf.Lerp(pansarLocks[n, 1], pansarLocks[n, 0], timeStacker) - camPos.y;
				sLeaser.sprites[PansarSegmentSprite(n)].alpha = Mathf.Lerp(1f, 0.8f, Mathf.InverseLerp(0.3f, -0.2f, Mathf.Lerp(pansarLocks[n, 1], pansarLocks[n, 0], timeStacker)));
				if (n % 2 == 1)
				{
					sLeaser.sprites[PansarSegmentSprite(n)].rotation = 90f * Mathf.Lerp(pansarLocks[n, 3], pansarLocks[n, 2], timeStacker) * ((n / 2 % 2 == 0 == flip) ? (-1f) : 1f);
				}
			}
			for (int num7 = 0; num7 < 4; num7++)
			{
				if (Closed == 1f || (poles[num7, 1] == 1f && poles[num7, 0] == 1f))
				{
					sLeaser.sprites[PoleSprite(num7)].isVisible = false;
					continue;
				}
				sLeaser.sprites[PoleSprite(num7)].isVisible = true;
				sLeaser.sprites[PoleSprite(num7)].x = posZ.x + (((num7 > 0 && num7 < 3) ? 14f : 11f) + ((num7 < 2) ? 1f : 0f)) * ((float)num7 - 1.5f) - camPos.x;
				float num8 = ((num7 % 2 == 0 == flip) ? (-1f) : 1f);
				sLeaser.sprites[PoleSprite(num7)].y = posZ.y - 90f + num8 * 200f * Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(poles[num7, 1], poles[num7, 0], timeStacker)), 1.4f) - camPos.y;
			}
			for (int num9 = 0; num9 < 4; num9++)
			{
				sLeaser.sprites[BoltSprite(num9)].x = posZ.x - camPos.x;
				sLeaser.sprites[BoltSprite(num9)].y = posZ.y - 30f - 40f * (float)num9 - camPos.y;
				sLeaser.sprites[BoltSprite(num9)].isVisible = boltsBolted[num9];
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					sLeaser.sprites[PoleSprite(i + j + j)].color = Color.Lerp(palette.blackColor, palette.fogColor, 0.12f);
					sLeaser.sprites[BlockSprite(i, j)].color = Color.Lerp(new Color(0.05f, 0.05f, 0f), Color.Lerp(palette.texture.GetPixel(6, 4), palette.fogColor, 0.5f), 0.25f);
				}
			}
		}

		private void PansarPush()
		{
			float num = 21f;
			num += 7f * PC;
			num += Mathf.Sin(PC * (float)Math.PI) * 25f;
			Room room = rgGraphics.gate.room;
			for (int i = 0; i < room.physicalObjects.Length; i++)
			{
				for (int j = 0; j < room.physicalObjects[i].Count; j++)
				{
					for (int k = 0; k < room.physicalObjects[i][j].bodyChunks.Length; k++)
					{
						BodyChunk bodyChunk = room.physicalObjects[i][j].bodyChunks[k];
						if (bodyChunk.pos.x > posZ.x - num - bodyChunk.rad && bodyChunk.pos.x < posZ.x)
						{
							bodyChunk.pos.x = posZ.x - num - bodyChunk.rad;
							bodyChunk.vel.x *= 0.2f;
						}
						else if (bodyChunk.pos.x < posZ.x + num + bodyChunk.rad && bodyChunk.pos.x > posZ.x)
						{
							bodyChunk.pos.x = posZ.x + num + bodyChunk.rad;
							bodyChunk.vel.x *= 0.2f;
						}
					}
				}
			}
		}
	}

	public class Clamp
	{
		public class Mode : ExtEnum<Mode>
		{
			public static readonly Mode Stacked = new Mode("Stacked", register: true);

			public static readonly Mode MovingDown = new Mode("MovingDown", register: true);

			public static readonly Mode WaitingForPartner = new Mode("WaitingForPartner", register: true);

			public static readonly Mode Locked = new Mode("Locked", register: true);

			public static readonly Mode MovingUp = new Mode("MovingUp", register: true);

			public Mode(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public DoorGraphic doorG;

		public int side;

		public int number;

		public Clamp previous;

		public Clamp next;

		public Clamp partner;

		public float velY;

		public Vector2 pos;

		public Vector2 lastPos;

		public float lastRotat;

		public float rotat;

		public float depth;

		private float fric;

		public float clampHeight;

		public float stackHeight;

		private int lockCounter;

		public Mode mode;

		public Vector2 posZ => doorG.posZ;

		public Vector2 StackPos
		{
			get
			{
				int num = 0;
				Clamp clamp = next;
				while (clamp != null && clamp.mode == Mode.Stacked)
				{
					num++;
					clamp = clamp.next;
				}
				return new Vector2(posZ.x, stackHeight) + new Vector2((-1f + 2f * (float)side) * (7f + 7f * (float)Math.Min(num, 2)), 6f * (float)num);
			}
		}

		public Clamp(DoorGraphic doorG, int side, int number)
		{
			this.doorG = doorG;
			this.side = side;
			this.number = number;
			stackHeight = posZ.y - 5f;
			clampHeight = posZ.y - 180f * ((float)(number + 1) / 9f);
			fric = Mathf.Lerp(0.75f, 0.95f, Mathf.Pow(UnityEngine.Random.value, 3f));
			if (doorG.door.closedFac == 1f)
			{
				mode = Mode.Locked;
				pos = new Vector2(posZ.x, clampHeight);
			}
			else
			{
				mode = Mode.Stacked;
				pos = new Vector2(posZ.x, stackHeight);
			}
			lastPos = pos;
		}

		public void Update(int desiredPosition)
		{
			lastPos = pos;
			lastRotat = rotat;
			if (mode == Mode.Locked)
			{
				rotat = 0f;
			}
			else
			{
				rotat = Mathf.Lerp(rotat, (float)(-1 + 2 * side) * 45f * Mathf.InverseLerp(stackHeight - 40f * ((float)(number + 1) / 9f), stackHeight, pos.y), 0.2f);
			}
			if (desiredPosition == -1 && mode != Mode.Stacked)
			{
				mode = Mode.MovingUp;
			}
			else if (desiredPosition == 1 && mode != Mode.Locked && mode != Mode.WaitingForPartner && (next == null || next.pos.y < stackHeight - 30f))
			{
				mode = Mode.MovingDown;
			}
			if (mode == Mode.Locked)
			{
				depth = 0.1f;
			}
			if (mode == Mode.MovingUp)
			{
				depth = 1f / 15f;
			}
			else
			{
				depth = Mathf.Lerp(depth, 0f, 0.1f);
			}
			if (next != null)
			{
				depth = Mathf.Min(depth, next.depth);
			}
			if (mode == Mode.Stacked)
			{
				velY = 0f;
				pos = Vector2.Lerp(pos, StackPos, 0.3f);
			}
			else if (mode == Mode.MovingDown)
			{
				pos.y += velY;
				velY *= fric;
				velY -= 0.5f;
				if (pos.y < clampHeight)
				{
					pos.y = clampHeight;
					velY = 0f;
					mode = Mode.WaitingForPartner;
					doorG.rgGraphics.gate.room.PlaySound(SoundID.Gate_Clamp_In_Position, pos, 1f * doorG.rgGraphics.QuieterSoundFactor, 1f);
				}
				else if (next != null && pos.y < next.pos.y + 20f)
				{
					next.velY += velY;
					velY = 0f;
					pos.y = next.pos.y + 20f;
					doorG.rgGraphics.gate.room.PlaySound(SoundID.Gate_Clamp_Collision, pos, 1f * doorG.rgGraphics.QuieterSoundFactor, 1f);
				}
			}
			else if (mode == Mode.WaitingForPartner)
			{
				if (partner != null && partner.mode == Mode.WaitingForPartner)
				{
					lockCounter++;
					if (lockCounter > 10)
					{
						mode = Mode.Locked;
						partner.mode = Mode.Locked;
						lockCounter = 0;
						doorG.rgGraphics.gate.room.PlaySound(SoundID.Gate_Clamp_Lock, pos, 1f * doorG.rgGraphics.QuieterSoundFactor, 1f);
					}
				}
			}
			else if (mode == Mode.MovingUp)
			{
				pos.y += velY;
				velY *= fric;
				velY = Mathf.Lerp(velY, 3.6f, 0.2f);
				if (pos.y > stackHeight)
				{
					mode = Mode.Stacked;
					doorG.rgGraphics.gate.room.PlaySound(SoundID.Gate_Clamp_Back_Into_Default, pos, 1f * doorG.rgGraphics.QuieterSoundFactor, 1f);
				}
				else if (previous != null && previous.mode != Mode.Stacked && pos.y > previous.pos.y - 20f)
				{
					previous.velY += velY;
					velY *= 0.2f;
					pos.y = previous.pos.y - 20f;
					doorG.rgGraphics.gate.room.PlaySound(SoundID.Gate_Clamp_Collision, pos, 1f * doorG.rgGraphics.QuieterSoundFactor, 1f);
				}
			}
			if (mode == Mode.Locked || mode == Mode.MovingUp)
			{
				pos.x = posZ.x;
			}
			else if (mode != Mode.Stacked)
			{
				pos.x = posZ.x + (float)(-1 + 2 * side) * 4f;
			}
		}
	}

	private RegionGate gate;

	public DoorGraphic[] doorGraphs;

	private Water water;

	public float WaterLevel;

	public Vector2[] heaterPositions;

	public int totalSprites;

	public SteamSmoke smoke;

	private float electricSteam;

	public Color blackColor;

	public Color fogColor;

	public float[,] heatersHeat;

	public Vector2[,][] heaterQuads;

	public LightSource heaterLightsource;

	private List<Bubble> bubbles;

	private int bubCounter;

	public StaticSoundLoop backgroundWorkingLoop;

	public StaticSoundLoop steamLoop;

	public StaticSoundLoop waterfallLoop;

	private float darkness;

	public int HeatDistortionSprite => doorGraphs[0].TotalSprites * 3 + 4;

	public int BatteryMeterSprite => doorGraphs[0].TotalSprites * 3;

	private float QuieterSoundFactor
	{
		get
		{
			if (!ModManager.MMF || !MMF.cfgQuieterGates.Value)
			{
				return 1f;
			}
			return 0.5f;
		}
	}

	public int WaterFallSprite(int waterFall)
	{
		return waterFall;
	}

	public int HeaterSprite(int heater, int part)
	{
		return doorGraphs[0].TotalSprites * 3 + heater * 2 + 1 - part;
	}

	public RegionGateGraphics(RegionGate gate)
	{
		this.gate = gate;
		WaterLevel = 0f;
		doorGraphs = new DoorGraphic[3];
		for (int i = 0; i < 3; i++)
		{
			doorGraphs[i] = new DoorGraphic(this, gate.doors[i]);
		}
		totalSprites = doorGraphs[0].TotalSprites * 3;
		backgroundWorkingLoop = new StaticSoundLoop((gate is WaterGate) ? SoundID.Gate_Water_Working_Background_LOOP : SoundID.Gate_Electric_Background_LOOP, new Vector2(gate.room.PixelWidth / 2f, gate.room.PixelHeight / 2f), gate.room, 0f, 1f);
		steamLoop = new StaticSoundLoop((gate is WaterGate) ? SoundID.Gate_Water_Steam_LOOP : SoundID.Gate_Electric_Steam_LOOP, new Vector2(0f, 0f), gate.room, 0f, 1f);
		if (gate is WaterGate)
		{
			waterfallLoop = new StaticSoundLoop(SoundID.Gate_Water_Waterfall_LOOP, new Vector2(0f, 0f), gate.room, 0f, 1f);
			totalSprites += 4;
			totalSprites++;
			WaterLevel = (gate as WaterGate).waterLeft;
			bool flag = true;
			if (gate.room.abstractRoom.name == "GATE_LF_SB" || gate.room.abstractRoom.name == "GATE_SB_SL" || gate.room.abstractRoom.name == "GATE_SH_SL")
			{
				flag = false;
			}
			if (flag)
			{
				water = new Water(gate.room, 60);
				gate.room.drawableObjects.Add(water);
				water.cosmeticLowerBorder = 520f;
			}
			heaterPositions = new Vector2[2];
			heaterPositions[0].y = 30f;
			heaterPositions[1].y = 30f;
			heaterPositions[0].x = 390f;
			heaterPositions[1].x = 570f;
			heatersHeat = new float[2, 3];
			heaterQuads = new Vector2[2, 2][];
			Vector2 size = Futile.atlasManager.GetElementWithName("RegionGate_Heater").sourceRect.size;
			for (int j = 0; j < 2; j++)
			{
				Vector2 vector = heaterPositions[j];
				Vector2[] array = new Vector2[4]
				{
					vector + new Vector2((0f - size.x) / 2f, (0f - size.y) / 2f),
					vector + new Vector2((0f - size.x) / 2f, size.y / 2f),
					vector + new Vector2(size.x / 2f, size.y / 2f),
					vector + new Vector2(size.x / 2f, (0f - size.y) / 2f)
				};
				if (j == 1)
				{
					Vector2 vector2 = array[0];
					array[0] = array[3];
					array[3] = vector2;
					vector2 = array[1];
					array[1] = array[2];
					array[2] = vector2;
				}
				for (int k = 0; k < 2; k++)
				{
					heaterQuads[j, k] = new Vector2[4];
					for (int l = 0; l < 4; l++)
					{
						array[l] += Custom.RNV() * UnityEngine.Random.value;
						heaterQuads[j, k][l] = array[l] + Custom.RNV() * UnityEngine.Random.value;
						array[l] += Custom.RNV() * UnityEngine.Random.value;
					}
				}
			}
			bubbles = new List<Bubble>();
		}
		else if (gate is ElectricGate)
		{
			totalSprites++;
		}
	}

	public void Update()
	{
		for (int i = 0; i < 3; i++)
		{
			doorGraphs[i].Update();
		}
		backgroundWorkingLoop.Update();
		if (gate.mode == RegionGate.Mode.MiddleClosed || gate.mode == RegionGate.Mode.Closed || gate.mode == RegionGate.Mode.Waiting || gate.mode == RegionGate.Mode.MiddleOpen)
		{
			backgroundWorkingLoop.volume = Mathf.Lerp(backgroundWorkingLoop.volume, 0f, 0.05f) * QuieterSoundFactor;
		}
		else
		{
			backgroundWorkingLoop.volume = Mathf.Lerp(backgroundWorkingLoop.volume, 1f, 0.05f) * QuieterSoundFactor;
		}
		if (gate.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.FakeGate) > 0f)
		{
			backgroundWorkingLoop.volume = 0f;
			steamLoop.volume = 0f;
			waterfallLoop.volume = 0f;
		}
		steamLoop.Update();
		float num = 0f;
		if (gate is WaterGate)
		{
			waterfallLoop.Update();
			float num2 = 0f;
			if (water != null)
			{
				bubCounter++;
				for (int j = 0; j < 3; j++)
				{
					if (bubCounter > 4 && UnityEngine.Random.value < 0.3f && doorGraphs[j].Bubble)
					{
						bubCounter = 0;
						Bubble bubble = new Bubble(doorGraphs[j].posZ + new Vector2(0f, 220f), new Vector2(0f, 0f), bottomBubble: false, fakeWaterBubble: true);
						gate.room.AddObject(bubble);
						bubbles.Add(bubble);
					}
				}
				if (Mathf.Abs(WaterLevel - (gate as WaterGate).waterLeft) > 0.01f)
				{
					water.GeneralUpsetSurface(Mathf.InverseLerp(0f, 0.06f, Mathf.Abs(WaterLevel - (gate as WaterGate).waterLeft)));
				}
				water.Update();
				water.cosmeticSurfaceDisplace = (1f - WaterLevel) * -34f * 20f;
				for (int num3 = bubbles.Count - 1; num3 >= 0; num3--)
				{
					if (bubbles[num3].pos.y > gate.room.PixelHeight || bubbles[num3].pos.y > 1200f + water.cosmeticSurfaceDisplace + 10f)
					{
						bubbles[num3].Destroy();
						bubbles.RemoveAt(num3);
					}
				}
			}
			WaterLevel = Mathf.Lerp(WaterLevel, (gate as WaterGate).waterLeft, 0.02f);
			int num4 = -1;
			for (int k = 0; k < 2; k++)
			{
				heatersHeat[k, 1] = heatersHeat[k, 0];
				if ((int)gate.mode > 0 && (int)gate.mode < 3 && k == 0 == gate.letThroughDir)
				{
					heatersHeat[k, 2] = Mathf.Min(heatersHeat[k, 2] + 0.0016666667f, 1f);
				}
				else
				{
					heatersHeat[k, 2] = Mathf.Max(heatersHeat[k, 2] - 0.0016666667f, 0f);
				}
				heatersHeat[k, 0] = Mathf.Lerp(heatersHeat[k, 0], heatersHeat[k, 2], 0.7f);
				if ((gate as WaterGate).waterFalls[k].flow > 0.2f && (gate as WaterGate).waterFalls[k].flow > num2)
				{
					num2 = (gate as WaterGate).waterFalls[k].flow;
					waterfallLoop.pos = heaterPositions[k];
				}
				if (heatersHeat[k, 0] > 0f)
				{
					if ((gate as WaterGate).waterFalls[k].topPos[0] > heaterPositions[k].y && (gate as WaterGate).waterFalls[k].bottomPos[0] < heaterPositions[k].y)
					{
						num = 1f;
						if (Mathf.Pow(UnityEngine.Random.value, 2f) < (gate as WaterGate).waterFalls[k].flow * 2f && Mathf.Pow(UnityEngine.Random.value, 2f) < heatersHeat[k, 2] * 2f)
						{
							FloatRect confines = new FloatRect((float)(17 + 9 * k) * 20f, 0f, (float)(22 + 9 * k) * 20f, 420f);
							if (smoke == null)
							{
								smoke = new SteamSmoke(gate.room);
								gate.room.AddObject(smoke);
							}
							Vector2 pos = heaterPositions[k] + new Vector2(Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 15f, Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 10f);
							smoke.EmitSmoke(pos, Custom.DegToVec(UnityEngine.Random.value * 360f), confines, Mathf.Pow(heatersHeat[k, 2], 0.75f));
							gate.room.PlaySound(SoundID.Gate_Water_Steam_Puff, pos, 1f * QuieterSoundFactor, 1f);
							heatersHeat[k, 0] = Mathf.Min(heatersHeat[k, 0], (1f - UnityEngine.Random.value * (gate as WaterGate).waterFalls[k].flow) * heatersHeat[k, 2]);
						}
					}
					else if (gate.room.FloatWaterLevel(heaterPositions[k].x) >= heaterPositions[k].y + 32f)
					{
						Bubble bubble2 = new Bubble(heaterPositions[k] + new Vector2(Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 35f, Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 32f), Custom.RNV(), bottomBubble: false, fakeWaterBubble: false);
						bubble2.ignoreWalls = true;
						gate.room.AddObject(bubble2);
					}
				}
				if (heatersHeat[k, 0] > 0f)
				{
					num4 = k;
				}
			}
			waterfallLoop.volume = Mathf.Lerp(waterfallLoop.volume, Mathf.Pow(num2, 0.5f), 0.1f) * QuieterSoundFactor;
			if (num4 < 0)
			{
				if (heaterLightsource != null)
				{
					heaterLightsource.setAlpha = 0f;
				}
			}
			else if (heaterLightsource != null)
			{
				heaterLightsource.stayAlive = true;
				heaterLightsource.setAlpha = Mathf.Pow(Mathf.InverseLerp(0.05f, 0.5f, heatersHeat[num4, 0]) * (0.8f + 0.2f * UnityEngine.Random.value), 0.75f);
				heaterLightsource.color = Custom.HSL2RGB(Mathf.InverseLerp(0.4f, 0.7f, heatersHeat[num4, 0]) * 0.045f, 1f, 0.5f + 0.1f * Mathf.InverseLerp(0.8f, 1f, heatersHeat[num4, 0] * (0.7f + UnityEngine.Random.value * 0.3f)));
				heaterLightsource.setPos = heaterPositions[num4];
				heaterLightsource.setRad = Mathf.Lerp(200f, 300f, Mathf.Sin((float)Math.PI * heatersHeat[num4, 0]));
				if (heaterLightsource.slatedForDeletetion)
				{
					heaterLightsource = null;
				}
			}
			else
			{
				heaterLightsource = new LightSource(new Vector2(-100f, -100f), environmentalLight: false, new Color(1f, 1f, 1f), null);
				heaterLightsource.affectedByPaletteDarkness = 0.5f;
				heaterLightsource.requireUpKeep = true;
				gate.room.AddObject(heaterLightsource);
			}
		}
		else if (gate is ElectricGate)
		{
			if (gate.mode == RegionGate.Mode.Waiting && gate.washingCounter > 0)
			{
				electricSteam = Mathf.Min(1f, electricSteam + 0.025f);
			}
			else
			{
				electricSteam = Mathf.Max(0f, electricSteam - 0.025f);
			}
			if (electricSteam > 0.5f)
			{
				num = 1f;
			}
			if (Mathf.Pow(UnityEngine.Random.value, 1.5f) < electricSteam * 2f)
			{
				int num5 = ((!gate.letThroughDir) ? 1 : 0);
				FloatRect confines2 = new FloatRect((float)(17 + 9 * num5) * 20f, 0f, (float)(22 + 9 * num5) * 20f, 420f);
				if (smoke == null)
				{
					smoke = new SteamSmoke(gate.room);
					gate.room.AddObject(smoke);
				}
				Vector2 pos2 = new Vector2(10f + (gate.letThroughDir ? 19f : 28f) * 20f, 30f) + new Vector2(Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 15f, Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 10f);
				smoke.EmitSmoke(pos2, Custom.DegToVec(UnityEngine.Random.value * 360f), confines2, Mathf.Pow(electricSteam, 0.75f));
				gate.room.PlaySound(SoundID.Gate_Electric_Steam_Puff, pos2, 1f * QuieterSoundFactor, 1f);
			}
		}
		steamLoop.pos = new Vector2(10f + (gate.letThroughDir ? 19f : 28f) * 20f, 30f);
		if (steamLoop.volume < num)
		{
			steamLoop.volume = Mathf.Min(num, steamLoop.volume + 1f / 30f) * QuieterSoundFactor;
		}
		else
		{
			steamLoop.volume = Mathf.Max(num, steamLoop.volume - 0.1f) * QuieterSoundFactor;
		}
		if (smoke != null && (smoke.slatedForDeletetion || smoke.room != gate.room))
		{
			smoke = null;
		}
	}

	public Color HeaterColor(int heater, float timeStacker)
	{
		float num = Mathf.Lerp(heatersHeat[heater, 1], heatersHeat[heater, 0], timeStacker);
		Color result = Color.Lerp(blackColor, new Color(1f, 0f, 0f), Mathf.InverseLerp(0f, 0.3f, num));
		if (num > 0.3f)
		{
			result = Custom.HSL2RGB(Mathf.InverseLerp(0.3f, 1f, num) * 0.16f, 1f, 0.5f + 0.2f * Mathf.InverseLerp(0.3f, 1f, num));
		}
		return result;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[totalSprites];
		for (int i = 0; i < 3; i++)
		{
			doorGraphs[i].InitiateSprites(sLeaser, rCam);
		}
		if (gate is WaterGate)
		{
			for (int j = 0; j < 2; j++)
			{
				for (int k = 0; k < 2; k++)
				{
					sLeaser.sprites[HeaterSprite(k, j)] = TriangleMesh.MakeGridMesh("RegionGate_Heater", 5);
				}
			}
			sLeaser.sprites[HeatDistortionSprite] = new FSprite("Futile_White");
			sLeaser.sprites[HeatDistortionSprite].shader = rCam.room.game.rainWorld.Shaders["HeatDistortion"];
		}
		else if (gate is ElectricGate)
		{
			sLeaser.sprites[BatteryMeterSprite] = new FSprite("pixel");
			sLeaser.sprites[BatteryMeterSprite].color = new Color(1f, 1f, 0f);
			sLeaser.sprites[BatteryMeterSprite].scaleY = 8f;
			sLeaser.sprites[BatteryMeterSprite].shader = rCam.room.game.rainWorld.Shaders["CustomDepth"];
			sLeaser.sprites[BatteryMeterSprite].alpha = 14f / 15f;
		}
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < 3; i++)
		{
			doorGraphs[i].DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
		if (gate is WaterGate)
		{
			int num = -1;
			for (int j = 0; j < 2; j++)
			{
				float num2 = Mathf.Lerp(heatersHeat[j, 1], heatersHeat[j, 0], timeStacker);
				if (num2 > 0f)
				{
					num = j;
				}
				Color color = HeaterColor(j, timeStacker);
				for (int k = 0; k < 2; k++)
				{
					if (k == 1)
					{
						color = Color.Lerp(color, Color.Lerp(blackColor, fogColor, Mathf.Lerp(0.3f, 0.8f, num2)), Mathf.Lerp(0.8f, 0.1f, num2));
					}
					Vector2[] array = new Vector2[4];
					for (int l = 0; l < 4; l++)
					{
						array[l] = heaterQuads[j, k][l] - camPos;
					}
					TriangleMesh.QuadGridMesh(array, sLeaser.sprites[HeaterSprite(j, k)] as TriangleMesh, 5);
					sLeaser.sprites[HeaterSprite(j, k)].color = color;
				}
			}
			if (num < 0)
			{
				sLeaser.sprites[HeatDistortionSprite].isVisible = false;
				return;
			}
			sLeaser.sprites[HeatDistortionSprite].isVisible = true;
			float num3 = Mathf.InverseLerp(0.15f, 0.8f, heatersHeat[num, 2]);
			sLeaser.sprites[HeatDistortionSprite].x = heaterPositions[num].x - camPos.x;
			sLeaser.sprites[HeatDistortionSprite].y = heaterPositions[num].y + 40f * num3 - camPos.y;
			sLeaser.sprites[HeatDistortionSprite].scaleX = Mathf.Lerp(10f, 15f, num3);
			sLeaser.sprites[HeatDistortionSprite].scaleY = Mathf.Lerp(15f, 30f, num3);
			sLeaser.sprites[HeatDistortionSprite].alpha = Custom.SCurve(heatersHeat[num, 2], 1.5f);
		}
		else if (gate is ElectricGate)
		{
			sLeaser.sprites[BatteryMeterSprite].y = (gate as ElectricGate).meterHeight - camPos.y;
			sLeaser.sprites[BatteryMeterSprite].x = 480f - camPos.x;
			sLeaser.sprites[BatteryMeterSprite].scaleX = 420f * (gate as ElectricGate).batteryLeft - ((gate as ElectricGate).batteryChanging ? (5f * UnityEngine.Random.value) : 0f);
			float num4 = ((gate as ElectricGate).batteryChanging ? 1f : 0f);
			sLeaser.sprites[BatteryMeterSprite].color = Color.Lerp(Custom.HSL2RGB(0.03f + UnityEngine.Random.value * (0.035f * num4 + 0.025f), 1f, (0.5f + UnityEngine.Random.value * 0.2f * num4) * Mathf.Lerp(1f, 0.25f, darkness)), blackColor, 0.5f);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		for (int i = 0; i < 3; i++)
		{
			doorGraphs[i].ApplyPalette(sLeaser, rCam, palette);
		}
		blackColor = palette.blackColor;
		fogColor = palette.fogColor;
		darkness = palette.darkness;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Water");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			if (i != HeatDistortionSprite)
			{
				if (i % doorGraphs[0].TotalSprites >= 10 && i % doorGraphs[0].TotalSprites <= 13)
				{
					rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[i]);
				}
				else
				{
					newContatiner.AddChild(sLeaser.sprites[i]);
				}
			}
		}
		if (gate is WaterGate)
		{
			for (int j = 0; j < 2; j++)
			{
				rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[doorGraphs[0].TotalSprites * 3 + j]);
			}
			rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[HeatDistortionSprite]);
		}
		else if (gate is ElectricGate)
		{
			rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[BatteryMeterSprite]);
		}
	}
}
