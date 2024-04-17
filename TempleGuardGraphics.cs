using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class TempleGuardGraphics : GraphicsModule, HasDanglers
{
	public class Arm
	{
		public class Bead
		{
			public int num;

			public Vector2 pos;

			public Vector2 lastPos;

			public Vector2 vel;

			public Bead(int num)
			{
				this.num = num;
			}

			public void Update()
			{
				lastPos = pos;
				pos += vel;
				vel *= 0.99f;
			}
		}

		public TempleGuardGraphics owner;

		public int firstSprite;

		public int totalSprites;

		private GenericBodyPart bodyPart;

		public int side;

		public int height;

		public float flip;

		public float lastFlip;

		public int cut = -1;

		public Bead[] beads;

		public Vector2 handMovement;

		public int moveHand;

		public bool working;

		public Vector2 IdlePos
		{
			get
			{
				if (moveHand > 0)
				{
					return ConnPos(1f) + handMovement;
				}
				if (owner.armsPos)
				{
					return owner.guard.mainBodyChunk.pos + owner.ArmDir(1f) * (95f - (float)height * 75f) + Custom.PerpendicularVector(owner.ArmDir(1f)) * 50f * ((side == 0) ? (-1f) : 1f);
				}
				return owner.guard.mainBodyChunk.pos + owner.ArmDir(1f) * (55f - (float)height * 13f) + Custom.PerpendicularVector(owner.ArmDir(1f)) * ((height == 0) ? 0.5f : 1.5f) * ((side == 0) ? (-1f) : 1f);
			}
		}

		public Vector2 ConnPos(float timeStacker)
		{
			return Vector2.Lerp(owner.guard.mainBodyChunk.lastPos, owner.guard.mainBodyChunk.pos, timeStacker) + owner.ArmDir(timeStacker) * (57f - (float)height * 27f) + Custom.PerpendicularVector(owner.ArmDir(timeStacker)) * ((height == 0) ? 22f : 17f) * ((side == 0) ? (-1f) : 1f);
		}

		public Arm(TempleGuardGraphics owner, int firstSprite, int height, int side)
		{
			this.owner = owner;
			this.firstSprite = firstSprite;
			this.height = height;
			this.side = side;
			bodyPart = new GenericBodyPart(owner, 5f, 0.5f, 0.92f, owner.guard.mainBodyChunk);
			beads = new Bead[(height == 0) ? 28 : 24];
			for (int i = 0; i < beads.Length; i++)
			{
				beads[i] = new Bead(i);
			}
			if (UnityEngine.Random.value < 1f / 30f && beads.Length != 0)
			{
				cut = UnityEngine.Random.Range(0, beads.Length);
			}
			totalSprites = 3 + beads.Length;
		}

		public void Reset()
		{
			bodyPart.pos = ConnPos(1f);
			for (int i = 0; i < beads.Length; i++)
			{
				beads[i].pos = bodyPart.pos;
				beads[i].lastPos = beads[i].pos;
				beads[i].vel *= 0f;
			}
		}

		public void Update()
		{
			bodyPart.Update();
			bodyPart.ConnectToPoint(ConnPos(1f), 70f, push: false, 0f, owner.guard.mainBodyChunk.vel, 0.25f, 0f);
			bodyPart.vel.y -= ((height == 1) ? 0.6f : 0.9f);
			bodyPart.vel.x += 0.4f * ((side == 0) ? (-1f) : 1f);
			if (owner.guard.Consious)
			{
				Vector2 vector = Vector2.ClampMagnitude(IdlePos - bodyPart.pos, 20f) / 10f;
				if (owner.guard.AI.telekinArm.x == side && owner.guard.AI.telekinArm.y == height)
				{
					vector = Vector2.Lerp(vector, Vector2.ClampMagnitude(owner.guard.telekineticPoint + owner.guard.telekineticDir * 50f * owner.telekinesis - bodyPart.pos, 20f) / 2f, owner.telekinesis);
					bodyPart.pos += Custom.RNV() * 2f * owner.telekinesis;
				}
				bodyPart.vel += vector;
			}
			if (moveHand < 1)
			{
				if (working && owner.telekinesis < 0.5f)
				{
					moveHand = UnityEngine.Random.Range(10, 40);
					handMovement = Custom.DegToVec(Mathf.Lerp(45f, 135f, UnityEngine.Random.value) * ((side == 0) ? (-1f) : 1f)) * Mathf.Lerp(10f, 80f, UnityEngine.Random.value);
				}
			}
			else
			{
				moveHand--;
			}
			if (UnityEngine.Random.value < 0.005f)
			{
				working = !working && owner.guard.mainBodyChunk.vel.magnitude < 1f && (UnityEngine.Random.value < 0.02f || (owner.guard.AI.focusCreature == null && UnityEngine.Random.value < 0.2f));
			}
			if (UnityEngine.Random.value < 0.025f)
			{
				owner.arms[UnityEngine.Random.Range(0, 2), UnityEngine.Random.Range(0, 2)].working = working;
			}
			lastFlip = flip;
			flip = Mathf.Lerp(flip, Custom.LerpMap(Custom.DistanceToLine(bodyPart.pos, owner.guard.mainBodyChunk.pos, ConnPos(1f)), -10f, 10f, -1f, 1f), 0.1f);
			Vector2 a = new Vector2(0f, 1f - Mathf.Pow(owner.guard.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidMelt), 0.2f));
			a = ((owner.guard.AI.telekinArm.x != side || owner.guard.AI.telekinArm.y != height) ? Vector2.Lerp(a, Custom.RNV() * 0.3f, owner.telekinesis) : Vector2.Lerp(a, Custom.RNV(), owner.telekinesis));
			for (int i = 0; i < beads.Length; i++)
			{
				beads[i].vel -= a;
				beads[i].Update();
			}
			beads[beads.Length - 1].pos = bodyPart.pos;
			beads[0].pos = owner.guard.mainBodyChunk.pos + owner.guard.StoneDir * 20f;
			for (int j = 0; j < beads.Length - 1; j++)
			{
				if (j != cut)
				{
					Vector2 vector2 = beads[j + 1].pos - beads[j].pos;
					if (vector2.magnitude > 7f)
					{
						vector2 = vector2.normalized * (vector2.magnitude - 7f);
						beads[j].pos += vector2 / 2f;
						beads[j].vel += vector2 / 2f;
						beads[j + 1].pos -= vector2 / 2f;
						beads[j + 1].vel -= vector2 / 2f;
					}
				}
			}
			beads[beads.Length - 1].pos = bodyPart.pos;
			beads[0].pos = owner.guard.mainBodyChunk.pos + owner.guard.StoneDir * 20f;
			for (int num = beads.Length - 1; num > 0; num--)
			{
				if (num - 1 != cut)
				{
					Vector2 vector3 = beads[num - 1].pos - beads[num].pos;
					if (vector3.magnitude > 7f)
					{
						vector3 = vector3.normalized * (vector3.magnitude - 7f);
						beads[num].pos += vector3 / 2f;
						beads[num].vel += vector3 / 2f;
						beads[num - 1].pos -= vector3 / 2f;
						beads[num - 1].vel -= vector3 / 2f;
					}
				}
			}
			beads[beads.Length - 1].pos = bodyPart.pos;
			beads[0].pos = owner.guard.mainBodyChunk.pos + owner.guard.StoneDir * 20f;
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[firstSprite] = TriangleMesh.MakeLongMesh(6, pointyTip: false, customColor: false);
			sLeaser.sprites[firstSprite + 1] = new FSprite("guardianArm");
			sLeaser.sprites[firstSprite + 1].scaleX = ((side == 0) ? (-1f) : 1f) * 0.75f;
			sLeaser.sprites[firstSprite + 1].anchorY = 0.05f;
			sLeaser.sprites[firstSprite + 2] = new FSprite("guardianArmB");
			sLeaser.sprites[firstSprite + 2].scaleX = ((side == 0) ? (-1f) : 1f) * 0.75f;
			sLeaser.sprites[firstSprite + 2].anchorY = 0.05f;
			for (int i = 0; i < beads.Length; i++)
			{
				sLeaser.sprites[firstSprite + 3 + i] = new FSprite("haloGlyph-1");
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 headPos, Vector2 headDir)
		{
			Vector2 vector = ConnPos(timeStacker);
			Vector2 vector2 = Vector2.Lerp(bodyPart.lastPos, bodyPart.pos, timeStacker);
			float num = Mathf.Lerp(lastFlip, flip, timeStacker);
			Vector2 vector3 = Custom.InverseKinematic(vector, vector2, 35f, 35f, num);
			Vector2 vector4 = headPos + headDir * 10f;
			for (int i = 0; i < 6; i++)
			{
				float num2 = (float)i / 5f;
				float num3 = 3f;
				Vector2 vector5 = Custom.Bezier(headPos, Vector2.Lerp(headPos + headDir * 50f, vector, 0.5f), vector3, vector, num2);
				Vector2 normalized = (vector4 - vector5).normalized;
				Vector2 vector6 = Custom.PerpendicularVector(normalized);
				float num4 = Vector2.Distance(vector5, vector4) / 5f;
				if (num2 == 0f || num2 == 1f)
				{
					num4 = 0f;
				}
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4, vector4 - vector6 * num3 + normalized * num4 - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector4 + vector6 * num3 + normalized * num4 - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector5 - vector6 * num3 - normalized * num4 - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector5 + vector6 * num3 - normalized * num4 - camPos);
				vector4 = vector5;
			}
			for (int j = 1; j < 3; j++)
			{
				sLeaser.sprites[firstSprite + j].x = vector3.x - camPos.x;
				sLeaser.sprites[firstSprite + j].y = vector3.y - camPos.y;
				sLeaser.sprites[firstSprite + j].rotation = Custom.AimFromOneVectorToAnother(vector3, vector2);
				sLeaser.sprites[firstSprite + j].scaleY = (Vector2.Distance(vector3, vector) - 5f) / 38f;
			}
			for (int k = 0; k < beads.Length; k++)
			{
				sLeaser.sprites[firstSprite + 3 + k].x = Mathf.Lerp(beads[k].lastPos.x, beads[k].pos.x, timeStacker) - camPos.x;
				sLeaser.sprites[firstSprite + 3 + k].y = Mathf.Lerp(beads[k].lastPos.y, beads[k].pos.y, timeStacker) - camPos.y;
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			sLeaser.sprites[firstSprite + 2].color = Color.Lerp(palette.blackColor, new Color(1f, 1f, 1f), 0.5f);
			for (int i = 0; i < beads.Length; i++)
			{
				float num = (float)i / (float)(beads.Length - 1);
				sLeaser.sprites[firstSprite + 3 + i].color = Color.Lerp(new Color(1f, 1f, 1f), palette.blackColor, 1.5f - num);
			}
		}
	}

	public class Halo
	{
		public class GlyphSwapper
		{
			public class Cursor
			{
				private GlyphSwapper owner;

				public IntVector2 pos;

				public IntVector2 nextPos;

				private float prog;

				private float lastProg;

				private int num;

				public Cursor(GlyphSwapper owner, int num)
				{
					this.owner = owner;
					this.num = num;
					pos = RandomGlyphPos();
					nextPos = pos;
					prog = 1f;
					lastProg = 1f;
				}

				public void Update()
				{
					lastProg = prog;
					if (nextPos == pos)
					{
						if (UnityEngine.Random.value < owner.halo.Speed / 10f && owner.halo.glyphPositions[pos.x][pos.y, 0] == 1f && pos.x > 0)
						{
							owner.halo.glyphs[pos.x][pos.y] = UnityEngine.Random.Range(0, 7);
							owner.halo.dirtyGlyphs[pos.x][pos.y] = true;
						}
						if ((owner.counter == 0 && UnityEngine.Random.value < owner.halo.Speed / 40f && owner.cursors[1 - num].prog == 1f) || pos.x >= owner.halo.ringsActive)
						{
							nextPos = RandomGlyphPos();
							lastProg = 0f;
							prog = 0f;
						}
						return;
					}
					prog += 5f * Mathf.Lerp(owner.halo.Speed, 1f, 0.7f) / Mathf.Max(1f, Vector2.Distance(owner.halo.GlyphPos(pos.x, pos.y, 1f), owner.halo.GlyphPos(nextPos.x, nextPos.y, 1f)));
					if (prog >= 1f)
					{
						pos = nextPos;
						prog = 1f;
						owner.counter = (int)(Mathf.Lerp(10f, 70f, UnityEngine.Random.value) / owner.halo.Speed);
						owner.switchAt = owner.counter / 2;
						if (UnityEngine.Random.value < 0.5f && owner.halo.glyphs[pos.x][pos.y] > -1 && pos.y > 0)
						{
							owner.halo.glyphPositions[pos.x][pos.y, 0] = 1f - owner.halo.glyphPositions[pos.x][pos.y, 0];
						}
					}
				}

				public Vector2 CursorPos(float timeStacker)
				{
					Vector2 a = Vector2.Lerp(owner.halo.GlyphPos(pos.x, pos.y, timeStacker), owner.halo.GlyphPos(nextPos.x, nextPos.y, timeStacker), Mathf.Lerp(lastProg, prog, timeStacker));
					Vector2 b = Vector3.Slerp(owner.halo.GlyphPos(pos.x, pos.y, timeStacker), owner.halo.GlyphPos(nextPos.x, nextPos.y, timeStacker), Mathf.Lerp(lastProg, prog, timeStacker));
					return Vector2.Lerp(a, b, 0.5f);
				}

				private IntVector2 RandomGlyphPos()
				{
					IntVector2 result = new IntVector2(0, 0);
					result.x = UnityEngine.Random.Range(0, owner.halo.ringsActive);
					result.y = UnityEngine.Random.Range(0, owner.halo.glyphs[result.x].Length);
					return result;
				}
			}

			private Halo halo;

			public Cursor[] cursors;

			public int counter;

			public int switchAt;

			public GlyphSwapper(Halo halo)
			{
				this.halo = halo;
				cursors = new Cursor[2];
				for (int i = 0; i < cursors.Length; i++)
				{
					cursors[i] = new Cursor(this, i);
				}
			}

			public void Update()
			{
				if (counter > 0)
				{
					counter--;
				}
				if (counter == switchAt)
				{
					int num = halo.glyphs[cursors[0].pos.x][cursors[0].pos.y];
					int num2 = halo.glyphs[cursors[1].pos.x][cursors[1].pos.y];
					if (num == -1 && num2 == -1)
					{
						num = UnityEngine.Random.Range(0, 7);
						num2 = UnityEngine.Random.Range(0, 7);
					}
					else if (num == num2)
					{
						num = -1;
						num2 = -1;
					}
					else if (num == -1)
					{
						num = num2;
					}
					else if (num2 == -1)
					{
						num2 = num;
					}
					halo.glyphs[cursors[0].pos.x][cursors[0].pos.y] = num2;
					halo.glyphs[cursors[1].pos.x][cursors[1].pos.y] = num;
					halo.dirtyGlyphs[cursors[0].pos.x][cursors[0].pos.y] = true;
					halo.dirtyGlyphs[cursors[1].pos.x][cursors[1].pos.y] = true;
				}
				for (int i = 0; i < cursors.Length; i++)
				{
					cursors[i].Update();
				}
			}

			public void InitiateSprites(int frst, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
			{
				sLeaser.sprites[frst + 2] = new FSprite("pixel");
				sLeaser.sprites[frst + 2].color = RainWorld.SaturatedGold;
				sLeaser.sprites[frst + 2].anchorY = 0f;
				for (int i = 0; i < 2; i++)
				{
					sLeaser.sprites[frst + i] = new FSprite("Futile_White");
					sLeaser.sprites[frst + i].scale = 1.25f;
					sLeaser.sprites[frst + i].color = RainWorld.SaturatedGold;
					sLeaser.sprites[frst + i].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
					sLeaser.sprites[frst + i].alpha = 0.1f;
				}
			}

			public void DrawSprites(int frst, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 haloPos)
			{
				Vector2 vector = cursors[0].CursorPos(timeStacker) + haloPos;
				Vector2 vector2 = cursors[1].CursorPos(timeStacker) + haloPos;
				sLeaser.sprites[frst].x = vector.x - camPos.x;
				sLeaser.sprites[frst].y = vector.y - camPos.y;
				sLeaser.sprites[frst + 1].x = vector2.x - camPos.x;
				sLeaser.sprites[frst + 1].y = vector2.y - camPos.y;
				sLeaser.sprites[frst + 2].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
				sLeaser.sprites[frst + 2].scaleY = Vector2.Distance(vector, vector2) - 20f;
				vector += Custom.DirVec(vector, vector2) * 10f;
				sLeaser.sprites[frst + 2].x = vector.x - camPos.x;
				sLeaser.sprites[frst + 2].y = vector.y - camPos.y;
			}
		}

		private TempleGuardGraphics owner;

		public int firstSprite;

		public int totalSprites;

		public int firstSwapperSprite;

		public int firstLineSprite;

		public int firstSmallCircleSprite;

		public int[][] glyphs;

		public bool[][] dirtyGlyphs;

		public float[][,] glyphPositions;

		private GlyphSwapper[] swappers;

		public int circles = 7;

		public float[,] rotation;

		public float[,] lines;

		public float[,] smallCircles;

		private float[,] rad;

		private float savDisruption;

		public float activity;

		public float slowRingsActive;

		public float lastSlowRingsActive;

		public int ringsActive = 2;

		public Vector2 pos;

		public Vector2 lastPos;

		private bool firstUpdate = true;

		public bool deactivated;

		public List<EntityID> reactedToCritters;

		public float Speed
		{
			get
			{
				float b = 1.8f;
				if (owner.guard.AI.focusCreature != null && owner.guard.AI.FocusCreatureMovingTowardsProtectExit && owner.guard.AI.focusCreature.VisualContact && owner.guard.AI.focusCreature.representedCreature.realizedCreature != null)
				{
					b = Custom.LerpMap(Vector2.Distance(owner.guard.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.lastPos, owner.guard.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos), 1.5f, 5f, 1.2f, 3f);
				}
				return Mathf.Lerp(0.2f, b, activity);
			}
		}

		public float Circumference(float rad)
		{
			return 2f * rad * (float)Math.PI;
		}

		public float RadAtCircle(float circle, float timeStacker, float disruption)
		{
			return ((circle + 1f) * 20f + Mathf.Lerp(rad[0, 1], rad[0, 0], timeStacker) * (1f - Mathf.Lerp(owner.lastTelekin, owner.telekinesis, timeStacker))) * Mathf.Lerp(Mathf.Lerp(rad[1, 1], rad[1, 0], timeStacker), 0.7f, Mathf.Lerp(owner.lastTelekin, owner.telekinesis, timeStacker)) * Mathf.Lerp(1f, UnityEngine.Random.value * disruption, Mathf.Pow(disruption, 2f));
		}

		public float CircumferenceAtCircle(float circle, float timeStacker, float disruption)
		{
			return Circumference(RadAtCircle(circle, timeStacker, disruption));
		}

		public Halo(TempleGuardGraphics owner, int firstSprite)
		{
			this.owner = owner;
			this.firstSprite = firstSprite;
			rad = new float[2, 3];
			rad[0, 0] = 0f;
			rad[0, 1] = 0f;
			rad[0, 2] = 0f;
			rad[1, 0] = 1f;
			rad[1, 1] = 1f;
			rad[1, 2] = 1f;
			glyphs = new int[4][];
			dirtyGlyphs = new bool[glyphs.Length][];
			glyphPositions = new float[glyphs.Length][,];
			for (int i = 0; i < glyphs.Length; i++)
			{
				glyphs[i] = new int[(int)(CircumferenceAtCircle(i * 2, 1f, 0f) / 15f)];
				dirtyGlyphs[i] = new bool[glyphs[i].Length];
				glyphPositions[i] = new float[glyphs[i].Length, 3];
				for (int j = 0; j < glyphs[i].Length; j++)
				{
					glyphs[i][j] = ((UnityEngine.Random.value < 1f / 30f) ? (-1) : UnityEngine.Random.Range(0, 7));
				}
			}
			rotation = new float[circles, 2];
			for (int k = 0; k < rotation.GetLength(0); k++)
			{
				rotation[k, 0] = UnityEngine.Random.value;
				rotation[k, 1] = rotation[k, 0];
			}
			totalSprites = circles;
			for (int l = 0; l < glyphs.Length; l++)
			{
				totalSprites += glyphs[l].Length;
			}
			firstSwapperSprite = firstSprite + totalSprites;
			swappers = new GlyphSwapper[3];
			for (int m = 0; m < swappers.Length; m++)
			{
				swappers[m] = new GlyphSwapper(this);
			}
			totalSprites += swappers.Length * 3;
			firstLineSprite = totalSprites;
			lines = new float[40, 4];
			for (int n = 0; n < lines.GetLength(0); n++)
			{
				lines[n, 0] = UnityEngine.Random.value;
				lines[n, 1] = lines[n, 0];
				lines[n, 2] = UnityEngine.Random.Range(0, 3);
				lines[n, 3] = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value);
			}
			totalSprites += lines.GetLength(0);
			firstSmallCircleSprite = totalSprites;
			smallCircles = new float[10, 5];
			for (int num = 0; num < smallCircles.GetLength(0); num++)
			{
				smallCircles[num, 0] = UnityEngine.Random.value;
				smallCircles[num, 1] = smallCircles[num, 0];
				smallCircles[num, 2] = UnityEngine.Random.Range(0, UnityEngine.Random.Range(0, 6));
				smallCircles[num, 3] = UnityEngine.Random.Range((int)smallCircles[num, 2] + 1, 7);
				smallCircles[num, 4] = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value);
			}
			totalSprites += smallCircles.GetLength(0);
			reactedToCritters = new List<EntityID>();
		}

		public void ReactToCreature(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
		{
			if (Mathf.Abs(owner.guard.mainBodyChunk.pos.x - owner.guard.room.MiddleOfTile(creatureRep.BestGuessForPosition()).x) < 300f && !reactedToCritters.Contains(creatureRep.representedCreature.ID))
			{
				ringsActive = Math.Max(ringsActive, UnityEngine.Random.Range(3, 5));
				rad[0, 2] = ((UnityEngine.Random.value > activity) ? 0f : ((float)UnityEngine.Random.Range(-1, 3) * 20f));
				rad[1, 2] = ((UnityEngine.Random.value < 1f / Mathf.Lerp(1f, 5f, activity)) ? 1f : Mathf.Lerp(0.75f, 1.25f, UnityEngine.Random.value));
				reactedToCritters.Add(creatureRep.representedCreature.ID);
				for (int i = 0; i < (int)Custom.LerpMap(creatureRep.representedCreature.realizedCreature.TotalMass, 0.2f, 2f, 4f, 100f); i++)
				{
					int num = UnityEngine.Random.Range(0, glyphs.Length);
					int num2 = UnityEngine.Random.Range(0, glyphs[num].Length);
					glyphs[num][num2] = -1;
					dirtyGlyphs[num][num2] = true;
				}
				activity = Mathf.Min(1f, activity + 0.2f);
				return;
			}
			for (int j = 0; j < (int)Custom.LerpMap(creatureRep.representedCreature.realizedCreature.TotalMass, 0.2f, 2f, 2f, 11 * ringsActive); j++)
			{
				int num3 = UnityEngine.Random.Range(0, ringsActive);
				int num4 = UnityEngine.Random.Range(0, glyphs[num3].Length);
				glyphs[num3][num4] = UnityEngine.Random.Range(0, 7);
				dirtyGlyphs[num3][num4] = true;
				if (UnityEngine.Random.value < 0.5f)
				{
					glyphPositions[num3][num4, 2] = 1f;
				}
			}
		}

		public void Update()
		{
			if (owner.guard.dead)
			{
				deactivated = true;
			}
			if (activity > owner.guard.AI.stress)
			{
				activity = Mathf.Max(owner.guard.AI.stress - 0.0033333334f, owner.guard.AI.stress);
			}
			else
			{
				activity = owner.guard.AI.stress;
			}
			if (UnityEngine.Random.value < 0.01f)
			{
				ringsActive = Custom.IntClamp((int)Mathf.Lerp(2f, 9f, Mathf.Pow(owner.guard.AI.stress, 0.5f)), 2, 4);
			}
			lastSlowRingsActive = slowRingsActive;
			if (slowRingsActive < (float)ringsActive)
			{
				slowRingsActive = Mathf.Min(ringsActive, slowRingsActive + 0.1f);
			}
			else
			{
				slowRingsActive = Mathf.Max(ringsActive, slowRingsActive - 0.05f);
			}
			Vector2 vector = owner.guard.mainBodyChunk.pos - owner.guard.StoneDir * Mathf.Lerp(200f, RadAtCircle(2f + slowRingsActive * 2f, 1f, 0f), 0.5f);
			lastPos = pos;
			pos += Vector2.ClampMagnitude(vector - pos, 10f);
			pos = Vector2.Lerp(pos, vector, 0.1f);
			if (firstUpdate)
			{
				pos = vector;
				lastPos = pos;
				firstUpdate = false;
			}
			savDisruption = Mathf.InverseLerp(10f, 150f, Vector2.Distance(pos, vector));
			for (int i = 0; i < rotation.GetLength(0); i++)
			{
				rotation[i, 1] = rotation[i, 0];
				rotation[i, 0] += 0.2f / Mathf.Max(1f, CircumferenceAtCircle(i, 1f, savDisruption)) * ((i % 2 == 0) ? (-1f) : 1f) * Mathf.Lerp(Speed, 3f, owner.telekinesis);
			}
			for (int j = 0; j < swappers.Length; j++)
			{
				swappers[j].Update();
			}
			for (int k = 0; k < lines.GetLength(0); k++)
			{
				lines[k, 1] = lines[k, 0];
				lines[k, 0] += 1f / 120f * lines[k, 3] * Speed;
			}
			for (int l = 0; l < smallCircles.GetLength(0); l++)
			{
				smallCircles[l, 1] = smallCircles[l, 0];
				smallCircles[l, 0] += 0.004166667f * smallCircles[l, 4] * Speed;
			}
			for (int m = 0; m < glyphs.Length; m++)
			{
				for (int n = 0; n < glyphs[m].Length; n++)
				{
					glyphPositions[m][n, 1] = glyphPositions[m][n, 0];
					if (UnityEngine.Random.value < Speed / 160f)
					{
						if (UnityEngine.Random.value < 1f / 30f && glyphPositions[m][n, 0] == 0f && glyphs[m][n] > -1)
						{
							if (m == glyphs.Length - 1)
							{
								glyphPositions[m][n, 0] = -1f;
							}
							else if (m == glyphs.Length - 2 && ringsActive == 4)
							{
								glyphPositions[m][n, 0] = -3f;
							}
						}
						else
						{
							glyphPositions[m][n, 0] = ((UnityEngine.Random.value < 0.05f) ? 1f : 0f);
						}
					}
					if (glyphPositions[m][n, 0] == 1f && glyphs[m][n] == -1)
					{
						glyphs[m][n] = UnityEngine.Random.Range(0, 7);
						dirtyGlyphs[m][n] = true;
					}
					if (glyphPositions[m][n, 2] > 0f && glyphs[m][n] > -1)
					{
						glyphPositions[m][n, 2] -= 0.05f;
						glyphs[m][n] = UnityEngine.Random.Range(0, 7);
						dirtyGlyphs[m][n] = true;
					}
				}
			}
			for (int num = 0; num < smallCircles.GetLength(0); num++)
			{
				if (!(UnityEngine.Random.value < Speed / 120f) || !(smallCircles[num, 3] < (float)(ringsActive * 2)))
				{
					continue;
				}
				float num2 = RadAtCircle(smallCircles[num, 2] - 0.5f, 1f, savDisruption);
				float num3 = RadAtCircle(smallCircles[num, 3] - 0.5f, 1f, savDisruption);
				Vector2 p = Custom.DegToVec(smallCircles[num, 0] * 360f) * Mathf.Lerp(num2, num3, 0.5f);
				for (int num4 = 0; num4 < glyphs.Length; num4++)
				{
					for (int num5 = 0; num5 < glyphs[num4].Length; num5++)
					{
						if (Custom.DistLess(p, GlyphPos(num4, num5, 1f), (num3 - num2) / 2f))
						{
							glyphPositions[num4][num5, 2] = 1f;
						}
					}
				}
			}
			int num6 = 0;
			for (int num7 = 0; num7 < glyphs[0].Length; num7++)
			{
				if (glyphPositions[0][num7, 0] == 1f)
				{
					num6++;
				}
			}
			if (num6 > 1)
			{
				for (int num8 = 0; num8 < glyphs[0].Length; num8++)
				{
					glyphPositions[0][num8, 0] = 0f;
				}
			}
			for (int num9 = 0; num9 < 2; num9++)
			{
				rad[num9, 1] = rad[num9, 0];
				if (rad[num9, 0] < rad[num9, 2])
				{
					rad[num9, 0] = Mathf.Min(rad[num9, 2], rad[num9, 0] + ((num9 == 0) ? 0.15f : 0.0035714286f));
				}
				else
				{
					rad[num9, 0] = Mathf.Max(rad[num9, 2], rad[num9, 0] - ((num9 == 0) ? 0.15f : 0.0035714286f));
				}
				rad[num9, 0] = Mathf.Lerp(rad[num9, 0], rad[num9, 2], 0.01f);
			}
			if (UnityEngine.Random.value < Speed / 120f)
			{
				rad[0, 2] = ((UnityEngine.Random.value > activity) ? 0f : ((float)UnityEngine.Random.Range(-1, 3) * 20f));
				rad[1, 2] = ((UnityEngine.Random.value < 1f / Mathf.Lerp(1f, 5f, activity)) ? 1f : Mathf.Lerp(0.75f, 1.25f, UnityEngine.Random.value));
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			Color saturatedGold = RainWorld.SaturatedGold;
			for (int i = 0; i < circles; i++)
			{
				sLeaser.sprites[firstSprite + i] = new FSprite("Futile_White");
				sLeaser.sprites[firstSprite + i].color = saturatedGold;
				sLeaser.sprites[firstSprite + i].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
			}
			int num = circles;
			for (int j = 0; j < glyphs.Length; j++)
			{
				for (int k = 0; k < glyphs[j].Length; k++)
				{
					sLeaser.sprites[firstSprite + num] = new FSprite("haloGlyph" + glyphs[j][k]);
					sLeaser.sprites[firstSprite + num].color = saturatedGold;
					num++;
				}
			}
			for (int l = 0; l < swappers.Length; l++)
			{
				swappers[l].InitiateSprites(firstSwapperSprite + l * 3, sLeaser, rCam);
			}
			for (int m = 0; m < lines.GetLength(0); m++)
			{
				sLeaser.sprites[firstSprite + firstLineSprite + m] = new FSprite("pixel");
				sLeaser.sprites[firstSprite + firstLineSprite + m].color = saturatedGold;
			}
			for (int n = 0; n < smallCircles.GetLength(0); n++)
			{
				sLeaser.sprites[firstSprite + firstSmallCircleSprite + n] = new FSprite("Futile_White");
				sLeaser.sprites[firstSprite + firstSmallCircleSprite + n].color = saturatedGold;
				sLeaser.sprites[firstSprite + firstSmallCircleSprite + n].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 headPos, Vector2 headDir)
		{
			Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
			float num = Mathf.InverseLerp(10f, 150f, Vector2.Distance(vector, headPos - headDir * Mathf.Lerp(200f, RadAtCircle(2f + slowRingsActive * 2f, timeStacker, 0f), 0.5f)));
			int num2 = Custom.IntClamp((int)(Mathf.Lerp(lastSlowRingsActive, slowRingsActive, timeStacker) + Mathf.Lerp(-0.4f, 0.4f, UnityEngine.Random.value) * Mathf.InverseLerp(0.01f, 0.1f, Mathf.Abs(lastSlowRingsActive - slowRingsActive))), 2, 4);
			if (UnityEngine.Random.value < num || deactivated)
			{
				for (int i = firstSprite; i < firstSprite + totalSprites; i++)
				{
					sLeaser.sprites[i].isVisible = false;
				}
				return;
			}
			for (int j = firstSprite; j < firstSprite + totalSprites; j++)
			{
				sLeaser.sprites[j].isVisible = true;
			}
			for (int k = 0; k < circles; k++)
			{
				sLeaser.sprites[firstSprite + k].x = vector.x - camPos.x;
				sLeaser.sprites[firstSprite + k].y = vector.y - camPos.y;
				float num3 = RadAtCircle((float)k - 0.5f, timeStacker, num);
				sLeaser.sprites[firstSprite + k].scale = num3 / 8f;
				sLeaser.sprites[firstSprite + k].alpha = 1f / num3;
				sLeaser.sprites[firstSprite + k].isVisible = k < num2 * 2;
			}
			int num4 = circles;
			for (int l = 0; l < glyphs.Length; l++)
			{
				for (int m = 0; m < glyphs[l].Length; m++)
				{
					Vector2 vector2 = vector + GlyphPos(l, m, timeStacker);
					sLeaser.sprites[firstSprite + num4].x = vector2.x - camPos.x;
					sLeaser.sprites[firstSprite + num4].y = vector2.y - camPos.y;
					if (dirtyGlyphs[l][m])
					{
						sLeaser.sprites[firstSprite + num4].element = Futile.atlasManager.GetElementWithName("haloGlyph" + glyphs[l][m]);
						dirtyGlyphs[l][m] = false;
					}
					sLeaser.sprites[firstSprite + num4].isVisible = UnityEngine.Random.value > num && l < num2;
					if (glyphs[l][m] == -1 || (l == 0 && glyphPositions[l][m, 0] == 1f))
					{
						sLeaser.sprites[firstSprite + num4].rotation = 0f;
					}
					else
					{
						sLeaser.sprites[firstSprite + num4].rotation = ((float)m / (float)glyphs[l].Length + Mathf.Lerp(rotation[l, 1], rotation[l, 0], timeStacker)) * 360f;
					}
					num4++;
				}
			}
			for (int n = 0; n < swappers.Length; n++)
			{
				swappers[n].DrawSprites(firstSwapperSprite + n * 3, sLeaser, rCam, timeStacker, camPos, vector);
			}
			for (int num5 = 0; num5 < lines.GetLength(0); num5++)
			{
				float num6 = Mathf.Lerp(lines[num5, 1], lines[num5, 0], timeStacker);
				Vector2 vector3 = Custom.DegToVec(num6 * 360f) * RadAtCircle(lines[num5, 2] * 2f + 1f, timeStacker, num) + vector;
				sLeaser.sprites[firstSprite + firstLineSprite + num5].isVisible = lines[num5, 2] < (float)(num2 - 1);
				if (UnityEngine.Random.value > num || UnityEngine.Random.value > 0.25f)
				{
					sLeaser.sprites[firstSprite + firstLineSprite + num5].rotation = num6 * 360f;
					sLeaser.sprites[firstSprite + firstLineSprite + num5].scaleY = RadAtCircle(lines[num5, 2] - 0.5f, timeStacker, num) - RadAtCircle(lines[num5, 2] + 0.5f, timeStacker, num);
				}
				else
				{
					vector3 = Vector2.Lerp(vector3, headPos, 0.4f);
					sLeaser.sprites[firstSprite + firstLineSprite + num5].rotation = Custom.AimFromOneVectorToAnother(vector3, headPos);
					sLeaser.sprites[firstSprite + firstLineSprite + num5].scaleY = Vector2.Distance(vector3, headPos) * 1.5f * UnityEngine.Random.value;
				}
				sLeaser.sprites[firstSprite + firstLineSprite + num5].x = vector3.x - camPos.x;
				sLeaser.sprites[firstSprite + firstLineSprite + num5].y = vector3.y - camPos.y;
			}
			for (int num7 = 0; num7 < smallCircles.GetLength(0); num7++)
			{
				float num8 = Mathf.Lerp(smallCircles[num7, 1], smallCircles[num7, 0], timeStacker);
				float num9 = RadAtCircle(smallCircles[num7, 2] - 0.5f, timeStacker, num);
				float num10 = RadAtCircle(smallCircles[num7, 3] - 0.5f, timeStacker, num);
				sLeaser.sprites[firstSprite + firstSmallCircleSprite + num7].isVisible = smallCircles[num7, 3] < (float)(num2 * 2);
				Vector2 vector4 = Custom.DegToVec(num8 * 360f) * Mathf.Lerp(num9, num10, 0.5f) + vector;
				sLeaser.sprites[firstSprite + firstSmallCircleSprite + num7].x = vector4.x - camPos.x;
				sLeaser.sprites[firstSprite + firstSmallCircleSprite + num7].y = vector4.y - camPos.y;
				sLeaser.sprites[firstSprite + firstSmallCircleSprite + num7].scale = (num10 - num9) / 16f;
				sLeaser.sprites[firstSprite + firstSmallCircleSprite + num7].alpha = 2f / (num10 - num9);
			}
		}

		public Vector2 GlyphPos(int circle, int glyph, float timeStacker)
		{
			if ((float)circle * 2f - Mathf.Lerp(glyphPositions[circle][glyph, 1], glyphPositions[circle][glyph, 0], timeStacker) < 0f)
			{
				return new Vector2(0f, 0f);
			}
			float num = Mathf.Lerp(rotation[circle, 1], rotation[circle, 0], timeStacker);
			return Custom.DegToVec(((float)glyph / (float)glyphs[circle].Length + num) * 360f) * RadAtCircle((float)circle * 2f - Mathf.Lerp(glyphPositions[circle][glyph, 1], glyphPositions[circle][glyph, 0], timeStacker), timeStacker, savDisruption);
		}
	}

	private TempleGuard guard;

	private Halo halo;

	public Dangler[,] robes;

	public float[,,] robeProps;

	public Dangler.DanglerProps danglerVals;

	public float telekinesis;

	public float lastTelekin;

	private int HeadSprite;

	public bool armsPos;

	public int armsPosCounter;

	private Vector2 danglerMedPos;

	private Vector2 lastMedPos;

	public float eyeBlinking;

	public float lastEyeBlinking;

	public Arm[,] arms;

	private int FirstHaloSprite => HeadSprite + 3;

	private int TotalSprites => HeadSprite + 3 + halo.totalSprites;

	private Vector2 StoneDir(float timeStacker)
	{
		return Custom.DirVec(Vector2.Lerp(Vector2.Lerp(guard.bodyChunks[1].lastPos, guard.bodyChunks[1].pos, timeStacker), Vector2.Lerp(guard.bodyChunks[2].lastPos, guard.bodyChunks[2].pos, timeStacker), 0.5f), Vector2.Lerp(Vector2.Lerp(guard.bodyChunks[3].lastPos, guard.bodyChunks[3].pos, timeStacker), Vector2.Lerp(guard.bodyChunks[4].lastPos, guard.bodyChunks[4].pos, timeStacker), 0.5f));
	}

	private Vector2 ArmDir(float timeStacker)
	{
		return Vector3.Slerp(StoneDir(timeStacker), Custom.DirVec(guard.mainBodyChunk.pos, Vector2.Lerp(lastMedPos, danglerMedPos, timeStacker)), 0.6f);
	}

	private int RobeSprite(int a, int i)
	{
		return a * robes.GetLength(1) + i;
	}

	private int EyeSprite(int part)
	{
		return HeadSprite + 1 + part;
	}

	public TempleGuardGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		guard = ow as TempleGuard;
		danglerVals = new Dangler.DanglerProps();
		danglerVals.elasticity = 1f;
		danglerVals.weightSymmetryTendency = 0.55f;
		danglerVals.airFriction = 0.9f;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(guard.abstractCreature.ID.RandomSeed);
		robes = new Dangler[2, 10];
		robeProps = new float[robes.GetLength(0), robes.GetLength(1), 1];
		for (int i = 0; i < robes.GetLength(0); i++)
		{
			for (int j = 0; j < robes.GetLength(1); j++)
			{
				float num = (float)j / (float)(robes.GetLength(1) - 1);
				robes[i, j] = new Dangler(this, i * robes.GetLength(1) + j, UnityEngine.Random.Range(14, 25), Mathf.Lerp(3f, 6f, Mathf.Sin(num * (float)Math.PI)), 20f);
				robeProps[i, j, 0] = UnityEngine.Random.value;
			}
		}
		arms = new Arm[2, 2];
		int num2 = robes.Length;
		for (int k = 0; k < 2; k++)
		{
			for (int l = 0; l < 2; l++)
			{
				arms[k, l] = new Arm(this, num2, k, l);
				num2 += arms[k, l].totalSprites;
			}
		}
		HeadSprite = num2;
		halo = new Halo(this, FirstHaloSprite);
		armsPos = UnityEngine.Random.value < 0.5f;
		armsPosCounter = UnityEngine.Random.Range(50, 1500);
		UnityEngine.Random.state = state;
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < robes.GetLength(0); i++)
		{
			for (int j = 0; j < robes.GetLength(1); j++)
			{
				robes[i, j].Reset();
			}
		}
		for (int k = 0; k < 2; k++)
		{
			for (int l = 0; l < 2; l++)
			{
				arms[k, l].Reset();
			}
		}
	}

	public void ReactToCreature(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
	{
		halo.ReactToCreature(firstSpot, creatureRep);
		if (creatureRep.representedCreature.realizedCreature != null)
		{
			if (firstSpot)
			{
				eyeBlinking = Custom.LerpMap(creatureRep.representedCreature.realizedCreature.TotalMass, 0.2f, 4f, 2f, 7f);
			}
			else
			{
				eyeBlinking = Custom.LerpMap(creatureRep.representedCreature.realizedCreature.TotalMass, 0.2f, 4f, 0.5f, 2f);
			}
		}
	}

	public override void Update()
	{
		base.Update();
		halo.Update();
		lastTelekin = telekinesis;
		telekinesis = guard.telekinesis;
		lastEyeBlinking = eyeBlinking;
		eyeBlinking = Mathf.Max(0f, eyeBlinking - 0.2f);
		armsPosCounter--;
		if (guard.telekinesis > 0f)
		{
			armsPosCounter -= 10;
		}
		if (armsPosCounter < 1)
		{
			armsPos = !armsPos && guard.telekinesis == 0f;
			armsPosCounter = UnityEngine.Random.Range(50, 1500);
		}
		Vector2 vector = StoneDir(1f);
		Vector2 vector2 = Custom.PerpendicularVector(vector);
		lastMedPos = danglerMedPos;
		Vector2 vector3 = Custom.DirVec(guard.mainBodyChunk.pos, danglerMedPos);
		danglerMedPos *= 0f;
		int num = 0;
		for (int i = 0; i < robes.GetLength(0); i++)
		{
			for (int j = 0; j < robes.GetLength(1); j++)
			{
				float num2 = (float)j / (float)(robes.GetLength(1) - 1);
				robes[i, j].Update();
				Vector2 vector4 = vector * Mathf.Sin(num2 * (float)Math.PI);
				vector4 += vector2 * Custom.LerpMap(j, 0f, robes.GetLength(1) - 1, -1f, 1f) * Mathf.Pow(1f - Mathf.Sin(num2 * (float)Math.PI), 0.1f) * 1.5f;
				float num3 = 0f;
				for (int k = 0; k < robes[i, j].segments.Length; k++)
				{
					float num4 = (float)k / (float)(robes[i, j].segments.Length - 1);
					num3 += robes[i, j].segments[k].conRad;
					robes[i, j].segments[k].vel += vector4 * Mathf.InverseLerp(2f, 0f, k) * 2f;
					robes[i, j].segments[k].vel.y += num4 * Mathf.Lerp(0.5f, 0.1f, telekinesis);
					robes[i, j].segments[k].vel += Vector2.ClampMagnitude(guard.mainBodyChunk.pos + vector3 * num3 - robes[i, j].segments[k].pos, Mathf.Pow(Mathf.Sin(Mathf.Pow(num4, 0.5f) * (float)Math.PI), 5f) * 60f * (1f - Mathf.Sin(num2 * (float)Math.PI))) / 80f;
					robes[i, j].segments[k].vel += Custom.DirVec(guard.mainBodyChunk.pos + vector3 * num3, robes[i, j].segments[k].pos) * Custom.LerpMap(num4, 0.5f, 1f, 0f, 0.4f) * (1f - Mathf.Sin(num2 * (float)Math.PI));
					if (telekinesis > 0f)
					{
						robes[i, j].segments[k].vel += Custom.RNV() * num4 * 3f * telekinesis;
					}
					danglerMedPos += robes[i, j].segments[k].pos;
					num++;
				}
			}
		}
		danglerMedPos /= (float)num;
		for (int l = 0; l < 2; l++)
		{
			for (int m = 0; m < 2; m++)
			{
				arms[l, m].Update();
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[HeadSprite] = new FSprite("guardHead");
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[EyeSprite(i)] = new FSprite("guardEye");
		}
		for (int j = 0; j < robes.GetLength(0); j++)
		{
			for (int k = 0; k < robes.GetLength(1); k++)
			{
				robes[j, k].InitSprite(sLeaser, RobeSprite(j, k));
				sLeaser.sprites[RobeSprite(j, k)].shader = rCam.room.game.rainWorld.Shaders["TentaclePlant"];
				sLeaser.sprites[RobeSprite(j, k)].alpha = 0.5f + robeProps[j, k, 0] * 0.4f;
			}
		}
		halo.InitiateSprites(sLeaser, rCam);
		for (int l = 0; l < 2; l++)
		{
			for (int m = 0; m < 2; m++)
			{
				arms[l, m].InitiateSprites(sLeaser, rCam);
			}
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		for (int i = 0; i < FirstHaloSprite; i++)
		{
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
		for (int j = FirstHaloSprite; j < TotalSprites; j++)
		{
			rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[j]);
		}
		if (sLeaser.containers != null)
		{
			FContainer[] containers = sLeaser.containers;
			foreach (FContainer node in containers)
			{
				newContatiner.AddChild(node);
			}
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2 headPos = Vector2.Lerp(guard.mainBodyChunk.lastPos, guard.mainBodyChunk.pos, timeStacker);
		Vector2 vector = StoneDir(timeStacker);
		sLeaser.sprites[HeadSprite].x = headPos.x - camPos.x;
		sLeaser.sprites[HeadSprite].y = headPos.y - camPos.y;
		sLeaser.sprites[HeadSprite].rotation = Custom.VecToDeg(vector);
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[EyeSprite(i)].x = headPos.x - camPos.x + (1f - (float)i);
			sLeaser.sprites[EyeSprite(i)].y = headPos.y - camPos.y - (1f - (float)i);
			sLeaser.sprites[EyeSprite(i)].rotation = Custom.VecToDeg(vector);
		}
		for (int j = 0; j < robes.GetLength(0); j++)
		{
			for (int k = 0; k < robes.GetLength(1); k++)
			{
				robes[j, k].DrawSprite(RobeSprite(j, k), sLeaser, rCam, timeStacker, camPos);
			}
		}
		for (int l = 0; l < 2; l++)
		{
			for (int m = 0; m < 2; m++)
			{
				arms[l, m].DrawSprites(sLeaser, rCam, timeStacker, camPos, headPos, vector);
			}
		}
		halo.DrawSprites(sLeaser, rCam, timeStacker, camPos, headPos, vector);
		sLeaser.sprites[EyeSprite(1)].color = new Color(Mathf.Lerp(0.1f, 1f, Mathf.Max(Mathf.Lerp(lastTelekin, telekinesis, timeStacker), Mathf.Min(1f, Mathf.Lerp(lastEyeBlinking, eyeBlinking, timeStacker))) * UnityEngine.Random.value), 0f, 0f);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		for (int i = 0; i <= HeadSprite; i++)
		{
			sLeaser.sprites[i].color = palette.blackColor;
		}
		for (int j = 0; j < 2; j++)
		{
			for (int k = 0; k < 2; k++)
			{
				arms[j, k].ApplyPalette(sLeaser, rCam, palette);
			}
		}
		sLeaser.sprites[EyeSprite(0)].color = Color.Lerp(palette.blackColor, new Color(1f, 1f, 1f), 0.05f);
	}

	public Vector2 DanglerConnection(int index, float timeStacker)
	{
		return Vector2.Lerp(guard.mainBodyChunk.lastPos, guard.mainBodyChunk.pos, timeStacker) + StoneDir(timeStacker) * 9.5f;
	}

	public Dangler.DanglerProps Props(int index)
	{
		return danglerVals;
	}
}
