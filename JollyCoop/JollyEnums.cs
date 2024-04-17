namespace JollyCoop;

public class JollyEnums
{
	public class Name
	{
		public static SlugcatStats.Name JollyPlayer1;

		public static SlugcatStats.Name JollyPlayer2;

		public static SlugcatStats.Name JollyPlayer3;

		public static SlugcatStats.Name JollyPlayer4;

		public static void RegisterValues()
		{
			JollyPlayer1 = new SlugcatStats.Name("JollyPlayer1", register: true);
			JollyPlayer2 = new SlugcatStats.Name("JollyPlayer2", register: true);
			JollyPlayer3 = new SlugcatStats.Name("JollyPlayer3", register: true);
			JollyPlayer4 = new SlugcatStats.Name("JollyPlayer4", register: true);
		}

		public static void UnregisterValues()
		{
			if (JollyPlayer1 != null)
			{
				JollyPlayer1.Unregister();
				JollyPlayer1 = null;
			}
			if (JollyPlayer2 != null)
			{
				JollyPlayer2.Unregister();
				JollyPlayer2 = null;
			}
			if (JollyPlayer3 != null)
			{
				JollyPlayer3.Unregister();
				JollyPlayer3 = null;
			}
			if (JollyPlayer4 != null)
			{
				JollyPlayer4.Unregister();
				JollyPlayer4 = null;
			}
		}
	}

	public class JollyManualPages : ExtEnum<JollyManualPages>
	{
		public static readonly JollyManualPages Introduction = new JollyManualPages("Introduction", register: true);

		public static readonly JollyManualPages Difficulties = new JollyManualPages("Difficulties", register: true);

		public static readonly JollyManualPages Surviving_a_cycle = new JollyManualPages("Surviving_a_cycle", register: true);

		public static readonly JollyManualPages Camera = new JollyManualPages("Camera", register: true);

		public static readonly JollyManualPages Piggybacking = new JollyManualPages("Piggybacking", register: true);

		public static readonly JollyManualPages Pointing = new JollyManualPages("Pointing", register: true);

		public static readonly JollyManualPages Selecting_a_slugcat = new JollyManualPages("Selecting_a_slugcat", register: true);

		public JollyManualPages(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public static void InitExtEnumTypes()
	{
		_ = JollyManualPages.Difficulties;
	}

	public static void RegisterAllEnumExtensions()
	{
		Name.RegisterValues();
	}

	public static void UnregisterAllEnumExtensions()
	{
		Name.UnregisterValues();
	}
}
