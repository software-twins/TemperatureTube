using System;
using UnityEngine;

namespace Simulation
	{
	abstract public class Material : ScriptableObject //UnityEngine.Material 
		{
		public Material() : base() 
			{
			} 

		public Material set(int fragments)
			{
			_materials = new UnityEngine.Material[fragments];

			for (int i = 0; i < fragments; i ++)
				{
				_materials[i] = new UnityEngine.Material(Shader.Find("Standard"));
				_materials[i].SetColor("_Color", _albedo); 
				}
			
			return this;
			}
			
		public UnityEngine.Material [] r(int [] texturelength)
			{
			_fields = new Texture2D[texturelength.Length];

			for (int i = 0; i < texturelength.Length; i ++)
				{
				if (texturelength[i] == 0)
					continue;
				
				_materials[i].EnableKeyword("_EMISSION");
								
				//Texture2D t = _fields[i] = new Texture2D(texturelength[i], 1);

				_materials[i].SetColor("_EmissionColor", _albedo);
				_materials[i].SetTexture("_EmissionMap", _fields[i] = new Texture2D(texturelength[i], 1));
				}

			return _materials;
			}

		public Material pixels(Color [] cols, int i)
			{
			_fields[i].SetPixels(cols, 0);
			_fields[i].Apply(true);	

			return this;
			}

		public Color [] pixels(int i)
			{
			return _fields[i].GetPixels(0);
			}

		public abstract double conduct(double value);
		public abstract double density(double value);
		public abstract double capacity(double value);

		protected Color _albedo;

		private UnityEngine.Material [] _materials;
		
		private Texture2D [] _fields;
		}
	}