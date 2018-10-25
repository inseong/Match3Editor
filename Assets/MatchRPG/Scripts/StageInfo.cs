using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class StageInfo
{
	public string name;
	public int startLevelIndex;
	public StageLevelInfo [] levels;

	public int LevelLength
	{
		get { return levels.Length; }
	}

	public StageLevelInfo GetLevel( int lv )
	{
		if( levels != null && lv < levels.Length )
			return levels[lv];

		return null;
	}
}

