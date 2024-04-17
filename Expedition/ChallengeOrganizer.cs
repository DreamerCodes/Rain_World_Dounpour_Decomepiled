using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MoreSlugcats;
using UnityEngine;

namespace Expedition;

public static class ChallengeOrganizer
{
	public static List<Challenge> availableChallengeTypes;

	public static List<string> filterChallengeTypes = new List<string>();

	public static List<string> creatureBlacklist;

	public static void AssignChallenge(int slot, bool hidden)
	{
		int num = 0;
		while (true)
		{
			if (num >= 15)
			{
				ExpLog.Log("ChallengOrganiser gave up after 15 attempts!");
				break;
			}
			if (ExpeditionData.challengeList == null)
			{
				break;
			}
			Challenge challenge = RandomChallenge(hidden);
			if (!challenge.ValidForThisSlugcat(ExpeditionData.slugcatPlayer))
			{
				num++;
				continue;
			}
			bool flag = false;
			for (int i = 0; i < ExpeditionData.challengeList.Count; i++)
			{
				if (!ExpeditionData.challengeList[i].Duplicable(challenge))
				{
					flag = true;
				}
			}
			if (flag)
			{
				num++;
				continue;
			}
			if (hidden && !challenge.CanBeHidden())
			{
				num++;
				continue;
			}
			if (ExpeditionData.challengeList.Count <= slot && slot <= 4)
			{
				ExpeditionData.challengeList.Add(challenge);
			}
			else
			{
				challenge.hidden = hidden;
				ExpeditionData.challengeList[slot] = challenge;
			}
			ExpLog.Log("Got new challenge: " + challenge.GetType().Name);
			break;
		}
	}

	public static void SetupChallengeTypes()
	{
		if (availableChallengeTypes != null)
		{
			return;
		}
		availableChallengeTypes = new List<Challenge>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			ReflectionTypeLoadException reflError;
			foreach (Type item in from TheType in GetTypesSafely(assemblies[i], out reflError)
				where TheType.IsClass && !TheType.IsAbstract && TheType.IsSubclassOf(typeof(Challenge))
				select TheType)
			{
				Challenge challenge = (Challenge)Activator.CreateInstance(item);
				ExpLog.Log("Add challenge type: " + challenge.ChallengeName());
				availableChallengeTypes.Add(challenge);
				ExpeditionGame.challengeNames.Add(item.Name, challenge.ChallengeName());
			}
		}
	}

	public static IEnumerable<Type> GetTypesSafely(Assembly asm, out ReflectionTypeLoadException reflError)
	{
		try
		{
			reflError = null;
			return asm.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			return (reflError = ex).Types.Where((Type t) => t != null);
		}
	}

	public static Challenge RandomChallenge(bool hidden)
	{
		if (availableChallengeTypes == null)
		{
			SetupChallengeTypes();
		}
		List<Challenge> list = new List<Challenge>();
		for (int i = 0; i < availableChallengeTypes.Count; i++)
		{
			if (!filterChallengeTypes.Contains(availableChallengeTypes[i].GetType().Name) || hidden)
			{
				list.Add(availableChallengeTypes[i]);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)].Generate();
	}

	public static void InitCreatureBlacklist()
	{
		creatureBlacklist = new List<string>
		{
			CreatureTemplate.Type.GarbageWorm.value,
			CreatureTemplate.Type.SeaLeech.value,
			CreatureTemplate.Type.Spider.value,
			CreatureTemplate.Type.Overseer.value,
			CreatureTemplate.Type.TempleGuard.value
		};
		if (ModManager.MSC)
		{
			creatureBlacklist.Add(MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy.value);
		}
	}
}
