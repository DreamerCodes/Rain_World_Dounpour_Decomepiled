public class OddJobAIModule : AIModule
{
	public class Tag : ExtEnum<Tag>
	{
		public static readonly Tag MouseDanglePosFinder = new Tag("MouseDanglePosFinder", register: true);

		public Tag(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public Tag tag;

	public OddJobAIModule(ArtificialIntelligence AI, Tag tag)
		: base(AI)
	{
		this.tag = tag;
	}
}
