using System.IO;
using UnityEngine;

public static class PNGSaver
{
	public static void SaveTextureToFile(Texture2D texture, string fileName)
	{
		Texture2D texture2D = new Texture2D(texture.width, texture.height);
		texture2D.SetPixels32(texture.GetPixels32());
		texture2D.Apply(updateMipmaps: false);
		File.WriteAllBytes(fileName, texture2D.EncodeToPNG());
	}
}
