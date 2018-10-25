using UnityEngine;
using System;
using System.Collections;

//[Serializable]
public struct EntityCondition
{
	UInt16 order;
	UInt16 count;
}
//
////[Serializable]
//public struct BombCondition
//{
//	BombType type;
//	UInt16 count;
//}
//
////[Serializable]
//public struct SpecialCondition
//{
//	UInt16 id;
//
//}

//[Serializable]
public class WinCondition
{
	public UInt16 id;
	public EntityCondition [] entityConditions;
}
