using UnityEngine;

public interface IDrawable
{
	void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam);

	void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos);

	void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette);

	void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner);
}
