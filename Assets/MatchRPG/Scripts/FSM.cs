using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FSM<T>
{
	//===================================================================================
	//  the Finite Stage Machine for EditorBoardSquare class.
	public Dictionary<Type, BaseFSM<T>> stateDic = new Dictionary<Type, BaseFSM<T>>();
	public BaseFSM<T> currentState;
	public BaseFSM<T> previousState;

	public FSM(T owner)
	{
		RegisterStates( owner );
	}

	//--- FSM Methods ------------------------------------------------------
	// Regist all state classes.
	public void RegisterStates( T owner )
	{
		try
		{
			BaseFSM<T> instance;
			Type thisType = owner.GetType();

			Type[] nestedTypes = thisType.GetNestedTypes();

			foreach (Type t in nestedTypes)
			{
				if (t.IsSubclassOf(typeof(BaseFSM<T>)) && !t.IsAbstract)
				{
					if (stateDic.ContainsKey(t) == false)
					{
						instance = Activator.CreateInstance(t) as BaseFSM<T>;
						instance.Initialize( owner );
						stateDic.Add( t, instance );
					}
				}
			}

			// Add a deactivate state class
			if (stateDic.ContainsKey(typeof(DeactivationState)) == false)
			{
				instance = Activator.CreateInstance( typeof(DeactivationState)) as BaseFSM<T>;
				instance.Initialize( owner );
				stateDic.Add( typeof(DeactivationState), instance );
			}

		}
		catch(Exception e)
		{
			Debug.Log("Error "+e.Message);
		}
	}

	public void ChangeState( Type stateType )
	{
		if( stateDic.ContainsKey(stateType) )
		{
			BaseFSM<T> fsm = stateDic[ stateType ];

			if( fsm != null )
			{
				previousState = currentState;
				currentState = fsm;
				currentState.Begin();
			}
		}
	}

	public void RestoreState()
	{
		ChangeState(previousState.GetType());
	}

	public void Pause( bool isPaused )
	{
		if( isPaused )
		{
			if( currentState.GetType() != typeof (DeactivationState) )
			{
				previousState = currentState;
				ChangeState( typeof(DeactivationState) );
			}
		}
		else
		{
			if( currentState.GetType() == typeof (DeactivationState) )
				ChangeState( previousState.GetType() );
		}
	}

	public class DeactivationState : BaseFSM<T>
	{
		public DeactivationState() {}
		public DeactivationState( T status )
		{
			Initialize( status );
		}
	}
}

