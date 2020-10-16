using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;


namespace Grapher
{
    public sealed partial class EqGrapher : UserControl
    {
        private double _w = 470;
        private double _h = 520;
        private int _graphLt = 0;
        private int _graphTp = 0;
        private int _graphWd = 440;
        private int _graphHt = 330;
        private readonly SolidColorBrush _darkblue;
        private readonly SolidColorBrush _orange;
        private double _curZoom = 1.0;
        private bool _polarQ = false;
        private double _aValue = 1.0;
        private int _nStep = 4;
        private double _prevAngle;
        private Coords _coords;
        private readonly Parser _parser = new Parser();

        private string _equation = "";

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

        private readonly Stopwatch _sw = new Stopwatch();
        private void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var ds = args.DrawingSession;

            _sw.Restart();
            var graph = new Graph(ds, _coords);
            graph.DrawGraph();

            var parts = _equation.Split("=");
            var s1 = $"({parts[0]})";
            if (parts.Length > 1)
                s1 += $"-({parts[1]})";
            //_parser.RadiansQ = true;
            _parser.SetVarVal("a", _aValue);
            _parser.NewParse(s1);

            if (_parser.RootNode.IsLeaf)
                return;

            if (_polarQ)
            {
                DrawPlot(ds, "polar");
            }
            else
            {
                DrawPlot(ds, "implicit");
            }
            _sw.Stop();
            txtTime.Text = $"Completed in {_sw.ElapsedMilliseconds / 1000.0} s";
        }

        private void DrawPlot(CanvasDrawingSession ds, string plotType)
        {
            switch (plotType)
            {
                case "implicit":
                    DrawNewton(ds);
                    return;
                   

                case "signchange":
                    DrawImplicitSign(2, plotType);
                    break;

                case "shaded":
                    DrawShaded(2, plotType);
                    break;
            }

            var breakQ = true;
            double prevxPix = -1;
            double prevyPix = -1;
            var yState = 9;
            int ptNumMin;
            int ptNumMax = 0;
            var stepSize = 0.0;

            // determine ptNumMin, ptNumMax, and stepSize
            switch (plotType)
            {
                case "xy":
                case "dydx":
                    ptNumMax = (int)_coords.Width;
                    break;

                case "polar":
                    var revFrom = 0;
                    var revTo = 4;
                    var angle0 = revFrom * 6.2832;
                    var angle1 = revTo * 6.2832;
                    var points = Math.Min(Math.Max(600, (revTo - revFrom) * 250), 3000);
                    stepSize = (angle1 - angle0) / points;
                    if (stepSize == 0) stepSize = 1;
                    ptNumMin = (int)Math.Floor(angle0 / stepSize);
                    ptNumMax = (int)Math.Ceiling(angle1 / stepSize);
                    break;

                case "para":
                    var tFrom = 0;
                    var tTo = 3;
                    points = Math.Min(Math.Max(600, (tTo - tFrom) * 250), 3000);
                    stepSize = (tTo - tFrom) / points;
                    if (stepSize == 0) stepSize = 1;
                    ptNumMin = (int)Math.Floor(tFrom / stepSize);
                    ptNumMax = (int)Math.Ceiling(tTo / stepSize);
                    break;
            }

            var line = new List<Pt>();
            var prevxVal = double.NegativeInfinity;
            var prevyVal = double.NegativeInfinity;
            var recipdx = 1 / _coords.XScale;
            for (var ptNum = 0; ptNum <= ptNumMax; ptNum++)
            {
                double xVal = 0;
                double yVal = 0;
                double xPix = 0;
                double yPix = 0;
                switch (plotType)
                {
                    case "xy":
                        xPix = ptNum;
                        xVal = _coords.XStart + xPix * _coords.XScale;
                        _parser.SetVarVal("x", xVal);
                        yVal = _parser.GetVal();
                        yPix = _coords.ToYPix(yVal);
                        break;
                    case "dydx":
                        xPix = ptNum;
                        xVal = _coords.XStart + xPix * _coords.XScale;
                        _parser.SetVarVal("x", xVal);
                        var thisyVal = _parser.GetVal();
                        yVal = (thisyVal - prevyVal) * recipdx;
                        prevyVal = thisyVal;
                        break;
                    case "polar":
                        var angle = ptNum * stepSize;
                        _parser.SetVarVal("x", angle);
                        var radius = _parser.GetVal();
                        xVal = radius * Math.Cos(angle);
                        yVal = radius * Math.Sin(angle);
                        break;
                    case "para":
                        // TODO:
                        //xPix = ptNum;
                        //var t = _coords.XStart + xPix * _coords.XScale;
                        //var xy = ConicVals(t);
                        //xVal = xy[0];
                        //yVal = xy[1];
                        //if (_vals["b"] > 1.1)
                        //{
                        //    if (yVal < -0.95 && yVal > -1.05)
                        //    {
                        //        pt1[0] = xy[0];
                        //        pt1[1] = xy[1];
                        //    }
                        //}
                        //else
                        //{
                        //    if (yVal < -this.vals["b"] * 0.7 && yVal > -this.vals["b"] * 0.8)
                        //    {
                        //        pt1[0] = xy[0];
                        //        pt1[1] = xy[1];
                        //    }
                        //}
                        break;
                }

                var prevbreakQ = breakQ;
                breakQ = false;
                var prevyState = yState;
               yState = 0;
                if (yVal < _coords.YStart) yState = -1;
                if (yVal > _coords.YEnd) yState = 1;
                if (yVal == double.NegativeInfinity)
                {
                    yState = -1;
                    yVal = _coords.YStart - _coords.YScale * 10;
                }
                if (yVal == double.PositiveInfinity)
                {
                    yState = 1;
                    yVal = _coords.YEnd + _coords.YScale * 10;
                }

                breakQ = prevyState * yState != 0;
                if (double.IsNaN(yVal))
                {
                    yState = 9;
                    breakQ = true;
                }
                if (plotType == "polar" || plotType == "para")
                {
                    xVal = Math.Min(
                      Math.Max(_coords.XStart - _coords.XScale * 10, xVal),
                      _coords.XEnd + _coords.XScale * 10
                    );
                    xPix = (xVal - _coords.XStart) / _coords.XScale;
                }
                yVal = Math.Min(
                  Math.Max(_coords.YStart - _coords.YScale * 10, yVal),
                  _coords.YEnd + _coords.YScale * 10
                );
                yPix = (_coords.YEnd - yVal) / _coords.YScale;
                if (breakQ)
                {
                    if (prevbreakQ)
                    {
                    }
                    else
                    {
                        if (yState < 9)
                        {
                            line.Add(new Pt(xPix, yPix));
                        }

                    }
                }
                else
                {
                    if (prevbreakQ)
                    {
                        if (prevyState < 9)
                        {
                            line.Add(null);
                            line.Add(new Pt(prevxPix, prevyPix));
                            line.Add(new Pt(xPix, yPix));
                        }
                        else
                        {
                            line.Add(null);
                            line.Add(new Pt(xPix, yPix));
                        }
                    }
                    else
                    {
                        line.Add(new Pt(xPix, yPix));
                    }
                }
                prevxVal = xVal;
                prevxPix = xPix;
                prevyPix = yPix;
            }

            var sttQ = true;
            var xstart = 0.0;
            var ystart = 0.0;
            var xend = 0.0;
            var yend = 0.0;
            for (var i = 0; i < line.Count; i++)
            {
                var pt = line[i];
                if (pt == null)
                {
                    sttQ = true;
                    ds.DrawLine((float)xstart, (float)ystart, (float)xend, (float)yend, Colors.Black);
                }
                else
                {
                    if (sttQ)
                    {
                        xstart = pt.X;
                        ystart = pt.Y;
                        sttQ = false;
                    }
                    else
                    {
                        xend = pt.X;
                        yend = pt.Y;
                    }
                }
            }
            ds.DrawLine((float)xstart, (float)ystart, (float)xend, (float)yend, Colors.Black);
        }


        private object ConicVals(double t)
        {
            throw new NotImplementedException();
        }

        private void DrawShaded(int v, string plotType)
        {
            throw new NotImplementedException();
        }

        private void DrawImplicitSign(int v, string plotType)
        {
            throw new NotImplementedException();
        }

        private void DrawNewton(CanvasDrawingSession ds)
        {
            var width = _coords.Width;
            var height = _coords.Height;
            var xmin = _coords.XStart;
            var xmax = _coords.XEnd;
            var ymin = _coords.YStart;
            var ymax = _coords.YEnd;
            var xpixstep = (xmax - xmin) / width;
            var ypixstep = -(ymax - ymin) / height;
            var pix = new byte[width * height * 4]; // 4 bytes per pixe
            for (var i = 0; i < pix.Length; i++)
            {
                pix[i] = 0;
            }
            var toler = xpixstep * xpixstep * 0.1;
            var xGridStep = xpixstep * _nStep;
            var yGridStep = ypixstep * _nStep;
            for (var x1 = xmin; x1 < xmax; x1 += xGridStep)
            {
                for (var y1 = ymin; y1 < ymax; y1 -= yGridStep)
                {
                    double score;
                    if (_nStep > 1)
                    {
                        var widefact = 1;
                        score = GetNewtonScore(
                          x1 - xpixstep * widefact,
                          y1 + ypixstep * widefact,
                          x1 + xGridStep + xpixstep * widefact,
                          y1 - yGridStep - ypixstep * widefact,
                          x1 + xGridStep / 2,
                          y1 - yGridStep / 2,
                          10,
                          toler * 10
                        );
                    }
                    else
                    {
                        score = 1;
                    }

                    if (score > 0)
                    {
                        for (int i = 0; i < _nStep; i++)
                        {
                            for (var j = 0; j < _nStep; j++)
                            {
                                var x2 = x1 + i * xpixstep;
                                var y2 = y1 - j * ypixstep;
                                score = GetNewtonScore(
                                  x2,
                                  y2,
                                  x2 + xpixstep,
                                  y2 - ypixstep,
                                  x2 + xpixstep / 2,
                                  y2 - ypixstep / 2,
                                  5,
                                  toler
                                );
                                if (score > 0)
                                {
                                    var xn = (int)_coords.ToXPix(x2 + xpixstep / 2);
                                    var yn = (int)_coords.ToYPix(y2 - ypixstep / 2);
                                    var clrDensity = 255 * score;
                                    var pts = new (int a, int b, int c)[] { (0, 0, 1) };
                                    foreach (var (a, b, c) in pts)
                                    {
                                        SetPix(pix, xn + a, yn + b, (int)(clrDensity * c));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var target = CanvasBitmap.CreateFromBytes(ds, pix, _coords.Width, _coords.Height, Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized);
            ds.DrawImage(target);
        }

        private double GetNewtonScore(double a, double b, double c, double d, double x0, double y0, int count, double toler)
        {
            double delta;
            for (var j = 0; j < count; j++)
            {
                var z = fOf(x0, y0);
                if (double.IsNaN(z))
                {
                    return 0;
                }
                if (Math.Abs(z) < toler)
                {
                    var fromCtr = Dist((a + c) / 2 - x0, (b + d) / 2 - y0);
                    var diag = Dist(c - a, b - d);
                    var fromEdge = diag / 2 - fromCtr;
                    var ctrWt = fromEdge / (diag / 2);
                    return ctrWt;
                }
                var dFact = 0.00001;
                delta = Math.Abs(c - a) * dFact;
                var f1 = f1Of(x0, y0, delta);
                if (double.IsNaN(f1))
                {
                    return 0;
                }
                delta = Math.Abs(d - b) * dFact;
                var f2 = f2Of(x0, y0, delta);
                if (double.IsNaN(f2))
                {
                    return 0;
                }
                var norm = f1 * f1 + f2 * f2;
                if (norm < toler)
                {
                    return 0;
                }
                x0 -= (f1 * z) / norm;
                y0 -= (f2 * z) / norm;

                if (x0 < a || x0 > c || y0 < b || y0 > d)
                {
                    return 0;
                }
            }
            return 0;
        }

        private double f2Of(double xPos, double yPos, double delta)
        {
            return (fOf(xPos, yPos + delta) - fOf(xPos, yPos - delta)) / (2 * delta);

        }

        private double f1Of(double xPos, double yPos, double delta)
        {
            return (fOf(xPos + delta, yPos) - fOf(xPos - delta, yPos)) / (2 * delta);
        }

        private double Dist(double dx, double dy)
        {
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private double fOf(double xval, double yval)
        {
            _parser.SetVarVal("x", xval);
            _parser.SetVarVal("y", yval);
            return _parser.GetVal();
        }

        
        private void SetPix(byte[] pix, int nx, int ny,  int val)
        {
            if (nx < 0) return;
            if (nx >= _coords.Width) return;
            if (ny < 0) return;
            if (ny >= _coords.Height) return;
            var index = ((ny >> 0) * _coords.Width + (nx >> 0)) * 4;
            val = 55 + Math.Min(200, val >> 0);
            pix[index] = 255; 
            pix[++index] = 0; 
            pix[++index] = 0; 
            pix[++index] = (byte)val; //intensity???
        }

        private void RunTest(object sender, RoutedEventArgs e)
        {
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
            if (_dragging)
            {
                var pt = e.GetCurrentPoint(canvas);
                if (pt.Properties.IsLeftButtonPressed)
                {
                    var xdiff = _prevPoint.X - pt.Position.X;
                    var ydiff = -(_prevPoint.Y - pt.Position.Y);

                    if (Math.Abs(xdiff) >= 2 && Math.Abs(ydiff) >= 2)
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

        private void OnEquationTextChanged(object sender, TextChangedEventArgs e)
        {
            _equation = txtEquation.Text;
            canvas.Invalidate();
        }
    }
}
