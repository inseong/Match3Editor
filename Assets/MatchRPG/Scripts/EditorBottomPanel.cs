using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EditorBottomPanel : MonoBehaviour
{
	public float panelSpeed = 150.0f;
	private bool showFlag = true;
	private RectTransform rectTrans;
	private Vector3 originPos;
	//float panelHeight;

	// Use this for initialization
	void Start ()
	{
		rectTrans = gameObject.GetComponent<RectTransform>();
		originPos =  rectTrans.localPosition;
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public void OnHideShowPanel()
	{
		showFlag = !showFlag;
		if( showFlag )
		{
			StartCoroutine( ShowingBottomPanel() );
		}
		else
		{
			StartCoroutine( HidingBottomPanel() );
		}
	}

	IEnumerator ShowingBottomPanel()
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

	IEnumerator HidingBottomPanel()
	{
		if( rectTrans != null )
		{
			while( rectTrans.localPosition.y > originPos.y - rectTrans.rect.height )
			{
				yield return new WaitForEndOfFrame();
				rectTrans.localPosition = new Vector3( rectTrans.localPosition.x, rectTrans.localPosition.y - Time.deltaTime * panelSpeed, rectTrans.localPosition.z );;
			}
			Vector3 targetPos = rectTrans.localPosition;
			targetPos.y = originPos.y - rectTrans.rect.height;
			rectTrans.localPosition = targetPos;
		}

		SendMessageUpwards( "HideBottomPanel", SendMessageOptions.DontRequireReceiver );
		yield return null;
	}
}
