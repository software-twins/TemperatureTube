using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
	{
    	public Camera cam;
  
    	// Start is called before the first frame update
    	void Start ()
    		{
				cam.transform.LookAt (targetObject, Vector3.up);
     		}
     	
    	public Transform targetObject;
		public float 	 orbitSpeed = 10.0f, moveSpeed = 3.0f;

    	// Update is called once per frame
    	void Update ()
    		{
    			if ( Input.GetMouseButton (0) ) {
        				float delta = Input.GetAxis ("Mouse X");
        				cam.transform.RotateAround (targetObject.transform.position, Vector3.up, delta * orbitSpeed);
    					}
		 
			   	float scroll = Input.GetAxis ("Vertical") * .01f;//; //("Mouse ScrollWheel");
               	cam.transform.Translate (0, 0, scroll, Space.Self);

				if ( Input.GetMouseButton (2) ) {
						float delta = Input.GetAxis ("Mouse X");
        				cam.transform.Translate (0.01f * delta * moveSpeed, 0, 0, Space.Self);
    					}
    		}
	}
