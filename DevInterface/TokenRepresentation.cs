using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class TokenRepresentation : ResizeableObjectRepresentation
{
	public class TokenControlPanel : Panel, IDevUISignals
	{
		public class IndexControlSlider : Slider
		{
			public int maxNubInt;

			private int helpSlider;

			private int helpSlider_cooldown;

			public CollectToken.CollectTokenData TokenData => (parentNode.parentNode as TokenRepresentation).pObj.data as CollectToken.CollectTokenData;

			public IndexControlSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
				helpSlider = 0;
				helpSlider_cooldown = 0;
				if (ModManager.MSC && TokenData.isWhite)
				{
					maxNubInt = ExtEnum<ChatlogData.ChatlogID>.values.Count - 1;
				}
				else if (ModManager.MSC && TokenData.isGreen)
				{
					maxNubInt = ExtEnum<MultiplayerUnlocks.SlugcatUnlockID>.values.Count - 1;
				}
				else if (ModManager.MSC && TokenData.isRed)
				{
					maxNubInt = ExtEnum<MultiplayerUnlocks.SafariUnlockID>.values.Count - 1;
				}
				else if (TokenData.isBlue)
				{
					maxNubInt = ExtEnum<MultiplayerUnlocks.SandboxUnlockID>.values.Count - 1;
				}
				else
				{
					maxNubInt = ExtEnum<MultiplayerUnlocks.LevelUnlockID>.values.Count - 1;
				}
			}

			public override void Refresh()
			{
				base.Refresh();
				int num = 0;
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Index_Slider")
				{
					if (ModManager.MSC && TokenData.isWhite && TokenData.ChatlogCollect != null)
					{
						num = TokenData.ChatlogCollect.Index;
					}
					else if (ModManager.MSC && TokenData.isGreen && TokenData.SlugcatUnlock != null)
					{
						num = TokenData.SlugcatUnlock.Index;
					}
					else if (ModManager.MSC && TokenData.isRed && TokenData.SafariUnlock != null)
					{
						num = TokenData.SafariUnlock.Index;
					}
					else if (TokenData.isBlue && TokenData.SandboxUnlock != null)
					{
						num = TokenData.SandboxUnlock.Index;
					}
					else if (!TokenData.isBlue && TokenData.LevelUnlock != null)
					{
						num = TokenData.LevelUnlock.Index;
					}
				}
				base.NumberText = num.ToString();
				RefreshNubPos((float)num / (float)maxNubInt);
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Index_Slider")
				{
					if (ModManager.MSC && TokenData.isWhite)
					{
						int num = Mathf.FloorToInt(nubPos * (float)maxNubInt);
						if (helpSlider_cooldown <= 0 || Math.Abs(helpSlider - num) > 10)
						{
							if (Math.Abs(helpSlider - num) > 10)
							{
								helpSlider = Mathf.FloorToInt(nubPos * (float)maxNubInt);
							}
							else
							{
								helpSlider_cooldown = 30;
								num = (helpSlider = ((helpSlider >= num) ? (helpSlider - 1) : (helpSlider + 1)));
							}
							if (num > maxNubInt)
							{
								num = maxNubInt;
							}
							if (num < 0)
							{
								num = 0;
							}
							string entry = ExtEnum<ChatlogData.ChatlogID>.values.GetEntry(num);
							if (entry != null)
							{
								TokenData.ChatlogCollect = new ChatlogData.ChatlogID(entry);
							}
						}
						else
						{
							helpSlider_cooldown--;
						}
					}
					else if (ModManager.MSC && TokenData.isGreen)
					{
						string entry2 = ExtEnum<MultiplayerUnlocks.SlugcatUnlockID>.values.GetEntry(Mathf.FloorToInt(nubPos * (float)maxNubInt));
						if (entry2 != null)
						{
							TokenData.SlugcatUnlock = new MultiplayerUnlocks.SlugcatUnlockID(entry2);
						}
					}
					else if (ModManager.MSC && TokenData.isRed)
					{
						string entry3 = ExtEnum<MultiplayerUnlocks.SafariUnlockID>.values.GetEntry(Mathf.FloorToInt(nubPos * (float)maxNubInt));
						if (entry3 != null)
						{
							TokenData.SafariUnlock = new MultiplayerUnlocks.SafariUnlockID(entry3);
						}
					}
					else if (TokenData.isBlue)
					{
						string entry4 = ExtEnum<MultiplayerUnlocks.SandboxUnlockID>.values.GetEntry(Mathf.FloorToInt(nubPos * (float)maxNubInt));
						if (entry4 != null)
						{
							TokenData.SandboxUnlock = new MultiplayerUnlocks.SandboxUnlockID(entry4);
						}
					}
					else
					{
						string entry5 = ExtEnum<MultiplayerUnlocks.LevelUnlockID>.values.GetEntry(Mathf.FloorToInt(nubPos * (float)maxNubInt));
						if (entry5 != null)
						{
							TokenData.LevelUnlock = new MultiplayerUnlocks.LevelUnlockID(entry5);
						}
					}
				}
				parentNode.parentNode.Refresh();
				(parentNode as TokenControlPanel).UpdateTokenText();
				Refresh();
			}
		}

		public Button[] buttons;

		public DevUILabel lbl;

		public CollectToken.CollectTokenData TokenData => (parentNode as TokenRepresentation).pObj.data as CollectToken.CollectTokenData;

		public TokenControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 100f), "Collectable Token")
		{
			lbl = new DevUILabel(owner, "Token_Label", this, new Vector2(5f, 75f), 240f, "");
			subNodes.Add(lbl);
			subNodes.Add(new IndexControlSlider(owner, "Index_Slider", this, new Vector2(5f, 55f), "Token Index: "));
			buttons = new Button[ExtEnum<SlugcatStats.Name>.values.Count];
			float num = 10f;
			float num2 = 20f;
			for (int i = 0; i < buttons.Length; i++)
			{
				buttons[i] = new Button(owner, "Button_" + i, this, new Vector2(5f + (num2 + 5f) * ((float)i % num), 5f + (float)(int)((float)i / num) * (num2 + 5f)), num2, "");
				subNodes.Add(buttons[i]);
			}
			UpdateButtonText();
			UpdateTokenText();
		}

		private void UpdateTokenText()
		{
			if (ModManager.MSC && TokenData.isWhite)
			{
				if (TokenData.ChatlogCollect == null)
				{
					lbl.Text = "Undefined Chatlog";
				}
				else
				{
					lbl.Text = TokenData.ChatlogCollect.value;
				}
			}
			else if (ModManager.MSC && TokenData.isGreen)
			{
				if (TokenData.SlugcatUnlock == null)
				{
					lbl.Text = "Undefined Slugcat";
				}
				else
				{
					lbl.Text = TokenData.SlugcatUnlock.value;
				}
			}
			else if (ModManager.MSC && TokenData.isRed)
			{
				if (TokenData.SafariUnlock == null)
				{
					lbl.Text = "Undefined Safari";
				}
				else
				{
					lbl.Text = TokenData.SafariUnlock.value;
				}
			}
			else if (TokenData.isBlue)
			{
				if (TokenData.SandboxUnlock == null)
				{
					lbl.Text = "Undefined Sandbox";
				}
				else
				{
					lbl.Text = TokenData.SandboxUnlock.ToString() + ((TokenData.SandboxUnlock == MultiplayerUnlocks.SandboxUnlockID.Slugcat || MultiplayerUnlocks.ParentSandboxID(TokenData.SandboxUnlock) != null) ? " ~ HAS PARENT ~ Do not place" : "");
				}
			}
			else if (TokenData.LevelUnlock == null)
			{
				lbl.Text = "Undefined Level";
			}
			else
			{
				lbl.Text = TokenData.LevelUnlock.ToString() + ((TokenData.LevelUnlock == MultiplayerUnlocks.LevelUnlockID.Default || TokenData.LevelUnlock == MultiplayerUnlocks.LevelUnlockID.Hidden) ? " ~ Do not place" : "");
			}
		}

		private void UpdateButtonText()
		{
			for (int i = 0; i < buttons.Length; i++)
			{
				SlugcatStats.Name name = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(i));
				if (TokenData.availableToPlayers.Contains(name))
				{
					buttons[i].Text = name.value.Substring(0, 2);
				}
				else
				{
					buttons[i].Text = "--";
				}
			}
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			for (int i = 0; i < buttons.Length; i++)
			{
				if (buttons[i] == sender)
				{
					SlugcatStats.Name item = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(i));
					if (((parentNode as TokenRepresentation).pObj.data as CollectToken.CollectTokenData).availableToPlayers.Contains(item))
					{
						((parentNode as TokenRepresentation).pObj.data as CollectToken.CollectTokenData).availableToPlayers.Remove(item);
					}
					else
					{
						((parentNode as TokenRepresentation).pObj.data as CollectToken.CollectTokenData).availableToPlayers.Add(item);
					}
				}
			}
			UpdateButtonText();
		}
	}

	private int lineSprite;

	public TokenRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj)
		: base(owner, IDstring, parentNode, pObj, TokenName(pObj.data as CollectToken.CollectTokenData), showRing: false)
	{
		subNodes.Add(new TokenControlPanel(owner, "Token_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as TokenControlPanel).pos = (pObj.data as CollectToken.CollectTokenData).panelPos;
		fSprites.Add(new FSprite("pixel"));
		lineSprite = fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(fSprites[lineSprite]);
		fSprites[lineSprite].anchorY = 0f;
	}

	public static string TokenName(CollectToken.CollectTokenData data)
	{
		if (ModManager.MSC)
		{
			if (data.isDev)
			{
				return "Dev Token";
			}
			if (data.isRed)
			{
				return "Red Token";
			}
			if (data.isWhite)
			{
				return "White Token";
			}
			if (data.isGreen)
			{
				return "Green Token";
			}
		}
		if (data.isBlue)
		{
			return "Blue Token";
		}
		return "Gold Token";
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(lineSprite, absPos);
		fSprites[lineSprite].scaleY = (subNodes[1] as TokenControlPanel).pos.magnitude;
		fSprites[lineSprite].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[1] as TokenControlPanel).absPos);
		(pObj.data as CollectToken.CollectTokenData).panelPos = (subNodes[1] as Panel).pos;
	}
}
