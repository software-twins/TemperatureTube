using System;
using UnityEngine;

namespace Simulation
	{
	abstract public class Substance : ScriptableObject
		{
		public Substance() : base()
			{
			}

		public Substance set(double value)
			{
			_temperature = value;
			return this;
			}
	
		public Substance temperature(double value)
			{
			_temperature = value;
			return this;
			}

		public double temperature()
			{
			return _temperature;
			} 	

		/**
	  	  * calculation of the average flow temperature based on differential equation
	  	  * 	over _length_
	  	  *		for channel of _radius_
	  	  * 	for flow rate _volume_
	  	  *		for wall temperature _wall_
	  	  */
		public double temperature(double speed, double wall, double radius, double length)
			{
			double value = 2 * Math.PI * radius * heatransfer(speed, radius, length) 
					/ (heatcapacity() * speed / (988.5 * Math.PI * radius * radius));
			return wall + (_temperature - wall) / (length * value) * (1.0 - Math.Exp(-1.0 * value * length));
			}			

		/* substance */			
		public double heatransfer(double speed, double radius, double length)
			{
			double re, pr; 

			re = speed * 2 * radius * /*density*/ 988.5 / viscosity ();
			pr = heatcapacity () * viscosity () / heatconduct ();

			return re > 2300 ? 
					0.021 * Math.Pow(re, 0.8) * Math.Pow(pr, 0.43) * heatconduct() / (2 * radius)
					: 
					1.55 * heatconduct() * Math.Pow(speed * radius * radius * heatcapacity() * 988.5 
							/ (length * heatconduct()),  0.333)  / (2 * radius);
			}

		abstract public double heatcapacity (); /* flow */
		abstract public double viscosity ();
		abstract public double heatconduct ();

		/**
	  	  *	 _public_ - since the Instantiate metod of ScriptableObject used to copy Unity objects
	  	  *  only copies the _public_ members of the object 
	  	  */		
		public double _temperature = 0; 
		}
	}