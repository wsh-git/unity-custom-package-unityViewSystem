using UnityEngine.UI;

namespace Wsh.View {

    public class ViewUtils {

        public static void SetImageAlpha(Image image, float alpha) {
            var color = image.color;
            color.a = alpha;
            image.color = color;
        }

    }
}