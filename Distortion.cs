using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackBaron
{
    class Distortion
    {
        #region Constant Data
        public float blurAmount = 2.0f;
        public float DistortionScale = 0.025f;
        public bool DistortionBlur = false;

        #endregion


        #region Initialization
        public Distortion() { }


        #endregion


        #region Blur Calculation
        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        /// <remarks>
        /// This function was originally provided in the BloomComponent class in the 
        /// Bloom Postprocess sample.
        /// </remarks>
        public float ComputeGaussian(float n)
        {
            return (float)((1.0 / Math.Sqrt(2 * Math.PI * blurAmount)) *
                           Math.Exp(-(n * n) / (2 * blurAmount * blurAmount)));
        }
        #endregion
    }
}
