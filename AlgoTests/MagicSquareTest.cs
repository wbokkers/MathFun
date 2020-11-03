using System;
using System.Collections.Generic;
using Xunit;

namespace AlgoTests
{
    public class MagicSquare
    {
        private int _n; // side of the sqare
        private int _magicNumber;

        public int[,] Values { get; private set; }

        public MagicSquare(int n)
        {
            _n = n;
            Values = new int[n, n];
            _magicNumber = _n * (_n * _n + 1) / 2;

            ClearValues();
        }

        private void ClearValues()
        {
            for (int r = 0; r < _n; r++)
                for (int c = 0; c < _n; c++)
                    Values[r, c] = 0;
        }


        private List<int[,]> _solutions = new List<int[,]>();

        public List<int[,]> Solve()
        {
            ClearValues();
            _solutions.Clear();
            
            DoSolve();
            return _solutions; 
        }


        private bool DoSolve()
        {
            if (IsNotConvergingToSolution())
            {
                return false;
            }

            for (int i = 0; i < _n * _n; i++)
            {
                var row = i / _n;
                var col = i % _n;

                if (Values[row, col] == 0)
                {
                    // Check all numbers 
                    for (int value = 1; value <= _n * _n; value++)
                    {
                        if (!NumberIsUsed(value, Values))
                        {
                            Values[row, col] = value;

                            if (IsSolved())
                            {
                                _solutions.Add(CloneValues());
                                return true;
                            }

                            if (DoSolve())
                                return true;
                        }
                    }

                    // no solution found, backtrack
                    Values[row, col] = 0;
                    break;
                }
            }

            return false;
        }

        private bool IsNotConvergingToSolution()
        {
            // check vertical
            int sum;
            int zeroes;
            for (int r = 0; r < _n; r++)
            {
                sum = 0;
                zeroes = 0;
                for (int c = 0; c < _n; c++)
                {
                    var value = Values[r, c];
                    if (value == 0) zeroes++;
                    sum += value;
                }
                if (zeroes > 0 && sum >= _magicNumber || zeroes == 0 && sum != _magicNumber)
                    return true;
            }


            // check horizontal
            for (int c = 0; c < _n; c++)
            {
                sum = 0;
                zeroes = 0;
                for (int r = 0; r < _n; r++)
                {
                    var value = Values[r, c];
                    if (value == 0) zeroes++;
                    sum += value;
                }
                if (zeroes > 0 && sum >= _magicNumber || zeroes == 0 && sum != _magicNumber)
                    return true;
            }


            // check diagonal 1
            sum = 0;
            zeroes = 0;
            for (int c = 0; c < _n; c++)
            {
                var value = Values[c, c];
                if (value == 0) zeroes++;
                sum += value;
            }
            if (zeroes > 0 && sum >= _magicNumber || zeroes == 0 && sum != _magicNumber)
                return true;

            // check diagonal 2
            sum = 0;
            zeroes = 0;
            for (int c = 0; c < _n; c++)
            {
                var value = Values[c, _n - 1 - c];
                if (value == 0) zeroes++;
                sum += value;


            }
            if (zeroes > 0 && sum >= _magicNumber || zeroes == 0 && sum != _magicNumber)
                return true;

            return false;
        }


        private bool NumberIsUsed(int number, int[,] state)
        {
            for (int r = 0; r < _n; r++)
                for (int c = 0; c < _n; c++)
                    if (state[r, c] == number)
                        return true;

            return false;
        }



        public bool IsSolved()
        {
            // check vertical
            int sum = 0;
            for (int r = 0; r < _n; r++)
            {
                sum = 0;
                for (int c = 0; c < _n; c++)
                {
                    sum += Values[r, c];
                }
                if (sum != _magicNumber)
                    return false;
            }


            // check horizontal
            for (int c = 0; c < _n; c++)
            {
                sum = 0;
                for (int r = 0; r < _n; r++)
                {
                    sum += Values[r, c];
                }
                if (sum != _magicNumber)
                    return false;
            }


            // check diagonal 1
            sum = 0;
            for (int c = 0; c < _n; c++)
            {
                sum += Values[c, c];
            }
            if (sum != _magicNumber)
                return false;

            // check diagonal 2
            sum = 0;
            for (int c = 0; c < _n; c++)
            {
                sum += Values[c, _n - 1 - c];
            }
            if (sum != _magicNumber)
                return false;

            return true;
        }

        private int[,] CloneValues()
        {
            var clone = new int[_n, _n];
            for (int r = 0; r < _n; r++)
                for (int c = 0; c < _n; c++)
                    clone[r, c] = Values[r, c];
            return clone;
        }
    }
    public class MagicSquareTest
    {
        [Fact]
        public void Solve()
        {
            var sqr = new MagicSquare(3);
            var solutions = sqr.Solve();
            Assert.True(solutions.Count > 0);
            Assert.True(sqr.IsSolved());
        }
    }
}
