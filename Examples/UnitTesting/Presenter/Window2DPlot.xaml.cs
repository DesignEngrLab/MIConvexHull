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

namespace TVGL
{
    /// <summary>
    ///     Enum Plot2DType
    /// </summary>
    public enum Plot2DType
    {
        /// <summary>
        ///     The line
        /// </summary>
        Line,

        /// <summary>
        ///     The points
        /// </summary>
        Points
    }

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

        /// <summary>
        ///     Initializes a new instance of the <see cref="Window2DPlot" /> class.
        /// </summary>
        /// <param name="listOfArrayOfPoints">The list of array of points.</param>
        /// <param name="title">The title.</param>
        /// <param name="plot2DType">Type of the plot2 d.</param>
        /// <param name="closeShape">if set to <c>true</c> [close shape].</param>
        /// <param name="marker">The marker.</param>
        public Window2DPlot(IEnumerable<Point[]> listOfArrayOfPoints, string title, Plot2DType plot2DType, bool closeShape,
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
        /// <param name="listOfListOfPoints">The list of list of points.</param>
        /// <param name="title">The title.</param>
        /// <param name="plot2DType">Type of the plot2 d.</param>
        /// <param name="closeShape">if set to <c>true</c> [close shape].</param>
        /// <param name="marker">The marker.</param>
        public Window2DPlot(IEnumerable<List<Point>> listOfListOfPoints, string title, Plot2DType plot2DType, bool closeShape,
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

        public Window2DPlot(IEnumerable<List<List<Point>>> listofListOfListOfPoints, string title, Plot2DType plot2DType, bool closeShape,
            MarkerType marker) : this(title)
        {
            var i = 0;
            
            var colorPalet = ColorPalet();
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
        public Window2DPlot(IList<Point> points, string title, Plot2DType plot2DType, bool closeShape, MarkerType marker)
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
        private void AddLineSeriesToModel(IList<Point> points, bool closeShape, MarkerType marker)
        {
            var series = new LineSeries {MarkerType = marker};
            foreach (var point in points)
                series.Points.Add(new DataPoint(point.X, point.Y));
            if (closeShape) series.Points.Add(new DataPoint(points[0].X, points[0].Y));
            Model.Series.Add(series);
        }

        /// <summary>
        ///     Adds the scatter series to model.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="marker">The marker.</param>
        private void AddScatterSeriesToModel(IEnumerable<Point> points, MarkerType marker)
        {
            var series = new LineSeries {MarkerType = marker};
            foreach (var point in points)
                series.Points.Add(new DataPoint(point.X, point.Y));
            Model.Series.Add(series);
        }

        //A palet of distinguishable colors
        //http://graphicdesign.stackexchange.com/questions/3682/where-can-i-find-a-large-palette-set-of-contrasting-colors-for-coloring-many-d
        private string[] ColorPalet()
        {
            return new[]
            {
                "#000000",
                "#1CE6FF",
                "#FF34FF",
                "#FF4A46",
                "#008941",
                "#006FA6",
                "#A30059",
                "#FFDBE5",
                "#7A4900",
                "#0000A6",
                "#63FFAC",
                "#B79762",
                "#004D43",
                "#8FB0FF",
                "#997D87",
                "#5A0007",
                "#809693",
                "#FEFFE6",
                "#1B4400",
                "#4FC601",
                "#3B5DFF",
                "#4A3B53",
                "#FF2F80",
                "#61615A",
                "#BA0900",
                "#6B7900",
                "#00C2A0",
                "#FFAA92",
                "#FF90C9",
                "#B903AA",
                "#D16100",
                "#DDEFFF",
                "#000035",
                "#7B4F4B",
                "#A1C299",
                "#300018",
                "#0AA6D8",
                "#013349",
                "#00846F",
                "#372101",
                "#FFB500",
                "#C2FFED",
                "#A079BF",
                "#CC0744",
                "#C0B9B2",
                "#C2FF99",
                "#001E09",
                "#00489C",
                "#6F0062",
                "#0CBD66",
                "#EEC3FF",
                "#456D75",
                "#B77B68",
                "#7A87A1",
                "#788D66",
                "#885578",
                "#FAD09F",
                "#FF8A9A",
                "#D157A0",
                "#BEC459",
                "#456648",
                "#0086ED",
                "#886F4C",
                "#34362D",
                "#B4A8BD",
                "#00A6AA",
                "#452C2C",
                "#636375",
                "#A3C8C9",
                "#FF913F",
                "#938A81",
                "#575329",
                "#00FECF",
                "#B05B6F",
                "#8CD0FF",
                "#3B9700",
                "#04F757",
                "#C8A1A1",
                "#1E6E00",
                "#7900D7",
                "#A77500",
                "#6367A9",
                "#A05837",
                "#6B002C",
                "#772600",
                "#D790FF",
                "#9B9700",
                "#549E79",
                "#FFF69F",
                "#201625",
                "#72418F",
                "#BC23FF",
                "#99ADC0",
                "#3A2465",
                "#922329",
                "#5B4534",
                "#FDE8DC",
                "#404E55",
                "#0089A3",
                "#CB7E98",
                "#A4E804",
                "#324E72",
                "#6A3A4C"
            };
        }
    }
}