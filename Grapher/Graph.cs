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
        private bool _horizontalLinesVisible = true;
        private bool _verticalLinesVisible = true;
        private bool _xValuesVisible = true;
        private bool _yValuesVisible = true;
        private bool _isSkewed = false;

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
                if (_horizontalLinesVisible)
                {
                    DrawHorizontalLines();
                }
            }
            if (_coords.UseYLog)
            {
                DrawLinesLogY();
            }
            else
            {
                if (_verticalLinesVisible)
                {
                    DrawVerticalLines();
                }
            }
        }

        private void DrawLinesLogX()
        {
            throw new System.NotImplementedException();
        }

        private void DrawHorizontalLines()
        {
            var ticks = _coords.GetTicks(_coords.YStart, _coords.YEnd - _coords.YStart, _coords.Width/200);
            var tickFontFormat = new CanvasTextFormat
            {
                FontFamily = "Segoe UI",
                FontWeight = FontWeights.SemiBold,
                FontSize = 13,
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

                if (tickLevel == 0 && _yValuesVisible && yVal != 0)
                {
                    _ds.DrawText(yVal.ToString(), (float)_vtNumsX, (float)yPix, Colors.DarkGreen,
                        tickFontFormat);
                }
            }
            if (_isSkewed) return;

            // horizontal axis
            _ds.DrawLine(
                (float)_coords.ToXPix(_coords.XStart), (float)_hzAxisY,
                (float)_coords.ToXPix(_coords.XEnd), (float)_hzAxisY,
                Colors.DarkBlue, strokeWidth: 2f);

        }

        private void DrawLinesLogY()
        {
        }

        private void DrawVerticalLines()
        {
            var ticks = _coords.GetTicks(_coords.XStart, _coords.XEnd - _coords.XStart, _coords.Height/200);
            var tickFontFormat = new CanvasTextFormat
            {
                FontFamily = "Segoe UI",
                FontWeight = FontWeights.SemiBold,
                FontSize = 13,
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

                if (tickLevel == 0 && _xValuesVisible && xVal != 0)
                {
                    _ds.DrawText(xVal.ToString(), (float)xPix, (float)_hzNumsY, Colors.DarkBlue,
                        tickFontFormat);
                }
            }
            if (_isSkewed) return;

            // vertical axis
            _ds.DrawLine(
                (float)_vtAxisX, (float)_coords.ToYPix(_coords.YStart),
                (float)_vtAxisX, (float)_coords.ToYPix(_coords.YEnd),
                Colors.DarkGreen, strokeWidth: 2f);
        }
    }
}
