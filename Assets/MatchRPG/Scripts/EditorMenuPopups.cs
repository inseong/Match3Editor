using UnityEngine;
using System.Collections;

public class EditorMenuPopups : MonoBehaviour
{
	public GameObject mainMenuWindow;
	public GameObject editMenuWindow;

	// Use this for initialization
	void Start () {
	
	}

	// Update is called once per frame
	void Update ()
	{
	}

	public void OnShowMainMenuWindow()
	{
		this.gameObject.SetActive( true );
		EditorMenuManager.instance.fsm.Pause( true );
		if( mainMenuWindow != null )
			mainMenuWindow.SetActive( true );

		if( editMenuWindow != null )
			editMenuWindow.SetActive( false );
	}

	public void OnShowEditMenuWindow()
	{
		this.gameObject.SetActive( true );
		EditorMenuManager.instance.fsm.Pause( true );
		if( mainMenuWindow != null )
			mainMenuWindow.SetActive( false );

		if( editMenuWindow != null )
			editMenuWindow.SetActive( true );
	}

	public void OnCloseWindow()
	{
		this.gameObject.SetActive(false);
		EditorMenuManager.instance.fsm.Pause( false );
	}
}
