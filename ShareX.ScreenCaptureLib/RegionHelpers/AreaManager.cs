﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright © 2007-2015 ShareX Developers

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using ShareX.HelpersLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ShareX.ScreenCaptureLib
{
    public class AreaManager
    {
        public List<RegionInfo> Areas { get; private set; }

        public RegionInfo[] ValidAreas
        {
            get
            {
                return Areas.Where(x => IsAreaValid(x.Area)).ToArray();
            }
        }

        public int SelectedAreaIndex { get; private set; }

        public RegionInfo CurrentRegionInfo
        {
            get
            {
                if (SelectedAreaIndex > -1)
                {
                    return Areas[SelectedAreaIndex];
                }

                return null;
            }
        }

        public Rectangle CurrentArea
        {
            get
            {
                if (SelectedAreaIndex > -1)
                {
                    return Areas[SelectedAreaIndex].Area;
                }

                return Rectangle.Empty;
            }
            set
            {
                if (SelectedAreaIndex > -1)
                {
                    Areas[SelectedAreaIndex].Area = value;
                }
            }
        }

        public bool IsCurrentAreaValid
        {
            get
            {
                return IsAreaValid(CurrentArea);
            }
        }

        public Rectangle CurrentHoverArea { get; private set; }

        public bool IsCurrentHoverAreaValid
        {
            get
            {
                return IsAreaValid(CurrentHoverArea);
            }
        }

        public float RoundedRectangleRadius { get; set; }
        public int RoundedRectangleRadiusIncrement { get; set; }
        public TriangleAngle TriangleAngle { get; set; }

        public ResizeManager ResizeManager { get; private set; }
        public bool IsCreating { get; private set; }
        public bool IsMoving { get; private set; }

        public bool IsResizing
        {
            get
            {
                return ResizeManager.IsResizing;
            }
        }

        public List<Rectangle> Windows { get; set; }
        public bool WindowCaptureMode { get; set; }
        public bool IncludeControls { get; set; }
        public int MinimumSize { get; set; }

        private RectangleRegion surface;
        private Point currentPosition;
        private Point positionOnClick;
        private bool proportionalResizing;

        public AreaManager(RectangleRegion surface)
        {
            this.surface = surface;
            ResizeManager = new ResizeManager(surface, this);

            Areas = new List<RegionInfo>();
            SelectedAreaIndex = -1;
            RoundedRectangleRadius = 25;
            RoundedRectangleRadiusIncrement = 3;
            TriangleAngle = TriangleAngle.Top;
            MinimumSize = 10;

            surface.MouseDown += surface_MouseDown;
            surface.MouseUp += surface_MouseUp;
            surface.KeyDown += surface_KeyDown;
            surface.KeyUp += surface_KeyUp;
        }

        private void surface_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.ShiftKey:
                    proportionalResizing = true;
                    break;
                case Keys.NumPad1:
                    ChangeCurrentShape(RegionShape.Rectangle);
                    break;
                case Keys.NumPad2:
                    ChangeCurrentShape(RegionShape.RoundedRectangle);
                    break;
                case Keys.NumPad3:
                    ChangeCurrentShape(RegionShape.Ellipse);
                    break;
                case Keys.NumPad4:
                    ChangeCurrentShape(RegionShape.Triangle);
                    break;
                case Keys.NumPad5:
                    ChangeCurrentShape(RegionShape.Diamond);
                    break;
                case Keys.Add:
                    switch (surface.Config.CurrentRegionShape)
                    {
                        case RegionShape.RoundedRectangle:
                            RoundedRectangleRadius += RoundedRectangleRadiusIncrement;
                            break;
                        case RegionShape.Triangle:
                            if (TriangleAngle == TriangleAngle.Left)
                            {
                                TriangleAngle = TriangleAngle.Top;
                            }
                            else
                            {
                                TriangleAngle++;
                            }
                            break;
                    }
                    UpdateCurrentRegionInfo();
                    break;
                case Keys.Subtract:
                    switch (surface.Config.CurrentRegionShape)
                    {
                        case RegionShape.RoundedRectangle:
                            RoundedRectangleRadius = Math.Max(0, RoundedRectangleRadius - RoundedRectangleRadiusIncrement);
                            break;
                        case RegionShape.Triangle:
                            if (TriangleAngle == TriangleAngle.Top)
                            {
                                TriangleAngle = TriangleAngle.Left;
                            }
                            else
                            {
                                TriangleAngle--;
                            }
                            break;
                    }
                    UpdateCurrentRegionInfo();
                    break;
            }
        }

        private void surface_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.ShiftKey:
                    proportionalResizing = false;
                    break;
                case Keys.Delete:
                    RemoveCurrentArea();
                    break;
            }
        }

        public void Update()
        {
            if (IsMoving)
            {
                Rectangle rect = CurrentArea;
                rect.X += InputManager.MouseVelocity.X;
                rect.Y += InputManager.MouseVelocity.Y;
                CurrentArea = rect;
            }

            if (IsCreating && !CurrentArea.IsEmpty)
            {
                currentPosition = InputManager.MousePosition0Based;

                Point newPosition = currentPosition;

                if (proportionalResizing)
                {
                    newPosition = CaptureHelpers.ProportionalPosition(positionOnClick, currentPosition);
                }

                CurrentArea = CaptureHelpers.CreateRectangle(positionOnClick, newPosition);
            }

            CheckHover();

            ResizeManager.Update();
        }

        private void CheckHover()
        {
            CurrentHoverArea = Rectangle.Empty;

            if (!ResizeManager.IsCursorOnNode() && !IsCreating && !IsMoving && !IsResizing)
            {
                Rectangle hoverArea = GetAreaIntersectWithMouse();

                if (!hoverArea.IsEmpty)
                {
                    CurrentHoverArea = hoverArea;
                }
                else if (WindowCaptureMode && Windows != null)
                {
                    hoverArea = Windows.FirstOrDefault(x => x.Contains(InputManager.MousePosition));

                    if (!hoverArea.IsEmpty)
                    {
                        hoverArea = CaptureHelpers.ScreenToClient(hoverArea);
                        CurrentHoverArea = Rectangle.Intersect(surface.ScreenRectangle0Based, hoverArea);
                    }
                }
            }
        }

        private void surface_MouseDown(object sender, MouseEventArgs e)
        {
            int areaIndex = AreaIntersect(InputManager.MousePosition0Based);

            if (e.Button == MouseButtons.Left && !ResizeManager.IsCursorOnNode())
            {
                positionOnClick = InputManager.MousePosition0Based;

                if (areaIndex > -1) // Select area
                {
                    IsMoving = true;
                    SelectedAreaIndex = areaIndex;
                    SelectArea();
                }
                else if (!IsCreating) // Create new area
                {
                    DeselectArea();

                    Rectangle rect;

                    if (surface.Config.IsFixedSize)
                    {
                        IsMoving = true;
                        rect = new Rectangle(new Point(e.X - surface.Config.FixedSize.Width / 2, e.Y - surface.Config.FixedSize.Height / 2), surface.Config.FixedSize);
                    }
                    else
                    {
                        IsCreating = true;
                        rect = new Rectangle(e.Location, new Size(1, 1));
                    }

                    AddRegionInfo(rect);
                }
            }
        }

        private void surface_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                IsCreating = false;
                IsMoving = false;

                if (!CurrentArea.IsEmpty)
                {
                    if (!IsCurrentAreaValid)
                    {
                        RemoveCurrentArea();
                        CheckHover();
                    }
                    else if (surface.Config.QuickCrop)
                    {
                        surface.UpdateRegionPath();
                        surface.Close(SurfaceResult.Region);
                    }
                    else
                    {
                        SelectArea();
                    }
                }

                if (!CurrentHoverArea.IsEmpty)
                {
                    AddRegionInfo(CurrentHoverArea);

                    if (surface.Config.QuickCrop)
                    {
                        surface.UpdateRegionPath();
                        surface.Close(SurfaceResult.Region);
                    }
                    else
                    {
                        SelectArea();
                    }
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                int areaIndex = AreaIntersect();

                if (areaIndex > -1)
                {
                    Areas.RemoveAt(areaIndex);
                    DeselectArea();
                }
                else
                {
                    surface.Close(SurfaceResult.Close);
                }
            }
        }

        private void ChangeCurrentShape(RegionShape shape)
        {
            surface.Config.CurrentRegionShape = shape;
            UpdateCurrentRegionInfo();
        }

        private void AddRegionInfo(Rectangle rect)
        {
            Areas.Add(GetRegionInfo(rect));
            SelectedAreaIndex = Areas.Count - 1;
        }

        public RegionInfo GetRegionInfo(Rectangle rect)
        {
            RegionInfo regionInfo = new RegionInfo(rect, surface.Config.CurrentRegionShape);
            regionInfo.RoundedRectangleRadius = RoundedRectangleRadius;
            regionInfo.TriangleAngle = TriangleAngle;
            return regionInfo;
        }

        private void UpdateCurrentRegionInfo()
        {
            RegionInfo regionInfo = CurrentRegionInfo;

            if (regionInfo != null)
            {
                regionInfo.Shape = surface.Config.CurrentRegionShape;
                regionInfo.RoundedRectangleRadius = RoundedRectangleRadius;
                regionInfo.TriangleAngle = TriangleAngle;
            }
        }

        private void SelectArea()
        {
            if (!CurrentArea.IsEmpty && !surface.Config.IsFixedSize)
            {
                ResizeManager.Show();
            }
        }

        private void DeselectArea()
        {
            SelectedAreaIndex = -1;
            ResizeManager.Hide();
        }

        private void RemoveCurrentArea()
        {
            if (SelectedAreaIndex > -1)
            {
                Areas.RemoveAt(SelectedAreaIndex);
                DeselectArea();
            }
        }

        private bool IsAreaValid(Rectangle rect)
        {
            return !rect.IsEmpty && rect.Width >= MinimumSize && rect.Height >= MinimumSize;
        }

        public int AreaIntersect(Point mousePosition)
        {
            for (int i = Areas.Count - 1; i >= 0; i--)
            {
                if (Areas[i].Area.Contains(mousePosition))
                {
                    return i;
                }
            }

            return -1;
        }

        public int AreaIntersect()
        {
            return AreaIntersect(InputManager.MousePosition0Based);
        }

        public Rectangle GetAreaIntersectWithMouse()
        {
            int areaIndex = AreaIntersect();

            if (areaIndex > -1)
            {
                return Areas[areaIndex].Area;
            }

            return Rectangle.Empty;
        }

        public bool IsAreaIntersect()
        {
            return AreaIntersect() > -1;
        }

        public Rectangle CombineAreas()
        {
            RegionInfo[] areas = ValidAreas;

            if (areas.Length > 0)
            {
                Rectangle rect = areas[0].Area;

                for (int i = 1; i < areas.Length; i++)
                {
                    rect = Rectangle.Union(rect, areas[i].Area);
                }

                return rect;
            }

            return Rectangle.Empty;
        }
    }
}