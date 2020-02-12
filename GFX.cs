using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using System.Diagnostics;

using ScriptStack;
using ScriptStack.Compiler;
using ScriptStack.Runtime;

namespace github.com.mbrain
{
    public class Gfx : Form, Stack 
    {

        private enum Primitive
        {
            Colour, LineWidth, Line, DrawString,
            DrawRectangle, FillRectangle,
            DrawEllipse, FillEllipse,
            DrawPolygon, FillPolygon
        }

        private class GraphicsForm : Form
        {
            public GraphicsForm() : base()
            {
                DoubleBuffered = true;
            }
        }

        /* 
         * Todo: unify the parameters
         * int values are passed differently (when > 3 as list, otherwise each one individually), float values as arraylist 'points' 
         */
        private class DrawingInstruction
        {
            public Primitive primitive;
            public string type = "primitive";
            public int iVal0;
            public int iVal1;
            public int iVal2;
            public int iVal3;
            public String strVal;
            public PointF[] pointsF;
        }

        private static ReadOnlyCollection<CustomMethod> s_listHostFunctionPrototypes;

        private Form m_form;
        private List<DrawingInstruction> drawingInstructions;
        private Pen m_pen;
        private Brush m_brush;
        private Font m_font;
        private Stopwatch timer;

        [STAThread]
        private void OnPaint(object objectSender, PaintEventArgs paintEventArgs)
        {
            Graphics graphics = paintEventArgs.Graphics;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            foreach (DrawingInstruction drawingInstruction in drawingInstructions) {

                    switch (drawingInstruction.primitive) {
                        case Primitive.Colour:
                            Color color = Color.FromArgb(drawingInstruction.iVal0, drawingInstruction.iVal1, drawingInstruction.iVal2);
                            m_pen.Color = color;
                            m_brush = new SolidBrush(color);
                            break;
                        case Primitive.LineWidth:
                            m_pen.Width = drawingInstruction.iVal0;
                            break;
                        case Primitive.Line:
                            graphics.DrawLine(m_pen, drawingInstruction.iVal0, drawingInstruction.iVal1, drawingInstruction.iVal2, drawingInstruction.iVal3);
                            break;
                        case Primitive.DrawRectangle:
                            graphics.DrawRectangle(m_pen, drawingInstruction.iVal0, drawingInstruction.iVal1, drawingInstruction.iVal2, drawingInstruction.iVal3);
                            break;
                        case Primitive.FillRectangle:
                            graphics.FillRectangle(m_brush, drawingInstruction.iVal0, drawingInstruction.iVal1, drawingInstruction.iVal2, drawingInstruction.iVal3);
                            break;
                        case Primitive.DrawEllipse:
                            graphics.DrawEllipse(m_pen, drawingInstruction.iVal0, drawingInstruction.iVal1, drawingInstruction.iVal2, drawingInstruction.iVal3);
                            break;
                        case Primitive.FillEllipse:
                            graphics.FillEllipse(m_brush, drawingInstruction.iVal0, drawingInstruction.iVal1, drawingInstruction.iVal2, drawingInstruction.iVal3);
                            break;
                        case Primitive.DrawString:
                            graphics.DrawString(drawingInstruction.strVal, m_font, m_brush, drawingInstruction.iVal0, drawingInstruction.iVal1);
                            break;
                        case Primitive.DrawPolygon:
                            graphics.DrawPolygon(m_pen, drawingInstruction.pointsF);
                            break;
                        case Primitive.FillPolygon:
                            graphics.FillPolygon(m_brush, drawingInstruction.pointsF);
                            break;
                    }

            }
        }
        private void OnKeyDown(object objectSender, KeyEventArgs keyEventArgs)
        {
            //todo
        }
        private void OnKeyUp(object objectSender, KeyEventArgs keyEventArgs)
        {
            //todo
        }

        public Gfx()
        {
            m_form = null;
            drawingInstructions = new List<DrawingInstruction>();
            if (s_listHostFunctionPrototypes != null) return;

            List<CustomMethod> listHostFunctionPrototypes = new List<CustomMethod>();
            CustomMethod hostFunctionPrototype = null;
            List<Type> fourInts = new List<Type>();
            fourInts.Add(typeof(int));
            fourInts.Add(typeof(int));
            fourInts.Add(typeof(int));
            fourInts.Add(typeof(int));

            hostFunctionPrototype = new CustomMethod(typeof(bool), "Gfx_Init", typeof(int), typeof(int), typeof(string), "Create a new graphics form, pass with, height and title");
            listHostFunctionPrototypes.Add(hostFunctionPrototype);
            hostFunctionPrototype = new CustomMethod(typeof(bool), "Gfx_Close", "Destroy the graphics form");
            listHostFunctionPrototypes.Add(hostFunctionPrototype);
            hostFunctionPrototype = new CustomMethod(typeof(bool), "Gfx_Clear", "Clear the graphics form");
            listHostFunctionPrototypes.Add(hostFunctionPrototype);
            hostFunctionPrototype = new CustomMethod(typeof(bool), "Gfx_SetColour", typeof(int), typeof(int), typeof(int), "Set the color to use next");
            listHostFunctionPrototypes.Add(hostFunctionPrototype);
            hostFunctionPrototype = new CustomMethod(typeof(bool), "Gfx_SetLineWidth", typeof(int), "Set the line width to print");
            listHostFunctionPrototypes.Add(hostFunctionPrototype);
            hostFunctionPrototype = new CustomMethod(typeof(bool), "Gfx_DrawLine", fourInts, "Draw a simple line x,y,w,h");
            listHostFunctionPrototypes.Add(hostFunctionPrototype);
            hostFunctionPrototype = new CustomMethod(typeof(bool), "Gfx_DrawRectangle", fourInts, "Draw a rectangle x,y,w,h");
            listHostFunctionPrototypes.Add(hostFunctionPrototype);
            hostFunctionPrototype = new CustomMethod(typeof(bool), "Gfx_FillRectangle", fourInts, "Fill a rectangle x,y,w,h");
            listHostFunctionPrototypes.Add(hostFunctionPrototype);
            hostFunctionPrototype = new CustomMethod(typeof(bool), "Gfx_DrawEllipse", fourInts, "Draw an ellipse x,y,w,h");
            listHostFunctionPrototypes.Add(hostFunctionPrototype);
            hostFunctionPrototype = new CustomMethod(typeof(bool), "Gfx_FillEllipse", fourInts, "Fill an ellipse x,y,w,h");
            listHostFunctionPrototypes.Add(hostFunctionPrototype);
            hostFunctionPrototype = new CustomMethod(typeof(bool), "Gfx_DrawString", typeof(int), typeof(int), typeof(String), "Draw a string at position X:Y");
            listHostFunctionPrototypes.Add(hostFunctionPrototype);
            hostFunctionPrototype = new CustomMethod(typeof(bool), "Gfx_DrawPolygon", typeof(ArrayList), "Draw a convex polygon based on floating point values, pass vertices as array, each point as an array X:Y");
            listHostFunctionPrototypes.Add(hostFunctionPrototype);
            hostFunctionPrototype = new CustomMethod(typeof(bool), "Gfx_FillPolygon", typeof(ArrayList), "Draw a convex polygon based on floating point values, pass vertices as array, each point as an array X:Y");
            listHostFunctionPrototypes.Add(hostFunctionPrototype);
            hostFunctionPrototype = new CustomMethod(typeof(ArrayList), "Gfx_MousePosition", "Get the current mouseposition as an array X:Y");
            listHostFunctionPrototypes.Add(hostFunctionPrototype);
            hostFunctionPrototype = new CustomMethod(typeof(bool), "Gfx_MouseClick", typeof(string), "TEST! Check if a specific button was clicked");
            listHostFunctionPrototypes.Add(hostFunctionPrototype);
            hostFunctionPrototype = new CustomMethod(typeof(bool), "Gfx_StartTimer", "Start the chronometer");
            listHostFunctionPrototypes.Add(hostFunctionPrototype);
            hostFunctionPrototype = new CustomMethod(typeof(bool), "Gfx_ResetTimer", "Reset the chronometer");
            listHostFunctionPrototypes.Add(hostFunctionPrototype);
            hostFunctionPrototype = new CustomMethod(typeof(string), "Gfx_ElapsedTime", typeof(string), "Get the elapsed time from the chronometer, use c# format specifiers");
            listHostFunctionPrototypes.Add(hostFunctionPrototype);
            hostFunctionPrototype = new CustomMethod(typeof(bool), "Gfx_StopTimer", "Stop the chronometer");
            listHostFunctionPrototypes.Add(hostFunctionPrototype);
            hostFunctionPrototype = new CustomMethod(typeof(bool), "Gfx_Paint", "Finally paint everything to screen");
            listHostFunctionPrototypes.Add(hostFunctionPrototype);

            s_listHostFunctionPrototypes = listHostFunctionPrototypes.AsReadOnly();
        }

        [STAThread]
        public object OnMethodInvoke(String strFunctionName, List<object> listParameters)
        {
            if (strFunctionName == "Gfx_Init")
            {
                if (m_form != null) return false;
                int iWidth = (int)listParameters[0];
                int iHeight = (int)listParameters[1];
                if (iWidth < 16) return false;
                if (iHeight < 16) return false;
                m_form = new Form();
                if ((string)listParameters[2] != "") m_form.Text = (string)listParameters[2];
                else m_form.Text = "ScriptStack GFX";
                /* winforms and theyr fucking borders.. add them*/
                m_form.Width = iWidth + 16;
                m_form.Height = iHeight + 40;
                /* Todo: handle key events - clicked, pressed, just clicked,.. and so on */
                m_form.Paint += new PaintEventHandler(OnPaint);
                m_form.KeyDown += new KeyEventHandler(OnKeyDown);
                m_form.KeyUp += new KeyEventHandler(OnKeyUp);
                m_form.Show();
                drawingInstructions.Clear();
                m_pen = new Pen(Color.Black);
                m_brush = new SolidBrush(Color.Black);
                m_font = new Font(FontFamily.GenericSansSerif, 10.0f);
                m_form.Invalidate();
                return true;
            }
            if (strFunctionName == "Gfx_Close")
            {
                if (m_form == null) return true;
                m_form.Close();
                drawingInstructions.Clear();
                m_form.Invalidate();
                m_form = null;
                return true;
            }
            if (strFunctionName == "Gfx_Clear")
            {
                if (m_form == null) return false;
                drawingInstructions.Clear();
                //m_form.Invalidate();
                return true;
            }
            if (strFunctionName == "Gfx_StartTimer")
            {
                if (timer == null) timer = new Stopwatch();
                timer.Start();
            }
            if (strFunctionName == "Gfx_ResetTimer")
            {
                if(timer == null) timer = new Stopwatch();
                timer.Reset();
            }
            if (strFunctionName == "Gfx_StopTimer")
            {   
                if(timer.IsRunning) timer.Stop();
            }
            if (strFunctionName == "Gfx_ElapsedTime")
            {
                if (timer == null) { timer = new Stopwatch(); timer.Reset(); }
                TimeSpan ts = timer.Elapsed;
                return string.Format((string)listParameters[0], ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            }
            if (strFunctionName == "Gfx_MousePosition")
            {
                if (m_form == null) return false;
                Point p = m_form.PointToClient(Cursor.Position);
                ArrayList arr = new ArrayList();
                arr.Add(p.X);
                arr.Add(p.Y);
                return arr;
            }
            /* Todo mousestate */
            else if (strFunctionName == "Gfx_MouseClick")
            {
                if (m_form == null) return false;
                string button = (string)listParameters[0];
                if(button == "left") return (Control.MouseButtons & MouseButtons.Left) > 0;
                if(button == "right") return (Control.MouseButtons & MouseButtons.Right) > 0;
                if(button == "middle") return (Control.MouseButtons & MouseButtons.Middle) > 0;
                return false;
            }
            if (strFunctionName == "Gfx_SetColour")
            {
                if (m_form == null) return false;
                DrawingInstruction drawingInstruction = new DrawingInstruction();
                drawingInstruction.primitive = Primitive.Colour;
                drawingInstruction.iVal0 = (int)listParameters[0];
                drawingInstruction.iVal1 = (int)listParameters[1];
                drawingInstruction.iVal2 = (int)listParameters[2];
                drawingInstructions.Add(drawingInstruction);
                //m_form.Invalidate();
                return true;
            }
            /* deprecated */
            else if (strFunctionName == "Gfx_SetLineWidth")
            {
                if (m_form == null) return false;

                DrawingInstruction drawingInstruction = new DrawingInstruction();
                drawingInstruction.primitive = Primitive.LineWidth;
                drawingInstruction.iVal0 = (int)listParameters[0];
                drawingInstructions.Add(drawingInstruction);
                //m_form.Invalidate();
                return true;
            }
            else if (strFunctionName == "Gfx_DrawLine")
            {
                if (m_form == null) return false;
                DrawingInstruction drawingInstruction = new DrawingInstruction();
                drawingInstruction.primitive = Primitive.Line;
                drawingInstruction.iVal0 = (int)listParameters[0];
                drawingInstruction.iVal1 = (int)listParameters[1];
                drawingInstruction.iVal2 = (int)listParameters[2];
                drawingInstruction.iVal3 = (int)listParameters[3];
                drawingInstructions.Add(drawingInstruction);
                //m_form.Invalidate();
                return true;
            }
            else if (strFunctionName == "Gfx_DrawRectangle")
            {
                if (m_form == null) return false;
                DrawingInstruction drawingInstruction = new DrawingInstruction();
                drawingInstruction.primitive = Primitive.DrawRectangle;
                drawingInstruction.iVal0 = (int)listParameters[0];
                drawingInstruction.iVal1 = (int)listParameters[1];
                drawingInstruction.iVal2 = (int)listParameters[2];
                drawingInstruction.iVal3 = (int)listParameters[3];
                drawingInstructions.Add(drawingInstruction);
                //m_form.Invalidate();
                return true;
            }
            else if (strFunctionName == "Gfx_FillRectangle")
            {
                if (m_form == null) return false;
                DrawingInstruction drawingInstruction = new DrawingInstruction();
                drawingInstruction.primitive = Primitive.FillRectangle;
                drawingInstruction.iVal0 = (int)listParameters[0];
                drawingInstruction.iVal1 = (int)listParameters[1];
                drawingInstruction.iVal2 = (int)listParameters[2];
                drawingInstruction.iVal3 = (int)listParameters[3];
                drawingInstructions.Add(drawingInstruction);
                //m_form.Invalidate();
                return true;
            }
            else if (strFunctionName == "Gfx_DrawEllipse")
            {
                if (m_form == null) return false;
                DrawingInstruction drawingInstruction = new DrawingInstruction();
                drawingInstruction.primitive = Primitive.DrawEllipse;
                drawingInstruction.iVal0 = (int)listParameters[0];
                drawingInstruction.iVal1 = (int)listParameters[1];
                drawingInstruction.iVal2 = (int)listParameters[2];
                drawingInstruction.iVal3 = (int)listParameters[3];
                drawingInstructions.Add(drawingInstruction);
                //m_form.Invalidate();
                return true;
            }
            else if (strFunctionName == "Gfx_FillEllipse")
            {
                if (m_form == null) return false;
                DrawingInstruction drawingInstruction = new DrawingInstruction();
                drawingInstruction.primitive = Primitive.FillEllipse;
                drawingInstruction.iVal0 = (int)listParameters[0];
                drawingInstruction.iVal1 = (int)listParameters[1];
                drawingInstruction.iVal2 = (int)listParameters[2];
                drawingInstruction.iVal3 = (int)listParameters[3];
                drawingInstructions.Add(drawingInstruction);
                //m_form.Invalidate();
                return true;
            }
            else if (strFunctionName == "Gfx_DrawString")
            {
                if (m_form == null) return false;
                DrawingInstruction drawingInstruction = new DrawingInstruction();
                drawingInstruction.primitive = Primitive.DrawString;
                drawingInstruction.iVal0 = (int)listParameters[0];
                drawingInstruction.iVal1 = (int)listParameters[1];
                drawingInstruction.strVal = (String)listParameters[2];
                drawingInstructions.Add(drawingInstruction);
                //m_form.Invalidate();
                return true;
            }
            else if (strFunctionName == "Gfx_DrawPolygon")
            {
                if (m_form == null) return false;               
                ArrayList parameterPoints = (ArrayList)listParameters[0];
                PointF[] points = new PointF[parameterPoints.Count];
                int pointCounter = 0;
                foreach (KeyValuePair<object, object> entry in parameterPoints) {
                    ArrayList tempPoints = (ArrayList)entry.Value;
                    PointF tempPoint = new PointF((float)tempPoints[0], (float)tempPoints[1]);
                    points[pointCounter++] = tempPoint;
                }
                DrawingInstruction drawingInstruction = new DrawingInstruction();
                drawingInstruction.primitive = Primitive.DrawPolygon;
                drawingInstruction.pointsF = points;
                drawingInstructions.Add(drawingInstruction);
                //m_form.Invalidate();
                return true;
            }
            else if (strFunctionName == "Gfx_FillPolygon")
            {
                if (m_form == null) return false;
                ArrayList parameterPoints = (ArrayList)listParameters[0];
                PointF[] points = new PointF[parameterPoints.Count];
                int pointCounter = 0;
                foreach (KeyValuePair<object, object> entry in parameterPoints)
                {
                    ArrayList tempPoints = (ArrayList)entry.Value;
                    PointF tempPoint = new PointF((float)tempPoints[0], (float)tempPoints[1]);
                    points[pointCounter++] = tempPoint;
                }
                DrawingInstruction drawingInstruction = new DrawingInstruction();
                drawingInstruction.primitive = Primitive.FillPolygon;
                drawingInstruction.pointsF = points;
                drawingInstructions.Add(drawingInstruction);
                //m_form.Invalidate();
                return true;
            }
            if(strFunctionName == "Gfx_Paint")
            {
                if(m_form != null) m_form.Invalidate();
            }
            return false;
        }

        public ReadOnlyCollection<CustomMethod> Prototypes
        {
            get
            {
                return s_listHostFunctionPrototypes;
            }
        }

    }
}
