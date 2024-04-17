using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class ItemTracker : AIModule
{
	public class ItemRepresentation
	{
		public ItemTracker parent;

		public AbstractPhysicalObject representedItem;

		public int forgetCounter;

		public int age;

		private bool visualContact;

		private int ticksSinceSeen;

		public bool deleteMeNextFrame;

		public float priority;

		public WorldCoordinate lastSeenCoord;

		private DebugSprite dbSpr;

		public bool VisualContact
		{
			get
			{
				if (representedItem.realizedObject == null || representedItem.realizedObject.room != parent.AI.creature.Room.realizedRoom)
				{
					visualContact = false;
				}
				return visualContact;
			}
		}

		public int TicksSinceSeen
		{
			get
			{
				if (ticksSinceSeen < parent.seeAroundCorners)
				{
					return 0;
				}
				return ticksSinceSeen - parent.seeAroundCorners;
			}
		}

		public void MakeVisible()
		{
			ticksSinceSeen = 0;
			visualContact = true;
		}

		public ItemRepresentation(ItemTracker parent, AbstractPhysicalObject representedItem, float priority)
		{
			this.parent = parent;
			this.representedItem = representedItem;
			this.priority = priority;
			lastSeenCoord = representedItem.pos.WashNode();
			visualContact = true;
			if (parent.visualize)
			{
				dbSpr = new DebugSprite(representedItem.realizedObject.firstChunk.pos, new FSprite("Circle20"), parent.AI.creature.Room.realizedRoom);
				dbSpr.sprite.color = new Color(1f, 0f, 0f);
				dbSpr.sprite.scale = 0.7f;
				parent.AI.creature.Room.realizedRoom.AddObject(dbSpr);
			}
		}

		public virtual void Update()
		{
			age++;
			ticksSinceSeen++;
			if (ticksSinceSeen > parent.seeAroundCorners)
			{
				visualContact = false;
				if (!representedItem.InDen && representedItem.pos.room == parent.AI.creature.pos.room && representedItem.realizedObject != null)
				{
					BodyChunk[] bodyChunks = representedItem.realizedObject.bodyChunks;
					foreach (BodyChunk chunk in bodyChunks)
					{
						if (parent.AI.VisualContact(chunk))
						{
							ticksSinceSeen = 0;
							visualContact = true;
							break;
						}
					}
				}
			}
			if (visualContact)
			{
				if (representedItem.realizedObject == null || representedItem.pos.room != parent.AI.creature.pos.room)
				{
					visualContact = false;
					return;
				}
				lastSeenCoord = representedItem.pos.WashNode();
				forgetCounter = 0;
				if (representedItem.realizedObject != null && representedItem.realizedObject is Weapon && (representedItem.realizedObject as Weapon).mode == Weapon.Mode.Thrown)
				{
					(parent.AI as IUseItemTracker).SeeThrownWeapon(representedItem.realizedObject, (representedItem.realizedObject as Weapon).thrownBy);
				}
				if (parent.stopTrackingCarried && representedItem.realizedObject.grabbedBy.Count > 0)
				{
					Destroy();
				}
				if (representedItem.realizedObject.slatedForDeletetion)
				{
					Destroy();
				}
			}
			else
			{
				forgetCounter++;
				if (parent.framesToRememberItems > -1 && forgetCounter > parent.framesToRememberItems)
				{
					Destroy();
				}
			}
			if (parent.visualize)
			{
				dbSpr.pos = parent.AI.creature.Room.realizedRoom.MiddleOfTile(BestGuessForPosition());
			}
		}

		public virtual WorldCoordinate BestGuessForPosition()
		{
			return lastSeenCoord;
		}

		public void Destroy()
		{
			if (parent.visualize)
			{
				dbSpr.Destroy();
			}
			deleteMeNextFrame = true;
		}
	}

	private List<ItemRepresentation> items;

	private int seeAroundCorners;

	public int maxTrackedItems;

	public bool visualize;

	public int framesToRememberItems;

	public int forgetDistance;

	public bool stopTrackingCarried;

	public int ItemCount => items.Count;

	public ItemRepresentation GetRep(int index)
	{
		return items[index];
	}

	public ItemTracker(ArtificialIntelligence AI, int seeAroundCorners, int maxTrackedItems, int framesToRememberItems, int forgetDistance, bool stopTrackingCarried)
		: base(AI)
	{
		this.seeAroundCorners = seeAroundCorners;
		this.maxTrackedItems = maxTrackedItems;
		this.framesToRememberItems = framesToRememberItems;
		this.forgetDistance = forgetDistance;
		this.stopTrackingCarried = stopTrackingCarried;
		items = new List<ItemRepresentation>();
		visualize = false;
	}

	public override void Update()
	{
		AbstractWorldEntity abstractWorldEntity = null;
		if (AI.creature.Room.entities.Count > 0)
		{
			abstractWorldEntity = AI.creature.Room.entities[Random.Range(0, AI.creature.Room.entities.Count)];
			if (!(abstractWorldEntity is AbstractPhysicalObject) || abstractWorldEntity is AbstractCreature)
			{
				abstractWorldEntity = null;
			}
		}
		bool flag = true;
		for (int num = items.Count - 1; num >= 0; num--)
		{
			items[num].Update();
			if (items[num].representedItem.Equals(abstractWorldEntity))
			{
				flag = false;
			}
			if (items[num].deleteMeNextFrame)
			{
				items.RemoveAt(num);
			}
		}
		if (flag && abstractWorldEntity != null && (abstractWorldEntity as AbstractPhysicalObject).realizedObject != null)
		{
			BodyChunk[] bodyChunks = (abstractWorldEntity as AbstractPhysicalObject).realizedObject.bodyChunks;
			foreach (BodyChunk chunk in bodyChunks)
			{
				if (AI.VisualContact(chunk))
				{
					ItemNoticed(abstractWorldEntity as AbstractPhysicalObject);
					break;
				}
			}
		}
		if (items.Count > 0 && forgetDistance > -1)
		{
			ItemRepresentation itemRepresentation = items[Random.Range(0, items.Count)];
			if (!itemRepresentation.VisualContact && !Custom.DistLess(AI.creature.pos, itemRepresentation.BestGuessForPosition(), forgetDistance))
			{
				itemRepresentation.Destroy();
			}
		}
	}

	public void SeeItem(AbstractPhysicalObject item)
	{
		bool flag = false;
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].representedItem == item)
			{
				items[i].MakeVisible();
				flag = true;
			}
		}
		if (!flag)
		{
			ItemNoticed(item);
		}
	}

	public ItemRepresentation RepresentationForObject(PhysicalObject obj, bool AddIfMissing)
	{
		ItemRepresentation itemRepresentation = null;
		foreach (ItemRepresentation item in items)
		{
			if (item.representedItem.realizedObject == obj)
			{
				itemRepresentation = item;
				break;
			}
		}
		if (itemRepresentation == null && AddIfMissing)
		{
			itemRepresentation = ItemNoticed(obj.abstractPhysicalObject);
		}
		return itemRepresentation;
	}

	private ItemRepresentation ItemNoticed(AbstractPhysicalObject item)
	{
		if (!(AI as IUseItemTracker).TrackItem(item))
		{
			return null;
		}
		if (stopTrackingCarried && item.realizedObject != null && item.realizedObject.grabbedBy.Count > 0)
		{
			return null;
		}
		ItemRepresentation itemRepresentation = new ItemRepresentation(this, item, 0f);
		items.Add(itemRepresentation);
		if (items.Count > maxTrackedItems)
		{
			float num = float.MaxValue;
			ItemRepresentation itemRepresentation2 = null;
			foreach (ItemRepresentation item2 in items)
			{
				float num2 = item2.priority * 100000f + (item2.VisualContact ? 2f : 1f) / (1f + Vector2.Distance(IntVector2.ToVector2(item2.BestGuessForPosition().Tile), IntVector2.ToVector2(AI.creature.pos.Tile)));
				num2 /= Mathf.Lerp(item2.forgetCounter, 100f, 0.7f);
				if (num2 < num)
				{
					num = num2;
					itemRepresentation2 = item2;
				}
			}
			if (itemRepresentation2 == itemRepresentation)
			{
				itemRepresentation = null;
			}
			itemRepresentation2.Destroy();
		}
		return itemRepresentation;
	}
}
