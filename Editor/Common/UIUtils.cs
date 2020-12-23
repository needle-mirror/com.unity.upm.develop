using System;
using UnityEngine.UIElements;

namespace Unity.PackageManagerUI.Develop.Editor
{
    internal static class UIUtils
    {
        public static void ScrollIfNeeded(ScrollView container, VisualElement target)
        {
            if (target == null || container == null)
                return;

            var containerWorldBound = container.worldBound;
            var targetWorldBound = target.worldBound;

            var minY = containerWorldBound.yMin;
            var maxY = containerWorldBound.yMax;
            var itemMinY = targetWorldBound.yMin;
            var itemMaxY = targetWorldBound.yMax;

            var scroll = container.scrollOffset;

            if (itemMinY < minY)
            {
                scroll.y -= Math.Max(0, minY - itemMinY);
                container.scrollOffset = scroll;
            }
            else if (itemMaxY > maxY)
            {
                scroll.y += itemMaxY - maxY;
                container.scrollOffset = scroll;
            }
        }
    }
}