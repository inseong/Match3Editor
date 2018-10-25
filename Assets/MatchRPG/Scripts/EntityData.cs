using UnityEngine;
using System;
using System.Collections;

/*
* Entity composition
* 
	bit 15		: Fixed (0=Movable, 1=Fixed)
				- If its value is a true, this entity doesn't move to anywhere. 
	bit 14		: Locked (0=Unlocked, 1=Locked)
	bit 13~11	: Random (0=Normal, 1=Changeable or Random at beginning)
    bit 10		: Reserved (Not yet used)
    bit 9~8		: Basic Bomb (0=None, 1=Box, 2=Horizontal, 3=Vertical)
	bit 7~5		: Extra bomb (0=None, 1=Missile, 2=ReverseBomb, 3=TimeBomb, 4=ChangingEntity, ... )
	bit 4		: Special (0=Normal, 1=Special)
	bit 3~0		: Type number of entities; maximum 15
				- If the special bit is 1, those type numbers would be treated as special entities.
*/

public struct EntityBit
{
	//----- This part isn't used for composing a key. It would be used for composing ID. ------
	public const UInt16 FixedBit			= 0x8000;	// 1000 0000 0000 0000
	public const UInt16 LockedBit			= 0x4000;	// 0100 0000 0000 0000
	public const UInt16 RandomBit			= 0x3800;	// 0011 1000 0000 0000
	public const UInt16 ReservedBit			= 0x0700;	// 0000 0100 0000 0000

	//---- This part is used for a key of entity
	public const UInt16 BasicBombBit		= 0x0300;	// 0000 0011 0000 0000
	public const UInt16 ExtraBombBit		= 0x00E0;	// 0000 0000 1110 0000
	public const UInt16 SpecialBit			= 0x0010;	// 0000 0000 0001 0000
	public const UInt16 TypeNumberBit		= 0x000F;	// 0000 0000 0000 1111

	public const UInt16 FixedBitShift		= 15;
	public const UInt16 LockedBitShift		= 14;
	public const UInt16 RandomBitShift		= 11;

	public const UInt16 BasicBombBitShift	= 8;
	public const UInt16 ExtraBombBitShift	= 5;
	public const UInt16 SpecialBitShift		= 4;
	public const UInt16 TypeNumberBitShift	= 0;
}

public enum BasicBombType
{
	None,
	Box,
	Horizontal,
	Vertical,
}

public enum ExtraBombType
{
	None,
	Missile,
	ReverseBomb,
	TimeBomb,
	ChangingEntity,
}

public enum SpecialType
{
	None,
	Packing1,
	Packing2,
	Packing3,
	Packing4,
	Spreader,
	MultiBomb01,
	MultiBomb02,
	//CleanUpBomb,		// Not yet used.
	//XCrossBomb,			// Not yet used.

}

public enum RandomType
{
	None,
	Type,
	Box,
	Line,
	Missile,
	BoxLine,
	BoxLineMissile,
	//SpecialBomb,			// Not yet used.
}

[Serializable]
public struct EntityData
{
	public RandomType randomType;		// At the time of starting a level, it would be any entity randomly if this flag is checked,
	// and its type will be changed every turn while playing.
	public bool fixedPos;
	public bool locked;

	public BasicBombType basicBombType;
	public ExtraBombType extraBombType;
	public SpecialType specialType;
	public UInt16 typeNumber;				// 0 means that this is not a default entity. Entity number is started from 1.

	[HideInInspector]
	public UInt16 id;			// variable which has all informations of an entity.

	[HideInInspector]
	public UInt16 key;			// key for finding an entity prefab
	// An entity with random type doesn't need a key because it is transformed to a real entity when game is started.

	public void Clear()
	{
		Init();
		ComposeId();
	}

	public void Init()
	{
		randomType		= RandomType.None;
		fixedPos		= false;
		locked			= false;
		basicBombType	= BasicBombType.None;
		extraBombType	= ExtraBombType.None;
		specialType		= SpecialType.None;
		typeNumber		= 0;
	}

	public void Set(EntityData ent)
	{
		fixedPos		= ent.fixedPos;
		randomType		= ent.randomType;
		basicBombType	= ent.basicBombType;
		extraBombType	= ent.extraBombType;
		specialType		= ent.specialType;
		locked			= ent.locked;
		typeNumber		= ent.typeNumber;
		id				= ent.id;
		key				= ent.key;
	}

	public void Set( UInt16 entityBits )
	{
		Init();

		fixedPos		= (entityBits & EntityBit.FixedBit) != 0;
		locked			= (entityBits & EntityBit.LockedBit) != 0;
		randomType		= (RandomType)((entityBits & EntityBit.RandomBit) >> EntityBit.RandomBitShift);

		basicBombType	= (BasicBombType)((entityBits & EntityBit.BasicBombBit) >> EntityBit.BasicBombBitShift);
		extraBombType	= (ExtraBombType)((entityBits & EntityBit.ExtraBombBit) >> EntityBit.ExtraBombBitShift);
		bool specialFlag = (entityBits & EntityBit.SpecialBit) != 0;

		typeNumber	= (UInt16)((entityBits & EntityBit.TypeNumberBit) >> EntityBit.TypeNumberBitShift);
		if(specialFlag)
		{
			specialType = (SpecialType)typeNumber;
			typeNumber = 0;
		}

		ComposeId();
	}

	public void ComposeId()
	{
		UInt16 bits;
		UInt16 typeNum = typeNumber;

		id = 0;
		key = 0;

		if(fixedPos)
			id |= (UInt16)EntityBit.FixedBit;

		if(locked)
			id |= (UInt16)EntityBit.LockedBit;

		if(randomType != RandomType.None)
			id |= (UInt16)(((UInt16)randomType << EntityBit.RandomBitShift) & EntityBit.RandomBit);

		if(basicBombType != BasicBombType.None)
		{
			bits = (UInt16)(((UInt16)basicBombType << EntityBit.BasicBombBitShift) & EntityBit.BasicBombBit);
			id |= bits;
			key |= bits;					
		}

		if(extraBombType != ExtraBombType.None)
		{
			bits = (UInt16)(((UInt16)extraBombType << EntityBit.ExtraBombBitShift) & EntityBit.ExtraBombBit);
			id |= bits;
			key |= bits;
		}

		if(specialType != SpecialType.None)
		{
			bits = (UInt16)EntityBit.SpecialBit;
			id |= bits;
			key |= bits;
			typeNum = (UInt16)specialType;
		}

		bits = (UInt16)((typeNum << EntityBit.TypeNumberBitShift) & EntityBit.TypeNumberBit);
		id |= bits;
		key |= bits;
	}

	public UInt16 ComposeKey(BasicBombType basicBomb, ExtraBombType extraBomb, SpecialType special, UInt16 typeNum)
	{
		UInt16 newkey = 0;

		if(basicBomb != BasicBombType.None)
			newkey |= (UInt16)(((UInt16)basicBomb << EntityBit.BasicBombBitShift) & EntityBit.BasicBombBit);

		if(extraBomb != ExtraBombType.None)
			newkey |= (UInt16)(((UInt16)extraBomb << EntityBit.ExtraBombBitShift) & EntityBit.ExtraBombBit);

		if(special != SpecialType.None)
		{
			newkey |= (UInt16)EntityBit.SpecialBit;
			typeNum = (UInt16)specialType;
		}

		newkey |= (UInt16)((typeNum << EntityBit.TypeNumberBitShift) & EntityBit.TypeNumberBit);

		return newkey;
	}

	public void Normalize()
	{
		if(randomType != RandomType.None)
		{
			typeNumber = GameManager.instance.GetEntityTypeRandomly();

			if(randomType == RandomType.Box)
			{
				basicBombType = BasicBombType.Box;
			}
			else if(randomType == RandomType.Line)
			{
				int dir = UnityEngine.Random.Range(0, 2);
				if(dir == 0)
					basicBombType = BasicBombType.Horizontal;
				else
					basicBombType = BasicBombType.Vertical;
			}
			else if(randomType == RandomType.Missile)
			{
				extraBombType = ExtraBombType.Missile;
			}
			else if(randomType == RandomType.BoxLine)
			{
				basicBombType = (BasicBombType)(BasicBombType.Box + UnityEngine.Random.Range(0, 3));
			}
			else if(randomType == RandomType.BoxLineMissile)
			{
				int t = UnityEngine.Random.Range(0, 4);
				if(t < 3)
					basicBombType = (BasicBombType)(BasicBombType.Box + t);
				else
					extraBombType = ExtraBombType.Missile;
			}
			else
			{
				basicBombType = 0;
			}

			randomType = RandomType.None;
			ComposeId();
		}
	}
}

