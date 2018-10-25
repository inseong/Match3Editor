using UnityEngine;
using System;
using System.Collections;

public class EditorEntity : MonoBehaviour
{
	public EntityData entityData;
	public SpriteRenderer entitySprite;				// Sprite for entity itself
	public SpriteRenderer lockedSprite;				// Sprite for locked entity at beginning

	public EditorBoardSquare linkedSquare;

//	protected override void Awake()
//	{
//		base.Awake();
//	}
//
//	protected override void Start ()
//	{
//		base.Start();
//	}

//	protected override void Update ()
//	{
//		base.Update();
//	}

//	protected override void FixedUpdate()
//	{
//		base.FixedUpdate();
//	}


	public void Clear()
	{
		entityData.Clear();

		if(entitySprite != null)
			entitySprite.sprite = null;

		if(lockedSprite != null)
			lockedSprite.sprite = null;
	}

//	public void SetEntity(Entity ent)
//	{
//		entityData.Set(ent);
//		entityData.ComposeId();
//	}

//	public void SetSquare(EntityData ent)
//	{
//		base.Set(ent);
//		ComposeId();
//	}

	public void SetParent(EditorBoardSquare bs)
	{
		if(gameObject != null && bs != null && bs.gameObject != null)
		{
			gameObject.transform.SetParent(bs.gameObject.transform);
			linkedSquare = bs;
		}
	}

	public void SetSprite(Sprite sp)
	{
		SpriteRenderer sr = this.gameObject.GetComponent<SpriteRenderer>();
		if( sr != null )
			sr.sprite = sp;
	}

	public void SetEntity(UInt16 entId)
	{
		entityData.Set(entId);
//		type = entType;
//		Sprite sp = StageManager.instance.GetEntitySprite( entId );
//		SetSprite( sp );
	}
		
//	public void SetEntity(Entity ent)
//	{
//		if(ent != null)
//		{
//			base.Set(ent);
//		}
//	}
		
	public void SetEntitySprite(Sprite sp)
	{
		if(entitySprite != null)
			entitySprite.sprite = sp;
	}

	public void ShowSprites()
	{
		if(entitySprite != null)
			entitySprite.gameObject.SetActive(true);
		
		if(lockedSprite != null)
			lockedSprite.gameObject.SetActive(true);
	}

	public void HideSprites()
	{
		if(entitySprite != null)
			entitySprite.gameObject.SetActive(false);

		if(lockedSprite != null)
			lockedSprite.gameObject.SetActive(false);
	}

	public void SetDefaultEntity(UInt16 typeNum, Sprite sp)
	{
		entityData.typeNumber = typeNum;
		SetEntitySprite(sp);
		entityData.ComposeId();
	}

	public void SetBombBox()
	{
		entityData.basicBombType = BasicBombType.Box;
		entityData.specialType = SpecialType.None;

		// Initialize if the current randomtype is not the Type.
		// If the entity has RandomType.Type, it will be a box type bomb of random entity.
		if(entityData.randomType != RandomType.Type)
			entityData.randomType = RandomType.None;
		entityData.ComposeId();
		Sprite sp = PoolingManager.GetEntitySprite(entityData.key);
		if(sp != null)
			SetEntitySprite(sp);
	}

	public void SetBombHorizontal()
	{
		entityData.basicBombType = BasicBombType.Horizontal;
		entityData.specialType = SpecialType.None;
		if(entityData.randomType != RandomType.Type)
			entityData.randomType = RandomType.None;
		entityData.ComposeId();
		Sprite sp = PoolingManager.GetEntitySprite(entityData.key);
		if(sp != null)
			SetEntitySprite(sp);
	}

	public void SetBombVertical()
	{
		entityData.basicBombType = BasicBombType.Vertical;
		entityData.specialType = SpecialType.None;
		if(entityData.randomType != RandomType.Type)
			entityData.randomType = RandomType.None;
		entityData.ComposeId();
		Sprite sp = PoolingManager.GetEntitySprite(entityData.key);
		if(sp != null)
			SetEntitySprite(sp);
	}

	public void SetBombMissile()
	{
		entityData.extraBombType = ExtraBombType.Missile;
		entityData.specialType = SpecialType.None;
		if(entityData.randomType != RandomType.Type)
			entityData.randomType = RandomType.None;
		entityData.ComposeId();
		Sprite sp = PoolingManager.GetEntitySprite(entityData.key);
		if(sp != null)
			SetEntitySprite(sp);
	}

	public void SetLock(Sprite sp)
	{
		entityData.locked = true;
		if(lockedSprite != null)
			lockedSprite.sprite = sp;
	}

	public void SetRandomEntity(Sprite sp)
	{
		entityData.randomType = RandomType.Type;
		entityData.basicBombType = BasicBombType.None;
		entityData.extraBombType = ExtraBombType.None;
		entityData.specialType = SpecialType.None;
		SetEntitySprite(sp);
	}

	public void SetRandomBombBox(Sprite sp)
	{
		entityData.randomType = RandomType.Box;
		SetEntitySprite(sp);
	}

	public void SetRandomBombLine(Sprite sp)
	{
		entityData.randomType = RandomType.Line;
		SetEntitySprite(sp);
	}

	public void SetRandomBombMissle(Sprite sp)
	{
		entityData.randomType = RandomType.Missile;
		SetEntitySprite(sp);
	}

	public void SetRandomBombBoxLine(Sprite sp)
	{
		entityData.randomType = RandomType.BoxLine;
		SetEntitySprite(sp);
	}

	public void SetRandomBombAll(Sprite sp)
	{
		entityData.randomType = RandomType.BoxLineMissile;
		SetEntitySprite(sp);
	}

	public void SetMultiBomb01(Sprite sp)
	{
		entityData.specialType = SpecialType.MultiBomb01;
		entityData.typeNumber = 0;							// Must initialize by 0 because the special types don't have any type number.
		SetEntitySprite(sp);
	}

	public void SetMultiBomb02(Sprite sp)
	{
		entityData.specialType = SpecialType.MultiBomb02;
		entityData.typeNumber = 0;							// Must initialize by 0 because the special types don't have any type number.
		SetEntitySprite(sp);
	}

}

