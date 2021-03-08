using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VROOMBodyControll : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown("w"))
        {
            bool isWalk = gameObject.GetComponent<Animator>().GetBool("isWalk");
            gameObject.GetComponent<Animator>().SetBool("isWalk", !isWalk);
        }
    }
}
