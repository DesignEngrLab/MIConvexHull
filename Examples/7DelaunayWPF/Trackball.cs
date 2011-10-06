﻿//------------------------------------------------------------------
//
//  The following article discusses the mechanics behind this
//  trackball implementation: http://viewport3d.com/trackball.htm
//
//  Reading the article is not required to use this sample code,
//  but skimming it might be useful.
//
//  For licensing information and to get the latest version go to:
//  http://workspaces.gotdotnet.com/3dtools
//
//------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace Wpf3DTools
{
    /// <summary>
    ///     Trackball is a utility class which observes the mouse events
    ///     on a specified FrameworkElement and produces a Transform3D
    ///     with the resultant rotation and scale.
    /// 
    ///     Example Usage:
    /// 
    ///         Trackball trackball = new Trackball();
    ///         trackball.EventSource = myElement;
    ///         myViewport3D.Camera.Transform = trackball.Transform;
    /// 
    ///     Because Viewport3Ds only raise events when the mouse is over the
    ///     rendered 3D geometry (as opposed to not when the mouse is within
    ///     the layout bounds) you usually want to use another element as 
    ///     your EventSource.  For example, a transparent border placed on
    ///     top of your Viewport3D works well:
    ///     
    ///         <Grid>
    ///           <ColumnDefinition />
    ///           <RowDefinition />
    ///           <Viewport3D Name="myViewport" ClipToBounds="True" Grid.Row="0" Grid.Column="0" />
    ///           <Border Name="myElement" Background="Transparent" Grid.Row="0" Grid.Column="0" />
    ///         </Grid>
    ///     
    ///     NOTE: The Transform property may be shared by multiple Cameras
    ///           if you want to have auxiliary views following the trackball.
    /// 
    ///           It can also be useful to share the Transform property with
    ///           models in the scene that you want to move with the camera.
    ///           (For example, the Trackport3D's headlight is implemented
    ///           this way.)
    /// 
    ///           You may also use a Transform3DGroup to combine the
    ///           Transform property with additional Transforms.
    /// </summary>
    public class Trackball
    {
        private FrameworkElement _eventSource;
        private Point _previousPosition2D;
        private Vector3D _previousPosition3D = new Vector3D(0, 0, 1);

        private Transform3DGroup _transform;
        private RotateTransform3D _rotateTransform;
        private ScaleTransform3D _scale = new ScaleTransform3D();
        private AxisAngleRotation3D _rotation = new AxisAngleRotation3D();
        private TranslateTransform3D _translate = new TranslateTransform3D();
        
        private double _rotationFactor;
        private double _zoomFactor;

        public Trackball(double rotationFactor = 4.0, double zoomFacfor = 1.0)
        {
            _rotationFactor = rotationFactor;
            _zoomFactor = zoomFacfor;
            _transform = new Transform3DGroup();
            _transform.Children.Add(_scale);
            _rotateTransform = new RotateTransform3D(_rotation);
            _transform.Children.Add(_rotateTransform);
            _transform.Children.Add(_translate);
        }

        /// <summary>
        ///     A transform to move the camera or scene to the trackball's
        ///     current orientation and scale.
        /// </summary>
        public Transform3D Transform
        {
            get { return _transform; }
        }

        /// <summary>
        /// Rotation component of the transform
        /// </summary>
        public Transform3D RotateTransform
        {
            get { return _rotateTransform; }
        }

        #region Event Handling

        /// <summary>
        ///     The FrameworkElement we listen to for mouse events.
        /// </summary>
        public FrameworkElement EventSource
        {
            get { return _eventSource; }

            set
            {
                if (_eventSource != null)
                {
                    //_eventSource.MouseDown -= this.OnMouseDown;
                    //_eventSource.MouseUp -= this.OnMouseUp;
                    //_eventSource.MouseMove -= this.OnMouseMove;

                    _eventSource.PreviewMouseDown -= this.OnMouseDown;
                    _eventSource.PreviewMouseUp -= this.OnMouseUp;
                    _eventSource.PreviewMouseMove -= this.OnMouseMove;
                }

                _eventSource = value;

                _eventSource.PreviewMouseDown += this.OnMouseDown;
                _eventSource.PreviewMouseUp += this.OnMouseUp;
                _eventSource.PreviewMouseMove += this.OnMouseMove;
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            Mouse.Capture(EventSource, CaptureMode.SubTree);
            _previousPosition2D = e.GetPosition(EventSource);
            _previousPosition3D = ProjectToTrackball(
                EventSource.ActualWidth,
                EventSource.ActualHeight,
                _previousPosition2D);
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            Mouse.Capture(EventSource, CaptureMode.None);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            Point currentPosition = e.GetPosition(EventSource);

            if (e.LeftButton == MouseButtonState.Pressed && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                Pan(currentPosition);
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                Look(currentPosition);
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                Zoom(currentPosition);
            }

            _previousPosition2D = currentPosition;
        }

        #endregion Event Handling

        private void Look(Point currentPosition)
        {
            Vector3D currentPosition3D = ProjectToTrackball(
                EventSource.ActualWidth, EventSource.ActualHeight, currentPosition);

            if (_previousPosition3D.Equals(currentPosition3D)) return;
            
            Vector3D axis = Vector3D.CrossProduct(_previousPosition3D, currentPosition3D);            

            double angle = _rotationFactor * Vector3D.AngleBetween(_previousPosition3D, currentPosition3D);
            Quaternion delta = new Quaternion(axis, -angle);

            // Get the current orientation from the RotateTransform3D
            AxisAngleRotation3D r = _rotation;
            Quaternion q = new Quaternion(_rotation.Axis, _rotation.Angle);

            // Compose the delta with the previous orientation
            q *= delta;

            // Write the new orientation back to the Rotation3D
            _rotation.Axis = q.Axis;
            _rotation.Angle = q.Angle;

            _previousPosition3D = currentPosition3D;
        }

        private void Pan(Point currentPosition)
        {
            Vector3D currentPosition3D = ProjectToTrackball(
                EventSource.ActualWidth, EventSource.ActualHeight, currentPosition);

            Vector change = Point.Subtract(_previousPosition2D, currentPosition);

            Vector3D changeVector = new Vector3D(change.X, change.Y, 0);

            _translate.OffsetX += changeVector.X * .1;
            _translate.OffsetY -= changeVector.Y * .1;
            _translate.OffsetZ += changeVector.Z * .1;

            _previousPosition3D = currentPosition3D;
        }

        private Vector3D ProjectToTrackball(double width, double height, Point point)
        {
            double x = point.X / (width / 2);    // Scale so bounds map to [0,0] - [2,2]
            double y = point.Y / (height / 2);

            x = x - 1;                           // Translate 0,0 to the center
            y = 1 - y;                           // Flip so +Y is up instead of down

            double z2 = 1 - x * x - y * y;       // z^2 = 1 - x^2 - y^2
            double z = z2 > 0 ? Math.Sqrt(z2) : 0;

            return new Vector3D(x, y, z);
        }

        private void Zoom(Point currentPosition)
        {
            double yDelta = currentPosition.Y - _previousPosition2D.Y;

            double scale = _zoomFactor * Math.Exp(-yDelta / 100);    // e^(yDelta/100) is fairly arbitrary.

            _scale.ScaleX *= scale;
            _scale.ScaleY *= scale;
            _scale.ScaleZ *= scale;
        }
    }
}
