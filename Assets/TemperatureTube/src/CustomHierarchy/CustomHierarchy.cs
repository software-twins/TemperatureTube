using UnityEngine;
using UnityEditor;

using System;

[InitializeOnLoad]
public class CustomHierarchyView 
	{
	static CustomHierarchyView()
		{
		EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
		}

	/** this function is called for every item in the scene tree; this function is called for every item in the 
	  * scene tree; it checks whether the element contains errors or not; if they are, in the inspector, 
	  * an error is shown for it  
	  */
	static void HierarchyWindowItemOnGUI(int id, Rect rect)
		{			
		/** get the */
		GameObject gameObject = EditorUtility.InstanceIDToObject(id) as GameObject;

		/** ... not null GameObject */
		if (! gameObject)
    		return;
    		
		/** check to see if the GameObject is one we care about$ in our case it is TemperatureTube */
		TemperatureTube tube = gameObject.GetComponent<TemperatureTube>();
		
		/** we check if the object is our object and here we check if it contains errors */
		if (! tube || ! tube.errors())
			return; 
		
		/** 
		  * the part of code further to the end is running only if we as GameObject have a TemperatureTube
		  * and it contains an errors 
		  */
		Rect r = new Rect(rect);
      		
		// Position the rectangle off to the right of list, 15px in from the edge
      	r.x = rect.x + rect.width - 18; //gameObject.ToString().Length;

		GUIStyle style = new GUIStyle();
		style.normal.textColor = Color.red;

		GUI.Label(r, "Error", style);
		}
	}