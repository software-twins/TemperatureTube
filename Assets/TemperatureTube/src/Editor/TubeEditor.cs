using UnityEditor;
using UnityEngine;

using System;
using System.Collections;

public enum SubstanceEnum 
	{ 
	Water = 0 
	};

public enum MaterialEnum 
	{ 
	Steel = 0 
	};
	
[CustomEditor(typeof (TemperatureTube))]

public class TemperatureTubeEditor : Editor 
	{
	/**
	  * initializing properties and binding them to the data of the tube class inherited from monobehavior
	  */	
	void OnEnable() 
		{
		#region Tube material and geometry
		/** binding of the tube material property, */
		//material = serializedObject.FindProperty("_material"); 

		/** ... tube length, */
		length = serializedObject.FindProperty("_length");
				
		/** ... tube diameters and wall thickness, */
		inner_radius = serializedObject.FindProperty("_inner_radius");
		outer_radius = serializedObject.FindProperty("_outer_radius");
		#endregion

		#region Substance temperature and flow				
		/** ... substance, that flows in a tube, */
		//substance = serializedObject.FindProperty("_substance");

		/** ... its temperature sinus dependence parameters, */
		a_temp = serializedObject.FindProperty("_a_temp");
		b_temp = serializedObject.FindProperty("_b_temp");
		c_temp = serializedObject.FindProperty("_c_temp");	

		/** ... its flow rate sinus dependency parameters, */
		a_flow = serializedObject.FindProperty("_a_flow");
		b_flow = serializedObject.FindProperty("_b_flow");
		c_flow = serializedObject.FindProperty("_c_flow");
		#endregion

		#region Ambient temperature				
		/** ... ambient temperature sinus dependece parameters */
		a_envr = serializedObject.FindProperty("_a_envr");
		b_envr = serializedObject.FindProperty("_b_envr");
		c_envr = serializedObject.FindProperty("_c_envr");
		#endregion
								
		#region Generic settings - visualisation and calculation				
		/** ... visualisation gradient settings, */
		gradient = serializedObject.FindProperty("_gradient");

		/** ... visualisation gradient limits (minimal and maximal) temperature settings, */
		min_grad = serializedObject.FindProperty("_min_grad");
		max_grad = serializedObject.FindProperty("_max_grad");

		/** ... time delay for the start of the calculation relative to the start of the scene */
		time_delay = serializedObject.FindProperty("_delay");
		#endregion

		#region Errors sign
		errors = serializedObject.FindProperty("_input_errors");
		#endregion
		}

	/** tube material and */
	MaterialEnum material_enum;
	/** ... substance, that in tube flows combobox data */
	SubstanceEnum substance_enum;
		
	/**
	  * processing of editor events, called permanently and often
      */
	public override void OnInspectorGUI() 
		{
		serializedObject.Update();

		/** 
		  * constructing an editor group for properties, showing the properties of the
		  * tube - its material and geometry 
		  */
		if (tube_group = EditorGUILayout.Foldout(tube_group, "Tube"))
			{
			/** place combobox for material selection from which tube is made, */
			material_enum = (MaterialEnum) EditorGUILayout.EnumPopup("Material", material_enum);
			
			/** ... place editor line for tube length, */
			EditorGUILayout.PropertyField(length);
						
			/** ... place editor lines for tube radiuses, */
			EditorGUILayout.PropertyField(inner_radius);

			/** we set errors to the initial position. 
			  * If you set it higher, outside the if block, there may be situations when the block in the inspector
			  * is closed and the error status is not updated. Hence, a false interpretation of errors is possible
			  * when they are not determined
			  */
			errors.boolValue = false;

			/** get local value of flow speed and */
			double speed = (a_flow.doubleValue + b_flow.doubleValue) / (Math.PI * inner_radius.doubleValue 
					* inner_radius.doubleValue);

			/** ... show here non modal helpbox if speed is less than 6 m/s and set _errors as true */
			if (speed > 3)
				{
				EditorGUILayout.HelpBox(
						"At such inner radius and flow rate the flow speed will be more than 3 m/s. "
								+ "This value is related to the dependence of the change in the flow rate. Please "
								+ "increse the tube radiusese or decrease the parameters of flow dpendece on _Flow_ tab", 
						MessageType.Error);
				errors.boolValue = true;
				}

			EditorGUILayout.PropertyField(outer_radius);

			/** show here the helpbox if wall thickness is less than 1 mm and set _errors as true */
			if (outer_radius.doubleValue - inner_radius.doubleValue < .0009)
				{
				EditorGUILayout.HelpBox(
						"Wall thickness should not exceed 1 mm. Please coorect the inner or outer radius value",
						MessageType.Error);
				errors.boolValue = true;
				}
			
			/** processing the results of material selection in combo box */
			switch (material_enum) 
				{
				case MaterialEnum.Steel :
					//material.objectReferenceValue = ScriptableObject.CreateInstance<Simulation.Steel>(); 
					break;
															
				default : break;
				}
			}
					
		/** 
		  * constructing an editor group for properties, showing the properties of the
		  * substance, that in tube flows, its temperature and flow change dependeces 
		  */
		if (flow_group = EditorGUILayout.Foldout(flow_group, "Flow")) 
			{
			/** place combobox for substance, that in tube flows, */
			substance_enum = (SubstanceEnum) EditorGUILayout.EnumPopup ("Material", substance_enum);
												
			/** group of elements to determine the coefficients of the sinusoidal dependence of change the flow rate */			
			SinusGroup("Flow", a_flow, b_flow, c_flow);

			/**
			  * here, as well as in the previous block, which is responsible for the drop-down list of floats, 
			  * first we reset the error flag. This is necessary if the flow tab is closed and the code that 
			  * is placed in the corresponding _if_ block is not executed 
			  */
			errors.boolValue = false;

			/** check the values of the coefficients a and b, depending on, so that the flow rate is not negative */
			if (a_flow.doubleValue < b_flow.doubleValue)
				{
				EditorGUILayout.HelpBox(
						"The first coefficient is less than the coefficient at the sinus." 
					   			+ "With such values of the coefficients dependindence of the flow rate, it can be negative. "
								+ "Please correct the values of these coefficients.", 
						MessageType.Error);
				errors.boolValue = true;
				}			

			/** again compute the speed locally */
			double speed = (a_flow.doubleValue + b_flow.doubleValue) / (Math.PI * inner_radius.doubleValue 
					* inner_radius.doubleValue);

			/**
			  * and just like in the previous tab, we check its value and if it is less than 6 m/s we show the helpbox,
			  * the messages in the hepboxes are different, also set the error counter 
			  */
			if (speed > 3)
				{
				EditorGUILayout.HelpBox(
						"At such inner radius and flow rate the flow speed will be more than 3 m/s. " 
								+ "This value is related to the dependence of the change in the flow rate. "
								+ "Please increse the tube radiuses on tab _Tube_ or decrease the parameters of flow dpendece", 
						MessageType.Error);
				errors.boolValue = true;
				}
		
			/** ... and  to determine the coefficients of the sinusoidal dependence of change the temperature */			
			SinusGroup("Temperature", a_temp, b_temp, c_temp);

			/** check the top and  */
			if (a_temp.doubleValue + b_temp.doubleValue > 100)
				{
				EditorGUILayout.HelpBox(
						"At such values, the water temperature can exceeds 100 °C. Please decrease the first and second "
								+ "parameters of temperature dependece", 
						MessageType.Error);
				errors.boolValue = true;
				}

			/**
			  * ... and the lower value of the flow temperature, which will change according to the dependece 
			  * determined by the parameters; if it goes out of bounds when the substance exists in a liquid state,
			  * an error is also generated; here are set values for water 
			  */
			if (a_temp.doubleValue - b_temp.doubleValue < 0)
				EditorGUILayout.HelpBox(
						"At such values, the water temperature can be less than 0 °C. Please increase the first and second "
								+ "parameters of temperature dependece",
						MessageType.Error);
						
			/** ... and correcting upper temperature limit correction for gradient */
			max_grad.doubleValue = a_temp.doubleValue + b_temp.doubleValue;
			
			/** processing the results of substance selection in combo box */			
			switch (substance_enum)
				{
				case SubstanceEnum.Water :
					//substance.objectReferenceValue = (Simulation.Substance) ScriptableObject.CreateInstance("Water"); 
					break;
								
				default : break;
				}
			}

		/**
		  * constructing an editor group for property, showing the ambient temperature dependeces
          */
		if (ambient_group = EditorGUILayout.Foldout (ambient_group, "Ambient")) 
			{
			/** group to determine the coefficients of the sinusoidal dependence of change ambient temperature */
			SinusGroup("Temperature", a_envr, b_envr, c_envr);
			
			/** correcting lower temperature limit correction for gradient */	
			min_grad.doubleValue = a_envr.doubleValue - b_envr.doubleValue;
			}

		/**
		  * constructing an editor group for properties, showing the generic settings
		  * gradient and times
          */
		if (settings_group = EditorGUILayout.Foldout(settings_group, "Generic Settings")) 
			{
			/** place gradient editor component */ 
			EditorGUILayout.PropertyField(gradient, true, null);
				
			/** place minimum and maximum temperature values determined for the gradient, are places as line, 
			  * near to gradient editor component 
			  */
			EditorGUILayout.BeginHorizontal();
				
			/** atrifical formatting symbol, */
			EditorGUILayout.PrefixLabel(" ");
			/** ... minimal value */
			EditorGUILayout.PropertyField(min_grad, GUIContent.none);
			/** ... maximal value */
			EditorGUILayout.PropertyField(max_grad, GUIContent.none);
				
			EditorGUILayout.EndHorizontal();

			/** place editor line for step in time wit which the temperature field is calculating */ 
			/** ... and time delay for calculation in compare with start scene time */ 
			EditorGUILayout.PropertyField(time_delay);
			}
								
		//EditorGUILayout.LabelField ("Editor time :", EditorApplication.timeSinceStartup.ToString ());
        //this.Repaint ();
		serializedObject.ApplyModifiedProperties();
		}
		 
	/**
	  * this is not a native function of the parent class; it construct the sinus group for 
	  * entering coefficients for dependeces of change 
	  * sunstance input temperature, substance flow rate and ambient temperature	
      */
	private void SinusGroup(String name, SerializedProperty a, SerializedProperty b, SerializedProperty c)
		{
		/** construct label string */
		String str = name + " (time) =";
			
		/** all parameters are located in a line */
		EditorGUILayout.BeginHorizontal();
				
		
		/** ... label field - shown on the left, */
		EditorGUILayout.PrefixLabel(str);
		/** ... editor line for first coefficient - _a_, */ 
		EditorGUILayout.PropertyField(a, GUIContent.none);
		/** ... static field, */ 
		EditorGUILayout.LabelField(" + ", GUILayout.Width (15));
		/** ... editor line for first coefficient - _b_, */ 
		EditorGUILayout.PropertyField(b, GUIContent.none);
		/** ... static field, */ 
		EditorGUILayout.LabelField(" * Sin (", GUILayout.Width (35));
		/** ... editor line for first coefficient - _c_, */ 
		EditorGUILayout.PropertyField(c, GUIContent.none);
		/** ... editor line for first coefficient - _b_, */ 
		EditorGUILayout.LabelField(" * time)"); 
			
		EditorGUILayout.EndHorizontal();
		}

	/** definition of properties that are edited in the editor, 
	  * all properties are associated with the public parameters of the class inherited 
      */ 

	private SerializedProperty errors;

	/** ... tube properties, */  
	private SerializedProperty /*material,*/ inner_radius, outer_radius, length; 

	/** ... substance properties, */
	private SerializedProperty /*substance,*/ a_temp, b_temp, c_temp, a_flow, b_flow, c_flow;
 
	/** ... ambient temperature properties, */
	private SerializedProperty a_envr, b_envr, c_envr; 
	
	/** ... generic calculation settings */
	private SerializedProperty gradient, min_grad, max_grad, time_delay;
		
	/** definition of internal class properties, which show the open / closed state of the
	  * parameter groups that are edited in the editorthat are edited in the editor 
	  */ 
	private bool tube_group = false, flow_group = false, ambient_group = false, settings_group = false;
	}
