using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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

[Serializable]
public class BoardSquare : MonoBehaviour
{
	public BoardSquareData squareData;
	public BoardTile tile;
	public GameObject reversalObj;

	public Entity entity;
	public string currentStateName;
//	public static int entityCount;			// Entity counter for surveying entities which are gathering and crashing.

	[HideInInspector]
	public int dirX, dirY;
	public BoardSquare next;			// Next board square which it would move to.
//										// if this value would be null, the next position is decided by other parameters like gravity, direction etc.
	[HideInInspector]
	public GameObject entityMask;
//	[HideInInspector]
	public float cullingY;

//	public CrashInfo crashInfo;
	private FSM<BoardSquare> fsm;
	private float currentSpeed;			// Current speed of moving entity.
	private List<Entity> entityList;

	public bool IsGenerator
	{
		get { return (squareData.squareType == SquareType.Generator || squareData.squareType == SquareType.VendingMachine); }
	}

	void Awake()
	{
		fsm = new FSM<BoardSquare>(this);
		fsm.ChangeState( typeof(NormalState) );
	}

	void Start ()
	{
		CalculateDirection();
	}

//	void Update ()
//	{
//		currentStateName = fsm.currentState.ToString();
//		currentStateOfFixedUpdate = fsm.currentState.ToString();
//	}

	public void UserFixedUpdate ()
	{
		fsm.currentState.FixedUpdate();
		currentStateName = fsm.currentState.ToString();
	}

	public void Clear()
	{
		squareData.Init();
		entity = null;

		squareData.ComposeId();
		CalculateDirection();
	}

	public void DestroyEntity()
	{
		if(entity != null)
		{
			PoolingManager.DestroyPooling(entity);
			entity = null;
		}
	}

	public void SetTileSprite(Sprite tileSprite)
	{
		if(tile != null)
			tile.SetSprite(tileSprite);	
	}

	public void Set(BoardSquare bs)
	{
		if( bs != null )
		{
			squareData	= bs.squareData; 
			squareData.ComposeId();

			CalculateDirection();
		}
	}

	public void Set( UInt16 squareBits, int nextSquare=-1 )
	{
		squareData.Set(squareBits);
//		next = StageManager.instance.GetSquare( nextIndex );
	}

	public void ShowSprites()
	{
		tile.gameObject.SetActive(true);
		reversalObj.SetActive(true);
	}

	public void HideSprites()
	{
		tile.gameObject.SetActive(false);
		reversalObj.SetActive(false);
	}

	public void ShowReversalSprite(bool show)
	{
		if(show)
			reversalObj.SetActive(true);
		else
			reversalObj.SetActive(false);
	}

	public void StartFiniteState()
	{
//		if(squareData.squareType == SquareType.Normal)
//			fsm.ChangeState( typeof(NormalState) );
//		else
		if(squareData.squareType == SquareType.Generator)
			fsm.ChangeState( typeof(GeneratorState) );
		else if(squareData.squareType == SquareType.VendingMachine)
			fsm.ChangeState( typeof(VendingMahcineState) );
		else if(squareData.squareType == SquareType.SpreadMachine)
			fsm.ChangeState( typeof(SpreadMahcineState) );
	}

	void CalculateDirection()
	{
		if(squareData.reversal || squareData.slideDir == MoveDir.Up)
		{
			dirX = 0;
			dirY = -1;
		}
		else if(!squareData.reversal || squareData.slideDir == MoveDir.Down)
		{
			dirX = 0;
			dirY = 1;
		}

		if(squareData.slideDir == MoveDir.Left)
		{
			dirX = -1;
			dirY = 0;
		}
		else if(squareData.slideDir == MoveDir.Right)
		{
			dirX = 1;
			dirY = 0;
		}
	}

	public void ReplaceEntity()
	{
		if(entityList != null && entityList.Count > 0)
		{
			entity = entityList[entityList.Count-1];
//			entity.gameObject.SetActive(true);
			entityList.Remove(entity);
			entity.SetParent(this);
		}
	}

	public void ChangeToMovingState(float prevSpeed=0)
	{
		currentSpeed = prevSpeed;
		fsm.ChangeState(typeof(MovingState));
	}

	public void ChangeToGeneratorState()
	{
		currentSpeed = 0;
		fsm.ChangeState(typeof(GeneratorState));
	}

	bool IsMoving()
	{
		return GameManager.instance.IsEntityMoving(squareData.row, squareData.col);
	}

	public void InitToMoveEntity()
	{
		currentSpeed = 0;
		next = null;

		if(!squareData.disused && entity != null)
		{
			SetupNext();

			if(squareData.squareType == SquareType.Generator)
			{
//				ReplaceEntity();
				ChangeToGeneratorState();
			}
			else if(next != null && squareData.squareType == SquareType.Normal && next.entity == null)
			{
				next.entity = entity;
				next.entity.SetParent(next);
				next.ChangeToMovingState(currentSpeed);
				entity = null;
			}
		}
	}

	public void InitCullingY()
	{
		if(squareData.squareType == SquareType.Generator || squareData.squareType == SquareType.VendingMachine)
		{
			Vector3 vec = Camera.main.WorldToScreenPoint(transform.position);
			int h = UnityEngine.Display.main.renderingHeight;
			int w = UnityEngine.Display.main.renderingWidth;
			int sh = UnityEngine.Display.main.systemHeight;
			int sw = UnityEngine.Display.main.systemWidth;

			cullingY = vec.y + w / (GameDatabase.instance.playBoardCol+1) * 0.5f;
		}
		else
			cullingY = UnityEngine.Display.main.renderingHeight;
	}

	public BoardSquare SetupNext()
	{
		BoardSquare blockSq, nextSq, sideNextSq;
		int r = squareData.row + dirY;
		int c = squareData.col + dirX;

		next = null;

		if((!squareData.reversal && r < GameManager.instance.reverseBoundary
			|| squareData.reversal && r >= GameManager.instance.reverseBoundary)
			&& c >= 0 && c < GameDatabase.instance.playBoardCol)
		{
			nextSq = GameManager.instance.board[r, c];
			if(!nextSq.squareData.disused && nextSq.entity == null)
				next = nextSq;
			else
			{
				int sideCol = squareData.col-1;

				sideCol = squareData.col;
				for(int i = 0; i < 2; i++)
				{
					if(0 <= sideCol && sideCol < GameDatabase.instance.playBoardCol)
					{
						blockSq = GameManager.instance.board[squareData.row, sideCol];
						sideNextSq = GameManager.instance.board[r, sideCol];
						if(!sideNextSq.squareData.disused && sideNextSq.entity == null && blockSq.squareData.disused)
						{
							next = sideNextSq;
							break;
						}
					}

					sideCol = squareData.col+1;
				}

//				if(sideCol >= 0)
//				{
//					blockSq = GameManager.instance.board[squareData.row, sideCol];
//					sideNextSq = GameManager.instance.board[r, sideCol];
//					if(!sideNextSq.squareData.disused && sideNextSq.entity == null && blockSq.squareData.disused)
//						next = sideNextSq;
//				}
//
//				sideCol = squareData.col+1;
//
//				if(next == null && sideCol < GameDatabase.instance.playBoardCol)
//				{
//					blockSq = GameManager.instance.board[squareData.row, sideCol];
//					sideNextSq = GameManager.instance.board[r, sideCol];
//					if(!sideNextSq.squareData.disused && sideNextSq.entity == null && blockSq.squareData.disused)
//						next = sideNextSq;
//				}
			}
		}

		return next;
	}

//	public BoardSquare SetupNextReversed()
//	{
//		BoardSquare blockSq, nextSq, sideNextSq;
//		int r = squareData.row + dirY;
//		int c = squareData.col + dirX;
//
//		next = null;
//
//		if(r >= GameManager.instance.reverseBoundary && r < GameDatabase.instance.playBoardRow && c >= 0 && c < GameDatabase.instance.playBoardCol)
//		{
//			nextSq = GameManager.instance.board[r, c];
//			if(!nextSq.squareData.disused && nextSq.entity == null)
//				next = nextSq;
//			else
//			{
//				int sideCol = squareData.col+1;
//
//				if(sideCol >= 0)
//				{
//					blockSq = GameManager.instance.board[squareData.row, sideCol];
//					sideNextSq = GameManager.instance.board[r, sideCol];
//					if(!sideNextSq.squareData.disused && sideNextSq.entity == null && blockSq.squareData.disused)
//						next = sideNextSq;
//				}
//
//				sideCol = squareData.col-1;
//
//				if(next == null && sideCol < GameDatabase.instance.playBoardCol)
//				{
//					blockSq = GameManager.instance.board[squareData.row, sideCol];
//					sideNextSq = GameManager.instance.board[r, sideCol];
//					if(!sideNextSq.squareData.disused && sideNextSq.entity == null && blockSq.squareData.disused)
//						next = sideNextSq;
//				}
//			}
//		}
//
//		return next;
//	}

	void InitEntityList()
	{
		if(squareData.squareType == SquareType.Generator || squareData.squareType == SquareType.VendingMachine)
		{
			if(entityList == null)
				entityList = new List<Entity>();

			// Add entities in buffer if it is insufficient.
			if(entityList.Count < GameDatabase.instance.entityBufferSizeInGenerator)
			{
				int cnt = GameDatabase.instance.entityBufferSizeInGenerator - entityList.Count;
				for(int i = 0; i < cnt; i++)
					AddNewEntity();
			}

			// If an entity is empty, it brings a new entity from an entity list.
			if(entity == null)
				ReplaceEntity();
		}
	}

	void AddNewEntity()
	{
		Vector3 pos;
		UInt16 entKey = GameManager.instance.GetEntityKeyRandomly();
		Entity ent = PoolingManager.InstantiateByPooling(entKey);

		if(ent != null)
		{
			ent.SetParent(this);
			ent.transform.localScale = Vector3.one;

			//int cnt = owner.entityList.Count + 1;
			if(squareData.reversal == true)
				pos = new Vector3(0, -GameDatabase.instance.tileHeight, 0);
			else
				pos = new Vector3(0, GameDatabase.instance.tileHeight, 0);

			ent.transform.localPosition = pos;
			ent.gameObject.SetActive(false);
			entityList.Add(ent);
		}
	}

	//----------------------------------------------------------------------------------
	// FSM Classes
//	public class PauseState : BaseFSM<BoardSquare>
//	{
//	}

	public class NormalState : BaseFSM<BoardSquare>
	{
//		BoardSquare nextSq;
		public override void Begin()
		{
			base.Begin();
//			GameManager.instance.RemoveMovingFlag(owner.squareData.row, owner.squareData.col);
//			entityCount = 0;
		}

		public override void Finish()
		{
			base.Finish();
			owner.fsm.RestoreState();
		}

		protected override void FixedUpdateFunc()
		{
			if(owner.entity != null && owner.next != null && owner.next.entity == null && !owner.next.IsMoving())
			{
				owner.ChangeToMovingState();
				Finish();
			}
		}
	}

	public class MovingState : BaseFSM<BoardSquare>
	{
		Vector3 pos;
		float signX, signY;
		float v;

		public override void Begin()
		{
			base.Begin();

			if(owner.entity != null)
			{
				pos = owner.entity.transform.localPosition;

				if(pos.x < 0)
					signX = 1.0f;
				else if(pos.x > 0)
					signX = -1.0f;

				if(pos.y < 0)
					signY = 1.0f;
				else if(pos.y > 0)
					signY = -1.0f;

				GameManager.instance.SetMovingFlag(owner.squareData.row, owner.squareData.col);

				v = owner.currentSpeed;
			}
			else
				Finish();
		}

		public override void Finish()
		{
			base.Finish();
			GameManager.instance.RemoveMovingFlag(owner.squareData.row, owner.squareData.col);
			owner.fsm.RestoreState();
		}

		protected override void FixedUpdateFunc()
		{
			if(owner.entity != null )
			{
				pos = owner.entity.transform.localPosition;
				v += GameManager.instance.moveSpeed * Time.deltaTime; 
				if(v > GameManager.instance.maxMoveSpeed)
					v = GameManager.instance.maxMoveSpeed;
				
				pos.x += signX * v;
				pos.y += signY * v;

				if(signX > 0 && -Mathf.Epsilon < pos.x || signX < 0 && pos.x < Mathf.Epsilon)
					pos.x = 0;
				if(signY > 0 && -Mathf.Epsilon < pos.y || signY < 0 && pos.y < Mathf.Epsilon)
					pos.y = 0;

				if(pos != Vector3.zero)
					owner.entity.transform.localPosition = pos;
				else
				{
					if(owner.next != null && owner.next.entity == null)
					{
						owner.next.entity = owner.entity;
						owner.next.entity.SetParent(owner.next);
						owner.next.ChangeToMovingState(v);
						owner.entity = null;
					}
					else
						owner.entity.transform.localPosition = pos;

					Finish();
				}
			}
			else
				Finish();
		}
	}

	public class GeneratorState : BaseFSM<BoardSquare>
	{
		Vector3 pos;
		float signX, signY;
		float v;

		public override void Begin()
		{
			base.Begin();

			owner.InitEntityList();

			if(owner.entity != null)
			{
				pos = owner.entity.transform.localPosition;

				if(pos.x < 0)
					signX = 1.0f;
				else if(pos.x > 0)
					signX = -1.0f;

				if(pos.y < 0)
					signY = 1.0f;
				else if(pos.y > 0)
					signY = -1.0f;

				GameManager.instance.SetMovingFlag(owner.squareData.row, owner.squareData.col);

				v = 0;
			}
		}

		public override void Finish()
		{
			base.Finish();
		}

		protected override void FixedUpdateFunc()
		{
			if(owner.entityList.Count < GameDatabase.instance.entityBufferSizeInGenerator)
			{
				int cnt = GameDatabase.instance.entityBufferSizeInGenerator - owner.entityList.Count;
				for(int i = 0; i < cnt; i++)
					owner.AddNewEntity();
			}

			if(owner.entity != null)
			{
				SpriteRenderer sr = owner.entity.GetComponent<SpriteRenderer>();
				sr.material.SetFloat("_Trim", owner.cullingY);

				pos = owner.entity.transform.localPosition;
				v += GameManager.instance.moveSpeed * Time.deltaTime; 
				pos.x += signX * v;
				pos.y += signY * v;

				if(signY > 0 && -GameDatabase.instance.tileHeight < pos.y || signY < 0 && GameDatabase.instance.tileHeight > pos.y)
					owner.entity.gameObject.SetActive(true);
				
				if(signX > 0 && -Mathf.Epsilon < pos.x || signX < 0 && pos.x < Mathf.Epsilon)
					pos.x = 0;
				if(signY > 0 && -Mathf.Epsilon < pos.y || signY < 0 && pos.y < Mathf.Epsilon)
					pos.y = 0;

				if(pos != Vector3.zero)
					owner.entity.transform.localPosition = pos;
				else
				{
					if(owner.next != null && owner.next.entity == null)
					{
						owner.next.entity = owner.entity;
						owner.next.entity.SetParent(owner.next);
						owner.next.ChangeToMovingState(v);
						owner.ReplaceEntity();
						owner.entityMask.SetActive(true);


						//********************************  Very Important code  ********************************
						// A position of current entity must be corrected by the position of entity in next square.
						//
						owner.entity.transform.localPosition = owner.next.entity.transform.localPosition;
						//***************************************************************************************
					}
					else
					{
						owner.entity.transform.localPosition = pos;
						GameManager.instance.RemoveMovingFlag(owner.squareData.row, owner.squareData.col);
						owner.entityMask.SetActive(false);
						v = 0;
					}
				}
			}
			else
			{
				owner.ReplaceEntity();
				GameManager.instance.SetMovingFlag(owner.squareData.row, owner.squareData.col);
				owner.entityMask.SetActive(true);
				v = 0;
			}
		}

//		public void AddNewEntity()
//		{
//			Vector3 pos;
//			UInt16 entKey = GameManager.instance.GetEntityKeyRandomly();
//			Entity ent = PoolingManager.InstantiateByPooling(entKey);
//
//			if(ent != null)
//			{
//				ent.SetParent(owner);
//				ent.transform.localScale = Vector3.one;
//
//				//int cnt = owner.entityList.Count + 1;
//				if(owner.squareData.reversal == true)
//					pos = new Vector3(0, -GameDatabase.instance.tileHeight, 0);
//				else
//					pos = new Vector3(0, GameDatabase.instance.tileHeight, 0);
//
//				ent.transform.localPosition = pos;
//				ent.gameObject.SetActive(false);
//				owner.entityList.Add(ent);
//			}
//		}
	}

	public class VendingMahcineState : BaseFSM<BoardSquare>
	{
		Entity ent;
		public override void Begin()
		{
			base.Begin();
		}
		public override void Finish()
		{
			base.Finish();
		}
		protected override void FixedUpdateFunc()
		{
		}
	}

	public class SpreadMahcineState : BaseFSM<BoardSquare>
	{
		Entity ent;
		public override void Begin()
		{
			base.Begin();
		}
		public override void Finish()
		{
			base.Finish();
		}
		protected override void FixedUpdateFunc()
		{
		}
	}

	public class PortalMahcineState : BaseFSM<BoardSquare>
	{
		Entity ent;
		public override void Begin()
		{
			base.Begin();
		}
		public override void Finish()
		{
			base.Finish();
		}
		protected override void FixedUpdateFunc()
		{
		}
	}
}

