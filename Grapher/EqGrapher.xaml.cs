using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace Grapher
{
    public sealed partial class EqGrapher : UserControl
    {
        private double _w = 470;
        private double _h = 520;
        private double _graphLt = 0;
        private double _graphTp = 0;
        private double _graphWd = 880;
        private double _graphHt = 660;
        private readonly SolidColorBrush _darkblue;
        private readonly SolidColorBrush _orange;
        private double _curZoom = 1.0;
        private bool _polarQ = false;
        private double _aValue = 1.0;
        private Coords _coords;
        private readonly Parser _parser = new Parser();

        public EqGrapher()
        {
            this.InitializeComponent();

            _darkblue = new SolidColorBrush(Colors.DarkBlue);
            _orange = new SolidColorBrush(Colors.Orange);
            SetInitialCoords();
        }

        private void SetInitialCoords()
        {
            _coords = new Coords(_graphLt, _graphTp, _graphWd, _graphHt, -5, -3, 5, 3, true);
        }

        private void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var ds = args.DrawingSession;
   
            var graph = new Graph(ds, _coords);
            graph.DrawGraph();
        }

        public void Test()
        {
            canvas.Invalidate();
        }

        private void RunTest(object sender, RoutedEventArgs e)
        {
            Test();
        }

        private void OnCreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            
        }

        private void OnZoomChange(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_coords == null)
                return;
   
            var scaleBy = (e.NewValue * 2 + 0.01) / _curZoom;
            _coords.Scale(scaleBy);
            _curZoom *= scaleBy;
        
            canvas.Invalidate();
        }

        private void OnZoomFocusLost(object sender, RoutedEventArgs e)
        {
            _curZoom = 1.0;
            zoomSlider.Value = 0.5;
        }

        private Point _prevPoint;
        private Point _downPoint;
        private bool _dragging;

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _prevPoint = e.GetCurrentPoint(canvas).Position;
            _downPoint = _prevPoint;
            _dragging = true;
            e.Handled = true;
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            if (_downPoint == null)
                return;

            // if not moving too much, then choose new center
            var pt = e.GetCurrentPoint(canvas);
            var xdiff = _downPoint.X - pt.Position.X;
            var ydiff = -(_downPoint.Y - pt.Position.Y);
            if (Math.Abs(xdiff) < 2 && Math.Abs(ydiff) < 2)
            {
                _coords.NewCenter(pt.Position.X, pt.Position.Y);
                canvas.Invalidate();
            }
            _dragging = false;
        }

    
        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if(_dragging)
            {
                var pt = e.GetCurrentPoint(canvas);
                if(pt.Properties.IsLeftButtonPressed)
                {
                    var xdiff = _prevPoint.X - pt.Position.X;
                    var ydiff = -(_prevPoint.Y - pt.Position.Y);

                    if (Math.Abs(xdiff) > 1 && Math.Abs(ydiff) > 1)
                    {
                        _prevPoint = pt.Position;
                        _coords.Drag(xdiff, ydiff);
                        canvas.Invalidate();
                    }
                }
            }

            e.Handled = true;
        }

        private void ResetView(object sender, RoutedEventArgs e)
        {
            SetInitialCoords();
            canvas.Invalidate();
        }
    }
}
