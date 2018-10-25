using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//========================
// Square Menu types
// 0:Empty, 1:Erase(Blank,Block), 2:Generator, 3:Vending Machine, 4:Portal
// 5:Slide direction(Down,Left,Up,Right), 6:Lock(1,2,3), 7:Add Reverse

//========================
// Entity menu types
// 0:Erase, 1:Random Entity, 2:Entity(01~09), 3:Add Bomb(Box, Horizontal, Vertical, Missile), 
// 4:Restrict entity, 5:Random bomb(Box,Line,Missile,BoxLine,All), 6:Multi-bomb
public enum SquareMenu
{
	Empty			= 1000,
//	Erase			= 1100,
	EraseBlank		= 1101,
	EraseBlock		= 1102,
	Generator		= 1200,
	Vending			= 1300,
	Portal			= 1400,
//	Slide			= 1500,
	SlideDown		= 1501,
	SlideLeft		= 1502,
	SlideUp			= 1503,
	SlideRight		= 1504,
//	Lock			= 1600,
	Lock1			= 1601,
	Lock2			= 1602,
	Lock3			= 1603,
	Reverse			= 1700,
}

public enum EntityMenu
{
	Erase				= 5000,
	//Entity				= 5100,
	Entity01			= 5101,
	Entity02			= 5102,
	Entity03			= 5103,
	Entity04			= 5104,
	Entity05			= 5105,
	Entity06			= 5106,
	Entity07			= 5107,
	Entity08			= 5108,
	Entity09			= 5109,
	//Bomb				= 5200,
	BombBox				= 5201,
	BombHorizontal		= 5202,
	BombVertical		= 5203,
	BombMissile			= 5204,
	Restrict			= 5300,
	RandomEntity		= 5400,
	//RandomBox			= 5401,
	//RandomBomb			= 5500,
	RandomBombBox		= 5501,
	RandomBombLine		= 5502,
	RandomBombMissle	= 5503,
	RandomBombBoxLine	= 5504,
	RandomBombAll		= 5505,
	MultiBomb01			= 5601,
	MultiBomb02			= 5602,
}

public class EditorMenuManager : MonoBehaviour
{
	public GameObject elementRoot;
	public GameObject defaultSquare;
	public GameObject defaultEntity;
	public Sprite portalInSprite;
	public Sprite portalOutSprite;

	public GameManager gameManager;

	public EditorSquareButton [] squareButtons;
	public EditorEntityButton [] entityButtons;

	public static EditorSquareButton selectedSquareMenu;
	public static EditorEntityButton selectedEntityMenu;

	[HideInInspector]
	public int currentStage;

	[HideInInspector]
	public int currentLevel;

	public FSM<EditorMenuManager> fsm;

	//-------------------------------------------
	// Private members
	private Dictionary<SquareMenu, EditorSquareButton> squareMatchDic;	
	private Dictionary<EntityMenu, EditorEntityButton> entityMatchDic;				// The key is a menu id.

	private List<EditorBoardSquare> board	= new List<EditorBoardSquare>();
	private List<UInt16> usedEntityIndexes	= new List<UInt16>();
	private StageLevelInfo editingLevel		= new StageLevelInfo();

	private float startX;
	private float startY;

	private Sprite [] backSprites;
	private int playBoardRow;
	private int playBoardCol;
	private int tileWidth;
	private int tileHeight;

	private static EditorMenuManager _instance;
	public static EditorMenuManager instance
	{
		get { return _instance; }
	}

	void Awake()
	{
		if(_instance == null)
			_instance = this;
	}

	// Use this for initialization
	void Start ()
	{
		InitMenuManager();
		InitMatchTable();
		InitBoardSquares();
		InitPlayedInfo();

//		if(editingLevel.CopyFrom(GetCurrentLevel()) == false)
//			editingLevel.Initialize(GameDatabase.instance.playBoardRow, GameDatabase.instance.playBoardCol);

		InitUsedEntityList(editingLevel);

		fsm = new FSM<EditorMenuManager>(this);
		fsm.ChangeState( typeof(BoardSquareState) );

		gameManager.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
	{
		fsm.currentState.Update();
	}

	void FixedUpdate()
	{
		fsm.currentState.FixedUpdate();
	}

	void InitMenuManager()
	{
		if(elementRoot == null)
			elementRoot = this.gameObject;

		backSprites		= GameDatabase.instance.backSprites;
		playBoardRow	= GameDatabase.instance.playBoardRow;
		playBoardCol	= GameDatabase.instance.playBoardCol;
		tileWidth		= GameDatabase.instance.tileWidth;
		tileHeight		= GameDatabase.instance.tileHeight;

		if(editingLevel.CopyFrom(GetCurrentLevel()) == false)
			editingLevel.Initialize(playBoardRow, playBoardCol);
		
	}

	void InitMatchTable()
	{
		squareMatchDic = new Dictionary<SquareMenu, EditorSquareButton>();
		entityMatchDic = new Dictionary<EntityMenu, EditorEntityButton> ();

		if(squareButtons != null)
			for( int i = 0; i < squareButtons.Length; i++ )
				squareMatchDic.Add( squareButtons[i].menuId, squareButtons[i] );

		if(entityButtons != null)
			for( int i = 0; i < entityButtons.Length; i++ )
				entityMatchDic.Add( entityButtons[i].menuId, entityButtons[i] );

//		if(entityMenuObjects != null)
//			for( int i = 0; i < entityMenuObjects.Length; i++ )
//				menuMatchDic.Add( entityMenuObjects[i].menu, entityMenuObjects[i].entity );
	}

	void InitBoardSquares()
	{
		EditorBoardSquare square;

		startX = -((float)playBoardCol-1)*tileWidth / 2.0f;
		startY = (float)(playBoardRow-1)*tileHeight / 2.0f;

		for( int r = 0; r < playBoardRow; r++ )
		{			
			for( int c = 0; c < playBoardCol; c++ )
			{
				square = CreateSquareForEditing(r, c);
//				if(r == 0)
//				{
//					square.SetGenerator(GetSquareMenuSprite(SquareMenu.Generator));
//					EditorMenuManager.instance.SaveSquareToLevel(square.squareData);
//				}
				board.Add(square);
			}
		}
	}

	// Get number of the level which was played last by a player.
	void InitPlayedInfo()
	{
		// codes for getting a cleared stage number
		// ...
		//		clearedStage = 0;

		// codes for getting a cleared level number in the stage
		// ...
		//		clearedLevel = 0;

		// codes for getting a current stage number
		// ...
		currentStage = 0;

		// codes for getting a current level number
		currentLevel = 0;
	}

	public static Sprite GetSquareMenuSprite(SquareMenu menuId)
	{
		EditorSquareButton selectedMenu;

		if(_instance.squareMatchDic.TryGetValue(menuId, out selectedMenu))
		{
			return selectedMenu.sprite;
		}

		return null;
	}

	public static Sprite GetEntityMenuSprite(EntityMenu menuId)
	{
		EditorEntityButton selectedMenu;

		if(_instance.entityMatchDic.TryGetValue(menuId, out selectedMenu))	
		{
			SpriteRenderer sr = selectedMenu.GetComponent<SpriteRenderer>();
			if(sr != null)
				return sr.sprite;
		}	

		return null;
	}

	public StageInfo GetCurrentStage()
	{
		if( currentStage >= 0 && currentStage < GameDatabase.instance.StageLength )
			return GameDatabase.instance.GetStage( currentStage );

		return null;
	}

	public StageLevelInfo GetCurrentLevel()
	{
		StageInfo currentStg = GetCurrentStage();
		if( currentStg != null && currentLevel >= 0 && currentLevel < currentStg.LevelLength )
			return currentStg.GetLevel( currentLevel );

		return null;
	}

	public void ArrangeBoardSquareButtons(float sx, RectTransform parent)
	{		
		if( parent != null && squareButtons != null && squareButtons.Length > 0 )
		{
			int boardLength = squareButtons.Length;
			GameObject squareObj;
			RectTransform rectTrans;

			for( int i = 0; i < boardLength; i++ )
			{
				if(squareButtons[i].gameObject != null)
				{
					squareObj = Instantiate(squareButtons[i].gameObject);
					rectTrans = squareObj.GetComponent<RectTransform>();
					rectTrans.SetParent( parent );
					rectTrans.localPosition = new Vector3( i * rectTrans.rect.width + sx, 0, 0 );
					rectTrans.localScale = Vector3.one;
				}
			}
		}
	}

	public void ArrangeEntityButtons( float sx, RectTransform parent)
	{		
		if( parent != null && entityButtons != null && entityButtons.Length > 0 )
		{
			int entityLength = entityButtons.Length;
			GameObject entityObj;
			RectTransform rectTrans;

			for( int i = 0; i < entityLength; i++ )
			{
				if(entityButtons[i].gameObject != null)
				{
					entityObj = Instantiate(entityButtons[i].gameObject);
					rectTrans = entityObj.GetComponent<RectTransform>();
					rectTrans.SetParent( parent );
					rectTrans.localPosition = new Vector3( i * rectTrans.rect.width + sx, 0, 0 );
					rectTrans.localScale = Vector3.one;
				}
			}
		}
	}

	public EditorBoardSquare CreateSquareForEditing(int r, int c)
	{
		GameObject squareObj;
		EditorBoardSquare square;

		squareObj = Instantiate(defaultSquare);

		squareObj.name = "bd_"+r+"_"+c;
		squareObj.transform.SetParent(elementRoot.transform);
		squareObj.transform.localPosition = new Vector3( startX+c*tileWidth, startY-r*tileHeight, 0 );
		squareObj.transform.localScale = Vector3.one;

		square = squareObj.GetComponent<EditorBoardSquare>();
//		if(r == 0 || r == playBoardRow-1)
//		{
//			square.squareData.system = true;
//			square.SetTileSprite(backSprites[2]);
//		}
//		else
		{
//			square.squareData.system = false;
			square.SetTileSprite(backSprites[(c+r%2) % 2]);
		}
		square.squareData.row = (Int16)r;
		square.squareData.col = (Int16)c;
		square.squareData.ComposeId();

		return square;
	}

	public EditorEntity CreateEntityForEditing(string name, Transform parent)
	{
		GameObject entityObj;
		EditorEntity ent;

		entityObj = Instantiate(defaultEntity);
		entityObj.name = name;
		entityObj.transform.SetParent(parent);
		entityObj.transform.localPosition = Vector3.zero;
		entityObj.transform.localScale = Vector3.one;
		ent = entityObj.GetComponent<EditorEntity>();
		ent.entityData.ComposeId();

		return ent;
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

	void ClearPlayBoard()
	{

	}

	void RestoreElementsForEditing()
	{
		int index;

		int spriteLen = backSprites.Length;
		int rowSize = 0;

		for(int r = 0; r < playBoardRow; r++)
		{
			for(int c = 0; c < playBoardCol; c++)
			{
				index = rowSize+c;

				board[index].gameObject.SetActive(true);

				if(board[index].editingEntity != null && board[index].editingEntity.gameObject != null)
					board[index].editingEntity.gameObject.SetActive(true);				
			}
			rowSize += playBoardCol;
		}
	}

	public void AssignEntitiesRandomly()
	{
		if(editingLevel != null && usedEntityIndexes != null)
		{
			EditorEntity ent;
			UInt16 typeNum;

			for( int i = 0; i < board.Count; i++ )
			{
				if( board[i].squareData.disused == false && board[i].squareData.squareType != SquareType.SpreadMachine && board[i].editingEntity == null)
				{
					typeNum = usedEntityIndexes[UnityEngine.Random.Range( 0, usedEntityIndexes.Count)];
					ent = EditorMenuManager.instance.CreateEntityForEditing("ent_"+board[i].squareData.row+"_"+board[i].squareData.col, board[i].gameObject.transform);
					ent.entityData.typeNumber = typeNum;
					ent.entityData.ComposeId();

					ent.SetEntitySprite(PoolingManager.GetEntitySprite(ent.entityData.key));
					editingLevel.SetEntity(board[i].squareData.row, board[i].squareData.col, ent.entityData.id);
					board[i].editingEntity = ent;
				}
			}
		}
	}

	public void ClearAllSquares()
	{
//		int index;

		for( int i = 0; i < board.Count; i++ )
		{
			if(board[i].squareData.disused == false)
			{
				board[i].Init();
				editingLevel.SetSquare(board[i].squareData);
			}
		}
	}

	public void RemoveAllEntities()
	{
		for( int i = 0; i < board.Count; i++ )
		{
			if(board[i].editingEntity != null)
			{
				Destroy(board[i].editingEntity.gameObject);
				board[i].editingEntity = null;
			}
			board[i].Init();
			editingLevel.SetEntity(board[i].squareData.row, board[i].squareData.col, 0);
		}
	}

	public void InitializeBoard()
	{
		RemoveAllEntities();

		for( int i = 0; i < board.Count; i++ )
		{
			board[i].gameObject.SetActive( true );
			board[i].ShowTile(true);
			board[i].Init();
			editingLevel.SetSquare(board[i].squareData);
		}
	}

	public EditorBoardSquare SelectSquare( Vector3 pos )
	{
		int x = (int)(pos.x - board[0].transform.position.x + (float)tileWidth/2.0f);
		int y = tileHeight*playBoardRow - (int)(board[0].transform.position.y + pos.y + (float)tileHeight/2.0f);

		if( x > 0 && y > 0)
		{
			int c = x / tileWidth;
			int r = y / tileHeight;

			if( c < playBoardCol && r < playBoardRow )
			{
				return board[ r*playBoardCol + c ];
			}
		}

		return null;
	}

	public EditorBoardSquare SelectActiveSquare( Vector3 pos )
	{
		int x = (int)(pos.x - board[0].transform.position.x + (float)tileWidth/2.0f);
		int y = tileHeight*playBoardRow - (int)(board[0].transform.position.y + pos.y + (float)tileHeight/2.0f);

		if( x > 0 && y > 0)
		{
			int c = x / tileWidth;
			int r = y / tileHeight;

			if( c < playBoardCol && r < playBoardRow && board[ r*playBoardCol + c ].gameObject.activeSelf )
			{
				return board[ r*playBoardCol + c ];
			}
		}

		return null;
	}

	public void StartTestGame()
	{
		ChangeToPauseState();
		elementRoot.gameObject.SetActive(false);
		gameManager.gameObject.SetActive(true);
		StartCoroutine(RunGame());
//		GameManager.instance.RunGame(editingLevel);
	}

	IEnumerator RunGame()
	{
		yield return new WaitForEndOfFrame();
		StageManager.instance.CalibrateCamera(false);
		GameManager.instance.RunGame(editingLevel);
	}

	public void StopTestGame()
	{
		StageManager.instance.CalibrateCamera(true);

		GameManager.DestroyGame();
		gameManager.gameObject.SetActive(false);
		elementRoot.gameObject.SetActive(true);

		RestoreElementsForEditing();
		RestoreState();
	}

	//------------------------------------------------------------------------------
	public void ChangeState(Type tp)
	{
		fsm.ChangeState(tp);
	}

	public void ChangeToPauseState()
	{
		fsm.ChangeState(typeof(PauseState));
	}

	public void RestoreState()
	{
		fsm.RestoreState();
	}

	//----------------------------------------------------------------------------------
	// FSM Classes
	public class PauseState : BaseFSM<EditorMenuManager>
	{
	}

	public class BoardSquareState : BaseFSM<EditorMenuManager>
	{
		FSM<BoardSquareState> fsm;

		public BoardSquareState()
		{
			fsm = new FSM<BoardSquareState>(this);
			SetNormalState();
		}

		public void SetNormalState()
		{
			fsm.ChangeState( typeof(NormalState) );
		}

		public void SetPortalState()
		{
			fsm.ChangeState( typeof(PortalState) );
		}

		// Update is called once per frame
		protected override void UpdateFunc() 
		{
			fsm.currentState.Update();
		}

//		protected override void FixedUpdate()
//		{
//			fsm.currentState.FixedUpdate();
//		}

		public class NormalState : BaseFSM<BoardSquareState>
		{
			EditorBoardSquare bs = null, prevBs = null;
			EditorSquareButton menuButton;
			Vector2 pos, prevPos;
			bool isSelected = false;

//			protected override void Begin()
//			{
//				pos = Vector3.zero;
//			}

			protected override void UpdateFunc()
			{
//				if(Input.GetMouseButtonDown(0))
//				{
//				}
//				else
				if(Input.GetMouseButton(0))
				{
					prevPos = pos;
					pos = Camera.main.ScreenPointToRay(Input.mousePosition).origin;

					prevBs = bs;
					bs = EditorMenuManager.instance.SelectSquare(pos);
					menuButton = EditorMenuManager.selectedSquareMenu;

					if(bs != null && menuButton != null)
					{
						switch(menuButton.menuId)
						{
						case SquareMenu.Empty:
							bs.SetEmpty();
							bs.ShowTile(true);
							break;
						case SquareMenu.EraseBlank:
							bs.SetEraseBlank(menuButton.sprite);
							break;
						case SquareMenu.EraseBlock:
							bs.SetEraseBlock(menuButton.sprite);
							break;
						case SquareMenu.Generator:
							bs.SetGenerator(menuButton.sprite);
							break;
						case SquareMenu.Vending:
							bs.SetVending(menuButton.sprite);
							break;
						case SquareMenu.SlideDown:
							bs.SetSlideDown(menuButton.sprite);
							break;
						case SquareMenu.SlideLeft:
							bs.SetSlideLeft(menuButton.sprite);
							break;
						case SquareMenu.SlideUp:
							bs.SetSlideUp(menuButton.sprite);
							break;
						case SquareMenu.SlideRight:
							bs.SetSlideRight(menuButton.sprite);
							break;
						case SquareMenu.Lock1:
							bs.SetLock1(menuButton.sprite);
							break;
						case SquareMenu.Lock2:
							bs.SetLock2(menuButton.sprite);
							break;
						case SquareMenu.Lock3:
							bs.SetLock3(menuButton.sprite);
							break;
						case SquareMenu.Reverse:
							SetReverse(menuButton.sprite);
							break;
						}

						EditorMenuManager.instance.SaveSquareToLevel(bs.squareData);
					}
				}
				else if(Input.GetMouseButtonUp(0))
				{
					isSelected = false;	
				}
			}

			void SetReverse(Sprite sp)
			{
				int rpos;
				if(EditorMenuManager.instance.editingLevel != null)
				{					
					if(bs != prevBs || isSelected == false) 
					{
						isSelected = true;	
						if(bs.squareData.reversal == false)
						{
							for(int r = bs.squareData.row; r < EditorMenuManager.instance.editingLevel.reverseBoundary; r++)
							{
								rpos = r * EditorMenuManager.instance.playBoardCol;
								for(int c = 0; c < EditorMenuManager.instance.playBoardCol; c++)
								{
									EditorMenuManager.instance.board[rpos+c].SetReverse(sp);
									//EditorMenuManager.instance.editingLevel.SetSquare(EditorMenuManager.instance.board[rpos+c].squareData);
								}
							}

							EditorMenuManager.instance.editingLevel.reverseBoundary = bs.squareData.row;
						}
						else
						{
							for(int r = EditorMenuManager.instance.editingLevel.reverseBoundary; r <= bs.squareData.row; r++)
							{
								rpos = r * EditorMenuManager.instance.playBoardCol;
								for(int c = 0; c < EditorMenuManager.instance.playBoardCol; c++)
								{
									EditorMenuManager.instance.board[rpos+c].RemoveReverse();
									//EditorMenuManager.instance.editingLevel.SetSquare(EditorMenuManager.instance.board[rpos+c].squareData);
								}
							}

							EditorMenuManager.instance.editingLevel.reverseBoundary = bs.squareData.row+1;

						}
					}
					else if(isSelected == false)
					{
						isSelected = true;
						rpos = bs.squareData.row * EditorMenuManager.instance.playBoardCol;
						if(bs.squareData.reversal == false)
							for(int c = 0; c < EditorMenuManager.instance.playBoardCol; c++)
								EditorMenuManager.instance.board[rpos+c].SetReverse(sp);
						else
							for(int c = 0; c < EditorMenuManager.instance.playBoardCol; c++)
								EditorMenuManager.instance.board[rpos+c].RemoveReverse();
					}

					if(bs != prevBs)
						isSelected = false;
				}
			}
		}

		public class PortalState : BaseFSM<BoardSquareState>
		{
			EditorBoardSquare bs;
			EditorBoardSquare bsPrev;
			EditorBoardSquare beginBS;
			EditorSquareButton squareMenu;

			protected override void UpdateFunc()
			{
				if(Input.GetMouseButtonDown(0))
				{
					beginBS = EditorMenuManager.instance.SelectSquare(Camera.main.ScreenPointToRay(Input.mousePosition).origin);
					squareMenu = EditorMenuManager.selectedSquareMenu;
					if(beginBS != null && squareMenu != null)
					{
						if(squareMenu.menuId == SquareMenu.Portal)
						{
							if(!beginBS.squareData.portalIn && !beginBS.squareData.portalOut)
							{
								beginBS.SetPortalIn(owner.owner.portalInSprite);
								bsPrev = beginBS;
							}
							else
								beginBS = null;
						}
					}
				}
				else if(Input.GetMouseButton(0))
				{
					bs = EditorMenuManager.instance.SelectSquare(Camera.main.ScreenPointToRay(Input.mousePosition).origin);
					squareMenu = EditorMenuManager.selectedSquareMenu;
					if(bs != null && squareMenu != null)
					{
						if(squareMenu.menuId == SquareMenu.Portal)
						{
							if(beginBS != null && bs != beginBS && bs != bsPrev && !bs.squareData.portalIn && !bs.squareData.portalOut)
							{
								bsPrev.RemovePortalOut();
								bs.SetPortalOut(owner.owner.portalOutSprite);
								bsPrev = bs;
							}
						}
					}

				}
				else if(Input.GetMouseButtonUp(0))
				{
					EditorMenuManager.instance.SaveSquareToLevel(beginBS.squareData);
					EditorMenuManager.instance.SaveSquareToLevel(bs.squareData);
					beginBS = null;
				}
			
			}
		}
	}

	public void SaveSquareToLevel(BoardSquareData bs)
	{
		if(editingLevel != null)
			editingLevel.SetSquare(bs);
	}

	public void SaveEntityToLevel(int r, int c, UInt16 entityId)
	{
		if(editingLevel != null)
			editingLevel.SetEntity(r, c, entityId);
	}

	public class EntityState : BaseFSM<EditorMenuManager>
	{
		EditorBoardSquare bs;
		EditorEntity ent;
		EditorEntityButton menuButton;

		protected override void UpdateFunc()
		{
			if( Input.GetMouseButtonDown(0) )
			{
			}
			else if( Input.GetMouseButton(0) )
			{
				bs = EditorMenuManager.instance.SelectActiveSquare( Camera.main.ScreenPointToRay( Input.mousePosition ).origin );
				menuButton = EditorMenuManager.selectedEntityMenu;

				if(bs != null && menuButton != null && bs.squareData.disused == false)// && bs.squareData.system == false )
				{	
					if(bs.editingEntity == null)
					{
						ent = EditorMenuManager.instance.CreateEntityForEditing("ent_"+bs.squareData.row+"_"+bs.squareData.col, bs.gameObject.transform);
						bs.editingEntity = ent;
					}

					ent = bs.editingEntity;

					switch(menuButton.menuId)
					{
					case EntityMenu.Erase:
						bs.EraseEntity();
						break;
					case EntityMenu.Entity01:
					case EntityMenu.Entity02:
					case EntityMenu.Entity03:
					case EntityMenu.Entity04:
					case EntityMenu.Entity05:
					case EntityMenu.Entity06:
					case EntityMenu.Entity07:
					case EntityMenu.Entity08:
					case EntityMenu.Entity09:
						ent.SetDefaultEntity((UInt16)(menuButton.menuId-EntityMenu.Entity01+1), menuButton.sprite);
						break;
					case EntityMenu.BombBox:
						ent.SetBombBox();
						break;
					case EntityMenu.BombHorizontal:
						ent.SetBombHorizontal();
						break;
					case EntityMenu.BombVertical:
						ent.SetBombVertical();
						break;
					case EntityMenu.BombMissile:
						ent.SetBombMissile();
						break;
					case EntityMenu.Restrict:
						ent.SetLock(menuButton.sprite);
						break;
					case EntityMenu.RandomEntity:
						ent.SetRandomEntity(menuButton.sprite);
						break;
//					case EntityMenu.RandomBox:
//						bs.SetRandomBox(entityMenu.sprite);
//						break;
					case EntityMenu.RandomBombBox:
						ent.SetRandomBombBox(menuButton.sprite);
						break;
					case EntityMenu.RandomBombLine:
						ent.SetRandomBombLine(menuButton.sprite);
						break;
					case EntityMenu.RandomBombMissle:
						ent.SetRandomBombMissle(menuButton.sprite);
						break;
					case EntityMenu.RandomBombBoxLine:
						ent.SetRandomBombBoxLine(menuButton.sprite);
						break;
					case EntityMenu.RandomBombAll:
						ent.SetRandomBombAll(menuButton.sprite);
						break;
					case EntityMenu.MultiBomb01:
						ent.SetMultiBomb01(menuButton.sprite);
						break;
					case EntityMenu.MultiBomb02:
						ent.SetMultiBomb02(menuButton.sprite);
						break;
					}

					ent.entityData.ComposeId();
					EditorMenuManager.instance.SaveEntityToLevel(bs.squareData.row, bs.squareData.col, ent.entityData.id);
				}
			}
			else if( Input.GetMouseButtonUp(0) )
			{
			}
		}
	}

//	public class RemoveEntityState : BaseFSM<EditorMenuManager>
//	{
//		protected override void UpdateFunc()
//		{
//			if( Input.GetMouseButtonDown(0) )
//			{
//			}
//			else if( Input.GetMouseButton(0) )
//			{
//				EditorBoardSquare bs = StageManager.instance.SelectActiveSquare( Camera.main.ScreenPointToRay( Input.mousePosition ).origin );
//				bs.RemoveEntity();
////				StageManager.instance.RemoveEntityInSquare (bs);//.AssignEntityInSquare( bs, EditorMenuManager.selectedEntityIcon );
//			}
//			else if( Input.GetMouseButtonUp(0) )
//			{
//			}
//		}
//	}

	public class SpecialEntityState : BaseFSM<EditorMenuManager>
	{
		public override void Begin()
		{
			base.Begin();
		}
		public override void Finish()
		{
			base.Finish();
		}
		protected override void UpdateFunc()
		{
			if( Input.GetMouseButtonDown(0) )
			{
			}
			else if( Input.GetMouseButton(0) )
			{
				//				EditorBoardSquare bs = StageManager.instance.SelectActiveSquare( Camera.main.ScreenPointToRay( Input.mousePosition ).origin );
				//				StageManager.instance.AssignEntityInSquare( bs, EditorMenuManager.selectedEntityIcon );
			}
			else if( Input.GetMouseButtonUp(0) )
			{
			}
		}
	}

	public class CriteriaState : BaseFSM<EditorMenuManager>
	{
		public override void Begin()
		{
			base.Begin();
		}
		public override void Finish()
		{
			base.Finish();
		}
		protected override void UpdateFunc()
		{
			if( Input.GetMouseButtonDown(0) )
			{
			}
			else if( Input.GetMouseButton(0) )
			{
//				EditorBoardSquare bs = StageManager.instance.SelectActiveSquare( Camera.main.ScreenPointToRay( Input.mousePosition ).origin );
//				StageManager.instance.AssignEntityInSquare( bs, EditorMenuManager.selectedEntityIcon );
			}
			else if( Input.GetMouseButtonUp(0) )
			{
			}
		}
	}
}

