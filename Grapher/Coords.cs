using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.UI.Xaml.Automation.Peers;

namespace Grapher
{
    internal class Pt
    {
        public Pt(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; set; }
        public double Y { get; set; }
    }

    internal class Tick
    {
        public Tick(double val, int level)
        {
            Val = val;
            Level = level;
        }

        public double Val { get; set; }
        public int Level { get; set; }

    }

    internal class Coords
    {
        private bool _uniScaleQ; // use uniform scaling? (same scale in both directions)

        private double _xScale; // calculated 
        private double _xLogScale; // calculated 
        private double _yScale; // calculated 
        private double _yLogScale; // calculated

        public double XStart { get; private set; } // graph x start
        public double YStart { get; private set; } // graph y start
        public double XEnd { get; private set; } // graph x end
        public double YEnd { get; private set; } // graph y end

        public double Left { get; private set; } // canvas left
        public double Top { get; private set; } // canvas top
        public double Width { get; private set; } // canvas width
        public double Height { get; private set; } // canvas height

        public bool UseXLog { get; private set; } // use log x scaling?
        public bool UseYLog { get; private set; } // use log y scaling?

        public Coords(double left, double top, double width, double height, double xStt, double yStt, double xEnd, double yEnd, bool uniScaleQ)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
            XStart = xStt;
            YStart = yStt;
            XEnd = xEnd;
            YEnd = yEnd;
            _uniScaleQ = uniScaleQ;
            UseXLog = false;
            UseYLog = false;
            CalcScale();
        }

        /// <summary>
        /// Scale the graph from the center of the graph
        /// </summary>
        public void Scale(double factor)
        {
            var xMid = (XStart + XEnd) / 2;
            var yMid = (YStart + YEnd) / 2;
            Scale(factor, xMid, yMid);
        }

        /// <summary>
        /// Scale the graph from the specified center
        /// </summary>
        public void Scale(double factor, double xMid, double yMid)
        {
            XStart = xMid - (xMid - XStart) * factor;
            XEnd = xMid + (XEnd - xMid) * factor;
            YStart = yMid - (yMid - YStart) * factor;
            YEnd = yMid + (YEnd - yMid) * factor;
            CalcScale();
        }

        /// <summary>
        /// Drag the graph using the specified offset
        /// </summary>
        public void Drag(double xPix, double yPix)
        {
            XStart += xPix * _xScale;
            XEnd += xPix * _xScale;
            YStart += yPix * _yScale;
            YEnd += yPix * _yScale;
            CalcScale();
        }

        /// <summary>
        /// Use a new center position
        /// </summary>
        public void NewCenter(double x, double y)
        {
            var xMid = XStart + x * _xScale;
            var xhalfspan = (XEnd - XStart) / 2;
            XStart = xMid - xhalfspan;
            XEnd = xMid + xhalfspan;
            var yMid = YEnd - y * _yScale;
            var yhalfspan = (YEnd - YStart) / 2;
            YStart = yMid - yhalfspan;
            YEnd = yMid + yhalfspan;
            CalcScale();
        }

        public void FitToPoints(List<Pt> pts, double borderFactor)
        {
            for (var i = 0; i < pts.Count; i++)
            {
                var pt = pts[i];
                if (i == 0)
                {
                    XStart = pt.X;
                    XEnd = pt.X;
                    YStart = pt.Y;
                    YEnd = pt.Y;
                }
                else
                {
                    XStart = Math.Min(XStart, pt.X);
                    XEnd = Math.Max(XEnd, pt.X);
                    YStart = Math.Min(YStart, pt.Y);
                    YEnd = Math.Max(YEnd, pt.Y);
                }
            }
            var xMid = (XStart + XEnd) / 2;
            var xhalfspan = (borderFactor * (XEnd - XStart)) / 2;
            XStart = xMid - xhalfspan;
            XEnd = xMid + xhalfspan;
            var yMid = (YStart + YEnd) / 2;
            var yhalfspan = (borderFactor * (YEnd - YStart)) / 2;
            YStart = yMid - yhalfspan;
            YEnd = yMid + yhalfspan;
            CalcScale();
        }

        public double ToXPix(double val)
        {
            if (UseXLog)
            {
                return Left + (Math.Log(val) - Math.Log(XStart)) / _xLogScale;
            }
            else
            {
                return Left + (val - XStart) / _xScale;
            }
        }
        public double ToYPix(double val)
        {
            if (UseYLog)
            {
                return Top + (Math.Log(YEnd) - Math.Log(val)) / _yLogScale;
            }
            else
            {
                return Top + (YEnd - val) / _yScale;
            }
        }

        public Pt ToPtVal(Pt pt, bool useCornerQ)
        {
            return new Pt(ToXVal(pt.X, useCornerQ), ToYVal(pt.Y, useCornerQ));
        }

        public double ToXVal(double pix, bool useCornerQ)
        {
            if (useCornerQ)
            {
                return XStart + (pix - Left) * _xScale;
            }
            else
            {
                return XStart + pix * _xScale;
            }
        }

        public double ToYVal(double pix, bool useCornerQ)
        {
            if (useCornerQ)
            {
                return YEnd - (pix - Top) * _yScale;
            }
            else
            {
                return YEnd - pix * _yScale;
            }
        }

        public List<Tick> GetTicks(double start, double span, double ratio)
        {
            var ticks = new List<Tick>();
            var inter = TickInterval(span / ratio, false);
            var tickStart = Math.Ceiling(start / inter) * inter;
            var i = 0;
            double tick;
            do
            {
                tick = tickStart + i * inter;
                tick = Math.Round(tick, 8);
                ticks.Add(new Tick(tick,1));
                i++;
            } while (tick < start + span);
            
            // Set inner tick levels to 0
            inter = TickInterval(span / ratio, true);
            for (i = 0; i < ticks.Count; i++)
            {
                var t = ticks[i].Val;
                if (Math.Abs(Math.Round(t / inter) - t / inter) < 0.001)
                {
                    ticks[i].Level = 0;
                }
            }
            return ticks;
        }

        private double TickInterval(double span, bool majorQ)
        {
            var pow10 = Math.Pow(10, Math.Floor(Math.Log10(span)));
            var mantissa = span / pow10;
            if (mantissa >= 5)
            {
                if (majorQ)
                {
                    return 5 * pow10;
                }
                else
                {
                    return 1 * pow10;
                }
            }
            if (mantissa >= 3)
            {
                if (majorQ)
                {
                    return 2 * pow10;
                }
                else
                {
                    return 0.2 * pow10;
                }
            }
            if (mantissa >= 1.4)
            {
                if (majorQ)
                {
                    return 0.5 * pow10;
                }
                else
                {
                    return 0.2 * pow10;
                }
            }
            if (mantissa >= 0.8)
            {
                if (majorQ)
                {
                    return 0.5 * pow10;
                }
                else
                {
                    return 0.1 * pow10;
                }
            }
            if (majorQ)
            {
                return 0.2 * pow10;
            }
            else
            {
                return 0.1 * pow10;
            }
        }


        private void CalcScale()
        {
            if (UseXLog)
            {
                if (XStart <= 0) XStart = 1;
                if (XEnd <= 0) XEnd = 1;
            }
            if (UseYLog)
            {
                if (YStart <= 0) YStart = 1;
                if (YEnd <= 0) YEnd = 1;
            }
            double temp;
            if (XStart > XEnd)
            {
                temp = XStart;
                XStart = XEnd;
                XEnd = temp;
            }
            if (YStart > YEnd)
            {
                temp = YStart;
                YStart = YEnd;
                YEnd = temp;
            }
            var xSpan = XEnd - XStart;
            if (xSpan <= 0) xSpan = 1e-9;
            _xScale = xSpan / Width;
            _xLogScale = (Math.Log(XEnd) - Math.Log(XStart)) / Width;
            var ySpan = YEnd - YStart;
            if (ySpan <= 0) ySpan = 1e-9;
            _yScale = ySpan / Height;
            _yLogScale = (Math.Log(YEnd) - Math.Log(YStart)) / Height;
            if (_uniScaleQ && !UseXLog && !UseYLog)
            {
                var newScale = Math.Max(_xScale, _yScale);
                _xScale = newScale;
                xSpan = _xScale * Width;
                var xMid = (XStart + XEnd) / 2;
                XStart = xMid - xSpan / 2;
                XEnd = xMid + xSpan / 2;
                _yScale = newScale;
                ySpan = _yScale * Height;
                var yMid = (YStart + YEnd) / 2;
                YStart = yMid - ySpan / 2;
                YEnd = yMid + ySpan / 2;
            }
        }
    }
}