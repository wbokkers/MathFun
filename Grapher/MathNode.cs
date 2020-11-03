using System;
using System.Collections.Generic;

namespace Grapher
{
    enum NodeType
    {
        Real,
        Variable,
        Operation,
        Function
    }

    internal class MathNode
    {
        private double _real; // real number
        private string _value; // variable name
        private string _op; // function  

        private int _childCount = 0;
        private readonly List<MathNode> _child = new List<MathNode>();

        public NodeType Type { get; private set; }
        public bool UseRadians { get; private set; }

        public MathNode(NodeType type, string value, bool useRadians)
        {
            UseRadians = useRadians;

            Clear();

            Type = type;
            switch (type)
            {
                case NodeType.Real:
                    _real = double.Parse(value);
                    break;
                case NodeType.Variable:
                    _value = value;
                    break;
                case NodeType.Operation:
                    _op = value;
                    break;
                case NodeType.Function:
                    _op = value;
                    break;
            }
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

        internal double Walk(double[] vals)
        {
            if (Type == NodeType.Real) return _real;

            if (Type == NodeType.Variable)
            {
                switch (_value)
                {
                    case "pi":
                        return Math.PI;
                    case "e":
                        return Math.E;

                    default:
                        var varIndex = _value[0] - 'a';
                        if (varIndex >= 0 && varIndex < 26)
                            return vals[varIndex];
                        else
                            return 0.0;
                }
            }

            var val = 0.0;
            if (Type == NodeType.Operation)
            {
                for (var i = 0; i < _childCount; i++)
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

            if (Type == NodeType.Function)
            {
                var lhs = _child[0].Walk(vals);
                var angleFact = 1.0;
                if (!UseRadians) angleFact = 180.0 / Math.PI;

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



        private void Clear()
        {
            _real = 1;
            _value = string.Empty;
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



    }


}