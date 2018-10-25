using UnityEngine;
using System.Collections;

public class EditorTestRunPanel : MonoBehaviour
{
	public float panelSpeed = 150.0f;
	private bool showFlag = false;
	private RectTransform rectTrans;
	private Vector3 originPos;

	// Use this for initialization
	void Start ()
	{
		rectTrans = gameObject.GetComponent<RectTransform>();
		originPos =  rectTrans.localPosition;

		MoveToOrigin();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnHideShowPanel()
	{
		showFlag = !showFlag;
		if( showFlag )
		{
			StartCoroutine( ShowBottomPanel() );
		}
		else
		{
			StartCoroutine( HideBottomPanel() );
		}
	}

	IEnumerator ShowBottomPanel()
	{
		if( rectTrans != null )
		{
			while( rectTrans.localPosition.y < originPos.y )
			{
				yield return new WaitForEndOfFrame();
				rectTrans.localPosition = new Vector3( rectTrans.localPosition.x, rectTrans.localPosition.y + Time.deltaTime * panelSpeed, rectTrans.localPosition.z );;
			}

			rectTrans.localPosition = originPos;
		}

		yield return null;
	}

	IEnumerator HideBottomPanel()
	{
		RectTransform rectTrans = gameObject.GetComponent<RectTransform>();
		if( rectTrans != null )
		{
			while( rectTrans.localPosition.y > originPos.y - rectTrans.rect.height )
			{
				yield return new WaitForEndOfFrame();
				rectTrans.localPosition = new Vector3( rectTrans.localPosition.x, rectTrans.localPosition.y - Time.deltaTime * panelSpeed, rectTrans.localPosition.z );;
			}

			MoveToOrigin();
		}

		yield return null;
	}

	void MoveToOrigin()
	{
		Vector3 targetPos = rectTrans.localPosition;
		targetPos.y = originPos.y - rectTrans.rect.height;
		rectTrans.localPosition = targetPos;
	}

	public void OnStartTestGame()
	{
		EditorMenuManager.instance.StartTestGame();
	}

	public void OnStopTestGame()
	{
		SendMessageUpwards( "OnShowAllPanels", SendMessageOptions.DontRequireReceiver );
		EditorMenuManager.instance.StopTestGame();
	}
}
