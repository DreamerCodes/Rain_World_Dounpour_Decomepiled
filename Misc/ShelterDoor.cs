using System;
using System.Collections.Generic;
using System.Linq;
using JollyCoop;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class ShelterDoor : UpdatableAndDeletable, IDrawable
{
	public class Door
	{
		private ShelterDoor shelter;

		public int number;

		public float closedFac;

		public float closeSpeed;

		public float openSpeed;

		public bool lastClosed;

		public bool movementStalledByGraphicsModule;

		public Door(ShelterDoor shelter, int number)
		{
			this.shelter = shelter;
			this.number = number;
			closedFac = shelter.closedFac;
			closeSpeed = 1f / 180f;
			openSpeed = 0.0045454544f;
		}

		public void Update()
		{
			if (movementStalledByGraphicsModule)
			{
				return;
			}
			if (closedFac > shelter.closedFac)
			{
				closedFac = Mathf.Max(0f, closedFac - openSpeed);
			}
			else if (closedFac < shelter.closedFac)
			{
				closedFac = Mathf.Min(1f, closedFac + closeSpeed);
			}
			if (shelter.room.readyForAI)
			{
				bool flag = closedFac > 0f;
				if (flag != lastClosed)
				{
					shelter.ChangeAncientDoorsStatus(number, !flag);
				}
				lastClosed = flag;
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
				return new Vector2(posZ.x, stackHeight) + new Vector2((-1f + 2f * (float)side) * (7f + 7f * (float)Mathf.Min(num, 2)), 6f * (float)num);
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
				depth = (depth = Mathf.Lerp(depth, 0f, 0.1f));
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
					doorG.myShelter.room.PlaySound(SoundID.Gate_Clamp_In_Position, pos, 1f * doorG.myShelter.QuieterSoundFactor, 1f);
				}
				else if (next != null && pos.y < next.pos.y + 20f)
				{
					next.velY += velY;
					velY = 0f;
					pos.y = next.pos.y + 20f;
					doorG.myShelter.room.PlaySound(SoundID.Gate_Clamp_Collision, pos, 1f * doorG.myShelter.QuieterSoundFactorMax, 1f);
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
						doorG.myShelter.room.PlaySound(SoundID.Gate_Clamp_Lock, pos, 1f * doorG.myShelter.QuieterSoundFactor, 1f);
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
					doorG.myShelter.room.PlaySound(SoundID.Gate_Clamp_Back_Into_Default, pos, 1f * doorG.myShelter.QuieterSoundFactor, 1f);
				}
				else if (previous != null && previous.mode != Mode.Stacked && pos.y > previous.pos.y - 20f)
				{
					previous.velY += velY;
					velY *= 0.2f;
					pos.y = previous.pos.y - 20f;
					doorG.myShelter.room.PlaySound(SoundID.Gate_Clamp_Collision, pos, 1f * doorG.myShelter.QuieterSoundFactorMax, 1f);
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

	public class DoorGraphic
	{
		public ShelterDoor myShelter;

		public Door door;

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

		public DoorGraphic(ShelterDoor shelter, Door door)
		{
			myShelter = shelter;
			this.door = door;
			lastClosedFac = door.closedFac;
			posZ = new Vector2((13f + 21f * (float)door.number) * 20f, 440f);
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
				bigScrews[l, 0] = myShelter.room.game.SeededRandom(myShelter.room.abstractRoom.index + door.number + l) * 360f;
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
				flip = myShelter.room.game.SeededRandom(myShelter.room.abstractRoom.index) > 0.5f;
			}
			rustleLoop = new StaticSoundLoop(SoundID.Gate_Clamps_Moving_LOOP, posZ, myShelter.room, 0f, 1f);
			screwTurnLoop = new StaticSoundLoop(SoundID.Gate_Water_Screw_Turning_LOOP, posZ, myShelter.room, 0f, 1f);
			Reset();
		}

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
				myShelter.room.PlaySound(SoundID.Gate_Poles_And_Rails_In, posZ, 1f * myShelter.QuieterSoundFactor, 1f);
			}
			else if (Closed == 0f && lastClosedFac > 0f)
			{
				myShelter.room.PlaySound(SoundID.Gate_Poles_Out, posZ, 1f * myShelter.QuieterSoundFactor, 1f);
			}
			if (lastClosedFac <= 0.78f && Closed > 0.78f)
			{
				myShelter.room.PlaySound(SoundID.Gate_Secure_Rail_Down, posZ, 1f * myShelter.QuieterSoundFactor, 1f);
			}
			else if (lastClosedFac >= 0.9f && Closed < 0.9f)
			{
				myShelter.room.PlaySound(SoundID.Gate_Secure_Rail_Up, posZ, 1f * myShelter.QuieterSoundFactor, 1f);
			}
			float num = Mathf.InverseLerp(0.2f, 0.5f, lastClosedFac);
			if (BlocksClosed >= 0.2f && num < 0.2f)
			{
				myShelter.room.PlaySound(SoundID.Gate_Pillows_Move_In, posZ, 1f * myShelter.QuieterSoundFactor, 1f);
			}
			else if (BlocksClosed >= 0.9f && num < 0.9f)
			{
				myShelter.room.PlaySound(SoundID.Gate_Pillows_In_Place, posZ, 1f * myShelter.QuieterSoundFactor, 1f);
			}
			else if (BlocksClosed <= 0.9f && num > 0.9f)
			{
				myShelter.room.PlaySound(SoundID.Gate_Pillows_Move_Out, posZ, 1f * myShelter.QuieterSoundFactor, 1f);
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
				myShelter.room.PlaySound(SoundID.Gate_Panser_On, posZ, 1f * myShelter.QuieterSoundFactor, 1f);
			}
			else if (lastPC == 1f && PC < 1f)
			{
				myShelter.room.PlaySound(SoundID.Gate_Panser_Off, posZ, 1f * myShelter.QuieterSoundFactor, 1f);
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
				tracks[i, 0] = Mathf.InverseLerp((i != 1) ? 0.7f : 1f, tracks[i, 2], TracksClosed);
				if (i == 1 && tracks[i, 0] == 0f && tracks[i, 1] > 0f)
				{
					myShelter.room.ScreenMovement(posZ, new Vector2(0f, 0f), 0.5f);
					myShelter.room.PlaySound(SoundID.Gate_Rails_Collide, posZ, 1f * myShelter.QuieterSoundFactorMax, 1f);
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
						myShelter.room.ScreenMovement(posZ, new Vector2(0f, 0f), 0.3f);
						myShelter.room.PlaySound(SoundID.Gate_Bolt, posZ, 1f * myShelter.QuieterSoundFactorMax, 1f);
					}
					else if (pansarLocks[k, 0] < 0f && pansarLocks[k, 1] >= 0f)
					{
						boltsBolted[k / 2] = false;
					}
				}
			}
			if (pansarLocks[8, 0] > 0.94f && (double)num2 <= 0.94)
			{
				myShelter.room.PlaySound(SoundID.Gate_Secure_Rail_Slam, posZ, 1f * myShelter.QuieterSoundFactorMax, 1f);
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
			rustleLoop.volume = Mathf.Lerp(rustleLoop.volume, Mathf.Pow((float)num4 / 17f, 0.5f), 0.5f) * myShelter.QuieterSoundFactorMin;
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
				if (myShelter.closedFac == 1f)
				{
					bigScrews[num8, 0] += Mathf.Sin((float)Math.PI * Mathf.InverseLerp(0f, 0.6f, Closed)) * 2.5f * ((num8 == 0 != flip) ? 1f : (-1f));
				}
				else
				{
					bigScrews[num8, 0] -= Mathf.Pow(Mathf.Sin((float)Math.PI * Mathf.InverseLerp(0.3f, 1f, Closed)), 2f) * 3f * ((num8 == 0 != flip) ? 1f : (-1f));
				}
				num7 = Mathf.Max(num7, Mathf.InverseLerp(0.2f, 2f, Mathf.Abs(bigScrews[num8, 0] - bigScrews[num8, 1])));
			}
			screwTurnLoop.volume = Mathf.Lerp(screwTurnLoop.volume, num7, 0.1f) * myShelter.QuieterSoundFactor;
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
				sLeaser.sprites[CenterTrackSprite(j)] = new FSprite("RegionGate_CenterTrack" + ((j != 0) ? "B" : "A"));
				sLeaser.sprites[CenterTrackSprite(j)].anchorY = ((j != 0) ? 0f : 1f);
				sLeaser.sprites[CenterTrackSprite(j)].shader = myShelter.room.game.rainWorld.Shaders["ColoredSprite2"];
				sLeaser.sprites[BigScrewSprite(j)] = new FSprite("RegionGate_BigScrew");
				sLeaser.sprites[BigScrewSprite(j)].shader = myShelter.room.game.rainWorld.Shaders["ColoredSprite2"];
				sLeaser.sprites[PansarSprite(j)] = new FSprite("RegionGate_Pansar" + (j + 1));
				sLeaser.sprites[PansarSprite(j)].anchorX = ((j != 0) ? 0f : 1f);
				sLeaser.sprites[PansarSprite(j)].shader = myShelter.room.game.rainWorld.Shaders["ColoredSprite2"];
				sLeaser.sprites[BehindPansarSprite(j)] = new FSprite("RegionGate_Pansar" + (j + 1));
				sLeaser.sprites[BehindPansarSprite(j)].anchorX = ((j != 0) ? 0f : 1f);
				sLeaser.sprites[BehindPansarSprite(j)].shader = myShelter.room.game.rainWorld.Shaders["ColoredSprite2"];
				for (int k = 0; k < 2; k++)
				{
					sLeaser.sprites[TrackSprite(j, k)] = new FSprite("RegionGate_Track" + ((j != 0) ? "B" : "A"));
					sLeaser.sprites[TrackSprite(j, k)].anchorY = ((j != 0) ? 0f : 1f);
					sLeaser.sprites[TrackSprite(j, k)].scaleX = ((k != 0) ? (-1f) : 1f);
					sLeaser.sprites[TrackSprite(j, k)].anchorX = 1f;
					sLeaser.sprites[TrackSprite(j, k)].shader = myShelter.room.game.rainWorld.Shaders["ColoredSprite2"];
					sLeaser.sprites[BlockSprite(j, k)] = new FSprite("RegionGate_Block" + (3 - (j + j + (1 - k)) + 1));
					sLeaser.sprites[BlockSprite(j, k)].anchorX = ((k != 0) ? 0f : 1f);
					sLeaser.sprites[BlockSprite(j, k)].anchorY = 1f;
					sLeaser.sprites[BlockSprite(j, k)].shader = myShelter.room.game.rainWorld.Shaders["ColoredSprite2"];
					sLeaser.sprites[HandSprite(j, k)] = new FSprite("RegionGate_Hand");
					sLeaser.sprites[HandSprite(j, k)].scaleX = ((k != 0) ? (-1f) : 1f);
					sLeaser.sprites[HandSprite(j, k)].shader = myShelter.room.game.rainWorld.Shaders["ColoredSprite2"];
					sLeaser.sprites[HandSprite(j, k)].alpha = 14f / 15f;
					sLeaser.sprites[ArmSprite(j, k)] = new FSprite("RegionGate_Pixel");
					sLeaser.sprites[ArmSprite(j, k)].anchorY = 0f;
					sLeaser.sprites[ArmSprite(j, k)].scaleX = 3f;
					sLeaser.sprites[ArmSprite(j, k)].scaleY = 100f;
					sLeaser.sprites[ArmSprite(j, k)].shader = myShelter.room.game.rainWorld.Shaders["ColoredSprite2"];
					sLeaser.sprites[ArmSprite(j, k)].alpha = 14f / 15f;
					for (int l = 0; l < 2; l++)
					{
						sLeaser.sprites[CogSprite(j, k, l)] = new FSprite("RegionGate_Cog");
						sLeaser.sprites[CogSprite(j, k, l)].shader = myShelter.room.game.rainWorld.Shaders["ColoredSprite2"];
						sLeaser.sprites[CogSprite(j, k, l)].alpha = 1f - ((l != 0) ? 15f : 12f) / 30f;
					}
				}
				for (int m = 0; m < 9; m++)
				{
					sLeaser.sprites[ClampSprite(j, m)] = new FSprite("RegionGate_Clamp" + ((m % 2 != 0) ? "B" : "A") + (j + 1));
					sLeaser.sprites[ClampSprite(j, m)].anchorX = ((j != 0) ? 0f : 1f);
					sLeaser.sprites[ClampSprite(j, m)].anchorY = 0f;
					sLeaser.sprites[ClampSprite(j, m)].shader = myShelter.room.game.rainWorld.Shaders["ColoredSprite2"];
				}
			}
			for (int n = 0; n < 4; n++)
			{
				sLeaser.sprites[BoltSprite(n)] = new FSprite("RegionGate_Bolt");
				sLeaser.sprites[BoltSprite(n)].alpha = 14f / 15f;
				sLeaser.sprites[BoltSprite(n)].shader = myShelter.room.game.rainWorld.Shaders["ColoredSprite2"];
			}
			for (int num = 0; num < 9; num++)
			{
				sLeaser.sprites[PansarSegmentSprite(num)] = new FSprite((num % 2 != 0) ? "RegionGate_PansarLock" : "RegionGate_PansarSegment");
				sLeaser.sprites[PansarSegmentSprite(num)].shader = myShelter.room.game.rainWorld.Shaders["ColoredSprite2"];
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
				sLeaser.sprites[CenterTrackSprite(i)].y = posZ.y + ((i != 0) ? (-180f - num * 130f) : (num * 65f)) - camPos.y;
				sLeaser.sprites[CenterTrackSprite(i)].alpha = 1f - Mathf.Lerp(1.5f, 2.5f, num) / 30f;
				Vector2 vector = new Vector2(posZ.x, posZ.y + ((i == 0) ? (-220f) : 40f));
				for (int j = 0; j < 2; j++)
				{
					sLeaser.sprites[TrackSprite(i, j)].x = posZ.x + ((j != 0) ? 9f : (-9f)) - camPos.x;
					num = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(tracks[j * 2, 1], tracks[j * 2, 0], timeStacker)), 1.4f);
					sLeaser.sprites[TrackSprite(i, j)].y = posZ.y + ((i != 0) ? (-180f - num * 65f) : (num * 130f)) - camPos.y;
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
					float num4 = Mathf.Lerp(0f, 100f * ((i != 0) ? 1f : (-1f)), Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(arms[i, j, 1], arms[i, j, 0], timeStacker)), 0.8f));
					float y = Mathf.Lerp(-10f, -80f, (i != 0) ? 1f : 0f);
					Vector2 vector4 = vector2 + Custom.RotateAroundOrigo(new Vector2(22f * (float)(-1 + 2 * j), y), num3);
					vector4.y += num4;
					Vector2 vector5 = posZ + new Vector2((float)(-1 + 2 * j) * Mathf.Lerp(30f, -35f, Mathf.Pow(0.5f * num2 + 0.5f * Mathf.Sin(num2 * (float)Math.PI), 1f + 4f * num2)), (i != 1) ? (-240f) : 60f);
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
				vector6.x += Mathf.Sin(num6 * (float)Math.PI) * 25f * ((i != 0) ? 1f : (-1f));
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
						vector7.y -= 90f + ((i != 0) ? 1f : (-1f)) * ((m != 0) ? 175f : 150f) * ((i != 0) ? 1.2f : 1f);
						vector7.x += ((l != 0) ? 1f : (-1f)) * ((m != 0) ? 50f : 40f) * ((i != 0) ? 0.8f : 1f);
						sLeaser.sprites[CogSprite(i, l, m)].x = vector7.x - camPos.x;
						sLeaser.sprites[CogSprite(i, l, m)].y = vector7.y - camPos.y;
						sLeaser.sprites[CogSprite(i, l, m)].rotation = ((l != 0) ? 1f : (-1f)) * ((i != 0) ? (-1f) : 1f) * (GearsTurned * 0.5f + 0.5f * Mathf.Sin(GearsTurned * (float)Math.PI)) * ((m != 0) ? 210f : 90f);
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
					sLeaser.sprites[PansarSegmentSprite(n)].rotation = 90f * Mathf.Lerp(pansarLocks[n, 3], pansarLocks[n, 2], timeStacker) * ((n / 2 % 2 == 0 != flip) ? 1f : (-1f));
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
				sLeaser.sprites[PoleSprite(num7)].x = posZ.x + (((num7 <= 0 || num7 >= 3) ? 11f : 14f) + ((num7 >= 2) ? 0f : 1f)) * ((float)num7 - 1.5f) - camPos.x;
				float num8 = ((num7 % 2 == 0 != flip) ? 1f : (-1f));
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
			Room room = myShelter.room;
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
							BodyChunk bodyChunk2 = bodyChunk;
							bodyChunk2.vel.x = bodyChunk2.vel.x * 0.2f;
						}
						else if (bodyChunk.pos.x < posZ.x + num + bodyChunk.rad && bodyChunk.pos.x > posZ.x)
						{
							bodyChunk.pos.x = posZ.x + num + bodyChunk.rad;
							BodyChunk bodyChunk3 = bodyChunk;
							bodyChunk3.vel.x = bodyChunk3.vel.x * 0.2f;
						}
					}
				}
			}
		}
	}

	private float closedFac;

	private float closeSpeed;

	private float[,] segmentPairs;

	private float[,] pistons;

	private float[,] covers;

	private float[,] pumps;

	private Vector2 pZero;

	private Vector2 dir;

	private Vector2 perp;

	private IntVector2[] closeTiles;

	private bool lastClosed;

	private RainCycle rainCycle;

	private float openUpTicks = 350f;

	private float initialWait = 80f;

	public IntVector2 playerSpawnPos;

	public StaticSoundLoop workingLoop;

	public StaticSoundLoop gasketMoverLoop;

	private bool killHostiles;

	private bool retrieveStuffStuckInWalls = true;

	private float brokenSegs;

	private float brokenFlaps;

	private bool checkForSpawnGhostSlugcat;

	private bool lastViewed;

	public AImapper aiMapper;

	private IntVector2[] _cachedTls = new IntVector2[100];

	public int openTime;

	public bool isAncient;

	public DoorGraphic[] doorGraphs;

	private Door[] ancientDoors;

	public bool IsClosing => closeSpeed > 0f;

	private float Closed
	{
		get
		{
			if (Broken)
			{
				return 0f;
			}
			return Mathf.Clamp(closedFac, 0f, 1f);
		}
	}

	private float FlapsOpen
	{
		get
		{
			if (!Broken)
			{
				return Mathf.InverseLerp(0.04f, 0f, Closed);
			}
			return brokenFlaps;
		}
	}

	private float PistonsClosed => Mathf.InverseLerp(0.2f, 0.1f, Closed);

	private float Segments
	{
		get
		{
			if (!Broken)
			{
				return Mathf.InverseLerp(0.2f, 0.38f, Closed);
			}
			return brokenSegs;
		}
	}

	private float Pistons => Mathf.InverseLerp(0.38f, 0.41f, Closed);

	private float Covers => Mathf.InverseLerp(0.41f, 0.51f, Closed);

	private float Cylinders => Mathf.InverseLerp(0.53f, 0.61f, Closed);

	private float PumpsEnter => Mathf.InverseLerp(0.59f, 0.7f, Closed);

	private float PumpsExit => Mathf.InverseLerp(0.75f, 1f, Closed);

	private float QuieterSoundFactorMax
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

	private float QuieterSoundFactor
	{
		get
		{
			if (!ModManager.MMF || !MMF.cfgQuieterGates.Value)
			{
				return 1f;
			}
			return 0.7f;
		}
	}

	private float QuieterSoundFactorMin
	{
		get
		{
			if (!ModManager.MMF || !MMF.cfgQuieterGates.Value)
			{
				return 1f;
			}
			return 0.9f;
		}
	}

	public bool Broken => room.world.brokenShelters[room.abstractRoom.shelterIndex];

	public bool IsOpening => closedFac != 0f;

	private int CogSprite(int cog)
	{
		return cog;
	}

	private int PistonSprite(int piston)
	{
		return 4 + piston;
	}

	private int PlugSprite(int plug)
	{
		return 6 + plug;
	}

	private int SegmentSprite(int segment)
	{
		return 14 + segment;
	}

	private int CylinderSprite(int cylinder)
	{
		return 24 + cylinder;
	}

	private int CoverSprite(int cover)
	{
		return 28 + cover;
	}

	private int PumpSprite(int pump)
	{
		return 32 + pump;
	}

	private int FlapSprite(int flap)
	{
		return 40 + flap;
	}

	public ShelterDoor(Room room)
	{
		base.room = room;
		rainCycle = room.world.rainCycle;
		if (room.abstractRoom.isAncientShelter)
		{
			isAncient = true;
		}
		if (ModManager.MSC && rainCycle.preTimer > 0 && room.world.GetAbstractRoom(room.world.game.startingRoom) == room.abstractRoom)
		{
			bool flag = false;
			for (int i = 0; i < room.roomSettings.placedObjects.Count; i++)
			{
				if (room.roomSettings.placedObjects[i].type == PlacedObject.Type.BrokenShelterWaterLevel)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				room.world.brokenShelters[room.abstractRoom.shelterIndex] = true;
				room.world.brokenShelterIndexDueToPrecycle = room.abstractRoom.shelterIndex;
			}
			if (!isAncient)
			{
				room.PlaySound(SoundID.Shelter_Bolt_Close, pZero, 1f * QuieterSoundFactor, 1f);
			}
			openUpTicks = 0f;
			initialWait = 0f;
		}
		if (Broken)
		{
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState(room.abstractRoom.index);
			brokenSegs = 0.25f + 0.5f * UnityEngine.Random.value;
			brokenFlaps = 0.5f + 0.5f * UnityEngine.Random.value;
			UnityEngine.Random.state = state;
		}
		for (int j = 0; j < room.abstractRoom.creatures.Count; j++)
		{
			if (room.abstractRoom.creatures[j].state.socialMemory != null)
			{
				for (int k = 0; k < room.game.Players.Count; k++)
				{
					SocialMemory.Relationship orInitiateRelationship = room.abstractRoom.creatures[j].state.socialMemory.GetOrInitiateRelationship(room.game.Players[k].ID);
					orInitiateRelationship.like = Mathf.Max(0.6f, orInitiateRelationship.like);
					orInitiateRelationship.tempLike = 1f;
				}
			}
		}
		if (rainCycle == null)
		{
			closedFac = 1f;
			closeSpeed = 1f;
		}
		else if (ModManager.MSC && rainCycle.preTimer > 0 && room.world.GetAbstractRoom(room.world.game.startingRoom) == room.abstractRoom)
		{
			closedFac = 0f;
			closeSpeed = -1f;
		}
		else
		{
			closedFac = (room.game.setupValues.cycleStartUp ? Mathf.InverseLerp(initialWait + openUpTicks, initialWait, rainCycle.timer) : 1f);
			closeSpeed = -1f;
		}
		closeTiles = new IntVector2[4];
		if (!isAncient)
		{
			for (int l = 0; l < room.TileWidth; l++)
			{
				for (int m = 0; m < room.TileHeight; m++)
				{
					if (room.GetTile(l, m).Terrain != Room.Tile.TerrainType.ShortcutEntrance)
					{
						continue;
					}
					pZero = room.MiddleOfTile(new IntVector2(l, m));
					dir = new Vector2(0f, -1f);
					for (int n = 0; n < 4; n++)
					{
						if (room.GetTile(l + Custom.fourDirections[n].x, m + Custom.fourDirections[n].y).Terrain == Room.Tile.TerrainType.Solid)
						{
							continue;
						}
						dir = Custom.fourDirections[n].ToVector2();
						for (int num = 0; num < 4; num++)
						{
							closeTiles[num] = new IntVector2(l, m) + Custom.fourDirections[n] * (num + 2);
						}
						playerSpawnPos = new IntVector2(l, m);
						while (room.GetTile(playerSpawnPos + Custom.fourDirections[n]).Terrain != Room.Tile.TerrainType.Solid)
						{
							playerSpawnPos += Custom.fourDirections[n];
						}
						if (dir.y == 1f)
						{
							while (room.GetTile(playerSpawnPos + new IntVector2(-1, 0)).Terrain != Room.Tile.TerrainType.Solid)
							{
								playerSpawnPos.x--;
							}
						}
						while (room.GetTile(playerSpawnPos + new IntVector2(0, -1)).Terrain != Room.Tile.TerrainType.Solid)
						{
							playerSpawnPos.y--;
						}
						break;
					}
					pZero += dir * 60f;
					break;
				}
			}
		}
		else
		{
			playerSpawnPos = new IntVector2(20, 13);
		}
		perp = Custom.PerpendicularVector(dir);
		segmentPairs = new float[5, 3];
		for (int num2 = 0; num2 < 5; num2++)
		{
			segmentPairs[num2, 0] = closedFac;
			segmentPairs[num2, 1] = closedFac;
		}
		pistons = new float[2, 3];
		for (int num3 = 0; num3 < 2; num3++)
		{
			pistons[num3, 0] = closedFac;
			pistons[num3, 1] = closedFac;
		}
		covers = new float[4, 3];
		for (int num4 = 0; num4 < 4; num4++)
		{
			covers[num4, 0] = closedFac;
			covers[num4, 1] = closedFac;
		}
		pumps = new float[8, 3];
		for (int num5 = 0; num5 < 8; num5++)
		{
			pumps[num5, 0] = closedFac;
			pumps[num5, 1] = closedFac;
		}
		workingLoop = new StaticSoundLoop(SoundID.Shelter_Working_Background_Loop, pZero, room, 0f, 1f);
		gasketMoverLoop = new StaticSoundLoop(SoundID.Shelter_Gasket_Mover_LOOP, pZero, room, 0f, 1f);
		Reset();
		if (Broken)
		{
			for (int num6 = 0; num6 < room.roomSettings.placedObjects.Count; num6++)
			{
				if (room.roomSettings.placedObjects[num6].type == PlacedObject.Type.BrokenShelterWaterLevel)
				{
					room.AddWater();
					room.waterObject.fWaterLevel = room.roomSettings.placedObjects[num6].pos.y;
					room.waterObject.originalWaterLevel = room.roomSettings.placedObjects[num6].pos.y;
					break;
				}
			}
		}
		for (int num7 = 0; num7 < room.abstractRoom.creatures.Count; num7++)
		{
			if (room.abstractRoom.creatures[num7].creatureTemplate.type == CreatureTemplate.Type.Slugcat)
			{
				killHostiles = true;
			}
		}
		if (isAncient)
		{
			ancientDoors = new Door[2];
			ancientDoors[0] = new Door(this, 0);
			ancientDoors[0].closedFac = closedFac;
			ChangeAncientDoorsStatus(0, closedFac < 1f);
			ancientDoors[1] = new Door(this, 1);
			ancientDoors[1].closedFac = closedFac;
			ChangeAncientDoorsStatus(1, closedFac < 1f);
			doorGraphs = new DoorGraphic[2];
			for (int num8 = 0; num8 < 2; num8++)
			{
				doorGraphs[num8] = new DoorGraphic(this, ancientDoors[num8]);
			}
		}
		if (!room.game.IsStorySession || Broken)
		{
			return;
		}
		checkForSpawnGhostSlugcat = room.game.StoryCharacter == SlugcatStats.Name.Yellow || (ModManager.MSC && room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer);
		bool flag2 = !ModManager.MSC || !room.game.rainWorld.safariMode;
		bool flag3 = !ModManager.MMF || MMF.cfgExtraTutorials.Value;
		if (!ModManager.Expedition || !room.game.rainWorld.ExpeditionMode)
		{
			if (flag2 && flag3 && room.game.rainWorld.progression.miscProgressionData.starvationTutorialCounter > 6 && room.game.GetStorySession.Players[0].pos.room == room.abstractRoom.index && room.game.GetStorySession.saveState.food > 0 && room.game.GetStorySession.saveState.food < room.game.session.characterStats.foodToHibernate && room.game.GetStorySession.saveState.GetSaveStateDenToUse() == room.abstractRoom.name && (room.game.GetStorySession.saveState.saveStateNumber == SlugcatStats.Name.Yellow || room.game.GetStorySession.saveState.saveStateNumber == SlugcatStats.Name.White))
			{
				room.AddObject(new StarvationTutorial(room));
			}
			if (ModManager.MSC && flag2 && room.game.rainWorld.progression.miscProgressionData.starvationTutorialCounter > 2 && room.game.GetStorySession.Players[0].pos.room == room.abstractRoom.index && room.game.GetStorySession.saveState.GetSaveStateDenToUse() == room.abstractRoom.name && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				room.AddObject(new HypothermiaTutorial(room));
			}
			if (ModManager.MMF && MMF.cfgExtraTutorials.Value && flag2 && flag3 && room.game.rainWorld.progression.miscProgressionData.returnExplorationTutorialCounter == 0 && room.game.GetStorySession.saveState.denPosition == room.abstractRoom.name && MMF.cfgExtraTutorials.Value && (room.game.GetStorySession.saveState.saveStateNumber == SlugcatStats.Name.Yellow || room.game.GetStorySession.saveState.saveStateNumber == SlugcatStats.Name.White))
			{
				room.game.rainWorld.progression.miscProgressionData.returnExplorationTutorialCounter = -1;
				room.AddObject(new ReturnExplorationTutorial(room));
			}
			if (ModManager.MSC && flag2 && room.game.rainWorld.progression.miscProgressionData.starvationTutorialCounter > 1 && room.game.GetStorySession.saveState.denPosition == room.abstractRoom.name && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				room.AddObject(new MaulingTutorial(room));
			}
			if (ModManager.MSC && flag2 && room.game.rainWorld.progression.miscProgressionData.starvationTutorialCounter > 1 && room.game.GetStorySession.saveState.denPosition == room.abstractRoom.name && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				room.AddObject(new GourmandCombatTutorial(room));
			}
		}
	}

	public void Close()
	{
		if (Broken)
		{
			return;
		}
		if (ModManager.CoopAvailable)
		{
			List<AbstractCreature> playersToProgressOrWin = room.game.PlayersToProgressOrWin;
			List<AbstractCreature> list = (from x in room.physicalObjects.SelectMany((List<PhysicalObject> x) => x).OfType<Player>()
				select x.abstractCreature).ToList();
			bool flag = true;
			bool flag2 = false;
			bool flag3 = false;
			foreach (AbstractCreature item in playersToProgressOrWin)
			{
				if (!list.Contains(item))
				{
					int playerNumber = (item.state as PlayerState).playerNumber;
					flag3 = true;
					flag = false;
					flag2 = false;
					if (room.BeingViewed)
					{
						try
						{
							room.game.cameras[0].hud.jollyMeter.playerIcons[playerNumber].blinkRed = 20;
						}
						catch
						{
						}
					}
				}
				if (flag3)
				{
					foreach (Player item2 in list.Select((AbstractCreature x) => x.realizedCreature as Player))
					{
						item2.forceSleepCounter = 0;
						item2.sleepCounter = 0;
						item2.touchedNoInputCounter = 0;
					}
				}
				if (!item.state.dead)
				{
					Player obj2 = item.realizedCreature as Player;
					if (!obj2.ReadyForWinJolly)
					{
						flag = false;
					}
					if (obj2.ReadyForStarveJolly)
					{
						flag2 = true;
					}
				}
			}
			if (!(flag || flag2))
			{
				return;
			}
		}
		closeSpeed = 0.003125f;
		if (!room.game.rainWorld.progression.miscProgressionData.regionsVisited.ContainsKey(room.world.name))
		{
			room.game.rainWorld.progression.miscProgressionData.regionsVisited.Add(room.world.name, new List<string>());
		}
		if (!room.game.rainWorld.progression.miscProgressionData.regionsVisited[room.world.name].Contains(room.game.GetStorySession.saveStateNumber.value))
		{
			room.game.rainWorld.progression.miscProgressionData.regionsVisited[room.world.name].Add(room.game.GetStorySession.saveStateNumber.value);
		}
	}

	private void Reset()
	{
		for (int i = 0; i < 5; i++)
		{
			segmentPairs[i, 2] = UnityEngine.Random.value * 0.5f;
		}
		for (int j = 0; j < 2; j++)
		{
			pistons[j, 2] = UnityEngine.Random.value * 0.8f;
		}
		for (int k = 0; k < 4; k++)
		{
			covers[k, 2] = UnityEngine.Random.value * 0.5f;
		}
		for (int l = 0; l < 8; l++)
		{
			pumps[l, 2] = UnityEngine.Random.value * 0.5f;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (ModManager.MSC && base.room.game.globalRain.drainWorldFlood > 0f && !base.room.game.globalRain.drainWorldFloodFlag && base.room.world.GetAbstractRoom(base.room.world.game.startingRoom) == base.room.abstractRoom && base.room.game.Players.Count > 0)
		{
			Room room = null;
			WorldCoordinate worldCoordinate = default(WorldCoordinate);
			AbstractCreature firstAlivePlayer = base.room.game.FirstAlivePlayer;
			if (firstAlivePlayer != null && firstAlivePlayer.Room != null && firstAlivePlayer.Room != base.room.abstractRoom && firstAlivePlayer.Room.realizedRoom != null)
			{
				room = firstAlivePlayer.Room.realizedRoom;
				worldCoordinate = firstAlivePlayer.pos;
			}
			if (room != null && base.room.game.Players.Count > 0)
			{
				Custom.Log("Jumped drainworld flooding!");
				base.room.game.globalRain.drainWorldFloodFlag = true;
				ShortcutData shortcutData = room.ShortcutLeadingToNode(worldCoordinate.abstractNode);
				base.room.game.globalRain.DrainWorldFloodInit(shortcutData.startCoord);
				base.room.game.globalRain.drainWorldFlood -= base.room.world.rainCycle.timer / 5;
				if ((room.roomSettings.DangerType == RoomRain.DangerType.Flood || room.roomSettings.DangerType == RoomRain.DangerType.FloodAndRain) && room.waterObject == null)
				{
					room.AddWater();
					base.room.waterObject.fWaterLevel = base.room.game.Players[0].Room.realizedRoom.roomRain.FloodLevel;
				}
			}
		}
		if (checkForSpawnGhostSlugcat && base.room.game.Players.Count > 0)
		{
			bool beingViewed = base.room.BeingViewed;
			if (beingViewed && !lastViewed)
			{
				Player player = null;
				if (!ModManager.CoopAvailable)
				{
					player = base.room.game.Players[0].realizedCreature as Player;
				}
				else
				{
					AbstractCreature firstAlivePlayer2 = base.room.game.FirstAlivePlayer;
					player = ((base.room.game.AlivePlayers.Count <= 0 || firstAlivePlayer2 == null || firstAlivePlayer2.realizedCreature == null) ? (base.room.game.FirstAnyPlayer.realizedCreature as Player) : (firstAlivePlayer2.realizedCreature as Player));
				}
				if (player != null && !player.dead && !player.Sleeping && !player.stillInStartShelter && player.FoodInRoom(base.room, eatAndDestroy: false) >= player.slugcatStats.foodToHibernate && base.room.game.GetStorySession.saveState.dreamsState != null && base.room.game.GetStorySession.saveState.dreamsState.IfSleepNowIsThereADreamComingUp(base.room.game.GetStorySession.saveState, base.room.world.region.name, base.room.abstractRoom.name) && !base.room.game.GetStorySession.saveState.dreamsState.hasShownDreamSlugcat)
				{
					base.room.AddObject(new SlugcatGhost(base.room.MiddleOfTile(playerSpawnPos), base.room));
					checkForSpawnGhostSlugcat = false;
					base.room.game.GetStorySession.saveState.dreamsState.hasShownDreamSlugcat = true;
				}
			}
			lastViewed = beingViewed;
		}
		if (retrieveStuffStuckInWalls && base.room.readyForAI)
		{
			GetStuffInWalls();
			retrieveStuffStuckInWalls = false;
		}
		if (Closed == 0f)
		{
			openTime++;
		}
		else
		{
			openTime = 0;
		}
		if (openTime < 120)
		{
			for (int i = 0; i < base.room.abstractRoom.creatures.Count; i++)
			{
				if (!IsTileInsideShelterRange(base.room.abstractRoom, base.room.abstractRoom.creatures[i].pos.Tile))
				{
					continue;
				}
				bool flag = false;
				if (base.room.abstractRoom.creatures[i].state.socialMemory != null)
				{
					for (int j = 0; j < base.room.game.Players.Count; j++)
					{
						if (ModManager.MSC && base.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer && (base.room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.Scavenger || base.room.abstractRoom.creatures[i].creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite))
						{
							flag = false;
							continue;
						}
						SocialMemory.Relationship orInitiateRelationship = base.room.abstractRoom.creatures[i].state.socialMemory.GetOrInitiateRelationship(base.room.game.Players[j].ID);
						if (orInitiateRelationship.like >= 0.5f || orInitiateRelationship.tempLike > 0.5f)
						{
							flag = true;
						}
					}
				}
				if (!flag && base.room.abstractRoom.creatures[i].realizedCreature != null && base.room.abstractRoom.creatures[i].creatureTemplate.type != CreatureTemplate.Type.Slugcat && (!ModManager.MSC || base.room.abstractRoom.creatures[i].creatureTemplate.type != MoreSlugcatsEnums.CreatureTemplateType.SlugNPC))
				{
					base.room.abstractRoom.creatures[i].realizedCreature.stun = Math.Max(base.room.abstractRoom.creatures[i].realizedCreature.stun, 20);
				}
			}
		}
		if (aiMapper != null)
		{
			if (aiMapper.done)
			{
				if (base.room.readyForAI)
				{
					base.room.aimap.map = aiMapper.ReturnAIMap().map;
					UpdatePathfindingCreatures();
					aiMapper = null;
				}
			}
			else
			{
				for (int k = 0; k < 100; k++)
				{
					aiMapper.Update();
				}
			}
		}
		float num = closedFac;
		float num2 = rainCycle.maxPreTimer - rainCycle.preTimer;
		if (ModManager.MSC && rainCycle.maxPreTimer > 0 && num2 < openUpTicks + initialWait && base.room.game.setupValues.cycleStartUp)
		{
			closedFac = Mathf.InverseLerp(initialWait + openUpTicks, initialWait, num2);
		}
		else if ((!ModManager.MSC || rainCycle.maxPreTimer == 0) && (float)rainCycle.timer < openUpTicks + initialWait && base.room.game.setupValues.cycleStartUp)
		{
			closedFac = Mathf.InverseLerp(initialWait + openUpTicks, initialWait, rainCycle.timer);
		}
		else
		{
			closedFac = Mathf.Clamp(closedFac + closeSpeed, 0f, 1f);
			if (closedFac == 1f && closeSpeed > 0f)
			{
				DoorClosed();
			}
		}
		if (closeSpeed < 0f && num > 0f && closedFac == 0f)
		{
			aiMapper = new AImapper(base.room);
		}
		float num3 = Mathf.InverseLerp(0.53f, 0.61f, num);
		if (closeSpeed < 0f && Custom.Decimal(num3 * 4f) < Custom.Decimal(Cylinders * 4f))
		{
			base.room.PlaySound(SoundID.Shelter_Bolt_Open, pZero, 1f * QuieterSoundFactor, 1f);
		}
		else if (closeSpeed > 0f && Custom.Decimal(num3 * 4f) > Custom.Decimal(Cylinders * 4f))
		{
			base.room.PlaySound(SoundID.Shelter_Bolt_Close, pZero, 1f * QuieterSoundFactor, 1f);
		}
		if (closeSpeed > 0f && closedFac > 0.87f && num <= 0.87f)
		{
			for (int l = 0; l < base.room.game.Players.Count; l++)
			{
				if (base.room.game.Players[l].realizedCreature != null && (base.room.game.Players[l].realizedCreature as Player).FoodInRoom(base.room, eatAndDestroy: false) >= (base.room.game.Players[l].realizedCreature as Player).slugcatStats.foodToHibernate)
				{
					(base.room.game.Players[l].realizedCreature as Player).sleepWhenStill = true;
				}
			}
		}
		if (closeSpeed > 0f && closedFac > 0.4f && num <= 0.4f)
		{
			for (int m = 0; m < base.room.game.Players.Count; m++)
			{
				if (base.room.game.Players[m].realizedCreature != null && (base.room.game.Players[m].realizedCreature as Player).FoodInRoom(base.room, eatAndDestroy: false) < ((!base.room.game.GetStorySession.saveState.malnourished) ? 1 : (base.room.game.Players[m].realizedCreature as Player).slugcatStats.maxFood) && (!ModManager.CoopAvailable || (!(base.room.game.Players[m].realizedCreature as Player).playerState.permaDead && base.room.game.Players[m].Room == base.room.abstractRoom)))
				{
					int num4 = (base.room.game.Players[m].realizedCreature as Player).FoodInRoom(base.room, eatAndDestroy: false);
					int num5 = ((!base.room.game.GetStorySession.saveState.malnourished) ? 1 : (base.room.game.Players[m].realizedCreature as Player).slugcatStats.maxFood);
					JollyCustom.Log($"Starving! {num4}/{num5}. FoodInStomach: {(base.room.game.Players[m].realizedCreature as Player).FoodInStomach}");
					base.room.game.GoToStarveScreen();
				}
			}
		}
		workingLoop.Update();
		if (Closed > 0.1f && Closed < 1f)
		{
			workingLoop.volume = Mathf.Lerp(workingLoop.volume, 1f, 0.1f) * QuieterSoundFactorMin;
		}
		else
		{
			workingLoop.volume = Mathf.Max(0f, workingLoop.volume - 0.05f) * QuieterSoundFactorMin;
		}
		if (num >= 0.04f && closedFac < 0.04f)
		{
			base.room.PlaySound(SoundID.Shelter_Little_Hatch_Open, pZero, 1f * QuieterSoundFactor, 1f);
			if (ModManager.MSC && base.room.game.IsStorySession && base.room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint && base.room.world.region != null && World.CheckForRegionGhost(MoreSlugcatsEnums.SlugcatStatsName.Saint, base.room.world.region.name))
			{
				GhostWorldPresence.GhostID ghostID = GhostWorldPresence.GetGhostID(base.room.world.region.name);
				if (!(base.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo.ContainsKey(ghostID) || (base.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo[ghostID] < 2)
				{
					base.room.AddObject(new GhostPing(base.room));
				}
			}
		}
		else if (num < 0.04f && closedFac >= 0.04f)
		{
			base.room.PlaySound(SoundID.Shelter_Little_Hatch_Open, pZero, 1f * QuieterSoundFactor, 1f);
		}
		for (int n = 0; n < 5; n++)
		{
			segmentPairs[n, 1] = segmentPairs[n, 0];
			segmentPairs[n, 0] = Mathf.InverseLerp(segmentPairs[n, 2], segmentPairs[n, 2] + 0.5f, Segments);
			if (segmentPairs[n, 0] == 1f && segmentPairs[n, 1] < 1f)
			{
				base.room.ScreenMovement(pZero, dir * 0f, 0.1f);
				base.room.PlaySound(SoundID.Shelter_Segment_Pair_Collide, pZero, 1f * QuieterSoundFactor, 1f);
			}
			if (segmentPairs[n, 1] <= 0.1f && segmentPairs[n, 0] > 0.1f)
			{
				base.room.PlaySound(SoundID.Shelter_Segment_Pair_Move_In, pZero, 1f * QuieterSoundFactor, 1f);
			}
			else if (segmentPairs[n, 1] == 1f && segmentPairs[n, 0] < 1f)
			{
				base.room.PlaySound(SoundID.Shelter_Segment_Pair_Move_Out, pZero, 1f * QuieterSoundFactor, 1f);
			}
		}
		for (int num6 = 0; num6 < 2; num6++)
		{
			pistons[num6, 1] = pistons[num6, 0];
			pistons[num6, 0] = Mathf.InverseLerp(pistons[num6, 2], 1f, Mathf.Max(Pistons, PistonsClosed));
			if (pistons[num6, 0] >= 0.95f && pistons[num6, 1] < 0.95f)
			{
				if (PistonsClosed < 0.5f)
				{
					base.room.ScreenMovement(pZero, dir * 3f, 0f);
					base.room.PlaySound(SoundID.Shelter_Piston_In_Hard, pZero, 1f * QuieterSoundFactor, 1f);
				}
				else
				{
					base.room.PlaySound(SoundID.Shelter_Piston_In_Soft, pZero, 1f * QuieterSoundFactor, 1f);
				}
			}
			else if (pistons[num6, 0] < 1f && pistons[num6, 1] == 1f)
			{
				base.room.PlaySound(SoundID.Shelter_Piston_Out, pZero, 1f * QuieterSoundFactor, 1f);
			}
		}
		for (int num7 = 0; num7 < 4; num7++)
		{
			covers[num7, 1] = covers[num7, 0];
			covers[num7, 0] = Mathf.InverseLerp(covers[num7, 2], covers[num7, 2] + 0.5f, Covers);
			if (covers[num7, 1] == 1f && covers[num7, 0] < 1f)
			{
				base.room.PlaySound(SoundID.Shelter_Protective_Cover_Move_Out, pZero, 1f * QuieterSoundFactor, 1f);
			}
			else if (covers[num7, 1] == 0f && covers[num7, 0] > 0f)
			{
				base.room.PlaySound(SoundID.Shelter_Protective_Cover_Move_In, pZero, 1f * QuieterSoundFactor, 1f);
			}
			else if (covers[num7, 1] < 1f && covers[num7, 0] == 1f)
			{
				base.room.PlaySound(SoundID.Shelter_Protective_Cover_Click_Into_Place, pZero, 1f * QuieterSoundFactor, 1f);
			}
		}
		for (int num8 = 0; num8 < 8; num8++)
		{
			pumps[num8, 1] = pumps[num8, 0];
			pumps[num8, 0] = Mathf.InverseLerp(pumps[num8, 2], 1f, PumpsEnter);
		}
		if (pumps[0, 0] == 1f && pumps[0, 1] < 1f)
		{
			base.room.PlaySound(SoundID.Shelter_Gaskets_Seal, pZero, 1f * QuieterSoundFactor, 1f);
		}
		else if (pumps[0, 0] < 1f && pumps[0, 1] == 1f)
		{
			base.room.PlaySound(SoundID.Shelter_Gaskets_Unseal, pZero, 1f * QuieterSoundFactor, 1f);
		}
		gasketMoverLoop.Update();
		if ((PumpsEnter > 0f && PumpsEnter < 1f) || (PumpsExit > 0f && PumpsExit < 1f))
		{
			gasketMoverLoop.volume = Mathf.Lerp(gasketMoverLoop.volume, 1f, 0.5f) * QuieterSoundFactor;
			if (PumpsEnter > 0f && PumpsEnter < 1f)
			{
				gasketMoverLoop.pitch = Mathf.Lerp(gasketMoverLoop.pitch, 1.2f, 0.2f);
			}
			else
			{
				gasketMoverLoop.pitch = Mathf.Lerp(gasketMoverLoop.pitch, 0.8f, 0.2f);
			}
		}
		else
		{
			gasketMoverLoop.volume = Mathf.Max(0f, gasketMoverLoop.volume - 0.05f) * QuieterSoundFactor;
		}
		if (PumpsEnter > 0f && PumpsExit < 1f)
		{
			base.room.ScreenMovement(pZero, dir * 0f, 0.1f);
		}
		bool flag2 = Closed > 0f;
		if (!isAncient)
		{
			if (flag2 != lastClosed)
			{
				Reset();
				for (int num9 = 0; num9 < closeTiles.Length; num9++)
				{
					base.room.GetTile(closeTiles[num9]).Terrain = (flag2 ? Room.Tile.TerrainType.Solid : Room.Tile.TerrainType.Air);
				}
			}
		}
		else
		{
			ancientDoors[0].Update();
			ancientDoors[1].Update();
			doorGraphs[0].Update();
			doorGraphs[1].Update();
		}
		lastClosed = flag2;
		if (killHostiles)
		{
			KillAllHostiles();
		}
	}

	private void DoorClosed()
	{
		bool flag = true;
		if (ModManager.CoopAvailable)
		{
			List<PhysicalObject> list = (from x in room.physicalObjects.SelectMany((List<PhysicalObject> x) => x)
				where x is Player
				select x).ToList();
			int num = list.Count();
			int num2 = 0;
			int y = SlugcatStats.SlugcatFoodMeter(room.game.StoryCharacter).y;
			flag = num >= room.game.PlayersToProgressOrWin.Count;
			JollyCustom.Log("Player(s) in shelter: " + num + " Survived: " + flag);
			if (flag)
			{
				foreach (PhysicalObject item in list)
				{
					num2 = Math.Max((item as Player).FoodInRoom(room, eatAndDestroy: false), num2);
				}
				JollyCustom.Log("Survived!, food in room " + num2);
				foreach (AbstractCreature player in room.game.Players)
				{
					if (player.Room != room.abstractRoom)
					{
						try
						{
							JollyCustom.WarpAndRevivePlayer(player, room.abstractRoom, room.LocalCoordinateOfNode(0));
						}
						catch (Exception arg)
						{
							JollyCustom.Log($"Could not warp and revive player {player} [{arg}]");
						}
					}
				}
				room.game.Win(num2 < y);
			}
			else
			{
				room.game.GoToDeathScreen();
			}
			return;
		}
		for (int i = 0; i < room.game.Players.Count; i++)
		{
			if (!room.game.Players[i].state.alive)
			{
				flag = false;
			}
		}
		if (flag)
		{
			room.game.Win((room.game.Players[0].realizedCreature as Player).FoodInRoom(room, eatAndDestroy: false) < (room.game.Players[0].realizedCreature as Player).slugcatStats.foodToHibernate);
		}
		else
		{
			room.game.GoToDeathScreen();
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		if (isAncient)
		{
			sLeaser.sprites = new FSprite[doorGraphs[0].TotalSprites + doorGraphs[1].TotalSprites];
			for (int i = 0; i < 2; i++)
			{
				doorGraphs[i].InitiateSprites(sLeaser, rCam);
			}
			AddToContainer(sLeaser, rCam, null);
			return;
		}
		sLeaser.sprites = new FSprite[42];
		for (int j = 0; j < 4; j++)
		{
			sLeaser.sprites[CogSprite(j)] = new FSprite("ShelterGate_cog");
			sLeaser.sprites[CogSprite(j)].alpha = 1f - ((j < 2) ? 6f : 5f) / 30f;
		}
		for (int k = 0; k < 2; k++)
		{
			sLeaser.sprites[PistonSprite(k)] = new FSprite("ShelterGate_piston" + (k + 1));
			sLeaser.sprites[PistonSprite(k)].alpha = 0.9f;
		}
		for (int l = 0; l < 8; l++)
		{
			sLeaser.sprites[PlugSprite(l)] = new FSprite("ShelterGate_plug" + (l + 1));
		}
		for (int m = 0; m < 10; m++)
		{
			sLeaser.sprites[SegmentSprite(m)] = new FSprite("ShelterGate_segment" + (m + 1));
		}
		for (int n = 0; n < 4; n++)
		{
			sLeaser.sprites[CylinderSprite(n)] = new FSprite("ShelterGate_cylinder" + (n + 1));
		}
		for (int num = 0; num < 4; num++)
		{
			sLeaser.sprites[CoverSprite(num)] = new FSprite("ShelterGate_cover" + (num + 1));
		}
		for (int num2 = 0; num2 < 8; num2++)
		{
			sLeaser.sprites[PumpSprite(num2)] = new FSprite("ShelterGate_pump" + (num2 + 1));
		}
		for (int num3 = 0; num3 < 2; num3++)
		{
			sLeaser.sprites[FlapSprite(num3)] = new FSprite("ShelterGate_Hatch");
			sLeaser.sprites[FlapSprite(num3)].anchorX = 0.2f;
			sLeaser.sprites[FlapSprite(num3)].anchorY = 0.43f;
			if (num3 == 1)
			{
				sLeaser.sprites[FlapSprite(num3)].scaleX = -1f;
			}
		}
		float rotation = Custom.AimFromOneVectorToAnother(dir, new Vector2(0f, 0f));
		for (int num4 = 0; num4 < sLeaser.sprites.Length; num4++)
		{
			sLeaser.sprites[num4].rotation = rotation;
			sLeaser.sprites[num4].shader = room.game.rainWorld.Shaders["ColoredSprite2"];
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		camPos.x += 0.25f;
		camPos.y += 0.25f;
		if (isAncient)
		{
			for (int i = 0; i < 2; i++)
			{
				doorGraphs[i].DrawSprites(sLeaser, rCam, timeStacker, camPos);
			}
			return;
		}
		for (int j = 0; j < 4; j++)
		{
			Vector2 vector = perp * ((j % 2 == 0) ? (-1f) : 1f) * ((j >= 2) ? 35f : 55f);
			vector -= dir * ((j >= 2) ? 50f : 15f);
			sLeaser.sprites[CogSprite(j)].x = pZero.x - camPos.x + vector.x;
			sLeaser.sprites[CogSprite(j)].y = pZero.y - camPos.y + vector.y;
			sLeaser.sprites[CogSprite(j)].rotation = Closed * ((j >= 2) ? 400f : (-150f)) * ((j % 2 == 0) ? (-1f) : 1f);
		}
		for (int k = 0; k < 10; k++)
		{
			int num = k / 2;
			Vector2 vector2 = perp * Mathf.Pow(1f - Mathf.Lerp(segmentPairs[num, 1], segmentPairs[num, 0], timeStacker), 0.75f) * 55f * ((k % 2 == 0) ? (-1f) : 1f);
			sLeaser.sprites[SegmentSprite(k)].x = pZero.x - camPos.x + vector2.x;
			sLeaser.sprites[SegmentSprite(k)].y = pZero.y - camPos.y + vector2.y;
			sLeaser.sprites[SegmentSprite(k)].alpha = 1f - 4f * Mathf.InverseLerp(0.78f, 0.61f, Closed) / 30f;
		}
		for (int l = 0; l < 2; l++)
		{
			Vector2 vector3 = dir * (1f - Mathf.Lerp(pistons[l, 1], pistons[l, 0], timeStacker)) * -120f;
			if (PistonsClosed > 0f)
			{
				vector3 += perp * 5f * ((l == 0) ? (-1f) : 1f);
			}
			sLeaser.sprites[PistonSprite(l)].x = pZero.x - camPos.x + vector3.x;
			sLeaser.sprites[PistonSprite(l)].y = pZero.y - camPos.y + vector3.y;
		}
		for (int m = 0; m < 4; m++)
		{
			Vector2 vector4 = perp * Mathf.Pow(1f - Mathf.Lerp(covers[m, 1], covers[m, 0], timeStacker), 2.5f) * 65f * ((m >= 2) ? (-1f) : 1f);
			sLeaser.sprites[CoverSprite(m)].x = pZero.x - camPos.x + vector4.x;
			sLeaser.sprites[CoverSprite(m)].y = pZero.y - camPos.y + vector4.y;
		}
		for (int n = 0; n < 4; n++)
		{
			if ((float)n / 4f < Cylinders)
			{
				sLeaser.sprites[CylinderSprite(n)].x = pZero.x - camPos.x;
				sLeaser.sprites[CylinderSprite(n)].y = pZero.y - camPos.y;
				sLeaser.sprites[CylinderSprite(n)].isVisible = true;
			}
			else
			{
				sLeaser.sprites[CylinderSprite(n)].isVisible = false;
			}
		}
		for (int num2 = 0; num2 < 8; num2++)
		{
			Vector2 vector5 = perp * Mathf.Lerp(pumps[num2, 1], pumps[num2, 0], timeStacker) * -42f * ((num2 % 2 == 0) ? (-1f) : 1f);
			vector5 += -dir * PumpsExit * 80f;
			sLeaser.sprites[PumpSprite(num2)].x = pZero.x - camPos.x + vector5.x;
			sLeaser.sprites[PumpSprite(num2)].y = pZero.y - camPos.y + vector5.y;
			vector5 = perp * Mathf.Clamp(1f - Mathf.Lerp(pumps[num2, 1], pumps[num2, 0], timeStacker) - 0.35f, 0f, 1f) * 60f * ((num2 % 2 == 0) ? (-1f) : 1f);
			sLeaser.sprites[PlugSprite(num2)].x = pZero.x - camPos.x + vector5.x;
			sLeaser.sprites[PlugSprite(num2)].y = pZero.y - camPos.y + vector5.y;
		}
		for (int num3 = 0; num3 < 2; num3++)
		{
			Vector2 vector6 = pZero + dir * 46f + perp * Mathf.Lerp(15f, 25f, FlapsOpen) * ((num3 == 0) ? 1f : (-1f));
			sLeaser.sprites[FlapSprite(num3)].x = vector6.x - camPos.x;
			sLeaser.sprites[FlapSprite(num3)].y = vector6.y - camPos.y;
			sLeaser.sprites[FlapSprite(num3)].rotation = Custom.AimFromOneVectorToAnother(-dir, dir) - 90f * ((num3 == 0) ? (-1f) : 1f) * FlapsOpen;
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		if (isAncient)
		{
			for (int i = 0; i < 2; i++)
			{
				doorGraphs[i].ApplyPalette(sLeaser, rCam, palette);
			}
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = ((!isAncient) ? rCam.ReturnFContainer("Items") : rCam.ReturnFContainer("Midground"));
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}

	private void UpdatePathfindingCreatures()
	{
		for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
		{
			if (room.abstractRoom.creatures[i].abstractAI != null && room.abstractRoom.creatures[i].abstractAI.RealAI != null && room.abstractRoom.creatures[i].abstractAI.RealAI.pathFinder != null)
			{
				room.abstractRoom.creatures[i].abstractAI.RealAI.pathFinder.Reset(room);
			}
		}
	}

	private void KillAllHostiles()
	{
		bool flag = false;
		for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
		{
			if (!IsThisHostileCreatureForShelter(room.abstractRoom.creatures[i]) || !IsTileInsideShelterRange(room.abstractRoom, room.abstractRoom.creatures[i].pos.Tile))
			{
				continue;
			}
			if (room.abstractRoom.creatures[i].state.alive)
			{
				flag = true;
			}
			if (room.abstractRoom.creatures[i].realizedCreature != null)
			{
				room.abstractRoom.creatures[i].state.Die();
				room.abstractRoom.creatures[i].realizedCreature.Die();
			}
			if (IsThisBigCreatureForShelter(room.abstractRoom.creatures[i]))
			{
				Custom.Log($"Removed giant creature {room.abstractRoom.creatures[i]} from shelter");
				if (!room.world.singleRoomWorld && room.world.GetSpawner(room.abstractRoom.creatures[i].ID) != null)
				{
					WorldCoordinate den = room.world.GetSpawner(room.abstractRoom.creatures[i].ID).den;
					room.abstractRoom.creatures[i].Move(den);
				}
				if (room.abstractRoom.creatures[i].realizedCreature != null)
				{
					room.abstractRoom.creatures[i].realizedCreature.Destroy();
				}
			}
		}
		if (!flag)
		{
			killHostiles = false;
			Custom.Log("killed all hostiles in shelter");
		}
	}

	private static bool IsThisHostileCreatureForShelter(AbstractCreature creature)
	{
		CreatureTemplate.Type type = creature.creatureTemplate.type;
		if (ModManager.MSC)
		{
			if (type == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs)
			{
				return true;
			}
			if (type == MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy)
			{
				return true;
			}
			if (type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture)
			{
				return true;
			}
		}
		if (type == CreatureTemplate.Type.Deer)
		{
			return true;
		}
		if (type == CreatureTemplate.Type.BigNeedleWorm)
		{
			return true;
		}
		if (type == CreatureTemplate.Type.RedCentipede)
		{
			return true;
		}
		if (type == CreatureTemplate.Type.Centipede)
		{
			return true;
		}
		if (type == CreatureTemplate.Type.Centiwing)
		{
			return true;
		}
		if (type == CreatureTemplate.Type.BigSpider)
		{
			return true;
		}
		if (type == CreatureTemplate.Type.BrotherLongLegs)
		{
			return true;
		}
		if (type == CreatureTemplate.Type.DaddyLongLegs)
		{
			return true;
		}
		if (type == CreatureTemplate.Type.MirosBird)
		{
			return true;
		}
		if (type == CreatureTemplate.Type.PoleMimic)
		{
			return true;
		}
		if (type == CreatureTemplate.Type.TentaclePlant)
		{
			return true;
		}
		if (creature.creatureTemplate.IsVulture)
		{
			return true;
		}
		if (type == CreatureTemplate.Type.Spider)
		{
			return true;
		}
		if (type == CreatureTemplate.Type.SpitterSpider)
		{
			return true;
		}
		if (type == CreatureTemplate.Type.DropBug)
		{
			return true;
		}
		return false;
	}

	private static bool IsThisBigCreatureForShelter(AbstractCreature creature)
	{
		CreatureTemplate.Type type = creature.creatureTemplate.type;
		if (ModManager.MSC)
		{
			if (type == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs)
			{
				return true;
			}
			if (type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture)
			{
				return true;
			}
		}
		if (type == CreatureTemplate.Type.Deer)
		{
			return true;
		}
		if (type == CreatureTemplate.Type.BrotherLongLegs)
		{
			return true;
		}
		if (type == CreatureTemplate.Type.DaddyLongLegs)
		{
			return true;
		}
		if (type == CreatureTemplate.Type.RedCentipede)
		{
			return true;
		}
		if (type == CreatureTemplate.Type.MirosBird)
		{
			return true;
		}
		if (type == CreatureTemplate.Type.PoleMimic)
		{
			return true;
		}
		if (type == CreatureTemplate.Type.TentaclePlant)
		{
			return true;
		}
		if (creature.creatureTemplate.IsVulture)
		{
			return true;
		}
		return false;
	}

	private void GetStuffInWalls()
	{
		try
		{
			List<PhysicalObject> list = new List<PhysicalObject>();
			for (int i = 0; i < room.physicalObjects.Length; i++)
			{
				for (int j = 0; j < room.physicalObjects[i].Count; j++)
				{
					bool flag = true;
					for (int k = 0; k < room.physicalObjects[i][j].bodyChunks.Length; k++)
					{
						if (!room.GetTile(room.physicalObjects[i][j].bodyChunks[k].pos).Solid)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						list.Add(room.physicalObjects[i][j]);
					}
				}
			}
			if (list.Count < 1)
			{
				return;
			}
			Custom.Log("Stuff in walls:", list.Count.ToString());
			for (int l = 0; l < list.Count; l++)
			{
				IntVector2 intVector = playerSpawnPos;
				int num;
				for (num = SharedPhysics.RayTracedTilesArray(room.MiddleOfTile(intVector), list[l].firstChunk.pos, _cachedTls); num >= _cachedTls.Length; num = SharedPhysics.RayTracedTilesArray(room.MiddleOfTile(intVector), list[l].firstChunk.pos, _cachedTls))
				{
					Custom.LogWarning($"GetStuffInWalls ray tracing limit exceeded, extending cache to {_cachedTls.Length + 100} and trying again!");
					Array.Resize(ref _cachedTls, _cachedTls.Length + 100);
				}
				for (int m = 0; m < num - 1; m++)
				{
					if (!room.GetTile(_cachedTls[m]).IsSolid() && room.GetTile(_cachedTls[m + 1]).Solid)
					{
						intVector = _cachedTls[m];
						break;
					}
				}
				for (int n = 0; n < list[l].bodyChunks.Length; n++)
				{
					list[l].bodyChunks[n].HardSetPosition(room.MiddleOfTile(intVector) + Custom.RNV());
				}
				Custom.Log($"moving {list[l]} inside shelter room (from {list[l].abstractPhysicalObject.pos.Tile} to {intVector})");
			}
		}
		catch
		{
			Custom.Log("!!!! MOVE OUT OF WALL ERROR");
		}
	}

	private void DestroyExcessiveObjects()
	{
		int num = 10;
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		List<PhysicalObject> list = new List<PhysicalObject>();
		for (int i = 0; i < room.physicalObjects.Length; i++)
		{
			for (int j = 0; j < room.physicalObjects[i].Count; j++)
			{
				PhysicalObject physicalObject = room.physicalObjects[i][j];
				if (!(physicalObject is Creature) && (!ModManager.MMF || !MMF.cfgKeyItemTracking.Value || physicalObject.abstractPhysicalObject.tracker == null) && !(physicalObject is DataPearl) && !(physicalObject is OracleSwarmer) && physicalObject.grabbedBy.Count == 0 && physicalObject.abstractPhysicalObject.stuckObjects.Count == 0 && (!(physicalObject is Spear) || !(physicalObject as Spear).abstractSpear.stuckInWall))
				{
					string key = physicalObject.abstractPhysicalObject.type.value;
					if (physicalObject is Spear && (physicalObject as Spear).abstractSpear.explosive)
					{
						key = "FireSpear";
					}
					if (ModManager.MSC && physicalObject is Spear && (physicalObject as Spear).abstractSpear.electric)
					{
						key = "ElectricSpear";
					}
					if (!dictionary.ContainsKey(key))
					{
						dictionary[key] = 0;
					}
					dictionary[key]++;
					if (dictionary[key] >= num)
					{
						list.Add(physicalObject);
					}
				}
			}
		}
		foreach (PhysicalObject item in list)
		{
			item.Destroy();
		}
	}

	public static bool IsTileInsideShelterRange(AbstractRoom room, IntVector2 tile)
	{
		return CoordInsideShelterRange(tile, room.isAncientShelter);
	}

	[Obsolete]
	public static bool CoordInsideShelterRange(IntVector2 tile, bool ancient)
	{
		if (ancient)
		{
			if (tile.x > 13 && tile.x < 33 && tile.y > 9)
			{
				return tile.x < 30;
			}
			return false;
		}
		return true;
	}

	public void ChangeAncientDoorsStatus(int door, bool open)
	{
		int num = 12 + door * 21;
		for (int i = 0; i < 2; i++)
		{
			for (int j = 13; j <= 21; j++)
			{
				room.GetTile(num + i, j).Terrain = ((!open) ? Room.Tile.TerrainType.Solid : Room.Tile.TerrainType.Air);
			}
		}
		aiMapper = new AImapper(room);
	}
}
