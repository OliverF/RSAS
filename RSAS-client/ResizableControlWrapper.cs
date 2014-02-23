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

    Control baseControl;

    public Control BaseControl { get { return this.baseControl; } }

    public ResizableControlWrapper(Control control)
    {
        this.baseControl = control;
        this.baseControl.Resize += new EventHandler(ResizableControl_Resize);
        this.baseControl.MouseDown += new MouseEventHandler(ResizableControl_MouseDown);
        this.baseControl.MouseUp += new MouseEventHandler(ResizableControl_MouseUp);
        this.baseControl.MouseMove += new MouseEventHandler(ResizableControl_MouseMove);
        this.baseControl.MouseLeave += new EventHandler(ResizableControl_MouseLeave);
        this.baseControl.Paint += new PaintEventHandler(ResizableControl_Paint);
    }

    void ResizableControl_Paint(object sender, PaintEventArgs e)
    {
        ControlPaint.DrawSizeGrip(e.Graphics, this.baseControl.BackColor, gripRect);
    }

    void ResizableControl_Resize(object sender, EventArgs e)
    {
        //rectangle within which the 'grip' graphics will be drawn (with respect to parent)
        //x, y, width, height
        gripRect = new Rectangle(this.baseControl.ClientSize.Width - gripSize, this.baseControl.ClientSize.Height - gripSize, gripSize, gripSize);
    }

    void ResizableControl_MouseLeave(object sender, EventArgs e)
    {
        this.baseControl.Cursor = Cursors.Default;
    }

    void ResizableControl_MouseMove(object sender, MouseEventArgs e)
    {
        Point mouse = Control.MousePosition;
        Point mouseWithOffset = this.baseControl.Parent.PointToClient(mouse);

        if (!this.isHeld)
            RecalculateDragMode(e.Location);

        if (this.dragMode == DragMode.Resize)
        {
            this.baseControl.Cursor = Cursors.SizeNWSE;

            if (this.isHeld)
            {
                int x = mouseWithOffset.X - this.baseControl.Location.X;
                int y = mouseWithOffset.Y - this.baseControl.Location.Y;
                this.baseControl.Width = x - (x % 10);
                this.baseControl.Height = y - (y % 10);
            }
        }
        else
        {
            this.baseControl.Cursor = Cursors.SizeAll;

            if (this.isHeld)
            {
                int x = mouseWithOffset.X - holdOffset.X;
                int y = mouseWithOffset.Y - holdOffset.Y;
                this.baseControl.Location = new Point(x - (x % 10), y - (y % 10));
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
