using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class EntityScrollManager : MonoBehaviour
{
	public Vector2 boxSize = new Vector2( 40.0f, 40.0f );
	public float buttonSpace = 5.0f;
	public RectTransform squarePanel;
	public RectTransform entityPanel;
	public RectTransform specialPanel;
	public Button [] buttons;

	private RectTransform contentRect;
	float buttonPosX;
	float buttonPosY;
	float buttonWidth;

	private static EntityScrollManager _instance;
	public static EntityScrollManager instance
	{
		get
		{
			return _instance;
		}
	}

	void Awake()
	{
		if( _instance == null )
			_instance = this;
	}

	// Use this for initialization
	void Start () 
	{
		ScrollRect scrollRect = GetComponent<ScrollRect>();
		contentRect = scrollRect.content;

		squarePanel.gameObject.SetActive(false);
		entityPanel.gameObject.SetActive(false);

		float startX = boxSize.x/2.0f + buttonSpace;
		float buttonWidth = boxSize.x + buttonSpace;

		if( squarePanel != null )
			squarePanel.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal
				, buttonWidth * EditorMenuManager.instance.squareButtons.Length + buttonSpace );

		if( entityPanel != null )
			entityPanel.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal
				, buttonWidth * EditorMenuManager.instance.entityButtons.Length + buttonSpace );

//		ArrangeBoardSquareButtons( startX, buttonWidth, squarePanel, EditorMenuManager.instance.squareButtons );
		EditorMenuManager.instance.ArrangeBoardSquareButtons(startX, squarePanel);
		EditorMenuManager.instance.ArrangeEntityButtons(startX, entityPanel);
//		ArrangeEntityButtons( startX, buttonWidth, entityPanel, EditorMenuManager.instance.entityButtons );
		EventSystem.current.UpdateModules();

		ShowBoardSquares();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnArrangeBoardSquare()
	{
		ShowBoardSquares();
		EditorMenuManager.instance.fsm.ChangeState( typeof(EditorMenuManager.BoardSquareState) );
	}

	public void OnArrangeEntities()
	{
		ShowEntities();
		EditorMenuManager.instance.fsm.ChangeState( typeof(EditorMenuManager.EntityState) );
	}

	public void OnArrangeSpecialEntities()
	{
		ShowSpecialEntities();
		EditorMenuManager.instance.fsm.ChangeState( typeof(EditorMenuManager.SpecialEntityState) );
	}

	public void ShowBoardSquares()
	{
		if( squarePanel != null )
		{
			squarePanel.gameObject.SetActive( true );
			contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, squarePanel.rect.width );
		}

		if( entityPanel != null )
		{
			entityPanel.gameObject.SetActive( false );
		}

		if( specialPanel != null )
		{
			specialPanel.gameObject.SetActive( false );
		}
	}

	public void ShowEntities()
	{
		if( entityPanel != null )
		{
			entityPanel.gameObject.SetActive( true );
			contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, entityPanel.rect.width );
		}

		if( squarePanel != null )
		{
			squarePanel.gameObject.SetActive( false );
		}

		if( specialPanel != null )
		{
			specialPanel.gameObject.SetActive( false );
		}
	}

	public void ShowSpecialEntities()
	{
		if( specialPanel != null )
		{
			specialPanel.gameObject.SetActive( true );
			contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, specialPanel.rect.width );
		}
			
		if( entityPanel != null )
		{
			entityPanel.gameObject.SetActive( false );
		}

		if( squarePanel != null )
		{
			squarePanel.gameObject.SetActive( false );
		}
	}

	public void OnHideAllButtons()
	{
		for(int i = 0; i < buttons.Length; i++)
		{
			buttons[i].gameObject.SetActive( false );
		}
	}

	public void OnShowAllButtons()
	{
		for(int i = 0; i < buttons.Length; i++)
		{
			buttons[i].gameObject.SetActive( true );
		}
	}

//	void ArrangeBoardSquareButtons( float sx, float width, RectTransform parent, EditorSquareButton [] squareButtons )
//	{		
//		if( parent != null && squareButtons != null && squareButtons.Length > 0 )
//		{
//			int boardLength = squareButtons.Length;
//
//			GameObject icon;
//			EditorSquareButton btn;
//
//			//parent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * boardLength + blockSpace);
//			RectTransform rect;
//			for( int i = 0; i < boardLength; i++ )
//			{
////				icon = LevelEditorUtility.CreateButtonObject( "sq_icon_"+i, i * width + sx, 0, boxSize.x, boxSize.y, squareButtons[i].sprite, parent );
////				btn = icon.AddComponent<EditorSquareButton>();
////				btn.menuId = squareButtons[i].menuId;
//				rect = squareButtons[i].GetComponent<RectTransform>();
//				rect.SetParent( parent );
//				rect.localPosition = new Vector3( i * width + sx, 0, 0 );
//
//			}
//		}
//	}


//	void ArrangeEntityButtons( float sx, float width, RectTransform parent, EditorEntityButton [] entityButtons )
//	{		
//		if( parent != null && entityButtons != null && entityButtons.Length > 0 )
//		{
//			int boardLength = entityButtons.Length;
//
//			GameObject icon;
//			EditorEntityButton btn;
//
//			for( int i = 0; i < boardLength; i++ )
//			{
//				icon = LevelEditorUtility.CreateButtonObject( "ent_icon_"+i, i * width + sx, 0, boxSize.x, boxSize.y, entityButtons[i].sprite, parent );
//				btn = icon.AddComponent<EditorEntityButton>();
//				btn.menuId = entityButtons[i].menuId;
//			}
//		}
//	}

}
