using UnityEngine;
using System;
using System.Collections;

/*
* Sqaure composition
* 
	bit 14     : System square (0=Normal, 1=System)
	bit 13     : Occupied territory (0=Lost, 1=Mine)
	bit 12     : Disused square (0=Disused, 1=Activated)
	bit 11     : Transport (0=Normal, 1=Portal or passable)  <- Activated~Portal bits : 00=Unused, 01=Unused but passable, 10=Normal, 11=Portal
	bit 10~8   : Square type ( 0=Normal, 1=Generator, 2=Vending_machine, 3=Spread_machine_01, 4,5,6,7=Reserved )
	bit 7      : Reversal (0=Normal, 1=Reversal)
	//bit 7~6    : Gravity direction (0=Down, 1=Up, 2=Left, 3=Right)
	bit 5~4    : Slide direction (0=Down, 1=Up, 2=Left, 3=Right)
	//bit 3      : Bondage (0=Normal, 1=Bondage)
	bit 2~0    : Count for breaking ice
*/

public struct SquareBit
{
	//public const UInt16 CategoryBit		= 0x8000;	// 1000 0000 0000 0000
//	public const UInt16 SystemBit		= 0x4000;	// 0100 0000 0000 0000
	public const UInt16 OccupyBit		= 0x2000;	// 0010 0000 0000 0000
	public const UInt16 DisuseBit		= 0x1000;	// 0001 0000 0000 0000
	public const UInt16 PortalInBit		= 0x0800;	// 0000 1000 0000 0000
	public const UInt16 PortalOutBit	= 0x0400;	// 0000 0100 0000 0000
	public const UInt16 SquareTypeBit	= 0x0380;	// 0000 0011 1000 0000
	public const UInt16 ReversalBit		= 0x0040;	// 0000 0000 0100 0000
	public const UInt16 SlideBit		= 0x0030;	// 0000 0000 0011 0000
	//	public const UInt16 BondageBit		= 0x0000;	// 0000 0000 0000 0000
	public const UInt16 CountBit		= 0x0007;	// 0000 0000 0000 0111					

	//	public const UInt16 CategoryBitShift		= 15;
//	public const UInt16 SystemBitShift			= 14;
	public const UInt16 OccupyBitShift		= 13;
	public const UInt16 DisuseBitShift		= 12;
	public const UInt16 PortalInBitShift		= 11;
	public const UInt16 PortalOutBitShift		= 10;
	public const UInt16 SquareTypeBitShift		= 7;
	public const UInt16 ReversalBitShift		= 6;
	public const UInt16 SlideBitShift			= 4;
	public const UInt16 CountBitShift			= 0;
}

public enum SquareType
{
	Normal,
	Generator,
	VendingMachine,
	SpreadMachine,
	Reserved01,
	Reserved02,
	Reserved03,
	Reserved04,
}

public enum MoveDir
{
	Down,
	Up,
	Left,
	Right,
}

[Serializable]
public class BoardSquareData
{
//	public bool system;					// uses for generators of 1st row & last row, and it is permanent.
	public bool occupied;				// if it is true, it means this block is yours.
	public bool disused;				// if it is true, this board square isn't used.
	public bool portalIn;
	public bool portalOut;
	public SquareType squareType;
	public bool reversal;
	public MoveDir slideDir;
	public UInt16 count;				// breaking count
	public UInt16 nextIndex;

	[HideInInspector]
	public UInt16 id;

	[HideInInspector]
	public Int16 row;

	[HideInInspector]
	public Int16 col;

	public void Init()
	{
		occupied	= false;
		disused		= false;
		portalIn	= false;
		portalOut	= false;
		squareType	= SquareType.Normal;
		reversal	= false;
		slideDir	= MoveDir.Down;
		count		= 0;
	}

	public void Set(UInt16 squareBits)
	{
		Init();

//		system		= (squareBits & SquareBit.SystemBit) != 0;
		occupied	= (squareBits & SquareBit.OccupyBit) != 0;
		disused		= (squareBits & SquareBit.DisuseBit) != 0;
		portalIn	= (squareBits & SquareBit.PortalInBit) != 0;
		portalOut	= (squareBits & SquareBit.PortalOutBit) != 0;

		squareType	= (SquareType)((squareBits & SquareBit.SquareTypeBit) >> SquareBit.SquareTypeBitShift);

		if((squareBits & SquareBit.ReversalBit) != 0)
			reversal = true;

		slideDir	= (MoveDir)((squareBits & SquareBit.SlideBit) >> SquareBit.SlideBitShift);
		count	= (UInt16)((squareBits & SquareBit.CountBit) >> SquareBit.CountBitShift);

		ComposeId();
	}

	public void ComposeId()
	{
		id = 0;

//		if(system)
//			id |= SquareBit.SystemBit;
		if(occupied)
			id |= SquareBit.OccupyBit;
		if(disused)
			id |= SquareBit.DisuseBit;
		if(portalIn)
			id |= SquareBit.PortalInBit;
		if(portalOut)
			id |= SquareBit.PortalOutBit;

		id |= (UInt16)((UInt16)squareType << SquareBit.SquareTypeBitShift);

		if(reversal)
			id |= (UInt16)SquareBit.ReversalBit;

		id |= (UInt16)((UInt16)slideDir << SquareBit.SlideBitShift);
		id |= (UInt16)((count & SquareBit.CountBit) << SquareBit.CountBitShift);
	}
}