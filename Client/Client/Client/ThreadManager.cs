using System;
using System.Collections.Generic;

namespace Networking.Client
{

    public static class ThreadManager
    {

        private static readonly List<Action> executeOnMainThread = new List<Action>();
        private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
        private static bool actionToExecuteOnMainThread = false;

        public static void ExecuteOnMainThread(Action action) {
            if (action == null) {
                Console.WriteLine("No action to run on main thread", ConsoleColor.Red);
                return;
            }

            // Add action
            lock (executeOnMainThread) {
                executeOnMainThread.Add(action);
                actionToExecuteOnMainThread = true;
            }
        }

        public static void Update() {
            if (actionToExecuteOnMainThread) {
                // Clear current get ready for copy
                executeCopiedOnMainThread.Clear();
                
                // Copy actions
                lock (executeOnMainThread) {
                    executeCopiedOnMainThread.AddRange(executeOnMainThread);
                    executeOnMainThread.Clear();
                    actionToExecuteOnMainThread = false;
                }

                for (int i = 0; i < executeCopiedOnMainThread.Count; i++) {
                    executeCopiedOnMainThread[i](); // Call the action
                }
            }
        }

    }

}