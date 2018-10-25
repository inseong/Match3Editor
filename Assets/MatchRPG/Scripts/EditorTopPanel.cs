using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EditorTopPanel : MonoBehaviour
{
	public float panelSpeed = 150.0f;
	public Dropdown selectStage;
	public Dropdown selectLevel;

	private bool showFlag = true;
	private EntityScrollManager entityScrollManager;
	private RectTransform rectTrans;
	private Vector3 originPos;

//	private delegate void HideShowPanel();

	// Use this for initialization
	void Start ()
	{
		entityScrollManager = GetComponentInChildren<EntityScrollManager>();

		rectTrans = gameObject.GetComponent<RectTransform>();
		originPos =  rectTrans.localPosition;
	}

	// Update is called once per frame
	void Update ()
	{

	}

//	void FixedUpdate()
//	{
//		
//	}

	public void OnSelectStage( int value )
	{
		
	}

	public void OnSelectLevel( int value )
	{
		
	}

	public void OnHideShowPanel()
	{
		showFlag = !showFlag;
		if( showFlag )
		{
			entityScrollManager.OnShowAllButtons();
			StartCoroutine( ShowingTopPanel() );
		}
		else
		{
			StartCoroutine( HidingTopPanel() );
		}
	}

	IEnumerator ShowingTopPanel()
	{
		if( rectTrans != null )
		{
			while( rectTrans.localPosition.y > originPos.y )
			{
				rectTrans.localPosition = new Vector3( rectTrans.localPosition.x, rectTrans.localPosition.y - Time.deltaTime * panelSpeed, rectTrans.localPosition.z );;
				yield return new WaitForFixedUpdate();
			}

			rectTrans.localPosition = originPos;
		}

		yield return null;
	}

	IEnumerator HidingTopPanel()
	{
		if( rectTrans != null )
		{
			while( rectTrans.localPosition.y < originPos.y + rectTrans.rect.height )
			{
				rectTrans.localPosition = new Vector3( rectTrans.localPosition.x, rectTrans.localPosition.y + Time.deltaTime * panelSpeed, rectTrans.localPosition.z );;
				yield return new WaitForFixedUpdate();
			}
			Vector3 targetPos = rectTrans.localPosition;
			targetPos.y = originPos.y + rectTrans.rect.height;
			rectTrans.localPosition = targetPos;

			entityScrollManager.OnHideAllButtons();
		}

		SendMessageUpwards( "HideTopPanel", SendMessageOptions.DontRequireReceiver );
		yield return null;
	}

	public void OnTestRun()
	{
		SendMessageUpwards( "OnHideAllPanels", SendMessageOptions.DontRequireReceiver );
		//StageManager.instance.InitLevel( selectStage.value, selectLevel.value );	//selectStage
		EditorMenuManager.instance.StartTestGame();
	}
}
