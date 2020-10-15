using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Windows.UI;
using Windows.UI.Text;

namespace Grapher
{
    internal class Graph
    {
        private readonly CanvasDrawingSession _ds;
        private readonly Coords _coords;
        private bool _xLinesQ = true;
        private bool _yLinesQ = true;
        private bool _xValsQ = true;
        private bool _yValsQ = true;
        private bool _skewQ = false;

        private double _hzAxisY;
        private double _hzNumsY;
        private double _vtAxisX;
        private double _vtNumsX;

        public Graph(CanvasDrawingSession ds, Coords coords)
        {
            _ds = ds;
            _coords = coords;
        }

        public void DrawGraph()
        {
            _hzAxisY = _coords.ToYPix(0);
            if (_hzAxisY < 0) _hzAxisY = 0;
            if (_hzAxisY > _coords.Height) _hzAxisY = _coords.Height;
            _hzNumsY = _hzAxisY + 14;
            if (_hzAxisY > _coords.Height - 10) _hzNumsY = _coords.Height - 3;
            _vtAxisX = _coords.ToXPix(0);
            if (_vtAxisX < 0) _vtAxisX = 0;
            if (_vtAxisX > _coords.Width) _vtAxisX = _coords.Width;
            _vtNumsX = _vtAxisX - 5;
            if (_vtAxisX < 10) _vtNumsX = 20;
            if (_coords.UseXLog)
            {
                DrawLinesLogX();
            }
            else
            {
                if (_xLinesQ)
                {
                    DrawHzLines();
                }
            }
            if (_coords.UseYLog)
            {
                DrawLinesLogY();
            }
            else
            {
                if (_yLinesQ)
                {
                    DrawVtLines();
                }
            }
        }

        private void DrawLinesLogX()
        {
        }

        private void DrawHzLines()
        {
            var ticks = _coords.GetTicks(_coords.YStart, _coords.YEnd - _coords.YStart, 4);
            var tickFontFormat = new CanvasTextFormat
            {
                FontFamily = "Verdana",
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                VerticalAlignment = CanvasVerticalAlignment.Center,
                HorizontalAlignment = CanvasHorizontalAlignment.Right
            };

            foreach (var tick in ticks)
            {
                var yVal = tick.Val;
                var tickLevel = tick.Level;

                var strokeStyle = tickLevel == 0
                    ? Color.FromArgb(80, 0, 0, 255)
                    : Color.FromArgb(20, 0, 0, 255);

                var yPix = _coords.ToYPix(yVal);
                _ds.DrawLine(
                     (float)_coords.ToXPix(_coords.XStart), (float)yPix,
                     (float)_coords.ToXPix(_coords.XEnd), (float)yPix,
                    strokeStyle, strokeWidth: 1.0f);

                if (tickLevel == 0 && _yValsQ)
                {
                    _ds.DrawText(yVal.ToString(), (float)_vtNumsX, (float)yPix, Colors.Red,
                        tickFontFormat);
                }
            }
            if (_skewQ) return;

            // horizontal axis
            _ds.DrawLine(
                (float)_coords.ToXPix(_coords.XStart), (float)_hzAxisY,
                (float)_coords.ToXPix(_coords.XEnd), (float)_hzAxisY,
                Colors.Blue, strokeWidth: 2f);

        }

        private void DrawLinesLogY()
        {
        }

        private void DrawVtLines()
        {
            var ticks = _coords.GetTicks(_coords.XStart, _coords.XEnd - _coords.XStart, 4);
            var tickFontFormat = new CanvasTextFormat
            {
                FontFamily = "Verdana",
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                VerticalAlignment = CanvasVerticalAlignment.Center,
                HorizontalAlignment = CanvasHorizontalAlignment.Center
            };

            foreach (var tick in ticks)
            {
                var xVal = tick.Val;
                var tickLevel = tick.Level;

                var strokeStyle = tickLevel == 0
                    ? Color.FromArgb(80, 0, 0, 255)
                    : Color.FromArgb(20, 0, 0, 255);

                var xPix = _coords.ToXPix(xVal);
                _ds.DrawLine(
                    (float)xPix, (float)_coords.ToYPix(_coords.YStart),
                    (float)xPix, (float)_coords.ToYPix(_coords.YEnd),
                    strokeStyle, strokeWidth: 1.0f);

                if (tickLevel == 0 && _xValsQ)
                {
                    _ds.DrawText(xVal.ToString(), (float)xPix, (float)_hzNumsY, Colors.Blue,
                        tickFontFormat);
                }
            }
            if (_skewQ) return;

            // vertical axis
            _ds.DrawLine(
                (float)_vtAxisX, (float)_coords.ToYPix(_coords.YStart),
                (float)_vtAxisX, (float)_coords.ToYPix(_coords.YEnd),
                Colors.Red, strokeWidth: 1.5f);
        }
    }
}
