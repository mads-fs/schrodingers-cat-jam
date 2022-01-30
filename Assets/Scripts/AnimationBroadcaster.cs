using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationBroadcaster : MonoBehaviour
{
    public event EventHandler<string> OnBroadcastEvent;
    public void Broadcast(string eventName) => OnBroadcastEvent?.Invoke(this, eventName);
}
