using RWCustom;
using UnityEngine;

namespace DevInterface;

public class TriggerPanel : Panel, IDevUISignals
{
	public class TriggerPanelSlider : Slider
	{
		public EventTrigger trigger => (parentNode as TriggerPanel).trigger;

		public TriggerPanelSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
			: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 100f)
		{
		}

		public override void Refresh()
		{
			base.Refresh();
			float num = 0f;
			switch (IDstring)
			{
			case "To_Cycle_Slider":
				if (trigger.activeToCycle < 0)
				{
					num = 1f;
					base.NumberText = "NA";
				}
				else
				{
					num = (float)trigger.activeToCycle / 80f;
					base.NumberText = trigger.activeToCycle.ToString();
				}
				break;
			case "From_Cycle_Slider":
				num = (float)trigger.activeFromCycle / 80f;
				base.NumberText = trigger.activeFromCycle.ToString();
				break;
			case "Delay_Slider":
				num = (float)trigger.delay / 4800f;
				base.NumberText = (float)trigger.delay / 40f + "s";
				break;
			case "Chance_Slider":
				num = trigger.fireChance;
				base.NumberText = (int)(num * 100f) + "%";
				break;
			case "Karma_Slider":
				num = (float)trigger.karma / 4f;
				base.NumberText = ((trigger.karma < 1) ? "NA" : (trigger.karma + 1).ToString());
				break;
			}
			RefreshNubPos(num);
		}

		public override void NubDragged(float nubPos)
		{
			switch (IDstring)
			{
			case "To_Cycle_Slider":
				nubPos = Mathf.Max(nubPos, (float)trigger.activeFromCycle / 80f);
				trigger.activeToCycle = (int)(nubPos * 80f);
				if (nubPos == 1f)
				{
					trigger.activeToCycle = -1;
				}
				break;
			case "From_Cycle_Slider":
				if (trigger.activeToCycle >= 0)
				{
					nubPos = Mathf.Min(nubPos, (float)trigger.activeToCycle / 80f);
				}
				trigger.activeFromCycle = (int)(nubPos * 80f);
				break;
			case "Delay_Slider":
				trigger.delay = (int)(nubPos * 40f * 120f);
				break;
			case "Chance_Slider":
				trigger.fireChance = nubPos;
				break;
			case "Karma_Slider":
				trigger.karma = (int)(nubPos * 4f);
				break;
			}
			Refresh();
		}
	}

	public EventTrigger trigger;

	public StandardEventPanel eventPanel;

	public Button multiUseButton;

	public Button entranceButton;

	public FSprite entranceSprite;

	public int entranceSpriteIndex;

	public Button[] slugcatsButtons;

	public TriggerPanel(DevUI owner, DevUINode parentNode, Vector2 pos, EventTrigger trigger)
		: base(owner, "Trigger_Panel", parentNode, pos, new Vector2(245f, 210f), "Trigger : " + trigger.type.ToString())
	{
		this.trigger = trigger;
		Move(trigger.panelPosition);
		subNodes.Add(new TriggerPanelSlider(owner, "From_Cycle_Slider", this, new Vector2(5f, size.y - 16f - 5f), "Active from cycle:"));
		subNodes.Add(new TriggerPanelSlider(owner, "To_Cycle_Slider", this, new Vector2(5f, size.y - 16f - 25f), "Active up to cycle:"));
		subNodes.Add(new TriggerPanelSlider(owner, "Chance_Slider", this, new Vector2(5f, size.y - 16f - 45f), "Fire chance:"));
		subNodes.Add(new TriggerPanelSlider(owner, "Delay_Slider", this, new Vector2(5f, size.y - 16f - 65f), "Trigger delay:"));
		subNodes.Add(new TriggerPanelSlider(owner, "Karma_Slider", this, new Vector2(5f, size.y - 16f - 85f), "Karma req.:"));
		multiUseButton = new Button(owner, "Multi_Use_Button", this, new Vector2(5f, size.y - 125f), 235f, "");
		subNodes.Add(multiUseButton);
		entranceButton = new Button(owner, "Entrance_Button", this, new Vector2(5f, 65f), 235f, "");
		subNodes.Add(entranceButton);
		entranceSprite = new FSprite("pixel");
		entranceSpriteIndex = fSprites.Count;
		fSprites.Add(entranceSprite);
		owner.placedObjectsContainer.AddChild(entranceSprite);
		entranceSprite.anchorY = 0f;
		entranceSprite.scaleX = 2f;
		entranceSprite.color = new Color(1f, 0f, 0f);
		slugcatsButtons = new Button[ExtEnum<SlugcatStats.Name>.values.Count];
		float num = 10f;
		float num2 = 20f;
		for (int i = 0; i < slugcatsButtons.Length; i++)
		{
			slugcatsButtons[i] = new Button(owner, "Slugcat_Button", this, new Vector2(5f + (num2 + 5f) * ((float)i % num), 25f + num2 * (float)(int)((float)i / num)), num2, "");
			subNodes.Add(slugcatsButtons[i]);
		}
		subNodes.Add(new Button(owner, "Event_Button", this, new Vector2(5f, size.y - 205f), 235f, "Add Event"));
		if (trigger is SpotTrigger)
		{
			subNodes.Add(new SpotTriggerHandle(owner, "Spot_Trigger_Handle", this, trigger as SpotTrigger, pos + new Vector2(-50f, -100f)));
		}
		AddEventPanel();
		Refresh();
		UpdateSlugcatButtonText();
	}

	public override void Update()
	{
		base.Update();
		if (dragged && (base.Page as TriggersPage).draggedObject == null)
		{
			(base.Page as TriggersPage).draggedObject = this;
		}
	}

	private void UpdateSlugcatButtonText()
	{
		for (int i = 0; i < slugcatsButtons.Length; i++)
		{
			SlugcatStats.Name name = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(i));
			if (trigger.slugcats.Contains(name))
			{
				slugcatsButtons[i].Text = name.value.Substring(0, 2);
			}
			else
			{
				slugcatsButtons[i].Text = "--";
			}
		}
	}

	public override void Move(Vector2 newPos)
	{
		base.Move(newPos);
		trigger.panelPosition = absPos;
	}

	public override void Refresh()
	{
		base.Refresh();
		if (multiUseButton != null)
		{
			multiUseButton.Text = (trigger.multiUse ? "Can fire multiple times" : "Can only fire once");
		}
		if (entranceButton != null)
		{
			entranceButton.Text = ((trigger.entrance < 0) ? "No entrance requirement" : ("Entrance: " + trigger.entrance));
		}
		if (entranceSprite != null)
		{
			if (trigger.entrance < 0 || trigger.entrance >= owner.room.abstractRoom.connections.Length)
			{
				fSprites[entranceSpriteIndex].isVisible = false;
				return;
			}
			fSprites[entranceSpriteIndex].isVisible = true;
			Vector2 vector = absPos;
			Vector2 vector2 = owner.room.MiddleOfTile(owner.room.ShortcutLeadingToNode(trigger.entrance).startCoord) - owner.room.game.cameras[0].pos;
			MoveSprite(entranceSpriteIndex, vector);
			fSprites[entranceSpriteIndex].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
			fSprites[entranceSpriteIndex].scaleY = Vector2.Distance(vector, vector2);
		}
	}

	public void AddEvent(TriggeredEvent.EventType evnt)
	{
		if (evnt == TriggeredEvent.EventType.MusicEvent)
		{
			trigger.tEvent = new MusicEvent();
			trigger.multiUse = false;
		}
		else if (evnt == TriggeredEvent.EventType.StopMusicEvent)
		{
			trigger.tEvent = new StopMusicEvent();
			trigger.multiUse = true;
		}
		else if (evnt == TriggeredEvent.EventType.ShowProjectedImageEvent)
		{
			trigger.tEvent = new ShowProjectedImageEvent();
			trigger.multiUse = true;
		}
		else
		{
			trigger.tEvent = new TriggeredEvent(evnt);
		}
		RemoveSelectEventPanel();
		AddEventPanel();
		Refresh();
	}

	private void AddEventPanel()
	{
		if (trigger.tEvent != null)
		{
			if (trigger.tEvent.type == TriggeredEvent.EventType.MusicEvent)
			{
				eventPanel = new MusicEventPanel(owner, this);
			}
			else if (trigger.tEvent.type == TriggeredEvent.EventType.StopMusicEvent)
			{
				eventPanel = new StopMusicEventPanel(owner, this);
			}
			else if (trigger.tEvent.type == TriggeredEvent.EventType.ShowProjectedImageEvent)
			{
				eventPanel = new ShowProjectedImageEventPanel(owner, this);
			}
			else
			{
				eventPanel = new StandardEventPanel(owner, this, 30f);
			}
			if (eventPanel != null)
			{
				subNodes.Add(eventPanel);
			}
		}
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (sender == multiUseButton)
		{
			trigger.multiUse = !trigger.multiUse;
			Refresh();
		}
		else if (sender.IDstring == "Event_Button")
		{
			bool flag = true;
			if (trigger.tEvent != null)
			{
				trigger.tEvent = null;
				subNodes.Remove(eventPanel);
				eventPanel.ClearSprites();
				eventPanel = null;
				flag = false;
			}
			for (int i = 0; i < subNodes.Count; i++)
			{
				if (subNodes[i] is SelectEventPanel)
				{
					RemoveSelectEventPanel();
					flag = false;
					break;
				}
			}
			if (flag)
			{
				subNodes.Add(new SelectEventPanel(owner, this, new Vector2(300f, 0f)));
			}
		}
		else if (sender.IDstring == "Entrance_Button")
		{
			trigger.entrance++;
			if (trigger.entrance >= owner.room.abstractRoom.connections.Length)
			{
				trigger.entrance = -1;
			}
			Refresh();
		}
		else
		{
			if (!(sender.IDstring == "Slugcat_Button"))
			{
				return;
			}
			int index = 0;
			for (int j = 0; j < slugcatsButtons.Length; j++)
			{
				if (sender == slugcatsButtons[j])
				{
					index = j;
					break;
				}
			}
			SlugcatStats.Name item = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(index));
			if (trigger.slugcats.Contains(item))
			{
				trigger.slugcats.Remove(item);
			}
			else
			{
				trigger.slugcats.Add(item);
			}
			UpdateSlugcatButtonText();
		}
	}

	private void RemoveSelectEventPanel()
	{
		for (int i = 0; i < subNodes.Count; i++)
		{
			if (subNodes[i] is SelectEventPanel)
			{
				subNodes[i].ClearSprites();
				subNodes.RemoveAt(i);
				Refresh();
			}
		}
	}
}
