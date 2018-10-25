using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EditorMainMenu : MonoBehaviour
{
	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public void OnNewStage()
	{

	}

	public void OnMoveStage()
	{

	}

	public void OnDeleteStage()
	{

	}

	public void OnAddLevel()
	{
			
	}

	public void OnInsertLevel()
	{

	}

	public void OnMoveLevel()
	{
	}

	public void OnDeleteLevel()
	{
	}

	public void OnQuit( string name )
	{
		SceneManager.LoadScene( name );
	}

	public void OnSwitchMenu()
	{
		gameObject.SetActive( !gameObject.activeSelf );
	}
}
