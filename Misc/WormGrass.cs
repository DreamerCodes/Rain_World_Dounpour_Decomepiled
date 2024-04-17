using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

internal class WormGrass : UpdatableAndDeletable, IAccessibilityModifier, Explosion.IReactToExplosions
{
	public class Worm : UpdatableAndDeletable, IDrawable
	{
		public class Segment
		{
			public Vector2 pos;

			public Vector2 lastPos;

			public Vector2 vel;

			public float rad;

			public float stretchedRad;

			public Segment(Vector2 pos, float rad)
			{
				this.pos = pos;
				lastPos = pos;
				this.rad = rad;
				stretchedRad = rad;
			}
		}

		public WormGrass wormGrass;

		public WormGrassPatch patch;

		public Vector2 lastPos;

		public Vector2 pos;

		public Vector2 vel;

		public Vector2 basePos;

		public Vector2 belowGroundPos;

		public float length;

		public int color;

		public bool culled;

		public bool lastCulled;

		public bool cosmeticOnly;

		public float iFac;

		private float savLenghtFac;

		public Vector2? underGroundStuckPos;

		public float excitement;

		public float dragForce;

		public Creature focusCreature;

		public BodyChunk attachedChunk;

		public Vector2 attachedDir;

		public Vector2 lastVisPos;

		public Segment[] segments;

		private float savedReachHeight;

		public PhysicalObject repulsedObject;

		public Worm(WormGrass wormGrass, WormGrassPatch patch, Vector2 basePos, float reachHeight, float iFac, float lengthFac, bool cosmeticOnly)
		{
			this.wormGrass = wormGrass;
			this.basePos = basePos;
			this.cosmeticOnly = cosmeticOnly;
			color = UnityEngine.Random.Range(0, 11);
			Reset(patch, basePos, iFac, reachHeight, lengthFac);
		}

		public void Reset(WormGrassPatch patch, Vector2 newBasePos, float iFac, float reachHeight, float lengthFac)
		{
			this.patch = patch;
			savedReachHeight = reachHeight;
			basePos = newBasePos;
			this.iFac = iFac;
			savLenghtFac = lengthFac;
			belowGroundPos = basePos + new Vector2(0f, -10f);
			underGroundStuckPos = null;
			attachedChunk = null;
			length = reachHeight - basePos.y;
			length *= lengthFac;
			pos = basePos + new Vector2(0f, length);
			lastPos = pos;
			if (segments == null)
			{
				segments = new Segment[Custom.IntClamp((int)Mathf.Lerp(length / 10f, 5f, 0.5f), 2, 4)];
				for (int i = 0; i < segments.Length; i++)
				{
					float a = (float)i / (float)(segments.Length - 1);
					a = Mathf.Lerp(a, 0.5f, 0.2f);
					segments[i] = new Segment(basePos, Mathf.Lerp(1.5f, 5f, Mathf.Sin(a * (float)Math.PI)) * Mathf.Lerp(1f, 1.3f, iFac));
				}
			}
			for (int j = 0; j < segments.Length; j++)
			{
				segments[j].pos = Vector2.Lerp(pos, belowGroundPos, (float)j / (float)(segments.Length - 1));
				segments[j].lastPos = segments[j].pos;
				segments[j].vel *= 0f;
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			lastCulled = culled;
			culled = !wormGrass.room.ViewedByAnyCamera(pos, 200f);
			lastPos = pos;
			pos += vel;
			vel *= 0.9f;
			vel.y += 0.6f;
			if (wormGrass.room.GetTile(pos).Solid)
			{
				vel *= 0.5f;
			}
			if (attachedChunk != null)
			{
				Attached();
			}
			else if (underGroundStuckPos.HasValue)
			{
				vel *= 0f;
				pos = underGroundStuckPos.Value;
				if (UnityEngine.Random.value < 0.025f)
				{
					underGroundStuckPos = null;
				}
			}
			else
			{
				Act();
			}
			if (attachedChunk == null && !Custom.DistLess(pos, basePos, length))
			{
				Vector2 vector = Custom.DirVec(pos, basePos);
				float num = Vector2.Distance(pos, basePos);
				pos += vector * (num - length) * 0.1f;
				vel += vector * (num - length) * 0.1f;
			}
			if (!culled)
			{
				if (lastCulled)
				{
					Reset(patch, basePos, iFac, savedReachHeight, savLenghtFac);
				}
				else
				{
					float num2 = length / (float)segments.Length;
					for (int i = 0; i < segments.Length; i++)
					{
						float num3 = (float)i / (float)(segments.Length - 1);
						segments[i].lastPos = segments[i].pos;
						segments[i].pos += segments[i].vel;
						segments[i].vel *= 0.9f;
						segments[i].vel += Custom.RNV() * excitement;
						segments[i].vel += (basePos + new Vector2(0f, length * (1f - num3)) - segments[i].pos) * Mathf.InverseLerp(0f, length * 20f, Vector2.Distance(basePos, segments[i].pos));
						if (i > 0)
						{
							Vector2 vector2 = Custom.DirVec(segments[i].pos, segments[i - 1].pos);
							float num4 = Vector2.Distance(segments[i].pos, segments[i - 1].pos);
							segments[i].pos += vector2 * (num4 - num2) * 0.25f;
							segments[i].vel += vector2 * (num4 - num2) * 0.25f;
							segments[i - 1].pos -= vector2 * (num4 - num2) * 0.25f;
							segments[i - 1].vel -= vector2 * (num4 - num2) * 0.25f;
							float a = num2 / num4;
							a = Mathf.Lerp(a, 1f, 1f - Mathf.Sin((float)i / (float)segments.Length) * (float)Math.PI);
							segments[i - 1].stretchedRad = segments[i - 1].rad * ((num4 < num2) ? Mathf.Min(Mathf.Lerp(a, 1f, 0.4f), 2f) : Mathf.Pow(a, Custom.LerpMap(length, 35f, 50f, 0.5f, 0.85f)));
						}
					}
				}
			}
			segments[0].pos = pos;
			segments[0].vel = vel;
			segments[segments.Length - 1].pos = belowGroundPos;
			segments[segments.Length - 1].vel *= 0f;
		}

		public void Act()
		{
			float num = float.MaxValue;
			float a = 0f;
			if (focusCreature != null)
			{
				num = Vector2.Distance(pos, focusCreature.RandomChunk.pos);
				a = focusCreature.TotalMass;
				BodyChunk randomChunk = focusCreature.RandomChunk;
				vel += (Custom.DirVec(pos, randomChunk.pos + Custom.RNV() * randomChunk.rad) + Custom.RNV() * 0.5f) * UnityEngine.Random.value * Custom.LerpMap(num, 60f * Mathf.Lerp(focusCreature.TotalMass, 1f, 0.5f), 400f * Mathf.Lerp(focusCreature.TotalMass, 1f, 0.5f), 3.5f, 0f) * excitement;
				excitement = Mathf.Min(excitement + focusCreature.TotalMass / Mathf.Lerp(60f, 10f, iFac), 1f);
				if (num > 400f * Mathf.Lerp(focusCreature.TotalMass, 1f, 0.5f) || focusCreature.slatedForDeletetion || focusCreature.mainBodyChunk.pos.y < basePos.y - 50f || !wormGrass.room.VisualContact(pos, focusCreature.RandomChunk.pos))
				{
					focusCreature = null;
				}
			}
			else if (excitement > 0f)
			{
				vel += Custom.RNV() * 2f * UnityEngine.Random.value * excitement;
				excitement = Mathf.Max(excitement - 1f / Mathf.Lerp(40f, 300f, UnityEngine.Random.value), 0f);
			}
			if (excitement > 0.5f)
			{
				for (int i = 0; i < wormGrass.room.physicalObjects.Length; i++)
				{
					if (attachedChunk != null)
					{
						break;
					}
					for (int j = 0; j < wormGrass.room.physicalObjects[i].Count; j++)
					{
						if (attachedChunk != null)
						{
							break;
						}
						if (!(wormGrass.room.physicalObjects[i][j] is Creature) || (wormGrass.room.physicalObjects[i][j] as Creature).abstractCreature.tentacleImmune || !(Mathf.Pow(UnityEngine.Random.value, 0.7f) * 2f < excitement))
						{
							continue;
						}
						for (int k = 0; k < wormGrass.room.physicalObjects[i][j].bodyChunks.Length; k++)
						{
							if (Custom.DistLess(pos, wormGrass.room.physicalObjects[i][j].bodyChunks[k].pos, wormGrass.room.physicalObjects[i][j].bodyChunks[k].rad))
							{
								Attach(wormGrass.room.physicalObjects[i][j].bodyChunks[k]);
								break;
							}
						}
					}
				}
			}
			if (patch.trackedCreatures.Count > 0)
			{
				Creature creature = patch.trackedCreatures[UnityEngine.Random.Range(0, patch.trackedCreatures.Count)].creature;
				float num2 = Vector2.Distance(pos, creature.RandomChunk.pos);
				if (creature != focusCreature && num2 < Mathf.Pow(UnityEngine.Random.value, 15f) * 400f * Mathf.Lerp(creature.TotalMass, 1f, 0.5f) && num2 * Mathf.Lerp(a, 2f, 0.85f) < num * Mathf.Lerp(creature.TotalMass, 2f, 0.85f) && wormGrass.room.VisualContact(pos, creature.RandomChunk.pos))
				{
					focusCreature = creature;
				}
			}
			if (focusCreature != null)
			{
				repulsedObject = null;
			}
			else if (patch.wormGrass.repulsiveObjects.Count > 0)
			{
				if (repulsedObject != null && (repulsedObject.slatedForDeletetion || repulsedObject.room == null || repulsedObject.room != patch.wormGrass.room || Vector2.Distance(pos, repulsedObject.firstChunk.pos) >= 150f || !wormGrass.room.VisualContact(pos, repulsedObject.firstChunk.pos)))
				{
					repulsedObject = null;
				}
				if (repulsedObject == null)
				{
					repulsedObject = patch.wormGrass.repulsiveObjects[UnityEngine.Random.Range(0, patch.wormGrass.repulsiveObjects.Count)];
				}
				else if (repulsedObject.firstChunk.pos.x < pos.x)
				{
					vel.x += 0.5f;
				}
				else
				{
					vel.x -= 0.5f;
				}
			}
			else
			{
				repulsedObject = null;
			}
		}

		public void Attach(BodyChunk chunk)
		{
			attachedChunk = chunk;
			focusCreature = null;
			attachedDir = Custom.RotateAroundOrigo(Custom.DirVec(chunk.pos, pos), 0f - Custom.VecToDeg(chunk.Rotation));
			lastVisPos = pos;
			dragForce = 0f;
		}

		public void Attached()
		{
			Vector2 value = (pos = attachedChunk.pos + Custom.RotateAroundOrigo(attachedDir, Custom.VecToDeg(attachedChunk.Rotation)) * attachedChunk.rad * 0.7f);
			vel = attachedChunk.vel;
			excitement = 1f;
			WormGrassPatch.CreatureAndPull creatureAndPull = null;
			for (int i = 0; i < patch.trackedCreatures.Count; i++)
			{
				if (patch.trackedCreatures[i].creature == attachedChunk.owner)
				{
					creatureAndPull = patch.trackedCreatures[i];
					break;
				}
			}
			if (creatureAndPull == null)
			{
				attachedChunk = null;
				underGroundStuckPos = value;
				return;
			}
			if (cosmeticOnly && creatureAndPull.pull == 0f)
			{
				attachedChunk = null;
				return;
			}
			Vector2 vector = basePos + new Vector2(0f, (attachedChunk.rad + 20f) * (0f - creatureAndPull.bury));
			if (!Custom.DistLess(vector, pos, length * 4f))
			{
				attachedChunk = null;
			}
			else if (attachedChunk != null && attachedChunk.owner.room != wormGrass.room)
			{
				attachedChunk = null;
				excitement = 0f;
			}
			if (cosmeticOnly || attachedChunk == null)
			{
				return;
			}
			attachedChunk.vel *= Mathf.Lerp(1f, 0f, Mathf.Pow(Mathf.InverseLerp(0.3f, 0.8f, dragForce), 2f + attachedChunk.mass * Mathf.Max(1f, attachedChunk.owner.TotalMass) * 15f) / (attachedChunk.owner.TotalMass * 1.43f));
			attachedChunk.vel += Custom.DirVec(attachedChunk.pos, vector) * Custom.LerpMap(Vector2.Distance(attachedChunk.pos, vector), length / 2f, length * 4f, Mathf.Lerp(0.0001f, 0.001f, Mathf.Pow(dragForce, 4f)), Mathf.Lerp(0.05f, 0.5f, dragForce)) / attachedChunk.mass;
			dragForce += Mathf.Lerp(1f / Mathf.Lerp(4000f, 100f, dragForce), 1f / (Mathf.Max(Vector2.Distance(attachedChunk.lastPos, attachedChunk.pos), 1f) * 250f), 1f - dragForce) / (attachedChunk.owner.TotalMass * 1.43f);
			if (!(attachedChunk.owner as Creature).Consious)
			{
				dragForce += 1f / (57f * (attachedChunk.owner.TotalMass * Mathf.Max(1f, attachedChunk.owner.TotalMass)));
			}
			if (dragForce > 1f || creatureAndPull.bury > 0f)
			{
				dragForce = 1f;
				underGroundStuckPos = value;
				if (creatureAndPull.creature.grabbedBy.Count == 0)
				{
					creatureAndPull.bury = Mathf.Min(1f, creatureAndPull.bury + 1f / 60f);
				}
			}
			else if (wormGrass.room.VisualContact(basePos, pos))
			{
				lastVisPos = pos;
			}
			else
			{
				pos = lastVisPos;
				attachedChunk = null;
			}
			if (attachedChunk != null && attachedChunk.owner is Player && UnityEngine.Random.value < (attachedChunk.owner as Player).GraspWiggle / 20f)
			{
				attachedChunk = null;
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[2];
			sLeaser.sprites[0] = TriangleMesh.MakeLongMeshAtlased(segments.Length, pointyTip: true, customColor: true);
			sLeaser.sprites[1] = new FSprite("tinyStar", quadType: false);
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			if (culled)
			{
				sLeaser.sprites[0].isVisible = false;
				sLeaser.sprites[1].isVisible = false;
			}
			else
			{
				Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
				float num = 0.2f;
				sLeaser.sprites[1].x = vector.x - camPos.x;
				sLeaser.sprites[1].y = vector.y - camPos.y;
				sLeaser.sprites[1].isVisible = excitement > 0.05f;
				sLeaser.sprites[0].isVisible = true;
				vector += Custom.DirVec(segments[1].pos, segments[0].pos) * 1.5f;
				for (int i = 0; i < segments.Length; i++)
				{
					Vector2 vector2 = Vector2.Lerp(segments[i].lastPos, segments[i].pos, timeStacker);
					Vector2 normalized = (vector2 - vector).normalized;
					Vector2 vector3 = Custom.PerpendicularVector(normalized);
					float num2 = Vector2.Distance(vector2, vector) / 5f;
					(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * (num + segments[i].stretchedRad) * 0.5f + normalized * num2 - camPos);
					(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * (num + segments[i].stretchedRad) * 0.5f + normalized * num2 - camPos);
					if (i < segments.Length - 1)
					{
						(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * segments[i].stretchedRad - normalized * num2 - camPos);
						(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * segments[i].stretchedRad - normalized * num2 - camPos);
					}
					else
					{
						(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, belowGroundPos + new Vector2(0f, -10f) - camPos);
					}
					num = segments[i].stretchedRad;
					vector = vector2;
				}
			}
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			Color color = rCam.PixelColorAtCoordinate(belowGroundPos);
			Color color2 = Color.Lerp(palette.texture.GetPixel(this.color, 3), new Color(1f, 0f, 0f), iFac * 0.5f);
			if (ModManager.MSC && room?.world.region != null && room?.world.region.name == "OE")
			{
				float num = 1000f;
				float num2 = (float)room.world.rainCycle.dayNightCounter / num;
				color = Color.Lerp(color, Color.Lerp(new Color(0.17f, 0.38f, 0.17f), color2, 0.5f), num2 * 0.04f);
				color2 = Color.Lerp(color2, new Color(0.17f, 0.38f, 0.17f), num2 * 0.4f);
			}
			sLeaser.sprites[1].color = new Color(0.2f, 0f, 1f);
			for (int i = 0; i < segments.Length; i++)
			{
				(sLeaser.sprites[0] as TriangleMesh).verticeColors[i * 4] = Color.Lerp(color2, color, (float)i / (float)(segments.Length - 1));
				(sLeaser.sprites[0] as TriangleMesh).verticeColors[i * 4 + 1] = Color.Lerp(color2, color, (float)i / (float)(segments.Length - 1));
				(sLeaser.sprites[0] as TriangleMesh).verticeColors[i * 4 + 2] = Color.Lerp(color2, color, ((float)i + 0.5f) / (float)(segments.Length - 1));
				if (i < segments.Length - 1)
				{
					(sLeaser.sprites[0] as TriangleMesh).verticeColors[i * 4 + 3] = Color.Lerp(color2, color, ((float)i + 0.5f) / (float)(segments.Length - 1));
				}
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			newContatiner = rCam.ReturnFContainer("Items");
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public class WormGrassPatch
	{
		public class CreatureAndPull
		{
			public Creature creature;

			public float pull;

			public float bury;

			private int consumeTimer;

			private Vector2 ConsumedPos;

			public CreatureAndPull(Creature creature)
			{
				this.creature = creature;
				pull = 0f;
				bury = 0f;
				consumeTimer = 0;
			}

			public void Consume()
			{
				if (consumeTimer == 0)
				{
					consumeTimer = 1;
					ConsumedPos = creature.mainBodyChunk.pos;
				}
			}

			public void Update()
			{
				if (!ModManager.MMF)
				{
					return;
				}
				if (!creature.dead || (creature.grabbedBy.Count > 0 && creature.grabbedBy[0].grabber is Player))
				{
					if (creature.dead)
					{
						pull = 0f;
						bury = 0f;
						if (consumeTimer > 0)
						{
							creature.CollideWithTerrain = true;
						}
					}
					consumeTimer = 0;
				}
				else
				{
					if (consumeTimer <= 0)
					{
						return;
					}
					consumeTimer++;
					creature.CollideWithTerrain = false;
					creature.mainBodyChunk.vel = new Vector2(0f, 0f);
					if (consumeTimer >= 20 || creature.mainBodyChunk.pos.y < ConsumedPos.y - 40f)
					{
						Custom.Log($"WORMGRASS REMOVED CREATURE {creature}");
						if (creature.grabbedBy.Count > 0)
						{
							creature.grabbedBy[0].Release();
						}
						creature.abstractCreature.Move(new WorldCoordinate(creature.room.abstractRoom.index, -1, -1, 0));
						creature.Destroy();
					}
				}
			}
		}

		public HSLColor debugColor;

		public List<IntVector2> tiles;

		public float[,] sizes;

		public Vector2[][] cosmeticWormPositions;

		public float[][,] cosmeticWormLengths;

		public List<Worm> worms;

		public WormGrass wormGrass;

		public int highestTile;

		public List<CreatureAndPull> trackedCreatures;

		private LightSource GrassLight;

		public IntVector2 LeftTile => tiles[0];

		public IntVector2 RightTile => tiles[tiles.Count - 1];

		public int TotalWorms => worms.Count;

		public WormGrassPatch(WormGrass wormGrass, IntVector2 firstTile)
		{
			this.wormGrass = wormGrass;
			debugColor = new HSLColor(UnityEngine.Random.value, 1f, 0.5f);
			tiles = new List<IntVector2> { firstTile };
			trackedCreatures = new List<CreatureAndPull>();
		}

		private bool AlreadyTrackingCreature(Creature creature)
		{
			for (int i = 0; i < trackedCreatures.Count; i++)
			{
				if (trackedCreatures[i].creature == creature)
				{
					return true;
				}
			}
			return false;
		}

		public void Update()
		{
			if (wormGrass.room.abstractRoom.creatures.Count > 0)
			{
				Creature realizedCreature = wormGrass.room.abstractRoom.creatures[UnityEngine.Random.Range(0, wormGrass.room.abstractRoom.creatures.Count)].realizedCreature;
				if (realizedCreature != null && tiles.Count > 0 && realizedCreature.bodyChunks.Length != 0 && wormGrass.room.VisualContact(wormGrass.room.MiddleOfTile(tiles[UnityEngine.Random.Range(0, tiles.Count)]), realizedCreature.bodyChunks[UnityEngine.Random.Range(0, realizedCreature.bodyChunks.Length)].pos))
				{
					bool flag = ModManager.MSC && realizedCreature.WormGrassGooduckyImmune;
					if (!realizedCreature.Template.wormGrassImmune && !AlreadyTrackingCreature(realizedCreature) && (!flag || Mathf.InverseLerp(0f, 20f, realizedCreature.firstChunk.vel.magnitude) >= 0.32f + UnityEngine.Random.value * 0.09f))
					{
						trackedCreatures.Add(new CreatureAndPull(realizedCreature));
					}
				}
			}
			for (int num = trackedCreatures.Count - 1; num >= 0; num--)
			{
				if (trackedCreatures[num].creature.slatedForDeletetion || trackedCreatures[num].creature.enteringShortCut.HasValue || trackedCreatures[num].creature.room != wormGrass.room || (ModManager.MSC && trackedCreatures[num].creature.WormGrassGooduckyImmune && UnityEngine.Random.value < 0.1f && trackedCreatures[num].creature.firstChunk.vel.magnitude < 1f))
				{
					trackedCreatures.RemoveAt(num);
				}
				else
				{
					InteractWithCreature(trackedCreatures[num]);
				}
			}
			if (GrassLight != null)
			{
				GrassLight.stayAlive = true;
				GrassLight.HardSetAlpha(GrassLightIntensity() / 2f);
				GrassLight.color = GrassLightColor();
			}
		}

		private void InteractWithCreature(CreatureAndPull creatureAndPull)
		{
			creatureAndPull.Update();
			if (creatureAndPull.bury == 0f)
			{
				if (creatureAndPull.creature.mainBodyChunk.pos.x < (float)LeftTile.x * 20f - 200f || creatureAndPull.creature.mainBodyChunk.pos.x > (float)RightTile.x * 20f + 200f || creatureAndPull.creature.mainBodyChunk.pos.y > (float)highestTile * 20f + 300f)
				{
					LoseGrip(creatureAndPull);
					return;
				}
				bool flag = false;
				int num = wormGrass.room.GetTilePosition(creatureAndPull.creature.mainBodyChunk.pos).x - LeftTile.x;
				if (num > 0 && num < tiles.Count - 1)
				{
					for (int i = 0; i < creatureAndPull.creature.bodyChunks.Length; i++)
					{
						if (flag)
						{
							break;
						}
						flag = wormGrass.room.VisualContact(tiles[num], wormGrass.room.GetTilePosition(creatureAndPull.creature.bodyChunks[i].pos));
					}
				}
				for (int j = 0; j < tiles.Count; j++)
				{
					if (flag)
					{
						break;
					}
					flag = wormGrass.room.VisualContact(tiles[j], wormGrass.room.GetTilePosition(creatureAndPull.creature.bodyChunks[UnityEngine.Random.Range(0, creatureAndPull.creature.bodyChunks.Length)].pos));
				}
				if (!flag)
				{
					LoseGrip(creatureAndPull);
					return;
				}
			}
			bool flag2 = false;
			if (creatureAndPull.bury > 0f)
			{
				flag2 = true;
				if (!creatureAndPull.creature.dead)
				{
					if (ModManager.CoopAvailable && creatureAndPull.creature is Player player)
					{
						player.PermaDie();
					}
					else
					{
						creatureAndPull.creature.Die();
					}
				}
				creatureAndPull.Consume();
				creatureAndPull.creature.CollideWithTerrain = false;
			}
			bool flag3 = false;
			for (int k = 0; k < creatureAndPull.creature.bodyChunks.Length; k++)
			{
				float num2 = float.MinValue;
				int num3 = -1;
				for (int l = 0; l < tiles.Count; l++)
				{
					if (num3 != -1)
					{
						break;
					}
					if (tiles[l].x == wormGrass.room.GetTilePosition(creatureAndPull.creature.bodyChunks[k].pos).x)
					{
						num3 = l;
					}
				}
				float f = 0f;
				if (num3 > -1)
				{
					num2 = wormGrass.room.MiddleOfTile(tiles[num3]).y - 10f;
					float value = creatureAndPull.creature.bodyChunks[k].pos.y - creatureAndPull.creature.bodyChunks[k].rad - num2;
					f = Mathf.Pow(Mathf.InverseLerp(sizes[num3, 0] * 50f, sizes[num3, 0] * 50f * 0.5f, value), 6f) * sizes[num3, 1];
					if (creatureAndPull.creature.Consious && creatureAndPull.creature.abstractCreature.creatureTemplate.type != CreatureTemplate.Type.Slugcat)
					{
						for (int m = wormGrass.room.GetTilePosition(creatureAndPull.creature.bodyChunks[k].pos).y; m <= wormGrass.room.TileHeight; m++)
						{
							if (wormGrass.room.aimap != null && wormGrass.room.aimap.TileAccessibleToCreature(new IntVector2(tiles[num3].x, m), creatureAndPull.creature.Template))
							{
								creatureAndPull.creature.bodyChunks[k].vel.y = Mathf.Lerp(creatureAndPull.creature.bodyChunks[k].vel.y, 14f, Mathf.Pow(f, 0.2f) * Mathf.Pow(1f - creatureAndPull.pull, 0.8f));
								break;
							}
						}
					}
				}
				f = Mathf.Pow(f, 1f + creatureAndPull.creature.TotalMass);
				if (f > 0f)
				{
					flag3 = true;
					creatureAndPull.pull = Mathf.Min(1f, creatureAndPull.pull + 1f / Mathf.Lerp(800f, 140f, f / creatureAndPull.creature.TotalMass));
				}
				if (creatureAndPull.creature.grabbedBy.Count > 0)
				{
					flag3 = false;
					creatureAndPull.pull = 0f;
				}
				f *= creatureAndPull.pull;
				creatureAndPull.creature.bodyChunks[k].vel.x *= Mathf.Lerp(1f, 0f, Mathf.Pow(f, 6f));
				creatureAndPull.creature.bodyChunks[k].vel.y -= 10f * f;
				creatureAndPull.creature.bodyChunks[k].vel += Custom.RNV() * creatureAndPull.bury * 3f * f;
				if (creatureAndPull.creature is TubeWorm)
				{
					f = 0.2f;
				}
				if (creatureAndPull.pull >= 1f)
				{
					creatureAndPull.bury = Mathf.Min(1f, creatureAndPull.bury + 1f / Mathf.Lerp(600f, 40f, f / creatureAndPull.creature.TotalMass));
				}
				if (flag2)
				{
					if (creatureAndPull.bury < 1f && wormGrass.room.GetTile(creatureAndPull.creature.bodyChunks[k].pos + new Vector2(0f, -20f)).Solid && creatureAndPull.creature.bodyChunks[k].pos.y < num2 + creatureAndPull.creature.bodyChunks[k].rad)
					{
						creatureAndPull.creature.bodyChunks[k].pos.y = num2 + creatureAndPull.creature.bodyChunks[k].rad;
						flag2 = false;
					}
					if (creatureAndPull.bury < 1f || creatureAndPull.creature.bodyChunks[k].pos.y > num2 + creatureAndPull.creature.bodyChunks[k].rad - 1f)
					{
						flag2 = false;
					}
				}
			}
			if (!flag3)
			{
				LoseGrip(creatureAndPull);
			}
			else if (flag2)
			{
				creatureAndPull.creature.Destroy();
			}
		}

		private void LoseGrip(CreatureAndPull creatureAndPull)
		{
			if (!(creatureAndPull.bury > 0f))
			{
				creatureAndPull.bury = Mathf.Max(0f, creatureAndPull.bury - 0.1f);
				creatureAndPull.pull = Mathf.Max(0f, creatureAndPull.pull - 0.1f);
			}
		}

		public bool ShouldTileBeAdded(IntVector2 tile)
		{
			foreach (IntVector2 tile2 in tiles)
			{
				if ((tile.x == tile2.x - 1 || tile.x == tile2.x + 1) && Math.Abs(tile.y - tile2.y) < 2)
				{
					return true;
				}
			}
			return false;
		}

		public void SortTiles()
		{
			tiles.Sort((IntVector2 A, IntVector2 B) => A.x.CompareTo(B.x));
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState(tiles[0].x + tiles[0].y + wormGrass.room.abstractRoom.index);
			sizes = new float[tiles.Count, 2];
			cosmeticWormPositions = new Vector2[tiles.Count][];
			cosmeticWormLengths = new float[tiles.Count][,];
			bool flag = wormGrass.room.GetTile(LeftTile + new IntVector2(-1, 0)).Solid && wormGrass.room.GetTile(LeftTile + new IntVector2(-1, 1)).Solid;
			bool flag2 = wormGrass.room.GetTile(RightTile + new IntVector2(1, 0)).Solid && wormGrass.room.GetTile(RightTile + new IntVector2(1, 1)).Solid;
			highestTile = int.MinValue;
			for (int i = 0; i < tiles.Count; i++)
			{
				sizes[i, 0] = Mathf.Sin(Position(tiles[i]) * (float)Math.PI);
				if (flag)
				{
					sizes[i, 0] = Mathf.Lerp(sizes[i, 0], Mathf.Pow(1f - Position(tiles[i]), 2f), 1f - Position(tiles[i]));
				}
				if (flag2)
				{
					sizes[i, 0] = Mathf.Lerp(sizes[i, 0], Mathf.Pow(Position(tiles[i]), 2f), Position(tiles[i]));
				}
				sizes[i, 1] = Mathf.Min(Mathf.InverseLerp(3f, 11f, i), Mathf.InverseLerp(tiles.Count - 1 - 3, tiles.Count - 1 - 11, i));
				sizes[i, 0] = Mathf.Lerp(sizes[i, 0], 5f, sizes[i, 1] * 0.5f);
				if (tiles[i].y > highestTile)
				{
					highestTile = tiles[i].y;
				}
				for (int j = 0; (float)j < Mathf.Lerp(2f, 8f, sizes[i, 0] / 5f); j++)
				{
					wormGrass.room.GetTile(tiles[i].x, tiles[i].y + j).wormGrass = true;
				}
			}
			worms = new List<Worm>();
			for (int k = 0; k < tiles.Count; k++)
			{
				float num = wormGrass.room.MiddleOfTile(tiles[k]).y + sizes[k, 0] * 30f;
				float a = num;
				if (k > 0)
				{
					a = wormGrass.room.MiddleOfTile(tiles[k - 1]).y + sizes[k - 1, 0] * 30f;
				}
				float b = num;
				if (k < tiles.Count - 1)
				{
					b = wormGrass.room.MiddleOfTile(tiles[k + 1]).y + sizes[k + 1, 0] * 30f;
				}
				int num2 = (int)Mathf.Lerp(3f, 10f, sizes[k, 0] * Mathf.Lerp(1f, 0.3f, sizes[k, 1]));
				List<Vector2> list = new List<Vector2>();
				List<float> list2 = new List<float>();
				List<float> list3 = new List<float>();
				for (int l = 0; l < num2; l++)
				{
					float num3 = ((float)l + 0.5f) / (float)num2;
					Vector2 vector = wormGrass.room.MiddleOfTile(tiles[k]) + new Vector2(-10f + 20f * num3 + Mathf.Lerp(-4f, 4f, UnityEngine.Random.value), -10f);
					float num4 = Mathf.Lerp(num, Mathf.Lerp(a, b, num3), 0.5f);
					float num5 = Mathf.Lerp(0.2f, 1f, Mathf.Pow(UnityEngine.Random.value, 0.8f));
					if (UnityEngine.Random.value < sizes[k, 1])
					{
						list.Add(vector);
						list2.Add(num4);
						list3.Add(num5);
					}
					else
					{
						Worm worm = new Worm(wormGrass, this, vector, num4, sizes[k, 1], num5, cosmeticOnly: false);
						worms.Add(worm);
						wormGrass.worms.Add(worm);
						wormGrass.room.AddObject(worm);
					}
				}
				cosmeticWormPositions[k] = list.ToArray();
				cosmeticWormLengths[k] = new float[list.Count, 2];
				for (int m = 0; m < cosmeticWormLengths[k].GetLength(0); m++)
				{
					cosmeticWormLengths[k][m, 0] = list2[m];
					cosmeticWormLengths[k][m, 1] = list3[m];
				}
			}
			UnityEngine.Random.state = state;
		}

		public float Position(IntVector2 tile)
		{
			return Mathf.InverseLerp(LeftTile.x, RightTile.x, tile.x);
		}

		public float Size(IntVector2 tile)
		{
			for (int i = 0; i < tiles.Count; i++)
			{
				if (tiles[i] == tile)
				{
					return sizes[i, 0];
				}
			}
			return 0f;
		}

		public float IndividualFactor(IntVector2 tile)
		{
			for (int i = 0; i < tiles.Count; i++)
			{
				if (tiles[i] == tile)
				{
					return sizes[i, 1];
				}
			}
			return 1f;
		}

		private float GrassLightIntensity()
		{
			return Mathf.InverseLerp(1000f, 2000f, (float)wormGrass.room.world.rainCycle.dayNightCounter * Mathf.InverseLerp(0f, 30f, worms.Count));
		}

		private Color GrassLightColor()
		{
			return Color.Lerp(Color.black, new Color(0.17f, 0.38f, 0.17f), GrassLightIntensity() * 0.72f);
		}

		public void InitRegionalLight(bool turnOn)
		{
			if (turnOn)
			{
				Vector2 initPos = Vector2.Lerp(worms[0].pos, worms[worms.Count - 1].pos, 0.5f);
				initPos += new Vector2(0f, -40f);
				GrassLight = new LightSource(initPos, environmentalLight: false, GrassLightColor(), wormGrass);
				GrassLight.HardSetAlpha(GrassLightIntensity() / 2f);
				GrassLight.HardSetRad(Mathf.Max(190f, (float)worms.Count * 3f));
				GrassLight.fadeWithSun = false;
				GrassLight.noGameplayImpact = true;
				GrassLight.colorFromEnvironment = false;
				GrassLight.requireUpKeep = true;
				wormGrass.room.AddObject(GrassLight);
			}
		}
	}

	public new Room room;

	public List<WormGrassPatch> patches;

	public List<Worm> worms;

	public List<Worm> cosmeticWorms;

	public int[,] cameraPositions;

	public List<PhysicalObject> repulsiveObjects;

	public WormGrass(Room room, List<IntVector2> tiles)
	{
		this.room = room;
		if (room.game != null)
		{
			cameraPositions = new int[room.game.cameras.Length, 2];
		}
		else
		{
			cameraPositions = new int[1, 2];
		}
		for (int i = 0; i < cameraPositions.GetLength(0); i++)
		{
			cameraPositions[i, 0] = -1;
		}
		patches = new List<WormGrassPatch>();
		while (tiles.Count > 0)
		{
			IntVector2 intVector = tiles[0];
			tiles.RemoveAt(0);
			bool flag = false;
			for (int j = 0; j < patches.Count; j++)
			{
				if (patches[j].ShouldTileBeAdded(intVector))
				{
					patches[j].tiles.Add(intVector);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				patches.Add(new WormGrassPatch(this, intVector));
			}
		}
		worms = new List<Worm>();
		cosmeticWorms = new List<Worm>();
		repulsiveObjects = new List<PhysicalObject>();
		for (int k = 0; k < patches.Count; k++)
		{
			patches[k].SortTiles();
			if (ModManager.MSC)
			{
				patches[k].InitRegionalLight(room.game != null && room.world.region != null && room.world.region.name == "OE");
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		for (int i = 0; i < patches.Count; i++)
		{
			patches[i].Update();
		}
		for (int j = 0; j < room.game.cameras.Length; j++)
		{
			if (room.game.cameras[j].room != null)
			{
				if (cameraPositions[j, 0] != room.game.cameras[j].room.abstractRoom.index || cameraPositions[j, 1] != room.game.cameras[j].currentCameraPosition)
				{
					NewCameraPos();
				}
				cameraPositions[j, 0] = room.game.cameras[j].room.abstractRoom.index;
				cameraPositions[j, 1] = room.game.cameras[j].currentCameraPosition;
			}
		}
	}

	private void NewCameraPos()
	{
		List<Worm> list = new List<Worm>();
		for (int i = 0; i < cosmeticWorms.Count; i++)
		{
			list.Add(cosmeticWorms[i]);
		}
		cosmeticWorms.Clear();
		int num = 0;
		int num2 = 0;
		for (int j = 0; j < patches.Count; j++)
		{
			for (int k = 0; k < patches[j].tiles.Count; k++)
			{
				if (!room.ViewedByAnyCamera(room.MiddleOfTile(patches[j].tiles[k]), 200f))
				{
					continue;
				}
				for (int l = 0; l < patches[j].cosmeticWormPositions[k].Length; l++)
				{
					if (list.Count > 0)
					{
						Worm worm = list[0];
						list.RemoveAt(0);
						worm.Reset(patches[j], patches[j].cosmeticWormPositions[k][l], patches[j].sizes[k, 1], patches[j].cosmeticWormLengths[k][l, 0], patches[j].cosmeticWormLengths[k][l, 1]);
						cosmeticWorms.Add(worm);
						num++;
					}
					else
					{
						Worm worm2 = new Worm(this, patches[j], patches[j].cosmeticWormPositions[k][l], patches[j].cosmeticWormLengths[k][l, 0], patches[j].sizes[k, 1], patches[j].cosmeticWormLengths[k][l, 1], cosmeticOnly: true);
						cosmeticWorms.Add(worm2);
						room.AddObject(worm2);
						num2++;
					}
				}
			}
		}
		for (int m = 0; m < list.Count; m++)
		{
			list[m].Destroy();
		}
	}

	public void AddNewRepulsiveObject(PhysicalObject obj)
	{
		if (!repulsiveObjects.Contains(obj))
		{
			repulsiveObjects.Add(obj);
		}
	}

	public bool IsTileAccessible(IntVector2 tile, CreatureTemplate crit)
	{
		if (crit.wormGrassImmune || crit.wormgrassTilesIgnored)
		{
			return true;
		}
		return !room.GetTile(tile).wormGrass;
	}

	public void Explosion(Explosion explosion)
	{
		for (int i = 0; i < worms.Count; i++)
		{
			if (Custom.DistLess(worms[i].pos, explosion.pos, explosion.rad * 2f))
			{
				float num = Mathf.InverseLerp(explosion.rad * 2f, explosion.rad, Vector2.Distance(worms[i].pos, explosion.pos));
				if (UnityEngine.Random.value < num)
				{
					worms[i].vel += Custom.DirVec(explosion.pos, worms[i].pos) * explosion.force * 2f * num;
					worms[i].excitement = 0f;
					worms[i].focusCreature = null;
					worms[i].dragForce = 0f;
					worms[i].attachedChunk = null;
				}
			}
		}
		for (int j = 0; j < cosmeticWorms.Count; j++)
		{
			if (Custom.DistLess(cosmeticWorms[j].pos, explosion.pos, explosion.rad * 2f))
			{
				float num2 = Mathf.InverseLerp(explosion.rad * 2f, explosion.rad, Vector2.Distance(cosmeticWorms[j].pos, explosion.pos));
				if (UnityEngine.Random.value < num2)
				{
					cosmeticWorms[j].vel += Custom.DirVec(explosion.pos, cosmeticWorms[j].pos) * explosion.force * 2f * num2;
					cosmeticWorms[j].excitement = 0f;
					cosmeticWorms[j].focusCreature = null;
					cosmeticWorms[j].dragForce = 0f;
					cosmeticWorms[j].attachedChunk = null;
				}
			}
		}
	}
}
