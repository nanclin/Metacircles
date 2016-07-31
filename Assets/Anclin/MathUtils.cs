using UnityEngine;

namespace Anclin {
    public static class MathUtils {

        /// <summary>
        /// When value is at min, it returns 0, when value is at max, it returns 1, and interpolates inbetween.
        /// </summary>
        public static float RemapValue01(float value, float min, float max) {
            return (Mathf.Clamp(value, min, max) - min) / (max - min);
        }
    }
}
