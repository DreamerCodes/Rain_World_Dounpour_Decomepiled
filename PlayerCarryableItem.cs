using RWCustom;
using UnityEngine;

public abstract class PlayerCarryableItem : PhysicalObject
{
	public int blink;

	protected Color color;

	public Vector2? lastOutsideTerrainPos;

	public int forbiddenToPlayer { get; protected set; }

	public virtual float ThrowPowerFactor => 1f;

	public Color blinkColor
	{
		get
		{
			if (room != null && room.PointSubmerged(base.firstChunk.pos))
			{
				return new Color(0f, 0.003921569f, 0f);
			}
			return new Color(1f, 1f, 1f);
		}
	}

	public PlayerCarryableItem(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		lastOutsideTerrainPos = null;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (blink > 0)
		{
			blink--;
		}
		if (forbiddenToPlayer > 0)
		{
			forbiddenToPlayer--;
		}
		if (room.GetTile(base.firstChunk.pos).Solid)
		{
			if (grabbedBy.Count == 0 && base.firstChunk.collideWithTerrain && room.GetTile(base.firstChunk.lastPos).Solid && room.GetTile(base.firstChunk.lastLastPos).Solid && lastOutsideTerrainPos.HasValue && (!(this is Spear) || (this as Spear).mode != Weapon.Mode.StuckInCreature))
			{
				Custom.Log($"Resetting {abstractPhysicalObject.type} to outside terrain");
				for (int i = 0; i < base.bodyChunks.Length; i++)
				{
					base.bodyChunks[i].HardSetPosition(lastOutsideTerrainPos.Value + Custom.RNV() * Random.value);
					base.bodyChunks[i].vel /= 2f;
				}
			}
		}
		else
		{
			lastOutsideTerrainPos = base.firstChunk.pos;
		}
		base.firstChunk.collideWithObjects = grabbedBy.Count < 1;
	}

	public virtual void PickedUp(Creature upPicker)
	{
		room.PlaySound(SoundID.Slugcat_Pick_Up_Misc_Inanimate, base.firstChunk);
	}

	public void Blink()
	{
		if (grabbedBy.Count <= 0)
		{
			if (room != null)
			{
				room.PlaySound(SoundID.UI_Weapon_In_Range_To_Pick_Up, base.firstChunk);
			}
			blink = 5;
		}
	}

	public void Forbid()
	{
		forbiddenToPlayer = 40;
	}
}
