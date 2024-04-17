using RWCustom;
using UnityEngine;

namespace DevInterface;

public class ShowProjectedImageEventPanel : StandardEventPanel, IDevUISignals
{
	public Button afterEncounterButton;

	public Button showDirectionButton;

	public int lineSprite = -1;

	public ShowProjectedImageEvent imageEvent => base.tEvent as ShowProjectedImageEvent;

	public ShowProjectedImageEventPanel(DevUI owner, DevUINode parentNode)
		: base(owner, parentNode, 80f)
	{
		afterEncounterButton = new Button(owner, "After_Encounter_Button", this, new Vector2(5f, 35f), size.x - 10f, "");
		subNodes.Add(afterEncounterButton);
		showDirectionButton = new Button(owner, "Show_Direction_Buttun", this, new Vector2(5f, 5f), size.x - 10f, "");
		subNodes.Add(showDirectionButton);
		fSprites.Add(new FSprite("pixel"));
		lineSprite = fSprites.Count - 1;
		fSprites[lineSprite].anchorY = 0f;
		fSprites[lineSprite].scaleX = 2f;
		owner.placedObjectsContainer.AddChild(fSprites[lineSprite]);
	}

	public override void Refresh()
	{
		base.Refresh();
		if (afterEncounterButton != null)
		{
			afterEncounterButton.Text = (imageEvent.afterEncounter ? "After Encounter" : "Before Encounter");
		}
		if (showDirectionButton != null)
		{
			showDirectionButton.Text = (imageEvent.onlyWhenShowingDirection ? "Only prog. dir." : "---");
		}
		if (lineSprite <= -1)
		{
			return;
		}
		MoveSprite(lineSprite, absPos);
		Vector2 vector = absPos;
		Vector2 vector2 = vector + new Vector2(-45f, -45f);
		for (int i = 0; i < owner.room.roomSettings.placedObjects.Count; i++)
		{
			if (owner.room.roomSettings.placedObjects[i].type == PlacedObject.Type.ProjectedImagePosition)
			{
				vector2 = owner.room.roomSettings.placedObjects[i].pos - owner.room.game.cameras[0].pos;
			}
		}
		fSprites[lineSprite].scaleY = Vector2.Distance(vector, vector2);
		fSprites[lineSprite].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (sender == afterEncounterButton)
		{
			imageEvent.afterEncounter = !imageEvent.afterEncounter;
			Refresh();
		}
		if (sender == showDirectionButton)
		{
			imageEvent.onlyWhenShowingDirection = !imageEvent.onlyWhenShowingDirection;
			Refresh();
		}
	}
}
