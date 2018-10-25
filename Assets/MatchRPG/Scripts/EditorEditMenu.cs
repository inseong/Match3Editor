using UnityEngine;
using System.Collections;

public class EditorEditMenu : MonoBehaviour
{
	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
	}

	public void OnUndo()
	{
		OnShowEditMenu( false );
	}

	public void OnRedo()
	{
		OnShowEditMenu( false );
	}

	public void OnRandom()
	{
		StageLevelInfo level = EditorMenuManager.instance.GetCurrentLevel();
		if( level != null )
		{
			EditorMenuManager.instance.AssignEntitiesRandomly();
			OnShowEditMenu( false );
		}
	}

//	public void OnRemoveEntity()
//	{
//		EditorMenuManager.instance.fsm.ChangeState( typeof(EditorMenuManager.RemoveEntityState));
//		OnShowEditMenu( false );
//	}

	public void OnClearAllEntities()
	{
		EditorMenuManager.instance.RemoveAllEntities();
		OnShowEditMenu( false );
	}

	public void OnInitializeBoard()
	{
		EditorMenuManager.instance.InitializeBoard();
		OnShowEditMenu( false );
	}

//	public void OnSwitchSquare()
//	{
//		EditorMenuManager.instance.fsm.ChangeState( typeof(EditorMenuManager.SquareOnOffState) );
//		OnShowEditMenu( false );
//	}

	public void OnClearAllSquares()
	{
		EditorMenuManager.instance.ClearAllSquares();
		OnShowEditMenu( false );
	}

	public void OnShowEditMenu( bool show )
	{
		SendMessageUpwards( "OnCloseWindow", SendMessageOptions.DontRequireReceiver );
	}
}
