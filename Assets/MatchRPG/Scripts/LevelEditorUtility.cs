using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum SpriteOrder
{
	BackTile		= 2,
	BoardSquare		= 10,
	OccupiedSquare	= 20,

	Entity			= 100,
}

public class LevelEditorUtility
{
	public static GameObject CreateButtonObject( string name, float x, float y, float w, float h, Sprite sprite, RectTransform parent )
	{
		GameObject buttonObj = new GameObject( name );

		RectTransform rect = buttonObj.AddComponent<RectTransform>();
		rect.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, w );
		rect.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, h );
		rect.localPosition = Vector3.zero;
		rect.localScale = Vector3.one;
		rect.anchorMax = new Vector2(0, 0.5f);
		rect.anchorMin = new Vector2(0, 0.5f);

		rect.SetParent( parent );
		rect.localPosition = new Vector3( x, y, 0 );
		rect.localScale = Vector3.one;

		buttonObj.AddComponent<CanvasRenderer>();

		Image img = buttonObj.AddComponent<Image>();
		img.sprite = sprite;

		buttonObj.AddComponent<Button>();

		return buttonObj;
	}

	public static GameObject CreateSpriteObject( string name, float x, float y, Sprite sprite, Transform parent, int order )
	{
		GameObject spriteObj = new GameObject( name );

		spriteObj.transform.SetParent( parent );
		spriteObj.transform.localPosition = new Vector3( x, y, 0 );
		spriteObj.transform.localScale = Vector3.one;

		SpriteRenderer sr = spriteObj.AddComponent<SpriteRenderer>();
		sr.sprite = sprite;
		sr.sortingOrder = order;

		return spriteObj;
	}

	public static GameObject CreateSpriteObject( string name, Sprite sprite, Transform parent, int order )
	{
		GameObject spriteObj = new GameObject( name );

		spriteObj.transform.SetParent( parent );
		spriteObj.transform.localPosition = new Vector3( 0, 0, 0 );
		spriteObj.transform.localScale = Vector3.one;

		SpriteRenderer sr = spriteObj.AddComponent<SpriteRenderer>();
		sr.sprite = sprite;
		sr.sortingOrder = order;

		return spriteObj;
	}
}

