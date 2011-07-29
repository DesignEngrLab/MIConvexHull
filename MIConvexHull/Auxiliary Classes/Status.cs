/*************************************************************************
 *     This file & class is part of the MIConvexHull Library Project. 
 *     Copyright 2010 Matthew Ira Campbell, PhD.
 *
 *     MIConvexHull is free software: you can redistribute it and/or modify
 *     it under the terms of the GNU General Public License as published by
 *     the Free Software Foundation, either version 3 of the License, or
 *     (at your option) any later version.
 *  
 *     MIConvexHull is distributed in the hope that it will be useful,
 *     but WITHOUT ANY WARRANTY; without even the implied warranty of
 *     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *     GNU General Public License for more details.
 *  
 *     You should have received a copy of the GNU General Public License
 *     along with MIConvexHull.  If not, see <http://www.gnu.org/licenses/>.
 *     
 *     Please find further details and contact information on GraphSynth
 *     at http://miconvexhull.codeplex.com
 *************************************************************************/
namespace MIConvexHull
{
    public class StatusClass
    {
        readonly string[] MainTasks = new[]
        {
            "SetUp",
            "Finding_Extremes",
            "Defining Initial TernaryHull", 
            "Sorting Vertices into Initial Bins",
            "Refining Faces, Reducing Bins",
            "Creating Face Elements",
            "Creating Voronoi Graph Elements"
        };
        public int TaskNumber { get; set; }
        public int TotalTaskCount { get; set; }
        public int SubTaskNumber { get; set; }
        public int TotalSubTaskCount { get; set; }

        public override string ToString()
        {
            return "Task #" + TaskNumber + " of " + TotalTaskCount + " (" + MainTasks[TaskNumber]
                   + ")\n     SubTask #" + SubTaskNumber + " of " + TotalSubTaskCount + ".";
        }
    }

}
