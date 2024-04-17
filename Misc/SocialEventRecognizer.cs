using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class SocialEventRecognizer
{
	public class EventID : ExtEnum<EventID>
	{
		public static readonly EventID LethalAttackAttempt = new EventID("LethalAttackAttempt", register: true);

		public static readonly EventID LethalAttack = new EventID("LethalAttack", register: true);

		public static readonly EventID NonLethalAttackAttempt = new EventID("NonLethalAttackAttempt", register: true);

		public static readonly EventID NonLethalAttack = new EventID("NonLethalAttack", register: true);

		public static readonly EventID Theft = new EventID("Theft", register: true);

		public static readonly EventID Killing = new EventID("Killing", register: true);

		public static readonly EventID ItemOffering = new EventID("ItemOffering", register: true);

		public static readonly EventID ItemTransaction = new EventID("ItemTransaction", register: true);

		public EventID(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class OwnedItemOnGround
	{
		public SocialEventRecognizer socEventRecognizer;

		public PhysicalObject item;

		public Creature owner;

		public Creature offeredTo;

		public bool active;

		public int age;

		public bool offered;

		public OwnedItemOnGround(SocialEventRecognizer socEventRecognizer, PhysicalObject item, Creature owner)
		{
			this.socEventRecognizer = socEventRecognizer;
			this.item = item;
			this.owner = owner;
			active = true;
		}

		public void Update()
		{
			age++;
			if (owner.Template.type == CreatureTemplate.Type.Slugcat && !offered && !Custom.DistLess(owner.DangerPos, item.firstChunk.pos, 50f))
			{
				offered = true;
				offeredTo = socEventRecognizer.ItemOffered(owner, item, null);
			}
			if (age > ((offeredTo != null) ? 30 : 10) * ((item is Creature && !(item as Creature).dead) ? 20 : 40))
			{
				active = false;
			}
			if (item.grabbedBy.Count > 0)
			{
				if (offeredTo != null && item.grabbedBy[0].grabber == offeredTo)
				{
					socEventRecognizer.SocialEvent(EventID.ItemTransaction, owner, offeredTo, item);
					if (item.grabbedBy[0].grabber.abstractCreature.abstractAI != null && item.grabbedBy[0].grabber.abstractCreature.abstractAI.RealAI != null && item.grabbedBy[0].grabber.abstractCreature.abstractAI.RealAI.friendTracker != null && item.grabbedBy[0].grabber.abstractCreature.abstractAI.RealAI.friendTracker.giftOfferedToMe != null && item.grabbedBy[0].grabber.abstractCreature.abstractAI.RealAI.friendTracker.giftOfferedToMe == this)
					{
						item.grabbedBy[0].grabber.abstractCreature.abstractAI.RealAI.friendTracker.GiftRecieved();
					}
				}
				active = false;
			}
			if (owner.abstractCreature.Room.index != socEventRecognizer.room.abstractRoom.index)
			{
				active = false;
			}
		}
	}

	private static readonly AGLog<SocialEventRecognizer> Log = new AGLog<SocialEventRecognizer>();

	public Room room;

	public List<OwnedItemOnGround> ownedItemsOnGround;

	public List<EntityID> stolenProperty;

	public SocialEventRecognizer(Room room)
	{
		this.room = room;
		ownedItemsOnGround = new List<OwnedItemOnGround>();
		stolenProperty = new List<EntityID>();
	}

	public void Update()
	{
		for (int num = ownedItemsOnGround.Count - 1; num >= 0; num--)
		{
			if (!ownedItemsOnGround[num].active)
			{
				ownedItemsOnGround.RemoveAt(num);
			}
			else
			{
				ownedItemsOnGround[num].Update();
			}
		}
	}

	public void CreaturePutItemOnGround(PhysicalObject item, Creature creature)
	{
		for (int i = 0; i < ownedItemsOnGround.Count; i++)
		{
			if (ownedItemsOnGround[i].item == item)
			{
				return;
			}
		}
		ownedItemsOnGround.Add(new OwnedItemOnGround(this, item, creature));
	}

	public void WeaponAttack(PhysicalObject weapon, Creature thrower, Creature victim, bool hit)
	{
		bool flag = weapon is Spear || weapon is ScavengerBomb;
		EventID iD = ((weapon is SporePlant) ? EventID.LethalAttackAttempt : ((!flag) ? (hit ? EventID.NonLethalAttack : EventID.NonLethalAttackAttempt) : (hit ? EventID.LethalAttack : EventID.LethalAttackAttempt)));
		if (thrower is Player && (weapon is ScavengerBomb || hit) && (thrower as Player).SessionRecord != null)
		{
			(thrower as Player).SessionRecord.BreakPeaceful(victim);
		}
		SocialEvent(iD, thrower, victim, weapon);
	}

	public void Theft(PhysicalObject item, Creature theif, Creature victim)
	{
		AddStolenProperty(item.abstractPhysicalObject.ID);
		SocialEvent(EventID.Theft, theif, victim, item);
	}

	public void AddStolenProperty(EntityID ID)
	{
		for (int i = 0; i < stolenProperty.Count; i++)
		{
			if (stolenProperty[i] == ID)
			{
				return;
			}
		}
		stolenProperty.Add(ID);
	}

	public void Killing(Creature killer, Creature victim)
	{
		SocialEvent(EventID.Killing, killer, victim, null);
		if (killer is Player)
		{
			room.game.session.creatureCommunities.InfluenceLikeOfPlayer(victim.Template.communityID, room.world.RegionNumber, (killer as Player).playerState.playerNumber, -0.05f * victim.Template.communityInfluence, 0.25f, 0f);
			if ((killer as Player).SessionRecord != null)
			{
				(killer as Player).SessionRecord.AddKill(victim);
			}
		}
	}

	private void SocialEvent(EventID ID, Creature subjectCreature, Creature objectCreature, PhysicalObject involvedItem)
	{
		for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
		{
			if (room.abstractRoom.creatures[i].realizedCreature != null && !room.abstractRoom.creatures[i].realizedCreature.dead && room.abstractRoom.creatures[i].abstractAI != null && room.abstractRoom.creatures[i].abstractAI.RealAI != null && room.abstractRoom.creatures[i].abstractAI.RealAI is IReactToSocialEvents)
			{
				(room.abstractRoom.creatures[i].abstractAI.RealAI as IReactToSocialEvents).SocialEvent(ID, subjectCreature, objectCreature, involvedItem);
			}
		}
	}

	public Creature ItemOffered(Creature gifter, PhysicalObject item, Creature offeredTo)
	{
		if (gifter.Template.type == CreatureTemplate.Type.Slugcat)
		{
			for (int i = 0; i < stolenProperty.Count; i++)
			{
				if (stolenProperty[i] == item.abstractPhysicalObject.ID)
				{
					return null;
				}
			}
		}
		if (offeredTo == null)
		{
			float dst = 500f;
			for (int j = 0; j < room.abstractRoom.creatures.Count; j++)
			{
				if (room.abstractRoom.creatures[j].realizedCreature != null && room.abstractRoom.creatures[j].realizedCreature != gifter && (!ModManager.MSC || !(room.abstractRoom.creatures[j].realizedCreature is Player) || (room.abstractRoom.creatures[j].realizedCreature as Player).onBack == null) && (!ModManager.MSC || room.abstractRoom.creatures[j].realizedCreature.grabbedBy.Count == 0 || room.abstractRoom.creatures[j].realizedCreature.grabbedBy[0].grabber != gifter) && !room.abstractRoom.creatures[j].realizedCreature.dead && room.abstractRoom.creatures[j].abstractAI != null && room.abstractRoom.creatures[j].abstractAI.RealAI != null && room.abstractRoom.creatures[j].abstractAI.RealAI is IReactToSocialEvents && room.abstractRoom.creatures[j].abstractAI.RealAI.tracker != null && Custom.DistLess(item.firstChunk.pos, room.abstractRoom.creatures[j].realizedCreature.DangerPos, dst))
				{
					Tracker.CreatureRepresentation creatureRepresentation = room.abstractRoom.creatures[j].abstractAI.RealAI.tracker.RepresentationForCreature(gifter.abstractCreature, addIfMissing: false);
					if (creatureRepresentation != null && creatureRepresentation.TicksSinceSeen < 120)
					{
						dst = Vector2.Distance(item.firstChunk.pos, room.abstractRoom.creatures[j].realizedCreature.DangerPos);
						offeredTo = room.abstractRoom.creatures[j].realizedCreature;
					}
				}
			}
		}
		if (offeredTo != null)
		{
			SocialEvent(EventID.ItemOffering, gifter, offeredTo, item);
		}
		return offeredTo;
	}

	public OwnedItemOnGround ItemOwnership(PhysicalObject obj)
	{
		if (ownedItemsOnGround.Count < 1 || obj == null)
		{
			return null;
		}
		for (int i = 0; i < ownedItemsOnGround.Count; i++)
		{
			if (ownedItemsOnGround[i].item == obj)
			{
				return ownedItemsOnGround[i];
			}
		}
		return null;
	}

	public Creature ItemOfferedTo(PhysicalObject obj)
	{
		if (ownedItemsOnGround.Count < 1 || obj == null)
		{
			return null;
		}
		return ItemOwnership(obj)?.offeredTo;
	}
}
