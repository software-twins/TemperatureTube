using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;

[ExecuteAlways, RequireComponent (typeof(MeshFilter), typeof(MeshRenderer))]

public class TemperatureTube : MonoBehaviour
	{
	public TemperatureTube ()
		{
		_cells = new List<Simulation.Cell>();
		}
	
	/**
	  * an one of main function, that fills the cells of the pipe with a coolant;
	  * the regime of ideal displacement is considered;
	  * _volume_ - the amount of the heat substance during the time interval
	  * _temperature_ - its temperature, 
	  * it is considered that the temperature is constant throughout the entire time interval
	  */
	private double push (double temperature, double flow, double time)
		{
		//_debug_info.Write ("push: ");
		//_debug_info.Flush ();

		/**
		  *	 we determine the number of cells that will fill the incoming volume, 
		  *  of course, it will not be whole, in the general case, several cells will 
		  *  be filled completely and one partially
		  *  _flow_ (in metere per second) mult to _time_ (ind second) we get a volume of substance,
		  *  that come in tube in time period _time_
		  */
		double cells = (flow * time) / (Math.PI * _inner_radius * _inner_radius * cellength());

		//_debug_info.Write ("volume " + volume + " cells " + cells + " ");
		//_debug_info.Flush ();
					
		/**
		  * calculate the number of cells that are filled with the received quantity of coolant; 
		  * stop at a cell that is not fully filled
		  * round off the length passed by the coolant if it is greater than the length of the pipe 
		  */	
		int current = (cells > _cells.Count) ? _cells.Count : (int) Math.Floor(cells);

				//_debug_info.Write ("current " + current + " ");
		//_debug_info.Flush ();
						
		/** 
		  * shift the cells, making room for new cells
		  */	
		for ( int i = _cells.Count - 1; i  > current - 1; i -- )
			//_cells [i].flow ().temperature (_cells [i - current].flow ().temperature ());
			_cells [i].substance(_cells[i - current].substance ());
		/* fill the space with new cells */
		for (int i = 0; i < current; i ++)
			{
			_cells [i].substance(ScriptableObject.CreateInstance<Simulation.Water>().temperature(temperature));
			//_debug_info.Write (_cells [i].flow ().temperature () + " ");
			}

		//_debug_info.WriteLine ();
		//_debug_info.Flush ();
								
		/** 
		  * we calculate the length occupied by the remainder of the incoming coolant in the cell
		  */			
		double remain = cells - Math.Floor (cells);

				/** 
		  * calculation of the remainder that does not completely fill the cell; 
		  * it is calculated as the calculation of the temperature of the coolant in the cell as the
		  * average between the temperature that was in the cell and the remainder that entered it; 
		  * cell volumes and the amount of the received balance are taken into account
		  */
		for ( int i = _cells.Count - 1; i > current - 1; i -- ) 
			{
			double mix, value;
			mix = (i != 0) ? _cells[i - 1].substance().temperature() : temperature;
			value = (1 - remain) * _cells[i].substance().temperature() + remain * mix;
	
			_cells [i].substance().temperature(value);

			//_debug_info.Write ("push: loop i " + i + " ");
			//_debug_info.Flush ();
			}
							
		/** calculation of the flow rate, according to the flow rate value passed to the function, 
		  * so it seems that the push function will more reflect the physical meaning, 
		  * since this single function determines the hydrodynamics of the flow
		  */
		return flow / (Math.PI * _inner_radius * _inner_radius);
		}					

	/**
	  * calculation of the area of the _inner_ surface of the cell, 
	  * not the area of its section (!)
	  */	
	public double innersurface()
		{
		return 2 * Math.PI * _inner_radius * cellength(); 
		}

	/**
	  * calculation of the area of the _outer_ surface of the cell, 
	  * not the area of its section (!)
	  */
	public double outersurface()
		{
		return 2 * Math.PI * _outer_radius * cellength(); 
		}
	
	/**
	  * calculation of the cell length; 
	  * it is calculated so that the value of the cell length is close to the limit of .05 meters,
	  * from the greater or lesser side
	  */
	public double cellength()
		{
		return _cell_length;
		}

	/**
	  * calculation of the cthe volume of the tube along the length of the cell, 
	  * not its internal volume, but the volume of the tube wall
	  */
	public double cellvolume()
		{
		return Math.PI * (_outer_radius * _outer_radius - _inner_radius * _inner_radius) * cellength(); 		
		}
		
	/**
	  *	simple access function it needs for cells to calculate heat payment coefficient
	  * as geometrical parameters of channel, where subsctance flows
	  */
	public double inneradius()
		{
		return _inner_radius;
		}

	/**
	  *	then are come three functions, that calculates the 
	  * current values of the flow rate, of the sunstance, entering the tube, - _volume_,
	  * its temperature - _temperature_
	  * and the ambient temperature - _ambient_;
	  * the calculation is performed for the current moment in time, which is represented in the variable _time_counter_
	  */
	private double flow()
		{
		return _a_flow + _b_flow * Math.Sin(_c_flow * _time_counter);
		}

	private double temperature()
		{
		return _a_temp + _b_temp * Math.Sin(_c_temp * _time_counter);
		}

	private double ambient()
		{
		return _a_envr + _b_envr * Math.Sin(_c_envr * _time_counter);
		}
	
	/**
	  * calculation of the position of the gradient depending on the temperature
	  */
	private float percent (double value)
		{
		return (float) ((value - _min_grad) / (_max_grad - _min_grad));
		}

	/**
	  * calculation of the texture of the outer surface of the tube 
	  * based on the calculation of its temperature field
	  */
	private void loop ()
		{
		// _debug_info.WriteLine ("at : " + _time);
		// _debug_info.Flush ();
		Color [] cols = _material.pixels(0);
		// int width = Mathf.Max (1, _texture.width  >> mip);
		// Debug.Log ("mip :" + mip + " " + width);
		
		/** placing a quantity of a substance in a tube within a specified time interval
		  * the result is the flow rate in this case at a given flow rate
		  */
		double speed = push(temperature(), flow(), _step); 

		int pixels = 0;
		foreach (Simulation.Cell item in _cells) 
			{
			/** average cell temperature calculation  */
			float value = (float) item.calculate(speed, ambient(), _step);
			
			/**
			  * calculation of the texture color value for the obtained
			  * average temperature value along the cell length; 
			  * the tube surface is considered to have a constant temperature over the entire length of the cell
			  */
			Color c  = _gradient.Evaluate(percent(value));
			/** expandштп the resulting color value to the entire length of the cell,
			  * it is set in accordance with the scale factor
			  */
			for (int i = 0; i < 10; i ++)
				cols[i + pixels] = c * 0.8f;//1.1f;

			//Debug.Log("J counter " + j);
			pixels += 10;
			//_debug_info.WriteLine(i + ": " + value + " " + item.flow ().temperature ());
			//_debug_info.Flush ();
			}
		
		//_debug_info.WriteLine ();
		//sw.WriteLine (item.calculate (volume, 5.0));
		//cols [i ++] = color ((float) item.calculate (volume, 5.0));
        _material.pixels(cols, 0);
		
		_time_counter += _step;		
		}

	/** get method for the _errors property. The property itself is public, this is an unity editor's requirement,
	  * but to preserve the idea of OOP, a method is also declared
	  */
	public bool errors()
		{
		return _input_errors;
		}

	/**
	  * two get methods
	  */
	public Simulation.Material material()
		{
		return _material;
		}	
				
	public List<Simulation.Cell> cells()
		{
		return _cells;
		}

	/** 
	  * further tow Unity-methods _Start_ (start is called before the first frame update)
	  * and _OnDrawgizmos_ are described, that should be overloaded
	  * when inheriting a class from _Monobehavior_	
	  */	
	private void  Start()
		{
		/** initalize mesh */
        geometry(GetComponent<MeshFilter>().mesh = new Mesh()).hideFlags = HideFlags.HideAndDontSave;
		
		/** separating the editor functionality from the execution functionality and 
		  * and monitoring whether there are errors in the source data for the object, 
		  * if there are, then the object is not created either 
		  */
		if (! Application.IsPlaying(this) ||_input_errors)
			return;
		
		/** creation material and for each surface of tube, then */
		_material = ScriptableObject.CreateInstance<Simulation.Steel>().set(4);
				
		/** calculate cell number and then */
		double number = _length / _cell_length;
		
		/** ... adjusting the cell length from the base value */	
		_cell_length += (number - Math.Floor(number)) * _cell_length / Math.Floor(number);

		/** initialize cell list - this is calculation core structure */		
		for (int i = 0; i < number; i ++)
			_cells.Add(new Simulation.Cell(this)
					.temperature(_a_envr)
					.substance(ScriptableObject.CreateInstance<Simulation.Water>().temperature(_a_envr))); 
														
		/** get the unity rendering core component for mesh */
		Renderer renderer = GetComponent<Renderer>();
				
		/** 10 - here is the scale factor with which the texture is detailed */
		int [] textureslength = { 10 * _cells.Count };

		/** assign the resulting array to the same array that is only part of the unit core */
		//renderer.materials = materials; //new Material[4];
		renderer.materials = _material.r(textureslength); //new Material[4];

			/** finally, we define a function that will be called to calculate the temperature field - _loop_,
		  * the time interval after which it will be called - _time_step_ 
	      * and the time delay from the start of the scene, after which the calculation
		  * will start, are also determined	- _time_delay_;
		  * these values (_time_step_ and _time_delay_)are defined in the editor 
		  */	
		InvokeRepeating ("loop", _delay, _step);
		}

	private void OnDrawGizmos()
		{
		Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
				
		/** insure ourselves ... (?) */
		if (mesh == null)
			return;
				
		/** draw vertices */
		float radius = .25f * (float) (_outer_radius - _inner_radius);
				
		for (int i = 0; i < mesh.vertices.Length; i ++) 
			{
			Gizmos.color = (mesh.vertices [i].z < 0) ? Color.black : Color.yellow;
			Gizmos.DrawSphere (mesh.vertices[i], radius);
			}

		/** draw normals */
		//Gizmos.color = Color.red;
		//float scale = .05f * (float) _length;
				
		//for ( int i = 0; i < mesh.vertices.Length; i ++ )
		//		Gizmos.DrawRay (mesh.vertices [i], scale * mesh.normals [i]);

		/** draw a line between the side surfaces of the cylinder */
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, (float) _length));
		}

	/** the function, that draws a tube; 
	  * it is not purely a _Monobehavior_ method, but it use Unity protocols 
	  * to draw polygons of tube sides in Unity space
	  */
	private Mesh geometry(Mesh mesh)
		{
		mesh.Clear ();
 
		/** this code is borrowed from http://wiki.unity3d.com/index.php/ProceduralPrimitives and redone:
		  * 1.for horizontal pipe
		  * 2.for separated meshes
		  */
 		int nbSides = 24;
 
		/** Outter shell is at radius1 + radius2 / 2, inner shell at radius1 - radius2 / 2 */
		//float radius1 = (float) _outer_radius, radius2 = (float) _inner_radius; //.5f;
		/** float bottomRadius2 = (float) _inner_radius, topRadius2 = (float) _inner_radius; //.15f; */
		int nbVerticesCap = nbSides * 2 + 2, nbVerticesSides = nbSides * 2 + 2;

		#region Vertices
		/** bottom + top + sides */
		Vector3 [] vertices = new Vector3 [nbVerticesCap * 2 + nbVerticesSides * 2];
				
		int vert = 0;
		float _2pi = Mathf.PI * 2f;
 
		/** Bottom cap */
		int sideCounter = 0;
		while (vert < nbVerticesCap)
			{
			sideCounter = sideCounter == nbSides ? 0 : sideCounter;
 
			float r1 = (float) (sideCounter ++) / nbSides * _2pi;
			float cos = Mathf.Cos(r1);
			float sin = Mathf.Sin(r1);

			vertices[vert + 0] = new Vector3(cos * (float) _inner_radius, //(bottomRadius1 - bottomRadius2 * .5f), 
									   		 sin * (float) _inner_radius, //(bottomRadius1 - bottomRadius2 * .5f), 
											 0f);
			vertices[vert + 1] = new Vector3(cos * (float) _outer_radius, //(bottomRadius1 + bottomRadius2 * .5f), 
											 sin * (float) _outer_radius, //(bottomRadius1 + bottomRadius2 * .5f), 
											 0f);
					
			vert += 2;
			}
 
		/** Top cap */
		sideCounter = 0;
		while (vert < nbVerticesCap * 2)
			{
			sideCounter = sideCounter == nbSides ? 0 : sideCounter;
 
			float r1 = (float) (sideCounter ++) / nbSides * _2pi;
			float cos = Mathf.Cos(r1);
			float sin = Mathf.Sin(r1);

			vertices[vert + 0] = new Vector3(cos * (float) _inner_radius, //(topRadius1 - topRadius2 * .5f), 
											 sin * (float) _inner_radius, //(topRadius1 - topRadius2 * .5f),
											 (float) _length);
			vertices[vert + 1] = new Vector3(cos * (float) _outer_radius, //(topRadius1 + topRadius2 * .5f), 
											 sin * (float) _outer_radius, //(topRadius1 + topRadius2 * .5f),
											 (float) _length);

			vert += 2;
			}
 
		/** Sides (out) */
		sideCounter = 0;
		while (vert < nbVerticesCap * 2 + nbVerticesSides)
			{
			sideCounter = sideCounter == nbSides ? 0 : sideCounter;
 
			float r1 = (float) (sideCounter ++) / nbSides * _2pi;
			float cos = Mathf.Cos(r1);
			float sin = Mathf.Sin(r1);
 
			vertices[vert + 0] = new Vector3(cos * (float) _outer_radius, //(topRadius1 + topRadius2 * .5f),  
										     sin * (float) _outer_radius, //(topRadius1 + topRadius2 * .5f),
											 (float) _length);
			vertices[vert + 1] = new Vector3(cos * (float) _outer_radius, //(bottomRadius1 + bottomRadius2 * .5f), 
											 sin * (float) _outer_radius, //(bottomRadius1 + bottomRadius2 * .5f), 
											 0f);
					
			vert += 2;
			}
 
		/** Sides (in) */
		sideCounter = 0;
		while (vert < vertices.Length)
			{
			sideCounter = sideCounter == nbSides ? 0 : sideCounter;
 
			float r1 = (float) (sideCounter ++) / nbSides * _2pi;
			float cos = Mathf.Cos(r1);
			float sin = Mathf.Sin(r1);
 
			vertices [vert + 0] = new Vector3 (cos * (float) _inner_radius, //(topRadius1 - topRadius2 * .5f), 
											   sin * (float) _inner_radius, //(topRadius1 - topRadius2 * .5f),
											   (float) _length);
			vertices [vert + 1] = new Vector3 (cos * (float) _inner_radius, //(bottomRadius1 - bottomRadius2 * .5f), 
											   sin * (float) _inner_radius, //(bottomRadius1 - bottomRadius2 * .5f),
											   0f);
				
			vert += 2;
			}
				
		mesh.vertices = vertices;
		#endregion
 
		#region Normales
		/** bottom + top + sides */
		Vector3 [] normales = new Vector3 [vertices.Length];
				
		vert = 0;
 
		/** Bottom cap */
		while (vert < nbVerticesCap)
			normales[vert ++] = Vector3.back;
			 
		/** Top cap */
		while (vert < nbVerticesCap * 2)
			normales[vert ++] = Vector3.forward;
 
		/** Sides (out) */
		sideCounter = 0;
		while (vert < nbVerticesCap * 2 + nbVerticesSides )
			{
			sideCounter = sideCounter == nbSides ? 0 : sideCounter;
 
			float r1 = (float) (sideCounter ++) / nbSides * _2pi;
 
			normales[vert + 0] = new Vector3(Mathf.Cos(r1), Mathf.Sin(r1), 0f);
			normales[vert + 1] = normales[vert];
					
			vert += 2;
			}
 
		/** Sides (in) */
		sideCounter = 0;
		while (vert < vertices.Length)
			{	
			sideCounter = sideCounter == nbSides ? 0 : sideCounter;
 
			float r1 = (float) (sideCounter ++) / nbSides * _2pi;
 
			normales[vert + 0] = - (new Vector3(Mathf.Cos(r1), Mathf.Sin(r1), 0f));
			normales[vert + 1] = normales[vert];
					
			vert += 2;
			}

		mesh.normals = normales;
		#endregion
 
		#region UVs
		Vector2 [] uvs = new Vector2[vertices.Length];
 
		vert = 0;

		/** Bottom cap */
		sideCounter = 0;
		while (vert < nbVerticesCap)
			{
			float t = (float) (sideCounter ++) / nbSides;
					
			uvs[vert ++] = new Vector2(0f, t);//(0f, t);
			uvs[vert ++] = new Vector2(1f, t);//(1f, t);
			}
 
		/** Top cap */
		sideCounter = 0;
		while (vert < nbVerticesCap * 2)
			{
			float t = (float)(sideCounter++) / nbSides;
					
			uvs[vert ++] = new Vector2(0f, t);
			uvs[vert ++] = new Vector2(1f, t);
			}
 
		/** Sides (out) */
		sideCounter = 0;
		while (vert < nbVerticesCap * 2 + nbVerticesSides)
			{
			float t = (float) (sideCounter++) / nbSides;
	
			uvs[vert + 0] = new Vector2(0f, t);
			uvs[vert + 1] = new Vector2(1f, t);

			vert += 2;
			}
 
		/** Sides (in) */
		sideCounter = 0;
		while (vert < vertices.Length)
			{
			float t = (float)(sideCounter++) / nbSides;
						
			uvs[vert ++] = new Vector2(0f, t);
			uvs[vert ++] = new Vector2(1f, t);
			}
		mesh.uv = uvs;
		#endregion

		mesh.subMeshCount = 4;
 
		#region Triangles
		//int nbFace = nbSides * 4;
		//int nbTriangles = nbFace * 2;
		//int nbIndexes = nbTriangles * 3;
			
		sideCounter = 0;
			 
		int [] triangles = new int[nbSides * 6];
				
		/** Bottom cap */		
		int i = 0;
		while (sideCounter < nbSides)
			{
			int current = sideCounter * 2;
			int next = sideCounter * 2 + 2;
 
			triangles[i ++] = next + 1;
			triangles[i ++] = current; 
			triangles[i ++] = next;

			triangles[i ++] = current + 1;
			triangles[i ++] = current; 
			triangles[i ++] = next + 1;
 
			sideCounter ++;
			}
		
		mesh.SetTriangles(triangles, 1);
 
		/** Top cap */
		i = 0;
		while (sideCounter < nbSides * 2)
			{
			int current = sideCounter * 2 + 2;
			int next = sideCounter * 2 + 4;
 
			triangles[i ++] = current;
			triangles[i ++] = next + 1;
			triangles[i ++] = next;
 
			triangles[i ++] = current;
			triangles[i ++] = current + 1;
			triangles[i ++] = next + 1;
 
			sideCounter ++;
			}
		
		mesh.SetTriangles(triangles, 2);				
 			
		triangles = new int [nbSides * 3 * 6];

		/** Sides (out) */				
		i = 0;
		while (sideCounter < nbSides * 3)
			{
			int current = sideCounter * 2 + 4;
			int next = sideCounter * 2 + 6;
 
			triangles[i ++] = current;
			triangles[i ++] = next + 1;
			triangles[i ++] = next;
 
			triangles[i ++] = current;
			triangles[i ++] = current + 1;
			triangles[i ++] = next + 1;
 
			sideCounter ++;
			}
		
		mesh.SetTriangles (triangles, 0);				
 
		/** Sides (in) */
		i = 0;
		while (sideCounter < nbSides * 4)
			{
			int current = sideCounter * 2 + 6;
			int next = sideCounter * 2 + 8;
 
			triangles[i ++] = next + 1;
			triangles[i ++] = current;
			triangles[i ++] = next;
 
			triangles[i ++] = current + 1;
			triangles[i ++] = current; 
			triangles[i ++] = next + 1;
 
			sideCounter ++;
			} 
		mesh.SetTriangles (triangles, 3);
		#endregion
 
		mesh.RecalculateBounds ();
		mesh.Optimize ();

		return mesh;
		}

	/** 
	  * further follows the declaration of _public_ properties,
	  * that are available and configured in the Unity editor
	  */

	/** determination the time step with which we perform the calculation and visualization,
	  * it meets in the program 4 times in different places */
	private const float _step = .2f;
	
	/** determination of the material from which the pipe is made,*/
	[SerializeField] 
	public Simulation.Material _material;// = (PhysicMaterial) ScriptableObject.CreateInstance <Steel> ();
	
	/** ... flow substance, */	
	[SerializeField] 
	public Simulation.Substance _substance;

	/** ... parameters of the dependence of the substance temperature change, */
	public float _a_temp = 50.0f, _b_temp = 1f, _c_temp = 1f;

	/** ... parameters of the dependence of the substance flow change, */
	public float _a_flow = .001f, _b_flow = .001f, _c_flow = 1f;
	
	/** ... parameters of the dependence of the ambient temperature flow change, */
	public float _a_envr = 20f, _b_envr = 0f, _c_envr = 1f;

	/** ... tube geometry, */
	public double _inner_radius = 0.05, _outer_radius = 0.06, _length = 1.0;

	/** ... gradient that colors the tube temperature field, */
	public Gradient _gradient = new Gradient()
		{
    	/** the number of keys must be specified in this array initialiser */
    	colorKeys = new GradientColorKey[6] 
			{
        	/** add colour and specify the stop point */
        	new GradientColorKey(new Color(0.0f, 0.0f, 1.0f), 0.0f),
			new GradientColorKey(new Color(0.0f, 1.0f, 1.0f), 0.2f),
			new GradientColorKey(new Color(0.0f, 1.0f, 0.0f), 0.4f),
			new GradientColorKey(new Color(1.0f, 1.0f, 0.0f), 0.6f),
			new GradientColorKey(new Color(1.0f, 0.0f, 0.0f), 0.8f),
			new GradientColorKey(new Color(1.0f, 0.0f, 1.0f), 1.0f),
			},
    			
		/** this sets the alpha to 1 at both ends of the gradient */
    	alphaKeys = new GradientAlphaKey[6]
			{
        	new GradientAlphaKey(1, 0.0f),
        	new GradientAlphaKey(1, 0.2f),
			new GradientAlphaKey(1, 0.4f),
			new GradientAlphaKey(1, 0.6f),
        	new GradientAlphaKey(1, 0.8f),
			new GradientAlphaKey(1, 1.0f),
			}
		};

	/** ... minimum and maximum temperatures shown by the gradient, */
	public float _min_grad = 20.0f, _max_grad = 51.0f;
		
	/** ... time parameters - the time interval through which the calculations are performed _time_step_ and 
	  * the delay of the start of the calculation relative to the start of the scene - _time_delay_
	  */
	public float _delay = .0f;

	/** ... account errors in the source data that are set in the editor */ 
	public bool _input_errors = false;

	/** 
	  * now the usual private methods of the class are defined
	  */
	
	/** cells that are projected onto the tube, */
	private List<Simulation.Cell> _cells; // = new List < Cell > ();
	
	/** ... counter of the time, during which the calculation is performed, */
	private float _time_counter = 0;

	/** ... cell length, it is chosen so that all cells are the same length; first,
	  * the base value is shown here, which is then adjusted when the primitive is created;
	  * this is necessary in order for the cell to be near the base value,
	  */
	private double _cell_length = .05f;

	/*private void  material (Renderer renderer)
			{
				renderer.materials = new Material [4];
				
				for ( int i = 0; i < renderer.materials.Length; i ++ )
						renderer.materials [i].SetColor ("_Color", Color.gray); 
				
				//= //.mainTexture = texture [i] = new Texture2D (256, 1); 
				//renderer.materials [2].SetColor ("_EmissionColor", Color.white);
				renderer.materials [2].mainTexture = _texture = new Texture2D (_cells.Count, 1);  
			}
			
		/*public  void update_geometry ()
			{
				geometry (GetComponent <MeshFilter> ().mesh);
			}*/
	}