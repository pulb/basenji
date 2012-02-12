// MetadataUtils.cs
//
// Copyright (C) 2011, 2012 Patrick Ulbrich
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;

namespace VolumeDB.Metadata
{
	public static class MetadataUtils
	{
		// returns the libextractor 0.5.x duration format.
		// all metadata items that have a duration must use this format
		public static string SecsToMetadataDuration (double seconds) {
			if (seconds < 60.0)
				return ((int)Math.Round(seconds)).ToString() + "s";
			
			long totalSecs = (long)seconds;
			int mins = (int)(totalSecs / 60);
			int secs = (int)(totalSecs % 60);
			
			if (secs > 0)
				return string.Format("{0}m{1:D2}", mins, secs);
			else
				return string.Format("{0}m", mins);
		}
		
		public static TimeSpan MetadataDurationToTimespan(string duration) {
			TimeSpan t;
			string[] numbers = duration.Split(new string[] { "m", "s" }, StringSplitOptions.RemoveEmptyEntries);
			
			if (numbers.Length == 2) {
				// minutes AND seconds expected (e.g. "12m51")
				// (also "12m51s", although I've yet to see this occur)
				t = new TimeSpan(0, int.Parse(numbers[0]), int.Parse(numbers[1]));
			} else {
				// minutes OR seconds OR milliseconds expected 
				// (e.g. "12m", "51,43s", "51,43 s", "209711")
				if (duration[duration.Length - 1] == 'm')
					t = TimeSpan.FromMinutes(double.Parse(numbers[0]));
				else if (duration[duration.Length - 1] == 's')
					t = TimeSpan.FromSeconds(double.Parse(numbers[0]));
				else // ms expcepted
					t = TimeSpan.FromMilliseconds(double.Parse(numbers[0]));
			}
			
			return t;
		}
	}
}

