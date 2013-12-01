using System;
using System.Drawing;
using System.Windows.Forms;

class ResizableControlWrapper
{
    enum DragMode { Move, Resize };

    bool isHeld = false;
    Point holdOffset = new Point();
    int gripSize = 14;
    DragMode dragMode = DragMode.Move;
    Rectangle gripRect = new Rectangle();

    Control control;

    public Control Control { get { return this.control; } }

    public ResizableControlWrapper(Control control)
    {
        this.control = control;
        this.control.Resize += new EventHandler(ResizableControl_Resize);
        this.control.MouseDown += new MouseEventHandler(ResizableControl_MouseDown);
        this.control.MouseUp += new MouseEventHandler(ResizableControl_MouseUp);
        this.control.MouseMove += new MouseEventHandler(ResizableControl_MouseMove);
        this.control.MouseLeave += new EventHandler(ResizableControl_MouseLeave);
        this.control.Paint += new PaintEventHandler(ResizableControl_Paint);
    }

    void ResizableControl_Paint(object sender, PaintEventArgs e)
    {
        ControlPaint.DrawSizeGrip(e.Graphics, this.control.BackColor, gripRect);
    }

    void ResizableControl_Resize(object sender, EventArgs e)
    {
        //rectangle within which the 'grip' graphics will be drawn (with respect to parent)
        //x, y, width, height
        gripRect = new Rectangle(this.control.ClientSize.Width - gripSize, this.control.ClientSize.Height - gripSize, gripSize, gripSize);
    }

    void ResizableControl_MouseLeave(object sender, EventArgs e)
    {
        this.control.Cursor = Cursors.Default;
    }

    void ResizableControl_MouseMove(object sender, MouseEventArgs e)
    {
        Point mouse = Control.MousePosition;
        Point mouseWithOffset = this.control.Parent.PointToClient(mouse);

        if (!this.isHeld)
            RecalculateDragMode(e.Location);

        if (this.dragMode == DragMode.Resize)
        {
            this.control.Cursor = Cursors.SizeNWSE;

            if (this.isHeld)
            {
                int x = mouseWithOffset.X - this.control.Location.X;
                int y = mouseWithOffset.Y - this.control.Location.Y;
                this.control.Width = x - (x % 10);
                this.control.Height = y - (y % 10);
            }
        }
        else
        {
            this.control.Cursor = Cursors.SizeAll;

            if (this.isHeld)
            {
                int x = mouseWithOffset.X - holdOffset.X;
                int y = mouseWithOffset.Y - holdOffset.Y;
                this.control.Location = new Point(x - (x % 10), y - (y % 10));
            }
        }
    }

    void ResizableControl_MouseUp(object sender, MouseEventArgs e)
    {
        this.isHeld = false;
    }

    void ResizableControl_MouseDown(object sender, MouseEventArgs e)
    {
        this.isHeld = true;
        this.holdOffset = e.Location;
    }

    void RecalculateDragMode(Point mousePos)
    {
        if (gripRect.Contains(mousePos))
            this.dragMode = DragMode.Resize;
        else
            this.dragMode = DragMode.Move;
    }
}
