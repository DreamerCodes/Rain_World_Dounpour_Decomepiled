using UnityEngine;

namespace Stove.Sample.CloudFile;

public class CloudFileSample : MonoBehaviour
{
	[SerializeField]
	private string applicationKey;

	[SerializeField]
	private string applicationSecret;

	[SerializeField]
	private MemberForCloudFile memberInfoAccessor;

	[SerializeField]
	private string GameNo;

	[SerializeField]
	private string Category;

	[SerializeField]
	private string[] FilePaths;

	[SerializeField]
	private bool OverWrite;

	[SerializeField]
	private int pageSize = 10;

	[SerializeField]
	private string lastCreatedDate;

	[SerializeField]
	private string sortOrder;

	[SerializeField]
	private string partialFields;

	[SerializeField]
	private string fileid;

	[SerializeField]
	private bool shared;

	[SerializeField]
	private string fileFullPath;

	[SerializeField]
	private string fileCheckSum;
}
