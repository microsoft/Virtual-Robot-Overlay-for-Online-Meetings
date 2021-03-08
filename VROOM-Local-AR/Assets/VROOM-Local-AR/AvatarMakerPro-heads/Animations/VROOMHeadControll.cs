using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VROOMHeadControll : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        float headTurn = Input.GetAxis("Horizontal");
        float headNod = Input.GetAxis("Vertical");
        gameObject.GetComponent<Animator>().SetFloat("headTurn", headTurn);
        gameObject.GetComponent<Animator>().SetFloat("headNod", headNod);
        if (Input.GetKeyDown("space"))
        {
            bool isTalk = gameObject.GetComponent<Animator>().GetBool("isTalk");
            gameObject.GetComponent<Animator>().SetBool("isTalk", !isTalk);
        }
    }
}
