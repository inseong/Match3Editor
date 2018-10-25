using UnityEngine;
using System.Collections;

public class PopupWindow : MonoBehaviour
{
	public enum PopupType
	{
		Confirm,
		Notice,
		SaveConfirm
	}

	public PopupType popupType = PopupType.Notice;
	public GameObject confirmWindow;
	public GameObject noticeWindow;
	public GameObject saveConfirmWindow;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetType( PopupType type )
	{
		
	}
}
