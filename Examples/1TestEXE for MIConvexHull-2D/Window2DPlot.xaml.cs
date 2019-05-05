// ***********************************************************************
// Assembly         : TVGL Presenter
// Author           : Matt
// Created          : 05-20-2016
//
// Last Modified By : Matt
// Last Modified On : 05-24-2016
// ***********************************************************************
// <copyright file="MainWindow.xaml.cs" company="OxyPlot">
//     The MIT License (MIT)
/*
  Copyright(c) 2014 OxyPlot contributors


  Permission is hereby granted, free of charge, to any person obtaining a
  copy of this software and associated documentation files (the
  "Software"), to deal in the Software without restriction, including
  without limitation the rights to use, copy, modify, merge, publish,
  distribute, sublicense, and/or sell copies of the Software, and to
  permit persons to whom the Software is furnished to do so, subject to
  the following conditions:
  
  The above copyright notice and this permission notice shall be included
  in all copies or substantial portions of the Software.
  
  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
  OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
  MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
  IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
  CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
  TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
  SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using OxyPlot;
using OxyPlot.Series;
using TestEXE_for_MIConvexHull2D;
using DataPointSeries = OxyPlot.Wpf.DataPointSeries;

namespace TVGL
{
    /// <summary>
    ///     Class Window2DPlot.
    /// </summary>
    public partial class Window2DPlot : Window
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Window2DPlot" /> class.
        /// </summary>
        /// <param name="title">The title.</param>
        private Window2DPlot(string title)
        {
            Title = title;
            Model = new PlotModel();
        }


        public Window2DPlot(IEnumerable<Vertex[]> listOfArrayOfPoints, string title, Plot2DType plot2DType, bool closeShape,
            MarkerType marker) : this(title)
        {
            foreach (var points in listOfArrayOfPoints)
            {
                if (plot2DType == Plot2DType.Line)
                    AddLineSeriesToModel(points, closeShape, marker);
                else
                    AddScatterSeriesToModel(points, marker);
            }
            InitializeComponent();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Window2DPlot" /> class.
        /// </summary>
        /// <param name="listOfSeriesOfPoints">The list of array of points.</param>
        /// <param name="title">The title.</param>
        /// <param name="plot2DType">Type of the plot2 d.</param>
        /// <param name="closeShape">if set to <c>true</c> [close shape].</param>
        /// <param name="marker">The marker.</param>
        public Window2DPlot(IList<List<double[]>> listOfSeriesOfPoints, string title, Plot2DType plot2DType, bool closeShape,
            MarkerType marker, IList<TVGL.Color> colors = null) : this(title)
        {
            for (var i = 0; i < listOfSeriesOfPoints.Count(); i++ )
            {
                //Note: both methods below will accept null colors, so set to null by default
                TVGL.Color color = null;
                if(colors != null)
                {
                    color = colors[i];
                }

                if (plot2DType == Plot2DType.Line)
                    AddLineSeriesToModel(listOfSeriesOfPoints[i], closeShape, marker, color);
                else
                    AddScatterSeriesToModel(listOfSeriesOfPoints[i], marker, color);
            }
            InitializeComponent();
        }


        /// <summary>
        ///     Initializes a new instance of the <see cref="Window2DPlot" /> class.
        /// </summary>
        /// <param name="listOfListOfPoints">The list of list of points.</param>
        /// <param name="title">The title.</param>
        /// <param name="plot2DType">Type of the plot2 d.</param>
        /// <param name="closeShape">if set to <c>true</c> [close shape].</param>
        /// <param name="marker">The marker.</param>
        public Window2DPlot(IEnumerable<List<Vertex>> listOfListOfPoints, string title, Plot2DType plot2DType, bool closeShape,
            MarkerType marker) : this(title)
        {
            foreach (var points in listOfListOfPoints)
            {
                if (plot2DType == Plot2DType.Line)
                    AddLineSeriesToModel(points, closeShape, marker);
                else
                    AddScatterSeriesToModel(points, marker);
            }
            InitializeComponent();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Window2DPlot" /> class.
        ///     This version allows different markers to be set for each set of polygons.
        /// </summary>
        /// <param name="listOfListOfPoints2"></param>
        /// <param name="title">The title.</param>
        /// <param name="plot2DType">Type of the plot2 d.</param>
        /// <param name="closeShape">if set to <c>true</c> [close shape].</param>
        /// <param name="listOfListOfPoints1"></param>
        /// <param name="marker1"></param>
        /// <param name="marker2"></param>
        public Window2DPlot(IEnumerable<List<Vertex>> listOfListOfPoints1, IEnumerable<List<Vertex>> listOfListOfPoints2, 
            string title, Plot2DType plot2DType, bool closeShape,
            MarkerType marker1, MarkerType marker2) : this(title)
        {
            foreach (var points in listOfListOfPoints1)
            {
                if (plot2DType == Plot2DType.Line)
                    AddLineSeriesToModel(points, closeShape, marker1);
                else
                    AddScatterSeriesToModel(points, marker1);
            }
            foreach (var points in listOfListOfPoints2)
            {
                if (plot2DType == Plot2DType.Line)
                    AddLineSeriesToModel(points, closeShape, marker2);
                else
                    AddScatterSeriesToModel(points, marker2);
            }
            InitializeComponent();
        }

        public Window2DPlot(IEnumerable<List<List<Vertex>>> listofListOfListOfPoints, string title, Plot2DType plot2DType, bool closeShape,
            MarkerType marker) : this(title)
        {
            var i = 0;
            
            var colorPalet = Presenter.ColorPalette();
            var maxLength = colorPalet.Length;
            foreach (var listOfListOfPoints in listofListOfListOfPoints)
            {
                if (plot2DType == Plot2DType.Line)
                {
                    //Set each list of points as its own color.
                    //Close each list of points 
                    foreach (var points in listOfListOfPoints)
                    {
                        var color = new Color(colorPalet[i]);
                        i++;
                        if (i == maxLength) i = 0;
                        var series = new LineSeries
                        {
                            MarkerType = marker,
                            Color = OxyColor.FromRgb(color.R, color.G, color.B)
                        };
                        foreach (var point in points)
                        {
                            series.Points.Add(new DataPoint(point.X, point.Y));
                        }
                        if (closeShape) series.Points.Add(new DataPoint(points[0].X, points[0].Y));
                        Model.Series.Add(series);
                    }
                }
                else
                {
                    foreach (var points in listOfListOfPoints)
                    {
                        AddScatterSeriesToModel(points, marker);
                    }
                }
            }

            InitializeComponent();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Window2DPlot" /> class.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="title">The title.</param>
        /// <param name="plot2DType">Type of the plot2 d.</param>
        /// <param name="closeShape">if set to <c>true</c> [close shape].</param>
        /// <param name="marker">The marker.</param>
        public Window2DPlot(IList<double[]> points, string title, Plot2DType plot2DType, bool closeShape, MarkerType marker)
            : this(title)
        {
            if (plot2DType == Plot2DType.Line)
                AddLineSeriesToModel(points, closeShape, marker);
            else
                AddScatterSeriesToModel(points, marker);
            InitializeComponent();
        }

        /// <summary>
        ///     Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        public PlotModel Model { get; set; }

        /// <summary>
        ///     Adds the line series to model.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="closeShape">if set to <c>true</c> [close shape].</param>
        /// <param name="marker">The marker.</param>
        private void AddLineSeriesToModel(IList<Vertex> points, bool closeShape, MarkerType marker, TVGL.Color color = null)
        {
            AddLineSeriesToModel(PointsToDouble(points), closeShape, marker, color);
        }

        /// <summary>
        ///     Adds the line series to model.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="closeShape">if set to <c>true</c> [close shape].</param>
        /// <param name="marker">The marker.</param>
        private void AddLineSeriesToModel(IList<double[]> points, bool closeShape, MarkerType marker, TVGL.Color color = null)
        {
            var series = new LineSeries { MarkerType = marker };

            //Add color to series if applicable
            if (color != null)
            {
                series.Color = OxyColor.FromRgb(color.R, color.G, color.B);
            }
       
            foreach (var point in points)
                //point[0] == x, point[1] == y
                series.Points.Add(new DataPoint(point[0], point[1]));
            if (closeShape) series.Points.Add(new DataPoint(points[0][0], points[0][1]));
            Model.Series.Add(series);
        }

        /// <summary>
        ///     Adds the scatter series to model.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="marker">The marker.</param>
        private void AddScatterSeriesToModel(IList<Vertex> points, MarkerType marker, TVGL.Color color = null)
        {
            AddScatterSeriesToModel(PointsToDouble(points), marker, color);
        }

        /// <summary>
        ///     Adds the scatter series to model.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="marker">The marker.</param>
        private void AddScatterSeriesToModel(IList<double[]> points, MarkerType marker, TVGL.Color color = null)
        {
            var series = new LineSeries
            {
                MarkerType = marker,
                LineStyle = LineStyle.None
            };
            //Add color to series if applicable
            if (color != null)
            {
                series.Color = OxyColor.FromRgb(color.R, color.G, color.B);
            }

            foreach (var point in points)
                //point[0] == x, point[1] == y
                series.Points.Add(new DataPoint(point[0], point[1]));
            Model.Series.Add(series);
        }

        private List<double[]> PointsToDouble(IList<Vertex> points)
        {
            var doubleArray = new List<double[]>();
            for(var i = 0; i < points.Count(); i++)
            {
                doubleArray.Add(points[i].Position);
            }
            return doubleArray;
        }

        
    }
}