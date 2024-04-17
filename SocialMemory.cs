using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

public class SocialMemory
{
	public class Relationship
	{
		public EntityID subjectID;

		public float like;

		public float fear;

		public float know;

		public float tempLike;

		public float tempFear;

		public List<string> unrecognizedSaveStrings = new List<string>();

		public Relationship(EntityID subjectID)
		{
			this.subjectID = subjectID;
		}

		public void InfluenceLike(float change)
		{
			like = Mathf.Clamp(like + change * Mathf.Lerp(1f, 0.1f, know), -1f, 1f);
		}

		public void InfluenceFear(float change)
		{
			fear = Mathf.Clamp(fear + change * Mathf.Lerp(1f, 0.1f, know), -1f, 1f);
		}

		public void InfluenceKnow(float amountOfLerpUpwards)
		{
			know = Mathf.Lerp(know, 1f, Mathf.Min(amountOfLerpUpwards, 0.9f));
		}

		public void InfluenceTempLike(float change)
		{
			tempLike = Mathf.Clamp(tempLike + change, -1f, 1f);
		}

		public void InfluenceTempFear(float change)
		{
			tempFear = Mathf.Clamp(tempFear + change, -1f, 1f);
		}

		public void EvenOutTemps(float speed)
		{
			if (tempLike < like)
			{
				tempLike = Math.Min(like, tempLike + speed);
			}
			else
			{
				tempLike = Math.Max(like, tempLike - speed);
			}
			if (tempFear < fear)
			{
				tempFear = Math.Min(fear, tempFear + speed);
			}
			else
			{
				tempFear = Math.Max(fear, tempFear - speed);
			}
		}

		public override string ToString()
		{
			if (like == 0f && fear == 0f)
			{
				return "";
			}
			string text = "REL<rA>" + subjectID.ToString();
			if (like != 0f)
			{
				text += string.Format(CultureInfo.InvariantCulture, "<rA>L<rB>{0}", like);
			}
			if (fear != 0f)
			{
				text += string.Format(CultureInfo.InvariantCulture, "<rA>F<rB>{0}", fear);
			}
			if (know != 0f)
			{
				text += string.Format(CultureInfo.InvariantCulture, "<rA>K<rB>{0}", know);
			}
			foreach (string unrecognizedSaveString in unrecognizedSaveStrings)
			{
				text = text + "<rA>" + unrecognizedSaveString;
			}
			return text;
		}

		public static Relationship FromString(string s)
		{
			string[] array = Regex.Split(s, "<rA>");
			Relationship relationship = new Relationship(EntityID.FromString(array[1]));
			string[] array2 = null;
			for (int i = 2; i < array.Length; i++)
			{
				array2 = Regex.Split(array[i], "<rB>");
				switch (array2[0])
				{
				case "L":
					relationship.like = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					relationship.tempLike = relationship.like;
					continue;
				case "F":
					relationship.fear = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					relationship.tempFear = relationship.fear;
					continue;
				case "K":
					relationship.know = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				}
				if (array[i].Trim().Length > 0 && array2.Length >= 2)
				{
					relationship.unrecognizedSaveStrings.Add(array[i]);
				}
			}
			return relationship;
		}
	}

	public List<Relationship> relationShips;

	public SocialMemory()
	{
		relationShips = new List<Relationship>();
	}

	public Relationship GetOrInitiateRelationship(EntityID subjectID)
	{
		Relationship relationship = GetRelationship(subjectID);
		if (relationship != null)
		{
			return relationship;
		}
		relationship = new Relationship(subjectID);
		relationShips.Add(relationship);
		return relationship;
	}

	public void DiscardRelationship(EntityID subjectID)
	{
		for (int num = relationShips.Count - 1; num >= 0; num--)
		{
			if (relationShips[num].subjectID == subjectID)
			{
				relationShips.RemoveAt(num);
			}
		}
	}

	public Relationship GetRelationship(EntityID subjectID)
	{
		for (int i = 0; i < relationShips.Count; i++)
		{
			if (relationShips[i].subjectID == subjectID)
			{
				return relationShips[i];
			}
		}
		return null;
	}

	public float GetLike(EntityID subjectID)
	{
		for (int i = 0; i < relationShips.Count; i++)
		{
			if (relationShips[i].subjectID == subjectID)
			{
				return relationShips[i].like;
			}
		}
		return 0f;
	}

	public float GetTempLike(EntityID subjectID)
	{
		for (int i = 0; i < relationShips.Count; i++)
		{
			if (relationShips[i].subjectID == subjectID)
			{
				return relationShips[i].tempLike;
			}
		}
		return 0f;
	}

	public float GetKnow(EntityID subjectID)
	{
		for (int i = 0; i < relationShips.Count; i++)
		{
			if (relationShips[i].subjectID == subjectID)
			{
				return relationShips[i].know;
			}
		}
		return 0f;
	}

	public override string ToString()
	{
		string text = "";
		for (int i = 0; i < relationShips.Count; i++)
		{
			string text2 = relationShips[i].ToString();
			if (text2 != "")
			{
				text = text + text2 + ((i < relationShips.Count - 1) ? "<smA>" : "");
			}
		}
		return text;
	}

	public static SocialMemory FromString(string s)
	{
		SocialMemory socialMemory = new SocialMemory();
		string[] array = Regex.Split(s, "<smA>");
		for (int i = 0; i < array.Length; i++)
		{
			if (Regex.Split(array[i], "<rA>")[0] == "REL")
			{
				socialMemory.relationShips.Add(Relationship.FromString(array[i]));
			}
		}
		return socialMemory;
	}

	public void EvenOutAllTemps(float speed)
	{
		for (int i = 0; i < relationShips.Count; i++)
		{
			relationShips[i].EvenOutTemps(speed);
		}
	}

	public void CycleTick()
	{
		for (int num = relationShips.Count - 1; num >= 0; num--)
		{
			relationShips[num].like *= Custom.LerpMap(Mathf.Abs(relationShips[num].like), 0.5f, 0f, 1f, 0.85f);
			relationShips[num].fear *= Custom.LerpMap(Mathf.Abs(relationShips[num].fear), 0.5f, 0f, 1f, 0.85f);
			relationShips[num].know *= Custom.LerpMap(relationShips[num].know, 0.5f, 0f, 1f, 0.15f);
			if (Mathf.Abs(relationShips[num].like) < 0.25f && Mathf.Abs(relationShips[num].fear) < 0.25f)
			{
				relationShips.RemoveAt(num);
			}
		}
	}
}
