using RWCustom;
using UnityEngine;

internal class ScoreLabel : ISingleCameraDrawable
{
	public PlayerGraphics playerGraphics;

	private bool hidden;

	private int counter;

	private FLabel label;

	private FLabel textShadow;

	private Vector2 pos;

	private RoomCamera camera;

	public ScoreLabel(PlayerGraphics playerGraphics)
	{
		this.playerGraphics = playerGraphics;
		label = new FLabel(Custom.GetFont(), "0");
		label.alignment = FLabelAlignment.Left;
		label.color = new Color(1f, 1f, 1f);
		textShadow = new FLabel(Custom.GetFont(), "0");
		textShadow.alignment = FLabelAlignment.Left;
		textShadow.color = new Color(0f, 0f, 0f);
		camera = playerGraphics.owner.room.game.cameras[0];
		camera.AddSingleCameraDrawable(this);
		camera.ReturnFContainer("HUD").AddChild(textShadow);
		camera.ReturnFContainer("HUD").AddChild(label);
		hidden = true;
	}

	public void ShowScore()
	{
		counter = 0;
		pos = playerGraphics.owner.firstChunk.pos + new Vector2(0f, 40f);
		hidden = false;
		label.isVisible = true;
		textShadow.isVisible = true;
	}

	public void Update()
	{
		if (hidden)
		{
			label.isVisible = false;
			textShadow.isVisible = false;
			return;
		}
		label.text = (playerGraphics.owner as Player).FoodInStomach.ToString();
		textShadow.text = (playerGraphics.owner as Player).FoodInStomach.ToString();
		counter++;
		if (counter < 30)
		{
			pos.y += 2f;
		}
		else if (counter >= 60)
		{
			if (counter < 80)
			{
				label.isVisible = counter % 6 < 3;
				textShadow.isVisible = counter % 6 < 3;
			}
			else if (counter < 100)
			{
				label.isVisible = counter % 3 == 0;
				textShadow.isVisible = counter % 3 == 0;
			}
			else
			{
				hidden = true;
			}
		}
	}

	public void Draw(RoomCamera camera, float timeStacker, Vector2 camPos)
	{
		label.x = pos.x - camPos.x;
		label.y = pos.y - camPos.y;
		textShadow.x = pos.x + 1f - camPos.x;
		textShadow.y = pos.y - 1f - camPos.y;
	}
}
