﻿using System.Windows;
using System.Collections.Generic;

namespace RandomTestWPF
{
    /// <summary>
    /// Collection of numbers, supplied with the method returned numbers frequencies.
    /// </summary>
    public class Numbers
    {
        Point[] _pts = null;     // numbers frequencies
        bool _isSizeSel;
        //double[] _vals = null;
        int _summitInd;          // index of point with maximum y-coordinate

        /// <summary>Gets or sets generated random numbers.</summary>
        public int[] Data { get; set; }                      

        public Numbers(int count, bool isSizeSelection)
        {
            Data = new int[count];
            _isSizeSel = isSizeSelection;
        }

        /// <summary>
        /// Gets the total number of freq-val pairs in the number pairs collection.
        /// </summary>
        public int Length { get => Freqs.Length; }

        /// <summary>Gets the index of summit (vertex) point.</summary>
        public int SummitInd
        {
            get
            {
                if (_pts == null) SetPoints();
                return _summitInd;
            }
        }

        /// <summary>
        /// Gets the array of floating-points represented collection of points, 
        /// where each point represents a value (X) and its frequency (Y).
        /// </summary>
        public Point[] Freqs
        {
            get
            {
                if (_pts == null)   SetPoints();
                return _pts;
            }
        }

        //public double[] Values
        //{
        //    get
        //    {
        //        if (_pts == null) SetPoints();
        //        return _vals;
        //    }

        //}
        /// <summary>Sets Points collection.</summary>
        void SetPoints()
        {
            SortedDictionary<int, int> map = new SortedDictionary<int, int>();

            foreach (int n in Data)
                if (map.ContainsKey(n)) map[n]++;
                else map[n] = 1;

            Data = null;
            if(_isSizeSel)
                map.Remove(0);  // remove "empty" Data elements

            _pts = new Point[map.Count];
            //_vals = new double[map.Count];

            int ind = 0, maxY = 0;
            foreach (KeyValuePair<int, int> kvp in map)
            {
                if ((/*_vals[ind] =*/ _pts[ind].Y = kvp.Value) > maxY)   // freq
                {
                    maxY = kvp.Value;
                    _summitInd = ind;
                }
                _pts[ind++].X = kvp.Key;      // numb
            }
        }
    }
}
