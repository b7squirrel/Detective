// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("JJax2soYRHgZVvwXNDC7+V7ss9GeXboVNTHDkxRj5F3rOYUxeWGpnet/RWd3cpZKbIVrwwTfkOd1cRFyGCc4zb8UY6EMIceSTmVqbBoCaMfDVZo6MqV9OWAFHNE+4sGT5Wj6Fl6WuHKyjLzSuYluBwb01jeVm8i78+/ggsBJJCmLUY+0ymrEhbN48/h5YSDHikbPJPaXgipVPOd+Gv7msWjaWXpoVV5Rct4Q3q9VWVlZXVhb0wvTnHGU0zkKcvG6BVNSe8BMyILaWVdYaNpZUlraWVlYiOgSKkdszvGuTclSiCDnVjBl4JGIPru1B6ibGa/0Y8KZ7FOsmqcvZZvrQYYNTr4tbhL5iRELrXR6XOXWdpV8WUV5s5C92lMSLpPLI1pbWVhZ");
        private static int[] order = new int[] { 3,4,5,6,8,7,13,10,13,12,13,11,12,13,14 };
        private static int key = 88;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
