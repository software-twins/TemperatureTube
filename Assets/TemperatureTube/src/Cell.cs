using System;
using UnityEngine;

namespace Simulation
	{
	public class Cell 
		{
		public Cell(TemperatureTube value)
			{
       		_tube = value;
			}
					
		public double calculate(double speed, double ambient, double time)
			{
			double previous = 0, current = _temperature_wall;

			int iterator = 0;
				
			while (Math.Abs(current - previous) > 1e-4)
				{
				double substance = _substance.temperature(speed, current, _tube.inneradius(), _tube.cellength());

				double heat = _substance.heatransfer(speed, _tube.inneradius(), _tube.cellength()) 
						* (substance - current)	* _tube.innersurface() * time;
				double loss = 20.0 * (current - ambient) * _tube.outersurface() * time;
												
				previous = current;
				current = _temperature_wall + (heat - loss) 
						/ (_tube.cellvolume() * _tube.material().density(1.0) * _tube.material().capacity(1.0)); 

				iterator ++;
				}
						
			_temperature_wall = current;
			_substance.temperature (_substance.temperature(speed, current, _tube.inneradius(), _tube.cellength()));

			return _temperature_wall;
			}
				
		public Cell temperature(double value)
			{
			_temperature_wall = value;
			return this;
			}

		public double temperature()
			{
			return _temperature_wall;
			}

		public Cell substance(Substance value)
			{
			_substance = value;
			return this;
			}
		
		public  Substance substance()
			{
			return _substance;
			}
				
		/** definition of internal class properties */ 
		private TemperatureTube _tube;
				
		private Substance _substance;
		private double _temperature_wall;
		}
	}