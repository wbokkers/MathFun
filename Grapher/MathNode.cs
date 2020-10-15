using System;
using System.Collections.Generic;
using System.Data;

namespace Grapher
{
    internal class MathNode
    {
        private readonly int _tREAL = 0;
        private readonly int _tVAR = 1;
        private readonly int _tOP = 2;
        private readonly int _tFUNC = 3;

        private double _r; // real number
        private string _v; // variable name
        private string _op; // function  

        private int _childCount = 0;
        private readonly List<MathNode> _child = new List<MathNode>();
        
        public int Type { get; private set; }
        public bool RadiansQ { get; private set; }

        public MathNode(string type, string value, bool radiansQ)
        {
            SetNew(type, value, radiansQ);
        }

        public int GetLevelsHigh()
        {
            var lvl = 0;
            for (var i = 0; i < _childCount; i++)
            {
                lvl = Math.Max(lvl, _child[i].GetLevelsHigh());
            }
            return lvl + 1;
        }

        internal double Walk(Dictionary<string, double> vals)
        {
            if (Type == _tREAL) return _r;
            if (Type == _tVAR)
            {
                switch (_v)
                {
                    case "pi":
                        return Math.PI;
                    case "e":
                        return Math.E;

                    default:
                        if (vals.TryGetValue(_v, out double value))
                            return value;
                        else
                            return 0.0;
                }
            }

            var val = 0.0;
            if (Type == _tOP)
            {
                for(var i = 0; i < _childCount; i++)
                {
                    var val2 = 0.0;
                    if (_child[i] != null) val2 = _child[i].Walk(vals);
                    if (i == 0)
                    {
                        val = val2;
                    }
                    else
                    {
                        switch (_op)
                        {
                            case "add":
                                val += val2;
                                break;
                            case "sub":
                                val -= val2;
                                break;
                            case "mult":
                                val *= val2;
                                break;
                            case "div":
                                val /= val2;
                                break;
                            case "pow":
                                if (val2 == 2)
                                {
                                    val *= val;
                                }
                                else
                                {
                                    val = Math.Pow(val, val2);
                                }
                                break;
                        }
                    }
                }
                return val;
            }
            if (Type == _tFUNC)
            {
                var lhs = _child[0].Walk(vals);
                var angleFact = 1.0;
                if (!RadiansQ) angleFact = 180.0 / Math.PI;
    
                switch (_op)
                {
                    case "sin":
                        val = Math.Sin(lhs / angleFact);
                        break;
                    case "cos":
                        val = Math.Cos(lhs / angleFact);
                        break;
                    case "tan":
                        val = Math.Tan(lhs / angleFact);
                        break;
                    case "asin":
                        val = Math.Asin(lhs) * angleFact;
                        break;
                    case "acos":
                        val = Math.Acos(lhs) * angleFact;
                        break;
                    case "atan":
                        val = Math.Atan(lhs) * angleFact;
                        break;
                    case "sinh":
                        val = Math.Sinh(lhs);
                        break;
                    case "cosh":
                        val = Math.Cosh(lhs);
                        break;
                    case "tanh":
                        val = Math.Tanh(lhs);
                        break;
                    case "exp":
                        val = Math.Exp(lhs);
                        break;
                    case "log":
                        val = Math.Log10(lhs);
                        break;
                    case "ln":
                        val = Math.Log(lhs);
                        break;
                    case "abs":
                        val = Math.Abs(lhs);
                        break;
                    case "deg":
                        val = (lhs * 180) / Math.PI;
                        break;
                    case "rad":
                        val = (lhs * Math.PI) / 180;
                        break;
                    case "sign":
                        if (lhs < 0)
                        {
                            val = -1;
                        }
                        else
                        {
                            val = 1;
                        }
                        break;
                    case "sqrt":
                        val = Math.Sqrt(lhs);
                        break;
                    case "round":
                        val = Math.Round(lhs);
                        break;
                    case "int":
                        val = Math.Floor(lhs);
                        break;
                    case "floor":
                        val = Math.Floor(lhs);
                        break;
                    case "ceil":
                        val = Math.Ceiling(lhs);
                        break;
                    case "fact":
                        val = Factorial((int)lhs);
                        break;
                    default:
                        val = double.NaN;
                        break;
                }
                return val;
            }
            return val;
        }

        private double Factorial(int n)
        {
            if (n < 0) return double.NaN;
            if (n < 2) return 1;
            n <<= 0;
            int i = n;
            int f = n;
            while (i-- > 2)
            {
                f *= i;
            }
            return f;
        }

        private void SetNew(string type, string value, bool radiansQ)
        {
            RadiansQ = radiansQ;

            Clear();

            switch (type)
            {
                case "real":
                    Type = _tREAL;
                    _r = double.Parse(value);
                    break;
                case "var":
                    Type = _tVAR;
                    _v = value;
                    break;
                case "op":
                    Type= _tOP;
                    _op = value;
                    break;
                case "func":
                    Type = _tFUNC;
                    _op = value;
                    break;
            }
        }

     
        private void Clear()
        {
            _r = 1;
            _v = string.Empty;
            _op = string.Empty;
            _child.Clear();
            _childCount = 0;
        }

        internal MathNode AddChild(MathNode n)
        {
            _child.Add(n);
            _childCount++;
            return _child[_child.Count - 1]; // should be the same as n?????
        }

        internal bool IsLeaf => _childCount == 0;


        internal string WalkFmt()
        {
            var s = WalkFmta(true, "");
            s = s.Replace("Infinity", "Undefined");
            return s;
        }

        private string WalkFmta(bool noparq, string prevop)
        {
            var s = "";
            if (_childCount > 0)
            {
                var parq = false;
                if (_op == "add") parq = true;
                if (_op == "sub") parq = true;
                if (prevop == "div") parq = true;
                if (noparq) parq = false;
                if (Type == _tFUNC) parq = true;
                if (Type == _tOP)
                {
                }
                else
                {
                    s += Fmt(true);
                }
                if (parq) s += "(";
                for (var i = 0; i < _childCount; i++)
                {
                    if (Type == _tOP && i > 0)
                        s += Fmt();
                    s += _child[i].WalkFmta(false, _op);
                    if (Type == _tFUNC || (parq && i > 0))
                    {
                        s += ")";
                    }
                }
            }
            else
            {
                s += Fmt();
                if (prevop == "sin" || prevop == "cos" || prevop == "tan")
                {
                    if (RadiansQ)
                    {
                        s += " rad";
                    }
                    else
                    {
                        s += "&deg;";
                    }
                }
            }
            return s;
        }

        private string Fmt(bool htmlQ = false)
        {
            var s = "";
            if (Type == _tOP)
            {
                switch (_op.ToLower())
                {
                    case "add":
                        s = "+";
                        break;
                    case "sub":
                        s = htmlQ ? "&minus;" : "-";
                        break;
                    case "mult":
                        s = htmlQ ? "&times;" : "*";
                        break;
                    case "div":
                        s = htmlQ ? "&divide;" : "/";
                        break;
                    case "pow":
                        s = "^";
                        break;
                    default:
                        s = _op;
                        break;
                }
            }
            if (Type == _tREAL)
            {
                s = _r.ToString();
            }
            if (Type == _tVAR)
            {
                if (_r == 1)
                {
                    s = _v;
                }
                else
                {
                    if (_r != 0)
                    {
                        s = _r + _v;
                    }
                }
            }
            if (Type == _tFUNC)
            {
                s = _op;
            }
            return s;
        }

    }

   
}