using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class GameDatabase : ScriptableObject
{
	public int playBoardRow = 9;
	public int playBoardCol = 9;
	public int tileWidth = 128;
	public int tileHeight = 128;
	public int entityBufferSizeInGenerator = 9;

	public int clearedStage;
	public int clearedLevel;

	public Sprite [] backSprites;
	public BoardSquare [] squares;
	public Entity [] defaultEntities;
	public Entity [] basicBombs;
	public Entity [] extraBombs;
	public Entity [] specialEntities;

	public StageInfo [] stages;

	private static GameDatabase _instance;
	public static GameDatabase instance
	{
		get
		{
			if (_instance == null)
			{
				LoadDatabase( StageManager.instance.GetCurrentGameDataName() );
			}
//			Debug.LogAssertion("GameDatabase didn't be loaded.");
			return _instance;
		}
	}

	public static void LoadDatabase( string databaseName )
	{
#if UNITY_EDITOR
		string path = "Assets/MatchRPG/Resources/" + databaseName + ".asset";
#else
		string path = databaseName+".asset";
#endif
		_instance = GameDataLoader.instance.Load( path, typeof(GameDatabase)) as GameDatabase;
	}

	public int StageLength
	{
		get
		{
			if( stages != null )
				return stages.Length;
			return 0;
		}
	}

	public StageInfo GetStage(int stg)
	{
		if( stages != null && stg >= 0 && stg < stages.Length)
		{
			return stages[stg];
		}

		return null;
	}

	public StageLevelInfo GetLevel( int stg, int lv )
	{
		StageInfo stage = GetStage( stg );
		if( stage != null )
			return stage.GetLevel( lv );

		return null;
	}
}

