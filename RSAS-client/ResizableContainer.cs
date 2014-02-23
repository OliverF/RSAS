using System;
using System.Drawing;
using System.Windows.Forms;

namespace RSAS.ClientSide
{
    class ResizableContainer : GroupBox
    {
        enum DragMode { Move, Resize };

        bool isHeld = false;
        Point holdOffset = new Point();
        int gripSize = 14;
        DragMode dragMode = DragMode.Move;
        Rectangle gripRect = new Rectangle();

        public ResizableContainer()
        {
            this.Resize += new EventHandler(ResizableContainer_Resize);
            this.MouseDown += new MouseEventHandler(ResizableContainer_MouseDown);
            this.MouseUp += new MouseEventHandler(ResizableContainer_MouseUp);
            this.MouseMove += new MouseEventHandler(ResizableContainer_MouseMove);
            this.MouseLeave += new EventHandler(ResizableContainer_MouseLeave);
            this.Paint += new PaintEventHandler(ResizableContainer_Paint);
        }

        void ResizableContainer_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawSizeGrip(e.Graphics, this.BackColor, gripRect);
        }

        void ResizableContainer_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        void ResizableContainer_MouseMove(object sender, MouseEventArgs e)
        {
            Point mouse = Control.MousePosition;
            Point mouseWithOffset = this.Parent.PointToClient(mouse);

            if (!this.isHeld)
                RecalculateDragMode(e.Location);

            if (this.dragMode == DragMode.Resize)
            {
                this.Cursor = Cursors.SizeNWSE;

                if (this.isHeld)
                {
                    int x = mouseWithOffset.X - this.Location.X;
                    int y = mouseWithOffset.Y - this.Location.Y;
                    this.Width = x - (x % 10);
                    this.Height = y - (y % 10);
                }
            }
            else
            {
                this.Cursor = Cursors.SizeAll;

                if (this.isHeld)
                {
                    int x = mouseWithOffset.X - holdOffset.X;
                    int y = mouseWithOffset.Y - holdOffset.Y;
                    this.Location = new Point(x - (x % 10), y - (y % 10));
                }
            }
        }

        void ResizableContainer_MouseUp(object sender, MouseEventArgs e)
        {
            this.isHeld = false;
        }

        void ResizableContainer_MouseDown(object sender, MouseEventArgs e)
        {
            this.isHeld = true;
            this.holdOffset = e.Location;
        }

        void ResizableContainer_Resize(object sender, EventArgs e)
        {
            //rectangle within which the 'grip' graphics will be drawn (with respect to parent)
            //x, y, width, height
            gripRect = new Rectangle(this.ClientSize.Width - gripSize, this.ClientSize.Height - gripSize, gripSize, gripSize);
        }

        void RecalculateDragMode(Point mousePos)
        {
            if (gripRect.Contains(mousePos))
                this.dragMode = DragMode.Resize;
            else
                this.dragMode = DragMode.Move;
        }
    }
}
