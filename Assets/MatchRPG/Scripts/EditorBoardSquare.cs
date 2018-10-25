using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class EditorBoardSquare : MonoBehaviour
{
	public BoardSquareData squareData;
	public BoardTile tile;

	public SpriteRenderer squareSprite;
	public SpriteRenderer occupiedSprite;
	public SpriteRenderer portalInSprite;
	public SpriteRenderer portalOutSprite;
	public SpriteRenderer lockedSprite;
	public SpriteRenderer reversalSprite;
	public SpriteRenderer slideSprite;

	[HideInInspector]
	public EditorEntity editingEntity;

//	public FSM<EditorBoardSquare> fsm;

	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{

	}

	public void OnClick()
	{
	}

	public void Init()
	{
		squareData.Init();

		SetSquareSprite(null);

		if(squareSprite != null)
			squareSprite.sprite = null;
		
		if(occupiedSprite != null)
			occupiedSprite.sprite = null;
		
		if(portalInSprite != null)
			portalInSprite.sprite = null;
		
		if(portalOutSprite != null)
			portalOutSprite.sprite = null;
		
		if(lockedSprite != null)
			lockedSprite.sprite = null;
		
		if(reversalSprite != null)
			reversalSprite.sprite = null;
		
		if(slideSprite != null)
			slideSprite.sprite = null;

	}

	public void SetSquareSprite( Sprite sp )
	{
		if(squareSprite != null)
			squareSprite.sprite = sp;
	}

	public void SetTileSprite(Sprite tileSprite)
	{
		if(tile != null)
			tile.SetSprite(tileSprite);	
	}

	public void ShowTile(bool show)
	{
		tile.gameObject.SetActive(show);
	}

	public void SetEmpty()
	{
		Init();
		squareData.ComposeId();
	}

	public void SetEraseBlank(Sprite sp)
	{
		Init();
		squareData.disused = true;
		squareData.portalIn = true;
		squareData.portalOut = true;
		squareData.reversal = false;
		SetSquareSprite(sp);
		squareData.ComposeId();

		EraseEntity();
		ShowTile(true);
	}

	public void SetEraseBlock(Sprite sp)
	{
		Init();
		squareData.disused = true;
		squareData.portalIn = false;
		squareData.portalOut = false;
		SetSquareSprite(sp);
		squareData.ComposeId();

		EraseEntity();
		ShowTile(false);
	}

	public void SetGenerator(Sprite sp)
	{
		squareData.disused = false;
		squareData.squareType = SquareType.Generator;
		SetSquareSprite(sp);
		squareData.ComposeId();
		ShowTile(true);
	}

	public void SetVending(Sprite sp)
	{
		squareData.disused = false;
		squareData.squareType = SquareType.Normal;
		SetSquareSprite(sp);
		squareData.ComposeId();
		ShowTile(true);
	}

	public void SetPortalIn(Sprite sp)
	{
		if(!squareData.disused)// && !squareData.system)
		{
			squareData.squareType = SquareType.Normal;
			SetSquareSprite(null);
			squareData.portalIn = true;
			portalInSprite.sprite = sp;
			squareData.ComposeId();
		}
	}

	public void SetPortalOut(Sprite sp)
	{
		if(!squareData.disused)// && !squareData.system)
		{
			squareData.squareType = SquareType.Normal;
			SetSquareSprite(null);
			squareData.portalOut = true;
			portalOutSprite.sprite = sp;
			squareData.ComposeId();
		}
	}

	public void RemovePortalOut()
	{
		if(!squareData.disused && portalOutSprite != null)
		{
			squareData.portalOut = false;
			portalOutSprite.sprite = null;
			squareData.ComposeId();

		}
		portalOutSprite.sprite = null;
	}

	public void SetSlideDown(Sprite sp)
	{
		if(!squareData.disused)
		{
			squareData.slideDir = MoveDir.Down;
			slideSprite.sprite = sp;
			squareData.ComposeId();
		}
	}

	public void SetSlideLeft(Sprite sp)
	{
		if(!squareData.disused)
		{
			squareData.slideDir = MoveDir.Left;
			slideSprite.sprite = sp;
			squareData.ComposeId();
		}
	}

	public void SetSlideUp(Sprite sp)
	{
		if(!squareData.disused)
		{
			squareData.slideDir = MoveDir.Up;
			slideSprite.sprite = sp;
			squareData.ComposeId();
		}
	}

	public void SetSlideRight(Sprite sp)
	{
		if(!squareData.disused)
		{
			squareData.slideDir = MoveDir.Right;
			slideSprite.sprite = sp;
			squareData.ComposeId();
		}
	}

	public void SetLock1(Sprite sp)
	{
		if(!squareData.disused)
		{
			squareData.count = 1;
			lockedSprite.sprite = sp;
			squareData.ComposeId();
		}
	}

	public void SetLock2(Sprite sp)
	{
		if(!squareData.disused)
		{
			squareData.count = 2;
			lockedSprite.sprite = sp;
			squareData.ComposeId();
		}
	}

	public void SetLock3(Sprite sp)
	{
		if(!squareData.disused)
		{
			squareData.count = 3;
			lockedSprite.sprite = sp;
			squareData.ComposeId();
		}
	}

	public void SetReverse(Sprite sp)
	{
		if(!squareData.disused)
		{
			squareData.reversal = true;
			reversalSprite.sprite = sp;
			squareData.ComposeId();
		}
	}

	public void RemoveReverse()
	{
		if(!squareData.disused)
		{
			squareData.reversal = false;
			reversalSprite.sprite = null;
			squareData.ComposeId();
		}
	}

	public void EraseEntity()
	{
		if(editingEntity != null)
		{
			if(editingEntity != null && editingEntity.gameObject != null)
				Destroy(editingEntity.gameObject);
			
			editingEntity = null;
			//entity.Clear();
		}
	}

	public void MoveEntityToNext()
	{
		// Conditions for moving an entity to next square
		// 1.  A next sqaure is empty.
		// 2. A next square isn't empty and left square of the mext square is empty.
		// 3. A next square and its left square aren't empty, but its right square is empty.
	}

	public virtual void TransmitEntityToNext()
	{
		//EditorBoardSquare nextBS = GetNextSquare();
	}

	public virtual void ReceiveEntityFromPrevious()
	{
		
	}

//	public void AssignSquareMenu(EditorSquareButton menu)
//	{
//		if(menu != null)
//		{
//			usedMenu = menu;
//			SetSquare(menu.square);
//			SetSprite(menu.sprite);
//		}
//	}

	public void AssignEntityMenu(EditorEntity ent, Sprite sp)
	{
		if(ent != null && ent.gameObject != null)
		{
			if(editingEntity != null)
				DestroyObject(ent.gameObject);

			GameObject newObj = Instantiate(ent.gameObject);
			newObj.transform.parent = this.transform;
			editingEntity = newObj.GetComponent<EditorEntity>();
			editingEntity.SetEntitySprite(sp);
		}
	}
}

