using UnityEngine;
using System.Collections;

public struct PanelNames
{
	public const int TopMenu = 0;
	public const int BottomMenu = 1;
	public const int TestRun = 2;
}

public class EditorPanelManager : MonoBehaviour
{
	public GameObject [] panels;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnHideAllPanels()
	{
		for(int i = 0; i < panels.Length; i++)
		{
			panels[i].SendMessage( "OnHideShowPanel", SendMessageOptions.DontRequireReceiver );
		}
	}

	public void OnShowAllPanels()
	{
		for(int i = 0; i < panels.Length; i++)
		{
			panels[i].SetActive(true);
			panels[i].SendMessage( "OnHideShowPanel", SendMessageOptions.DontRequireReceiver );
		}
	}

	public void ShowTopPanel()
	{
		if(panels[PanelNames.TopMenu] != null)
			panels[PanelNames.TopMenu].SetActive(true);
	}

	public void HideTopPanel()
	{
		if(panels[PanelNames.TopMenu] != null)
			panels[PanelNames.TopMenu].SetActive(false);
	}

	public void ShowBottomPanel()
	{
		if(panels[PanelNames.BottomMenu] != null)
			panels[PanelNames.BottomMenu].SetActive(true);
	}

	public void HideBottomPanel()
	{
		if(panels[PanelNames.BottomMenu] != null)
			panels[PanelNames.BottomMenu].SetActive(false);
	}

	public void ShowTestRunPanel()
	{
		if(panels[PanelNames.TestRun] != null)
			panels[PanelNames.TestRun].SetActive(true);
	}

	public void HideTestRunPanel()
	{
		if(panels[PanelNames.TestRun] != null)
			panels[PanelNames.TestRun].SetActive(false);
	}

}
