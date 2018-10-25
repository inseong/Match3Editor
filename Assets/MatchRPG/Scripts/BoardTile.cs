using UnityEngine;
using System.Collections;

public class BoardTile : MonoBehaviour
{
	public void SetSprite( Sprite sp )
	{
		SpriteRenderer sr = this.gameObject.GetComponent<SpriteRenderer> ();
		if (sr != null)
			sr.sprite = sp;
	}
}

