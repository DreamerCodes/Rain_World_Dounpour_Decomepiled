using System.Collections.Generic;
using System.Globalization;
using RWCustom;
using UnityEngine;

public class PebblesPearl : DataPearl
{
	public class AbstractPebblesPearl : AbstractDataPearl
	{
		public int color;

		public int number;

		public AbstractPebblesPearl(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableData, int color, int number)
			: base(world, AbstractObjectType.PebblesPearl, realizedObject, pos, ID, originRoom, placedObjectIndex, consumableData, DataPearlType.PebblesPearl)
		{
			this.color = ((ModManager.MSC && number < 0) ? (-color) : color);
			if (this.color == 0 && ModManager.MSC && number < 0)
			{
				this.color = -3;
			}
			this.number = number;
		}

		public override string ToString()
		{
			string baseString = base.ToString() + string.Format(CultureInfo.InvariantCulture, "<oA>{0}<oA>{1}", color, number);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "<oA>", unrecognizedAttributes);
		}
	}

	public PhysicalObject orbitObj;

	public List<PebblesPearl> otherMarbles;

	public Vector2? hoverPos;

	public float orbitAngle;

	public float orbitSpeed;

	public float orbitDistance;

	public float orbitFlattenAngle;

	public float orbitFlattenFac;

	public int orbitCircle;

	public int marbleColor;

	private bool lookForMarbles;

	public GlyphLabel label;

	public Oracle oracle;

	public int marbleIndex;

	public bool NotCarried => grabbedBy.Count == 0;

	public PebblesPearl(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject, world)
	{
		otherMarbles = new List<PebblesPearl>();
		orbitAngle = Random.value * 360f;
		orbitSpeed = 3f;
		orbitDistance = 50f;
		collisionLayer = 0;
		orbitFlattenAngle = Random.value * 360f;
		orbitFlattenFac = 0.5f + Random.value * 0.5f;
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		UpdateOtherMarbles();
	}

	private void UpdateOtherMarbles()
	{
		for (int i = 0; i < otherMarbles.Count; i++)
		{
			otherMarbles[i].otherMarbles.Remove(this);
		}
		otherMarbles.Clear();
		for (int j = 0; j < room.physicalObjects[collisionLayer].Count; j++)
		{
			if (room.physicalObjects[collisionLayer][j] is PebblesPearl && room.physicalObjects[collisionLayer][j] != this)
			{
				if (!(room.physicalObjects[collisionLayer][j] as PebblesPearl).otherMarbles.Contains(this))
				{
					(room.physicalObjects[collisionLayer][j] as PebblesPearl).otherMarbles.Add(this);
				}
				if (!otherMarbles.Contains(room.physicalObjects[collisionLayer][j] as PebblesPearl))
				{
					otherMarbles.Add(room.physicalObjects[collisionLayer][j] as PebblesPearl);
				}
			}
		}
	}

	public override void Update(bool eu)
	{
		if (!lookForMarbles)
		{
			UpdateOtherMarbles();
			lookForMarbles = true;
		}
		if (oracle != null && oracle.room != room)
		{
			oracle = null;
		}
		abstractPhysicalObject.destroyOnAbstraction = oracle != null;
		if (label != null)
		{
			label.setPos = base.firstChunk.pos;
			if (label.room != room)
			{
				label.Destroy();
			}
		}
		else if (!ModManager.MSC || room.world.name != "RM")
		{
			label = new GlyphLabel(base.firstChunk.pos, GlyphLabel.RandomString(1, 1, 12842 + (abstractPhysicalObject as AbstractPebblesPearl).number, cyrillic: false));
			room.AddObject(label);
		}
		base.Update(eu);
		float num = orbitAngle;
		float num2 = orbitSpeed;
		float num3 = orbitDistance;
		float axis = orbitFlattenAngle;
		float num4 = orbitFlattenFac;
		if (room.gravity < 1f && NotCarried && oracle != null && (!ModManager.MSC || room.world.name != "RM"))
		{
			if (ModManager.MSC && oracle != null && oracle.marbleOrbiting)
			{
				float num5 = (float)marbleIndex / (float)oracle.marbles.Count;
				num = 360f * num5 + (float)oracle.behaviorTime * 0.1f;
				Vector2 vector = new Vector2(oracle.room.PixelWidth / 2f, oracle.room.PixelHeight / 2f) + Custom.DegToVec(num) * 275f;
				base.firstChunk.vel *= Custom.LerpMap(base.firstChunk.vel.magnitude, 1f, 6f, 0.999f, 0.9f);
				base.firstChunk.vel += Vector2.ClampMagnitude(vector - base.firstChunk.pos, 100f) / 100f * 0.4f * (1f - room.gravity);
			}
			else
			{
				Vector2 vector2 = base.firstChunk.pos;
				if (orbitObj != null)
				{
					int num6 = 0;
					int num7 = 0;
					int number = abstractPhysicalObject.ID.number;
					for (int i = 0; i < otherMarbles.Count; i++)
					{
						if (otherMarbles[i].orbitObj == orbitObj && otherMarbles[i].NotCarried && Custom.DistLess(otherMarbles[i].firstChunk.pos, orbitObj.firstChunk.pos, otherMarbles[i].orbitDistance * 4f) && otherMarbles[i].orbitCircle == orbitCircle)
						{
							num3 += otherMarbles[i].orbitDistance;
							if (otherMarbles[i].abstractPhysicalObject.ID.number < abstractPhysicalObject.ID.number)
							{
								num7++;
							}
							num6++;
							if (otherMarbles[i].abstractPhysicalObject.ID.number < number)
							{
								number = otherMarbles[i].abstractPhysicalObject.ID.number;
								num = otherMarbles[i].orbitAngle;
								num2 = otherMarbles[i].orbitSpeed;
								axis = otherMarbles[i].orbitFlattenAngle;
								num4 = otherMarbles[i].orbitFlattenFac;
							}
						}
					}
					num3 /= (float)(1 + num6);
					num += (float)num7 * (360f / (float)(num6 + 1));
					Vector2 vector3 = orbitObj.firstChunk.pos;
					if (orbitObj is Oracle && orbitObj.graphicsModule != null)
					{
						vector3 = (orbitObj.graphicsModule as OracleGraphics).halo.Center(1f);
					}
					vector2 = vector3 + Custom.FlattenVectorAlongAxis(Custom.DegToVec(num), axis, num4) * num3 * Mathf.Lerp(1f / num4, 1f, 0.5f);
				}
				else if (hoverPos.HasValue)
				{
					vector2 = hoverPos.Value;
				}
				base.firstChunk.vel *= Custom.LerpMap(base.firstChunk.vel.magnitude, 1f, 6f, 0.999f, 0.9f);
				base.firstChunk.vel += Vector2.ClampMagnitude(vector2 - base.firstChunk.pos, 100f) / 100f * 0.4f * (1f - room.gravity);
			}
		}
		orbitAngle += num2 * ((orbitCircle % 2 == 0) ? 1f : (-1f));
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (firstContact && speed > 2f)
		{
			room.PlaySound(SoundID.SS_AI_Marble_Hit_Floor, base.firstChunk, loop: false, Custom.LerpMap(speed, 0f, 8f, 0.2f, 1f), 1f);
		}
	}
}
