using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class GameDatabaseMenu
{
	[MenuItem("Assets/Create/MatchRPG/Game Database")]
	static public void CreateGameData()
	{
		ScriptableObjectUtility.CreateAsset<GameDatabase>();
	}

}