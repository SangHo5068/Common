﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;


namespace Common.Utilities
{
    /// <summary>
    /// Used when maximizing a fully custom window to determine which screen the window should be maximized to. 
    /// </summary>
    public static class ScreenFinder
    {
        public static Screen FindAppropriateScreen(Window window)
        {
            var windowRight = window.Left + window.Width;
            var windowBottom = window.Top + window.Height;

            var allScreens = Screen.AllScreens.ToList();

            // If the window is inside all of a single screen boundaries, maximize to that
            var screenInsideAllBounds = allScreens.Find(x => window.Top   >= x.Bounds.Top   && 
                                                             window.Left  >= x.Bounds.Left  && 
                                                             windowRight  <= x.Bounds.Right &&
                                                             windowBottom <= x.Bounds.Bottom);
            if (screenInsideAllBounds != null)
            {
                return screenInsideAllBounds;
            }

            // Failing the above (between two screens in side-by-side configuration)
            // Measure if the window is between the top and bottom of any screens.
            // Then measure the percentage it is within each screen and pick a winner
            var screensInBounds = allScreens.FindAll(x => window.Top >= x.Bounds.Top &&
                                                            windowBottom <= x.Bounds.Bottom);
            if (screensInBounds.Count > 0)
            {
                var values = new List<Tuple<double, Screen>>();
                // Determine the amount of width inside each screen
                foreach (var screen in screensInBounds.OrderBy(x => x.Bounds.Left))
                {
                    // This has only been tested in a two screen, side-by-side setup.
                    double amountInScreen;
                    if (screen.Bounds.Left == 0)
                    {
                        var rightOfWindow = window.Left + window.Width;
                        var outsideRightBoundary = rightOfWindow - screen.Bounds.Right;
                        amountInScreen = window.Width - outsideRightBoundary;
                        values.Add(new Tuple<double, Screen>(amountInScreen, screen));
                    }
                    else
                    {
                        var outsideLeftBoundary = screen.Bounds.Left - window.Left;
                        amountInScreen = window.Width - outsideLeftBoundary;
                        values.Add(new Tuple<double, Screen>(amountInScreen, screen));
                    }
                }

                values = values.OrderByDescending(x => x.Item1).ToList();
                if (values.Count > 0)
                {
                    return values[0].Item2;
                }
            }

            // Failing all else
            return Screen.PrimaryScreen;
        }

        private static Screen FindCurrentScreen(int monitorPosition)
        {
            var allScreens = Screen.AllScreens.ToList();
            return allScreens.Count > 1 ? 
                allScreens.First(f => f.DeviceName.Contains(monitorPosition.ToString())) : allScreens.First();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <param name="position">모니터 포지션</param>
        /// <param name="isHideTaskbar">작업표시줄 표시여부(True:숨김, False:표시)</param>
        public static void SetMonitorScreen(Window window, int position, bool isHideTaskbar = true)
        {
            if (window != null)
            {
                var screen = FindCurrentScreen(position);
                if (screen != null)
                {
                    var winTaskbarHeight = isHideTaskbar ? GetTaskBarHeight(screen) : 0; //윈도우 작업표시줄 높이

                    window.Top    = screen.WorkingArea.Top;
                    window.Left   = screen.WorkingArea.Left;
                    window.Width  = screen.WorkingArea.Width;
                    window.Height = screen.WorkingArea.Height + winTaskbarHeight; // Window Taskbar Height
                    //window.WindowState = WindowState.Maximized;
                    //window.Topmost = true; // Window FullScreen and Window Taskbar Hide
                }
            }
        }

        /// <summary>
        /// 윈도우 작업표시줄 높이
        /// </summary>
        /// <returns></returns>
        private static int GetTaskBarHeight(Screen screen)
        {
            //var PSH = SystemParameters.PrimaryScreenHeight;
            //var PSBH = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            var PSH = screen.Bounds.Height;
            var PSBH = screen.Bounds.Height;
            var ratio = PSH / PSBH;
            //var TaskBarHeight = PSBH - System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
            var TaskBarHeight = PSBH - screen.WorkingArea.Height;
            TaskBarHeight *= Math.Round(ratio);
            return (int)TaskBarHeight;
        }
    }
}
