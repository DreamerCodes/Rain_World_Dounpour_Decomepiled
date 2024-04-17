using System.Collections.Generic;

public class TriggeredEvent
{
	public class EventType : ExtEnum<EventType>
	{
		public static readonly EventType MusicEvent = new EventType("MusicEvent", register: true);

		public static readonly EventType StopMusicEvent = new EventType("StopMusicEvent", register: true);

		public static readonly EventType PoleMimicsSubtleReveal = new EventType("PoleMimicsSubtleReveal", register: true);

		public static readonly EventType ShowProjectedImageEvent = new EventType("ShowProjectedImageEvent", register: true);

		public static readonly EventType PickUpObjectInstruction = new EventType("PickUpObjectInstruction", register: true);

		public static readonly EventType RoomSpecificTextMessage = new EventType("RoomSpecificTextMessage", register: true);

		public static readonly EventType BringPlayerGuideToRoom = new EventType("BringPlayerGuideToRoom", register: true);

		public EventType(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public EventType type;

	public List<string> unrecognizedSaveStrings = new List<string>();

	public TriggeredEvent(EventType type)
	{
		this.type = type;
	}

	public override string ToString()
	{
		return type.ToString();
	}

	public virtual void FromString(string[] s)
	{
		type = new EventType(s[0]);
	}
}
