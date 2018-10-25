using UnityEngine;
using System;
using System.Collections;

public class Entity : MonoBehaviour
{
	public EntityData entityData;
	public BoardSquare linkedSquare;
	private FSM<Entity> fsm;

//	private GameManager.RearrangeState rearrangeState;
	private GameManager.CrashState crashState;
	private GameManager.SwapEntitiesState swapState;
	private GameManager.SwapFailedState swapFailedState; 
	private BoardSquare destSquare;

	void Awake()
	{
		fsm = new FSM<Entity>(this);

		// If this line is run in the Start method,
		//   it will changed the current state to a normal state by a time lag
		//   because the combined entity is a new one.
		fsm.ChangeState( typeof(NormalState) );	
	}

	void Start ()
	{
	}

//	void Update ()
//	{
//		fsm.currentState.Update();
//	}

	void FixedUpdate()
	{
		fsm.currentState.FixedUpdate();
	}

	public void SetParent(BoardSquare bs)
	{
		if(gameObject != null && bs != null && bs.gameObject != null)
		{
			gameObject.transform.SetParent(bs.gameObject.transform);
			linkedSquare = bs;
		}
	}

//	public void ChangeToMovingAnimationState(GameManager.RearrangeState rearrange=null)
//	{
//		rearrangeState = rearrange;
//		fsm.ChangeState(typeof(MovingAnimationState));
//	}

	public void ChangeToSwipeAnimationState(GameManager.SwapEntitiesState swap, BoardSquare dest, bool isSelected)
	{
		swapState = swap;
		destSquare = dest;
		fsm.ChangeState(typeof(SwipeAnimationState));
	}

	public void ChangeToSwipeFailedAnimationState(GameManager.SwapFailedState swapFailed, BoardSquare dest, bool isSelected)
	{
		swapFailedState = swapFailed;
		destSquare = dest;
		fsm.ChangeState(typeof(SwipeFailedAnimationState));
	}

	public void ChangeToGatheringAnimationState(GameManager.CrashState crash)
	{
		crashState = crash;
		fsm.ChangeState(typeof(GatheringAnimationState));
	}

	public void ChangeToCrashAnimationState(GameManager.CrashState crash)
	{
		crashState = crash;
		fsm.ChangeState(typeof(CrashAnimationState));
	}

	//----------------------------------------------------------------------------------
	// FSM Classes
	public class PauseState : BaseFSM<Entity>
	{
	}

	public class NormalState : BaseFSM<Entity>
	{
		public override void Begin()
		{
			base.Begin();

//			owner.rearrangeState = null;
			owner.crashState = null;
			owner.swapState = null;
		}
		public override void Finish()
		{
			base.Finish();
		}
		protected override void UpdateFunc()
		{
		}
	}

	public class SwipeAnimationState : BaseFSM<Entity>
	{
		Vector3 pos;
		Vector3 destPos;
		float signX = 0;
		float signY = 0;
		float v;

		public override void Begin()
		{
			base.Begin();

			signX = 0;
			signY = 0;
			v = 0;

			destPos = owner.destSquare.transform.localPosition - owner.linkedSquare.transform.localPosition;

			if(destPos.x > 0)
				signX = 1.0f;
			else if(destPos.x < 0)
				signX = -1.0f;

			if(destPos.y > 0)
				signY = 1.0f;
			else if(destPos.y < 0)
				signY = -1.0f;
		}

		public override void Finish()
		{
			base.Finish();

			owner.swapState.Discount();
			owner.fsm.ChangeState(typeof(NormalState));
		}

		protected override void FixedUpdateFunc()
		{
			if(owner.transform.localPosition != destPos)
			{
				pos = owner.transform.localPosition;
				v += GameManager.instance.swipeSpeed * Time.deltaTime; 
				pos.x += signX * v;
				pos.y += signY * v;

				if(signX > 0 && pos.x > destPos.x || signX < 0 && pos.x < destPos.x)
					pos.x = destPos.x;
				if(signY > 0 && pos.y > destPos.y || signY < 0 && pos.y < destPos.y)
					pos.y = destPos.y;

				owner.transform.localPosition = pos;
			}
			else
				Finish();
		}
	}

	public class SwipeFailedAnimationState : BaseFSM<Entity>
	{
		Vector3 pos;
		Vector3 destPos;
		float signX;
		float signY;
		float v;
		int count;

		public override void Begin()
		{
			base.Begin();

			signX = 0;
			signY = 0;
			count = 0;
			v = 0;

			destPos = owner.destSquare.transform.localPosition - owner.linkedSquare.transform.localPosition;
			if(destPos.x > 0)
				signX = 1.0f;
			else if(destPos.x < 0)
				signX = -1.0f;

			if(destPos.y > 0)
				signY = 1.0f;
			else if(destPos.y < 0)
				signY = -1.0f;
		}

		public override void Finish()
		{
			base.Finish();

			owner.swapFailedState.Discount();
			owner.fsm.ChangeState(typeof(NormalState));
		}

		protected override void FixedUpdateFunc()
		{
			if(count < 2)
			{
				if(owner.transform.localPosition != destPos)
				{
					pos = owner.transform.localPosition;
					v += GameManager.instance.swipeSpeed * Time.deltaTime; 
					pos.x += signX * v;
					pos.y += signY * v;

					if(signX > 0 && pos.x > destPos.x || signX < 0 && pos.x < destPos.x)
						pos.x = destPos.x;
					if(signY > 0 && pos.y > destPos.y || signY < 0 && pos.y < destPos.y)
						pos.y = destPos.y;

					owner.transform.localPosition = pos;
				}
				else
				{
					++count;
					destPos = Vector3.zero;
					signX *= -1;
					signY *= -1;
					v = 0;
				}
			}
			else
				Finish();
		}
	}

//	public class MovingAnimationState : BaseFSM<Entity>
//	{
//		Vector3 pos;
//		float signX = 0;
//		float signY = 0;
//		float v;
//
//		public override void Begin()
//		{
//			base.Begin();
//
//			pos = owner.transform.localPosition;
//
//			if(pos.x < 0)
//				signX = 1.0f;
//			else if(pos.x > 0)
//				signX = -1.0f;
//
//			if(pos.y < 0)
//				signY = 1.0f;
//			else if(pos.y > 0)
//				signY = -1.0f;
//
//			v = 0;
//		}
//
//		public override void Finish()
//		{
//			base.Finish();
//
//			if(owner.rearrangeState != null)
//				owner.rearrangeState.Discount();
//			owner.fsm.ChangeState(typeof(NormalState));
//		}
//
//		protected override void FixedUpdateFunc()
//		{
//			if(owner.transform.localPosition != Vector3.zero)
//			{
//				pos = owner.transform.localPosition;
//				v += GameManager.instance.moveSpeed * Time.deltaTime; 
//				pos.x += signX * v;
//				pos.y += signY * v;
//
//				if(signX > 0 && -Mathf.Epsilon < pos.x || signX < 0 && pos.x < Mathf.Epsilon)
//					pos.x = 0;
//				if(signY > 0 && -Mathf.Epsilon < pos.y || signY < 0 && pos.y < Mathf.Epsilon)
//					pos.y = 0;
//
//				owner.transform.localPosition = pos;
//			}
//			else
//				Finish();
//		}
//	}
//
	public class GatheringAnimationState : BaseFSM<Entity>
	{
		Vector3 originPos;
		float moveTime;
		float t;
		float timeRate;

		public override void Begin()
		{
			base.Begin();
			t = 0;
			originPos = owner.transform.localPosition;
			moveTime = GameManager.instance.gatheringTime;
			if(moveTime == 0)
				Finish();
		}

		public override void Finish()
		{
			base.Finish();

			owner.crashState.Discount();
			owner.fsm.ChangeState(typeof(NormalState));
		}

		protected override void FixedUpdateFunc()
		{
			if(t < moveTime)
			{
				timeRate = 1.0f - t*t/(moveTime*moveTime);
				owner.transform.localPosition = originPos * timeRate;
				SpriteRenderer sp = owner.gameObject.GetComponent<SpriteRenderer>();
				sp.material.color = new Color(sp.material.color.r, sp.material.color.g, sp.material.color.b, timeRate);
				t += Time.deltaTime;
			}
			else
			{
				owner.transform.localPosition = Vector3.zero;
				Finish();
			}
		}
	}

	public class CrashAnimationState : BaseFSM<Entity>
	{
		Vector3 newScale;
		float originScale;
		float moveTime;
		float t;
		float timeScale;

		public override void Begin()
		{
			base.Begin();
			t = 0;
			originScale = 1.0f;
			moveTime = GameManager.instance.gatheringTime;
			if(moveTime == 0)
				Finish();
		}	

		public override void Finish()
		{
			base.Finish();

			owner.crashState.Discount();
			owner.fsm.ChangeState(typeof(NormalState));
		}

		protected override void FixedUpdateFunc()
		{
			if(t < moveTime)
			{
				timeScale = originScale*(1.0f - t*t/(moveTime*moveTime));
				newScale = owner.transform.localScale;
				newScale.x = timeScale;
				newScale.y = timeScale;
				owner.transform.localScale = newScale;

				t += Time.deltaTime;
			}
			else
			{
				owner.transform.localScale = Vector3.zero;
				Finish();
			}
		}
	}
}
