using System;

namespace Simulation
	{
	public class Water : Substance
		{
		public Water () : base () 
			{
			}
				
		/* sudstance */
		override public double heatcapacity ()
			{
			return 4100.0;
			}

		override public double viscosity ()
			{
			return 1.0e-3 / (0.558 + 19.8e-3 * _temperature + 0.105e-3 * _temperature * _temperature);
			}
			
		override public double heatconduct ()
			{
			// by _temperature - 160 return NAN
			return Math.Pow (0.303 + 3.03e-3 * _temperature - 13.98e-6 * _temperature * _temperature, 0.5);
			}
		}
	}
