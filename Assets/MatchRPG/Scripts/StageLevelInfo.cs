using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//[Serializable]
//public struct BoardSquareData
//{
//	public bool system;					// uses for generators of 1st row & last row, and it is permanent.
//	public bool occupied;				// if it is true, it means this block is yours.
//	public bool disused;				// if it is true, this board square isn't used.
//	public bool portalIn;
//	public bool portalOut;
//	public SquareType squareType;
//	public bool reversal;
//	public MoveDir slideDir;
//	public UInt16 count;				// breaking count
//	public int nextIndex;
//
//	[HideInInspector]
//	public UInt16 id;
//
//	[HideInInspector]
//	public int row;
//
//	[HideInInspector]
//	public int col;
//
//	public void Set(BoardSquare bs)
//	{
//		if( bs != null )
//		{
//			system		= bs.system;
//			occupied	= bs.occupied;
//			disused		= bs.disused;
//			portalIn	= bs.portalIn;
//			portalOut	= bs.portalOut;
//			squareType	= bs.squareType;
//			reversal	= bs.reversal;
//			slideDir	= bs.slideDir;
//			count		= bs.count;
//			nextIndex	= bs.nextIndex;
//		}
//	}
//}
//
//[Serializable]
//public struct EntityData
//{
//	public RandomType randomType;		// At the time of starting a level, it would be any entity randomly if this flag is checked,
//										// and its type will be changed every turn while playing.
//	public bool fixedPos;
//	public bool locked;
//
//	public BasicBombType basicBombType;
//	public ExtraBombType extraBombType;
//	public SpecialType specialType;
//	public UInt16 typeNumber;			// 0 means that this is not a default entity. Entity number is started from 1.
//
//	[HideInInspector]
//	public UInt16 id;					// variable which has all informations of an entity.
//
//	[HideInInspector]
//	public UInt16 key;					// key for finding an entity prefab
//
//	public void Set(Entity ent)
//	{
//		if(ent != null)
//		{
//			fixedPos		= ent.fixedPos;
//			randomType		= ent.randomType;
//			basicBombType	= ent.basicBombType;
//			extraBombType	= ent.extraBombType;
//			specialType		= ent.specialType;
//			locked			= ent.locked;
//			typeNumber		= ent.typeNumber;
//			id				= ent.id;
//			key				= ent.key;
//		}
//	}
//}

[Serializable]
public class StageLevelInfo
{
	public UInt16 usedEntities;				// flag bits of used base entities; a level are composed of maximum 16 entities.
	public int rowSize, colSize;				// the contents size of this level
//	public BoardSquareData [] squares;
//	public EntityData [] entities;
	public UInt16 [,] squares;
	public UInt16 [,] entities;
	public UInt16 [,] portals;
	public int reverseBoundary;

	public StageLevelInfo()
	{
	}

	public StageLevelInfo(int r, int c)
	{
		Initialize(r, c);
	}

	public void Initialize(int r, int c)
	{
		if(r > 0 && c > 0)
		{
			rowSize = r;
			colSize = c;
			squares = new UInt16[r, c];
			entities = new UInt16[r, c];

//			for(int i = 0; i < squares.Length; i++)
//				squares[i] = new BoardSquareData();
//			for(int i = 0; i < entities.Length; i++)
//				entities[i] = new EntityData();
			
			InitUsedEntityBits();

			reverseBoundary = rowSize;
		}
	}

	public void InitUsedEntityBits()
	{
		int max = 0;
		if(GameDatabase.instance.defaultEntities != null && GameDatabase.instance.defaultEntities.Length > 0)
			max = GameDatabase.instance.defaultEntities.Length;
		if(max > 16)
			max = 16;
		
		UInt16 compare = 0x0001;
		for(int i = 0; i < max; i++)
		{
			usedEntities |= compare;
			compare <<= 1;
		}
	}

	public bool CopyFrom(StageLevelInfo lv)
	{
		if(lv != null)
		{
			usedEntities = lv.usedEntities;

			rowSize = lv.rowSize;
			if(rowSize <= 0)
				rowSize = GameDatabase.instance.playBoardRow;
			
			colSize = lv.colSize;
			if(colSize <= 0)
				colSize = GameDatabase.instance.playBoardCol;

			if(rowSize > 0 && colSize > 0)
			{
				//int size = width*height;
				if(lv.squares != null && lv.squares.Length > 0 && lv.entities != null && lv.entities.Length > 0 )
				{
					squares = new UInt16[rowSize, colSize];
					if(lv.squares.Length == rowSize*colSize)
						for(int r = 0; r < rowSize; r++)
							for(int c = 0; c < colSize; c++)
								squares[r, c] = lv.squares[r, c];
					
					entities = new UInt16[rowSize, colSize];
					if(lv.entities.Length == rowSize*colSize)
						for(int r = 0; r < rowSize; r++)
							for(int c = 0; c < colSize; c++)
								entities[r, c] = lv.entities[r, c];

					return true;
				}
			}
		}

		return false;
	}

	public void SetSquare(BoardSquareData bs)
	{
		if(bs != null && squares != null)
		{
			bs.ComposeId();
			squares[bs.row, bs.col] = bs.id;
		}
	}

	public void SetEntity(int r, int c, UInt16 entityId)
	{
		if(entities != null)
		{
			entities[r, c] = entityId;
		}
	}

//	public BoardSquare GetSquare(int index)
//	{
//		BoardSquare bs = new BoardSquare();
//		if(index >= 0 && index < squares.Length)
//			bs.Set(squares[index], -1);
//
//		return bs;
//	}

//	public Entity GetEntity(int index)
//	{
//		Entity ent = new Entity();
//		if(index >= 0 && index < entities.Length)
//			ent.Set(entities[index]);
//
//		return ent;
//	}
}

