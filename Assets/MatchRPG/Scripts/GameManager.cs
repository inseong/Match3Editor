using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum CombineType
{
	None,
	Normal,
	BoxBomb,
	HorizontalBomb,
	VerticalBomb,
	Missile,
	MultiBomb01,
	MultiBomb02,
}

public struct BoardPos
{
	public int row;
	public int col;

	public BoardPos(int r, int c)
	{
		row = r;
		col = c;
	}
}

public class CrashInfo
{
	public BoardPos [] squares;

	public CombineType type;

	public CrashInfo() {}
	public CrashInfo(CombineType t, int count)
	{
		type = t;
		squares = new BoardPos[count];
	}
}

public class GameManager : MonoBehaviour
{
	public GameObject elementRoot;
	public GameObject defaultSquare;
	public GameObject entityMask;
	public float moveSpeed = 60.0f;
	public float maxMoveSpeed = 100.0f;
	public float swipeSpeed = 140.0f;
	public float gatheringTime = 0.2f;

	public string currentStateName;
	public float displayCullY;

	public List<CrashInfo> crashList = new List<CrashInfo>();
	public FSM<GameManager> fsm;

	[HideInInspector]
	public BoardSquare [,] board;

	[HideInInspector]
	public int reverseBoundary;

	private List<UInt16> usedEntityIndexes	= new List<UInt16>();

	private Sprite [] backSprites;
	private int playBoardRow;
	private int playBoardCol;
	private int tileWidth;
	private int tileHeight;
	private float startX;
	private float startY;
	private BoardSquare selectedSquare, nextSquare;
	private bool touchDown = false;
	private UInt32 [] movingFlag;

	private static GameManager _instance;
	public static GameManager instance
	{
		get
		{
			return _instance;
		}
	}

	void Awake()
	{
		Application.targetFrameRate = 60;

		if( _instance == null )
		{
			_instance = this;
			DontDestroyOnLoad(this);
		}

		fsm = new FSM<GameManager>(this);
	}

	// Use this for initialization
	void Start ()
	{
		fsm.ChangeState( typeof(ReadyStartState) );
		InitGameManager();
	}
	
//	// Update is called once per frame
//	void Update ()
//	{
//		fsm.currentState.Update();
//	}

	void FixedUpdate ()
	{
		fsm.currentState.FixedUpdate();
		currentStateName = fsm.currentState.ToString();
	}

	void LateUpdate()
	{
		fsm.currentState.LateUpdate();
	}

	void InitGameManager()
	{
		backSprites		= GameDatabase.instance.backSprites;
		playBoardRow	= GameDatabase.instance.playBoardRow;
		playBoardCol	= GameDatabase.instance.playBoardCol;
		tileWidth		= GameDatabase.instance.tileWidth;
		tileHeight		= GameDatabase.instance.tileHeight;

		startX = -((float)playBoardCol-1)*tileWidth / 2.0f;
		startY = (float)(playBoardRow-1)*tileHeight / 2.0f;

		int cnt = playBoardRow*playBoardCol/32;
		if(playBoardRow*playBoardCol%32 > 0)
			++cnt;
		movingFlag = new UInt32[cnt];

		reverseBoundary = playBoardRow;
	}

	void InitUsedEntityList(StageLevelInfo level)
	{
		UInt16 compare = 0x0001;

		int max = GameDatabase.instance.defaultEntities.Length;
		if(max > 16)
			max = 16;

		usedEntityIndexes.Clear();

		for(UInt16 i = 1; i <= max; i++)
		{
			if((level.usedEntities & compare) > 0)
				usedEntityIndexes.Add(i);

			compare <<= 1;
		}
	}

	public UInt16 GetEntityTypeRandomly()
	{
		if( usedEntityIndexes != null && usedEntityIndexes.Count > 0 )
			return usedEntityIndexes[UnityEngine.Random.Range( 0, usedEntityIndexes.Count)];

		return 0;
	}

	public UInt16 GetEntityKeyRandomly()
	{
		UInt16 tp = GetEntityTypeRandomly();
		EntityData ent = new EntityData();
		ent.typeNumber = tp;
		ent.ComposeId();
		return ent.key;
	}

	public void AllocateBoardSquares()
	{
		if(board != null)
		{
			for(int r = 0; r < playBoardRow; r++)
				for(int c = 0; c < playBoardCol; c++)
					PoolingManager.DestroyPooling(board[r,c]);
		}
		else
			board = new BoardSquare[playBoardRow, playBoardCol];

		for(int r = 0; r < playBoardRow; r++)
		{
			for(int c = 0; c < playBoardCol; c++)
				board[r,c] = CreateSquareForPlaying(r, c);
		}
	}

	public void InitBoardSquares(StageLevelInfo level)
	{
		BoardSquare bs;
		EntityData normalEntity = new EntityData();
		Entity entity;

		for(int r = 0; r < playBoardRow; r++)
		{
			for(int c = 0; c < playBoardCol; c++)
			{
				bs = board[r, c];
				bs.Set(level.squares[r, c]);

				if(bs.squareData.disused)
					bs.HideSprites();
				else
				{
					UInt16 t = level.entities[r, c];
					normalEntity.Set(t);
					normalEntity.Normalize();
					entity = PoolingManager.InstantiateByPooling(normalEntity.key);

					if(entity != null)
					{
						entity.SetParent(bs);
						bs.entity = entity;
						entity.gameObject.transform.localPosition = Vector3.zero;
						entity.gameObject.transform.localScale = Vector3.one;
					}

					// Set a flag of reversal
					if(r >= level.reverseBoundary)
					{
						bs.ShowReversalSprite(true);
						bs.squareData.reversal = true;
					}
					else
					{
						bs.ShowReversalSprite(false);
						bs.squareData.reversal = false;
					}
					bs.squareData.ComposeId();
				}

				// Add a mask object
				if(bs.squareData.squareType == SquareType.Generator
					|| bs.squareData.disused == true && bs.squareData.portalIn == true)
				{
					GameObject maskObject = Instantiate(entityMask);
					maskObject.name = "mask_"+r+"_"+c;
					maskObject.transform.SetParent(bs.transform);

					Vector3 pos = Vector3.zero;//bs.transform.localPosition;

					if(bs.squareData.squareType == SquareType.Generator)
					{
						if(bs.squareData.reversal)
							pos.y -= tileHeight;
						else
							pos.y += tileHeight;
					}

					maskObject.transform.localPosition = pos;
					maskObject.transform.localScale = Vector2.one;
					bs.entityMask = maskObject;
				}

				bs.InitCullingY();
				bs.StartFiniteState();
			}
		}
	}

	public BoardSquare CreateSquareForPlaying(int r, int c)
	{
		GameObject squareObj;
		BoardSquare square;

		squareObj = Instantiate(defaultSquare);

		squareObj.name = "bd_"+r+"_"+c;
		squareObj.transform.SetParent(elementRoot.transform);
		squareObj.transform.localPosition = new Vector3( startX+c*tileWidth, startY-r*tileHeight, 0 );
		squareObj.transform.localScale = Vector3.one;

		square = squareObj.GetComponent<BoardSquare>();
		square.SetTileSprite(backSprites[(c+r%2) % 2]);

		square.squareData.row = (Int16)r;
		square.squareData.col = (Int16)c;
		square.squareData.ComposeId();

		return square;
	}

	public void RunGame(StageLevelInfo level)
	{
		if(level != null)
		{
			int sd = (int)Time.realtimeSinceStartup;
			UnityEngine.Random.InitState(sd);

			AllocateBoardSquares();
			InitUsedEntityList(level);
			InitBoardSquares(level);

			reverseBoundary = level.reverseBoundary;

			StartCoroutine(RunGame());
		}
	}

	IEnumerator RunGame()
	{
		yield return new WaitForEndOfFrame();
		fsm.ChangeState( typeof(RearrangeState) );
	}

	public static void DestroyGame()
	{
		for(int r = 0; r < _instance.playBoardRow; r++)
			for(int c = 0; c < _instance.playBoardCol; c++)
			{
				_instance.board[r,c].DestroyEntity();
				_instance.board[r,c].Clear();
			}
	}

	public List<CrashInfo> GetCrashList()
	{
		CrashInfo crashInfo;
		int r, c;
		int hcount, vcount;
		int typeNum;

		bool [,] check = new bool[playBoardRow, playBoardCol];

		for(r = 0; r < playBoardRow; r++)
			for(c = 0; c < playBoardCol; c++)
				check[r,c] = false;

		crashList.Clear();

		for(r = playBoardRow-1; r >= 0 ; r--)
			for(c = 0; c < playBoardCol; c++)
			{
				if(check[r, c] == false && !board[r, c].squareData.disused && board[r, c].entity != null)
				{
					typeNum = board[r, c].entity.entityData.typeNumber;

					hcount = 1;
					while(hcount < 5 && c+hcount < playBoardCol && !check[r, c+hcount] && board[r, c+hcount].entity != null && typeNum == board[r, c+hcount].entity.entityData.typeNumber)
						++hcount;

					vcount = 1;
					while(vcount < 5 && r-vcount > 0 && !check[r-vcount, c] && board[r-vcount, c].entity != null && typeNum == board[r-vcount, c].entity.entityData.typeNumber)
						++vcount;

					if((hcount == 2 && vcount > 1 && vcount < 4 || hcount > 1 && hcount < 4 && vcount == 2) && !check[r-1, c+1]
						&& board[r-1, c+1].entity != null && typeNum == board[r-1, c+1].entity.entityData.typeNumber)		// Check whether it is a missile or not.
					{
						crashInfo = new CrashInfo(CombineType.Missile, 4);
						crashInfo.squares[0] = new BoardPos(r, c);
						crashInfo.squares[1] = new BoardPos(r, c+1);
						crashInfo.squares[2] = new BoardPos(r-1, c);
						crashInfo.squares[3] = new BoardPos(r-1, c+1);

						check[r, c] = true;
						check[r, c+1] = true;
						check[r-1, c] = true;
						check[r-1, c+1] = true;

						c += 1;

						crashList.Add(crashInfo);
					}
					else if(hcount > 2 || vcount > 2)
					{
						if(hcount < vcount)
						{
							if(vcount == 5)
							{
								if(c > 0 && !check[r-2, c-1] && board[r-2, c-1].entity != null && typeNum == board[r-2, c-1].entity.entityData.typeNumber)
								{
									crashInfo = new CrashInfo(CombineType.MultiBomb02, vcount+1);

									crashInfo.squares[0] = new BoardPos(r-2, c);
									crashInfo.squares[1] = new BoardPos(r, c);
									crashInfo.squares[2] = new BoardPos(r-1, c);
									crashInfo.squares[3] = new BoardPos(r-3, c);
									crashInfo.squares[4] = new BoardPos(r-4, c);
									crashInfo.squares[5] = new BoardPos(r-2, c-1);

									for(int i = 0; i < vcount; i++)										
										check[r-i, c] = true;
									check[r-2, c-1] = true;
								}
								else if(c < playBoardCol-1 && !check[r-2, c+1] && board[r-2, c+1].entity != null && typeNum == board[r-2, c+1].entity.entityData.typeNumber)
								{
									crashInfo = new CrashInfo(CombineType.MultiBomb02, vcount+1);

									crashInfo.squares[0] = new BoardPos(r-2, c);
									crashInfo.squares[1] = new BoardPos(r, c);
									crashInfo.squares[2] = new BoardPos(r-1, c);
									crashInfo.squares[3] = new BoardPos(r-3, c);
									crashInfo.squares[4] = new BoardPos(r-4, c);
									crashInfo.squares[5] = new BoardPos(r-2, c+1);

									for(int i = 0; i < vcount; i++)										
										check[r-i, c] = true;
									check[r-2, c+1] = true;
								}
								else
								{
									crashInfo = new CrashInfo(CombineType.MultiBomb01, vcount);
									for(int i = 0; i < vcount; i++)
									{
										crashInfo.squares[i] = new BoardPos(r-i, c);
										check[r-i, c] = true;
									}
								}
							}
							else if(vcount == 4)
							{
								crashInfo = new CrashInfo(CombineType.HorizontalBomb, vcount);
								for(int i = 0; i < vcount; i++)
								{
									crashInfo.squares[i] = new BoardPos(r-i, c);
									check[r-i, c] = true;
								}
							}
							else
							{
								crashInfo = new CrashInfo(CombineType.Normal, vcount);

								for(int i = 0; i < vcount; i++)
								{
									crashInfo.squares[i] = new BoardPos(r-i, c);
									check[r-i, c] = true;
								}
							}

							crashList.Add(crashInfo);
						}
						else
						{
							if(hcount == 5)
							{
								if(r > 0 && !check[r-1, c+2] && board[r-1, c+2].entity != null && typeNum == board[r-1, c+2].entity.entityData.typeNumber)
								{
									crashInfo = new CrashInfo(CombineType.MultiBomb02, hcount+1);

									crashInfo.squares[0] = new BoardPos(r, c+2);
									crashInfo.squares[1] = new BoardPos(r, c);
									crashInfo.squares[2] = new BoardPos(r, c+1);
									crashInfo.squares[3] = new BoardPos(r, c+3);
									crashInfo.squares[4] = new BoardPos(r, c+4);
									crashInfo.squares[5] = new BoardPos(r-1, c+2);

									for(int i = 0; i < hcount; i++)										
										check[r, c+i] = true;
									check[r-1, c+2] = true;
								}
								else if(r < playBoardRow-1 && !check[r+1, c+2] && board[r+1, c+2].entity != null && typeNum == board[r+1, c+2].entity.entityData.typeNumber)
								{
									crashInfo = new CrashInfo(CombineType.MultiBomb02, hcount+1);

									crashInfo.squares[0] = new BoardPos(r, c+2);
									crashInfo.squares[1] = new BoardPos(r, c);
									crashInfo.squares[2] = new BoardPos(r, c+1);
									crashInfo.squares[3] = new BoardPos(r, c+3);
									crashInfo.squares[4] = new BoardPos(r, c+4);
									crashInfo.squares[5] = new BoardPos(r+1, c+2);

									for(int i = 0; i < hcount; i++)										
										check[r, c+i] = true;
									check[r+1, c+2] = true;
								}
								else
								{
									crashInfo = new CrashInfo(CombineType.MultiBomb01, hcount);
									for(int i = 0; i < hcount; i++)
									{
										crashInfo.squares[i] = new BoardPos(r, c+i);
										check[r, c+i] = true;
									}
								}
							}
							else if(hcount == 4)
							{
								crashInfo = new CrashInfo(CombineType.VerticalBomb, hcount);
								for(int i = 0; i < hcount; i++)
								{
									crashInfo.squares[i] = new BoardPos(r, c+i);
									check[r, c+i] = true;
								}
							}
							else
							{
								int r1, r2;
								crashInfo = null;

								// looks for matched entities by below patterns.
								//   #     #     #
								//   #     #     #
								//   ###  ###  ###

								if(r > 1)
								{
									r1 = r-1;
									r2 = r-2;
									crashInfo = FindThreeByThree(check, typeNum, r, c, r1, r2);
								}

								// looks for matched entities by below patterns.
								//   #     #     #
								//   ###  ###  ###
								//   #     #     #

								if(crashInfo == null && r > 0 && r < playBoardRow-1)
								{
									r1 = r-1;
									r2 = r+1;
									crashInfo = FindThreeByThree(check, typeNum, r, c, r1, r2);
								}

								// looks for matched entities by below patterns.
								//   ###  ###  ###
								//   #     #     #
								//   #     #     #
								if(crashInfo == null && r >= 0 && r < playBoardRow-2)
								{
									r1 = r+1;
									r2 = r+2;
									crashInfo = FindThreeByThree(check, typeNum, r, c, r1, r2);
								}

								if(crashInfo == null)
								{
									crashInfo = new CrashInfo(CombineType.Normal, hcount);

									for(int i = 0; i < hcount; i++)
									{
										crashInfo.squares[i] = new BoardPos(r, c+i);
										check[r, c+i] = true;
									}
								}
							}

							c += hcount-1;
							crashList.Add(crashInfo);
						}
					}
				}
			}
		
		return crashList;
	}

	CrashInfo FindThreeByThree(bool [,] check, int typeNum, int r, int c, int r1, int r2)
	{
		int c1, c2;
		CrashInfo crashInfo = null;

		for(int i = 0; i < 3; i++)
		{
			if(!check[r1, c+i] && board[r1, c+i].entity != null && typeNum == board[r1, c+i].entity.entityData.typeNumber
				&& !check[r2, c+i] && board[r2, c+i].entity != null && typeNum == board[r2, c+i].entity.entityData.typeNumber)
			{
				crashInfo = new CrashInfo(CombineType.BoxBomb, 5);
				crashInfo.squares[0] = new BoardPos(r, c+i);

				if(i == 0)
				{
					c1 = c+1;
					c2 = c+2;
				}
				else if(i == 1)
				{
					c1 = c;
					c2 = c+2;
				}
				else
				{
					c1 = c;
					c2 = c+1;
				}

				crashInfo.squares[1] = new BoardPos(r, c1);
				crashInfo.squares[2] = new BoardPos(r, c2);
				crashInfo.squares[3] = new BoardPos(r1, c+i);
				crashInfo.squares[4] = new BoardPos(r2, c+i);
				check[r, c+i] = true;
				check[r, c1] = true;
				check[r, c2] = true;
				check[r1, c+i] = true;
				check[r2, c+i] = true;
				break;
			}
		}

		return crashInfo;
	}

	/*===========================================================
	 * 1. Count same entities by four directions.
	 * 2. Calculate the number of horizontal and vertical entities.
	*/

	public CrashInfo FindMatchedAt(int r, int c)
	{
		CrashInfo crashInfo = null;
		int leftCount, rightCount;
		int upCount, downCount;
		int horizontal, vertical;
		int typeNum;
		int tmpR, tmpC, checkR, checkC;

		typeNum = board[r, c].entity.entityData.typeNumber;

		leftCount = 0;
		while(leftCount < 5 && c-1-leftCount >= 0 && board[r, c-1-leftCount].entity != null && typeNum == board[r, c-1-leftCount].entity.entityData.typeNumber)
			++leftCount;

		rightCount = 0;
		while(rightCount < 5 && c+1+rightCount < playBoardCol && board[r, c+1+rightCount].entity != null && typeNum == board[r, c+1+rightCount].entity.entityData.typeNumber)
			++rightCount;
		
		upCount = 0;
		while(upCount < 5 && r-1-upCount >= 0 && board[r-1-upCount, c].entity != null && typeNum == board[r-1-upCount, c].entity.entityData.typeNumber)
			++upCount;

		downCount = 0;
		while(downCount < 5 && r+1+downCount < playBoardRow && board[r+1+downCount, c].entity != null && typeNum == board[r+1+downCount, c].entity.entityData.typeNumber)
			++downCount;

		horizontal = leftCount + rightCount + 1;
		vertical = upCount + downCount + 1;

		// Finding matched elements for missile firstly.
		//        #
		//      E # E    
		//    # # x # #  =>  Four positions which would be checked in all cases.
		//      E # E
		//        #
		//                     E # E                 E #      # E
		//   Next four case -  # x #  or  # x #  or  # x  or  x # - are required to check two positions. 
		//                                E # E      E #      # E
		//
		// x : entity which is moved in the square.
		// E : entity which would be checked.
		//
		if(horizontal == 3 && vertical == 2)
		{
			//   E # E
			//   # x #  or  # x #
			//              E # E
			//
			if(leftCount == rightCount)
			{
				checkR = -upCount + downCount;
				checkC = -leftCount;
				tmpR = r + checkR;
				tmpC = c + checkC;

				if( board[tmpR, tmpC].entity != null && typeNum == board[tmpR, tmpC].entity.entityData.typeNumber)
					crashInfo = new CrashInfo(CombineType.Missile, horizontal+vertical);
				else
				{
					checkC = rightCount;
					tmpC = c + checkC;
					if( board[tmpR, tmpC].entity != null && typeNum == board[tmpR, tmpC].entity.entityData.typeNumber)
						crashInfo = new CrashInfo(CombineType.Missile, horizontal+vertical);
				}

				if(crashInfo != null)
				{
					crashInfo.squares[0] = new BoardPos(r, c);
					crashInfo.squares[1] = new BoardPos(r, tmpC);
					crashInfo.squares[2] = new BoardPos(tmpR, c);
					crashInfo.squares[3] = new BoardPos(tmpR, tmpC);
					crashInfo.squares[4] = new BoardPos(r, c-checkC);				// the opposite entity of the checked 'E' position.
				}
			}
			else
			{
				//   # E                        E #
				//   x # #   or   x # #   or  # # x  or  # # x
				//                # E                      E #
				//       ^
				//      

				int cnt;
				if(leftCount > rightCount)
				{
					cnt = leftCount;
					checkC = -1;
					tmpC = c + checkC;
				}
				else
				{
					cnt = rightCount;
					checkC = 1;
				}

				tmpR = r - upCount + downCount;
				tmpC = c + checkC;

				if( board[tmpR, tmpC].entity != null && typeNum == board[tmpR, tmpC].entity.entityData.typeNumber)
				{
					crashInfo = new CrashInfo(CombineType.Missile, horizontal+vertical);
					crashInfo.squares[0] = new BoardPos(r, c);
					crashInfo.squares[1] = new BoardPos(r, tmpC);
					crashInfo.squares[2] = new BoardPos(tmpR, c);
					crashInfo.squares[3] = new BoardPos(tmpR, tmpC);
					crashInfo.squares[4] = new BoardPos(r, c+checkC*cnt);
				}
			}
		}
		else if(horizontal == 2 && vertical == 3)
		{
			//   # E        E #
			//   x #   or   # x
			//   # E        E #
			//
			if(upCount == downCount)
			{
				checkR = -upCount;
				checkC = -leftCount + rightCount;
				tmpR = r + checkR;
				tmpC = c + checkC;

				if( board[tmpR, tmpC].entity != null && typeNum == board[tmpR, tmpC].entity.entityData.typeNumber)
					crashInfo = new CrashInfo(CombineType.Missile, horizontal+vertical);
				else
				{
					checkR = downCount;
					tmpR = r + checkR;
					if( board[tmpR, tmpC].entity != null && typeNum == board[tmpR, tmpC].entity.entityData.typeNumber)
						crashInfo = new CrashInfo(CombineType.Missile, horizontal+vertical);
				}

				if(crashInfo != null)
				{
					crashInfo.squares[0] = new BoardPos(r, c);
					crashInfo.squares[1] = new BoardPos(r, tmpC);
					crashInfo.squares[2] = new BoardPos(tmpR, c);
					crashInfo.squares[3] = new BoardPos(tmpR, tmpC);
					crashInfo.squares[4] = new BoardPos(r-checkR, c);				// the opposite entity of the checked 'E' position.
				}
			}
			else
			{
				//   #                        #
				//   # E                    E #
				//   x #    or   x #   or   # x   or   # x
				//               # E                   E #
				//               #                       #

				int cnt;
				if(upCount > downCount)
				{
					cnt = upCount;
					checkR = -1;
					tmpR = c + checkR;
				}
				else
				{
					cnt = downCount;
					checkR = 1;
				}

				tmpR = r + checkR;
				tmpC = c - leftCount + rightCount;

				if( board[tmpR, tmpC].entity != null && typeNum == board[tmpR, tmpC].entity.entityData.typeNumber)
				{
					crashInfo = new CrashInfo(CombineType.Missile, horizontal+vertical);
					crashInfo.squares[0] = new BoardPos(r, c);
					crashInfo.squares[1] = new BoardPos(r, tmpC);
					crashInfo.squares[2] = new BoardPos(tmpR, c);
					crashInfo.squares[3] = new BoardPos(tmpR, tmpC);
					crashInfo.squares[4] = new BoardPos(r+checkR*cnt, c);
				}
			}
		}
		else if(horizontal == 2 && vertical == 2)
		{
			tmpR = r - upCount + downCount;
			tmpC = c - leftCount + rightCount;
			if(board[tmpR, tmpC].entity != null && typeNum == board[tmpR, tmpC].entity.entityData.typeNumber)
			{
				crashInfo = new CrashInfo(CombineType.Missile, horizontal+vertical);
				crashInfo.squares[0] = new BoardPos(r, c);
				crashInfo.squares[1] = new BoardPos(r, tmpC);
				crashInfo.squares[2] = new BoardPos(tmpR, c);
				crashInfo.squares[3] = new BoardPos(tmpR, tmpC);
			}
		}

		if(crashInfo == null && (horizontal > 2 || vertical > 2))
		{
			if(horizontal == 5 || vertical == 5)
			{
				// 
				//                   #  <- additional elements for MultiBomb02
				//           r-> # # # # #
				//                   c
				if(horizontal == 5 && vertical > 1 || vertical == 5 && horizontal > 1)
					crashInfo = new CrashInfo(CombineType.MultiBomb02, horizontal+vertical-1);
				else
					crashInfo = new CrashInfo(CombineType.MultiBomb01, horizontal+vertical-1);
			}
			else if(horizontal == 4)
			{
				//                   #   <- additional elements for BoxBomb
				//                   #
				//           r-> # # # #    or    # # # #
				//                   c
				//
				if(vertical > 2)
					crashInfo = new CrashInfo(CombineType.BoxBomb, horizontal+vertical-1);
				else
				{
//					upCount = 0;
//					downCount = 0;
					crashInfo = new CrashInfo(CombineType.VerticalBomb, horizontal);
				}
			}
			else if(vertical == 4)
			{
				if(horizontal > 2)
					crashInfo = new CrashInfo(CombineType.BoxBomb, horizontal+vertical-1);
				else
				{
//					leftCount = 0;
//					rightCount = 0;
					crashInfo = new CrashInfo(CombineType.HorizontalBomb, vertical);
				}
			}
			else
			{
				//                 #     <- additional elements for BoxBomb
				//                 #  
				//           r-> # # #     or    # # #
				//                 c 
				//
				if(horizontal == vertical)
					crashInfo = new CrashInfo(CombineType.BoxBomb, horizontal+vertical-1);
				else
					crashInfo = new CrashInfo(CombineType.Normal, 3);
			}

			crashInfo.squares[0] = new BoardPos(r, c);
			int index = 1;
			if(crashInfo.type == CombineType.MultiBomb02 || vertical > 2)
			{
				for(int i = 0; i < upCount; i++)
					crashInfo.squares[index++] = new BoardPos(r-1-i, c);
				for(int i = 0; i < downCount; i++)
					crashInfo.squares[index++] = new BoardPos(r+1+i, c);
			}

			if(crashInfo.type == CombineType.MultiBomb02 || horizontal > 2)
			{
				for(int i = 0; i < leftCount; i++)
					crashInfo.squares[index++] = new BoardPos(r, c-1-i);
				for(int i = 0; i < rightCount; i++)
					crashInfo.squares[index++] = new BoardPos(r, c+1+i);
			}
		}

		return crashInfo;
	}

	public BoardSquare SelectSquare(Vector3 pos)
	{
		int x = (int)(pos.x - board[0,0].transform.position.x + (float)tileWidth/2.0f);
		int y = tileHeight*playBoardRow - (int)(board[0,0].transform.position.y + pos.y + (float)tileHeight/2.0f);

		if( x > 0 && y > 0)
		{
			int c = x / tileWidth;
			int r = y / tileHeight;

			if( c < playBoardCol && r < playBoardRow )
			{
				return board[r, c];
			}
		}

		return null;
	}

	void InitMovingFlag()
	{
		for(int i = 0; i < movingFlag.Length; i++)
			movingFlag[i] = 0;
	}

	void InitToMoveEntityOfAllSquares()
	{
		for(int r = playBoardRow-1; r >= 0; r--)
			for(int c = playBoardCol-1; c >= 0; c--)
			{
				board[r,c].InitToMoveEntity();
			}
	}

	void UpdateAllOfNextSquares()
	{
		for(int r = reverseBoundary-1; r >= 0 ; r--)
			for(int c = playBoardCol-1; c >= 0; c--)
			{
				board[r,c].SetupNext();
				board[r,c].UserFixedUpdate();
			}
		
		for(int r = reverseBoundary; r < playBoardRow; r++)
			for(int c = 0; c < playBoardCol; c++)
			{
				board[r,c].SetupNext();
				board[r,c].UserFixedUpdate();
			}
	}

//	void FixedUpdateAllSquares()
//	{
//		for(int r = playBoardRow-1; r >= 0; r--)
//			for(int c = playBoardCol-1; c >= 0; c--)
//			{
//				board[r,c].UserFixedUpdate();
//			}
//	}


	public void SetMovingFlag(int r, int c)
	{
		int index = r*playBoardCol + c;
		movingFlag[index/32] |= ((UInt32)1 << index%32);
	}

	public void RemoveMovingFlag(int r, int c)
	{
		int index = r*playBoardCol + c;
		movingFlag[index/32] &= ~((UInt32)1 << index%32);
	}
		
	public bool IsEntityMoving(int r, int c)
	{
		int index = r*playBoardCol + c;
		return (movingFlag[index/32] & ((UInt32)1 << index%32)) > 0;
	}

	public bool IsAllStopped()
	{
		UInt32 mov = 0;

		for(int i = 0; i < movingFlag.Length; i++)
		{
			if(movingFlag[i] != 0)
				++mov;
		}

		return mov > 0;
	}

//	public BoardSquare FindNext(BoardSquare bs)
//	{
//		BoardSquare retSq = null;
//		int r = bs.squareData.row + bs.dirY;
//		int c = bs.squareData.col + bs.dirX;
//
//		if(r >= 0 && r < playBoardRow && c >= 0 && c < playBoardCol)
//		{
//			if(!board[r, c].squareData.disused && board[r, c].entity == null)
//				retSq = board[r, c];
//			else
//			{
//				BoardSquare sq, blockSq;
//				int nextCol = bs.squareData.col-1;
//
//				if(nextCol >= 0)
//				{
//					blockSq = board[bs.squareData.row, nextCol];
//					sq = board[r, nextCol];
//					if(blockSq.squareData.disused && !sq.squareData.disused && sq.entity == null)
//						retSq = sq;
//				}
//
//				nextCol = bs.squareData.col+1;
//
//				if(retSq == null && nextCol < playBoardCol)
//				{
//					blockSq =board[bs.squareData.row, nextCol];
//					sq = board[r, nextCol];
//					if(blockSq.squareData.disused && !sq.squareData.disused && sq.entity == null)
//						retSq = sq;
//				}
//			}
//		}
//
//		return retSq;
//	}
//
	//----------------------------------------------------------------------------------
	// FSM Classes
	public class PauseState : BaseFSM<GameManager>
	{
		
	}

	public class ReadyStartState : BaseFSM<GameManager>
	{
		public delegate void DisplayCallback();

		public override void Begin()
		{
			base.Begin();
		}
		protected override void FixedUpdateFunc()
		{
		}

		public override void Finish()
		{
			base.Finish();
		}

		public void DisplayReadyView()
		{
		}

		public void FinishedReadView()
		{
			owner.fsm.ChangeState( typeof(NormalState) );
		}
	}

	public class NormalState : BaseFSM<GameManager>
	{
		Vector3 startPos, pos, entityPos;
		float d, dx, dy, adx, ady;
		BoardSquare bs;
		CrashInfo crash;
	
		public NormalState() {}

		public override void Begin()
		{
			base.Begin();

			owner.selectedSquare = null;
			owner.nextSquare = null;
		}

		protected override void FixedUpdateFunc()
		{
			if(Input.GetMouseButton(0))
			{
				if(owner.touchDown == false)
				{
					startPos = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
					bs = owner.SelectSquare(startPos);
					if(bs != null && bs.entity != null)
						owner.selectedSquare = bs;
					else
						owner.selectedSquare = null;

					owner.nextSquare = null;
					owner.touchDown = true;

//					if(bs != null && bs.entity != null)
//					{
//						SpriteRenderer sr = bs.entity.GetComponent<SpriteRenderer>();
//						owner.displayCullY = bs.transform.position.y;
//						sr.material.SetFloat("_Trim", bs.cullingY);
//					}
				}
				else if(owner.selectedSquare != null && owner.touchDown)
				{
					pos = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
					dx = pos.x-startPos.x;
					dy = pos.y-startPos.y;
					adx = Mathf.Abs(dx);
					ady = Mathf.Abs(dy);

					if(adx > 10 || ady > 10)
					{
						int r = owner.selectedSquare.squareData.row;
						int c = owner.selectedSquare.squareData.col;
						if(adx > ady)
						{
							if(dx > 0 && c < owner.playBoardCol-1 && owner.board[r, c+1].squareData.disused == false && owner.board[r, c+1].entity != null)
							{
								owner.nextSquare = owner.board[r, c+1];
							}
							else if(dx < 0 && c > 0 && owner.board[r, c-1].squareData.disused == false && owner.board[r, c-1].entity != null)
							{
								owner.nextSquare = owner.board[r, c-1];
							}
						}
						else if(adx < ady)
						{
							if(dy < 0 && r < owner.playBoardRow-1 && owner.board[r+1, c].squareData.disused == false && owner.board[r+1, c].entity != null)
							{
								owner.nextSquare = owner.board[r+1, c];
							}
							else if(dy > 0 && r > 0 && owner.board[r-1, c].squareData.disused == false && owner.board[r-1, c].entity != null)
							{
								owner.nextSquare = owner.board[r-1, c];
							}
						}

						if(owner.nextSquare != null)
						{
							Entity tempEnt;

							// Swipe two entities temporarily for finding matched elements.
							tempEnt = owner.selectedSquare.entity;
							owner.selectedSquare.entity = owner.nextSquare.entity;
							owner.nextSquare.entity = tempEnt;

							owner.crashList.Clear();

							crash = owner.FindMatchedAt(r, c);
							if(crash != null)
								owner.crashList.Add(crash);
							
							crash = owner.FindMatchedAt(owner.nextSquare.squareData.row, owner.nextSquare.squareData.col);
							if(crash != null)
								owner.crashList.Add(crash);

							// Recover two changed entities.
							tempEnt = owner.selectedSquare.entity;
							owner.selectedSquare.entity = owner.nextSquare.entity;
							owner.nextSquare.entity = tempEnt;

							if(owner.crashList.Count > 0)
								owner.fsm.ChangeState(typeof(SwapEntitiesState));
							else
							{
								owner.fsm.ChangeState(typeof(SwapFailedState));
							}
						}
					}
				}
			}
			else
			{
				owner.selectedSquare = null;
				owner.nextSquare = null;
				owner.touchDown = false;
			}
		}
	}

	public class RearrangeState : BaseFSM<GameManager>
	{
		private int entityCount;
		private List<int> movingList;

		public override void Begin()
		{
			base.Begin();

			owner.InitMovingFlag();
			owner.InitToMoveEntityOfAllSquares();
		}

		public override void Finish()
		{
			base.Finish();
			owner.GetCrashList();
			owner.fsm.ChangeState(typeof(CrashState));
		}

		protected override void FixedUpdateFunc()
		{
			owner.UpdateAllOfNextSquares();
			if(!owner.IsAllStopped())
			{
				Finish();
				//owner.StartCoroutine(DelayedFinish());
			}
		}

//		IEnumerator DelayedFinish()
//		{
//			if(!owner.IsAnyEntityMoving())
//				Finish();
//		}

//		protected override void LateUpdateFunc()
//		{
////			owner.UpdateAllOfNextSquares();
//			if(!owner.IsAnyEntityMoving())
//			{
//				Finish();
//			}
//		}
	}

	public class SwapEntitiesState : BaseFSM<GameManager>
	{
		private int entityCount;

		public override void Begin()
		{
			base.Begin();

			entityCount = 2;
			owner.selectedSquare.entity.ChangeToSwipeAnimationState(this, owner.nextSquare, true);
			owner.nextSquare.entity.ChangeToSwipeAnimationState(this, owner.selectedSquare, false);

		}

		public override void Finish()
		{
			base.Finish();

			Entity tempEnt = owner.selectedSquare.entity;
			owner.selectedSquare.entity = owner.nextSquare.entity;
			owner.nextSquare.entity = tempEnt;
			owner.selectedSquare.entity.SetParent(owner.selectedSquare);
			owner.nextSquare.entity.SetParent(owner.nextSquare);

			// If ChangeState() is called directly, it may not be run the crash animation in the CrashState of first entity in some cases.
			// Those cases are occured when the selected entity is moved from right to left, or from downside to upside.
			owner.StartCoroutine(DelayedChange());
		}

		IEnumerator DelayedChange()
		{
			yield return new WaitForEndOfFrame();
			owner.fsm.ChangeState(typeof(CrashState));
		}

		public void Discount()
		{
			if(--entityCount <= 0)
				Finish();
		}
	}

	public class SwapFailedState : BaseFSM<GameManager>
	{
		private int entityCount;

		public override void Begin()
		{
			base.Begin();

			entityCount = 2;
			owner.selectedSquare.entity.ChangeToSwipeFailedAnimationState(this, owner.nextSquare, true);
			owner.nextSquare.entity.ChangeToSwipeFailedAnimationState(this, owner.selectedSquare, false);

		}

		public override void Finish()
		{
			base.Finish();
			owner.fsm.ChangeState(typeof(NormalState));
		}

		public void Discount()
		{
			if(--entityCount <= 0)
				Finish();
		}
	}
		
	public class CrashState : BaseFSM<GameManager>
	{
		private int crashCount;
//		private List<CrashInfo> crashList;

		public override void Begin()
		{
			base.Begin();

			//crashList = owner.crashList; owner.GetCrashList();
			crashCount = 0;//owner.crashList.Count;

			if(owner.crashList == null || owner.crashList.Count == 0)
				owner.fsm.ChangeState(typeof(NormalState));
			else
			{
				int length;
				BoardPos pos;
				BoardSquare bs;

				crashCount = owner.crashList.Count;
				for(int i = 0; i < owner.crashList.Count; i++)
				{
					length = owner.crashList[i].squares.Length;
					if(length == 3)
					{
						for(int j = 0; j < length; j++)
						{
							pos = owner.crashList[i].squares[j];
							owner.board[pos.row, pos.col].entity.ChangeToCrashAnimationState(this);									
						}
					}
					else
					{
						for(int j = 0; j < length; j++)
						{
							pos = owner.crashList[i].squares[j];
							bs = owner.board[pos.row, pos.col];
							bs.entity.SetParent(owner.board[owner.crashList[i].squares[0].row, owner.crashList[i].squares[0].col]);
							bs.entity.ChangeToGatheringAnimationState(this);	
						}
					}
				}
			}
		}

		public override void Finish()
		{
			base.Finish();

			int length;
			BoardPos pos;
			EntityData entityData = new EntityData();

			for(int i = 0; i < owner.crashList.Count; i++)
			{					
				length = owner.crashList[i].squares.Length;
				if(length > 3)
				{
					pos = owner.crashList[i].squares[0];
					entityData.Set(owner.board[pos.row, pos.col].entity.entityData);
				}

				// Delete all entities which were used for combining bomb.
				for(int j = 0; j < length; j++)
				{
					pos = owner.crashList[i].squares[j];
					PoolingManager.DestroyPooling(owner.board[pos.row, pos.col].entity);
					owner.board[pos.row, pos.col].entity = null;
				}

				if(length > 3)
				{
					if(owner.crashList[i].type == CombineType.HorizontalBomb)
					{
						entityData.basicBombType = BasicBombType.Horizontal;
						entityData.specialType = SpecialType.None;
					}
					else if(owner.crashList[i].type == CombineType.VerticalBomb)
					{
						entityData.basicBombType = BasicBombType.Vertical;
						entityData.specialType = SpecialType.None;
					}
					else if(owner.crashList[i].type == CombineType.BoxBomb)
					{
						entityData.basicBombType = BasicBombType.Box;
						entityData.specialType = SpecialType.None;
					}
					else if(owner.crashList[i].type == CombineType.Missile)
					{
						entityData.extraBombType = ExtraBombType.Missile;
						entityData.specialType = SpecialType.None;
					}
					else if(owner.crashList[i].type == CombineType.MultiBomb01)
					{
						entityData.basicBombType = BasicBombType.None;
						entityData.extraBombType = ExtraBombType.None;
						entityData.specialType = SpecialType.MultiBomb01;
					}
					else if(owner.crashList[i].type == CombineType.MultiBomb02)
					{
						entityData.basicBombType = BasicBombType.None;
						entityData.extraBombType = ExtraBombType.None;
						entityData.specialType = SpecialType.MultiBomb02;
					}

					entityData.ComposeId();

					Entity newEntity = PoolingManager.InstantiateByPooling(entityData.key);

					if(newEntity != null)
					{
						pos = owner.crashList[i].squares[0];
						owner.board[pos.row, pos.col].entity = newEntity;
						newEntity.SetParent(owner.board[pos.row, pos.col]);
						newEntity.transform.localPosition = Vector3.zero;
						newEntity.transform.localScale = Vector3.one;
					}
				}
			}

			owner.fsm.ChangeState(typeof(RearrangeState));
		}

		public void Discount()
		{
			if(--crashCount <= 0)
				Finish();
		}
	}

//	public class PlayerCrashState : BaseFSM<GameManager>
//	{
//		private int crashCount;
//		private List<CrashInfo> crashList;
//
//		public override void Begin()
//		{
//			base.Begin();
//		}
//
//		public override void Finish()
//		{
//			base.Finish();
//		}
//
//		protected override void FixedUpdateFunc()
//		{
//			
//		}
//	}
}
