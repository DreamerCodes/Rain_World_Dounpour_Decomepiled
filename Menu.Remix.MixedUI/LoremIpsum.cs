using System.Text;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public static class LoremIpsum
{
	private static readonly string[] _words = new string[63]
	{
		"ad", "adipiscing", "aliqua", "aliquip", "amet", "anim", "aute", "cillum", "commodo", "consectetur",
		"consequat", "culpa", "cupidatat", "deserunt", "do", "dolor", "dolore", "duis", "ea", "eiusmod",
		"elit", "enim", "esse", "est", "et", "eu", "ex", "excepteur", "exercitation", "fugiat",
		"id", "in", "incididunt", "ipsum", "irure", "labore", "laboris", "laborum", "lorem", "magna",
		"minim", "mollit", "nisi", "non", "nostrud", "nulla", "occaecat", "officia", "pariatur", "proident",
		"qui", "quis", "reprehenderit", "sed", "sint", "sit", "sunt", "tempor", "ullamco", "ut",
		"velit", "veniam", "voluptate"
	};

	public const int meanCharPerSentence = 60;

	public static string Generate(int minSentences, int maxSentences, int numParagraphs = 1)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < numParagraphs; i++)
		{
			int num = Random.Range(minSentences, maxSentences) + 1;
			if (i > 0)
			{
				stringBuilder.Append("\n");
			}
			for (int j = 0; j < num; j++)
			{
				int num2 = Random.Range(4, 12);
				for (int k = 0; k < num2; k++)
				{
					if (k > 0 || j > 0)
					{
						stringBuilder.Append(" ");
					}
					string text = _words[Random.Range(0, _words.Length - 1)];
					if (k == 0)
					{
						text = text.Substring(0, 1).ToUpper() + text.Substring(1);
					}
					stringBuilder.Append(text);
				}
				stringBuilder.Append(".");
			}
		}
		return stringBuilder.ToString();
	}
}
