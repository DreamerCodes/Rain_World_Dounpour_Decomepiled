using System.Collections.Generic;
using System.Linq;
using Expedition;
using Modding.Expedition;

namespace Menu;

public class ExpeditionManualDialog : ManualDialog
{
	public static Dictionary<string, int> topicKeys = new Dictionary<string, int>
	{
		{ "introduction", 1 },
		{ "gameplay", 1 },
		{ "slugcatselect", 1 },
		{ "challengeselect", 2 },
		{ "quests", 1 },
		{ "mission", 1 },
		{
			"perks",
			ModManager.MSC ? 4 : 2
		},
		{ "burdens", 1 },
		{ "jukebox", 1 }
	};

	public ExpeditionManualDialog(ProcessManager manager, Dictionary<string, int> topics)
		: base(manager, topics)
	{
		currentTopic = base.topics.Keys.ElementAt(0);
		pageNumber = 0;
		GetManualPage(currentTopic, pageNumber);
	}

	public override void GetManualPage(string topic, int pageNumber)
	{
		if (currentTopicPage != null)
		{
			currentTopicPage.RemoveSprites();
			pages[1].RemoveSubObject(currentTopicPage);
		}
		if (topic == "introduction")
		{
			currentTopicPage = new IntroductionPage(this, pages[1]);
		}
		if (topic == "slugcatselect")
		{
			currentTopicPage = new SlugcatManualPage(this, pages[1]);
		}
		if (topic == "gameplay")
		{
			currentTopicPage = new GameplayPage(this, pages[1]);
		}
		if (topic == "challengeselect")
		{
			switch (pageNumber)
			{
			case 0:
				currentTopicPage = new ChallengeManualPage(this, pages[1]);
				break;
			case 1:
				currentTopicPage = new ChallengeControlsManualPage(this, pages[1]);
				break;
			}
		}
		if (topic == "quests")
		{
			currentTopicPage = new QuestManualPage(this, pages[1]);
		}
		if (topic == "mission")
		{
			currentTopicPage = new MissionManualPage(this, pages[1]);
		}
		if (topic == "jukebox")
		{
			currentTopicPage = new JukeboxManualPage(this, pages[1]);
		}
		if (topic == "perks")
		{
			currentTopicPage = new PerkManualPage(this, pages[1], pageNumber);
		}
		if (topic == "burdens")
		{
			currentTopicPage = new BurdenManualPage(this, pages[1], pageNumber);
		}
		pages[1].subObjects.Add(currentTopicPage);
	}

	public override string TopicName(string topic)
	{
		return topic switch
		{
			"introduction" => "EXPEDITION-INTRODUCTION", 
			"gameplay" => "GAMEPLAY CHANGES", 
			"slugcatselect" => "SLUGCAT SELECT", 
			"challengeselect" => "CHALLENGE SELECT", 
			"quests" => "QUESTS", 
			"mission" => "MISSIONS", 
			"perks" => "PERKS", 
			"burdens" => "BURDENS", 
			"jukebox" => "JUKEBOX", 
			_ => "NULL", 
		};
	}

	public string PerkManualDescription(string key)
	{
		if (!ExpeditionData.unlockables.Contains(key))
		{
			return "???";
		}
		switch (key)
		{
		case "unl-glow":
			return Translate("Start the expedition with a persistent glow around the player, usually granted by consuming a neuron. Useful in dark environments or when using the BLINDED burden.");
		case "unl-bomb":
			return Translate("Start the expedition with a Scavenger Bomb, a potential head start for combat related challenges.");
		case "unl-lantern":
			return Translate("Start the expedition with a Scavenger Lantern, a portable light source -- and for some slugcats -- a source of vital warmth.");
		case "unl-slow":
			return Translate("Grants the ability to slow time at will by holding PICK UP and pressing MAP. A visual indicator for the ability's cooldown appears above the cycle timer.");
		case "unl-passage":
			return Translate("re-enables the use of passages to travel to previously visited shelters and recover all karma while doing so. Rather than being granted by achievements, a passage is awarded for each completed challenge.");
		case "unl-backspear":
			return Translate("Grants The Hunter's ability to store spears on their back to other characters. A useful buff for players engaging in combat challenges.");
		case "unl-vulture":
			return Translate("Start the expedition with a Vulture Mask, helping to keep hostile creatures at bay, or make them fleeing targets.");
		case "unl-karma":
			return Translate("Start the expedition with reinforced karma. Reinforced karma is the only way to prevent permadeath at the lowest karma level. If the cycle ends in failure, a karma flower will appear at the site of death. Will be permanently lost if not recovered.");
		default:
			if (ModManager.MSC)
			{
				switch (key)
				{
				case "unl-electric":
					return Translate("Start the expedition with a charged Electric Spear. The spear is extremely useful for dealing high amounts of damage and stunning creatures. The spear can be recharged by impaling centipedes.");
				case "unl-dualwield":
					return Translate("Bestows the Spearmaster's dual-wielding ability to other characters. Being able to carry a spear in each hand greatly increases the speed at which you can deal damage.");
				case "unl-sing":
					return Translate("Start the expedition with a Singularity Bomb, a devastating weapon that can eliminate even the toughest of creatures, just make sure not to get caught in the blast.");
				case "unl-explosivejump":
					return Translate("Gives The Artificer's explosive double-jump ability to other characters, activated by pressing JUMP and PICK UP whilst in the air. Very useful for traversal and escaping dangerous situations.");
				case "unl-explosionimmunity":
					return Translate("Withstand explosive encounters by borrowing The Artificer's explosion immunity. Useful if unfavored by the Scavengers, or for surviving situations where an explosion may be a last resort.");
				case "unl-crafting":
					return Translate("Grants The Gourmand's crafting ability to other characters. Experiment with objects you find by combining them into new ones to help in different situations. Activate by holding UP and PICK UP with an item in each hand.");
				case "unl-agility":
					return Translate("Increases the chosen characters running and jumping abilities by borrowing The Rivulet's high agility, letting you cross great distances with ease. Less efective for larger characters.");
				case "unl-gun":
					return Translate("Start the expedition with the Joke Rifle. Originally shown off as an April Fools joke, the Rifle makes its way into Rain World: Downpour as a useful tool. Experiment by loading different items as ammo by holding the PICK UP button, and fire the Rifle with the THROW button.");
				}
			}
			return CustomPerks.PerkForID(key)?.ManualDescription ?? "";
		}
	}
}
