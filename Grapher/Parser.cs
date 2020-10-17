using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Grapher
{
    internal class Parser
    {
        private List<MathNode> _tempNode = new List<MathNode>();
        private bool _useRadians = true;
        private const string _operators = "+-*(/),^.";
        private const char _variable = 'x';
        private string _errMsg;
        private double[] _varValues = new double[26];
    
        public Parser()
        {
        }

        public void SetVarVal(char varName, double newVal)
        {
            var varIndex = varName - 'a';
            if (varIndex >= 0 && varIndex < 26)
                _varValues[varIndex] = newVal;
        }

        public double GetVal()
        {
            return RootNode.Walk(_varValues);
        }

   
        public MathNode RootNode { get; private set; }

        internal void NewParse(string s)
        {
            Reset();

            var sb = new StringBuilder(s);
            sb.Replace(" ", "");
            sb.Replace("[", "(");
            sb.Replace("]", ")");
            sb.Replace("\u2212", "-");
            sb.Replace("\u00F7", "/");
            sb.Replace("\u00D7", "*");
            sb.Replace("\u00B2", "^2");
            sb.Replace("\u00B3", "^3");
            sb.Replace("\u221a", "sqrt");
            var sfix = sb.ToString().ToLower();
            sfix = Fixxy(sfix);
            sfix = FixParentheses(sfix);
            sfix = FixUnaryMinus(sfix);
            sfix = FixImplicitMultply(sfix);
            
            Console.WriteLine("newParse: " + s + " => " + sfix);
            RootNode = Parse(sfix);
        }

        private bool IsNumeric(string s) => double.TryParse(s, out double n) && !double.IsInfinity(n); 

        private MathNode Parse(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return new MathNode(NodeType.Real, "0", _useRadians);
            }
            if ( IsNumeric(s))
            {
                return new MathNode(NodeType.Real, s, _useRadians);
            }
            if (s[0] == '$')
            {
                if (IsNumeric(s.Substring(1)))
                {
                    return _tempNode[int.Parse(s.Substring(1))];
                }
            }

            var sLo = s.ToLower();
            if (sLo.Length == 1)
            {
                if (sLo[0] >= 'a' && sLo[0] <= 'z')
                {
                    return new MathNode(NodeType.Variable, sLo, _useRadians);
                }
            }

            switch (sLo)
            {
                case "pi":
                    return new MathNode(NodeType.Variable, sLo, _useRadians);
            }
     
            var bracStt = s.LastIndexOf('(');
            if (bracStt > -1)
            {
                var bracEnd = s.IndexOf(")", bracStt);
                if (bracEnd < 0)
                {
                    _errMsg += "Missing ')'\n";
                    return new MathNode(NodeType.Real, "0", _useRadians);
                }
                bool isParam;
                if (bracStt == 0)
                {
                    isParam = false;
                }
                else
                {
                    var prefix = s.Substring(bracStt - 1, 1);
                    isParam = _operators.IndexOf(prefix) <= -1;
                }

                if (!isParam)
                {
                    _tempNode.Add(Parse(s.Substring(bracStt + 1, bracEnd - bracStt - 1)));
                    return Parse(
                      s.Substring(0, bracStt) +
                        "$" +
                        (_tempNode.Count - 1).ToString() +
                        s.Substring(bracEnd + 1, s.Length - bracEnd - 1)
                    );
                }
                else
                {
                    var startM = -1;
                    for (var u = bracStt - 1; u > -1; u--)
                    {
                        var found = _operators.IndexOf(s.Substring(u, 1));
                        if (found > -1)
                        {
                            startM = u;
                            break;
                        }
                    }
                    var nnew = new MathNode(NodeType.Function, s.Substring(startM + 1, bracStt - 1 - startM), _useRadians);
                    nnew.AddChild(Parse(s.Substring(bracStt + 1, bracEnd - bracStt - 1)));
                    _tempNode.Add(nnew);
                    return Parse(
                      s.Substring(0, startM + 1) +
                        "$" +
                        (_tempNode.Count - 1).ToString() +
                        s.Substring(bracEnd + 1, s.Length - bracEnd - 1)
                    );
                }
            }
            int k;
            var k1 = s.LastIndexOf('+');
            var k2 = s.LastIndexOf('-');
            if (k1 > -1 || k2 > -1)
            {
                if (k1 > k2)
                {
                    k = k1;
                    var nnew = new MathNode(NodeType.Operation, "add", _useRadians);
                    nnew.AddChild(Parse(s.Substring(0, k)));
                    nnew.AddChild(Parse(s.Substring(k + 1, s.Length - k - 1)));
                    return nnew;
                }
                else
                {
                    k = k2;
                    var nnew = new MathNode(NodeType.Operation, "sub", _useRadians);
                    nnew.AddChild(Parse(s.Substring(0, k)));
                    nnew.AddChild(Parse(s.Substring(k + 1, s.Length - k - 1)));
                    return nnew;
                }
            }
            k1 = s.LastIndexOf('*');
            k2 = s.LastIndexOf('/');
            if (k1 > -1 || k2 > -1)
            {
                if (k1 > k2)
                {
                    k = k1;
                    var nnew = new MathNode(NodeType.Operation, "mult", _useRadians);
                    nnew.AddChild(Parse(s.Substring(0, k)));
                    nnew.AddChild(Parse(s.Substring(k + 1, s.Length - k - 1)));
                    return nnew;
                }
                else
                {
                    k = k2;
                    var nnew = new MathNode(NodeType.Operation, "div", _useRadians);
                    nnew.AddChild(Parse(s.Substring(0, k)));
                    nnew.AddChild(Parse(s.Substring(k + 1, s.Length - k - 1)));
                    return nnew;
                }
            }
            k = s.IndexOf('^');
            if (k > -1)
            {
                var nnew = new MathNode(NodeType.Operation, "pow", _useRadians);
                nnew.AddChild(Parse(s.Substring(0, k)));
                nnew.AddChild(Parse(s.Substring(k + 1, s.Length - k - 1)));
                return nnew;
            }
            if (IsNumeric(s))
            {
                return new MathNode(NodeType.Real, s, _useRadians);
            }
            else
            {
                if (s.Length == 0)
                {
                    return new MathNode(NodeType.Real, "0", _useRadians);
                }
                else
                {
                    _errMsg += "'" + s + "' is not a number.\n";
                    return new MathNode(NodeType.Real, "0", _useRadians);
                }
            }
        }
   
  
        private string FixParentheses(string s)
        {
            var sttParCount = s.Count(c => c == '(');
            var endParCount = s.Count(c => c == ')');


            while (sttParCount < endParCount)
            {
                s = "(" + s;
                sttParCount++;
            }

            while (endParCount < sttParCount)
            {
                s += ")";
                endParCount++;
            }

            return s;
        }

        private string Fixxy(string s)
        {
            s = Regex.Replace(s, "x[y]", "x*y");
            return Regex.Replace(s, "([0-9a])x", match =>
            {
                return match.Groups[1].Value + "*x";
            });
        }

        private string FixUnaryMinus(string s)
        {
            var x = s + "\n";
            var y = "";
            var openQ = false;
            var prevType = "(";
            var thisType = "";
            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c >= '0' && c <= '9')
                {
                    thisType = "N";
                }
                else
                {
                    if (_operators.IndexOf(c) >= 0)
                    {
                        if (c == '-')
                        {
                            thisType = "-";
                        }
                        else
                        {
                            thisType = "O";
                        }
                    }
                    else
                    {
                        if (c == '.' || c == _variable)
                        {
                            thisType = "N";
                        }
                        else
                        {
                            thisType = "C";
                        }
                    }
                    if (c == '(')
                    {
                        thisType = "(";
                    }
                    if (c == ')')
                    {
                        thisType = ")";
                    }
                }

                x += thisType;
                if (prevType == "(" && thisType == "-")
                {
                    y += "0";
                }
                if (openQ)
                {
                    switch (thisType)
                    {
                        case "N":
                            break;
                        default:
                            y += ")";
                            openQ = false;
                            break;
                    }
                }

                if (prevType == "O" && thisType == "-")
                {
                    y += "(0";
                    openQ = true;
                }
                y += c;
                prevType = thisType;
            }

            if (openQ)
            {
                y += ")";
            }
            return y;
        }

        private string FixImplicitMultply(string s)
        {
            var x = s + "\n";
            var y = "";
            var prevType = "?";
            var prevName = "";
            var thisName = "";
            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                string thisType;
                if (c >= '0' && c <= '9')
                {
                    thisType = "N";
                }
                else
                {
                    if (_operators.IndexOf(c) >= 0 || c == '=')
                    {
                        thisType = "O";
                        thisName = "";
                    }
                    else
                    {
                        thisType = "C";
                        thisName += c;
                    }
                    if (c == '(')
                    {
                        thisType = "(";
                    }
                    if (c == ')')
                    {
                        thisType = ")";
                    }
                }
                x += thisType;
                if (prevType == "N" && thisType == "C")
                {
                    y += "*";
                    thisName = "";
                }
                if (prevType == "N" && thisType == "(")
                {
                    y += "*";
                }
                if (prevType == ")" && thisType == "(")
                {
                    y += "*";
                }
                if (thisType == "(")
                {
                    switch (prevName)
                    {
                        case "i":
                        case "pi":
                        case "e":
                        case "a":
                        case "x":
                            y += "*";
                            break;
                    }
                }
                y += c;
                prevType = thisType;
                prevName = thisName;
            }
            return y;
        }

        private void Reset()
        {
            _tempNode.Clear();
            _errMsg = string.Empty;
        }
    }
}