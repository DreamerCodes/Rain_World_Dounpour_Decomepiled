using MoreSlugcats;

public class PlayerState : CreatureState
{
	public int playerNumber;

	public SlugcatStats.Name slugcatCharacter;

	public int foodInStomach;

	public int quarterFoodPoints;

	public bool isGhost;

	public double permanentDamageTracking;

	public bool isPup;

	public bool forceFullGrown;

	public string swallowedItem;

	public bool permaDead { get; set; }

	public PlayerState(AbstractCreature crit, int playerNumber, SlugcatStats.Name slugcatCharacter, bool isGhost)
		: base(crit)
	{
		this.playerNumber = playerNumber;
		this.slugcatCharacter = slugcatCharacter;
		this.isGhost = isGhost;
		if (ModManager.MSC && slugcatCharacter == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
		{
			meatLeft *= 2;
		}
	}
}
