using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieMove : MonoBehaviour
{


    public float speed = 0.1f;
    [HideInInspector]
    public int row;
	// Update is called once per frame
	void Update () {
		transform.Translate(-speed*Time.deltaTime,0,0);
	}
}
