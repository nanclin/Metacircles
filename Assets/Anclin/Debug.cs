namespace Anclin {
    class Debug {

        public static void Log(string message, params object[] args) {
            Debug.Log(string.Format(message, args));
        }
    }
}