﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "event_", menuName = "Event")]
public class so_GameEvent : ScriptableObject
{
	private List<GameEventListener> listeners = new List<GameEventListener>();

	public void Raise()
	{
		for (int i = listeners.Count - 1; i >= 0; i--)
			listeners[i].OnEventRaised();
	}

	public void RegisterListener(GameEventListener listener)
	{ 
		listeners.Add(listener); 
	}

	public void UnregisterListener(GameEventListener listener)
	{ 
		listeners.Remove(listener); 
	}
}