using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PoolingManager : MonoBehaviour
{
	public Dictionary<UInt16, Entity> matchDic;				// matching dictionary for entities <item id, GameObject>

	private static PoolingManager _instance;
	public static PoolingManager instance
	{
		get
		{
			return _instance;
		}
	}

	//-------------------------------------------
	// Member functions

	void Awake()
	{
		if( _instance == null )
		{
			_instance = this;
			DontDestroyOnLoad(this);
		}
	}

	// Use this for initialization
	void Start ()
	{
		InitPoolingManager();
	}
//	
//	// Update is called once per frame
//	void Update ()
//	{
//	
//	}

	void InitPoolingManager()
	{
		matchDic = new Dictionary<UInt16, Entity>();

		//	BoardSquare [] squares		= GameDatabase.instance.squares;
		Entity [] defaultEntities	= GameDatabase.instance.defaultEntities;
		Entity [] basicBombs		= GameDatabase.instance.basicBombs;
		Entity [] extraBombs		= GameDatabase.instance.extraBombs;
		Entity [] specialEntities	= GameDatabase.instance.specialEntities;

		if(defaultEntities != null && defaultEntities.Length > 0)
			for( int i = 0; i < defaultEntities.Length; i++ )
				if(defaultEntities[i] != null)
				{
					defaultEntities[i].entityData.ComposeId();
					if(matchDic.ContainsKey(defaultEntities[i].entityData.key) == false)
						matchDic.Add(defaultEntities[i].entityData.key, defaultEntities[i]);
					else
						Debug.LogAssertion("Same key in defaultEntities["+i+"]");
				}

		if(basicBombs != null && basicBombs.Length > 0)
			for( int i = 0; i < basicBombs.Length; i++ )
				if(basicBombs[i] != null)
				{
					basicBombs[i].entityData.ComposeId();
					if(matchDic.ContainsKey(basicBombs[i].entityData.key) == false)
						matchDic.Add(basicBombs[i].entityData.key, basicBombs[i]);
					else
						Debug.LogAssertion("Same key in basicBombs["+i+"]");
				}

		if(extraBombs != null && extraBombs.Length > 0)
			for( int i = 0; i < extraBombs.Length; i++ )
				if(extraBombs[i] != null)
				{
					extraBombs[i].entityData.ComposeId();
					if(matchDic.ContainsKey(extraBombs[i].entityData.key) == false)
						matchDic.Add(extraBombs[i].entityData.key, extraBombs[i]);
					else
						Debug.LogAssertion("Same key in extraBombs["+i+"]");
				}

		if(specialEntities != null && specialEntities.Length > 0)
			for( int i = 0; i < specialEntities.Length; i++ )
				if(specialEntities[i] != null)
				{
					specialEntities[i].entityData.ComposeId();
					if(matchDic.ContainsKey(specialEntities[i].entityData.key) == false)
						matchDic.Add(specialEntities[i].entityData.key, specialEntities[i]);
					else
						Debug.LogAssertion("Same key in specialEntities["+i+"]");
				}
	}

	//*****  There is no a pooling system yet. *****
	public static Entity InstantiateByPooling(UInt16 key)
	{
		Entity foundEntity;
		GameObject newObj = null;

		if(_instance.matchDic.TryGetValue(key, out foundEntity) && foundEntity != null)
		{
			newObj = Instantiate(foundEntity.gameObject);
			foundEntity = newObj.GetComponent<Entity>();
		}

		return foundEntity;
	}

	public static void DestroyPooling(GameObject obj, UInt16 objectId)
	{
		Destroy(obj);
	}

	public static void DestroyPooling(Entity ent)
	{
		if(ent != null && ent.gameObject != null)
			Destroy(ent.gameObject);
	}

	public static void DestroyPooling(BoardSquare bs)
	{
		if(bs != null && bs.gameObject != null)
			Destroy(bs.gameObject);
	}
	//************************************************

	public static Sprite GetEntitySprite( UInt16 key )
	{
		Entity ent;

		if(_instance.matchDic.TryGetValue(key, out ent) )
		{
			SpriteRenderer sr = ent.GetComponent<SpriteRenderer>();
			if( sr != null )
				return sr.sprite;
		}
		return null;
	}

}

