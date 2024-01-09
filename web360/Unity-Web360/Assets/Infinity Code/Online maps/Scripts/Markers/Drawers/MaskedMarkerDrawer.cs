using UnityEngine;

namespace InfinityCode.OnlineMapsSupport
{
    public class MaskedMarkerDrawer : MonoBehaviour
    {
        public OnlineMaps map;
        public RectTransform mask;

        private void Start()
        {
            if (map == null) map = OnlineMaps.instance;
            map.control.markerDrawer = new Drawer(map.control, mask);
        }

        public class Drawer : OnlineMapsMarkerBufferDrawer
        {
            private RectTransform mask;

            public Drawer(OnlineMapsControlBase control, RectTransform mask) : base(control)
            {
                this.mask = mask;
            }

            private bool IsPositionMasked(Vector2 position)
            {
                return RectTransformUtility.RectangleContainsScreenPoint(mask, position);
            }

            public override OnlineMapsMarker GetMarkerFromScreen(Vector2 screenPosition)
            {
                if (!IsPositionMasked(screenPosition)) return null;
                return base.GetMarkerFromScreen(screenPosition);
            }
        }
    }
}