using System.Collections.Generic;
using System.Linq;
using Menu;

namespace JollyCoop.JollyManual;

public class JollyManualDialog : ManualDialog
{
	public JollyManualDialog(ProcessManager manager, Dictionary<JollyEnums.JollyManualPages, int> topics)
		: base(manager, topics.ToDictionary((KeyValuePair<JollyEnums.JollyManualPages, int> entry) => entry.Key.ToString(), (KeyValuePair<JollyEnums.JollyManualPages, int> entry) => entry.Value))
	{
		currentTopic = base.topics.Keys.ElementAt(0);
		pageNumber = 0;
		GetManualPage(currentTopic, pageNumber);
		try
		{
			Update();
			GrafUpdate(0f);
		}
		catch
		{
		}
	}

	public override void GetManualPage(string topicString, int pageNumber)
	{
		JollyEnums.JollyManualPages jollyManualPages = new JollyEnums.JollyManualPages(topicString);
		if (currentTopicPage != null)
		{
			currentTopicPage.RemoveSprites();
			pages[1].RemoveSubObject(currentTopicPage);
		}
		if (jollyManualPages == JollyEnums.JollyManualPages.Introduction)
		{
			currentTopicPage = new IntroductionPage(this, pages[1]);
		}
		if (jollyManualPages == JollyEnums.JollyManualPages.Difficulties)
		{
			currentTopicPage = new DifficultiesPage(this, pages[1]);
		}
		if (jollyManualPages == JollyEnums.JollyManualPages.Surviving_a_cycle)
		{
			currentTopicPage = new Surviving(this, pages[1]);
		}
		if (jollyManualPages == JollyEnums.JollyManualPages.Camera)
		{
			switch (pageNumber)
			{
			case 0:
				currentTopicPage = new CameraFirst(this, pages[1]);
				break;
			case 1:
				currentTopicPage = new CameraSecond(this, pages[1]);
				break;
			}
		}
		if (jollyManualPages == JollyEnums.JollyManualPages.Piggybacking)
		{
			currentTopicPage = new Piggybacking(this, pages[1]);
		}
		if (jollyManualPages == JollyEnums.JollyManualPages.Pointing)
		{
			currentTopicPage = new Pointing(this, pages[1]);
		}
		if (jollyManualPages == JollyEnums.JollyManualPages.Selecting_a_slugcat)
		{
			currentTopicPage = new SelectingSlugcat(this, pages[1]);
		}
		pages[1].subObjects.Add(currentTopicPage);
	}

	public override string TopicName(string topic)
	{
		return topic.ToString().Replace("_", " ").ToUpper();
	}
}
