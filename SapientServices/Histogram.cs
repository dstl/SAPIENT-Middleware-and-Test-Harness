// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: Histogram.cs$
// <copyright file="Histogram.cs" company="QinetiQ">
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// </copyright>

namespace SapientServices
{
    /// <summary>
    /// class to provide a simple histogram of network timing
    /// </summary>
    public class Histogram
    {
        /// <summary>
        /// Histogram array
        /// </summary>
        public int[] histo = new int[10];

        /// <summary>
        /// Initialize histogram
        /// </summary>
        public void Initialise()
        {
            int index;
            for (index = 0; index < 10; index++)
            {
                this.histo[index] = 0;
            }
        }

        /// <summary>
        /// Add a data point to the histogram
        /// </summary>
        /// <param name="value">data point to add to histogram</param>
        public void Add(double value)
        {
            if ((value >= 0) && (value <= 10.0))
            {
                this.histo[0]++;
            }
            else if ((value > 10.0) && (value <= 20.0))
            {
                this.histo[1]++;
            }
            else if ((value > 20.0) && (value <= 50.0))
            {
                this.histo[2]++;
            }
            else if ((value > 50.0) && (value <= 100.0))
            {
                this.histo[3]++;
            }
            else if ((value > 100.0) && (value <= 200.0))
            {
                this.histo[4]++;
            }
            else if ((value > 200.0) && (value <= 500.0))
            {
                this.histo[5]++;
            }
            else if ((value > 500.0) && (value <= 1000.0))
            {
                this.histo[6]++;
            }
            else if (value > 1000.0)
            {
                this.histo[7]++;
            }
            else if (value < 0)
            {
                this.histo[8]++;
            }
        }

        /// <summary>
        /// return the histogram as a text string
        /// </summary>
        /// <returns>text string</returns>
        public string Print()
        {
            return string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, -{8} ", histo[0], histo[1], histo[2], histo[3], histo[4], histo[5], histo[6], histo[7], histo[8]);
        }
    }
}
