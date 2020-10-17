using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;


namespace Grapher
{
    public sealed partial class EqGrapher : UserControl
    {
        private int _graphLt = 0;
        private int _graphTp = 0;
        private int _graphWd = 800;
        private int _graphHt = 600;
        private double _curZoom = 1.0;
        private double _aValue = 1.0;
        private int _nStep = 4;
        private Coords _coords;
        private readonly Parser _parser = new Parser();

        public EqGrapher()
        {
            this.InitializeComponent();

            SetInitialCoords();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            txtEquation.Text = "cos(x)+0.1x=sin(y)-0.1*y";
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
            }

            _nStep = 4;
            canvas.Invalidate();
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
                        _nStep = 6;
                        canvas.Invalidate();
                    }
                }
            }

            e.Handled = true;
        }

        private void ResetView(object sender, RoutedEventArgs e)
        {
            SetInitialCoords();
            _nStep = 4;
            canvas.Invalidate();
        }

        private void OnEquationTextChanged(object sender, TextChangedEventArgs e)
        {
            _nStep = 4;
            DrawEquation();
        }


        private void DrawEquation()
        {
            if (_parser == null)
                return;

            // Rewrite equation 
            var equation = txtEquation.Text;
            var parts = equation.Split("=");
            var s1 = $"({parts[0]})";
            if (parts.Length > 1)
                s1 += $"-({parts[1]})";

            _parser.SetVarVal('a', _aValue);
            _parser.NewParse(s1);

            canvas.Invalidate();
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

            DrawNewton(ds);
            _sw.Stop();
            txtTime.Text = $"Completed in {_sw.ElapsedMilliseconds / 1000.0} s";
        }


        private void DrawNewton(CanvasDrawingSession ds)
        {
            if (_parser?.RootNode == null)
                return;

            var width = _coords.Width;
            var height = _coords.Height;
            var xmin = _coords.XStart;
            var xmax = _coords.XEnd;
            var ymin = _coords.YStart;
            var ymax = _coords.YEnd;
            var xpixstep = (xmax - xmin) / width;
            var ypixstep = -(ymax - ymin) / height;

            var bitmap = new byte[width * height * 4]; // 4 bytes per pixel
            for (var i = 0; i < bitmap.Length; i++)
            {
                bitmap[i] = 0;
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
                                    SetPix(bitmap, xn, yn, (int)(255 * score));
                                }
                            }
                        }
                    }
                }
            }

            var target = new CanvasRenderTarget(ds, _coords.Width, _coords.Height);
            target.SetPixelBytes(bitmap);

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
            _parser.SetVarVal('x', xval);
            _parser.SetVarVal('y', yval);
            return _parser.GetVal();
        }


        private void SetPix(byte[] pix, int nx, int ny, int val)
        {
            if (nx < 0) return;
            if (nx >= _coords.Width) return;
            if (ny < 0) return;
            if (ny >= _coords.Height) return;
            var index = (ny * _coords.Width + nx) * 4;
            val = 100 + Math.Min(155, val);
            pix[index] = 100;
            pix[++index] = 0;
            pix[++index] = 0;
            pix[++index] = (byte)val; //intensity???
        }


        private void OnCreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {

        }


    }
}
