using System.Collections.Generic;

public class ProjectionScreen : UpdatableAndDeletable
{
	public List<ProjectedImage> images;

	public ProjectionScreen(Room room)
	{
		base.room = room;
		images = new List<ProjectedImage>();
	}

	public ProjectedImage AddImage(string name)
	{
		return AddImage(new List<string> { name }, 0);
	}

	public ProjectedImage AddImage(List<string> names, int cycleTime)
	{
		images.Add(new ProjectedImage(names, cycleTime));
		room.AddObject(images[images.Count - 1]);
		return images[images.Count - 1];
	}

	public void RemoveImage(string firstImageName)
	{
		for (int num = images.Count - 1; num >= 0; num--)
		{
			if (images[num].imageNames[0] == firstImageName)
			{
				images[num].Destroy();
				images.RemoveAt(num);
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
	}
}
