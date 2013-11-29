using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.DataVisualization.Charting.Data;
using System.Windows.Forms.DataVisualization.Charting.ChartTypes;

class ResizableChart : Chart
{
    enum DragMode { Move, Resize };

    bool isHeld = false;
    Point holdOffset = new Point();
    int gripSize = 14;
    DragMode dragMode = DragMode.Move;
    Rectangle gripRect = new Rectangle();

    public ResizableChart()
    {
        this.ResizeRedraw = true;
        this.Resize += new EventHandler(ResizableChart_Resize);
        this.MouseDown += new MouseEventHandler(ResizableChart_MouseDown);
        this.MouseUp += new MouseEventHandler(ResizableChart_MouseUp);
        this.MouseMove += new MouseEventHandler(ResizableChart_MouseMove);
        this.MouseLeave += new EventHandler(ResizableChart_MouseLeave);
    }

    void ResizableChart_Resize(object sender, EventArgs e)
    {
        //rectangle within which the 'grip' graphics will be drawn (with respect to parent)
        //x, y, width, height
        gripRect = new Rectangle(this.ClientSize.Width - gripSize, this.ClientSize.Height - gripSize, gripSize, gripSize);
    }

    void ResizableChart_MouseLeave(object sender, EventArgs e)
    {
        Cursor = Cursors.Default;
    }

    void ResizableChart_MouseMove(object sender, MouseEventArgs e)
    {
        Point mouse = MousePosition;
        Point mouseWithOffset = this.Parent.PointToClient(mouse);

        if (!this.isHeld)
            RecalculateDragMode(e.Location);

        if (this.dragMode == DragMode.Resize)
        {
            Cursor = Cursors.SizeNWSE;

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
            Cursor = Cursors.SizeAll;

            if (this.isHeld)
            {
                int x = mouseWithOffset.X - holdOffset.X;
                int y = mouseWithOffset.Y - holdOffset.Y;
                this.Location = new Point(x - (x % 10), y - (y % 10));
            }
        }
    }

    void ResizableChart_MouseUp(object sender, MouseEventArgs e)
    {
        this.isHeld = false;
    }

    void ResizableChart_MouseDown(object sender, MouseEventArgs e)
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

    protected override void OnPaint(PaintEventArgs e)
    {
        //draw parent control
        base.OnPaint(e);
        //draw 'grip' graphics
        ControlPaint.DrawSizeGrip(e.Graphics, this.BackColor, gripRect);
    }
}