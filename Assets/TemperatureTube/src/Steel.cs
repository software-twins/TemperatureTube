using System;
using UnityEngine; 

namespace Simulation
	{
	public class Steel : Material
		{
		public Steel () : base () 
			{
			_albedo = new Color(.7f, .7f, .7f, .7f);
		 	} 

		public override double conduct  (double value)
			{
			return 70.0;
			}

		public override double density  (double value)
			{
			return 7800.0;
			} 

		public override double capacity (double value)
			{
			return 462.0;
			}
		}
	}
