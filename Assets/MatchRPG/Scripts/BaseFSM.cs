//using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BaseFSM<T>
{
	protected T owner;
	public delegate void UpdateMethod();
	public UpdateMethod Update;
	public UpdateMethod LateUpdate;
	public UpdateMethod FixedUpdate;

	public virtual void Initialize( T own )
	{
		owner = own;
	}

	public virtual void Begin()
	{
		Update = UpdateFunc;
		LateUpdate = LateUpdateFunc;
		FixedUpdate = FixedUpdateFunc;
	}

	protected virtual void UpdateFunc() {}
	protected virtual void LateUpdateFunc() {}
	protected virtual void FixedUpdateFunc() {}
	public virtual void Finish()
	{
		Update = this.UpdateFunc;
		LateUpdate = this.LateUpdateFunc;
		FixedUpdate = this.FixedUpdateFunc;
	}

//	protected virtual IEnumerator WaitingEnd( float sec )
//	{
//		yield return new WaitForSeconds( sec );
//		Finish();
//	}
}
