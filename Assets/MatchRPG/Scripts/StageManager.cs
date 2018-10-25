using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


/**********************************************
 * 
 * Contents rules which make stages and levels 
 *                             28 March 2016
 *                    created by Inseong Kim
 **********************************************/

//	All infomations are composed of 16 bits.
//
//	BoardSqaure info needs N*2 bytes (N means a square count. If the board size is 9 by 9, N is 81.)
//	Entity info needs N*2 bytes.
//	So a level needs 324 bytes (81*2 + 81*2) if the board size is 9 by 9. 
//	If a game has 1000 levels, data size for organization of levels would be almost 300~400KB.
//	But if a level has multi-boards, the size would be increased several times.
//
//	1. Board configuration
//
//	* Square & Entity (16 bit)
//	* Common bits
//		bit 16  : Type  (0=Entity, 1=Square)
//
//		* Sqaure composition
//
//		bit 14     : Occupied territory (0=Lost, 1=Mine)
//		bit 13     : Activated square (0=Unused, 1=activated)
//		bit 12     : Transport (0=Normal, 1=Portal or passable)  <- Activated~Portal bits : 00=Unused, 01=Unused but passable, 10=Normal, 11=Portal
//		bit 11~9   : Square type ( 0=Normal, 1=Generator, 2=Vending_machine, 3=Spread_machine_01, 4,5,6,7=Reserved )
//		bit 8      : Reversal (0=Normal, 1=Reversal)
//		bit 7~6    : Gravity direction (0=Down, 1=Up, 2=Left, 3=Right)
//		bit 5~4    : Slide direction (0=Down, 1=Up, 2=Left, 3=Right)
//		bit 3      : Bondage (0=Normal, 1=Bondage)
//		bit 2~0    : Count. It would be a count for breaking ice if a bondage bit is 0, and would be a count for breaking bondage if the bit is 1.
//
//		* Entity composition
//
//		bit 15		: Fixed (0=Movable, 1=Fixed)
//					- If its value is a true, this entity doesn't move to anywhere. 
//		bit 14		: Locked (0=Unlocked, 1=Locked)
//		bit 13~11	: Random (0=Normal, 1=Changeable or Random at beginning)
//		bit 10		: Reserved (Not yet used)
//		bit 9~8		: Basic Bomb (0=None, 1=Box, 2=Horizontal, 3=Vertical)
//		bit 7~5		: Extra bomb (0=None, 1=Missile, 2=ReverseBomb, 3=TimeBomb, 4=ChangingEntity, ... )
//		bit 4		: Special (0=Normal, 1=Special)
//		bit 3~0		: Type number of entities; maximum 15
//					- If the special bit is 1, those type numbers would be treated as special entities.

//	2. Additional infomations for game data files
//
//	* Stage Level header
//	  Stage header
//	  [Stage count] : 1 byte ( Maximum 256 stages )
//	     [Level count] : 1 byte ( Maximum 256 levels per a stage )
//	        [Board size] : 1 byte ( a board consits of maximum 256(=16x16) pieces; 4bit for row + 4bit for column )
//	        [Board count] : 1 byte ( Maximum 256 boards in a level )
//	            [Board info]
//	                [Square composition] : 2 x R x C bytes ( R=a row size of board, C=a column size of board )
//	                [Entity composition] : 2 x R x C bytes
//	                [Portal composition] : 2 x P  ( P=a count of portals in a level )
//	                [Directon of next board] : 1 byte (0=None, 1=Downward, 2=Upward, 3=Leftward, 4=Rightward
//	            [Board info]
//	                ...
//	            [Board info]
//	            
//	* Portal info (2 byte per a portal)
//	  - An entity item would be transfered from an in-portal to an out-portal.
//	  byte 1 : In-portal; sequencial index of play board.
//	  byte 2 : Out-portal


//[Serializable]
//public class SpriteInfo
//{
//	public GameObject relatedObject;
//	public Sprite sprite;
//}

public class StageManager : MonoBehaviour
{
	public string [] gameDataNames;
	public int currentDataIndex;

	private static StageManager _instance;
	public static StageManager instance
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
		InitStageManager();
		CalibrateCamera();
	}

	void InitStageManager()
	{
	}

	public void CalibrateCamera(bool editorFlag=true)
	{
//		float screenRatio = 16.0f / 9.0f;
//		if( Screen.width > Screen.height )
//			screenRatio = 9.0f / 16.0f;

		float screenRatio;

		if(editorFlag)
			screenRatio = 16.0f / 9.0f;
		else
			screenRatio = (float)Screen.height / (float)Screen.width;

		int tileWidth = GameDatabase.instance.tileWidth;
		int playBoardCol = GameDatabase.instance.playBoardCol;

		Camera.main.orthographicSize = ((int)(tileWidth * (playBoardCol+1) * screenRatio))/2.0f;	// the default screen rate is 9:16
	}

	public string GetCurrentGameDataName()
	{
		if( currentDataIndex >= 0 && currentDataIndex < gameDataNames.Length )
		{
			return gameDataNames[currentDataIndex];
		}

		return null;
	}
}

