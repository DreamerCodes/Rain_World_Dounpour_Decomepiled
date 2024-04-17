using RWCustom;
using UnityEngine;

namespace DevInterface;

public class ReliableDirectionRep : ResizeableObjectRepresentation
{
	public class DirectionControlPanel : Panel, IDevUISignals
	{
		public Button exitButton;

		public Button symbolButton;

		public Button showButton;

		public Button conditionButton;

		public Button pointBackButton;

		public Button[] availableButtons;

		public FSprite exitSprite;

		public int exitSpriteIndex;

		public ReliableIggyDirection.ReliableIggyDirectionData data => (parentNode as ReliableDirectionRep).data;

		public DirectionControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 85f), "Realible Iggy Direction")
		{
			exitButton = new Button(owner, "Exit_Button", this, new Vector2(5f, 65f), 115f, "Exit");
			subNodes.Add(exitButton);
			symbolButton = new Button(owner, "Shelter_Button", this, new Vector2(125f, 65f), 115f, "Shelter");
			subNodes.Add(symbolButton);
			showButton = new Button(owner, "Show_Button", this, new Vector2(5f, 45f), 115f, "Show");
			subNodes.Add(showButton);
			conditionButton = new Button(owner, "Condition_Button", this, new Vector2(125f, 45f), 115f, "Condition");
			subNodes.Add(conditionButton);
			pointBackButton = new Button(owner, "Back_Button", this, new Vector2(5f, 25f), 230f, "Back");
			subNodes.Add(pointBackButton);
			availableButtons = new Button[ExtEnum<SlugcatStats.Name>.values.Count];
			for (int i = 0; i < availableButtons.Length; i++)
			{
				availableButtons[i] = new Button(owner, "Button_" + i, this, new Vector2(5f + 80f * (float)i, 5f), 71.666664f, "");
				subNodes.Add(availableButtons[i]);
			}
			exitSprite = new FSprite("pixel");
			fSprites.Add(exitSprite);
			exitSpriteIndex = fSprites.Count;
			owner.placedObjectsContainer.AddChild(exitSprite);
			exitSprite.anchorY = 0f;
			exitSprite.scaleX = 2f;
			exitSprite.color = new Color(1f, 0f, 0f);
			UpdateButtonText();
		}

		public override void Refresh()
		{
			base.Refresh();
			if (exitSprite != null)
			{
				if (data.exit < 0 || data.exit >= owner.room.abstractRoom.connections.Length)
				{
					exitSprite.isVisible = false;
					return;
				}
				exitSprite.isVisible = true;
				Vector2 vector = (parentNode as PositionedDevUINode).absPos;
				Vector2 vector2 = owner.room.MiddleOfTile(owner.room.ShortcutLeadingToNode(data.exit).startCoord) - owner.room.game.cameras[0].pos;
				exitSprite.x = vector.x;
				exitSprite.y = vector.y;
				exitSprite.rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
				exitSprite.scaleY = Vector2.Distance(vector, vector2);
			}
		}

		private void UpdateButtonText()
		{
			for (int i = 0; i < availableButtons.Length; i++)
			{
				SlugcatStats.Name name = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(i));
				if (data.availableToPlayers.Contains(name))
				{
					availableButtons[i].Text = name.ToString();
				}
				else
				{
					availableButtons[i].Text = "--";
				}
			}
			exitButton.Text = "Exit: " + data.exit;
			symbolButton.Text = data.symbol.ToString();
			if (data.cyclesToShow == 0)
			{
				showButton.Text = "Forever";
			}
			else
			{
				showButton.Text = "Show " + data.cyclesToShow + "times (cycles)";
			}
			conditionButton.Text = data.condition.ToString();
			pointBackButton.Text = (data.pointPlayerBack ? "Pointing towards player entry door" : "Not pointing player towards where entered");
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			for (int i = 0; i < availableButtons.Length; i++)
			{
				if (availableButtons[i] == sender)
				{
					SlugcatStats.Name item = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(i));
					if (data.availableToPlayers.Contains(item))
					{
						data.availableToPlayers.Remove(item);
					}
					else
					{
						data.availableToPlayers.Add(item);
					}
				}
			}
			if (sender == exitButton)
			{
				data.exit++;
				if (data.exit >= owner.room.abstractRoom.exits)
				{
					data.exit = 0;
				}
			}
			else if (sender == showButton)
			{
				data.cyclesToShow++;
				if (data.cyclesToShow > 9)
				{
					data.cyclesToShow = 0;
				}
			}
			else if (sender == symbolButton)
			{
				int num = (int)data.symbol;
				num++;
				if (num >= ExtEnum<ReliableIggyDirection.ReliableIggyDirectionData.Symbol>.values.Count)
				{
					num = 0;
				}
				data.symbol = new ReliableIggyDirection.ReliableIggyDirectionData.Symbol(ExtEnum<ReliableIggyDirection.ReliableIggyDirectionData.Symbol>.values.GetEntry(num));
			}
			else if (sender == pointBackButton)
			{
				data.pointPlayerBack = !data.pointPlayerBack;
			}
			else if (sender == conditionButton)
			{
				int num2 = (int)data.condition;
				num2++;
				if (num2 >= ExtEnum<ReliableIggyDirection.ReliableIggyDirectionData.Condition>.values.Count)
				{
					num2 = 0;
				}
				data.condition = new ReliableIggyDirection.ReliableIggyDirectionData.Condition(ExtEnum<ReliableIggyDirection.ReliableIggyDirectionData.Condition>.values.GetEntry(num2));
			}
			UpdateButtonText();
		}
	}

	private int lineSprite;

	public ReliableIggyDirection.ReliableIggyDirectionData data => pObj.data as ReliableIggyDirection.ReliableIggyDirectionData;

	public ReliableDirectionRep(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj)
		: base(owner, IDstring, parentNode, pObj, "Reliable Direction", showRing: true)
	{
		subNodes.Add(new DirectionControlPanel(owner, "Direction_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as DirectionControlPanel).pos = data.panelPos;
		fSprites.Add(new FSprite("pixel"));
		lineSprite = fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(fSprites[lineSprite]);
		fSprites[lineSprite].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(lineSprite, absPos);
		fSprites[lineSprite].scaleY = (subNodes[1] as DirectionControlPanel).pos.magnitude;
		fSprites[lineSprite].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[1] as DirectionControlPanel).absPos);
		data.panelPos = (subNodes[1] as Panel).pos;
	}
}
