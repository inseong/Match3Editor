using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.ComponentModel;


class GameDataLoader : MonoBehaviour
{
//	public enum LoadingType
//	{
//		FromLocal,
//		FromCDN,
//	}
//
//	[System.Serializable]
//	public class DownloadUrls
//	{
//		public string Android = null;
//		public string IOS = null;
//		public string LocalPath = null;
//	}
//
//	public bool IsDownloaded { get { return this.isDownloaded; } }

//	public LoadingType loadingType = LoadingType.FromLocal;
//	public DownloadUrls downloadUrls = null;
//	public string unitPrefabPath = "Assets/FortuneSaga/Resources/Units/Prefabs";
//	public string unitIconsPath = "Assets/FortuneSaga/Resources/Units/Icons";
//	public string backgroundPrefabPath = "Assets/FortuneSaga/Resources/Background/Prefabs";
//	public string reelPrefabPath = "Assets/FortuneSaga/Resources/Reels/Prefabs";
//	public string gameplayPrefabPath = "Assets/FortuneSaga/Resources/GamePlay/Prefabs";
//	public string effectPrefabPath = "Assets/FortuneSaga/Resources/Effects/Prefabs";
//	public int maximumCachingSize = 700;

//	bool isDownloaded;
//	List<AssetBundle> assetBundleList = new List<AssetBundle>();
//	Dictionary<string, AssetBundle> assetDictionary = new Dictionary<string, AssetBundle>();

	/// <summary>
	/// //////////////////////////////////////////////////////////////////////
	/// </summary>

	static GameDataLoader _instance;

	/// <summary>
	/// //////////////////////////////////////////////////////////////////////
	/// </summary>

	public static GameDataLoader instance
	{
		get
		{
			return _instance;
		}
	}

	void Awake()
	{
		if (_instance != null)
			return;

		_instance = this;
		DontDestroyOnLoad(this);
	}

//	public GameObject LoadUnitPrefab( string unitId )
//	{
//		return Load ( unitPrefabPath + "/" + unitId +".prefab", typeof(GameObject) ) as GameObject;
//	}
//	
//	public Texture2D LoadUnitIcon( string unitId )
//	{
//		return Load ( unitIconsPath + "/" + unitId +".png", typeof(Texture2D) ) as Texture2D;
//	}
//
//	public GameObject LoadReelSlotPrefab( string prefabName )
//	{
//		return Load ( reelPrefabPath + "/" + prefabName +".prefab", typeof(GameObject) ) as GameObject;
//	}

	public Object Load(string path, System.Type type)
	{
//		if (!this.assetDictionary.ContainsKey(path))
		{
#if UNITY_EDITOR
			//DebugConsole.Log("ContentLoader new path="+path);
			return UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(path);
#else
//			if( loadingType == LoadingType.FromLocal )
			{
//				string str = "Assets/MatchRPG/Resources/";
//				string newPath = path.Substring( str.Length );
				string dir = System.IO.Path.GetDirectoryName(path);
				string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

				if( dir.Length > 0 )
					return Resources.Load( dir+"/"+fileName, type );
				else
					return Resources.Load( fileName, type );
			}

			return null;
#endif
		}

//		AssetBundle assetBundle = this.assetDictionary[path];
//		return assetBundle.LoadAsset(System.IO.Path.GetFileNameWithoutExtension(path));//, type);
	}

//	public GameDataRequest LoadAsync(string path)
//	{
//		if (!this.assetDictionary.ContainsKey(path))
//		{
//#if UNITY_EDITOR
//			/*
//			string[] only_villian_path = path.Split('.');
//			string[] only_villian_path1 = only_villian_path[0].Split('_');
//			string make_villian_path = "Villian_" + only_villian_path1[1];
//
//
//			GameObject gobj = GameObject.Instantiate(ContentsLoader.Instance.Load(path, typeof(GameObject))) as GameObject;
//
//			gobj.SetActive(false);
//
//			Transform t1 = gobj.transform;
//			GameObject champion_childobj = t1.FindChild(make_villian_path).gameObject;  
//			Animation ani = champion_childobj.GetComponent<Animation>();
//
//			ani.playAutomatically = true;
//
//			SetLayerRecursively(champion_childobj, LayerMask.NameToLayer("GUI"));
//			*/
//			return new GameDataRequest(path);
//
//			//return new ContentsRequest(Resources.LoadAssetAtPath<Object>(path));
//#else
//			/*
//			//return null;
//			DebugConsole.Log("ContentsRequest LoadAsync");
//			return null;
//			*/
//			return new GameDataRequest(path);
//#endif
//		}
//
//		//DebugConsole.Log("ContentsRequest LoadAsync 2");
//		AssetBundleRequest request = this.assetDictionary[path].LoadAssetAsync(System.IO.Path.GetFileNameWithoutExtension(path));//, typeof(GameObject));
//
//		return new GameDataRequest(path, request);
//	}

//	public void Unload(bool unloadAllLoadedObjects)
//	{
//		if (!this.isDownloaded)
//			return;
//
//		foreach(AssetBundle bundle in this.assetBundleList)
//			bundle.Unload(unloadAllLoadedObjects);
//
//		this.assetBundleList.Clear();
//		this.assetDictionary.Clear();
//
//		this.isDownloaded = false;
//	}
	
//	public Coroutine RequestDownload(System.Action<float, float> progressAction, System.Action<bool, int,int> countAction, bool cleanCache)
//	{
//		return StartCoroutine(Downloading(progressAction, countAction, cleanCache));
//	}
//
//	IEnumerator Downloading(System.Action<float, float> progressAction, System.Action<bool, int, int> countAction, bool cleanCache)
//	{
//		if (this.isDownloaded || this.loadingType != LoadingType.FromCDN)
//			yield break;
//
//		/*
//		DownloadUrls downloadUrls = null;
//		if (this.loadingType == LoadingType.FromCDN)
//		{
//			while (true)
//			{
//				using (WWW www = new WWW(contentsDownloadUrl))
//				{
//					yield return StartCoroutine(WaitingDownload(www, null));
//					if (www.error != null)
//					continue;
//
//					downloadUrls = JsonMapper.ToObject<DownloadUrls>(www.text);
//					break;
//				}
//			}
//		}
//		*/
//
//		string downloadUrl = "";
//#if UNITY_ANDROID
//		downloadUrl = downloadUrls.Android;
//#elif UNITY_IPHONE
//		downloadUrl = downloadUrls.IOS;
//#else
//		downloadUrl = downloadUrls.LocalPath;
//#endif
//
//		if(cleanCache)
//			Caching.CleanCache();
//
//		while (!Caching.ready)
//			yield return null;
//
//		Caching.maximumAvailableDiskSpace = this.maximumCachingSize * 1024 * 1000;
//
//		//////////////////////////////////////////////////////////////////////////////////////////////////
//		// 목록 다운 로드
//		//////////////////////////////////////////////////////////////////////////////////////////////////
//		List<AssetBuildData> bundleList = new List<AssetBuildData>();
//		while(true)
//		{
//			using (WWW www = new WWW(downloadUrl + "/BundleList.txt"))
//			{
//				yield return StartCoroutine(WaitingDownload(www, null));
//				if (www.error != null)
//					continue;
//				Debug.Log("Downloaded " + www.url + " " + www.bytes.Length);
//				bundleList = JsonMapper.ToObject<List<AssetBuildData>>(www.text);
//				Debug.Log("bundleList " + bundleList[0].version);
//				break;
//			}
//		}
//
//		//////////////////////////////////////////////////////////////////////////////////////////////////
//		// Asset Bundle 목록 다운로드
//		//////////////////////////////////////////////////////////////////////////////////////////////////
//		long totalSize = 0;
//		foreach (AssetBuildData data in bundleList)
//			totalSize += data.size;
//
//		int fileCount = 0;
//		long donwloadedSize = 0;
//		foreach (AssetBuildData data in bundleList)
//		{
//			string url = downloadUrl + "/" + data.bundleName;
//			while (true)
//			{
//				//Debug.Log("Requset Download " + url);
//
//				bool versionCached = Caching.IsVersionCached(url, data.version);
//				if(versionCached)
//					Caching.MarkAsUsed(url, data.version);
//
//				bool isDownload = false;
//				if (countAction != null)
//					countAction(isDownload, fileCount++, bundleList.Count);
//
//				using (WWW www = WWW.LoadFromCacheOrDownload(url, data.version, data.crc32))
//				{
//					yield return StartCoroutine(WaitingDownload(www, delegate(float progress)
//					{
//						if (progressAction != null)
//						progressAction(((float)data.size * progress) / (float)data.size, ((float)donwloadedSize + data.size * progress) / (float)totalSize);
//
//						if (countAction != null && progress > 0f && !isDownload)
//						{
//						countAction(true, fileCount, bundleList.Count);
//						isDownload = true;
//						}
//					}));
//
//					if (www.error != null)
//						continue;
//
//					this.assetBundleList.Add(www.assetBundle);
//					if (!data.streamedScene)
//					{
//						foreach (string assetName in data.assetNameList)
//						this.assetDictionary.Add(assetName, www.assetBundle);
//					}
//					donwloadedSize += data.size;
//
//					if (progressAction != null)
//						progressAction(1f, (float)donwloadedSize / (float)totalSize);
//
//					yield return new WaitForSeconds(0.05f);
//
//					//Debug.Log("Downloaded " + www.url);
//					break;
//				}
//			}
//		}
//
//		yield return new WaitForSeconds(0.1f);
//
//		this.isDownloaded = true;
//	}
//
//
//
//	IEnumerator WaitingDownload(WWW www, System.Action<float> progressAction)
//	{
//		www.threadPriority = ThreadPriority.High;
//
//		if (progressAction == null)
//		{
//			//ActivityIndicator.Instance.StartActivity();
//			yield return www;
//			//ActivityIndicator.Instance.StopActivity();
//		}
//		else
//		{
//			while (!www.isDone && www.error == null)
//			{
//				progressAction(www.progress);
//				yield return null;
//			}
//		}
//
//		if (www.error == null)
//			yield break;
//
//		bool waitComfirm = true;
//
//		//GUIMessageDialog.ShowDialog("Dialog Window", "Retry Download", "Download Error", GUIMessageDialog.MessageType.NoButton, delegate(int ret)
//		//{
//		//    waitComfirm = false;
//		//});
//
//		while (waitComfirm)
//			yield return null;
//	}
}


