/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of use Online Maps Drawing API.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/DrawingAPI_Example")]
    public class DrawingAPI_Example : MonoBehaviour
    {
        private void Start()
        {
            List<Vector2> line = new List<Vector2>
            {
                //Geographic coordinates
                new Vector2(8.93266215617905f, 44.40604327943526f),
                new Vector2(8.932524131901832f, 44.40601228591227f),
                new Vector2(8.932359430658943f, 44.405996835286025f),
                new Vector2(8.93207661034279f, 44.40596474551083f)
            };

            List<Vector2> poly = new List<Vector2>
            {
                //Geographic coordinates
                new Vector2(8.93266215617905f, 44.40604327943526f),
                new Vector2(8.932524131901832f, 44.40601228591227f),
                new Vector2(8.932359430658943f, 44.405996835286025f),
                new Vector2(8.93207661034279f, 44.40596474551083f)
            };

            // Draw line
            OnlineMapsDrawingElementManager.AddItem(new OnlineMapsDrawingLine(line, Color.magenta, 5));

            // Draw filled transparent poly
            //OnlineMapsDrawingElementManager.AddItem(new OnlineMapsDrawingPoly(poly, Color.red, 1, new Color(1, 1, 1, 0.5f)));

            // Draw filled rectangle
            // (position, size, borderColor, borderWidth, backgroundColor)
            //OnlineMapsDrawingElementManager.AddItem(new OnlineMapsDrawingRect(new Vector2(2, 2), new Vector2(1, 1), Color.green, 1, Color.blue));
        }
    }
}