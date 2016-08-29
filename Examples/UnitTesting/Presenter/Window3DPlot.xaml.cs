// ***********************************************************************
// Assembly         : TVGL Presenter
// Author           : Matt
// Created          : 05-20-2016
//
// Last Modified By : Matt
// Last Modified On : 05-18-2016
// ***********************************************************************
// <copyright file="Window3DPlot.xaml.cs" company="">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Windows;


namespace Presenter
{
    /// <summary>
    ///     Class Window3DPlot.
    /// </summary>
    public partial class Window3DPlot : Window
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Window3DPlot" /> class.
        /// </summary>
        public Window3DPlot()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Handles the OnChecked event of the GridLines control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void GridLines_OnChecked(object sender, RoutedEventArgs e)
        {
            GridLines.Visible = true;
        }

        /// <summary>
        ///     Handles the OnUnChecked event of the GridLines control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void GridLines_OnUnChecked(object sender, RoutedEventArgs e)
        {
            GridLines.Visible = false;
        }
    }
}