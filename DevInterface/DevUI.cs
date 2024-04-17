using UnityEngine;

namespace DevInterface;

public class DevUI
{
	public RainWorldGame game;

	public string[] pages;

	public Page activePage;

	public Vector2 lastMousePos;

	public Vector2 mousePos;

	public bool mouseDown;

	public bool mouseClick;

	public bool lastMouseDown;

	public DevUINode draggedNode;

	public FContainer placedObjectsContainer;

	public Room room => game.cameras[0].room;

	public DevUI(RainWorldGame game)
	{
		this.game = game;
		placedObjectsContainer = new FContainer();
		if (game != null)
		{
			Futile.stage.AddChild(placedObjectsContainer);
		}
		pages = new string[6] { "Room Settings", "Objects", "Sound", "Map", "Triggers", "Dialog" };
		SwitchPage(game.setupValues.defaultSettingsScreen);
	}

	public void Update()
	{
		if (game.rainWorld.buildType != 0 || ModManager.DevTools)
		{
			lastMousePos = mousePos;
			mousePos = Futile.mousePosition;
			mouseDown = Input.GetMouseButton(0);
			mouseClick = mouseDown && !lastMouseDown;
			lastMouseDown = mouseDown;
			draggedNode = null;
			activePage.Update();
		}
	}

	public void ClearSprites()
	{
		if (activePage != null)
		{
			activePage.ClearSprites();
		}
	}

	public void SwitchPage(int newPage)
	{
		ClearSprites();
		switch (newPage)
		{
		case 0:
			activePage = new RoomSettingsPage(this, "Room_Settings_Page", null, "Room Settings");
			break;
		case 1:
			activePage = new ObjectsPage(this, "Objects_Page", null, "Objects");
			break;
		case 2:
			activePage = new SoundPage(this, "Sound_Page", null, "Sound");
			break;
		case 3:
			activePage = new MapPage(this, game.world, "Map_Page", null, "Map", forceRenderMode: false);
			break;
		case 4:
			activePage = new TriggersPage(this, "Triggers_Page", null, "Triggers");
			break;
		case 5:
			activePage = new DialogPage(this, "Dialog_Page", null, "Dialog");
			break;
		}
	}
}
