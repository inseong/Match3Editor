using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EditorSquareButton : MonoBehaviour
{
	public SquareMenu menuId;
	public Sprite sprite;		// For assigning a sprite to a board square in editor mode

	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public void OnClick()
	{
		EditorMenuManager.instance.fsm.ChangeState( typeof(EditorMenuManager.BoardSquareState) );
		EditorMenuManager.selectedSquareMenu = this;
		if(menuId != SquareMenu.Portal)
		{
			EditorMenuManager.BoardSquareState state = (EditorMenuManager.BoardSquareState)EditorMenuManager.instance.fsm.currentState;
			state.SetNormalState();
		}
	}

	public void OnClickPortal()
	{
		EditorMenuManager.instance.fsm.ChangeState( typeof(EditorMenuManager.BoardSquareState) );
		EditorMenuManager.selectedSquareMenu = this;
		if(menuId == SquareMenu.Portal)
		{
			EditorMenuManager.BoardSquareState state = (EditorMenuManager.BoardSquareState)EditorMenuManager.instance.fsm.currentState;
			state.SetPortalState();
		}

	}
}

